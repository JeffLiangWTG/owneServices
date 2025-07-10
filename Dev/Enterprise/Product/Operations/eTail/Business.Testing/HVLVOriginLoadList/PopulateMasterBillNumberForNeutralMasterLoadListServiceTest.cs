using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing;

[TestedType(typeof(PopulateMasterBillNumberForNeutralMasterLoadListService))]
public class PopulateMasterBillNumberForNeutralMasterLoadListServiceTest : TestCaseWithFactory
{
	public void TestPopulateMasterBillNumberForNeutralMasterLoadListService_PopulateMasterBillNumber()
	{
		const string masterBillNumber = "MAWB";
		var consolForLoadListNeutralMaster = Factory.NewWithValidTestData<ForwardingConsol>();
		consolForLoadListNeutralMaster.JK_MasterBillNum = masterBillNumber;

		var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
		loadList.HVL_Status = "LDG";
		loadList.HVL_IsNeutralMaster = true;

		var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
		bookingHeader.HVH_IsBookingConfirmed = true;
		bookingHeader.HVH_IsBookingReceived = true;
		var consignment = bookingHeader.Consignments.AddNew();
		var item = consignment.Items.AddNew();
		item.HVI_HVL_LoadList = loadList.PK;

		var service = new PopulateMasterBillNumberForNeutralMasterLoadListService(consolForLoadListNeutralMaster, loadList);

		service.ProcessBusinesObjects([]);

		AssertEquals("Load list's master bill number should be populated.", loadList.HVL_MasterBillNumber, consolForLoadListNeutralMaster.JK_MasterBillNum);
	}
}
