using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(TransferHeaderMessageChooserItem))]
	sealed class ArrivalMessageChooserItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
			return chooser.ChooserItems[0];
		}

		public void TestProperties()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var inBondCarrier = Factory.New<OrgHeader>();
			inBondCarrier.OH_Code = "IBCA";
			var inBondCarrierAddress = inBondCarrier.MainAddress;
			inBondCarrierAddress.Address1 = "Address1";
			var destinationWarehouseOrg = Factory.New<OrgHeader>();
			destinationWarehouseOrg.OH_Code = "DWHO";
			var destinationWarehouseAddress = destinationWarehouseOrg.MainAddress;
			destinationWarehouseAddress.Address1 = "Address1";
			arrivalHeader.ATH_VoyageFlightNo = "FN01";
			arrivalHeader.ATH_Reference = "A";
			arrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2021, 11, 29);
			(transferHeader as IBusinessObjectInternals).IsCopying = true;
			transferHeader.ATF_RL_NKDestinationPortCode = "AUSYD";
			transferHeader.ATF_TransferType = "R";
			transferHeader.ATF_OA_Carrier = inBondCarrierAddress.PK;
			transferHeader.ATF_CarrierID = "AAA";
			transferHeader.ATF_OnwardCarrier = "ONC1";
			transferHeader.ATF_OA_DestinationWarehouse = destinationWarehouseAddress.PK;
			transferHeader.ATF_DestinationWarehouseID = "DDD";
			(transferHeader as IBusinessObjectInternals).IsCopying = false;
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
			var transferHeaderMessageChooserItem = chooser.ChooserItems[0];
			transferHeaderMessageChooserItem.StatusCode = "7";
			AssertEquals("VoyageFlightNo", "FN01", transferHeaderMessageChooserItem.VoyageFlightNo);
			AssertEquals("ArrivalReference", "A", transferHeaderMessageChooserItem.ArrivalReference);
			AssertEquals("ETAAtDischargePort", new ZDateTime(2021, 11, 29), transferHeaderMessageChooserItem.ETAAtDischargePort);
			AssertEquals("DestinationPort", "AUSYD", transferHeaderMessageChooserItem.DestinationPort);
			AssertEquals("TransferType", "R", transferHeaderMessageChooserItem.TransferType);
			AssertEquals("InBondCarrier", "IBCA", transferHeaderMessageChooserItem.InBondCarrier);
			AssertEquals("InBondCarrierID", "AAA", transferHeaderMessageChooserItem.InBondCarrierID);
			AssertEquals("OnwardCarrier", "ONC1", transferHeaderMessageChooserItem.OnwardCarrier);
			AssertEquals("BondedPremises", "DWHO", transferHeaderMessageChooserItem.BondedPremises);
			AssertEquals("BondedPremisesID", "DDD", transferHeaderMessageChooserItem.BondedPremisesID);
			AssertEquals("StatusCode", "7", transferHeaderMessageChooserItem.StatusCode);
		}

		public void TestLookup()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
			var transferHeaderMessageChooserItem = chooser.ChooserItems[0];
			AssertType("ArrivalStatusList is AIMArrivalStatusCodes", typeof(AIMArrivalStatusCodes), transferHeaderMessageChooserItem.ArrivalStatusList);
		}
	}
}
