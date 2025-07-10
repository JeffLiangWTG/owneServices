using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(AgentsReferenceFilterHandler))]
	sealed class AgentsReferenceFilterHandlerTest : BaseJobDeclarationIndexFilterHandlerTestCase<AgentsReferenceFilterHandler>
	{
		protected override IReadOnlyCollection<string> ExpectedQueries =>
			new[]
			{
				"(startswith(AGENTSREFERENCE,'ABC123'))",
			};

		protected override void SimulateFilterAdded(FilterStripBusinessObject filterStripBusinessObject)
		{
			var agentRef = filterStripBusinessObject.ModuleFilters.First(mf => mf.Category == FilterCategories.NumbersAndReferences &&
																			   mf.Description == DeclarationFilterConstants.NumberFilterTypes.AgentsReference) as IndexSearchModuleTextFilter;

			agentRef.Property = "ABC123";
			agentRef.IsActive = true;

			base.SimulateFilterAdded(filterStripBusinessObject);
		}
	}
}
