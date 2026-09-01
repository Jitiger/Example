using UnityEngine;

public class StringMethodExample : MonoBehaviour
{
   /*
    * 문자열은 문자열만의 여러가지 속성과 기능을 가지고 있다.
    * 서식, 길이, 탐색, 변형, 분할
    */
    void Start()
    {
        // 길이 속성
        string name = "  황금독수리 새상을 놀라게 하다.";
            Debug.Log(name.Length);
        // 문자열은 불변식이므로 문자가 수정될떄마다 새로 메모리를 할당한다.
        // 그러므로 잘 해라 ~~~!!!! int는 ?? 설명해주셨는데 멍때리다 놓침 미쳤네
        // 탐색 기능 : 문자열 안에서 특정 문자열이 있는지 없는지, 있다면 어디인지 등등을 알수가 있다.
        int index = name.IndexOf("놀");
        Debug.Log(index);
        bool isHwang = name.StartsWith("황");
        Debug.Log(isHwang);
        bool isContainEagle = name.Contains("독수리");
        Debug.Log(isContainEagle);
        
        
        // 변형 기능 : 대소문자 변환 혹은 추가, 대체, 삭제
        // 문자열은 불변 변경하는 횟수가 많아질 수록 메모리를 많이 쓴다.
        name = name.Trim(); // 공백제거
        name = name.Replace(".", "");
        name = name.Replace("새", "세");
        name = name.Insert(6, "온 "); // 중간 삽입
        Debug.Log(name);

        string name2 = "karina";
        name2 = name2.ToUpper();
        Debug.Log(name2);
        name2 = name2.ToLower();
        Debug.Log(name2);
        /*
         * int : 4바이트
         * double : 8바이트
         * string : 2 * 문자열의 길이 바이트
         * string name = "김홍일"
         * string othername = "황금독수리온세상을 놀라게하다."
         */

    }

    
}
