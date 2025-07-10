using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(AddInfoModuleNkFilter))]
	sealed class AddInfoModuleNkFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new AddInfoModuleNkFilter("moo", ModuleIDs.SalesEnquiry, list, DummyBizoSchema.Z0_Code, "Z0_Code");
		}
	}
}
