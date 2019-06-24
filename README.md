# DeepLearningStudyProject

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/ae8d4203fc5340e098da498d4cc82471)](https://app.codacy.com/app/normal111/DeepLearningStudyProject?utm_source=github.com&utm_medium=referral&utm_content=normal111/DeepLearningStudyProject&utm_campaign=Badge_Grade_Dashboard)

## 개발 목적
1. 신기술 AI, 딥 러닝에 대하여 조금이라도 알아보려고 시작하였습니다.
2. 예전에 보았던 그네 타기 알고리즘 학습 영상을 보고 영감을 받아 만들어보고 싶어서 시작하였습니다.
[https://www.youtube.com/watch?v=Yr_nRnqeDp0&t=7s]

## 개발 기간
2019.05.23 ~ 2019.06.19

## 개발 목표
스스로 움직이는 경첩을 만드는 것이 이 프로젝트의 목표입니다. (영화 인터스텔라에 나오는 타스 같은 느낌)
![타스](https://user-images.githubusercontent.com/37904040/60002729-2e791180-96a4-11e9-8b8d-779c692b8dcc.gif)

## 개발 알고리즘
### 경첩의 알고리즘
1. 0이면 경첩을 접고 1이면 경첩을 폅니다.
2. 무작위로 배열을 만듭니다. [0, 1, 0, 1, 1, 1, 0, 0, 1, 0, .....]
3. 배열을 순서대로 읽어 경첩을 움직입니다.
![경첩](https://user-images.githubusercontent.com/37904040/60010355-959dc280-96b2-11e9-97e8-e84f361d3e15.PNG)

### 시스템 알고리즘 순서
1. 프로그램이 시작되면 완전 랜덤 구조의 1세대를 생성합니다.
2. 그 세대의 유전자를 읽어 50개의 경첩을 세팅 후 실행합니다.
3. 가장 멀리 움직인 유전자를 바탕으로 다음 세대를 생성합니다.
4. 2번으로 가서 테스트를 반복하여 점점 경첩들을 학습 시킵니다.
![학습](https://user-images.githubusercontent.com/37904040/60010342-87e83d00-96b2-11e9-9fe4-c25320290c23.PNG)

## 개발 결과
학습은 성공적이었습니다.
처음에는 10m조차 움직이기 힘들었던 경첩들이
학습 이후에는 40m를 가뿐하게 넘겼습니다
1세대 경첩들의 모습
![1](https://user-images.githubusercontent.com/37904040/60010409-b7974500-96b2-11e9-838b-b4cfde79453c.gif)
2281세대 경첩들의 모습
![2281](https://user-images.githubusercontent.com/37904040/60010433-c120ad00-96b2-11e9-9b8f-624fb9bd0a7c.gif)

## 문제점, 한계
이 프로젝트를 진행하면서 가장 큰 문제를 맞닥뜨렸습니다.
바로 유니티 엔진 자체의 오차였습니다.
비록 오차가 크지 않았지만 작은 오차로 경첩들이 잘못된 부모 유전자를 받아 학습을 하지 못하고 오히려 퇴화하는 경우도 있었습니다. 이 문제점을 해결하려고 FixedUpdate를 사용하거나 유니티 내부 설정을 바꾸는 등 여러 노력을 해보았지만 약간의 오차는 어쩔 수 없다는 것을 알았고 위 방법들로 최대한 오차를 줄여 개발하기로 하였습니다.

또한 사실상 500세대 정도만 학습을 해도 40m를 움직일 수 있었지만. 어디까지 갈 수 있을지 궁금해서 약 3000세대까지 학습을 시켜보았습니다. 그러나 500세대~3000세대에서는 더 이상 학습하지 못하고 거의 같은 움직임을 보여주었습니다. 이 말은 오차 때문에 더 이상 학습을 할 수 없다고 볼 수도 있지만, 이 조건에서 이 이상 멀리 갈 수 없다는 것을 뜻하기도 합니다. 그래서 저는 여기서 이 프로젝트를 마쳤습니다.
