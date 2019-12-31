using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sanmei_AirConditioner.Web.Models
{
    public static class ConfigData
    {
        public static Dictionary<string, string> ConfigDataDics = new Dictionary<string, string>();
        static ConfigData()
        {
            foreach(var key in System.Configuration.ConfigurationManager.AppSettings.AllKeys)
            {
                if (!ConfigDataDics.ContainsKey(key))
                {
                    ConfigDataDics.Add(key, System.Configuration.ConfigurationManager.AppSettings[key]);
                }
            }
        }
    }
}