using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RatingHeaderLookups : AutoRatingHeaderLookups
	{
		public RatingHeaderLookups(AutoRatingHeader parent)
			: base(parent)
		{
		}

		protected new RatingHeader Parent
		{
			get { return (RatingHeader)base.Parent; }
		}

		#region Companies

		public override GlbCompanyCollection Companies
		{
			get
			{
				return Factory.GetCachedValue((NoResString)"Companies", delegate // Factory Cache Key
				{
					var filter = new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True);
					filter.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");
					return new GlbCompanyCollection(Factory, filter);
				});
			}
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollection", delegate { return new LocationCollection(Factory); }); }
		}

		#endregion

		#region Clients

		public OrgHeaderCollection Clients =>
			Parent.IsCosting()
				? CostClients
				: Parent.IsIntercompanyTariff()
					? IntercompanyTariffServiceProviders
					: RateClients;
		public OrgHeaderCollection Agents =>
			Parent.IsCosting()
				? CostClients
				: Parent.IsIntercompanyTariff()
					? IntercompanyTariffServiceProviders
					: OverseasAgents;

		OrgHeaderCollection RateClients
		{
			get
			{
				return Factory.GetCachedValue("RateClients", delegate
				{
					var query = new ZDBOnlyQuery(typeof(OrgHeader));
					query.AddToFilter(OrgHeaderSchema.OH_IsConsignee, SQLComparisonOperator.Equal, ZBool.True);
					query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignor, SQLComparisonOperator.Equal, ZBool.True);
					query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsSalesLead, SQLComparisonOperator.Equal, ZBool.True);
					query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsWarehouseClient, SQLComparisonOperator.Equal, ZBool.True);
					query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsTransportClient, SQLComparisonOperator.Equal, ZBool.True);

					var collection = new OrgHeaderCollection(Factory, query);
					collection.SetOverrideNotificationWhenAdditionalFilterNotMet(ErrorMessages.InvalidClientRateHeader);

					return collection;
				});
			}
		}

		OrgHeaderCollection OverseasAgents
		{
			get
			{
				return Factory.GetCachedValue("OverseasAgents", delegate
				{
					var query = new ZDBOnlyQuery(typeof(OrgHeader));
					query.AddToFilter(OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, ZBool.True);
					query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsControllingAgent, SQLComparisonOperator.Equal, ZBool.True);

					var collection = new OrgHeaderCollection(Factory, query);
					collection.SetOverrideNotificationWhenAdditionalFilterNotMet(ErrorMessages.InvalidClientRateHeader);

					return collection;
				});
			}
		}

		ServiceProviderCollection CostClients
		{
			get { return Factory.GetCachedValue("ServiceProviderCollection", delegate { return new ServiceProviderCollection(Factory); }); }
		}

		OrgHeaderCollection IntercompanyTariffServiceProviders =>
			Factory.GetCachedValue(nameof(IntercompanyTariffServiceProviders), delegate
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				var companyOrgProxiesSubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.GC_OH_OrgProxy);
				var branchOrgProxiesSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_OH_OrgProxy);
				query.AddSubQuery(companyOrgProxiesSubQuery, JoinCondition.Or);
				query.AddSubQuery(branchOrgProxiesSubQuery, JoinCondition.Or);

				var collection = new OrgHeaderCollection(Factory, query);
				collection.SetOverrideNotificationWhenAdditionalFilterNotMet(ErrorMessages.OrgProxyIsMandatoryForIntercompanyTariff);

				return collection;
			});

		#endregion

		#region Quote Cancellation Reason

		public CodeDescriptionPairList ActiveQuoteCancellationReasonCodes
		{
			get { return RatingDataRegistry.Instance.QuoteCancellationReasonCodes.Value.GetActiveCodeDescriptionPairList(); }
		}

		public CodeDescriptionPairList QuoteCancellationReasonCodes
		{
			get { return RatingDataRegistry.Instance.QuoteCancellationReasonCodes.Value.GetCodeDescriptionPairList(); }
		}

		#endregion
	}
}

