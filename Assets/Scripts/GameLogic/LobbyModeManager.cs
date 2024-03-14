using System;
using System.Collections.Generic;
using System.Linq;
using MultiSuika.Ball;
using MultiSuika.Cannon;
using MultiSuika.Container;
using MultiSuika.Manager;
using MultiSuika.Player;
using MultiSuika.UI;
using MultiSuika.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MultiSuika.GameLogic
{
    public class LobbyModeManager : MonoBehaviour, IGameModeManager
    {
        [Header("Manager Base Parameters")] [SerializeField]
        public GameData gameData;

        [SerializeField] public GameModeData gameModeData;

        [Header("Lobby Specific Parameters")] [SerializeField]
        public string nextSceneName;

        [SerializeField] public GameObject onJoinPopup;
        [SerializeField] public List<Scoreboard> lobbyScore;

        // private List<PlayerInputHandler> _playerInputHandlers = new List<PlayerInputHandler>();
        private List<CannonInstance> _cannons = new List<CannonInstance>();
        private List<ContainerInstance> _containers = new List<ContainerInstance>();
        private BallTracker _ballTracker = new BallTracker();

        private void Awake()
        {
            SetLobbyParameters();

            _containers = Initializer.InstantiateContainers(0, gameModeData);

            PlayerManager.Instance.SetJoiningEnabled(true);
            PlayerManager.Instance.SubscribePlayerPush(NewPlayerDetected);
        }

        public void ResetPlayers()
        {
            DisconnectPlayers();
            foreach (var ls in lobbyScore)
                ls.playerScore = null;
            PlayerManager.Instance.ClearAllPlayers();
        }

        public void StartGame()
        {
            PlayerManager.Instance.SetJoiningEnabled(false);
            SceneManager.LoadScene(nextSceneName);
        }

        private void SetLobbyParameters()
        {
            // _playerInputHandlers = new List<PlayerInputHandler>();
            _cannons = new List<CannonInstance>();
            _containers = new List<ContainerInstance>();
            _ballTracker = new BallTracker();

            var lobbyContainerTriggers = FindObjectsOfType<LobbyContainerTrigger>().ToList();
            foreach (var trigger in lobbyContainerTriggers)
            {
                trigger.SetNumberOfActivePlayerParameters(PlayerManager.Instance.GetNumberOfActivePlayer());
            }
        }

        private void NewPlayerDetected(int index, PlayerInput playerInput)
        {
            // var (newPlayerInputHandler, playerIndex) = ConnectPlayerToInputDevice(playerInput);

            var newPlayerInputHandler = PlayerManager.Instance.GetPlayerInputHandler();

            // _playerInputHandlers.Add(newPlayerInputHandler);

            CannonInstance newCannonInstance = Initializer.InstantiateCannon(gameModeData, _containers[0]);
            Initializer.SetCannonParameters(newCannonInstance, _containers[0], _ballTracker, gameModeData,
                gameData.playerDataList[index].mainScore, gameModeData.skinData.playersSkinData[index], this);
            newCannonInstance.SetInputParameters(newPlayerInputHandler);
            newCannonInstance.SetCannonInputEnabled(true);
            _cannons.Add(newCannonInstance);


            // Do custom stuff when a player joins in the lobby
            Color popupColor = gameModeData.skinData.playersSkinData[index].baseColor;
            AddPlayerJoinPopup(index, newCannonInstance, popupColor);

            ConnectToLobbyScore(gameData.playerDataList[index].mainScore, lobbyScore[index], popupColor);
        }

        private void ConnectToLobbyScore(IntReference scoreRef, Scoreboard scoreboard, Color color)
        {
            scoreboard.playerScore = scoreRef.Variable;
            scoreboard.connectedColor = color;
        }

        // private (PlayerInputSystem, int) ConnectPlayerToInputDevice(PlayerInput playerInput)
        // {
        //     var playerIndex = PlayerManager.Instance.GetNumberActivePlayer();
        //     if (playerIndex >= 4)
        //     {
        //         Debug.LogError("Something went awfully wrong, you're trying to register a fifth+ player");
        //         return (null, -1);
        //     }
        //
        //     var playerInputRegistered = playerInput.GetComponentInParent<PlayerInputSystem>();
        //     var newPlayerData = gameData.playerDataList[playerIndex];
        //
        //     // if (newPlayerData.playerIndexNumber != playerIndex)
        //     //     Debug.LogError($"Wrong player index when registering a new player, playerData : {{newPlayerData.playerIndexNumber}} and playerIndex : {playerIndex}");
        //     
        //     newPlayerData.SetInputParameters(playerInput.devices[0]);
        //
        //     return (playerInputRegistered, playerIndex);
        // }

        private void DisconnectPlayers()
        {
            foreach (var cannon in _cannons)
            {
                cannon.DestroyCurrentBall();
                Destroy(cannon.gameObject);
            }

            // foreach (var playerInputHandler in _playerInputHandlers)
            // {
            //     Destroy(playerInputHandler.gameObject);
            // }

            _cannons.Clear();
            // _playerInputHandlers.Clear();

            foreach (var playerData in gameData.playerDataList)
            {
                playerData.ResetInputParameters();
                playerData.ResetMainScore();
            }


            // _activePlayerNumber.Variable.SetValue(0);
        }

        private void AddPlayerJoinPopup(int playerIndex, CannonInstance cannonInstance, Color randColor)
        {
            var popup = Instantiate(onJoinPopup, cannonInstance.transform);
            var tmp = popup.GetComponent<TextMeshPro>();
            tmp.color = randColor;
            tmp.text = $"P{playerIndex + 1}";
        }

        public void OnBallFusion(BallInstance ballInstance)
        {
        }
    }
}