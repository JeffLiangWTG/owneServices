using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(BillCollection))]
	sealed class HouseBillCollectionTest : Customs.Business.Testing.BaseHouseBillCollectionTest
	{
		public void TestHasAtLeastHaveReceivedArrivedStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			var bill = declaration.Bills.AddNew();
			var disp = bill.DispositionCodes.AddNew();
			disp.US_Code = CargoReleaseProcessingResultList.Codes.AgricultureManifestHoldRemoved;
			Assert(!declaration.Bills.HasAtLeastHaveReceivedArrivedStatus);
			var disp2 = bill.DispositionCodes.AddNew();
			disp2.US_Code = CargoReleaseProcessingResultList.Codes.BillArrived;
			Assert(declaration.Bills.HasAtLeastHaveReceivedArrivedStatus);
		}

		public void TestAddPackageLineIfNoneExists()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			CusContainer container = declaration.CusContainers.AddNew();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "1";
			AssertEquals(1, bill.PackingGroups.Count);
			AssertEquals(1, bill.PackingGroups[0].Packages.Count);
			AssertEquals(container, bill.PackingGroups[0].Container);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(true, declaration.IsPluggedIntoShipment);
			CusContainer container2 = declaration.CusContainers.AddNew();
			AssertEquals(2, declaration.CusContainers.Count);
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "2";
			AssertEquals("not defaulted as packages will be synchronised from shipment", 0, bill2.PackingGroups.Count);
			declaration.JE_OverrideFreightDefaults = true;
			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill3.CU_BillNum = "3";
			AssertEquals(1, bill3.PackingGroups.Count);
			AssertEquals(1, bill3.PackingGroups[0].Packages.Count);
			AssertEquals("container is not defaulted", container2.PK, bill3.PackingGroups[0].CR_CO_Container);
		}

		public void TestFindBySequenceNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill masterBill1 = declaration.Bills.AddNew();
			masterBill1.US_SequenceNo = 1;
			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.US_SequenceNo = 2;
			AssertEquals(masterBill1, declaration.Bills.FindByUS_SequenceNo("1"));
			AssertEquals(masterBill1, declaration.Bills.FindByUS_SequenceNo("0001"));
			AssertEquals(masterBill2, declaration.Bills.FindByUS_SequenceNo("2"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			return declaration.Bills;
		}

		protected override Customs.Business.BaseJobDeclaration GetDeclaration()
		{
			var result = base.GetDeclaration();
			((JobDeclaration)result).DisableDefaultPackingInformation = true;
			return result;
		}
	}
}
