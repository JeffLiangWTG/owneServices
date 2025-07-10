using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ServiceProviderCollection : OrganisationsFindBoxCollection
	{
		public ServiceProviderCollection(BusinessObjectFactory readOnlyFactory) : base(readOnlyFactory)
		{
		}

		public ServiceProviderCollection(BusinessObjectFactory readOnlyFactory, ZQuery filter) : base(readOnlyFactory, filter)
		{
		}

		public ServiceProviderCollection(BusinessObjectFactory readOnlyFactory, OrganisationDefaults defaults) : base(readOnlyFactory, defaults)
		{
		}

		public ServiceProviderCollection(BusinessObjectFactory readOnlyFactory, ZQuery filter, OrganisationDefaults defaults) : base(readOnlyFactory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter() =>
			base.CreateAdditionalFilter().AddToFilter(GetServiceProviderFilter());

		public static ZQuery GetServiceProviderFilter()
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, ZBool.True);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsBroker, ZBool.True);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);

			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.Or);

			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var orgHeader = (OrgHeader)child;
			orgHeader.CompanyData.OB_IsCreditor = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagAP;
			orgHeader.OH_IsForwarder = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagFA;
			orgHeader.OH_IsShippingProvider = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagCrr;
			orgHeader.OH_IsBroker = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagBR;
			orgHeader.OH_IsMiscFreightServices = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSV;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property1", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property8", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			var org = (OrgHeader)selectedBusinessObject;

			if (!org.OH_IsCreditor && !org.OH_IsShippingProvider && !org.OH_IsForwarder && !org.OH_IsMiscFreightServices && !org.OH_IsBroker)
			{
				errors.Add(Res.GetString("919c42f8-2f42-467a-ba96-148850b07808", "The service provider must be flagged as either: Payables, Carrier, Forwarder, Services or Broker."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			var org = (OrgHeader)entity;
			org.SetAtleastOneOrgTypeAsExpectedAndValidate(org.CompanyData.OB_IsCreditorInfo, org.OH_IsShippingProviderInfo, org.OH_IsForwarderInfo, org.OH_IsBrokerInfo, org.OH_IsMiscFreightServicesInfo);
		}

		#endregion

	}
}
