using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSimulator.Core
{
    public interface ITemporalComponent
    {
        void OnTimeAdvanced(DateTime newTime, TimeSpan advancedDuration);
    }
}
