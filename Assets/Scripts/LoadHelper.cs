using Lerc2017;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

public class LoadHelper : Singleton<LoadHelper>
{
    private byte[] tmpData;

    public Dictionary<string, float[,]> heightDataList = new Dictionary<string, float[,]>();

    protected LoadHelper() { }

    public float[,] GetHeightData(int level, int x, int y, int size)
    {
        if (level > 14)
        {
            for (int i = level; i > 14; i--)
            {
                x = (int)Math.Floor(x / 2.0);
                y = (int)Math.Floor(y / 2.0);
            }

            level = 14;
        }

        if (heightDataList.ContainsKey("hd_" + level + "_" + x + "_" + y + "_" + size)) return heightDataList["hd_" + level + "_" + x + "_" + y];

        else
        {
            return LoadHeightMap(level, x, y, size);
        }
    }

    public IEnumerator DownloadRemoteFile(string uri)
    {
        Debug.Log("Waiting for download");
        using (var www = WWW.LoadFromCacheOrDownload(uri, 0))
        {

            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
                yield return null;
            }

            Debug.Log("sss");
            tmpData = www.bytes;
        }

    }

    internal float[,] LoadHeightMap(int level, int tileX, int tileY, int size)
    {
        string id = "hd_" + level + "_" + tileX + "_" + tileY + "_" + size;
        string uri = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer/tile/" + level + "/" + tileY + "/" + tileX;

        StartCoroutine("DownloadRemoteFile", uri);
        /*
        byte[] pLercBlob = tmpData;
        Debug.Log(pLercBlob.Length);
        String[] infoLabels = { "version", "data type", "nDim", "nCols", "nRows", "nBands", "num valid pixels", "blob size" };
        String[] dataRangeLabels = { "zMin", "zMax", "maxZErrorUsed" };

        int infoArrSize = infoLabels.Count();
        int dataRangeArrSize = dataRangeLabels.Count();

        UInt32[] infoArr = new UInt32[infoArrSize];
        double[] dataRangeArr = new double[dataRangeArrSize];

        UInt32 hr = LercDecode.lerc_getBlobInfo(pLercBlob, (UInt32)pLercBlob.Length, infoArr, dataRangeArr, infoArrSize, dataRangeArrSize);
        if (hr > 0)
        {
            Debug.LogError("function lerc_getBlobInfo(...) failed with error code " + hr);
        }

        int lercVersion = (int)infoArr[0];
        int dataType = (int)infoArr[1];
        int nDim = (int)infoArr[2];
        int nCols = (int)infoArr[3];
        int nRows = (int)infoArr[4];
        int nBands = (int)infoArr[5];

        byte[] pValidBytes = new byte[nCols * nRows];
        uint nValues = (uint)(nDim * nCols * nRows * nBands);

        double[] pData = new double[nValues];
        hr = LercDecode.lerc_decodeToDouble(pLercBlob, (UInt32)pLercBlob.Length, pValidBytes, nDim, nCols, nRows, nBands, pData);

        float[,] heightData = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                heightData[i, j] = (float)GetHeightValue(pValidBytes, pData, nCols, level, tileX, tileY, i, j);
            }
        }

        return heightData;
        */
        return null;
    }

    double GetValue(byte[] validBytes, double[] data, int width, double x, double y)
    {
        int x0 = (int)Math.Floor(x);
        int x1 = x0 + 1;
        int y0 = (int)Math.Floor(y);
        int y1 = y0 + 1;

        double v00 = validBytes[x0 + y0 * width] == 1 ? data[x0 + y0 * width] : 0;
        double v10 = validBytes[x1 + y0 * width] == 1 ? data[x1 + y0 * width] : 0;

        double v01 = validBytes[x0 + y1 * width] == 1 ? data[x0 + y1 * width] : 0;
        double v11 = validBytes[x1 + y1 * width] == 1 ? data[x1 + y1 * width] : 0;

        double v0 = v00 + (x - x0) * (v10 - v00);
        double v1 = v01 + (x - x0) * (v11 - v01);

        return v0 + (y - y0) * (v0 - v1);
    }

    double GetHeightValue(byte[] validBytes, double[] data, int width, int level, int tileX, int tileY, float x, float y)
    {
        if (level <= 14)
        {
            return GetValue(validBytes, data, width, x, y);
        }
        else
        {
            for (int i = level; i > 14; i--)
            {
                x /= 2;
                y /= 2;

                if (tileX % 2 != 0) x += 128;
                if (tileY % 2 != 0) y += 128;

                tileX = (int)Math.Floor(tileX / 2.0);
                tileY = (int)Math.Floor(tileY / 2.0);
            }

            return GetValue(validBytes, data, width, x, y);
        }
    }

}
