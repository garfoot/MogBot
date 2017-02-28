using System;
using MogBot.Host.Settings;

namespace MogBot.Host
{
    public static class DefinedSettings
    {
        public static ISettingInfo<string> BotToken = new SettingInfo<string>("BotToken");
    }
}