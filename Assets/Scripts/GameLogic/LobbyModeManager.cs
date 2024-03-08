using System;
using System.Collections.Generic;
using System.Linq;
using MultiSuika.Ball;
using MultiSuika.UI;
using MultiSuika.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using PlayerInputManager = MultiSuika.Player.PlayerInputManager;

namespace MultiSuika.GameLogic
{
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInputManager))]
    public class LobbyModeManager : MonoBehaviour, IGameModeManager
    {
        [SerializeField] public GameData gameData;
        [SerializeField] public GameModeData gameModeData;
        [SerializeField] public GameObject onJoinPopup;
        [SerializeField] public List<Scoreboard> lobbyScore;
    
        private UnityEngine.InputSystem.PlayerInputManager _playerInputManager;
        
        private IntReference _activePlayerNumber;
        private List<PlayerInputManager> _playerInputHandlers = new List<PlayerInputManager>();
        private List<Cannon.Cannon> _cannons = new List<Cannon.Cannon>();
        private List<Container.Container> _containers = new List<Container.Container>();
        private BallTracker _ballTracker = new BallTracker();
        
        private void Awake()
        {
            SetLobbyParameters();
            
            // Connect to the PlayerInputManager and Set the lobbyContainerTrigger
            _playerInputManager = FindObjectOfType<UnityEngine.InputSystem.PlayerInputManager>();
            _playerInputManager.playerJoinedEvent.AddListener(NewPlayerDetected);

            _containers = Initializer.InstantiateContainers(0, gameModeData);

            // Clear any connected players and enable joining with the PlayerInputManager
            DisconnectPlayers();
            _playerInputManager.EnableJoining();
        }

        public void ResetPlayers()
        {
            DisconnectPlayers();
            foreach (var ls in lobbyScore)
                ls.playerScore = null;
        }

        public void StartGame()
        {
            _playerInputManager.DisableJoining();
            SceneManager.LoadScene("PrototypeRacing");
        }

        private void SetLobbyParameters()
        {
            _activePlayerNumber = new IntReference
                { UseConstant = false, Variable = ScriptableObject.CreateInstance<IntVariable>() };
            
            _playerInputHandlers = new List<PlayerInputManager>();
            _cannons = new List<Cannon.Cannon>();
            _containers = new List<Container.Container>();
            _ballTracker = new BallTracker();
            
            var lobbyContainerTriggers = FindObjectsOfType<LobbyContainerTrigger>().ToList();
            foreach (var trigger in lobbyContainerTriggers)
            {
                trigger.SetActivePlayerNumberParameters(_activePlayerNumber);
            }
        }
        
        private void NewPlayerDetected(PlayerInput playerInput)
        {
            var (newPlayerInputHandler, playerIndex) = ConnectPlayerToInputDevice(playerInput);
            _playerInputHandlers.Add(newPlayerInputHandler);
            Cannon.Cannon newCannon = Initializer.InstantiateCannon(gameModeData, _containers[0]);
            _cannons.Add(newCannon);
            Initializer.SetCannonParameters(newCannon, _containers[0], _ballTracker, gameModeData, gameData.playerDataList[playerIndex], gameModeData.skinData.playersSkinData[playerIndex], this);
            newCannon.ConnectCannonToPlayer(newPlayerInputHandler);
            
            // Do custom stuff when a player joins in the lobby
            Color popupColor = gameModeData.skinData.playersSkinData[playerIndex].baseColor;
            AddPlayerJoinPopup(playerIndex, newCannon, popupColor);
        
            ConnectToLobbyScore(gameData.playerDataList[playerIndex].mainScore, lobbyScore[playerIndex], popupColor);
            
            _activePlayerNumber.Variable.ApplyChange(1);
        }
    
        private void ConnectToLobbyScore(IntReference scoreRef, Scoreboard scoreboard, Color color)
        {
            scoreboard.playerScore = scoreRef.Variable;
            scoreboard.connectedColor = color;
        }

        private (PlayerInputManager, int) ConnectPlayerToInputDevice(PlayerInput playerInput)
        {
            var playerIndex = gameData.GetConnectedPlayerQuantity();
            if (playerIndex >= 4)
            {
                Debug.LogError("Something went awfully wrong, you're trying to register a fifth+ player");
                return (null, -1);
            }

            var playerInputRegistered = playerInput.GetComponentInParent<PlayerInputManager>();
            var newPlayerData = gameData.playerDataList[playerIndex];
        
            if (newPlayerData.playerIndexNumber != playerIndex)
                Debug.LogError($"Wrong player index when registering a new player, playerData : {{newPlayerData.playerIndexNumber}} and playerIndex : {playerIndex}");
        
            newPlayerData.SetInputParameters(playerInput.devices[0]);

            return (playerInputRegistered, playerIndex);
        }
    
        private void DisconnectPlayers()
        {
            foreach (var cannon in _cannons)
            {
                cannon.DestroyCurrentBall();
                Destroy(cannon.gameObject);
            }

            foreach (var playerInputHandler in _playerInputHandlers)
            {
                Destroy(playerInputHandler.gameObject);
            }
            _cannons.Clear();
            _playerInputHandlers.Clear();

            foreach (var playerData in gameData.playerDataList)
            {
                playerData.ResetInputParameters();
                playerData.ResetMainScore();
            }
            _activePlayerNumber.Variable.SetValue(0);
        }
    
        private void AddPlayerJoinPopup(int playerIndex, Cannon.Cannon cannon, Color randColor)
        {
            var popup = Instantiate(onJoinPopup, cannon.transform);
            var tmp = popup.GetComponent<TextMeshPro>();
            tmp.color = randColor;
            tmp.text = $"P{playerIndex + 1}";
        }
        
        public void OnBallFusion(BallInstance ballInstance)
        {
        }
    }
}
