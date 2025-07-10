using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	[TestedType(typeof(AsycudaBillSynchroniser))]
	sealed class AsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestUCRNumbersSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var cusEntryInstruction = (ZA.Business.CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_IsUCROverridden = true;
			cusEntryInstruction.CEI_UCROverride = "123456789";
			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = "ZA";
			AssertEquals("123456789", manifestBill.ABL_UCRNumber);
		}

		public void TestLRNNumberSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "L1Test";
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader2.CH_BGMReference = "L2Test";
			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = "ZA";
			AssertEquals("L1Test", manifestBill.CustomsEntryNumbers[0].CE_EntryNum);
			AssertEquals(ZaLRNTypes.Codes.AFM, manifestBill.CustomsEntryNumbers[0].CE_EntryType);
			AssertEquals("L2Test", manifestBill.CustomsEntryNumbers[1].CE_EntryNum);
			AssertEquals(ZaLRNTypes.Codes.AFM, manifestBill.CustomsEntryNumbers[1].CE_EntryType);

			manifestBill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals(ZaLRNTypes.Codes.ABT, manifestBill.CustomsEntryNumbers[0].CE_EntryType);
			AssertEquals(ZaLRNTypes.Codes.ABT, manifestBill.CustomsEntryNumbers[1].CE_EntryType);
		}

		public void TestSynchroniseManifestUQ()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "UT";
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "NO";
			pack1.RP_Type = "GMB";
			pack1.RP_CustomsCountry = "AU";
			pack1.RP_ConversionFactor = 10;
			pack1.RP_CommercialPack = "DIZ";
			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = "ZA";
			AssertEquals("UT", manifestBill.ABL_ManifestUQ);
			AssertEquals(10, manifestBill.ABL_ManifestQty);
			shipment.JS_F3_NKPackType = "DIZ";
			AssertEquals("DIZ", manifestBill.ABL_ManifestUQ);
			AssertEquals(10, manifestBill.ABL_ManifestQty);
		}

		public void TestManifestBillForZAShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
		}

		public void TestManifestBillTypeNonSTD()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.ShippersConsolLead;
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(Core.Constants.ShipmentTypes.CoLoadMaster, manifestBill.ABL_BolType);
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertEquals(Core.Constants.ShipmentTypes.StandardHouse, manifestBill.ABL_BolType);
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse;
			AssertEquals(Core.Constants.ShipmentTypes.CoLoadMaster, manifestBill.ABL_BolType);
		}

		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;
	}
}
