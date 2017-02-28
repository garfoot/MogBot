using System;
using Autofac;

namespace MogBot.Host.Modules
{
    internal class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                   .AsSelf()
                   .AsImplementedInterfaces();
        }
    }
}
