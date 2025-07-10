using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class BondedWarehouseCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public BondedWarehouseCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public BondedWarehouseCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public BondedWarehouseCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public BondedWarehouseCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(new OrgHeaderFilterProvider().GetWarehouseFilter());
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsWarehouseClient = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagWH;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property7", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("9995c68a-0866-41a9-a1c7-2045d6013b01", "An Organization selected from here must have Warehouse selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;

			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsWarehouseClientInfo);
		}

		#endregion
	}
}
