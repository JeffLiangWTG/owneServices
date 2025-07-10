using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class BrokerCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public BrokerCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public BrokerCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public BrokerCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public BrokerCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsBroker, true);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsBroker = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagBR;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property8", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("ce47b83b-d80c-4f96-b64c-17adaf15e656", "An Organization selected from here must have an Organization Type of Broker selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;

			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsBrokerInfo);
		}

		#endregion

	}
}
