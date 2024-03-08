using System.Collections.Generic;
using System.Linq;
using MultiSuika.Ball;
using MultiSuika.GameLogic;
using MultiSuika.Player;
using MultiSuika.Skin;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using PlayerInputManager = MultiSuika.Player.PlayerInputManager;

namespace MultiSuika.Utilities
{
    public static class Initializer
    {
        #region Player

        public static List<PlayerInputManager> InstantiatePlayerInputHandlers(List<PlayerData> connectedPlayerData,
            GameModeData gameModeData)
        {
            List<PlayerInputManager> instantiatedPlayerInputHandlers = new List<PlayerInputManager>();
            foreach (var playerData in connectedPlayerData)
            {
                var playerInputObj = PlayerInput.Instantiate(gameModeData.playerInputPrefab,
                    playerData.playerIndexNumber,
                    pairWithDevice: playerData.inputDevice);
                instantiatedPlayerInputHandlers.Add(playerInputObj.GetComponentInParent<PlayerInputManager>());
            }

            return instantiatedPlayerInputHandlers;
        }

        #endregion

        #region Container

        public static List<Container.Container> InstantiateContainers(int playerCount,
            GameModeData gameModeData)
        {
            playerCount = playerCount <= 0 ? 1 : playerCount; // For cases like the lobby
            int containerToSpawn = DivideIntRoundedUp(playerCount, gameModeData.playerPerContainer);
            if (containerToSpawn <= 0)
                return null;

            List<Container.Container> instantiatedContainers = new List<Container.Container>();
            Vector2 distanceBetweenContainers = Vector2.zero;

            distanceBetweenContainers.x = (containerToSpawn > 1)
                ? Mathf.Abs(gameModeData.leftmostContainerPositions[containerToSpawn - 1].x) * 2f /
                  (containerToSpawn - 1)
                : 0f;

            Transform objHolder = GetObjectsHolder();
            
            for (int i = 0; i < containerToSpawn; i++)
            {
                Container.Container newContainer = Object.Instantiate(gameModeData.containerPrefab);
                ResetLocalTransform(newContainer.transform);

                instantiatedContainers.Add(newContainer);
                
                GameObject containerParent = new GameObject($"Container ({(i + 1)})");
                containerParent.transform.SetParent(objHolder, false);
                newContainer.ContainerParent = containerParent;
                
                containerParent.transform.position =
                    gameModeData.leftmostContainerPositions[containerToSpawn - 1] +
                    (i * distanceBetweenContainers);
                containerParent.transform.localScale =
                    Vector3.one * gameModeData.containerGeneralScaling[containerToSpawn - 1];
            }

            return instantiatedContainers;
        }

        public static void SetContainersParameters(List<Container.Container> containers, GameModeData gameModeData)
        {
            for (int i = 0; i < containers.Count; ++i)
                SetContainerParameters(containers[i], gameModeData.skinData.playersSkinData[i]);
        }

        private static void SetContainerParameters(Container.Container container, PlayerSkinData playerSkinData)
        {
            container.backgroundSpriteRenderer.sprite = playerSkinData.containerBackground;
            container.sideSpriteRenderer.sprite = playerSkinData.containerSide;
            container.failureSpriteRenderer.sprite = playerSkinData.containerFailure;
            container.successSpriteRenderer.sprite = playerSkinData.containerSuccess;
        }

        #endregion

        #region Cannon

        public static List<Cannon.Cannon> InstantiateCannons(int playerCount, GameModeData gameModeData,
            List<Container.Container> containers)
        {
            if (!containers.Any() || playerCount <= 0)
                return null;

            List<Cannon.Cannon> instantiatedCannons = new List<Cannon.Cannon>();

            for (int i = 0; i < playerCount; i++)
            {
                Container.Container cannonContainer =
                    containers[GetContainerIndexForPlayer(i, gameModeData.playerPerContainer)];
                instantiatedCannons.Add(InstantiateCannon(gameModeData, cannonContainer));
            }

            return instantiatedCannons;
        }

        public static Cannon.Cannon InstantiateCannon(GameModeData gameModeData, Container.Container container)
        {
            var newCannon = Object.Instantiate(gameModeData.cannonPrefab, container.ContainerParent.transform);
            ResetLocalTransform(newCannon.transform);

            float xPos = gameModeData.isCannonSpawnXPosRandom
                ? Random.Range(-container.GetContainerHorizontalHalfLength(),
                    container.GetContainerHorizontalHalfLength())
                : 0f;
            newCannon.transform.localPosition = new Vector2(xPos, gameModeData.cannonVerticalDistanceFromCenter);

            return newCannon;
        }

        public static void SetCannonsParameters(List<Cannon.Cannon> cannons, List<Container.Container> containers,
            BallTracker balltracker, GameModeData gameModeData,
            List<PlayerData> playerData, IGameModeManager gameModeManager)
        {
            for (int i = 0; i < cannons.Count; ++i)
            {
                SetCannonParameters(cannons[i],
                    containers[GetContainerIndexForPlayer(i, gameModeData.playerPerContainer)], balltracker,
                    gameModeData, playerData[i], gameModeData.skinData.playersSkinData[i], gameModeManager);
            }
        }

