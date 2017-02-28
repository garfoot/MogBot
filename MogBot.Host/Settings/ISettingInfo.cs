using System;

namespace MogBot.Host.Settings
{
    public interface ISettingInfo<out T>
    {
        string Name { get; }
        T DefaultValue { get; }
    }
}