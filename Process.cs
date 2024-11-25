using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessViewer;

public record Process(uint PID, string ProcessImage, ProcessPriority Priority)
{}
