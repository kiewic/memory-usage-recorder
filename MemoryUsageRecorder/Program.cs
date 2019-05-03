using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MemoryUsageRecorder
{
    class Program
    {
        static void Main(string[] args)
        {
            // First argument should be the process of a name, for example, 'notepad'
            var processName = args[0];

            var service = new MemoryUsageLogService();
            int time = 0;
            bool exitRequested = false;

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine(service);
                exitRequested = true;
            };

            while (exitRequested == false)
            {
                var label = time.ToString();
                uint[] processIds = FindProcessIdByName(processName);
                Console.WriteLine("{0}; {1}; {2}", args[0], String.Join(",", processIds), label);

                service.TakeMemoryUsageSnapshot(processIds, label);
                Thread.Sleep(1000);
                time += 1000;
            }
        }

        private static uint[] FindProcessIdByName(string arg)
        {
            var processes = Process.GetProcessesByName(arg);
            return processes.Select(x => (uint)x.Id).ToArray();
        }
    }
}
