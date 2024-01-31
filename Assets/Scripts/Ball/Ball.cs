using System.Linq;
using MultiSuika.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MultiSuika.Ball
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] public SpriteRenderer spriteRenderer;
        [SerializeField] public Rigidbody2D rb2d;

        public int tier;
        public int scoreValue;
        public IntReference ballScoreRef;
        public BallSetData ballSetData;
        public BallSpriteThemeData ballSpriteThemeData;
        public Container.Container container;

        public float impulseMultiplier;
        public float impulseExpPower;
        public float impulseRangeMultiplier;

        public AK.Wwise.Event WwiseEventBallFuseT0;
        public AK.Wwise.Event WwiseEventBallFuseT1;
        public AK.Wwise.Event WwiseEventBallFuseT2;
        public AK.Wwise.Event WwiseEventBallFuseT3;
        public AK.Wwise.Event WwiseEventBallFuseT4;
        public AK.Wwise.Event WwiseEventBallFuseT5;
        public AK.Wwise.Event WwiseEventBallFuseT6;
        public AK.Wwise.Event WwiseEventBallFuseT7;
        public AK.Wwise.Event WwiseEventBallFuseT8;
        public AK.Wwise.Event WwiseEventBallFuseT9;
        public AK.Wwise.Event WwiseEventBallFuseT10;

        public void EnableCollision()
        {
            rb2d.simulated = true;
            ApplyRotationForce();
        }

        private int GetBallTier() => tier;

        private void ClearBall()
        {
            ballScoreRef?.Variable.ApplyChange(scoreValue);
            rb2d.simulated = false;
            Destroy(gameObject);
        }

        private void ApplyRotationForce()
        {
            var zRotationValue = Random.Range(0.1f, 0.2f) * (Random.Range(0, 2) * 2 - 1);
            rb2d.AddTorque(zRotationValue, ForceMode2D.Force);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.transform.CompareTag("Ball")) return;
            var otherBall = collision.gameObject.GetComponent<Ball>();
            if (otherBall.GetBallTier() == tier && gameObject.GetInstanceID() > otherBall.gameObject.GetInstanceID())
            {
                FuseWithOtherBall(otherBall, collision.GetContact(0).point);
            }
        }

        private void FuseWithOtherBall(Ball other, Vector3 contactPosition)
        {
            other.ClearBall();
            ClearBall();
            if (tier < ballSetData.GetMaxTier)
            {
                AddFusionImpulse(tier + 1, contactPosition);
                var newBall = Initializer.InstantiateBall(ballSetData, container,
                    Initializer.WorldToLocalPosition(container.containerParent.transform, contactPosition));
                Initializer.SetBallParameters(newBall, tier + 1, ballScoreRef, ballSetData, ballSpriteThemeData, container);
                CallFuseSFX(tier, newBall.gameObject);
            }
        }

        private void AddFusionImpulse(int newBallTier, Vector3 contactPosition)
        {
            float impulseRadius = ballSetData.GetBallData(newBallTier).scale / 2f;
            var ballsInRange =
                from raycast in Physics2D.CircleCastAll(contactPosition, impulseRadius * container.containerParent.transform.localScale.x * impulseRangeMultiplier, Vector2.zero, Mathf.Infinity,
                    LayerMask.GetMask("Ball"))
                select raycast.collider;
        

            foreach (var ball in ballsInRange)
            {
                Vector2 pushDirection = ball.transform.position - contactPosition;
                pushDirection.Normalize();

                float pushIntensity = Mathf.Pow(Mathf.Abs(impulseRadius * impulseRangeMultiplier - Vector2.Distance(ball.ClosestPoint(contactPosition), contactPosition)) * impulseMultiplier, impulseExpPower);

                ball.GetComponent<Rigidbody2D>().AddForce(pushIntensity * pushDirection, ForceMode2D.Impulse);
            }
        }
        private void CallFuseSFX(int tier, GameObject newBallObj)
        {
            switch (tier)
            {
                case 0:
                    WwiseEventBallFuseT0.Post(newBallObj);
                    break;
                case 1:
                    WwiseEventBallFuseT1.Post(newBallObj);
                    break;
                case 2:
                    WwiseEventBallFuseT2.Post(newBallObj);
                    break;
                case 3:
                    WwiseEventBallFuseT3.Post(newBallObj);
                    break;
                case 4:
                    WwiseEventBallFuseT4.Post(newBallObj);
                    break;
                case 5:
                    WwiseEventBallFuseT5.Post(newBallObj);
                    break;
                case 6:
                    WwiseEventBallFuseT6.Post(newBallObj);
                    break;
                case 7:
                    WwiseEventBallFuseT7.Post(newBallObj);
                    break;
                case 8:
                    WwiseEventBallFuseT8.Post(newBallObj);
                    break;
                case 9:
                    WwiseEventBallFuseT9.Post(newBallObj);
                    break;
                case 10:
                    WwiseEventBallFuseT10.Post(newBallObj);
                    break;
            }
        }
    }
}