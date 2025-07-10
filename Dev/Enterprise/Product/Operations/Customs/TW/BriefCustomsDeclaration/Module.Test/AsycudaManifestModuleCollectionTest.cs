using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Module.Testing
{
	[TestedType(typeof(AsycudaManifestModuleCollection))]
	sealed class AsycudaManifestModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "UU1";
			var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.China;
			header2.AMA_JobReference = "UU2";

			var header3 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header3.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TRETrade;
			header3.AMA_JobReference = "UU3";

			var collection = GetCollectionToTest();
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { header1 }, collection);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaManifestModuleCollection(Factory);
	}
}
