using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class CarrierAndOrForwarderCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public CarrierAndOrForwarderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CarrierAndOrForwarderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public CarrierAndOrForwarderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public CarrierAndOrForwarderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsForwarder = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagFA;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
			query.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsSeaWholesaler, SQLComparisonOperator.Equal, ZBool.True);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, ZBool.True);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!((((OrgHeader)selectedBusinessObject).OH_IsShippingProvider && ((OrgHeader)selectedBusinessObject).OH_IsSeaWholesaler)
				|| ((OrgHeader)selectedBusinessObject).OH_IsForwarder))
			{
				errors.Add(Res.GetString("55863f55-997a-4c96-4954-20e9e64891c4", "An Organization selected from here must have an Organization type of Forwarder, NVOCC Carrier or both."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetAtleastOneOrgTypeAsExpectedAndValidate(organisation.OH_IsForwarderInfo, organisation.OH_IsShippingProviderInfo);
		}

		#endregion

	}
}
