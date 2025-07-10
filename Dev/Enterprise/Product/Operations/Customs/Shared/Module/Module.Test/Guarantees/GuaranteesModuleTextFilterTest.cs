using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GuaranteesModuleTextFilter))]
	sealed class GuaranteesModuleTextFilterTest : ModuleTextFilterTest
	{
		protected override BusinessObject GetNewBusinessObject() => new GuaranteesModuleTextFilter("Reference", new GuaranteesFilterStripBusinessObject());
	}
}
