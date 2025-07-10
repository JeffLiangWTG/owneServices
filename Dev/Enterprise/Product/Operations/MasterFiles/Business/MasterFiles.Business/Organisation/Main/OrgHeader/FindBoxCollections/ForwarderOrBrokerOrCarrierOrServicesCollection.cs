using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ForwarderOrBrokerOrCarrierOrServicesCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ForwarderOrBrokerOrCarrierOrServicesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ForwarderOrBrokerOrCarrierOrServicesCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ForwarderOrBrokerOrCarrierOrServicesCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ForwarderOrBrokerOrCarrierOrServicesCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, ZBool.True);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsBroker, SQLComparisonOperator.Equal, ZBool.True);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsMiscFreightServices, SQLComparisonOperator.Equal, ZBool.True);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property8", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("C128D89A-8DA0-4BCC-BC23-3C373582C344", "An Organization selected from here must have an Organization type of Forwarder, Broker, Carrier or Services selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetAtleastOneOrgTypeAsExpectedAndValidate(organisation.OH_IsForwarderInfo, organisation.OH_IsBrokerInfo, organisation.OH_IsMiscFreightServicesInfo, organisation.OH_IsShippingProviderInfo);
		}

		#endregion

	}
}
