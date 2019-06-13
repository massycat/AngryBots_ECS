using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(MoveForwardSystem))]
public class TurnTowardsPlayerSystem : JobComponentSystem
{
	[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
	struct TurnJob : IJobForEach<Translation, Rotation, RogueUtils.TurnSpeed>
	{
		public float3 playerPosition;
        public float dt;

        private quaternion Nlerp(quaternion q1, quaternion q2, float t)
        {
            float dt = math.dot(q1, q2);
            if (dt < 0.0f)
            {
                q2.value = -q2.value;
            }

            return math.normalize(math.quaternion(math.lerp(q1.value, q2.value, t)));
        }

        private quaternion Slerp(quaternion q1, quaternion q2, float t)
        {
            float dt = math.dot(q1, q2);
            if (dt < 0.0f)
            {
                dt = -dt;
                q2.value = -q2.value;
            }

            if (dt < 0.9995f)
            {
                float angle = math.acos(dt);
                float s = math.rsqrt(1.0f - dt * dt);    // 1.0f / sin(angle)
                float w1 = math.sin(angle * (1.0f - t)) * s;
                float w2 = math.sin(angle * t) * s;
                return math.quaternion(q1.value * w1 + q2.value * w2);
            }
            else
            {
                // if the angle is small, use linear interpolation
                return Nlerp(q1, q2, t);
            }
        }

        private static float Angle(quaternion a, quaternion b)
        {
            float f = math.dot(a, b);
            return math.acos(math.min(math.abs(f), 1f)) * 2f;// * 57.29578f;
        }

        public void Execute([ReadOnly] ref Translation pos, ref Rotation rot, [ReadOnly] ref RogueUtils.TurnSpeed turn_speed)
        {
#if true
#if false
            Quaternion q1 = rot.Value;
            float3 target_heading = playerPosition - pos.Value;
            target_heading.y = 0f;
            target_heading = math.normalize(target_heading);
            Quaternion q2 = quaternion.LookRotation(target_heading, math.up());

            float angle = Quaternion.Angle(q1, q2) * Mathf.Deg2Rad;

            float turn_angle = 1.0f * dt;// turn_speed.Value * dt;

            if (turn_angle < math.abs(angle))
            {
                float t = turn_angle / math.abs(angle);
                Quaternion q3 = Quaternion.Slerp(q1, q2, t);

                rot.Value = q3;
            }
            else
            {
                rot.Value = q2;
            }
#else
            quaternion q1 = rot.Value;

            float3 target_heading = playerPosition - pos.Value;
            target_heading.y = 0f;
            target_heading = math.normalize(target_heading);
            quaternion q2 = quaternion.LookRotation(target_heading, math.up());

            float angle = Angle(q1, q2);

            float turn_angle = turn_speed.Value * dt;

            if (turn_angle < math.abs(angle))
            {
                rot.Value = Slerp(q1, q2, turn_angle / math.abs(angle));
            }
            else
            {
                rot.Value = q2;
            }
#endif
#else
            float3 heading = playerPosition - pos.Value;
            heading.y = 0f;
            rot.Value = quaternion.LookRotation(heading, math.up());
#endif
        }
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		if (Settings.IsPlayerDead())
			return inputDeps;

        var job = new TurnJob
        {
            playerPosition = Settings.PlayerPosition,
            dt = Time.deltaTime
		};

		return job.Schedule(this, inputDeps);
	}
}

