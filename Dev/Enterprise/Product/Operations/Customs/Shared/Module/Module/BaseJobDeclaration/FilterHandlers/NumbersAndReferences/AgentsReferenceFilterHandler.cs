using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public class AgentsReferenceFilterHandler : BaseJobDeclarationIndexFilterHandlerBase
	{
		public AgentsReferenceFilterHandler(FilterStripBusinessObject parent) : base(parent)
		{
		}

		public override string[] RequiredIndexSearchFields => new[] { AgentReferenceField };

		protected override IEnumerable<ModuleFilter> GetFiltersCore()
		{
			var searchField = Parent.IndexSearchFields[AgentReferenceField];

			var textFilter = new IndexSearchModuleTextFilter(searchField, DeclarationFilterConstants.NumberFilterTypes.AgentsReference)
			{
				Category = FilterCategories.NumbersAndReferences,
				MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|AgentsReference", DeclarationFilterConstants.NumberFilterTypes.AgentsReference),
			};

			yield return textFilter;
		}

		const string AgentReferenceField = "AGENTSREFERENCE";
	}
}
