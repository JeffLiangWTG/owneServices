using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class UNDGDataItemBusinessObjectStrategy : IBusinessObjectStrategy
	{
		void IBusinessObjectStrategy.FetchForLoad(BusinessObject businessObject)
		{
			var factory = businessObject.Factory;
			if (factory != null && businessObject is IUNDGDataItemProvider provider && provider.NeedFetchHintForLoad)
			{
				factory.AddFetchHint(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID, businessObject.PK);
			}
		}

		void IBusinessObjectStrategy.OnDelete(BusinessObject businessObject)
		{
			var factory = businessObject.Factory;
			if (factory != null && businessObject is IUNDGDataItemProvider)
			{
				var query = new ZQuery(UNDGDataItemSchema.DI_ParentID, businessObject.PK);
				query.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;
				foreach (UNDGDataItem bizO in factory.Load<UNDGDataItem>(query))
				{
					bizO.Delete();
				}
			}
		}

		#region Unused

		void IBusinessObjectStrategy.BeforeSuccessfulDelete(BusinessObject businessObject)
		{
		}

		DeleteDetails IBusinessObjectStrategy.DeleteDetails(BusinessObject businessObject)
		{
			return null;
		}

		void IBusinessObjectStrategy.OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		void IBusinessObjectStrategy.OnFactorySaving(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnSaveRollback(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnSaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		void IBusinessObjectStrategy.OnSaving(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnSavingInObjectsWithLateChanges(BusinessObject businessObject)
		{
		}

		#endregion
	}
}
