using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

#if false
[UpdateAfter(typeof(MoveForwardSystem))]
[UpdateBefore(typeof(TimedDestroySystem))]
public class CollisionSystem : JobComponentSystem
{
	EntityQuery enemyGroup;
	EntityQuery bulletGroup;
	EntityQuery playerGroup;

	protected override void OnCreate()
	{
		playerGroup = GetEntityQuery(typeof(Health), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PlayerTag>());
		enemyGroup = GetEntityQuery(typeof(Health), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());
		bulletGroup = GetEntityQuery(typeof(TimeToLive), ComponentType.ReadOnly<Translation>());
	}

	[BurstCompile]
	struct CollisionJob : IJobChunk
	{
		public float radius;

        public bool doTimeToLive;

		public ArchetypeChunkComponentType<Health> healthType;
        public ArchetypeChunkComponentType<TimeToLive> timeToLiveType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;

		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Translation> transToTestAgainst;

        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<ArchetypeChunk> bullerChucks;


		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0f;
				Health health = chunkHealths[i];
				Translation pos = chunkTranslations[i];

                if (doTimeToLive)
                {
                    for (int b_chuck_i = 0; b_chuck_i < bullerChucks.Length; b_chuck_i++)
                    {
                        var b_chuck = bullerChucks[b_chuck_i];

                        var b_trans = b_chuck.GetNativeArray(translationType);
                        var b_times_to_lives = b_chuck.GetNativeArray(timeToLiveType);

                        for (int b_i = 0; b_i < b_chuck.Count; b_i++)
                        {
                            TimeToLive time_to_live = b_times_to_lives[b_i];

                            if (time_to_live.Value > 0.0f)
                            {
                                Translation pos2 = b_trans[b_i];

                                if (CheckCollision(pos.Value, pos2.Value, radius))
                                {
                                    damage += 1;

                                    time_to_live.Value = 0.0f;
                                    b_times_to_lives[b_i] = time_to_live;
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < transToTestAgainst.Length; j++)
                    {
                        Translation pos2 = transToTestAgainst[j];

                        if (CheckCollision(pos.Value, pos2.Value, radius))
                        {
                            damage += 1;
                        }
                    }
                }

				if (damage > 0)
				{
					health.Value -= damage;
					chunkHealths[i] = health;
				}
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		var healthType = GetArchetypeChunkComponentType<Health>(false);
        var timeToLiveType = GetArchetypeChunkComponentType<TimeToLive>(false);
		var translationType = GetArchetypeChunkComponentType<Translation>(true);

		float enemyRadius = Settings.EnemyCollisionRadius;
		float playerRadius = Settings.PlayerCollisionRadius;

        var time_to_lives = bulletGroup.ToComponentDataArray<TimeToLive>(Allocator.TempJob);
        for (int i = 0; i < time_to_lives.Length; i++)
        {
            var time_to_live = time_to_lives[i];
            time_to_live.Value = 0;
            time_to_lives[i] = time_to_live;
        }
        time_to_lives.Dispose();


        var jobEvB = new CollisionJob()
		{
			radius = enemyRadius * enemyRadius,
            doTimeToLive = true,
            healthType = healthType,
            timeToLiveType = timeToLiveType,
            translationType = translationType,
			transToTestAgainst = bulletGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            bullerChucks = bulletGroup.CreateArchetypeChunkArray(Allocator.TempJob)
        };

		JobHandle jobHandle = jobEvB.Schedule(enemyGroup, inputDependencies);

		if (Settings.IsPlayerDead())
			return jobHandle;

		var jobPvE = new CollisionJob()
		{
			radius = playerRadius * playerRadius,
            doTimeToLive = false,
            healthType = healthType,
            timeToLiveType = timeToLiveType,
            translationType = translationType,
			transToTestAgainst = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            bullerChucks = bulletGroup.CreateArchetypeChunkArray(Allocator.TempJob)
        };

		return jobPvE.Schedule(playerGroup, jobHandle);
	}

	static bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
	{
		float3 delta = posA - posB;
		float distanceSquare = delta.x * delta.x + delta.z * delta.z;

		return distanceSquare <= radiusSqr;
	}
}
#endif
