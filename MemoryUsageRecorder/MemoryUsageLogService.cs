using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MemoryUsageRecorder
{
    class MemoryUsageLogService
    {
        private readonly Queue<Snapshot> snapshots;

        public int MaxSnapshotCount { get; set; }
        public string JsonObject { get; private set; }

        public MemoryUsageLogService()
        {
            this.MaxSnapshotCount = 1000; // TODO: Throw an error and print results?
            this.snapshots = new Queue<Snapshot>();
        }

        public void TakeMemoryUsageSnapshot(uint[] processIds, string label)
        {
            CreateSnapshot(processIds, label);
            if (this.snapshots.Count > this.MaxSnapshotCount)
            {
                this.snapshots.Dequeue();
            }
        }

        public override string ToString()
        {
            var root = new MemoryUsageTrackerRoot()
            {
                Snapshots = this.snapshots,
            };
            return JsonConvert.SerializeObject(root);
        }

        private void CreateSnapshot(uint[] processIds, string label)
        {
            var processes = new List<SnapshotItem>();
            foreach (var processId in processIds)
            {
                QueryProcessesRecursively(processes, processId);
            }
            this.snapshots.Enqueue(new Snapshot()
            {
                Label = label,
                Processes = processes,
            });
        }

        private void QueryProcessesRecursively(List<SnapshotItem> processes, uint parentProcessId)
        {
            using (var items = new ManagementObjectSearcher(String.Format(
                CultureInfo.InvariantCulture,
                "Select * From Win32_Process Where ParentProcessId={0} Or ProcessId={0}",
                parentProcessId)).Get())
            {
                foreach (var item in items)
                {
                    var ProcessId = (UInt32)item["ProcessId"];

                    if (processes.Find(x => x.ProcessId == ProcessId) != null)
                    {
                        // Already included, skip.
                        continue;
                    }

                    var ParentProcessId = (UInt32)item["ParentProcessId"];
                    var Name = (String)item["Name"];
                    var CommandLine = (String)item["CommandLine"];
                    var CefType = ParseCefType(CommandLine);
                    var WorkingSetSize = (UInt64)item["WorkingSetSize"]; // Working Set in Task Manager originally in bytes
                    var PageFileUsage = (UInt32)item["PageFileUsage"]; // Commit Size in Task Manager in kb
                    var ReadTransferCount = (UInt64)item["ReadTransferCount"];
                    var WriteTransferCount = (UInt64)item["WriteTransferCount"];

                    processes.Add(new SnapshotItem()
                    {
                        ProcessId = ProcessId,
                        ParentProcessId = ParentProcessId,
                        Name = Name,
                        CefType = CefType,
                        WorkingSet = WorkingSetSize / (ulong)1024,
                        CommitSize = PageFileUsage,
                        ReadTransferCount = ReadTransferCount,
                        WriteTransferCount = WriteTransferCount,
                    });

                    if (ProcessId != parentProcessId)
                    {
                        //QueryProcessesRecursively(processes, parentProcessId);
                    }
                }
            }
        }

        private string ParseCefType(string commandLine)
        {
            if (commandLine != null)
            {
                // To avoid capturing PII, we extract only CefSharp process type from the CommandLine.
                // See possible values in ProcessDetails.CefType property.
                var regex = new Regex(@"--type=(\S+)");
                var match = regex.Match(commandLine);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            return String.Empty;
        }

        class MemoryUsageTrackerRoot
        {
            /// <summary>
            /// List of memory usage snapshots corresponding to RecordMemoryUsageEvents
            /// </summary>
            public Queue<Snapshot> Snapshots { get; set; }
        }

        class Snapshot
        {
            /// <summary>
            /// Snapshot label
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Label { get; set; }
            /// <summary>
            /// Memory usage by process id
            /// </summary>
            public IList<SnapshotItem> Processes { get; set; }
        }

        class SnapshotItem
        {
            /// <summary>
            /// Process id
            /// </summary>
            public uint ProcessId { get; set; }

            /// <summary>
            /// Parent process id
            /// </summary>
            public uint ParentProcessId { get; set; }

            /// <summary>
            /// Process name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// CEF process type, e.g., 'renderer', 'gpu-process' or 'crashpad-handler'
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string CefType { get; set; }

            /// <summary>
            /// Working set memory in kilobytes (WorkingSetSize field).
            /// This is a count of physical memory (RAM). Tepresents the subset process's virtual address space that is valid,
            /// meaning that it can be referenced without incurring a page fault.
            /// It includes the memory shared by other processes.
            /// </summary>
            public ulong WorkingSet { get; set; }

            /// <summary>
            /// Commit size in kylobytes  (PageFileUsage field).
            /// This is the total amount of memory that the memory manager has committed for a running process.
            /// This is composed of physical memory (RAM) and disk (pagefiles).
            /// </summary>
            public uint CommitSize { get; set; }

            /// <summary>
            /// Amount of data read
            /// </summary>
            public ulong ReadTransferCount { get; set; }

            /// <summary>
            /// Amount of data written
            /// </summary>
            public ulong WriteTransferCount { get; set; }
        }
    }
}
