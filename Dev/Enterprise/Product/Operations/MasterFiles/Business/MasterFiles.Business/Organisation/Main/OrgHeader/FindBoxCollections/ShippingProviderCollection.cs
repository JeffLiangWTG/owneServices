using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ShippingProviderCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		public ShippingProviderCollection(BusinessObjectFactory factory, bool filterDefaultShippingLine) : base(factory)
		{
			if (filterDefaultShippingLine)
			{
				SetShippingLineFilterBusinessObjectDefault();
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			if (!AllowOtherOrgTypes)
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, true);
				result.AddToFilter(query);
			}
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsShippingProvider = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagCrr;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True));
		}

		void SetShippingLineFilterBusinessObjectDefault()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", (ZString)(NoResString)"Carrier - Shipping Line"));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!((OrgHeader)selectedBusinessObject).OH_IsShippingProvider && !AllowOtherOrgTypes)
			{
				errors.Add(Res.GetString("a10f9b46-21d5-432e-9e8d-c7f56329521a", "An Organization selected from here must have an Organization type of Carrier selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			if (!AllowOtherOrgTypes)
			{
				organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsShippingProviderInfo);
			}
			else
			{
				organisation.Validation.ValidateOH_IsShippingProvider();
			}
		}

		#endregion

	}
}
