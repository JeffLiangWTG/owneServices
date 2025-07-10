using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class InlandWaterwayShippingProviderCollection : ShippingProviderCollection, IValidateForController
	{
		public InlandWaterwayShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public InlandWaterwayShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public InlandWaterwayShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public InlandWaterwayShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
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
			((OrgHeader)child).OH_IsInlandWaterwayProvider = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.InlandWaterway));
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
			return new ZQuery(OrgHeaderSchema.OH_IsInlandWaterwayProvider, true);
		}

		protected virtual ZString AdditionalFilterNotMetNotification => Res.GetString("d3375de0-2d85-4dd4-9002-c71cf734a2b3", "An Organization selected from here must have Inland Waterway Provider selected.");

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsInlandWaterwayProviderInfo);
		}

		#endregion
	}
}
