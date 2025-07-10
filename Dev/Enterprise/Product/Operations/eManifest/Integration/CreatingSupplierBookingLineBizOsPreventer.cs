using System;
using CargoWise.EntityFramework;

namespace Enterprise.eManifest.Integration
{
	public static class CreatingSupplierBookingLineBizOsPreventer
	{
		public static IDisposable SuspendCreatingSupplierBookingLineBizOs(BusinessObjectFactory factory)
		{
			return new SemaphoreManager(GetSemaphore(factory));
		}

		public static bool IsCreatingSupplierBookingLineBizOsSuspended(BusinessObjectFactory factory)
		{
			return GetSemaphore(factory).IsSuspended;
		}

		static CreatingSupplierBookingLineBizOsSemaphore GetSemaphore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CreatingSupplierBookingLineBizOsPreventer", () => new CreatingSupplierBookingLineBizOsSemaphore());
		}

		class CreatingSupplierBookingLineBizOsSemaphore : Semaphore
		{
		}
	}
}
