﻿using System;
using System.Diagnostics;

namespace OpenTracing.Tracer.Abstractions
{
    /// <summary>
    /// <para>The value of the system clock that DateTime.Now reads is only updated every 10-15 ms.
    /// This class uses a combination of <see cref="DateTime.UtcNow"/> and <see cref="Stopwatch"/>
    /// to calculate a more accurate timestamp.</para>
    /// <para>WARNING: The Stopwatch runs out of sync (by as much as half a second per hour)! This means,
    /// an instance of this class must be short-lived!
    /// (It could be re-synced (see SO link) but this would also lead to wrong calculations if it happens
    /// to be re-synced during a running span.</para>
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/questions/1416139/how-to-get-timestamp-of-tick-precision-in-net-c
    /// </remarks>
    public class HighResClock : IClock
    {
        private readonly DateTime _startTime = DateTime.UtcNow;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public virtual DateTime UtcNow =>_startTime.AddTicks(_stopwatch.Elapsed.Ticks);
    }
}