﻿using CPPCli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginNs.Utilities
{
    static class Const
    {
        public static readonly string LogFilePath = Path.Combine(Directory.GetParent(Assembly.GetCallingAssembly().Location).FullName,
            "TestPlugin.txt");
        public static readonly string PluginId = "__CREATE_UUID__";
        public static readonly string PluginKey = "__ASK_PELCO_REPRESENTITIVE_partnerfirst@schneider-electric.com__";

        public static readonly string VxSdkLogFilePath = Directory.GetParent(Assembly.GetCallingAssembly().Location).FullName;
        public static readonly LogLevel.Value VxSdkLogLevel = LogLevel.Value.Debug;
        public static readonly string VxSdkKey = "__GENERATE_VXSDK_KEY_FROM_External\Pelco\VxSdk-1.2\Tools\VxSdkKeyGen.exe__";

        public static readonly string PersistentModel = "PersistentModel";
        public static readonly string RegionMainView = "RegionMainView";
        public static readonly string ILogger = "ILogger";
    }
}
