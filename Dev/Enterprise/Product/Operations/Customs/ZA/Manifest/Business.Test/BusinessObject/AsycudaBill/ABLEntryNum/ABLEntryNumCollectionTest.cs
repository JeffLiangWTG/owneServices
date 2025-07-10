using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(ABLEntryNumCollection))]
	public class ABLEntryNumCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			return new ABLEntryNumCollection(bill);
		}
	}
}
