using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class CTOCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public CTOCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CTOCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public CTOCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public CTOCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(new OrgHeaderFilterProvider().GetCTOFilter());
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
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.CTO));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("301f5cf8-77ea-4748-b589-0a0c81dc8ada", "An Organization selected from here must have an Organization type of Services selected and must have either Air or Sea CTO selected."));
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
				organisation.SetAtleastOneOrgTypeAsExpectedAndValidate(organisation.OH_IsAirCTOInfo, organisation.OH_IsSeaCTOInfo);
			}
		}

		protected bool IsConsignee(OrgHeader organisation)
		{
			return organisation.OH_IsConsignee;
		}

		protected bool IsCTO(OrgHeader organisation)
		{
			return organisation.OH_IsMiscFreightServices && (organisation.OH_IsAirCTO || organisation.OH_IsSeaCTO);
		}

		#endregion

	}
}
