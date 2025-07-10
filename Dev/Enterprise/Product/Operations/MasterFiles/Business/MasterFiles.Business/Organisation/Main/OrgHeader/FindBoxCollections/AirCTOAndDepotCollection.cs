using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AirCTOAndDepotCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public AirCTOAndDepotCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AirCTOAndDepotCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AirCTOAndDepotCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public AirCTOAndDepotCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery packDepot = new ZQuery(OrgHeaderSchema.OH_IsPackDepot, true);
			ZQuery unpackDepot = new ZQuery(OrgHeaderSchema.OH_IsUnpackDepot, true);
			ZQuery miscServ = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			ZQuery airCTO = new ZQuery(OrgHeaderSchema.OH_IsAirCTO, true);

			ZQuery depot = new ZQuery(packDepot, JoinCondition.Or, unpackDepot);
			ZQuery depotAndCTO = new ZQuery(depot, JoinCondition.Or, airCTO);
			ZQuery filter = new ZQuery(miscServ, JoinCondition.And, depotAndCTO);

			ZQuery query = new ZQuery(filter);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsMiscFreightServices = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSV;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.Depot));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("34437eca-7013-4d83-ae23-a2dfe4605e32", "An Organization selected from here must have an Organization type of Services selected and must have either Pack Depot, Unpack Depot or Air CTO selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsMiscFreightServicesInfo);
			if (organisation.OH_IsMiscFreightServices)
			{
				organisation.SetAtleastOneOrgTypeAsExpectedAndValidate(organisation.OH_IsPackDepotInfo, organisation.OH_IsUnpackDepotInfo, organisation.OH_IsAirCTOInfo);
			}
		}

		protected bool IsConsignee(OrgHeader organisation)
		{
			return organisation.OH_IsConsignee;
		}

		protected bool IsDepotOrAirCTO(OrgHeader organisation)
		{
			return organisation.OH_IsMiscFreightServices && (organisation.OH_IsPackDepot || organisation.OH_IsUnpackDepot || organisation.OH_IsAirCTO);
		}
		#endregion

	}
}
