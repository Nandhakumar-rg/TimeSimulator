using System;

namespace TimeSimulator.Core.Interceptors
{
    public static class DateTimeInterceptor
    {
        private static Simulator _simulator;

        public static void Initialize(Simulator simulator)
        {
            _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        }

        public static DateTime Now => _simulator?.TimeProvider.Now ?? DateTime.Now;

        public static DateTime UtcNow => _simulator?.TimeProvider.UtcNow ?? DateTime.UtcNow;
    }
}