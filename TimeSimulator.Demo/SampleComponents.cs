using System;
using TimeSimulator.Core;

namespace TimeSimulator.Demo
{
    // A sample component that models a cache with expiration
    public class TemporalCache : ITemporalComponent
    {
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        private readonly TimeSpan _defaultExpiration;

        public TemporalCache(TimeSpan defaultExpiration)
        {
            _defaultExpiration = defaultExpiration;
        }

        public void Set(string key, object value, TimeSpan? expiration = null)
        {
            var expiresAt = DateTime.UtcNow + (expiration ?? _defaultExpiration);
            _cache[key] = new CacheEntry { Value = value, ExpiresAt = expiresAt };
            Console.WriteLine($"[Cache] Set '{key}' (expires: {expiresAt:HH:mm:ss})");
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
            {
                value = (T)entry.Value;
                Console.WriteLine($"[Cache] Hit: '{key}'");
                return true;
            }

            Console.WriteLine($"[Cache] Miss: '{key}'");
            return false;
        }

        public void OnTimeAdvanced(DateTime newTime, TimeSpan advancedDuration)
        {
            // Periodically clean expired entries
            var keysToRemove = _cache.Where(kv => kv.Value.ExpiresAt <= newTime)
                                    .Select(kv => kv.Key)
                                    .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                Console.WriteLine($"[Cache] Expired: '{key}'");
            }
        }

        private class CacheEntry
        {
            public object Value { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }

    // A sample component that models a scheduled job system
    public class ScheduledJobSystem : ITemporalComponent
    {
        private readonly List<ScheduledJob> _jobs = new List<ScheduledJob>();

        public void ScheduleRecurringJob(string name, TimeSpan interval, Action action)
        {
            var job = new ScheduledJob
            {
                Name = name,
                Interval = interval,
                Action = action,
                NextRunTime = DateTime.UtcNow + interval
            };

            _jobs.Add(job);
            Console.WriteLine($"[Scheduler] Scheduled job '{name}' to run every {interval.TotalHours} hours");
        }

        public void OnTimeAdvanced(DateTime newTime, TimeSpan advancedDuration)
        {
            foreach (var job in _jobs.Where(j => j.NextRunTime <= newTime).ToList())
            {
                Console.WriteLine($"[Scheduler] Running job '{job.Name}' at {newTime:HH:mm:ss}");

                try
                {
                    job.Action();
                    Console.WriteLine($"[Scheduler] Job '{job.Name}' completed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Scheduler] Job '{job.Name}' failed: {ex.Message}");
                }

                job.NextRunTime = newTime + job.Interval;
                Console.WriteLine($"[Scheduler] Job '{job.Name}' next run at {job.NextRunTime:HH:mm:ss}");
            }
        }

        private class ScheduledJob
        {
            public string Name { get; set; }
            public TimeSpan Interval { get; set; }
            public Action Action { get; set; }
            public DateTime NextRunTime { get; set; }
        }
    }
}