using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public sealed class JobCO2eBusinessObjectStrategy : IBusinessObjectStrategy
	{
		public void FetchForLoad(BusinessObject businessObject)
		{
			var factory = businessObject.Factory;
			if (factory != null)
			{
				if (businessObject is ICO2eParent parent)
				{
					factory.AddFetchHint(typeof(JobCO2e), GetJobCO2eQuery(parent.JobCO2eParentID, parent.JobCO2eParentTableCode));
				}
				// this is special case because ICO2eProvider logic is implemented in QuotedBooking NPBO which is initialized from ViewQuotedBooking
				else if (businessObject is IViewQuotedBooking)
				{
					factory.AddFetchHint(typeof(JobCO2e), GetJobCO2eQuery(businessObject.PK, ViewQuotedBookingSchema.Constants.Prefix));
				}
			}
		}

		ZQuery GetJobCO2eQuery(ZGuid parentPK, ZString parentTableCode)
		{
			var query = new ZQuery(JobCO2eSchema.JCO_ParentID, parentPK);
			query.AddToFilter(JobCO2eSchema.JCO_ParentTableCode, parentTableCode);
			return query;
		}

		public void OnDelete(BusinessObject businessObject)
		{
			var factory = businessObject.Factory;
			if (factory != null && businessObject is ICO2eParent parent)
			{
				var query = GetJobCO2eQuery(parent.JobCO2eParentID, parent.JobCO2eParentTableCode);
				query.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;
				foreach (var bizO in factory.Load<JobCO2e>(query))
				{
					bizO.Delete();
				}
			}
		}

		public void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
		}

		public DeleteDetails DeleteDetails(BusinessObject businessObject) => null;

		public void OnSaving(BusinessObject businessObject)
		{
		}

		public void OnSaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		public void OnFactorySaving(BusinessObject businessObject)
		{
		}

		public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		public void OnSaveRollback(BusinessObject businessObject)
		{
		}

		public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject)
		{
		}
	}
}
