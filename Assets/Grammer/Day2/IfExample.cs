using UnityEngine;

public class IfExample : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { // Allman 스타일
        /*
       * 코드는 위에서 아래로(순차적) 실행되는 흐름을 가지고 있다.
       * 이 흐름에 변화를 주는 것을 "제어문" ->
       * 제어문: 조건문, 분기문, 반복문
       *
       * 조건문: 조건식에 따라 "무엇을 할지 안할지" 정할 수 있다
       * 조건식: 비교 or 관계 연산자를 쓰는 식 => bool(true/false)
       * C#의 조건문 if, else if, else 가 있다.
       * if: 만약 ~ 라면? ~ 한다.
       * else if : 그렇지 않고 ~라면 ~ 한다.
       * else : 이도저도 아니라면? ~ 한다.
       *
       *      if (조건식 / 만약 ~ 라면
       *       {
       *           ~ 한다.
       *             조건식이 참(ture)이면 실행할 코드
       *       }
       *      else if (조건식) / 그렇지 않고 ~ 라면? ~ 한다.
       *       {
       *
       *       }
       *      else 이도저도 아니라면? ~ 한다
       *      {
       *
       *      }
       *
       */
    // 무조건 양수인 숫자가 짝수면 짝수! 홀수면 홀수! 0이면 "0!"이라고 출력하는 코드를 작성해보세요.
      
    }
        /* 중첩 조건문
         조건문 안에 또 조건문이 들어가 있는 형태다.
         ex) 로그인할 때 아이디와 비밀번호를 입력 받는다.
         - 아이디를 입력하지 않으면: "존재하지 않는 ID 입니다."
         - 아이디가 있고, 비밀번호가 없거나 틀리면: "비밀번호가 틀렸습니다."
         - 아이디가 있고, 비밀번호가 맞다면: "로그인 성공!"
         */
     
}
