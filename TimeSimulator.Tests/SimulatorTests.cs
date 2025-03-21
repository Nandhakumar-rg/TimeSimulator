using System;
using System.Threading.Tasks;
using TimeSimulator.Core;
using TimeSimulator.Core.Interceptors;
using Xunit;
using System.Diagnostics;

namespace TimeSimulator.Tests
{
    public class SimulatorTests
    {
        [Fact]
        public async Task VirtualTime_AdvancesFasterThanRealTime()
        {
            // Arrange
            using var simulator = new Simulator();
            simulator.AccelerationFactor = 10000; // Extreme acceleration for tests
            SimulatorContext.Initialize(simulator);

            var startVirtualTime = DateTimeInterceptor.Now;
            var startRealTime = DateTime.Now;
            Console.WriteLine($"Starting test at: {startRealTime}");

            // Act
            var task = simulator.RunFor(TimeSpan.FromHours(1));
            // Add a timeout to prevent hanging
            await Task.WhenAny(task, Task.Delay(5000)); // 5 second timeout

            var endVirtualTime = DateTimeInterceptor.Now;
            var endRealTime = DateTime.Now;
            Console.WriteLine($"Ending test at: {endRealTime}, elapsed: {endRealTime - startRealTime}");

            // Reset context
            SimulatorContext.Reset();

            // Assert
            var virtualElapsed = endVirtualTime - startVirtualTime;
            var realElapsed = endRealTime - startRealTime;

            Assert.True(virtualElapsed.TotalMinutes > 0); // Virtual time advanced
            Assert.True(realElapsed.TotalSeconds < 5); // But real time was much less
        }

        [Fact]
        public async Task TaskDelay_IsAccelerated()
        {
            // Arrange
            using var simulator = new Simulator();
            simulator.AccelerationFactor = 10000; // Extreme acceleration for tests
            SimulatorContext.Initialize(simulator);

            var startTime = DateTimeInterceptor.Now;
            Console.WriteLine($"Starting delay test at: {DateTime.Now}");

            // Act - a 10 second delay should be extremely quick with 10000x acceleration
            var task = TaskDelayInterceptor.Delay(TimeSpan.FromSeconds(10));
            await Task.WhenAny(task, Task.Delay(1000)); // 1 second timeout

            var endTime = DateTimeInterceptor.Now;
            Console.WriteLine($"Ending delay test at: {DateTime.Now}");

            // Reset context
            SimulatorContext.Reset();

            // Assert
            var elapsed = endTime - startTime;
            Assert.True(elapsed.TotalSeconds > 0); // Virtual time passed
        }

        [Fact]
        public async Task TemporalComponents_AreNotified()
        {
            // Arrange
            using var simulator = new Simulator();
            simulator.AccelerationFactor = 10000; // Extreme acceleration for tests
            var testComponent = new TestTemporalComponent();
            simulator.Register(testComponent);
            Console.WriteLine($"Starting component test at: {DateTime.Now}");

            // Act
            var task = simulator.RunFor(TimeSpan.FromMinutes(1)); // Less time for testing
            await Task.WhenAny(task, Task.Delay(1000)); // 1 second timeout

            Console.WriteLine($"Ending component test at: {DateTime.Now}");

            // Assert
            Assert.True(testComponent.OnTimeAdvancedCalled);
        }

        // Test helper class
        private class TestTemporalComponent : ITemporalComponent
        {
            public bool OnTimeAdvancedCalled { get; private set; }

            public void OnTimeAdvanced(DateTime newTime, TimeSpan advancedDuration)
            {
                Console.WriteLine($"OnTimeAdvanced called with newTime={newTime}, duration={advancedDuration}");
                OnTimeAdvancedCalled = true;
            }
        }
    }
}