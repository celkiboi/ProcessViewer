using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessViewer;

public enum ProcessPriority
{
    Idle = 0x00000040,
    BelowNormal = 0x00004000,
    Normal = 0x00000020,
    AboveNormal = 0x00008000,
    High = 0x00000080,
    Realtime = 0x00000100,
    Unknown = 0
}

