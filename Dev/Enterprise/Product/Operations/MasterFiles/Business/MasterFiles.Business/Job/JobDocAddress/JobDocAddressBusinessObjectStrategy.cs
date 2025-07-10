using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class JobDocAddressBusinessObjectStrategy : IBusinessObjectStrategy
	{
		void IBusinessObjectStrategy.OnDelete(BusinessObject businessObject)
		{
			var factory = businessObject.Factory;
			if (factory != null && businessObject is IDocAddresses)
			{
				var query = new ZQuery(JobDocAddressSchema.E2_ParentID, businessObject.PK);
				query.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;
				var bizObjs = factory.Load<JobDocAddress>(query);
				foreach (var bizO in bizObjs)
				{
					bizO.FetchStrategy.FetchForDelete();
				}
				foreach (var bizO in bizObjs)
				{
					bizO.Delete();
				}
			}
		}

		#region Unused

		void IBusinessObjectStrategy.FetchForLoad(BusinessObject businessObject)
		{
		}

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
