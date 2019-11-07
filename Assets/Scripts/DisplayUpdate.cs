using UnityEngine;
using System.Collections;

public class DisplayUpdate : MonoBehaviour
{
    // 時間
    float time = 0;
    [Header("========== 初始化參數的檔名 ==========")]
    public string InitParamsFileName;
    public new LightSource light;
    


    public bool IsDebugFirst = false;

    void Awake()
    {
        if(InitParamsFileName != " ")
        {
            Params.ReadParamsFromFile(InitParamsFileName);

            #region Log
            // log 用
            light.logPoint = Params.ReadLogFromFile("log");
            #endregion
        }
    }

    void Update ()
    {
        if(!IsDebugFirst)
        {
            time += Time.deltaTime / 2;

            #region 光的移動
            // 位置
            light.setPosition(Params.GetLightPath(time + 0.01f));

            // 旋轉
            float rosp = time * Params.light_rotation_speed + 0.01f;
            Vector3 dir = new Vector3(Mathf.Cos(rosp * 0.3f),
                Mathf.Sin(rosp * 0.2f) * 0.7f,
                -Mathf.Sin(rosp * 0.3f));

            light.setDirection(dir.normalized);
            #endregion
        }
    }
}
