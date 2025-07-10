
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class BillIssuerOrganisationFindBoxCollection : OrganisationsFindBoxCollection
	{
		public BillIssuerOrganisationFindBoxCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetOverrideNotificationWhenAdditionalFilterNotMet("The organisation should be Forwarder, Local Transport, Rail Provider or Shipping Line.");
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property6", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.False));
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(BillIssuerQuery);
			return result;
		}

		ZQuery BillIssuerQuery
		{
			get
			{
				if (fBillIssuerQuery == null)
				{
					fBillIssuerQuery = new ZQuery();
					fBillIssuerQuery.AddToFilter(OrgHeaderSchema.OH_IsAirLine, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsLocalTransport, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsRailProvider, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingLine, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingProvider, SQLComparisonOperator.Equal, ZBool.True);
				}
				return fBillIssuerQuery;
			}
		}
		ZQuery fBillIssuerQuery;
	}
}
