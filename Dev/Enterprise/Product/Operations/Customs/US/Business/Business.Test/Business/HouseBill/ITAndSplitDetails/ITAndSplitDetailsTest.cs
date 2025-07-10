using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ITAndSplitDetails))]
	sealed class ITAndSplitDetailsTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ITAndSplitDetails>
	{
		public void TestIITNumberMember()
		{
			var itNo = Factory.New<ITAndSplitDetails>();
			itNo.US_ITNumber = "1243";
			itNo.US_NoOfPacks = 3;
			var iITNo = itNo as IITNumber;
			AssertEquals("IT Number", "1243", iITNo.ITNumber);
			AssertEquals("Quantity", 3, iITNo.PackageQuantity);
		}

		public void TestIBillDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_ITDate = ZDateTime.Today;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MASTER1";
			masterBill.US_UI_NKBillIssuerSCAC = "AAAD";
			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HOUSE1";
			houseBill.CU_PackType = "KG";
			houseBill.US_UI_NKBillIssuerSCAC = "AAAD";
			var subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillNum = "SubBill_1 234232";
			subHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.US_UI_NKBillIssuerSCAC = "AAAD";
			subHouseBill.CU_PackType = "KG";
			var itNo = subHouseBill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "1245789";
			itNo.US_NoOfPacks = 45;
			IBillDetails iBillDetails = itNo;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals(false, iBillDetails.IsExpressTracking);
			AssertEquals("", iBillDetails.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("", iBillDetails.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("AAAD", iBillDetails.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("", iBillDetails.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("AAAD", iBillDetails.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportModeCodes.Codes.AirContainer;
			AssertEquals("", iBillDetails.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("AAAD", iBillDetails.IssuerCodeOfMasterBillNumber);
			AssertEquals("AAAD", iBillDetails.IssuerCodeOfSubHouseBillNumber);
			AssertEquals("SubBill12342", iBillDetails.SubHouseBillNumber);
			declaration.JE_TransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("", iBillDetails.IssuerCodeOfSubHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("", iBillDetails.IssuerCodeOfSubHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("", iBillDetails.IssuerCodeOfSubHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("AAAD", iBillDetails.IssuerCodeOfSubHouseBillNumber);
			AssertEquals("HOUSE1", iBillDetails.HouseBillNumber);
			AssertEquals("1245789", iBillDetails.ITNumber);
			AssertEquals(ZDateTime.Today, iBillDetails.ITDate);
			AssertEquals(45, iBillDetails.PackageQuantity);
			AssertEquals("KG", iBillDetails.PackageType);
			IBillDetails details = itNo;
			Assert(!details.IsNonAMS);
			AssertEquals(false, details.IsSplit);
			subHouseBill.US_SESplitShip = true;
			AssertEquals(true, details.IsSplit);
			itNo.US_ArrivalDate = ZDateTime.Today.AddDays(2);
			itNo.US_CarrierCode = "D0";
			itNo.US_FlightNumber = "009I";
			var split = details as IConveyanceOrSplitDetails;
			AssertEquals(ZDateTime.Today.AddDays(2), split.ArrivalDate);
			AssertEquals("D0", split.CarrierCode);
			AssertEquals("009I", split.FlightNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			subHouseBill.CU_NoOfPacks = 29m;
			AssertEquals(29, iBillDetails.PackageQuantity);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(29, iBillDetails.PackageQuantity);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Assert(details.IsNonAMS);
			declaration.Bills.RemoveAndDeleteAll();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBillExpressTracking = true;
			declaration.JE_MasterBill = "123456789";
			declaration.JE_HouseBill = "HB22222222";
			var bill = declaration.Bills[0];
			bill.CU_NoOfPacks = 100;
			bill.CU_PackType = "PK";
			bill.ITNumber = "VHH65425142";
			AssertEquals(1, bill.ITAndSplitDetails.Count);
			iBillDetails = bill.ITAndSplitDetails[0];
			AssertEquals(true, iBillDetails.IsExpressTracking);
			AssertEquals("VHH65425142", iBillDetails.ITNumber);
			AssertEquals("123456789", iBillDetails.MasterBillNumber);
			AssertEquals(ZString.Empty, iBillDetails.HouseBillNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			return bill.ITAndSplitDetails.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var result = bill.ITAndSplitDetails.AddNew();
			result.US_ITNumber = "IT2312313";
			return result;
		}
	}
}
