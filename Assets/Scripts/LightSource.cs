using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using OpenCVForUnity;

public class LightSource : MonoBehaviour
{
    #region Camera 方向參數
    Vector3 right, up;
    #endregion

    [Header("======== 初始化撒點 ==========")]
    public int InitVPLCount = 64;
    public RenderTexture renderTexture;

    private List<Vector3> VPLPoints = new List<Vector3>();
    private List<Vector3> BestPs = new List<Vector3>();
    private Voronoi voronoiObject = new Voronoi();

    public List<List<Vector2>> logPoint;
    private List<Color> colorList;
    private Texture2D VoronoiDiagram2D;
    private Mat VoronoiDiagram;
    private const int width = 1024;
    private const int height = 1024;
    private string type = "Point 1ight";
    private int index = 0;
    private GameObject parent;
    Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
    public RawImage VD;
    public GameObject PL;


    // VPL 會保留下來的
    private List<GraphEdge> allEdge;
    private List<Site> sites;

    void Start()
    {
        Vector3 org = this.transform.position;
        RaycastHit hit;

        #region Halton 撒點
        if (type == "Point light")
        {
            #region Point light 
            for (int i = 0; i < InitVPLCount; i++)
            {
                float sx = (i + 0.5f) / (float)InitVPLCount;
                float sy = halton2(i + 1);

                Vector3 dir;
                Vector3 bestP = new Vector3(0, 0, 0);

                // 如果是 Spot light
                float a = sx * 3.14159265f * 2.0f;
                float l = Mathf.Sqrt(sy);
                bestP.x = Mathf.Cos(a) * l;
                bestP.y = Mathf.Sin(a) * l;
                BestPs.Add(bestP);

                l = Vector3.Dot(bestP, bestP);
                float z;
                if (l >= 1.0f)
                    z = 0.0f;
                else
                    z = Mathf.Sqrt(1.0f - l);

                // 確定方向在哪
                dir = this.transform.right * bestP.x + this.transform.up * bestP.y - this.transform.forward * z;

                Physics.Raycast(org, dir, out hit);
                VPLPoints.Add(hit.point);
            }
            #endregion
        }
        else
        {
            #region 測試
            for (int i = 0; i < logPoint[0].Count; i++)
            {
                Vector3 bestP = new Vector3(0, 0, 0);
                bestP.x = logPoint[0][i].x;
                bestP.y = logPoint[0][i].y;

                float l = Vector3.Dot(bestP, bestP);
                float z;
                if (l >= 1.0f)
                    z = 0.0f;
                else
                    z = Mathf.Sqrt(1.0f - l);
                BestPs.Add(bestP);

                // 確定方向在哪
                Vector3 dir = this.transform.right * logPoint[0][i].x + this.transform.up * logPoint[0][i].y - this.transform.forward * z;

                Physics.Raycast(org, dir, out hit);
                VPLPoints.Add(hit.point);
            }
            #endregion
        }

        allEdge = MakeVoronoiGraph(BestPs, width, height);
        VoronoiDiagram = Mat.zeros(new Size(width, height), CvType.CV_8UC3);
        DrawVoronoiDiagram(allEdge, out colorList);

        VoronoiDiagram2D = new Texture2D(width, height);
        Utils.matToTexture2D(VoronoiDiagram, VoronoiDiagram2D);
        VD.texture = VoronoiDiagram2D;


        // 產生 VPL
        parent = new GameObject();
        parent.name = "VPL Point";
        parent.transform.position = new Vector3(0, 0, 0);

        for (int i = 0; i < VPLPoints.Count; i++)
        {
            GameObject temp = GameObject.Instantiate(PL);
            temp.transform.parent = parent.transform;
            temp.transform.position = VPLPoints[i];
            temp.GetComponent<Light>().color = colorList[i];
        }
        #endregion
    }

