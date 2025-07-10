using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class UpdateBusinessObjectCriticalChangesVersionIDHelper<T>
		where T : BusinessObject, ICriticalChangesVersionID
	{
		#region UpdateBusinessObjectVersionAndSubscribeToFactory

		public static void UpdateBusinessObjectVersionAndSubscribeToFactory(BusinessObjectFactory factory, ZGuid bizOPK)
		{
			if (!bizOPK.IsValid)
			{
				throw new ArgumentException("BusinessObject PK should be valid.");
			}

			GetUpdateBizOVersionService(factory).RegisterBusinessObjectPK(bizOPK);
		}

		#endregion

		#region GetUpdateBizOVersionService

		static UpdateCriticalChangesVersionIDServiceProvider<T> GetUpdateBizOVersionService(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			var updateBizOVersionService = factory.ServiceContainer.GetAfterOnSavingService<UpdateCriticalChangesVersionIDServiceProvider<T>>();
			if (updateBizOVersionService == null)
			{
				updateBizOVersionService = new UpdateCriticalChangesVersionIDServiceProvider<T>(factory);
				factory.ServiceContainer.AddAfterOnSavingService(updateBizOVersionService);
			}

			return updateBizOVersionService;
		}

		#endregion
	}
}
