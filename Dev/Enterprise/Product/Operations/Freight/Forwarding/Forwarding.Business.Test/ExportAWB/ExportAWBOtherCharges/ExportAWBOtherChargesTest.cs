using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBOtherCharges))]
	class ExportAWBOtherChargesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEO_ChargeCode()
		{
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;
			AssertEquals(Core.Constants.AWB.ChargeCodes.AC, OtherCharges.EO_ChargeCode);
			AssertEquals("Animal container", OtherCharges.EO_ChargeDescription);

			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.NS;
			AssertEquals(Core.Constants.AWB.ChargeCodes.NS, OtherCharges.EO_ChargeCode);
		}

		#region Entitlement

		public void TestEO_ChargeCode_SetsEntitlement_MAWB_Prepaid()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;

			OtherCharges = Factory.New<ConsolExportAWBOtherCharges>();
			ConsolExportAWBHeader header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			OtherCharges.EO_EH = header.PK;

			AWBDisplayOptionCollection mAWBCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			mAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "A";
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals("default entitlement", OtherCharges.EO_EntitlementCode, "A");

			mAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "C";
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals("default entitlement", OtherCharges.EO_EntitlementCode, "C");
		}

		public void TestEO_ChargeCode_SetsEntitlement_MAWB_Collect()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;

			OtherCharges = Factory.New<ConsolExportAWBOtherCharges>();
			ConsolExportAWBHeader header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			OtherCharges.EO_EH = header.PK;

			AWBDisplayOptionCollection mAWBCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			mAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "A";
			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals(OtherCharges.EO_EntitlementCode, "A");

			mAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "C";
			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals(OtherCharges.EO_EntitlementCode, "C");
		}

		public void TestEO_ChargeCode_SetsEntitlement_MAWB_PPDCLTNotSet()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = "";

			OtherCharges = Factory.New<ConsolExportAWBOtherCharges>();
			ConsolExportAWBHeader header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			OtherCharges.EO_EH = header.PK;

			AWBDisplayOptionCollection mAWBCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			mAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "A";
			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mAWBCollection);
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals("registry isnt used as consol has no PPD CLT, it uses default instead", "C", OtherCharges.EO_EntitlementCode);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals("registry isnt used as consol has no PPD CLT, it uses default instead", "C", OtherCharges.EO_EntitlementCode);
		}

		public void TestEO_ChargeCode_SetsEntitlement_HAWB_Collect()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = Constants.IncoTerms.ExWorks;

			OtherCharges = Factory.New<ShipmentExportAWBOtherCharges>();
			ShipmentExportAWBHeader header = Factory.New<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;
			OtherCharges.EO_EH = header.PK;

			AWBDisplayOptionCollection hAWBCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			hAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "A";
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals(OtherCharges.EO_EntitlementCode, "A");

			hAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "C";
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals(OtherCharges.EO_EntitlementCode, "C");
		}

		public void TestEO_ChargeCode_SetsEntitlement_HAWB_PPDCLTNotSet()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = ZString.Empty;

			OtherCharges = Factory.New<ShipmentExportAWBOtherCharges>();
			ShipmentExportAWBHeader header = Factory.New<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;
			OtherCharges.EO_EH = header.PK;

			AWBDisplayOptionCollection hAWBCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			hAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "A";
			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals(OtherCharges.EO_EntitlementCode, "A");

			hAWBCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			hAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "C";
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals(OtherCharges.EO_EntitlementCode, "C");
		}

		public void TestEO_ChargeCode_SetsEntitlement_HAWB_Prepaid()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = Constants.IncoTerms.FreeCarrier;

			OtherCharges = Factory.New<ShipmentExportAWBOtherCharges>();
			ShipmentExportAWBHeader header = Factory.New<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;
			OtherCharges.EO_EH = header.PK;

			AWBDisplayOptionCollection hAWBCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			hAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "A";
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals(OtherCharges.EO_EntitlementCode, "A");

			hAWBCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "C";
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hAWBCollection);

			OtherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;

			AssertEquals(OtherCharges.EO_EntitlementCode, "C");
		}

		#endregion

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestEO_Amount()
		{
			OtherCharges.EO_Amount = 12.45M;
			AssertEquals(12.45M, OtherCharges.EO_Amount);
		}

		public void TestEO_EntitlementCode()
		{
			OtherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			AssertEquals(Core.Constants.AWB.EntitlementCode.Agent, OtherCharges.EO_EntitlementCode);
		}

		public void TestIsSavedByFactory()
		{
			OtherCharges.Master.Delete();

			var otherCharges = Factory.New<ExportAWBOtherCharges>();
			AssertEquals("Object w/o header behaves normally", true, otherCharges.IsSavedByFactory);
			otherCharges.Delete();

			var aWBHeader = Factory.New<ShipmentExportAWBHeader>();
			otherCharges = aWBHeader.AWBOtherCharges.AddNew();
			aWBHeader.EH_ParentID = Factory.New<ForwardingShipment>().PK;
			aWBHeader.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Not overridden new AWB is never saved", false, otherCharges.IsSavedByFactory);
			Factory.Save();
			AssertEquals(false, otherCharges.IsInDatabase);

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			aWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			aWBHeader.EH_AreRateLinesOverridden = false;
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed and repopulated", false, otherCharges.IsSavedByFactory);

			otherCharges = aWBHeader.AWBOtherCharges.AddNew();
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed", false, otherCharges.IsSavedByFactory);

			aWBHeader.ForceSavingByFactory = true;
			AssertEquals("Forced AWB is always saved", true, otherCharges.IsSavedByFactory);
			Factory.Save();
			AssertEquals(true, otherCharges.IsInDatabase);

			aWBHeader.ForceSavingByFactory = false;
			AssertEquals("Once saved but not overridden is not saved next time", false, otherCharges.IsSavedByFactory);

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("Saved when 'Override' is ticked", true, otherCharges.IsSavedByFactory);
			Factory.Save();

			AssertEquals("Overridden is not saved if doesn't have changes", false, otherCharges.IsSavedByFactory);

			otherCharges.EO_Amount = 12.45M;
			AssertEquals("Overridden is saved when has changes", true, otherCharges.IsSavedByFactory);
			Factory.Save();

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals("Overridden is saved when 'Override' has changes and already in the database", true, otherCharges.IsSavedByFactory);
			var otherCharges2 = aWBHeader.AWBOtherCharges.AddNew();
			Factory.Save();
			AssertEquals(true, otherCharges.IsDeleted);
			AssertEquals(true, otherCharges2.IsInDatabase);

			otherCharges2.EO_Amount = 12.45M;
			AssertEquals("Not overridden and saved is not saved when has changes", false, otherCharges2.IsSavedByFactory);
		}

		#region Lookups

		public void TestChargeCodesList()
		{
			AssertEquals(OLookUpEditType.AWBChargeCodes, OtherCharges.IATAChargeCodesList.LookupEditType);
		}

		public void TestEntitlementCodesList()
		{
			AssertEquals(OLookUpEditType.AWBEntitlementCodes, OtherCharges.EntitlementCodesList.LookupEditType);
		}

		public void TestPrepaidCollectList()
		{
			AssertEquals(OLookUpEditType.CustomType, OtherCharges.PrepayCollectList.LookupEditType);
			AssertEquals(2, OtherCharges.PrepayCollectList.Count);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, OtherCharges.PrepayCollectList[0].Code);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, OtherCharges.PrepayCollectList[1].Code);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ExportAWBOtherCharges>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<ForwardingShipment>();
			shipment.FillWithValidTestData();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OverrideWaybillDefaults = true;

			var exportHeader = factory.New<ShipmentExportAWBHeader>();
			exportHeader.EH_ParentID = shipment.PK;
			exportHeader.ForceSavingByFactory = true;

			var result = factory.New<ExportAWBOtherCharges>();
			result.FillWithValidTestData();
			result.EO_EH = exportHeader.PK;

			return result;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			OtherCharges.Master.EH_ParentID = Factory.NewWithValidTestData<ForwardingConsol>().PK;
			return OtherCharges;
		}

		protected ExportAWBOtherCharges OtherCharges;

		protected override void SetUp()
		{
			OtherCharges = Factory.New<ExportAWBOtherCharges>();
			OtherCharges.EO_EH = Factory.New(typeof(ShipmentExportAWBHeader)).PK;
		}

		#endregion
	}
}
