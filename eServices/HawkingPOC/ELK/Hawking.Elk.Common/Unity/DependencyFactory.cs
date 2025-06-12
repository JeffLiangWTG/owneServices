using System.Configuration;
using Microsoft.Practices.Unity.Configuration;
using Unity;

namespace Hawking.Elk.Common.Unity
{
    public class DependencyFactory
    {
        public static IUnityContainer Container { get; private set; }

        static DependencyFactory()
        {
            var container = new UnityContainer();

            var section = (UnityConfigurationSection) ConfigurationManager.GetSection("unity");
            if (section != null)
            {
                section.Configure(container);
            }

            Container = container;
        }

        public static T Resolve<T>() where T : class
        {
            T concreteImplementation = default(T);

            if (Container.IsRegistered(typeof(T)))
            {
                concreteImplementation = Container.Resolve<T>();
            }

            return concreteImplementation;
        }
    }
}
