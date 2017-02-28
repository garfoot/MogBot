using System;
using System.Configuration;
using System.Reflection;
using FeatureSwitcher;
using FeatureSwitcher.Configuration;

namespace MogBot.Host.Features
{
    public static class FeatureExtensions
    {
        public static IConfigureFeatures AppConfig(this IConfigureBehavior source)
        {
            return source.Custom(i =>
            {
                string config = ConfigurationManager.AppSettings[$"feature.{i.Value}"];
                var isEnabled = false;
                if (config != null)
                {
                    bool.TryParse(config, out isEnabled);
                }

                return isEnabled;
            });
        }

        public static IConfigureFeatures Attribute(this IConfigureNaming source)
        {
            return source.Custom(type =>
            {
                var name = type.GetCustomAttribute<FeatureName>()?.Name;
                if (name == null)
                {
                    throw new InvalidOperationException($"FeatureName attribute is not present on {type.FullName}.");
                }

                return new Feature.Name(type, name);
            });
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureName : Attribute
    {
        public string Name { get; }

        public FeatureName(string name)
        {
            Name = name;
        }
    }
}