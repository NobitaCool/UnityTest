using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GenerateBoard : MonoBehaviour
{
    private const int BOARD_SIZE = 8;
    private const float INIT_X = -3.75f;
    private const float INIT_Y = -13.75f;
    private const float RANGE = 2.5f;
    private const int INIT_BALL = 5;
    private const int NEXT_BALL = 3;
    private const int BALL_TYPE = 8;
    private int score = 0;
    private bool isGameOver = false;
    private bool hasEatBall = false;

    private const int EAT_BALL_LINE_NUM = 5;
    private const int EAT_BALL_BLOCK_NUM = 6;

    public UnityEvent ResetBallSelection;

    [SerializeField] private GameObject defaultBall;
    [SerializeField] private GameObject queueBall;
    [SerializeField] private Material[] materials;
    [SerializeField] private ParticleSystem[] explosions;
    [SerializeField] private TextMeshProUGUI scoreValue;
    [SerializeField] private GameObject gameOverCanvas;
    public GameObject tile;


    private int[,] board = new int[BOARD_SIZE, BOARD_SIZE];
    private GameObject[,] ballList = new GameObject[BOARD_SIZE, BOARD_SIZE];
    private List<GameObject> ballQueue = new List<GameObject>();

    public void MoveBall(GameObject ball, Tile tile)
    {

        Ball ballComponent = ball.GetComponent<Ball>();

        if (!ballComponent.isSelected) return;

        if (board[tile.x, tile.y] < 0) return;

        Vector2 from = new Vector2(ballComponent.x, ballComponent.y);
        Vector2 to = new Vector2(tile.x, tile.y);


        if (FindPath(from, to) && !isGameOver)
        {
            Ball temp = ball.GetComponent<Ball>();

            board[temp.x, temp.y] = 0;

            board[tile.x, tile.y] = temp.ballType;

            Spawn();
            GeneratingBoard();

            // CheckEatBall(ballList[tile.x, tile.y]);

            if (CountEmptyTile() <= 0) isGameOver = true;

            if (isGameOver)
            {
                gameOverCanvas.SetActive(true);
            }
        }
        // Debug.Log(string.Format("Board X: {0}, Y: {1},  Value: {2}", tile.x, tile.y, board[tile.x, tile.y]));
    }

    private void Spawn()
    {
        EnableBallQueue();
        SpawnNextBall();
    }

    private void CheckEatBall(GameObject ball)
    {
        if(!hasEatBall) CheckLine(ball);
        if(!hasEatBall) CheckBlock(ball);
        
        hasEatBall = false;
    }

    private void GeneratingBoard()
    {
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                int type = board[x, y];

                if (type < 0)
                {
                    ballList[x, y].transform.localScale /= 2;
                }

                type = (type < 0) ? -type : type;

                ballList[x, y].GetComponent<MeshRenderer>().material = materials[type];
                ballList[x, y].GetComponent<Ball>().ballType = type;
                ballList[x, y].GetComponent<Ball>().x = x;
                ballList[x, y].GetComponent<Ball>().y = y;

                if (type == 0)
                {
                    Vector3 vec3 = new Vector3(ballList[x, y].transform.position.x, ballList[x, y].transform.position.y, 1);
                    ballList[x, y].transform.position = vec3;
                }
                else
                {
                    Vector3 vec3 = new Vector3(ballList[x, y].transform.position.x, ballList[x, y].transform.position.y, 0);
                    ballList[x, y].transform.position = vec3;   
                    CheckEatBall(ballList[x, y]);                 
                }

            }
        }
    }

    private bool FindPath(Vector2 from, Vector2 to)
    {
        ResetBallSelection.Invoke();

        if (ballList[(int)from.x, (int)from.y].GetComponent<Ball>().ballType == BALL_TYPE - 1) return true;

        if (from == to) return false;

        Queue<Vector2> tileQueue = new Queue<Vector2>();
        List<Vector2> searched = new List<Vector2>();

        bool hasFounded = false;

        tileQueue.Enqueue(from);

        int[] u = { 1, 0, -1, 0 };
        int[] v = { 0, 1, 0, -1 };

        while (tileQueue.Count > 0)
        {
            if (hasFounded) break;
            Vector2 cur = tileQueue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                if (cur.x == to.x && cur.y == to.y)
                {
                    hasFounded = true;
                    break;
                }

                int x = (int)cur.x + u[i];
                int y = (int)cur.y + v[i];

                if (!(x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE)) continue;

                if (board[x, y] > 0 || searched.Contains(new Vector2(x, y))) continue;

                searched.Add(new Vector2(x, y));

                tileQueue.Enqueue(new Vector2(x, y));
            }
        }
        Debug.Log(string.Format("Tile Count Path: {0}", tileQueue.Count));
        tileQueue.Clear();
        return hasFounded;
    }

    private void CheckLine(GameObject ball)
    {
        int[] u = { 0, 1, 1, 1 };
        int[] v = { 1, 0, -1, 1 };

        int x = 0;
        int y = 0;
        int count = 0;

        for (int i = 0; i < 4; i++)
        {
            Ball ballComponent = ball.GetComponent<Ball>();
            count = 1;

            x = (int)ballComponent.x;
            y = (int)ballComponent.y;

            while (true)
            {
                x += u[i];
                y += v[i];

                if (!(x >= 0 && y >= 0 && x < BOARD_SIZE && y < BOARD_SIZE)) break;

                if (board[x, y] != ballComponent.ballType) break;

                count++;
            }

            x = (int)ballComponent.x;
            y = (int)ballComponent.y;

            while (true)
            {
                x -= u[i];
                y -= v[i];

                if (!(x >= 0 && y >= 0 && x < BOARD_SIZE && y < BOARD_SIZE)) break;

                if (board[x, y] != ballComponent.ballType) break;

                count++;
            }

            // Debug.Log(string.Format("Count: {0}", count));

            if (count >= EAT_BALL_LINE_NUM)
            {
                while (count-- > 0)
                {
                    x += u[i];
                    y += v[i];

                    board[x, y] = 0;

                    SpawnExplosion(ballList[x, y]);

                    ballList[x, y].GetComponent<MeshRenderer>().material = materials[0];
                    ballList[x, y].GetComponent<Ball>().ballType = 0;

                    Vector3 vec3 = new Vector3(ballList[x, y].transform.position.x, ballList[x, y].transform.position.y, 1);
                    ballList[x, y].transform.position = vec3;
                    score++;
                    scoreValue.text = score.ToString();
                }
                hasEatBall = true;
            }
        }
    }

    private void CheckBlock(GameObject ball)
    {
        int[] u = { 1, 0, -1, 0 };
        int[] v = { 0, 1, 0, -1 };

        Vector2 vec2 = new Vector2(ball.GetComponent<Ball>().x, ball.GetComponent<Ball>().y);

        Queue<Vector2> tileQueue = new Queue<Vector2>();
        List<Vector2> searched = new List<Vector2>();

        tileQueue.Enqueue(vec2);

        while (tileQueue.Count > 0)
        {
            Vector2 cur = tileQueue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int x = (int)cur.x + u[i];
                int y = (int)cur.y + v[i];

                if (!(x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE)) continue;

                if (board[x, y] != ball.GetComponent<Ball>().ballType || searched.Contains(new Vector2(x, y))) continue;

                searched.Add(new Vector2(x, y));

                tileQueue.Enqueue(new Vector2(x, y));
            }
        }

        if (searched.Count >= EAT_BALL_BLOCK_NUM)
        {
            for (int i = 0; i < searched.Count; i++)
            {
                int x = (int)searched[i].x;
                int y = (int)searched[i].y;

                board[x, y] = 0;
                SpawnExplosion(ballList[x, y]);

                ballList[x, y].GetComponent<MeshRenderer>().material = materials[0];
                ballList[x, y].GetComponent<Ball>().ballType = 0;

                Vector3 vec3 = new Vector3(ballList[x, y].transform.position.x, ballList[x, y].transform.position.y, 1);
                ballList[x, y].transform.position = vec3;
                score++;
                scoreValue.text = score.ToString();
            }
            hasEatBall = true;
        }

        Debug.Log(string.Format("Tile Count Path: {0}", tileQueue.Count));
        tileQueue.Clear();
    }

    private void SpawnExplosion(GameObject ball)
    {
        Ball ballComponent = ball.GetComponent<Ball>();
        if (ballComponent.ballType <= 0) return;
        Instantiate(explosions[ballComponent.ballType - 1], ball.transform.position, Quaternion.identity);
    }
    public void GenerateAllTiles()
    {
        isGameOver = false;
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                Instantiate(tile, new Vector3(INIT_X + RANGE * x, INIT_Y + RANGE * y, -0.5f), Quaternion.identity);
                board[x, y] = 0;
            }
        }
        BallPool();
        UpdateBallQueue();
        SpawnBall();
        SpawnNextBall();
        GeneratingBoard();
    }

    private void SpawnBall()
    {
        int available;

        for (int i = 0; i < INIT_BALL; i++)
        {
            available = Random.Range(0, CountEmptyTile()) + 1;
            bool stop = false;
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                if (stop) break;
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (board[x, y] == 0)
                    {
                        available--;
                        if (available == 0)
                        {
                            int type = (Random.Range(0, BALL_TYPE - 1) + 1);
                            board[x, y] = type;
                            stop = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void SpawnNextBall()
    {
        int available;

        for (int i = 0; i < ballQueue.Count; i++)
        {
            available = Random.Range(0, CountEmptyTile()) + 1;
            bool stop = false;
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                if (stop) break;
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (board[x, y] == 0)
                    {
                        available--;
                        if (available == 0)
                        {
                            board[x, y] = ballQueue[i].GetComponent<Ball>().ballType;
                            stop = true;
                            break;
                        }
                    }
                }
            }
        }
        UpdateBallQueue();
    }

    private void EnableBallQueue()
    {
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (board[x, y] < 0)
                {
                    ballList[x, y].transform.localScale *= 2;
                    ballList[x, y].GetComponent<Ball>().ballType *= -1;
                    board[x, y] *= -1;
                }
            }
        }
    }

    private int CountEmptyTile()
    {
        int emptyTile = 0;

        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (board[x, y] == 0) emptyTile++;
            }
        }

        return emptyTile;
    }

    private void BallPool()
    {
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                GameObject spawner = Instantiate(defaultBall, new Vector3(INIT_X + RANGE * x, INIT_Y + RANGE * y, 1), Quaternion.identity);
                spawner.GetComponent<Ball>().x = x;
                spawner.GetComponent<Ball>().y = y;
                ballList[x, y] = spawner;
            }
        }
        for (int i = 0; i < NEXT_BALL; i++)
        {
            GameObject spawner = Instantiate(queueBall, new Vector3(INIT_X + RANGE * (8 + i), INIT_Y + RANGE * 6, 0), Quaternion.identity);
            spawner.transform.localScale /= 2;
            ballQueue.Add(spawner);
        }
    }

    private void UpdateBallQueue()
    {
        for (int i = 0; i < ballQueue.Count; i++)
        {
            int type = Random.Range(0, BALL_TYPE - 1) + 1;
            ballQueue[i].GetComponent<MeshRenderer>().material = materials[type];
            ballQueue[i].GetComponent<Ball>().ballType = -type;
        }
        // Debug.Log(string.Format("Ball in Queue: {0}", ballQueue.Count));
    }
}
