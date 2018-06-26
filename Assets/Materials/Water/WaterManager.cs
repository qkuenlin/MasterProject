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

    public float heightMax;
    public float U0 ;


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
        /*
        computeShader.SetFloat("FFT_SIZE", FFT_SIZE);
        computeShader.SetInt("N_SLOPE_VARIANCE", N_SLOPE_VARIANCE);

        computeShader.SetVector("INVERSE_GRID_SIZE", new Vector4(2.0f * Mathf.PI * FFT_SIZE / GRID1_SIZE, 2.0f * Mathf.PI * FFT_SIZE / GRID2_SIZE, 2.0f * Mathf.PI * FFT_SIZE / GRID3_SIZE, 2.0f * Mathf.PI * FFT_SIZE / GRID4_SIZE));
        computeShader.SetVector("GRID_SIZE", new Vector4(GRID1_SIZE, GRID2_SIZE, GRID3_SIZE, GRID4_SIZE));


        spectrums = new Texture2DArray(FFT_SIZE, FFT_SIZE, 2, TextureFormat.RGBAFloat, true);
        butterfly = new Texture2D(FFT_SIZE, PASSES, TextureFormat.RGBAFloat, true);

        SlopeVariance = new RenderTexture(N_SLOPE_VARIANCE, N_SLOPE_VARIANCE, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
        SlopeVariance.enableRandomWrite = true;
        SlopeVariance.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        SlopeVariance.volumeDepth = N_SLOPE_VARIANCE;
        SlopeVariance.Create();

        generateWaveSpectrum();
        //ComputeSlopeVariance();
        computeButterflyLookupTexture();

        int kernel = computeShader.FindKernel("Init");

        textureInit = new RenderTexture(FFT_SIZE, FFT_SIZE, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
        textureInit.enableRandomWrite = true;
        textureInit.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        textureInit.volumeDepth = 5;
        textureInit.useMipMap = true;
        textureInit.autoGenerateMips = false;
        textureInit.Create();

        kernel = computeShader.FindKernel("FFTX");
        computeShader.SetTexture(kernel, "ButterFly", butterfly);

        textureFFT = new RenderTexture(FFT_SIZE, FFT_SIZE, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
        textureFFT.enableRandomWrite = true;
        textureFFT.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        textureFFT.volumeDepth = 5;
        textureFFT.Create();

        kernel = computeShader.FindKernel("FFTY");
        computeShader.SetTexture(kernel, "ButterFly", butterfly);

        foreach (Material m in terrainMaterials)
        {
            m.SetVector("GRID_SIZE", new Vector4(GRID1_SIZE, GRID2_SIZE, GRID3_SIZE, GRID4_SIZE));
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.unscaledTime;
        /*
        int kernel = computeShader.FindKernel("Init");
        computeShader.SetTexture(kernel, "Input", spectrums);
        computeShader.SetTexture(kernel, "Output", textureInit);
        computeShader.SetFloat("Time", t);
        computeShader.Dispatch(kernel, FFT_SIZE / 8, FFT_SIZE / 8, 1);
        
        kernel = computeShader.FindKernel("FFTX");
        
        for (int i = 0; i < PASSES; i++)
        {
            if (i % 2 == 0)
            {
                computeShader.SetTexture(kernel, "Input", textureInit);
                computeShader.SetTexture(kernel, "Output", textureFFT);
            }
            else
            {
                computeShader.SetTexture(kernel, "Input", textureFFT);
                computeShader.SetTexture(kernel, "Output", textureInit);
            }
            computeShader.SetFloat("_pass", (i + 0.5f) / PASSES);
            computeShader.Dispatch(kernel, FFT_SIZE / 8, FFT_SIZE / 8, 5);
        }
        /*
        kernel = computeShader.FindKernel("FFTY");

        for (int i = 0; i < 2 * PASSES; i++)
        {
            if (i % 2 == 0)
            {
                computeShader.SetTexture(kernel, "Input", textureInit);
                computeShader.SetTexture(kernel, "Output", textureFFT);
            }
            else
            {
                computeShader.SetTexture(kernel, "Input", textureFFT);
                computeShader.SetTexture(kernel, "Output", textureInit);
            }

            computeShader.SetFloat("_pass", (i - PASSES + 0.5f) / PASSES);
            computeShader.Dispatch(kernel, FFT_SIZE / 8, FFT_SIZE / 8, 5);
        }
        */

        // textureInit.GenerateMips();

        RaycastHit info;
        Physics.Raycast(Camera.main.transform.position, new Vector3(0, -1, 0), out info, 5000);

        Vector4 lods = new Vector4(gridSize, 
                                    Mathf.Atan(2.0f/info.distance) * gridSize, 
                                    Mathf.Log(lambdaMin) / Mathf.Log(2.0f), 
                                    (nbWaves - 1.0f) / (Mathf.Log(lambdaMax) / Mathf.Log(2.0f) - Mathf.Log(lambdaMin) / Mathf.Log(2.0f)));

        Debug.Log(lods);

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
            float amplitude;

            float step = (max - min) / (nbWaves - 1); // dlambda/di
            float omega0 = 9.81f / U0;
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
            amplitude = (8.1e-3f * 9.81f * 9.81f) / pow(omega, 5) * exp(-0.74f * pow(omega0 / omega, 4));
            amplitude *= 0.5f * sqrt(2 * Mathf.PI * 9.81f / lambda) * nbAngles * step;
            amplitude = 3 * heightMax * sqrt(amplitude);

            if (amplitude > 1.0f / knorm)
            {
                amplitude = 1.0f / knorm;
            }
            else if (amplitude < -1.0f / knorm)
            {
                amplitude = -1.0f / knorm;
            }

            waves.x = amplitude;
            waves.y = omega;
            waves.z = knorm * Mathf.Cos(ktheta);
            waves.w = knorm * Mathf.Sin(ktheta);

            sigmaXsq += pow(Mathf.Cos(ktheta), 2.0f) * (1.0f - sqrt(1.0f - knorm * knorm * amplitude * amplitude));
            sigmaYsq += pow(Mathf.Sin(ktheta), 2.0f) * (1.0f - sqrt(1.0f - knorm * knorm * amplitude * amplitude));
            meanHeight -= knorm * amplitude * amplitude * 0.5f;
            heightVariance += amplitude * amplitude * (2.0f - knorm * knorm * amplitude * amplitude) * 0.25f;
            amplitudeMax += Mathf.Abs(amplitude);

            wavesTexture.SetPixel(i, 0, new Color(waves.x, waves.y, waves.z, waves.w));
        }
        wavesTexture.Apply();
        float var = 4.0f;
        amplitudeMax = 2.0f * var * sqrt(heightVariance);

        byte[] tex = wavesTexture.EncodeToPNG();
        FileStream file = File.Open(@"waves.png", FileMode.Create);
        BinaryWriter binary = new BinaryWriter(file);
        binary.Write(tex);
        file.Close();
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

    // 1/kx and 1/ky in meters
    float spectrum(float kx, float ky, bool omnispectrum = false)
    {
        float U10 = WIND;
        float Omega = OMEGA;

        // phase speed
        float k = Mathf.Sqrt(kx * kx + ky * ky);
        float c = omega(k) / k;

        // spectral peak
        float kp = 9.81f * (Omega / U10) * (Omega / U10); // after Eq 3
        float cp = omega(kp) / kp;

        // friction velocity
        float z0 = 3.7e-5f * sqr(U10) / 9.81f * Mathf.Pow(U10 / cp, 0.9f); // Eq 66
        float u_star = 0.41f * U10 / Mathf.Log(10.0f / z0); // Eq 60

        float Lpm = Mathf.Exp(-1.25f * sqr(kp / k)); // after Eq 3
        float gamma = Omega < 1.0f ? 1.7f : 1.7f + 6.0f * Mathf.Log(Omega); // after Eq 3 // log10 or log??
        float sigma = 0.08f * (1.0f + 4.0f / Mathf.Pow(Omega, 3.0f)); // after Eq 3
        float Gamma = Mathf.Exp(-1.0f / (2.0f * sqr(sigma)) * sqr(Mathf.Sqrt(k / kp) - 1.0f));
        float Jp = Mathf.Pow(gamma, Gamma); // Eq 3
        float Fp = Lpm * Jp * Mathf.Exp(-Omega / Mathf.Sqrt(10.0f) * (Mathf.Sqrt(k / kp) - 1.0f)); // Eq 32
        float alphap = 0.006f * Mathf.Sqrt(Omega); // Eq 34
        float Bl = 0.5f * alphap * cp / c * Fp; // Eq 31

        float alpham = 0.01f * (u_star < cm ? 1.0f + Mathf.Log(u_star / cm) : 1.0f + 3.0f * Mathf.Log(u_star / cm)); // Eq 44
        float Fm = Mathf.Exp(-0.25f * sqr(k / km - 1.0f)); // Eq 41
        float Bh = 0.5f * alpham * cm / c * Fm * Lpm; // Eq 40 (fixed)

        if (omnispectrum)
        {
            return A * (Bl + Bh) / (k * sqr(k)); // Eq 30
        }

        float a0 = Mathf.Log(2.0f) / 4.0f; float ap = 4.0f; float am = 0.13f * u_star / cm; // Eq 59
        float Delta = (float)System.Math.Tanh(a0 + ap * Mathf.Pow(c / cp, 2.5f) + am * Mathf.Pow(cm / c, 2.5f)); // Eq 57

        float phi = Mathf.Atan2(ky, kx);

        if (kx < 0.0f)
        {
            return 0.0f;
        }
        else
        {
            Bl *= 2.0f;
            Bh *= 2.0f;
        }

        return A * (Bl + Bh) * (1.0f + Delta * Mathf.Cos(2.0f * phi)) / (2.0f * Mathf.PI * sqr(sqr(k))); // Eq 67
    }

    void getSpectrumSample(int i, int j, float lengthScale, float kMin, ref float[] result, int offset)
    {
        float dk = 2.0f * Mathf.PI / lengthScale;
        float kx = 1.0f * i * dk;
        float ky = 1.0f * j * dk;
        if (Mathf.Abs(kx) < kMin && Mathf.Abs(ky) < kMin)
        {
            result[0 + offset] = 0.0f;
            result[1 + offset] = 0.0f;
        }
        else
        {
            float S = spectrum(kx, ky);
            float h = Mathf.Sqrt(S * 0.5f) * dk;
            float phi = frandom() * 2.0f * Mathf.PI;
            result[0 + offset] = 1000 * h * Mathf.Cos(phi);
            result[1 + offset] = 1000 * h * Mathf.Sin(phi);
        }
    }
    /*
    void generateWaveSpectrum()
    {
        spectrum12 = new float[FFT_SIZE * FFT_SIZE * 4];
        spectrum34 = new float[FFT_SIZE * FFT_SIZE * 4];

        Color[] spectrum12Color = new Color[FFT_SIZE * FFT_SIZE];
        Color[] spectrum34Color = new Color[FFT_SIZE * FFT_SIZE];


        for (int y = 0; y < FFT_SIZE; ++y)
        {
            for (int x = 0; x < FFT_SIZE; ++x)
            {
                int offset = 4 * (x + y * FFT_SIZE);
                int i = x >= FFT_SIZE / 2 ? x - FFT_SIZE : x;
                int j = y >= FFT_SIZE / 2 ? y - FFT_SIZE : y;
                getSpectrumSample(i, j, GRID1_SIZE, Mathf.PI / GRID1_SIZE, ref spectrum12, offset);
                getSpectrumSample(i, j, GRID2_SIZE, Mathf.PI * FFT_SIZE / GRID1_SIZE, ref spectrum12, offset + 2);
                getSpectrumSample(i, j, GRID3_SIZE, Mathf.PI * FFT_SIZE / GRID2_SIZE, ref spectrum34, offset);
                getSpectrumSample(i, j, GRID4_SIZE, Mathf.PI * FFT_SIZE / GRID3_SIZE, ref spectrum34, offset + 2);

                spectrum12Color[x + y * FFT_SIZE] = new Color(spectrum12[offset], spectrum12[offset + 1], spectrum12[offset + 2], spectrum12[offset + 3]);
                spectrum34Color[x + y * FFT_SIZE] = new Color(spectrum34[offset], spectrum34[offset + 1], spectrum34[offset + 2], spectrum34[offset + 3]);
            }
        }

        spectrums.SetPixels(spectrum12Color, 0);
        spectrums.SetPixels(spectrum34Color, 1);

        spectrums.Apply();

        Texture2D spec12 = new Texture2D(FFT_SIZE, FFT_SIZE, TextureFormat.RGBAFloat, false);
        spec12.SetPixels(spectrum12Color);

        byte[] tex1 = spec12.EncodeToPNG();
        FileStream file1 = File.Open(@"spectrum12.png", FileMode.Create);
        BinaryWriter binary1 = new BinaryWriter(file1);
        binary1.Write(tex1);
        file1.Close();

        Texture2D spec34 = new Texture2D(FFT_SIZE, FFT_SIZE, TextureFormat.RGBAFloat, false);
        spec34.SetPixels(spectrum34Color);

        byte[] tex2 = spec34.EncodeToPNG();
        FileStream file2 = File.Open(@"spectrum34.png", FileMode.Create);
        BinaryWriter binary2 = new BinaryWriter(file2);
        binary2.Write(tex2);
        file2.Close();

        foreach (Material m in terrainMaterials)
        {
            m.SetTexture("_Sat", spec12);
        }

    }

    int bitReverse(int i, int N)
    {
        int j = i;
        int M = N;
        int Sum = 0;
        int W = 1;
        M = M / 2;
        while (M != 0)
        {
            j = (i & M) > M - 1 ? 1 : 0;
            Sum += j * W;
            W *= 2;
            M = M / 2;
        }
        return Sum;
    }

    void computeWeight(int N, int k, ref float Wr, ref float Wi)
    {
        Wr = Mathf.Cos(2.0f * Mathf.PI * k / (float)(N));
        Wi = Mathf.Sin(2.0f * Mathf.PI * k / (float)(N));
    }

    void computeButterflyLookupTexture()
    {
        float[] data = new float[FFT_SIZE * PASSES * 4];

        for (int i = 0; i < PASSES; i++)
        {
            int nBlocks = (int)Mathf.Pow(2.0f, (float)(PASSES - 1 - i));
            int nHInputs = (int)Mathf.Pow(2.0f, (float)(i));
            for (int j = 0; j < nBlocks; j++)
            {
                for (int k = 0; k < nHInputs; k++)
                {
                    int i1, i2, j1, j2;
                    if (i == 0)
                    {
                        i1 = j * nHInputs * 2 + k;
                        i2 = j * nHInputs * 2 + nHInputs + k;
                        j1 = bitReverse(i1, FFT_SIZE);
                        j2 = bitReverse(i2, FFT_SIZE);
                    }
                    else
                    {
                        i1 = j * nHInputs * 2 + k;
                        i2 = j * nHInputs * 2 + nHInputs + k;
                        j1 = i1;
                        j2 = i2;
                    }

                    float wr = 0, wi = 0;
                    computeWeight(FFT_SIZE, k * nBlocks, ref wr, ref wi);

                    int offset1 = 4 * (i1 + i * FFT_SIZE);
                    data[offset1 + 0] = (j1 + 0.5f) / FFT_SIZE;
                    data[offset1 + 1] = (j2 + 0.5f) / FFT_SIZE;
                    data[offset1 + 2] = wr;
                    data[offset1 + 3] = wi;

                    int offset2 = 4 * (i2 + i * FFT_SIZE);
                    data[offset2 + 0] = (j1 + 0.5f) / FFT_SIZE;
                    data[offset2 + 1] = (j2 + 0.5f) / FFT_SIZE;
                    data[offset2 + 2] = -wr;
                    data[offset2 + 3] = -wi;
                }
            }
        }

        Color[] color = new Color[FFT_SIZE * PASSES];

        for (int y = 0; y < PASSES; ++y)
        {
            for (int x = 0; x < FFT_SIZE; ++x)
            {
                int offset = 4 * (x + y * FFT_SIZE);
                int i = x >= FFT_SIZE / 2 ? x - FFT_SIZE : x;
                int j = y >= FFT_SIZE / 2 ? y - FFT_SIZE : y;

                color[x + y * FFT_SIZE] = new Color(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
            }
        }

        butterfly.SetPixels(color);
        butterfly.Apply();

        byte[] tex = butterfly.EncodeToPNG();
        FileStream file = File.Open(@"butterfly.png", FileMode.Create);
        BinaryWriter binary = new BinaryWriter(file);
        binary.Write(tex);
        file.Close();

    }

    float getSlopeVariance(float kx, float ky, ref float[] spectrumSample, int offset)
    {
        float kSquare = kx * kx + ky * ky;
        float real = spectrumSample[0 + offset];
        float img = spectrumSample[1 + offset];
        float hSquare = real * real + img * img;
        return kSquare * hSquare * 2.0f;
    }

    void ComputeSlopeVariance()
    {
        float theoreticSlopeVariance = 0.0f;
        float k = 5e-3f;
        while (k < 1e3)
        {
            float nextK = k * 1.001f;
            theoreticSlopeVariance += k * k * spectrum(k, 0, true) * (nextK - k);
            k = nextK;
        }

        float totalSlopeVariance = 0.0f;

        for (int y = 0; y < FFT_SIZE; ++y)
        {
            for (int x = 0; x < FFT_SIZE; ++x)
            {
                int offset = 4 * (x + y * FFT_SIZE);
                float i = 2.0f * Mathf.PI * (x >= FFT_SIZE / 2 ? x - FFT_SIZE : x);
                float j = 2.0f * Mathf.PI * (y >= FFT_SIZE / 2 ? y - FFT_SIZE : y);
                totalSlopeVariance += getSlopeVariance(1.0f * i / GRID1_SIZE, j / GRID1_SIZE, ref spectrum12, offset);
                totalSlopeVariance += getSlopeVariance(1.0f * i / GRID2_SIZE, j / GRID2_SIZE, ref spectrum12, offset + 2);
                totalSlopeVariance += getSlopeVariance(1.0f * i / GRID3_SIZE, j / GRID3_SIZE, ref spectrum34, offset);
                totalSlopeVariance += getSlopeVariance(1.0f * i / GRID4_SIZE, j / GRID4_SIZE, ref spectrum34, offset + 2);
            }
        }

        int kernel = computeShader.FindKernel("Variance");

        computeShader.SetTexture(kernel, "Input", spectrums);
        computeShader.SetTexture(kernel, "SlopeVariance", SlopeVariance);
        computeShader.SetFloat("slopeVarianceDelta", 0.5f * (theoreticSlopeVariance - totalSlopeVariance));
        computeShader.Dispatch(kernel, N_SLOPE_VARIANCE, N_SLOPE_VARIANCE, N_SLOPE_VARIANCE);

        Debug.Log(theoreticSlopeVariance + "  " + totalSlopeVariance + "   " + 0.5f * (theoreticSlopeVariance - totalSlopeVariance));

        foreach (Material m in terrainMaterials)
        {
            m.SetTexture("_WaterSlopeVariance", SlopeVariance);
        }
    }*/
}