    void Update()
    {
        Vector3 org = this.transform.position;
        RaycastHit hit;
        if (type == "Point light")
        {

        }
        else
        {
            VPLPoints.Clear();
            BestPs.Clear();
            for (int i = 0; i < logPoint[index].Count; i++)
            {
                Vector3 bestP = new Vector3(0, 0, 0);
                bestP.x = logPoint[index][i].x;
                bestP.y = logPoint[index][i].y;
                BestPs.Add(bestP);

                float l = Vector3.Dot(bestP, bestP);
                float z;
                if (l >= 1.0f)
                    z = 0.0f;
                else
                    z = Mathf.Sqrt(1.0f - l);


                // 確定方向在哪
                Vector3 dir = right * logPoint[index][i].x + up * logPoint[index][i].y - this.transform.forward * z;

                Physics.Raycast(org, dir, out hit);

                VPLPoints.Add(hit.point);
                //Debug.DrawLine(org, hit.point);
            }

            allEdge.Clear();
            allEdge = MakeVoronoiGraph(BestPs, width, height);
            VoronoiDiagram = Mat.zeros(new Size(width, height), CvType.CV_8UC3);
            DrawVoronoiDiagram(allEdge, out colorList);
            
            Utils.matToTexture2D(VoronoiDiagram, VoronoiDiagram2D);

            // 產生 VPL
            Destroy(parent);
            parent = new GameObject();
            parent.name = "VPL Point";
            parent.transform.position = new Vector3(0, 0, 0);

            index = (++index) % logPoint.Count;
            for (int i = 0; i < VPLPoints.Count; i++)
            {
                GameObject temp = GameObject.Instantiate(PL);
                temp.transform.parent = parent.transform;
                temp.transform.position = VPLPoints[i];
                temp.GetComponent<Light>().color = colorList[i];
            }
        }
    }

    // Halton 2
    private float halton2(int k)
    {
        int ret = 0;
        int n = 1;

        while (k > 0)
        {
            ret <<= 1;
            if (k % 2 == 1)
                ret |= 1;
            k >>= 1;
            n <<= 1;
        }

        return ret / (float)n;
    }


    #region Voroni 相關
    List<GraphEdge> MakeVoronoiGraph(List<Vector3> point, int width, int height)
    {
        double[] xVal = new double[point.Count];
        double[] yVal = new double[point.Count];
        for (int i = 0; i < point.Count; i++)
        {
            xVal[i] = (point[i].x + 1) * width / 2;
            yVal[i] = (point[i].y + 1) * height / 2;
        }
        return voronoiObject.generateVoronoi(xVal, yVal, 0, width, 0, height, out sites);

    }
    private void DrawVoronoiDiagram(List<GraphEdge> edges, out List<Color> colorTemp)
    {
        Imgproc.rectangle(VoronoiDiagram, new Point(0, 0), new Point(width - 1, height - 1), new Scalar(255, 255, 255), -1);

        Texture2D temp = new Texture2D(width, height);
        RenderTexture.active = renderTexture;
        temp.ReadPixels(new UnityEngine.Rect(0, 0, width, height), 0, 0);

        colorTemp = new List<Color>();
        colorTemp.Clear();
        // 畫點
        for (int i = 0; i < BestPs.Count; i++)
        {
            Point p = Vector3ToPoint(BestPs[i]);

            colorTemp.Add(temp.GetPixel((int)p.x, (int)p.y));
            Imgproc.circle(VoronoiDiagram, p, 5, new Scalar(colorTemp[colorTemp.Count - 1].r * 255, colorTemp[colorTemp.Count -1].g * 255, colorTemp[colorTemp.Count - 1].b * 255), -1);
        }

        // 畫線
        for (int i = 0; i < edges.Count; i++)
        {
            Point p1 = new Point(edges[i].x1, edges[i].y1);
            Point p2 = new Point(edges[i].x2, edges[i].y2);
            Imgproc.line(VoronoiDiagram, p1, p2, new Scalar(0, 0, 0), 3);
        }
        DestroyImmediate(temp);
    }
    #endregion

    private Point Vector3ToPoint(Vector3 p)
    {
        return new Point((p.x + 1) * width / 2, (p.y + 1) * height / 2);
    }


    #region 光的動的方向
    public void setPosition(Vector3 p)
    {
        this.gameObject.transform.localPosition = p;
    }
    public void setDirection(Vector3 d)
    {
        right = Vector3.Cross(new Vector3(0, 1, 0), d).normalized;
        up = Vector3.Cross(d, right);

        this.gameObject.transform.localRotation = Quaternion.LookRotation(d, right);

        // 在 Unity 裡面，座標的 Z 軸，和 OpenGL 裡面的 Z 軸是相反的
        this.gameObject.transform.Rotate(new Vector3(0, 0, 1), -270);
        this.gameObject.transform.Rotate(new Vector3(0, 1, 0), -180);
    }
    #endregion
}
