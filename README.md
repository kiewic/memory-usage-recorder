# Memory Usage Recorder App

A C# application to record the memory usage of a tree of processes during a period of time.

To start recording a tree of processes, pass the name of the top process as first argument, for example:

```
MemoryUsageRecorder.exe pbidesktop
```

To stop recording, press `CTRL + C` on the command line.

The memory snapshots will be serialized in a JSON string, for example:

```
{
  "Snapshots": [{
      "Label": "1000",
      "Processes": [{
          "ProcessId": 21916,
          "ParentProcessId": 9000,
          "Name": "PBIDesktop.exe",
          "CefType": "",
          "WorkingSet": 226100,
          "CommitSize": 184820,
          "ReadTransferCount": 46449359,
          "WriteTransferCount": 0
        }
      ]
    }, {
      "Label": "2000",
      "Processes": [{
          "ProcessId": 21916,
          "ParentProcessId": 9000,
          "Name": "PBIDesktop.exe",
          "CefType": "",
          "WorkingSet": 242276,
          "CommitSize": 197252,
          "ReadTransferCount": 47587453,
          "WriteTransferCount": 5245
        }, {
          "ProcessId": 23636,
          "ParentProcessId": 21916,
          "Name": "msmdsrv.exe",
          "CefType": "",
          "WorkingSet": 50904,
          "CommitSize": 87984,
          "ReadTransferCount": 1462166,
          "WriteTransferCount": 74314
        }
      ]
    }, {
      "Label": "3000",
      "Processes": [{
          "ProcessId": 21916,
          "ParentProcessId": 9000,
          "Name": "PBIDesktop.exe",
          "CefType": "",
          "WorkingSet": 278876,
          "CommitSize": 227776,
          "ReadTransferCount": 47600526,
          "WriteTransferCount": 188186
        }, {
          "ProcessId": 23636,
          "ParentProcessId": 21916,
          "Name": "msmdsrv.exe",
          "CefType": "",
          "WorkingSet": 90468,
          "CommitSize": 159768,
          "ReadTransferCount": 10726928,
          "WriteTransferCount": 105711
        }
      ]
    }
  ]
}
```
