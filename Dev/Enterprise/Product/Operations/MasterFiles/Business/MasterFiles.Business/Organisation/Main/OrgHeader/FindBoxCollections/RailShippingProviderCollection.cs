using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RailShippingProviderCollection : ShippingProviderCollection, IValidateForController
	{
		public RailShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RailShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RailShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public RailShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = GetShippingProviderTransportTypeQuery();
			result.AddToFilter(base.CreateAdditionalFilter());

			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgHeader)child).OH_IsRailProvider = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.Rail));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(AdditionalFilterNotMetNotification);
			}
		}

		protected virtual ZQuery GetShippingProviderTransportTypeQuery()
		{
			return new ZQuery(OrgHeaderSchema.OH_IsRailProvider, true);
		}

		protected virtual ZString AdditionalFilterNotMetNotification => Res.GetString("4c2b4d67-a0cf-4c5e-9d7d-a03c2ba94b9a", "An Organization selected from here must have Rail Provider selected.");

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsRailProviderInfo);
		}

		#endregion
	}
}
