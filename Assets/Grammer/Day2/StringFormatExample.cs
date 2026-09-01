using UnityEngine;

public class StringFormatExample : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string name = "하지호";
        int age = 28;
        bool isMan = false;
        
        //나의 이름은 --- 이고, 나이는 -- 살 입니다. 성별은 -- 입니다.
        //1번째 방법
        Debug.Log("나의 이름은" + name + "이고, 나이는 " + age + "살 입니다. 성별은 " + (isMan ? "여자" : "남자") + "입니다.");
        
        //2. 문자열 서식(String.Format)을 이용한 방식
        string gender = isMan ? "여자":"남자";
        string introduceString = string.Format("나의 이름은 {0}이고, 나이는 {1}살 입니다. 성별은 {2}입니다.", name, age, isMan ? "여자":"남자");
        //->
        
        //3. $ 기호를 이용한 문자열 보간
        string introduceString2 = $"나의 이름은{name}이고, 나이는 {age}살 입니다. 성별은 {gender}입니다.";
        Debug.Log(introduceString2);
       
        float height = 163.888f;
        float money = 656421;
        Debug.Log(height); // 182.754 // 그런데 나는 소수점 첫밴째 자리까지만 출력을 하고 싶다..        ->서식 문자열
        Debug.Log(money); // 6564321 // 그런데 나는 원 단위로 숫자 세자리마다 , 를 붙여서 출력하고 싶다..-> 서식 문자열
       
        Debug.Log(string.Format("{0:F1}", height)); //<- 이런 기능이 있음 f라는 키워드를 기억해서 사용 다 외울 필요 없음
        Debug.Log(string.Format("{0:N0}", money));

        int hour = 3;
        int minute = 13;
        //03시 13분
        Debug.Log($"{hour:D2}시 {minute}분");
       
        //
    }

    
}

