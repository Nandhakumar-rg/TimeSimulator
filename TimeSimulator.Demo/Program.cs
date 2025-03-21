using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSimulator.Core;
using TimeSimulator.Core.Interceptors;
using TimeSimulator.Core.Visualization;

namespace TimeSimulator.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Temporal Simulator - Advanced Demo");
            Console.WriteLine("==================================");

            // Create simulator with 1000x acceleration
            using var simulator = new Simulator();
            simulator.AccelerationFactor = 1000;
            simulator.RecordEvents = true;

            // Initialize the context
            SimulatorContext.Initialize(simulator);

            var realStartTime = DateTime.Now;
            var virtualStartTime = DateTimeInterceptor.Now;

            Console.WriteLine($"Actual start time: {realStartTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Virtual start time: {virtualStartTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Acceleration: {simulator.AccelerationFactor}x");

            // Create and register temporal components
            var cache = new TemporalCache(TimeSpan.FromHours(2));
            var scheduler = new ScheduledJobSystem();

            simulator.Register(cache);
            simulator.Register(scheduler);

            // Configure components
            cache.Set("user:123", new { Name = "John", LastLogin = DateTime.Now });
            cache.Set("settings", new { Theme = "Dark", Language = "en-US" }, TimeSpan.FromHours(24));

            int reportCount = 0;
            scheduler.ScheduleRecurringJob("DailyReport", TimeSpan.FromHours(24), () => {
                reportCount++;
                Console.WriteLine($"[Report] Generated daily report #{reportCount}");
            });

            scheduler.ScheduleRecurringJob("CacheCleanup", TimeSpan.FromHours(1), () => {
                Console.WriteLine("[Maintenance] Performing cache maintenance...");
            });

            // Run simulation for multiple days
            Console.WriteLine("\nRunning simulation for 5 days...");
            await simulator.RunFor(TimeSpan.FromDays(5));

            // Test cache after time advancement
            if (cache.TryGet<dynamic>("user:123", out var user))
            {
                Console.WriteLine($"User still in cache: {user.Name}");
            }
            else
            {
                Console.WriteLine("User cache expired");
            }

            if (cache.TryGet<dynamic>("settings", out var settings))
            {
                Console.WriteLine($"Settings still in cache: {settings.Theme}");
            }
            else
            {
                Console.WriteLine("Settings cache expired");
            }

            // Display summary
            var virtualEndTime = DateTimeInterceptor.Now;
            var realEndTime = DateTime.Now;

            Console.WriteLine($"\nSimulation complete!");
            Console.WriteLine($"Virtual time elapsed: {virtualEndTime - virtualStartTime}");
            Console.WriteLine($"Real time elapsed: {realEndTime - realStartTime}");

            // Generate and display timeline
            var visualizer = new TimelineVisualizer(simulator);
            Console.WriteLine("\nEvent Statistics:");
            foreach (var stat in visualizer.GenerateCategoryStatistics())
            {
                Console.WriteLine($"- {stat.Key}: {stat.Value} events");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();

            // Reset simulator context
            SimulatorContext.Reset();
        }
    }
}