using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ControllingCustomerCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ControllingCustomerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ControllingCustomerCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ControllingCustomerCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public ControllingCustomerCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		public bool ControllingCustomerValidationsEnabled
		{
			get { return OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			if (!ControllingCustomerValidationsEnabled)
			{
				return base.CreateAdditionalFilter();
			}

			var query = new ZQuery(OrgHeaderSchema.OH_IsControllingCustomer, true);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefault filterBODefault = new FilterBusinessObjectDefault("Organisation Types", "Property13", ZBool.True);
			FilterBusinessObjectDefaults.Add(filterBODefault);
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			if (ControllingCustomerValidationsEnabled && !((OrgHeader)selectedBusinessObject).OH_IsControllingCustomer)
			{
				errors.Add(Res.GetString("2ed767a4-ab23-4d8c-82f7-a7b72641a010", "An Organization selected from here must have an Organization type of Controlling Customer selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);

			var organization = (OrgHeader)entity;

			if (ControllingCustomerValidationsEnabled)
			{
				organization.SetOrgTypeAsExpectedAndValidate(organization.OH_IsControllingCustomerInfo);
			}
			else
			{
				organization.Validation.ValidateOH_IsControllingCustomer();
			}
		}

		#endregion
	}
}
