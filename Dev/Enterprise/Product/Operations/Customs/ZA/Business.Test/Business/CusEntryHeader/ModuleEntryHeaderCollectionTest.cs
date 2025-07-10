using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ModuleEntryHeaderCollection))]
	sealed class ModuleEntryHeaderCollectionTest : Customs.Business.Testing.ModuleEntryHeaderCollectionTest<ModuleEntryHeaderCollection>
	{
		protected override ModuleEntryHeaderCollection GetCollectionToTest() => new ModuleEntryHeaderCollection(new BusinessObjectFactory());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "TEST_REF";
			Factory.Save();
			return entry;
		}
	}
}
