using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Business.Testing
{
	public class QuickPODTest : BaseFreightTest
	{
		public void TestSettingOnePropertySetsOtherReadOnly()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UniqueConsignRef = "S11";

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			pod1.ShipmentID = "11";
			AssertEquals("setting shipment id should make house bill read only", true, pod1.HouseBillInfo.ReadOnly);
			pod1.ShipmentID = "";
			AssertEquals("blank shipment id should make both infos writable", false, pod1.HouseBillInfo.ReadOnly);
			AssertEquals("blank shipment id should make both infos writable", false, pod1.ShipmentIDInfo.ReadOnly);
			pod1.HouseBill = "FindMe";
			AssertEquals("setting shipment id should make shipment id read only", true, pod1.ShipmentIDInfo.ReadOnly);
			pod1.HouseBill = "";
			AssertEquals("blank house bill should make both infos writable", false, pod1.HouseBillInfo.ReadOnly);
			AssertEquals("blank house bill should make both infos writable", false, pod1.ShipmentIDInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenConfirmDeletedWithDivots()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_HouseBill = "TEST BILL";

			var packline = shipment.OuterPackLines.AddNew();

			var quickPODs = new QuickPODs(Factory);
			var pod = quickPODs.QuickPODsCollection.AddNew();

			pod.HouseBill = "TEST BILL";

			var divot = pod.DeliveryConfirm.GetDivot(packline);

			Factory.Save();

			divot.J8_DeliveryWeight = 1439535.500;
			pod.Delete();

			Factory.Save();
		}

		public void TestSettingInvalidShipmentPropertyBlanksTheOther()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UniqueConsignRef = "S11";

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			pod1.HouseBill = "hbhbhbhbh";
			pod1.ShipmentID = "1321";
			AssertEquals("setting invalid shipment id should make house bill blank", true, pod1.HouseBill.IsEmpty);
			pod1.HouseBill = "fdfdsfd";
			AssertEquals("setting invalid house bill should make shipment id blank", true, pod1.ShipmentID.IsEmpty);
		}

		public void TestValidationErrorHasCorrectPropertyValue()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UniqueConsignRef = "S11";

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			pod1.HouseBill = "hbhbhbhbh";
			AssertHasError("setting invalid house bill should invalidate with house bill", pod1.HouseBillInfo, "no match found.");
			pod1.ShipmentID = "fdfdsfd";
			AssertHasError("setting invalid house bill should invalidate with house bill", pod1.ShipmentIDInfo, "no match found.");
		}

		public void TestShipmentIDSetsShipmentAndHouseBill()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UniqueConsignRef = "S11";

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				AssertNull("pod1: HouseBill blank - Shipment should be null", pod1.Shipment);

				pod1.ShipmentID = "11";
				AssertEquals("pod1: HouseBill matches - Shipment should be set", shipment, pod1.Shipment);
				AssertEquals("pod1: HouseBill matches - house bill should be set", "FINDME", pod1.HouseBill);
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestFindShipmentConvertedFromBooking()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "AAA";
			shipment.JS_UniqueConsignRef = "S11";
			shipment.JS_IsBooking = true;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				AssertNotEquals("should have not found any shipments", shipment, pod1.Shipment);
				pod1.HouseBill = "AAA";
				AssertEquals("should have found shipment with housebill AAA", shipment, pod1.Shipment);
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		#region Validation

		public void TestMissingShipmentValidates()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UniqueConsignRef = "S11";

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				AssertNull("pod1: HouseBill blank - Shipment should be null", pod1.Shipment);

				pod1.ShipmentID = "12";
				AssertHasErrors("should validate ID info", pod1.ShipmentIDInfo);
				AssertHasErrors("should validate house bill info", pod1.HouseBillInfo);

				pod1.ShipmentID = "";
				pod1.HouseBill = "FindMe";
				AssertHasErrors("should validate house bill info", pod1.HouseBillInfo);
				AssertHasErrors("should validate shipment ID info", pod1.ShipmentIDInfo);
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestBookingCannotBeAttachedValidation()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UniqueConsignRef = "S11";
			shipment.OuterPackLines.AddNew();
			shipment.DeliveryConfirms.AddNew();

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			pod1.ShipmentID = shipment.JS_UniqueConsignRef;
			AssertNoErrors(pod1.ShipmentIDInfo);

			shipment.JS_UniqueConsignRef = "S12";
			shipment.JS_IsBooking = true;
			shipment.JS_IsCFSRegistered = false;
			shipment.JS_IsForwardRegistered = false;

			pod1.ShipmentID = shipment.JS_UniqueConsignRef;
			AssertHasErrors(pod1.ShipmentIDInfo);
		}

		public void TestShipmentIDErrorsForWrongShipmentType()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S11";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				AssertNotEquals("should have not found any shipments", shipment, pod1.Shipment);
				pod1.ShipmentID = "S11";
				AssertNotNull(pod1.ShipmentIDInfo);
				AssertHasErrorContaining(pod1.ShipmentIDInfo, "'S11' is a Co-Load Master. Confirmations need to be entered on the related Shipments");
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		#endregion

		public void TestHouseBillSetsShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UniqueConsignRef = "s11";

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();
			AssertNull("pod1: HouseBill blank - Shipment should be null", pod1.Shipment);

			pod1.HouseBill = "FindMe";
			AssertEquals("pod1: HouseBill matches - Shipment should be set", shipment, pod1.Shipment);
			AssertEquals("pod1: HouseBill matches - Shipment ID should be set", "s11", pod1.ShipmentID);

			pod1.HouseBill = "DontFindMe";
			AssertNull("pod1: HouseBill invalid - Shipment should be null", pod1.Shipment);
		}

		public void TestDeliverTimeInSetsShipmentDeliveredDate()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";
			shipment.OuterPackLines.AddNew();

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();
			pod1.HouseBill = "FindMe";

			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			pod1.DeliveryConfirm.EU_PickupDeliveryTime = ZDateTime.Now;

			shipment.Job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();
			AssertEquals(pod1.DeliveryConfirm.EU_PickupDeliveryTime, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			pod1.PacksDelivered = 5;
			pod1.DeliveryConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			AssertEquals("Packs Delviered != Total Packs", ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
		}

		public void TestChargeCodeSetsCharge()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "FindMe";

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod = quickPODs.QuickPODsCollection.AddNew();
			pod.HouseBill = "FindMe";
			AssertEquals("pod: HouseBill matches - Shipment should be set", shipment, pod.Shipment);

			pod.ChargeCode = ZGuid.Empty;
			AssertNull("pod: Charge should be null", pod.Charge);

			AccChargeCode accCharge = Factory.New<AccChargeCode>();
			accCharge.AC_Desc = "Desc";

			pod.ChargeCode = accCharge.PK;
			JobCharge charge = pod.Charge;
			AssertNotNull("pod1: valid Charge", charge);

			pod.ChargeCode = ZGuid.Invalid;
			AssertNull("pod: invalid charge - Charge should be null", pod.Charge);
			Assert("should be deleted", charge.IsDeleted);

			pod.ChargeCode = accCharge.PK;
			charge = pod.Charge;
			AssertNotNull("pod1: valid Charge", charge);

			pod.ChargeCode = ZGuid.Missing;
			AssertNull("pod: missing charge - Charge should be null", pod.Charge);
			Assert("should be deleted", charge.IsDeleted);

			pod.ChargeCode = accCharge.PK;
			charge = pod.Charge;
			AssertNotNull("valid Charge", charge);

			AccChargeCode accCharge2 = Factory.New<AccChargeCode>();
			accCharge2.AC_Desc = "Desc2";
			pod.ChargeCode = accCharge2.PK;
			Assert("Previous charge should be deleted", charge.IsDeleted);
			charge = pod.Charge;
			AssertNotNull("New charge is valid and should not be null", charge);
		}

		public void TestContainerisedShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_HouseBill = "FindMe";
			OrgHeader deliveryCompany = Factory.NewWithValidTestData<OrgHeader>();
			shipment.DocsAndCartage.DeliveryCartageCoPK = deliveryCompany.PK;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				AssertNull("pod1: No HouseBill - Shipment should be null", pod1.Shipment);
				AssertEquals("pod1: Packs Delivered", 0, pod1.PacksDelivered);
				AssertEquals("pod1: No HouseBill - Delivery Weight", 0m, pod1.DeliveryWeight);
				AssertEquals("pod1: No HouseBill - Delivery Volume", 0m, pod1.DeliveryVolume);

				pod1.HouseBill = "FindMe";
				AssertEquals("pod1: HouseBill matches - Shipment should be set", shipment, pod1.Shipment);
				AssertNull("Containerised, so don't set Confirm", pod1.DeliveryConfirm);
				AssertNull("Containerised, so don't set Charge", pod1.Charge);
				Assert("Containerised, so error", pod1.ShipmentIDInfo.HasError("Shipment with House Bill 'FindMe' is Containerized. Only Loose Delivery Shipments can be used in Quick POD."));
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestAssemblyMasterShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			shipment.JS_HouseBill = "FindMe";
			OrgHeader deliveryCompany = Factory.NewWithValidTestData<OrgHeader>();
			shipment.DocsAndCartage.DeliveryCartageCoPK = deliveryCompany.PK;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			PackLine packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				AssertNull("pod1: No HouseBill - Shipment should be null", pod1.Shipment);
				AssertEquals("pod1: Packs Delivered", 0, pod1.PacksDelivered);
				AssertEquals("pod1: No HouseBill - Delivery Weight", 0m, pod1.DeliveryWeight);
				AssertEquals("pod1: No HouseBill - Delivery Volume", 0m, pod1.DeliveryVolume);

				pod1.HouseBill = "FindMe";
				AssertEquals("pod1: HouseBill matches - Shipment should be set", shipment, pod1.Shipment);
				AssertNull("Containerised, so don't set Confirm", pod1.DeliveryConfirm);
				AssertNull("Containerised, so don't set Charge", pod1.Charge);
				Assert(pod1.ShipmentIDInfo.HasError("Shipment with House Bill 'FindMe' is an Assembly Master. Confirmations need to be entered on the related Shipments."));
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestCoLoadMasterShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			shipment.JS_HouseBill = "FindMe";
			OrgHeader deliveryCompany = Factory.NewWithValidTestData<OrgHeader>();
			shipment.DocsAndCartage.DeliveryCartageCoPK = deliveryCompany.PK;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			PackLine packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				AssertNull("pod1: No HouseBill - Shipment should be null", pod1.Shipment);
				AssertEquals("pod1: Packs Delivered", 0, pod1.PacksDelivered);
				AssertEquals("pod1: No HouseBill - Delivery Weight", 0m, pod1.DeliveryWeight);
				AssertEquals("pod1: No HouseBill - Delivery Volume", 0m, pod1.DeliveryVolume);

				pod1.HouseBill = "FindMe";
				AssertEquals("pod1: HouseBill matches - Shipment should be set", shipment, pod1.Shipment);
				AssertNull("Containerised, so don't set Confirm", pod1.DeliveryConfirm);
				AssertNull("Containerised, so don't set Charge", pod1.Charge);
				Assert(pod1.ShipmentIDInfo.HasError("Shipment with House Bill 'FindMe' is a Co-Load Master. Confirmations need to be entered on the related Shipments."));
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestLooseShipmentWith1PackLineNoLegs()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FindMe";
			OrgHeader deliveryCompany = Factory.NewWithValidTestData<OrgHeader>();
			shipment.DocsAndCartage.DeliveryCartageCoPK = deliveryCompany.PK;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				AssertNull("pod1: No HouseBill - Shipment should be null", pod1.Shipment);
				AssertEquals("pod1: Packs Delivered", 0, pod1.PacksDelivered);
				AssertEquals("pod1: No HouseBill - Delivery Weight", 0m, pod1.DeliveryWeight);
				AssertEquals("pod1: No HouseBill - Delivery Volume", 0m, pod1.DeliveryVolume);

				pod1.HouseBill = "FindMe";
				AssertEquals("pod1: HouseBill matches - Shipment should be set", shipment, pod1.Shipment);
				AssertEquals("pod1: Packs Delivered", 99, pod1.PacksDelivered);
				AssertEquals("pod1: Delivery Weight", 111.111m, pod1.DeliveryWeight);
				AssertEquals("pod1: Delivery Volume", 222.222m, pod1.DeliveryVolume);
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestLooseShipmentWithMultiplePackLinesNoLegs()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FindMe";
			OrgHeader deliveryCompany = Factory.NewWithValidTestData<OrgHeader>();
			shipment.DocsAndCartage.DeliveryCartageCoPK = deliveryCompany.PK;

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 100;
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine1.JL_ActualVolume = 200m;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 100;
			packLine2.JL_ActualWeight = 100m;
			packLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine2.JL_ActualVolume = 200m;
			packLine2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				pod.HouseBill = "FindMe";
				AssertEquals("HouseBill matches - Shipment should be set", shipment, pod.Shipment);
				AssertEquals("Packs Delivered", 200, pod.PacksDelivered);
				AssertEquals("Delivery Weight", 200m, pod.DeliveryWeight);
				AssertEquals("Delivery Volume", 400m, pod.DeliveryVolume);
			}
			finally
			{
				pod.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestLooseShipmentWithMultiplePackLines1Leg()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FindMe";

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm deliveryLeg = shipment.DeliveryConfirms.AddNew();
			deliveryLeg.EU_PickupDeliveryTime = ZDateTime.Now;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				pod.HouseBill = "FindMe";
				AssertEquals("HouseBill matches, shipment should be set", shipment, pod.Shipment);
				AssertNull("HouseBill matches but DeliveryLeg should not be set as there are already legs that have a date", pod.DeliveryConfirm);
				AssertNull("HouseBill matches but Charge should not be set as there are already legs", pod.Charge);

				deliveryLeg.EU_PickupDeliveryTime = ZDateTime.Empty;

				quickPODs = new QuickPODs(Factory);
				pod = quickPODs.QuickPODsCollection.AddNew();
				pod.HouseBill = "FindMe";
				AssertEquals("HouseBill matches, shipment should be set", shipment, pod.Shipment);
				AssertEquals("HouseBill matches and DeliveryLeg should match the only leg", deliveryLeg, pod.DeliveryConfirm);
				AssertNull("HouseBill matches and Charge should be null, not set", pod.Charge);

				AccChargeCode accCharge = Factory.New<AccChargeCode>();
				accCharge.AC_Desc = "Desc";
				pod.ChargeCode = accCharge.PK;
				AssertNotNull("pod1: valid Charge", pod.Charge);
			}
			finally
			{
				pod.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestLooseShipmentWithMultiplePackLinesDifferentUnits()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 100;
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine1.JL_ActualVolume = 200m;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 100;
			packLine2.JL_ActualWeight = 100m;
			packLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine2.JL_ActualVolume = 200m;
			packLine2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				pod.HouseBill = "FindMe";
				AssertEquals("HouseBill matches - Shipment should be set", shipment, pod.Shipment);
				AssertNotNull("HouseBill matches - DeliveryLeg should be set", pod.DeliveryConfirm);
				AssertNull("HouseBill matches - Charge should be set if code is valid", pod.Charge);
				CommonConfirmDivot divot1 = pod.DeliveryConfirm.GetDivot(packLine1);
				CommonConfirmDivot divot2 = pod.DeliveryConfirm.GetDivot(packLine2);
				AssertNotNull("Packline1 should have a divot", divot1);
				AssertNotNull("Packline2 should have a divot", divot2);

				AssertEquals("Default - Packs Delivered", 200, pod.PacksDelivered);
				AssertEquals("Default - Delivery Weight in Shipment Units (pounds)", 320.462m, Utilities.Round(pod.DeliveryWeight, 3));
				AssertEquals("Default - Delivery Volume in Shipment Units (cf)", 7262.933m, Utilities.Round(pod.DeliveryVolume, 3));

				pod.PacksDelivered = 100;
				AssertEquals("Packs Delivered", 100, pod.PacksDelivered);
				AssertEquals("Delivery Weight in Shipment Units (pounds)", 100m, pod.DeliveryWeight);
				AssertEquals("Delivery Volume in Shipment Units (cf)", 200m, pod.DeliveryVolume);
				AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
				AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
				AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
				AssertEquals("Divot 2 - Packs Delivered", 0, divot2.J8_PackagesDelivered);
				AssertEquals("Divot 2 - Delivery Weight", 0m, divot2.J8_DeliveryWeight);
				AssertEquals("Divot 2 - Delivery Volume", 0m, divot2.J8_DeliveryVolume);

				pod.PacksDelivered = 150;
				AssertEquals("Packs Delivered", 150, pod.PacksDelivered);
				AssertEquals("Delivery Weight in Shipment Units (pounds)", 210.231m, Utilities.Round(pod.DeliveryWeight, 3));
				AssertEquals("Delivery Volume in Shipment Units (cf)", 3731.467m, Utilities.Round(pod.DeliveryVolume, 3));
				AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
				AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
				AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
				AssertEquals("Divot 2 - Packs Delivered", 50, divot2.J8_PackagesDelivered);
				AssertEquals("Divot 2 - Delivery Weight", 50m, divot2.J8_DeliveryWeight);
				AssertEquals("Divot 2 - Delivery Volume", 100m, divot2.J8_DeliveryVolume);

				pod.DeliveryWeight = 220m;
				AssertEquals("Packs Delivered", 150, pod.PacksDelivered);
				AssertEquals("Delivery Weight in Shipment Units (pounds)", 220m, Utilities.Round(pod.DeliveryWeight, 3));
				AssertEquals("Delivery Volume in Shipment Units (cf)", 3731.467m, Utilities.Round(pod.DeliveryVolume, 3));
				AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
				AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
				AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
				AssertEquals("Divot 2 - Packs Delivered", 50, divot2.J8_PackagesDelivered);
				AssertEquals("Divot 2 - Delivery Weight", 54.431m, Utilities.Round(divot2.J8_DeliveryWeight, 3)); //120 pounds in kg = 54.431
				AssertEquals("Divot 2 - Delivery Volume", 100m, divot2.J8_DeliveryVolume);

				pod.DeliveryVolume = 3000m;
				AssertEquals("Packs Delivered", 150, pod.PacksDelivered);
				AssertEquals("Delivery Weight in Shipment Units (pounds)", 220m, Utilities.Round(pod.DeliveryWeight, 3));
				AssertEquals("Delivery Volume in Shipment Units (cf)", 2999.994m, Utilities.Round(pod.DeliveryVolume, 3));
				AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
				AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
				AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
				AssertEquals("Divot 2 - Packs Delivered", 50, divot2.J8_PackagesDelivered);
				AssertEquals("Divot 2 - Delivery Weight", 54.431m, Utilities.Round(divot2.J8_DeliveryWeight, 3));
				AssertEquals("Divot 2 - Delivery Volume", 79.287m, Utilities.Round(divot2.J8_DeliveryVolume, 3)); //2800 cf in m3 = 79.287
			}
			finally
			{
				pod.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestLooseShipmentWithMultiplePackLinesDifferentUnitsExtraDelivered()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 100;
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine1.JL_ActualVolume = 200m;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 100;
			packLine2.JL_ActualWeight = 100m;
			packLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine2.JL_ActualVolume = 200m;
			packLine2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPOD pod = quickPODs.QuickPODsCollection.AddNew();

			try
			{
				pod.HouseBill = "FindMe";
				AssertEquals("HouseBill matches - Shipment should be set", shipment, pod.Shipment);
				AssertNotNull("HouseBill matches - DeliveryLeg should be set", pod.DeliveryConfirm);
				AssertNull("HouseBill matches - Charge should be set if code is valid", pod.Charge);
				CommonConfirmDivot divot1 = pod.DeliveryConfirm.GetDivot(packLine1);
				CommonConfirmDivot divot2 = pod.DeliveryConfirm.GetDivot(packLine2);
				AssertNotNull("Packline1 should have a divot", divot1);
				AssertNotNull("Packline2 should have a divot", divot2);

				AssertEquals("Default - Packs Delivered", 200, pod.PacksDelivered);
				AssertEquals("Default - Delivery Weight in Shipment Units (pounds)", 320.462m, Utilities.Round(pod.DeliveryWeight, 3));
				AssertEquals("Default - Delivery Volume in Shipment Units (cf)", 7262.933m, Utilities.Round(pod.DeliveryVolume, 3));
				AssertEquals("Default - Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
				AssertEquals("Default - Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
				AssertEquals("Default - Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
				AssertEquals("Default - Divot 2 - Packs Delivered", 100, divot2.J8_PackagesDelivered);
				AssertEquals("Default - Divot 2 - Delivery Weight", 100m, divot2.J8_DeliveryWeight);
				AssertEquals("Default - Divot 2 - Delivery Volume", 200m, divot2.J8_DeliveryVolume);

				pod.PacksDelivered = 210;
				AssertEquals("Packs Delivered", 210, pod.PacksDelivered);
				AssertEquals("Delivery Weight in Shipment Units (pounds)", 342.508m, Utilities.Round(pod.DeliveryWeight, 3));
				AssertEquals("Delivery Volume in Shipment Units (cf)", 7969.227m, Utilities.Round(pod.DeliveryVolume, 3));
				AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
				AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
				AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
				AssertEquals("Divot 2 - Packs Delivered", 110, divot2.J8_PackagesDelivered);
				AssertEquals("Divot 2 - Delivery Weight", 110m, divot2.J8_DeliveryWeight);
				AssertEquals("Divot 2 - Delivery Volume", 220m, divot2.J8_DeliveryVolume);

				pod.DeliveryWeight = 350m;
				AssertEquals("Packs Delivered", 210, pod.PacksDelivered);
				AssertEquals("Delivery Weight in Shipment Units (pounds)", 350m, Utilities.Round(pod.DeliveryWeight, 3));
				AssertEquals("Delivery Volume in Shipment Units (cf)", 7969.227m, Utilities.Round(pod.DeliveryVolume, 3));
				AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
				AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
				AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
				AssertEquals("Divot 2 - Packs Delivered", 110, divot2.J8_PackagesDelivered);
				AssertEquals("Divot 2 - Delivery Weight", 113.398m, Utilities.Round(divot2.J8_DeliveryWeight, 3));
				AssertEquals("Divot 2 - Delivery Volume", 220m, divot2.J8_DeliveryVolume);

				pod.DeliveryVolume = 8000m;
				AssertEquals("Packs Delivered", 210, pod.PacksDelivered);
				AssertEquals("Delivery Weight in Shipment Units (pounds)", 350m, Utilities.Round(pod.DeliveryWeight, 3));
				AssertEquals("Delivery Volume in Shipment Units (cf)", 7999.986m, Utilities.Round(pod.DeliveryVolume, 3));
				AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
				AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
				AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
				AssertEquals("Divot 2 - Packs Delivered", 110, divot2.J8_PackagesDelivered);
				AssertEquals("Divot 2 - Delivery Weight", 113.398m, Utilities.Round(divot2.J8_DeliveryWeight, 3));
				AssertEquals("Divot 2 - Delivery Volume", 220.871m, Utilities.Round(divot2.J8_DeliveryVolume, 3));
			}
			finally
			{
				pod.Shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestBOsAreDiscardedIfPODRemoved()
		{
			var quickPODs = new QuickPODs(Factory);
			var pod = quickPODs.QuickPODsCollection.AddNew();

			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var packline1 = shipment1.OuterPackLines.AddNew();
			shipment1.JS_HouseBill = "FindMe";
			Factory.Save();

			pod.HouseBill = "FindMe";
			var leg1 = pod.DeliveryConfirm;
			var charge1 = pod.Charge;
			AssertNotNull("leg not null", leg1);
			AssertNull("charge null if no code", charge1);

			pod.HouseBill = "";
			AssertNull("leg deleted", pod.DeliveryConfirm);
			AssertNull("charge deleted", pod.Charge);
			Assert("leg deleted", leg1.IsDeleted);

			var shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_HouseBill = "FindMe2";
			shipment2.JS_RL_NKDestination = OverseasPort;
			shipment2.JS_RL_NKOrigin = HomePort;
			var packline2 = shipment2.OuterPackLines.AddNew();
			pod.HouseBill = "FindMe2";
			var leg2 = pod.DeliveryConfirm;
			leg2.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);

			var accCharge = Factory.New<AccChargeCode>();
			accCharge.AC_Desc = "Desc";
			accCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			pod.ChargeCode = accCharge.PK;

			var charge2 = pod.Charge;
			var chargeCodes = new AccChargeCodeCollection(Factory, charge2.Lookups.ChargeCodes.CompleteFilter);
			chargeCodes.Load();
			charge2.JR_AC = chargeCodes[0].PK;
			AssertNotNull("leg not null", leg2);
			AssertNotNull("charge not null", charge2);

			Factory.Save();

			leg2.EU_GoodsSignForBy = "bob";
			charge2.JR_LocalCostAmt = 123.4m;

			pod.HouseBill = "";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var leg2InNewFactory = newFactory.Load<CommonPickupDeliveryConfirm>(leg2.PK);
			var charge2InNewFactory = newFactory.Load<JobCharge>(charge2.PK);
			AssertNotEquals("JU_DeliverySignedFor should not be bob", "bob", leg2InNewFactory.EU_GoodsSignForBy);
			AssertNotEquals("JR_Desc should not be 1", 123.4m, charge2InNewFactory.JR_LocalCostAmt);
		}
	}
}
