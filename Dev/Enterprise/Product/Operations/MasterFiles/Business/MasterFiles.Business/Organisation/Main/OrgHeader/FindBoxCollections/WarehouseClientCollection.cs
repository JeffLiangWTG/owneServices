using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class WarehouseClientCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public WarehouseClientCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseClientCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WarehouseClientCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public WarehouseClientCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		public bool IsValidClient(OrgHeader client)
		{
			return client.OH_IsActive && client.OH_IsWarehouseClient;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsWarehouseClient, true);
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
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property7", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!IsValidClient((OrgHeader)selectedBusinessObject))
			{
				errors.Add(Res.GetString("a76b49fd-4e74-4d77-9031-e1bb6a7769a8", "An Organization selected from here must have an Organization type of Warehouse selected."));
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
