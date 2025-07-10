using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public class ContainerNumberFilterHandler : BaseJobDeclarationIndexFilterHandlerBase
	{
		public ContainerNumberFilterHandler(FilterStripBusinessObject parent) : base(parent)
		{
		}

		public override string[] RequiredIndexSearchFields => new[] { ContainerNumberField };

		protected override IEnumerable<ModuleFilter> GetFiltersCore()
		{
			var searchField = Parent.IndexSearchFields[ContainerNumberField];

			var textFilter = new IndexSearchModuleTextFilter(searchField, DeclarationFilterConstants.NumberFilterTypes.ContainerNumber)
			{
				Category = FilterCategories.NumbersAndReferences,
				MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ContainerNumber", DeclarationFilterConstants.NumberFilterTypes.ContainerNumber),
			};

			yield return textFilter;
		}

		const string ContainerNumberField = "CONTAINERNUMBER";
	}
}
