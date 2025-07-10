using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	public class ETailAirManifestDataObjectReaderHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestMarkUnprocessedExistingBillsFor_WhenRegistryEnabled_MarksHVLVBillsOnlyFromSameShipment()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentPK = ZGuid.NewZGuid();

				var mawb = (CusMAWB)Factory.BOFactory.New<Integration.Customs.NZ.ICusMAWB>();

				var hawb1_HVLV_ThisShipment = mawb.ChildBills.AddNew();
				hawb1_HVLV_ThisShipment.CS_HAWB = "HB1";
				hawb1_HVLV_ThisShipment.CS_IsHVLV = true;
				hawb1_HVLV_ThisShipment.CS_JS = shipmentPK;
				var hawb2_HVLV_OtherShipment = mawb.ChildBills.AddNew();
				hawb2_HVLV_OtherShipment.CS_HAWB = "HB2";
				hawb2_HVLV_OtherShipment.CS_IsHVLV = true;
				hawb2_HVLV_OtherShipment.CS_JS = ZGuid.NewZGuid();
				var hawb3_HVLV_ManuallyAdded = mawb.ChildBills.AddNew();
				hawb3_HVLV_ManuallyAdded.CS_HAWB = "HB3";
				hawb3_HVLV_ManuallyAdded.CS_IsHVLV = true;
				hawb3_HVLV_ManuallyAdded.CS_JS = ZGuid.Empty;

				var hawb4_STD_OtherShipment = mawb.ChildBills.AddNew();
				hawb4_STD_OtherShipment.CS_HAWB = "HB4";
				hawb4_STD_OtherShipment.CS_JS = ZGuid.NewZGuid();
				var hawb5_STD_ManuallyAdded = mawb.ChildBills.AddNew();
				hawb5_STD_ManuallyAdded.CS_HAWB = "HB5";
				hawb5_STD_ManuallyAdded.CS_JS = ZGuid.Empty;

				var helper = new ETailAirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.NewZealand, shipmentPK);
				helper.MarkUnprocessedExistingBillsFor(mawb);
				helper.DeleteUnprocessedBillsFor(mawb, new TestErrorLogger());

				CombineAssertions(() =>
				{
					Assert("Hawb1 is deleted as it is HVLV and on this Shipment", hawb1_HVLV_ThisShipment.IsDeleted);
					Assert("Hawb2 is not deleted as it HVLV and on a different Shipment", !hawb2_HVLV_OtherShipment.IsDeleted);
					Assert("Hawb3 is deleted as it is not on any shipment (manually added)", hawb3_HVLV_ManuallyAdded.IsDeleted);
					Assert("Hawb4 is not deleted as it is Not HVLV but is attached to a shipment", !hawb4_STD_OtherShipment.IsDeleted);
					Assert("Hawb5 is deleted as it is Not attached to any shipment (manually added)", hawb5_STD_ManuallyAdded.IsDeleted);
				});
			}
		}

		public void TestMarkUnprocessedExistingBillsFor_WhenRegistryDisabled_MarksAllHVLVBills()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipmentPK = ZGuid.NewZGuid();

				var mawb = (CusMAWB)Factory.BOFactory.New<Integration.Customs.NZ.ICusMAWB>();

				var hawb1_HVLV_ThisShipment = mawb.ChildBills.AddNew();
				hawb1_HVLV_ThisShipment.CS_HAWB = "HB1";
				hawb1_HVLV_ThisShipment.CS_IsHVLV = true;
				hawb1_HVLV_ThisShipment.CS_JS = shipmentPK;
				var hawb2_HVLV_OtherShipment = mawb.ChildBills.AddNew();
				hawb2_HVLV_OtherShipment.CS_HAWB = "HB2";
				hawb2_HVLV_OtherShipment.CS_IsHVLV = true;
				hawb2_HVLV_OtherShipment.CS_JS = ZGuid.NewZGuid();
				var hawb3_HVLV_ManuallyAdded = mawb.ChildBills.AddNew();
				hawb3_HVLV_ManuallyAdded.CS_HAWB = "HB3";
				hawb3_HVLV_ManuallyAdded.CS_IsHVLV = true;
				hawb3_HVLV_ManuallyAdded.CS_JS = ZGuid.Empty;

				var hawb4_STD_OtherShipment = mawb.ChildBills.AddNew();
				hawb4_STD_OtherShipment.CS_HAWB = "HB4";
				hawb4_STD_OtherShipment.CS_JS = ZGuid.NewZGuid();
				var hawb5_STD_ManuallyAdded = mawb.ChildBills.AddNew();
				hawb5_STD_ManuallyAdded.CS_HAWB = "HB5";
				hawb5_STD_ManuallyAdded.CS_JS = ZGuid.Empty;

				var helper = new ETailAirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.NewZealand, shipmentPK);
				helper.MarkUnprocessedExistingBillsFor(mawb);
				helper.DeleteUnprocessedBillsFor(mawb, new TestErrorLogger());

				CombineAssertions(() =>
				{
					Assert("Hawb1 is deleted as it is HVLV", hawb1_HVLV_ThisShipment.IsDeleted);
					Assert("Hawb2 is deleted as it is HVLV", hawb2_HVLV_OtherShipment.IsDeleted);
					Assert("Hawb3 is deleted as is not on any shipment (manually added)", hawb3_HVLV_ManuallyAdded.IsDeleted);
					Assert("Hawb4 is not deleted as it is Not HVLV but is attached to a shipment", !hawb4_STD_OtherShipment.IsDeleted);
					Assert("Hawb5 is deleted as it is Not attached to any shipment (manually added)", hawb5_STD_ManuallyAdded.IsDeleted);
				});
			}
		}
	}
}
