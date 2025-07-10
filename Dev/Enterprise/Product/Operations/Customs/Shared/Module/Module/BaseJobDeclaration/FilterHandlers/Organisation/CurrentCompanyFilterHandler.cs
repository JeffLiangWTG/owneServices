using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Module
{
	public sealed class CurrentCompanyFilterHandler : BaseJobDeclarationIndexFilterHandlerBase
	{
		public CurrentCompanyFilterHandler(FilterStripBusinessObject parent) : base(parent)
		{ }

		#region Overrides of IndexFilterHandlerBase

		public override string[] RequiredIndexSearchFields => new[] { CompanyCodeField };

		protected override bool IsApplicableCore()
		{
			return ObjectFactory.Get<ITagRulePolicy>().ShouldAddCompanyRelatedFilters;
		}

		protected override IEnumerable<ModuleFilter> GetFiltersCore()
		{
			var searchField = Parent.IndexSearchFields[CompanyCodeField];
			var textFilter = new IndexSearchModuleTextFilter(searchField, DeclarationFilterConstants.Country)
			{
				Category = FilterCategories.Organisations,
				IsPublishedOnWeb = false,
				MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|Country", DeclarationFilterConstants.Country),
				ComparisonOperator = IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AnyExact,
				Property = GlbCompany.CurrentCompany.GC_Code
			};

			if (!Globals.IsWeb)
			{
				textFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}

			yield return textFilter;
		}

		#endregion

		const string CompanyCodeField = "CompanyCode";
	}
}
