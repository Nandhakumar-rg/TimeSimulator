using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimeSimulator.Core.EventTracking;

namespace TimeSimulator.Core
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }

    public class VirtualTimeProvider : ITimeProvider
    {
        private readonly DateTime _startTime;
        private TimeSpan _elapsedTime;
        private readonly object _lock = new object();

        public VirtualTimeProvider(DateTime? startTime = null)
        {
            _startTime = startTime ?? DateTime.UtcNow;
            _elapsedTime = TimeSpan.Zero;
        }

        public DateTime Now => _startTime.ToLocalTime() + _elapsedTime;
        public DateTime UtcNow => _startTime + _elapsedTime;

        public void Advance(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
                throw new ArgumentException("Cannot advance time backwards", nameof(timeSpan));

            lock (_lock)
            {
                _elapsedTime += timeSpan;
            }
        }
    }

    public class Simulator : IDisposable
    {
        private readonly VirtualTimeProvider _timeProvider;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private readonly List<ITemporalComponent> _registeredComponents = new List<ITemporalComponent>();
        private bool _disposed;

        public double AccelerationFactor { get; set; } = 1.0;
        public bool RecordEvents { get; set; } = true;
        public EventTracker EventTracker { get; }

        public Simulator(DateTime? startTime = null)
        {
            _timeProvider = new VirtualTimeProvider(startTime);
            EventTracker = new EventTracker(this);
        }

        public ITimeProvider TimeProvider => _timeProvider;

        public void Register(ITemporalComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            _registeredComponents.Add(component);

            if (RecordEvents)
            {
                EventTracker.TrackEvent(
                    $"Component registered: {component.GetType().Name}",
                    "Registration",
                    component);
            }
        }

        public async Task RunFor(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("Duration must be positive", nameof(duration));

            if (RecordEvents)
            {
                EventTracker.TrackEvent(
                    "Simulation started",
                    "Simulation",
                    new { Duration = duration });
            }

            var steps = Math.Max(1, (int)(duration.TotalMilliseconds / 10));
            var stepSize = TimeSpan.FromTicks(duration.Ticks / steps);

            for (int i = 0; i < steps; i++)
            {
                if (_cancellationSource.Token.IsCancellationRequested)
                    break;

                // Advance virtual time
                _timeProvider.Advance(stepSize);

                // Notify registered components
                NotifyComponents(stepSize);

                // Simulate real-time delay based on acceleration factor
                var realTimeDelay = TimeSpan.FromMilliseconds(stepSize.TotalMilliseconds / AccelerationFactor);
                await Task.Delay(realTimeDelay, _cancellationSource.Token).ConfigureAwait(false);
            }

            if (RecordEvents)
            {
                EventTracker.TrackEvent(
                    "Simulation completed",
                    "Simulation",
                    new { EndTime = _timeProvider.UtcNow });
            }
        }

        private void NotifyComponents(TimeSpan advancedDuration)
        {
            foreach (var component in _registeredComponents)
            {
                try
                {
                    component.OnTimeAdvanced(_timeProvider.UtcNow, advancedDuration);
                }
                catch (Exception ex)
                {
                    if (RecordEvents)
                    {
                        EventTracker.TrackEvent(
                            $"Component error: {component.GetType().Name}",
                            "Error",
                            new { Exception = ex.Message });
                    }
                }
            }
        }

        public void Stop()
        {
            _cancellationSource.Cancel();

            if (RecordEvents)
            {
                EventTracker.TrackEvent("Simulation stopped", "Simulation");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cancellationSource.Cancel();
                _cancellationSource.Dispose();
                _disposed = true;
            }
        }
    }
}