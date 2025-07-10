using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RoadDepotTransitShedCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public RoadDepotTransitShedCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RoadDepotTransitShedCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RoadDepotTransitShedCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public RoadDepotTransitShedCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery roadFreight = new ZQuery(OrgHeaderSchema.OH_IsRoadFreightDepot, ZBool.True);
			ZQuery miscServ = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
			ZQuery filter = new ZQuery(miscServ, JoinCondition.And, roadFreight);
			filter.AddToFilter(base.CreateAdditionalFilter());
			return filter;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsMiscFreightServices = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSV;
			orgHeader.OH_IsRoadFreightDepot = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.RoadDepotTransitShed));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("d42f1576-6f43-40b6-9267-9953427f9e9f", "An Organization selected from here must have an Organization type of Services selected and \"Road Depot/Transit Shed\" service selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsMiscFreightServicesInfo, organisation.OH_IsRoadFreightDepotInfo);
		}

		protected bool IsRoadFreightDepot(OrgHeader organisation)
		{
			return organisation.OH_IsMiscFreightServices && organisation.OH_IsRoadFreightDepot;
		}

		#endregion

	}
}
