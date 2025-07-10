using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class SPTSContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMandatoryCheckOfBC_ContainerNum()
		{
			var header = Factory.New<SPTSHeader>();
			var sptsHeaderContainer = header.HeaderContainers.AddNew();

			sptsHeaderContainer.BC_ContainerNum = ZString.Empty;
			AssertHasMessageErrorContaining(sptsHeaderContainer.BC_ContainerNumInfo, "You have not entered a Container No.");

			sptsHeaderContainer.BC_ContainerNum = "XX1111";
			AssertNoMessageErrorContaining(sptsHeaderContainer.BC_ContainerNumInfo, "You have not entered a Container No.");

			var bill = header.Bills.AddNew();
			var sptsBillContainer = bill.SPTSBillContainers.AddNew();

			sptsBillContainer.BC_ContainerNum = ZString.Empty;
			AssertHasMessageErrorContaining(sptsBillContainer.BC_ContainerNumInfo, "You have not entered a Container No.");

			sptsBillContainer.BC_ContainerNum = "XX1111";
			AssertNoMessageErrorContaining(sptsBillContainer.BC_ContainerNumInfo, "You have not entered a Container No.");
		}

		public void TestDuplicateCheckOfBC_ContainerNum()
		{
			var header = Factory.New<SPTSHeader>();

			var headercontainer1 = header.HeaderContainers.AddNew();
			headercontainer1.BC_ContainerNum = "XX1111";
			AssertNoMessageErrorContaining(headercontainer1.BC_ContainerNumInfo, "This Container Number already exists.");

			var headercontainer2 = header.HeaderContainers.AddNew();
			headercontainer2.BC_ContainerNum = "XX2222";
			AssertNoMessageErrorContaining(headercontainer2.BC_ContainerNumInfo, "This Container Number already exists.");

			var headercontainer3 = header.HeaderContainers.AddNew();
			headercontainer3.BC_ContainerNum = "XX1111";
			AssertHasMessageErrorContaining(headercontainer3.BC_ContainerNumInfo, "This Container Number already exists.");

			var bill = header.Bills.AddNew();

			var billContainer1 = bill.SPTSBillContainers.AddNew();
			billContainer1.BC_ContainerNum = "XX1111";
			AssertNoMessageErrorContaining(billContainer1.BC_ContainerNumInfo, "This container number has already been selected.");

			var billContainer2 = bill.SPTSBillContainers.AddNew();
			billContainer2.BC_ContainerNum = "XX2222";
			AssertNoMessageErrorContaining(billContainer2.BC_ContainerNumInfo, "This container number has already been selected.");

			var billContainer3 = bill.SPTSBillContainers.AddNew();
			billContainer3.BC_ContainerNum = "XX1111";
			AssertHasMessageErrorContaining(billContainer3.BC_ContainerNumInfo, "This container number has already been selected.");
		}
	}
}
