using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class DepotCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public DepotCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DepotCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public DepotCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public DepotCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(new OrgHeaderFilterProvider().GetDepotFilter());
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
				errors.Add(Res.GetString("4519d3b0-9111-4f19-be6d-510ba6b328f6", "An Organization selected from here must have an Organization type of Services selected and must have either a Pack or an Unpack CFS selected."));
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
				organisation.SetAtleastOneOrgTypeAsExpectedAndValidate(organisation.OH_IsPackDepotInfo, organisation.OH_IsUnpackDepotInfo);
			}
		}

		protected bool IsConsignee(OrgHeader organisation)
		{
			return organisation.OH_IsConsignee;
		}

		protected bool IsDepot(OrgHeader organisation)
		{
			return organisation.IsDepot;
		}

		#endregion

	}
}
