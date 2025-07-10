using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class UnpackDepotCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public UnpackDepotCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public UnpackDepotCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public UnpackDepotCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public UnpackDepotCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery unpackDepot = new ZQuery(OrgHeaderSchema.OH_IsUnpackDepot, true);
			ZQuery miscServ = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			ZQuery filter = new ZQuery(miscServ, JoinCondition.And, unpackDepot);

			ZQuery query = new ZQuery(filter);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsMiscFreightServices = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSV;
			orgHeader.OH_IsUnpackDepot = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.UnpackingDepot));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("0baa1c38-63b5-429d-bced-f5f111dc01e7", "An Organization selected from here must have an Organization type of Services selected and must have Unpack CFS selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsMiscFreightServicesInfo, organisation.OH_IsUnpackDepotInfo);
		}

		#endregion

	}
}
