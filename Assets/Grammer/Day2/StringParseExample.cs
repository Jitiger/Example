using UnityEngine;

public class StringParseExample : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string stringAge = "38";
        int intAge = 0;
        bool result1 = int.TryParse(stringAge, out intAge);
        if (result1 == true)
        {
            Debug.Log(intAge);
        }
        else
        {
            Debug.Log("변환에 실패했습니다.");
        }
        //float => 문자열
        float floatHeight = 174f;
        string stringHeight = floatHeight.ToString();
        
        // 문자열을 float로 변환
        string stringWeight = "70.2kg";
        float floatWeight = 0f;
        bool result2 = float.TryParse(stringWeight, out floatWeight);
        Debug.Log(floatWeight);
    }

  
}
