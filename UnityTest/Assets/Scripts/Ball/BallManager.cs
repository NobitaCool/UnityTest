using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    [SerializeField] private GameObject[] ballList;
    public void ResetBallSelected()
    {
        ballList = GameObject.FindGameObjectsWithTag("Ball");
        for(int i = 0; i < ballList.Length; i++)
        {
            ballList[i].GetComponent<Ball>().isSelected = false;
        }
    }
}
