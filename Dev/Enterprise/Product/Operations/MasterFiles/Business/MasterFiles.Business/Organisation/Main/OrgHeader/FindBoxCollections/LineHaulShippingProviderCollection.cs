using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class LineHaulShippingProviderCollection : ShippingProviderCollection, IValidateForController
	{
		public LineHaulShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public LineHaulShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public LineHaulShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public LineHaulShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
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
			((OrgHeader)child).OH_IsLineHaulProvider = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.LineHaul));
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
			return new ZQuery(OrgHeaderSchema.OH_IsLineHaulProvider, true);
		}

		protected virtual ZString AdditionalFilterNotMetNotification => Res.GetString("ff938a3e-f4e6-45c3-99fb-dba61781c553", "An Organization selected from here must have Line Haul selected.");

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsLineHaulProviderInfo);
		}

		#endregion
	}
}
