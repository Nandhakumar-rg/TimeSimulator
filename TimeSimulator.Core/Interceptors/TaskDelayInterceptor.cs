using System;
using System.Threading;
using System.Threading.Tasks;

namespace TimeSimulator.Core.Interceptors
{
    public static class TaskDelayInterceptor
    {
        private static Simulator _simulator;

        public static void Initialize(Simulator simulator)
        {
            _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        }

        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            if (_simulator == null)
                return Task.Delay(delay, cancellationToken);

            var tcs = new TaskCompletionSource<bool>();

            // Calculate how much real time to wait based on the acceleration factor
            var realTimeDelay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds / _simulator.AccelerationFactor);

            // First, advance the virtual time
            if (_simulator is Simulator sim && sim.TimeProvider is VirtualTimeProvider vtp)
            {
                vtp.Advance(delay);
            }

            // Then wait for the real time to catch up
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(realTimeDelay, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                        tcs.TrySetResult(true);
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken = default)
        {
            return Delay(TimeSpan.FromMilliseconds(millisecondsDelay), cancellationToken);
        }
    }
}