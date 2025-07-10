using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class CommonCartageIntegrationTest : BaseRatingIntegrationTest
	{
		#region Port Transport (CommonCartage) Rating By Calculator

		#region TestCommonCartage_CMBCalculator

		public void TestCommonCartage_CMBCalculator_ByContainer()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PT", "Transport", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, CombinedCalculator.Code, QuantityUnit.CN);
			var lineCalc = line.GetCalculator<CombinedCalculator>();
			lineCalc.AddRateLineItem(Calculator.Items.Operator.Minus, 4, 1000, breakUnit: QuantityUnit.PK);
			lineCalc.AddRateLineItem(Calculator.Items.Operator.Plus, 4, 900);

			var cartage = CreateContainerisedCartage(client);

			cartage.CartageLegs[0].BookedCtgMove.EW_BookedPackCount = 2;
			cartage.CartageLegs[0].BookedCtgMove.EW_F3_NKPackType = PkgUnit.Package;

			cartage.CartageLegs[1].BookedCtgMove.EW_BookedPackCount = 0;
			cartage.CartageLegs[1].BookedCtgMove.EW_F3_NKPackType = PkgUnit.Package;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 1000m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 1000.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 0m,
					RevenueCalculationDescription = "PT: Calculation failed due to Port Transport - Weight / Volume / Packages information was not specified on this job."
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		#endregion

		#region TestCommonCartage_UnitCalculator

		public void TestCommonCartage_UnitCalculator_ByContainer()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PT", "Transport", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cartage = CreateContainerisedCartage(client);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 100.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 100.00/Container"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		public void TestCommonCartage_UnitCalculator_ByDistance()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PT", "Transport", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KM);
			line.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var pickupAddress = Helper.NewOrgHeader().MainAddress;
			pickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			pickupAddress.OA_Address1 = "1310 LYTTON ROAD";
			pickupAddress.OA_City = "HEMMANT";
			pickupAddress.OA_PostCode = "4174";
			pickupAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			pickupAddress.OA_State = "QLD";
			pickupAddress.OA_AIREquipmentNeeded = LCLAIREquipmentNeeded.Premise;
			pickupAddress.OA_FCLEquipmentNeeded = FCLEquipmentNeeded.WaitForUnpack;
			pickupAddress.OA_LCLEquipmentNeeded = LCLAIREquipmentNeeded.Premise;

			var deliveryAddress = Helper.NewOrgHeader(1).MainAddress;
			deliveryAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			deliveryAddress.OA_Address1 = "3 RARNLY DRIVE";
			deliveryAddress.OA_City = "BURLEIGH HEADS";
			deliveryAddress.OA_PostCode = "4220";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			deliveryAddress.OA_State = "QLD";
			deliveryAddress.OA_AIREquipmentNeeded = LCLAIREquipmentNeeded.Premise;
			deliveryAddress.OA_FCLEquipmentNeeded = FCLEquipmentNeeded.WaitForUnpack;
			deliveryAddress.OA_LCLEquipmentNeeded = LCLAIREquipmentNeeded.Premise;

			var cartage = CreateContainerisedCartage(client);
			cartage.FirstDocAddress.E2_OA_Address = pickupAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = deliveryAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 804.69m,
					RevenueCalculationDescription = "PT: 80.469 Kilometer(s) @ AUD 10.00/Kilometer"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 804.69m,
					RevenueCalculationDescription = "PT: 80.469 Kilometer(s) @ AUD 10.00/Kilometer"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		#region TestCommonCartage_ContainerHiddenServices

		public void TestCommonCartage_UnitCalculator_ByContainer_ContainerHiddenServices()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PTDEM", "Demurrage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cartage = CreateContainerisedCartage(client);

			var leg1 = cartage.ContainerBookedMoves[0].CartageLegs[0];
			leg1.JU_CartagePickupDemurrage = new TimeSpan(2, 30, 0);

			var leg2 = cartage.ContainerBookedMoves[1].CartageLegs[0];
			leg2.JU_CartagePickupDemurrage = new TimeSpan(1, 0, 0);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 100.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 100.00/Container"
				}
			};

			AutorateAndAssert("", expected, cartage, client, autorateCosts: false);
		}

		public void TestCommonCartage_UnitCalculator_ByServiceCount_ContainerHiddenServices()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PTDEM", "Demurrage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);
			line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cartage = CreateContainerisedCartage(client);

			var leg1 = cartage.ContainerBookedMoves[0].CartageLegs[0];
			leg1.JU_CartagePickupDemurrage = new TimeSpan(2, 30, 0);

			var leg2 = cartage.ContainerBookedMoves[1].CartageLegs[0];
			leg2.JU_CartagePickupDemurrage = new TimeSpan(1, 0, 0);

			Factory.Save();

			const string demurrageDescription = "Truck Wait Time (Penalty Type - TWT)";

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = $"1 Port Transport Charges {demurrageDescription} @ AUD 100.00/Port Transport Charges {demurrageDescription}"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = $"1 Port Transport Charges {demurrageDescription} @ AUD 100.00/Port Transport Charges {demurrageDescription}"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		public void TestCommonCartage_UnitCalculator_ByServiceDuration_ContainerHiddenServices()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PTDEM", "Demurrage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HR);
			line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cartage = CreateContainerisedCartage(client);

			var leg1 = cartage.ContainerBookedMoves[0].CartageLegs[0];
			leg1.JU_CartagePickupDemurrage = new TimeSpan(2, 30, 0);

			var leg2 = cartage.ContainerBookedMoves[1].CartageLegs[0];
			leg2.JU_CartagePickupDemurrage = new TimeSpan(1, 0, 0);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 250m,
					RevenueCalculationDescription = "2.5 Hour(s) @ AUD 100.00/Hour"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 Hour(s) @ AUD 100.00/Hour"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		#endregion

		#region TestCommonCartage_ContainerStandardServices

		public void TestCommonCartage_UnitCalculator_ByContainer_ContainerStandardServices()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PTFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cartage = CreateContainerisedCartage(client);

			var container1 = cartage.ContainerBookedMoves[0].Container;
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1m;
			service1.ES_Duration = new TimeSpan(2, 30, 0);

			var container2 = cartage.ContainerBookedMoves[1].Container;
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1m;
			service2.ES_Duration = new TimeSpan(1, 0, 0);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 100.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 100.00/Container"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		public void TestCommonCartage_UnitCalculator_ByServiceCount_ContainerStandardServices()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PTFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);
			line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cartage = CreateContainerisedCartage(client);

			var container1 = cartage.ContainerBookedMoves[0].Container;
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1m;
			service1.ES_Duration = new TimeSpan(2, 30, 0);

			var container2 = cartage.ContainerBookedMoves[1].Container;
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1m;
			service2.ES_Duration = new TimeSpan(1, 0, 0);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 Port Transport Charges Fumigation @ AUD 100.00/Port Transport Charges Fumigation"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 Port Transport Charges Fumigation @ AUD 100.00/Port Transport Charges Fumigation"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		public void TestCommonCartage_UnitCalculator_ByServiceCount_ContainerStandardServices2()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			var client = Helper.NewOrgHeader();

			#region Rates Setup

			var chargeCode = Helper.ChargeCodes.New("SSS", "Transport Cleaning", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Cleaning);

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.ALL, "AU", "");
			entry.TI_RC = RefContainer.PK;

			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);
			line.GetCalculator<UnitCalculator>().PerUnit = 1000;

			#endregion

			#region Cartage Setup

			var cartage = Helper.Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLImportToCNE;
			cartage.LocalClientAddressPK = client.MainAddress.PK;

			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_RC = RefContainer.PK;

			var containerService = container.Services.AddNew();
			containerService.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			containerService.ES_Completed = ZDateTime.Now;
			containerService.ES_ServiceCount = 5;

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 5000m,
						}
				};

			AutorateAndAssert(expected, cartage, client);
		}

		public void TestCommonCartage_UnitCalculator_ByServiceCount_LooseContainerStandardServices()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			var client = Helper.NewOrgHeader();

			#region Rates Setup

			var chargeCode = Helper.ChargeCodes.New("SSS", "Transport Cleaning", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Cleaning);

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.ALL, "AU", "");

			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);
			line.GetCalculator<UnitCalculator>().PerUnit = 1000;

			#endregion

			#region Cartage Setup

			var cartage = Helper.Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_DomesticLoosePickup;
			cartage.LocalClientAddressPK = client.MainAddress.PK;

			var looseMoveService = cartage.BookedMovesCollection[0].Services.AddNew();
			looseMoveService.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			looseMoveService.ES_Completed = ZDateTime.Now;
			looseMoveService.ES_ServiceCount = 5;

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 5000m,
						}
				};

			AutorateAndAssert(expected, cartage, rate.Header);
		}

		public void TestCommonCartage_UnitCalculator_ByServiceDuration_ContainerStandardServices()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PTFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HR);
			line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cartage = CreateContainerisedCartage(client);

			var container1 = cartage.ContainerBookedMoves[0].Container;
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1m;
			service1.ES_Duration = new TimeSpan(2, 30, 0);

			var container2 = cartage.ContainerBookedMoves[1].Container;
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1m;
			service2.ES_Duration = new TimeSpan(1, 0, 0);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 250m,
					RevenueCalculationDescription = "2.5 Hour(s) @ AUD 100.00/Hour"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 Hour(s) @ AUD 100.00/Hour"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		#endregion

		#endregion

		#region TestCommonCartage_TimeCalculator

		public void TestCommonCartage_TimeCalculator_ContainerHiddenServices()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PTDEM", "Demurrage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.SV);

			var rateLineItem1 = line.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.Minus;
			rateLineItem1.TM_Break = 2;
			rateLineItem1.TM_RelevantValue = 200;
			rateLineItem1.TM_BreakWeightVolume = QuantityUnit.HR;

			var rateLineItem2 = line.RateLineItems.AddNew();
			rateLineItem2.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem2.TM_Break = 2;
			rateLineItem2.TM_RelevantValue = 150;

			var cartage = CreateContainerisedCartage(client);

			var leg1 = cartage.ContainerBookedMoves[0].CartageLegs[0];
			leg1.JU_CartagePickupDemurrage = new TimeSpan(2, 30, 0);

			var leg2 = cartage.ContainerBookedMoves[1].CartageLegs[0];
			leg2.JU_CartagePickupDemurrage = new TimeSpan(1, 0, 0);

			Factory.Save();

			const string demurrageDescription = "Truck Wait Time (Penalty Type - TWT)";

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 475m,
					RevenueCalculationDescription = $"PTDEM: 2 Port Transport Charges {demurrageDescription} x Hour (1 Port Transport Charges {demurrageDescription} x 2 Hour(s)) @ AUD 200.00/Port Transport Charges {demurrageDescription} x Hour + 0.5 Port Transport Charges {demurrageDescription} x Hour (1 Port Transport Charges {demurrageDescription} x 0.5 Hour(s)) @ AUD 150.00/Port Transport Charges {demurrageDescription} x Hour"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 200m,
					RevenueCalculationDescription = $"PTDEM: 1 Port Transport Charges {demurrageDescription} x Hour @ AUD 200.00/Port Transport Charges {demurrageDescription} x Hour"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		public void TestCommonCartage_TimeCalculator_ContainerStandardServices()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("PTFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			entry.TI_RC = RefContainer.PK;
			var line = entry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.SV);

			var rateLineItem1 = line.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.Minus;
			rateLineItem1.TM_Break = 2;
			rateLineItem1.TM_RelevantValue = 200;
			rateLineItem1.TM_BreakWeightVolume = QuantityUnit.HR;

			var rateLineItem2 = line.RateLineItems.AddNew();
			rateLineItem2.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem2.TM_Break = 2;
			rateLineItem2.TM_RelevantValue = 150;

			var cartage = CreateContainerisedCartage(client);

			var container1 = cartage.ContainerBookedMoves[0].Container;
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1m;
			service1.ES_Duration = new TimeSpan(2, 30, 0);

			var container2 = cartage.ContainerBookedMoves[1].Container;
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1m;
			service2.ES_Duration = new TimeSpan(1, 0, 0);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 475m,
					RevenueCalculationDescription = "PTFUM: 2 Port Transport Charges Fumigation x Hour (1 Port Transport Charges Fumigation x 2 Hour(s)) @ AUD 200.00/Port Transport Charges Fumigation x Hour + 0.5 Port Transport Charges Fumigation x Hour (1 Port Transport Charges Fumigation x 0.5 Hour(s)) @ AUD 150.00/Port Transport Charges Fumigation x Hour"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 200m,
					RevenueCalculationDescription = "PTFUM: 1 Port Transport Charges Fumigation x Hour @ AUD 200.00/Port Transport Charges Fumigation x Hour"
				}
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		#endregion

		#region TestCommonCartage_PercentageCalculator

		[TestDate(2023, 12, 01)]
		public void TestCommonCartage_PercentageCalculator_WorksWithMultipleRatingAdapter()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());

			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("PT1", "Transport", FlatCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			var chargeCode2 = Helper.ChargeCodes.NewConsolChargeCode("PT2", "Surcharge", PercentageCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var client = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(client);
			rate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", startDate: new ZDate(2023, 12, 01), endDate: new ZDate(2023, 12, 31))
				.AddFlatCharge(chargeCode1.AC_Code, 1000)
				.AddPercentageCharge(chargeCode2.AC_Code, chargeCode1, 12);
			rate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", startDate: new ZDate(2024, 01, 01), endDate: new ZDate(2024, 12, 31))
				.AddFlatCharge(chargeCode1.AC_Code, 1000)
				.AddPercentageCharge(chargeCode2.AC_Code, chargeCode1, 15);

			Factory.Save();

			// Setup has 4 moves, 2 in 2023 and 2 in 2024 as well as 1 existing charge on the job.
			// There is a PT1 Flat charge $1000 for each move and a PT2 charge which is percentage of PT1 - 12% in 2023 and 15% in 2024
			//
			// Expected percentage calculator to be applied to moves as pet their dates, i.e. to 2 moves must apply 12% and to other 2 moves - 15%.
			// Also, percentage calculator must calculate an existing PT1 charge on the job.

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = "ISCC";
			cartage.LocalClientAddressPK = client.MainAddress.PK;

			var move1 = cartage.ContainerBookedMoves.AddNew();
			move1.Container.JC_RC = RefContainer.PK;
			move1.CartageLegs[0].JU_PickupTimeIn = new ZDate(2023, 12, 30);
			move1.CartageLegs[0].JU_DeliverTimeOut = new ZDate(2023, 12, 30);

			var move2 = cartage.ContainerBookedMoves.AddNew();
			move2.Container.JC_RC = RefContainer.PK;
			move2.CartageLegs[0].JU_PickupTimeIn = new ZDate(2024, 01, 30);
			move2.CartageLegs[0].JU_DeliverTimeOut = new ZDate(2024, 01, 30);

			var move3 = cartage.ContainerBookedMoves.AddNew();
			move3.Container.JC_RC = RefContainer.PK;
			move3.CartageLegs[0].JU_PickupTimeIn = new ZDate(2023, 12, 30);
			move3.CartageLegs[0].JU_DeliverTimeOut = new ZDate(2023, 12, 30);

			var move4 = cartage.ContainerBookedMoves.AddNew();
			move4.Container.JC_RC = RefContainer.PK;
			move4.CartageLegs[0].JU_PickupTimeIn = new ZDate(2024, 01, 30);
			move4.CartageLegs[0].JU_DeliverTimeOut = new ZDate(2024, 01, 30);

			Factory.Save();

			var job = Factory.NewJobForTesting<Job>();
			using (job.GetValidationSuspender())
			{
				job.JH_JobNum = cartage.JJ_ConsignmentID;
				job.JH_ParentID = cartage.PK;
				job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.PlugInData = cartage;
				job.JH_OA_LocalChargesAddr = client.MainAddress.PK;

				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode1.PK;
				charge.JR_OH_SellAccount = client.PK;
				charge.JR_RX_NKSellCurrency = CurrencyCodes.Australia;
				charge.JR_OSSellAmt = 1500m;
				charge.JR_SellRatingOverride = true;

				var expected = new[]
				{
					new AssertionCharge	//this charge has been replaced with existing JobCharge of the same charge code.
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1500m,
					},
					new AssertionCharge //Autorated charge (since couldn't overwrite overriden charge)
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1000m,
						JR_OSCostAmt = 1000m,
					},
					new AssertionCharge
					{
						ChargeCode = "PT2",
						JR_OSSellAmt = 300m,
						RevenueCalculationDescription = "PT2: 12.00% of (AUD 2500.00 (PT1 1000.00 + PT1* 1500.00))"
					},
					new AssertionCharge
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1000m,
						RevenueCalculationDescription = "PT1: Base Rate AUD 1000.00"
					},
					new AssertionCharge
					{
						ChargeCode = "PT2",
						JR_OSSellAmt = 120m,
						RevenueCalculationDescription = "PT2: 12.00% of (AUD 1000.00 (PT1))"
					},
					new AssertionCharge
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1000m,
						RevenueCalculationDescription = "PT1: Base Rate AUD 1000.00"
					},
					new AssertionCharge
					{
						ChargeCode = "PT2",
						JR_OSSellAmt = 375m,
						RevenueCalculationDescription = "PT2: 15.00% of (AUD 2500.00 (PT1 1000.00 + PT1* 1500.00))"
					},
					new AssertionCharge
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1000m,
						RevenueCalculationDescription = "PT1: Base Rate AUD 1000.00"
					},
					new AssertionCharge
					{
						ChargeCode = "PT2",
						JR_OSSellAmt = 150m,
						RevenueCalculationDescription = "PT2: 15.00% of (AUD 1000.00 (PT1))"
					},
				};

				AutorateAndAssert(expected, cartage, client, null, job);
			}
		}

		public void TestCommonCartage_PercentageCalculator_WorksWithMultipleRatingAdapter2()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());

			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("PT1", "Transport", FlatCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			var chargeCode2 = Helper.ChargeCodes.NewConsolChargeCode("PT2", "Surcharge", PercentageCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var client = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("TRN", "ALL", "AU", "");

			var line1 = entry.AddRateLine(chargeCode1, FlatCalculator.Code);
			line1.GetCalculator<FlatCalculator>().BaseRate = 1000;

			var line2 = entry.AddRateLine(chargeCode2, PercentageCalculator.Code);
			line2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = chargeCode1.PK;
			line2.GetCalculator<PercentageCalculator>().Percent = 5m;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = "ISCC";
			cartage.LocalClientAddressPK = client.MainAddress.PK;

			var c1 = cartage.ContainerBookedMoves.AddNew().Container;
			var c2 = cartage.ContainerBookedMoves.AddNew().Container;

			c1.JC_RC = RefContainer.PK;
			c2.JC_RC = RefContainer.PK;

			Factory.Save();

			var job = Factory.NewJobForTesting<Job>();
			using (job.GetValidationSuspender())
			{
				job.JH_JobNum = cartage.JJ_ConsignmentID;
				job.JH_ParentID = cartage.PK;
				job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.PlugInData = cartage;
				job.JH_OA_LocalChargesAddr = client.MainAddress.PK;

				var charge1 = job.Charges.AddNew();
				charge1.JR_SellRatingOverride = true;
				charge1.JR_AC = chargeCode1.PK;
				charge1.JR_OH_SellAccount = client.PK;
				charge1.JR_RX_NKSellCurrency = CurrencyCodes.Australia;
				charge1.JR_OSSellAmt = 1000m;

				var charge2 = job.Charges.AddNew();
				charge2.JR_SellRatingOverride = true;
				charge2.JR_AC = chargeCode2.PK;
				charge2.JR_OH_SellAccount = client.PK;
				charge2.JR_RX_NKSellCurrency = CurrencyCodes.Australia;
				charge2.JR_OSSellAmt = 50m;

				var charge3 = job.Charges.AddNew();
				charge3.JR_SellRatingOverride = true;
				charge3.JR_AC = chargeCode1.PK;
				charge3.JR_OH_SellAccount = client.PK;
				charge3.JR_RX_NKSellCurrency = CurrencyCodes.Australia;
				charge3.JR_OSSellAmt = 1000m;

				var charge4 = job.Charges.AddNew();
				charge4.JR_SellRatingOverride = true;
				charge4.JR_AC = chargeCode2.PK;
				charge4.JR_OH_SellAccount = client.PK;
				charge4.JR_RX_NKSellCurrency = CurrencyCodes.Australia;
				charge4.JR_OSSellAmt = 50m;

				var expected = new[]
				{
					//Newly autorated charges
					new AssertionCharge
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1000m,
						RevenueCalculationDescription = "PT1: Base Rate AUD 1000.00"
					},
					new AssertionCharge
					{
						ChargeCode = "PT2",
						JR_OSSellAmt = 150m,
						RevenueCalculationDescription = "PT2: 5.00% of (AUD 3000.00 (PT1* 1000.00 + PT1* 1000.00 + PT1 1000.00))"
					},
					new AssertionCharge
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1000m,
						RevenueCalculationDescription = "PT1: Base Rate AUD 1000.00"
					},
					new AssertionCharge
					{
						ChargeCode = "PT2",
						JR_OSSellAmt = 50m,
						RevenueCalculationDescription = "PT2: 5.00% of (AUD 1000.00 (PT1))"
					},

					//Existing charges
					new AssertionCharge
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1000m,
						RevenueCalculationDescription = ""
					},
					new AssertionCharge
					{
						ChargeCode = "PT2",
						JR_OSSellAmt = 50m,
						RevenueCalculationDescription = ""
					},
					new AssertionCharge
					{
						ChargeCode = "PT1",
						JR_OSSellAmt = 1000m,
						RevenueCalculationDescription = ""
					},
					new AssertionCharge
					{
						ChargeCode = "PT2",
						JR_OSSellAmt = 50m,
						RevenueCalculationDescription = ""
					}
				};

				AutorateAndAssert(expected, cartage, client, null, job);
			}
		}

		#endregion

		#region TestCommonCartage_CartageZoneCalculator

		[TestDate(2016, 06, 06)]
		public void TestCommonCartage_CartageZoneCalculator_CalculatesDistanceFromAddresses()
		{
			#region Setup organizations

			var supplier = Helper.NewOrgHeader();

			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "AUBNE";

			consignor.MainAddress.OA_Address1 = "1310 LYTTON ROAD";
			consignor.MainAddress.OA_Address2 = "HEMMANT QLD";
			consignor.MainAddress.OA_AIREquipmentNeeded = "PSL";
			consignor.MainAddress.OA_City = "AUBNE";
			consignor.MainAddress.OA_Code = "PST: 1310 LYTTON ROAD";
			consignor.MainAddress.OA_FCLEquipmentNeeded = "WUP";
			consignor.MainAddress.OA_LCLEquipmentNeeded = "PSL";
			consignor.MainAddress.OA_PostCode = "4174";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			consignor.MainAddress.OA_State = "QLD";

			var consignorAddress1 = consignor.Addresses.AddNew();
			consignorAddress1.AddAddressType(OrgAddressType.PickupAndDelivery);
			consignorAddress1.OA_Address1 = "12 BOURKE STREET";
			consignorAddress1.OA_AIREquipmentNeeded = "PSL";
			consignorAddress1.OA_City = "ALEXANDRIA";
			consignorAddress1.OA_Code = "12 BOURKE STREET";
			consignorAddress1.OA_FCLEquipmentNeeded = "WUP";
			consignorAddress1.OA_LCLEquipmentNeeded = "PSL";
			consignorAddress1.OA_PostCode = "9999";
			consignorAddress1.OA_RL_NKRelatedPortCode = "";
			consignorAddress1.OA_State = "NSW";

			var consignorAddress2 = consignor.Addresses.AddNew();
			consignorAddress2.AddAddressType(OrgAddressType.PickupAndDelivery);
			consignorAddress2.OA_Address1 = "1310 LYTTON ROAD";
			consignorAddress2.OA_AIREquipmentNeeded = "PSL";
			consignorAddress2.OA_City = "HEMMANT";
			consignorAddress2.OA_Code = "Pickup and Delivery Addre";
			consignorAddress2.OA_FCLEquipmentNeeded = "WUP";
			consignorAddress2.OA_LCLEquipmentNeeded = "PSL";
			consignorAddress2.OA_PostCode = "2000";
			consignorAddress2.OA_RL_NKRelatedPortCode = "";
			consignorAddress2.OA_State = "QLD";

			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignee = true;
			consignee.OH_RL_NKClosestPort = "AUBNE";

			consignee.MainAddress.OA_Address1 = "3 RARNLY DRIVE";
			consignee.MainAddress.OA_Address2 = "BURLEIGH HEADS, QLD";
			consignee.MainAddress.OA_AIREquipmentNeeded = "PSL";
			consignee.MainAddress.OA_City = "AUBNE";
			consignee.MainAddress.OA_Code = "PST: 3 RARNLY DRIVE";
			consignee.MainAddress.OA_FCLEquipmentNeeded = "WUP";
			consignee.MainAddress.OA_LCLEquipmentNeeded = "PSL";
			consignee.MainAddress.OA_PostCode = "4220";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "";
			consignee.MainAddress.OA_State = "QLD";

			var consigneeAddress1 = consignee.Addresses.AddNew();
			consigneeAddress1.AddAddressType(OrgAddressType.PickupAndDelivery);
			consigneeAddress1.OA_AccessPoint = "";
			consigneeAddress1.OA_Address1 = "3 RARNLY DRIVE";
			consigneeAddress1.OA_Address2 = "BURLEIGH";
			consigneeAddress1.OA_AIREquipmentNeeded = "PSL";
			consigneeAddress1.OA_City = "HEADS";
			consigneeAddress1.OA_Code = "Pickup and Delivery Addre";
			consigneeAddress1.OA_FCLEquipmentNeeded = "WUP";
			consigneeAddress1.OA_LCLEquipmentNeeded = "PSL";
			consigneeAddress1.OA_PostCode = "2060";
			consigneeAddress1.OA_RL_NKRelatedPortCode = "AUBNE";
			consigneeAddress1.OA_State = "QLD";

			#endregion

			#region Setup zones

			var postCode1 = Helper.CreateRefPostCode("2000");
			var postCode2 = Helper.CreateRefPostCode("2999");

			var cityTown1 = Factory.New<RefCityTown>();
			cityTown1.R9_InternationalName = "Alexandria";
			cityTown1.R9_RN_NKCountry = CountryCodes.Australia;
			cityTown1.PostCodes.AddNew().RK_CityTownPostCode = "9999";

			var cityTown2 = Factory.New<RefCityTown>();
			cityTown2.R9_InternationalName = "Mascot";
			cityTown2.R9_RN_NKCountry = CountryCodes.Australia;

			var transportZoneSet = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia);
			var zone0 = transportZoneSet.CreateRateTransportZoneForTest("P2000-P2999");
			zone0.CreateRateTransportZoneItemForTest(postCode1, postCode2);

			var zone1 = transportZoneSet.CreateRateTransportZoneForTest("D50-D100");
			zone1.CreateRateTransportZoneItemForTest(50, 100);

			var airportZone = transportZoneSet.CreateRateTransportZoneForTest("Airport");
			airportZone.CreateRateTransportZoneItemForTest(cityTown1);
			airportZone.CreateRateTransportZoneItemForTest(cityTown2);

			#endregion

			#region Setup rates

			var chargeCode = Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			Factory.Save();

			var tariff = Helper.NewCompanyTariff();

			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			entry1.TI_OH_Supplier = supplier.PK;
			entry1.TI_RS_NKServiceLevel_NI = "D2D";

			var line1 = entry1.AddRateLine(chargeCode, CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			line1.Calculator.EquipmentType = "PSL";
			line1.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 10m, ZGuid.Empty);
			line1.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 50m, transportZoneSet.Zones[2].PK);
			line1.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 30m, transportZoneSet.Zones[1].PK);
			line1.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 20m, transportZoneSet.Zones[0].PK);

			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			entry2.TI_OH_Supplier = supplier.PK;

			var line2 = entry2.AddRateLine(chargeCode, CartageZoneDistanceCalculator.Code, QuantityUnit.KM);
			line2.Calculator.EquipmentType = "PSL";
			line2.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 10m, ZGuid.Empty);
			line2.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 50m, transportZoneSet.Zones[2].PK);
			line2.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 30m, transportZoneSet.Zones[1].PK);

			tariff.Factory.Save();

			#endregion

			#region Setup cartage types and cartages

			CreateCartageType("LRLA", true);
			CreateCartageType("LRLB", false);

			var cartage1 = CreateCartage("T00001001", "LRLA", consignorAddress2.PK, consigneeAddress1.PK, supplier.PK);
			cartage1.LooseBookedMoves[0].EW_Distance = 89.328m;

			var cartage2 = CreateCartage("T00001002", "LRLA", consignorAddress2.PK, consigneeAddress1.PK, supplier.PK);

			var cartage3 = CreateCartage("T00001003", "LRLA", consignorAddress1.PK, consigneeAddress1.PK, supplier.PK);
			cartage3.JJ_RS_NKServiceLevel = "D2D";

			var cartage4 = CreateCartage("T00001004", "LRLB", consigneeAddress1.PK, consignorAddress1.PK, supplier.PK);
			cartage4.JJ_RS_NKServiceLevel = "D2D";

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CART",
							JR_OSSellAmt = 2679.84m,
						}
				};

			// First log is from FindAndLogCartageZone()
			// Second is from DistanceChargeableCalculationStrategy
			var expectedLogLines = new[] {
@"Information: Matched 'D50-D100' for RateLine CART-CTZ-KM-Base Company Tariff
	- Pickup Distance: 89.328
	- 'D50-D100' matched by distance (89.328)
Information: CART-CTZ-KM-Base Company Tariff 89.328 KM
	- Pickup Distance: 89.328" };

			AutorateAndAssert(expected, cartage1, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(cartage1, "Log should contain expected lines", expectedLogLines);

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CART",
							JR_OSSellAmt = 10.0m,
						}
				};

			expectedLogLines = new[] {
@"Information: Matched 'Standard' for RateLine CART-CTZ-KM-Base Company Tariff
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: 3.23 (Consignor Pickup Address to Consignee Delivery Address)
	- No transport zone could be matched by distance (3.23)
	- 'P2000-P2999' transport zone matched by Consignee fallback
	- 'P2000-P2999' transport zone cannot be used as there are no rates set on the calculator
	- 'Standard' fall back used
Information: CART-CTZ-KM-Base Company Tariff 3.23 KM
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: 3.23 (Consignor Pickup Address to Consignee Delivery Address)" };

			AutorateAndAssert(expected, cartage2, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(cartage2, "Log should contain calculation description", expectedLogLines);

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CART",
							JR_OSSellAmt = 2400.0m,
						}
				};

			expectedLogLines = new string[] { "Information: RatingHeader Found Base Company Tariff Entries: 2",
"Information: RateLine Found CART-CTZ-KG-Base Company Tariff",
"Information: RateLine Found CART-CTZ-KM-Base Company Tariff",
"Information: RateLine Filtered CART-CTZ-KM-Base Company Tariff	reason:	overridden by CART-CTZ-KG-Base Company Tariff by TI_RS_NKServiceLevel_NI comparer",
@"Information: Matched 'P2000-P2999' for RateLine CART-CTZ-KG-Base Company Tariff
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to Consignee Delivery Address)
	- No transport zone could be matched by distance (0)
	- 'P2000-P2999' transport zone matched by Consignee fallback" };

			AutorateAndAssert(expected, cartage3, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(cartage3, "Log should contain expected lines", expectedLogLines);

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CART",
							JR_OSSellAmt = 6000.0m,
							RevenueCalculationDescription = "120 Kilogram(s) @ AUD 50.00/KG"
						}
				};

			expectedLogLines = new[] { "Information: RatingHeader Found Base Company Tariff Entries: 2",
"Information: RateLine Found CART-CTZ-KG-Base Company Tariff",
"Information: RateLine Found CART-CTZ-KM-Base Company Tariff",
"Information: RateLine Filtered CART-CTZ-KM-Base Company Tariff	reason:	overridden by CART-CTZ-KG-Base Company Tariff by TI_RS_NKServiceLevel_NI comparer",
@"Information: Matched 'Airport' for RateLine CART-CTZ-KG-Base Company Tariff
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to Consignee Delivery Address)
	- No transport zone could be matched by distance (0)
	- 'Airport' transport zone matched by Consignor fallback" };

			AutorateAndAssert(expected, cartage4, consignee, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(cartage4, "Log should contain expected lines", expectedLogLines);
		}

		#region Cost/CompanyTariff Based Calculator - Equipment Type

		[TestDate(2000, 07, 01)]
		public void TestAutorate_CSTEquipmentType()
		{
			var chargeCode = Helper.ChargeCodes.New("TCART", "Transport Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			CreateCartageType("LRLA", true);

			Consignor.OH_RL_NKClosestPort = "AUBTB";
			Consignee.OH_RL_NKClosestPort = "AUSYD";

			var costing = Helper.NewCosting(TransportProvider1);
			var costingRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "", removeLines: true);
			var costingRateLine = costingRateEntry.AddRateLine(chargeCode, CartageCalculator.Code, "KG");
			costingRateLine.Calculator.EquipmentType = LCLAIREquipmentNeeded.Premise;
			costingRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var clientRate = Helper.NewClientRate(Consignee);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "", removeLines: true);
			var clientRateLine = clientRateEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode, "KG");
			var clientRateCalculator = clientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			clientRateCalculator.PerUnitPercent = 20m;

			var cartage = CreateCartage("T100216", "LRLA", Consignor.MainAddress.PK, Consignee.MainAddress.PK, TransportProvider1.PK, dropMode: LCLAIREquipmentNeeded.Premise, cartageWeight: 120m, cartageWeightUQ: QuantityUnit.KG, cartageVolume: 0.001m, cartageVolumeUQ: QuantityUnit.M3, cartageMoveWeight: 100m, cartageMoveVolume: 0.001m);

			Factory.Save();
			AssertNullOrEmpty("Precondition: CST calculator EquipmentType is empty", clientRateCalculator.EquipmentType);
			AutorateAndAssert
			(
				"GIVEN ClientRate with CST empty EquipmentType and Cost with CTG PSL EquipmentType WHEN autorate job with PSL DropMode THEN should return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 4000m, CostCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 40.00/KG", JR_OSSellAmt = 4800m, RevenueCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 48.00/KG" } },
				cartage,
				Consignee
			);

			clientRateCalculator.EquipmentType = EquipmentNeeded.Any;
			Factory.Save();
			AutorateAndAssert
			(
				"GIVEN ClientRate with CST ANY EquipmentType and Cost with CTG PSL EquipmentType WHEN autorate job with PSL DropMode THEN should return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 4000m, CostCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 40.00/KG", JR_OSSellAmt = 4800m, RevenueCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 48.00/KG" } },
				cartage,
				Consignee
			);

			clientRateCalculator.EquipmentType = LCLAIREquipmentNeeded.Premise;
			Factory.Save();
			AutorateAndAssert
			(
				"GIVEN ClientRate with CST PSL EquipmentType and Cost with CTG PSL EquipmentType WHEN autorate job with PSL Dropmode THEN should return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 4000m, CostCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 40.00/KG", JR_OSSellAmt = 4800m, RevenueCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 48.00/KG" } },
				cartage,
				Consignee
			);

			clientRateCalculator.EquipmentType = LCLAIREquipmentNeeded.Haulier;
			Factory.Save();
			AutorateAndAssert
			(
				"GIVEN ClientRate with CST HSL EquipmentType and Cost with CTG PSL EquipmentType WHEN autorate job with PSL DropMode THEN should not return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 4000m, CostCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 40.00/KG", JR_OSSellAmt = 4000m, RevenueCalculationDescription = "" } },
				cartage,
				Consignee
			);

			clientRateCalculator.EquipmentType = EquipmentNeeded.Any;
			costingRateLine.Calculator.EquipmentType = LCLAIREquipmentNeeded.Haulier;
			Factory.Save();
			AutorateAndAssert
			(
				"GIVEN ClientRate with CST ANY EquipmentType and Cost with CTG HSL EquipmentType WHEN autorate job with PSL DropMode THEN should not return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 0m, CostCalculationDescription = "", JR_OSSellAmt = 0m, RevenueCalculationDescription = "TCART: Calculation failed due to charge code is using the Cost Based Calculator, however there are conflicting or no costing rates found." } },
				cartage,
				Consignee
			);
		}

		[TestDate(2000, 07, 01)]
		public void TestAutorate_CTBEquipmentType()
		{
			var chargeCode = Helper.ChargeCodes.New("TCART", "Transport Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			CreateCartageType("LRLA", true);
			Factory.Save();

			Consignor.OH_RL_NKClosestPort = "AUBTB";
			Consignee.OH_RL_NKClosestPort = "AUSYD";
			Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var tariff = Helper.NewCompanyTariff();
			var tarifRateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "", removeLines: true);
			var tarifRateLine = tarifRateEntry.AddRateLine(chargeCode, CartageCalculator.Code, "KG");
			tarifRateLine.Calculator.EquipmentType = LCLAIREquipmentNeeded.Premise;
			tarifRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;
			tariff.Factory.Save();

			var clientRate = Helper.NewClientRate(Consignee);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "", removeLines: true);
			var clientRateLine = clientRateEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, "KG");
			var clientRateCalculator = clientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			clientRateCalculator.PerUnitPercent = 20m;

			var cartage = CreateCartage("T100216", "LRLA", Consignor.MainAddress.PK, Consignee.MainAddress.PK, TransportProvider1.PK, cartageWeight: 120m, cartageWeightUQ: QuantityUnit.KG, cartageVolume: 0.001m, cartageVolumeUQ: QuantityUnit.M3, cartageMoveWeight: 100m, cartageMoveVolume: 0.001m);

			Factory.Save();

			AssertNullOrEmpty("Precondition: CTB calculator EquipmentType is empty", clientRateCalculator.EquipmentType);
			AutorateAndAssert
			(
				"GIVEN ClientRate with CTB empty EquipmentType and CompanyTariff with CTG PSL EquipmentType WHEN autorate job with PSL DropMode THEN should return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 4800m, CostCalculationDescription = "", JR_OSSellAmt = 4800m, RevenueCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 48.00/KG" } },
				cartage,
				Consignee,
				autorateCosts: false
			);

			clientRateCalculator.EquipmentType = EquipmentNeeded.Any;
			Factory.Save();
			AutorateAndAssert
			(
				"GIVEN ClientRate with CTB ANY EquipmentType and CompanyTariff with CTG PSL EquipmentType WHEN autorate job with PSL DropMode THEN should return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 4800m, CostCalculationDescription = "", JR_OSSellAmt = 4800m, RevenueCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 48.00/KG" } },
				cartage,
				Consignee
			);

			clientRateCalculator.EquipmentType = LCLAIREquipmentNeeded.Premise;
			Factory.Save();
			AutorateAndAssert
			(
				"GIVEN ClientRate with CTB PSL EquipmentType and CompanyTariff with CTG PSL EquipmentType WHEN autorate job with PSL DropMode THEN should return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 4800m, CostCalculationDescription = "", JR_OSSellAmt = 4800m, RevenueCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 48.00/KG" } },
				cartage,
				Consignee
			);

			clientRateCalculator.EquipmentType = LCLAIREquipmentNeeded.Haulier;
			Factory.Save();
			AutorateAndAssert
			(
				"GIVEN ClientRate with CTB HSL EquipmentType and CompanyTariff with CTG PSL EquipmentType WHEN autorate job with PSL DropMode THEN should not return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 4000m, CostCalculationDescription = "", JR_OSSellAmt = 4000m, RevenueCalculationDescription = "TCART: 100 Kilogram(s) @ AUD 40.00/KG" } },
				cartage,
				Consignee
			);

			clientRateCalculator.EquipmentType = EquipmentNeeded.Any;
			Factory.Save();
			tarifRateLine.Calculator.EquipmentType = LCLAIREquipmentNeeded.Haulier;
			tariff.Factory.Save();
			AutorateAndAssert
			(
				"GIVEN ClientRate with CTB ANY EquipmentType and CompanyTariff with CTG HSL EquipmentType WHEN autorate job with PSL DropMode THEN should not return ClientRate",
				expected: new[] { new AssertionCharge { JR_OSCostAmt = 0m, CostCalculationDescription = "", JR_OSSellAmt = 0m, RevenueCalculationDescription = "TCART: Calculation failed due to charge code is using the Company Tariff Based Calculator, however there are conflicting or no tariff rates found." } },
				cartage,
				Consignee
			);
		}

		#endregion

		[TestDate(2016, 06, 06)]
		public void TestCommonCartage_CartageZoneCalculator_DistanceCalculationService()
		{
			#region setup organizations

			var supplier = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "AUSYD";

			consignor.MainAddress.OA_Address1 = "58 Mentmore Ave";
			consignor.MainAddress.OA_City = "Roseberry";
			consignor.MainAddress.OA_PostCode = "2018";
			consignor.MainAddress.OA_State = "NSW";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignor.MainAddress.OA_LCLEquipmentNeeded = "PSL";

			var consignorAddress1 = consignor.Addresses.AddNew();
			consignorAddress1.AddAddressType(OrgAddressType.PickupAndDelivery);
			consignorAddress1.OA_Address1 = "389 Crown St";
			consignorAddress1.OA_City = "Surry Hills";
			consignorAddress1.OA_PostCode = "2010";
			consignorAddress1.OA_State = "NSW";
			consignorAddress1.OA_RL_NKRelatedPortCode = "AUSYD";
			consignorAddress1.OA_LCLEquipmentNeeded = "PSL";

			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignee = true;
			consignee.OH_RL_NKClosestPort = "AUBTB";

			consignee.MainAddress.OA_Address1 = "72 O'Riordan St";
			consignee.MainAddress.OA_City = "Alexandria";
			consignee.MainAddress.OA_PostCode = "2015";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUBTB";
			consignee.MainAddress.OA_LCLEquipmentNeeded = "PSL";

			var consigneeAddress1 = consignee.Addresses.AddNew();
			consigneeAddress1.AddAddressType(OrgAddressType.PickupAndDelivery);
			consigneeAddress1.OA_Address1 = "118 March St";
			consigneeAddress1.OA_City = "Richmond";
			consigneeAddress1.OA_PostCode = "2753";
			consigneeAddress1.OA_State = "NSW";
			consigneeAddress1.OA_RL_NKRelatedPortCode = "AURCH";
			consigneeAddress1.OA_LCLEquipmentNeeded = "PSL";

			#endregion

			var transportZoneSet = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia, 0, 50, 250);
			var chargeCode = Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			Factory.Save();

			var tariff = Helper.NewCompanyTariff();
			var rateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			rateEntry.TI_OH_Supplier = supplier.PK;
			var rateLine = rateEntry.AddRateLine(chargeCode, CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			rateLine.ConversionFactor = new ConversionFactor(194m, Volume.CubicInches, Weight.Pounds);

			var calculator = rateLine.Calculator;
			calculator.EquipmentType = "PSL";
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 10m, ZGuid.Empty);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 20m, transportZoneSet.Zones[0].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 30m, transportZoneSet.Zones[1].PK);

			tariff.Factory.Save();

			CreateCartageType("LRLA", true);

			#region Assert Distance Calculation: On

			Env.Registry.Rating.UseDistanceCalculationService = true;

			var cartage1 = CreateCartage("T00001001", "LRLA", consignor.MainAddress.PK, consigneeAddress1.PK, supplier.PK);
			var cartage2 = CreateCartage("T00001002", "LRLA", consignorAddress1.PK, consignee.MainAddress.PK, supplier.PK);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 3600m,
							RevenueCalculationDescription = "CART: 120 Kilogram(s) @ AUD 30.00/KG"
						}
				};

			var expectedLogLines = new[] { @"Information: RateLine Found CART-CTZ-KG-Base Company Tariff",
@"Information: Matched '50 to 249' for RateLine CART-CTZ-KG-Base Company Tariff
	- Pickup Distance: empty
	- Distance Calculation Service: 76 (Consignor Pickup Address to Consignee Delivery Address)
	- '50 to 249' matched by distance (76)" };

			AutorateAndAssert(expected, cartage1, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(cartage1, "Log should contain expected lines", expectedLogLines);

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 3600m,
							RevenueCalculationDescription = "CART: 120 Kilogram(s) @ AUD 30.00/KG"
						}
				};

			expectedLogLines = new[] { @"Information: RateLine Found CART-CTZ-KG-Base Company Tariff",
@"Information: Matched '50 to 249' for RateLine CART-CTZ-KG-Base Company Tariff
	- Pickup Distance: empty
	- Distance Calculation Service: 80 (Consignor Pickup Address to Consignee Delivery Address)
	- '50 to 249' matched by distance (80)" };

			AutorateAndAssert(expected, cartage2, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(cartage2, "Log should contain expected lines", expectedLogLines);

			#endregion

			#region Asset Distance Calculation Service: Off

			Env.Registry.Rating.UseDistanceCalculationService = false;

			var cartage3 = CreateCartage("T00001003", "LRLA", consignor.MainAddress.PK, consigneeAddress1.PK, supplier.PK);
			var cartage4 = CreateCartage("T00001004", "LRLA", consignorAddress1.PK, consignee.MainAddress.PK, supplier.PK);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 3600m,
							RevenueCalculationDescription = "CART: 120 Kilogram(s) @ AUD 30.00/KG"
						}
				};

			expectedLogLines = new[] { @"Information: RateLine Found CART-CTZ-KG-Base Company Tariff",
@"Information: Matched '50 to 249' for RateLine CART-CTZ-KG-Base Company Tariff
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: 58.04 (Consignor Pickup Address to Consignee Delivery Address)
	- '50 to 249' matched by distance (58.04)" };

			AutorateAndAssert(expected, cartage3, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(cartage3, "Log should contain expected lines", expectedLogLines);

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 2400m,
							RevenueCalculationDescription = "CART: 120 Kilogram(s) @ AUD 20.00/KG"
						}
				};

			expectedLogLines = new[]
			{
				@"Information: RateLine Found CART-CTZ-KG-Base Company Tariff",
				@"Information: Matched '0 to 49' for RateLine CART-CTZ-KG-Base Company Tariff
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: 3.548 (Consignor Pickup Address to Consignee Delivery Address)
	- '0 to 49' matched by distance (3.548)"
			};

			AutorateAndAssert(expected, cartage4, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(cartage4, "Log should contain expected lines", expectedLogLines);

			#endregion
		}

		public void TestCommonCartage_CartageZoneCalculator_LeavesNoDistanceGaps()
		{
			#region Org Set up

			var supplier = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "AUBNE";

			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_IsConsignee = true;
			consignee.OH_RL_NKClosestPort = "AUGOC";

			var consignorAddressPK = consignor.MainAddress.PK;
			var consigneeAddressPK = consignee.MainAddress.PK;

			#endregion

			#region Zones Setup

			var zoneProvider = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia);

			var zone0 = zoneProvider.CreateRateTransportZoneForTest("0-79");
			zone0.CreateRateTransportZoneItemForTest(0, 79);

			var zone1 = zoneProvider.CreateRateTransportZoneForTest("79-80");
			zone1.CreateRateTransportZoneItemForTest(79, 80);

			var zone2 = zoneProvider.CreateRateTransportZoneForTest("80-500");
			zone2.CreateRateTransportZoneItemForTest(80, 500);

			#endregion

			#region Rates Setup

			var chargeCode = Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			Factory.Save();

			var tariff = Helper.NewCompanyTariff();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			entry.TI_OH_Supplier = supplier.PK;
			var line = entry.AddRateLine(chargeCode, CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = line.Calculator;
			calculator.EquipmentType = "PSL";
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 10m, ZGuid.Empty);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 20m, zoneProvider.Zones[0].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 30m, zoneProvider.Zones[1].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 50m, zoneProvider.Zones[2].PK);

			tariff.Factory.Save();

			#endregion

			#region Cartage Setup

			var jobType = "LRLA";
			CreateCartageType(jobType, true);

			var cartage1 = CreateCartage("T00001001", jobType, consignorAddressPK, consigneeAddressPK, supplier.PK);
			cartage1.LooseBookedMoves[0].EW_Distance = 78.5m;

			var cartage2 = CreateCartage("T00001002", jobType, consignorAddressPK, consigneeAddressPK, supplier.PK);
			cartage2.LooseBookedMoves[0].EW_Distance = 79.4m;

			var cartage3 = CreateCartage("T00001003", jobType, consignorAddressPK, consigneeAddressPK, supplier.PK);
			cartage3.LooseBookedMoves[0].EW_Distance = 79.9m;

			var cartage4 = CreateCartage("T00001004", jobType, consignorAddressPK, consigneeAddressPK, supplier.PK);
			cartage4.LooseBookedMoves[0].EW_Distance = 80.0m;

			var cartage5 = CreateCartage("T00001005", jobType, consignorAddressPK, consigneeAddressPK, supplier.PK);
			cartage5.LooseBookedMoves[0].EW_Distance = 80.1m;

			var cartage6 = CreateCartage("T00001006", jobType, consignorAddressPK, consigneeAddressPK, supplier.PK);
			cartage6.LooseBookedMoves[0].EW_Distance = 280m;

			var cartage7 = CreateCartage("T00001007", jobType, consignorAddressPK, consigneeAddressPK, supplier.PK);
			cartage7.LooseBookedMoves[0].EW_Distance = 9001m;

			#endregion

			Factory.Save();

			var zoneDescription = DescriptionHelpers.FormatWithTab("Zone:");

			var expectedLessThan70 = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 2400,
							RevenueCalculationDescription = @"CART: 120 Kilogram(s) @ AUD 20.00/KG
