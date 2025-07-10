using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickableDocketFetchStrategy : WhsDocketFetchStrategy
	{
		public WhsPickableDocketFetchStrategy(WhsPickableDocket docket)
			: base(docket)
		{
		}

		protected override void AddDocketSpecificFetchHintsForView(string columnName)
		{
			base.AddDocketSpecificFetchHintsForView(columnName);

			//Performance tested in WhsFilterControlDbHitsTestCase
			if (columnName.Contains("WP_PickNo"))
			{
				Factory.AddFetchHint(WhsPickSchema.PK, Docket.WD_WP);
			}
			else if (columnName == nameof(WhsOrder.ContainerID))
			{
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, Docket.PK);
			}
			else if (columnName == nameof(WhsOrder.TransportJobNumber))
			{
				Factory.AddFetchHint(JobCartageSchema.JJ_ParentID, Docket.PK);
				OrderListForDelayedJobNumberFetchHints.GetInstanceForFactory(Factory).Add(Docket);
			}
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();

			// tested in TestDBHitsForPickValidation_PickByBOM
			foreach (var docketLine in Docket.Lines)
			{
				Factory.AddFetchHint(WhsDocketLineSchema.WE_WE_ParentDocketLine, docketLine.PK);
			}
		}

		protected override void FetchForFactorySaveBeforeTransactionCore()
		{
			base.FetchForFactorySaveBeforeTransactionCore();
			
			Factory.AddFetchHint(JobDocAddressSchema.Instance, FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, Docket.PK));
			Factory.AddFetchHint(StmALogSchema.Instance, FetchHintsHelper.GetStmALogQuery(Docket.PK, AutoEvents.SetToInactiveCode));
		}
	}
}
