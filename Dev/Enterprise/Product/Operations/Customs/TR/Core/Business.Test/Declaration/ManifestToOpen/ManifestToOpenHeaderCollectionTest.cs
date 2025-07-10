using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(ManifestToOpenHeaderCollection))]
	class ManifestToOpenHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<ManifestToOpenHeaderCollection>
	{
		protected override ManifestToOpenHeaderCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new ManifestToOpenHeaderCollection(declaration);
		}

		public void TestSetDefaultsForNewElementCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var manifest = declaration.ManifestToOpenHeaders.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("CE_ParentTable", declaration.TableName, manifest.CE_ParentTable);
				AssertEquals("CE_EntryType", CusEntryNumberTypes.Turkey.ManifestToOpen, manifest.CE_EntryType);
				AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Turkey, manifest.CE_RN_NKCountryCode);
			});
		}
	}
}
