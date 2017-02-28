using System;

namespace MogBot.Host.Settings
{
    public class SettingInfo<T> : ISettingInfo<T>
    {
        public T DefaultValue { get; }
        public string Name { get; }
        public bool HasDefault { get; }

        public SettingInfo(string name)
        {
            Name = name;
        }

        public SettingInfo(string name, T defaultValue):
            this(name)
        {
            DefaultValue = defaultValue;
            HasDefault = true;
        }
    }
}