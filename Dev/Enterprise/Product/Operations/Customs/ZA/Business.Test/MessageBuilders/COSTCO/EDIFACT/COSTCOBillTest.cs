using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class COSTCOBillTest : TestCaseWithFactory
	{
		public void TestLineNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("1", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).LineNumber);
		}

		public void TestTransportDocumentNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "VWG";
			AssertEquals("VWG", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).TransportDocumentNumber);
		}

		public void TestCargoCarrierCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillIssuer = "VWG";
			AssertEquals("VWG", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).CargoCarrierCode);
		}

		public void TestExternalReference()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "VWG";
			var bill = header.Bills.AddNew();
			AssertEquals("VWG", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).ExternalReference);
		}

		public void TestMasterCargoCarrierCode()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "VVV";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "GGG", Core.Constants.CountryCodes.SouthAfrica);
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.OA_Address1 = "VVV Address";
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = carrierAddress.PK;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "VWG";
			AssertEquals("GGG", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).MasterCargoCarrierCode);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("GGG", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).MasterCargoCarrierCode);
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("GGG", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).MasterCargoCarrierCode);
			header.AMA_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("GGG", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).MasterCargoCarrierCode);
		}

		public void TestMasterBillOfLadingNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.MasterBill.ABL_BillNumber = "VWG";
			AssertEquals("VWG", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).MasterBillOfLadingNumber);
		}

		public void TestConsolidationIndicator()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_AgentType = Core.Constants.AgentType.Agent;
			var bill = header.Bills.AddNew();
			AssertEquals("CO", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).ConsolidationIndicator);
			header.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("CO", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).ConsolidationIndicator);
			header.AMA_AgentType = Core.Constants.TransportModes.Other;
			AssertEquals("CO", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).ConsolidationIndicator);
			header.AMA_AgentType = Core.Constants.AgentType.AWBCoload;
			AssertEquals("ST", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).ConsolidationIndicator);
		}

		public void TestLRNExit()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "456";
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "123";
			entryNumber.CE_EntryType = Enterprise.Customs.Common.CusEntryNumberTypes.SouthAfrica.LocalReferenceNumber;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			entryNumber.CE_ParentID = bill.PK;
			entryNumber.CE_ParentTable = bill.TableName;
			Factory.Save();
			AssertEquals("123", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).LRNExit);
		}

		public void TestExportProcedure()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.CustomsCPC = "123";
			AssertEquals("123", ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).ExportProcedure);
		}

		public void TestPacks()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Packs.AddNew();
			AssertEquals(true, ((ICOSTCOLineLevelInformation)new COSTCOBill(bill, 0)).Packs.Any());
		}
	}
}
