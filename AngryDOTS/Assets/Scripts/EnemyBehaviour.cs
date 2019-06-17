using Unity.Entities;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class EnemyBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
	[Header("Movement")]
	public float speed = 2f;

    public float turnSpeed = 1.0f;

	[Header("Life Settings")]
	public float enemyHealth = 1f;

	Rigidbody rigidBody;


	void Start()
	{
		rigidBody = GetComponent<Rigidbody>();
	}

	void Update()
	{
		if (!Settings.IsPlayerDead())
		{
#if true
            Quaternion q1 = transform.rotation;
            Vector3 target_heading = Settings.PlayerPosition - transform.position;
            target_heading.y = 0f;
            target_heading.Normalize();
            Quaternion q2 = Quaternion.LookRotation(target_heading, Vector3.up);

            float angle = Quaternion.Angle(q1, q2) * Mathf.Deg2Rad;

            float turn_angle = turnSpeed * Time.deltaTime;

            if (turn_angle < Mathf.Abs(angle))
            {
                float t = turn_angle / Mathf.Abs(angle);
                Quaternion q3 = Quaternion.Slerp(q1, q2, t);

                transform.rotation = q3;
            }
            else
            {
                transform.rotation = q2;
            }
#else
            Vector3 heading = Settings.PlayerPosition - transform.position;
			heading.y = 0f;
			transform.rotation = Quaternion.LookRotation(heading);
#endif
		}

		Vector3 movement = transform.forward * speed * Time.deltaTime;
        if (rigidBody != null)
        {
            rigidBody.MovePosition(transform.position + movement);
        }
    }

	void OnTriggerEnter(Collider theCollider)
	{
		if (!theCollider.CompareTag("Bullet"))
			return;

		if(--enemyHealth <= 0)
		{
			Destroy(gameObject);
			BulletImpactPool.PlayBulletImpact(transform.position);
		}
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		manager.AddComponent(entity, typeof(EnemyTag));
		manager.AddComponent(entity, typeof(MoveForward));

        RogueUtils.TurnSpeed turn_speed = new RogueUtils.TurnSpeed { Value = turnSpeed };
        manager.AddComponentData(entity, turn_speed);

        MoveSpeed moveSpeed = new MoveSpeed { Value = speed };
		manager.AddComponentData(entity, moveSpeed);

		Health health = new Health { Value = enemyHealth };
		manager.AddComponentData(entity, health);
	}
}
