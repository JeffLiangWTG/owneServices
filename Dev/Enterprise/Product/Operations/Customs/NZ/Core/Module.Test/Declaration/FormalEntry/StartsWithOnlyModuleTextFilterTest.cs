using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(NZJobDeclarationFilterBusinessObject.StartsWithOnlyModuleTextFilter))]
	sealed class StartsWithOnlyModuleTextFilterTest : ModuleTextFilterTest
	{
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new NZJobDeclarationFilterBusinessObject.StartsWithOnlyModuleTextFilter("moo", (_, x_) => new ZQuery());
		}
	}
}
