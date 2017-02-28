using System;
using System.ComponentModel;
using System.Configuration;

namespace MogBot.Host.Settings
{
    public class AppSettingsProvider : ISettings
    {
        public T GetSetting<T>(ISettingInfo<T> setting)
        {
            string settingValue = ConfigurationManager.AppSettings[setting.Name];

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
            {
                return (T)converter.ConvertFrom(settingValue);
            }

            return setting.DefaultValue;
        }
    }
}