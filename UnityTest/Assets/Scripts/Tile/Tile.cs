using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    private const float INIT_X = -3.75f;
    private const float INIT_Y = -13.75f;
    private const float RANGE = 2.5f;
    [SerializeField] private GenerateBoard generateBoard;
    [SerializeField] private GameObject[] balls;

    void Awake()
    {
        generateBoard = GameObject.FindGameObjectWithTag("Board").GetComponent<GenerateBoard>();
        x = (int) ((transform.position.x - INIT_X)/RANGE);
        y = (int) ((transform.position.y - INIT_Y)/RANGE);
    }

    void OnMouseDown()
    {
        Debug.Log(string.Format("X: {0}, Y:{1}", x, y));
        balls = GameObject.FindGameObjectsWithTag("Ball");
        if(BallSelected() != null)  generateBoard.MoveBall(BallSelected(), this);
    }

    private GameObject BallSelected()
    {
        if(balls == null) return null;
        for(int i = 0; i < balls.Length; i++)
        {
            if(balls[i].GetComponent<Ball>().isSelected) return balls[i];
        }
        return null;
    }
}
