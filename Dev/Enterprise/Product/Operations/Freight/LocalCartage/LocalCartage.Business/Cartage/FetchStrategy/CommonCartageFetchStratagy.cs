using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CommonCartageFetchStrategy(CommonCartage cartage)
			: base(cartage)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(CommonBookedCtgMove), JobBookedCtgMoveSchema.EW_JJ, Cartage.PK);
			Factory.AddFetchHint(typeof(CommonCartageType), LocalCartageJobTypeSchema.E3_JobType, Cartage.JJ_E3_NKJobType);
			Factory.AddFetchHint(typeof(JobSailing), JobSailingSchema.PK, Cartage.JJ_JX_Sailing);

			//Test in CartageLegPlannerFilterControl
			ZQuery query = new ZQuery(JobHeaderSchema.JH_ParentID, Cartage.PK);
			query.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			Factory.AddFetchHint(typeof(JobHeader), query);

			if (Cartage.JJ_ParentTableCode == JobShipmentSchema.Constants.Prefix)
			{
				Factory.AddFetchHint(JobShipmentSchema.PK, Cartage.JJ_ParentID);
			}
			else if (Cartage.JJ_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
			{
				Factory.AddFetchHint(JobDeclarationSchema.PK, Cartage.JJ_ParentID);
			}
			else if (Cartage.JJ_ParentTableCode == JobConsolSchema.Constants.Prefix)
			{
				Factory.AddFetchHint(JobConsolSchema.PK, Cartage.JJ_ParentID);
			}
			else if (Cartage.JJ_ParentTableCode == WhsDocketSchema.Constants.Prefix)
			{
				Factory.AddFetchHint(WhsDocketSchema.PK, Cartage.JJ_ParentID);
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			//Test in CartageLegPlannerFilterControl
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, Cartage.PK);
		}

		CommonCartage Cartage
		{
			get { return (CommonCartage)BusinessObject; }
		}
	}
}