        public static void SetCannonParameters(Cannon.Cannon cannon, Container.Container container,
            BallTracker ballTracker, GameModeData gameModeData,
            PlayerData playerData, PlayerSkinData playerSkinData, IGameModeManager gameModeManager)
        {
            cannon.speed = gameModeData.cannonSpeed;
            cannon.reloadCooldown = gameModeData.cannonReloadCooldown;
            cannon.shootingForce = gameModeData.cannonShootingForce;
            cannon.emptyDistanceBetweenBallAndCannon = gameModeData.emptyDistanceBetweenBallAndCannon;
            cannon.isUsingPeggleMode = gameModeData.isCannonUsingPeggleMode;
            cannon.horizontalMargin = container.GetContainerHorizontalHalfLength();

            cannon.ballSetData = gameModeData.ballSetData;
            cannon.ballSpriteData = playerSkinData.ballTheme;
            cannon.scoreReference = playerData.mainScore;
            cannon.container = container;
            cannon.ballTracker = ballTracker;
            cannon.spriteRenderer.sprite = playerSkinData.cannonSprite;

            cannon.gameModeManager = gameModeManager;
        }

        public static void ConnectCannonsToPlayerInputs(List<Cannon.Cannon> cannons,
            List<PlayerInputManager> playerInputHandlers)
        {
            for (int i = 0; i < cannons.Count; ++i)
            {
                cannons[i].ConnectCannonToPlayer(playerInputHandlers[i]);
            }
        }

        #endregion

        #region Ball

        public static Ball.BallInstance InstantiateBall(BallSetData ballSetData, Container.Container container,
            Vector3 position, float randomRotationRange = 35f)
        {
            var newBall = Object.Instantiate(ballSetData.ballInstancePrefab, container.ContainerParent.transform);
            ResetLocalTransform(newBall.transform);

            newBall.transform.SetLocalPositionAndRotation(position,
                Quaternion.Euler(0f, 0f, Random.Range(-randomRotationRange, randomRotationRange)));

            return newBall;
        }

        public static void SetBallParameters(Ball.BallInstance ballInstance, int ballTierIndex, IntReference scoreRef,
            BallSetData ballSetData, BallTracker ballTracker, BallSpriteThemeData ballSpriteThemeData,
            Container.Container container, IGameModeManager gameModeManager, bool disableCollision = false)
        {
            var ballData = ballSetData.GetBallData(ballTierIndex);
            if (ballData == null)
            {
                Debug.LogError("Trying to spawn a ball with a tier that doesn't exist");
                Object.Destroy(ballInstance.gameObject);
            }

            ballInstance.spriteRenderer.sprite = ballSpriteThemeData.ballSprites[ballTierIndex];
            var ballTransform = ballInstance.transform;
            ballTransform.localScale = Vector3.one * ballData.scale;
            ballTransform.name = $"Ball T{ballInstance.tier} (ID: {ballInstance.transform.GetInstanceID()})";

            ballInstance.rb2d.mass = ballData.mass;
            var ballPhysMat = new PhysicsMaterial2D("ballPhysMat")
            {
                bounciness = ballSetData.bounciness,
                friction = ballSetData.friction
            };
            ballInstance.rb2d.sharedMaterial = ballPhysMat;

            ballInstance.tier = ballData.index;
            ballInstance.scoreValue = ballData.GetScoreValue();
            ballInstance.ballScoreRef = scoreRef;
            ballInstance.ballSetData = ballSetData;
            ballInstance.ballSpriteThemeData = ballSpriteThemeData;
            ballInstance.container = container;
            ballInstance.ballTracker = ballTracker;

            ballInstance.impulseMultiplier = ballSetData.impulseMultiplier;
            ballInstance.impulseExpPower = ballSetData.impulseExpPower;
            ballInstance.impulseRangeMultiplier = ballSetData.impulseRangeMultiplier;

            ballInstance.gameModeManager = gameModeManager;

            if (disableCollision)
                ballInstance.rb2d.simulated = false;
        }

        #endregion

        public static Vector3 WorldToLocalPosition(Transform relativeTargetTransform, Vector3 worldPosition) =>
            relativeTargetTransform.InverseTransformPoint(worldPosition);

        private static Transform GetObjectsHolder()
        {
            var objects = GameObject.Find("Objects");
            if (objects == null)
                objects = new GameObject($"Objects");
            return objects.transform;
        }
        
        private static int GetContainerIndexForPlayer(int playerIndex, int playerPerContainer) =>
            DivideIntRoundedUp(playerIndex + 1, playerPerContainer) - 1;

        private static int DivideIntRoundedUp(int a, int b) => a / b + (a % b > 0 ? 1 : 0);

        private static void ResetLocalTransform(Transform child)
        {
            child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            child.localScale = Vector3.one;
        }
    }
}