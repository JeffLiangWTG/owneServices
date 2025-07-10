using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	class CTODepotContainerYardConsigneeForwarderCollection : OrganisationsFindBoxCollection
	{
		public CTODepotContainerYardConsigneeForwarderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery subServiceTypeFilter = new ZQuery();
			subServiceTypeFilter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsAirCTO, true);
			subServiceTypeFilter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsSeaCTO, true);
			subServiceTypeFilter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsPackDepot, true);
			subServiceTypeFilter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsUnpackDepot, true);
			subServiceTypeFilter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsContainerYard, true);

			ZQuery serviceTypeFilter = new ZQuery();
			serviceTypeFilter.AddToFilter(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			serviceTypeFilter.AddToFilter(subServiceTypeFilter);

			ZQuery filter = base.CreateAdditionalFilter();
			ZQuery consigneeForwarderFilter = new ZQuery();

			consigneeForwarderFilter.AddToFilter(OrgHeaderSchema.OH_IsConsignee, true);
			consigneeForwarderFilter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, true);
			consigneeForwarderFilter.AddToFilter(serviceTypeFilter, JoinCondition.Or);

			filter.AddToFilter(consigneeForwarderFilter);

			return filter;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("2c9a729e-3f02-4d25-889c-9497f8dd5c4f", "An Organization selected from here must be a Consignee, Forwarder, CTO, Depot or Container Yard."));
			}
		}
	}
}

#region Implementation
#region Get*Org
#endregion
#endregion
