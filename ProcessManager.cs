using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.CodeDom;
using System.Diagnostics;

namespace ProcessViewer;

internal class ProcessManager
{
    static ProcessManager? instance;
    public static ProcessManager Instance
    {
        get { return instance ??= new(); }
    }

    public IEnumerable<Process> Processes { get; private set; }

    private ProcessManager()
    {
        Processes = []; 
    }

    const uint PROCESS_QUERY_INFORMATION = 0x0400;
    const uint PROCESS_SET_INFORMATION = 0x0200;

    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool EnumProcesses([Out] uint[] processIds, uint arraySizeBytes, out uint bytesReturned);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint processAccess, bool inheritHandle, uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName(IntPtr hProcess, int flags, StringBuilder lpExeName, ref int lpdwSize);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint GetPriorityClass(IntPtr hProcess);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetPriorityClass(IntPtr hProcess, uint priorityClass);

    IEnumerable<Process> GetProcesses()
    {
        uint bufferSize = 512;
        uint[] processIDs;
        uint bytesNeeded;

        do
        {
            bufferSize *= 2;
            processIDs = new uint[bufferSize];
            if (!EnumProcesses(processIDs, (uint)processIDs.Length * sizeof(uint), out bytesNeeded))
                return [];
        } while (bytesNeeded > bufferSize);

        List<Process> processes = [];
        foreach (uint processID in processIDs)
        {
            IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_SET_INFORMATION, false, processID);
            if (processHandle == IntPtr.Zero) 
                continue;

            StringBuilder processImageName = new(4096);
            int size = processImageName.Capacity;
            if (!QueryFullProcessImageName(processHandle, 0, processImageName, ref size)) 
                goto CloseHandle;

            ProcessPriority priority = (ProcessPriority)GetPriorityClass(processHandle);
            if (priority == ProcessPriority.Unknown)
                goto CloseHandle;
            

            Process process = new(processID, processImageName.ToString(), priority);
            processes.Add(process);

            CloseHandle:
                CloseHandle(processHandle);
        }

        return processes;
    }

    public void RefreshProcesses()
    {
        Processes = GetProcesses();
    }

    public bool SetPriority(Process process, ProcessPriority priority)
    {
        IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_SET_INFORMATION, false, process.PID);
        if (processHandle == IntPtr.Zero) 
            return false;

        bool success = SetPriorityClass(processHandle, (uint)priority);

        if (priority == ProcessPriority.Realtime 
            && GetPriorityClass(processHandle) != (uint)ProcessPriority.Realtime)
            success = false;

        CloseHandle(processHandle);
        return success;
    }
}
