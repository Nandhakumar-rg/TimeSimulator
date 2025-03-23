Let's update the project with a README file and the necessary metadata in the .csproj file before publishing to NuGet.

### Step 1: Create a README.md file

Create a file named `README.md` in the root directory of your project (C:\Users\Nandha5321\source\TimeSimulator):

```
# TimeSimulator

A .NET library for simulating accelerated time execution to test time-dependent systems without waiting for real time to pass.

## Features

- Virtual time acceleration (run days of simulation in seconds)
- Intercepts .NET time operations (DateTime.Now, Task.Delay, etc.)
- Component registration for time-dependent objects
- Event tracking and visualization for time-based patterns
- Easily test caching, scheduling, and other time-dependent behaviors

## Installation

```bash
dotnet add package TimeSimulator
```

## Quick Start

```csharp
// Create simulator with 1000x acceleration
using var simulator = new Simulator();
simulator.AccelerationFactor = 1000;

// Initialize time interceptors
SimulatorContext.Initialize(simulator);

// Now use standard .NET time operations with accelerated time
Console.WriteLine($"Starting at: {DateTimeInterceptor.Now}");

// This will only take a few milliseconds in real time
await TaskDelayInterceptor.Delay(TimeSpan.FromHours(1));

Console.WriteLine($"After 1 hour: {DateTimeInterceptor.Now}");

// Run simulation forward
await simulator.RunFor(TimeSpan.FromDays(7));

Console.WriteLine($"After 7 days: {DateTimeInterceptor.Now}");

// Don't forget to reset when done
SimulatorContext.Reset();
```

## Creating Time-Aware Components

Implement the `ITemporalComponent` interface to create components that respond to time changes:

```csharp
public class CacheSystem : ITemporalComponent
{
    // Your implementation
    
    public void OnTimeAdvanced(DateTime newTime, TimeSpan advancedDuration)
    {
        // Handle time advancement, e.g., expire cache entries
    }
}

// Register with simulator
simulator.Register(myCache);
```

## Use Cases

- Test caching systems with expiration
- Verify scheduled job execution over long periods
- Identify time-based race conditions
- Test time-dependent behaviors without long waits
- Simulate complex time-based patterns



