using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[ModuleID(ModuleId.CusDec)]
	public class BaseJobDeclarationCollection : BusinessObjectCollection<BaseJobDeclaration>, Integration.Customs.IBaseJobDeclarationCollection
	{
		public BaseJobDeclarationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BaseJobDeclarationCollection(BusinessObjectFactory factory, GlbCompany company)
			: this(factory, GetCompanyQuery(company.PK))
		{
		}

		public BaseJobDeclarationCollection(BusinessObjectFactory factory, ZString countryCode)
			: this(factory, GetCountryQuery(countryCode))
		{
		}

		public BaseJobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: this(factory, GetCompanyQuery(companyPkToFilterOn))
		{
			CompanyPkToFilterOn = companyPkToFilterOn;
		}
		internal readonly ZGuid CompanyPkToFilterOn;

		public BaseJobDeclarationCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		protected internal static ZDBOnlyQuery GetCountryQuery(ZString countryCode)
		{
			var query = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			if (ObjectFactory.Get<ITagRulePolicy>().ShouldAddCompanyRelatedFilters)
			{
				var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
				branchQuery.AddToFilter(new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, countryCode));
				query.AddSubQuery(branchQuery, JoinCondition.And);
			}

			return query;
		}

		public static ZDBOnlyQuery GetCompanyQuery(ZGuid companyPkToFilterOn)
		{
			var query = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			if (ObjectFactory.Get<ITagRulePolicy>().ShouldAddCompanyRelatedFilters)
			{
				var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
				branchQuery.AddToFilter(GlbBranchSchema.GB_GC, companyPkToFilterOn);
				query.AddSubQuery(branchQuery, JoinCondition.And);
			}

			return query;
		}

		#region FindBox List Provider

		protected override IFindBoxListProvider FindBoxListProvider => new BaseJobDeclarationCollectionProvider(this);

		public class BaseJobDeclarationCollectionProvider : FindBoxListProvider
		{
			public BaseJobDeclarationCollectionProvider(BaseJobDeclarationCollection collection)
				: base(collection)
			{
			}

			new BaseJobDeclarationCollection List => (BaseJobDeclarationCollection)base.List;

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code) => base.BizObjsFromCodeWithCompleteFilter(code);

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				base.AddCodeEqualsFilter(query, code);
				AddCompanyQueryIfNecessary(query);
			}

			protected override void AddCodeStartsWithFilter(ZQuery query, string code)
			{
				base.AddCodeStartsWithFilter(query, code);
				AddCompanyQueryIfNecessary(query);
			}

			protected override void AddDescriptionEqualsFilter(ZQuery query, string description)
			{
				base.AddDescriptionEqualsFilter(query, description);
				AddCompanyQueryIfNecessary(query);
			}

			protected override void AddDescriptionStartsWithFilter(ZQuery query, string description)
			{
				base.AddDescriptionStartsWithFilter(query, description);
				AddCompanyQueryIfNecessary(query);
			}

			protected void AddCompanyQueryIfNecessary(ZQuery query)
			{
				var companyPkToFilterOn = List.CompanyPkToFilterOn;
				if (!companyPkToFilterOn.IsEmpty)
				{
					query.AddToFilter(BaseJobDeclarationCollection.GetCompanyQuery(companyPkToFilterOn));
				}
			}
		}

		#endregion

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new FetchStrategies.BaseJobDeclarationCollectionFetchStrategy(this);
	}

	public class JobDeclarationCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.IBaseJobDeclarationCollectionProvider
	{
		public JobDeclarationCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection() => new BaseJobDeclarationCollection(BusinessObjectFactory);

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.JobDeclaration;

		public override int MaxLength => JobDeclarationSchema.JE_DeclarationReference.MaxLength;
	}
}
