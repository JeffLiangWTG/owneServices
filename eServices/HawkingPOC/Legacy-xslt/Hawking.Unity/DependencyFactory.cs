using System;
using Unity;

namespace Hawking.Unity
{
    public sealed class DependencyFactory : IDisposable
    {
        static readonly Lazy<DependencyFactory> lazy = new Lazy<DependencyFactory>(() => new DependencyFactory());

        public static DependencyFactory Instance { get { return lazy.Value; } }

        static DependencyFactory()
        {
            Container = new UnityContainer();
        }

        public static IUnityContainer Container { get; private set; }

        public static T Resolve<T>() where T : class
        {
            T concreteImplementation = default(T);

            if (Container.IsRegistered(typeof(T)))
            {
                concreteImplementation = Container.Resolve<T>();
            }

            return concreteImplementation;
        }

        #region IDisposable

        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeInternal();
            }

            disposed = true;
        }

        void DisposeInternal()
        {
            Container?.Dispose();
            Container = null;
        }
        
        #endregion
    }
}
