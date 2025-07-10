using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ServiceContainerSuspenderHelper : IService
	{
		#region FunctionalitySuspender

		public class FunctionalitySuspenderService : IService
		{
			internal int UseCount = 1;
		}

		public class FunctionalitySuspender<T> : IDisposable where T : FunctionalitySuspenderService, new()
		{
			public static FunctionalitySuspender<T> GetSuspender(BusinessObjectFactory factory)
			{
				return new FunctionalitySuspender<T>(factory);
			}

			public static bool IsSuspended(BusinessObjectFactory factory)
			{
				return factory.ServiceContainer.GetService<T>() != null;
			}

			public static T GetService(BusinessObjectFactory factory)
			{
				return factory.ServiceContainer.GetService<T>();
			}

			FunctionalitySuspender(BusinessObjectFactory factory)
			{
				this.Factory = factory;
				T service = factory.ServiceContainer.GetService<T>();
				if (service == null)
				{
					factory.ServiceContainer.AddService(new T());
				}
				else
				{
					service.UseCount++;
				}
			}

			readonly BusinessObjectFactory Factory;

			void IDisposable.Dispose()
			{
				T service = Factory.ServiceContainer.GetService<T>();
				if (service != null)
				{
					service.UseCount--;
					if (service.UseCount == 0)
					{
						Factory.ServiceContainer.RemoveService<T>();
					}
				}
			}
		}

		#endregion

		#region ApplyRevenueRecognitionDateSuspender

		class ApplyRevenueRecognitionDateSuspenderService : FunctionalitySuspenderService
		{
		}

		public static IDisposable GetApplyRevenueRecognitionDateSuspender(BusinessObjectFactory factory)
		{
			return FunctionalitySuspender<ApplyRevenueRecognitionDateSuspenderService>.GetSuspender(factory);
		}

		public static bool IsApplyRevenueRecognitionDateSuspended(BusinessObjectFactory factory)
		{
			return FunctionalitySuspender<ApplyRevenueRecognitionDateSuspenderService>.IsSuspended(factory);
		}

		#endregion

		public class GUIRelatedActionsSuspend : FunctionalitySuspenderService
		{
		}

		#region DocManagerInfoNewFactorySuspend

		public class DocManagerInfoNewFactorySuspend : FunctionalitySuspenderService
		{
		}

		public static IDisposable GetDocManagerInfoNewFactorySuspender(BusinessObjectFactory factory)
		{
			return FunctionalitySuspender<DocManagerInfoNewFactorySuspend>.GetSuspender(factory);
		}

		public static bool IsDocManagerInfoNewFactorySuspended(BusinessObjectFactory factory)
		{
			return FunctionalitySuspender<DocManagerInfoNewFactorySuspend>.IsSuspended(factory);
		}

		#endregion

#if DEBUG
		//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		//Do not create classes like below. Use Attribute inherited from TestSetupAttribute instead.
		//
		//Also do not suspend critical validation for all tests by default. Critical validation is a production code. This excludes production code from tests except couple dedicated tests. 
		//Any new functionality changes will not be checked by such critical validation.
		//Such validation will be release build only code most developers will never test use as we run debug build usually.
		//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

		public class PostDateCriticalValidationSuspenderService : FunctionalitySuspenderService
		{
		}

		public class AccTransactionHeaderWithLinesCriticalValidationActivatorService
		{
			AccTransactionHeaderWithLinesCriticalValidationActivatorService()
			{
				StaticCount = 0;
			}

			public static AccTransactionHeaderWithLinesCriticalValidationActivatorService Instance
			{
				get
				{
					if (instance == null)
					{
						instance = new AccTransactionHeaderWithLinesCriticalValidationActivatorService();
					}
					return instance;
				}
			}

			[ThreadStatic]
			static AccTransactionHeaderWithLinesCriticalValidationActivatorService instance;

			[ThreadStatic]
			static int StaticCount;

			public DisposableAction GetActivator()
			{
				StaticCount++;
				return new DisposableAction(() => { StaticCount--; });
			}

			public bool IsActivated => StaticCount > 0;
		}
#endif
	}
}
