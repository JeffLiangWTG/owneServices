using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	sealed class AIMMessageChooserLookupsTest : TestCaseWithFactory
	{
		public void TestReasonList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new AIMMessageChooser(header, items, string.Empty);
			var list = chooser.Lookups.ReasonList;
			var cachedList = chooser.Lookups.ReasonList;
			AssertType<AIMReasonCodes>(list);
			AssertSame("Should be cached.", cachedList, list);
		}

		public void TestFreightStatusRequestCodeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FSQ);
			var filteredList = chooser.Lookups.FreightStatusRequestCodeList;
			AssertType<AIMFreightStatusRequestCodes>(filteredList);
			Assert(!filteredList.ContainsCode(AIMFreightStatusRequestCodes.Codes.RequestForHouseInformationAssociatedToMaster));
			AssertEquals(5, filteredList.Count);
			AssertSame("Should be cached.", filteredList, chooser.Lookups.FreightStatusRequestCodeList);
			chooser.IsManifestMessage = true;
			var fullList = chooser.Lookups.FreightStatusRequestCodeList;
			AssertType<AIMFreightStatusRequestCodes>(fullList);
			Assert(fullList.ContainsCode(AIMFreightStatusRequestCodes.Codes.RequestForHouseInformationAssociatedToMaster));
			AssertEquals(6, fullList.Count);
			AssertSame("Should be cached.", fullList, chooser.Lookups.FreightStatusRequestCodeList);
		}
	}
}
