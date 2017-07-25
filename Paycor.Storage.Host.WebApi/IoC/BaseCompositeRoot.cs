using System.Web.Hosting;
using LightInject;

namespace Paycor.Storage.Host.WebApi.IoC
{
    public abstract class BaseCompositeRoot : ICompositionRoot
    {
        public abstract void Compose(IServiceRegistry serviceRegistry);

        protected bool IsHostedApp { get { return HostingEnvironment.IsHosted; } }

        protected ILifetime PerRequestLifetimeIfHosted()
        {
            return IsHostedApp ? ((ILifetime)new PerRequestLifeTime()) : ((ILifetime)new PerContainerLifetime());
        }
    }
}