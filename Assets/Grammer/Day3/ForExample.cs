using System;
using System.Net;
using UnityEngine;

public class ForExample : MonoBehaviour
{

    void Start()
    {
        //반복문: 설정한 조건이 True 일 동안 (거짓이 될 때까지) 코드 블록 내부를 반복해서 실행한다.
        // for 반복문
        
        /*

         for(초기식, 조건식, 증감 연산자)
         {
         반복할 코드들
         }
        */
        
        // 초기식: 처음 한 번 실행할 식(보통 변수의 선언) : ex. int i = 0;
        // 조건식: 값을 비교해서 결과를 참/거짓인지 판명해주는 연산자
        // 증감연산자: ex. i++

        for (int i = 0; i < 1000; i++)
        {
            Debug.Log($"안녕하세요. {i:4d}");
        };


        int sum = 0;
        for (int number = 1; number <= 1000; number++)
        {
            if (number % 2 == 0)
            {
                sum+=number;
            }
        }
        Debug.Log(sum);
        //구구단 2단부터 9단까지 5단을 제외하고 중첩 분복문을 이용해서 출력해보세요.
        // 점프문: 반복문 내부에서 흐름을 끊고 코드 실행 위치를 원하는 곳으로 점프해준다.
        // - break : 현재 실행중인 반복문이나 분기문의 실행을 중단할 떄 사용.(종료/탈출 이라고 부르기도 함)
        // - continue : 반복문 코드블록 내에서 현재 실행중인 코드 라인의 아래 코드들의 실행을 건너 뛸때 사용
        for (int i = 2; i < 10; i++)
        {
            if (i == 5)
            {
                continue;
            }

            Debug.Log($"[{i}단 시작!]");

            for (int j = 1; j < 10; j++)
            {
                Debug.Log($"{i} * {j} = {i * j:2d}");
            }
            
        }
        
        // 문자열: 문자를 순서대로 나'열' (문자 배열)
        string myName = "황금독수리온세상을놀라게하다";
        Debug.Log(myName);
        Debug.Log(myName[0]); //황
        Debug.Log(myName[1]); //금
        Debug.Log(myName[myName.Length - 1]); //다
        
        for (int i = 0; i < myName.Length; i++)
        {
            Debug.Log(myName[i]);
        }
        //foreach 배열이나 컬렉션의 요소를 처음부터 끝까지 순서대로 간편하게 접근할때 사용하는 반복문
        foreach (char c in myName)
        {
            Debug.Log(c);
            //황금다
        }
    }

 
}
