using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	class InvoiceNumberFilterHandler : BaseJobDeclarationIndexFilterHandlerBase
	{
		public InvoiceNumberFilterHandler(FilterStripBusinessObject parent) : base(parent)
		{
		}

		public override string[] RequiredIndexSearchFields => new[] { InvoiceNumberField };

		protected override IEnumerable<ModuleFilter> GetFiltersCore()
		{
			var searchField = Parent.IndexSearchFields[InvoiceNumberField];

			var textFilter = new IndexSearchModuleTextFilter(searchField, DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber)
			{
				Category = FilterCategories.NumbersAndReferences,
				MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|InvoiceNumber", DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber),
			};

			yield return textFilter;
		}

		const string InvoiceNumberField = "NONGROUPINVOICENUMBER";
	}
}
