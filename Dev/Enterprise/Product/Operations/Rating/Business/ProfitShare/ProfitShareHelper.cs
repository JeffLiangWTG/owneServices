using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public static class ProfitShareHelper
	{
		public static bool IsConsolAlreadyProcessed(BusinessObjectFactory factory, ZGuid forwardingConsolPK, IJobInvoicingSupporter jobInvoicingSupporter)
		{
			var query = new ZDBOnlyQuery(typeof(ConsolidationProfitShare));
			query.AddToFilter(ConsolidationProfitShareSchema.CPS_JK, forwardingConsolPK);
			query.AddToFilter(GetJobFilter(ConsolidationProfitShareSchema.CPS_JH_ConsolJob, jobInvoicingSupporter));

			return factory.ExistsInDatabase(ConsolidationProfitShareSchema.Constants.TableName, query);
		}

		public static bool IsShipmentAlreadyProcessed(BusinessObjectFactory factory, ZGuid forwardingShipmentPK, IJobInvoicingSupporter jobInvoicingSupporter)
		{
			var query = new ZDBOnlyQuery(typeof(ShipmentProfitShares));
			query.AddToFilter(ShipmentProfitSharesSchema.PSS_JS, forwardingShipmentPK);
			query.AddToFilter(GetJobFilter(ShipmentProfitSharesSchema.PSS_JH_ShipmentJob, jobInvoicingSupporter));

			return factory.ExistsInDatabase(ShipmentProfitSharesSchema.Constants.TableName, query);
		}

		static ZQuery GetJobFilter(SchemaColumn jobSchemaColumn, IJobInvoicingSupporter jobInvoicingSupporter)
		{
			var jobQuery = new ZQuery();
			jobQuery.DefaultJoinCondition = JoinCondition.Or;
			jobQuery.AddToFilter(jobSchemaColumn, DBNull.Value);
			if (jobInvoicingSupporter.Job != null)
			{
				jobQuery.AddToFilter(jobSchemaColumn, jobInvoicingSupporter.Job.PK);
			}

			return jobQuery;
		}
	}
}
