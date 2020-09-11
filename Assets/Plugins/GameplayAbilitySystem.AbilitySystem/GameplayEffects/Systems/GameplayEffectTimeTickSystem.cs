﻿using GameplayAbilitySystem.AbilitySystem.GameplayEffects.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace GameplayAbilitySystem.AbilitySystem.GameplayEffects.Systems
{
    public class GameplayEffectTimeTickSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
        NativeQueue<Entity> ScheduledTicks;
        protected override void OnCreate()
        {
            m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            ScheduledTicks = new NativeQueue<Entity>(Allocator.Persistent);
            for (var i = 0; i < 10000; i++)
            {
                var entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity(typeof(TimeDurationComponent));
                SetComponent(entity, TimeDurationComponent.New(1f, 20f));
            }
        }

        protected override void OnUpdate()
        {
            var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
            ScheduledTicks.Clear();
            var ScheduledTicksWriter = ScheduledTicks.AsParallelWriter();
            var dt = Time.DeltaTime;

            Entities
                .ForEach((int entityInQueryIndex, Entity entity, ref TimeDurationComponent durationComponent) =>
                {
                    if (durationComponent.Tick(dt))
                    {
                        ScheduledTicksWriter.Enqueue(entity);
                    }

                    if (durationComponent.IsExpired()) ecb.DestroyEntity(entityInQueryIndex, entity);
                })
                .WithBurst()
                .ScheduleParallel();
        }
    }
}