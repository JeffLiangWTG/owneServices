using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	[TestedType(typeof(ActiveABLEntryNumCollection))]
	public class ActiveABLEntryNumCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveABLEntryNumCollection>
	{
		public override void TestTypedget_Item()
		{
			var item = Bill.CustomsEntryNumbers.AddNew();
			var collection = new ActiveABLEntryNumCollection(ManifestHeader, Core.Constants.CountryCodes.SouthAfrica);
			collection.Load();
			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals(item, collection[0]);
		}

		protected override ActiveABLEntryNumCollection GetCollectionToTest() => new ActiveABLEntryNumCollection(ManifestHeader, Core.Constants.CountryCodes.SouthAfrica);
		protected override BusinessObject GetNewElementToAddToTheCollection() => Bill.CustomsEntryNumbers.AddNew();

		AsycudaManifestHeader manifestHeader;
		AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (manifestHeader == null)
				{
					manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				}
				return manifestHeader;
			}
		}

		AsycudaBill bill;
		AsycudaBill Bill => bill ?? (bill = ManifestHeader.Bills.AddNew());
	}
}
