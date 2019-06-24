using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HingeControl : MonoBehaviour
{
    public const float SPRINGANGLE = -90.0f;    // 경첩이 펴지는 각도

    public int[] movementInfo;                  // 행동배열

    HingeJoint hinge;                           // 경첩의 Component
    JointSpring spring;                         // 경첩의 각도를 조절하는 옵션

    void Start()
    {
        hinge = this.GetComponentInChildren<HingeJoint>();
        spring = hinge.spring;
    }

    // i번째 배열을 읽어 행동한다
    public void Move(int i)
    {
        if (movementInfo[i] == 1)
        {
            spring.targetPosition = SPRINGANGLE;
        }
        else
        {
            spring.targetPosition = 0;
        }
        hinge.spring = spring;
    }
}

/*
int cnt = 0;
long milliseconds = 0;
long newMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
long howLongMilliseconds = newMilliseconds - milliseconds;
Debug.Log(cnt++ + "/" + howLongMilliseconds);
milliseconds = newMilliseconds;*/
