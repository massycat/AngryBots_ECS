using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using static Unity.Mathematics.math;

#if false
[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class ATestPhysicsCollisionSystem : JobComponentSystem
{
    BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    StepPhysicsWorld m_StepPhysicsWorldSystem;
    EntityQuery m_BulletsGroup;

    protected override void OnCreate()
    {
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        m_BulletsGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(TimeToLive), }
        });
    }

    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    [BurstCompile]
    struct CollisionEventeJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<TimeToLive> BulletsGroup;
        public ComponentDataFromEntity<Health> EnemyGroup;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.Entities.EntityA;
            Entity entityB = collisionEvent.Entities.EntityB;

#if true
            bool is_A_bullet = BulletsGroup.Exists(entityA);
            bool is_B_bullet = BulletsGroup.Exists(entityB);

            bool is_A_enemy = EnemyGroup.Exists(entityA);
            bool is_B_enemy = EnemyGroup.Exists(entityB);

            if (is_A_bullet && is_B_enemy)
            {
                var time_to_live = BulletsGroup[entityA];

                if (time_to_live.Value > 0.0f)
                {
                    var health = EnemyGroup[entityB];
                    health.Value = health.Value - 1;
                    EnemyGroup[entityB] = health;

                    time_to_live.Value = 0.0f;
                    BulletsGroup[entityA] = time_to_live;
                }
            }
            if (is_B_bullet && is_A_enemy)
            {
                var time_to_live = BulletsGroup[entityB];

                if (time_to_live.Value > 0.0f)
                {
                    var health = EnemyGroup[entityA];
                    health.Value = health.Value - 1;
                    EnemyGroup[entityA] = health;

                    time_to_live.Value = 0.0f;
                    BulletsGroup[entityB] = time_to_live;
                }
            }
#endif
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var bullet_group = GetComponentDataFromEntity<TimeToLive>();
        var enemy_group = GetComponentDataFromEntity<Health>();
        JobHandle jobHandle = new CollisionEventeJob
        {
            BulletsGroup = bullet_group,
            EnemyGroup = enemy_group
        }.Schedule(m_StepPhysicsWorldSystem.Simulation,
                    ref m_BuildPhysicsWorldSystem.PhysicsWorld, inputDependencies);

        // Now that the job is set up, schedule it to be run. 
        return jobHandle;
    }
}
#endif
