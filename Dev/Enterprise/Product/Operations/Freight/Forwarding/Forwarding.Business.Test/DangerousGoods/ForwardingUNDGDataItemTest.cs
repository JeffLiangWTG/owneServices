using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingUNDGDataItem))]
	sealed class ForwardingUNDGDataItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsNotOtherwiseSpecified()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var dataItem = packLine.UNDGs.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "0190";
			substance.DG_Code = "0190";
			substance.DG_PSN = "Samples, explosive";
			dataItem.DI_DG = substance.PK;

			Assert("DI_IsNotOtherwiseSpecifiedInfo should be read only while DG substance has not n.o.s. Allowed ticked ", dataItem.DI_IsNotOtherwiseSpecifiedInfo.ReadOnly);

			substance.DG_IsNotOtherwiseSpecified = true;
			Assert("DI_IsNotOtherwiseSpecifiedInfo should not be read only while DG substance has  n.o.s. Allowed ticked ", !dataItem.DI_IsNotOtherwiseSpecifiedInfo.ReadOnly);
		}

		public void TestParentPackLine()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			var collection = new ForwardingUNDGDataItemCollection(packLine);

			var dataItem = collection.FirstItemForBinding.FirstOrDefault() as ForwardingUNDGDataItem;
			AssertNotNull(dataItem);
			AssertEquals(ZGuid.Empty, dataItem.DI_ParentID);
			AssertEquals(packLine, dataItem.ParentPackLine);

			dataItem = collection.AddNew();
			AssertEquals(packLine.PK, dataItem.DI_ParentID);
			AssertEquals(packLine, dataItem.ParentPackLine);
		}

		public void TestIsPSAGroupApplicable_Origin()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var packLine = shipment.OuterPackLines.AddNew();

			var item1 = packLine.UNDGs.AddNew();
			Assert(!item1.IsPSAGroupApplicable);

			shipment.JS_RL_NKOrigin = "SGAYC";

			var item2 = packLine.UNDGs.AddNew();
			Assert("PSA Group is applicable when originating in SG", item2.IsPSAGroupApplicable);
		}

		public void TestIsPSAGroupApplicable_Destination()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var packLine = shipment.OuterPackLines.AddNew();

			var item1 = packLine.UNDGs.AddNew();
			Assert(!item1.IsPSAGroupApplicable);

			shipment.JS_RL_NKDestination = "SGAYC";

			var item2 = packLine.UNDGs.AddNew();
			Assert("PSA Group is applicable when discharging in SG", item2.IsPSAGroupApplicable);
		}

		public void TestDI_DGFlashPoint_ReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "TTT";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var item = Factory.New<ForwardingUNDGDataItem>();

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item.LinkDefault(subs);
				item.Substance.DG_FlashPoint = "AAA";
				AssertEquals("should be readonly DI_IsCombustible is false", true, item.DI_DGFlashPointInfo.ReadOnly);

				item.DI_IsCombustible = true;
				AssertEquals("should not be readonly when DI_IsCombustible is true", false, item.DI_DGFlashPointInfo.ReadOnly);

				var iataSub = Factory.New<UNDGSubstance>();
				iataSub.DG_UNNO = "IATA";
				iataSub.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				item.DI_DG = iataSub.PK;
				AssertEquals("should not be readonly when have a substance regardless of the substance standard type and DI_IsCombustible is true", false, item.DI_DGFlashPointInfo.ReadOnly);

				item.Substance.DG_FlashPoint = ZString.Empty;
				AssertEquals("should not be readonly when the Flash Point of substance is empty and DI_IsCombustible is true", false, item.DI_DGFlashPointInfo.ReadOnly);
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				item = Factory.New<ForwardingUNDGDataItem>();
				AssertEquals("should be readonly when no substance attached", true, item.DI_DGFlashPointInfo.ReadOnly);

				item.LinkDefault(subs);
				AssertEquals("should not be readonly when substance attached", false, item.DI_DGFlashPointInfo.ReadOnly);
			}
		}

		public void TestWhenParentShipmentModeChangesSubstancesGetRelinked()
		{
			var imoSubstance = Factory.New<UNDGSubstance>();
			imoSubstance.DG_UNNO = "9999";
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_UNNO = "9999";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			undgDataItem.DI_DG = imoSubstance.PK;
			undgDataItem.LinkDefault(imoSubstance);

			Factory.Save();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;

			AssertEquals("substance should have been relinked", undgDataItem.DI_DG, ridSubstance.PK);
			AssertEquals("substance should have been relinked", undgDataItem.Substance.StandardSubstance as UNDGSubstanceRID, ridSubstance);
		}

		public void TestWhenParentShipmentModeChangesSubstancesDontRelinkWhenVariantExists()
		{
			var imoSubstance = Factory.New<UNDGSubstance>();
			imoSubstance.DG_UNNO = "8888";
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var ridSubstanceA = Factory.New<UNDGSubstanceRID>();
			ridSubstanceA.RID_UNNO = "8888";
			ridSubstanceA.RID_Variant = "A";

			var ridSubstanceB = Factory.New<UNDGSubstanceRID>();
			ridSubstanceB.RID_UNNO = "8888";
			ridSubstanceB.RID_Variant = "B";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			undgDataItem.DI_DG = imoSubstance.PK;
			undgDataItem.LinkDefault(imoSubstance);

			Factory.Save();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;

			AssertEquals("substance should not have been relinked", undgDataItem.DI_DG, imoSubstance.PK);
			AssertEquals("substance should not have been relinked", undgDataItem.Substance, imoSubstance);
		}

		public void TestDI_RadioactiveLabelCategoryWhenHRCQSetTrue()
		{
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_RadioactiveLabelCategory = ZString.Empty;
			undgDataItem.DI_IsHighwayRouteControlledQuantity = ZBool.False;

			AssertNullOrEmpty("Precondition.", undgDataItem.DI_RadioactiveLabelCategory);
			Assert("Precondition.", !undgDataItem.DI_IsHighwayRouteControlledQuantity);

			undgDataItem.DI_IsHighwayRouteControlledQuantity = ZBool.True;
			Assert(undgDataItem.DI_IsHighwayRouteControlledQuantity);
			AssertEquals(RadioactiveLabelCategoryList.Codes.YellowIII, undgDataItem.DI_RadioactiveLabelCategory);
		}

		#region ICanDelete

		public void TestCanDelete()
		{
			var errorMessage = "You do not have the security rights to add or remove Class 7 (Radioactive) materials on a Shipment.";
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "123";

			var substance_Class7 = Factory.New<UNDGSubstance>();
			substance_Class7.DG_UNNO = "234";

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_DG = substance.PK;
			dataItem.LinkDefault(substance);

			var dataItem_Class7 = packline.UNDGs.AddNew();
			dataItem_Class7.DI_DG = substance_Class7.PK;
			dataItem_Class7.DI_IMOClass = "7";
			dataItem_Class7.LinkDefault(substance_Class7);

			Factory.Save();

			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = false;

			CombineAssertions(() =>
			{
				Assert("Should be able to delete substance without class 7", dataItem.CanDelete);
				AssertNullOrEmpty(dataItem.ReasonForNotAbleToDelete);

				Assert("Should NOT be able to delete substance with class 7", !dataItem_Class7.CanDelete);
				AssertEquals(errorMessage, dataItem_Class7.ReasonForNotAbleToDelete);
			});

			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = true;

			CombineAssertions("Should all be deletable because security right is given", () =>
			{
				Assert(dataItem.CanDelete);
				Assert(dataItem_Class7.CanDelete);

				AssertNullOrEmpty(dataItem.ReasonForNotAbleToDelete);
				AssertNullOrEmpty(dataItem_Class7.ReasonForNotAbleToDelete);
			});
		}

		public void TestCanDelete_NonDatabaseItems()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();

			var substance_Class7 = Factory.New<UNDGSubstance>();
			substance_Class7.DG_UNNO = "234";

			var dataItem_Class7 = packline.UNDGs.AddNew();
			dataItem_Class7.DI_DG = substance_Class7.PK;
			dataItem_Class7.DI_IMOClass = "7";
			dataItem_Class7.LinkDefault(substance_Class7);

			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = false;

			Assert("Pre-Condition - Not in database", !dataItem_Class7.IsInDatabase);
			Assert("Should be able to delete even if no security right as it's not saved yet", dataItem_Class7.CanDelete);
		}

		#endregion

		public void TestParentHVLVItem()
		{
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_ParentTableCode = HVLVItemSchema.Constants.Prefix;

			var hvlvItem = Factory.New<IHVLVItem>();
			undgDataItem.DI_ParentID = hvlvItem.PK;

			AssertNotNull(undgDataItem.ParentHVLVItem);
			AssertEquals(hvlvItem, undgDataItem.ParentHVLVItem);
		}

		public void TestLabelCategory_Readonly()
		{
			var nonClass7Substance = Factory.New<UNDGSubstanceCFR>();
			nonClass7Substance.CFR_UNNO = "1234";
			nonClass7Substance.CFR_PrimaryClass = "1";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = nonClass7Substance.PK;
			undgDataItem.LinkDefault(nonClass7Substance);

			Factory.Save();

			AssertEquals("Non Class 7 so should be read only", true, undgDataItem.DI_RadioactiveLabelCategoryInfo.ReadOnly);

			var class7Substance = Factory.New<UNDGSubstanceCFR>();
			class7Substance.CFR_UNNO = "2345";
			class7Substance.CFR_PrimaryClass = "7";

			var editableUNDGDataItem = Factory.New<ForwardingUNDGDataItem>();
			editableUNDGDataItem.DI_DG = class7Substance.PK;
			editableUNDGDataItem.LinkDefault(class7Substance);

			Factory.Save();

			AssertEquals("Class 7 so should be editable", false, editableUNDGDataItem.DI_RadioactiveLabelCategoryInfo.ReadOnly);
		}

		public void TestDI_RadionuclideElement_ReadOnly()
		{
			var nonClass7Substance = Factory.New<UNDGSubstanceCFR>();
			nonClass7Substance.CFR_UNNO = "1234";
			nonClass7Substance.CFR_PrimaryClass = "1";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = nonClass7Substance.PK;
			undgDataItem.LinkDefault(nonClass7Substance);

			Factory.Save();

			AssertEquals("Non Class 7 so should be read only", true, undgDataItem.DI_RadionuclideElementInfo.ReadOnly);

			var class7Substance = Factory.New<UNDGSubstanceCFR>();
			class7Substance.CFR_UNNO = "2345";
			class7Substance.CFR_PrimaryClass = "7";

			var editableUNDGDataItem = Factory.New<ForwardingUNDGDataItem>();
			editableUNDGDataItem.DI_DG = class7Substance.PK;
			editableUNDGDataItem.LinkDefault(class7Substance);

			Factory.Save();

			AssertEquals("Class 7 so should be editable", false, editableUNDGDataItem.DI_RadionuclideElementInfo.ReadOnly);
		}

		public void DI_RadionuclideElementSuffix_ReadOnly()
		{
			var nonClass7Substance = Factory.New<UNDGSubstanceCFR>();
			nonClass7Substance.CFR_UNNO = "1234";
			nonClass7Substance.CFR_PrimaryClass = "1";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = nonClass7Substance.PK;
			undgDataItem.LinkDefault(nonClass7Substance);

			Factory.Save();

			AssertEquals("Non Class 7 so should be read only", true, undgDataItem.DI_RadionuclideElementSuffixInfo.ReadOnly);

			var class7Substance = Factory.New<UNDGSubstanceCFR>();
			class7Substance.CFR_UNNO = "2345";
			class7Substance.CFR_PrimaryClass = "7";

			var editableUNDGDataItem = Factory.New<ForwardingUNDGDataItem>();
			editableUNDGDataItem.DI_DG = class7Substance.PK;
			editableUNDGDataItem.LinkDefault(class7Substance);

			Factory.Save();

			AssertEquals("Class 7 so should be editable", false, editableUNDGDataItem.DI_RadionuclideElementSuffixInfo.ReadOnly);
		}

		public void DI_RadioactiveMaximumActivity_ReadOnly()
		{
			var nonClass7Substance = Factory.New<UNDGSubstanceCFR>();
			nonClass7Substance.CFR_UNNO = "1234";
			nonClass7Substance.CFR_PrimaryClass = "1";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = nonClass7Substance.PK;
			undgDataItem.LinkDefault(nonClass7Substance);

			Factory.Save();

			AssertEquals("Non Class 7 so should be read only", true, undgDataItem.DI_RadioactiveMaximumActivityInfo.ReadOnly);

			var class7Substance = Factory.New<UNDGSubstanceCFR>();
			class7Substance.CFR_UNNO = "2345";
			class7Substance.CFR_PrimaryClass = "7";

			var editableUNDGDataItem = Factory.New<ForwardingUNDGDataItem>();
			editableUNDGDataItem.DI_DG = class7Substance.PK;
			editableUNDGDataItem.LinkDefault(class7Substance);

			Factory.Save();

			AssertEquals("Class 7 so should be editable", false, editableUNDGDataItem.DI_RadioactiveMaximumActivityInfo.ReadOnly);
		}

		public void DI_RadioactiveMaximumActivityUnit_ReadOnly()
		{
			var nonClass7Substance = Factory.New<UNDGSubstanceCFR>();
			nonClass7Substance.CFR_UNNO = "1234";
			nonClass7Substance.CFR_PrimaryClass = "1";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = nonClass7Substance.PK;
			undgDataItem.LinkDefault(nonClass7Substance);

			Factory.Save();

			AssertEquals("Non Class 7 so should be read only", true, undgDataItem.DI_RadioactiveMaximumActivityUnitInfo.ReadOnly);

			var class7Substance = Factory.New<UNDGSubstanceCFR>();
			class7Substance.CFR_UNNO = "2345";
			class7Substance.CFR_PrimaryClass = "7";

			var editableUNDGDataItem = Factory.New<ForwardingUNDGDataItem>();
			editableUNDGDataItem.DI_DG = class7Substance.PK;
			editableUNDGDataItem.LinkDefault(class7Substance);

			Factory.Save();

			AssertEquals("Class 7 so should be editable", false, editableUNDGDataItem.DI_RadioactiveMaximumActivityUnitInfo.ReadOnly);
		}

		public void TestRadioactiveTransportIndex_ReadOnly()
		{
			var nonCFRSubstance = Factory.New<UNDGSubstanceADN>();
			nonCFRSubstance.ADN_UNNO = "6969";

			var nonClass7Substance = Factory.New<UNDGSubstanceCFR>();
			nonClass7Substance.CFR_UNNO = "1234";
			nonClass7Substance.CFR_PrimaryClass = "1";

			var class7CFRSubstance = Factory.New<UNDGSubstanceCFR>();
			class7CFRSubstance.CFR_UNNO = "420";
			class7CFRSubstance.CFR_PrimaryClass = "7";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = nonCFRSubstance.PK;
			undgDataItem.LinkDefault(nonCFRSubstance);
			Factory.Save();

			Assert("Non CFR substance, so should be read only.", undgDataItem.DI_RadioactiveTransportIndexInfo.ReadOnly);

			undgDataItem.DI_DG = nonClass7Substance.PK;
			undgDataItem.LinkDefault(nonClass7Substance);
			Factory.Save();
			Assert("Non Class 7 substance, so should be read only.", undgDataItem.DI_RadioactiveTransportIndexInfo.ReadOnly);

			undgDataItem.DI_DG = class7CFRSubstance.PK;
			undgDataItem.LinkDefault(class7CFRSubstance);
			Factory.Save();
			AssertEquals("Class 7 CFR substance, so should not be read only.", false, undgDataItem.DI_RadioactiveTransportIndexInfo.ReadOnly);
		}

		public void TestClear49CFRRadioactiveFieldsIfNecessary()
		{
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_RadioactiveLabelCategory = RadioactiveLabelCategoryList.Codes.YellowII;
			undgDataItem.DI_IsHighwayRouteControlledQuantity = true;
			undgDataItem.DI_RadioactiveTransportIndex = 69.69;
			undgDataItem.DI_MaterialFormDescription = "bleh";

			AssertNotNullOrEmpty("Precondition.", undgDataItem.DI_RadioactiveLabelCategory);
			Assert("Precondition.", undgDataItem.DI_IsHighwayRouteControlledQuantity);
			AssertEquals("Precondition.", new ZDecimal(69.69), undgDataItem.DI_RadioactiveTransportIndex);
			AssertNotNullOrEmpty("Precondition.", undgDataItem.DI_MaterialFormDescription);

			undgDataItem.DI_DG = ZGuid.BrettsGuid;
			AssertNullOrEmpty(undgDataItem.DI_RadioactiveLabelCategory);
			Assert(!undgDataItem.DI_IsHighwayRouteControlledQuantity);
			AssertEquals(ZDecimal.Zero, undgDataItem.DI_RadioactiveTransportIndex);
			AssertNullOrEmpty(undgDataItem.DI_MaterialFormDescription);
		}

		public void TestDI_MaterialFormDescription_ReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "TTT";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			subs.DG_Class = RadioactiveConstants.RadioactiveClass;
			subs.DG_PSN = "contains special form";

			var subsWithNoSpecialForm = Factory.New<UNDGSubstance>();
			subsWithNoSpecialForm.DG_UNNO = "VVV";
			subsWithNoSpecialForm.DG_Variant = "";
			subsWithNoSpecialForm.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			subsWithNoSpecialForm.DG_Class = RadioactiveConstants.RadioactiveClass;
			subsWithNoSpecialForm.DG_PSN = "random psn";

			var nonRadioactiveSubs = Factory.New<UNDGSubstance>();
			nonRadioactiveSubs.DG_UNNO = "UUU";
			nonRadioactiveSubs.DG_Variant = "";
			nonRadioactiveSubs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			nonRadioactiveSubs.DG_Class = "1";
			nonRadioactiveSubs.DG_PSN = "random psn";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			AssertEquals("Material Form Description readonly expected to be true", undgDataItem.DI_MaterialFormDescriptionInfo.ReadOnly, true);

			undgDataItem.DI_DG = subs.PK;
			AssertEquals("Material Form Description readonly expected to be true", undgDataItem.DI_MaterialFormDescriptionInfo.ReadOnly, true);

			undgDataItem.DI_DG = subsWithNoSpecialForm.PK;
			AssertEquals("Material Form Description readonly expected to be false", undgDataItem.DI_MaterialFormDescriptionInfo.ReadOnly, false);

			undgDataItem.DI_DG = nonRadioactiveSubs.PK;
			AssertEquals("Material Form Description readonly expected to be true", undgDataItem.DI_MaterialFormDescriptionInfo.ReadOnly, true);
		}

		public void TestDI_IsHighwayRouteControlledQuantity_ReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "TTT";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			subs.DG_Class = RadioactiveConstants.RadioactiveClass;
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();

			AssertEquals("HRCQ readonly expected to be true", undgDataItem.DI_IsHighwayRouteControlledQuantityInfo.ReadOnly, true);
			undgDataItem.DI_DG = subs.PK;

			AssertEquals("HRCQ readonly expected to be false", undgDataItem.DI_IsHighwayRouteControlledQuantityInfo.ReadOnly, false);

			subs.DG_Class = "1";

			AssertEquals("HRCQ readonly expected to be true", undgDataItem.DI_IsHighwayRouteControlledQuantityInfo.ReadOnly, true);
		}

		public void TestDI_IsFissileExcepted_ReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "TTT";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			subs.DG_Class = RadioactiveConstants.RadioactiveClass;
			subs.DG_PSN = "contains fissile";

			var nonFissileSubs = Factory.New<UNDGSubstance>();
			nonFissileSubs.DG_UNNO = "VVV";
			nonFissileSubs.DG_Variant = "";
			nonFissileSubs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			nonFissileSubs.DG_Class = RadioactiveConstants.RadioactiveClass;
			nonFissileSubs.DG_PSN = "random psn";

			var nonRadioactiveSubs = Factory.New<UNDGSubstance>();
			nonRadioactiveSubs.DG_UNNO = "UUU";
			nonRadioactiveSubs.DG_Variant = "";
			nonRadioactiveSubs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			nonRadioactiveSubs.DG_Class = "1";
			nonRadioactiveSubs.DG_PSN = "random psn";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			AssertEquals("Material Form Description readonly expected to be true", undgDataItem.DI_IsFissileExceptedInfo.ReadOnly, true);

			undgDataItem.DI_DG = subs.PK;
			AssertEquals("Material Form Description readonly expected to be true", undgDataItem.DI_IsFissileExceptedInfo.ReadOnly, true);

			undgDataItem.DI_DG = nonFissileSubs.PK;
			AssertEquals("Material Form Description readonly expected to be false", undgDataItem.DI_IsFissileExceptedInfo.ReadOnly, false);

			undgDataItem.DI_DG = nonRadioactiveSubs.PK;
			AssertEquals("Material Form Description readonly expected to be true", undgDataItem.DI_IsFissileExceptedInfo.ReadOnly, true);
		}

		public void TestDI_IsExclusiveUse_ReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "TTT";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			subs.DG_Class = RadioactiveConstants.RadioactiveClass;

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			AssertEquals("HRCQ readonly expected to be true", undgDataItem.DI_IsExclusiveUseInfo.ReadOnly, true);

			undgDataItem.DI_DG = subs.PK;
			AssertEquals("HRCQ readonly expected to be false", undgDataItem.DI_IsExclusiveUseInfo.ReadOnly, false);

			subs.DG_Class = "1";
			AssertEquals("HRCQ readonly expected to be true", undgDataItem.DI_IsExclusiveUseInfo.ReadOnly, true);
		}

		public void TestApprovalCertificate_ReadOnly()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Class = "7";
			substance.DG_Variant = "a";
			substance.DG_UNNO = "2911";

			var item = Factory.New<ForwardingUNDGDataItem>();
			item.DI_DG = substance.PK;
			AssertEquals(false, item.DI_ApprovalCertificateType_ReadOnly);
			AssertEquals(false, item.DI_ApprovalCertificateIDMark_ReadOnly);

			item.DI_DG = ZGuid.Empty;
			AssertEquals(true, item.DI_ApprovalCertificateType_ReadOnly);
			AssertEquals(true, item.DI_ApprovalCertificateIDMark_ReadOnly);
		}

		public void TestApprovalCertificate_UpdateSubstance()
		{
			var substance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance1.DG_Class = "7";
			substance1.DG_Variant = "a";
			substance1.DG_UNNO = "2911";

			var substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_Class = "7";
			substance2.DG_Variant = ZString.Empty;
			substance2.DG_UNNO = "2910";

			var substance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance3.DG_Class = "7";
			substance3.DG_Variant = ZString.Empty;
			substance3.DG_UNNO = "2912";

			var item = Factory.New<ForwardingUNDGDataItem>();
			item.DI_DG = substance1.PK;
			item.DI_ApprovalCertificateType = ApprovalCertificateTypeList.Codes.FissileMaterialExcepted;
			item.DI_ApprovalCertificateIDMark = "123";

			item.DI_DG = substance2.PK;
			AssertEquals(ApprovalCertificateTypeList.Codes.FissileMaterialExcepted, item.DI_ApprovalCertificateType);
			AssertEquals("123", item.DI_ApprovalCertificateIDMark);

			item.DI_DG = substance3.PK;
			AssertEquals(ZString.Empty, item.DI_ApprovalCertificateType);
			AssertEquals(ZString.Empty, item.DI_ApprovalCertificateIDMark);
		}

		public void TestReadonly_SecurityRight()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Class = RadioactiveConstants.RadioactiveClass;
			var item = Factory.New<ForwardingUNDGDataItem>();
			item.DI_DG = substance.PK;

			var infos = GetRadioactiveInfos(item);
			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = true;
			foreach (var info in infos)
			{
				AssertEquals($"{info.Name} should be editable", false, info.ReadOnly);
			}

			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = false;
			foreach (var info in infos)
			{
				AssertEquals($"{info.Name} should be readonly", true, info.ReadOnly);
			}
		}

		IEnumerable<ZPropertyInfo> GetRadioactiveInfos(UNDGDataItem undg)
		{
			yield return undg.DI_RadioactiveLabelCategoryInfo;
			yield return undg.DI_RadioactiveTransportIndexInfo;
			yield return undg.DI_IsHighwayRouteControlledQuantityInfo;
			yield return undg.DI_IsExclusiveUseInfo;
			yield return undg.DI_MaterialFormDescriptionInfo;
			yield return undg.DI_IsFissileExceptedInfo;
			yield return undg.DI_RadionuclideElementInfo;
			yield return undg.DI_RadionuclideElementSuffixInfo;
			yield return undg.DI_RadioactiveMaximumActivityInfo;
			yield return undg.DI_RadioactiveMaximumActivityUnitInfo;
		}

		#region UNDGContact Defaulting

		public void TestUNDGContact_OneDGContact_DefaultsToPickupFrom()
		{
			var (shipment, pickup, shipper, pickupAddress, shipperAddress) = GetPickUpAndShipper();

			var (expectedContact, _) = CreateContactAllocation(pickup, workingAddress: pickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);
			CreateContactAllocation(shipper, workingAddress: shipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.NZCustoms);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("One DG allocated contact in `Pickup From` organisation", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_OneDGContact_FallsBackToShipment()
		{
			var (shipment, pickup, shipper, pickupAddress, shipperAddress) = GetPickUpAndShipper();

			CreateContactAllocation(pickup, workingAddress: pickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.NZCustoms);
			var (expectedContact, _) = CreateContactAllocation(shipper, workingAddress: shipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("One DG allocated contact in `Shipper` organisation", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_OneDGContactEach_DifferentWorkingAddress_DefaultsToPickup()
		{
			var (shipment, pickup, shipper, _, _) = GetPickUpAndShipper();
			var someOtherPickupAddress = pickup.Addresses.AddNew();
			var someOtherShipperAddress = shipper.Addresses.AddNew();

			var (expectedContact, _) = CreateContactAllocation(pickup, workingAddress: someOtherPickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);
			CreateContactAllocation(shipper, workingAddress: someOtherShipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.NZCustoms);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("One DG allocated contact in `Pickup From` organisation", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_OneDGContact_DifferentWorkingAddress_FallsBackToShipment()
		{
			var (shipment, pickup, shipper, _, _) = GetPickUpAndShipper();
			var someOtherPickupAddress = pickup.Addresses.AddNew();
			var someOtherShipperAddress = shipper.Addresses.AddNew();

			CreateContactAllocation(pickup, workingAddress: someOtherPickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.NZCustoms);
			var (expectedContact, _) = CreateContactAllocation(shipper, workingAddress: someOtherShipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("One DG allocated contact in `Shipper` organisation", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_MultipleDGContact_OneMatchPickupAddress()
		{
			var (shipment, pickup, shipper, pickupAddress, shipperAddress) = GetPickUpAndShipper();

			var (expectedContact, _) = CreateContactAllocation(pickup, workingAddress: pickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);
			CreateContactAllocation(shipper, workingAddress: shipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("Multiple DG contacts, only one which matches Pickup From address", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_MultipleDGContact_MultipleMatchPickupAddress()
		{
			var (shipment, pickup, shipper, pickupAddress, shipperAddress) = GetPickUpAndShipper();

			var (expectedContact, _) = CreateContactAllocation(pickup, workingAddress: pickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);
			CreateContactAllocation(pickup, workingAddress: pickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);

			CreateContactAllocation(shipper, workingAddress: shipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("Multiple DG contacts, multiple match Pickup From address, should randomly pick", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_MultipleDGContact_OneMatchShipperAddress()
		{
			var (shipment, pickup, shipper, pickupAddress, shipperAddress) = GetPickUpAndShipper();

			CreateContactAllocation(pickup, workingAddress: pickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.USFSV);
			var (expectedContact, _) = CreateContactAllocation(shipper, workingAddress: shipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("Multiple DG contacts, only one which matches Shipper address", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_MultipleDGContact_MultipleMatchShipperAddress()
		{
			var (shipment, pickup, shipper, pickupAddress, shipperAddress) = GetPickUpAndShipper();

			CreateContactAllocation(pickup, workingAddress: pickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.CNCUS);

			var (expectedContact, _) = CreateContactAllocation(shipper, workingAddress: shipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);
			CreateContactAllocation(shipper, workingAddress: shipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("Multiple DG contacts, multiple match Shipper address, should randomly pick", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_ShouldNotDefault_IfNoHAZContactsExist()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("No contacts", ZGuid.Empty, undgDataItem.DI_OC_DGContact);
		}

		public void TestUNDGContact_ShouldOnlyDefault_ActiveContacts()
		{
			var (shipment, pickup, shipper, pickupAddress, shipperAddress) = GetPickUpAndShipper();

			CreateContactAllocation(pickup, workingAddress: pickupAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ, isActive: false);
			var (expectedContact, _) = CreateContactAllocation(shipper, workingAddress: shipperAddress.PK, allocationType: OrgConstants.ContactAllocationType.HAZ);

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			AssertEquals("Should only default to active UNDG contacts", expectedContact.PK, undgDataItem.DI_OC_DGContact);
		}

		#endregion

		#region Implementation

		public (ForwardingShipment shipment, OrgHeader pickup, OrgHeader shipper, OrgAddress pickupAddress, OrgAddress shipperAddress) GetPickUpAndShipper()
		{
			var pickup = Factory.New<OrgHeader>();
			var pickupAddress = pickup.Addresses.AddNew();

			var shipper = Factory.New<OrgHeader>();
			var shipperAddress = shipper.Addresses.AddNew();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			shipment.ConsignorPickupAddress.OrganisationPK = pickup.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;

			shipment.ConsignorDocumentaryAddress.OrganisationPK = shipper.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAddress.PK;

			return (shipment, pickup, shipper, pickupAddress, shipperAddress);
		}

		(OrgContact contact, OrgContactAllocation allocation) CreateContactAllocation(OrgHeader organisation, ZGuid workingAddress, ZString allocationType, bool isActive = true)
		{
			var contact = organisation.Contacts.AddNew();
			var allocation = contact.Allocations.AddNew();

			contact.OC_IsActive = isActive;
			contact.WorkingAddressPK = workingAddress;
			allocation.PC_Type = allocationType;

			return (contact, allocation);
		}

		#endregion

		protected override void TearDown()
		{
			base.TearDown();
			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.ClearOverriddenSecurityValue();
		}
	}
}
