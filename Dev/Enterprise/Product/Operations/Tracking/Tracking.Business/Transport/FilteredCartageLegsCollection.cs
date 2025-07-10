using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class FilteredCartageLegsCollection : BusinessObjectCollection<CommonCartageLeg>
	{
		public FilteredCartageLegsCollection(CommonCartageLegCollection collectionToFilter)
			: this(collectionToFilter, null)
		{ }

		public FilteredCartageLegsCollection(CommonCartageLegCollection collectionToFilter, OrgContact loggedInContact)
			: base(collectionToFilter.Factory)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(CommonCartageLeg));
			if (loggedInContact != null && !WebDataRegistry.Instance.TransportShowAllLegs.Value)
			{
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, loggedInContact.ParentOrg.PK);

				ZDBOnlySubQuery docAddressSubquery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.PK);
				docAddressSubquery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);
				filter.AddSubQuery(JobContainerLegsSchema.JU_E2DeliveryAddressID, docAddressSubquery, JoinCondition.Or);
				filter.AddSubQuery(JobContainerLegsSchema.JU_E2PickupAddressID, docAddressSubquery, JoinCondition.Or);
				filter.AddSubQuery(JobContainerLegsSchema.JU_E2WaitPointAddressID, docAddressSubquery, JoinCondition.Or);
			}

			if (filter.IsEmpty)
			{
				this.AddRange(collectionToFilter);
			}
			else
			{
				this.AddRange(collectionToFilter.Find(filter));
			}
		}
	}
}
