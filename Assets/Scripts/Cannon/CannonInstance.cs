using MultiSuika.Ball;
using MultiSuika.Container;
using MultiSuika.GameLogic;
using MultiSuika.Player;
using MultiSuika.Utilities;
using UnityEngine;

namespace MultiSuika.Cannon
{
    public class CannonInstance : MonoBehaviour
    {
        [SerializeField] public SpriteRenderer spriteRenderer;
    
        // PlayerIndex
        private int _playerIndex;
        
        // Cannon Parameters
        public float speed;
        public float reloadCooldown;
        public float shootingForce;
        public float emptyDistanceBetweenBallAndCannon;
        public bool isUsingPeggleMode = false;
        private PlayerInputHandler _playerInputHandler;
    
        // Positioning
        public float horizontalMargin;
        private float _shootingAngle = 0f;
        private Vector2 _shootingDirection = Vector2.down;

        // Ball Parameters
        public BallSetData ballSetData;
        public BallSpriteThemeData ballSpriteData;
        public FloatReference scoreReference;
        public ContainerInstance containerInstance;
        public BallTracker ballTracker;
        private BallInstance _currentBallInstance;
        private float _currentBallDistanceFromCannon;

        // Wwise Event
        public AK.Wwise.Event WwiseEventCannonShoot;
        
        public void DestroyCurrentBall()
        {
            if (_currentBallInstance != null)
                Destroy(_currentBallInstance.gameObject);
        }

        public void SetCannonInputEnabled(bool isActive)
        {
            if (_playerInputHandler == null)
                return;
            if (isActive)
            {
                _playerInputHandler.onHorizontalMvtContinuous += MoveCannon;
                _playerInputHandler.onShoot += DropBall;
                if (_currentBallInstance == null)
                    LoadNewBall();
            }
            else
            {
                _playerInputHandler.onHorizontalMvtContinuous -= MoveCannon;
                _playerInputHandler.onShoot -= DropBall;
            }
        }

        // public void ConnectCannonToPlayer(PlayerInputSystem playerInputSystem)
        // {
        //     _playerInputSystem = playerInputSystem;
        //     SetCannonInputConnexion(true);
        // }

        public void DisconnectCannonToPlayer()
        {
            SetCannonInputEnabled(false);
            _playerInputHandler = null;
        }
        
        private void DropBall()
        {
            if (_currentBallInstance == null)
                return;
        
            _currentBallInstance.DropBallFromCannon();
            _currentBallInstance.rb2d.AddForce(_shootingDirection.normalized * shootingForce);
            _currentBallInstance = null;
            Invoke("LoadNewBall", reloadCooldown);
            WwiseEventCannonShoot.Post(gameObject);
        }
    
        private void MoveCannon(float xAxis)
        {
            if (isUsingPeggleMode)
            {
                if (xAxis < 0 && _shootingAngle > -Mathf.PI / 2 + 0.1f || xAxis > 0 && _shootingAngle < Mathf.PI / 2 - 0.1f)
                {
                    _shootingAngle += xAxis * speed * Time.deltaTime;
                    _shootingDirection = new Vector2(Mathf.Sin(_shootingAngle), -Mathf.Cos(_shootingAngle));
                }
            }
            else
            {
                if (xAxis < 0 && transform.localPosition.x > -horizontalMargin || xAxis > 0 && transform.localPosition.x < horizontalMargin)
                    transform.Translate(xAxis*Time.deltaTime*speed, 0, 0);
            }

            if (_currentBallInstance != null)
                _currentBallInstance.transform.localPosition = (Vector2)transform.localPosition + _shootingDirection.normalized * _currentBallDistanceFromCannon;
        }
    
        private void LoadNewBall()
        {
            var newBallIndex = ballSetData.GetRandomBallTier();
            _currentBallDistanceFromCannon = ballSetData.GetBallData(newBallIndex).scale / 2f + emptyDistanceBetweenBallAndCannon;
            _currentBallInstance = Initializer.InstantiateBall(ballSetData, containerInstance,
                (Vector2)transform.localPosition + _shootingDirection.normalized * _currentBallDistanceFromCannon);
            
            // TODO: Check if we can better fit that into the initialization encapsulation (we're setting in two different places)
            _currentBallInstance.transform.SetLayerRecursively(gameObject.layer);
            
            _currentBallInstance.SetBallParameters(_playerIndex, newBallIndex, ballSetData, ballSpriteData);
            _currentBallInstance.SetSimulatedParameters(false);
        }

        #region Setter
        public void SetInputParameters(PlayerInputHandler playerInputHandler)
        {
            _playerInputHandler = playerInputHandler;
        }

        public void SetPlayerIndex(int playerIndex)
        {
            _playerIndex = playerIndex;
        }
        

        #endregion
        
    }
}