using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(ETradeCollection))]
	public class ETradeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "UU1";
			var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_JobReference = "UU2";
			header2.AMA_ApplicationCode = header1.AMA_ApplicationCode;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.China;
			Factory.Save();

			var header = Factory.Load<ManifestBase.AsycudaManifestHeader>(header1.PK);

			var collection = GetCollectionToTest();
			collection.Load();

			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { header }, collection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ETradeCollection(Factory);
		}
	}
}
