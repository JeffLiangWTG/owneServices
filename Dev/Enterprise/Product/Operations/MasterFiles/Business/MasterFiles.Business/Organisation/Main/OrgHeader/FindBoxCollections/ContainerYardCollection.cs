using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ContainerYardCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ContainerYardCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ContainerYardCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ContainerYardCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ContainerYardCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var containerYard = new ZQuery(OrgHeaderSchema.OH_IsContainerYard, true);
			var miscServ = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			var filter = new ZQuery(miscServ, JoinCondition.And, containerYard);

			var query = new ZQuery(filter);
			query.AddToFilter(base.CreateAdditionalFilter());

			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsMiscFreightServices = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSV;
			orgHeader.OH_IsContainerYard = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.ContainerYard));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("8a38037f-be7a-4c57-a7aa-76ab5e5f8b01", "An Organization selected from here must have an Organization type of Services selected and must have Container Yard selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsMiscFreightServicesInfo, organisation.OH_IsContainerYardInfo);
		}

		#endregion

	}
}