" + zoneDescription + "TESTORG1 0-79"
						}
				};

			var expected79To80 = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 3600m,
							RevenueCalculationDescription = @"CART: 120 Kilogram(s) @ AUD 30.00/KG
" + zoneDescription + "TESTORG1 79-80"
						}
				};

			var expected80To500 = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 6000m,
							RevenueCalculationDescription = @"CART: 120 Kilogram(s) @ AUD 50.00/KG
" + zoneDescription + "TESTORG1 80-500"
						}
				};

			var emptyCharge = new[] { new AssertionCharge { } };

			AutorateAndAssert("78.5km should belong in Zone 0-79", expectedLessThan70, cartage1, consignor);

			AutorateAndAssert("79.4km should belong in Zone 79-80", expected79To80, cartage2, consignor);
			AutorateAndAssert("79.9km should also belong in Zone 79-80", expected79To80, cartage3, consignor);
			AutorateAndAssert("80.0km should belong in Zone 79-80", expected79To80, cartage4, consignee);

			AutorateAndAssert("80.1km should belong in Zone 80-500", expected80To500, cartage5, consignee);
			AutorateAndAssert("280.0km should belong in Zone 80-500", expected80To500, cartage6, consignee);

			AutorateAndAssert("CTZ calc does not cover zones with a distance over 9000", emptyCharge, cartage7, consignee);
		}

		public void TestCommonCartage_CartageZoneCalculator_CalculatorOptions()
		{
			#region setup organizations

			var supplier = Helper.NewOrgHeader();

			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "AUBNE";

			consignor.MainAddress.OA_Address1 = "1310 LYTTON ROAD";
			consignor.MainAddress.OA_Address2 = "HEMMANT QLD";
			consignor.MainAddress.OA_AIREquipmentNeeded = "PSL";
			consignor.MainAddress.OA_City = "AUBNE";
			consignor.MainAddress.OA_Code = "PST: 1310 LYTTON ROAD";
			consignor.MainAddress.OA_FCLEquipmentNeeded = "WUP";
			consignor.MainAddress.OA_LCLEquipmentNeeded = "PSL";
			consignor.MainAddress.OA_PostCode = "4174";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			consignor.MainAddress.OA_State = "QLD";

			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignee = true;
			consignee.OH_RL_NKClosestPort = "AUBNE";

			consignee.MainAddress.OA_Address1 = "3 RARNLY DRIVE";
			consignee.MainAddress.OA_Address2 = "BURLEIGH HEADS, QLD";
			consignee.MainAddress.OA_AIREquipmentNeeded = "PSL";
			consignee.MainAddress.OA_City = "AUBNE";
			consignee.MainAddress.OA_Code = "PST: 3 RARNLY DRIVE";
			consignee.MainAddress.OA_FCLEquipmentNeeded = "WUP";
			consignee.MainAddress.OA_LCLEquipmentNeeded = "PSL";
			consignee.MainAddress.OA_PostCode = "4220";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "";
			consignee.MainAddress.OA_State = "QLD";

			#endregion

			#region setup zones

			var zoneSetHeader = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia);
			var zone = zoneSetHeader.CreateRateTransportZoneForTest("D50-D100");
			zone.CreateRateTransportZoneItemForTest(50, 100);

			#endregion

			#region setup rates

			var chargeCode = Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			Factory.Save();

			var tariff = Helper.NewCompanyTariff();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			entry.TI_OH_Supplier = supplier.PK;

			var line = entry.AddRateLine(chargeCode, CartageZoneDistanceCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			line.ConversionFactor = new ConversionFactor(194m, Volume.CubicInches, Weight.Pounds);
			var calculator = line.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = "PSL";
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 150m, 10m, zoneSetHeader.Zones[0].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 150m, 5m, zoneSetHeader.Zones[0].PK);
			tariff.Factory.Save();

			#endregion

			#region setup cartage types and cartages

			CreateCartageType("LRLA", true);

			var cartage = CreateCartage("T00001001", "LRLA", consignor.MainAddress.PK, consignee.MainAddress.PK, supplier.PK);
			cartage.LooseBookedMoves[0].EW_Distance = 80m;

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CART",
							JR_OSSellAmt = 1200m,
						}
				};

			AutorateAndAssert(expected, cartage, consignor);

			cartage.LooseBookedMoves[0].EW_BookedWeight = 140m;

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "CART",
						JR_OSSellAmt = 1400m,
					}
			};

			AutorateAndAssert(expected, cartage, consignor);

			calculator.UseHigherChargeableLowerRateRule = true;
			tariff.Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "CART",
						JR_OSSellAmt = 750m,
					}
			};

			AutorateAndAssert(expected, cartage, consignor);

			calculator.UseHigherChargeableLowerRateRule = false;
			cartage.LooseBookedMoves[0].EW_BookedWeight = 160m;
			tariff.Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "CART",
						JR_OSSellAmt = 800m,
					}
			};

			AutorateAndAssert(expected, cartage, consignor);

			calculator.IsAccumulated = true;
			tariff.Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "CART",
						JR_OSSellAmt = 1550m,
					}
			};

			AutorateAndAssert(expected, cartage, consignor);

			calculator.IsAccumulated = false;
			cartage.LooseBookedMoves[0].EW_BookedWeight = 150m;
			tariff.Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "CART",
						JR_OSSellAmt = 750m,
					}
			};

			AutorateAndAssert(expected, cartage, consignor);

			calculator.UseInclusiveBreaks = true;
			tariff.Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "CART",
						JR_OSSellAmt = 1500m,
					}
			};

			AutorateAndAssert(expected, cartage, consignor);
		}

		public void TestCommonCartage_CartageZoneCalculator_MatchedZoneSetHasZonesMatchingByDifferentCriteria()
		{
			var cityTown = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			var zoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia);

			var cityZone = zoneSet.CreateRateTransportZoneForTest("City Zone");
			cityZone.CreateRateTransportZoneItemForTest(cityTown);

			var distanceZone = zoneSet.CreateRateTransportZoneForTest("Distance Zone");
			distanceZone.CreateRateTransportZoneItemForTest(0, 40);

			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "AUBTB";

			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_IsConsignee = true;
			consignee.OH_RL_NKClosestPort = "AUSYD";

			var clientRate = Helper.NewClientRate(consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-1);

			var chargeCode = Helper.ChargeCodes.New("TCART", "Transport Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			var rateLine = rateEntry.AddRateLine(chargeCode, CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, cityZone.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 12m, distanceZone.PK);

			CreateCartageType("LRLA", true);

			var cartage = CreateCartage("T100216", "LRLA", consignor.MainAddress.PK, consignee.MainAddress.PK, Helper.NewOrgHeader().PK);
			cartage.LooseBookedMoves[0].EW_Distance = 24.5m;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 1440m,
						RevenueCalculationDescription = @"TCART: 120 Kilogram(s) @ AUD 12.00/KG
" + DescriptionHelpers.FormatWithTab("Zone:") + "Distance Zone"
					}
			};

			AutorateAndAssert("Expected distance measure to be prefered to cartage address city town", expected, cartage, consignee);
		}

		#endregion

		#region CostBased Calculator

		public void TestCommonCartage_MultipleLegs_CSTCalculator()
		{
			var year = ZDateTime.Now.Year;
			var zero = new ZDateTime(year, 1, 1);
			var client = Helper.NewOrgHeader(1);
			var chargeCode = Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal);

			Factory.Save();

			var tariff = Helper.NewCosting(null);
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			var line = entry.AddRateLine(chargeCode, TimeCalculator.Code, "HR");
			line.GetCalculator<TimeCalculator>().PerUnit = 100;

			var clientRate = Helper.NewClientRate(client);
			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			var clientLine = clientEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			clientLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 50m;

			Factory.Save();

			var cartage = CreateContainerisedCartage(client);
			cartage.ContainerBookedMoves[0].CartageLegs[0].JU_CartageWaitPointDemurrage = zero.AddHours(1);
			cartage.ContainerBookedMoves[1].CartageLegs[0].JU_CartageWaitPointDemurrage = zero.AddHours(2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CART",
					JR_OSCostAmt = 100m,
					JR_OSSellAmt = 150m,
					CostCalculationDescription = "CART: 1 Hour x Day @ AUD 100.00/Hour x Day"
				},
				new AssertionCharge
				{
					ChargeCode = "CART",
					JR_OSCostAmt = 200m,
					JR_OSSellAmt = 300m,
					CostCalculationDescription = "CART: 2 Hour x Day (2 Hour(s) x 1 Day(s)) @ AUD 100.00/Hour x Day"
				},
			};

			AutorateAndAssert("", expected, cartage, client);
		}

		#endregion

		#endregion

		#region Port Transport (CommonCartage) Rate Entry Filtering

		#region TestCommonCartage_RateEntryFiltering_ArrivalAndDepartureDates

		public void TestCommonCartage_RateEntryFiltering_ArrivalAndDepartureDates()
		{
			var supplier = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader(1);

			var chargeCode = Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			Factory.Save();

			var tariff = Helper.NewCompanyTariff();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			entry.TI_RateStartDate = ZDate.Today.AddMonths(-3);
			entry.TI_RateEndDate = ZDate.Today.AddMonths(-1);
			entry.TI_OH_Supplier = supplier.PK;

			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 200m;

			tariff.Factory.Save();

			CreateCartageType("LRLA", true);
			var cartage = CreateCartage("T00001001", "LRLA", ZGuid.Empty, ZGuid.Empty, supplier.PK);

			var move = cartage.LooseBookedMoves[0];
			var leg1 = move.CartageLegs[0];
			leg1.JU_PickupTimeIn = ZDateTime.Today.AddMonths(-2);
			var leg2 = move.CartageLegs.AddNew();

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CART",
							JR_OSSellAmt = 200m,
						}
				};

			AutorateAndAssert(expected, cartage, consignor);

			leg1.JU_PickupTimeIn = ZDateTime.Today.AddMonths(-4);
			AutorateAndAssert(Array.Empty<AssertionCharge>(), cartage, consignor);

			var dateConfig = AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.Value;
			dateConfig.FilterType = RatingDateFilterTypes.Codes.Arrival;
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateConfig);
			AutorateAndAssert(Array.Empty<AssertionCharge>(), cartage, consignor);

			leg2.JU_DeliverTimeOut = ZDateTime.Today.AddMonths(-2);
			AutorateAndAssert(expected, cartage, consignor);

			leg2.JU_DeliverTimeOut = ZDateTime.Today.AddMonths(-4);
			AutorateAndAssert(Array.Empty<AssertionCharge>(), cartage, consignor);
		}

		#endregion

		#region TestCommonCartage_RateEntryFiltering_ConsigneeConsignorAddresses

		public void TestCommonCartage_RateEntryFiltering_ConsigneeConsignorAddresses()
		{
			var client = Helper.NewOrgHeader();

			var consignor = Helper.NewOrgHeader();
			consignor.MainAddress.AddAddressType(OrgAddressType.Pickup);

			var consignee = Helper.NewOrgHeader();
			consignee.MainAddress.AddAddressType(OrgAddressType.Delivery);

			var chargeCode = Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			entry.TI_OH_Consignor = consignor.PK;
			entry.TI_OH_Consignee = consignee.PK;

			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 20m;

			CreateCartageType("LRLA", false);

			var cartage = CreateCartage("T00001001", "LRLA", consignor.MainAddress.PK, consignee.MainAddress.PK, Guid.Empty);
			cartage.LocalClientAddressPK = client.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CART",
							JR_OSSellAmt = 20.00m,
						}
				};

			AutorateAndAssert("Pick and Delivery addresses on a cartage job should match CNE/CNR", expected, cartage, client);
		}

		#endregion

		#region TestCommonCartage_RateEntryFiltering_LocalClient

		public void TestCommonCartage_RateEntryFiltering_OnlyLocalClientSellRatesAreApplied()
		{
			var client = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			consignor.MainAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			consignee.MainAddress.AddAddressType(OrgAddressType.PickupAndDelivery);

			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("PT1", "Local Client", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			var chargeCode2 = Helper.ChargeCodes.NewConsolChargeCode("PT2", "Consignor", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			var chargeCode3 = Helper.ChargeCodes.NewConsolChargeCode("PT3", "Consignee", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			var line = entry.AddRateLine(chargeCode1, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 100m;

			clientRate = Helper.NewClientRate(consignor);
			entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			line = entry.AddRateLine(chargeCode2, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 70m;

			clientRate = Helper.NewClientRate(consignee);
			entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			line = entry.AddRateLine(chargeCode3, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 120m;

			var cartage = CreateContainerisedCartage(client);
			var consignorDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter);
			var consigneeDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			consignorDocAddress.E2_OA_Address = consignor.MainAddress.PK;
			consigneeDocAddress.E2_OA_Address = consignee.MainAddress.PK;

			var move1 = cartage.ContainerBookedMoves[0];
			move1.EW_E2PickupAddressID = consignorDocAddress.PK;

			var move2 = cartage.ContainerBookedMoves[1];
			move2.EW_E2DeliveryAddressID = consigneeDocAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode1.AC_Code,
					JR_LocalSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode1.AC_Code,
					JR_LocalSellAmt = 100m
				}
			};

			AutorateAndAssert("Only Local Client Charges expected", expected, cartage, client);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode3.AC_Code,
					JR_LocalSellAmt = 120m
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode3.AC_Code,
					JR_LocalSellAmt = 120m
				}
			};

			AutorateAndAssert("Changing the local client should change the results", expected, cartage, consignee);
		}

		public void TestCommonCartage_RateEntryFiltering_MatchesImportBrokerAsLocalClient()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			consignor.MainAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			consignee.MainAddress.AddAddressType(OrgAddressType.PickupAndDelivery);

			var chargeCode = Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var rate = Helper.NewClientRate(consignee);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "");
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 20m;

			CreateCartageType("LRLA", false);

			var cartage = CreateCartage("T00001001", "LRLA", consignor.MainAddress.PK, consignee.MainAddress.PK, Guid.Empty);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CART",
							JR_OSSellAmt = 20.00m,
						}
				};

			AutorateAndAssert(expected, cartage, consignee);

			consignee.OH_IsBroker = true;

			AutorateAndAssert(expected, cartage, consignee);
		}

		#endregion

		#endregion

		#region Port Transport (CommonCartage) Rate Entry Filtering

		class ContainerPKSort : IComparer<CommonCartageLeg>
		{
			public int Compare(CommonCartageLeg x, CommonCartageLeg y)
				=> x.Container.PK.CompareTo(y.Container.PK);
		}

		class ReverseContainerPKSort : IComparer<CommonCartageLeg>
		{
			public int Compare(CommonCartageLeg x, CommonCartageLeg y)
				=> -x.Container.PK.CompareTo(y.Container.PK);
		}

		public void TestMergeSameChargeCodeCharges_AutorateCostAndRevenue()
		{
			var chargeCode = Helper.ChargeCodes.New("PT", "Port Transport", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			CreateSameChargeCodeRates(chargeCode);
			Factory.Save();

			var cartage = CreateContainerizedCartageWithFourContainers();
			cartage.CartageLegs.ApplySort(new ContainerPKSort());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 100m,
					JR_LocalSellAmt = 110m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 200m,
					JR_LocalSellAmt = 220m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 250m,
					JR_LocalSellAmt = 255m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 370m,
					JR_LocalSellAmt = 377m,
				},
			};

			AutorateAndAssert("", expected, cartage, NewClient);

			// Reverse container order
			cartage.CartageLegs.ApplySort(new ReverseContainerPKSort());
			AutorateAndAssert("", expected, cartage, NewClient);

			// Remove container numbers and repeat
			foreach (var move in cartage.ContainerBookedMoves)
			{
				move.Container.JC_ContainerNum = ZString.Empty;
			}
			AutorateAndAssert("", expected, cartage, NewClient);

			// Change order again
			cartage.CartageLegs.ApplySort(new ContainerPKSort());
			AutorateAndAssert("", expected, cartage, NewClient);
		}

		public void TestMergeSameChargeCodeCharges_AutorateCostThenAutorateRevenue()
		{
			var chargeCode = Helper.ChargeCodes.New("PT", "Port Transport", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			CreateSameChargeCodeRates(chargeCode);
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var cartage = CreateContainerizedCartageWithFourContainers();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 100m,
					JR_LocalSellAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 200m,
					JR_LocalSellAmt = 200m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 250m,
					JR_LocalSellAmt = 250m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 370m,
					JR_LocalSellAmt = 370m,
				},
			};

			AutorateAndAssert("", expected, cartage, NewClient, autorateRevenue: false);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 100m,
					JR_LocalSellAmt = 110m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 200m,
					JR_LocalSellAmt = 220m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 250m,
					JR_LocalSellAmt = 255m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 370m,
					JR_LocalSellAmt = 377m,
				}
			};

			using (var job = new JobHeader.Loader(cartage).TryLoadOrCreate() as Job)
			{
				AutorateAndAssert("", expected, cartage, NewClient, autorateCosts: false, job: job);
			}
		}

		public void TestMergeSameChargeCodeCharges_AutorateRevenueThenAutorateCost()
		{
			var chargeCode = Helper.ChargeCodes.New("PT", "Port Transport", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			CreateSameChargeCodeRates(chargeCode);
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var cartage = CreateContainerizedCartageWithFourContainers();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 110m,
					JR_LocalSellAmt = 110m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 220m,
					JR_LocalSellAmt = 220m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 255m,
					JR_LocalSellAmt = 255m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 377m,
					JR_LocalSellAmt = 377m,
				},
			};

			AutorateAndAssert("", expected, cartage, NewClient, autorateCosts: false);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 100m,
					JR_LocalSellAmt = 110m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 200m,
					JR_LocalSellAmt = 220m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 250m,
					JR_LocalSellAmt = 255m,
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 370m,
					JR_LocalSellAmt = 377m,
				}
			};

			using (var job = new JobHeader.Loader(cartage).TryLoadOrCreate() as Job)
			{
				AutorateAndAssert("", expected, cartage, NewClient, autorateRevenue: false, job: job);
			}
		}

		CommonCartage CreateContainerizedCartageWithFourContainers()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLCTOtoCFS;
			cartage.JJ_ContainerMode = CartageContainerMode.Containerized;
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			var container20GP1 = Factory.NewWithValidTestData<CommonContainer>();
			container20GP1.JC_RC = GP20.PK;
			container20GP1.JC_ContainerNum = "AAA";
			container20GP1.JC_GrossWeight = 1000;

			var move20GP1 = Factory.New<CommonBookedCtgMove>();
			move20GP1.EW_JJ = cartage.PK;
			move20GP1.EW_JC_Container = container20GP1.PK;
			move20GP1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move20GP1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg20GP1 = move20GP1.CartageLegs.AddNew();
			leg20GP1.JU_RunSheetSequence = 1;
			leg20GP1.JU_EW = move20GP1.PK;

			var container20GP2 = Factory.NewWithValidTestData<CommonContainer>();
			container20GP2.JC_RC = GP20.PK;
			container20GP2.JC_ContainerNum = "BBB";
			container20GP2.JC_GrossWeight = 8000;

			var move20GP2 = Factory.New<CommonBookedCtgMove>();
			move20GP2.EW_JJ = cartage.PK;
			move20GP2.EW_JC_Container = container20GP2.PK;
			move20GP2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move20GP2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg20GP2 = move20GP2.CartageLegs.AddNew();
			leg20GP2.JU_RunSheetSequence = 1;
			leg20GP2.JU_EW = move20GP2.PK;

			var container40GP = Factory.NewWithValidTestData<CommonContainer>();
			container40GP.JC_RC = GP40.PK;
			container40GP.JC_ContainerNum = "CCC";

			var move40GP = Factory.New<CommonBookedCtgMove>();
			move40GP.EW_JJ = cartage.PK;
			move40GP.EW_JC_Container = container40GP.PK;
			move40GP.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move40GP.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg40GP = move40GP.CartageLegs.AddNew();
			leg40GP.JU_RunSheetSequence = 1;
			leg40GP.JU_EW = move40GP.PK;

			var hc40 = Helper.Containers["40HC"];
			var container40HC = Factory.NewWithValidTestData<CommonContainer>();
			container40HC.JC_RC = hc40.PK;
			container40HC.JC_ContainerNum = "DDD";

			var move40HC = Factory.New<CommonBookedCtgMove>();
			move40HC.EW_JJ = cartage.PK;
			move40HC.EW_JC_Container = container40HC.PK;
			move40HC.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move40HC.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg40HC = move40HC.CartageLegs.AddNew();
			leg40HC.JU_RunSheetSequence = 1;
			leg40HC.JU_EW = move40HC.PK;

			return cartage;
		}

		void CreateSameChargeCodeRates(AccChargeCode chargeCode)
		{
			var standardCosting = Helper.NewCosting(null);
			var costingEntry20GP = standardCosting.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "20GP");
			AddCombinedCalculatorWithTwoWeightRates(costingEntry20GP.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN));

			var costingEntry40GP = standardCosting.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "40GP");
			costingEntry40GP.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 250m;
			var costingEntry40HC = standardCosting.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "40HC");
			costingEntry40HC.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 370m;

			var clientRate = Helper.NewClientRate(NewClient);
			var clientRateEntry20GP = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "20GP");
			AddCombinedCalculatorWithTwoWeightRates(clientRateEntry20GP.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN), 5000m, 110m, 220m);
			var clientRateEntry40GP = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "40GP");
			clientRateEntry40GP.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 255m;
			var clientRateEntry40HC = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "40HC");
			clientRateEntry40HC.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 377m;
		}

		void AddCombinedCalculatorWithTwoWeightRates(RateLine line, decimal weightBreakKg = 5000, decimal rate1 = 100m, decimal rate2 = 200m)
		{
			line.RateCalculatorType = CalculatorType.Combined;
			var calculator = line.GetCalculator<CombinedCalculator>();
			var weightText = weightBreakKg.ToString(CultureInfo.InvariantCulture);
			calculator["-" + weightText] = (ZDecimal)rate1;
			calculator["+" + weightText] = (ZDecimal)rate2;
			line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus).TM_BreakWeightVolume = QuantityUnit.KG;
		}

		public void TestMergeSameChargeCodeCharges_ManyLegsMoveTheSameContainer_JobDirectionAndLegOrderShouldDecideWhichChargesToMerge()
		{
			// Scenario:
			// - Transport booking with 1 container. The container is moved by 2 legs. 2 cost charges are created with cartage legs and
			//  and only 1 revenue charge is created with cartage move. The revenue charge must merge to only 1 of the 2 cost charges.
			// GIVEN:
			// - 1 transport booking (cartage) having a defined moving direction. Eg. in this test, NEW_FCLCTOtoCFS (ISFF) => Import job
			// - 1 container
			// - 2 run sheets for moving the container with 2 different transport companies
			// - 2 legs with the 2 run sheets
			// - 2 costing rates from the 2 transport companies
			// - 1 client rate for local client
			// WHEN:
			// - Autorate Cost and (then) Revenue in the same session
			// - Since legs move the same container, the JU_DisplayOrder is crucial for which cost/revenue charges for merging autorating results.
			// - Job movement direction is import, the cartage leg for revenue will be the last leg defined by JU_DisplayOrder.
			// THEN:
			// - There are 2 cost charges created from 2 legs.
			// - Single revenue charge should merge with the cost charge from the last leg.

			var chargeCode = Helper.ChargeCodes.New("PT", "Port Transport", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var costing1 = Helper.NewCosting(TransportProvider1);
			var costingEntry1 = costing1.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "20GP");
			costingEntry1.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 200;

			var costing2 = Helper.NewCosting(TransportProvider2);
			var costingEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "20GP");
			costingEntry2.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 250;

			var clientRate = Helper.NewClientRate(NewClient);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "20GP");
			clientRateEntry.AddRateLine(chargeCode, lineUnit: QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 255;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLCTOtoCFS;
			cartage.JJ_ContainerMode = CartageContainerMode.Containerized;

			var container = Factory.NewWithValidTestData<CommonContainer>();
			container.JC_ContainerNum = "ABCD1012023";
			container.JC_RC = GP20.PK;
			container.JC_GrossWeight = 1000;

			var move = Factory.New<CommonBookedCtgMove>();
			move.EW_JJ = cartage.PK;
			move.EW_JC_Container = container.PK;
			move.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg1 = move.CartageLegs.AddNew();
			leg1.JU_RunSheetSequence = 1;
			leg1.JU_EW = move.PK;
			leg1.JU_DisplayOrder = 1;

			var workSheet1 = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet1.CartageLegs.AddRange(new[] { leg1 });
			workSheet1.EY_OH_TransportCo = TransportProvider1.PK;
			workSheet1.EY_RunSheetNumber = "RS001";

			var leg2 = move.CartageLegs.AddNew();
			leg2.JU_EW = move.PK;
			leg2.JU_RunSheetSequence = 2;
			leg2.JU_DisplayOrder = 2;

			var workSheet2 = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet2.CartageLegs.AddRange(new[] { leg2 });
			workSheet2.EY_OH_TransportCo = TransportProvider2.PK;
			workSheet2.EY_RunSheetNumber = "RS002";

			Factory.Save();

			var autoratedForDescription = DescriptionHelpers.FormatWithTab("Autorated for:");

			var expected = new[]
			{
				// As described about cartage.JJ_E3_NKJobType and leg.JU_DisplayOrder, this charge should not be merged.
				// Hence JR_LocalSellAmt should not be changed and RevenueCalculationDescription should be empty.
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 200,
					JR_LocalSellAmt = 200,
					CostCalculationDescription = $"{autoratedForDescription}Leg for ABCD1012023  20GP  T00001000/A",
					RevenueCalculationDescription = "",
				},
				// Revenue charge is merged with Cost charge:
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 250,
					JR_LocalSellAmt = 255,
					CostCalculationDescription = $"{autoratedForDescription}Leg for ABCD1012023  20GP  T00001000/B",
					RevenueCalculationDescription = $"{autoratedForDescription}JobBookedCtgMove, JobDocAddress, JobDocAddress",
				},
			};
			AutorateAndAssert("", expected, cartage, NewClient);
		}

		#endregion

		#region Services

		public void TestCommonCartage_MultipleMovesAndLegs_AutorateServiceCost()
		{
			var chargeCode1 = Helper.ChargeCodes.New("PTFUM20", "Port Transport Fumigation 20GP", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);
			var chargeCode2 = Helper.ChargeCodes.New("PTFUM40", "Port Transport Fumigation 40GP", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);

			var costing = Helper.NewCosting(null);
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "20GP");
			var line1 = entry1.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			line1.GetCalculator<UnitCalculator>().PerUnit = 100;

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "40GP");
			var line2 = entry2.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			line2.GetCalculator<UnitCalculator>().PerUnit = 110;

			NewClient.OH_IsDebtor = true;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLCTOtoCFS;
			cartage.JJ_ContainerMode = CartageContainerMode.Containerized;
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			// move1 setup
			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			container1.JC_RC = GP20.PK;

			var service1 = container1.Services.AddNew();
			service1.ES_Booked = ZDateTime.Today;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_ServiceCount = 1;

			var cartageMove1 = Factory.New<CommonBookedCtgMove>();
			cartageMove1.EW_JJ = cartage.PK;
			cartageMove1.EW_JC_Container = container1.PK;
			cartageMove1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var cartageLeg11 = cartageMove1.CartageLegs.AddNew();
			cartageLeg11.JU_RunSheetSequence = 1;
			cartageLeg11.JU_EW = cartageMove1.PK;

			var cartageLeg12 = cartageMove1.CartageLegs.AddNew();
			cartageLeg12.JU_RunSheetSequence = 2;
			cartageLeg12.JU_EW = cartageMove1.PK;

			// move2 setup
			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.JC_RC = GP40.PK;

			var service2 = container2.Services.AddNew();
			service2.ES_Booked = ZDateTime.Today;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_ServiceCount = 2;

			var cartageMove2 = Factory.New<CommonBookedCtgMove>();
			cartageMove2.EW_JJ = cartage.PK;
			cartageMove2.EW_JC_Container = container2.PK;
			cartageMove2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var cartageLeg21 = cartageMove2.CartageLegs.AddNew();
			cartageLeg21.JU_RunSheetSequence = 1;
			cartageLeg21.JU_EW = cartageMove2.PK;

			var cartageLeg22 = cartageMove2.CartageLegs.AddNew();
			cartageLeg22.JU_RunSheetSequence = 2;
			cartageLeg22.JU_EW = cartageMove2.PK;

			var cartageLeg23 = cartageMove2.CartageLegs.AddNew();
			cartageLeg23.JU_RunSheetSequence = 3;
			cartageLeg23.JU_EW = cartageMove2.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "PTFUM20",
					JR_LocalCostAmt = 100m // move1: 1 SV x $100/SV
				},
				new AssertionCharge
				{
					ChargeCode = "PTFUM40",
					JR_LocalCostAmt = 220m // move2: 2 SV x $110/SV
				},
			};

			AutorateAndAssert("Regardless of how many legs there should be charges for services from moves.", expected, cartage, NewClient, autorateRevenue: false);
		}

		public void TestCommonCartage_MultipleMovesAndLegs_AutorateServiceRevenue()
		{
			var chargeCode1 = Helper.ChargeCodes.New("PTFUM20", "Port Transport Fumigation 20GP", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);
			var chargeCode2 = Helper.ChargeCodes.New("PTFUM40", "Port Transport Fumigation 40GP", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);

			var clientRate = Helper.NewClientRate(NewClient);
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "20GP");
			var line1 = entry1.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			line1.GetCalculator<UnitCalculator>().PerUnit = 100;
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", container: "40GP");
			var line2 = entry2.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			line2.GetCalculator<UnitCalculator>().PerUnit = 110;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLCTOtoCFS;
			cartage.JJ_ContainerMode = CartageContainerMode.Containerized;
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			// move1 setup
			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			container1.JC_RC = GP20.PK;

			var service1 = container1.Services.AddNew();
			service1.ES_Booked = ZDateTime.Today;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_ServiceCount = 1;

			var cartageMove1 = Factory.New<CommonBookedCtgMove>();
			cartageMove1.EW_JJ = cartage.PK;
			cartageMove1.EW_JC_Container = container1.PK;
			cartageMove1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var cartageLeg11 = cartageMove1.CartageLegs.AddNew();
			cartageLeg11.JU_RunSheetSequence = 1;
			cartageLeg11.JU_EW = cartageMove1.PK;

			var cartageLeg12 = cartageMove1.CartageLegs.AddNew();
			cartageLeg12.JU_RunSheetSequence = 2;
			cartageLeg12.JU_EW = cartageMove1.PK;

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.JC_RC = GP40.PK;

			var service2 = container2.Services.AddNew();
			service2.ES_Booked = ZDateTime.Today;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_ServiceCount = 2;

			var cartageMove2 = Factory.New<CommonBookedCtgMove>();
			cartageMove2.EW_JJ = cartage.PK;
			cartageMove2.EW_JC_Container = container2.PK;
			cartageMove2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var cartageLeg21 = cartageMove2.CartageLegs.AddNew();
			cartageLeg21.JU_RunSheetSequence = 1;
			cartageLeg21.JU_EW = cartageMove2.PK;

			var cartageLeg22 = cartageMove2.CartageLegs.AddNew();
			cartageLeg22.JU_RunSheetSequence = 2;
			cartageLeg22.JU_EW = cartageMove2.PK;

			var cartageLeg23 = cartageMove2.CartageLegs.AddNew();
			cartageLeg23.JU_RunSheetSequence = 3;
			cartageLeg23.JU_EW = cartageMove2.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "PTFUM20",
					JR_LocalCostAmt = 100m // move1: 1 SV x $100/SV
				},
				new AssertionCharge
				{
					ChargeCode = "PTFUM40",
					JR_LocalCostAmt = 220m // move2: 2 SV x $110/SV
				},
			};

			AutorateAndAssert("Regardless of how many legs there should be charges for services from moves.", expected, cartage, NewClient, autorateCosts: false);
		}

		public void TestCommonCartage_Loose_MultipleMovesAndLegs_AutorateServiceCost()
		{
			var chargeCode1 = Helper.ChargeCodes.New("PTFUM", "Port Transport Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);
			var chargeCode2 = Helper.ChargeCodes.New("PTCLN", "Port Transport Cleaning Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Cleaning);

			var costing = Helper.NewCosting(null);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU");
			var line1 = entry.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			line1.GetCalculator<UnitCalculator>().PerUnit = 100;
			var line2 = entry.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			line2.GetCalculator<UnitCalculator>().PerUnit = 110;

			NewClient.OH_IsDebtor = true;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLCTOtoCFS;
			cartage.JJ_ContainerMode = CartageContainerMode.Loose;
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			// move1 setup
			var cartageMove1 = Factory.New<CommonBookedCtgMove>();
			cartageMove1.EW_JJ = cartage.PK;
			cartageMove1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var service1 = cartageMove1.Services.AddNew();
			service1.ES_Booked = ZDateTime.Today;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_ServiceCount = 1;

			var cartageLeg11 = cartageMove1.CartageLegs.AddNew();
			cartageLeg11.JU_RunSheetSequence = 1;
			cartageLeg11.JU_EW = cartageMove1.PK;

			var cartageLeg12 = cartageMove1.CartageLegs.AddNew();
			cartageLeg12.JU_RunSheetSequence = 2;
			cartageLeg12.JU_EW = cartageMove1.PK;

			// move2 setup
			var cartageMove2 = Factory.New<CommonBookedCtgMove>();
			cartageMove2.EW_JJ = cartage.PK;
			cartageMove2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var service2 = cartageMove2.Services.AddNew();
			service2.ES_Booked = ZDateTime.Today;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			service2.ES_ServiceCount = 2;

			var cartageLeg21 = cartageMove2.CartageLegs.AddNew();
			cartageLeg21.JU_RunSheetSequence = 1;
			cartageLeg21.JU_EW = cartageMove2.PK;

			var cartageLeg22 = cartageMove2.CartageLegs.AddNew();
			cartageLeg22.JU_RunSheetSequence = 2;
			cartageLeg22.JU_EW = cartageMove2.PK;

			var cartageLeg23 = cartageMove2.CartageLegs.AddNew();
			cartageLeg23.JU_RunSheetSequence = 3;
			cartageLeg23.JU_EW = cartageMove2.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "PTFUM",
					JR_LocalCostAmt = 100m // move1: 1 FUM SV x $100/SV
				},
				new AssertionCharge
				{
					ChargeCode = "PTCLN",
					JR_LocalCostAmt = 220m // move2: 2 CLN SV x $110/SV
				},
			};

			AutorateAndAssert("Regardless of how many legs there should be charges for services from moves.", expected, cartage, NewClient, autorateRevenue: false);
		}

		public void TestCommonCartage_Loose_MultipleMovesAndLegs_AutorateServiceRevenue()
		{
			var chargeCode1 = Helper.ChargeCodes.New("PTFUM", "Port Transport Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Fumigation);
			var chargeCode2 = Helper.ChargeCodes.New("PTCLN", "Port Transport Cleaning Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, FreightServiceType.Codes.Cleaning);

			var clientRate = Helper.NewClientRate(NewClient);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU");
			var line1 = entry.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			line1.GetCalculator<UnitCalculator>().PerUnit = 100;
			var line2 = entry.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			line2.GetCalculator<UnitCalculator>().PerUnit = 110;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLCTOtoCFS;
			cartage.JJ_ContainerMode = CartageContainerMode.Loose;
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			// move1 setup
			var cartageMove1 = Factory.New<CommonBookedCtgMove>();
			cartageMove1.EW_JJ = cartage.PK;
			cartageMove1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var service1 = cartageMove1.Services.AddNew();
			service1.ES_Booked = ZDateTime.Today;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_ServiceCount = 1;

			var cartageLeg11 = cartageMove1.CartageLegs.AddNew();
			cartageLeg11.JU_RunSheetSequence = 1;
			cartageLeg11.JU_EW = cartageMove1.PK;

			var cartageLeg12 = cartageMove1.CartageLegs.AddNew();
			cartageLeg12.JU_RunSheetSequence = 2;
			cartageLeg12.JU_EW = cartageMove1.PK;

			// move2 setup
			var cartageMove2 = Factory.New<CommonBookedCtgMove>();
			cartageMove2.EW_JJ = cartage.PK;
			cartageMove2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var service2 = cartageMove2.Services.AddNew();
			service2.ES_Booked = ZDateTime.Today;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			service2.ES_ServiceCount = 2;

			var cartageLeg21 = cartageMove2.CartageLegs.AddNew();
			cartageLeg21.JU_RunSheetSequence = 1;
			cartageLeg21.JU_EW = cartageMove2.PK;

			var cartageLeg22 = cartageMove2.CartageLegs.AddNew();
			cartageLeg22.JU_RunSheetSequence = 2;
			cartageLeg22.JU_EW = cartageMove2.PK;

			var cartageLeg23 = cartageMove2.CartageLegs.AddNew();
			cartageLeg23.JU_RunSheetSequence = 3;
			cartageLeg23.JU_EW = cartageMove2.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "PTFUM",
					JR_LocalCostAmt = 100m // move1: 1 SV x $100/SV
				},
				new AssertionCharge
				{
					ChargeCode = "PTCLN",
					JR_LocalCostAmt = 220m // move2: 2 SV x $110/SV
				},
			};

			AutorateAndAssert("Regardless of how many legs there should be charges for services from moves.", expected, cartage, NewClient, autorateCosts: false);
		}

		#endregion

		#region Implementation

		void CreateCartageType(string jobType, bool consignorIsBillToParty)
		{
			var firstOrg = consignorIsBillToParty ? "CNR" : "CNE";
			var secondOrg = consignorIsBillToParty ? "CNE" : "CNR";

			var cartageType = Factory.NewWithValidTestData<CommonCartageType>();
			cartageType.E3_JobType = jobType;
			cartageType.E3_Description = "DOOR TO DOOR";
			cartageType.E3_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			cartageType.E3_ShippingTransportMode = TransportModes.Road;

			var cartageOrg1 = cartageType.CommonCartageOrganisations.AddNew();
			cartageOrg1.E5_OrgType = firstOrg;
			cartageOrg1.E5_IsBillToParty = true;

			var cartageOrg2 = cartageType.CommonCartageOrganisations.AddNew();
			cartageOrg2.E5_OrgType = secondOrg;
			cartageOrg2.E5_IsBillToParty = false;

			var looseLeg = cartageType.LooseCartageLegTypes.AddNew();
			looseLeg.E4_DisplayOrder = 1;
			looseLeg.E4_E5_FromOrg = cartageType.CommonCartageOrganisations[0].PK;
			looseLeg.E4_E5_ToOrg = cartageType.CommonCartageOrganisations[1].PK;
		}

		CommonCartage CreateCartage(string consignmentID, string jobType, ZGuid firstDocAddressPK, ZGuid secondDocAddressPK, ZGuid transportCompanyPK, string dropMode = LCLAIREquipmentNeeded.Premise, decimal cartageWeight = 120m, string cartageWeightUQ = QuantityUnit.KG, decimal cartageVolume = 0.012m, string cartageVolumeUQ = QuantityUnit.M3, decimal cartageMoveWeight = 120m, decimal cartageMoveVolume = 0.012m)
		{
			var now = ZDateTime.Now.AddMonths(2);
			var cartage = Factory.New<CommonCartage>();

			cartage.JJ_A_JCL = now;
			cartage.JJ_ConsignmentID = consignmentID;
			cartage.JJ_DropMode = dropMode;
			cartage.JJ_EstimatedDelivery = now;
			cartage.JJ_EstimatedPickup = now.AddDays(-2);
			cartage.JJ_F3_NKPackType = PkgUnit.Pallet;
			cartage.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartage.JJ_IsCancelled = false;
			cartage.JJ_E3_NKJobType = jobType;
			cartage.JJ_OuterPacks = 12;
			cartage.JJ_Status = "v2";
			cartage.JJ_Volume = cartageVolume;
			cartage.JJ_VolumeUQ = cartageVolumeUQ;
			cartage.JJ_Weight = cartageWeight;
			cartage.JJ_WeightUQ = cartageWeightUQ;
			cartage.FirstDocAddress.E2_OA_Address = firstDocAddressPK;
			cartage.SecondDocAddress.E2_OA_Address = secondDocAddressPK;

			cartage.LooseBookedMoves.DeleteAll();
			var ctgMove = cartage.LooseBookedMoves.AddNew();
			ctgMove.EW_BookedHeight = 10.000m;
			ctgMove.EW_BookedLength = 10.000m;
			ctgMove.EW_BookedPackCount = 12;
			ctgMove.EW_BookedVolume = cartageMoveVolume;
			ctgMove.EW_BookedWeight = cartageMoveWeight;
			ctgMove.EW_BookedWidth = 10.000m;
			ctgMove.EW_DimUnit = "CM";
			ctgMove.EW_DisplayOrder = 1;
			ctgMove.EW_Distance = 0m;
			ctgMove.EW_DistanceUnit = QuantityUnit.KM;
			ctgMove.EW_DropMode = dropMode;
			ctgMove.EW_F3_NKPackType = PkgUnit.Pallet;
			ctgMove.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			ctgMove.EW_E2WaitPointAddressID = cartage.SecondDocAddress.PK;

			var leg = ctgMove.CartageLegs.AddNew();
			leg.JU_CartageDeliveryDemurrage = now.AddMonths(-1);
			leg.JU_CartagePickupDemurrage = now.AddMonths(-1);
			leg.JU_DeliverTimeIn = now;
			leg.JU_DeliverTimeOut = now;
			leg.JU_DeliverySignedFor = "Justine";
			leg.JU_Distance = 0.000m;
			leg.JU_DistanceUnit = QuantityUnit.KM;
			leg.JU_EstimatedDeliveryTime = now;
			leg.JU_IsEmptyContainer = false;
			leg.JU_MessageStatus = "DL";
			leg.JU_PickupTimeIn = now.AddDays(-1);
			leg.JU_PickupTimeOut = now.AddDays(-1);
			leg.JU_PlannedPickupTime = now.AddDays(-2);
			leg.QuickOHTransportCompany = transportCompanyPK;

			return cartage;
		}

		CommonCartage CreateContainerisedCartage(OrgHeader client)
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLImportToCNE;
			cartage.JJ_ContainerMode = CartageContainerMode.Containerized;
			cartage.LocalClientAddressPK = client.MainAddress.PK;

			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			container1.JC_RC = RefContainer.PK;

			var move1 = Factory.New<CommonBookedCtgMove>();
			move1.EW_JJ = cartage.PK;
			move1.EW_JC_Container = container1.PK;
			move1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg1 = move1.CartageLegs.AddNew();
			leg1.JU_RunSheetSequence = 1;
			leg1.JU_EW = move1.PK;

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.JC_RC = RefContainer.PK;

			var move2 = Factory.New<CommonBookedCtgMove>();
			move2.EW_JJ = cartage.PK;
			move2.EW_JC_Container = container2.PK;
			move2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg2 = move2.CartageLegs.AddNew();
			leg2.JU_RunSheetSequence = 2;
			leg2.JU_EW = move2.PK;

			return cartage;
		}

		RefContainer RefContainer
		{
			get { return refContainer ?? (refContainer = Helper.Containers["20GP"]); }
		}

		RefContainer refContainer;

		#endregion
	}
}
