using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	class TransportBookingRatingIntegrationTest : BaseRatingIntegrationTest
	{
		#region Percentage Calculator Shipment With Multiple Transport Bookings

		public void TestPercentageCalculatorShipmentWithMultipleTransportBookings()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			Helper.ChargeCodes.New("TBK1", "Transport Booking UNT", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK2", "Transport Booking PER", PercentageCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			TestHelper.SetChargeableFactorRegistry(TransportBookingChargeableFactorRegistryItem, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 7000, Volume.CubicCentimeters, Weight.Kilograms, 294, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 8000, Volume.CubicCentimeters, Weight.Kilograms, 394, Volume.CubicInches, Weight.Pounds);

			#region Rates Setup

			var costProvider = Factory.NewWithValidTestData<OrgHeader>();
			costProvider.OH_IsCreditor = true;

			var cost = Helper.NewCosting(costProvider);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine("TBK1", UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 100;

			var costLinePercentage = costEntry.AddRateLine("TBK2", PercentageCalculator.Code);
			costLinePercentage.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["TBK1"].PK;
			costLinePercentage.GetCalculator<PercentageCalculator>().Percent = 10m;

			#endregion

			#region Create shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			#endregion

			#region Create booking consolidation with bookings

			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "JS";

			var booking1 = bookingConsolidation.Bookings.AddNew();
			booking1.KM_KT_NKBookingTemplate = "EFPU";
			booking1.KM_RatingFreightMode = "LSE";
			booking1.KM_JobID = "TM00000001";
			booking1.Address.OrganisationPK = costProvider.PK;

			var fromInstruction1 = booking1.Instructions.AddNew();
			fromInstruction1.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction1.KN_IsLooseRateable = true;
			fromInstruction1.Address.OrganisationPK = localClient.PK;

			var toInstruction1 = booking1.Instructions.AddNew();
			toInstruction1.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction1.KN_IsLooseRateable = true;
			toInstruction1.Address.OrganisationPK = consignee.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box1 = CreatePackage(booking1, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet1 = CreatePackage(booking1, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton1 = CreatePackage(booking1, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction1, box1, pallet1, carton1);
			CreateInstructionPkgDivots(toInstruction1, box1, pallet1, carton1);

			var booking2 = bookingConsolidation.Bookings.AddNew();
			booking2.KM_KT_NKBookingTemplate = "EFPU";
			booking2.KM_RatingFreightMode = "LSE";
			booking2.KM_JobID = "TM00000002";
			booking2.Address.OrganisationPK = costProvider.PK;

			var fromInstruction2 = booking2.Instructions.AddNew();
			fromInstruction2.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction2.KN_IsLooseRateable = true;
			fromInstruction2.Address.OrganisationPK = localClient.PK;

			var toInstruction2 = booking2.Instructions.AddNew();
			toInstruction2.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction2.KN_IsLooseRateable = true;
			toInstruction2.Address.OrganisationPK = consignee.PK;

			var box2 = CreatePackage(booking2, 6, PkgUnit.Box, commodity, 60m, Weight.Kilograms, 2m, Volume.CubicMetres);
			var pallet2 = CreatePackage(booking2, 46, PkgUnit.Pallet, commodity, 40m, Weight.Kilograms, 0.30m, Volume.CubicMetres);
			var carton2 = CreatePackage(booking2, 10, PkgUnit.Carton, commodity, 100m, Weight.Kilograms, 0.60m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction2, box2, pallet2, carton2);
			CreateInstructionPkgDivots(toInstruction2, box2, pallet2, carton2);

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TBK1",
						JR_OSSellAmt = 24166.70m,
						JR_OSCostAmt = 24166.70m,
						CostCalculationDescription = "Transport Booking TM00000001"
					},
					new AssertionCharge
					{
						ChargeCode = "TBK1",
						JR_OSSellAmt = 48333.30m,
						JR_OSCostAmt = 48333.30m,
						CostCalculationDescription = "Transport Booking TM00000002"
					},
					new AssertionCharge
					{
						ChargeCode = "TBK2",
						JR_OSSellAmt = 4833.33m,
						JR_OSCostAmt = 4833.33m,
						CostCalculationDescription = "TBK2: 10.00% of (AUD 48333.30 (TBK1))"
					},
					new AssertionCharge
					{
						ChargeCode = "TBK2",
						JR_OSSellAmt = 2416.67m,
						JR_OSCostAmt = 2416.67m,
						CostCalculationDescription = "TBK2: 10.00% of (AUD 24166.70 (TBK1))"
					}
				};

			AutorateAndAssert("Percentage calculator should be applied to correct Transport Booking", expected, shipment, localClient);
		}

		public void TestPercentageCalculatorShipmentWithMultipleTransportBookings_ShipmentWithPostedCharges()
		{
			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;
			var consignee = Helper.NewOrgHeader(1);

			var tbk1ChargeCode = Helper.ChargeCodes.New("TBK1", "Transport Booking UNT", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			var tbk2ChargeCode = Helper.ChargeCodes.New("TBK2", "Transport Booking PER", PercentageCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			TestHelper.SetChargeableFactorRegistry(TransportBookingChargeableFactorRegistryItem, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 7000, Volume.CubicCentimeters, Weight.Kilograms, 294, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 8000, Volume.CubicCentimeters, Weight.Kilograms, 394, Volume.CubicInches, Weight.Pounds);

			#region Rates Setup

			var costProvider = Factory.NewWithValidTestData<OrgHeader>();
			costProvider.OH_IsCreditor = true;

			var cost = Helper.NewCosting(costProvider);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine("TBK1", UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 100;

			var costLinePercentage = costEntry.AddRateLine("TBK2", PercentageCalculator.Code);
			costLinePercentage.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["TBK1"].PK;
			costLinePercentage.GetCalculator<PercentageCalculator>().Percent = 10m;

			#endregion

			#region Create shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			#endregion

			#region Create booking consolidation with bookings

			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "JS";

			var booking1 = bookingConsolidation.Bookings.AddNew();
			booking1.KM_KT_NKBookingTemplate = "EFPU";
			booking1.KM_RatingFreightMode = "LSE";
			booking1.KM_JobID = "TM00000001";
			booking1.Address.OrganisationPK = costProvider.PK;

			var fromInstruction1 = booking1.Instructions.AddNew();
			fromInstruction1.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction1.KN_IsLooseRateable = true;
			fromInstruction1.Address.OrganisationPK = localClient.PK;

			var toInstruction1 = booking1.Instructions.AddNew();
			toInstruction1.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction1.KN_IsLooseRateable = true;
			toInstruction1.Address.OrganisationPK = consignee.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box1 = CreatePackage(booking1, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet1 = CreatePackage(booking1, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton1 = CreatePackage(booking1, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction1, box1, pallet1, carton1);
			CreateInstructionPkgDivots(toInstruction1, box1, pallet1, carton1);

			var booking2 = bookingConsolidation.Bookings.AddNew();
			booking2.KM_KT_NKBookingTemplate = "EFPU";
			booking2.KM_RatingFreightMode = "LSE";
			booking2.KM_JobID = "TM00000002";
			booking2.Address.OrganisationPK = costProvider.PK;

			var fromInstruction2 = booking2.Instructions.AddNew();
			fromInstruction2.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction2.KN_IsLooseRateable = true;
			fromInstruction2.Address.OrganisationPK = localClient.PK;

			var toInstruction2 = booking2.Instructions.AddNew();
			toInstruction2.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction2.KN_IsLooseRateable = true;
			toInstruction2.Address.OrganisationPK = consignee.PK;

			var box2 = CreatePackage(booking2, 6, PkgUnit.Box, commodity, 60m, Weight.Kilograms, 2m, Volume.CubicMetres);
			var pallet2 = CreatePackage(booking2, 46, PkgUnit.Pallet, commodity, 40m, Weight.Kilograms, 0.30m, Volume.CubicMetres);
			var carton2 = CreatePackage(booking2, 10, PkgUnit.Carton, commodity, 100m, Weight.Kilograms, 0.60m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction2, box2, pallet2, carton2);
			CreateInstructionPkgDivots(toInstruction2, box2, pallet2, carton2);

			#endregion

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.LocalChargesPK = localClient.PK;
				var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate1.AT_Code = "TAX1";
				taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate1.SetRateNumerator_ForTestOnly(5);

				var charge = job.Charges.AddNew();
				charge.JR_AC = tbk1ChargeCode.PK;
				charge.JR_OSCostAmt = 200m;

				var th = Factory.NewWithValidTestData<AccTransactionHeader>();
				th.AH_Ledger = "AP";
				var tl = Factory.NewWithValidTestData<AccTransactionLines>();
				tl.AL_AH = th.PK;
				tl.AL_LineType = TransactionLineTypes.Cost;
				tl.AL_LineAmount = -200;
				tl.AL_OSAmount = -200;
				tl.AL_RX_NKTransactionCurrency = "AUD";
				tl.AL_RevRecognitionType = "IMM";

				charge.JR_AL_APLine = tl.PK;
				charge.JR_AT_SellGSTRate = taxRate1.PK;

				Assert("Prerequisite:", charge.IsCostPosted);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TBK1",
						JR_OSCostAmt = 200m,
						JR_OSSellAmt = 200m,
					},
					new AssertionCharge
					{
						ChargeCode = "TBK1",
						JR_OSSellAmt = 24166.70m,
						JR_OSCostAmt = 24166.70m,
						CostCalculationDescription = "Transport Booking TM00000001"
					},
					new AssertionCharge
					{
						ChargeCode = "TBK1",
						JR_OSSellAmt = 48333.30m,
						JR_OSCostAmt = 48333.30m,
						CostCalculationDescription = "Transport Booking TM00000002"
					},
					new AssertionCharge
					{
						ChargeCode = "TBK2",
						JR_OSSellAmt = 4833.33m,
						JR_OSCostAmt = 4833.33m,
						CostCalculationDescription = "TBK2: 10.00% of (AUD 48333.30 (TBK1))"
					},
					new AssertionCharge
					{
						ChargeCode = "TBK2",
						JR_OSSellAmt = 2416.67m,
						JR_OSCostAmt = 2416.67m,
						CostCalculationDescription = "TBK2: 10.00% of (AUD 24166.70 (TBK1))"
					}
				};

				AutorateAndAssert("Percentage calculator should be applied to correct Transport Booking", expected, shipment, localClient, job: job, autorateRevenue: false);
			}
		}

		#endregion

		#region Apportioned Charges

		public void TestApportionedCharges_GivenStandAloneTransportBooking_ThenShouldUseTransportBookingChargeableFactor()
		{
			AssertEquals("Transport Booking Chargeable Factor Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds);

			var commodity = Factory.New<RefCommodityCode>();

			var dtbBookingConsolidationMultiJob = Factory.New<DtbBookingConsolidation>();
			dtbBookingConsolidationMultiJob.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

			var booking1 = CreateBooking(Factory, dtbBookingConsolidationMultiJob, InstructionTypes.Codes.PickUp, 1, PkgUnit.Box, commodity, 10m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var booking2 = CreateBooking(Factory, dtbBookingConsolidationMultiJob, InstructionTypes.Codes.PickUp, 4, PkgUnit.Box, commodity, 20m, Weight.Kilograms, 0.25m, Volume.CubicMetres);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Booking1 ActualVolumeWeight = 0.15 M3 x 333.3333 KG/M3", 50m, booking1.KM_Calc_ActualVolumeWeight);
				AssertEquals("Booking2 ActualVolumeWeight = 0.25 M3 x 333.3333 KG/M3", 83.333333m, booking2.KM_Calc_ActualVolumeWeight);
			});

			var apportionmentListing = new ApportionmentListing(Factory, dtbBookingConsolidationMultiJob);
			var jobConsolCost = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			jobConsolCost.E6_OSCostAmount = 1000m;
			jobConsolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;

			Factory.Save();

			var apportionmentCharges = jobConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>();
			AssertContainsExactElementsInExactOrder
			(
				"TransportBookingConsolidation ApportionmentCharges should use TransportBookingChargeableFactor",
				new[]
				{
					"FRT => Chargeable: 50, Cost: 375.00", // 50 / (50+83.333) x 1000 = 375
					"FRT => Chargeable: 83.333, Cost: 625.00"  // 83.333 / (50+83.333) x 1000 = 625
				},
				apportionmentCharges.Select(charge => $"{charge.ChargeCode.AC_Code} => Chargeable: {charge.JR_Chargeable}, Cost: {charge.JR_OSCostAmt}")
			);
		}

		static DtbBooking CreateBooking(BusinessObjectFactory factory, DtbBookingConsolidation dtbBookingConsolidationMultiJob, string instructionType, int quantity, string quantityUQ, RefCommodityCode commodity, decimal weight, string weightUQ, decimal volume, string volumeUQ, ZGuid? parentId = null, string parentTableCode = null)
		{
			var dtbBookingConsolidation = factory.New<DtbBookingConsolidation>();

			if (parentId != null && parentTableCode != null)
			{
				dtbBookingConsolidation.KB_ParentID = parentId.Value;
				dtbBookingConsolidation.KB_ParentTableCode = parentTableCode;
			}

			var booking = dtbBookingConsolidation.Bookings.AddNew();
			booking.KM_KB_BookingConsolidationMultiJob = dtbBookingConsolidationMultiJob.PK;

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = instructionType;

			var package = CreatePackage(booking, quantity, quantityUQ, commodity, weight, weightUQ, volume, volumeUQ);
			CreateInstructionPkgDivots(fromInstruction, package);

			return booking;
		}

		public void TestApportionedCharges_GivenTransportBookingWithShipment_WithActualUnitSameWithTargetUnit_ThenShouldUseTransportBookingChargeableFactor()
		{
			AssertEquals("Transport Booking Chargeable Factor Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds); // 1/6000 = 166.6667 KG/M3
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds); // 1/6000 = 166.6667 KG/M3

			var commodity = Factory.New<RefCommodityCode>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var dtbBookingConsolidationMultiJob = Factory.New<DtbBookingConsolidation>();
			dtbBookingConsolidationMultiJob.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

			var shipment1 = CreateForwardingShipment(TransportModes.Road, Consignor.PK, consignee.PK, "AUBNE", "USLAX", 110m, 120m);
			var booking1 = CreateBooking(Factory, dtbBookingConsolidationMultiJob, InstructionTypes.Codes.PickUp, 1, PkgUnit.Box, commodity, 10m, Weight.Kilograms, 0.15m, Volume.CubicMetres, shipment1.PK, JobShipmentSchema.Constants.Prefix);

			var shipment2 = CreateForwardingShipment(TransportModes.Road, Consignor.PK, consignee.PK, "AUBNE", "USLAX", 130m, 140m);
			var booking2 = CreateBooking(Factory, dtbBookingConsolidationMultiJob, InstructionTypes.Codes.PickUp, 4, PkgUnit.Box, commodity, 20m, Weight.Kilograms, 0.25m, Volume.CubicMetres, shipment2.PK, JobShipmentSchema.Constants.Prefix);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Booking1 ActualVolumeWeight = 0.15 M3 x 333.3333 KG/M3", 50m, booking1.KM_Calc_ActualVolumeWeight);
				AssertEquals("Booking2 ActualVolumeWeight = 0.25 M3 x 333.3333 KG/M3", 83.333333m, booking2.KM_Calc_ActualVolumeWeight);
				AssertEquals("Shipment1 ActualVolumeWeight = 120 M3 x 166.6667 KG/M3", 20000m, shipment1.JS_Calc_ActualVolumeWeight);
				AssertEquals("Shipment2 ActualVolumeWeight = 140 M3 x 166.6667 KG/M3", 23333.333333m, shipment2.JS_Calc_ActualVolumeWeight);
			});

			var apportionmentListing = new ApportionmentListing(Factory, dtbBookingConsolidationMultiJob);
			var jobConsolCost = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			jobConsolCost.E6_OSCostAmount = 1000m;
			jobConsolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;

			var apportionmentCharges = jobConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>();
			AssertContainsExactElementsInExactOrder
			(
				"Should use TransportBookingChargeableFactor",
				new[]
				{
					"FRT => Chargeable: 50, Cost: 375.00", // 50 / (50+83.333) x 1000 = 375
					"FRT => Chargeable: 83.333, Cost: 625.00"  // 83.333 / (50+83.333) x 1000 = 625
				},
				apportionmentCharges.Select(charge => $"{charge.ChargeCode.AC_Code} => Chargeable: {charge.JR_Chargeable}, Cost: {charge.JR_OSCostAmt}")
			);

			foreach (var apportionmentCharge in apportionmentCharges)
			{
				apportionmentCharge.Job.Dispose();
			}
		}

		public void TestApportionedCharges_GivenTransportBookingWithShipment_WithActualUnitDifferentFromTargetUnit_ThenShouldUseTransportBookingChargeableFactor()
		{
			PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Weight.Tonnes);

			AssertEquals("Transport Booking Chargeable Factor Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds); // 1/6000 = 166.6667 KG/M3
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds); // 1/6000 = 166.6667 KG/M3

			var commodity = Factory.New<RefCommodityCode>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var dtbBookingConsolidationMultiJob = Factory.New<DtbBookingConsolidation>();
			dtbBookingConsolidationMultiJob.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

			var shipment1 = CreateForwardingShipment(TransportModes.Road, Consignor.PK, consignee.PK, "AUBNE", "USLAX", 110m, 120m);
			shipment1.JS_UnitOfWeight = Weight.Tonnes;
			var booking1 = CreateBooking(Factory, dtbBookingConsolidationMultiJob, InstructionTypes.Codes.PickUp, 1, PkgUnit.Box, commodity, 10m, Weight.Kilograms, 150m, Volume.CubicMetres, shipment1.PK, JobShipmentSchema.Constants.Prefix);

			var shipment2 = CreateForwardingShipment(TransportModes.Road, Consignor.PK, consignee.PK, "AUBNE", "USLAX", 130m, 140m);
			shipment2.JS_UnitOfWeight = Weight.Tonnes;
			var booking2 = CreateBooking(Factory, dtbBookingConsolidationMultiJob, InstructionTypes.Codes.PickUp, 4, PkgUnit.Box, commodity, 20m, Weight.Kilograms, 250m, Volume.CubicMetres, shipment2.PK, JobShipmentSchema.Constants.Prefix);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Booking1 ActualVolumeWeight = 0.15 M3 x 333.3333 KG/M3", 50m, booking1.KM_Calc_ActualVolumeWeight);
				AssertEquals("Booking2 ActualVolumeWeight = 0.25 M3 x 333.3333 KG/M3", 83.333333M, booking2.KM_Calc_ActualVolumeWeight);
				AssertEquals("Shipment1 ActualVolumeWeight = 120 M3 x 166.6667 KG/M3 / 1000 T/KG", 20m, shipment1.JS_Calc_ActualVolumeWeight);
				AssertEquals("Shipment2 ActualVolumeWeight = 140 M3 x 166.6667 KG/M3 / 1000 T/KG", 23.333333m, shipment2.JS_Calc_ActualVolumeWeight);
			});

			var apportionmentListing = new ApportionmentListing(Factory, dtbBookingConsolidationMultiJob);
			var jobConsolCost = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			jobConsolCost.E6_OSCostAmount = 1000m;
			jobConsolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;

			var apportionmentCharges = jobConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>();
			AssertContainsExactElementsInExactOrder
			(
				"Should use TransportBookingChargeableFactor",
				new[]
				{
					// 120 M3 x 333.3333 KG/M3 = 40
					"FRT => Chargeable: 40, Cost: 461.54", // 40 / (40+40.667) x 1000 = 461.54
					// 140 M3 x 333.3333 KG/M3 = 46.667
					"FRT => Chargeable: 46.667, Cost: 538.46"  // 40.667 / (40+40.667) x 1000 = 538.46
				},
				apportionmentCharges.Select(charge => $"{charge.ChargeCode.AC_Code} => Chargeable: {charge.JR_Chargeable}, Cost: {charge.JR_OSCostAmt}")
			);

			foreach (var apportionmentCharge in apportionmentCharges)
			{
				apportionmentCharge.Job.Dispose();
			}
		}

		#endregion

		#region Autorate

		public void TestAutorate_GivenStandAloneTransportBooking_ThenShouldUseTransportBookingChargeableFactor()
		{
			AssertEquals("Transport Booking Chargeable Factor Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 150, Volume.CubicCentimeters, Weight.Kilograms, 100, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 150, Volume.CubicCentimeters, Weight.Kilograms, 100, Volume.CubicInches, Weight.Pounds);

			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew();
			consignorPickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			SetAddress(consignorPickupAddress, "389 Crown St", "Surry Hills", "2010", "NSW", "AUSYD");

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetAddress(consignee.MainAddress, "65 Katoomba St", "Katoomba", "2780", "NSW", "AUSYD");

			var clientRate = Helper.NewClientRate(consignor);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "", "TBK1", 10m, QuantityUnit.KG);

			var booking = CreateBooking(Factory, Factory.NewWithValidTestData<OrgHeader>().PK, RatingFreightModes.Codes.Loose, consignor.PK, consignee.PK, PkgUnit.Box, 2m, QuantityUnit.KG, 3m, Volume.CubicMetres);

			// should use Transport Booking Registry Conversion Factory: 3 M3 X 333.3333 KG/M3
			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { RevenueCalculationDescription = "TBK1: 1000 Kilogram(s) @ AUD 10.00/KG", ChargeCode = "TBK1", JR_OSSellAmt = 10000m, } },
				booking,
				consignor,
				autorateCosts: false
			);
		}

		public void TestAutorate_GivenTransportBookingWithShipment_WhenAutoRateTransportBooking_ThenShouldUseTransportBookingChargeableFactor()
		{
			AssertEquals("Transport Booking Chargeable Factor Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 150, Volume.CubicCentimeters, Weight.Kilograms, 100, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 150, Volume.CubicCentimeters, Weight.Kilograms, 100, Volume.CubicInches, Weight.Pounds);

			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew();
			consignorPickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			SetAddress(consignorPickupAddress, "389 Crown St", "Surry Hills", "2010", "NSW", "AUSYD");

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetAddress(consignee.MainAddress, "65 Katoomba St", "Katoomba", "2780", "NSW", "AUSYD");

			var clientRate = Helper.NewClientRate(consignor);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "", "TBK1", 10m, QuantityUnit.KG);

			var shipment = CreateForwardingShipment(TransportModes.Road, Consignor.PK, consignee.PK, "AUBNE", "USLAX", 40m, 50m);
			var booking = CreateBooking(Factory, Factory.NewWithValidTestData<OrgHeader>().PK, RatingFreightModes.Codes.Loose, consignor.PK, consignee.PK, PkgUnit.Box, 2m, QuantityUnit.KG, 3m, Volume.CubicMetres, shipment.PK);

			// should use Transport Booking Registry Conversion Factory: 3M3 X 333.3333 KG/M3
			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { RevenueCalculationDescription = "TBK1: 1000 Kilogram(s) @ AUD 10.00/KG", ChargeCode = "TBK1", JR_OSSellAmt = 10000m, } },
				booking,
				consignor,
				job: (Job)shipment.Job,
				autorateCosts: false
			);
		}

		public void TestAutorate_GivenTransportBookingWithShipment_WhenAutoRateShipment_ThenShouldUseTransportBookingChargeableFactor()
		{
			AssertEquals("Transport Booking Chargeable Factor Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 150, Volume.CubicCentimeters, Weight.Kilograms, 100, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 150, Volume.CubicCentimeters, Weight.Kilograms, 100, Volume.CubicInches, Weight.Pounds);

			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew();
			consignorPickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			SetAddress(consignorPickupAddress, "389 Crown St", "Surry Hills", "2010", "NSW", "AUSYD");

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetAddress(consignee.MainAddress, "65 Katoomba St", "Katoomba", "2780", "NSW", "AUSYD");

			var clientRate = Helper.NewClientRate(consignor);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "", "TBK1", 10m, QuantityUnit.KG);

			var shipment = CreateForwardingShipment(TransportModes.Road, Consignor.PK, consignee.PK, "AUBNE", "USLAX", 40m, 50m);
			var booking = CreateBooking(Factory, Factory.NewWithValidTestData<OrgHeader>().PK, RatingFreightModes.Codes.Loose, consignor.PK, consignee.PK, PkgUnit.Box, 2m, QuantityUnit.KG, 3m, Volume.CubicMetres, shipment.PK);

			// should use Transport Booking Registry Conversion Factory: 3M3 X 333.3333 KG/M3
			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { RevenueCalculationDescription = "TBK1: 1000 Kilogram(s) @ AUD 10.00/KG", ChargeCode = "TBK1", JR_OSSellAmt = 10000m, } },
				shipment,
				consignor,
				autorateCosts: false
			);
		}

		#endregion

		#region ShipmentWithTransportBookings

		public void TestShipmentWithTransportBooking_JobHeaderDoesntSwapParents()
		{
			var client = NewClient;
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line.GetCalculator<UnitCalculator>().PerUnit = 5;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "AUBNE", "USLAX", 3000);
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = false;

			var manager = new DtbDeliveryManager(Factory, shipment, DtbBookingDirection.PIC, false);

			try
			{
				manager.CreateTransportBooking();

				var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
				var frtCharge = job.Charges.AddNew();
				frtCharge.JR_AC = Helper.ChargeCodes["FRT"].PK;
				frtCharge.JR_OH_SellAccount = client.PK;
				frtCharge.ApplyCustomQuickCalculator(shipment.RatingAdapter, 2000m, 0m);
				frtCharge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
				frtCharge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;
				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 15000m,
						JR_OSSellAmt = 15000m,
					}
				};

				AutorateAndAssert("Should override existing quick calculated charge", expected, shipment, client, null, job);
			}
			finally
			{
				if (manager.LastControllerForTest != null
					&& manager.LastControllerForTest.LastShownForm != null)
				{
					manager.LastControllerForTest.LastShownForm.Dispose();
				}
			}
		}

		public void TestShipmentWithTransportBooking_ShouldOverrideReaChargeWithSpecifiedCostReference()
		{
			var client = NewClient;
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line.GetCalculator<UnitCalculator>().PerUnit = 5;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "AUBNE", "USLAX", 3000);
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = false;

			var manager = new DtbDeliveryManager(Factory, shipment, DtbBookingDirection.PIC, false);

			try
			{
				manager.CreateTransportBooking();

				var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
				var frtCharge = job.Charges.AddNew();
				frtCharge.JR_AC = Helper.ChargeCodes["FRT"].PK;
				frtCharge.JR_OH_SellAccount = client.PK;
				frtCharge.ApplyCustomQuickCalculator(shipment.RatingAdapter, 2000m, 0m);
				frtCharge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
				frtCharge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;
				frtCharge.JR_CostReference = "CostRef1";
				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 15000m,
						JR_OSSellAmt = 15000m,
					}
				};

				AutorateAndAssert("Should override existing quick calculated charge", expected, shipment, client, null, job);
			}
			finally
			{
				if (manager.LastControllerForTest != null
					&& manager.LastControllerForTest.LastShownForm != null)
				{
					manager.LastControllerForTest.LastShownForm.Dispose();
				}
			}
		}

		public void TestShipmentWithMultipleTransportBookings()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			TestHelper.SetChargeableFactorRegistry(TransportBookingChargeableFactorRegistryItem, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 7000, Volume.CubicCentimeters, Weight.Kilograms, 294, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 8000, Volume.CubicCentimeters, Weight.Kilograms, 394, Volume.CubicInches, Weight.Pounds);

			#region Rates Setup

			var rate = Helper.NewClientRate(localClient);
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var line2 = rateEntry2.AddRateLine("TBK1", FlatCalculator.Code);
			line2.GetCalculator<FlatCalculator>().BaseRate = 3000;

			var costProvider = Factory.NewWithValidTestData<OrgHeader>();
			costProvider.OH_IsCreditor = true;

			var cost = Helper.NewCosting(costProvider);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine("TBK1", UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 3;

			#endregion

			#region Create shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			#endregion

			#region Create booking consolidation with bookings

			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "JS";

			var booking1 = bookingConsolidation.Bookings.AddNew();
			booking1.KM_KT_NKBookingTemplate = "EFPU";
			booking1.KM_RatingFreightMode = "LSE";
			booking1.KM_JobID = "TM00000001";
			booking1.Address.OrganisationPK = costProvider.PK;

			var fromInstruction1 = booking1.Instructions.AddNew();
			fromInstruction1.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction1.KN_IsLooseRateable = true;
			fromInstruction1.Address.OrganisationPK = localClient.PK;

			var toInstruction1 = booking1.Instructions.AddNew();
			toInstruction1.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction1.KN_IsLooseRateable = true;
			toInstruction1.Address.OrganisationPK = consignee.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box1 = CreatePackage(booking1, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m,
				Volume.CubicMetres);
			var pallet1 = CreatePackage(booking1, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m,
				Volume.CubicMetres);
			var carton1 = CreatePackage(booking1, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m,
				Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction1, box1, pallet1, carton1);
			CreateInstructionPkgDivots(toInstruction1, box1, pallet1, carton1);

			var booking2 = bookingConsolidation.Bookings.AddNew();
			booking2.KM_KT_NKBookingTemplate = "EFPU";
			booking2.KM_RatingFreightMode = "LSE";
			booking2.KM_JobID = "TM00000002";
			booking2.Address.OrganisationPK = costProvider.PK;

			var fromInstruction2 = booking2.Instructions.AddNew();
			fromInstruction2.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction2.KN_IsLooseRateable = true;
			fromInstruction2.Address.OrganisationPK = localClient.PK;

			var toInstruction2 = booking2.Instructions.AddNew();
			toInstruction2.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction2.KN_IsLooseRateable = true;
			toInstruction2.Address.OrganisationPK = consignee.PK;

			var box2 = CreatePackage(booking2, 6, PkgUnit.Box, commodity, 60m, Weight.Kilograms, 2m, Volume.CubicMetres);
			var pallet2 = CreatePackage(booking2, 46, PkgUnit.Pallet, commodity, 40m, Weight.Kilograms, 0.30m, Volume.CubicMetres);
			var carton2 = CreatePackage(booking2, 10, PkgUnit.Carton, commodity, 100m, Weight.Kilograms, 0.60m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction2, box2, pallet2, carton2);
			CreateInstructionPkgDivots(toInstruction2, box2, pallet2, carton2);

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 15000m,
							JR_OSCostAmt = 15000m,
							CostReferenceNumber = "",
						},
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSSellAmt = 3000,
							JR_OSCostAmt = 1450m,
							CostReferenceNumber = "TM00000002",
						},
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSSellAmt = 3000m,
							JR_OSCostAmt = 725m,
							CostReferenceNumber = "TM00000001",
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestBillOfLadingCostsWithMultipleTransportBookingsRatesAdditionalJobsAndSetsDescriptions()
		{
			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = false;
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_IsDebtor = false;

			var chargeCode = Helper.ChargeCodes.New("TBK1", "TBCHARGE", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			chargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			TestHelper.SetChargeableFactorRegistry(TransportBookingChargeableFactorRegistryItem, 6000, Volume.CubicCentimeters, Weight.Kilograms, 194, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.DomesticChargeableFactorRoad, 7000, Volume.CubicCentimeters, Weight.Kilograms, 294, Volume.CubicInches, Weight.Pounds);
			TestHelper.SetChargeableFactorRegistry(FreightDataRegistry.Instance.InternationalChargeableFactorRoad, 8000, Volume.CubicCentimeters, Weight.Kilograms, 394, Volume.CubicInches, Weight.Pounds);

			#region Costs Setup

			var costProvider = Factory.NewWithValidTestData<OrgHeader>();
			costProvider.OH_IsCreditor = true;

			var cost = Helper.NewCosting(costProvider);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 3;

			#endregion

			#region Rates Setup

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("TBK1", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 4;

			#endregion

			#region Create shipment

			var shipment = Factory.NewWithValidTestData<BillOfLading>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_ActualVolume = 5;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;
			shipment.ConsigneePK = consignee.PK;

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false));

			var jobHeader = new Job.Loader(shipment).TryCreate();
			jobHeader.JH_GE = department.PK;
			Factory.Save();

			#endregion

			#region Create booking consolidation with bookings

			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "JS";

			var booking1 = bookingConsolidation.Bookings.AddNew();
			booking1.KM_KT_NKBookingTemplate = "EFPU";
			booking1.KM_RatingFreightMode = "LSE";
			booking1.KM_JobID = "TM00000001";
			booking1.Address.OrganisationPK = costProvider.PK;

			var fromInstruction1 = booking1.Instructions.AddNew();
			fromInstruction1.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction1.KN_IsLooseRateable = true;
			fromInstruction1.Address.OrganisationPK = localClient.PK;

			var toInstruction1 = booking1.Instructions.AddNew();
			toInstruction1.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction1.KN_IsLooseRateable = true;
			toInstruction1.Address.OrganisationPK = consignee.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box1 = CreatePackage(booking1, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet1 = CreatePackage(booking1, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton1 = CreatePackage(booking1, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction1, box1, pallet1, carton1);
			CreateInstructionPkgDivots(toInstruction1, box1, pallet1, carton1);

			var booking2 = bookingConsolidation.Bookings.AddNew();
			booking2.KM_KT_NKBookingTemplate = "EFPU";
			booking2.KM_RatingFreightMode = "LSE";
			booking2.KM_JobID = "TM00000002";
			booking2.Address.OrganisationPK = costProvider.PK;

			var fromInstruction2 = booking2.Instructions.AddNew();
			fromInstruction2.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction2.KN_IsLooseRateable = true;
			fromInstruction2.Address.OrganisationPK = localClient.PK;

			var toInstruction2 = booking2.Instructions.AddNew();
			toInstruction2.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction2.KN_IsLooseRateable = true;
			toInstruction2.Address.OrganisationPK = consignee.PK;

			var box2 = CreatePackage(booking2, 6, PkgUnit.Box, commodity, 60m, Weight.Kilograms, 2m, Volume.CubicMetres);
			var pallet2 = CreatePackage(booking2, 46, PkgUnit.Pallet, commodity, 40m, Weight.Kilograms, 0.30m, Volume.CubicMetres);
			var carton2 = CreatePackage(booking2, 10, PkgUnit.Carton, commodity, 100m, Weight.Kilograms, 0.60m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction2, box2, pallet2, carton2);
			CreateInstructionPkgDivots(toInstruction2, box2, pallet2, carton2);

			#endregion

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSCostAmt = 1450m,
							JR_Desc = "TBCHARGE {TM00000002}",
							CostReferenceNumber = "TM00000002",
						},
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSCostAmt = 725m,
							JR_Desc = "TBCHARGE {TM00000001}",
							CostReferenceNumber = "TM00000001",
						}
				};

			AutorateAndAssert("Should set proper description for costs", expected, shipment, localClient, autorateRevenue: false);

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSSellAmt = 1933.33m,
							JR_Desc = "TBCHARGE {TM00000002}",
							CostReferenceNumber = "TM00000002",
						},
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSSellAmt = 966.67m,
							JR_Desc = "TBCHARGE {TM00000001}",
							CostReferenceNumber = "TM00000001",
						}
				};

			AutorateAndAssert("Should set proper description for revenue", expected, shipment, localClient, autorateCosts: false);
		}

		#endregion

		#region TransportBooking - Rate By Distance

		public void TestTransportBooking_Distance_Loose()
		{
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew();
			consignorPickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			SetAddress(consignorPickupAddress, "389 Crown St", "Surry Hills", "2010", "NSW", "AUSYD");

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetAddress(consignee.MainAddress, "65 Katoomba St", "Katoomba", "2780", "NSW", "AUSYD");

			var clientRate = Helper.NewClientRate(consignor);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "", "TBK1", 5m, Length.Kilometres);

			var booking = CreateBooking
			(
				Factory,
				Factory.NewWithValidTestData<OrgHeader>().PK,
				RatingFreightModes.Codes.Loose,
				consignor.PK,
				consignee.PK,
				packageUnit: PkgUnit.Box,
				packageWeight: 30m,
				packageWeightUQ: Weight.Pounds
			);

			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { RevenueCalculationDescription = "TBK1: 85.202 Kilometer(s) @ AUD 5.00/Kilometer", ChargeCode = "TBK1", JR_OSSellAmt = 426.01m, } },
				booking,
				consignor,
				autorateCosts: false
			);
		}

		public void TestTransportBooking_Distance_Container()
		{
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew();
			consignorPickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			SetAddress(consignorPickupAddress, "389 Crown St", "Surry Hills", "2010", "NSW", "AUSYD");

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetAddress(consignee.MainAddress, "65 Katoomba St", "Katoomba", "2780", "NSW", "AUSYD");

			var clientRate = Helper.NewClientRate(consignor);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "", "TBK1", 5m, Length.Kilometres);

			var booking = CreateBooking
			(
				Factory,
				Factory.NewWithValidTestData<OrgHeader>().PK,
				RatingFreightModes.Codes.Containerised,
				consignor.PK,
				consignee.PK,
				packageUnit: PkgUnit.Container,
				packageWeight: 30m,
				packageWeightUQ: Weight.Pounds
			);

			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { RevenueCalculationDescription = "TBK1: 85.202 Kilometer(s) @ AUD 5.00/Kilometer", ChargeCode = "TBK1", JR_OSSellAmt = 426.01m, } },
				booking,
				consignor,
				autorateCosts: false
			);
		}

		public void TestTransportBooking_Distance_Both()
		{
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK2", "Transport Booking 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK3", "Transport Booking 3", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK4", "Transport Booking 4", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew();
			consignorPickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			SetAddress(consignorPickupAddress, "389 Crown St", "Surry Hills", "2010", "NSW", "AUSYD");

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetAddress(consignee.MainAddress, "65 Katoomba St", "Katoomba", "2780", "NSW", "AUSYD");

			var clientRate = Helper.NewClientRate(consignor);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "", "TBK1", 5m, Length.Kilometres);
			rateEntry.AddRateLine("TBK2", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 10m;
			rateEntry.AddRateLine("TBK3", UnitCalculator.Code, PkgUnit.Box).GetCalculator<UnitCalculator>().PerUnit = 15m;
			rateEntry.AddRateLine("TBK4", UnitCalculator.Code, Weight.Kilograms).GetCalculator<UnitCalculator>().PerUnit = 20m;

			var booking = CreateBooking
			(
				Factory,
				Factory.NewWithValidTestData<OrgHeader>().PK,
				RatingFreightModes.Codes.Both,
				consignor.PK,
				consignee.PK,
				packageUnit: PkgUnit.Container,
				packageWeight: 100m,
				packageWeightUQ: Weight.Kilograms
			);

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "BBB";
			var package = CreatePackage(booking, 1, PkgUnit.Box, commodity, 200m, Weight.Kilograms, 2000m, Volume.CubicMetres);
			CreateInstructionPkgDivots(booking.Instructions[0], package);
			CreateInstructionPkgDivots(booking.Instructions[1], package);

			TestHelper.SetChargeableFactorRegistry(TransportBookingChargeableFactorRegistryItem, 333, Weight.Kilograms, Volume.CubicMetres, 100, Weight.Pounds, Volume.CubicInches);

			AutorateAndAssert
			(
				expected: new[]
				{
					new AssertionCharge { RevenueCalculationDescription = "TBK1: 85.202 Kilometer(s) @ AUD 5.00/Kilometer", ChargeCode = "TBK1", JR_OSSellAmt = 426.01m, },
					new AssertionCharge { RevenueCalculationDescription = "TBK2: 1 Container(s) @ AUD 10.00/Container", ChargeCode = "TBK2", JR_OSSellAmt = 10m, },
					new AssertionCharge { RevenueCalculationDescription = "TBK3: 1 Box(s) @ AUD 15.00/Box", ChargeCode = "TBK3", JR_OSSellAmt = 15m, },
					new AssertionCharge { RevenueCalculationDescription = "TBK4: 300 Kilogram(s) @ AUD 20.00/KG", ChargeCode = "TBK4", JR_OSSellAmt = 6000.00m, }, // Weight from Box and Container because Container-Package has 0 volume
					new AssertionCharge { RevenueCalculationDescription = "TBK4: 666000 Kilogram(s) @ AUD 20.00/KG", ChargeCode = "TBK4", JR_OSSellAmt = 13320000, } // Box-Package Volume i.e. 2000 M3
				},
				booking,
				consignor,
				autorateCosts: false
			);
		}

		public void TestTransportBooking_Container()
		{
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew();
			consignorPickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			SetAddress(consignorPickupAddress, "389 Crown St", "Surry Hills", "2010", "NSW", "AUSYD");

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetAddress(consignee.MainAddress, "65 Katoomba St", "Katoomba", "2780", "NSW", "AUSYD");

			var clientRate = Helper.NewClientRate(consignor);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "", "TBK1", 5m, QuantityUnit.CN);

			var booking = CreateBooking
			(
				Factory,
				Factory.NewWithValidTestData<OrgHeader>().PK,
				RatingFreightModes.Codes.Containerised,
				consignor.PK,
				consignee.PK,
				packageUnit: PkgUnit.Container,
				packageWeight: 30m,
				packageWeightUQ: Weight.Pounds
			);

			AutorateAndAssert
			(
				expected: new[] { new AssertionCharge { RevenueCalculationDescription = "TBK1: 1 Container(s) @ AUD 5.00/Container", ChargeCode = "TBK1", JR_OSSellAmt = 5.00m, } },
				booking,
				consignor,
				autorateCosts: false
			);
		}

		static void SetAddress(OrgAddress orgAddress, string address1, string city, string postcode, string state, string portCode)
		{
			orgAddress.OA_Address1 = address1;
			orgAddress.OA_City = city;
			orgAddress.OA_PostCode = postcode;
			orgAddress.OA_State = state;
			orgAddress.OA_RL_NKRelatedPortCode = portCode;
		}

		static DtbBooking CreateBooking(BusinessObjectFactory factory, ZGuid bookingAddressOrganizationPK, string freightMode, ZGuid consignorPK, ZGuid consigneePK, string packageUnit, decimal packageWeight, string packageWeightUQ, decimal packageVolume = 0m, string packageVolumeUQ = "M3", ZGuid? shipmentPK = null)
		{
			var consol = factory.New<DtbBookingConsolidation>();

			if (shipmentPK != null)
			{
				consol.KB_ParentID = shipmentPK.Value;
				consol.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}

			var booking = consol.Bookings.AddNew();
			booking.Address.OrganisationPK = bookingAddressOrganizationPK;
			booking.KM_RatingFreightMode = freightMode;

			factory.Save();

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.Address.OrganisationPK = consignorPK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.Address.OrganisationPK = consigneePK;

			if (freightMode == RatingFreightModes.Codes.Containerised)
			{
				fromInstruction.KN_IsContainerRateable = true;
				toInstruction.KN_IsContainerRateable = true;
			}
			else if (freightMode == RatingFreightModes.Codes.Loose)
			{
				fromInstruction.KN_IsLooseRateable = true;
				toInstruction.KN_IsLooseRateable = true;
			}
			else if (freightMode == RatingFreightModes.Codes.Both)
			{
				fromInstruction.KN_IsContainerRateable = true;
				toInstruction.KN_IsContainerRateable = true;
				fromInstruction.KN_IsLooseRateable = true;
				toInstruction.KN_IsLooseRateable = true;
			}
			else
			{
				throw new NotImplementedException();
			}

			var commodity = factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var package = CreatePackage(booking, 1, packageUnit, commodity, packageWeight, packageWeightUQ, packageVolume, packageVolumeUQ);
			CreateInstructionPkgDivots(fromInstruction, package);
			CreateInstructionPkgDivots(toInstruction, package);

			return booking;
		}

		#endregion

		#region RateTransportBookingsByPackType

		public void TestRateTransportBookingsByPackType()
		{
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK2", "Transport Booking 2", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK3", "Transport Booking 3", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			cnr.OH_RL_NKClosestPort = "AUBNE";

			var cne = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var line1 = entry.AddRateLine("TBK1", UnitCalculator.Code, PkgUnit.Box);
			line1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var line2 = entry.AddRateLine("TBK2", CombinedCalculator.Code, PkgUnit.Pallet);
			var line2Calc = line2.GetCalculator<CombinedCalculator>();
			line2Calc["-10"] = (ZDecimal)65m;
			line2Calc["+10"] = (ZDecimal)70m;
			line2Calc["+20"] = (ZDecimal)80m;
			line2Calc["+30"] = (ZDecimal)90m;

			var line3 = entry.AddRateLine("TBK3", CombinedCalculator.Code, PkgUnit.Carton);
			var line3Calc = line3.GetCalculator<CombinedCalculator>();
			line3Calc["-5"] = (ZDecimal)70m;
			line3Calc["+5"] = (ZDecimal)80m;
			line3Calc["+15"] = (ZDecimal)90m;
			line3Calc["+45"] = (ZDecimal)90m;
			line3.RateLineItems[0].TM_BreakWeightVolume = QuantityUnit.KG;

			var consol = Factory.New<DtbBookingConsolidation>();
			var booking = consol.Bookings.AddNew();
			booking.Address.OrganisationPK = transportCo.PK;
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;

			Factory.Save();

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = cnr.PK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = cne.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box = CreatePackage(booking, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet = CreatePackage(booking, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton = CreatePackage(booking, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction, box, pallet, carton);
			CreateInstructionPkgDivots(toInstruction, box, pallet, carton);

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSCostAmt = 15.00m,
						},

					new AssertionCharge
						{
							ChargeCode = "TBK2",
							JR_OSCostAmt = 1840.00m,
						},

					new AssertionCharge
						{
							ChargeCode = "TBK3",
							JR_OSCostAmt = 400.00m,
						},
				};

			AutorateAndAssert(expected, booking, cnr);
		}

		public void TestRateTransportBookingsShouldNotBringPostedChargesAgainForRevenue()
		{
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK2", "Transport Booking 2", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK3", "Transport Booking 3", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			cnr.OH_RL_NKClosestPort = "AUBNE";

			var cne = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();

			var clientRate = Helper.NewClientRate(cnr);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var line1 = entry.AddRateLine("TBK1", UnitCalculator.Code, PkgUnit.Box);
			line1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var line2 = entry.AddRateLine("TBK2", FlatCalculator.Code, PkgUnit.Box);
			line2.GetCalculator<FlatCalculator>().BaseRate = 75;

			var line3 = entry.AddRateLine("TBK3", FlatCalculator.Code, PkgUnit.Box);
			line3.GetCalculator<FlatCalculator>().BaseRate = 80;

			var consol = Factory.New<DtbBookingConsolidation>();
			var booking = consol.Bookings.AddNew();
			booking.Address.OrganisationPK = transportCo.PK;
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;

			Factory.Save();

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = cnr.PK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = cne.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box = CreatePackage(booking, 1, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet = CreatePackage(booking, 1, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton = CreatePackage(booking, 1, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction, box, pallet, carton);
			CreateInstructionPkgDivots(toInstruction, box, pallet, carton);

			using (var job = new JobHeader.Loader(booking).TryLoadOrCreate() as Job)
			{
				job.LocalChargesPK = cnr.PK;
				var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate1.AT_Code = "TAX1";
				taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate1.SetRateNumerator_ForTestOnly(5);

				var th = Factory.NewWithValidTestData<AccTransactionHeader>();
				th.AH_Ledger = "AP";
				var tl = Factory.NewWithValidTestData<AccTransactionLines>();
				tl.AL_AH = th.PK;
				tl.AL_LineType = TransactionLineTypes.Revenue;
				tl.AL_LineAmount = -200;
				tl.AL_OSAmount = -200;
				tl.AL_RX_NKTransactionCurrency = "AUD";
				tl.AL_RevRecognitionType = "IMM";

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TBK1",
						JR_OSCostAmt = 5.00m,
					},

					new AssertionCharge
					{
						ChargeCode = "TBK2",
						JR_OSCostAmt = 75.00m,
					},

					new AssertionCharge
					{
						ChargeCode = "TBK3",
						JR_OSCostAmt = 80.00m,
					},
				};

				AutorateAndAssert(expected, booking, cnr, autorateCosts: false);

				foreach (Charge jc in job.Charges)
				{
					jc.JR_AL_ARLine = tl.PK;
					jc.JR_AT_SellGSTRate = taxRate1.PK;
					Assert("Prerequisite:", jc.IsRevenuePosted);
				}

				AutorateAndAssert(expected, booking, cnr, autorateCosts: false);

				foreach (Charge jc in job.Charges)
				{
					AssertEquals("Posting Not Changed", false, jc.JR_IsCostPosted);
					AssertEquals("Posting Not Changed", true, jc.JR_IsRevenuePosted);
				}
			}
		}

		DtbBooking CreateTestContainerWithPackage(ZGuid bookingPK, ZGuid consignorPK, ZGuid consigneePK, decimal packageWeight, string packageWeightUQ)
		{
			var consol = Factory.New<DtbBookingConsolidation>();
			var booking = consol.Bookings.AddNew();
			booking.Address.OrganisationPK = bookingPK;
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Containerised;

			Factory.Save();

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsContainerRateable = true;
			fromInstruction.Address.OrganisationPK = consignorPK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsContainerRateable = true;
			toInstruction.Address.OrganisationPK = consigneePK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var container = CreatePackage(booking, 1, PkgUnit.Container, commodity, packageWeight, packageWeightUQ, 0m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction, container);
			CreateInstructionPkgDivots(toInstruction, container);

			return booking;
		}

		#endregion

		#region TestAutoRateWithCalculatorWhenUnitIsInvalid

		public void TestAutoRateWithCalculatorWhenCannotConvertInvalidUnit()
		{
			const string invalidUnit = "WW";
			const string validUnit = "KG";

			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK2", "Transport Booking 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUBNE";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var line1Calc = entry.AddRateLine("TBK1", CombinedCalculator.Code, validUnit).GetCalculator<CombinedCalculator>();
			line1Calc.AddRateLineItem(Calculator.Items.Operator.Minus, 10m, 0, 15);
			line1Calc.AddRateLineItem(Calculator.Items.Operator.Plus, 10m, 0, 10);

			var rateLineItem1 = line1Calc.RateLineItems.Cast<RateLineItem>().Single(x => x.TM_Type == "-" && x.TM_Break == 10m);
			rateLineItem1.TM_BreakWeightVolume = invalidUnit;
			entry.Unit = invalidUnit;

			var line2Calc = entry.AddRateLine("TBK2", UnitCalculator.Code, validUnit).GetCalculator<UnitCalculator>();
			line2Calc.PerUnit = 10;

			Factory.Save();

			var booking = CreateTestContainerWithPackage(transportCo.PK, consignor.PK, consignee.PK, 176.37m, Weight.Pounds);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TBK2",
					JR_OSCostAmt = 800m
				},
			};

			AutorateAndAssert(expected, booking, consignor);
			AssertAutoratingAuditLogNoteContainsLines(booking,
				"Log should contain expected line",
				"Information: RateLine Filtered TBK1-CMB-WW-Costing ZGP5LX5SQPEB	reason:	Enter a valid Units.");
		}

		#endregion

		#region Flat Amounts Works On CMB Claculator With Custom Break Units

		public void TestFlatAmountsWorksOnCMBClaculatorWithCustomBreakUnits()
		{
			var code = Helper.ChargeCodes.New("TBK1", "Transport Booking 1", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUBNE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var line = entry.AddRateLine("TBK1", CombinedCalculator.Code, PkgUnit.Carton);
			var line1Calc = line.GetCalculator<CombinedCalculator>();
			line1Calc["-10"] = (ZDecimal)15m;
			line1Calc["+10"] = (ZDecimal)10m;

			var rateLineItem1 = line1Calc.RateLineItems.Cast<RateLineItem>().Single(x => x.TM_Type == "-" && x.TM_Break == 10m);
			rateLineItem1.TM_BreakWeightVolume = "KG";

			var rateLineItem2 = line1Calc.RateLineItems.Cast<RateLineItem>().Single(x => x.TM_Type == "+" && x.TM_Break == 10m);
			rateLineItem2.TM_FlatAmount = 2m;

			var consol = Factory.New<DtbBookingConsolidation>();
			var booking = consol.Bookings.AddNew();
			booking.Address.OrganisationPK = transportCo.PK;
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;

			Factory.Save();

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = consignor.PK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = consignee.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var carton = CreatePackage(booking, 4, PkgUnit.Carton, commodity, 80m, Weight.Kilograms, 1m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction, carton);
			CreateInstructionPkgDivots(toInstruction, carton);

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSCostAmt = 42.00m,
							CostCalculationDescription = "TBK1: Base Rate AUD 2.00 + 4 Carton(s) @ AUD 10.00/Carton"
						},
				};

			AutorateAndAssert(expected, booking, consignor);
		}

		#endregion

		#region Test Autorating WhenContainer And RateLine Use Different Measure Units

		public void TestAutoratingWhenContainerAndRateLineUseDifferentMeasureUnits()
		{
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUBNE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var line1Calc = entry.AddRateLine("TBK1", CombinedCalculator.Code, "KG").GetCalculator<CombinedCalculator>();
			line1Calc["-10"] = (ZDecimal)15m;
			line1Calc["+10"] = (ZDecimal)10m;

			var rateLineItem1 = line1Calc.RateLineItems.Cast<RateLineItem>().Single(x => x.TM_Type == "-" && x.TM_Break == 10m);
			rateLineItem1.TM_BreakWeightVolume = "KG";

			Factory.Save();

			var booking = CreateTestContainerWithPackage(transportCo.PK, consignor.PK, consignee.PK, 176.37m, Weight.Pounds);

			var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSCostAmt = 800.00m,
						},
					};

			AutorateAndAssert(expected, booking, consignor);
		}

		#endregion

		#region TestChargeableForTransportBooking

		public void TestChargeableForTransportBooking()
		{
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK2", "Transport Booking 2", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK3", "Transport Booking 3", CombinedCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUBNE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var line1 = entry.AddRateLine("TBK1", UnitCalculator.Code, Weight.Kilograms);
			line1.GetCalculator<UnitCalculator>().PerUnit = 5m;
			line1.TL_Rounding = RatingRoundingTypes.Chargeable;

			var line2 = entry.AddRateLine("TBK2", UnitCalculator.Code, Weight.Pounds);
			line2.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var line3 = entry.AddRateLine("TBK3", UnitCalculator.Code, Weight.Pounds);
			line3.GetCalculator<UnitCalculator>().PerUnit = 20m;
			line3.TL_ActualPercentage = 80;

			Factory.Save();

			var booking = CreateTestContainerWithPackage(transportCo.PK, consignor.PK, consignee.PK, 176.37m, Weight.Pounds);

			AssertEquals("Precondition: Original Chargeable", 80m, booking.KM_Chargeable);
			AssertEquals("Precondition: Original Chargeable Unit", Weight.Kilograms, booking.KM_ChargeableUnit);

			booking.KM_OverrideChargeable = true;
			booking.KM_Chargeable = 25;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TBK1",
					JR_OSCostAmt = 125.00m, //(KM_Chargeable * PerUnit = 25 * 5)
				},

				new AssertionCharge
				{
					ChargeCode = "TBK2",
					JR_OSCostAmt = 1763.70m, //(CalculatedChargeableInPound * PerUnit = 176.37 * 10)
				},
				new AssertionCharge
				{
					ChargeCode = "TBK3",
					JR_OSCostAmt = 3527.40m, //(CalculatedChargeableInPound * 0.20 + TotalWeightInPound * 0.80) * PerUnit = (176.37 * 0.20 + 176.37 * 0.80) * 20
				}
			};

			AutorateAndAssert(expected, booking, consignor);
		}

		#endregion

		ChargeableFactorRegistryItem TransportBookingChargeableFactorRegistryItem
		{
			get
			{
				var transportRegistry = ObjectFactory.Get<ITransportBookingRegistryProvider>();
				return (ChargeableFactorRegistryItem)transportRegistry.TransportBookingChargeableFactor;
			}
		}

		ChargeableFactor TransportBookingChargeableFactor => TransportBookingChargeableFactorRegistryItem.Value;
	}
}
