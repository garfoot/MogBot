using System;

namespace MogBot.Host.Settings
{
    public interface ISettings
    {
        T GetSetting<T>(ISettingInfo<T> setting);
    }
}