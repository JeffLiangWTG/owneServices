using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SeaShippingProviderCollection : ShippingProviderCollection, IValidateForController
	{
		public SeaShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public SeaShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public SeaShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public SeaShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery shippingLineQuery = new ZQuery(OrgHeaderSchema.OH_IsShippingLine, true);
			shippingLineQuery.AddToFilter(base.CreateAdditionalFilter());
			shippingLineQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsSeaWholesaler, true);
			return shippingLineQuery;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgHeader)child).OH_IsShippingLine = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.ShippingLine));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("0f58c9c1-a455-4e9a-86a9-ec46cd7b153a", "An Organization selected from here must have Shipping Line selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsShippingLineInfo);
		}

		#endregion
	}
}
