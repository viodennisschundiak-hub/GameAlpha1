using Bobo.Achievements;
using Bobo.Persistence;
using Bobo.Rendering;
using Bobo.Simulation;
using UnityEngine;
using UnityEngine.Events;

namespace Bobo.UI
{
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private SimulationSettings settings;
        [SerializeField] private WorldRenderer worldRenderer;
        [SerializeField] private AchievementManager achievementManager;
        [SerializeField] private SaveSystem saveSystem;
        [SerializeField] private UnityEvent onTick;

        public SimulationCore Simulation { get; private set; }

        private float _tickTimer;
        private float _ticksPerSecond = 10f;
        private bool _isRunning = true;

        private void Awake()
        {
            Simulation = new SimulationCore(settings);
            if (worldRenderer != null)
            {
                worldRenderer.Initialize(Simulation);
            }

            if (achievementManager != null)
            {
                achievementManager.Initialize(Simulation);
            }

            if (saveSystem != null)
            {
                saveSystem.Initialize(Simulation);
            }
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            _tickTimer += Time.deltaTime;
            float tickInterval = 1f / Mathf.Max(1f, _ticksPerSecond);
            while (_tickTimer >= tickInterval)
            {
                _tickTimer -= tickInterval;
                Simulation.Tick();
                onTick?.Invoke();
            }
        }

        public void TogglePlayPause()
        {
            _isRunning = !_isRunning;
        }

        public void Step()
        {
            Simulation.Tick();
            onTick?.Invoke();
        }

        public void ResetWorld()
        {
            Simulation.Reset();
        }

        public void SetTicksPerSecond(float value)
        {
            _ticksPerSecond = Mathf.Clamp(value, 1f, 60f);
        }

        public void SetSimulationParameter(string parameter, float value)
        {
            switch (parameter)
            {
                case "gain_free":
                    settings.gainFree = value;
                    break;
                case "cost_alive":
                    settings.costAlive = value;
                    break;
                case "cost_crowd":
                    settings.costCrowd = value;
                    break;
                case "reproduce_threshold":
                    settings.reproduceThreshold = value;
                    break;
                case "newborn_energy":
                    settings.newbornEnergy = value;
                    break;
            }
        }

        public void SetSimulationParameterInt(string parameter, int value)
        {
            switch (parameter)
            {
                case "free_threshold":
                    settings.freeThreshold = value;
                    break;
                case "move_stress_threshold":
                    settings.moveStressThreshold = value;
                    break;
            }
        }
    }
}
