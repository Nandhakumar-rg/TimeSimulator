using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeSimulator.Core.EventTracking
{
    public class TemporalEvent
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Category { get; }
        public DateTime VirtualTimeStamp { get; }
        public DateTime ActualTimeStamp { get; }
        public object Data { get; }

        public TemporalEvent(string name, string category, DateTime virtualTimeStamp, object data = null)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Category = category ?? "General";
            VirtualTimeStamp = virtualTimeStamp;
            ActualTimeStamp = DateTime.UtcNow;
            Data = data;
        }
    }

    public class EventTracker
    {
        private readonly Simulator _simulator;
        private readonly List<TemporalEvent> _events = new List<TemporalEvent>();

        public EventTracker(Simulator simulator)
        {
            _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        }

        public void TrackEvent(string name, string category = "General", object data = null)
        {
            var evt = new TemporalEvent(name, category, _simulator.TimeProvider.UtcNow, data);
            _events.Add(evt);
        }

        public IEnumerable<TemporalEvent> GetEvents(string category = null)
        {
            if (string.IsNullOrEmpty(category))
                return _events;

            return _events.Where(e => e.Category == category);
        }

        public IEnumerable<TemporalEvent> GetEventsBetween(DateTime start, DateTime end)
        {
            return _events.Where(e => e.VirtualTimeStamp >= start && e.VirtualTimeStamp <= end);
        }

        public void Clear()
        {
            _events.Clear();
        }
    }
}