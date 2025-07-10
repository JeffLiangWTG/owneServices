using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(InvoiceNumberFilterHandler))]
	sealed class InvoiceNumberFilterHandlerTest : BaseJobDeclarationIndexFilterHandlerTestCase<InvoiceNumberFilterHandler>
	{
		protected override IReadOnlyCollection<string> ExpectedQueries =>
			new[]
			{
				"(startswith(NONGROUPINVOICENUMBER,'ERT123'))",
			};

		protected override void SimulateFilterAdded(FilterStripBusinessObject filterStripBusinessObject)
		{
			var agentRef = filterStripBusinessObject.ModuleFilters.First(mf => mf.Category == FilterCategories.NumbersAndReferences &&
																			   mf.Description == DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber) as IndexSearchModuleTextFilter;

			agentRef.Property = "ERT123";
			agentRef.IsActive = true;

			base.SimulateFilterAdded(filterStripBusinessObject);
		}
	}
}
