using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(OutturnAndGateInOutModuleCollection))]
	sealed class OutturnAndGateInOutModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultFilter()
		{
			var newFactory = new BusinessObjectFactory();
			var header1 = newFactory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			var header2 = newFactory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.AMA_ApplicationCode = header1.AMA_ApplicationCode;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.China;
			newFactory.Save();
			var header = Factory.Load<ManifestBase.AsycudaManifestHeader>(header1.PK);
			CombineAssertions(() =>
			{
				var testCollection = GetCollectionToTest();
				testCollection.Load();
				AssertEquals(1, testCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { header }, testCollection);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OutturnAndGateInOutModuleCollection(Factory);
	}
}
