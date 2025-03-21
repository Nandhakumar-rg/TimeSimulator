using System;
using System.Threading.Tasks;
using TimeSimulator.Core.Interceptors;

namespace TimeSimulator.Core
{
    public static class SimulatorContext
    {
        private static Simulator _currentSimulator;

        public static void Initialize(Simulator simulator)
        {
            _currentSimulator = simulator ?? throw new ArgumentNullException(nameof(simulator));

            // Initialize all interceptors
            TaskDelayInterceptor.Initialize(simulator);
            DateTimeInterceptor.Initialize(simulator);
        }

        public static Simulator Current => _currentSimulator;

        public static void Reset()
        {
            _currentSimulator = null;
        }

        public static async Task<T> RunWithVirtualTime<T>(Func<Task<T>> action, double? accelerationFactor = null)
        {
            using (var simulator = new Simulator())
            {
                if (accelerationFactor.HasValue)
                {
                    simulator.AccelerationFactor = accelerationFactor.Value;
                }

                Initialize(simulator);
                try
                {
                    return await action();
                }
                finally
                {
                    Reset();
                }
            }
        }

        public static async Task RunWithVirtualTime(Func<Task> action, double? accelerationFactor = null)
        {
            using (var simulator = new Simulator())
            {
                if (accelerationFactor.HasValue)
                {
                    simulator.AccelerationFactor = accelerationFactor.Value;
                }

                Initialize(simulator);
                try
                {
                    await action();
                }
                finally
                {
                    Reset();
                }
            }
        }
    }
}