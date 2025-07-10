using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ModuleEntryHeaderCollection))]
	sealed class ModuleEntryHeaderCollectionTest : ModuleEntryHeaderCollectionTest<ModuleEntryHeaderCollection>
	{
		protected override ModuleEntryHeaderCollection GetCollectionToTest()
		{
			return new ModuleEntryHeaderCollection(new BusinessObjectFactory(), GlbCompany.CurrentCompany);
		}
	}
}
