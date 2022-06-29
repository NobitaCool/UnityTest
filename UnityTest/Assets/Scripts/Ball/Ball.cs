using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum BallType
{
    None = 0,
    Red = 1,
    Yellow = 2,
    Blue = 3,
    Green = 4,
    Ocean = 5,
    Purple = 6,
    Ghost = 7,
}

public class Ball : MonoBehaviour
{
    private const float INIT_X = -3.75f;
    private const float INIT_Y = -13.75f;
    private const float RANGE = 2.5f;
    public int x;
    public int y;

    public int ballType;
    public bool isSelected;
    public UnityEvent ResetSelection;
    void OnMouseDown()
    {
        ResetSelection.Invoke();
        Debug.Log(string.Format("X:{0}, Y:{1}, Type: {2}", x, y, ballType));
        isSelected = true;
    }

    public void UpdateLocation()
    {
        x = (int) ((transform.position.x - INIT_X)/RANGE);
        y = (int) ((transform.position.y - INIT_Y)/RANGE);
    }
    
}
