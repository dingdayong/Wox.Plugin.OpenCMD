using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Wox.Infrastructure.Storage;
using Wox.Plugin;
using System.Drawing;
using System.Reflection;

namespace Wox.Plugin.OpenCMD
{
    /// <summary>
    /// Setting from default and user.
    /// </summary>
    public class UserSettingStorage : JsonStrorage<UserSettingStorage>
    {
        #region Property
        [JsonProperty]
        public string Cmder { get; set; }

        [JsonProperty]
        public List<String> Explorers { get; set; }

        [JsonProperty]
        public string CmderArgs { get; set; }

        #endregion

        #region location
        protected override string ConfigFolder
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        protected override string ConfigName
        {
            get { return "Wox.Plugin.OpenCMD"; }
        }

        protected override string FileSuffix
        {
            get { return ".user-settings"; }
        }
        #endregion

        protected override UserSettingStorage LoadDefault()
        {
            Explorers = new List<string>() { @"explorer", @"clover" };
            Cmder = string.Empty;
            CmderArgs = string.Empty;
            return this;
        }

        protected override void OnAfterLoad(UserSettingStorage storage)
        {
            if (storage.Explorers == null || storage.Explorers.Count <= 0)
            {
                storage.Explorers = new List<string>() { @"explorer", @"clover" };
            }
        }
    }


}