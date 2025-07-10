using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BillOfLadingValidationTest : BaseAgencyTest
	{
		public void TestValidateJS_HouseBill()
		{
			const string message = "You have not entered an Ocean Bill Of Lading.";
			const string duplicateHouseBill = "The same Bill number already exists on V0000101.\r\nBill of Lading number must be unique per vessel-voyage.";
			Shipment.JS_HouseBill = "";
			Shipment.Validation.ValidateJS_HouseBill();
			AssertHasMessageError(Shipment.JS_HouseBillInfo, message);
			Shipment.JS_HouseBill = "Blat2981123";
			Shipment.Validation.ValidateJS_HouseBill();
			AssertNoMessageError(Shipment.JS_HouseBillInfo, message);
			SetSailings();
			BillOfLading shipment1 = Factory.NewWithValidTestData<BillOfLading>();
			shipment1.JS_JX = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, Destination2.JB_RL_NKPortOfDischarge).PK;
			shipment1.JS_UniqueConsignRef = "V0000101";
			shipment1.JS_HouseBill = "BillNumber 1";
			Factory.Save();
			BillOfLading shipment2 = Factory.NewWithValidTestData<BillOfLading>();
			shipment2.JS_JX = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin2.JA_RL_NKPortOfLoading, Destination3.JB_RL_NKPortOfDischarge).PK;
			shipment2.JS_UniqueConsignRef = "V0000102";
			shipment2.JS_HouseBill = "BillNumber 1";
			shipment2.Validation.ValidateJS_HouseBill();
			AssertHasError(shipment2.JS_HouseBillInfo, duplicateHouseBill);
			AssertNoWarning(shipment2.JS_HouseBillInfo, "This Bill number is already in use on: \r\nV0000101\r\n");
			shipment2.JS_HouseBill = "BillNumber 2";
			shipment2.Validation.ValidateJS_HouseBill();
			AssertNoError(shipment2.JS_HouseBillInfo, duplicateHouseBill);
			shipment2.JS_HouseBill = "BillNumber 1";
			shipment2.JS_JX = ZGuid.NewZGuid();
			shipment2.Validation.ValidateJS_HouseBill();
			AssertNoError(shipment2.JS_HouseBillInfo, duplicateHouseBill);
			AssertHasWarning(shipment2.JS_HouseBillInfo, "This Bill number is already in use on: \r\nV0000101\r\n");
		}

		#region Implementation
		BillOfLading Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<BillOfLading>());
			}
		}

		BillOfLading shipment;
		JobVoyage Voyage;
		VoyageOrigin Origin1;
		VoyageOrigin Origin2;
		VoyageOrigin Origin3;
		VoyageDestination Destination1;
		VoyageDestination Destination2;
		VoyageDestination Destination3;
		JobSailing Sailing4;
		JobVoyage OtherVoyage;
		VoyageOrigin OtherOrigin1;
		JobSailing OtherSailing1;
		public void SetSailings()
		{
			Voyage = Factory.New<JobVoyage>();
			Origin1 = Voyage.Origins.AddNew();
			Origin1.JA_RL_NKPortOfLoading = "AUSYD";
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(10);
			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Origin2.JA_E_DEP = ZDateTime.Now.AddDays(16);
			Origin3 = Voyage.Origins.AddNew();
			Origin3.JA_RL_NKPortOfLoading = "NZAKL";
			Origin3.JA_E_DEP = ZDateTime.Now.AddDays(22);
			Destination1 = Voyage.Destinations.AddNew();
			Destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			Destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);
			Destination2 = Voyage.Destinations.AddNew();
			Destination2.JB_RL_NKPortOfDischarge = "NZAKL";
			Destination2.JB_E_ARV = ZDateTime.Now.AddDays(20);
			Destination3 = Voyage.Destinations.AddNew();
			Destination3.JB_RL_NKPortOfDischarge = "SGSIN";
			Destination3.JB_E_ARV = ZDateTime.Now.AddDays(26);
			Sailing4 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin3.JA_RL_NKPortOfLoading, Destination3.JB_RL_NKPortOfDischarge);
			Sailing4.JX_JA = Origin3.PK;
			Sailing4.JX_JB = Destination3.PK;
		}

		public void SetOtherSailing()
		{
			OtherVoyage = Factory.New<JobVoyage>();
			OtherOrigin1 = OtherVoyage.Origins.AddNew();
			OtherOrigin1.JA_RL_NKPortOfLoading = "AUSYD";
			OtherOrigin1.JA_E_DEP = ZDateTime.Now.AddDays(10);
			VoyageDestination destination1 = OtherVoyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);
			OtherSailing1 = OtherVoyage.Sailings.GetSailingFromLoadAndDischarge(OtherOrigin1.JA_RL_NKPortOfLoading, destination1.JB_RL_NKPortOfDischarge);
			OtherSailing1.JX_JA = OtherOrigin1.PK;
			OtherSailing1.JX_JB = destination1.PK;
		}
		#endregion
	}
}
