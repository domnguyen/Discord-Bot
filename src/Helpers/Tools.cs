using System;
using System.Diagnostics;

namespace WhalesFargo
{
    public static class Tools
    {
        // Check if a process is running or not.
        public static bool IsRunning(this Process process)
        {
            try { Process.GetProcessById(process.Id); }
            catch (InvalidOperationException) { return false; }
            catch (ArgumentException) { return false; }
            return true;
        }
    }
}
