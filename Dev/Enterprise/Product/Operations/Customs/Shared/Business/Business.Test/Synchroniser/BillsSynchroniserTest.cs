using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BillsSynchroniserTest : TestCaseWithFactory
	{
		public void TestSyncJE_MasterBill_HouseBill()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			ZString localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode)).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRANY";
			consol.JK_RL_NKDischargePort = localPort;
			consol.JK_MasterBillNum = "Mo89342879"; // should have a lower case character

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "H098342";

			var expConsol = Factory.New<ForwardingConsol>();
			expConsol.JK_RL_NKLoadPort = localPort;
			expConsol.JK_RL_NKDischargePort = "KRANY";
			expConsol.JK_MasterBillNum = "Mo89342879"; // should have a lower case character
			shipment.Consols.Add(expConsol);

			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("PreCondition", expConsol, declaration.RelevantConsol);

			declaration.ShipmentSynchroniser.SetEnabled(true, false);

			declaration.ShipmentSynchroniser.Synchronise();
			AssertEquals("JE_MasterBill synchronised", "MO89342879", declaration.JE_MasterBill);
			AssertEquals("JE_HouseBill synchronised", "H098342", declaration.JE_HouseBill);

			AssertEquals("2 bills", 2, declaration.Bills.Count);
			var primaryHouseBill = declaration.PrimaryHouseBill;
			var primaryMasterBill = declaration.PrimaryMasterBill;

			AssertEquals("MO89342879", primaryMasterBill.CU_BillNum);
			AssertEquals("H098342", primaryHouseBill.CU_BillNum);
			AssertEquals("primaryHouseBill.ParentBill", primaryMasterBill, primaryHouseBill.ParentBill);

			var primaryMasterBillPK = primaryMasterBill.PK;
			var primaryHouseBillPK = primaryHouseBill.PK;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("PreCondition", consol, declaration.RelevantConsol);

			AssertEquals("JE_MasterBill synchronised", "MO89342879", declaration.JE_MasterBill);
			AssertEquals("JE_HouseBill synchronised", "H098342", declaration.JE_HouseBill);

			AssertEquals("2 bills", 2, declaration.Bills.Count);
			primaryHouseBill = declaration.PrimaryHouseBill;
			primaryMasterBill = declaration.PrimaryMasterBill;

			AssertEquals("MO89342879", primaryMasterBill.CU_BillNum);
			AssertEquals("H098342", primaryHouseBill.CU_BillNum);
			AssertEquals("primaryHouseBill.ParentBill", primaryMasterBill, primaryHouseBill.ParentBill);
			AssertEquals(primaryHouseBillPK, primaryHouseBill.PK);
			AssertEquals(primaryMasterBillPK, primaryMasterBill.PK);

			shipment.JS_HouseBill = "HB2";
			AssertEquals("JE_HouseBill synchronised", "HB2", declaration.JE_HouseBill);
			AssertEquals("HB2", declaration.PrimaryHouseBill.CU_BillNum);

			primaryHouseBill = declaration.PrimaryHouseBill;
			AssertEquals(primaryHouseBillPK, primaryHouseBill.PK);
		}

		public void TestSynchroniserAssemblyMasterCS00247420()
		{
			ZString localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Code)).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRANY";
			consol.JK_RL_NKDischargePort = localPort;
			consol.JK_MasterBillNum = "M89342879";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_HouseBill = "H098342";

			var shipment1 = shipment.CoLoadShipments.AddNew();
			shipment1.JS_HouseBill = "SH1";

			var shipment2 = shipment.CoLoadShipments.AddNew();
			shipment2.JS_HouseBill = "SH2";
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			declaration.ShipmentSynchroniser.Synchronise();
			AssertEquals("HouseBill", "H098342", declaration.JE_HouseBill);
			AssertEquals("only house bill from assembly-master shipment should be synched", 1, declaration.Bills.Cast<Bill>().Count(bill => !bill.IsMasterBill));
		}

		public void TestSynchroniserBuyersConsolLead()
		{
			ZString localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Code)).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRANY";
			consol.JK_RL_NKDischargePort = localPort;
			consol.JK_MasterBillNum = "M89342879";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_OuterPacks = 100;
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_HouseBill = "AAAAA";

			var shipment1 = shipment.CoLoadShipments.AddNew();
			shipment1.JS_HouseBill = "BBBBB";
			shipment1.JS_OuterPacks = 200;
			shipment1.JS_F3_NKPackType = "PKG";

			var shipment2 = shipment.CoLoadShipments.AddNew();
			shipment2.JS_HouseBill = "CCCCC";
			shipment2.JS_OuterPacks = 300;
			shipment2.JS_F3_NKPackType = "PAI";
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			declaration.ShipmentSynchroniser.Synchronise();
			var mb = declaration.Bills.Cast<Bill>().First(bill => bill.IsMasterBill);
			var hb = declaration.Bills.Cast<Bill>().First(bill => bill.IsHouseBill && bill.CU_HouseBill == shipment.JS_HouseBill);
			var hb1 = declaration.Bills.Cast<Bill>().First(bill => bill.IsHouseBill && bill.CU_HouseBill == shipment1.JS_HouseBill);
			var hb2 = declaration.Bills.Cast<Bill>().First(bill => bill.IsHouseBill && bill.CU_HouseBill == shipment2.JS_HouseBill);
			AssertEquals(100, (int)hb.CU_NoOfPacks);
			AssertEquals("PLT", hb.CU_PackType);
			AssertEquals(200, (int)hb1.CU_NoOfPacks);
			AssertEquals("PKG", hb1.CU_PackType);
			AssertEquals(300, (int)hb2.CU_NoOfPacks);
			AssertEquals("PAI", hb2.CU_PackType);
		}

		public void TestSyncJE_BillsIssueDate()
		{
			var now = ZDateTime.Today;
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Code)).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRANY";
			consol.JK_RL_NKDischargePort = localPort;
			consol.JK_MasterBillNum = "M89342879";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "LLLLL";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			var shipment1 = shipment.CoLoadShipments.AddNew();
			shipment1.JS_HouseBill = "HBL1";
			var shipment2 = shipment.CoLoadShipments.AddNew();
			shipment2.JS_HouseBill = "HBL2";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			consol.JK_MasterBillIssueDate = now;
			shipment2.JS_HouseBillIssueDate = now.AddDays(-3);
			shipment1.JS_HouseBillIssueDate = now.AddDays(-2);
			shipment.JS_HouseBillIssueDate = now.AddDays(-1);

			var mb = declaration.Bills.Cast<Bill>().First(bill => bill.IsMasterBill);
			var hb = declaration.Bills.Cast<Bill>().First(bill => bill.IsHouseBill && bill.CU_HouseBill == shipment.JS_HouseBill);
			var hb1 = declaration.Bills.Cast<Bill>().First(bill => bill.IsHouseBill && bill.CU_HouseBill == shipment1.JS_HouseBill);
			var hb2 = declaration.Bills.Cast<Bill>().First(bill => bill.IsHouseBill && bill.CU_HouseBill == shipment2.JS_HouseBill);

			AssertEquals("Master Bill Issue Date Synchronization from Consol", consol.JK_MasterBillIssueDate, mb.CU_IssueDate);
			AssertEquals("House Bill Issue Date Synchronization from Shipment 0", shipment.JS_HouseBillIssueDate, hb.CU_IssueDate);
			AssertEquals("House Bill Issue Date Synchronization from Shipment 1", shipment1.JS_HouseBillIssueDate, hb1.CU_IssueDate);
			AssertEquals("House Bill Issue Date Synchronization from Shipment 2", shipment2.JS_HouseBillIssueDate, hb2.CU_IssueDate);
		}

		public void TestSynchroniseOnDataRefresh()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var factory1 = new BusinessObjectFactory();
				var shipment = factory1.New<ForwardingShipment>();
				shipment.JS_HouseBill = "HB01";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				var declaration = factory1.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_JS = shipment.PK;

				((Integration.Customs.IJobDeclarationWithShipmentSynchonisation)declaration).SynchroniseWithShipmentIfNeeded();
				factory1.Save();
				AssertEquals("Dec has House Bill but no Master Bill", 1, declaration.Bills.Count);

				var factory2 = new BusinessObjectFactory();
				var shipmentF2 = factory2.Load<ForwardingShipment>(shipment.PK);

				var domesticConsol = shipmentF2.Consols.AddNew();
				domesticConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				domesticConsol.JK_RL_NKLoadPort = "NZAKL"; // first 2 chars of load port must match declaration country code
				domesticConsol.JK_RL_NKDischargePort = "AUSYD";
				domesticConsol.JK_MasterBillNum = "MB01";
				factory2.Save();

				AssertEquals("Dec has House Bill and Master Bill (via Consols_CountChanged)", 2, declaration.Bills.Count);

				var declarationF2 = (BaseJobDeclaration)shipmentF2.Declarations[0];
				AssertEquals("Dec in F2 has House Bill but no Master Bill yet.", 1, declarationF2.Bills.Count);

				_ = (BaseJobDeclaration)shipmentF2.DeclarationForDocuments;
				AssertEquals("Dec in F2 has House Bill and Master Bill (via Synchroniser)", 2, declarationF2.Bills.Count);

				factory2.Save();
				AssertEquals("Dec in F2 has same House Bill and Master Bill", 2, declarationF2.Bills.Count);
				AssertEquals("Dec has same House Bill and Master Bill (after Data Refresh)", 2, declaration.Bills.Count);
			}
		}
	}
}
