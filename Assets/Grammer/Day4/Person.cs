using UnityEngine;

// 상속이란: 부모(상위/기반/슈퍼) 클래스의 속성과 기능을 자식(하위/파생/서브) 클래스가 물려받아 그대로 사용하거나 확장/재정의 하는것
// 장점:
// - 코드 재사용 (이미 만들어진 부모 클래스의 속성과 기능을 공유하므로 중복 최소화)
// - 확장 (자식 클래스만의 고유한 속성이나 기능 추가 가능)
// - 재정의: 부모에게 물려 받은 메서드를 상황에 맞게 재정의(오버라이

public class Person : Animal
{
    // 필드 (속성, 멤버, 변수)
    // public: 외부에서 접근(읽기,쓰기)가 가능하다.
    // private: 외부에서 접근이 불가능하다.
    // -> 접근 제한자(public, private, protected, internal)
   
    // 추상화 -> 은닉화 -> 캡슐화
   
    private int _age = 0;
    public bool HasGlasses = false;

    // 생성자 메서드 
    // - 객체가 생성될 때 new 키워드와 함께 호출하는 메서드
    // - 클래스 이름과 같아야 한다.
    public Person(string name, int age, float height, double weight, bool hasGlasses)
    {
        _name = name;
        _age = age;
        _height = height;
        _weight = weight;
      
        HasGlasses = hasGlasses;
    }
   
   
    // 메서드 (기능, 함수)
    public override void Introduce()
    {
        Debug.Log($"안녕하세요. 제 이름은 {_name}이고, 나이는 {_age}입니다. 키와 몸무게는({_height}cm/{_weight}kg) 입니다.");
    }
   
    // 메서드 오버로드: 같은 이름의 메서드를 중복하여 여러 개 만드는 것
    // - 매개 변수의 개수나 자료형이 달라야 한다.
   
    public void SeatDown()
    {
        Debug.Log("땅바닥에 앉는다!");
    }
   
    public void SeatDown(Chair chair)
    {
        Debug.Log($"{chair.Color}색 의자에 앉는다!");
    }


    public void Punch(Person otherPerson)
    {
        // public  필드 변수는 : Name (파스칼)
        // private 필드 변수는 : _name (_카멜)
        //         지역 변수는 : name (카멜)     
      
      
        // 상곤이가 윤미이를 때렸다.
        Debug.Log($"{_name}이가 {otherPerson._name}이를 때렸다!");
    }
}