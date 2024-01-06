using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Specific Parameters")]
    [Min(0f)] [SerializeField] public float speed;
    [Min(0f)] [SerializeField] public float reloadCooldown;
    // It's not used anymore with the shootingDirection that's already at -1 when at 0°
    // [Min(0f)] [SerializeField] public float yBallDelta;
    [Min(0f)] [SerializeField] public float boundariesFromCenter;
    
    [Header("General Game Parameters")]
    [SerializeField] public PlayerGameData playerGameData;
    [SerializeField] public IntReference playerScore;

    [Header("Ball Cannon Parameters")] [SerializeField]
    private bool isUsingPeggleControls = false;
    [Min(0f)] [SerializeField] public float shootingForce = 200f;
    [Range(-Mathf.PI/2+0.1f, Mathf.PI/2-0.1f)] [SerializeField] public float shootingAngle = 0f;
    [SerializeField] public Vector2 shootingDirection = Vector2.down;
    
    
    private Vector3 centeredPosition;
    private Vector2 horizontalMargin;
    private Queue<Ball> nextBalls;
    private Ball currentBall;
    // private Vector2 ballDelta;

    private void Start()
    {
        InitializePlayerData();
        LoadNewBall();
    }

    private void Update()
    {
        UpdatePlayerPosition(Input.GetAxis("Horizontal"));
        if (Input.GetKeyDown(KeyCode.Space) && currentBall != null)
            DropBall();
    }

    private void DropBall()
    {
        currentBall.EnableCollision();
        // TESTING ANGLE DROPPING, REFACTOR THIS LATER!!!
        currentBall.rb2d.AddForce(shootingDirection * shootingForce);
        currentBall = null;
        Invoke("LoadNewBall", reloadCooldown);
    }

    private void LoadNewBall()
    {
        currentBall = playerGameData.ballSetData.SpawnNewBall((Vector2)transform.position + /*ballDelta +*/ shootingDirection, playerScore, disableCollision: true);
    }
    
    private void InitializePlayerData()
    {
        if (playerGameData == null)
        {
            Debug.LogError("The player doesn't have gameData");
        }
        // ballDelta = new Vector3(0f, -yBallDelta, 0f);
        centeredPosition = transform.position;
        horizontalMargin = new Vector2(centeredPosition.x - boundariesFromCenter,
            centeredPosition.x + boundariesFromCenter);
    }

    private void UpdatePlayerPosition(float xAxis)
    {
        if (xAxis == 0) return;
        if (isUsingPeggleControls)
        {
            if (xAxis < 0 && shootingAngle > -Mathf.PI / 2 + 0.1f || xAxis > 0 && shootingAngle < Mathf.PI / 2 - 0.1f)
            {
                shootingAngle += xAxis * speed * Time.deltaTime;
                shootingDirection = new Vector2(Mathf.Sin(shootingAngle), -Mathf.Cos(shootingAngle));
            }
        }
        else
        {
            if (xAxis < 0 && transform.position.x > horizontalMargin.x || xAxis > 0 && transform.position.x < horizontalMargin.y)
                transform.Translate(xAxis*Time.deltaTime*speed, 0, 0);
        }

        if (currentBall != null)
            currentBall.transform.position = (Vector2)transform.position + /*ballDelta +*/ shootingDirection;

    }
}
