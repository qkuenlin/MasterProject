using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public int nbWaves;
    public float wavesDirection;
    public float waveDispersion;
    public float lambdaMin;
    public float lambdaMax;
    public float v20;
    public float amplitude;

    public float nyquistMin;
    public float nyquistMax;
    public Color waterColor;
    public float waterRoughness;

    public float gridSize;
    // public ComputeShader computeShader;
    public List<Material> terrainMaterials;

    float sigmaXsq = 0.0f;
    float sigmaYsq = 0.0f;
    float meanHeight = 0.0f;
    float heightVariance = 0.0f;
    float amplitudeMax = 0.0f;

    Texture2D wavesTexture;

    //  RenderTexture textureFFT;
    // RenderTexture textureInit;
    // RenderTexture SlopeVariance;


    // Texture2DArray spectrums;
    // Texture2D butterfly;

    // const int PASSES = 8;
    //  const int FFT_SIZE = 1 << PASSES;

    //  const int N_SLOPE_VARIANCE = 8;

    float GRID1_SIZE = 5488.0f; // size in meters (i.e. in spatial domain) of the first grid
    float GRID2_SIZE = 392.0f; // size in meters (i.e. in spatial domain) of the second grid
    float GRID3_SIZE = 28.0f; // size in meters (i.e. in spatial domain) of the third grid
    float GRID4_SIZE = 2.0f; // size in meters (i.e. in spatial domain) of the fourth grid

    float WIND = 5.0f; // wind speed in meters per second (at 10m above surface)
    float OMEGA = 0.84f; // sea state (inverse wave age)
    float A = 1.0f; // wave amplitude factor (should be one)
    const float cm = 0.23f; // Eq 59
    const float km = 370.0f; // Eq 59

    float[] spectrum12;
    float[] spectrum34;

    static long seed = 1234567;
    static float y2;
    static bool use_last = false;

    // Use this for initialization
    void Start()
    {
        //gridSize = Camera.current.pixelWidth;
        wavesTexture = new Texture2D(nbWaves, 1, TextureFormat.RGBAFloat, true);
        generateWaves();
    }

    // Update is called once per frame
    void Update()
    {
        seed = 1234567;
        generateWaves();

        float t = Time.unscaledTime;

        Vector4 lods = new Vector4(2*Camera.main.pixelHeight / (Mathf.Deg2Rad*Camera.main.fieldOfView), 
                                    100.0F / gridSize, //size in meters of the grid (constant over the whole terrain)
                                    Mathf.Log(lambdaMin) / Mathf.Log(2.0f), 
                                    (nbWaves - 1.0f) / (Mathf.Log(lambdaMax) / Mathf.Log(2.0f) - Mathf.Log(lambdaMin) / Mathf.Log(2.0f)));

        Matrix4x4 worldToWind = new Matrix4x4();
        worldToWind[0, 0] = Mathf.Cos(wavesDirection);
        worldToWind[0, 1] = Mathf.Sin(wavesDirection);
        worldToWind[1, 0] = -Mathf.Sin(wavesDirection);
        worldToWind[1, 1] = Mathf.Cos(wavesDirection);

        Matrix4x4 windToWorld = new Matrix4x4();
        windToWorld[0, 0] = Mathf.Cos(wavesDirection);
        windToWorld[0, 1] = -Mathf.Sin(wavesDirection);
        windToWorld[1, 0] = Mathf.Sin(wavesDirection);
        windToWorld[1, 1] = Mathf.Cos(wavesDirection);

        Vector2 sigmaSqTotal = new Vector2(sigmaXsq, sigmaYsq);

        foreach (Material m in terrainMaterials)
        {
            if (m != null)
            {
                m.SetColor("_WaterColor", waterColor);
                m.SetFloat("_nyquistMin", nyquistMin);
                m.SetFloat("_nyquistMax", nyquistMax);
                m.SetFloat("_nbWaves", nbWaves);
                m.SetVector("_sigmaSqTotal", sigmaSqTotal);
                m.SetMatrix("_worldToWind", worldToWind);
                m.SetMatrix("_windToWorld", windToWorld);
                m.SetVector("_lods", lods);
                m.SetTexture("_wavesSampler", wavesTexture);
                m.SetFloat("_WaterRoughness", waterRoughness);

                // m.SetMatrix("screenToCamera", Camera.current.projectionMatrix.inverse);
                // m.SetMatrix("screenToCamera", Camera.current.worldToCameraMatrix.inverse);
            }
        }

    }

    float log(float x)
    {
        return Mathf.Log(x);
    }

    float exp(float x)
    {
        return Mathf.Exp(x);
    }

    float sqrt(float x)
    {
        return Mathf.Sqrt(x);
    }

    float pow(float x, float y)
    {
        return Mathf.Pow(x, y);
    }

    float angle(int i, int nbAngles)
    {
        return (1.5f * (((i) % nbAngles) / (float)(nbAngles / 2) - 1));
    }

    float grandom(float mean, float stdDeviation)
    {
        float x1, x2, w, y1;

        if (use_last)
        {
            y1 = y2;
            use_last = false;
        }
        else
        {
            do
            {
                x1 = 2.0f * frandom() - 1.0f;
                x2 = 2.0f * frandom() - 1.0f;
                w = x1 * x1 + x2 * x2;
            } while (w >= 1.0f);
            w = sqrt((-2.0f * log(w)) / w);
            y1 = x1 * w;
            y2 = x2 * w;
            use_last = true;
        }
        return mean + y1 * stdDeviation;
    }

    void generateWaves()
    {
        float min = log(lambdaMin) / log(2.0f);
        float max = log(lambdaMax) / log(2.0f);

        sigmaXsq = 0.0f;
        sigmaYsq = 0.0f;
        meanHeight = 0.0f;
        heightVariance = 0.0f;
        amplitudeMax = 0.0f;

        int nbAngles = 5; // even
        float dangle = (1.5f / (float)(nbAngles / 2));

        float[] Wa = new float[nbAngles]; // normalised gaussian samples
        int[] index = new int[nbAngles]; // to hash angle order
        float s = 0;
        for (int i = 0; i < nbAngles; i++)
        {
            index[i] = i;
            float a = angle(i, nbAngles);
            s += Wa[i] = exp(-0.5f * a * a);
        }
        for (int i = 0; i < nbAngles; i++)
        {
            Wa[i] /= s;
        }

        for (int i = 0; i < nbWaves; ++i)
        {
            Vector4 waves = new Vector4();
            float x = i / (nbWaves - 1.0f);

            float lambda = pow(2.0f, (1.0f - x) * min + x * max);
            float ktheta = grandom(0.0f, 1.0f) * waveDispersion;
            float knorm = 2.0f * Mathf.PI / lambda;
            float omega = sqrt(9.81f * knorm);
            float amplitude_;

            float step = (max - min) / (nbWaves - 1); // dlambda/di
            float omega0 = 9.81f / v20;
            if ((i % (nbAngles)) == 0)
            { // scramble angle ordre
                for (int k = 0; k < nbAngles; k++)
                {   // do N swap in indices
                    int n1 = (int)lrandom() % nbAngles, n2 = (int)lrandom() % nbAngles, n;
                    n = index[n1];
                    index[n1] = index[n2];
                    index[n2] = n;
                }
            }
            ktheta = waveDispersion * (angle(index[(i) % nbAngles], nbAngles) + 0.4f * srnd() * dangle);
            ktheta *= 1.0f / (1.0f + 40.0f * pow(omega0 / omega, 4));
            amplitude_ = (8.1e-3f * 9.81f * 9.81f) / pow(omega, 5) * exp(-0.74f * pow(omega0 / omega, 4));
            amplitude_ *= 0.5f * sqrt(2 * Mathf.PI * 9.81f / lambda) * nbAngles * step;
            amplitude_ = 3 * amplitude * sqrt(amplitude_);

            if (amplitude_ > 1.0f / knorm)
            {
                amplitude_ = 1.0f / knorm;
            }
            else if (amplitude_ < -1.0f / knorm)
            {
                amplitude_ = -1.0f / knorm;
            }

            waves.x = amplitude_;
            waves.y = omega;
            waves.z = knorm * Mathf.Cos(ktheta);
            waves.w = knorm * Mathf.Sin(ktheta);

            sigmaXsq += pow(Mathf.Cos(ktheta), 2.0f) * (1.0f - sqrt(1.0f - knorm * knorm * amplitude_ * amplitude_));
            sigmaYsq += pow(Mathf.Sin(ktheta), 2.0f) * (1.0f - sqrt(1.0f - knorm * knorm * amplitude_ * amplitude_));

            meanHeight -= knorm * amplitude_ * amplitude_ * 0.5f;
            heightVariance += amplitude_ * amplitude_ * (2.0f - knorm * knorm * amplitude_ * amplitude_) * 0.25f;
            amplitudeMax += Mathf.Abs(amplitude_);

            wavesTexture.SetPixel(i, 0, new Color(waves.x, waves.y, waves.z, waves.w));
        }
        wavesTexture.Apply();
        float var = 4.0f;
        amplitudeMax = 2.0f * var * sqrt(heightVariance);
    }

    float sqr(float x)
    {
        return x * x;
    }

    float omega(float k)
    {
        return Mathf.Sqrt(9.81f * k * (1.0f + sqr(k / km))); // Eq 24
    }

    float srnd()
    {
        return 2 * frandom() - 1;
    }

    long lrandom()
    {
        seed = (seed * 1103515245 + 12345) & 0x7FFFFFFF;
        return seed;
    }

    float frandom()
    {
        seed = (seed * 1103515245 + 12345) & 0x7FFFFFFF;
        long r = seed >> (31 - 24);
        return r / (float)(1 << 24);
    }    
}
