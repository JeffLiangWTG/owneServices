using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(DMOBillMessageSendingObject))]
sealed class DMOBillMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When Bill is null", () => new DMOBillMessageSendingObject(null));
	}

	public void TestCustomsLevel()
	{
		CombineAssertions(() =>
		{
			var masterBillMSO = new DMOBillMessageSendingObject(masterBill);
			AssertEquals("When Bill Type is BOL", NODMOEDIMessageTypeList.Descriptions.MCS, masterBillMSO.CustomsLevel);

			var regularBillSTDMSO = new DMOBillMessageSendingObject(regularBill);
			AssertEquals("When Bill Type is STD", NODMOEDIMessageTypeList.Descriptions.HCS, regularBillSTDMSO.CustomsLevel);

			var masterBillAddress = Factory.New<OrgAddress>();
			masterBill.ABL_OA_Forwarder = masterBillAddress.PK;

			var cldBill = header.Bills.AddNew();
			cldBill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			var regularBillCLDMSO = new DMOBillMessageSendingObject(cldBill);
			AssertEquals("When Bill Type is CLD and forwarder is empty", NODMOEDIMessageTypeList.Descriptions.HCS, regularBillCLDMSO.CustomsLevel);

			header.MasterBill.ABL_OA_Forwarder = masterBillAddress.PK;
			cldBill.ABL_OA_Forwarder = masterBillAddress.PK;
			AssertEquals("When Bill Type is CLD and forwarder is equal to the MasterBill.Forwarder", NODMOEDIMessageTypeList.Descriptions.HCS, regularBillCLDMSO.CustomsLevel);

			var cldBillAddress = Factory.New<OrgAddress>();
			cldBill.ABL_OA_Forwarder = cldBillAddress.PK;
			AssertEquals("When Bill Type is CLD and forwarder is not equal to the MasterBill.Forwarder", NODMOEDIMessageTypeList.Descriptions.MCS, regularBillCLDMSO.CustomsLevel);
		});
	}

	public void TestBillNumber()
	{
		var masterBill = header.MasterBill;
		masterBill.ABL_BillNumber = "XYZ456";

		var masterBillMSO = new DMOBillMessageSendingObject(masterBill);
		AssertEquals("BillNumber is populated from MasterBill", "XYZ456", masterBillMSO.BillNumber);
	}

	public void TestRepresentative()
	{
		var address = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "TEST";
		address.OA_OH = orgHeader.PK;

		regularBill.ABL_OA_Forwarder = address.PK;
		var regularBillMSO = new DMOBillMessageSendingObject(regularBill);
		AssertEquals("Representative for Bill", "TEST", regularBillMSO.Representative);
	}

	public void TestConsignee()
	{
		var address = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "TEST";
		address.OA_OH = orgHeader.PK;

		regularBill.ABL_OA_Consignee = address.PK;
		var regularBillMSO = new DMOBillMessageSendingObject(regularBill);
		AssertEquals("Consignee for Bill", "TEST", regularBillMSO.Consignee);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new DMOBillMessageSendingObject(masterBill);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<AsycudaManifestHeader>();

		masterBill = header.Bills.AddNew();
		masterBill.ABL_BolType = AsycudaBill.ChildBolCode;

		regularBill = header.Bills.AddNew();
		regularBill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
	}

	AsycudaManifestHeader header;

	AsycudaBill masterBill;
	AsycudaBill regularBill;
}
