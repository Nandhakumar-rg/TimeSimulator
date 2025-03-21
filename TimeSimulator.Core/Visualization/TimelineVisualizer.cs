using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSimulator.Core.EventTracking;

namespace TimeSimulator.Core.Visualization
{
    public class TimelineVisualizer
    {
        private readonly Simulator _simulator;

        public TimelineVisualizer(Simulator simulator)
        {
            _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        }

        public string GenerateTimeline()
        {
            var events = _simulator.EventTracker.GetEvents().OrderBy(e => e.VirtualTimeStamp).ToList();
            if (!events.Any())
                return "No events recorded.";

            var startTime = events.First().VirtualTimeStamp;
            var endTime = events.Last().VirtualTimeStamp;
            var duration = endTime - startTime;

            var sb = new StringBuilder();
            sb.AppendLine($"Timeline ({startTime:yyyy-MM-dd HH:mm:ss} to {endTime:yyyy-MM-dd HH:mm:ss})");
            sb.AppendLine($"Total duration: {FormatTimeSpan(duration)}");
            sb.AppendLine(new string('-', 80));
            sb.AppendLine($"{"Time":22} | {"Elapsed":14} | {"Category":15} | {"Event":30}");
            sb.AppendLine(new string('-', 80));

            foreach (var evt in events)
            {
                var elapsed = evt.VirtualTimeStamp - startTime;
                sb.AppendLine($"{evt.VirtualTimeStamp:yyyy-MM-dd HH:mm:ss} | {FormatTimeSpan(elapsed):14} | {evt.Category:15} | {evt.Name:30}");
            }

            return sb.ToString();
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{timeSpan.Days}d {timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            else if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds / 10:00}";
            else
                return $"{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}s";
        }

        public Dictionary<string, int> GenerateCategoryStatistics()
        {
            return _simulator.EventTracker.GetEvents()
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}