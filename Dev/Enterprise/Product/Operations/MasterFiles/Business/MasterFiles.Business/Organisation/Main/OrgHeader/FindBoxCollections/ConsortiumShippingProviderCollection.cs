using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ConsortiumShippingProviderCollection : ShippingProviderCollection, IValidateForController
	{
		public ConsortiumShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ConsortiumShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ConsortiumShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ConsortiumShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsShippingConsortium, true);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgHeader)child).OH_IsShippingConsortium = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.VesselConsortium));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("e808f05e-4fbf-4c9e-bc8c-e85895fd2c42", "An Organization selected from here must have Vessel Consortium selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsShippingConsortiumInfo);
		}

		#endregion
	}
}
