using UnityEngine;
using UnityEngine.Rendering; // 네임스페이스

public class VariableExample : MonoBehaviour //클래스
{

    void Start() // 함수(메서드)
    {
     //여기 있는 내용은 게임 오브젝트가 처음 대본을 받을때 한번 실행한다. (읽는다.)
     Debug.Log("시작");
     
     //변수
     //데이터를 저장/조회/수정 할 수 있도록 메모리부터 할당받은 공간 혹은 공간의 이름
     //데이터를 담을 수 있는 공간(상자) 또는 그 공간의 이름
     // - 상자의 종류에 따라 담을 수 있는 데이터의 종류 제한이 있다. (자료형)
     // - 상자의 크기에 따라 담을 수 있는 데이터의 크기 제한이 있다. (변수 자료형의 크기)
    // - 상자의 이름을 변수명이라고 하는데... 보통 변수라고 하면 변수명까지 포함하는 관계다.
    
    //변수의 선언: 데이터를 담을 필요한 공간을 메모리로부터 할당받는 과정
    // - 그 데이터의 공개 범위
    // - 그 데이터의 타입 (자료형)
    // - 그 데이터의 크기 (자료형의 크기)
    // - 이름           (변수명)
    // -> <공개범위> [자료형] [변수명]

    //int health; // 체력 데이터를 저장할 변수
    int damage; // 공격력 데이터를 저장할 변수
    float exp; //경험치를 저장할 변수
    double speed; //속도를 저장할 변수
    //string name; // 이름을 저장할 변수
    bool isDie; // 죽었는지 안죽었는지를 저장할 변수
    
    //변수의 이름은 대, 소문자를 구분한다.
    int health; // 체력 데이터를 저장할 변수
    int Health; // 서로 다른 변수다.
    // 변수를 사용하기전에 값을 미리 할당해두는 것이 좋다. (초기화/초기값)
    // 초기화: 변수를 선언과 동시에 값을 설정하는 것
    // 초기값: 처음 초기화할 때 저장되는 값
    string name = "강윤민";
    int age = 27;
    float height = 182.3f;
    double weight = 56d;
    
    Debug.Log(name); 
    Debug.Log(age);
    Debug.Log(height);
    Debug.Log(weight);
    }

  
 
}
