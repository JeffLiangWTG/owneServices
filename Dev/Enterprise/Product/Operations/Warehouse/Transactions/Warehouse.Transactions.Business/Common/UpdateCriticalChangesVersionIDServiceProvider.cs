using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class UpdateCriticalChangesVersionIDServiceProvider<T> : IAfterOnSavingBOProcessingService
		where T : BusinessObject, ICriticalChangesVersionID
	{
		public UpdateCriticalChangesVersionIDServiceProvider(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}

		public void RegisterBusinessObjectPK(ZGuid pK)
		{
			if (!pK.IsValid)
			{
				throw new ArgumentException("PK should be valid guid.");
			}

			PKs.Add(pK);
		}

		void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var pk in PKs)
			{
				var bizo = Factory.Load<T>(pk);

				if (bizo != null && bizo.IsInDatabase)
				{
					UpdateVersion(bizo);
				}
			}

			ClearPKs();
		}

		void ClearPKs()
		{
			PKs.Clear();
			ClearPKsCore();
		}

		protected virtual void ClearPKsCore()
		{
		}

		void UpdateVersion(ICriticalChangesVersionID bizo)
		{
			if (bizo.IsImmutableStatus)
			{
				if (!bizo.CriticalChangesVersionID.IsEmpty)
				{
					bizo.CriticalChangesVersionID = ZGuid.Empty;
				}
			}
			else
			{
				bizo.CriticalChangesVersionID = ZGuid.NewZGuid();
			}
		}

		BusinessObjectFactory Factory { get; }
		readonly HashSet<ZGuid> PKs = new HashSet<ZGuid>();
	}
}
