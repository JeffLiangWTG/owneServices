using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class CarrierCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public CarrierCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CarrierCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CarrierCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public CarrierCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			if (!AllowOtherOrgTypes)
			{
				var query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, true);
				result.AddToFilter(query);
			}
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var orgHeader = (OrgHeader)child;
			orgHeader.OH_IsShippingProvider = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagCrr;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			var filterBODefault = new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True);
			FilterBusinessObjectDefaults.Add(filterBODefault);
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			if (!((OrgHeader)selectedBusinessObject).OH_IsShippingProvider)
			{
				errors.Add(Res.GetString("2bc273b6-cfe0-4b9e-b1f9-8a8cafe50b34", "An Organization selected from here must have an Organization type of Carrier selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);

			var organization = (OrgHeader)entity;

			if (!AllowOtherOrgTypes)
			{
				organization.SetOrgTypeAsExpectedAndValidate(organization.OH_IsShippingProviderInfo);
			}
			else
			{
				organization.Validation.ValidateOH_IsShippingProvider();
			}
		}

		#endregion
	}
}
