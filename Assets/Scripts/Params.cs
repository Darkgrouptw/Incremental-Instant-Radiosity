using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Params : MonoBehaviour
{
    #region 跟 Model 有關的參數
    public static float obj_trans_x = 0;
    public static float obj_trans_y = 0;
    public static float obj_trans_z = 0;
    public static float obj_scale = 0;
    #endregion
    #region 跟 light 有關的參數
    public static float light_x = 0;
    public static float light_y = 6;
    public static float light_z = 0;

    public static float light_x_scale = 4;
    public static float light_y_scale = 1;
    public static float light_z_scale = 4;

    public static float light_speed_xz = 1;
    public static float light_speed_y = 1;
    public static float light_rotation_speed = 1;

    public static float light_intensity = 50;
    public static float light_size = 1;
    #endregion

    // 從指定的檔案，去讀取參數檔
    public static void ReadParamsFromFile(string FileName)
    {
        TextAsset textAsset = (TextAsset)Resources.Load(FileName);
        if(textAsset == null)
            Debug.LogError("沒有讀取到檔案!!");
        else
        {
            string[] lineText = textAsset.text.Split('\n');
            Debug.Log("總共 " + lineText.Length + " 行");

            #region 開始 go through 每一行
            for (int i = 0; i < lineText.Length; i++)
                if (lineText[i].StartsWith("#"))
                    continue;

                // 跟 Model 有關的參數
                else if (lineText[i].StartsWith("obj"))
                {
                    if (lineText[i].StartsWith("obj_trans_x"))
                        obj_trans_x = float.Parse(lineText[i].Replace("obj_trans_x = ", ""));
                    else if (lineText[i].StartsWith("obj_trans_y"))
                        obj_trans_y = float.Parse(lineText[i].Replace("obj_trans_y = ", ""));
                    else if (lineText[i].StartsWith("obj_trans_z"))
                        obj_trans_z = float.Parse(lineText[i].Replace("obj_trans_z = ", ""));
                    else if (lineText[i].StartsWith("obj_scale"))
                        obj_scale = float.Parse(lineText[i].Replace("obj_scale = ", ""));
                }

                // 跟 light 有關的參數
                else if (lineText[i].StartsWith("light"))
                {
                    if (lineText[i].StartsWith("light_x_scale"))
                        light_x_scale = float.Parse(lineText[i].Replace("light_x_scale = ", ""));
                    else if (lineText[i].StartsWith("light_y_scale"))
                        light_y_scale = float.Parse(lineText[i].Replace("light_y_scale = ", ""));
                    else if (lineText[i].StartsWith("light_z_scale"))
                        light_z_scale = float.Parse(lineText[i].Replace("light_z_scale = ", ""));

                    else if (lineText[i].StartsWith("light_x"))
                        light_x = float.Parse(lineText[i].Replace("light_x = ", ""));
                    else if (lineText[i].StartsWith("light_y"))
                        light_y = float.Parse(lineText[i].Replace("light_y = ", ""));
                    else if (lineText[i].StartsWith("light_z"))
                        light_z = float.Parse(lineText[i].Replace("light_z = ", ""));

                    else if (lineText[i].StartsWith("light_speed_xz"))
                        light_speed_xz = float.Parse(lineText[i].Replace("light_speed_xz = ", ""));
                    else if (lineText[i].StartsWith("light_speed_y"))
                        light_speed_y = float.Parse(lineText[i].Replace("light_speed_y = ", ""));
                    else if (lineText[i].StartsWith("light_rotation_speed"))
                        light_rotation_speed = float.Parse(lineText[i].Replace("light_rotation_speed = ", ""));

                    else if (lineText[i].StartsWith("light_intensity"))
                        light_intensity = float.Parse(lineText[i].Replace("light_intensity = ", ""));
                    else if (lineText[i].StartsWith("light_size"))
                        light_size = float.Parse(lineText[i].Replace("light_size = ", ""));
                }
            #endregion
        }
    }
    public static List<List<Vector2>> ReadLogFromFile(string FileName)
    {
        TextAsset textAsset = (TextAsset)Resources.Load(FileName);
        List<List<Vector2>> pos = new List<List<Vector2>>();
        if (textAsset == null)
            Debug.LogError("沒有讀取到檔案!!");
        else
        {
            string[] lineText = textAsset.text.Split('\n');
            #region 開始 go through 每一行
            for (int i = 0; i < lineText.Length; i++)
            {
                string[] logNumber = lineText[i].Split(' ');
                pos.Add(new List<Vector2>());
                for(int j = 0;  j < logNumber.Length - 1; j++)
                {
                    string[] number = logNumber[j].Split(',');
                    float x = float.Parse(number[0]);
                    float y = float.Parse(number[1]);
                    pos[pos.Count - 1].Add(new Vector2(x, y)); 
                }
            }
            #endregion
        }
        return pos;
    }


    public static Vector3 GetLightPath(float t)
    {
        // 在 Unity 中 Z 軸和 OpenGL 的方向是相反的
        return new Vector3(light_x + Mathf.Cos(t * light_speed_xz) * light_x_scale,
            light_y + Mathf.Cos(t * light_speed_y) * light_y_scale,
            -(light_z + Mathf.Sin(t * light_speed_xz) * light_z_scale));
    }
}
