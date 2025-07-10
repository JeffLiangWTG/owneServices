using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(ContainerNumberFilterHandler))]
	sealed class ContainerNumberFilterHandlerTest : BaseJobDeclarationIndexFilterHandlerTestCase<ContainerNumberFilterHandler>
	{
		protected override IReadOnlyCollection<string> ExpectedQueries =>
			new[]
			{
				"(startswith(CONTAINERNUMBER,'DEF123'))",
			};

		protected override void SimulateFilterAdded(FilterStripBusinessObject filterStripBusinessObject)
		{
			var agentRef = filterStripBusinessObject.ModuleFilters.First(mf => mf.Category == FilterCategories.NumbersAndReferences &&
																			   mf.Description == DeclarationFilterConstants.NumberFilterTypes.ContainerNumber) as IndexSearchModuleTextFilter;

			agentRef.Property = "DEF123";
			agentRef.IsActive = true;

			base.SimulateFilterAdded(filterStripBusinessObject);
		}
	}
}
