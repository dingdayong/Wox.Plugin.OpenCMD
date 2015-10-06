using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NLog;

namespace Wox.Infrastructure.Storage
{
    /// <summary>
    /// Serialize object using json format.
    /// </summary>
    public abstract class JsonStrorage<T> : BaseStorage<T> where T : class, IStorage, new()
    {
        /// <summary>
        /// LOG.
        /// </summary>
        private static NLog.Logger Log = LogManager.GetCurrentClassLogger();

        private static object syncObject = new object();
        protected override string FileSuffix
        {
            get { return ".json"; }
        }

        protected override void LoadInternal()
        {
            string json = File.ReadAllText(ConfigPath);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    serializedObject = JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception ex)
                {
                    Log.Error(ex,@"load setting file %s fail!!! load default.",ConfigPath); 
                    serializedObject = LoadDefault();
                }
            }
            else
            {
                serializedObject = LoadDefault();
            }
        }

        protected override void SaveInternal()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                lock (syncObject)
                {
                    string json = JsonConvert.SerializeObject(serializedObject, Formatting.Indented);
                    File.WriteAllText(ConfigPath, json);
                }
            });
        }
    }
}
