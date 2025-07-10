using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.FreightInclusiveCalculator;
using Directions = Enterprise.MasterFiles.Business.Directions;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.RatingTests.Testing.GUI
{
	// Stop adding tests to this file.It becomes unmaintainable. Please create separate logical files testing specific area of rating.
	// For example:
	//	\Dev\Enterprise\Product\Operations\Rating\Rating.Testing\BusinessTests\Autorating\AutoratingForwardingShipmentIntegrationTest.cs
	//  \Dev\Enterprise\Product\Operations\Rating\Rating.Testing\BusinessTests\GatewayBilling\

	public class RatingIntegrationBusinessTest : BaseRatingIntegrationTest
	{
		#region TestRatesWithFeeChargeTypeAndLevelAutoratedCorrectly

		public void TestRatesWithFeeChargeTypeAndLevelAutoratedCorrectlyWhenSetInCompanyTariff()
		{
			const string DomesticWarranty = "DWY";
			const string FuelSurcharge = "FSE";
			const string StandardLevel = "STD";
			const string NewLevel = "NEW";
			const string NewLevelDescription = "NEW Description";

			var chargeLevelSection = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var fuelSurchargeChargeType = chargeLevelSection.FeeChargeTypes
				.Cast<FeeChargeType>()
				.First(x => x.Code == FuelSurcharge);

			var newFeeChargeLevel = fuelSurchargeChargeType.FeeChargeLevels.AddNew();
			newFeeChargeLevel.Code = NewLevel;
			newFeeChargeLevel.EnglishDescription = NewLevelDescription;
			newFeeChargeLevel.Amount1Type = "NON";
			newFeeChargeLevel.Amount1 = 0;
			newFeeChargeLevel.Amount1Currency = "AUD";
			newFeeChargeLevel.Amount2Type = "NON";
			newFeeChargeLevel.Amount2 = 0;
			newFeeChargeLevel.Amount2Currency = "AUD";

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeLevelSection);

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();

			#region Create Rate

			var rate = Factory.New<CompanyTariff>();
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var rateLine2 = rateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 150m;
			rateLine2.TL_FeeChargeType = DomesticWarranty;
			rateLine2.TL_FeeChargeLevel = StandardLevel;

			var rateLine3 = rateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 200m;
			rateLine3.TL_FeeChargeType = FuelSurcharge;
			rateLine3.TL_FeeChargeLevel = StandardLevel;

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUBNE", "USLAX", 300);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			Factory.Save();

			#region Asserts

			var feeChargeLevel = consignee.CompanyData.RateFeeChargeLevels.AddNew();
			feeChargeLevel.ORF_ServiceType = FuelSurcharge;
			feeChargeLevel.ORF_Level = StandardLevel;

			var feeChargeTypeDescription = DescriptionHelpers.FormatWithTab("Fee Charge Type:");
			var feeChargeLevelDescription = DescriptionHelpers.FormatWithTab("Fee Charge Level:");

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  200m,
							RevenueCalculationDescription = $"{feeChargeTypeDescription}FSE - Fuel Surcharge" + System.Environment.NewLine + $"{feeChargeLevelDescription}STD - Standard Level"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			feeChargeLevel.ORF_Level = NewLevel;

			AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, localClient);

			rateLine3.TL_FeeChargeLevel = NewLevel;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  200m,
							RevenueCalculationDescription = $"{feeChargeTypeDescription}FSE - Fuel Surcharge" + System.Environment.NewLine + $"{feeChargeLevelDescription}NEW - NEW Description"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			var exportCollectPriorities = RatingDataRegistry.Instance.ExportCollectPriorities.Value;
			var consigneeType = exportCollectPriorities
				.Cast<RatesPriorities>()
				.First(x => x.OrganizationType == "CNE");
			exportCollectPriorities.Remove(consigneeType);
			RatingDataRegistry.Instance.ExportCollectPriorities.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, exportCollectPriorities);

			AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, localClient);

			RatingDataRegistry.Instance.ExportCollectPriorities.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RatingDataRegistry.Instance.ExportCollectPriorities.DefaultValue);

			consignee.CompanyData.RateFeeChargeLevels.DeleteAll();

			feeChargeLevel = consignor.CompanyData.RateFeeChargeLevels.AddNew();
			feeChargeLevel.ORF_ServiceType = FuelSurcharge;
			feeChargeLevel.ORF_Level = StandardLevel;

			Factory.Save();

			AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, localClient);

			#endregion
		}

		public void TestRatesWithFeeChargeTypeAndLevelAutoratedCorrectlyWhenSetInCompanyTariffWith2Level()
		{
			const string DomesticWarranty = "DWY";
			const string FuelSurcharge = "FSE";
			const string StandardLevel = "STD";

			var localClient = Helper.NewOrgHeader(2);
			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader(1);

			#region Create Rate

			var companyTariffLevel1 = Factory.New<CompanyTariff>();
			var rateEntry1 = companyTariffLevel1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var rateLine2 = rateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 150m;
			rateLine2.TL_FeeChargeType = DomesticWarranty;
			rateLine2.TL_FeeChargeLevel = StandardLevel;

			var rateLine3 = rateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 200m;
			rateLine3.TL_FeeChargeType = FuelSurcharge;
			rateLine3.TL_FeeChargeLevel = StandardLevel;

			var companyTariffLevel2 = Factory.New<CompanyTariff>();
			companyTariffLevel2.TH_GlobalRateLevel = 2;

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUBNE", "USLAX", 300);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			Factory.Save();

			#region Asserts

			var feeChargeLevel = consignee.CompanyData.RateFeeChargeLevels.AddNew();
			feeChargeLevel.ORF_ServiceType = FuelSurcharge;
			feeChargeLevel.ORF_Level = StandardLevel;
			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  200m,
							RevenueCalculationDescription = $"{DescriptionHelpers.FormatWithTab("Fee Charge Type:")}FSE - Fuel Surcharge" + System.Environment.NewLine + $"{DescriptionHelpers.FormatWithTab("Fee Charge Level:")}STD - Standard Level"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion
		}

		public void TestRatesWithFeeChargeTypeAndLevelAutoratedCorrectlyWhenSetInCompanyTariffWithConsigneeHigherPriority()
		{
			const string DomesticWarranty = "DWY";
			const string StandardLevel = "STD";
			const string NewLevel = "NLV";
			const string NewLevelDescription = "NewLevelDescription";

			var priorities = RatingDataRegistry.Instance.ExportCollectPriorities.Value;

			var consigneeRatesPriorities = priorities.Cast<RatesPriorities>().FirstOrDefault(x => x.OrganizationType == "CNE");
			consigneeRatesPriorities.UseCompanyTariff = false;

			var consignorRatesPriorities = priorities.AddNew();
			consignorRatesPriorities.OrganizationType = "CNR";
			consignorRatesPriorities.UseCompanyTariff = true;

			AssertEquals(priorities.Cast<RatesPriorities>().ElementAt(2).OrganizationType, "CNR");

			RatingDataRegistry.Instance.ExportCollectPriorities.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, priorities);

			#region AddFeeChargeLevel

			var chargeLevelSection = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var domesticWarrantyCharge = chargeLevelSection.FeeChargeTypes
				.Cast<FeeChargeType>()
				.First(x => x.Code == DomesticWarranty);

			var newFeeChargeLevel = domesticWarrantyCharge.FeeChargeLevels.AddNew();
			newFeeChargeLevel.Code = NewLevel;
			newFeeChargeLevel.EnglishDescription = NewLevelDescription;
			newFeeChargeLevel.Amount1Type = "NON";
			newFeeChargeLevel.Amount1 = 0;
			newFeeChargeLevel.Amount1Currency = "AUD";
			newFeeChargeLevel.Amount2Type = "NON";
			newFeeChargeLevel.Amount2 = 0;
			newFeeChargeLevel.Amount2Currency = "AUD";

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeLevelSection);

			#endregion

			#region Create Organizations

			var localClient = Helper.NewOrgHeader();

			var consignee = Helper.NewOrgHeader();
			var consigneeFeeChargeLevel = consignee.CompanyData.RateFeeChargeLevels.AddNew();
			consigneeFeeChargeLevel.ORF_ServiceType = "DWY";
			consigneeFeeChargeLevel.ORF_Level = "STD";

			var consignor = Helper.NewOrgHeader();
			var consignorFeeChargeLevel = consignor.CompanyData.RateFeeChargeLevels.AddNew();
			consignorFeeChargeLevel.ORF_ServiceType = "DWY";
			consignorFeeChargeLevel.ORF_Level = NewLevel;

			#endregion

			#region Create Rate

			var companyTariff = Factory.New<CompanyTariff>();
			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var rateLine2 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 340m;
			rateLine2.TL_FeeChargeType = DomesticWarranty;
			rateLine2.TL_FeeChargeLevel = NewLevel;

			var rateLine3 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 150;
			rateLine3.TL_FeeChargeType = DomesticWarranty;
			rateLine3.TL_FeeChargeLevel = StandardLevel;

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUBNE", "USLAX", 300);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			Factory.Save();

			#region Asserts

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  150m,
							RevenueCalculationDescription = $"{DescriptionHelpers.FormatWithTab("Fee Charge Type:")}DWY - Domestic Warranty" + System.Environment.NewLine + $"{DescriptionHelpers.FormatWithTab("Fee Charge Level:")}STD - Standard Level"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion
		}

		public void TestRatesWithFeeChargeTypeAndLevelAutoratedCorrectlyWhenSetInCompanyTariffAndConsigneeAndConsignorHaveHigherThanZeroLevel()
		{
			const string DomesticWarranty = "DWY";
			const string StandardLevel = "STD";
			const string NewLevel = "NLV";
			const string NewLevelDescription = "NewLevelDescription";

			#region Set Priorities

			var priorities = RatingDataRegistry.Instance.ExportCollectPriorities.Value;

			var consigneeRatesPriorities = priorities.Cast<RatesPriorities>().FirstOrDefault(x => x.OrganizationType == "CNE");
			consigneeRatesPriorities.UseCompanyTariff = false;

			var consignorRatesPriorities = priorities.AddNew();
			consignorRatesPriorities.OrganizationType = "CNR";
			consignorRatesPriorities.UseCompanyTariff = true;

			AssertEquals(priorities.Cast<RatesPriorities>().ElementAt(2).OrganizationType, "CNR");

			RatingDataRegistry.Instance.ExportCollectPriorities.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, priorities);

			#endregion

			#region AddFeeChargeLevel

			var chargeLevelSection = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var domesticWarrantyCharge = chargeLevelSection.FeeChargeTypes
				.Cast<FeeChargeType>()
				.First(x => x.Code == DomesticWarranty);

			var newFeeChargeLevel = domesticWarrantyCharge.FeeChargeLevels.AddNew();
			newFeeChargeLevel.Code = NewLevel;
			newFeeChargeLevel.EnglishDescription = NewLevelDescription;
			newFeeChargeLevel.Amount1Type = "NON";
			newFeeChargeLevel.Amount1 = 0;
			newFeeChargeLevel.Amount1Currency = "AUD";
			newFeeChargeLevel.Amount2Type = "NON";
			newFeeChargeLevel.Amount2 = 0;
			newFeeChargeLevel.Amount2Currency = "AUD";

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeLevelSection);

			#endregion

			#region Create Organizations

			var localClient = Helper.NewOrgHeader();

			var consignee = Helper.NewOrgHeader(1);
			var consigneeFeeChargeLevel = consignee.CompanyData.RateFeeChargeLevels.AddNew();
			consigneeFeeChargeLevel.ORF_ServiceType = "DWY";
			consigneeFeeChargeLevel.ORF_Level = "STD";

			var consignor = Helper.NewOrgHeader(2);
			var consignorFeeChargeLevel = consignor.CompanyData.RateFeeChargeLevels.AddNew();
			consignorFeeChargeLevel.ORF_ServiceType = "DWY";
			consignorFeeChargeLevel.ORF_Level = NewLevel;

			#endregion

			#region Create Rate

			var companyTariff1 = Factory.New<CompanyTariff>();
			var rateEntry = companyTariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var rateLine2 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 340m;
			rateLine2.TL_FeeChargeType = DomesticWarranty;
			rateLine2.TL_FeeChargeLevel = NewLevel;

			var rateLine3 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 150;
			rateLine3.TL_FeeChargeType = DomesticWarranty;
			rateLine3.TL_FeeChargeLevel = StandardLevel;

			var companyTariff2 = Factory.New<CompanyTariff>();
			companyTariff2.TH_GlobalRateLevel = 2;

			companyTariff1.Factory.Save();
			companyTariff2.Factory.Save();

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUBNE", "USLAX", 300);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			Factory.Save();

			#region Asserts

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  150m,
							RevenueCalculationDescription = $"{DescriptionHelpers.FormatWithTab("Fee Charge Type:")}DWY - Domestic Warranty" + System.Environment.NewLine + $"{DescriptionHelpers.FormatWithTab("Fee Charge Level:")}STD - Standard Level"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion
		}

		public void TestRatesWithFeeChargeTypeAndLevelAutoratedCorrectlyWhenSetInCosting()
		{
			const string DomesticWarranty = "DWY";
			const string FuelSurcharge = "FSE";
			const string StandardLevel = "STD";
			const string NewLevel = "NEW";
			const string NewLevelDescription = "NEW Description";

			var chargeLevelSection = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var fuelSurchargeChargeType = chargeLevelSection.FeeChargeTypes
				.Cast<FeeChargeType>()
				.First(x => x.Code == FuelSurcharge);

			var newFeeChargeLevel = fuelSurchargeChargeType.FeeChargeLevels.AddNew();
			newFeeChargeLevel.Code = NewLevel;
			newFeeChargeLevel.EnglishDescription = NewLevelDescription;
			newFeeChargeLevel.Amount1Type = "NON";
			newFeeChargeLevel.Amount1 = 0;
			newFeeChargeLevel.Amount1Currency = "AUD";
			newFeeChargeLevel.Amount2Type = "NON";
			newFeeChargeLevel.Amount2 = 0;
			newFeeChargeLevel.Amount2Currency = "AUD";

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeLevelSection);

			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();

			#region Create Cost

			var cost = Factory.New<Costing>();
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			costEntry.TI_RX_NKCurrency = "AUD";
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine1 = costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var costLine2 = costEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLine2.GetCalculator<FlatCalculator>().BaseRate = 150m;
			costLine2.TL_FeeChargeType = DomesticWarranty;
			costLine2.TL_FeeChargeLevel = StandardLevel;

			var costLine3 = costEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLine3.GetCalculator<FlatCalculator>().BaseRate = 200m;
			costLine3.TL_FeeChargeType = FuelSurcharge;
			costLine3.TL_FeeChargeLevel = StandardLevel;

			#endregion

			#region Create shipment and Consol

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUBNE", "USLAX", 300);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			#endregion

			Factory.Save();

			#region Asserts

			var expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 1500m,
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("", null, expectedCosts, consol, false);

			costEntry.TI_OH_TransportProvider = carrier.PK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("", null, expectedCosts, consol, false);

			var feeChargeLevel = carrier.CompanyData.RateFeeChargeLevels.AddNew();
			feeChargeLevel.ORF_ServiceType = FuelSurcharge;
			feeChargeLevel.ORF_Level = StandardLevel;

			var feeChargeTypeDescription = DescriptionHelpers.FormatWithTab("Fee Charge Type:");
			var feeChargeLevelDescription = DescriptionHelpers.FormatWithTab("Fee Charge Level:");

			expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 1700m,
								CostCalculationDescription = $"{feeChargeTypeDescription}FSE - Fuel Surcharge" + System.Environment.NewLine + $"{feeChargeLevelDescription}STD - Standard Level"
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("", null, expectedCosts, consol, false);

			feeChargeLevel.ORF_Level = NewLevel;

			expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 1500m,
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("", null, expectedCosts, consol, false);

			costLine3.TL_FeeChargeLevel = NewLevel;

			Factory.Save();

			expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 1700m,
								CostCalculationDescription = $"{feeChargeTypeDescription}FSE - Fuel Surcharge" + System.Environment.NewLine + $"{feeChargeLevelDescription}NEW - NEW Description"
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("", null, expectedCosts, consol, false);

			#endregion
		}

		public void TestReturnRateLinesForTariffLevelSetOnOrg_UnlessLevelZero()
		{
			var cnr = Helper.NewOrgHeader();
			var cne = Helper.NewOrgHeader(2);

			var tariff1 = Factory.New<CompanyTariff>();
			var tariffRate1 = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE").AddRateLine("FRT", FlatCalculator.Code);
			tariffRate1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var tariff2 = Factory.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 20m);

			var clientRate = Helper.NewClientRate(cne);
			var line = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE").AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			AddParityExchangeRate(line.Currency);

			var shipment = CreateForwardingShipment(TransportModes.Air, cnr.PK, cne.PK, "USLAX", "AUBNE", 3000);

			Factory.Save();

			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 88,
							}
					};

			AutorateAndAssert(expected, shipment, cne);

			cne.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			Factory.Save();

			expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 110,
							}
					};

			AutorateAndAssert(expected, shipment, cne);
		}

		public void TestReturnRateLinesForTariffLevelSetOnOrgWithFeeChargeType_UnlessLevelZero()
		{
			var cnr = Helper.NewOrgHeader();
			var cne = Helper.NewOrgHeader(0);
			cne.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			var tariff1 = Factory.New<CompanyTariff>();
			var tariffRateEntry1 = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			tariffRateEntry1.RateLines.RemoveAndDeleteAll();
			var tariffRateLine1 = tariffRateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			tariffRateLine1.TL_FeeChargeType = "DWY";
			tariffRateLine1.TL_FeeChargeLevel = "STD";
			tariffRateLine1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var tariffRateLine2 = tariffRateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			tariffRateLine2.TL_FeeChargeType = "INW";
			tariffRateLine2.TL_FeeChargeLevel = "STD";
			tariffRateLine2.GetCalculator<FlatCalculator>().BaseRate = 50m;

			var tariff2 = Factory.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 20m);

			var clientRate = Helper.NewClientRate(cne);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(clientRateEntry.Currency);
			var line = clientRateEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			line.TL_FeeChargeType = "DWY";
			line.TL_FeeChargeLevel = "STD";

			var shipment = CreateForwardingShipment(TransportModes.Air, cnr.PK, cne.PK, "USLAX", "AUBNE", 3000);

			Factory.Save();

			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 0,
							}
					};

			AutorateAndAssert(expected, shipment, cne);

			var feesAndChargesLevel1 = cne.CompanyData.RateFeeChargeLevels.AddNew();
			feesAndChargesLevel1.ORF_ServiceType = "DWY";
			feesAndChargesLevel1.ORF_Level = "STD";

			Factory.Save();

			expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 110,
							}
					};

			AutorateAndAssert(expected, shipment, cne);

			Factory.Save();

			expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 110,
							}
					};

			AutorateAndAssert(expected, shipment, cne);

			line.TL_FeeChargeType = string.Empty;
			line.TL_FeeChargeLevel = string.Empty;

			Factory.Save();

			expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 100,
							},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							RevenueCalculationDescription = "Calculation failed due to charge code is using the Company Tariff Based Calculator, however there are conflicting or no tariff rates found."
						}
					};

			AutorateAndAssert(expected, shipment, cne);
		}

		public void TestRateLinesWithSameChargeCodeWithFeeChargeInCompanyTariff()
		{
			var cnr = Helper.NewOrgHeader();
			var cne = Helper.NewOrgHeader(0);

			var companyTariff = Factory.New<CompanyTariff>();
			var tariffRate = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE").AddRateLine("FRT", FlatCalculator.Code);
			tariffRate.TL_FeeChargeType = "DWY";
			tariffRate.TL_FeeChargeLevel = "STD";
			tariffRate.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var clientRate = Helper.NewClientRate(cne);
			var line = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE").AddRateLine("FRT", FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 30m;
			AddParityExchangeRate(line.Currency);

			var shipment = CreateForwardingShipment(TransportModes.Air, cnr.PK, cne.PK, "USLAX", "AUBNE", 3000);

			var feesAndChargesLevel = cne.CompanyData.RateFeeChargeLevels.AddNew();
			feesAndChargesLevel.ORF_ServiceType = "DWY";
			feesAndChargesLevel.ORF_Level = "STD";

			Factory.Save();

			var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 100,
							RevenueCalculationDescription = "Charge located in Company Tariff Level 1 (Linked to: TESTORG2) with the following details:"
						},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 30,
							RevenueCalculationDescription = "Charge located in TESTORG2 client rate with the following details:"
						}
					};

			AutorateAndAssert(expected, shipment, cne);
		}

		#endregion

		#region TestChargesNotDuplicatedWhenChargeGroupInvalid

		[TestDate(2014, 6, 11)]
		public void TestChargesNotDuplicatedWhenChargeGroupInvalid()
		{
			Env.Registry.Rating.SetFreightRatedCodes("ORG,LOD,UNL,DST,INS,FRT,BON,NGC");

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var chargeCode = Helper.ChargeCodes.New("AAA", "Desc", FlatCalculator.Code, "BON");

			var rate = Helper.NewClientRate(localClient);

			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry1.TI_RateStartDate = new ZDate(2014, 1, 1);
			rateEntry1.TI_RateEndDate = new ZDate(2014, 6, 9);

			var rateLine1 = rateEntry1.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry2.TI_RateStartDate = new ZDate(2014, 6, 10);
			rateEntry2.RateLines.RemoveAndDeleteAll();

			var rateLine2 = rateEntry2.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 30m;
			AddParityExchangeRate(rateLine2.Currency);

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20);
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "AAA",
							JR_OSSellAmt =  30,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;
			Factory.Save();

			AutorateAndAssert(expected, shipment, localClient);
		}

		#endregion

		#region TestAutoRatingWithGroupClientRates

		public void TestAutoRatingWithGroupClientRates()
		{
			var consignee = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader();
			var groupClient1 = Helper.NewOrgHeader();
			var groupClient2 = Helper.NewOrgHeader();
			Factory.Save();

			groupClient1.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient);
			groupClient2.RelatedManagementSubsidiaryRelations.AddOrganisation(groupClient1);
			Factory.Save();

			AssertEquals("Precondition: groupClient1 should have 1 subsidiary company", 1, groupClient1.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Precondition: groupClient2 should have 1 subsidiary company", 1, groupClient2.RelatedManagementSubsidiaryRelations.Organisations.Count());

			#region Rates Setup

			var clientRate = Helper.NewClientRateWithSingleRateLine(localClient, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 10m);
			var groupClientRate1 = Helper.NewClientRateWithSingleRateLine(groupClient1, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 20m);
			var groupClientRate2 = Helper.NewClientRateWithSingleRateLine(groupClient2, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 30m);

			var clientRateEntry = clientRate.GetRateEntryCollectionForCategory("AIR")[0];
			var groupRateEntry1 = groupClientRate1.GetRateEntryCollectionForCategory("AIR")[0];
			var groupRateEntry2 = groupClientRate2.GetRateEntryCollectionForCategory("AIR")[0];

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 1000);
			Factory.Save();

			#region expected rate

			var expectedClientRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m,
					JR_OSCostAmt = 10m,
				},
			};

			var expectedGroupClientRate1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 20m,
					JR_OSCostAmt = 20m,
				},
			};

			var expectedGroupClientRate2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
					JR_OSCostAmt = 30m,
				},
			};

			var expectedGroupClientRate1AndGroupClientRate2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 20m,
					JR_OSCostAmt = 20m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
					JR_OSCostAmt = 30m,
				},
			};

			var expectedNoRate = Array.Empty<AssertionCharge>();

			#endregion

			AutorateAndAssert("Should load the client rate from localClient", expectedClientRate, shipment, localClient);

			var defaultLevel = localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel.P7_ApplyGroupRate = true;

			clientRateEntry.TI_DestinationLRC = "AUMEL";
			Factory.Save();
			// breaking change WI00238340: rate from groupClient2 does not fail similarity check because it is NOT applicable to org groupClient1
			AutorateAndAssert("Should load the group client rate from groupClient1 and groupClient2", expectedGroupClientRate1AndGroupClientRate2, shipment, localClient);

			defaultLevel = groupClient1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			defaultLevel.P7_ApplyGroupRate = true;
			Factory.Save();

			AutorateAndAssert("Should load the group client rates from groupClient1 and groupClient2 but the latter fails of similarity", expectedGroupClientRate1, shipment, localClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Line filter message", "Information: RateLine Filtered FRT-FLT-Client Rate TESTORG4	reason:	overridden by FRT-FLT-Client Rate TESTORG3 by Rate Type comparer");

			groupRateEntry1.TI_DestinationLRC = "AUMEL";
			Factory.Save();
			AutorateAndAssert("Should load the group client rate from groupClient2", expectedGroupClientRate2, shipment, localClient);

			groupRateEntry2.TI_DestinationLRC = "AUMEL";
			Factory.Save();
			AutorateAndAssert("Should have no rate found", expectedNoRate, shipment, localClient);
		}

		public void TestAutoRatingWithGroupClientRates_CorrectRateApplies()
		{
			var localClient = Helper.NewOrgHeader();
			var groupClient = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			Factory.Save();

			var defaultLevel = localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel.P7_ApplyGroupRate = true;

			groupClient.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient);

			#region Rates Setup

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			var rate = Helper.NewClientRate(groupClient);
			var rateEntry1 = rate.AddRateEntryWithFlatRateLine("DST", "LSE", "", "AUSYD", "DDOC", 10m);
			var rateEntry2 = rate.AddRateEntryWithFlatRateLine("DST", "LSE", "", "AUSYD", "DDOC", 20m);
			rateEntry2.TI_RS_NKServiceLevel_NI = "AAA";

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.MainAddress.PK, localClient.MainAddress.PK, "AUBNE", "AUSYD", 100);
			shipment.JS_RS_NKServiceLevel = "AAA";
			shipment.JS_PackingMode = "LSE";

			Factory.Save();

			var expectedClientRate = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_OSSellAmt = 20m,
						},
				};

			AutorateAndAssert("Should load the client rate from groupClient and correct service level", expectedClientRate, shipment, localClient);
		}

		public void TestAutoRatingWithGroupClientRates_CompanyTariff()
		{
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();
			var groupClient = Helper.NewOrgHeader();
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			Factory.Save();

			var companyTariff = Helper.NewLevel1CompanyTariffWithSingleRateLine("AIR", "LSE", "USLAX", "AUBNE", "FRT", 100m);
			var groupClientRate = Helper.NewClientRateWithSingleRateLine(groupClient, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 30m);

			#region expected rate

			var expectedCompanyTariffRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
				}
			};

			var expectedGroupClientRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
				}
			};

			var expectedClientRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m,
				}
			};

			#endregion

			var defaultLevel = localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel.P7_ApplyGroupRate = true;

			AutorateAndAssert("Should load rates from Company Tariff", expectedCompanyTariffRate, shipment, localClient);

			groupClient.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient);
			Factory.Save();
			AssertEquals("Precondition: Should have 1 subsidiary company", 1, groupClient.RelatedManagementSubsidiaryRelations.Organisations.Count());

			AutorateAndAssert("Should load rates from Group Client Rates ", expectedGroupClientRate, shipment, localClient);

			var localClientRate = Helper.NewClientRateWithSingleRateLine(localClient, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 10m);

			AutorateAndAssert("Should load rates from Client Rates", expectedClientRate, shipment, localClient);
		}

		public void TestAutoRatingWithGroupClientRates_NonDefaultTariff()
		{
			var childClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();
			var parentCompany = Helper.NewOrgHeader();
			Factory.Save();

			parentCompany.RelatedManagementSubsidiaryRelations.AddOrganisation(childClient);
			Factory.Save();
			AssertEquals("Precondition: Should have 1 subsidiary company", 1, parentCompany.RelatedManagementSubsidiaryRelations.Organisations.Count());

			var shipment = CreateForwardingShipment(TransportModes.Air, childClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			Factory.Save();

			Helper.NewLevel1CompanyTariffWithSingleRateLine("AIR", "LSE", "USLAX", "AUBNE", "FRT", 100m);

			#region expected rate

			var expectedCompanyTariffRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
				}
			};

			var expectedParentRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
				}
			};

			var expectedChildRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m,
				}
			};

			#endregion

			AutorateAndAssert("Should load rates from Company Tariff", expectedCompanyTariffRate, shipment, childClient);

			Helper.NewClientRateWithSingleRateLine(parentCompany, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 30m);

			var level = childClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			level.P7_ApplyGroupRate = false;

			var airRateTariff = childClient.CompanyData.RateTariffLevels.AddNew();
			airRateTariff.P7_TariffLevel = 0;
			airRateTariff.P7_ApplyGroupRate = true;
			airRateTariff.P7_Mode = "LSE";
			airRateTariff.P7_TariffType = "FRT";

			AutorateAndAssert("Should load rates from Group Client Rates ", expectedParentRate, shipment, childClient);

			Helper.NewClientRateWithSingleRateLine(childClient, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 10m);
			AutorateAndAssert("Should load rates from Client Rates", expectedChildRate, shipment, childClient);
		}

		public void TestAutoRatingWithGroupClientRates_CompanyTariffLevelAreSame()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var groupClient = Helper.NewOrgHeader(1);
			Factory.Save();

			groupClient.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient);
			Factory.Save();

			AssertEquals("Precondition: Should have 1 subsidiary company", 1, groupClient.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Precondition: company tariff level (LocalClient)", 0, localClient.CompanyData.RateTariffLevels.DefaultLevel);
			AssertEquals("Precondition: company tariff level (GroupClient)", 1, groupClient.CompanyData.RateTariffLevels.DefaultLevel);

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			Factory.Save();

			var companyTariff = Helper.NewLevel1CompanyTariffWithSingleRateLine("AIR", "LSE", "USLAX", "AUBNE", "FRT", 100m);

			#region expected rate

			var expectedCompanyTariffRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
					RevenueCalculationDescription = "Charge located in Company Tariff Level 1 (Linked to: TESTORG1) with the following details:"
				}
			};

			var expectedNoRate = Array.Empty<AssertionCharge>();

			#endregion

			AutorateAndAssert("Should have no rate found", expectedNoRate, shipment, localClient);

			var defaultLevel = localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel.P7_ApplyGroupRate = true;
			Factory.Save();

			AssertEquals("Default company tariff level (LocalClient)", 1, localClient.CompanyData.RateTariffLevels.DefaultLevel);
			AssertEquals("Default company tariff level (GroupClient)", 1, groupClient.CompanyData.RateTariffLevels.DefaultLevel);
			AutorateAndAssert("Should load rates from Company Tariff", expectedCompanyTariffRate, shipment, localClient);
		}

		public void TestAutoRatingWithGroupClientRates_CompanyTariffLevelAreDifferent()
		{
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();
			var groupClient = Helper.NewOrgHeader(1);
			Factory.Save();

			groupClient.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient);
			Factory.Save();
			AssertEquals("Precondition: Should have 1 subsidiary company", 1, groupClient.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Precondition: company tariff level (LocalClient)", 1, localClient.CompanyData.RateTariffLevels.DefaultLevel);
			AssertEquals("Precondition: company tariff level (GroupClient)", 1, groupClient.CompanyData.RateTariffLevels.DefaultLevel);

			var companyTariff1 = Helper.NewLevel1CompanyTariffWithSingleRateLine("AIR", "LSE", "USLAX", "AUBNE", "FRT", 100m);
			var companyTariff2 = Helper.NewNonLevel1CompanyTariff(2, discountType: RatingConstants.RateCategory.AIR, discount: 10m);

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			Factory.Save();

			#region expected rate

			var expectedCompanyTariffRate1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
					RevenueCalculationDescription = "Charge located in Company Tariff Level 1 (Linked to: TESTORG1) with the following details:"
				}
			};

			var expectedCompanyTariffRate2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 90m,
					RevenueCalculationDescription = "Charge located in Company Tariff Level 2 (Linked to: TESTORG1) with the following details:"
				}
			};

			#endregion

			AutorateAndAssert("Should load rates from Company Tariff from Level 1", expectedCompanyTariffRate1, shipment, localClient);

			var defaultLevel = localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			AssertEquals("company tariff level (LocalClient)", 2, localClient.CompanyData.RateTariffLevels.DefaultLevel);
			AssertEquals("company tariff level (GroupClient)", 1, groupClient.CompanyData.RateTariffLevels.DefaultLevel);
			AutorateAndAssert("Should load rates from Company Tariff from Level 2", expectedCompanyTariffRate2, shipment, localClient);
		}

		public void TestAutoRatingWithGroupClientRates_DifferentGroup()
		{
			var localClient1 = Helper.NewOrgHeader(1);
			var localClient2 = Helper.NewOrgHeader(1);
			var localClient3 = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();
			var groupClient = Helper.NewOrgHeader(1);
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient3.PK, consignee.PK, "USLAX", "AUBNE", 100, 0);
			Factory.Save();

			var expectedNoRate = Array.Empty<AssertionCharge>();
			AutorateAndAssert("Precondition: Should have no rate found", expectedNoRate, shipment, localClient3);

			var companyTariff1 = Helper.NewLevel1CompanyTariffWithSingleRateLine("AIR", "LSE", "USLAX", "AUBNE", "FRT", 200m);
			var groupClientRate = Helper.NewClientRateWithSingleRateLine(groupClient, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 100m);
			var localClientRate1 = Helper.NewClientRateWithSingleRateLine(localClient1, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 10m);
			var localClientRate2 = Helper.NewClientRateWithSingleRateLine(localClient2, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 20m);

			#region expected rate

			var expectedCompanyTariffRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m,
					RevenueCalculationDescription = "Charge located in Company Tariff Level 1 (Linked to: TESTORG3) with the following details:"
				}
			};

			var expectedGroupClientRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
					RevenueCalculationDescription = "Charge located in TESTORG5 group client rate (Linked to client rate: TESTORG3) with the following details"
				}
			};

			var expectedClientRate1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = "Charge located in TESTORG1 group client rate (Linked to client rate: TESTORG3) with the following details"
				}
			};

			var expectedClientRate3 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
				}
			};
			#endregion

			var defaultLevel1 = localClient1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel1.P7_ApplyGroupRate = true;
			var defaultLevel2 = localClient2.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel2.P7_ApplyGroupRate = true;
			var defaultLevel3 = localClient3.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel3.P7_ApplyGroupRate = true;

			AutorateAndAssert("Should load rates from Company Tariff", expectedCompanyTariffRate, shipment, localClient3);

			groupClient.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient1);
			groupClient.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient2);
			groupClient.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient3);
			Factory.Save();

			AssertEquals("Should have 3 subsidiary companies", 3, groupClient.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AutorateAndAssert("Should load rates from Group Client Rate (GroupClient1)", expectedGroupClientRate, shipment, localClient3);

			groupClient.RelatedManagementSubsidiaryRelations.RemoveOrganisation(localClient2);
			groupClient.RelatedManagementSubsidiaryRelations.RemoveOrganisation(localClient3);
			localClient1.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient2);
			localClient1.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient3);
			Factory.Save();

			AssertEquals("Should have 1 subsidiary companies", 1, groupClient.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Should have 2 subsidiary companies", 2, localClient1.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AutorateAndAssert("Should load rates from Group Client Rate (LocalClient1)", expectedClientRate1, shipment, localClient3);

			var localClientRate3 = Helper.NewClientRateWithSingleRateLine(localClient3, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 30m);
			AutorateAndAssert("Should load rates from Client Rate", expectedClientRate3, shipment, localClient3);
		}

		public void TestAutoRatingWithGroupClientRates_AllRatingDebtorOrgTypes()
		{
			var localClient = Helper.NewOrgHeader(1);
			localClient.OH_FullName = "localClient";
			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_FullName = "consignor";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_FullName = "consignee";
			var agent = Helper.NewOrgHeader(1);
			agent.OH_FullName = "agent";

			var groupClient1 = Helper.NewOrgHeader();
			groupClient1.OH_FullName = "groupClient1";
			var groupClient2 = Helper.NewOrgHeader();
			groupClient2.OH_FullName = "groupClient2";
			var groupClient3 = Helper.NewOrgHeader();
			groupClient3.OH_FullName = "groupClient3";

			Factory.Save();

			groupClient1.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient);
			groupClient2.RelatedManagementSubsidiaryRelations.AddOrganisation(consignor);
			groupClient3.RelatedManagementSubsidiaryRelations.AddOrganisation(consignee);

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			Factory.Save();

			AssertEquals("Precondition: Should have 1 subsidiary company", 1, groupClient1.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Precondition: Should have 1 subsidiary company", 1, groupClient2.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Precondition: Should have 1 subsidiary company", 1, groupClient3.RelatedManagementSubsidiaryRelations.Organisations.Count());

			var companyTariff = Helper.NewLevel1CompanyTariffWithSingleRateLine("AIR", "LSE", "USLAX", "AUBNE", "FRT", 200m);
			Helper.NewClientRateWithSingleRateLine(consignee, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 20m);
			Helper.NewClientRateWithSingleRateLine(consignor, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 30m);
			Helper.NewClientRateWithSingleRateLine(groupClient2, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 120m);
			Helper.NewClientRateWithSingleRateLine(groupClient3, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 130m);

			Factory.Save();

			#region expected rate

			var expectedCompanyTariff1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m,
				}
			};

			var expectedGroupClientRate1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 110m,
				}
			};

			var expectedClientRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m,
				}
			};

			#endregion

			AutorateAndAssert("Should load rates from expectedCompanyTariff1", expectedCompanyTariff1, shipment, localClient);
			var defaultLevel = localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel.P7_ApplyGroupRate = true;
			Helper.NewClientRateWithSingleRateLine(groupClient1, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 110m);
			AutorateAndAssert("Should load rates from groupClientRate1", expectedGroupClientRate1, shipment, localClient);

			Helper.NewClientRateWithSingleRateLine(localClient, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 10m);
			AutorateAndAssert("Should load rates from Client Rates", expectedClientRate, shipment, localClient);
		}

		public void TestAutoRatingWithGroupClientRates_ClientRateDefaultingByRelationsHierachy()
		{
			var localClient = Helper.NewOrgHeader(1);
			localClient.OH_FullName = "Sid's Milk";
			var subsidiaryOrg = Helper.NewOrgHeader();
			subsidiaryOrg.OH_FullName = "Beneath Sid's Milk";

			Factory.Save();

			localClient.RelatedManagementSubsidiaryRelations.AddOrganisation(subsidiaryOrg);
			var defaultLevel = subsidiaryOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel.P7_ApplyGroupRate = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, Helper.NewOrgHeader(1).PK, "USLAX", "AUBNE", 5);

			Factory.Save();

			AssertEquals("Precondition", 1, localClient.RelatedManagementSubsidiaryRelations.Organisations.Count());

			Helper.NewClientRateWithSingleRateLine(localClient, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 250m);
			Helper.NewClientRateWithSingleRateLine(subsidiaryOrg, "AIR", "LSE", "USLAX", "AUBNE", "BAF", 40m);

			var expectedClientRate = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 250m }
			};

			AutorateAndAssert("Local Client's rate should be matched, even though there's a subsidiary rate", expectedClientRate, shipment, localClient);

			shipment.ConsignorPK = subsidiaryOrg.PK;
			Factory.Save();

			expectedClientRate = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 250m },
				new AssertionCharge { ChargeCode = "BAF", JR_OSSellAmt = 40m }
			};

			AutorateAndAssert("Subsidiary org should pull rates though from the parent org", expectedClientRate, shipment, subsidiaryOrg);
		}

		#endregion

		#region TestFPUCalculator

		public void TestFlatAmountsPerWeightBreak()
		{
			var consignee = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("BAF", CombinedCalculator.Code, Weight.Kilograms);
			var combinedCalculator = line.GetCalculator<CombinedCalculator>();
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 0, 10);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 0, 20);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 0, 40);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 200m, 0, 50);

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "AUSYD", "USLAX", 166, 1);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 40m,
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestFPUCalculatorWithZeroMeasureShouldNotProduceFlatAmount()
		{
			var consignee = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("BAF", FlatPlusPerUnitCalculator.Code, QuantityUnit.CN);
			line.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate = 123;
			line.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit = 10;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "AUSYD", "USLAX", 0m, 0m);
			Factory.Save();

			AutorateAndAssert(null, shipment, client, autorateCosts: false);
		}

		public void TestCMBCalculatorWithZeroMeasureShouldNotProduceFlatAmount()
		{
			var consignee = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("BAF", CombinedCalculator.Code, QuantityUnit.KG);
			line.TL_WeightVolume = "";
			var combinedCalculator = line.GetCalculator<CombinedCalculator>();
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 8, 10);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 7, 20);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 6, 40);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 200m, 5, 50);

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "AUSYD", "USLAX", 0m, 0m);
			Factory.Save();

			AutorateAndAssert(null, shipment, client, autorateCosts: false);
		}

		#endregion

		#region TestPaymentBasisChargeableLength

		public void TestPaymentBasisChargeableLength()
		{
			var consignee = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 3000000m);
			var percentageCalculator = entry.AddRateLine("BAF", PercentageCalculator.Code).GetCalculator<PercentageCalculator>();
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			percentageCalculator.Percent = 1m;
			percentageCalculator.IsPartThereof = true;
			percentageCalculator.ValueOrPartThereOf = 2000000m;
			percentageCalculator.Rate = 5m;
			percentageCalculator.IncludeGST = false;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "AUSYD", "USLAX", 25000m, 5m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3000000m,
					RevenueCalculationDescription = "FRT: Base Rate AUD 3000000.00"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = "BAF: 2 AUD 2000000.00 @ AUD 5.00/AUD 2000000.00 (per AUD 2000000.00 or part thereof for AUD 3000000.00 (Freight Charges FRT))"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			var charge = (shipment.Job as Job).Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "BAF");
			AssertNotNull(charge);
			var paymentBasis = charge.SellPaymentBases.First();
			AssertEquals(paymentBasis.PBS_ChargeableUnit, "AUD 2000000.00");
			AssertEquals(paymentBasis.PBS_RateUnit, "AUD 2000000.00");
		}

		#endregion

		#region RatingResults Does Not Contain AutoRatingExplorer

		public void TestRatingResultsDoesNotContainAutoRatingExplorer()
		{
			var client = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "");
			var rateLine = rateEntry.AddRateLine("ODOC", UnitCalculator.Code, Weight.Kilograms, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 10;

			var shipment = CreateForwardingShipment(TransportModes.Air, Helper.NewOrgHeader().PK, Helper.NewOrgHeader().PK, "AUSYD", "GBSUN", 30);
			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 300 }, };

			RatingResults results = AutorateAndAssert(expected, shipment, client);

			AssertEquals("Results Count", 1, results.Results.Length);
			AssertEquals("Target", shipment.HumanReadableName, results.Results[0].Target);

			var message = "AutoRatingExplorer should remain empty as it is only required when autorating for Glow via AccountingRatingService.";
			Assert(message, string.IsNullOrEmpty(results.Results[0].AutoRatingExplorer));
		}

		#endregion

		#region TestLogNotes

		[TestDate(2015, 02, 02)]
		public void TestLogNotes()
		{
			Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").RC_HandlingRateClass = "20GN";
			Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").RC_HandlingRateClass = "20GN";

			var costing = Helper.NewCosting(TransportProvider1);
			var disbCharge = Helper.ChargeCodes.New("DISB", "Disbursement  Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			disbCharge.AC_ChargeType = ChargeType.Disbursement;

			var costLine1 = costing.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "STD", "20GP").AddRateLine("ODOC", FlatCalculator.Code);
			costLine1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;
			costLine1.Parent.TI_MatchContainerRateClass = true;

			var costEntry = costing.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "STD", "20RE");

			var costLine2 = costEntry.AddRateLine("ODOC", FlatCalculator.Code);
			costLine2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			var costLine3 = costEntry.AddRateLine(disbCharge, FlatCalculator.Code);
			costLine3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)60m;

			var costLine4 = costEntry.AddRateLine("OCART", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var tariff = Helper.NewCompanyTariff();

			var orgTariffEntry = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "STD", "20GP");
			var orgTariffline = orgTariffEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			orgTariffline.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;
			orgTariffline.Parent.TI_MatchContainerRateClass = true;

			var ocartRateLine = orgTariffEntry.AddRateLine("OCART", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 2000m;

			tariff.Factory.Save();

			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var frtEntry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20RE");
			frtEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(frtEntry.Currency);

			var frtTariffline = frtEntry.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)frtTariffline.Calculator).BaseRate = 2000.00;
			frtTariffline.TL_Condition = RateLineConditions.ForwardingAndBrokerage;

			var opchTariffLine = frtEntry.AddRateLine(disbCharge, FlatCalculator.Code);
			((FlatCalculator)opchTariffLine.Calculator).BaseRate = 70.00;

			var dstEntry = rate.AddRateEntry("DST", "FCL", "AUSYD", "USLAX", "", "20RE");
			var dstTariffline = dstEntry.AddRateLine("DDOC", FlatCalculator.Code);
			((FlatCalculator)dstTariffline.Calculator).BaseRate = 75.00;
			dstTariffline.TL_Condition = RateLineConditions.OwnBrokerage;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_OH_ExportBroker = TransportProvider1.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "DISB", JR_OSCostAmt = 70, JR_OSSellAmt = 70 },
				new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 100, JR_OSSellAmt = 105 },
				new AssertionCharge { ChargeCode = "OCART", JR_OSCostAmt = 1000, JR_OSSellAmt = 1000 },
				new AssertionCharge { ChargeCode = "OCART", JR_OSCostAmt = 2000, JR_OSSellAmt = 2000 }
			};

			RatingResults results;

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				results = AutorateAndAssert(expected, shipment, client);
			}

			string[] expectedLogNote = new string[]
			{
				@"User:				CargoWise Support
Time:				02-Feb-15 00:00

Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription
Information: Resetting previously auto-rated charges for Shipment SHIPMENT1
Information: AUTORATING COSTS FOR Shipment SHIPMENT1
Information: RatingHeader Found Costing TRASPROV1 Entries: 2
Information: RateLine Found ODOC-FLT-20GP-Costing TRASPROV1
Information: RateLine Found DISB-FLT-20RE-Costing TRASPROV1
Information: RateLine Found OCART-UNT-CN-20RE-Costing TRASPROV1
Information: RateLine Found ODOC-FLT-20RE-Costing TRASPROV1
Information: RateLine Filtered DISB-FLT-20RE-Costing TRASPROV1	reason:	TRASPROV1 is not among creditors/contractors on a job for FRT charge group
Information: RateLine Filtered ODOC-FLT-20GP-Costing TRASPROV1	reason:	failed similarity check
Information: Chargeable was added for
				RateLine OCART-UNT-CN-20RE-Costing TRASPROV1
					Job's info:
					Container 20RE: 1 ContainerCount
Information: CHARGES CALCULATED:
	OCART: 1 20RE Container(s) @ AUD 1000.00/Container
	ODOC: Base Rate AUD 100.00
Information: Shipment SHIPMENT1 was auto-costed.
	The following costs were found:
	  • OCART charge from Costing TRASPROV1
	  • ODOC charge from Costing TRASPROV1
	Charges created: OCART, ODOC
Information: AUTORATING REVENUE FOR Shipment SHIPMENT1
Information: RatingHeader Found Client Rate TESTORG1 Entries: 2
Information: RatingHeader Found Base Company Tariff Entries: 1
Information: RateLine Found OCART-UNT-CN-20GP-Base Company Tariff
Information: RateLine Found ODOC-CST-20GP-Base Company Tariff
Information: RateLine Found DDOC-FLT-20RE-Client Rate TESTORG1
Information: RateLine Found DISB-FLT-20RE-Client Rate TESTORG1
Information: RateLine Found FRT-FLT-20RE-Client Rate TESTORG1
Information: RateLine Filtered DDOC-FLT-20RE-Client Rate TESTORG1	reason:	RateLine condition BRK not met
Information: RateLine Filtered FRT-FLT-20RE-Client Rate TESTORG1	reason:	RateLine condition FBR not met
Information: Chargeable was added for
				RateLine OCART-UNT-CN-20GP-Base Company Tariff
					Job's info:
					Container 20RE: 1 ContainerCount
Information: CHARGES CALCULATED:
	DISB: Base Rate USD 70.00	:	 Disbursements autorated from Sell Rates override unapportioned Cost or a Cost that is not flagged 'Override Rating'.
	OCART: 1 20GP Container(s) @ AUD 2000.00/Container
	ODOC: 105.00% of (Base Rate AUD 100.00)
Information: Shipment SHIPMENT1 was auto-rated.
	The following rates were found:
	  • DISB charge from Client Rate TESTORG1
	  • OCART charge from Base Company Tariff
	  • ODOC charge from Base Company Tariff
	Charges created: DISB, OCART, ODOC"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, string.Empty, expectedLogNote);

			var expectedResults = new List<RatesAdditionInfo>
			{
				new RatesAdditionInfo
				{
					Target = "Shipment SHIPMENT1",
					CostSell = "cost",
					CreatedCharges = new List<ChargeInfo>
					{
						new ChargeInfo { ChargeCode = "OCART" },
						new ChargeInfo { ChargeCode = "ODOC" }
					}.ToArray(),
					ModifiedCharges = new List<ChargeInfo>().ToArray(),
					DeletedChargesCount = 0,
					RatesFound = new List<ChargeInfo>
					{
						new ChargeInfo { ChargeCode = "OCART" },
						new ChargeInfo { ChargeCode = "ODOC" }
					}.ToArray()
				},
				new RatesAdditionInfo
				{
					Target = "Shipment SHIPMENT1",
					CostSell = "revenue",
					CreatedCharges = new List<ChargeInfo>
					{
						new ChargeInfo { ChargeCode = "DISB" },
						new ChargeInfo { ChargeCode = "OCART" },
						new ChargeInfo { ChargeCode = "ODOC" }
					}.ToArray(),
					ModifiedCharges = new List<ChargeInfo>().ToArray(),
					DeletedChargesCount = 0,
					RatesFound = new List<ChargeInfo>
					{
						new ChargeInfo { ChargeCode = "DISB" },
						new ChargeInfo { ChargeCode = "OCART" },
						new ChargeInfo { ChargeCode = "ODOC" }
					}.ToArray(),
				},
				new RatesAdditionInfo
				{
					Target = "Shipment SHIPMENT1",
					CostSell = "revenue",
					CreatedCharges = new List<ChargeInfo>().ToArray(),
					ModifiedCharges = new List<ChargeInfo>().ToArray(),
					DeletedChargesCount = 0,
					RatesFound = new List<ChargeInfo>().ToArray(),
				}
			};

			AssertEquals("Results Count", 1, results.Results.Length);
			AssertEquals("Target", shipment.HumanReadableName, results.Results[0].Target);
			AssertContains("", expectedResults.ToJSON(), results.Results[0].Results.ToJSON());
		}

		#endregion

		#region TestAutoRatingLogExcludesCostInformation

		[TestDate(2017, 08, 21)]
		public void TestAutoRatingLogExcludesCostInformation()
		{
			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader();

			var testRate = Helper.NewClientRate(consignee);
			var entry = testRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var lineA = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			lineA.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 1000);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			Factory.Save();

			var expected = new[]
				 {
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 10000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 10.00/KG",
						}
				};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutorateAndAssert(expected, shipment, consignee);
			}

			var expectedLogNote = @"User:				CargoWise Support
Time:				21-Aug-17 00:00

Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription
Information: Resetting previously auto-rated charges for Shipment SHIPMENT1
Information: AUTORATING COSTS FOR Shipment SHIPMENT1
Information: CHARGES CALCULATED:
Warning: Shipment SHIPMENT1 was auto-costed.
	No costs were found.
Information: AUTORATING REVENUE FOR Shipment SHIPMENT1
Information: RatingHeader Found Client Rate TESTORG1 Entries: 1
Information: RateLine Found FRT-UNT-KG-Client Rate TESTORG1
Information: Chargeable was added for
				RateLine FRT-UNT-KG-Client Rate TESTORG1
					Job's info:
					: 1000 Weight
					: 0 Volume
					: 1000 Chargeable
Information: CHARGES CALCULATED:
	FRT: 1000 Kilogram(s) @ AUD 10.00/KG
Information: Shipment SHIPMENT1 was auto-rated.
	The following rates were found:
	  • FRT charge from Client Rate TESTORG1
	Charges created: FRT
Information: AUTORATING PROFIT SHARE FOR Shipment SHIPMENT1
Information: RatingHeader Found Client Rate TESTORG1 Entries: 1
Information: RateLine Found FRT-UNT-KG-Client Rate TESTORG1
Information: RateLine Filtered FRT-UNT-KG-Client Rate TESTORG1	reason:	Not applicable in rebate calculation mode
Information: CHARGES CALCULATED:";

			AssertAutoratingAuditLogNote(shipment, expectedLogNote);
		}

		#endregion

		#region TestAutoRateLoadList

		public void TestAutoRateLoadList()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("CLL1", "Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.CFSLoadList);

			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.PAC, RateMode.FCL, "AU", "NZ");

			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 1200m;

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "LOADLIST1";
			loadList.JK_TransportMode = TransportModes.Sea;
			loadList.JK_ConsolMode = ContainerModes.FCL;
			loadList.JK_RL_NKLoadPort = "AUSYD";
			loadList.JK_RL_NKDischargePort = "NZAKL";
			loadList.JK_OH_Forwarder = client.PK;

			var container = loadList.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = "FCL";

			CreateJob(loadList, "LOADLIST1");

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = chargeCode.AC_Code, JR_OSCostAmt = 1200m } };

			AutorateAndAssert(expected, loadList, client);

			var log = loadList.Logs.MostRecentLogByEventTime(AutoEvents.ChargesHaveBeenAutoRated);
			AssertEquals("LoadList", log.Parameters[EventReferenceParameters.Type]);
			AssertEquals("LOADLIST1", log.Parameters[EventReferenceParameters.JobNumber]);
		}

		public void TestAutoRateLoadList_FakePacklinesAreNotCreated()
		{
			var client = GlbBranch.CurrentBranch.OrgProxy;
			client.OH_IsMiscFreightServices = true;
			client.OH_IsPackDepot = true;
			client.OH_IsUnpackDepot = true;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			Factory.Save();

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "LOADLIST1";
			loadList.JK_TransportMode = TransportModes.Sea;
			loadList.JK_ConsolMode = ContainerModes.FCL;
			loadList.JK_RL_NKLoadPort = "AUSYD";
			loadList.JK_RL_NKDischargePort = "NZAKL";
			loadList.JK_OH_Forwarder = client.PK;
			loadList.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;

			var cont20GPType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var container1 = loadList.Containers.AddNew();
			container1.JC_ContainerNum = "CONT4100010";
			container1.JC_RC = cont20GPType.PK;
			container1.JC_ContainerMode = "FCL";

			var container2 = loadList.Containers.AddNew();
			container2.JC_ContainerNum = "CONT4100011";
			container2.JC_RC = cont20GPType.PK;
			container2.JC_ContainerMode = "FCL";

			var container3 = loadList.Containers.AddNew();
			container3.JC_ContainerNum = "CONT4100012";
			container3.JC_RC = cont20GPType.PK;
			container3.JC_ContainerMode = "FCL";

			Factory.Save();

			var shipment = loadList.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "HSHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_ActualWeight = 20;
			shipment.JS_ActualVolume = 2;
			shipment.JS_OuterPacks = 50;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.JS_OH_HandledOnBehalfOfForwarder = client.PK;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ActualVolume = 2.1m;
			packline.JL_ActualWeight = 250m;
			packline.JL_PackageCount = 50;

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			using (var plugin = new InvoicingPluginToFreight(shipment))
			using (var form = new ShipmentReceivalForm(shipment))
			{
				form.Show();
				plugin.ShowPreSaveDialogs();

				var expected = @"All containers must be allocated to a Shipment before Autorating can occur.
The following containers are not allocated to any Shipment:

  - Container CONT4100010
  - Container CONT4100011
  - Container CONT4100012

Since this is the only Shipment on the Consol, all Containers will be allocated to this Shipment. Is this correct?";
				var actualMessages = UnitTestUserNotification.Instance.PreviousMessages;

				Assert("A warning should be shown to user", actualMessages.ContainsMessageWithThisText(expected));
				AssertEquals("Shipment has 1 packline after pre-save dialog is shown and user declined allocating packlines", 1, shipment.OuterPackLines.Count);
			}
		}

		[TestDate(2014, 6, 11)]
		public void TestAutoRateLoadListDoesNotAutorateShipments()
		{
			var client = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CLL1", "Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.CFSLoadList, FreightServiceType.Codes.Cleaning);
			var chargeCode2 = Helper.ChargeCodes.New("CFS1", "No Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.CFSShipment);

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.PAC, RateMode.ALL, "AU", "NZ");
			rateEntry.TI_RateStartDate = new ZDate(2014, 3, 1);

			var rateLine1 = rateEntry.AddRateLine(chargeCode1, FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 1200m;

			var rateLine2 = rateEntry.AddRateLine(chargeCode2, FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "LOADLIST1";
			loadList.JK_TransportMode = TransportModes.Sea;
			loadList.JK_ConsolMode = ContainerModes.FCL;
			loadList.JK_RL_NKLoadPort = "AUSYD";
			loadList.JK_RL_NKDischargePort = "NZAKL";
			loadList.JK_OH_Forwarder = client.PK;

			var container = loadList.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = "FCL";

			var service = container.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			service.ES_Booked = new ZDateTime(2014, 3, 23);
			service.ES_Completed = new ZDateTime(2014, 3, 23);
			service.ES_ServiceCount = 2;

			var shipment = loadList.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_E_DEP = new ZDateTime(2014, 6, 7);
			shipment.JS_E_ARV = new ZDateTime(2014, 6, 12);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = client.MainAddress.PK;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;
			packline.JL_ActualVolume = 2.1m;
			packline.JL_ActualWeight = 250m;

			CreateJob(loadList, "LOADLIST1");

			Factory.Save();

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				shipmentJob.JH_OA_LocalChargesAddr = client.MainAddress.PK;
				shipmentJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;

				var expected = new[] { new AssertionCharge { ChargeCode = chargeCode1.AC_Code, JR_OSCostAmt = 1200m } };
				AutorateAndAssert(expected, loadList, client);
				AssertCharges("", Array.Empty<AssertionCharge>(), shipmentJob);
			}
		}

		#endregion

		#region TestApportionmentByCapacityPerContainerType

		public void TestApportionmentByCapacityPerContainer_ByVolume()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.CapacityPerContainer);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";
			consol.Transports[0].JW_IsLinked = ZBool.True;

			// when dates are empty, Consol form will have validation errors but that should not stop RS showing up with today's date.
			consol.Transports[0].JW_ETA = ZDateTime.Empty;
			consol.Transports[0].JW_ETD = ZDateTime.Empty;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_ContainerNum = "CONT00001";
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container40GP.JC_ContainerNum = "CONT00002";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 25.2m;
			packline1_1.JL_ActualWeight = 100m;
			packline1_1.JL_JC = container20GP.PK;

			var packline1_2 = shipment1.OuterPackLines.AddNew();
			packline1_2.JL_ActualVolume = 2.7m;
			packline1_2.JL_ActualWeight = 200m;
			packline1_2.JL_JC = container40GP.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee.PK;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;

			var packline2_1 = shipment2.OuterPackLines.AddNew();
			packline2_1.JL_ActualVolume = 5.8m;
			packline2_1.JL_ActualWeight = 300m;
			packline2_1.JL_JC = container20GP.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job2.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1000m,
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 2000m,
					}
				};

				AutoCostAndAssert("Total cost of both rate entries with FRT charge applied to the consol", null, expectedCosts, consol);

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).ToArray();
				AssertEquals(2, consolCosts.Length);

				AssertEquals(2, consolCosts[0].ApportionmentCharges.Count);
				AssertEquals(2, consolCosts[0].ApportionmentCharges.Count);

				Factory.Save();

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment1, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 812.90m
							},
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 2000.00m
							}
						}
					},
					{
						shipment2, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 187.1m
							}
						}
					}
				};

				AssertCharges("Charges should be apportioned based on what fraction of total volume loaded into a container each shipment has contributed", expectedCharges);
			}
		}

		public void TestApportionmentByCapacityPerContainer_ByWeight()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.CapacityPerContainer);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container20GP.JC_ContainerNum = "CONT00001";
			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container40GP.JC_ContainerNum = "CONT00002";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 5.2m;
			packline1_1.JL_ActualWeight = 15000m;
			packline1_1.JL_JC = container20GP.PK;

			var packline1_2 = shipment1.OuterPackLines.AddNew();
			packline1_2.JL_ActualVolume = 3.7m;
			packline1_2.JL_ActualWeight = 25000m;
			packline1_2.JL_JC = container40GP.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee.PK;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;

			var packline2_1 = shipment2.OuterPackLines.AddNew();
			packline2_1.JL_ActualVolume = 7.8m;
			packline2_1.JL_ActualWeight = 6000m;
			packline2_1.JL_JC = container20GP.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job2.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1000m,
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 2000m,
					},
				};

				AutoCostAndAssert("Total cost of both rate entries with FRT charge applied to the consol", null, expectedCosts, consol);

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).ToArray();

				AssertEquals(2, consolCosts[0].ApportionmentCharges.Count);
				AssertEquals(2, consolCosts[1].ApportionmentCharges.Count);

				Factory.Save();

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment1, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 714.29m
							},
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 2000.00m
							}
						}
					},
					{
						shipment2, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 285.71m
							}
						}
					}
				};

				AssertCharges("Charges should be apportioned based on what fraction of total weight loaded into a container each shipment has contributed", expectedCharges);
			}
		}

		[ExpectNoExceptions]
		public void TestLCLContainerUnitLinesCanBeApportionedByCapacityPerContainer()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("FRT", UnitCalculator.Code, "CN", "AUD").GetCalculator<UnitCalculator>().PerUnit = 360m;

			Factory.Save();
			var shipment1 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 3000m, 6m);
			var shipment2 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 2000m, 2m);
			var shipment3 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 500m, 1m);
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", creditor, shipment1, PaymentType.Prepaid);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);

			shipment3.OuterPackLines[0].Containers.RemoveAndDeleteAll();

			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_ContainerMode = "LCL";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> GetExpectedCharges(ZDecimal amount1, ZDecimal amount2, ZDecimal amount3)
			{
				var result = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>();
				result.Add(shipment1, new[] { new AssertionCharge { JR_LocalCostAmt = amount1 } });
				result.Add(shipment2, new[] { new AssertionCharge { JR_LocalCostAmt = amount2 } });
				result.Add(shipment3, new[] { new AssertionCharge { JR_LocalCostAmt = amount3 } });

				return result;
			}

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 360m,
					E6_ApportionmentMethod = AllocationMethod.ChargeableUnits
				}
			};

			AutoCostAndAssert("Total cost of  rate entry with FRT charge applied to the consol", GetExpectedCharges(240, 80, 40), expectedCosts, consol);

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).ToArray();

			consolCosts[0].E6_ApportionmentMethod = AllocationMethod.CapacityPerContainer;
			AssertEquals(consolCosts[0].E6_OSCostAmount, 360m);
		}

		public void TestApportionmentByCapacityPerContainer_ByVolumeAndWeight()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.CapacityPerContainer);
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			var rateLine20GP = costEntry20GP.AddRateLine("FRT", CombinedCalculator.Code, "CN");
			rateLine20GP.UseOnlyActualWeightMeasure = true;
			var combinedCalculator = rateLine20GP.GetCalculator<CombinedCalculator>();
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 6000m, 2000m, "KG");
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 6000m, 4000m, "KG");

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 6000m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var refContainer20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var refContainer40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var container20GP_1 = consol.Containers.AddNew();
			container20GP_1.JC_RC = refContainer20GP.PK;
			container20GP_1.JC_ContainerNum = "CONT00001";
			var container20GP_2 = consol.Containers.AddNew();
			container20GP_2.JC_RC = refContainer20GP.PK;
			container20GP_2.JC_ContainerNum = "CONT00002";

			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = refContainer40GP.PK;
			container40GP.JC_ContainerNum = "CONT00003";
			var container40GP_BlankNumber = consol.Containers.AddNew();
			container40GP_BlankNumber.JC_RC = refContainer40GP.PK;
			container40GP_BlankNumber.JC_ContainerCount = 2;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 15.2m;
			packline1_1.JL_ActualWeight = 200m;
			packline1_1.JL_JC = container20GP_1.PK;

			var packline1_2 = shipment1.OuterPackLines.AddNew();
			packline1_2.JL_ActualVolume = 3.7m;
			packline1_2.JL_ActualWeight = 25000m;
			packline1_2.JL_JC = container40GP.PK;

			var packline1_3 = shipment1.OuterPackLines.AddNew();
			packline1_3.JL_ActualVolume = 1.7m;
			packline1_3.JL_ActualWeight = 25000m;
			packline1_3.JL_JC = container20GP_2.PK;

			var packline1_4 = shipment1.OuterPackLines.AddNew();
			packline1_4.JL_ActualVolume = 8.8m;
			packline1_4.JL_ActualWeight = 7000m;
			packline1_4.JL_JC = container40GP_BlankNumber.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee.PK;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;

			var packline2_1 = shipment2.OuterPackLines.AddNew();
			packline2_1.JL_ActualVolume = 7.8m;
			packline2_1.JL_ActualWeight = 600m;
			packline2_1.JL_JC = container20GP_1.PK;

			var packline2_2 = shipment2.OuterPackLines.AddNew();
			packline2_2.JL_ActualVolume = 7.8m;
			packline2_2.JL_ActualWeight = 6000m;
			packline2_2.JL_JC = container40GP_BlankNumber.PK;

			var packline2_3 = shipment2.OuterPackLines.AddNew();
			packline2_3.JL_ActualVolume = 2.1m;
			packline2_3.JL_ActualWeight = 37000m;
			packline2_3.JL_JC = container20GP_2.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job2.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 6000m,
						CostCalculationDescription = "FRT: 1 20GP Container(s) @ AUD 4000.00/Container + 1 20GP Container(s) @ AUD 2000.00/Container",
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 18000m,
						CostCalculationDescription = "FRT: 1 40GP Container(s) @ AUD 6000.00/Container + 2 40GP Container(s) @ AUD 6000.00/Container",
					},
				};

				AutoCostAndAssert("Total cost of both rate entries with FRT charge applied to the consol", null, expectedCosts, consol);

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).ToArray();

				AssertEquals(2, consolCosts[0].ApportionmentCharges.Count);
				AssertEquals(2, consolCosts[1].ApportionmentCharges.Count);

				Factory.Save();

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment1, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 2934.64m
							},
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 12461.54m
							}
						}
					},
					{
						shipment2, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 3065.36m
							},
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 5538.46m
							}
						}
					}
				};

				AssertCharges("Charges should be apportioned based on what fraction of total weight and volume loaded into a container each shipment has contributed", expectedCharges);
			}
		}

		public void TestApportionmentByCapacityPerContainer_ForTotalValueZero_ApportionedValueIsZero()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.CapacityPerContainer);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 0;

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1;
			costEntry40GP.AddRateLine("FRT", FlatPlusPerUnitCalculator.Code, QuantityUnit.CN).GetCalculator<FlatPlusPerUnitCalculator>().PerUnit = -1;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";

			// when dates are empty, Consol form will have validation errors but that should not stop RS showing up with today's date.
			consol.Transports[0].JW_ETA = ZDateTime.Empty;
			consol.Transports[0].JW_ETD = ZDateTime.Empty;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_ContainerNum = "CONT00001";
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var container40GP = consol.Containers.AddNew();
			container40GP.JC_ContainerNum = "CONT00002";
			container40GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";
			shipment.JS_OH_DeliveryAgent = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Factory.Save();

			using (var form = new ZForm(consol))
			{
				var starter = new AutoRatingStarter(consol, new AutoRatingGUIInteractor(form));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertNoExceptionThrown(() => { starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue.With(billingType: BillingType.Apportionment)); });

				Factory.Save();
			}

			var charges = ((Job)shipment.Job).Charges;

			AssertEquals("Number of job charges", 2, charges.Count);
			AssertEquals("Charge code on charge line", "FRT", charges[0].ChargeCode.AC_Code);
			AssertEquals("Cost amount on charge line", 0m, charges[0].JR_OSCostAmt);
			AssertEquals("Charge code on charge line", "FRT", charges[1].ChargeCode.AC_Code);
			AssertEquals("Cost amount on charge line", 0m, charges[1].JR_OSCostAmt);
		}

		#endregion

		#region NoteCalculator

		void AssertAutoRateShowRateNoteForNoteCalculator(bool isCrossTrade)
		{
			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();

			var origin = isCrossTrade ? "USLAX" : "AUSYD";
			var destination = "CNSHA";
			var localClient = isCrossTrade ? consignor : consignee;

			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", NoteCalculator.Code);
			rateLine1.GetCalculator<NoteCalculator>().ShowOnBillingWithoutPrefix = false;
			var rateLine2 = rateEntry.AddRateLine("BAF", NoteCalculator.Code);
			rateLine2.GetCalculator<NoteCalculator>().ShowOnBillingWithoutPrefix = true;

			var rateLineItem1 = rateLine1.RateLineItems.AddNew();
			rateLineItem1.TM_Text = "test note1";
			rateLineItem1.TM_Value = 60m;
			var rateLineItem2 = rateLine2.RateLineItems.AddNew();
			rateLineItem2.TM_Text = "test note2";
			rateLineItem2.TM_Value = 90m;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, origin, destination, 3000);

			Factory.Save();

			var expected = new[]
							{
								new AssertionCharge
								{
									ChargeCode = "FRT",
									JR_OSSellAmt = 0m,
									JR_Desc = @"RATE NOTE: International Freight
	test note1	 $60"
								},
								new AssertionCharge
								{
									ChargeCode = "BAF",
									JR_OSSellAmt = 0m,
									JR_Desc = @"Bunker Adjustment Factor
	test note2	 $90"
								}
							};

			var expectedErrors = new string[] { $@"Error {shipment.JS_UniqueConsignRef} has encountered the following errors while AutoRating:
	•  Description: A Rate Note was added to your invoice. You have two choices:
	- modify the description and enter the appropriate amount (see description for details), or
	- remove the charge if it doesn't apply." };

			AutorateAndAssert(expected, shipment, localClient, autorateCosts: false, expectedErrors: expectedErrors);
		}

		public void TestAutoRateShowRateNoteForNoteCalculator()
		{
			AssertAutoRateShowRateNoteForNoteCalculator(false);
		}

		public void TestAutoRateShowRateNoteForNoteCalculatorWithCrossTrade()
		{
			AssertAutoRateShowRateNoteForNoteCalculator(true);
		}

		public void TestNoteCalculatorWithPercentageCalculator()
		{
			Helper.ChargeCodes["DDOC"].AC_AT_GSTRate = Helper.ChargeCodes["DCART"].AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "EETLL", ZString.Empty, "20GP");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var noteRateLine = rateEntry.AddRateLine("DCART", NoteCalculator.Code);
			var rateLineItem1 = noteRateLine.RateLineItems.AddNew();
			rateLineItem1.TM_Text = "Option A";
			rateLineItem1.TM_Value = 50m;
			var rateLineItem2 = noteRateLine.RateLineItems.AddNew();
			rateLineItem2.TM_Text = "Option B";
			rateLineItem2.TM_Value = 100m;

			var percentageRateLine = rateEntry.AddRateLine("DDOC", PercentageCalculator.Code);
			percentageRateLine.GetCalculator<PercentageCalculator>().Percent = 10m;
			var percentageApplyToRateLineItem = percentageRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			percentageApplyToRateLineItem.TM_AC = Helper.ChargeCodes["DCART"].PK;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "SGSIN", "EETLL", 10000m, 20m);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = CreateForwardingConsol(TransportModes.Sea, "SGSIN", "EETLL", TransportProvider1, shipment);

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "YYCC4100011";
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			Assert(shipment.IsCrossTrade());

			var expected = new[]
							{
								new AssertionCharge
								{
									ChargeCode = "DCART",
									JR_OSSellAmt = 0m,
									RevenueCalculationDescription = "Rate Note: Value must be manually specified."
								},
								new AssertionCharge
								{
									ChargeCode = "DDOC",
									JR_OSSellAmt = 0m,
									RevenueCalculationDescription = "DDOC: 10.00% of (EUR 0.00 (DCART))"
								}
							};

			var expectedErrors = $@"Error {shipment.JS_UniqueConsignRef} has encountered the following errors while AutoRating:
	•  Description: A Rate Note was added to your invoice. You have two choices:
	- modify the description and enter the appropriate amount (see description for details), or
	- remove the charge if it doesn't apply.";

			AutorateAndAssert(expected, shipment, Consignor, autorateCosts: false, expectedErrors: new[] { expectedErrors });

			var shipmentJob = shipment.Job as Job;
			var rate = shipmentJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "SGD";
			rate.JF_BaseRate = 1.02m;

			var nteCharge = shipmentJob.Charges.Cast<Charge>().FirstOrDefault(x => x.ChargeCode.AC_Code == "DCART");
			nteCharge.JR_Desc = "Charged option B";
			nteCharge.JR_OSSellAmt = 100m;

			Factory.Save();

			expected = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "DCART",
								JR_OSSellAmt = 0m,
								RevenueCalculationDescription = @"DCART:
Rate Note: Value must be manually specified."
							},
							new AssertionCharge
							{
								ChargeCode = "DCART",
								JR_OSSellAmt = 100m,
							},
							new AssertionCharge
							{
								ChargeCode = "DDOC",
								JR_OSSellAmt = 10m,
								RevenueCalculationDescription = "DDOC: 10.00% of (EUR 100.00 (DCART*))"
							}
						};

			AutorateAndAssert(expected, shipment, Consignor, null, shipmentJob, true, false, expectedErrors: new[] { expectedErrors });
		}

		#endregion

		#region TestOmitTimePortionWhenAutoRating

		[TestDate(2013, 08, 8)]
		public void TestOmitTimePortionWhenAutoRating()
		{
			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(consignee);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.TI_RateStartDate = new ZDate(2013, 7, 1);
			rateEntry.TI_RateEndDate = new ZDate(2013, 7, 31);

			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 0.0m;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";
			shipment.JS_E_DEP = new ZDateTime(2013, 7, 31, 21, 24, 0);
			shipment.JS_E_ARV = new ZDateTime(2013, 9, 6, 9, 39, 0);

			Factory.Save();

			var expected = new[]
							 {
									new AssertionCharge
										 {
												ChargeCode = "FRT",
												JR_OSSellAmt = 2000m,
										 }
							 };

			AutorateAndAssert(expected, shipment, consignee);
		}

		#endregion

		#region CLM Consol

		public void TestCLMConsol()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var cnr = Helper.NewOrgHeader();
			var cne = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;
			costEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = .5;

			var clm = Factory.NewWithValidTestData<ForwardingConsol>();
			clm.JK_TransportMode = TransportModes.Air;
			clm.JK_AgentType = AgentType.AWBMaster;
			clm.JK_ConsolMode = ContainerModes.Loose;
			clm.JK_RL_NKLoadPort = "AUSYD";
			clm.JK_RL_NKDischargePort = "NZAKL";
			clm.SetDefaultShippingLineAddress(creditor);
			clm.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			clm.JK_PrepaidCollect = PaymentType.Prepaid;

			var cla1 = clm.ColoadConsols.AddNew();
			var shipment11 = cla1.Shipments.AddNew();
			shipment11.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment11.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			var packline11 = shipment11.OuterPackLines.AddNew();
			packline11.JL_PackageCount = 44;
			packline11.JL_F3_NKPackType = "PLT";
			packline11.JL_ActualWeight = 890;
			packline11.JL_ActualWeightUQ = "KG";
			packline11.JL_ActualVolume = 3;
			packline11.JL_ActualVolumeUQ = "M3";

			var shipment12 = cla1.Shipments.AddNew();
			shipment12.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment12.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			var packline12 = shipment12.OuterPackLines.AddNew();
			packline12.JL_PackageCount = 12;
			packline12.JL_F3_NKPackType = "PLT";
			packline12.JL_ActualWeight = 555;
			packline12.JL_ActualWeightUQ = "KG";
			packline12.JL_ActualVolume = 6;
			packline12.JL_ActualVolumeUQ = "M3";

			var cla2 = clm.ColoadConsols.AddNew();
			var shipment21 = cla2.Shipments.AddNew();
			shipment21.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment21.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			var packline21 = shipment21.OuterPackLines.AddNew();
			packline21.JL_PackageCount = 232;
			packline21.JL_F3_NKPackType = "KEG";
			packline21.JL_ActualWeight = 134;
			packline21.JL_ActualWeightUQ = "KG";
			packline21.JL_ActualVolume = 3;
			packline21.JL_ActualVolumeUQ = "M3";

			shipment11.UpdateShipmentFromOuterPackLines();
			shipment12.UpdateShipmentFromOuterPackLines();
			shipment21.UpdateShipmentFromOuterPackLines();

			Factory.Save();

			Assert(true);
			//			The below bit has to be uncommented once Apportionments start to work for CLMs

			//			using (var job11 = new JobHeader.Loader(shipment11).LoadOrCreate())
			//			using (var job12 = new JobHeader.Loader(shipment12).LoadOrCreate())
			//			using (var job21 = new JobHeader.Loader(shipment21).LoadOrCreate())
			//			{
			//				job11.JH_OA_LocalChargesAddr = cnr.MainAddress.PK;
			//				job12.JH_OA_LocalChargesAddr = cnr.MainAddress.PK;
			//				job21.JH_OA_LocalChargesAddr = cnr.MainAddress.PK;
			//				Factory.Save();
			//
			//				var expectedCosts = new[]
			//				{
			//					new AssertionCost
			//						{
			//							CostCalculationDescription = "FRT: 1500 Kilogram(s) @ AUD 1.00/KG",
			//							ChargeCode = "FRT",
			//							E6_OSCostAmount = 1500,
			//						},
			//				};
			//
			//				AutoCostAndAssert("", null, expectedCosts, clm);
			//			}
		}

		#endregion

		#region CountryZoneSuburbDefaulting

		public void TestCountryZoneSuburbDefaulting()
		{
			Helper.ChargeCodes.New("TBKC", "Transport Booking Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			//Org and address setup

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();

			var addrCapalaba = org.MainAddress;
			addrCapalaba.OA_Address1 = "5/9 Giraffe street";
			addrCapalaba.OA_City = "Capalaba";
			addrCapalaba.OA_PostCode = "4444";

			var addrWoolloo = org.Addresses.AddNew();
			addrWoolloo.OA_Address1 = "7/8 Heroes of Labour street";
			addrWoolloo.AddAddressType(OrgAddressType.PickupAndDelivery);
			addrWoolloo.OA_City = "Woolloongabba";
			addrWoolloo.OA_PostCode = "6666";

			var addrInnerBne = org.Addresses.AddNew();
			addrInnerBne.OA_Address1 = "2a/18 Yeskoff street";
			addrInnerBne.AddAddressType(OrgAddressType.PickupAndDelivery);
			addrInnerBne.OA_PostCode = "1500";

			var addrOuterBne = org.Addresses.AddNew();
			addrOuterBne.OA_Address1 = "1/44 AutoRating street";
			addrOuterBne.AddAddressType(OrgAddressType.PickupAndDelivery);
			addrOuterBne.OA_PostCode = "2500";

			//zones and locations setup

			var woollooCity = Factory.NewWithValidTestData<RefCityTown>();
			woollooCity.R9_RN_NKCountry = "AU";
			woollooCity.R9_InternationalName = "Woolloongabba";
			woollooCity.PostCodes.AddNew().RK_CityTownPostCode = "6666";
			var capabalaCity = Factory.NewWithValidTestData<RefCityTown>();
			capabalaCity.R9_RN_NKCountry = "AU";
			capabalaCity.R9_InternationalName = "Capalaba";
			capabalaCity.PostCodes.AddNew().RK_CityTownPostCode = "4444";

			var postCode1000 = Helper.CreateRefPostCode("1000");
			var postCode1999 = Helper.CreateRefPostCode("1999");
			var postCode2000 = Helper.CreateRefPostCode("2000");
			var postCode2999 = Helper.CreateRefPostCode("2999");

			var zoneSet = Helper.CreateRateTransportZoneSet(org, CountryCodes.Australia);
			var innerBneZone = zoneSet.CreateRateTransportZoneForTest("Inner Brisbane");
			innerBneZone.CreateRateTransportZoneItemForTest(postCode1000, postCode1999);
			var outerBneZone = zoneSet.CreateRateTransportZoneForTest("Outer Brisbane");
			outerBneZone.CreateRateTransportZoneItemForTest(postCode2000, postCode2999);
			var woollooZone = zoneSet.CreateRateTransportZoneForTest("Woolloongabba");
			woollooZone.CreateRateTransportZoneItemForTest(woollooCity);
			var capalabaZone = zoneSet.CreateRateTransportZoneForTest("Capalaba");
			capalabaZone.CreateRateTransportZoneItemForTest(capabalaCity);

			//rates setup

			var rate = Helper.NewCosting(transportCo);

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.LRO, "AU", "");
			entry1.RateLines.RemoveAndDeleteAll();
			var line1 = entry1.AddRateLine("TBKC", UnitCalculator.Code, PkgUnit.Box);
			line1.GetCalculator<UnitCalculator>().PerUnit = 2m;

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.LRO, "AU", "");
			entry2.OriginSuburbPK = capabalaCity.PK;
			entry2.TI_TZ_DestinationZone = outerBneZone.PK;
			entry2.RateLines.RemoveAndDeleteAll();
			var line2 = entry2.AddRateLine("TBKC", UnitCalculator.Code, PkgUnit.Box);
			line2.GetCalculator<UnitCalculator>().PerUnit = 4m;

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.LRO, "AU", "");
			entry3.TI_TZ_OriginZone = innerBneZone.PK;
			entry3.TI_TZ_DestinationZone = woollooZone.PK;
			entry3.RateLines.RemoveAndDeleteAll();
			var line3 = entry3.AddRateLine("TBKC", UnitCalculator.Code, PkgUnit.Box);
			line3.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var entry4 = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.LRO, "AU", "");
			entry4.TI_TZ_OriginZone = capalabaZone.PK;
			entry4.TI_TZ_DestinationZone = outerBneZone.PK;
			entry4.RateLines.RemoveAndDeleteAll();
			var line4 = entry4.AddRateLine("TBKC", UnitCalculator.Code, PkgUnit.Box);
			line4.GetCalculator<UnitCalculator>().PerUnit = 7m;

			//Transport Booking setup

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var consol = Factory.New<DtbBookingConsolidation>();

			var tb1 = consol.Bookings.AddNew();
			tb1.Address.OrganisationPK = transportCo.PK;
			tb1.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;

			var tb2 = consol.Bookings.AddNew();
			tb2.Address.OrganisationPK = transportCo.PK;
			tb2.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;

			Factory.Save();

			var picInstr1 = tb1.Instructions.AddNew();
			picInstr1.KN_InstructionType = InstructionTypes.Codes.PickUp;
			picInstr1.KN_IsLooseRateable = true;
			picInstr1.Address.E2_OA_Address = addrInnerBne.PK;

			var dlvInstr1 = tb1.Instructions.AddNew();
			dlvInstr1.KN_InstructionType = InstructionTypes.Codes.Delivery;
			dlvInstr1.KN_IsLooseRateable = true;
			dlvInstr1.Address.E2_OA_Address = addrWoolloo.PK;

			var picInstr2 = tb2.Instructions.AddNew();
			picInstr2.KN_InstructionType = InstructionTypes.Codes.PickUp;
			picInstr2.KN_IsLooseRateable = true;
			picInstr2.Address.E2_OA_Address = addrCapalaba.PK;

			var dlvInstr2 = tb2.Instructions.AddNew();
			dlvInstr2.KN_InstructionType = InstructionTypes.Codes.Delivery;
			dlvInstr2.KN_IsLooseRateable = true;
			dlvInstr2.Address.E2_OA_Address = addrOuterBne.PK;

			var box1 = CreatePackage(tb1, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var box2 = CreatePackage(tb2, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);

			CreateInstructionPkgDivots(picInstr1, box1);
			CreateInstructionPkgDivots(dlvInstr1, box1);
			CreateInstructionPkgDivots(picInstr2, box2);
			CreateInstructionPkgDivots(dlvInstr2, box2);

			var expected1 = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TBKC",
							JR_OSCostAmt = 15.00,
							CostCalculationDescription = "TBKC: 3 Box(s) @ AUD 5.00/Box"
						}
				};

			var expected2 = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TBKC",
							JR_OSCostAmt = 12.00,
							CostCalculationDescription = "TBKC: 3 Box(s) @ AUD 4.00/Box"
						}
				};

			AutorateAndAssert("entry3 is used because it is more specific than entry1 because of zones", expected1, tb1, org);
			AutorateAndAssert("entry2 is used because it is more specific than entry4 because suburb is more specific than zone", expected2, tb2, org);

			entry2.Delete();
			entry3.Delete();
			Factory.Save();

			expected1 = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TBKC",
							JR_OSCostAmt = 6.00,
							CostCalculationDescription = "TBKC: 3 Box(s) @ AUD 2.00/Box"
						}
				};

			expected2 = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TBKC",
							JR_OSCostAmt = 21.00,
							CostCalculationDescription = "TBKC: 3 Box(s) @ AUD 7.00/Box"
						}
				};

			AutorateAndAssert("Now should default to generic entry1", expected1, tb1, org);
			AutorateAndAssert("Now should default to entry4 which has appropriate zone", expected2, tb2, org);
		}

		#endregion

		#region TestDebtorDefaultingOnTransportCharges

		public void TestDebtorDefaultingOnTransportCharge()
		{
			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			var cne = Factory.NewWithValidTestData<OrgHeader>();

			var tbcChargeCode = Helper.ChargeCodes["TBC1"];
			var trnChargeCode = Helper.ChargeCodes["TRN1"];
			tbcChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.TransportBooking;
			trnChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;

			var shipment = CreateForwardingShipment(TransportModes.Air, cnr.PK, cne.PK, "AUBNE", "USLAX", 3000);

			Factory.Save();

			var manager = new DtbDeliveryManager(Factory, shipment, DtbBookingDirection.PIC, false);

			manager.CreateTransportBooking();

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			var tbcCharge = job.Charges.AddNew();
			tbcCharge.JR_AC = tbcChargeCode.PK;

			AssertEquals(cnr.PK, tbcCharge.JR_OH_SellAccount);

			var trnCharge = job.Charges.AddNew();
			trnCharge.JR_AC = trnChargeCode.PK;

			AssertEquals(cnr.PK, trnCharge.JR_OH_SellAccount);
		}

		#endregion

		#region TestCalculationOrderResolved

		public void TestCalculationOrderResolvedWhenBAFDependsOnFRTWithHRM()
		{
			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader();

			var testRate = Helper.NewClientRate(consignee);
			var entry = testRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var lineB = entry.AddRateLine("BAF", HighestRateCalculator.Code);
			lineB.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.AsFreightedHighestRateWhenMin;

			var itemB1 = lineB.RateLineItems.AddNew();
			itemB1.TM_Type = Calculator.Items.Operator.UNT;
			itemB1.TM_BreakWeightVolume = QuantityUnit.KG;
			itemB1.TM_RelevantValue = 2m;

			var itemB2 = lineB.RateLineItems.AddNew();
			itemB2.TM_Type = Calculator.Items.Operator.UNT;
			itemB2.TM_BreakWeightVolume = QuantityUnit.M3;
			itemB2.TM_RelevantValue = 1000m;

			var lineA = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			lineA.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var lineC = entry.AddRateLine("CAF", PercentageCalculator.Code);
			lineC.GetCalculator<PercentageCalculator>().Percent = 15m;

			var itemC = lineC.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			itemC.TM_AC = lineB.TL_AC;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 1000, 0.1M);
			Factory.Save();

			var expected = new[]
				 {
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 2000M,
							RevenueCalculationDescription = "BAF: 1000 Kilogram(s) @ AUD 2.00/KG (As Freighted)",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 10000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 10.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 300M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 2000.00 (BAF))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);
		}

		public void TestCalculationOrderResolvedWhenBAFDependsOnFRTWithDAM()
		{
			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader();

			var testRate = Helper.NewClientRate(consignee);
			var entry = testRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var lineC = entry.AddRateLine("CAF", PercentageCalculator.Code);
			lineC.GetCalculator<PercentageCalculator>().Percent = 15m;

			var lineB = entry.AddRateLine("BAF", HighestRateCalculator.Code);
			lineB.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.AsFreightedDontApplyWhenMin;

			var itemB1 = lineB.RateLineItems.AddNew();
			itemB1.TM_Type = Calculator.Items.Operator.UNT;
			itemB1.TM_BreakWeightVolume = QuantityUnit.KG;
			itemB1.TM_RelevantValue = 2m;

			var itemB2 = lineB.RateLineItems.AddNew();
			itemB2.TM_Type = Calculator.Items.Operator.UNT;
			itemB2.TM_BreakWeightVolume = QuantityUnit.M3;
			itemB2.TM_RelevantValue = 1000m;

			var lineA = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			lineA.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var itemC = lineC.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			itemC.TM_AC = lineB.TL_AC;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 1000, 0.1M);
			Factory.Save();

			var expected = new[]
				 {
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 2000M,
							RevenueCalculationDescription = "BAF: 1000 Kilogram(s) @ AUD 2.00/KG (As Freighted)",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 10000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 10.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 300M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 2000.00 (BAF))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);
		}

		public void TestCalculationOrderResolvedBySequence()
		{
			var testCompanyTariff = Helper.NewCompanyTariff();
			testCompanyTariff.TH_GlobalRateLevel = 1;
			var companyTariffEntry = testCompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			companyTariffEntry.RateLines.RemoveAndDeleteAll();

			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader();

			var testRate = Helper.NewClientRate(consignee);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var frtLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var warLine = entry.AddRateLine("WAR", PercentageCalculator.Code);
			var warLineCalculator = warLine.GetCalculator<PercentageCalculator>();
			warLineCalculator.Percent = 5m;

			var warApplyToItem1 = warLineCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			warApplyToItem1.TM_AC = Helper.ChargeCodes["CAF"].PK;
			var warApplyToItem2 = warLineCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			warApplyToItem2.TM_AC = frtLine.TL_AC;

			var bafLine = entry.AddRateLine("BAF", PercentageCalculator.Code);
			var bafLineCalculator = bafLine.GetCalculator<PercentageCalculator>();
			bafLineCalculator.Percent = 10m;

			var bafApplyToItem1 = bafLineCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			bafApplyToItem1.TM_AC = warLine.TL_AC;
			var bafApplyToItem2 = bafLineCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			bafApplyToItem2.TM_AC = frtLine.TL_AC;

			var cafLine = entry.AddRateLine("CAF", PercentageCalculator.Code);
			var cafLineCalculator = cafLine.GetCalculator<PercentageCalculator>();
			cafLineCalculator.Percent = 15m;

			var cafApplyToItem1 = cafLineCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			cafApplyToItem1.TM_AC = bafLine.TL_AC;
			var cafApplyToItem2 = cafLineCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			cafApplyToItem2.TM_AC = frtLine.TL_AC;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 1000, 0.1M);
			Factory.Save();

			var expected = new[]
				 {
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 500M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 5000.00 (WAR + FRT))",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 750M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 5000.00 (BAF + FRT))"
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 250M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5000.00 (CAF + FRT))"
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
				};

			AutorateAndAssert(expected, shipment, consignee);

			warLine.SetCalculationOrder(5);

			Factory.Save();

			AssertEquals(5, warLine.GetCalculationOrder());

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 525M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 5250.00 (WAR + FRT WAR 250.00 + FRT 5000.00))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 828.75M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 5525.00 (BAF + FRT BAF 525.00 + FRT 5000.00))"
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 250M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5000.00 (CAF + FRT))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);

			warLine.SetCalculationOrder(5);
			bafLine.SetCalculationOrder(6);
			cafLine.SetCalculationOrder(7);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 525M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 5250.00 (WAR + FRT WAR 250.00 + FRT 5000.00))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 828.75M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 5525.00 (BAF + FRT BAF 525.00 + FRT 5000.00))"
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 250M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5000.00 (CAF + FRT))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);

			warLine.SetCalculationOrder(5);
			bafLine.SetCalculationOrder(6);
			cafLine.SetCalculationOrder(4);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 528.75M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 5287.50 (WAR + FRT WAR 287.50 + FRT 5000.00))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 750M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 5000.00 (BAF + FRT))"
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 287.5M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5750.00 (CAF + FRT CAF 750.00 + FRT 5000.00))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);

			warLine.SetCalculationOrder(0);
			cafLine.SetCalculationOrder(0);
			bafLine.SetCalculationOrder(100);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 500M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 5000.00 (WAR + FRT))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 825M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 5500.00 (BAF + FRT BAF 500.00 + FRT 5000.00))"
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 291.25M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5825.00 (CAF + FRT CAF 825.00 + FRT 5000.00))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);

			warLine.SetCalculationOrder(6);
			cafLine.SetCalculationOrder(0);
			bafLine.SetCalculationOrder(5);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 500M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 5000.00 (WAR + FRT))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 825M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 5500.00 (BAF + FRT BAF 500.00 + FRT 5000.00))"
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 250M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5000.00 (CAF + FRT))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);

			//Assertions below make sure sequence is ignored when there are no conflicts

			warApplyToItem1.Delete();
			cafLine.SetCalculationOrder(4);
			bafLine.SetCalculationOrder(5);
			warLine.SetCalculationOrder(6);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 525M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 5250.00 (WAR + FRT WAR 250.00 + FRT 5000.00))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 828.75M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 5525.00 (BAF + FRT BAF 525.00 + FRT 5000.00))"
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 250M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5000.00 (FRT))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);

			warApplyToItem1 = warLineCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			warApplyToItem1.TM_AC = cafLine.TL_AC;
			bafApplyToItem1.Delete();

			warLine.SetCalculationOrder(4);
			cafLine.SetCalculationOrder(5);
			bafLine.SetCalculationOrder(6);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 500M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 5000.00 (FRT))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 825M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 5500.00 (BAF + FRT BAF 500.00 + FRT 5000.00))"
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 291.25M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5825.00 (FRT + CAF CAF 825.00 + FRT 5000.00))"
						}
				};

			AutorateAndAssert(expected, shipment, consignee);
		}

		public void TestCalculationOrderResolvedBySequence2()
		{
			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader();

			var testRate = Helper.NewClientRate(consignee);
			var entry = testRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var lineA = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			lineA.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var lineBA = entry.AddRateLine("WAR", PercentageCalculator.Code);
			var lineBACalculator = lineBA.GetCalculator<PercentageCalculator>();
			lineBACalculator.Percent = 5m;

			var bAitem = lineBACalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			bAitem.TM_AC = lineA.TL_AC;

			var lineCBE = entry.AddRateLine("BAF", PercentageCalculator.Code);
			var lineCBECalculator = lineCBE.GetCalculator<PercentageCalculator>();
			lineCBECalculator.Percent = 10m;

			var lineEDA = entry.AddRateLine("FSC", PercentageCalculator.Code);
			var lineEDACalculator = lineEDA.GetCalculator<PercentageCalculator>();
			lineEDACalculator.Percent = 20m;

			var cBitem = lineCBECalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			cBitem.TM_AC = lineBA.TL_AC;
			var cEitem = lineCBECalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			cEitem.TM_AC = lineEDA.TL_AC;

			var lineDC = entry.AddRateLine("CAF", PercentageCalculator.Code);
			lineDC.GetCalculator<PercentageCalculator>().Percent = 15m;

			var dCitem = lineDC.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			dCitem.TM_AC = lineCBE.TL_AC;

			var eDitem = lineEDACalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			eDitem.TM_AC = lineDC.TL_AC;
			var eAitem = lineEDACalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			eAitem.TM_AC = lineA.TL_AC;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 1000, 0.1M);
			Factory.Save();

			var expected = new[]
				 {
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 25M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 250.00 (WAR + FSC))",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 0M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 0.00 (BAF))",
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 250M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5000.00 (FRT))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "FSC",
							JR_OSSellAmt = 1000M,
							RevenueCalculationDescription = "FSC: 20.00% of (AUD 5000.00 (CAF + FRT))",
						},
				};

			AutorateAndAssert(expected, shipment, consignee);

			lineEDA.SetCalculationOrder(1);
			lineCBE.SetCalculationOrder(2);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 125M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 1250.00 (WAR + FSC WAR 250.00 + FSC 1000.00))",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 18.75M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 125.00 (BAF))",
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 250M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5000.00 (FRT))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "FSC",
							JR_OSSellAmt = 1000M,
							RevenueCalculationDescription = "FSC: 20.00% of (AUD 5000.00 (CAF + FRT))",
						},
				};

			AutorateAndAssert(expected, shipment, consignee);

			lineEDA.SetCalculationOrder(2);
			lineCBE.SetCalculationOrder(1);

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 25M,
							RevenueCalculationDescription = "BAF: 10.00% of (AUD 250.00 (WAR + FSC))",
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 3.75M,
							RevenueCalculationDescription = "CAF: 15.00% of (AUD 25.00 (BAF))",
						},
					new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 250M,
							RevenueCalculationDescription = "WAR: 5.00% of (AUD 5000.00 (FRT))",
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5000M,
							RevenueCalculationDescription = "1000 Kilogram(s) @ AUD 5.00/KG",
						},
					new AssertionCharge
						{
							ChargeCode = "FSC",
							JR_OSSellAmt = 1000M,
							RevenueCalculationDescription = "FSC: 20.00% of (AUD 5000.00 (CAF + FRT))",
						},
				};

			AutorateAndAssert(expected, shipment, consignee);
		}

		public void TestCalculationOrderResolved_LoadingAndCustomsBrokerageCharges()
		{
			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader();
			var testRate = Helper.NewClientRate(consignee);

			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Apportion;

			var entryAIR = testRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entryAIR.RateLines.RemoveAndDeleteAll();
			entryAIR.AddFlatRateLine("BAF", 2000m);
			AddDependentRateLine(entryAIR, "CAF", ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Freight, 50m);

			var entryORG = testRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX");
			entryORG.RateLines.RemoveAndDeleteAll();
			AddDependentRateLine(entryORG, "LOD", ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.Freight, 10m);
			AddDependentRateLine(entryORG, "OBR", ChargeCodeGroupList.Codes.OriginBrokerage, ChargeCodeGroupList.Codes.Loading, 40m, true);

			var entryDST = testRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "USLAX");
			entryDST.RateLines.RemoveAndDeleteAll();
			AddDependentRateLine(entryDST, "BRK", ChargeCodeGroupList.Codes.Brokerage, ChargeCodeGroupList.Codes.OriginBrokerage, 50m);
			AddDependentRateLine(entryDST, "UNL", ChargeCodeGroupList.Codes.Unloading, ChargeCodeGroupList.Codes.Brokerage, 25m, true);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 1000);

			Factory.Save();

			var chargeCodes = string.Join(",", ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.OriginBrokerage, ChargeCodeGroupList.Codes.Brokerage, ChargeCodeGroupList.Codes.Unloading);
			Env.Registry.Rating.SetFreightRatedCodes(chargeCodes);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 2000M,
					RevenueCalculationDescription = "BAF: Base Rate AUD 2000.00"
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 1000M,
					RevenueCalculationDescription = "CAF: 50.00% of (AUD 2000.00 (Freight Charges BAF))",
				},
				new AssertionCharge
				{
					ChargeCode = "LOD",
					JR_OSSellAmt = 300M,
					RevenueCalculationDescription = "LOD: 10.00% of (AUD 3000.00 (Freight Charges CAF 1000.00 + BAF 2000.00))"
				},
				new AssertionCharge
				{
					ChargeCode = "OBR",
					JR_OSSellAmt = 1320M,
					RevenueCalculationDescription = "OBR: 40.00% of (AUD 3300.00 (Loading Charges + Freight Charges LOD 300.00 + CAF 1000.00 + BAF 2000.00))"
				},
				new AssertionCharge
				{
					ChargeCode = "BRK",
					JR_OSSellAmt = 660M,
					RevenueCalculationDescription = "BRK: 50.00% of (AUD 1320.00 (Origin Customs Brokerage Charges OBR))"
				},
				new AssertionCharge
				{
					ChargeCode = "UNL",
					JR_OSSellAmt = 915M,
					RevenueCalculationDescription = "UNL: 25.00% of (AUD 3660.00 (Customs Brokerage Charges + Freight Charges BRK 660.00 + CAF 1000.00 + BAF 2000.00))"
				},
			};

			AutorateAndAssert(expected, shipment, consignee);

			RateLine AddDependentRateLine(RateEntry entry, string chargeCode, string chargeGroup, string depedentChargeGroup, decimal percent, bool addFRTDependency = false)
			{
				var accChargeCode = Helper.ChargeCodes[chargeCode];
				accChargeCode.AC_ChargeGroup = chargeGroup;

				var line = entry.AddRateLine(accChargeCode, PercentageCalculator.Code, "", "AUD");
				line.GetCalculator<PercentageCalculator>().Percent = percent;

				var lineItem = line.RateLineItems.AddNew();
				lineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
				lineItem.TM_Text = depedentChargeGroup;

				if (addFRTDependency)
				{
					lineItem = line.RateLineItems.AddNew();
					lineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
					lineItem.TM_Text = ChargeCodeGroupList.Codes.Freight;
				}
				return line;
			}
		}

		#endregion

		#region ShippingOnlyLooksAtLocalClientRates

		public void TestShippingOnlyLooksAtLocalClientRates()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUBNE", "USLAX", ZString.Empty, "40GP");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUBNE", "USLAX", ZString.Empty, "40GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1900.1234m;

			var bill = Factory.NewWithValidTestData<BillOfLading>();
			bill.JS_TransportMode = TransportModes.Sea;
			bill.JS_PackingMode = ContainerModes.FCL;
			bill.JS_INCO = "CLT";
			bill.JS_RL_NKOrigin = "AUBNE";
			bill.JS_RL_NKDestination = "USLAX";
			bill.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			bill.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			var container = bill.ShippingContainers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 1900.12m,
						}
				};

			AutorateAndAssert(expected, bill, localClient);
		}

		#endregion

		#region AutoRate BillOfLading

		public void TestAutoRatingBillOfLading_ChargesAreNotFilteredDueToPaymentTerm()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OPCH");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DTHC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntryContainerized = rate.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "EETLL", "AUSYD", ZString.Empty, "20GP");
			rateEntryContainerized.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntryContainerized.Currency);
			var rateLineContainerized = rateEntryContainerized.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateLineContainerized.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var rateEntryOrigin = rate.AddRateEntry(RatingConstants.RateCategory.SOR, RateMode.FCL, "EETLL", "AUSYD", ZString.Empty, "20GP");
			rateEntryOrigin.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntryOrigin.Currency);
			var rateLineOrigin = rateEntryOrigin.AddRateLine("OPCH", UnitCalculator.Code, QuantityUnit.CN);
			rateLineOrigin.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var rateEntryDestination = rate.AddRateEntry(RatingConstants.RateCategory.SDE, RateMode.FCL, "EETLL", "AUSYD", ZString.Empty, "20GP");
			rateEntryDestination.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntryDestination.Currency);
			var rateLineDestination1 = rateEntryDestination.AddRateLine("DTHC", UnitCalculator.Code, QuantityUnit.CN);
			rateLineDestination1.GetCalculator<UnitCalculator>().PerUnit = 50m;
			var rateLineDestination2 = rateEntryDestination.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN);
			rateLineDestination2.GetCalculator<UnitCalculator>().PerUnit = 70m;

			var bill = Factory.NewWithValidTestData<BillOfLading>();
			bill.JS_TransportMode = TransportModes.Sea;
			bill.JS_PackingMode = ContainerModes.FCL;
			bill.JS_INCO = "PPD";
			bill.JS_RL_NKOrigin = "EETLL";
			bill.JS_RL_NKDestination = "AUSYD";
			bill.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			bill.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			var container = bill.ShippingContainers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			Factory.Save();

			var expectedCharges = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 1000m
						},
					new AssertionCharge
						{
							ChargeCode = "OPCH",
							JR_OSSellAmt = 100m
						},
					new AssertionCharge
						{
							ChargeCode = "DTHC",
							JR_OSSellAmt = 50m
						},
					new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_OSSellAmt = 70m
						},
				};

			AutorateAndAssert(expectedCharges, bill, localClient);
		}

		#endregion

		#region IncludeGSTCheckboxForDINAndPEBCalculators

		public void TestIncludeGSTCheckboxForDINAndPEBCalculators()
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("FRT", FlatCalculator.Code, "", CurrencyCodes.Australia);
			line1.GetCalculator<FlatCalculator>().BaseRate = 1000m;

			var line2 = entry.AddRateLine("BAF", DisbursementInterestCalculator.Code, "", CurrencyCodes.Australia);
			line2.GetCalculator<DisbursementInterestCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			line2.GetCalculator<DisbursementInterestCalculator>().Uplift = 5;
			line2.GetCalculator<DisbursementInterestCalculator>().AdjustmentDays = 3;

			var line3 = entry.AddRateLine("CAF", PercentageBreaksCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			line3.GetCalculator<PercentageBreaksCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			line3.GetCalculator<PercentageBreaksCalculator>()["-900"] = (ZDecimal)0m;
			line3.RateLineItems[line3.RateLineItems.Count - 1].TM_BreakMinimum = 7m;
			line3.GetCalculator<PercentageBreaksCalculator>()["+900"] = (ZDecimal)0m;
			line3.RateLineItems[line3.RateLineItems.Count - 1].TM_BreakMinimum = 6m;

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TAX1";
			taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate1.SetRateNumerator_ForTestOnly(5);

			var chargeCode = frtChargeCode;
			chargeCode.AC_AT_GSTRate = taxRate1.PK;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";
			shipment.CustomsEntryNumberType = "T1";
			shipment.JS_ActualWeight = 500M;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 1000m,
							RevenueCalculationDescription = "FRT: Base Rate AUD 1000.00"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 0.41m,
							RevenueCalculationDescription = "BAF: AUD 1000.00 (FRT) @ 5 % pa - 0 Days + 3 Adjustment Days Cash On Delivery (3 effective days) For Test Client #1",
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 70m,
							RevenueCalculationDescription = "CAF: 7.00% of (AUD 1000.00 (FRT))",
						}
				};

			AutorateAndAssert(expected, shipment, null, rate.Header);

			line2.GetCalculator<DisbursementInterestCalculator>().IncludeGST = true;
			line3.GetCalculator<PercentageBreaksCalculator>().IncludeGST = true;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 1000m,
							RevenueCalculationDescription = "FRT: Base Rate AUD 1000.00"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 0.43m,
							RevenueCalculationDescription = "BAF: AUD 1050.00 (FRT) @ 5 % pa - 0 Days + 3 Adjustment Days Cash On Delivery (3 effective days) For Test Client #1",
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 73.50m,
							RevenueCalculationDescription = "CAF: 7.00% of (AUD 1050.00 (FRT))",
						},
				};

			AutorateAndAssert(expected, shipment, null, rate.Header);
		}

		#endregion

		#region TestExportPPDOriginMayBePaidByConsignee

		public void TestExportPPDOriginMayBePaidByConsignee()
		{
			var collection = new RatesPrioritiesCollection();
			collection.AddNew(RatingDebtorOrgTypes.LC);
			collection.AddNew(RatingDebtorOrgTypes.CNR);
			collection.AddNew(RatingDebtorOrgTypes.LCBK);
			collection.AddNew(RatingDebtorOrgTypes.AG);
			collection.AddNew(RatingDebtorOrgTypes.CNE);

			RatingDataRegistry.Instance.ExportPrepaidPriorities.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_FullName = "CNR ORG";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_FullName = "CNE ORG";
			var agent = Helper.NewOrgHeader(1);
			agent.OH_FullName = "AGN ORG";

			var tariff = Helper.NewCompanyTariff();
			var tariffFRTEntry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
			tariffFRTEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(tariffFRTEntry.Currency);
			var tariffFRTLine = tariffFRTEntry.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)tariffFRTLine.Calculator).BaseRate = 50;

			var tariffORGEntry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			tariffORGEntry.RateLines.RemoveAndDeleteAll();
			var tariffORGLine1 = tariffORGEntry.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)tariffORGLine1.Calculator).BaseRate = 5;

			var tariffORGLine2 = tariffORGEntry.AddRateLine("OCART", FlatCalculator.Code);
			((FlatCalculator)tariffORGLine2.Calculator).BaseRate = 10;
			tariff.Factory.Save();

			var rate = Helper.NewClientRate(consignee);
			var rateFRTEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "US");
			rateFRTEntry.RateLines.RemoveAndDeleteAll();
			var rateFRTLine1 = rateFRTEntry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.M3);
			((MinimumOrPerUnitCalculator)rateFRTLine1.Calculator).Minimum = 20;
			((MinimumOrPerUnitCalculator)rateFRTLine1.Calculator).PerUnit = 40;

			var rateORGEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "US");
			rateORGEntry.RateLines.RemoveAndDeleteAll();
			var rateORGLine = rateORGEntry.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)rateORGLine.Calculator).BaseRate = 6;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_INCO = "EXW";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_ActualWeight = 120m;
			shipment.JS_ActualVolume = 0.12m;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 50.00m,
							RevenueCalculationDescription = @"FRT: Base Rate USD 50.00
Charge located in Company Tariff Level 1 (Linked to: AGNORGSYD, CNEORGSYD, CNRORGSYD) with the following details:"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 5.00m,
							RevenueCalculationDescription = @"ODOC: Base Rate AUD 5.00
Charge located in Company Tariff Level 1 (Linked to: AGNORGSYD, CNEORGSYD, CNRORGSYD) with the following details:"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 10.00m,
							RevenueCalculationDescription = @"OCART: Base Rate AUD 10.00
Charge located in Company Tariff Level 1 (Linked to: AGNORGSYD, CNEORGSYD, CNRORGSYD) with the following details:"
						}
				};

			AutorateAndAssert(expected, shipment, consignor, agent);

			consignor.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			agent.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 20.00m,
							RevenueCalculationDescription = @"FRT: Minimum USD 20.00
Charge located in CNEORGSYD client rate with the following details:"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 6.00m,
							RevenueCalculationDescription = @"ODOC: Base Rate AUD 6.00
Charge located in CNEORGSYD client rate with the following details:"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 10.00m,
							RevenueCalculationDescription = @"OCART: Base Rate AUD 10.00
Charge located in Company Tariff Level 1 (Linked to: CNEORGSYD) with the following details:"
						}
				};

			AutorateAndAssert(expected, shipment, consignor, agent);
		}

		#endregion

		#region TestPerUnitPriceCanBeZero

		public void TestPerUnitPriceCanBeZero()
		{
			var client = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("ORG", "SEA", "AUSYD", "USLAX");
			var line = entry.AddRateLine("ODOC", FlatPlusPerUnitCalculator.Code, QuantityUnit.KG);
			((FlatPlusPerUnitCalculator)line.Calculator).BaseRate = 200;
			((FlatPlusPerUnitCalculator)line.Calculator).PerUnit = 0;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 200.00m,
							RevenueCalculationDescription = "ODOC: Base Rate AUD 200.00"
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region TestCompanyTariffRelated

		public void TestCompanyTariffLevel2AndClientRate()
		{
			var client = Helper.NewOrgHeader(2);
			client.OH_Code = "ORG1";
			var consignor = Helper.NewOrgHeader(0);
			consignor.OH_Code = "ORG3";

			var companyTariff1 = Helper.NewCompanyTariff();
			var tariffEntry = companyTariff1.AddRateEntry("DST", "LCL", "USLAX", "AUSYD");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine("DDOC", FlatCalculator.Code);
			((FlatCalculator)tariffLine.Calculator).BaseRate = 10m;

			var companyTariff2 = Helper.NewCompanyTariff();
			companyTariff2.TH_GlobalRateLevel = 2;
			companyTariff2.Discounts.SetDiscount("DST", 30m);

			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry("DST", "LCL", "USLAX", "AUSYD");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			((CompanyTariffOrCostBasedCalculator)line.Calculator).Percent = 10m;
			line.TL_CompanyTariffLevel = 1; //I guess this is what happens when you apply comany tariff in the GUI

			companyTariff1.Factory.Save();
			companyTariff2.Factory.Save();
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = client.PK;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7.70m,
					RevenueCalculationDescription = @"DDOC: 110.00% of (Base Rate AUD 7.00)
Charge located in ORG1 client rate"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestCompanyTariffOverridesClientRate()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)tariffLine.Calculator).PerUnit = 5.1612m;

			tariff.Factory.Save();

			var rate1 = Helper.NewClientRate(consignee);
			var rateEntry1 = rate1.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry1.Currency);
			var rateLine1 = rateEntry1.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			((CompanyTariffOrCostBasedCalculator)rateLine1.Calculator).Percent = 20m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorPK = Helper.NewOrgHeader().PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 5161.2m,
							RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ USD 5.1612/KG"
						}
				};

			AutorateAndAssert(expected, shipment, client);

			var rate2 = Helper.NewClientRate(client);
			var rateEntry2 = rate2.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)rateLine2.Calculator).PerUnit = 7m;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7000m,
					RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ USD 7.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}
		public void TestTariffLevelIsNotObtainedViaCompanyData()
		{
			var newFactory = new BusinessObjectFactory();
			var consignee = newFactory.NewWithValidTestData<OrgHeader>();
			var client = newFactory.NewWithValidTestData<OrgHeader>();

			var consigneeCompanyDataPK = consignee.CompanyData.PK;
			var clientCompanyDataPK = client.CompanyData.PK;
			consignee.CompanyData.Delete();
			client.CompanyData.Delete();

			var orgRateTariffLevelClient = newFactory.NewWithValidTestData<OrgRateTariffLevel>();
			orgRateTariffLevelClient.P7_TariffType = "DEF";
			orgRateTariffLevelClient.P7_Mode = "ALL";
			orgRateTariffLevelClient.P7_Direction = "ALL";
			orgRateTariffLevelClient.P7_TariffLevel = 1;
			orgRateTariffLevelClient.P7_OH = client.PK;
			orgRateTariffLevelClient.P7_GC = Env.CurrentCompanyPK;

			newFactory.Save();

			var consigneeCompanyData = Factory.Load<OrgCompanyData>(consigneeCompanyDataPK);
			var clientCompanyData = Factory.Load<OrgCompanyData>(clientCompanyDataPK);
			AssertNull(consigneeCompanyData);
			AssertNull(clientCompanyData);
			Assert(orgRateTariffLevelClient.IsInDatabase);

			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD");
			AddParityExchangeRate(tariffEntry.Currency);
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)tariffLine.Calculator).PerUnit = 5.1612m;
			tariff.Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorPK = Helper.NewOrgHeader().PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 5161.2m,
							RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ USD 5.1612/KG"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestRateAuditPickUpCorrectCompanyTariffLevel()
		{
			var client = Helper.NewOrgHeader(2);
			client.OH_Code = "ORG1";
			var consignee = Helper.NewOrgHeader(0);
			consignee.OH_Code = "ORG2";
			var consignor = Helper.NewOrgHeader(0);
			consignee.OH_Code = "ORG3";

			var companyTariff1 = Helper.NewCompanyTariff();
			var tariffEntry = companyTariff1.AddRateEntry("DST", "LCL", "USLAX", "AUSYD");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)tariffLine.Calculator).BaseRate = 10m;

			var companyTariff2 = Helper.NewCompanyTariff();
			companyTariff2.TH_GlobalRateLevel = 2;
			companyTariff2.Discounts.SetDiscount("DST", 30m);

			companyTariff1.Factory.Save();
			companyTariff2.Factory.Save();
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 7m,
							RevenueCalculationDescription = @"FRT: Base Rate AUD 7.00
Charge located in Company Tariff Level 2 (Linked to: ORG1) with the following details:"
						}
				};

			AutorateAndAssert(expected, shipment, client);

			var tariffEntry2 = companyTariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST)[0];
			var index = tariffEntry2.RateLines.OverrideTariffLines(new[] { tariffEntry2.RateLines[0] });
			var overriddenLine = tariffEntry2.RateLines[index];
			overriddenLine.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)overriddenLine.Calculator).BaseRate = 15m;

			companyTariff2.Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 15m,
							RevenueCalculationDescription = @"FRT: Base Rate AUD 15.00
Charge located in Company Tariff Level 2 (Linked to: ORG1) with the following details:"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestAutoRateFilterSupplierBasedOnRateEntryCategory()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var client = Helper.NewOrgHeader(1);
			client.OH_Code = "ORG1";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_Code = "ORG2";
			var consignor = Helper.NewOrgHeader(1);
			consignee.OH_Code = "ORG3";

			TransportProvider1.OH_IsCreditor = true;
			TransportProvider2.OH_IsCreditor = true;

			TransportProvider1.Factory.Save();
			TransportProvider2.Factory.Save();

			var baseCompanyTariff = Helper.NewCompanyTariff();

			var freightChargesTariffEntry = baseCompanyTariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, origin, destination);
			freightChargesTariffEntry.RateLines.RemoveAndDeleteAll();
			Helper.AddRateLineWithFlatCalculatorToRateEntry(freightChargesTariffEntry, "FRT", 180m);

			var originChargesTariffEntry1 = baseCompanyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, origin, string.Empty);
			Helper.AddRateLineWithFlatCalculatorToRateEntry(originChargesTariffEntry1, "OCART", 200m);
			Helper.AddRateLineWithFlatCalculatorToRateEntry(originChargesTariffEntry1, "ODOC", 100m);

			var originChargesTariffEntry2 = baseCompanyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, origin, string.Empty);
			originChargesTariffEntry2.TI_OH_Supplier = TransportProvider1.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(originChargesTariffEntry2, "OCART", 10m);
			Helper.AddRateLineWithFlatCalculatorToRateEntry(originChargesTariffEntry2, "ODOC", 5m);

			var destinationChargesTariffEntry1 = baseCompanyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, string.Empty, destination);
			Helper.AddRateLineWithFlatCalculatorToRateEntry(destinationChargesTariffEntry1, "DCART", 150m);
			Helper.AddRateLineWithFlatCalculatorToRateEntry(destinationChargesTariffEntry1, "DDOC", 75m);

			var destinationChargesTariffEntry2 = baseCompanyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, string.Empty, destination);
			destinationChargesTariffEntry2.TI_OH_Supplier = TransportProvider2.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(destinationChargesTariffEntry2, "DCART", 30m);
			Helper.AddRateLineWithFlatCalculatorToRateEntry(destinationChargesTariffEntry2, "DDOC", 15m);

			baseCompanyTariff.Factory.Save();

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_ActualWeight = 1000m;

			Factory.Save();

			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = TransportProvider1.MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = TransportProvider2.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 180m,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 5m,
					CostAccountCode = TransportProvider1.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "OCART",
					JR_OSSellAmt = 10m,
					CostAccountCode = TransportProvider1.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 15m,
					CostAccountCode = TransportProvider2.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSSellAmt = 30m,
					CostAccountCode = TransportProvider2.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, client);

			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = TransportProvider2.MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = TransportProvider1.MainAddress.PK;

			expected = new[]
			{
					new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 180m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "OCART",
					JR_OSSellAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 75m
				},
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSSellAmt = 150m
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestCompanyTarrifBasedAgencyCalculator()
		{
			var tariff = Helper.NewCompanyTariff();
			var rateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "", "AUSYD");

			var rateLine = rateEntry.AddRateLine("FRT", AgencyCalculator.Code, currencyCode: "AUD");
			rateLine.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			rateLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
			rateLine.GetCalculator<AgencyCalculator>().AgencyRate = 150m;
			rateLine.Factory.Save();

			var client = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "", "AUSYD");
			var clientRateLine = clientRateEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, currencyCode: "AUD");
			clientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = -25m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorPK = Helper.NewOrgHeader().PK;
			shipment.ConsigneePK = client.PK;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 125m,
							RevenueCalculationDescription = "FRT: Base Rate AUD 150.00 + Base Rate AUD -25.00"
						}
				};

			AutorateAndAssert(expected, shipment, client);

			rateLine.GetCalculator<AgencyCalculator>().MessageType = SharedJobMessageTypeList.Codes.Import;
			rateLine.GetCalculator<AgencyCalculator>().MessageSubType = "FRM";
			rateLine.Factory.Save();

			//This is basically to confirm having message type and sub type has no effect on the result of auto rating
			AutorateAndAssert(expected, shipment, client);
		}

		public void TestMatchCompanyTariffRatesBasedOnOrgCompanyTariffData()
		{
			var localClient = Helper.NewOrgHeader();
			localClient.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.EXP), "LSE", 2);

			var overseasAgent = Helper.NewOrgHeader();
			overseasAgent.CompanyData.RateTariffLevels.SetLevel("DST", nameof(OrgRateTariffLevel.Directions.EXP), "LSE", 2);

			var shipment = CreateForwardingShipment("AIR", Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1000, 9.68m);
			shipment.JS_UniqueConsignRef = "S00001";
			shipment.JS_INCO = string.Empty;

			var levelOneCompanyTariff = Helper.NewCompanyTariff();
			levelOneCompanyTariff.TH_GlobalRateLevel = 1;

			var orgCharges = levelOneCompanyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", string.Empty);
			orgCharges.RateLines.RemoveAndDeleteAll();
			Helper.AddRateLineWithFlatCalculatorToRateEntry(orgCharges, "OCART", 200m);
			Helper.AddRateLineWithFlatCalculatorToRateEntry(orgCharges, "ODOC", 100m);

			var dstCharges = levelOneCompanyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "AU", "CN");
			dstCharges.RateLines.RemoveAndDeleteAll();
			Helper.AddRateLineWithFlatCalculatorToRateEntry(dstCharges, "DCART", 150m);
			Helper.AddRateLineWithFlatCalculatorToRateEntry(dstCharges, "DDOC", 75m);

			levelOneCompanyTariff.Factory.Save();

			var levelTwoCompanyTariff = Helper.NewCompanyTariff();
			levelTwoCompanyTariff.TH_GlobalRateLevel = 2;

			levelTwoCompanyTariff.Discounts.SetDiscount("DST", 10m);
			levelTwoCompanyTariff.Discounts.SetDiscount("ORG", 10m);

			levelTwoCompanyTariff.Factory.Save();

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 90m,
				},
				new AssertionCharge
				{
					ChargeCode = "OCART",
					JR_OSSellAmt = 180m,
				},
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSSellAmt = 135m,
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 67.5m,
				}
			};

			AutorateAndAssert(expected, shipment, localClient, overseasAgent);
		}

		#endregion

		#region  Support fallback of checking RateOrigin/RateDestination in Autorating

		public void TestAutoRatingShipment_ShouldConsiderCriteriaRateOriginAndDestinationAndBringRates()
		{
			var rate = Helper.NewClientRate(NewClient);

			var rateEntryA = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUBNE", "US", "CAF", 10);
			var rateEntryB = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "CN", "CAF", 20);

			var rateEntryC = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUBNE", "", "BAF", 30);
			var rateEntryD = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "CNSHA", "BAF", 40);

			rate.AddRateEntryWithFlatRateLine("ORG", "LSE", "AU", "", "ODOC", 50);
			rate.AddRateEntryWithFlatRateLine("DST", "LSE", "", "CN", "DDOC", 60);

			var shipment = CreateForwardingShipment(TransportModes.Air, NewClient.PK, Consignee.PK, "AUBNE", "USLAX", 1000m);
			shipment.JS_RL_NKFreightRateOrigin = "AUSYD";
			shipment.JS_RL_NKFreightRateDestination = "CNSHA";
			shipment.JS_INCO = "";

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 10m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 40m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 50m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 60m
				}
			};

			var message = @"RateEntryA and RateEntryD are more specific ones for each charge group.
rateEntryA matches Shipment Origin and Destination and rateEntryD matches Shipment Rate Origin and Rate Destination";
			AutorateAndAssert(message, expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Log should contain expected lines",
				"Information: RateLine Filtered BAF-FLT-Client Rate NEWTESSYD	reason:	overridden by BAF-FLT-Client Rate NEWTESSYD by Origin Destination comparer",
				"Information: RateLine Filtered CAF-FLT-Client Rate NEWTESSYD	reason:	overridden by CAF-FLT-Client Rate NEWTESSYD by Origin Destination comparer");

			rateEntryB.TI_DestinationLRC = "CNSHA";
			rateEntryC.TI_DestinationLRC = "USLAX";
			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 10m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 30m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 50m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 60m
				}
			};

			message = @"For CAF Charge Code Group, RateEntryA is should be selected over RateEntryB because for Freight RateEntry, more specific Origin is preferred.
For BAF ChargeCode Group, Both RateEntryC and RateEntryD are specific but RateEntryC take more priority as it match Shipment Job Origin/Destination (RateEntryD matches Shipment Job RateOrigin/Destination)";
			AutorateAndAssert(message, expectedCharges, shipment, NewClient, autorateCosts: false);
		}

		public void TestAutoRatingShipment_OriginAndDestinationOfRateEntryHasPriorityOverRateOriginAndRateDestinationOfRateEntry()
		{
			var rate = Helper.NewClientRate(NewClient);

			var rateEntryA = rate.AddRateEntryWithFlatRateLine("ORG", "LSE", "AUBNE", "US", "ODOC", 10);
			rateEntryA.TI_RateOrigin = "AU";
			rateEntryA.TI_RateDestination = "CN";

			var rateEntryB = rate.AddRateEntryWithFlatRateLine("ORG", "LSE", "AU", "CN", "ODOC", 20);
			rateEntryB.TI_RateOrigin = "AUSYD";
			rateEntryB.TI_RateDestination = "CNSHA";

			var rateEntryC = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUBNE", "", "BAF", 30);
			rateEntryC.TI_RateOrigin = "AUSYD";
			rateEntryC.TI_RateDestination = "CN";

			var rateEntryD = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "CNSHA", "BAF", 40);
			rateEntryD.TI_RateOrigin = "AUSYD";
			rateEntryD.TI_RateDestination = "CN";

			var rateEntryE = rate.AddRateEntryWithFlatRateLine("DST", "LSE", "", "USLAX", "DDOC", 50);
			var rateEntryF = rate.AddRateEntryWithFlatRateLine("DST", "LSE", "", "CNSHA", "DDOC", 60);

			var shipment = CreateForwardingShipment(TransportModes.Air, NewClient.PK, Consignee.PK, "AUBNE", "USLAX", 1000m);
			shipment.JS_RL_NKFreightRateOrigin = "AUSYD";
			shipment.JS_RL_NKFreightRateDestination = "CNSHA";
			shipment.JS_INCO = "";

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 10m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 40m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 50m
				}
			};

			var message = @"RateEntryA, RateEntryD, and RateEntryE are more specific ones for each charge group.
rateEntryA matches Shipment Origin and Destination. rateEntryD matches Shipment Rate Origin and Rate Destination. RateEntryE matches Shipment Destination";
			AutorateAndAssert(message, expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Log should contain expected lines",
				"Information: RateLine Filtered BAF-FLT-Client Rate NEWTESSYD	reason:	overridden by BAF-FLT-Client Rate NEWTESSYD by Origin Destination comparer",
				"Information: RateLine Filtered ODOC-FLT-Client Rate NEWTESSYD	reason:	overridden by ODOC-FLT-Client Rate NEWTESSYD by Origin Destination comparer",
				"Information: RateLine Filtered DDOC-FLT-Client Rate NEWTESSYD\treason:\toverridden by DDOC-FLT-Client Rate NEWTESSYD by Origin Destination comparer");
		}

		public void TestAutoRatingShipment_MoreSpecificRateOriginAndRateDestinationIsPrefered()
		{
			var rate = Helper.NewClientRate(NewClient);

			var rateEntryA = rate.AddRateEntryWithFlatRateLine("DST", "LSE", "AUBNE", "USLAX", "DDOC", 10);
			rateEntryA.TI_RateOrigin = "AUSYD";
			rateEntryA.TI_RateDestination = "CN";

			var rateEntryB = rate.AddRateEntryWithFlatRateLine("DST", "LSE", "AUBNE", "USLAX", "DDOC", 20);
			rateEntryB.TI_RateOrigin = "AU";
			rateEntryB.TI_RateDestination = "CNSHA";

			var rateEntryC = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUBNE", "USLAX", "BAF", 30);
			rateEntryC.TI_RateOrigin = "AUSYD";
			rateEntryC.TI_RateDestination = "";

			var rateEntryD = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUBNE", "USLAX", "BAF", 40);
			rateEntryD.TI_RateOrigin = "AU";
			rateEntryD.TI_RateDestination = "CNSHA";

			var shipment = CreateForwardingShipment(TransportModes.Air, NewClient.PK, Consignee.PK, "AUBNE", "USLAX", 1000m);
			shipment.JS_RL_NKFreightRateOrigin = "AUSYD";
			shipment.JS_RL_NKFreightRateDestination = "CNSHA";
			shipment.JS_INCO = "";

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 30m
				}
			};

			AutorateAndAssert("RateEntryB and RateEntryC are more specific ones for each charge group", expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Log should contain expected lines",
				"Information: RateLine Filtered BAF-FLT-Client Rate NEWTESSYD	reason:	overridden by BAF-FLT-Client Rate NEWTESSYD by Rate Origin Rate Destination comparer",
				"Information: RateLine Filtered DDOC-FLT-Client Rate NEWTESSYD	reason:	overridden by DDOC-FLT-Client Rate NEWTESSYD by Rate Origin Rate Destination comparer");
		}

		#endregion

		#region CrossTradeRates

		public void TestCrossTradeRates()
		{
			var client = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(consignor);
			var entry = rate.AddRateEntry("DST", "SEA", "", "");
			entry.TI_IsCrossTrade = true;
			var line = entry.AddRateLine("DDOC", FlatCalculator.Code);
			((FlatCalculator)line.Calculator).BaseRate = 200;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "LCL";
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = client.PK;
			shipment1.JS_INCO = "FOB";

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "LCL";
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = client.PK;
			shipment2.JS_INCO = "FOB";

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "LCL";
			shipment3.ConsignorPK = consignor.PK;
			shipment3.ConsigneePK = client.PK;
			shipment3.JS_INCO = "FOB";

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_TransportMode = "SEA";
			shipment4.JS_PackingMode = "LCL";
			shipment4.ConsignorPK = consignor.PK;
			shipment4.ConsigneePK = client.PK;
			shipment4.JS_INCO = "FOB";

			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";

			shipment2.JS_RL_NKOrigin = "USLAX";
			shipment2.JS_RL_NKDestination = "AUSYD";

			shipment3.JS_RL_NKOrigin = "USLAX";
			shipment3.JS_RL_NKDestination = "GBLON";

			shipment4.JS_RL_NKOrigin = "USLAX";         //through the whole system such type of overseas movements is considered Domestic and not CrossTrade
			shipment4.JS_RL_NKDestination = "USCHI";

			Factory.Save();

			var expected1 = Array.Empty<AssertionCharge>();
			var expected2 = Array.Empty<AssertionCharge>();
			var expected4 = Array.Empty<AssertionCharge>();

			var expected3 = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 200.00m,
						RevenueCalculationDescription = "DDOC: Base Rate AUD 200.00"
					},
			};

			Assert(!shipment1.IsCrossTrade());
			Assert(!shipment2.IsCrossTrade());
			Assert(shipment3.IsCrossTrade());
			Assert(!shipment4.IsCrossTrade());
			AutorateAndAssert(expected1, shipment1, client);
			AutorateAndAssert(expected2, shipment2, client);
			AutorateAndAssert(expected3, shipment3, consignor);
			AutorateAndAssert(expected4, shipment4, client);
		}

		#endregion

		#region Support PlannedLoad and Discharge When AutoRating Shipment

		public void TestAutoRatingShipmentWithPlannedLoadAndDischarge_Export()
		{
			var rate = Helper.NewClientRate(NewClient);

			var rateEntryA = rate.AddRateEntryWithFlatRateLine("DST", "LCL", "AUSYD", "USLAX", "DDOC", 500);
			rateEntryA.TI_PlannedLoadLRC = "CNSHA";

			var rateEntryB = rate.AddRateEntryWithFlatRateLine("DST", "LCL", "AUSYD", "USLAX", "DDOC", 650);
			rateEntryB.TI_PlannedDischargeLRC = "HKHKG";

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "AUSYD", "USLAX", 8000m);
			shipment.JS_RL_NKLoadPort = "CNSHA";
			shipment.JS_RL_NKDischargePort = "HKHKG";
			shipment.JS_INCO = "";

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 650m,
				}
			};

			AutorateAndAssert("for Destination Entries, the rate entry which has matching discharge port is prefered (rate entry B).", expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Log should contain expected lines",
				"Information: RatingHeader Found Client Rate NEWTESSYD Entries: 2",
				"Information: RateLine Filtered DDOC-FLT-Client Rate NEWTESSYD	reason:	overridden by DDOC-FLT-Client Rate NEWTESSYD by Planned Load Planned Discharge comparer");

			rateEntryB.TI_PlannedLoadLRC = "CN";
			rateEntryB.TI_PlannedDischargeLRC = "HKHKG";
			Factory.Save();

			AutorateAndAssert("rateEntryB should still come through", expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Log should contain expected lines",
				"Information: RatingHeader Found Client Rate NEWTESSYD Entries: 2",
				"Information: RateLine Filtered DDOC-FLT-Client Rate NEWTESSYD	reason:	overridden by DDOC-FLT-Client Rate NEWTESSYD by Planned Load Planned Discharge comparer");

			rateEntryB.TI_PlannedLoadLRC = "";
			rateEntryB.TI_PlannedDischargeLRC = "HKKTG";
			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 500m,
				}
			};

			AutorateAndAssert("rateEntryB is no longer valid as its PlannedDischarge on rate entry does not match with planned discharge on Shipment. rateEntryA is a valid rate.", expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Only one entry should be found",
				"Information: RatingHeader Found Client Rate NEWTESSYD Entries: 1");

			shipment.JS_RL_NKLoadPort = "";
			shipment.JS_RL_NKDischargePort = "";
			Factory.Save();

			expectedCharges = Array.Empty<AssertionCharge>();
			AutorateAndAssert("There is no Planned Load and Planned Discharge on job and none of the rate entries are matched.", expectedCharges, shipment, NewClient, autorateCosts: false);
		}

		public void TestAutoRatingShipmentWithPlannedLoadAndDischarge_Import()
		{
			var rate = Helper.NewClientRate(NewClient);

			var rateEntryA = rate.AddRateEntryWithFlatRateLine("ORG", "LCL", "CNSHA", "AUSYD", "ODOC", 100);
			rateEntryA.TI_PlannedLoadLRC = "CN";

			var rateEntryB = rate.AddRateEntryWithFlatRateLine("ORG", "LCL", "CNSHA", "AUSYD", "ODOC", 200);
			rateEntryB.TI_PlannedDischargeLRC = "CN";

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "CNSHA", "AUSYD", 8000m);
			shipment.JS_RL_NKLoadPort = "CNAID";
			shipment.JS_RL_NKDischargePort = "CNANI";
			shipment.JS_INCO = "";

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 100m,
				}
			};

			AutorateAndAssert("For origin entries, the rate entry which has matching origin load port is prefered (rateEntryA).", expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Both entries should be found and one to be overriden",
				"Information: RatingHeader Found Client Rate NEWTESSYD Entries: 2",
				"Information: RateLine Filtered ODOC-FLT-Client Rate NEWTESSYD	reason:	overridden by ODOC-FLT-Client Rate NEWTESSYD by Planned Load Planned Discharge comparer");

			rateEntryA.TI_PlannedLoadLRC = "";
			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 200m,
				}
			};

			AutorateAndAssert("rateEntryB is a now more specific case", expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Both entries should be found and one to be overriden",
				"Information: RatingHeader Found Client Rate NEWTESSYD Entries: 2",
				"Information: RateLine Filtered ODOC-FLT-Client Rate NEWTESSYD	reason:	overridden by ODOC-FLT-Client Rate NEWTESSYD by Planned Load Planned Discharge comparer");

			rateEntryB.TI_PlannedLoadLRC = "CN";
			rateEntryB.TI_PlannedDischargeLRC = "CNSHA";
			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 100m,
				}
			};

			AutorateAndAssert("Only rateEntryA is applicable", expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Only one rate entry should be found",
				"Information: RatingHeader Found Client Rate NEWTESSYD Entries: 1");
		}

		#endregion

		#region TestRevenueAndCostAutoratedFromConsol

		public void TestRevenueAndCostRatedFromConsol()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var cnr = Helper.NewOrgHeader(1);
			var cne = Helper.NewOrgHeader();
			cnr.OH_IsDebtor = true;
			Factory.Save();

			var ct = Helper.NewCompanyTariff();
			var ctEntry = ct.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			ctEntry.RateLines.RemoveAndDeleteAll();
			ctEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 5;
			ctEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 4;

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;
			costEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = .5;

			ct.Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CIF";

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate())
			{
				job.JH_OA_LocalChargesAddr = cnr.MainAddress.PK;
				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
						{
							CostCalculationDescription = "BAF: 1500 Kilogram(s) @ AUD 1.00/KG",
							ChargeCode = "BAF",
							E6_OSCostAmount = 1500,
						},
					new AssertionCost
						{
							CostCalculationDescription = "CAF: 1500 Kilogram(s) @ AUD 0.50/KG",
							ChargeCode = "CAF",
							E6_OSCostAmount = 750,
						}
				};

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment, new[]
							{
								new AssertionCharge
									{
										ChargeCode = "BAF",
										JR_OSSellAmt = 7500,
										JR_OSCostAmt = 1500
									},
								new AssertionCharge
									{
										ChargeCode = "CAF",
										JR_OSSellAmt = 6000,
										JR_OSCostAmt = 750
									},
							}
					}
				};

				AutoCostAndAssert("", expectedCharges, expectedCosts, consol);
			}
		}

		#endregion

		#region Autorating Costs (Non-Consol Level Charge Only)

		public void TestAutoratingCostsNonConsolLevelChargeOnly_ShipmentLevel()
		{
			var (_, shipment, consignor, creditor) = SetupToAutorateNonConsolLevelChargeOnly();

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.LocalChargesPK = creditor.PK;
			testJob.JH_A_JOP = ZDateTime.Now;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "BAF" }, // Non-Consol Level Charge
			};

			AutorateAndAssert(expected, shipment, consignor, autorateCosts: true, isConsolLevelChargeExcluded: true, autorateRevenue: false);

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT" }, // Consol Level Charge
				new AssertionCharge { ChargeCode = "BAF" }, // Non-Consol Level Charge
			};

			AutorateAndAssert(expected, shipment, consignor, autorateCosts: true, isConsolLevelChargeExcluded: false, autorateRevenue: false);
		}

		public void TestAutoratingCostsNonConsolLevelChargeOnlyAndRevenue_ShipmentLevel()
		{
			var (_, shipment, consignor, creditor) = SetupToAutorateNonConsolLevelChargeOnly();

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.LocalChargesPK = creditor.PK;
			testJob.JH_A_JOP = ZDateTime.Now;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "BAF" }, // Non-Consol Level Charge
				new AssertionCharge { ChargeCode = "CC" }, // Consol Level Charge for Revenue
			};

			AutorateAndAssert(expected, shipment, consignor, autorateCosts: true, isConsolLevelChargeExcluded: true, autorateRevenue: true);

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT" }, // Consol Level Charge
				new AssertionCharge { ChargeCode = "BAF" }, // Non-Consol Level Charge
				new AssertionCharge { ChargeCode = "CC" }, // Consol Level Charge for Revenue
			};

			AutorateAndAssert(expected, shipment, consignor, autorateCosts: true, isConsolLevelChargeExcluded: false, autorateRevenue: true);
		}

		public void TestAutoratingCostsNonConsolLevelChargeOnly_ConsolShouldNotHaveThisOption()
		{
			var (consol, _, _, _) = SetupToAutorateNonConsolLevelChargeOnly();

			Factory.Save();

			using (var consolForm = new ZForm(consol))
			{
				consolForm.PlugIns.Add(ControllerIDs.Apportionment);
				consolForm.DisplayMode = ODisplayMode.Edit;

				using (var plugin = (ApportionmentPlugin)consolForm.PlugIns.Instances[0])
				{
					var actualMenuItems = plugin.TopLevelMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text);

					AssertCollectionNotContains("Autorate Costs (Non-Consol Level Charge Only)", actualMenuItems);
					AssertCollectionNotContains("Autorate Costs (Non-Consol Level Charge Only) and Revenue", actualMenuItems);
				}
			}
		}

		(ForwardingConsol consol, ForwardingShipment shipment, OrgHeader consignor, OrgHeader creditor) SetupToAutorateNonConsolLevelChargeOnly()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Debtor Full Name";
			consignor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromMonthEnd;
			consignor.CompanyData.OB_APPaymentTermDays = 90;
			consignor.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromInvoiceDate;
			creditor.CompanyData.OB_APPaymentTermDays = 100;
			creditor.OH_FullName = "Transport Provider One";
			creditor.OH_IsCreditor = true;

			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("CC", "ConsolLevelCharge", FlatCalculator.Code);
			var clientRate = Helper.NewClientRate(consignor);
			var rateEntry = clientRate.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE", "", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(chargeCode.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var rate = Helper.NewCosting(creditor);
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtRateLine.TL_LineOrder = 0;
			frtRateLine.ChargeCode.AC_IsGroupageCharge = true;
			frtRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			frtRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			frtRateLine.TL_RX_NKCurrency = "AUD";

			var bafRateLine = entry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			bafRateLine.TL_LineOrder = 1;
			bafRateLine.ChargeCode.AC_IsGroupageCharge = false;
			bafRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			bafRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;
			bafRateLine.TL_RX_NKCurrency = "AUD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = creditor.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_INCO = "FOB";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_ActualVolume = 30m;
			shipment.JS_ActualChargeable = 100m;
			shipment.JS_RX_NKFreightCostRateCurrency = "AUD";
			shipment.JS_RX_NKFrtRateCurrency = "AUD";

			return (consol, shipment, consignor, creditor);
		}

		#endregion

		#region TestAutorateConsolLevelCostingShowsConsistentCurrency

		public void TestAutorateConsolLevelCostingShowsConsistentCurrency()
		{
			TransportProvider1.OH_IsCreditor = true;
			Factory.Save();

			var dofChargeCode = Helper.ChargeCodes["DOF"];
			dofChargeCode.AC_IsGroupageCharge = true;
			dofChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddYears(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddYears(1);
			exchangeRate.RE_SellRate = 2m;
			exchangeRate.RE_RX_NKExCurrency = "NZD";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			var costing = Helper.NewCosting(null);
			var costEntry1 = costing.AddRateEntry("DST", "SEA", "JPAAM", "AUBNE");
			costEntry1.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costEntry1.TI_OH_TransportProvider = TransportProvider1.PK;

			costEntry1.RateLines.RemoveAndDeleteAll();

			var costLine1 = costEntry1.AddRateLine("DOF", UnitCalculator.Code, QuantityUnit.LW);
			var costLine2 = costEntry1.AddRateLine("DOF", UnitCalculator.Code, QuantityUnit.HB);

			costLine1.GetCalculator<UnitCalculator>().PerUnit = 40;
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 60;

			costLine1.TL_RX_NKCurrency = "NZD";
			costLine2.TL_RX_NKCurrency = "NZD";

			var shipment1 = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "JPAAM", "AUBNE", 100, 5);
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_INCO = "FOB";

			var shipment2 = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "JPAAM", "AUBNE", 100, 5);
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_INCO = "FOB";

			var consol = CreateForwardingConsol(TransportModes.Sea, "JPAAM", "AUBNE", TransportProvider1, shipment1);
			consol.JK_AgentType = "AGT";
			consol.JK_ConsolMode = "LCL";
			consol.JK_PrepaidCollect = "CCX";

			consol.Shipments.Add(shipment2);
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "DOF",
					E6_OSCostAmount = 120m,
					E6_RX_NKCurrency = "NZD",
					E6_LocalCostAmount = 60m
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("Consol cost should show consistent currency.", null, expectedCosts, consol, false);
		}

		#endregion

		#region TestRatingDisbursementFromClientRateWhenCostPosted

		public void TestRatingDisbursementFromClientRateWhenCostPosted()
		{
			var glh = Factory.NewWithValidTestData<AccGLHeader>();

			var chargeCode = Helper.ChargeCodes.New("TSTDSB", "Test Disbursement", FlatCalculator.Code, "FRT");
			chargeCode.AC_ChargeType = ChargeType.Disbursement;
			chargeCode.AC_AG_AccrualAccount = glh.PK;
			chargeCode.AC_AG_WIPAccount = glh.PK;
			chargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code, "", CurrencyCodes.Australia);
			line.GetCalculator<FlatCalculator>().BaseRate = 200;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 200.00m,
							JR_OSCostAmt = 200.00m,
						},
				};

			AutorateAndAssert(expected, shipment, client);

			var job = shipment.Job as Job;
			var charge = job.Charges[0];
			charge.JR_OH_SellAccount = ZGuid.Empty;

			var tr = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", "RAT", 0);

			var th = Factory.NewWithValidTestData<AccTransactionHeader>();
			th.AH_Ledger = LedgerTypes.AccountsPayable;
			th.AH_TransactionType = TransactionTypes.Invoice;
			var tl = Factory.NewWithValidTestData<AccTransactionLines>();
			tl.AL_AH = th.PK;
			tl.AL_LineType = TransactionLineTypes.Cost;
			tl.AL_LineAmount = -200;
			tl.AL_OSAmount = -200;
			tl.AL_RX_NKTransactionCurrency = "AUD";
			tl.AL_RevRecognitionType = "IMM";
			tl.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			charge.JR_AL_APLine = tl.PK;
			charge.JR_AT_SellGSTRate = tr.PK;

			line.GetCalculator<FlatCalculator>().BaseRate = 190;

			Factory.Save();

			Assert("Precondition: the above magic should have posted the cost", charge.IsCostPosted);

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 200.00m,
							JR_OSCostAmt = 200.00m,
							RevenueCalculationDescription = "TSTDSB: Base Rate AUD 200.00",
							CostCalculationDescription = "TSTDSB: Base Rate AUD 200.00"
						},
				};

			AutorateAndAssert("", expected, shipment, client, null, job, autorateRevenue: false);
		}

		#endregion

		#region TestRatingDisbursementChargeDescription

		void CreateTSTDSBCharge()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TSTDSB";
			chargeCode.AC_Desc = "Test Disbursement";
			chargeCode.AC_ChargeGroup = "FRT";
			chargeCode.AC_ChargeType = ChargeType.Disbursement;

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TAX1";
			taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate1.SetRateNumerator_ForTestOnly(5);

			chargeCode.AC_AT_GSTRate = taxRate1.PK;

			var usdBuyRate = Factory.New<RefExchangeRate>();
			usdBuyRate.RE_GC = GlbCompany.CurrentCompany.PK;
			usdBuyRate.RE_RX_NKExCurrency = "USD";
			usdBuyRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
			usdBuyRate.RE_SellRate = 0.8m;
			usdBuyRate.RE_StartDate = ZDate.Today.AddDays(-1);
			usdBuyRate.RE_ExpiryDate = ZDate.Today.AddDays(1);

			var usdSellRate = Factory.New<RefExchangeRate>();
			usdSellRate.RE_GC = GlbCompany.CurrentCompany.PK;
			usdSellRate.RE_RX_NKExCurrency = "USD";
			usdSellRate.RE_ExRateType = ExchangeRateTypes.Code.SellRate;
			usdSellRate.RE_SellRate = 2m;
			usdSellRate.RE_StartDate = ZDate.Today.AddDays(-1);
			usdSellRate.RE_ExpiryDate = ZDate.Today.AddDays(1);
		}

		public void TestRatingConsolDisbursementChargeDescriptionOnRevenue()
		{
			CreateTSTDSBCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var consignor = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			Factory.Save();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine("TSTDSB", UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5;
			costLine.TL_RX_NKCurrency = "AUD";

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("TSTDSB", UnitCalculator.Code, "KG");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 6;
			rateLine.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate())
			{
				job.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						CostCalculationDescription = "TSTDSB: 1500 Kilogram(s) @ AUD 5.00/KG",
						ChargeCode = "TSTDSB",
						E6_OSCostAmount = 7500,
					}
				};

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "TSTDSB",
								JR_OSSellAmt = 7500,
								JR_OSCostAmt = 7500,
								JR_Desc = "Test Disbursement",
								CostCalculationDescription = "TSTDSB: 1500 Kilogram(s) @ AUD 5.00/KG",
								RevenueCalculationDescription = "TSTDSB: 1500 Kilogram(s) @ AUD 5.00/KG"
							}
						}
					}
				};

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //this is to reply 'Yes' for a question regarding that there is only 1 shipment on the consol

				AutoCostAndAssert("Revenue Calculation description and charge Desription should be copied over from Cost", expectedCharges, expectedCosts, consol);
			}
		}

		public void TestRatingShipmentDisbursementRevenuePrevailsOverCost()
		{
			CreateTSTDSBCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var consignor = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			Factory.Save();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine("TSTDSB", UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5;
			costLine.TL_RX_NKCurrency = "AUD";

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("TSTDSB", UnitCalculator.Code, "KG");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 6;
			rateLine.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TSTDSB",
					JR_OSSellAmt = 9000,
					JR_OSCostAmt = 9000,
					CostCalculationDescription = "TSTDSB: 1500 Kilogram(s) @ AUD 6.00/KG",
					RevenueCalculationDescription = "TSTDSB: 1500 Kilogram(s) @ AUD 6.00/KG"
				}
			};

			AutorateAndAssert("Rate should prevail over Cost in this case", expectedCharges, shipment, client);
		}

		#endregion

		#region TestRatingDisbursementFromClientRateWhenCostPosted

		public void TestRatingDisbursementFromClientRateResultsInCorrectSellCurrency()
		{
			var chargeCode = Helper.ChargeCodes.New("TSTDSB", "Test Disbursement", FlatCalculator.Code, "FRT");
			chargeCode.AC_ChargeType = ChargeType.Disbursement;
			chargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var rate = Helper.NewClientRate(NewClient);
			var line = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "TSTDSB", 200, CurrencyCodes.Ukraine);
			AddParityExchangeRate(line.Currency);

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, ZGuid.Empty, "AUSYD", "USLAX", 50m);
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_RX_NKCostCurrency = "UAH",
							JR_RX_NKSellCurrency = "UAH",
							JR_OSSellAmt = 200.00m,
							JR_OSCostAmt = 200.00m,
						},
				};

			AutorateAndAssert(expected, shipment, NewClient);
		}

		#endregion

		#region Autorating with Cost Based Calcullator

		public void TestAutorateRevenueUsingCSTCalculator_CostsApportionedFromConsol_ShouldUpdateApportionedChargesWithSellAmounts()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = costing.AddRateEntry("DST", "FCL", "UA", "AU", "STD", "20GP");
			var costLine1 = costEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 1000;

			var costEntry2 = costing.AddRateEntry("DST", "FCL", "UA", "AU", "STD", "40GP");
			var costLine2 = costEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 2000;

			var clientRate = Helper.NewClientRate(NewClient);

			var clientEntry1 = clientRate.AddRateEntry("DST", "FCL", "UA", "AU", "STD", "20GP");
			var clientLine1 = clientEntry1.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.CN);
			clientLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 100m;

			var clientEntry2 = clientRate.AddRateEntry("DST", "FCL", "UA", "AU", "STD", "40GP");
			var clientLine2 = clientEntry2.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.CN);
			clientLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, NewClient.PK, "UAIEV", "AUSYD", 85m);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			var consol = CreateForwardingConsol(TransportModes.Sea, "UAIEV", "AUSYD", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = Helper.Containers["20GP"].PK;
			container1.AddPackLine(packLine1);

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = Helper.Containers["40GP"].PK;
			container2.AddPackLine(packLine2);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 1000m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 1000.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 2000m,
					CostCalculationDescription = @"FRT: 1 40GP Container(s) @ AUD 2000.00/Container"
				},
			};

			AutoCostAndAssert("Should create consol costs", null, expectedCosts, consol);

			var expectedShipmentCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000m,
					JR_OSSellAmt = 1100m,
					RevenueCalculationDescription = @"FRT: Base Rate AUD 100.00 + 1 20GP Container(s) @ AUD 1000.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 2000m,
					JR_OSSellAmt = 2200m,
					RevenueCalculationDescription = @"FRT: Base Rate AUD 200.00 + 1 40GP Container(s) @ AUD 2000.00/Container"
				},
			};

			AutorateAndAssert("Should populate sell amounts on apportioned charges", expectedShipmentCharges, shipment, NewClient, autorateCosts: false, autorateRevenue: true);
		}

		public void TestCostBasedCalculatorDoesntIncreaseContainerCount()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry1 = costing.AddRateEntry("DST", "FCL", "CNSHA", "AU", "STD", "20GP");
			var costLine1 = costEntry1.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 370;

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry1 = tariff.AddRateEntry("DST", "FCL", "CNSHA", "AUFRE", "STD", "20GP");
			tariffEntry1.RateLines.RemoveAndDeleteAll();
			var tariffLine1 = tariffEntry1.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 45m;

			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var rate = Helper.NewClientRate(NewClient);
			var rateEntry1 = rate.AddRateEntry("DST", "FCL", "", "AU", "STD", "20GP");
			var rateLine1 = rateEntry1.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.CN);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 40m;

			costLine1.TL_RX_NKCurrency = "AUD";
			rateLine1.TL_RX_NKCurrency = "AUD";
			tariffLine1.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			var quickBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, NewClient, null, Consignee, TransportProvider1, "CNSHA", "AUFRE", 450m, 1m, QuotedBookingState.BookingOnly);

			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_OSSellAmt = 410,
							RevenueCalculationDescription =  "DDOC: 1 20GP Container(s) @ AUD 410.00/Container"
						}
				};

			AutorateAndAssert(expected, quickBooking, NewClient);
		}

		public void TestCostBasedCalculatorDoesntIncreaseContainerCount_MultipleRateLines()
		{
			var chargeCode = Helper.ChargeCodes["DDOC"];
			var refContainerPK = Helper.Containers["40OT"].PK;
			var origin = "CNSHA";
			var destination = "AUSYD";

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, origin, destination);
			costEntry.TI_RC = refContainerPK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 16m;

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, origin, destination);
			tariffEntry.TI_RC = refContainerPK;

			var tariffLine1 = tariffEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0m;

			var tariffLine2 = tariffEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0m;

			var tariffLine3 = tariffEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffLine3.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0m;

			Factory.Save();

			var consignee = Helper.NewOrgHeader(1);
			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, consignee.PK, origin, destination, 85m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, TransportProvider1, shipment);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = refContainerPK;
			container1.JC_ContainerNum = "1111";
			container1.AddPackLine(packLine1);

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = refContainerPK;
			container2.JC_ContainerNum = "2222";
			container2.AddPackLine(packLine2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalCostAmt = 32,
					JR_LocalSellAmt = 96,
					CostCalculationDescription = "DDOC: 2 40OT Container(s) @ AUD 16.00/Container",
					RevenueCalculationDescription = @"This charge is calculated from multiple rates
DDOC: 2 40OT Container(s) @ AUD 16.00/Container
DDOC: 2 40OT Container(s) @ AUD 16.00/Container
DDOC: 2 40OT Container(s) @ AUD 16.00/Container"
				}
			};

			var message = "Should not increase the number of containers when mutliple CST calculators are used.";
			AutorateAndAssert(message, expected, shipment, consignee);
		}

		#endregion

		#region CostBasedCalculatorWithDifferentRateLineUnits

		[TestDate(2016, 02, 01)]
		public void TestCostBasedCalculatorWithDifferentRateLineUnits()
		{
			var client = Helper.NewOrgHeader(1);

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AUFRE", "STD", "40GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.CN);

			var costCalculator = costLine.GetCalculator<CombinedCalculator>();
			costCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 100m, 25m, Weight.Tonnes);
			costCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 10m);

			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AUFRE", "STD", "40GP");
			rateEntry.TI_OH_Supplier = costing.Header.PK;
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 15m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUFRE";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.CreditorPK = TransportProvider1.PK;

			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ActualVolume = 10m;
			packline.JL_ActualWeight = 100m;
			packline.JL_JC = container.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_OSSellAmt = 40m,
							JR_OSCostAmt = 25m,
							CostCalculationDescription = "DDOC: 1 40GP Container(s) @ AUD 25.00/Container",
							RevenueCalculationDescription = "DDOC: 1 40GP Container(s) @ AUD 40.00/Container"
						}
				};

			AutorateAndAssert(expected, shipment, client);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines",
@"Information: RateLine Found DDOC-CMB-CN-40GP-Costing TRASPROV1",
@"Information: RateLine Found DDOC-CST-40GP-Client Rate TESTORG1");
		}

		#endregion

		#region FullInheritanceAndMultipleContainerTypesAndAmounts

		public void TestFullInheritanceAndMultipleContainerTypesAndAmounts()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry1 = costing.AddRateEntry("FCL", "SEA", "CNSHA", "AUFRE", "STD", "20GP");
			var costLine1 = costEntry1.RateLines[0];
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var costEntry2 = costing.AddRateEntry("FCL", "SEA", "CNSHA", "AUFRE", "STD", "40GP");
			var costLine2 = costEntry2.RateLines[0];
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry1 = tariff.AddRateEntry("FCL", "SEA", "CNSHA", "AUFRE", "STD", "20GP");
			var tariffLine1 = tariffEntry1.RateLines[0];
			tariffLine1.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 5m;

			var tariffEntry2 = tariff.AddRateEntry("FCL", "SEA", "CNSHA", "AUFRE", "STD", "40GP");
			var tariffLine2 = tariffEntry2.RateLines[0];
			tariffLine2.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			tariffLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			tariffLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;
			tariff.Factory.Save();

			var client = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(client);
			var rateEntry1 = rate.AddRateEntry("FCL", "SEA", "CNSHA", "AUFRE", "STD", "20GP");
			var rateLine1 = rateEntry1.RateLines[0];
			rateLine1.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 6m;
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 6m;

			var rateEntry2 = rate.AddRateEntry("FCL", "SEA", "CNSHA", "AUFRE", "STD", "40GP");
			var rateLine2 = rateEntry2.RateLines[0];
			rateLine2.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			rateLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 11m;
			rateLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 11m;

			costLine1.TL_RX_NKCurrency = "AUD";
			costLine2.TL_RX_NKCurrency = "AUD";
			tariffLine1.TL_RX_NKCurrency = "AUD";
			tariffLine2.TL_RX_NKCurrency = "AUD";
			rateLine1.TL_RX_NKCurrency = "AUD";
			rateLine2.TL_RX_NKCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUFRE";
			shipment.JS_INCO = "FOB";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerMode = ContainerModes.FCL;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 2;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container2.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 1000m,
						JR_OSSellAmt = 1113m,
						RevenueCalculationDescription = "FRT: 1 20GP Container(s) @ AUD 1113.00/Container",
						CostCalculationDescription = "FRT: 1 20GP Container(s) @ AUD 1000.00/Container"
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 4000m,
						JR_OSSellAmt = 4884m,
						RevenueCalculationDescription = "FRT: 2 40GP Container(s) @ AUD 2442.00/Container",
						CostCalculationDescription = "FRT: 2 40GP Container(s) @ AUD 2000.00/Container"
					}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region TestMinimalOfMaxAmountsIsApplied

		public void TestMinimalOfMaxAmountsIsApplied()
		{
			var client = Helper.NewOrgHeader(1);

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<CombinedCalculator>().Maximum = 300m;
			rateLine1.GetCalculator<CombinedCalculator>().PerUnit = 5m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "KRSEL";
			shipment.JS_INCO = "CIF";
			shipment.JS_ActualWeight = 230m;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "KRSEL";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 1150m,
							JR_OSSellAmt = 300m,
							RevenueCalculationDescription = "FRT: Maximum AUD 300.00",
							CostCalculationDescription = "FRT: 230 Kilogram(s) @ AUD 5.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region Value Range Calculator's Apply To

		public void TestValueRangeCalculator()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine("FRT", ValueRangeCalculator.Code);
			costLine.Calculator["-100"] = new ZDecimal(10m);
			costLine.Calculator["+100"] = new ZDecimal(12m);

			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", ValueRangeCalculator.Code);
			rateLine.Calculator["-100"] = new ZDecimal(20m);
			rateLine.Calculator["+100"] = new ZDecimal(22m);

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "CNSHA", 10m);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			shipment.JS_GoodsValue = 500m;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 12m,
							JR_OSSellAmt = 22m,
							CostCalculationDescription = "FRT: AUD 500.00 (Value of Goods) @ Base Rate AUD 12.00 (AUD 100.00 or more)",
							RevenueCalculationDescription = "FRT: AUD 500.00 (Value of Goods) @ Base Rate AUD 22.00 (AUD 100.00 or more)"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestValueRangeCalculator_ThrowsExceptionWhenApplyToIsBlank()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine("FRT", ValueRangeCalculator.Code);
			costLine.Calculator["-100"] = new ZDecimal(10m);
			costLine.Calculator["+100"] = new ZDecimal(12m);

			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", ValueRangeCalculator.Code);
			rateLine.Calculator["-100"] = new ZDecimal(20m);
			rateLine.Calculator["+100"] = new ZDecimal(22m);

			var costApplyTo = costLine.FindRateLineItem(CalculatorConstants.Type.ApplyTo) as RateLineItem;
			var rateApplyTo = rateLine.FindRateLineItem(CalculatorConstants.Type.ApplyTo) as RateLineItem;
			costApplyTo.TM_Text = "";
			rateApplyTo.TM_Text = "";

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "CNSHA", 10m);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			shipment.JS_GoodsValue = 500m;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			var expectedErrors = new[]
			{
				@"Error Autorating has encountered an error:
Cannot complete Auto-Rating as this Job has been matched to an invalid Rate Line. Please either correct the 'Apply to' field or delete the 'FRT' Rate Line that uses a 'IXC' Calculator with currently an Apply To of '' on Client Rate TESTORG1.",
				@"Error Autorating has encountered an error:
Cannot complete Auto-Rating as this Job has been matched to an invalid Rate Line. Please either correct the 'Apply to' field or delete the 'FRT' Rate Line that uses a 'IXC' Calculator with currently an Apply To of '' on Costing TRASPROV1."
			};

			AutorateAndAssert(expected, shipment, client, expectedErrors: expectedErrors);
		}

		#endregion

		#region CostCTClientRateInheritance

		public void TestCostCTClientRateInheritance()
		{
			var client = Helper.NewOrgHeader(1);

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffLine1 = tariffEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.KG);
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 50m;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 50m;
			tariff.Factory.Save();

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 23000;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "KRSEL";
			shipment.JS_INCO = "CIF";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "KRSEL";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 115000,
							JR_OSSellAmt = 189750,
							RevenueCalculationDescription = "FRT: 23000 Kilogram(s) @ AUD 8.25/KG",	//110% of 150% = 165%
							CostCalculationDescription = "FRT: 23000 Kilogram(s) @ AUD 5.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region With Different Weight/Volume for Client/Provider

		public void TestWithDifferentWeightVolumeForClientProvider()
		{
			var creditor = Helper.NewOrgHeader();
			var cost = Helper.NewCosting(creditor);
			var frtCost = cost.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			frtCost.TL_RateCalculator = UnitCalculator.Code;
			frtCost.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4m;

			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var frtRate = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			frtRate.TL_RateCalculator = UnitCalculator.Code;
			frtRate.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_DocumentedWeight = 110;
			shipment.JS_ManifestedWeight = 90;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .5;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = creditor.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 360m,
							JR_OSSellAmt = 550m,
							RevenueCalculationDescription = "FRT: 110 Kilogram(s) @ AUD 5.00/KG",
							CostCalculationDescription = "FRT: 90 Kilogram(s) @ AUD 4.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, client);

			shipment.JS_ActualVolume = 1;
			shipment.JS_DocumentedVolume = 1.2;
			shipment.JS_ManifestedVolume = .9;

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 600m,
							JR_OSSellAmt = 1000m,
							RevenueCalculationDescription = "FRT: 200 Kilogram(s) @ AUD 5.00/KG",
							CostCalculationDescription = "FRT: 150 Kilogram(s) @ AUD 4.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region TestInclusiveCharges

		[TestDate(2017, 07, 24)]
		public void TestInclusiveCharges()
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var odocChargeCode = Helper.ChargeCodes["ODOC"];

			var frtInc20GPChargeCode = Helper.ChargeCodes.New("FINC20GP", "20GP Freight Inclusive", Code, ChargeCodeGroupList.Codes.Freight);
			var frtInc40GPChargeCode = Helper.ChargeCodes.New("FINC40GP", "40GP Freight Inclusive", Code, ChargeCodeGroupList.Codes.Freight);

			var orgIncChargeCode = Helper.ChargeCodes.New("OINC", "Origin Inclusive", Code, ChargeCodeGroupList.Codes.Origin);
			var orgInc20GPChargeCode = Helper.ChargeCodes.New("OINC20GP", "20GP Origin Inclusive", Code, ChargeCodeGroupList.Codes.Origin);
			var orgInc40GPChargeCode = Helper.ChargeCodes.New("OINC40GP", "40GP Origin Inclusive", Code, ChargeCodeGroupList.Codes.Origin);

			var cont20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var cont40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry20GP = costing.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", cont20GP.RC_Code);
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine(frtChargeCode.AC_Code, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 20;
			var inc20GPRateLine = costEntry20GP.AddRateLine(frtInc20GPChargeCode.AC_Code, Code, currencyCode: CurrencyCodes.Australia);
			inc20GPRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;

			var costEntry40GP = costing.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", cont40GP.RC_Code);
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine(frtChargeCode.AC_Code, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 40;
			var inc40GPRateLine = costEntry40GP.AddRateLine(frtInc40GPChargeCode.AC_Code, Code, currencyCode: CurrencyCodes.Australia);
			inc40GPRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.SubjectTo;

			var orgRateEntry = costing.AddRateEntry("ORG", TransportModes.Sea, "AUSYD", "");
			orgRateEntry.RateLines.RemoveAndDeleteAll();
			var odocRateLine = orgRateEntry.AddRateLine(odocChargeCode.AC_Code, FlatCalculator.Code, null, CurrencyCodes.Australia);
			odocRateLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			var oincRateLine = orgRateEntry.AddRateLine(orgIncChargeCode, Code, null, CurrencyCodes.Australia);
			oincRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;

			var orgRateEntry20GP = costing.AddRateEntry("ORG", ContainerModes.FCL, "AUSYD", "USLAX", "", cont20GP.RC_Code);
			orgRateEntry20GP.RateLines.RemoveAndDeleteAll();
			var orgInc20GPRateLine = orgRateEntry20GP.AddRateLine(orgInc20GPChargeCode.AC_Code, Code, null, CurrencyCodes.Australia);
			orgInc20GPRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;

			var orgRateEntry40GP = costing.AddRateEntry("ORG", ContainerModes.FCL, "AUSYD", "USLAX", "", cont40GP.RC_Code);
			orgRateEntry40GP.RateLines.RemoveAndDeleteAll();
			var orgInc40GPRateLine = orgRateEntry40GP.AddRateLine(orgInc40GPChargeCode.AC_Code, Code, null, CurrencyCodes.Australia);
			orgInc40GPRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.NotApplicable;

			var client = NewClient;
			client.OH_IsDebtor = true;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "KIKI4100011";
			container1.JC_RC = cont20GP.PK;
			container1.JC_ContainerCount = 1;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.PackLines.Add(packline1);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "KIKI4100022";
			container2.JC_RC = cont40GP.PK;
			container2.JC_ContainerCount = 1;
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.PackLines.Add(packline2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 20,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 20.00/Container

International Freight

Inclusive charges:
FINC20GP - 20GP Freight Inclusive (Included)
OINC - Origin Inclusive (Included)
OINC20GP - 20GP Origin Inclusive (Included)"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 40,
					CostCalculationDescription = @"FRT: 1 40GP Container(s) @ AUD 40.00/Container

International Freight

Inclusive charges:
FINC40GP - 40GP Freight Inclusive (Subject To)
OINC - Origin Inclusive (Included)
OINC40GP - 40GP Origin Inclusive (Not Applicable)"
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 15,
					CostCalculationDescription = "ODOC: Base Rate AUD 15.00"
				},
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutorateAndAssert(expected, shipment, client, autorateRevenue: false);
			}

			var expectedLogLines = new string[]
			{
				@"User:				CargoWise Support
Time:				24-Jul-17 00:00

Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription
Information: Resetting previously auto-rated charges for Shipment EBM22Q33TU475BXH3P60
Information: AUTORATING COSTS FOR Shipment EBM22Q33TU475BXH3P60
Information: RatingHeader Found Costing TRASPROV1 Entries: 5",
@"Information: RateLine Found ODOC-FLT-Costing TRASPROV1
Information: RateLine Found OINC-FRT-Costing TRASPROV1
Information: RateLine Found FINC20GP-FRT-20GP-Costing TRASPROV1
Information: RateLine Found FRT-UNT-CN-20GP-Costing TRASPROV1
Information: RateLine Found OINC20GP-FRT-20GP-Costing TRASPROV1
Information: RateLine Found FINC40GP-FRT-40GP-Costing TRASPROV1
Information: RateLine Found FRT-UNT-CN-40GP-Costing TRASPROV1
Information: RateLine Found OINC40GP-FRT-40GP-Costing TRASPROV1
Information: Chargeable was added for
				RateLine FRT-UNT-CN-40GP-Costing TRASPROV1
					Job's info:
					Container 40GP: 1 ContainerCount
	RateLine FRT-UNT-CN-40GP-Costing TRASPROV1 will not rate Container 20GP by ContainerCount	reason: expected '20GP' ContainerType while rate is for '40GP' ContainerType
				RateLine FRT-UNT-CN-20GP-Costing TRASPROV1
					Job's info:
					Container 20GP: 1 ContainerCount
	RateLine FRT-UNT-CN-20GP-Costing TRASPROV1 will not rate Container 40GP by ContainerCount	reason: expected '40GP' ContainerType while rate is for '20GP' ContainerType
Information: CHARGES CALCULATED:
	FRT: 1 20GP Container(s) @ AUD 20.00/Container
	FRT: 1 40GP Container(s) @ AUD 40.00/Container
	ODOC: Base Rate AUD 15.00
Information: Shipment EBM22Q33TU475BXH3P60 was auto-costed.
	The following costs were found:
	  • FRT charge from Costing TRASPROV1 (x2)
	  • ODOC charge from Costing TRASPROV1
	Charges created: FRT (x2), ODOC"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected line", expectedLogLines);
		}

		public void TestFreightInclusiveChargesAreNotAddedIfNotFilteredOut()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1000m;
			var baf20GPRateLine = costEntry20GP.AddRateLine("BAF", Code);
			baf20GPRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 2000m;
			var baf40GPRateLine = costEntry40GP.AddRateLine("BAF", Code);
			baf40GPRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container20GP.JC_ContainerNum = "CONT00001";
			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container40GP.JC_ContainerNum = "CONT00002";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 5.2m;
			packline1_1.JL_ActualWeight = 15000m;
			packline1_1.JL_JC = container20GP.PK;

			var packline1_2 = shipment1.OuterPackLines.AddNew();
			packline1_2.JL_ActualVolume = 3.7m;
			packline1_2.JL_ActualWeight = 25000m;
			packline1_2.JL_JC = container40GP.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1000m,
						CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 1000.00/Container

International Freight

Inclusive charges:
BAF - Bunker Adjustment Factor (Included)"
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 2000m,
						CostCalculationDescription = @"FRT: 1 40GP Container(s) @ AUD 2000.00/Container

International Freight

Inclusive charges:
BAF - Bunker Adjustment Factor (Included)"
					},
				};

				AutoCostAndAssert("BAF inclusive costs should not be created", null, expectedCosts, consol);
			}
		}

		public void TestAutoratingShipmentCosts_ConsolAndNonConsolLevelChargesWithFreightInclusiveCalculatorFound_ShouldBothBeAddedToFreight()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var cost = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			cost.TI_RX_NKCurrency = "AUD";
			cost.RateLines.RemoveAndDeleteAll();

			var frtLine = cost.AddRateLine("FRT", UnitCalculator.Code, "CN");
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 1000m;
			frtLine.ChargeCode.AC_IsGroupageCharge = true;

			var includedNonConsolLevelCharge = cost.AddRateLine("BAF", Code);
			includedNonConsolLevelCharge.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;
			includedNonConsolLevelCharge.ChargeCode.AC_IsGroupageCharge = false;
			includedNonConsolLevelCharge.TL_RX_NKCurrency = ZString.Empty;

			var includedConsolLevelCharge = cost.AddRateLine("CAF", Code);
			includedConsolLevelCharge.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;
			includedConsolLevelCharge.ChargeCode.AC_IsGroupageCharge = true;
			includedConsolLevelCharge.TL_RX_NKCurrency = ZString.Empty;

			var nonConsolLevelCharge = cost.AddRateLine("WAR", UnitCalculator.Code, "CN");
			nonConsolLevelCharge.GetCalculator<UnitCalculator>().PerUnit = 500m;
			nonConsolLevelCharge.ChargeCode.AC_IsGroupageCharge = false;
			nonConsolLevelCharge.TL_RX_NKCurrency = "AUD";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container20GP.JC_ContainerNum = "CONT00001";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";
			shipment.JS_OH_DeliveryAgent = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container20GP.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					// CAF is consol level charge so it is expected to be added to a consol level FRT charge
					//
					// BAF is non consol level charge and normaly should have been discarded when autorating consol costs for shipment,
					// but since it is Freight Inclusive charge, we don't care. If FRT charge is calculated, then all included charges
					// must be included regardless of consol level charges we are autorating or non-consol level charges.
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 1000m,
						CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 1000.00/Container

International Freight

Inclusive charges:
BAF - Bunker Adjustment Factor (Included)
CAF - Currency Adjustment Factor (Included)"
					},
					new AssertionCharge
					{
						ChargeCode = "WAR",
						JR_OSCostAmt = 500m,
						CostCalculationDescription = @"WAR: 1 20GP Container(s) @ AUD 500.00/Container"
					},
				};

				AutorateAndAssert(expectedCosts, shipment, creditor);
			}
		}

		#endregion

		#region NegativeResultWithDefaultMinimum

		public void TestNegativeResultWithDefaultMinimum()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var client = Helper.NewOrgHeader(1);
			client.OH_IsDebtor = true;

			TransportProvider1.CompanyData.OB_APCostsSelfBilled = true;

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			costEntry.RateLines.RemoveAndDeleteAll();

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];

			var frtLine = costEntry.AddRateLine(frtChargeCode.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 1.6;

			var bafLine = costEntry.AddRateLine(bafChargeCode.AC_Code, PercentageCalculator.Code);
			bafLine.GetCalculator<PercentageCalculator>().Percent = -5;
			bafLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, creditor.PK, "AUFRE", "USLAX", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUFRE";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSCostAmt = 1600.0m,
					CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 1.60/KG",
				},
				new AssertionCharge
				{
					ChargeCode = bafChargeCode.AC_Code,
					JR_OSCostAmt = -80.0m,
					CostCalculationDescription = "BAF: -5.00% of (AUD 1600.00 (Freight Charges FRT))",
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region CostsAreNotRemovedByIncoTermRemover

		public void TestCostsAreNotRemovedByIncoTermRemover()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var localClient = Helper.NewOrgHeader();
			var creditor = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			rateEntry.TI_OH_Supplier = creditor.PK;
			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			rateLine.Calculator[CalculatorConstants.Type.PER] = (ZDecimal)25m;
			rateLine.Calculator[CalculatorConstants.Type.PRU] = (ZDecimal)25m;
			rateLine.TL_RX_NKCurrency = "AUD";

			var costing = Helper.NewCosting(creditor);
			var costLine = costing.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD").RateLines[0];
			costLine.TL_RateCalculator = UnitCalculator.Code;
			costLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4m;
			costLine.TL_RX_NKCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .5;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = "FOB";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = creditor.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 400,
							JR_OSSellAmt = 500,
						},
				};

			AutorateAndAssert(expected, shipment, localClient, creditor);
		}

		#endregion

		#region CalculateCostsMultipleProviders

		public void TestCalculateCostsMultipleProviders()
		{
			var serviceProvider1 = Helper.CreateCreditor("SUPPLIER1");
			var serviceProvider2 = Helper.CreateCreditor("SUPPLIER2");

			var costing1 = Helper.NewCosting(serviceProvider1);
			var costEntry = costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "ODOC", 15m);
			var cartLine = costEntry.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.KG);
			cartLine.ConversionFactor = new ConversionFactor(250m, Weight.Kilograms, Volume.CubicMetres);
			cartLine.GetCalculator<UnitCalculator>().PerUnit = 1.5m;

			var costing2 = Helper.NewCosting(serviceProvider2);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "ODOC", 25m);

			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "ODOC", 35m);

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "USLAX", 100m, 0.5m);
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = serviceProvider1.MainAddress.PK;
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = serviceProvider2.PK;

			Factory.Save();

			var expected = new[]
				{
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 15,
							JR_OSSellAmt = 35,
						},
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 25,
							JR_OSSellAmt = 0,
						},
						new AssertionCharge
						{
							ChargeCode = "OCART",
							JR_OSCostAmt = 187.5,
							JR_OSSellAmt = 187.5,
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region PercentageCalculatorLoopedCalculationCostBased

		public void TestPercentageCalculatorLoopedCalculationCostBased()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();

			var frtCostLine = costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtCostLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var cafCostLine = costEntry.AddRateLine("CAF", PercentageCalculator.Code);
			cafCostLine.GetCalculator<PercentageCalculator>().Percent = 10m;
			cafCostLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);

			var bafCostLine = costEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			bafCostLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var client = NewClient;
			var testRate = Helper.NewClientRate(client);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var frtLine = entry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			frtLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			frtLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var line2 = entry.AddRateLine("CAF", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 20m;

			var line3 = entry.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line3.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			line3.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 500,
							JR_OSSellAmt = 550,
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSCostAmt = 100,
							JR_OSSellAmt = 110,
						},
						new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSCostAmt = 60,
							JR_OSSellAmt = 72,
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestPercentageCalculatorLoopedCalculationCostBased2()
		{
			var awbChargeCode = Helper.ChargeCodes.New("TESTAWB", "Test Airway Bill Fee", PercentageCalculator.Code, "ORG");
			awbChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var testCost = Helper.NewCosting(TransportProvider1);
			var costEntry = testCost.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine1 = costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var costLine2 = costEntry.AddRateLine("CAF", PercentageCalculator.Code);
			costLine2.GetCalculator<PercentageCalculator>().Percent = 10m;
			costLine2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);

			var costLine3 = costEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			costLine3.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var costOrgEntry = testCost.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");

			var costORGLine = costOrgEntry.AddRateLine(awbChargeCode, PercentageCalculator.Code);
			costORGLine.GetCalculator<PercentageCalculator>().Percent = 10m;
			costORGLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);

			var client = NewClient;
			var testRate = Helper.NewClientRate(client);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			line1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var line2 = entry.AddRateLine("CAF", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 20m;

			var line3 = entry.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line3.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			line3.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var line4 = entry.AddRateLine("WAR", PercentageCalculator.Code);
			line4.GetCalculator<PercentageCalculator>().Percent = 4m;
			line4.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.OriginCharges);

			var orgEntry = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");

			var orgLine = orgEntry.AddRateLine(awbChargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			orgLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 25m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 500,
							JR_OSSellAmt = 550,
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSCostAmt = 100,
							JR_OSSellAmt = 110,
						},
						new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSCostAmt = 60,
							JR_OSSellAmt = 72,
						},
						new AssertionCharge
						{
							ChargeCode = "TESTAWB",
							JR_OSCostAmt = 66,
							JR_OSSellAmt = 82.5,
						},
						new AssertionCharge
						{
							ChargeCode = "WAR",
							JR_OSSellAmt = 3.3,
							RevenueCalculationDescription = "WAR: 4.00% of (AUD 82.50 (Origin Charges TESTAWB))"
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region CalculateCosts

		public void TestCalculateCosts()
		{
			var awbCharge = Helper.ChargeCodes.New("TESTAWB", "Test Airway Bill Fee", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var thcCharge = Helper.ChargeCodes.New("TESTTHC", "Test Terminal Handling Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			thcCharge.AC_MarginPercentage = 0m;

			var bbkCharge = Helper.ChargeCodes.New("TESTBBK", "Test Breakbulk", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bbkCharge.AC_MarginPercentage = 0m;

			var client = NewClient;
			var clientRate = Helper.NewClientRate(client);

			var airRateEntry1 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			airRateEntry1.TI_OH_Supplier = TransportProvider1.PK;
			airRateEntry1.RateLines.RemoveAndDeleteAll();
			var airRateLine1A = airRateEntry1.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			airRateLine1A.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			airRateLine1A.Calculator["-45"] = (ZDecimal)4.5m;
			airRateLine1A.Calculator["+45"] = (ZDecimal)4.0m;
			airRateLine1A.Calculator["+100"] = (ZDecimal)3.5m;
			airRateLine1A.Calculator["+250"] = (ZDecimal)3.0m;
			airRateLine1A.Calculator["+500"] = (ZDecimal)2.5m;
			airRateLine1A.Calculator["+1000"] = (ZDecimal)2.0m;

			var airRateLine1B = airRateEntry1.AddRateLine("BAF", PercentageCalculator.Code);
			airRateLine1B.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = airRateLine1A.TL_AC;
			airRateLine1B.GetCalculator<PercentageCalculator>().Percent = 13.62m;

			var airRateLine1C = airRateEntry1.AddRateLine("CAF", PercentageCalculator.Code);
			airRateLine1C.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = airRateLine1A.TL_AC;
			airRateLine1C.GetCalculator<PercentageCalculator>().Percent = 8.75m;

			var orgRateEntry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var orgRateLine1A = orgRateEntry1.AddRateLine(awbCharge, FlatCalculator.Code);
			orgRateLine1A.GetCalculator<FlatCalculator>().BaseRate = 50m;

			var orgRateLine1B = orgRateEntry1.AddRateLine(bbkCharge, CombinedCalculator.Code, QuantityUnit.KG);
			orgRateLine1B.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)10m;
			orgRateLine1B.Calculator["-45"] = (ZDecimal)0.50m;
			orgRateLine1B.Calculator["+45"] = (ZDecimal)0.45m;
			orgRateLine1B.Calculator["+100"] = (ZDecimal)0.40m;
			orgRateLine1B.Calculator["+250"] = (ZDecimal)0.35m;

			var orgRateLine1C = orgRateEntry1.AddRateLine(thcCharge, FlatCalculator.Code);
			orgRateLine1C.GetCalculator<FlatCalculator>().BaseRate = 18.50m;

			var costing1 = Helper.NewCosting(TransportProvider1);

			var cAirRateEntry1 = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			cAirRateEntry1.RateLines.RemoveAndDeleteAll();
			var cAirRateLine1A = cAirRateEntry1.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			cAirRateLine1A.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)25m;
			cAirRateLine1A.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.9m;

			var cAirRateLine1B = cAirRateEntry1.AddRateLine("BAF", PercentageCalculator.Code);
			cAirRateLine1B.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = airRateLine1A.TL_AC;
			cAirRateLine1B.GetCalculator<PercentageCalculator>().Percent = 10m;

			var costing2 = Helper.NewCosting(TransportProvider2);

			var cORGRateEntry1 = costing2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var cORGRateLine1A = cORGRateEntry1.AddRateLine(awbCharge, FlatCalculator.Code);
			cORGRateLine1A.GetCalculator<FlatCalculator>().BaseRate = 45m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 55;
			shipment.JS_ActualVolume = .5;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF5461";
			transport.JW_OA_CarrierAddress = TransportProvider1.MainAddress.PK;
			transport.JW_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			consol.CreditorPK = TransportProvider2.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 333.33,
							JR_RX_NKSellCurrency = "AUD",
							JR_OSCostAmt = 158.33,
							JR_RX_NKCostCurrency = "AUD",
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 45.4,
							JR_RX_NKSellCurrency = "AUD",
							JR_OSCostAmt = 15.83,
							JR_RX_NKCostCurrency = "AUD",
						},
						new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 29.17,
							JR_RX_NKSellCurrency = "AUD",
							JR_OSCostAmt = 29.17,
							JR_RX_NKCostCurrency = "AUD",
						},
						new AssertionCharge
						{
							ChargeCode = "TESTAWB",
							JR_OSSellAmt = 50,
							JR_RX_NKSellCurrency = "AUD",
							JR_OSCostAmt = 45,
							JR_RX_NKCostCurrency = "AUD",
						},
						new AssertionCharge
						{
							ChargeCode = "TESTBBK",
							JR_OSSellAmt = 37.5,
							JR_RX_NKSellCurrency = "AUD",
							JR_OSCostAmt = 0,
							JR_RX_NKCostCurrency = "AUD",
						},
						new AssertionCharge
						{
							ChargeCode = "TESTTHC",
							JR_OSSellAmt = 18.5,
							JR_RX_NKSellCurrency = "AUD",
							JR_OSCostAmt = 0,
							JR_RX_NKCostCurrency = "AUD",
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region CostsUseCostServiceLevel

		public void TestCostsUseCostServiceLevel()
		{
			var clientServiceLevelCode = "ABC";
			var carrierServiceLevelCode = "XYZ";

			var carrier = TransportProvider1;
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = carrierServiceLevelCode;
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "Extra Yellow Zebras";

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.TI_PL_NKCarrierServiceLevel = clientServiceLevelCode;
			costEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			costEntry.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 1000m;

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.TI_RS_NKServiceLevel_NI = clientServiceLevelCode;
			rateEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			rateEntry.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 2000m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "AUSYD", "USLAX", 55m);
			shipment.JS_RS_NKServiceLevel = clientServiceLevelCode;

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", carrier, shipment);
			consol.CreditorPK = carrier.PK;
			consol.JK_AWBServiceLevel = carrierServiceLevelCode;

			Factory.Save();

			var serviceLevel = shipment.RatingAdapter.ServiceLevel;
			AssertEquals("Pre-condition", clientServiceLevelCode, serviceLevel.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("Pre-condition", carrierServiceLevelCode, serviceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 2000,
							CostCalculationDescription = "",
							RevenueCalculationDescription = "FRT: Base Rate AUD 2000.00",
						},
				};

			AutorateAndAssert(expected, shipment, client);

			costEntry.TI_PL_NKCarrierServiceLevel = carrierServiceLevelCode;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 2000,
							JR_OSCostAmt = 1000,
							CostCalculationDescription = "FRT: Base Rate AUD 1000.00",
							RevenueCalculationDescription = "FRT: Base Rate AUD 2000.00",
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		[TestDate(2015, 12, 7)]
		public void TestCalculationDescriptionShowsClientAndCarrierServiceLevels()
		{
			var clientServiceLevelCode = "ABC";
			var carrierServiceLevelCode = "XYZ";

			var carrier = TransportProvider1;
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = carrierServiceLevelCode;
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "Extra Yellow Zebras";
			Factory.Save();

			var cost = Helper.NewCosting(carrier);
			var costing = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costing.TI_RS_NKServiceLevel_NI = clientServiceLevelCode;
			costing.TI_PL_NKCarrierServiceLevel = carrierServiceLevelCode;
			costing.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			costing.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 1000m;

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.TI_RS_NKServiceLevel_NI = clientServiceLevelCode;
			rateEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			rateEntry.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 2000m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "AUSYD", "USLAX", 55m);
			shipment.JS_RS_NKServiceLevel = clientServiceLevelCode;

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", carrier, shipment);
			consol.CreditorPK = carrier.PK;
			consol.JK_AWBServiceLevel = carrierServiceLevelCode;

			Factory.Save();

			var serviceLevel = shipment.RatingAdapter.ServiceLevel;
			AssertEquals("Pre-condition", clientServiceLevelCode, serviceLevel.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("Pre-condition", carrierServiceLevelCode, serviceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 2000,
							JR_OSCostAmt = 1000,
							RevenueCalculationDescription = "FRT: Base Rate AUD 2000.00",
							CostCalculationDescription = $"FRT: Base Rate AUD 1000.00\r\n{DescriptionHelpers.FormatWithTab("Service Level:")}ABC\r\n{DescriptionHelpers.FormatWithTab("Carrier Service Level:")}XYZ"
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		[TestDate(2015, 12, 7)]
		public void TestCalculationDescriptionShowsClientAndCarrierDoorToDoorServiceLevels()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "DD1";
			serviceLevel.RS_IsDoorToDoor = true;

			var carrier = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(carrier);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "");
			costEntry.TI_RS_NKServiceLevel_NI = "DD1";
			costEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)costEntry.RateLines[0].Calculator).BaseRate = 100m;

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			rateEntry.TI_RS_NKServiceLevel_NI = "DD1";
			rateEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)rateEntry.RateLines[0].Calculator).BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "USLAX", 50m);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			shipment.JS_UniqueConsignRef = "S00125923";
			shipment.JS_RS_NKServiceLevel = "DD1";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var serviceLevelDescription = DescriptionHelpers.FormatWithTab("Service Level:");

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 100m,
							JR_OSSellAmt = 200m,
							CostCalculationDescription = $"{serviceLevelDescription}DD1 (Door-to-Door)\r\n",
							RevenueCalculationDescription = $"{serviceLevelDescription}DD1 (Door-to-Door)\r\n"
						},
				};

			AutorateAndAssert(expected, shipment, client);

			rateEntry.TI_RS_NKServiceLevel_NI = "STD";
			costEntry.TI_RS_NKServiceLevel_NI = "STD";
			Factory.Save();

			shipment.JS_RS_NKServiceLevel = "STD";

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 100m,
							JR_OSSellAmt = 200m,
							CostCalculationDescription = $"{serviceLevelDescription}STD\r\n",
							RevenueCalculationDescription = $"{serviceLevelDescription}STD\r\n"
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region CTBCalculatorFiltersBySimilarity

		public void TestCTBCalculator_CostHasDifferentContainerOfTheSameClass_CTBShouldBeApplied()
		{
			Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").RC_HandlingRateClass = "20GN";
			Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").RC_HandlingRateClass = "20GN";

			var costing = Helper.NewCosting(TransportProvider1);
			var costLine = costing.AddRateEntry("ORG", "FCL", "AUSYD", "", "STD", "20RE").AddRateLine("ODOC", FlatCalculator.Code);
			costLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			var tariff = Helper.NewCompanyTariff();
			var tariffline = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "", "STD", "20GP").AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffline.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;
			tariffline.Parent.TI_MatchContainerRateClass = true;

			var client = Helper.NewOrgHeader(1);
			tariff.Factory.Save();
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 55;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .5;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_OuterPacks = 1;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;
			consol.JK_PrepaidCollect = "PPD";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container1.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						CostCalculationDescription = "ODOC: Base Rate AUD 100.00",
						RevenueCalculationDescription = "ODOC: 105.00% of (Base Rate AUD 100.00)"
					},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region FCL Freight Tests

		public void TestAutoRateFCL20GPand40GPBothBasedOnCosts_WhereSellRateIsNotContainerSpecific()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var cost = Helper.NewCosting(creditor);

			var cost20GP = cost.AddRateEntry("DST", "FCL", "USLAX", "AUSYD", "", "20GP");
			var costLine20GP = cost20GP.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN);
			costLine20GP.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var cost40GP = cost.AddRateEntry("DST", "FCL", "USLAX", "AUSYD", "", "40GP");
			var costLine40GP = cost40GP.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN);
			costLine40GP.GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var rate = Helper.NewClientRate(NewClient);

			var rateEntry = rate.AddRateEntry("DST", "FCL", "USLAX", "AUSYD");
			rateEntry.TI_OH_Supplier = creditor.PK;
			var rateLine = rateEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 500m;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "USLAX", "AUSYD", 55m, 0.5m);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = CreateForwardingConsol(TransportModes.Sea, "USLAX", "AUSYD", TransportProvider1, shipment);
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = creditor.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerCount = 3;
			container1.JC_ContainerMode = ContainerModes.FCL;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP40.PK;
			container2.JC_ContainerCount = 2;
			container2.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSCostAmt = 4000m,
					JR_OSSellAmt = 5000m,
					RevenueCalculationDescription = "DDOC: 2 40GP Container(s) @ AUD 2500.00/Container",
					CostCalculationDescription = "DDOC: 2 40GP Container(s) @ AUD 2000.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSCostAmt = 3000m,
					JR_OSSellAmt = 4500m,
					RevenueCalculationDescription = "DDOC: 3 20GP Container(s) @ AUD 1500.00/Container",
					CostCalculationDescription = "DDOC: 3 20GP Container(s) @ AUD 1000.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, NewClient);
		}

		public void TestAutoRateFCL20GPAnd40GPBothBasedOnCosts()
		{
			var creditor = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(creditor);

			var cost20GP = cost.AddRateEntry("DST", "FCL", "USLAX", "AUSYD", "", "20GP");
			var costLine20GP = cost20GP.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN);
			costLine20GP.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var cost40GP = cost.AddRateEntry("DST", "FCL", "USLAX", "AUSYD", "", "40GP");
			var costLine40GP = cost40GP.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN);
			costLine40GP.GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var rate = Helper.NewClientRate(client);

			var rate20GP = rate.AddRateEntry("DST", "FCL", "USLAX", "AUSYD", "", "20GP");
			rate20GP.TI_OH_Supplier = creditor.PK;
			var rateLine20GP = rate20GP.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			rateLine20GP.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 500m;

			var rate40GP = rate.AddRateEntry("DST", "FCL", "USLAX", "AUSYD", "", "40GP");
			rate40GP.TI_OH_Supplier = creditor.PK;
			var rateLine40GP = rate40GP.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			rateLine40GP.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 300m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 55;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .5;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = "CIF";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = creditor.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerCount = 3;
			container1.JC_ContainerMode = ContainerModes.FCL;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container2.JC_ContainerCount = 2;
			container2.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSCostAmt = 4000m,
					JR_OSSellAmt = 4600m,
					RevenueCalculationDescription = "DDOC: 2 40GP Container(s) @ AUD 2300.00/Container",
					CostCalculationDescription = "DDOC: 2 40GP Container(s) @ AUD 2000.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSCostAmt = 3000m,
					JR_OSSellAmt = 4500m,
					RevenueCalculationDescription = "DDOC: 3 20GP Container(s) @ AUD 1500.00/Container",
					CostCalculationDescription = "DDOC: 3 20GP Container(s) @ AUD 1000.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region Set Standard Rate Cost

		public void TestSetStandardRateCostAsConsolChargeable()
		{
			var standardCosting = Helper.NewCosting(null);
			var standardCostEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			standardCostEntry.RateLines.RemoveAndDeleteAll();
			var standardCostLine = standardCostEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.M3);
			standardCostLine.GetCalculator<CombinedCalculator>().PerUnit = 100m;

			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;
			var costing = Helper.NewCosting(creditor);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.M3);
			costLine.GetCalculator<CombinedCalculator>().PerUnit = 50m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var consignor = Helper.NewOrgHeader(1);
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 810m, 5m));
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 190m, 7m));

			Factory.Save();

			var expected = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 600m,
					CostCalculationDescription = "FRT: 12 Cubic Meter(s) @ AUD 50.00/M3",
				}
			};

			var expectedLogNote = new[] { "(Searching for Freight Cost to update the job) RateLine Found FRT-CMB-M3-Standard Costs (TACT/General Rates)" };

			AutoCostAndAssert("Auto-rating should pick the more specific carrier costing", null, expected, consol, autorateRevenue: false);
			AssertAutoratingAuditLogNoteContainsLines(consol, "Contains message about lines being calculated from standard costing", expectedLogNote);

			AssertEquals("Consol chargeable rate should be updated from standard costing, even when it's filtered out.", 100m, consol.JK_ConsolChargeableRate);
		}

		#endregion

		#region TestLocalClientsTariffsTakePresidenceOverConsigneeConsignorClientRates

		public void TestLocalClientsTariffsTakePresidenceOverConsigneeConsignorClientRates()
		{
			var tariff1 = new BusinessObjectFactory().New<CompanyTariff>();
			var frtEntry = tariff1.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			var frtLine = frtEntry.RateLines[0];
			frtLine.TL_RateCalculator = UnitCalculator.Code;
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 5m;
			tariff1.AddRateEntry("ORG", "AIR", "USLAX", "").AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 30m;
			tariff1.AddRateEntry("DST", "AIR", "", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 20m;

			tariff1.Factory.Save();

			var tariff2 = new BusinessObjectFactory().New<CompanyTariff>();
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 10m);
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 10m);
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.DST, 10m);

			tariff2.Factory.Save();

			var tariff3 = new BusinessObjectFactory().New<CompanyTariff>();
			tariff3.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 20m);
			tariff3.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			tariff3.Discounts.SetDiscount(RatingConstants.RateCategory.DST, 20m);

			tariff3.Factory.Save();

			OrgHeader client = Helper.NewOrgHeader(1);
			OrgHeader consignor = Helper.NewOrgHeader(1);
			OrgHeader consignee = Helper.NewOrgHeader(1);

			consignor.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.IMP), "ALL", 1);
			consignor.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.IMP), "AIR", 2);
			consignee.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.IMP), "AIR", 3);
			consignee.CompanyData.RateTariffLevels.SetLevel("DST", nameof(OrgRateTariffLevel.Directions.EXP), "AIR", 2);
			consignee.CompanyData.RateTariffLevels.SetLevel("DST", nameof(OrgRateTariffLevel.Directions.IMP), "AIR", 3);

			Factory.Save();

			var testObject = new AutoRatingObject("USLAX", "AUSYD", FreightMode.LSE, null, 100m, 0.5m, client);
			testObject.Consignor = consignor;
			testObject.Consignee = consignee;
			var autoRater = new FreightAutoRater(new RatingContext());

			testObject.JobDirection = Directions.Import;
			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = "International Freight",
							Amount = 500m,
							CalculationSingleLineDescription = "FRT: 100 Kilogram(s) @ USD 5.00/KG",
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Origin Documentation Fee",
							Amount = 30m,
							CalculationSingleLineDescription = "ODOC: Base Rate USD 30.00",
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Destination Documentation Fee",
							Amount = 20m,
							CalculationSingleLineDescription = "DDOC: Base Rate AUD 20.00",
						},
				};

			AssertRatingResults(expected, results);

			client = Helper.NewOrgHeader();
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = client;

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = "International Freight",
							Amount = 500m,
							CalculationSingleLineDescription = "FRT: 100 Kilogram(s) @ USD 5.00/KG",
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Origin Documentation Fee",
							Amount = 24m,
							CalculationSingleLineDescription = "ODOC: Base Rate USD 24.00",
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Destination Documentation Fee",
							Amount = 16m,
							CalculationSingleLineDescription = "DDOC: Base Rate AUD 16.00",
						},
				};

			AssertRatingResults(expected, results);
		}

		#endregion

		#region Rate Entry Consignee/Consignor Filtering

		public void TestConsigneeConsignorRateEntryFiltering_Costing()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			AssertConsigneeConsignorRateEntryFiltering(TransportProvider1, costing, true);
		}

		public void TestConsigneeConsignorRateEntryFiltering_ClientRate()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			AssertConsigneeConsignorRateEntryFiltering(client, clientRate, false);
		}

		public void TestConsigneeConsignorRateEntryFiltering_Tariff()
		{
			var client = Helper.NewOrgHeader(1);
			var baseTariff = Factory.NewWithValidTestData<CompanyTariff>();
			AssertConsigneeConsignorRateEntryFiltering(client, baseTariff, false);
		}

		public void AssertConsigneeConsignorRateEntryFiltering(OrgHeader client, RatingHeader parent, bool autorateCosts)
		{
			var pickUpConsignor = Helper.NewOrgHeader();
			pickUpConsignor.OH_IsConsignor = true;

			var deliverToConsignee = Helper.NewOrgHeader();
			deliverToConsignee.OH_IsConsignee = true;

			var consignor1Line = CreateRateEntryWithOrg(parent, true, Consignor, 1);
			var consignor2Line = CreateRateEntryWithOrg(parent, true, pickUpConsignor, 2);
			var consignee1Line = CreateRateEntryWithOrg(parent, false, Consignee, 3);
			var consignee2Line = CreateRateEntryWithOrg(parent, false, deliverToConsignee, 4);

			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, ZGuid.Empty, "AUSYD", "GBLON", 10m);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Consignor.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = pickUpConsignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = deliverToConsignee.PK;

			Factory.Save();

			AssertionCharge[] expected;
			if (autorateCosts)
			{
				var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "GBLON", client, shipment);
				consol.JK_ConsolMode = ContainerModes.LCL;

				expected = new[]
					{
						new AssertionCharge { JR_OSCostAmt = 2m, ChargeCode = "ODOC" },
						new AssertionCharge { JR_OSCostAmt = 4m, ChargeCode = "FRT" }
					};
			}
			else
			{
				expected = new[]
					{
						new AssertionCharge { JR_OSSellAmt = 2m, ChargeCode = "ODOC" },
						new AssertionCharge { JR_OSSellAmt = 4m, ChargeCode = "FRT" }
					};
			}

			AutorateAndAssert(expected, shipment, client, autorateCosts: autorateCosts);

			var expectedLines = new[]
			{
				$"Information: RateLine Filtered {consignee1Line.DisplayInfo()}	reason:	overridden by {consignee2Line.DisplayInfo()} by Consignee comparer",
				$"Information: RateLine Filtered {consignor1Line.DisplayInfo()}	reason:	overridden by {consignor2Line.DisplayInfo()} by Consignor comparer"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Expected a message", expectedLines);
		}

		RateLine CreateRateEntryWithOrg(RatingHeader parent, bool isConsignor, OrgHeader orgHeader, int rate)
		{
			if (isConsignor)
			{
				Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
				var entry = parent.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.LCL, "AUSYD", "");
				entry.TI_OH_Consignor = orgHeader.PK;

				var line = entry.AddRateLine("ODOC", FlatCalculator.Code, "", CurrencyCodes.Australia);
				line.GetCalculator<FlatCalculator>().BaseRate = rate;

				return line;
			}
			else
			{
				Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
				var entry = parent.AddRateEntry(RatingConstants.RateCategory.LCL, RatingConstants.RateCategory.LCL, "AUSYD", "");
				entry.TI_OH_Consignee = orgHeader.PK;
				entry.RateLines.RemoveAndDeleteAll();

				var line = entry.AddRateLine("FRT", FlatCalculator.Code, "", CurrencyCodes.Australia);
				line.GetCalculator<FlatCalculator>().BaseRate = rate;

				return line;
			}
		}

		#endregion

		#region ClientRateNotApplicableToIncoterm

		public void TestClientRateNotApplicableToIncoterm()
		{
			var receivingAgent = Helper.NewOrgHeader();
			var consolSendingAgent = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(2);

			var tariff1Factory = new BusinessObjectFactory();

			var tariff1 = tariff1Factory.New<CompanyTariff>();
			var tariffEntry1 = tariff1.AddRateEntry("LCL", "LCL", "AUBNE", "FJSUV");
			tariffEntry1.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(tariffEntry1.Currency);

			var tariffLine1 = tariffEntry1.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.M3);
			((MinimumOrPerUnitCalculator)tariffLine1.Calculator).Minimum = 105m;
			((MinimumOrPerUnitCalculator)tariffLine1.Calculator).PerUnit = 105m;

			tariff1Factory.Save();
			var tariff2Factory = new BusinessObjectFactory();

			var tariff2 = tariff2Factory.New<CompanyTariff>();
			var tariffEntry2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL)[0];
			var index = tariffEntry2.RateLines.OverrideTariffLines(new[] { tariffEntry2.RateLines[0] });
			var overriddenLine = tariffEntry2.RateLines[index];
			overriddenLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			((CompanyTariffOrCostBasedCalculator)overriddenLine.Calculator).PerUnit = -15m;
			((CompanyTariffOrCostBasedCalculator)overriddenLine.Calculator).Minimum = -15m;

			tariff2Factory.Save();

			var rate = Helper.NewClientRate(receivingAgent);
			var rateEntry = rate.AddRateEntry("LCL", "LCL", "AUBNE", "FJSUV");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.M3);
			((MinimumOrPerUnitCalculator)rateLine.Calculator).Minimum = 100m;
			((MinimumOrPerUnitCalculator)rateLine.Calculator).PerUnit = 100m;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, receivingAgent.PK, "AUBNE", "FJSUV", 1910m, 5m);
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.Groupage;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "FJSUV";

			consol.SetDefaultSendingForwarderAddress(consolSendingAgent);
			consol.SetDefaultReceivingForwarderAddress(receivingAgent);
			consol.SetDefaultShippingLineAddress(carrier);

			Factory.Save();

			var expected = new[]
							{
								new AssertionCharge
									{
										JR_OSSellAmt = 450m,
										RevenueCalculationDescription = "FRT: 5 Cubic Meter(s) @ USD 90.00/M3"
									}
							};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		[TestDate(2019, 04, 15)]
		public void TestAutoRatingOneOffQuote_PaymentTermFiltering()
		{
			var priorities = RatingDataRegistry.Instance.ExportCollectPriorities.Value;

			var localClientRatesPriorities = priorities.AddNew();
			localClientRatesPriorities.OrganizationType = "LC";
			localClientRatesPriorities.UseCompanyTariff = true;

			var consignorRatesPriorities = priorities.AddNew();
			consignorRatesPriorities.OrganizationType = "CNR";
			consignorRatesPriorities.UseCompanyTariff = true;

			AssertEquals(priorities.Cast<RatesPriorities>().ElementAt(2).OrganizationType, "LC");
			AssertEquals(priorities.Cast<RatesPriorities>().ElementAt(3).OrganizationType, "CNR");

			RatingDataRegistry.Instance.ExportCollectPriorities.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, priorities);

			var consignor = Helper.NewOrgHeader(1);

			var tariff = Factory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;

			var frtEntry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
			frtEntry.RateLines.RemoveAndDeleteAll();
			var frtRateLine = frtEntry.AddRateLine("FRT", FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 30m;

			var orgEntry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			orgEntry.RateLines.RemoveAndDeleteAll();
			var orgRateLine = orgEntry.AddRateLine("ODOC", FlatCalculator.Code);
			orgRateLine.GetCalculator<FlatCalculator>().BaseRate = 50m;

			tariff.Factory.Save();
			Factory.Save();

			var exportCollectSpotQuote = CreateQuotedBooking(TransportModes.Sea, RateMode.LCL, IncoTerms.ExWorks, NewClient, consignor, null, TransportProvider1, "AUMEL", "SGSIN", 450m, 1m, QuotedBookingState.QuoteOnly);
			exportCollectSpotQuote.QuotedBookingNumber = "Q00010001";
			exportCollectSpotQuote.StartDate = ZDateTime.Today;
			exportCollectSpotQuote.EndDate = ZDate.Today.AddDays(14);

			var expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 50m } };
			var message = @"for export collect, charge parties are consignee and overseas agent.
since there was no overseas agent and consignee, based on registry setting, consignor is picked as charge party.
FRT charge is filtered because FRT charge group is not applicable for AUMEL-SGSIN Export CLT";

			AutorateAndAssert(message, expected, exportCollectSpotQuote, consignor);

			var exportPrepaidSpotQuote = CreateQuotedBooking(TransportModes.Sea, RateMode.LCL, IncoTerms.DeliveredDutyPaid, NewClient, consignor, null, TransportProvider1, "AUMEL", "SGSIN", 450m, 1m, QuotedBookingState.QuoteOnly);
			exportPrepaidSpotQuote.QuotedBookingNumber = "Q00010002";
			exportPrepaidSpotQuote.StartDate = ZDateTime.Today;
			exportPrepaidSpotQuote.EndDate = ZDate.Today.AddDays(14);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 50m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m
				}
			};

			message = "for export prepaid, charge parties are consignor and local client";
			AutorateAndAssert(message, expected, exportPrepaidSpotQuote, NewClient);
		}

		#region Cost is correctly applied to Origin/Destination

		[TestDate(2016, 1, 21)]
		public void TestCostIsCorrectlyAppliedToOriginDestination()
		{
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(carrier);
			var costEntry = costing.AddRateEntry("AIR", "LSE", "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine1 = costEntry.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)costLine1.Calculator).BaseRate = 100m;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.ConsignorPK = localClient.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "JPNRT";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 1m;
			shipment.JS_INCO = "CFR";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = "AIR";
			consol1.JK_ConsolMode = "LSE";
			consol1.JK_RL_NKLoadPort = "JPNRT";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol1.JK_PrepaidCollect = "PPD";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = "AIR";
			consol2.JK_ConsolMode = "LSE";
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "AUMEL";
			consol2.JK_PrepaidCollect = "PPD";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m,
					CostCalculationDescription = @"Charge located in TESTORG3 (Consol C00001000 --> Carrier) cost with the following details:

Mode:			LSE
Charge Code Group:	FRT
Start Date:		21 January 2016
End Date:		21 July 2016
Origin:			AU
Commodity Code:		GEN
Currency:		AUD
Autorated for:		Shipment S00001000
Leg:			AUMEL-AUSYD"
				}
			};

			AutorateAndAssert(expected, shipment, localClient, autorateRevenue: false);
		}

		#endregion

		#region Universal charge code is in cost calculation description

		public void TestUniversalChargeCodeIsInDescription()
		{
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();
			var carrier = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(carrier);
			var costEntry = costing.AddRateEntry("AIR", "LSE", "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine1 = costEntry.AddRateLine("FRT", FlatCalculator.Code);
			var mapping1 = costLine1.ChargeCode.UniversalChargeCodeMappingsCollection.AddNew();
			mapping1.AUP_Code = "BAF";

			((FlatCalculator)costLine1.Calculator).BaseRate = 100m;

			#region Setup shippment

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.ConsignorPK = localClient.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "JPNRT";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 1m;
			shipment.JS_INCO = "CFR";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = "AIR";
			consol1.JK_ConsolMode = "LSE";
			consol1.JK_RL_NKLoadPort = "JPNRT";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol1.JK_PrepaidCollect = "PPD";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = "AIR";
			consol2.JK_ConsolMode = "LSE";
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "AUMEL";
			consol2.JK_PrepaidCollect = "PPD";

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m,
					CostCalculationDescription = @"Charge located in TESTORG3 (Consol C00001000 --> Carrier) cost with the following details:

Universal Charge Codes:	BAF"
				}
			};

			AutorateAndAssert(expected, shipment, localClient, autorateRevenue: false);
		}

		#endregion

		#region PackageAndDistanceRatingInShipping

		public void TestPackageAndDistanceRatingInShipping()
		{
			var chargeCode = Helper.ChargeCodes.New("BOLFRT", "Bill Of Ladding Freight", UnitCalculator.Code);
			Helper.ChargeCodes.NewChargeTypeOverride(chargeCode, JobInvoicingConsumerTypes.AgencyBillOfLadingCode);

			var shipORGCharge = Helper.ChargeCodes.New("SOR1", "Shipping Pickup Cartage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var shipFRTCharge = Helper.ChargeCodes.New("SFR2", "Shipping RoRo Freight", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var rate = Helper.NewClientRate(NewClient);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.SNC, "LCL", "AUBNE", "USLAX");
			AddParityExchangeRate(entry.Currency);

			entry.RateLines.RemoveAndDeleteAll();
			var line11 = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.PK);
			line11.GetCalculator<UnitCalculator>().PerUnit = 20;

			var line12 = entry.AddRateLine(shipFRTCharge, CombinedCalculator.Code, QuantityUnit.CN);
			line12.Calculator["-500"] = (ZDecimal)500m;
			line12.Calculator["+500"] = (ZDecimal)900m;
			line12.Calculator["+1000"] = (ZDecimal)1800m;
			line12.Calculator["+2000"] = (ZDecimal)3000m;
			line12.Calculator["+5000"] = (ZDecimal)5000m;
			line12.Calculator["+10000"] = (ZDecimal)9000m;

			line12.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus).TM_BreakWeightVolume = QuantityUnit.KG;
			line12.RateLineItems[0].TM_BreakWeightVolume = QuantityUnit.KG;

			var entry2 = rate.AddRateEntry("SOR", "ALL", "AUBNE", "");
			var line2 = entry2.AddRateLine(shipORGCharge, UnitCalculator.Code, QuantityUnit.KM);
			line2.GetCalculator<UnitCalculator>().PerUnit = 10;

			Factory.Save();

			var bill = Factory.NewWithValidTestData<BillOfLading>();
			bill.JS_TransportMode = TransportModes.Sea;
			bill.JS_PackingMode = ContainerModes.RollOnRollOff;
			bill.JS_INCO = "FOB";
			bill.JS_RL_NKOrigin = "AUBNE";
			bill.JS_RL_NKDestination = "USLAX";

			var vehicle1 = bill.RealContainers.AddNew();
			vehicle1.JC_ContainerMode = ContainerModes.RollOnRollOff;
			vehicle1.JC_GrossWeight = 1.024m;
			vehicle1.JC_GrossWeightUQ = Weight.Tonnes;

			var vehicle2 = bill.RealContainers.AddNew();
			vehicle2.JC_ContainerMode = ContainerModes.RollOnRollOff;
			vehicle2.JC_GrossWeight = 2.048m;
			vehicle2.JC_GrossWeightUQ = Weight.Tonnes;

			var route1 = bill.Transports.AddNew();
			route1.JW_RL_NKLoadPort = "AUBNE";
			route1.JW_RL_NKDiscPort = "AUBNE";
			route1.JW_TransportMode = TransportModes.Road;
			route1.JW_TransportType = TransportPlanningType.PreCarriage;
			route1.JW_OA_DepartureLocation = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			route1.JW_Distance = 20m;
			route1.JW_DistanceUnit = Length.Kilometres;

			var route2 = bill.Transports.AddNew();
			route2.JW_RL_NKLoadPort = "AUBNE";
			route2.JW_RL_NKDiscPort = "AUSYD";
			route2.JW_TransportMode = TransportModes.Road;
			route2.JW_Distance = 1000m;
			route2.JW_DistanceUnit = Length.Kilometres;

			var route3 = bill.Transports.AddNew();
			route3.JW_RL_NKLoadPort = "AUSYD";
			route3.JW_RL_NKDiscPort = "USLAX";
			route3.JW_TransportMode = TransportModes.Sea;
			route3.JW_TransportType = TransportPlanningType.MainVessel;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 4800m,
					RevenueCalculationDescription = "SFR2: 1 Container(s) @ USD 3000.00/Container + 1 Container(s) @ USD 1800.00/Container"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 40m,
					RevenueCalculationDescription = "BOLFRT: 2 Package(s) @ USD 20.00/Package"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 200m,
					RevenueCalculationDescription = "SOR1: 20 Kilometer(s) @ AUD 10.00/Kilometer"
				}
			};

			AutorateAndAssert(expected, bill, rate.Header);
		}

		#endregion

		#region Custom Breaks On Sliding Calculators Will AutoRate With out Minus RateLine Item

		public void TestCombinedCalculatorWillAutoRateWithoutMinusRateLineItem()
		{
			var shipORLGCharge = Helper.ChargeCodes.New("NEW1", "New CMB Calculator Code", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var rate = Helper.NewClientRate(NewClient);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			var line = entry.AddRateLine(shipORLGCharge.AC_Code, CombinedCalculator.Code, QuantityUnit.KG);

			var calculator = line.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50m, 25m, 0);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 10m, 0);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 300m, 5m, 0);

			var shipment1 = CreateForwardingShipment(TransportModes.Air, NewClient.PK, ZGuid.Empty, "AUBNE", "USLAX", 45m);
			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 1125m,
						RevenueCalculationDescription = "NEW1: 45 Kilogram(s) @ AUD 25.00/KG"
					}
				};

			AutorateAndAssert(expected, shipment1, NewClient);

			var shipment2 = CreateForwardingShipment(TransportModes.Air, NewClient.PK, ZGuid.Empty, "AUBNE", "USLAX", 50m);
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 1250m,
						RevenueCalculationDescription = "NEW1: 50 Kilogram(s) @ AUD 25.00/KG"
					}
				};

			AutorateAndAssert(expected, shipment2, NewClient);

			var shipment3 = CreateForwardingShipment(TransportModes.Air, NewClient.PK, ZGuid.Empty, "AUBNE", "USLAX", 300m);
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 1500m,
						RevenueCalculationDescription = "NEW1: 300 Kilogram(s) @ AUD 5.00/KG"
					}
				};

			AutorateAndAssert(expected, shipment3, NewClient);
		}

		#endregion

		#region Call For Pricing encountered on Rate

		public void TestClientRateWithCallForPricingNotifiesUser()
		{
			var testCharge = Helper.ChargeCodes.New("NEW1", "New CMB Calculator Code", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "AUBNE", "USLAX");
			var line = entry.AddRateLine(testCharge.AC_Code, CombinedCalculator.Code, QuantityUnit.KG);

			var combinedCalculator = line.GetCalculator<CombinedCalculator>();
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50m, 25m, 0);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 10m, 0);
			var lastPlusItem = combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 300m, 5m, 0);
			lastPlusItem.TM_CallForPricing = true;
			lastPlusItem.TM_Text = "Call us for pricing";

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, ZGuid.Empty, "AUBNE", "USLAX", 300m);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				job.JH_OA_LocalChargesAddr = rate.Header.MainAddress.PK;

				plugin.ExecuteAutorating(AutoRateOptions.AutorateRevenue);
				var messages = UnitTestUserNotification.Instance.PreviousMessages.ToArray();
				Assert("Should contain call for pricing warning", messages.Any(x => x.Text == "Greater than 300 Kilogram(s) Call us for pricing"));
			}
		}

		#endregion

		#region ContainerOwnership

		public void TestContainerOwnership()
		{
			var chargeCode = Helper.ChargeCodes.New("BOLFRT", "Bill Of Lading Freight", UnitCalculator.Code);
			Helper.ChargeCodes.NewChargeTypeOverride(chargeCode, JobInvoicingConsumerTypes.AgencyBillOfLadingCode);

			var rate = Helper.NewClientRate(NewClient);
			var entry = rate.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			entry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(entry.Currency);

			var line1 = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line1.GetCalculator<UnitCalculator>().PerUnit = 2000;

			var line2 = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line2.GetCalculator<UnitCalculator>().PerUnit = 1100;
			line2.TL_ContainerOwnership = ContainerOwnership.Codes.ShipperOwned;

			Factory.Save();

			var bill = Factory.NewWithValidTestData<BillOfLading>();
			bill.JS_TransportMode = TransportModes.Sea;
			bill.JS_PackingMode = ContainerModes.FCL;
			bill.JS_INCO = "FOB";
			bill.JS_RL_NKOrigin = "AUSYD";
			bill.JS_RL_NKDestination = "USLAX";

			var c1 = bill.RealContainers.AddNew();
			var c2 = bill.RealContainers.AddNew();
			var c3 = bill.RealContainers.AddNew();
			var c4 = bill.RealContainers.AddNew();
			var c5 = bill.RealContainers.AddNew();

			c1.JC_RC = GP20.PK;
			c2.JC_RC = GP20.PK;
			c3.JC_RC = GP20.PK;
			c4.JC_RC = GP20.PK;
			c5.JC_RC = GP20.PK;

			c1.JC_IsShipperOwned = true;
			c2.JC_IsShipperOwned = true;
			c3.JC_IsShipperOwned = false;
			c4.JC_IsShipperOwned = false;
			c5.JC_IsShipperOwned = false;

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 8200m,
					RevenueCalculationDescription = "BOLFRT: 3 20GP Container(s) @ USD 2000.00/Container\n\tFRT: 2 20GP Shipper Owned Container(s) @ USD 1100.00/Container"
				}
			};

			AutorateAndAssert(expected, bill, NewClient);

			var line3 = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line3.GetCalculator<UnitCalculator>().PerUnit = 1900;
			line3.TL_ContainerOwnership = ContainerOwnership.Codes.CarrierOwned;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7900m,
					RevenueCalculationDescription = "BOLFRT: 3 20GP Carrier Owned Container(s) @ USD 1900.00/Container\n\tFRT: 2 20GP Shipper Owned Container(s) @ USD 1100.00/Container"
				}
			};

			AutorateAndAssert(expected, bill, NewClient);
		}

		public void TestContainerOwnership_IncludesLineWithEmptyContainerOwnership()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			var costing = Factory.New<Costing>();
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ULD, "AU", "");
			rateEntry.TI_RC = Helper.Containers["PM-2H"].PK;

			var rateLine1 = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 25;

			var rateLine2 = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 10;

			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "DEHAM", 100));
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "DEHAM", 200));

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = rateEntry.TI_RC;
			container.JC_ContainerMode = ContainerModes.ULD;
			container.JC_IsShipperOwned = false;

			Assert("Pre-condition: ContainerOwnership should be empty by default on CN unit lines", rateLine1.TL_ContainerOwnership.IsEmpty);

			Factory.Save();

			var expectedCosts = new[]
					{
						new AssertionCost
						{
							ChargeCode = "FRT",
							E6_OSCostAmount = 45m,
							CostCalculationDescription = @"FRT: 1 PM-2H Container(s) @ AUD 25.00/Container
FRT: 2 House Bill(s) @ AUD 10.00/House Bill"
						}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("CN Rate should not be filtered out", null, expectedCosts, consol, false);
		}

		public void TestContainerOwnership_PrefersMatchingContainerOwnershipToEmpty()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			var costing = Factory.New<Costing>();
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ULD, "AU", "");
			rateEntry.TI_RC = Helper.Containers["PM-2H"].PK;

			var standardLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			standardLine.GetCalculator<UnitCalculator>().PerUnit = 10;

			var shipperOwnedLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			shipperOwnedLine.GetCalculator<UnitCalculator>().PerUnit = 25;
			shipperOwnedLine.TL_ContainerOwnership = ContainerOwnership.Codes.ShipperOwned;

			var carrierOwnedLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			carrierOwnedLine.GetCalculator<UnitCalculator>().PerUnit = 30;
			carrierOwnedLine.TL_ContainerOwnership = ContainerOwnership.Codes.CarrierOwned;

			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "DEHAM", 100));
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "DEHAM", 200));

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = rateEntry.TI_RC;
			container.JC_ContainerMode = ContainerModes.ULD;
			container.JC_IsShipperOwned = false;

			Factory.Save();

			var expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 30m,
								CostCalculationDescription = @"FRT: 1 PM-2H Carrier Owned Container(s) @ AUD 30.00/Container"
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutoCostAndAssert("Only the carrier Owned RateLine should be included", null, expectedCosts, consol, false);

			container.JC_IsShipperOwned = true;

			expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 25m,
								CostCalculationDescription = @"FRT: 1 PM-2H Shipper Owned Container(s) @ AUD 25.00/Container"
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutoCostAndAssert("Should now match the shipper line", null, expectedCosts, consol, false);
		}

		#endregion

		#region TransportZonesCostAndSellMerging()

		[TestDate(2016, 01, 01)]
		public void TestTransportZonesCostAndSellMerging()
		{
			var cartageCo = Helper.NewOrgHeader();

			var postCode1 = Helper.CreateRefPostCode("2100");
			var postCode2 = Helper.CreateRefPostCode("2200");
			var globalPostCode1 = Helper.CreateRefPostCode("2100");
			var globalPostCode2 = Helper.CreateRefPostCode("2200");

			var zoneSet = Helper.CreateRateTransportZoneSet(cartageCo, CountryCodes.Australia);
			var zone = zoneSet.CreateRateTransportZoneForTest("CARTZ 1");
			zone.CreateRateTransportZoneItemForTest(postCode1, postCode2);

			var globalZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia);
			var globalZone = globalZoneSet.CreateRateTransportZoneForTest("CARTZ 1");
			globalZone.CreateRateTransportZoneItemForTest(globalPostCode1, globalPostCode2);

			Factory.Save();

			var consignor = Helper.NewOrgHeader();
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.OA_PostCode = "2125";

			var consignee = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(cartageCo);
			var costEntry = cost.AddRateEntry("ORG", "LCL", "AU", "");
			var costLine = costEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.M3);
			costLine.TL_RX_NKCurrency = "AUD";
			costLine.ConversionFactor = new ConversionFactor(1000m, Weight.Kilograms, Volume.CubicMetres);
			costLine.TL_RateCalculator = CartageZoneDistanceCalculator.Code;

			var costCartageZones = costLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones;
			costLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, costCartageZones[1].ZonePK);

			var clientRate = Helper.NewClientRate(consignor);
			var clientEntry = clientRate.AddRateEntry("ORG", "LCL", "AU", "");
			var clientLine = clientEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.M3);
			clientLine.TL_RX_NKCurrency = "AUD";
			clientLine.ConversionFactor = new ConversionFactor(1000m, Weight.Kilograms, Volume.CubicMetres);
			clientLine.TL_RateCalculator = CartageZoneDistanceCalculator.Code;
			var clientCartageZones = clientLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones;
			clientLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, clientCartageZones[1].ZonePK);

			var shipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, consignee.PK, "AUSYD", "USLAX", 300m);
			shipment.JS_OA_ExportReceivingDepot = cartageCo.MainAddress.PK;

			Factory.Save();

			var zoneDescription = DescriptionHelpers.FormatWithTab("Zone:");
			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 3m,
							JR_OSSellAmt = 3m,
							CostCalculationDescription = @"ODOC: 0.3 Cubic Meter(s) @ AUD 10.00/M3
" + zoneDescription + "TESTORG1 CARTZ 1",
							RevenueCalculationDescription = @"ODOC: 0.3 Cubic Meter(s) @ AUD 10.00/M3
" + zoneDescription + "CARTZ 1"
						}
				};

			var expectedLogLines = new[] { @"Information: Matched 'CARTZ 1' for RateLine ODOC-CTZ-M3-Costing TESTORG1
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'CARTZ 1' transport zone matched by Consignor Pickup/Delivery Address fallback",
@"
Information: Matched 'CARTZ 1' for RateLine ODOC-CTZ-M3-Client Rate TESCLISYD
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'CARTZ 1' transport zone matched by Consignor Pickup/Delivery Address fallback" };

			AutorateAndAssert(expected, shipment, consignor);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain CTZ distance information", expectedLogLines);
		}

		#endregion

		#region CarrierContractNumbers

		public void TestOverwritingContractNumbersOnRating()
		{
			var creditor1 = Helper.NewOrgHeader();
			creditor1.OH_Code = "creditor1";
			var creditor2 = Factory.New<OrgHeader>();
			creditor2.OH_Code = "creditor2";

			var sCosting = Helper.NewCosting(null);

			var sEntry = sCosting.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX");
			sEntry.RateLines.RemoveAndDeleteAll();

			var sLine = sEntry.AddRateLine("CAF", FlatCalculator.Code);
			sLine.GetCalculator<FlatCalculator>().BaseRate = 700m;

			var costing = Helper.NewCosting(creditor1);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX");
			entry.TI_ContractNumber = "1111";
			entry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			entry.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 1000;

			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();

			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			consol1.JK_ConsolMode = ContainerModes.FCL;
			consol1.JK_TransportMode = TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "USLAX";
			consol1.JK_OA_CreditorAddress = creditor1.MainAddress.PK;

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = creditor1.MainAddress.PK;

			Factory.Save();

			var mockDialogService = new Mock<IDialogService>();
			mockDialogService
				.Setup(x => x.SelectSingleCarrierContractNumber(new[] { "", "1111" }))
				.Returns(new SingleCarrierContractNumberSelectionResult("1111"));

			FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var autoRater = new FreightAutoRater(new RatingContext(null, mockDialogService.Object));
			autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Cost);

			AssertEquals("1111", consol1.JK_CarrierContractNumber);
		}

		public void TestPopulateContractNumberForDifferentConsolsOnTheSameShipment()
		{
			var creditor1 = Helper.NewOrgHeader();
			creditor1.OH_Code = "creditor1";
			var creditor2 = Factory.New<OrgHeader>();
			creditor2.OH_Code = "creditor2";

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_IsGroupageCharge = true;
			chargeCode.AC_Code = "AAA";
			chargeCode.AC_ChargeGroup = "FRT";

			var contractNumber1 = "CTR009";
			var contractNumber2 = "CTR008";
			var costing1 = Helper.NewCosting(creditor1);
			var entry1 = costing1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "NZ", "AU");
			entry1.TI_ContractNumber = contractNumber1;
			entry1.RateLines[0].TL_AC = chargeCode.PK;
			entry1.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			entry1.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 1000;

			var costing2 = Helper.NewCosting(creditor2);
			var entry2 = costing2.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "NZCFT", "AU");
			entry2.TI_ContractNumber = contractNumber2;
			entry2.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			entry2.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 1000;

			var entry3 = costing2.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "NZ", "AUSYD");
			entry3.TI_ContractNumber = "";
			entry3.RateLines[0].TL_AC = Helper.ChargeCodes["WAR"].PK;
			entry3.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			entry3.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 600;

			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZCFT";
			shipment.JS_RL_NKDestination = "AUSYD";

			consol1.JK_ConsolMode = ContainerModes.Loose;
			consol1.JK_TransportMode = TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "AUMEL";
			consol1.JK_OA_CreditorAddress = creditor1.MainAddress.PK;
			consol1.JK_CarrierContractNumber = contractNumber1;

			consol2.JK_ConsolMode = ContainerModes.Loose;
			consol2.JK_TransportMode = TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "NZCFT";
			consol2.JK_RL_NKDischargePort = "AUSYD";
			consol2.JK_PrepaidCollect = PaymentType.Collect;
			consol2.JK_OA_ShippingLineAddress = creditor2.MainAddress.PK;
			consol2.JK_OA_CreditorAddress = ZGuid.Empty;
			consol2.JK_CarrierContractNumber = contractNumber2;

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = creditor1.MainAddress.PK;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Cost);
			AssertEquals(contractNumber1, consol1.JK_CarrierContractNumber);
			AssertEquals(contractNumber2, consol2.JK_CarrierContractNumber);

			testJob.Charges.RemoveAndDeleteAll();

			consol2.JK_OA_ShippingLineAddress = ZGuid.Empty;

			autoRater = new FreightAutoRater(new RatingContext());
			autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Cost);

			AssertEquals(contractNumber1, consol1.JK_CarrierContractNumber);
			AssertEquals(contractNumber2, consol2.JK_CarrierContractNumber);
		}

		#endregion

		#region Brokerage Services

		[TestDate(2014, 10, 17, 12, 11, 0)]
		public void TestBrokerageServices()
		{
			AccChargeCode charge1 = Helper.ChargeCodes.New("#TST1", "Test 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerage, FreightServiceType.Codes.Tailgate);
			AccChargeCode charge2 = Helper.ChargeCodes.New("#TST2", "Test 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerageOnly, FreightServiceType.Codes.Cleaning);
			AccChargeCode charge3 = Helper.ChargeCodes.New("#TST3", "Test 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
			AccChargeCode charge4 = Helper.ChargeCodes.New("#TST4", "Test 4", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);
			AccChargeCode charge5 = Helper.ChargeCodes.New("#TST5", "Test 5", FlatCalculator.Code, ChargeCodeGroupList.Codes.CustomsDuty, FreightServiceType.Codes.Fumigation);

			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());
			RateEntry entry1 = rate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX", "", "");

			RateLine line1 = entry1.AddRateLine(charge1, FlatCalculator.Code);
			line1.GetCalculator<FlatCalculator>().BaseRate = 10;
			line1.TL_RX_NKCurrency = "USD";

			RateLine line2 = entry1.AddRateLine(charge2, FlatCalculator.Code);
			line2.GetCalculator<FlatCalculator>().BaseRate = 20m;
			line2.TL_RX_NKCurrency = "USD";

			RateLine line3 = entry1.AddRateLine(charge3, FlatCalculator.Code);
			line3.GetCalculator<FlatCalculator>().BaseRate = 15m;
			line3.TL_RX_NKCurrency = "USD";

			RateLine line4 = entry1.AddRateLine(charge4, FlatCalculator.Code);
			line4.GetCalculator<FlatCalculator>().BaseRate = 5m;
			line4.TL_RX_NKCurrency = "USD";

			RateLine line5 = entry1.AddRateLine(charge5, FlatCalculator.Code);
			line5.GetCalculator<FlatCalculator>().BaseRate = 25m;
			line5.TL_RX_NKCurrency = "USD";

			var declaration1 = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration1.JE_TransportMode = TransportModes.Sea;
			declaration1.JE_ContainerMode = ContainerModes.LCL;
			declaration1.JE_MessageType = "EXP";
			declaration1.JE_RL_NKOrigin = "AUSYD";
			declaration1.JE_RL_NKFinalDestination = "USLAX";
			declaration1.JE_ShipmentIncoTerm = "EXW";

			JobService service11 = declaration1.DocsAndCartage.Services.AddNew();
			service11.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			service11.ES_Completed = ZDateTime.Today;
			service11.ES_ServiceId = "Service11";

			JobService service12 = declaration1.DocsAndCartage.Services.AddNew();
			service12.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service12.ES_Completed = ZDateTime.Today;

			Job testJob1 = CreateJob(declaration1, declaration1.JE_DeclarationReference);
			testJob1.PlugInData = declaration1;
			testJob1.JH_OA_AgentCollectAddr = rate.Header.MainAddress.PK;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(declaration1.GetFirstAdapter()), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 20m,
										InvoiceLineDesc = "Test 2",
										CalculationSingleLineDescription = "#TST2: Base Rate USD 20.00 (Service ID Service11)"
									}
							};

			AssertRatingResults(expected, results);

			GlbCompany.CurrentCompany.SetCountry("US");

			var declaration2 = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration2.JE_TransportMode = TransportModes.Sea;
			declaration2.JE_ContainerMode = ContainerModes.LCL;
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_RL_NKOrigin = "AUSYD";
			declaration2.JE_RL_NKFinalDestination = "USLAX";
			declaration2.JE_ShipmentIncoTerm = "EXW";

			JobService service21 = declaration2.DocsAndCartage.Services.AddNew();
			service21.ES_ServiceCode = FreightServiceType.Codes.Tailgate;
			service21.ES_Completed = ZDateTime.Today;

			JobService service22 = declaration2.DocsAndCartage.Services.AddNew();
			service22.ES_ServiceId = "Service22";
			service22.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service22.ES_Completed = ZDateTime.Today;

			Job testJob2 = CreateJob(declaration2, declaration2.JE_DeclarationReference);
			testJob2.PlugInData = declaration2;
			testJob2.JH_OA_LocalChargesAddr = rate.Header.MainAddress.PK;

			Factory.Save();

			results = autoRater.AutoRate(new AutoRatingProxy(declaration2.GetFirstAdapter()), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 25m,
										InvoiceLineDesc = "Test 5",
										CalculationSingleLineDescription = $"#TST5: Base Rate USD 25.00 (Service ID Service22)"
									},
							};

			AssertRatingResults(expected, results);
		}

		public void TestBrokerageServicesAutorateWithoutDeclarationAttachedIfDefinedInRegistry()
		{
			var today = ZDate.Today;
			Env.Registry.Rating.SetFreightRatedCodes("ORG,LOD,UNL,DST,INS,FRT,BRK,BON,OBR,OBO");

			var brkCharge = Helper.ChargeCodes.New("TST1", "TST1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Brokerage, FreightServiceType.Codes.CustomsHold);
			var brkonlyCharge = Helper.ChargeCodes.New("TST2", "TST2", UnitCalculator.Code, ChargeCodeGroupList.Codes.BrokerageOnly, FreightServiceType.Codes.ExtraInspection);
			var orgBrkCharge = Helper.ChargeCodes.New("TST3", "TST3", UnitCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerage, FreightServiceType.Codes.Tailgate);
			var orgBrkOnlyCharge = Helper.ChargeCodes.New("TST4", "TST4", UnitCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerageOnly, FreightServiceType.Codes.Cleaning);

			Helper.Factory.Save();

			var tariff = Helper.NewCompanyTariff();
			var dstEntry = tariff.AddRateEntry("DST", "ALL", "", "AU");
			dstEntry.TI_RateStartDate = today.AddDays(-30);
			dstEntry.RateLines.RemoveAndDeleteAll();

			var customsHoldLine = dstEntry.AddRateLine(brkCharge, FlatCalculator.Code);
			((FlatCalculator)customsHoldLine.Calculator).BaseRate = 10;

			var extraInspectionLine = dstEntry.AddRateLine(brkonlyCharge, FlatCalculator.Code);
			((FlatCalculator)extraInspectionLine.Calculator).BaseRate = 15;

			var orgEntry = tariff.AddRateEntry("ORG", "ALL", "DE", "");
			orgEntry.TI_RateStartDate = today.AddDays(-30);
			orgEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			orgEntry.RateLines.RemoveAndDeleteAll();

			var tailgateLine = orgEntry.AddRateLine(orgBrkCharge, FlatCalculator.Code);
			((FlatCalculator)tailgateLine.Calculator).BaseRate = 20;

			var cleaningLine = orgEntry.AddRateLine(orgBrkOnlyCharge, FlatCalculator.Code);
			((FlatCalculator)cleaningLine.Calculator).BaseRate = 30;

			tariff.Factory.Save();

			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader(2);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "DEFRA", "AUSYD", 1000m);
			shipment.JS_E_DEP = today.AddDays(-20);
			shipment.JS_E_ARV = today.AddDays(2);

			Factory.Save();

			var customsHold = shipment.DocsAndCartage.Services.AddNew();
			customsHold.ES_ServiceCode = FreightServiceType.Codes.CustomsHold;
			customsHold.ES_Booked = shipment.JS_E_DEP.AddDays(10);
			customsHold.ES_Completed = customsHold.ES_Booked;
			customsHold.ES_ServiceCount = 1.0m;
			customsHold.ES_OH_Contractor = Factory.NewWithValidTestData<OrgHeader>().PK;

			var extraInspection = shipment.DocsAndCartage.Services.AddNew();
			extraInspection.ES_ServiceCode = FreightServiceType.Codes.ExtraInspection;
			extraInspection.ES_Booked = shipment.JS_E_DEP.AddDays(10);
			extraInspection.ES_Completed = customsHold.ES_Booked;
			extraInspection.ES_ServiceCount = 1.0m;
			extraInspection.ES_OH_Contractor = Factory.NewWithValidTestData<OrgHeader>().PK;

			var tailgate = shipment.DocsAndCartage.Services.AddNew();
			tailgate.ES_ServiceCode = FreightServiceType.Codes.Tailgate;
			tailgate.ES_Booked = shipment.JS_E_DEP.AddDays(-1);
			tailgate.ES_Completed = tailgate.ES_Booked;
			tailgate.ES_ServiceCount = 1.0m;
			tailgate.ES_OH_Contractor = Factory.NewWithValidTestData<OrgHeader>().PK;

			var cleaning = shipment.DocsAndCartage.Services.AddNew();
			cleaning.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			cleaning.ES_Booked = shipment.JS_E_DEP.AddDays(-1);
			cleaning.ES_Completed = tailgate.ES_Booked;
			cleaning.ES_ServiceCount = 1.0m;
			cleaning.ES_OH_Contractor = Factory.NewWithValidTestData<OrgHeader>().PK;

			var jobServicesCollection = (shipment.RatingAdapter).JobServices;
			Assert(jobServicesCollection.IsEnabled(ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.CustomsHold));
			Assert(jobServicesCollection.IsEnabled(ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.ExtraInspection));
			Assert(jobServicesCollection.IsEnabled(ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Tailgate));
			Assert(jobServicesCollection.IsEnabled(ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Cleaning));

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 10.00m,
							RevenueCalculationDescription = "TST1: Base Rate AUD 10.00"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 15.00m,
							RevenueCalculationDescription = "TST2: Base Rate AUD 15.00"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 20.00m,
							RevenueCalculationDescription = "TST3: Base Rate AUD 20.00"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 30.00m,
							RevenueCalculationDescription = "TST4: Base Rate AUD 30.00"
						},
				};

			AutorateAndAssert(expected, shipment, consignee);
		}

		#endregion

		#region PercentageCalculatorAndMultipleRatings

		// Test this
		public void TestPercentageCalculatorWithMultipleRatingsWithCustomsCharges()
		{
			CreateRefCusRateCode(Constants.Customs.CusEntryFeeTypes.DutyAmount, Constants.Customs.CusEntryFeeTypes.DutyAmount);

			var cusDSB = Helper.ChargeCodes["CCLR"];

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)entry.RateLines[0].Calculator).BaseRate = 1000;
			entry.RateLines[0].TL_RX_NKCurrency = "AUD";

			var line2 = entry.AddRateLine("CAF", PercentageCalculator.Code);
			line2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = cusDSB.PK;
			line2.GetCalculator<PercentageCalculator>().Percent = 10m;
			line2.TL_RX_NKCurrency = "AUD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";
			shipment.CustomsEntryNumberType = "T1";

			Factory.Save();

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_EntryStatus = "DWC";

			ServiceLocator.GetService<ICustomsCharges>(declaration);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew(Constants.Customs.CusEntryFeeTypes.DutyAmount, 700m);

			var chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var chargeTypeSettingDuty = chargeTypeSettings.AddNew();
			chargeTypeSettingDuty.ChargeType = Constants.Customs.CusEntryFeeTypes.DutyAmount;
			chargeTypeSettingDuty.AC_ChargeCode = cusDSB.PK;

			entryHeader.EntryChargeTypeList.RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeTypeSettings);

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = rate.Header.MainAddress.PK;

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = cusDSB.PK;
			charge.JR_OH_SellAccount = rate.Header.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OSSellAmt = 600m;
			charge.JR_SellRatingOverride = true;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CCLR",
					JR_OSSellAmt = 600m,
					RevenueCalculationDescription = ""
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1000m,
					RevenueCalculationDescription = "FRT: Base Rate AUD 1000.00"
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 60.00m,
					RevenueCalculationDescription = "CAF: 10.00% of (AUD 600.00 (CCLR*))"
				},
			};

			AutorateAndAssert(expected, shipment, null, rate.Header, testJob);

			declaration.JE_JS = shipment.PK;

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1000m,
					RevenueCalculationDescription = "FRT: Base Rate AUD 1000.00"
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 130m,
					RevenueCalculationDescription = "CAF: 10.00% of (AUD 1300.00 (CCLR* 600.00 + CCLR 700.00))"
				},
				new AssertionCharge
				{
					ChargeCode = "CCLR",
					JR_OSCostAmt = 700m,
					JR_OSSellAmt = 600m,
					RevenueCalculationDescription = "",
				},
				new AssertionCharge
				{
					ChargeCode = "CCLR",
					JR_OSSellAmt = 700m,
				}
			};

			AutorateAndAssert(expected, shipment, null, rate.Header, testJob);

			charge.JR_SellRatingOverride = false;
			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1000m,
					RevenueCalculationDescription = "FRT: Base Rate AUD 1000.00"
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 70.00m,
					RevenueCalculationDescription = "CAF: 10.00% of (AUD 700.00 (CCLR))"
				},
				new AssertionCharge
				{
					ChargeCode = "CCLR",
					JR_OSCostAmt = 700m,
					JR_OSSellAmt = 700m,
				},
			};

			AutorateAndAssert(expected, shipment, null, rate.Header, testJob);
		}

		#endregion

		#region TestAutoratingResultsAuditLogDoesNotThrowExceptionWhenCustomsChargesAreDeleted

		public void TestAutoratingResultsAuditLogDoesNotThrowExceptionWhenCustomsChargesAreDeleted()
		{
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var frtChargeCode = Helper.ChargeCodes["FRT"];

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = clientRateEntry.AddRateLine(frtChargeCode, FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 1000m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Air;
			declaration.JE_JS = shipment.PK;

			declaration.CustomsEntryHeaders.AddNew();

			var customsJob = new Job.Loader(declaration).TryLoadOrCreate();

			customsJob.Parent = declaration;

			AccTaxRate.LoadExistingOrCreateNewTaxRate(new BusinessObjectFactory(), "EXEMPT", AccTaxRate.Types.Exempt, 0).Factory.Save();

			var customsDSB = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			customsDSB.AC_DepartmentFilterList = "ALL";
			customsDSB.AC_GC = GlbCompany.CurrentCompany.PK;

			var chargeLine = customsJob.Charges.AddNew();
			chargeLine.JR_AC = customsDSB.PK;
			chargeLine.JR_OSCostAmt = 100m;
			chargeLine.JR_OSSellAmt = 100m;
			chargeLine.JR_EstimatedCost = 100m;
			Assert("Prerequisite: ", chargeLine.IsCustomsCharge);

			Factory.Save();

			using (var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate())
			{
				shipmentJob.LocalChargesPK = client.PK;
				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = frtChargeCode.AC_Code,
						JR_OSSellAmt = 1000m,
					},
				};

				AutorateAndAssert(expected, shipment, client, job: shipmentJob);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Expect message", "Charges deleted: 1 Deleted Charges");
			}
		}

		#endregion

		#region Autorating Should Use Client Specific Exchange Rates

		public void TestAutoratingShouldUseClientSpecificExchangeRates()
		{
			Helper.NewExchangeRate(CurrencyCodes.UnitedStates, "BUY", 0.5m);
			Helper.NewExchangeRate(CurrencyCodes.UnitedStates, "SEL", 1.5m);

			var client = NewClient;
			var exconfig = client.CompanyData.AccARExchangeRateConfigurations.AddNew();
			exconfig.JCE_JobType = "ALL";
			exconfig.JCE_ServiceDirection = "ALL";
			exconfig.JCE_TransportMode = "ALL";
			exconfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			exconfig.JCE_ConfigType = "ERT";
			exconfig.JCE_ParentID = client.PK;
			exconfig.JCE_ParentTableCode = "OH";
			exconfig.JCE_Preference = "TDR";
			exconfig.JCE_Ledger = "AR";

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			clientRateEntry.TI_RX_NKCurrency = CurrencyCodes.UnitedStates;
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var cafChargeCode = Helper.ChargeCodes["CAF"];

			var frtRateLine = clientRateEntry.AddRateLine(frtChargeCode, FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 1200m;

			var cafRateLine = clientRateEntry.AddRateLine(cafChargeCode, PercentageCalculator.Code);
			cafRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			cafRateLine.GetCalculator<PercentageCalculator>().Percent = 5m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 1.2m, client.PK, ExchangeRateOrgTypeEnum.Debtor);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_RX_NKCostCurrency = CurrencyCodes.UnitedStates,
					JR_OSCostAmt = 1200m,
					JR_LocalCostAmt = 2400m, // General exchange rate is used
					JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates,
					JR_OSSellAmt = 1200m,
					JR_LocalSellAmt = 1000m, // Client specific exchange rate is used
				},
				new AssertionCharge
				{
					ChargeCode = cafChargeCode.AC_Code,
					JR_RX_NKCostCurrency = CurrencyCodes.UnitedStates,
					JR_OSCostAmt = 60m,
					JR_LocalCostAmt = 120m, // General exchange rate is used
					JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates,
					JR_OSSellAmt = 60m,
					JR_LocalSellAmt = 50m, // Client specific exchange rate is used
					RevenueCalculationDescription = "CAF: 5.00% of (USD 1200.00 (FRT))"
				}
			};

			AutorateAndAssert(expected, shipment, client, job: shipmentJob, autorateCosts: false);
		}

		#endregion

		#region Percentage Calculator and Existing/Posted Charges

		public void TestPercentageCalculatorIsNotAppliedToExistingChargeWhenChargeIsNotCleanedUp_Revenue()
		{
			var client = NewClient;
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = clientRateEntry.AddRateLine(frtChargeCode, FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 1000m;

			var bafRateLine = clientRateEntry.AddRateLine(bafChargeCode, PercentageCalculator.Code);
			bafRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			bafRateLine.GetCalculator<PercentageCalculator>().Percent = 10m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = frtChargeCode.PK;
			charge.JR_OH_SellAccount = client.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OSSellAmt = 900m; // => revenue subjects to retain

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSSellAmt = 1000m,
					JR_OSCostAmt = 1000m,
				},
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSSellAmt = 900m,
					JR_OSCostAmt = 900m,
				},
				new AssertionCharge
				{
					ChargeCode = bafChargeCode.AC_Code,
					JR_OSSellAmt = 190m,
					RevenueCalculationDescription = "BAF: 10.00% of (AUD 1900.00 (FRT* 900.00 + FRT 1000.00))"
				}
			};

			AutorateAndAssert(expected, shipment, client, job: shipmentJob, autorateCosts: false);
		}

		public void TestPercentageCalculatorIsNotAppliedToExistingChargeWhenChargeIsNotCleanedUp_Costs()
		{
			var client = NewClient;
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];

			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			costingEntry.RateLines.RemoveAndDeleteAll();

			var frtCostLine = costingEntry.AddRateLine(frtChargeCode, FlatCalculator.Code);
			frtCostLine.GetCalculator<FlatCalculator>().BaseRate = 900m;

			var bafCostLine = costingEntry.AddRateLine(bafChargeCode, PercentageCalculator.Code);
			bafCostLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			bafCostLine.GetCalculator<PercentageCalculator>().Percent = 10m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = frtChargeCode.PK;
			charge.JR_OH_CostAccount = TransportProvider1.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OSCostAmt = 1000m; // cost subjects to retain

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSSellAmt = 1000m,
					JR_OSCostAmt = 1000m,
				},
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSSellAmt = 900m,
					JR_OSCostAmt = 900m,
				},
				new AssertionCharge
				{
					ChargeCode = bafChargeCode.AC_Code,
					JR_OSCostAmt = 190.00m,
					CostCalculationDescription = "BAF: 10.00% of (AUD 1900.00 (FRT 900.00 + FRT* 1000.00))"
				}
			};

			var message = "We do not merge REA charges with no payment basis";
			AutorateAndAssert(message, expected, shipment, client, job: shipmentJob, autorateRevenue: false);
		}

		public void TestShipmentWithTransportBooking_ShipmentWithPostedCosttCharges_ShouldNotBringPostedChargesAgainForCost()
		{
			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;
			var consignee = Helper.NewOrgHeader(1);

			var tbk1ChargeCode = Helper.ChargeCodes.New("TBK1", "Transport Booking UNT", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK2", "Transport Booking PER", PercentageCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			#region Rates Setup

			var costProvider = Factory.NewWithValidTestData<OrgHeader>();
			costProvider.OH_IsCreditor = true;

			var cost = Helper.NewCosting(costProvider);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine("TBK1", FlatCalculator.Code);
			costLine.GetCalculator<FlatCalculator>().BaseRate = 100;

			var costLinePercentage = costEntry.AddRateLine("TBK2", FlatCalculator.Code);
			costLinePercentage.GetCalculator<FlatCalculator>().BaseRate = 90;

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

			var booking = bookingConsolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = "EFPU";
			booking.KM_RatingFreightMode = "LSE";
			booking.KM_JobID = "TM00000001";
			booking.Address.OrganisationPK = costProvider.PK;

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = localClient.PK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = consignee.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box = CreatePackage(booking, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet = CreatePackage(booking, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton = CreatePackage(booking, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction, box, pallet, carton);
			CreateInstructionPkgDivots(toInstruction, box, pallet, carton);

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
						JR_OSSellAmt = 100m,
						JR_OSCostAmt = 100m,
						CostCalculationDescription = "Transport Booking TM00000001"
					},
					new AssertionCharge
					{
						ChargeCode = "TBK2",
						JR_OSSellAmt = 90.0m,
						JR_OSCostAmt = 90.0m
					}
				};

				AutorateAndAssert("Percentage calculator should be applied to correct Transport Booking", expected, shipment, localClient, job: job, autorateRevenue: false);
				var jobCharges = job.Charges;

				jobCharges[1].JR_AL_APLine = tl.PK;
				jobCharges[1].JR_AT_SellGSTRate = taxRate1.PK;
				Assert("Prerequisite:", jobCharges[1].IsCostPosted);

				jobCharges[2].JR_AL_APLine = tl.PK;
				jobCharges[2].JR_AT_SellGSTRate = taxRate1.PK;
				Assert("Prerequisite:", jobCharges[2].IsCostPosted);

				AutorateAndAssert("Should not bring posted charges again for costing", expected, shipment, localClient, job: job, autorateRevenue: false);

				foreach (Charge jc in job.Charges)
				{
					AssertEquals("Posting Not Changed", true, jc.JR_IsCostPosted);
					AssertEquals("Posting Not Changed", false, jc.JR_IsRevenuePosted);
				}
			}
		}

		public void TestShipmentWithMultipleTransportBookings_ShipmentWithPostedRevenueCharges_ShouldNotBringPostedChargesAgainForRevenue()
		{
			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;
			var consignee = Helper.NewOrgHeader(1);

			Helper.ChargeCodes.New("TBK1", "Transport Booking 1 FLT", UnitCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New("TBK2", "Transport Booking 2 FLT", PercentageCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			#region Rates Setup

			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("TBK1", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 40;

			var rateLinePercentage = rateEntry.AddRateLine("TBK2", FlatCalculator.Code);
			rateLinePercentage.GetCalculator<FlatCalculator>().BaseRate = 50m;

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
			booking1.Address.OrganisationPK = localClient.PK;

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

			var box = CreatePackage(booking1, 1, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet = CreatePackage(booking1, 1, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton = CreatePackage(booking1, 1, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction1, box, pallet, carton);
			CreateInstructionPkgDivots(toInstruction1, box, pallet, carton);

			#endregion

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.LocalChargesPK = localClient.PK;
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
						JR_OSSellAmt = 40.0m,
						JR_OSCostAmt = 40.0m,
					},
					new AssertionCharge
					{
						ChargeCode = "TBK2",
						JR_OSSellAmt = 50m,
						JR_OSCostAmt = 50m,
					}
				};

				AutorateAndAssert("Percentage calculator should be applied to correct Transport Booking", expected, shipment, localClient, job: job, autorateCosts: false);

				foreach (Charge jc in job.Charges)
				{
					jc.JR_AL_ARLine = tl.PK;
					jc.JR_AT_SellGSTRate = taxRate1.PK;
					Assert("Prerequisite:", jc.IsRevenuePosted);
				}

				AutorateAndAssert("Should not bring posted charges again for revenue", expected, shipment, localClient, job: job, autorateCosts: false);

				foreach (Charge jc in job.Charges)
				{
					AssertEquals("Posting Not Changed", false, jc.JR_IsCostPosted);
					AssertEquals("Posting Not Changed", true, jc.JR_IsRevenuePosted);
				}
			}
		}

		public void TestPercentageCalculatorWithApplyToCUDAppliesToAllCDSCharges()
		{
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var testRate = Helper.NewClientRate(client);
			var entry = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var cusdsbTestChargeCode = Helper.ChargeCodes["CUSDSB_TST"];
			var dtyTestChargeCode = Helper.ChargeCodes["DTYTEST"];
			var dtyAndTaxChargeCode = Helper.ChargeCodes["DUTYANDTAX"];
			var nonCDSChargeCode = Helper.ChargeCodes["NONCDS"];

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TAX1";
			taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate1.SetRateNumerator_ForTestOnly(5);

			cusdsbTestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			cusdsbTestChargeCode.AC_ChargeType = ChargeType.Disbursement;
			cusdsbTestChargeCode.AC_AT_GSTRate = taxRate1.PK;
			dtyTestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			dtyTestChargeCode.AC_ChargeType = ChargeType.Disbursement;
			dtyTestChargeCode.AC_AT_GSTRate = taxRate1.PK;
			dtyAndTaxChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			dtyAndTaxChargeCode.AC_ChargeType = ChargeType.Disbursement;
			dtyAndTaxChargeCode.AC_AT_GSTRate = taxRate1.PK;
			nonCDSChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			nonCDSChargeCode.AC_ChargeType = ChargeType.Margin;
			nonCDSChargeCode.AC_AT_GSTRate = taxRate1.PK;

			var colChargeCode = Helper.ChargeCodes["COL"];
			colChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			var rateLine = entry.AddRateLine(colChargeCode.AC_Code, PercentageCalculator.Code);
			rateLine.GetCalculator<PercentageCalculator>().Percent = 10;
			var rateLineItem = rateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement);

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;

			var charge1 = shipmentJob.Charges.AddNew();
			charge1.JR_AC = cusdsbTestChargeCode.PK;
			charge1.JR_OH_CostAccount = TransportProvider1.PK;
			charge1.JR_OSCostAmt = 100;

			var charge2 = shipmentJob.Charges.AddNew();
			charge2.JR_AC = dtyTestChargeCode.PK;
			charge2.JR_OH_CostAccount = TransportProvider1.PK;
			charge2.JR_OSCostAmt = 100;

			var charge3 = shipmentJob.Charges.AddNew();
			charge3.JR_AC = dtyAndTaxChargeCode.PK;
			charge3.JR_OH_CostAccount = TransportProvider1.PK;
			charge3.JR_OSCostAmt = 100;

			var charge4 = shipmentJob.Charges.AddNew();
			charge4.JR_AC = nonCDSChargeCode.PK;
			charge4.JR_OH_CostAccount = TransportProvider1.PK;
			charge4.JR_OSCostAmt = 100;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = cusdsbTestChargeCode.AC_Code,
					JR_OSSellAmt = 100m,
					JR_OSCostAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = dtyTestChargeCode.AC_Code,
					JR_OSSellAmt = 100m,
					JR_OSCostAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = dtyAndTaxChargeCode.AC_Code,
					JR_OSSellAmt = 100m,
					JR_OSCostAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = nonCDSChargeCode.AC_Code,
					JR_OSSellAmt = 100m,
					JR_OSCostAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = colChargeCode.AC_Code,
					JR_OSCostAmt = 30.00m,
					RevenueCalculationDescription = "COL: 10.00% of (AUD 300.00 (Customs Disbursement CUSDSB_TST* 100.00 + DTYTEST* 100.00 + DUTYANDTAX* 100.00))"
				}
			};

			using (RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, cusdsbTestChargeCode.PK.ToGuid()))
			{
				AutorateAndAssert(expected, shipment, client, job: shipmentJob, autorateCosts: false);
			}
		}

		public void TestPercentageCalculatorShipment_WhenRatingBehaviourForExistingJobChargeIsNEW()
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var cafChargeCode = Helper.ChargeCodes["CAF"];

			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;

			var clientRate = Helper.NewClientRate(localClient);
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = clientRateEntry.AddRateLine(frtChargeCode.AC_Code, FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 230m;

			var bafRateLine = clientRateEntry.AddRateLine(bafChargeCode.AC_Code, PercentageCalculator.Code);
			bafRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			bafRateLine.GetCalculator<PercentageCalculator>().Percent = 10m;

			var cafRateLine = clientRateEntry.AddRateLine(cafChargeCode.AC_Code, FlatCalculator.Code);
			cafRateLine.GetCalculator<FlatCalculator>().BaseRate = 50m;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, localClient.PK, "AUSYD", "USLAX", 1000m);
			shipment.JS_INCO = "CFR";

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				shipmentJob.LocalChargesPK = localClient.PK;
				var charge = shipmentJob.Charges.AddNew();
				charge.JR_AC = frtChargeCode.PK;
				charge.JR_OH_SellAccount = localClient.PK;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_OSSellAmt = 900m;
				charge.JR_OSCostAmt = 900m;
				charge.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 900m,
					},
						new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 230m,
					},
					new AssertionCharge
					{
						ChargeCode = "CAF",
						JR_OSSellAmt = 50m,
					},
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSSellAmt = 113m,
						RevenueCalculationDescription = "BAF: 10.00% of (AUD 1130.00 (FRT 230.00 + FRT* 900.00))"
					}
				};

				AutorateAndAssert(expected, shipment, localClient, job: shipmentJob, autorateCosts: false);
			}
		}

		public void TestPercentageCalculatorShipment_WhenRatingBehaviourForExistingJobChargeIsREA()
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var cafChargeCode = Helper.ChargeCodes["CAF"];

			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;

			var clientRate = Helper.NewClientRate(localClient);
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = clientRateEntry.AddRateLine(frtChargeCode.AC_Code, FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 230m;

			var bafRateLine = clientRateEntry.AddRateLine(bafChargeCode.AC_Code, PercentageCalculator.Code);
			bafRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			bafRateLine.GetCalculator<PercentageCalculator>().Percent = 10m;

			var cafRateLine = clientRateEntry.AddRateLine(cafChargeCode.AC_Code, FlatCalculator.Code);
			cafRateLine.GetCalculator<FlatCalculator>().BaseRate = 50m;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, localClient.PK, "AUSYD", "USLAX", 1000m);
			shipment.JS_INCO = "CFR";

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				shipmentJob.LocalChargesPK = localClient.PK;
				var charge = shipmentJob.Charges.AddNew();
				charge.JR_AC = frtChargeCode.PK;
				charge.JR_OH_SellAccount = localClient.PK;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.ApplyCustomQuickCalculator(shipment.RatingAdapter, 900m, 900m);
				charge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 230m
					},
					new AssertionCharge
					{
						ChargeCode = "CAF",
						JR_OSSellAmt = 50m
					},
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSSellAmt = 23m,
						RevenueCalculationDescription = "BAF: 10.00% of (AUD 230.00 (FRT))"
					}
				};

				AutorateAndAssert(expected, shipment, localClient, job: shipmentJob, autorateCosts: false);
			}
		}

		public void TestPercentageCalculatorShipment_WhenRatingBehaviourForExistingJobChargeIsSTP()
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var cafChargeCode = Helper.ChargeCodes["CAF"];

			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;

			var clientRate = Helper.NewClientRate(localClient);
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = clientRateEntry.AddRateLine(frtChargeCode.AC_Code, FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 230m;

			var bafRateLine = clientRateEntry.AddRateLine(bafChargeCode.AC_Code, PercentageCalculator.Code);
			bafRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			bafRateLine.GetCalculator<PercentageCalculator>().Percent = 10m;

			var cafRateLine = clientRateEntry.AddRateLine(cafChargeCode.AC_Code, FlatCalculator.Code);
			cafRateLine.GetCalculator<FlatCalculator>().BaseRate = 50m;

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, localClient.PK, "AUSYD", "USLAX", 1000m);
			shipment.JS_INCO = "CFR";

			Factory.Save();

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				shipmentJob.LocalChargesPK = localClient.PK;

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "TAX1";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRateNumerator_ForTestOnly(5);

				var charge = shipmentJob.Charges.AddNew();
				charge.JR_AC = frtChargeCode.PK;
				charge.JR_OH_SellAccount = localClient.PK;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_OSSellAmt = 900m;
				charge.JR_OSCostAmt = 900m;
				charge.JR_OH_SellAccount = localClient.PK;
				charge.JR_SellRatingOverride = false;

				var transactionHeader = Factory.New<AccTransactionHeader>();
				transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
				var transactionLine = (ARInvoiceLine)Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
				transactionLine.AL_AH = transactionHeader.PK;
				transactionLine.AL_LineType = TransactionLineTypes.Revenue;
				transactionLine.AL_LineAmount = -900;
				transactionLine.AL_OSAmount = -900;
				transactionLine.AL_RX_NKTransactionCurrency = "AUD";
				transactionLine.AL_RevRecognitionType = "IMM";

				charge.JR_AL_ARLine = transactionLine.PK;
				charge.JR_AT_CostGSTRate = taxRate.PK;

				Assert(charge.IsRevenuePosted);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 900m,
					},
					new AssertionCharge
					{
						ChargeCode = "CAF",
						JR_OSSellAmt = 50m,
					},
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSSellAmt = 90m,
						RevenueCalculationDescription = "BAF: 10.00% of (AUD 900.00 (FRT*))"
					}
				};

				AutorateAndAssert(expected, shipment, localClient, job: shipmentJob, autorateCosts: false);
			}
		}

		#endregion

		#region PercentageCalculatorsGST

		public void TestPercentageCalculatorHonoursGSTOverrides()
		{
			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());
			RateEntry entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)entry.RateLines[0].Calculator).BaseRate = 1000;

			RateLine line2 = entry.AddRateLine("BAF", PercentageCalculator.Code);
			line2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			line2.GetCalculator<PercentageCalculator>().Percent = 5m;
			line2.GetCalculator<PercentageCalculator>().IncludeGST = true;

			RateLine line3 = entry.AddRateLine("CAF", PercentageCalculator.Code);
			line3.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			line3.GetCalculator<PercentageCalculator>().Percent = 10m;
			line3.GetCalculator<PercentageCalculator>().IncludeGST = true;

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TAX1";
			taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate1.SetRateNumerator_ForTestOnly(5);

			AccChargeCode chargeCode = entry.RateLines[0].ChargeCode;
			chargeCode.AC_AT_GSTRate = taxRate1.PK;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Code = "TAX2";
			taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate2.SetRateNumerator_ForTestOnly(15);

			AccChargeTaxOverride override1 = chargeCode.TaxOverrides.AddNew();
			override1.AO_CostSellAll = "REV";
			override1.AO_Direction = "ALL";
			override1.AO_IncoTerm = "FOB";
			override1.AO_JobType = "ALL";
			override1.AO_Origin = "ALL";
			override1.AO_Destination = "ALL";
			override1.AO_TaxRegCntryOrGroup = "ALL";
			override1.AO_AT = taxRate2.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";
			shipment.CustomsEntryNumberType = "T1";

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = rate.Header.MainAddress.PK;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			var expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 1000m,
							InvoiceLineDesc = "International Freight",
							CalculationSingleLineDescription = "FRT: Base Rate USD 1000.00"
						},
					new SimpleArInfo
						{
							Amount = 57.50m,
							InvoiceLineDesc = "Bunker Adjustment Factor",
							CalculationDescription = "BAF: 5.00% of (USD 1150.00 (FRT))"
						},
					new SimpleArInfo
						{
							Amount = 120.75m,
							InvoiceLineDesc = "Currency Adjustment Factor",
							CalculationDescription = "CAF: 10.00% of (USD 1207.50 (Freight Charges BAF 57.50 + FRT 1150.00))"
						}
				};

			AssertRatingResults(expected, results);
		}

		public void TestGSTCalculationsOnExistingCharges()
		{
			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());
			RateEntry entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			RateLine line2 = entry.AddRateLine("BAF", PercentageCalculator.Code);
			line2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			line2.GetCalculator<PercentageCalculator>().Percent = 5m;
			line2.GetCalculator<PercentageCalculator>().IncludeGST = true;

			RateLine line3 = entry.AddRateLine("CAF", PercentageCalculator.Code);
			line3.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			line3.GetCalculator<PercentageCalculator>().Percent = 10m;
			line3.GetCalculator<PercentageCalculator>().IncludeGST = true;

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TAX1";
			taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate1.SetRateNumerator_ForTestOnly(5);

			AccChargeCode chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_AT_GSTRate = taxRate1.PK;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Code = "TAX2";
			taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate2.SetRateNumerator_ForTestOnly(15);

			AccChargeTaxOverride override1 = chargeCode.TaxOverrides.AddNew();
			override1.AO_CostSellAll = "REV";
			override1.AO_Direction = "ALL";
			override1.AO_IncoTerm = "FOB";
			override1.AO_JobType = "ALL";
			override1.AO_Origin = "ALL";
			override1.AO_Destination = "ALL";
			override1.AO_TaxRegCntryOrGroup = "ALL";
			override1.AO_AT = taxRate2.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";
			shipment.CustomsEntryNumberType = "T1";

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = rate.Header.MainAddress.PK;

			JobCharge charge = testJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OH_SellAccount = rate.Header.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 1000m;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			var expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 52.5m,
							InvoiceLineDesc = "Bunker Adjustment Factor",
							CalculationDescription = "BAF: 5.00% of (USD 1050.00 (FRT*))"
						},
					new SimpleArInfo
						{
							Amount = 110.25m,
							InvoiceLineDesc = "Currency Adjustment Factor",
							CalculationDescription = "CAF: 10.00% of (USD 1102.50 (Freight Charges BAF 52.50 + FRT* 1050.00))"
						}
				};

			AssertRatingResults(expected, results);
		}

		#endregion

		#region Profit Share / Rebate Calculator

		public void TestProfitShareRebateCalculator_AllCharges()
		{
			var flatChargeCode = Helper.ChargeCodes["FRT"];
			var percentageChargeCode = Helper.ChargeCodes["BAF"];
			var rebateChargeCode = Helper.ChargeCodes["CAF"];
			var manualChargeCode = Helper.ChargeCodes["WAR"];

			//Set up Costs
			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");
			costEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(flatChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			//Set up Revenue
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntry.RateLines.RemoveAndDeleteAll();

			var frtLine = rateEntry.AddRateLine(flatChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 15m;

			var percentageCalculator = rateEntry.AddRateLine(percentageChargeCode, PercentageCalculator.Code).GetCalculator<PercentageCalculator>();
			percentageCalculator.Percent = 10m;
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = flatChargeCode.PK;

			var rebateCalculator = rateEntry.AddRateLine(rebateChargeCode, ProfitShareRebateCalculator.Code).GetCalculator<ProfitShareRebateCalculator>();
			rebateCalculator.AddApplyToItem(CalculatorConstants.Text.AllCharges);
			rebateCalculator.Percent = 10m;
			rebateCalculator.BaseRate = 10m;

			//Set up Job with Charge
			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, NewClient.PK, "JPOSA", "AUSYD", 100m);
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			job.LocalChargesPK = NewClient.PK;

			Factory.Save();

			var postedCharge = job.Charges.AddNew();
			postedCharge.JR_AC = manualChargeCode.PK;
			postedCharge.JR_LocalCostAmt = 22m;
			postedCharge.JR_LocalSellAmt = 77m;
			PostRevenue(postedCharge);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = flatChargeCode.AC_Code,
					JR_LocalCostAmt = 1000m,
					JR_LocalSellAmt = 1500m,
					CostCalculationDescription = "100 Kilogram(s) @ AUD 10.00/KG",
					RevenueCalculationDescription = "100 Kilogram(s) @ AUD 15.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = percentageChargeCode.AC_Code,
					JR_LocalCostAmt = 150m,
					JR_LocalSellAmt = 150m,
					RevenueCalculationDescription = "10.00% of (AUD 1500.00 (FRT))"
				},
				new AssertionCharge
				{
					ChargeCode = manualChargeCode.AC_Code,
					JR_LocalCostAmt = 22m,
					JR_LocalSellAmt = 77m,
				},
				new AssertionCharge
				{
					ChargeCode = rebateChargeCode.AC_Code,
					JR_LocalSellAmt = 65.5m,
					RevenueCalculationDescription = "CAF: Base Rate AUD 10.00 + 10.00% of (AUD 555.00 (All Charge Codes profit))"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, job: job);
		}

		public void TestProfitShareRebateCalculator_AppliesToMargin()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddYears(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddYears(1);
			exchangeRate.RE_SellRate = 2m;
			exchangeRate.RE_RX_NKExCurrency = "NZD";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			var chargeCode = Helper.ChargeCodes.New("DSTMRG", "Margin Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			chargeCode.AC_MarginPercentage = 75m;
			chargeCode.Factory.Save();

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "", "AU");
			rateEntry.AddRateLine(chargeCode, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100m;

			var line = rateEntry.AddRateLine("DDOC", ProfitShareRebateCalculator.Code);
			line.TL_RX_NKCurrency = "NZD";
			var profitShareRebateCalculator = line.GetCalculator<ProfitShareRebateCalculator>();
			profitShareRebateCalculator.Percent = 10m;
			profitShareRebateCalculator.BaseRate = 5m;
			profitShareRebateCalculator.AddApplyToItem(CalculatorConstants.Text.AllCharges);

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "JPOSA", "AUSYD", 100m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DSTMRG",
					JR_LocalCostAmt = 75m,
					JR_LocalSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalCostAmt = 0m,
					JR_OSCostAmt = 0m,
					JR_LocalSellAmt = 5m,
					JR_OSSellAmt = 10m,
					JR_RX_NKSellCurrency = "NZD",
					RevenueCalculationDescription = "DDOC: Base Rate NZD 5.00 + 10.00% of (NZD 50.00 (All Charge Codes profit 25.00 AUD))"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee);
		}

		public void TestProfitShareRebateCalculator_ByChargeGroup()
		{
			var profitShareChargeCode = Helper.ChargeCodes["BAF"];

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var profitShareRebateCalculator = rateEntry.AddRateLine(profitShareChargeCode, ProfitShareRebateCalculator.Code).GetCalculator<ProfitShareRebateCalculator>();
			profitShareRebateCalculator.Percent = 10m;
			profitShareRebateCalculator.AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			profitShareRebateCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["ODOC"].PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, NewClient.PK, "JPOSA", "AUSYD", 100m);
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			job.LocalChargesPK = NewClient.PK;

			Factory.Save();

			var taxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", "RAT", 0);
			void AddCharge(string chargeCode, decimal cost, decimal revenue)
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = Helper.ChargeCodes[chargeCode].PK;
				charge.JR_LocalCostAmt = cost;
				charge.JR_AgentDeclaredCostAmtLocal = cost;
				charge.JR_LocalSellAmt = revenue;
				charge.JR_AgentDeclaredSellAmtLocal = revenue;
				charge.JR_AT_SellGSTRate = taxRate.PK;
			}

			AddCharge("FRT", 1m, 2m);
			AddCharge("ODOC", 33.3m, 55.5m);
			AddCharge("OCART", 300m, 500m);
			AddCharge("DDOC", 11.1m, 22.2m);
			AddCharge("FRT", 100m, 200m);

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT" },
				new AssertionCharge { ChargeCode = "FRT" },
				new AssertionCharge { ChargeCode = "DDOC" },
				new AssertionCharge { ChargeCode = "ODOC" },
				new AssertionCharge { ChargeCode = "OCART" },
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_LocalSellAmt = 12.4m,
					RevenueCalculationDescription = "BAF: 10.00% of (AUD 124.00 (Freight Charges + ODOC profit))"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, job: job);
		}

		public void TestProfitShareRebateCalculator_NegativeCalculation()
		{
			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AU", "DDOC", 50);

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AU", "DDOC", 150);
			var profitShareRebateCalculator = rateEntry.AddRateLine("DCART", ProfitShareRebateCalculator.Code).GetCalculator<ProfitShareRebateCalculator>();
			profitShareRebateCalculator.AddApplyToItem(CalculatorConstants.Text.AllCharges);
			profitShareRebateCalculator.Percent = -25m;
			profitShareRebateCalculator.ZeroWhenLoss = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "JPOSA", "AUSYD", 100m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalCostAmt = 50m,
					JR_LocalSellAmt = 150m
				},
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_LocalSellAmt = -25m,
					RevenueCalculationDescription = "DCART: -25.00% of (AUD 100.00 (All Charge Codes profit))"
				},
			};

			var message = "Zero when Loss ONLY applies to when the Job is a Loss. It does not apply a negative percentage makes the result negative.";
			//better would of course be to have a Consol and a Creditor who can do self-billing
			var expectedErrors = @"Error EBM22Q33TU475BXH3P60 has encountered the following errors while AutoRating:
	•  Overseas Cost Amount: A negative cost can only be entered if you specify an AP Invoice Date and Invoice Number or if the Creditor is setup to issue self billing invoices (Organization -> A/P -> Configuration -> Issue Self Billing Invoice).
	Note that negative accruals are not created.";
			AutorateAndAssert(message, expected, shipment, Consignee, expectedErrors: new[] { expectedErrors });
		}

		public void TestProfitShareRebateCalculator_PrefersAgentCostSell()
		{
			var profitShareChargeCode = Helper.ChargeCodes["BAF"];

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var profitShareRebateCalculator = rateEntry.AddRateLine(profitShareChargeCode, ProfitShareRebateCalculator.Code).GetCalculator<ProfitShareRebateCalculator>();
			profitShareRebateCalculator.Percent = 10m;
			profitShareRebateCalculator.AddApplyToItem(CalculatorConstants.Text.AllCharges);

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, NewClient.PK, "JPOSA", "AUSYD", 100m);
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			job.LocalChargesPK = NewClient.PK;

			Factory.Save();

			var taxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", "RAT", 0);

			var charge = job.Charges.AddNew();
			charge.JR_AC = Helper.ChargeCodes["FRT"].PK;
			charge.JR_LocalSellAmt = 40m;
			charge.JR_AgentDeclaredSellAmtLocal = 33m;
			charge.JR_LocalCostAmt = 20m;
			charge.JR_AgentDeclaredCostAmtLocal = 11m;
			charge.JR_AT_SellGSTRate = taxRate.PK;

			AssertEquals("Pre-Condition", 11m, charge.JR_AgentDeclaredCostAmtLocal);
			AssertEquals("Pre-Condition", 33m, charge.JR_AgentDeclaredSellAmtLocal);

			var expected = new[]
			{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 20m,
						JR_LocalSellAmt = 40m,
					},
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_LocalSellAmt = 2.20m,
						RevenueCalculationDescription = "BAF: 10.00% of (AUD 22.00 (All Charge Codes profit))"
					},
				};

			AutorateAndAssert(expected, shipment, Consignee, job: job);
		}

		public void TestProfitShareRebateCalculator_ZeroWhenLoss()
		{
			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AU", "DDOC", 100);

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AU", "DDOC", 80);
			var profitShareRebateCalculator = rateEntry.AddRateLine("DCART", ProfitShareRebateCalculator.Code).GetCalculator<ProfitShareRebateCalculator>();
			profitShareRebateCalculator.AddApplyToItem(CalculatorConstants.Text.AllCharges);
			profitShareRebateCalculator.Percent = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "JPOSA", "AUSYD", 100m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalCostAmt = 100m,
					JR_LocalSellAmt = 80m
				},
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_LocalSellAmt = -2m,
					RevenueCalculationDescription = "DCART: 10.00% of (AUD -20.00 (All Charge Codes profit))"
				},
			};

			var expectedErrors = @"Error EBM22Q33TU475BXH3P60 has encountered the following errors while AutoRating:
	•  Overseas Cost Amount: A negative cost can only be entered if you specify an AP Invoice Date and Invoice Number or if the Creditor is setup to issue self billing invoices (Organization -> A/P -> Configuration -> Issue Self Billing Invoice).
	Note that negative accruals are not created.";

			AutorateAndAssert(expected, shipment, Consignee, expectedErrors: new[] { expectedErrors });

			profitShareRebateCalculator.ZeroWhenLoss = true;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalCostAmt = 100m,
					JR_LocalSellAmt = 80m
				},
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_LocalSellAmt = 0m,
					RevenueCalculationDescription = "DCART: 10.00% of (AUD 0.00 (All Charge Codes Zero When Loss))"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee);
		}

		#endregion

		#region Autorate for a shipment with equal consignee and consignor but different local client and agent

		public void TestSameConsignorandConsigneeButDifferentLocalClientAndAgent()
		{
			OrgHeader consignorConsignee = Helper.NewOrgHeader();
			OrgHeader localClient = Helper.NewOrgHeader();
			OrgHeader agent = Helper.NewOrgHeader();

			ClientRate rate = Helper.NewClientRate(consignorConsignee);
			RateEntry entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			entry.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = consignorConsignee.PK;
			shipment.ConsignorPK = consignorConsignee.PK;
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 0.0m;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = rate.Header.MainAddress.PK;
			testJob.LocalChargesPK = localClient.PK;
			testJob.AgentCollectPK = agent.PK;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var result = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			var autoRateInfo = result.RateInfoCollection[0];
			AssertEquals((ZDecimal)1, autoRateInfo.Bases.FirstOrDefault(x => !x.Chargeable.IsEmpty).Chargeable.Amount);
		}

		#endregion

		#region Autorate for a shipment with results should not show not-found message while autorate again for additional results

		public void TestAutoRateResultNotFoundMessage()
		{
			var frtQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			frtQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var frtChargeCode = Factory.LoadTop1<AccChargeCode>(frtQuery);

			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry("AIR", "LSE", "AU", "ZA");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine("FRT", FlatCalculator.Code);
			tariffLine.TL_RateCalculator = FlatCalculator.Code;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";

			shipment.ConsignorPK = localClient.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ZAJNB";

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = frtChargeCode.PK;

			Factory.Save();

			var interactor = new TestInteractor();
			var starter = new AutoRatingStarter(shipment, interactor);
			starter.ExecuteAutorating(AutoRateOptions.AutorateRevenue);

			Assert("Should not show unable to find message when autorate again.", !interactor.Information.Any(info => info.Contains("Autorating was unable to find any rates that match this job.")));
		}

		#endregion

		#region Shipment Only Pulls Current Company Declaration

		[GuiTest]
		public void TestShipmentOnlyPullsCurrentCompanyDeclaration()
		{
			OrgHeader consignee = Helper.NewOrgHeader();

			Helper.ChargeCodes.New("AGN", "Agency Reimbursements", FlatCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerage);

			ClientRate rate = Helper.NewClientRate(consignee);
			RateEntry entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL", "", "");
			entry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)entry.RateLines[0].Calculator).BaseRate = 1000;
			entry.RateLines[0].TL_RX_NKCurrency = "AUD";

			RateLine line2 = entry.AddRateLine("AGN", FlatCalculator.Code);
			line2.TL_RateCalculator = FlatCalculator.Code;
			line2.GetCalculator<FlatCalculator>().BaseRate = 800;
			line2.TL_RX_NKCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_INCO = "EXW";

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = "EXP";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_TransportMode = TransportModes.Air;
			declaration.JE_ContainerMode = ContainerModes.Loose;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.ImporterDeliveryAddress.OrganisationPK = consignee.PK;
			declaration.JE_GB = Env.CurrentBranch.PK;

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = consignee.MainAddress.PK;

			Factory.Save();

			using (var ip = new InvoicingPluginToFreight(shipment))
			{
				ip.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertEquals(2, testJob.Charges.Count);
			}

			testJob.Charges.RemoveAndDeleteAll();

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_INCO = "EXW";

			declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_TransportMode = TransportModes.Air;
			declaration.JE_ContainerMode = ContainerModes.Loose;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.JE_JS = shipment.PK;
			declaration.ImporterDeliveryAddress.OrganisationPK = consignee.PK;

			var query = new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentBranch.PK);
			query.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);

			declaration.JE_GB = Factory.LoadTop1<GlbBranch>(query).PK;

			Env.Registry.Rating.SetOriginBrokerageRatedCodes("OBR,OBO,BRK");
			Env.Registry.Rating.SetBrokerageRatedCodes("BRK,BON,CDS,FRT,DST");
			Env.Registry.Rating.SetFreightRatedCodes("ORG,LOD,UNL,DST,INS,FRT");

			testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = consignee.MainAddress.PK;

			Factory.Save();

			using (var ip = new InvoicingPluginToFreight(shipment))
			{
				ip.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertEquals(1, testJob.Charges.Count);
			}
		}

		#endregion

		#region Several Costs Apply For One CTB Calculator

		public void TestSeveralCostsApplyForOneCTBCalculator()
		{
			var consignor = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(2);

			Costing costing1 = Helper.NewCosting(TransportProvider1);

			RateEntry costEntry11 = costing1.AddRateEntry("AIR", "LSE", "CNSHA", "USLAX");
			var costLine11 = costEntry11.RateLines[0];
			costLine11.TL_RateCalculator = UnitCalculator.Code;
			costLine11.GetCalculator<UnitCalculator>().PerUnit = 5m;

			RateEntry costEntry12 = costing1.AddRateEntry("ORG", "AIR", "CNSHA", "USLAX");
			var costLine12 = costEntry12.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)costLine12.Calculator).BaseRate = 100;

			Costing costing2 = Helper.NewCosting(TransportProvider2);

			RateEntry costEntry21 = costing2.AddRateEntry("AIR", "LSE", "CNSHA", "USLAX");
			var costLine21 = costEntry21.RateLines[0];
			costLine21.TL_RateCalculator = UnitCalculator.Code;
			costLine21.GetCalculator<UnitCalculator>().PerUnit = 7m;

			RateEntry costEntry22 = costing2.AddRateEntry("ORG", "AIR", "CNSHA", "USLAX");
			var costLine22 = costEntry22.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)costLine22.Calculator).BaseRate = 150;

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry1 = tariff.AddRateEntry("AIR", "LSE", "CNSHA", "USLAX");
			var tariffLine1 = tariffEntry1.RateLines[0];
			tariffLine1.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var tariffEntry2 = tariff.AddRateEntry("ORG", "AIR", "CNSHA", "USLAX");
			var tariffLine2 = tariffEntry2.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 15m;

			ClientRate rate = Helper.NewClientRate(consignor);
			var entry = rate.AddRateEntry("ORG", "AIR", "CNSHA", "USLAX");
			var rateLine = entry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 20m;

			costLine11.TL_RX_NKCurrency = "AUD";
			costLine12.TL_RX_NKCurrency = "AUD";
			costLine21.TL_RX_NKCurrency = "AUD";
			costLine22.TL_RX_NKCurrency = "AUD";
			tariffLine1.TL_RX_NKCurrency = "AUD";
			tariffLine2.TL_RX_NKCurrency = "AUD";
			rateLine.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 2.0m;

			var consol = shipment.Consols.AddNew();
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = TransportProvider2.MainAddress.PK;
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 5500m,
					RevenueCalculationDescription = @"FRT: 1000 Kilogram(s) @ AUD 5.50/KG
Based On:
	TRASPROV1 (Consol C00001000 --> Creditor/Co-Loader) cost",
					JR_OSCostAmt = 5000m,
					CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 5.00/KG",
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 138m,
					RevenueCalculationDescription = @"ODOC: 120.00% of (115.00% of (Base Rate AUD 100.00))
Based On:
	Company Tariff Level 1 (Linked to: TESTORG1)
	Based On:
		TRASPROV1 (Consol C00001000 --> Creditor/Co-Loader) cost",
					JR_OSCostAmt = 100m,
					CostCalculationDescription = "ODOC: Base Rate AUD 100.00",
				},
			};

			AutorateAndAssert(expected, shipment, consignor);

			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = TransportProvider2.MainAddress.PK;

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7700m,
					RevenueCalculationDescription = @"FRT: 1000 Kilogram(s) @ AUD 7.70/KG
Based On:
	TRASPROV2 (Consol C00001000 --> Creditor/Co-Loader) cost",
					JR_OSCostAmt = 7000m,
					CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 7.00/KG",
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 207m,
					RevenueCalculationDescription = @"ODOC: 120.00% of (115.00% of (Base Rate AUD 150.00))
Based On:
	Company Tariff Level 1 (Linked to: TESTORG1)
	Based On:
		TRASPROV2 (Consol C00001000 --> Creditor/Co-Loader) cost",
					JR_OSCostAmt = 150m,
					CostCalculationDescription = "ODOC: Base Rate AUD 150.00",
				},
			};

			AutorateAndAssert(expected, shipment, consignor);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = TransportProvider2.MainAddress.PK;

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 5500m,
					RevenueCalculationDescription = @"FRT: 1000 Kilogram(s) @ AUD 5.50/KG
Based On:
	TRASPROV1 (Consol C00001000 --> Carrier) cost",
					JR_OSCostAmt = 5000m,
					CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 5.00/KG",
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 207m,
					RevenueCalculationDescription = "ODOC: 120.00% of (115.00% of (Base Rate AUD 150.00))",
					JR_OSCostAmt = 150m,
					CostCalculationDescription = "ODOC: Base Rate AUD 150.00"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 138m,
					RevenueCalculationDescription = @"ODOC: 120.00% of (115.00% of (Base Rate AUD 100.00))
Based On:
	Company Tariff Level 1 (Linked to: TESTORG1)",
					JR_OSCostAmt = 100m,
					CostCalculationDescription = "ODOC: Base Rate AUD 100.00",
				},
			};

			AutorateAndAssert(expected, shipment, consignor);
		}

		#endregion

		#region Audit Log Warnings

		[TestDate(2016, 02, 10)]
		public void TestShowRatesAdditionResult_InformationAddedWhenRatesAreFound()
		{
			var cost = Helper.NewCosting(null);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "CN", "AU");
			costEntry.AddRateLine("DDOC", UnitCalculator.Code, Weight.Kilograms).GetCalculator<UnitCalculator>().PerUnit = 5;

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "CN", "AU");
			rateEntry.AddRateLine("DAQF", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 15;

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, client.PK, "CNSHA", "AUSYD", 21m);
			shipment.JS_UniqueConsignRef = "S00001312";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 15m,
							ChargeCode = "DAQF"
						}
				};

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Rate was found so message should be information", "Information: Shipment S00001312 was auto-rated.");

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSCostAmt = 105m,
							ChargeCode = "DDOC"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 15m,
							ChargeCode = "DAQF"
						}
				};

			AutorateAndAssert(expected, shipment, client);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Rate was found so message type is info", "Information: Shipment S00001312 was auto-rated.");
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Cost was found so message type is info", "Information: Shipment S00001312 was auto-costed.");
		}

		[TestDate(2016, 02, 10)]
		public void TestShowRatesAdditionResult_WarningAddedWhenNoRatesFound()
		{
			var client = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, client.PK, "CNSHA", "AUSYD", 21m);
			Factory.Save();

			var results = Array.Empty<AssertionCharge>();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutorateAndAssert("No costs or rates have been set", results, shipment, client);
			}

			var errorMessage = @"User:				CargoWise Support
Time:				10-Feb-16 00:00

Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription
Information: Resetting previously auto-rated charges for Shipment EBM22Q33TU475BXH3P60
Information: AUTORATING COSTS FOR Shipment EBM22Q33TU475BXH3P60
Information: CHARGES CALCULATED:
Warning: Shipment EBM22Q33TU475BXH3P60 was auto-costed.
	No costs were found.
Information: AUTORATING REVENUE FOR Shipment EBM22Q33TU475BXH3P60
Information: CHARGES CALCULATED:
Warning: Shipment EBM22Q33TU475BXH3P60 was auto-rated.
	No rates were found.
Information: AUTORATING PROFIT SHARE FOR Shipment EBM22Q33TU475BXH3P60
Information: CHARGES CALCULATED:";

			AssertAutoratingAuditLogNote(shipment, errorMessage, "No rate or costs were found so message type is warning");
		}

		#endregion

		#region Multiple Costs Audit Log

		public void TestMultipleCostsCorrectlyShownOnAuditLog()
		{
			var transportCompany = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(transportCompany);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AUSYD");

			var costLine1 = costEntry.AddRateLine("DDOC", MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			var calc1 = costLine1.GetCalculator<MinimumOrPerUnitCalculator>();
			calc1.Minimum = 25m;
			calc1.PerUnit = 1m;

			var costLine2 = costEntry.AddRateLine("DDOC", MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			var calc2 = costLine2.GetCalculator<MinimumOrPerUnitCalculator>();
			calc2.Minimum = 2m;
			calc2.PerUnit = 0.01m;

			var costLine3 = costEntry.AddRateLine("DDOC", MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			var calc3 = costLine3.GetCalculator<MinimumOrPerUnitCalculator>();
			calc3.Minimum = 25m;
			calc3.PerUnit = 0.3m;

			var costLine4 = costEntry.AddRateLine("DDOC", FlatCalculator.Code);
			costLine4.GetCalculator<FlatCalculator>().BaseRate = 35m;

			var costLine5 = costEntry.AddRateLine("DDOC", FlatCalculator.Code);
			costLine5.GetCalculator<FlatCalculator>().BaseRate = 50m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "DESTR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "DESTR";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.JS_ActualWeight = 5m;
			shipment.JS_ActualVolume = 0.029m;

			shipment.JS_OH_ImportBroker = transportCompany.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSCostAmt = 137m,
							ChargeCode = "DDOC",
							CostCalculationDescription = @"This charge is calculated from multiple rates

DDOC: Minimum AUD 2.00"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region BiggestMinimumUsed

		public void TestBiggestMinimumUsed()
		{
			var org = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(org);

			var rateEntry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.PK);
			rateLine1.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 800m;
			rateLine1.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 5m;

			var rateEntry2 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			rateEntry2.TI_RH_NKCommodityCode = "HAZ";
			rateEntry2.RateLines.RemoveAndDeleteAll();

			var rateLine2 = rateEntry2.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.PK);
			rateLine2.GetCalculator<CombinedCalculator>().Minimum = 1700m;
			rateLine2.GetCalculator<CombinedCalculator>().PerUnit = 6m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "KRSEL";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "KRSEL";
			shipment.JS_INCO = IncoTerms.CostAndFreight;

			var line1 = shipment.OuterPackLines.AddNew();
			var line2 = shipment.OuterPackLines.AddNew();
			var line3 = shipment.OuterPackLines.AddNew();

			line1.JL_RH_NKCommodityCode = "GEN";
			line2.JL_RH_NKCommodityCode = "HAZ";
			line3.JL_RH_NKCommodityCode = "GEN";

			line1.JL_PackageCount = 100;
			line2.JL_PackageCount = 100;
			line3.JL_PackageCount = 100;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 1700m,
					ChargeCode = "FRT",
					RevenueCalculationDescription = "FRT: Minimum AUD 1700.00"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 1000m,
					ChargeCode = "FRT",
					RevenueCalculationDescription = "FRT: 200 Package(s) @ AUD 5.00/Package"
				}
			};

			AutorateAndAssert(expected, shipment, org);

			rateLine2.GetCalculator<CombinedCalculator>().Minimum = 1500m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 1500m,
					ChargeCode = "FRT",
					RevenueCalculationDescription = "FRT: Minimum AUD 1500.00"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 1000m,
					ChargeCode = "FRT",
					RevenueCalculationDescription = "FRT: 200 Package(s) @ AUD 5.00/Package"
				}
			};

			AutorateAndAssert(expected, shipment, org);

			rateLine2.GetCalculator<CombinedCalculator>().Maximum = 550m;
			rateLine2.GetCalculator<CombinedCalculator>().Minimum = 0m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 550m,
					ChargeCode = "FRT",
					RevenueCalculationDescription = "FRT: Maximum AUD 550.00"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 1000m,
					ChargeCode = "FRT",
					RevenueCalculationDescription = "FRT: 200 Package(s) @ AUD 5.00/Package"
				}
			};

			AutorateAndAssert(expected, shipment, org);
		}

		#endregion

		#region TransportProvidersByChargeCodeGroup

		public void TestTransportProvidersByChargeCodeGroup()
		{
			var exportCFS = Helper.CreateCreditor("EXPCFS");
			var creditor = Helper.CreateCreditor();
			var importCFS = Helper.CreateCreditor("IMPCFS");
			var client = Helper.NewOrgHeader(1);

			var costing1 = Helper.NewCosting(exportCFS);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "CN", "AU", "ODOC", 100m, CurrencyCodes.Australia);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "CN", "AU", "DDOC", 120m, CurrencyCodes.Australia);

			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "CN", "AU", "ODOC", 200m, CurrencyCodes.Australia);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "CN", "AU", "DDOC", 240m, CurrencyCodes.Australia);

			var costing3 = Helper.NewCosting(importCFS);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "CN", "AU", "ODOC", 300m, CurrencyCodes.Australia);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "CN", "AU", "DDOC", 360m, CurrencyCodes.Australia);

			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, client.PK, "CNSHA", "AUFRE", 100m);
			shipment.JS_UniqueConsignRef = "S00001111";
			shipment.JS_INCO = ZString.Empty;
			shipment.JS_OA_ExportReceivingDepot = exportCFS.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = importCFS.MainAddress.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00002222";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.JK_PrepaidCollect = "CCX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = creditor.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;
			container1.PackLines.Add(shipment.OuterPackLines.AddNew());

			Factory.Save();

			var message = "Different Service Providers apply per charge code group";
			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 200m,
							CostCalculationDescription = "CREDITOR (Consol C00002222 --> Creditor/Co-Loader) cost"
						},
					new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_OSCostAmt = 240m,
							CostCalculationDescription = "CREDITOR (Consol C00002222 --> Creditor/Co-Loader) cost"
						},
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 100m,
							CostCalculationDescription = "EXPCFS (Shipment S00001111 --> Pickup CFS Address) cost"
						},
					new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_OSCostAmt = 360m,
							CostCalculationDescription = "(Shipment S00001111 --> Delivery CFS Address) cost"
						},
				};

			AutorateAndAssert(message, expected, shipment, client);
		}

		#endregion

		#region TransportProvidersSpecificCostsVsTransportProviderOwnCosts

		public void TestCarrierCostsVsCarrierSpecificCosts()
		{
			var org = Helper.NewOrgHeader(1);
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = costing.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "CNSHA", "AUFRE", "", "20GP");
			costEntry1.RateLines.RemoveAndDeleteAll();

			var costLine1 = costEntry1.AddRateLine(frtChargeCode, UnitCalculator.Code, QuantityUnit.CN, currencyCode: CurrencyCodes.Australia);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUFRE";
			shipment.JS_INCO = IncoTerms.CostAndFreight;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = TransportProvider1.PK;
			consol.CreditorPK = TransportProvider1.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = frtChargeCode.AC_Code,
							JR_OSCostAmt = 2000m,
							CostCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 1000.00/Container"
						},
				};

			AutorateAndAssert(expected, shipment, org);

			var costEntry2 = costing.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "CNSHA", "AUFRE", "", "20GP");
			costEntry2.TI_OH_TransportProvider = TransportProvider1.PK;
			costEntry2.RateLines.RemoveAndDeleteAll();

			var costLine2 = costEntry2.AddRateLine(frtChargeCode, UnitCalculator.Code, QuantityUnit.CN, currencyCode: CurrencyCodes.Australia);
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 1200m;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = frtChargeCode.AC_Code,
							JR_OSCostAmt = 2400m,
							CostCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 1200.00/Container"
						},
				};

			AutorateAndAssert(expected, shipment, org);
		}

		#endregion

		#region Minimum Not Applied When Calculated Count Is Zero

		public void TestMinimumNotAppliedWhenCalculatedCountIsZero()
		{
			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader(2);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ZAJNB";
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 0m;

			var rate = Helper.NewClientRate(consignor);
			var entry = rate.AddRateEntry("ORG", "ALL", "AUSYD", "ZA");
			entry.RateLines.RemoveAndDeleteAll();
			entry.AddRateLine("ODOC", MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			entry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			entry.RateLines[0].Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)10m;

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = rate.Header.MainAddress.PK;
			testJob.LocalChargesPK = consignee.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt = 10m,
							RevenueCalculationDescription = "ODOC: Minimum AUD 10.00"
						}
				};

			AutorateAndAssert(expected, shipment, consignor, consignee, testJob);

			shipment.JS_ActualWeight = 0m;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt = 0m,
							RevenueCalculationDescription = "ODOC: 0 Kilogram(s) @ AUD 1.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, consignor, consignee, testJob);
		}

		#endregion

		#region TACT Rate Lines and commodity

		public void TestTACTRatesAreNotFilteredByCommodityCode()
		{
			var client = Helper.NewOrgHeader(1);
			TransportProvider1.OH_IsCreditor = true;

			var costing = Helper.NewCosting(TransportProvider1);

			var costEntryGENIsTact = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "STD", ZString.Empty);
			costEntryGENIsTact.RateLines.RemoveAndDeleteAll();
			costEntryGENIsTact.TI_IsTact = true;
			var costLineGEN = costEntryGENIsTact.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costLineGEN.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var costEntryGENIsNotTact = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "STD", ZString.Empty);
			costEntryGENIsNotTact.RateLines.RemoveAndDeleteAll();
			costEntryGENIsNotTact.TI_IsTact = false;
			var costLineGENNonTACT = costEntryGENIsNotTact.AddRateLine("WAR", UnitCalculator.Code, QuantityUnit.KG);
			costLineGENNonTACT.GetCalculator<UnitCalculator>().PerUnit = 3m;

			var costEntryHAZ = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "STD", ZString.Empty);
			costEntryHAZ.TI_RH_NKCommodityCode = "HAZ";
			costEntryHAZ.RateLines.RemoveAndDeleteAll();
			costEntryHAZ.TI_IsTact = true;
			var costLineHAZ = costEntryHAZ.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			costLineHAZ.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var costEntryCOFF = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "STD", ZString.Empty);
			costEntryCOFF.TI_RH_NKCommodityCode = "COFF";
			costEntryCOFF.RateLines.RemoveAndDeleteAll();
			costEntryCOFF.TI_IsTact = true;
			var costLineCOFF = costEntryCOFF.AddRateLine("CAF", UnitCalculator.Code, QuantityUnit.KG);
			costLineCOFF.GetCalculator<UnitCalculator>().PerUnit = 2m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.CreditorPK = TransportProvider1.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var packlineGEN = shipment.OuterPackLines.AddNew();
			packlineGEN.JL_ActualVolume = 0.7m;
			packlineGEN.JL_ActualWeight = 120m;
			packlineGEN.JL_RH_NKCommodityCode = "GEN";

			var packlineHAZ = shipment.OuterPackLines.AddNew();
			packlineHAZ.JL_ActualVolume = 0.170m;
			packlineHAZ.JL_ActualWeight = 30m;
			packlineHAZ.JL_RH_NKCommodityCode = "HAZ";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 1500m,
					CostCalculationDescription = "FRT: 150 Kilogram(s) @ AUD 10.00/KG"
				},
				new AssertionCost
				{
					E6_OSCostAmount = 750m,
					CostCalculationDescription = "BAF: 150 Kilogram(s) @ AUD 5.00/KG"
				},
				new AssertionCost
				{
					E6_OSCostAmount = 360,
					CostCalculationDescription = "WAR: 120 Kilogram(s) @ AUD 3.00/KG"
				}
			};

			AutoCostAndAssert("TACT rates should be not affected by commodity code, however rate entries with mismatching commodity should be filtered", null, expected, consol, false);
		}

		#endregion

		#region DisbursementInterestCalculator Related Tests

		public void TestEmptyDebtorDoesntThrowErrorInDINCalculator()
		{
			var rate = Helper.NewClientRate(Consignee);
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD", "", "");
			entry.RateLines.RemoveAndDeleteAll();
			entry.AddRateLine("FRT", DisbursementInterestCalculator.Code, QuantityUnit.KG);

			entry.RateLines[0].Calculator.RateLineItems.AddNew().TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.Disbursements;
			((DisbursementInterestCalculator)entry.RateLines[0].Calculator).Uplift = 1.5m;

			Factory.Save();

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", "FOB", Consignee, null, Consignee, null, "USLAX", "AUSYD", 1m, 1m);

			Job testJob = CreateJob(quotedBooking, quotedBooking.Quote.TH_QuoteNumber);
			testJob.PlugInData = quotedBooking;
			testJob.JH_OA_AgentCollectAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			testJob.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Assert("Precondition", testJob.JH_OA_LocalChargesAddr.IsEmpty);

			var expected = new[]
							{
								new SimpleArInfo
									{
										InvoiceLineDesc = "International Freight",
										Amount = 0m
									}
							};

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(quotedBooking.GetRatingAdapters().FirstOrDefault()), CostSell.Revenue);

			AssertRatingResults(expected, results);
		}

		public void TestDINCalculatorGetsAPTermFromServiceProviderForCostingShipment()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Debtor Full Name";
			consignor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromMonthEnd;
			consignor.CompanyData.OB_APPaymentTermDays = 90;
			consignor.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromInvoiceDate;
			creditor.CompanyData.OB_APPaymentTermDays = 100;
			creditor.OH_FullName = "Transport Provider One";
			creditor.OH_IsCreditor = true;

			var rate = Helper.NewCosting(creditor);
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtRateLine.ChargeCode.AC_IsGroupageCharge = true;
			frtRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			frtRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			frtRateLine.TL_LineOrder = 0;
			frtRateLine.TL_RX_NKCurrency = "AUD";

			var bafRateLine = entry.AddRateLine("BAF", DisbursementInterestCalculator.Code);
			((DisbursementInterestCalculator)bafRateLine.Calculator).Uplift = 10m;
			((DisbursementInterestCalculator)bafRateLine.Calculator).AdjustmentDays = 0;
			((DisbursementInterestCalculator)bafRateLine.Calculator).OutstandingDays = true;
			bafRateLine.TL_LineOrder = 1;
			bafRateLine.ChargeCode.AC_IsGroupageCharge = false;
			bafRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			bafRateLine.TL_RX_NKCurrency = "AUD";

			var appRateLineItem = bafRateLine.RateLineItems.AddNew();
			appRateLineItem.TM_Type = "APP";
			appRateLineItem.TM_Text = "ALL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = creditor.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_INCO = "FOB";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_ActualVolume = 30m;
			shipment.JS_ActualChargeable = 100m;
			shipment.JS_RX_NKFreightCostRateCurrency = "AUD";
			shipment.JS_RX_NKFrtRateCurrency = "AUD";

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.LocalChargesPK = creditor.PK;
			testJob.JH_A_JOP = ZDateTime.Now;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 5000.00m,
						CostCalculationDescription = @"FRT: 5000 Kilogram(s) @ AUD 1.00/KG"
					},
				new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSCostAmt = 136.99m,
						CostCalculationDescription = "BAF: AUD 5000.00 (All Charge Codes FRT) @ 10 % pa - 100 Days (100 effective outstanding days) For Transport Provider One"
					}
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutorateAndAssert(expected, shipment, consignor);
		}

		public void TestDINCalculatorGetsAPTermFromServiceProviderForCostingConsol()
		{
			DataRegistryRating.Instance.RatesServiceSubscription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Debtor Full Name";
			consignor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromMonthEnd;
			consignor.CompanyData.OB_APPaymentTermDays = 90;
			consignor.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromInvoiceDate;
			creditor.CompanyData.OB_APPaymentTermDays = 100;
			creditor.OH_FullName = "Transport Provider One";
			creditor.OH_IsCreditor = true;

			var rate = Helper.NewCosting(creditor);
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtRateLine.ChargeCode.AC_IsGroupageCharge = true;
			frtRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			frtRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			frtRateLine.TL_LineOrder = 0;
			frtRateLine.TL_RX_NKCurrency = "AUD";

			var bafRateLine = entry.AddRateLine("BAF", DisbursementInterestCalculator.Code);
			((DisbursementInterestCalculator)bafRateLine.Calculator).Uplift = 10m;
			((DisbursementInterestCalculator)bafRateLine.Calculator).AdjustmentDays = 0;
			((DisbursementInterestCalculator)bafRateLine.Calculator).OutstandingDays = true;
			bafRateLine.TL_LineOrder = 1;
			bafRateLine.ChargeCode.AC_IsGroupageCharge = true;
			bafRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			bafRateLine.TL_RX_NKCurrency = "AUD";

			var appRateLineItem = bafRateLine.RateLineItems.AddNew();
			appRateLineItem.TM_Type = "APP";
			appRateLineItem.TM_Text = "ALL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = creditor.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_INCO = "FOB";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_ActualVolume = 30m;
			shipment.JS_ActualChargeable = 100m;
			shipment.JS_RX_NKFreightCostRateCurrency = "AUD";
			shipment.JS_RX_NKFrtRateCurrency = "AUD";

			new JobHeader.Loader(shipment).TryLoadOrCreate();

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 5000.00m,
						CostCalculationDescription = @"FRT: 5000 Kilogram(s) @ AUD 1.00/KG"
					},
				new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 136.99m,
						CostCalculationDescription = "BAF: AUD 5000.00 (All Charge Codes FRT) @ 10 % pa - 100 Days (100 effective outstanding days) For Transport Provider One"
					}
			};

			AutoCostAndAssert("", null, expectedCosts, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestDINCalculatorGetsAPTermFromCreditorForStandardCosting()
		{
			DataRegistryRating.Instance.RatesServiceSubscription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Debtor Full Name";
			consignor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromMonthEnd;
			consignor.CompanyData.OB_APPaymentTermDays = 90;
			consignor.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromInvoiceDate;
			creditor.CompanyData.OB_APPaymentTermDays = 100;
			creditor.OH_FullName = "Creditor One";
			creditor.OH_IsCreditor = true;

			// Standard costing - no client
			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtRateLine.ChargeCode.AC_IsGroupageCharge = true;
			frtRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			frtRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			frtRateLine.TL_LineOrder = 0;
			frtRateLine.TL_RX_NKCurrency = "AUD";

			var bafRateLine = entry.AddRateLine("BAF", DisbursementInterestCalculator.Code);
			((DisbursementInterestCalculator)bafRateLine.Calculator).Uplift = 10m;
			((DisbursementInterestCalculator)bafRateLine.Calculator).AdjustmentDays = 0;
			((DisbursementInterestCalculator)bafRateLine.Calculator).OutstandingDays = true;
			bafRateLine.TL_LineOrder = 1;
			bafRateLine.ChargeCode.AC_IsGroupageCharge = true;
			bafRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			bafRateLine.TL_RX_NKCurrency = "AUD";

			var appRateLineItem = bafRateLine.RateLineItems.AddNew();
			appRateLineItem.TM_Type = "APP";
			appRateLineItem.TM_Text = "ALL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = creditor.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_INCO = "FOB";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_ActualVolume = 30m;
			shipment.JS_ActualChargeable = 100m;
			shipment.JS_RX_NKFreightCostRateCurrency = "AUD";
			shipment.JS_RX_NKFrtRateCurrency = "AUD";

			new JobHeader.Loader(shipment).TryLoadOrCreate();

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 5000.00m,
						CostCalculationDescription = @"FRT: 5000 Kilogram(s) @ AUD 1.00/KG"
					},
				new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 136.99m,
						CostCalculationDescription = "BAF: AUD 5000.00 (All Charge Codes FRT) @ 10 % pa - 100 Days (100 effective outstanding days) For Creditor One"
					}
			};

			AutoCostAndAssert("", null, expectedCosts, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestDINCalculatorGetsARTermFromDebtor()
		{
			NewClient.OH_FullName = "Debtor Full Name";

			ClientRate rate = Helper.NewClientRate(NewClient);
			RateEntry entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtRateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;
			frtRateLine.TL_LineOrder = 0;

			var bafRateLine = entry.AddRateLine("BAF", DisbursementInterestCalculator.Code);
			var calculator = bafRateLine.GetCalculator<DisbursementInterestCalculator>();
			calculator.Uplift = 10m;
			calculator.AdjustmentDays = 0;
			calculator.OutstandingDays = true;
			bafRateLine.TL_LineOrder = 1;
			var appRateLineItem = bafRateLine.RateLineItems.AddNew();
			appRateLineItem.TM_Type = "APP";
			appRateLineItem.TM_Text = "ALL";

			OrgARTerms consignorDSBTerm = Consignor.CompanyData.CreateOrLoadDisbursementARTerm();
			consignorDSBTerm.PY_InvoiceDays = 50;
			consignorDSBTerm.PY_InvoiceTerm = InvoiceTerms.FromInvoiceDate;

			OrgARTerms localClientDSBTerm = NewClient.CompanyData.CreateOrLoadDisbursementARTerm();
			localClientDSBTerm.PY_InvoiceDays = 100;
			localClientDSBTerm.PY_InvoiceTerm = InvoiceTerms.FromInvoiceDate;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "USLAX", "AUSYD", 1m, 3m);

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.LocalChargesPK = NewClient.PK;
			testJob.JH_A_JOP = ZDateTime.Now;

			var expected = new[]
			{
				new SimpleArInfo
				{
					InvoiceLineDesc = "International Freight",
					Amount = 500m,
					CalculationSingleLineDescription = @"FRT: 500 Kilogram(s) @ USD 1.00/KG"
				},
				new SimpleArInfo
				{
					InvoiceLineDesc = "Bunker Adjustment Factor",
					Amount = 13.70m,
					CalculationSingleLineDescription = "BAF: USD 500.00 (All Charge Codes FRT) @ 10 % pa - 100 Days (100 effective outstanding days) For Debtor Full Name"
				}
			};

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			AssertRatingResults(expected, results);
		}

		public void TestEmptyArrivalTimeDoesntThrowErrorInDINCalculator()
		{
			NewClient.OH_FullName = "Debtor Full Name";

			ClientRate rate = Helper.NewClientRate(NewClient);
			RateEntry entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtRateLine.GetCalculator<UnitCalculator>().PerUnit = 1;
			frtRateLine.TL_LineOrder = 0;

			var bafRateLine = entry.AddRateLine("BAF", DisbursementInterestCalculator.Code);
			var calculator = bafRateLine.GetCalculator<DisbursementInterestCalculator>();
			calculator.Uplift = 10m;
			calculator.AdjustmentDays = 0;
			calculator.OutstandingDays = false;
			bafRateLine.TL_LineOrder = 1;

			var appRateLineItem = bafRateLine.RateLineItems.AddNew();
			appRateLineItem.TM_Type = "APP";
			appRateLineItem.TM_Text = "ALL";

			OrgARTerms localClientDSBTerm = NewClient.CompanyData.CreateOrLoadDisbursementARTerm();
			localClientDSBTerm.PY_InvoiceDays = 0;
			localClientDSBTerm.PY_InvoiceTerm = InvoiceTerms.FromShipmentDate;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "USLAX", "AUSYD", 1, 3);

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.LocalChargesPK = NewClient.PK;
			testJob.JH_A_JOP = ZDateTime.Now;

			var expected = new[]
			{
				new SimpleArInfo
				{
					InvoiceLineDesc = "International Freight",
					Amount = 500m,
					CalculationSingleLineDescription = @"FRT: 500 Kilogram(s) @ USD 1.00/KG"
				},
				new SimpleArInfo
				{
					InvoiceLineDesc = "Bunker Adjustment Factor",
					Amount = 0,
					CalculationSingleLineDescription = "BAF: Calculation failed due to Cannot calculate disbursement interest because job arrival date is empty and it is impossible to establish the number of effective interest days."
				}
			};

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			AssertRatingResults(expected, results);

			shipment.JS_E_ARV = ZDateTime.Now.AddDays(-101);

			Factory.Save();

			expected = new[]
			{
				new SimpleArInfo
				{
					InvoiceLineDesc = "International Freight",
					Amount = 500m,
					CalculationSingleLineDescription = @"FRT: 500 Kilogram(s) @ USD 1.00/KG"
				},
				new SimpleArInfo
				{
					InvoiceLineDesc = "Bunker Adjustment Factor",
					Amount = 13.70m,
					CalculationSingleLineDescription = "BAF: USD 500.00 (All Charge Codes FRT) @ 10 % pa - 0 Days From Date of Shipment (100 effective days) For Debtor Full Name"
				}
			};

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			AssertRatingResults(expected, results);
		}

		#endregion

		#region If the chargable weight afer rounding equal break value, the max rate should be applied

		public void TestAutoRate_RoundedChargableWeightEqualsBreakValue_ApplyMaxRate()
		{
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntry("ORG", "ALL", "UAIEV", "", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var rateLine = entry.AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_Rounding = RatingRoundingTypes.UpTo1;

			var rateLineItem1 = rateLine.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.Minus;
			rateLineItem1.TM_Break = 45m;
			rateLineItem1.TM_RelevantValue = 1m;

			var rateLineItem2 = rateLine.RateLineItems.AddNew();
			rateLineItem2.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem2.TM_Break = 45m;
			rateLineItem2.TM_RelevantValue = 2m;

			AddParityExchangeRate(entry.Currency);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = Helper.NewOrgHeader(2).PK;
			shipment.ConsignorPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "ZAJNB";
			shipment.JS_INCO = "FOB";
			shipment.JS_ActualWeight = 24m;
			shipment.JS_ActualVolume = 268.182m;
			shipment.JS_UnitOfVolume = "D3";
			shipment.JS_UnitOfWeight = "KG";
			AssertEquals("Precondition", 44.697m, shipment.JS_ActualChargeable);

			Factory.Save();

			// Testing with rounding
			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 90m,
					RevenueCalculationDescription = "ODOC: 45 Kilogram(s) @ UAH 2.00/KG"
				},
			};

			AutorateAndAssert(expected, shipment, client);

			// Testing without rounding
			rateLine.TL_Rounding = RatingRoundingTypes.NoRounding;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 44.70m,
					RevenueCalculationDescription = "ODOC: 44.697 Kilogram(s) @ UAH 1.00/KG"
				},
			};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region TestPerUnitCalcWithMultiplesAndRounding

		public void TestRoundingUsesWeightVolumeMultiple()
		{
			var client = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("ORG", "SEA", "AUSYD", "USLAX");
			var line = entry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)line.Calculator).PerUnit = 100m;
			line.TL_WeightVolumeMultiple = 100m;
			line.TL_Rounding = RatingRoundingTypes.UpTo1;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 95.5m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 96m,
					RevenueCalculationDescription = "ODOC: 96 Kilogram(s) @ AUD 100.00/100 KG"
				},
			};

			AutorateAndAssert(expected, shipment, client);

			RatingDataRegistry.Instance.RoundingUsesWeightVolumeMultiple.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 100m,
					RevenueCalculationDescription = "ODOC: 100 Kilogram(s) @ AUD 100.00/100 KG"
				},
			};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region Test Chargeable Exception Not Applicable When Rate Line does not require weight volume

		public void TestChargeableExceptionNotApplicableWhenWeightVolumeNotRequired()
		{
			var client = Helper.NewOrgHeader();
			var rateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "CNSHA", "AUSYD");
			var unitRateLine = rateEntry.AddRateLine("DCART", UnitCalculator.Code, QuantityUnit.M3);
			unitRateLine.GetCalculator<UnitCalculator>().PerUnit = 10m;
			unitRateLine.TL_Rounding = RatingRoundingTypes.Chargeable;

			var percentageRateLine = rateEntry.AddRateLine("DDOC", PercentageCalculator.Code);
			percentageRateLine.GetCalculator<PercentageCalculator>().Percent = 5m;
			percentageRateLine.TL_Rounding = RatingRoundingTypes.Chargeable;
			var percentageApplyToRateLineItem = percentageRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			percentageApplyToRateLineItem.TM_AC = Helper.ChargeCodes["DCART"].PK;

			AssertEquals("Pre-condition: this calculator does not require a unit", false, percentageRateLine.RequiresWeightVolume());

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_OuterPacks = 5;
			shipment.JS_ActualVolume = 50m;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.ConsigneePK = client.PK;

			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew();
			shipment.OuterPackLines.AddNew();

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 500m,
							RevenueCalculationDescription = "DCART: 50 Cubic Meter(s) @ AUD 10.00/M3"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 25m,
							RevenueCalculationDescription = "DDOC: 5.00% of (AUD 500.00 (DCART))"
						}
				};

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);
		}

		public void TestCostOrCompanyTariffBasedCalculatorDependsOnMasterRateLine_UnsupportedMeasure()
		{
			var zQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "DSEC");
			zQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var dsecCC = Factory.LoadTop1<AccChargeCode>(zQuery);
			dsecCC.AC_DepartmentFilterList = "FIS";

			var tariffEntry = Helper.NewCompanyTariff().AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "CNSHA", "AUSYD");
			var tariffRateLine = tariffEntry.AddRateLine("DSEC", UnitCalculator.Code, QuantityUnit.HR, CurrencyCodes.Australia);
			tariffRateLine.GetCalculator<UnitCalculator>().PerUnit = 20m;
			tariffEntry.Factory.Save();

			var client = Helper.NewOrgHeader(1);
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "CNSHA", "AUSYD");
			var clientRateLine = clientRateEntry.AddRateLine("DSEC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, "", CurrencyCodes.Australia);
			clientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;
			clientRateLine.TL_Rounding = RatingRoundingTypes.Chargeable;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_OuterPacks = 5;
			shipment.JS_ActualVolume = 50m;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.ConsigneePK = client.PK;

			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew();
			shipment.OuterPackLines.AddNew();

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 0m,
					RevenueCalculationDescription = "Calculation failed due to charge code is using the Company Tariff Based Calculator, however there are conflicting or no tariff rates found."
				},
			};

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);

			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Log should contain expected line",
				"Information: RateLine Filtered DSEC-UNT-HR-Base Company Tariff	reason:	no Time measure on the job.");
		}

		public void TestChargeableExceptionForCostOrCompanyTariffBasedCalculatorDependsOnMasterRateLine_NoExceptionThrown()
		{
			var tariffEntry = Helper.NewCompanyTariff().AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "CNSHA", "AUSYD");
			var tariffRateLine1 = tariffEntry.AddRateLine("DDOC", FlatCalculator.Code);
			tariffRateLine1.GetCalculator<FlatCalculator>().BaseRate = 500m;
			var tariffRateLine2 = tariffEntry.AddRateLine("DCART", UnitCalculator.Code, QuantityUnit.M3);
			tariffRateLine2.GetCalculator<UnitCalculator>().PerUnit = 20m;
			tariffEntry.Factory.Save();

			var client = Helper.NewOrgHeader(1);
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "CNSHA", "AUSYD");
			var clientRateLine1 = clientRateEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			clientRateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;
			clientRateLine1.TL_Rounding = RatingRoundingTypes.Chargeable;
			var clientRateLine2 = clientRateEntry.AddRateLine("DCART", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			clientRateLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;
			clientRateLine2.TL_Rounding = RatingRoundingTypes.Chargeable;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_OuterPacks = 5;
			shipment.JS_ActualVolume = 50m;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.ConsigneePK = client.PK;

			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew();
			shipment.OuterPackLines.AddNew();

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 1000m,
							RevenueCalculationDescription = "DCART: 50 Cubic Meter(s) @ AUD 20.00/M3"
						},
					new AssertionCharge
						{
							JR_OSSellAmt = 525m,
							RevenueCalculationDescription = "DDOC: 105.00% of (Base Rate AUD 500.00)"
						}
				};

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);
		}

		#endregion

		#region Percentage Calculator Handles Deleted ChargeCodes

		public void TestPercentageCalculatorHandlesDeletedChargeCodes()
		{
			var anotherFactory = new BusinessObjectFactory();

			var chargeCode = anotherFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "NEWCC";
			chargeCode.AC_Desc = "Something new, not currently in DB";
			chargeCode.AC_ChargeGroup = "FRT";

			anotherFactory.Save();

			OrgHeader consignor = Helper.NewOrgHeader();
			OrgHeader consignee = Helper.NewOrgHeader();

			ClientRate rate = Helper.NewClientRate(consignor);
			RateEntry entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");

			entry.RateLines.RemoveAndDeleteAll();

			RateLine line = entry.AddRateLine("BAF", PercentageCalculator.Code);
			line.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			line.GetCalculator<PercentageCalculator>().Percent = 50m;
			line.GetCalculator<PercentageCalculator>().IncludeGST = false;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 0m;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";

			Job job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_AgentCollectAddr = rate.Header.MainAddress.PK;
			job.LocalChargesPK = consignor.PK;

			Factory.Save();

			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = 235m;
			charge.JR_RX_NKCostCurrency = "AUD";

			chargeCode.Delete();
			anotherFactory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());

			AssertNoExceptionThrown(() => autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue));
		}

		#endregion

		#region Percentage Calculator uses buy exchange rate for costings/sell exchange rate for client rates

		public void TestPercentageCalculatorUsesBuyExchangeRateForCostings()
		{
			var carrier = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(carrier);

			SetupPercentageCalculator(costing);

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, client.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_LocalCostAmt = 1250m,
					CostCalculationDescription = "FRT: 1000 Kilogram(s) @ USD 1.00/KG",
				},
				new AssertionCharge
				{
					JR_LocalCostAmt = 125m,
					CostCalculationDescription = "BAF: 10.00% of (AUD 1250.00 (Freight Charges FRT))",
				},
			};

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var rate = job.ExchangeRates.AddNew();
				rate.JF_RX_NKRateCurrency = "USD";
				rate.JF_BaseRate = 0.8m;

				AutorateAndAssert("Buy exchange rate was used for costing", expected, shipment, client, job: job);
			}
		}

		public void TestPercentageCalculatorUsesSellExchangeRateForClientRates()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			SetupPercentageCalculator(clientRate);

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, client.PK, "USLAX", "AUSYD", 1000m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 1000m,
						RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ USD 1.00/KG",
					},
				new AssertionCharge
					{
						JR_OSSellAmt = 50m,
						RevenueCalculationDescription = "BAF: 10.00% of (AUD 500.00 (Freight Charges FRT))",
					},
			};

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var rate = job.ExchangeRates.AddNew();
				rate.JF_RX_NKRateCurrency = "USD";
				rate.OrgType = ExchangeRateOrgTypeEnum.Creditor;
				rate.JF_BaseRate = 0.8m;

				rate = job.ExchangeRates.AddNew();
				rate.JF_RX_NKRateCurrency = "USD";
				rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
				rate.JF_BaseRate = 2m;

				AutorateAndAssert("Sell exchange rate was used for client rate", expected, shipment, client, job: job);
			}
		}

		void SetupPercentageCalculator(RatingHeader rate)
		{
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "US", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var frtLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtLine.TL_RX_NKCurrency = "USD";

			var bafLine = rateEntry.AddRateLine("BAF", PercentageCalculator.Code);
			bafLine.TL_RX_NKCurrency = "AUD";

			(frtLine.Calculator as UnitCalculator).PerUnit = 1;
			(bafLine.Calculator as PercentageCalculator).Percent = 10;
			(bafLine.Calculator as PercentageCalculator).AddApplyToItem(CalculatorConstants.Text.FreightCharges);
		}

		#endregion

		#region Percentage Calculator Combined Charges Description

		public void TestPercentageCalculator_CombinedChargesDescription()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(consignee);

			var airEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			airEntry.RateLines.RemoveAndDeleteAll();

			var flatCalcLine = airEntry.AddRateLine(TestFRT.AC_Code, FlatCalculator.Code);
			flatCalcLine.TL_RX_NKCurrency = "USD";
			((FlatCalculator)flatCalcLine.Calculator).BaseRate = 12m;

			var combinedCalcLine = airEntry.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, Weight.Kilograms);
			combinedCalcLine.TL_RX_NKCurrency = "USD";
			((UnitCalculator)combinedCalcLine.Calculator).PerUnit = 2m;

			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "USLAX", "AUSYD");
			orgEntry.RateLines.RemoveAndDeleteAll();

			var ocaaLine = orgEntry.AddRateLine("OCAA", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 143;

			var percentageLine = airEntry.AddRateLine(TestFSC.AC_Code, PercentageCalculator.Code);
			percentageLine.TL_RX_NKCurrency = "USD";
			((PercentageCalculator)percentageLine.Calculator).Percent = 50m;

			var insuranceItem = percentageLine.RateLineItems.AddNew();
			insuranceItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.InsuranceValue;
			insuranceItem.TM_Type = CalculatorConstants.Type.ApplyTo;

			var chargeCodeItem = percentageLine.RateLineItems.AddNew();
			chargeCodeItem.TM_Text = CalculatorConstants.Text.ChargeCode;
			chargeCodeItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			chargeCodeItem.TM_AC = TestFRT.PK;

			var orgItem = percentageLine.RateLineItems.AddNew();
			orgItem.TM_Text = CalculatorConstants.Text.OriginCharges;
			orgItem.TM_Type = CalculatorConstants.Type.ApplyTo;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUSYD", 24m);
			Factory.Save();

			shipment.JS_InsuranceValue = 40m;
			shipment.JS_RX_NKInsuranceCurrency = "USD";

			var odocQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC");
			odocQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var odocCC = Factory.LoadTop1<AccChargeCode>(odocQuery);

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = consignee.MainAddress.PK;
			testJob.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = odocCC.PK;
			charge.JR_OH_SellAccount = consignee.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 78m;

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "TESTFRT",
							JR_OSSellAmt = 60m,
							RevenueCalculationDescription = "This charge is calculated from multiple rates"
						},
						new AssertionCharge
						{
							ChargeCode = "OCAA",
							JR_OSSellAmt = 143m,
							RevenueCalculationDescription = "OCAA: Base Rate USD 143.00",
						},
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt = 78m,
						},
					new AssertionCharge
						{
							ChargeCode = "TESTFSC",
							JR_OSSellAmt = 160.5m,
							RevenueCalculationDescription = "TESTFSC: 50.00% of (USD 321.00 (Insurance Value + TESTFRT + Origin Charges TESTFRT 12.00 + Insurance Value 40.00 + TESTFRT 48.00 + ODOC* 78.00 + OCAA 143.00))",
						}
				};

			AutorateAndAssert(expected, shipment, null, null, testJob);
		}

		#endregion

		#region Inactive ChargeCodes are Skipped

		public void TestInactiveChargeCodesAreSkipped()
		{
			var inactiveChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			inactiveChargeCode1.AC_Code = "NEWCC1";
			inactiveChargeCode1.AC_Desc = "Inactive ChargeCode 1";
			inactiveChargeCode1.AC_ChargeGroup = "FRT";
			inactiveChargeCode1.AC_IsActive = false;

			var inactiveChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			inactiveChargeCode2.AC_Code = "NEWCC2";
			inactiveChargeCode2.AC_Desc = "Inactive ChargeCode 2";
			inactiveChargeCode2.AC_ChargeGroup = "FRT";
			inactiveChargeCode2.AC_IsActive = false;

			var activeChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			activeChargeCode1.AC_Code = "NEWCC3";
			activeChargeCode1.AC_Desc = "Active ChargeCode 1";
			activeChargeCode1.AC_ChargeGroup = "FRT";

			var activeChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			activeChargeCode2.AC_Code = "NEWCC4";
			activeChargeCode2.AC_Desc = "Active ChargeCode 2";
			activeChargeCode2.AC_ChargeGroup = "FRT";

			Factory.Save();

			OrgHeader consignor = Helper.NewOrgHeader();
			OrgHeader consignee = Helper.NewOrgHeader();

			ClientRate rate = Helper.NewClientRate(consignor);
			RateEntry entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");

			entry.RateLines.RemoveAndDeleteAll();

			RateLine line1 = entry.AddRateLine("NEWCC1", FlatCalculator.Code);
			((FlatCalculator)line1.Calculator).BaseRate = 20;

			RateLine line2 = entry.AddRateLine("NEWCC3", FlatCalculator.Code);
			((FlatCalculator)line2.Calculator).BaseRate = 10;

			RateLine line3 = entry.AddRateLine("BAF", PercentageCalculator.Code);
			line3.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			line3.GetCalculator<PercentageCalculator>().Percent = 50m;
			line3.GetCalculator<PercentageCalculator>().IncludeGST = false;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 0m;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "FOB";

			Job job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_AgentCollectAddr = rate.Header.MainAddress.PK;
			job.LocalChargesPK = consignor.PK;

			var activeCharge = job.Charges.AddNew();
			activeCharge.JR_AC = activeChargeCode2.PK;
			activeCharge.JR_OSSellAmt = 30m;
			activeCharge.JR_RX_NKCostCurrency = "AUD";

			var inactiveCharge = job.Charges.AddNew();
			inactiveCharge.JR_AC = inactiveChargeCode2.PK;
			inactiveCharge.JR_OSSellAmt = 50m;
			inactiveCharge.JR_RX_NKCostCurrency = "AUD";

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			var expectedResult = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = "Active ChargeCode 1",
							Amount = 10m,
							CalculationSingleLineDescription = "NEWCC3: Base Rate AUD 10.00",
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Bunker Adjustment Factor",
							Amount = 20m,
							CalculationDescription = "BAF: 50.00% of (AUD 40.00 (Freight Charges NEWCC3 10.00 + NEWCC4* 30.00))",
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Inactive ChargeCode 1",
							Amount = 20m,
							CalculationSingleLineDescription = "NEWCC1: Base Rate AUD 20.00",
						},
				};

			AssertRatingResults(expectedResult, results);
		}

		#endregion

		public void TestZeroAmountDisbursementCharge_WhenAutorateAndUseQuickCalculate_ShouldUpdateSellAmountBaseOnCostAmount()
		{
			DataRegistryRating.Instance.RatesServiceSubscription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());

			var dsbChargeCode = Helper.ChargeCodes.New("OCUSDSB", "Disbursement Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeType.Disbursement);
			dsbChargeCode.AC_ChargeType = ChargeType.Disbursement;

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "OCUSDSB", flatRateAmount: 0m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", weight: 100m, volume: 200m);

			Factory.Save();

			AutorateAndAssert
			(
				expected: new[]
				{
					new AssertionCharge
					{
						ChargeCode = "OCUSDSB",
						JR_LocalCostAmt = 0m,
						JR_LocalSellAmt = 0m,
						RevenueCalculationDescription = @"OCUSDSB: Base Rate AUD 0.00"
					}
				},
				shipment,
				Consignee
			);

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var shipmentJobCharge = (Charge)shipmentJob.Charges.Single();
				shipmentJobCharge.ApplyCustomQuickCalculator(shipment.RatingAdapter, costAmount: 10m, sellAmount: 0m);

				CombineAssertions
				(
					"GIVEN autorate returns DSB charge with $0 WHEN apply QuickCalculator THEN sellAmount should be set from costAmount",
					() =>
					{
						AssertEquals("Local Cost Amt", 10m, shipmentJobCharge.JR_LocalCostAmt);
						AssertEquals("Local Sell Amt", 10m, shipmentJobCharge.JR_LocalSellAmt);
					}
				);
			}
		}

		#region Organization Types Priorities For Sell Rates

		public void TestExportPrepaidCollectRatePriorities()
		{
			var consignor = Helper.NewOrgHeader(1);
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);
			var controllingCustomer = Helper.NewOrgHeader(1);

			var tariff = Helper.NewCompanyTariff();
			var tarifEntry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			tarifEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tarifEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)tariffLine.Calculator).PerUnit = 5m;
			tariff.Factory.Save();

			var rate1 = Helper.NewClientRate(consignor);
			var rateEntry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var rate2 = Helper.NewClientRate(localClient);
			var rateEntry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)200m;

			var rate3 = Helper.NewClientRate(consignee);
			var rateEntry3 = rate3.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry3.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;

			var rate4 = Helper.NewClientRate(controllingCustomer);
			var rateEntry4 = rate4.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry4.RateLines.RemoveAndDeleteAll();
			var rateLine4 = rateEntry4.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine4.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)400m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = Helper.NewOrgHeader(1).PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 5m;
			shipment.JS_INCO = "FOB";

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = consignor.MainAddress.PK;

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 100000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 100.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			var collection = new RatesPrioritiesCollection();
			collection.RemoveAndDeleteAll();

			RatesPriorities ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNE);

			Factory.Save();

			RatingDataRegistry.Instance.ExportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			shipment.ConsigneePK = consignee.PK;

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 300000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 300.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			RatingDataRegistry.Instance.ExportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNE);

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);

			shipment.JS_INCO = "DAP";

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 300000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 300.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			collection.RemoveAndDeleteAll();

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CCUS);

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNE);

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);

			JobDocAddress scp = shipment.ControllingCustomerAddress;
			scp.OrganisationPK = controllingCustomer.PK;

			RatingDataRegistry.Instance.ExportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 400000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 400.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			rate1.Delete();
			rate2.Delete();
			rate3.Delete();
			rate4.Delete();

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 5000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 5.00/KG",
									}
							};

			AssertRatingResults(expected, results);
		}

		public void TestImportPrepaidCollectRatePriorities()
		{
			var consignor = Helper.NewOrgHeader(1);
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			var tariff = Helper.NewCompanyTariff();
			var tarifEntry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			tarifEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tarifEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)tariffLine.Calculator).PerUnit = 5m;

			tariff.Factory.Save();

			var rate1 = Helper.NewClientRate(consignor);
			var rateEntry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var rate2 = Helper.NewClientRate(localClient);
			var rateEntry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)200m;

			var rate3 = Helper.NewClientRate(consignee);
			var rateEntry3 = rate3.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			rateEntry3.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = Helper.NewOrgHeader(1).PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 5m;
			shipment.JS_INCO = "FOB";

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = consignor.MainAddress.PK;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 300000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ USD 300.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			testJob.LocalChargesPK = localClient.PK;

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 200000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ USD 200.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			var collection = new RatesPrioritiesCollection();
			collection.RemoveAndDeleteAll();

			RatesPriorities ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNR);

			Factory.Save();

			RatingDataRegistry.Instance.ImportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			Factory.Save();
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 100000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ USD 100.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			RatingDataRegistry.Instance.ImportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNR);

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);

			shipment.JS_INCO = "EXW";

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 100000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ USD 100.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			rate1.Delete();
			rate2.Delete();
			rate3.Delete();

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 5000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ USD 5.00/KG",
									}
							};

			AssertRatingResults(expected, results);
		}

		public void TestDomesticPrepaidCollectRatePriorities()
		{
			var consignor = Helper.NewOrgHeader(1);
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			var tariff = Helper.NewCompanyTariff();
			var tarifEntry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "AUSYD");
			tarifEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tarifEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)tariffLine.Calculator).PerUnit = 5m;
			tariff.Factory.Save();

			var rate1 = Helper.NewClientRate(consignor);
			var rateEntry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "AUSYD");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var rate2 = Helper.NewClientRate(localClient);
			var rateEntry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "AUSYD");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)200m;

			var rate3 = Helper.NewClientRate(consignee);
			var rateEntry3 = rate3.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "AUSYD");
			rateEntry3.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = Helper.NewOrgHeader(1).PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 5m;
			shipment.JS_INCO = DomesticPaymentTerms.Collect;

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = consignor.MainAddress.PK;
			testJob.LocalChargesPK = localClient.PK;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 300000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 300.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			var collection = new RatesPrioritiesCollection();
			collection.RemoveAndDeleteAll();

			RatesPriorities ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNR);

			Factory.Save();

			RatingDataRegistry.Instance.DomesticCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			Factory.Save();
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 100000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 100.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			RatingDataRegistry.Instance.DomesticPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNR);

			ratesPriority = collection.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);

			shipment.JS_INCO = "PPD";

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 100000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 100.00/KG",
									}
							};

			AssertRatingResults(expected, results);

			rate1.Delete();
			rate2.Delete();
			rate3.Delete();

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 5000m,
										InvoiceLineDesc = "International Freight",
										CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 5.00/KG",
									}
							};

			AssertRatingResults(expected, results);
		}

		public void TestSpecifyUsageOfCompanyTariffForRatePriorities()
		{
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			var consigneeRate = Helper.NewClientRate(consignee);

			var consigneeRateRateEntry = consigneeRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			consigneeRateRateEntry.RateLines.RemoveAndDeleteAll();

			var consigneeRateLineFRT = consigneeRateRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			consigneeRateLineFRT.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;

			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffLineFRT = tariffEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)tariffLineFRT.Calculator).PerUnit = 3m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = localClient.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_ActualVolume = 5m;
			shipment.JS_UnitOfVolume = Volume.CubicMetres;
			shipment.JS_INCO = "CFR";

			Job testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.JH_OA_AgentCollectAddr = localClient.MainAddress.PK;

			var ratePriorities = new RatesPrioritiesCollection();
			ratePriorities.RemoveAndDeleteAll();

			var ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);
			ratesPriority.UseCompanyTariff = false;

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNE);
			ratesPriority.UseCompanyTariff = true;

			RatingDataRegistry.Instance.ExportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratePriorities);

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Revenue);

			var expected = new[]
			{
				new SimpleArInfo
				{
					Amount = 1000m,
					InvoiceLineDesc = "International Freight",
					CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 1.00/KG",
				}
			};

			AssertRatingResults(expected, results);
		}

		public void TestCorrectConsignorIsObtainedFromRatingAdapter()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader();
			var overseasAgent = Helper.NewOrgHeader();
			var pickupCompany = Helper.NewOrgHeader();

			var consignorRate = Helper.NewClientRate(consignor);
			var consignorRateEntry = consignorRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "BOVVI");
			consignorRateEntry.RateLines.RemoveAndDeleteAll();

			var consignorRateLineFRT = consignorRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			consignorRateLineFRT.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;

			var shipment = CreateForwardingShipment("AIR", consignor.PK, consignee.PK, "AUSYD", "BOVVI", 1000, 4.058m);
			shipment.JS_UniqueConsignRef = "S00001";

			shipment.ConsignorPickupAddress.E2_OA_Address = pickupCompany.MainAddress.PK;

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = overseasAgent.PK;

			var ratePriorities = new RatesPrioritiesCollection();
			ratePriorities.RemoveAndDeleteAll();

			var ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNR);

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LCBK);

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNE);

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.AG);
			ratesPriority.UseCompanyTariff = true;

			RatingDataRegistry.Instance.ExportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratePriorities);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1000m,
					RevenueCalculationDescription = @"FRT: 1000 Kilogram(s) @ AUD 1.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, localClient, job: job);
		}

		public void TestCorrectConsigneeIsObtainedFromRatingAdapter()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader();
			var overseasAgent = Helper.NewOrgHeader();
			var deliveryCompany = Helper.NewOrgHeader();

			var consigneeRate = Helper.NewClientRate(consignee);
			var consigneeRateEntry = consigneeRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "BOVVI");
			consigneeRateEntry.RateLines.RemoveAndDeleteAll();

			var consigneeRateLineFRT = consigneeRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			consigneeRateLineFRT.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;

			var shipment = CreateForwardingShipment("AIR", consignor.PK, consignee.PK, "AUSYD", "BOVVI", 1000, 4.058m);
			shipment.JS_UniqueConsignRef = "S00001";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryCompany.MainAddress.PK;

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = overseasAgent.PK;

			var ratePriorities = new RatesPrioritiesCollection();
			ratePriorities.RemoveAndDeleteAll();

			var ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNE);

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LCBK);

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNR);

			ratesPriority = ratePriorities.AddNew();
			ratesPriority.OrganizationType = nameof(RatingDebtorOrgTypes.AG);
			ratesPriority.UseCompanyTariff = true;

			RatingDataRegistry.Instance.ExportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratePriorities);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1000m,
					RevenueCalculationDescription = @"FRT: 1000 Kilogram(s) @ AUD 1.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, localClient, job: job);
		}

		#endregion

		#region Controlling Customer

		public void TestControllingCustomerRate()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			var controllingCustomer = Helper.NewOrgHeader();
			controllingCustomer.OH_FullName = "CONT CUS";

			var rate = Helper.NewClientRate(Consignee);
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry1.TI_OH_Consignor = Consignor.PK;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG).GetCalculator<UnitCalculator>().PerUnit = 100m;

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry2.TI_OH_ControllingCustomer = controllingCustomer.PK;
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG).GetCalculator<UnitCalculator>().PerUnit = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUBNE", "USLAX", 1000, 5m);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 100000m,
							RevenueCalculationDescription = @"FRT: 1000 Kilogram(s) @ AUD 100.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, Consignee);

			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			var expectedRevenueCalculationDescription = @"FRT: 1000 Kilogram(s) @ AUD 200.00/KG

International Freight

Charge located in CONSIGNEE1 client rate with the following details:

" + DescriptionHelpers.FormatWithTab("Ctrl. Cus.:") + @"CONT CUS";

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 200000m,
							ChargeCode = "FRT",
							RevenueCalculationDescription = expectedRevenueCalculationDescription
						}
				};

			AutorateAndAssert(expected, shipment, Consignee);

			var expectedLogMessage = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 2",
				"Information: RateLine Found FRT-UNT-KG-Client Rate CONSIGNEE1 (x2)",
				"Information: RateLine Filtered FRT-UNT-KG-Client Rate CONSIGNEE1	reason:	overridden by FRT-UNT-KG-Client Rate CONSIGNEE1 by Controlling Customer comparer"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Expected a message", expectedLogMessage);
		}

		public void TestControllingCustomerCost()
		{
			var controllingCustomer = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(null);
			var costEntry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 500m);
			var costEntry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 600m);
			costEntry2.TI_OH_ControllingCustomer = controllingCustomer.PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 30m);
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "SGSIN", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.JK_OA_CreditorAddress = Helper.CreateCreditor().MainAddress.PK;

			var shipment2 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 30m, 0m, consol);
			shipment2.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 600m,
				}
			};

			var message = "Should prefer Controlling Customer cost because both shipments have the same controlling customer.";
			AutoCostAndAssert(message, null, expectedCosts, consol, false);

			var shipment3 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 30m, 0m, consol);
			shipment3.ControllingCustomerAddress.OrganisationPK = Helper.NewOrgHeader().PK;

			var expectedCosts2 = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 500m,
				}
			};

			message = "Because this third shipment has a different controlling customer, we cannot use the controlling customer rate.";
			AutoCostAndAssert(message, null, expectedCosts2, consol, false);

			shipment3.ControllingCustomerAddress.Delete();

			message = "Removing the controlling customer now means there's only one controlling customer across the two shipments.";
			AutoCostAndAssert(message, null, expectedCosts, consol, false);
		}

		#endregion

		#region Controlling Agent

		public void TestControllingAgentCost_StandAloneShipment()
		{
			var controllingAgent = Helper.CreateCreditor("AGENT");
			var cost = Helper.NewCosting(controllingAgent);
			cost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AU", "", "DCART", 100000m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUBNE", "GBLHR", 1);
			var controllingAgentAddress = shipment.DocAddresses.AddNew(DocAddressType.ControllingAgent);
			controllingAgentAddress.OrganisationPK = controllingAgent.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "DCART",
							JR_LocalCostAmt = 100000m,
						}
				};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestControllingAgentCost_ShipmentWithConsol()
		{
			var controllingAgent = Helper.CreateCreditor("AGENT");
			var cost = Helper.NewCosting(controllingAgent);
			cost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AU", "", "DCART", 100000m);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUBNE", "USLAX", 1, consol: consol);
			var controllingAgentAddress = shipment.DocAddresses.AddNew(DocAddressType.ControllingAgent);
			controllingAgentAddress.OrganisationPK = controllingAgent.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "DCART",
							JR_LocalCostAmt = 100000m,
						}
				};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		#endregion

		#region LocalClientIsImportBroker

		[GuiTest]
		public void TestLocalClientIsImportBroker()
		{
			var cnr = Helper.NewOrgHeader();
			var cne = Helper.NewOrgHeader();
			var lc = Helper.NewOrgHeader();

			var lcRate = Helper.NewClientRate(lc);
			var lcEntry = lcRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "DEFRA", "AUBNE");
			lcEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(lcEntry.Currency);
			var lcLine = lcEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			lcLine.GetCalculator<UnitCalculator>().PerUnit = 8;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment.JS_RL_NKOrigin = "DEFRA";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_INCO = "FOB";
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_OH_ImportBroker = lc.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 8000m,
						}
				};

			AutorateAndAssert(expected, shipment, lc);

			var cneRate = Helper.NewClientRate(cne);
			var cneEntry = cneRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "DEFRA", "AUBNE");
			cneEntry.RateLines.RemoveAndDeleteAll();
			var cneLine = cneEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			cneLine.GetCalculator<UnitCalculator>().PerUnit = 9;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 9000m,
						}
				};

			AutorateAndAssert(expected, shipment, lc);

			shipment.JS_OH_ImportBroker = ZGuid.Empty;

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 8000m,
						}
				};

			AutorateAndAssert(expected, shipment, lc);
		}

		#endregion

		#region LocalClientIsExportBroker

		[GuiTest]
		public void TestLocalClientIsExportBroker()
		{
			var cnr = Helper.NewOrgHeader();
			var cne = Helper.NewOrgHeader();
			var lc = Helper.NewOrgHeader();

			var lcRate = Helper.NewClientRate(lc);
			var lcEntry = lcRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUBNE", "DEFRA");
			lcEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(lcEntry.Currency);
			var lcLine = lcEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			lcLine.GetCalculator<UnitCalculator>().PerUnit = 8;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "DEFRA";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_OH_ExportBroker = lc.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 8000m,
						}
				};

			AutorateAndAssert(expected, shipment, lc);

			var cnrRate = Helper.NewClientRate(cnr);
			var cnrEntry = cnrRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUBNE", "DEFRA");
			cnrEntry.RateLines.RemoveAndDeleteAll();
			var cneLine = cnrEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			cneLine.GetCalculator<UnitCalculator>().PerUnit = 9;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 9000m,
						}
				};

			AutorateAndAssert(expected, shipment, lc);

			shipment.JS_OH_ExportBroker = ZGuid.Empty;

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 8000m,
						}
				};

			AutorateAndAssert(expected, shipment, lc);
		}

		#endregion

		#region Test Overridden Descriptions

		public void TestOverriddenDescriptions()
		{
			var localClient = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(null);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			costingEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(costingEntry.Currency);
			var costingLine1 = costingEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costingLine1.TL_RateDesc = "Description from the costing";
			costingLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;

			var costingLine2 = costingEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costingLine2.TL_RateDesc = "Another costing description";
			costingLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)80m;

			var clientRate = Helper.NewClientRate(localClient);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.TL_RateDesc = "Duplicate Client Charge";
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var rateLine2 = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.TL_RateDesc = "Duplicate Client Charge";
			rateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)200m;

			var rateLine3 = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.TL_RateDesc = rateLine3.ChargeCode.AC_Desc;
			rateLine3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;

			var rateLine4 = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine4.TL_RateDesc = "International Freight 1";
			rateLine4.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)400m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 5m;
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_Desc = @"Duplicate Client Charge
International Freight 1",
					CostCalculationDescription = "This charge is calculated from multiple rates",
				}
			};

			AutorateAndAssert("Multiple descriptions but only from rates (not costs) should be displayed", expected, shipment, localClient);
		}

		#endregion

		#region Ignore Rates With Invalid Currency

		public void TestAutoRate_SomeLinesHaveInvalidCurrency_IgnoreTheseLines()
		{
			var connection = ((IDbConnected)Factory).Connection;
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_CheckCorrectCurrencyOnRateEntry ON RateEntry");
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_CheckCorrectCurrencyOnRateLine ON RateLines");

			var localClient = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(localClient);

			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "UAIEV");
			clientRateEntry.TI_RX_NKCurrency = "BTC";
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "UAIEV";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 5m;
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			Factory.Save();

			AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, localClient);
			AssertAutoratingAuditLogNoteContainsLines(
				shipment,
				"Should contain message about ignored line with invalid currency",
				@"Information: RateEntry Filtered Client Rate TESTORG1 reason: Is invalid due to the following errors:
	• FRT-UNT-KG-Client Rate TESTORG1 - Invalid currency: 'BTC'.");
		}

		#endregion

		#region PaymentTerms

		[TestDate(2015, 02, 02)]
		public void TestChargeAlwaysRegistryPaymentTermIgnoreLoggedIfEncounteredForSell()
		{
			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "CNR";
			var consignee = Helper.NewOrgHeader();
			consignee.OH_Code = "CNE";
			var carrier = Helper.NewOrgHeader();

			var cnrRate = Helper.NewClientRate(consignor);

			var entry = cnrRate.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 5;
			rateLine.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 1m;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			shipment.JS_INCO = "FOB";
			consol.JK_PrepaidCollect = "CCX";

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			AutorateAndAssert(expected, shipment, consignor);

			var expectedLogMessage = new[]
			{
				"Information: RateLine Found FRT-FLT-Client Rate CNR",
				"Information: RateLine Filtered FRT-FLT-Client Rate CNR	reason:	FRT charge group is not applicable for AUSYD-SGSIN Export CCX"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Expected a message", expectedLogMessage);

			IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.Registry.FreightChargeCode.ToString());

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
				},
			};

			var expectedLogLines = @"Information: RateLine Found FRT-FLT-Client Rate CNR
Information: RateLine NOT Filtered FRT-FLT-Client Rate CNR	reason:	FRT is always charged per AutoRating -> Charge Codes -> Charge Local Client Always Charge Codes.";

			AutorateAndAssert(expected, shipment, consignor);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		[TestDate(2015, 02, 02)]
		public void TestChargeAlwaysRegistryPaymentTermIgnoreLoggedIfEncounteredForCosts()
		{
			var costProvider = Helper.NewOrgHeader();
			costProvider.OH_Code = "RAGT";

			var costing = Helper.NewCosting(costProvider);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 5;

			var shipment = CreateForwardingShipment(TransportModes.Air, costProvider.PK, ZGuid.Empty, "AUMEL", "SGSIN", 1m);
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_OA_ReceivingForwarderAddress = costProvider.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			var expectedLogMessage = new[]
			{
				"Information: RateLine Found FRT-FLT-Costing RAGT",
				"Information: RateLine Filtered FRT-FLT-Costing RAGT	reason:	FRT charge group is not applicable for AUMEL-SGSIN Export CCX"
			};

			AutorateAndAssert(expected, shipment, costProvider);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Expected a message", expectedLogMessage);

			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.Registry.FreightChargeCode.ToString());

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 5m,
				},
			};

			var expectedLogLines = new[]
			{
				"Information: RateLine Found FRT-FLT-Costing RAGT",
				"Information: RateLine NOT Filtered FRT-FLT-Costing RAGT	reason:	FRT is always charged per AutoRating -> Charge Codes -> Charge Agent Always Charge Codes."
			};

			AutorateAndAssert(expected, shipment, costProvider);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		public void TestPaymentTermsImport()
		{
			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "AGENT";
			var consignee = Helper.NewOrgHeader();
			consignee.OH_Code = "LOCALCIENT";
			var carrier = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(consignee);
			var consignorRate = Helper.NewClientRate(consignor);

			var leg1RateEntry = clientRate.AddRateEntry("AIR", "LSE", "CNSHA", "SGSIN");
			leg1RateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = leg1RateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 6;
			rateLine1.TL_RX_NKCurrency = "AUD";

			var leg2Entry = clientRate.AddRateEntry("AIR", "LSE", "SGSIN", "AUSYD");
			leg2Entry.RateLines.RemoveAndDeleteAll();
			var rateLine2 = leg2Entry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 5;
			rateLine2.TL_RX_NKCurrency = "AUD";

			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "", "AUSYD");
			orgEntry.RateLines.RemoveAndDeleteAll();
			var orgLine = orgEntry.AddRateLine("ODOC", FlatCalculator.Code);
			orgLine.GetCalculator<FlatCalculator>().BaseRate = 3;
			orgLine.TL_RX_NKCurrency = "AUD";

			var dstEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD");
			dstEntry.RateLines.RemoveAndDeleteAll();
			var dstLine = dstEntry.AddRateLine("DDOC", FlatCalculator.Code);
			dstLine.GetCalculator<FlatCalculator>().BaseRate = 2;
			dstLine.TL_RX_NKCurrency = "AUD";

			var consignorEntry = consignorRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD");
			consignorEntry.RateLines.RemoveAndDeleteAll();
			var consignorLine = consignorEntry.AddRateLine("DADF", FlatCalculator.Code);
			consignorLine.GetCalculator<FlatCalculator>().BaseRate = 4;
			consignorLine.TL_RX_NKCurrency = "AUD";

			var cost = Helper.NewCosting(carrier);

			var leg1CostEntry = cost.AddRateEntry("AIR", "LSE", "CNSHA", "SGSIN");
			leg1CostEntry.RateLines.RemoveAndDeleteAll();
			var costLine1 = leg1CostEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLine1.GetCalculator<FlatCalculator>().BaseRate = 13;
			costLine1.TL_RX_NKCurrency = "AUD";

			var leg2CostEntry = cost.AddRateEntry("AIR", "LSE", "SGSIN", "AUSYD");
			leg2CostEntry.RateLines.RemoveAndDeleteAll();
			var costLine2 = leg2CostEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLine2.GetCalculator<FlatCalculator>().BaseRate = 20;
			costLine2.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 1m;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			shipment.JS_INCO = "CPT";
			consol.JK_PrepaidCollect = "PPD";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DADF",
					JR_OSSellAmt = 4m
				}
			};

			AutorateAndAssert("Incoterm and paymentterm all prepaid, DST charges are agent payable and are local service, ORG are not local so not rated", expected, shipment, consignee);

			shipment.JS_INCO = "CIF";
			consol.JK_PrepaidCollect = "PPD";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2m,
				},
			};

			AutorateAndAssert("Incoterm and paymentterm all prepaid, DST is local client payable", expected, shipment, consignee);

			shipment.JS_INCO = "FOB";
			consol.JK_PrepaidCollect = "PPD";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2m,
				},
			};

			AutorateAndAssert("Incoterm collect, paymentterm prepaid - FRT not filtered out", expected, shipment, consignee);

			shipment.JS_INCO = "CPT";
			consol.JK_PrepaidCollect = string.Empty;

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 13m,
					JR_SellRated = false
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 20m,
					JR_SellRated = false
				},
				new AssertionCharge
				{
					ChargeCode = "DADF",
					JR_OSSellAmt = 4m
				}
			};

			AutorateAndAssert("Incoterm is prepaid and paymentterm is blank - FRT not filtered out", expected, shipment, consignee);

			var importCollection = RatingDataRegistry.Instance.ImportPrepaidPriorities.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			importCollection.AddNew(RatingDebtorOrgTypes.LC);
			RatingDataRegistry.Instance.ImportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, importCollection);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 13m,
					JR_OSSellAmt = 5m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 20m,
					JR_OSSellAmt = 6m
				},
				new AssertionCharge
				{
					ChargeCode = "DADF",
					JR_OSSellAmt = 4m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2m
				},
			};

			AutorateAndAssert("Incoterm is prepaid and paymentterm is blank - FRT not filtered out, LC added to sell rate priority so its rates are pulled over", expected, shipment, consignee);

			shipment.JS_INCO = "CIF";
			consol.JK_PrepaidCollect = "CCX";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2m
				},
			};

			AutorateAndAssert("Incoterm is prepaid and paymentterm is collect - FRT not filtered out, DADF filtered out due to CNR not in Import CLT sell rates priority", expected, shipment, consignee);

			shipment.JS_INCO = string.Empty;
			consol.JK_PrepaidCollect = "PPD";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2m
				},
				new AssertionCharge
				{
					ChargeCode = "DADF",
					JR_OSSellAmt = 4m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 3m
				},
			};

			AutorateAndAssert("Incoterm is blank and paymentterm is prepaid, due to blank incoterm all charges apply", expected, shipment, consignee);

			shipment.JS_INCO = "FOB";
			consol.JK_PrepaidCollect = "CCX";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2m
				},
			};

			AutorateAndAssert("Incoterm and paymentterm are all FRT collect", expected, shipment, consignee);

			shipment.JS_INCO = string.Empty;
			consol.JK_PrepaidCollect = string.Empty;

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2m
				},
				new AssertionCharge
				{
					ChargeCode = "DADF",
					JR_OSSellAmt = 4m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 3m
				},
			};

			AutorateAndAssert("All blank - all applies", expected, shipment, consignee);
		}

		public void TestPaymentTermsExport()
		{
			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "LOCALCIENT";
			var consignee = Helper.NewOrgHeader();
			consignee.OH_Code = "AGENT";
			var carrier = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(consignor);
			var agentRate = Helper.NewClientRate(consignee);

			var leg1ClientEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL");
			leg1ClientEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = leg1ClientEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 6;
			rateLine1.TL_RX_NKCurrency = "AUD";

			var leg2ClientEntry = clientRate.AddRateEntry("AIR", "LSE", "AUMEL", "SGSIN");
			leg2ClientEntry.RateLines.RemoveAndDeleteAll();
			var rateLine2 = leg2ClientEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 5;
			rateLine2.TL_RX_NKCurrency = "AUD";

			var leg1AgentEntry = agentRate.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL");
			leg1AgentEntry.RateLines.RemoveAndDeleteAll();
			var agentRateLine1 = leg1AgentEntry.AddRateLine("FRT", FlatCalculator.Code);
			agentRateLine1.GetCalculator<FlatCalculator>().BaseRate = 60;
			agentRateLine1.TL_RX_NKCurrency = "AUD";

			var leg2AgentEntry = agentRate.AddRateEntry("AIR", "LSE", "AUMEL", "SGSIN");
			leg2AgentEntry.RateLines.RemoveAndDeleteAll();
			var agentRateLine2 = leg2AgentEntry.AddRateLine("FRT", FlatCalculator.Code);
			agentRateLine2.GetCalculator<FlatCalculator>().BaseRate = 50;
			agentRateLine2.TL_RX_NKCurrency = "AUD";

			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "SGSIN");
			orgEntry.RateLines.RemoveAndDeleteAll();
			var orgLine = orgEntry.AddRateLine("ODOC", FlatCalculator.Code);
			orgLine.GetCalculator<FlatCalculator>().BaseRate = 3;
			orgLine.TL_RX_NKCurrency = "AUD";

			var dstEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "AUSYD", "SGSIN");
			dstEntry.RateLines.RemoveAndDeleteAll();
			var dstLine = dstEntry.AddRateLine("DDOC", FlatCalculator.Code);
			dstLine.GetCalculator<FlatCalculator>().BaseRate = 2;
			dstLine.TL_RX_NKCurrency = "AUD";

			var dstAgentEntry = agentRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "AUSYD", "");
			dstAgentEntry.RateLines.RemoveAndDeleteAll();
			var dstAgentLine = dstAgentEntry.AddRateLine("DADF", FlatCalculator.Code);
			dstAgentLine.GetCalculator<FlatCalculator>().BaseRate = 4;
			dstAgentLine.TL_RX_NKCurrency = "AUD";

			var cost = Helper.NewCosting(carrier);

			var leg1CostEntry = cost.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL");
			leg1CostEntry.RateLines.RemoveAndDeleteAll();
			var costLine1 = leg1CostEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLine1.GetCalculator<FlatCalculator>().BaseRate = 13;
			costLine1.TL_RX_NKCurrency = "AUD";

			var leg2CostEntry = cost.AddRateEntry("AIR", "LSE", "AUMEL", "SGSIN");
			leg2CostEntry.RateLines.RemoveAndDeleteAll();
			var costLine2 = leg2CostEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLine2.GetCalculator<FlatCalculator>().BaseRate = 20;
			costLine2.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 1m;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			shipment.JS_INCO = "FOB";
			consol.JK_PrepaidCollect = "CCX";

			Factory.Save();

			var expected = new[]
			{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 60,
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 3,
					},
			};

			AutorateAndAssert("Incoterm and paymentterm are all collect, FRT not charged because it's export collect, but the local FRT leg is charged", expected, shipment, consignor);

			shipment.JS_INCO = "FOB";
			consol.JK_PrepaidCollect = "PPD";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 50m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 3,
				},
			};

			AutorateAndAssert("Incoterm is collect and paymentterm is prepaid", expected, shipment, consignor);

			shipment.JS_INCO = "FOB";
			consol.JK_PrepaidCollect = string.Empty;

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 50m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 3,
				},
			};

			AutorateAndAssert("Incoterm is collect and paymentterm is blank", expected, shipment, consignor);

			shipment.JS_INCO = "CPT";
			consol.JK_PrepaidCollect = "CCX";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 3,
				},
			};

			AutorateAndAssert("Incoterm is prepaid (sell FRT rate from LC) and paymentterm is collect", expected, shipment, consignor);

			shipment.JS_INCO = string.Empty;
			consol.JK_PrepaidCollect = "CCX";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 50m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 6m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m,
					JR_OSCostAmt = 60m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 3,
				},
				new AssertionCharge
				{
					ChargeCode = "DADF",
					JR_OSSellAmt = 4,
				},
			};

			AutorateAndAssert("Incoterm is blank (causing everything to apply) and paymentterm is collect", expected, shipment, consignor);

			shipment.JS_INCO = "CIF";
			consol.JK_PrepaidCollect = "PPD";

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 3,
				},
			};

			AutorateAndAssert("All prepaid. For DST it is collect and due to empty destination on the rate is assumes it may be 1st local AUSYD-AUMEL leg - so DADF is charged to Agent", expected, shipment, consignor);

			shipment.JS_INCO = string.Empty;
			consol.JK_PrepaidCollect = string.Empty;

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 5m,
					JR_OSCostAmt = 13m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 50m,
					JR_OSCostAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6m,
					JR_OSCostAmt = 6m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m,
					JR_OSCostAmt = 60m
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 3,
				},
				new AssertionCharge
				{
					ChargeCode = "DADF",
					JR_OSSellAmt = 4,
				},
			};

			AutorateAndAssert("All blank - all applies", expected, shipment, consignor);
		}

		public void TestPaymentTerms_Forwarding_Domestic_Containerized()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OAQF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DAQF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			var containerRef = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;

			var localClient = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(localClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "AUFRE", "ODOC", 10);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "AUSYD", "AUFRE", "DDOC", 12);
			var fclClientRate = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "AUFRE", "FRT", 15);
			fclClientRate.TI_RC = containerRef;

			var carrier = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(carrier);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "AUFRE", "OAQF", 1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "AUSYD", "AUFRE", "DAQF", 2);
			var fclCostRate = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "AUFRE", "BAF", 3);
			fclCostRate.TI_RC = containerRef;
			AddParityExchangeRate(fclCostRate.Currency);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.ConsignorPK = localClient.PK;
			shipment.ConsigneePK = Helper.NewOrgHeader().PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUFRE";
			shipment.JS_ActualWeight = 1000m;

			shipment.JS_INCO = DomesticPaymentTerms.Collect;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			var container = consol.Containers.AddNew();
			container.JC_RC = containerRef;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 10 },
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 12 },
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 15 },

				new AssertionCharge { ChargeCode = "OAQF", JR_OSCostAmt = 1 },
				new AssertionCharge { ChargeCode = "DAQF", JR_OSCostAmt = 2 },
				new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 3 }
			};

			AutorateAndAssert("Domestic jobs shouldn't filter charge codes by payment term", expected, shipment, localClient);

			shipment.JS_INCO = DomesticPaymentTerms.Prepaid;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			AutorateAndAssert("Domestic jobs shouldn't filter charge codes by payment term", expected, shipment, localClient);
		}

		public void TestPaymentTerms_Forwarding_Domestic_Loose()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");

			var consignor = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignor);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AUSYD", "AUMEL", "ODOC", 10);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "FRT", 15);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "AUMEL", "DDOC", 12);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUSYD", "AUMEL", 0m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 10m },
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 15m },
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 12m }
			};

			AutorateAndAssert("All charges should apply to collect domestic shipment", expected, shipment, consignor);

			shipment.JS_INCO = DomesticPaymentTerms.Prepaid;

			AutorateAndAssert("Expected all charges to still apply as the mode is domestic", expected, shipment, consignor);
		}

		#endregion

		#region TestRateGroupSecurityDeniesBillingEntirely

		public void TestRateGroupSecurityDeniesBillingEntirely()
		{
			var rateSecurities = new CodeDescriptionPairList();
			rateSecurities.AddPair("ABC", "ABC Description");
			rateSecurities.AddPair("XYZ", "XYZ Description");

			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(rateSecurities));

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_LoginName = "test staff";
			staff.GS_Code = "TTS";

			var glbSecurity1 = Factory.New<GlbSecurity>();
			glbSecurity1.GU_SecurityItemIsAllowed = false;
			glbSecurity1.GU_SecurityRight = Env.Security.RatesSecurity.Code + "ABC";
			glbSecurity1.GU_GS = staff.PK;

			var glbSecurity2 = Factory.New<GlbSecurity>();
			glbSecurity2.GU_SecurityItemIsAllowed = true;
			glbSecurity2.GU_SecurityRight = Env.Security.RatesSecurity.Code + "XYZ";
			glbSecurity2.GU_GS = staff.PK;

			var glbSecurity3 = Factory.New<GlbSecurity>();
			glbSecurity3.GU_SecurityItemIsAllowed = true;
			glbSecurity3.GU_SecurityRight = Env.Security.Operations.Code;
			glbSecurity3.GU_GS = staff.PK;

			var deniedOrg = Factory.NewWithValidTestData<OrgHeader>();
			deniedOrg.CompanyData.OB_RateSecurityGroup = "ABC";

			var allowedOrg = Factory.NewWithValidTestData<OrgHeader>();
			allowedOrg.CompanyData.OB_RateSecurityGroup = "XYZ";

			var receive1 = Factory.NewWithValidTestData<WhsReceive>();
			receive1.WD_OH_Client = deniedOrg.PK;
			var receive1Job = new JobHeader.Loader(receive1).TryLoadOrCreate();

			var receive2 = Factory.NewWithValidTestData<WhsReceive>();
			receive2.WD_OH_Client = allowedOrg.PK;
			var receive2Job = new JobHeader.Loader(receive2).TryLoadOrCreate();

			AssertEquals(deniedOrg.MainAddress.PK, receive1Job.JH_OA_LocalChargesAddr);
			AssertEquals(allowedOrg.MainAddress.PK, receive2Job.JH_OA_LocalChargesAddr);

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment1Job = new JobHeader.Loader(shipment1).TryLoadOrCreate();
			shipment1Job.JH_OA_LocalChargesAddr = deniedOrg.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2Job = new JobHeader.Loader(shipment2).TryLoadOrCreate();
			shipment2Job.JH_OA_LocalChargesAddr = allowedOrg.MainAddress.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var printingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>();
				printingHelper.PrintJobProfitDocument(Factory, new[] { shipment1 });

				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Tariffs & Rates -> Rates' Security -> ABC", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				printingHelper.PrintJobProfitDocument(Factory, new[] { shipment2 });

				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Are you sure you want to print Job Profit Document"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var postingHelper = ObjectFactory.Get<IBulkPostingModuleHelper>();
				postingHelper.PostTransactions(JobInvoicingPostingOption.Revenue, new[] { receive2 });

				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Are you sure you want to Post All Revenue Charges"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				postingHelper.PostTransactions(JobInvoicingPostingOption.Revenue, new[] { receive1 });

				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Tariffs & Rates -> Rates' Security -> ABC", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Assert(!(shipment1 as IJobInvoicingPlugIn).InvoicingSupporter.JobInvoicingSecurity.IsAllowed);
				Assert(!(shipment1 as IJobInvoicingPlugIn).InvoicingSupporter.AuditSecurity.IsAllowed);
				Assert(!(shipment1 as IJobInvoicingPlugIn).InvoicingSupporter.EditSecurityCheckpoint.IsAllowed);
				Assert((shipment2 as IJobInvoicingPlugIn).InvoicingSupporter.JobInvoicingSecurity.IsAllowed);
				Assert((shipment2 as IJobInvoicingPlugIn).InvoicingSupporter.AuditSecurity.IsAllowed);
				Assert((shipment2 as IJobInvoicingPlugIn).InvoicingSupporter.EditSecurityCheckpoint.IsAllowed);

				Assert(!(receive1 as IJobInvoicingPlugIn).InvoicingSupporter.JobInvoicingSecurity.IsAllowed);
				Assert(!(receive1 as IJobInvoicingPlugIn).InvoicingSupporter.AuditSecurity.IsAllowed);
				Assert(!(receive1 as IJobInvoicingPlugIn).InvoicingSupporter.EditSecurityCheckpoint.IsAllowed);
				Assert((receive2 as IJobInvoicingPlugIn).InvoicingSupporter.JobInvoicingSecurity.IsAllowed);
				Assert((receive2 as IJobInvoicingPlugIn).InvoicingSupporter.AuditSecurity.IsAllowed);
				Assert((receive2 as IJobInvoicingPlugIn).InvoicingSupporter.EditSecurityCheckpoint.IsAllowed);
			}
		}

		#endregion

		#region Fall Back Rate Matcher On Container Type (FTL)

		public void TestFallBackRateMatcherOnContainerType()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FSC");

			var truckReference = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "VANT"));
			AssertNotNull("Pre-condition", truckReference);

			var consignee = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var genericRateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.FTL, "USLAX", "AU");
			genericRateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(genericRateEntry.Currency);

			var genericFRTRateLine = genericRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			genericFRTRateLine.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var genericFSCRateLine = genericRateEntry.AddRateLine("FSC", FlatCalculator.Code);
			genericFSCRateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var vehicleSpecificFSCRateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.FTL, "USLAX", "AU", ZString.Empty, truckReference.RC_Code);
			vehicleSpecificFSCRateEntry.RateLines.RemoveAndDeleteAll();

			var vehicleSpecificFSCRateLine = vehicleSpecificFSCRateEntry.AddRateLine("FSC", FlatCalculator.Code);
			vehicleSpecificFSCRateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var nonApplicableRateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.FTL, "USLAX", "AU", ZString.Empty, "DUMP");
			nonApplicableRateEntry.RateLines.RemoveAndDeleteAll();

			var nonApplicableRateLine = nonApplicableRateEntry.AddRateLine("WAR", FlatCalculator.Code);
			nonApplicableRateLine.GetCalculator<FlatCalculator>().BaseRate = 400m;

			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = ContainerModes.FTL;
			container.JC_RC = truckReference.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_PackingMode = ContainerModes.FTL;
			shipment.JS_INCO = "CLT";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = client.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 50m,
							RevenueCalculationDescription = "FRT: 1 Container(s) @ USD 50.00/Container"
						},
					new AssertionCharge
						{
							ChargeCode = "FSC",
							JR_OSSellAmt = 200m,
							RevenueCalculationDescription = "FSC: Base Rate USD 200.00"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region AutoRateWithTeuUnit

		public void TestAutoRateWithTeuUnit()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUBNE", "USLAX", ZString.Empty, "40GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.TU);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1900.1234m;
			AddParityExchangeRate(rateLine.Currency);

			var bill = Factory.NewWithValidTestData<BillOfLading>();
			bill.JS_TransportMode = TransportModes.Sea;
			bill.JS_PackingMode = ContainerModes.FCL;
			bill.JS_INCO = "CLT";
			bill.JS_RL_NKOrigin = "AUBNE";
			bill.JS_RL_NKDestination = "USLAX";
			bill.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			bill.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			var container = bill.ShippingContainers.AddNew();
			container.JC_RC = GP40.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 3800.25m,
						}
				};

			AutorateAndAssert(expected, bill, localClient);
		}

		public void TestAutoRateBookingWithQuoteWhenTEUUnitUsed()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var rate = Helper.NewClientRate(NewClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RC = GP40.PK;
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.TU, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 220m;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			booking.ConsignorDocumentaryAddress.OrganisationPK = NewClient.PK;
			booking.JS_TransportMode = TransportModes.Sea;
			booking.JS_PackingMode = ContainerModes.FCL;
			booking.JS_INCO = IncoTerms.CostAndFreight;
			booking.JS_RL_NKOrigin = "AUBNE";
			booking.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			var bookingWithQuote = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var container = bookingWithQuote.QuotedBookingContainers.AddNew();
			container.JC_RC = GP40.PK;

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 440m
						}
				};

			AutorateAndAssert(expected, bookingWithQuote, NewClient);
		}

		public void TestContainerBasedRatesAppliedPerActualCapacityOfShipment_PerTEU()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OITF");

			var consignee1 = Helper.NewOrgHeader(1);
			var consignee2 = Helper.NewOrgHeader(2);

			var rate = Helper.NewClientRate(NewClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AUSYD", "", ZString.Empty, "20GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("OITF", UnitCalculator.Code, QuantityUnit.TU);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT00001";
			container1.JC_RC = GP20.PK;
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT00002";
			container2.JC_RC = GP20.PK;

			var shipment1 = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, consignee1.PK, "AUSYD", "USLAX", 0m, 0m, consol);
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee1.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 150m;
			packline1_1.JL_ActualWeight = 100m;
			packline1_1.JL_JC = container1.PK;

			var packline1_2 = shipment1.OuterPackLines.AddNew();
			packline1_2.JL_ActualVolume = 4m;
			packline1_2.JL_ActualWeight = 200m;
			packline1_2.JL_JC = container2.PK;

			var shipment2 = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, consignee2.PK, "AUSYD", "USLAX", 0m, 0m, consol);
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee2.PK;

			var packline2_1 = shipment2.OuterPackLines.AddNew();
			packline2_1.JL_ActualVolume = 23m;
			packline2_1.JL_ActualWeight = 300m;
			packline2_1.JL_JC = container1.PK;

			var packline2_2 = shipment2.OuterPackLines.AddNew();
			packline2_2.JL_ActualVolume = 23m;
			packline2_2.JL_ActualWeight = 300m;
			packline2_2.JL_JC = container2.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 200m,
						RevenueCalculationDescription = "OITF: 2 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit"
					}
			};

			AutorateAndAssert(expected, shipment1, NewClient);

			rateLine.UseOnlyActualWeightMeasure = true;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 98.48m,
						RevenueCalculationDescription = "OITF: 0.9848 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit"
					}
			};

			AutorateAndAssert("Shipment2 occupied 2/3 of container1 and 1/3 of container2", expected, shipment2, NewClient);
		}

		public void TestContainerBasedRatesAppliedPerActualCapacityOfShipment()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OITF");

			var consignee1 = Helper.NewOrgHeader(1);
			var consignee2 = Helper.NewOrgHeader(2);

			var rate = Helper.NewClientRate(NewClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AUSYD", "", ZString.Empty, "20GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("OITF", UnitCalculator.Code, QuantityUnit.CN);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT00001";
			container.JC_RC = GP20.PK;

			var shipment1 = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, consignee1.PK, "AUSYD", "USLAX", 0m, 0m, consol);
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee1.PK;

			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.JL_ActualVolume = 10m;
			packline1.JL_ActualWeight = 100m;
			packline1.JL_JC = container.PK;

			var shipment2 = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, consignee2.PK, "AUSYD", "USLAX", 0m, 0m, consol);
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee2.PK;

			var packline2 = shipment2.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 40m;
			packline2.JL_ActualWeight = 300m;
			packline2.JL_JC = container.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "OITF: 1 20GP Container(s) @ AUD 10.00/Container"
					}
			};

			AutorateAndAssert(expected, shipment1, NewClient);

			rateLine.UseOnlyActualWeightMeasure = true;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 2m,
						RevenueCalculationDescription = "OITF: 0.2 20GP Container(s) @ AUD 10.00/Container"
					}
			};

			AutorateAndAssert(expected, shipment1, NewClient);

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 8m,
						RevenueCalculationDescription = "OITF: 0.8 20GP Container(s) @ AUD 10.00/Container"
					}
			};

			AutorateAndAssert(expected, shipment2, NewClient);
		}

		public void TestContainerBasedRatesAppliedPerActualCapacityOfShipment_OneShipment2Containers()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OITF");

			var localClient = Helper.NewOrgHeader();
			var consignee1 = Helper.NewOrgHeader(1);
			var consignee2 = Helper.NewOrgHeader(2);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AUSYD", "", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("OITF", UnitCalculator.Code, QuantityUnit.TU);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var container20GP = consol.Containers.AddNew();
			container20GP.JC_ContainerNum = "CONT00001";
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container40GP.JC_ContainerNum = "CONT00002";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee1.PK;
			shipment1.ConsignorPK = localClient.PK;
			shipment1.ConsigneePK = consignee1.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 150m;
			packline1_1.JL_ActualWeight = 100m;
			packline1_1.JL_JC = container20GP.PK;

			var packline1_2 = shipment1.OuterPackLines.AddNew();
			packline1_2.JL_ActualVolume = 0.05m;
			packline1_2.JL_ActualWeight = 10m;
			packline1_2.JL_JC = container40GP.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee2.PK;
			shipment2.ConsignorPK = localClient.PK;
			shipment2.ConsigneePK = consignee2.PK;

			var packline2_2 = shipment2.OuterPackLines.AddNew();
			packline2_2.JL_ActualVolume = 1000m;
			packline2_2.JL_ActualWeight = 100m;
			packline2_2.JL_JC = container40GP.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 300m,
						RevenueCalculationDescription = "OITF: 3 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit"
					}
			};

			AutorateAndAssert(expected, shipment1, localClient);

			rateLine.UseOnlyActualWeightMeasure = true;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 100.00m,
						RevenueCalculationDescription = "OITF: More than 1.0000 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit"
					}
			};

			AutorateAndAssert("Shipment1 occupied whole container1 and a bit of container2", expected, shipment1, localClient);

			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 1m;
			packline1_1.JL_ActualWeight = 9999m;
			packline1_1.JL_JC = container20GP.PK;

			var packline2_1 = shipment2.OuterPackLines.AddNew();
			packline2_1.JL_ActualVolume = 1m;
			packline2_1.JL_ActualWeight = 0.005m;
			packline2_1.JL_JC = container20GP.PK;

			packline2_2 = shipment2.OuterPackLines.AddNew();
			packline2_2.JL_ActualVolume = 1m;
			packline2_2.JL_ActualWeight = 10000m;
			packline2_2.JL_JC = container40GP.PK;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 100.00m,
						RevenueCalculationDescription = "OITF: Less than 1.0000 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit"
					}
			};

			AutorateAndAssert("Shipment1 occupied slightly less than 1 TEU", expected, shipment1, localClient);

			var decimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ORG", Decimals = "2" }
			};

			Factory.Save();

			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, decimals))
			{
				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "OITF",
						JR_OSSellAmt = 100.00m,
						RevenueCalculationDescription = "OITF: Less than 1.0000 Twenty foot equivalent unit(s) @ AUD 100.00/Twenty foot equivalent unit"
					}
				};

				AutorateAndAssert("Shipment1 occupied almost all container 1", expected, shipment1, localClient);
			}
		}

		public void TestContainerBasedRatesAppliedPerActualCapacityOfShipment_TwoConsolsAttached()
		{
			var rate = Helper.NewClientRate(NewClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "NZ", "AU", ZString.Empty, "20GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "NZWLF", "AUCBR", 45000m, 10m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_OH_DeliveryAgent = Consignee.PK;
			shipment.JS_INCO = "FOB";
			shipment.JS_OuterPacks = 1;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "AUMEL";

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			container1.JC_RC = GP20.PK;

			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "2";
			container2.JC_RC = GP20.PK;

			var container3 = consol1.Containers.AddNew();
			container3.JC_ContainerNum = "3";
			container3.JC_RC = GP20.PK;

			var containerA = consol2.Containers.AddNew();
			containerA.JC_ContainerNum = "A";
			containerA.JC_RC = GP20.PK;

			var containerB = consol2.Containers.AddNew();
			containerB.JC_ContainerNum = "B";
			containerB.JC_RC = GP20.PK;

			var containerC = consol2.Containers.AddNew();
			containerC.JC_ContainerNum = "C";
			containerC.JC_RC = GP20.PK;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualVolume = 10m;
			packline1.JL_ActualWeight = 700m;
			packline1.JL_JC = container2.PK;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 50m;
			packline2.JL_ActualWeight = 8000m;
			packline2.JL_JC = containerB.PK;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_ActualVolume = 30m;
			packline3.JL_ActualWeight = 1000m;
			packline3.JL_JC = container1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
					RevenueCalculationDescription = "FRT: 3 20GP Container(s) @ AUD 10.00/Container"
				}
			};

			AutorateAndAssert("shipment is packed into 3 containers", expected, shipment, NewClient);

			rateLine.UseOnlyActualWeightMeasure = true;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 10.00/Container"
				}
			};

			AutorateAndAssert("shipment is packed into 3 containers but based on the weight/volume it used only 2", expected, shipment, NewClient);
		}

		#endregion

		#region CTG Calculator Options

		public void TestCTGCalculatorOptions()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(consignee);
			var entry = rate.AddRateEntry("ORG", "LSE", "USLAX", "AUBNE");
			entry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(entry.Currency);

			var line = entry.AddRateLine("ODOC", "CTG", "KG");
			var calc = line.GetCalculator<CartageCalculator>();
			calc["-100"] = (ZDecimal)10m;
			calc["+100"] = (ZDecimal)5m;
			calc["+200"] = (ZDecimal)4m;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 90m, 0.1m);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt = 900.00m,
						}
				};

			AutorateAndAssert(expected, shipment, consignee);

			calc.UseHigherChargeableLowerRateRule = true;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 500.00m,
					}
			};

			AutorateAndAssert(expected, shipment, consignee);

			calc.UseHigherChargeableLowerRateRule = false;
			shipment.JS_ActualWeight = 210m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 840.00m,
					}
			};

			AutorateAndAssert(expected, shipment, consignee);

			calc.IsAccumulated = true;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 1540.00m,
					}
			};

			AutorateAndAssert(expected, shipment, consignee);

			calc.IsAccumulated = false;
			shipment.JS_ActualWeight = 100m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 500.00m,
					}
			};

			AutorateAndAssert(expected, shipment, consignee);

			calc.UseInclusiveBreaks = true;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 1000.00m,
					}
			};

			AutorateAndAssert(expected, shipment, consignee);

			shipment.JS_ActualWeight = 100m;
			calc.UseInclusiveBreaks = false;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 500.00m,
					}
			};

			AutorateAndAssert(expected, shipment, consignee);

			calc.UseInclusiveBreaks = true;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 1000.00m,
					}
			};

			AutorateAndAssert(expected, shipment, consignee);
		}

		#endregion

		#region CTZ Calculator Options

		[TestDate(2016, 02, 10)]
		public void TestCTZCalculatorUsesCorrectZoneSetForClientRate()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OCART");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DCART");

			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "SUPPLIER";
			var consignee = Helper.NewOrgHeader();
			consignee.OH_Code = "BUYER";
			var deliveryAddress = consignee.MainAddress;
			deliveryAddress.OA_PostCode = "3000";
			deliveryAddress.OA_State = "VIC";
			deliveryAddress.OA_City = "Melbourne";

			var sydPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var melPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "3000"));

			var auZoneSet = Helper.CreateRateTransportZoneSet(consignor, CountryCodes.Australia);
			var auZone = auZoneSet.CreateRateTransportZoneForTest("AU Zone");
			auZone.CreateRateTransportZoneItemForTest(sydPostCode, melPostCode);

			var sydZoneSet = Helper.CreateRateTransportZoneSet(consignor, "", Helper.GetCityTown("Sydney", "NSW"));
			var sydZone = sydZoneSet.CreateRateTransportZoneForTest("Syd Zone");
			sydZone.CreateRateTransportZoneItemForTest(sydPostCode);

			var melZoneSet = Helper.CreateRateTransportZoneSet(consignor, "", Helper.GetCityTown("Melbourne", "Vic"));
			var melZone = melZoneSet.CreateRateTransportZoneForTest("Mel Zone");
			melZone.CreateRateTransportZoneItemForTest(melPostCode);

			var clientRate = Helper.NewClientRate(consignor);
			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AUSYD", "AUMEL");
			var orgLine = orgEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var dstEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "AUMEL");
			var dstLine = dstEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("Pre-condition: zone should be displaying city town specific zone", sydZone.TZ_ZoneName, orgLine.Lookups.Zones[0].Code);
			AssertEquals(melZone.TZ_ZoneName, dstLine.Lookups.Zones[0].Code);

			var originCalculator = orgLine.GetCalculator<CartageZoneDistanceCalculator>();
			originCalculator.EquipmentType = EquipmentNeeded.Any;
			originCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, sydZone.PK);

			var dstCalculator = dstLine.GetCalculator<CartageZoneDistanceCalculator>();
			dstCalculator.EquipmentType = EquipmentNeeded.Any;
			dstCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 30m, melZone.PK);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "AUMEL", 10m);
			shipment.JS_UniqueConsignRef = "ShippyMcShipFace";

			Factory.Save();

			var message = "Expected to match the City Town specific Zones rather than the Australian Zone as there is enough info in the UNLOCO to match";

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OCART",
						JR_OSSellAmt = 100m,
						RevenueCalculationDescription = "OCART: 10 Kilogram(s) @ AUD 10.00/KG"
					},
				new AssertionCharge
					{
						ChargeCode = "DCART",
						JR_OSSellAmt = 300m,
						RevenueCalculationDescription = "DCART: 10 Kilogram(s) @ AUD 30.00/KG"
					}
			};

			var expectedLogLines = new[] { @"Information: Matched 'Mel Zone' for RateLine DCART-CTZ-KG-Client Rate SUPPLIER
	- Delivery Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Mel Zone' transport zone matched by Consignee Pickup/Delivery Address fallback",
@"Information: Matched 'Syd Zone' for RateLine OCART-CTZ-KG-Client Rate SUPPLIER
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Syd Zone' transport zone matched by Consignor Pickup/Delivery Address fallback" };

			AutorateAndAssert(message, expected, shipment, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain distance calculation details", expectedLogLines);
		}

		[TestDate(2016, 01, 10)]
		public void TestCTZCalculatorUsesCorrectZoneSetForClientRate_FallsbackToStandardZone()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OCART");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");

			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "SUPPLIER";

			var sydPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));

			var auZoneSet = Helper.CreateRateTransportZoneSet(consignor, CountryCodes.Australia);
			var auZone = auZoneSet.CreateRateTransportZoneForTest("AU Zone");
			auZone.CreateRateTransportZoneItemForTest(sydPostCode);

			var clientRate = Helper.NewClientRate(consignor);
			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AUSYD", "");
			var zoneLine = orgEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var standardLine = orgEntry.AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var calculator = zoneLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, auZone.PK);

			calculator = standardLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 30m, ZGuid.Empty);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, Helper.NewOrgHeader().PK, "AUSYD", "CNSHA", 10m);
			shipment.JS_UniqueConsignRef = "SH00038";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OCART",
						JR_OSSellAmt = 100m,
						RevenueCalculationDescription = @"OCART: 10 Kilogram(s) @ AUD 10.00/KG
" + DescriptionHelpers.FormatWithTab("Zone:") + "SUPPLIER AU Zone"
					},
				new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 300m,
						RevenueCalculationDescription = @"ODOC: 10 Kilogram(s) @ AUD 30.00/KG
" + DescriptionHelpers.FormatWithTab("Zone:") + "Standard"
					}
			};

			var expectedLogLines = new[] { @"Information: Matched 'AU Zone' for RateLine OCART-CTZ-KG-Client Rate SUPPLIER
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'AU Zone' transport zone matched by Consignor Pickup/Delivery Address fallback",
@"Information: Matched 'Standard' for RateLine ODOC-CTZ-KG-Client Rate SUPPLIER
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'AU Zone' transport zone matched by Consignor Pickup/Delivery Address fallback
	- 'AU Zone' transport zone cannot be used as there are no rates set on the calculator
	- 'Standard' fall back used" };

			AutorateAndAssert(expected, shipment, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain distance calculation details", expectedLogLines);
		}

		[TestDate(2016, 02, 10)]
		public void TestCTZCalculator_DestinationUsesDeliveryAddress()
		{
			var sydPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var auZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia);
			var auZone = auZoneSet.CreateRateTransportZoneForTest("AU Zone");
			auZone.CreateRateTransportZoneItemForTest(sydPostCode);

			var deliveryConsignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(deliveryConsignee);
			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "", "AUSYD");
			var zoneLine = orgEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var calculator = zoneLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, auZone.PK);

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, deliveryConsignee.PK, "CNSHA", "AUSYD", 10m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 100m,
						RevenueCalculationDescription = @"DCART: 10 Kilogram(s) @ AUD 10.00/KG
" + DescriptionHelpers.FormatWithTab("Zone:") + "AU Zone"
					}
			};

			AutorateAndAssert("Expected to sydney postcode from delivery address", expected, shipment, deliveryConsignee);
		}

		[TestDate(2016, 06, 06)]
		public void TestCTZCalculator_ZoneSetMatchesRateEntryInsteadOfJob_CountrySpecific()
		{
			var sydneyCityTown = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			AssertNotNull("Pre-condition", sydneyCityTown);

			var australianZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia);
			var auZone = australianZoneSet.CreateRateTransportZoneForTest("AU Zone");
			auZone.CreateRateTransportZoneItemForTest(sydneyCityTown);

			var sydneyZoneSet = Helper.CreateRateTransportZoneSet(null, "", sydneyCityTown);
			var sydZone = sydneyZoneSet.CreateRateTransportZoneForTest("Syd Zone");
			sydZone.CreateRateTransportZoneItemForTest(sydneyCityTown);

			var deliveryConsignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(deliveryConsignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "", "AU");
			var rateLine = rateEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, auZone.PK);

			Factory.Save();

			var expectedZones = new ZString[] { "Standard", "AU Zone" };
			var actualZones = calculator.CartageZones.Cast<CartageZone>().Select(x => x.Description);
			AssertContainsExactElementsInAnyOrder("CTZ calc should not include Syd zone as Rate Entry is not sydney specific", expectedZones, actualZones);

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, deliveryConsignee.PK, "CNSHA", "AUSYD", 10m);
			AssertEquals("Shipment has more address details than rate so should match city town based zone set", "Syd Zone", shipment.JS_Calc_DeliveryCartageZone);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 100m,
						RevenueCalculationDescription = "DCART: 10 Kilogram(s) @ AUD 10.00/KG"
					}
			};

			var expectedLogLines = @"Information: Matched 'AU Zone' for RateLine DCART-CTZ-KG-Client Rate TESTORG1
	- Delivery Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'AU Zone' transport zone matched by Consignee Pickup/Delivery Address fallback";

			AutorateAndAssert("Matched Sydney from delivery address, should have matched against zone set on rate", expected, shipment, deliveryConsignee);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain zone match description", expectedLogLines);
		}

		[TestDate(2016, 06, 06)]
		public void TestCTZCalculator_ZoneSetMatchesRateEntryInsteadOfJob_CityTownSpecific()
		{
			var sydneyCityTown = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			AssertNotNull("Pre-condition", sydneyCityTown);

			var australianZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia);
			var auZone = australianZoneSet.CreateRateTransportZoneForTest("AU Zone");
			auZone.CreateRateTransportZoneItemForTest(sydneyCityTown);

			var sydneyZoneSet = Helper.CreateRateTransportZoneSet(null, "", sydneyCityTown);
			var sydneyZone = sydneyZoneSet.CreateRateTransportZoneForTest("Syd Zone");
			sydneyZone.CreateRateTransportZoneItemForTest(sydneyCityTown);

			var deliveryConsignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(deliveryConsignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "", "AUSYD");
			var rateLine = rateEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 15m, sydneyZoneSet.Zones[0].PK);

			Factory.Save();

			var expectedZones = new ZString[] { "Standard", "Syd Zone" };
			var actualZones = calculator.CartageZones.Cast<CartageZone>().Select(x => x.Description);
			AssertContainsExactElementsInAnyOrder(expectedZones, actualZones);

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, deliveryConsignee.PK, "CNSHA", "AUSYD", 10m);
			AssertEquals("Shipment has more address details than rate so should match city town based zone set", "Syd Zone", shipment.JS_Calc_DeliveryCartageZone);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 150m,
						RevenueCalculationDescription = @"DCART: 10 Kilogram(s) @ AUD 15.00/KG
" + DescriptionHelpers.FormatWithTab("Zone:") + "Syd Zone"
					}
			};

			var expectedLogLines = @"Information: Matched 'Syd Zone' for RateLine DCART-CTZ-KG-Client Rate TESTORG1
	- Delivery Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Syd Zone' transport zone matched by Consignee Pickup/Delivery Address fallback";

			AutorateAndAssert("Matched Sydney from delivery address, should use rate zone set", expected, shipment, deliveryConsignee);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain Zone information", expectedLogLines);
		}

		public void TestCTZCalculatorUsesCorrectZoneSetForClientRate_WorksWithInternationalZones()
		{
			var panamaCityTown = Factory.New<RefCityTown>();
			panamaCityTown.R9_InternationalName = "Panama";
			panamaCityTown.R9_RN_NKCountry = CountryCodes.Panama;
			var bermudaCityTown = Factory.New<RefCityTown>();
			bermudaCityTown.R9_InternationalName = "Bermuda";
			bermudaCityTown.R9_RN_NKCountry = CountryCodes.Bermuda;

			var client = Helper.NewOrgHeader();
			var address = client.MainAddress;
			address.AddAddressType(OrgAddressType.PickupAndDelivery);
			address.OA_Address1 = "Edificio Arango Orillac Piso 1";
			address.OA_RL_NKRelatedPortCode = "PADAV";
			address.OA_City = panamaCityTown.R9_InternationalName;
			address.OA_State = ZString.Empty;
			address.OA_PostCode = ZString.Empty;

			client.OH_Code = "SUPPLIER";

			var taxHavens = Helper.NewInternationalZone("NOTX", null, CountryCodes.Panama, CountryCodes.Bermuda);

			var panamaZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.Panama);
			var panamaZone = panamaZoneSet.CreateRateTransportZoneForTest("PA Zone");
			panamaZone.CreateRateTransportZoneItemForTest(panamaCityTown);

			var bermudaZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.Bermuda);
			var bermudaZone = bermudaZoneSet.CreateRateTransportZoneForTest("BM Zone");
			bermudaZone.CreateRateTransportZoneItemForTest(bermudaCityTown);

			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, taxHavens.FZ_Code, "");
			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			Factory.Save();

			var calc = rateLine.GetCalculator<CartageZoneDistanceCalculator>();

			Assert("Pre-condition: standard", calc.CartageZones.ContainsZone(""));
			Assert("Should include the single zone from the matching Panana Zone Set", calc.CartageZones.ContainsZone("PA Zone"));
			Assert("Should include the single zone from the matching Bermuda Zone Set", calc.CartageZones.ContainsZone("BM Zone"));

			calc.EquipmentType = EquipmentNeeded.Any;
			calc.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, panamaZoneSet.Zones[0].PK);
			calc.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 5m, bermudaZoneSet.Zones[0].PK);

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "PADAV", "AUMEL", 14m);
			shipment.JS_INCO = "";

			Factory.Save();

			var zoneDescription = DescriptionHelpers.FormatWithTab("Zone:");
			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OCART",
						JR_OSSellAmt = 140m,
						RevenueCalculationDescription = @"OCART: 14 Kilogram(s) @ AUD 10.00/KG
" + zoneDescription + "SUPPLIER PA Zone"
					}
			};

			AutorateAndAssert(expected, shipment, client);

			address.OA_RL_NKRelatedPortCode = "BMBDA";
			address.OA_City = bermudaCityTown.R9_InternationalName;
			client.OH_Code = "SUPPLIER";

			shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "BMBDA", "AUMEL", 14m);
			shipment.JS_INCO = "";
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "OCART",
						JR_OSSellAmt = 70m,
						RevenueCalculationDescription = @"OCART: 14 Kilogram(s) @ AUD 5.00/KG
" + zoneDescription + "SUPPLIER BM Zone"
					}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestCTZCalculatorUsesCorrectZoneSetForClientRate_CrossBorderTransportZoneSets()
		{
			var internationalZone = Helper.NewInternationalZone("EURO", null, CountryCodes.Belgium, CountryCodes.Switzerland);

			var brusselsCityTown = Helper.GetCityTown("Brussels", "BRU", CountryCodes.Belgium);
			var baselCityTown = Helper.GetCityTown("Basel", "", CountryCodes.Switzerland);

			var client = Helper.NewOrgHeader();
			var baselAddress = client.MainAddress;
			baselAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			baselAddress.OA_Address1 = "Somewhere in Basel";
			baselAddress.OA_RL_NKRelatedPortCode = "CHBSL";
			baselAddress.OA_City = baselCityTown.R9_InternationalName;
			baselAddress.OA_State = ZString.Empty;
			baselAddress.OA_PostCode = ZString.Empty;
			baselAddress.OA_RN_NKCountryCode = CountryCodes.Switzerland;

			var zoneSet1 = Helper.CreateRateTransportZoneSet(null, CountryCodes.Belgium);
			var zone1 = zoneSet1.CreateRateTransportZoneForTest("BE With CH Zone");
			zone1.CreateRateTransportZoneItemForTest(brusselsCityTown);
			zone1.CreateRateTransportZoneItemForTest(baselCityTown);

			var zoneSet2 = Helper.CreateRateTransportZoneSet(null, CountryCodes.Switzerland);
			var zone2 = zoneSet2.CreateRateTransportZoneForTest("CH Zone");
			zone2.CreateRateTransportZoneItemForTest(baselCityTown);

			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "", "EURO");
			var rateLine = rateEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignee.PK, client.PK, "AUMEL", "CHBSL", 20m);
			shipment.JS_INCO = "";

			Factory.Save();

			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;

			Assert("Pre-condition: Standard Zone is included by default", calculator.CartageZones.ContainsZone(""));
			Assert("Pre-condition", calculator.CartageZones.ContainsZone("BE With CH Zone"));
			Assert("Pre-condition", calculator.CartageZones.ContainsZone("CH Zone"));

			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 111m, zone1.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 222m, zone2.PK);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 222m,
						RevenueCalculationDescription = $"{DescriptionHelpers.FormatWithTab("Zone:")}CH Zone"
					}
			};

			var message = "Criteria Destination is in Switzerland and Delivery Address matches CH Zone";
			AutorateAndAssert(message, expected, shipment, client, autorateCosts: false);
		}

		[TestDate(2016, 02, 10)]
		public void TestCTZCalculatorCalculatesDistanceCorrectly()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OCART");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DCART");

			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "SUPPLIER";
			consignor.OH_IsConsignor = true;

			var pickupAddress = consignor.Addresses.AddNew();
			pickupAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			pickupAddress.OA_Address1 = "389 Crown St";
			pickupAddress.OA_City = "Surry Hills";
			pickupAddress.OA_PostCode = "2010";
			pickupAddress.OA_State = "NSW";
			pickupAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var consignee = Helper.NewOrgHeader();
			consignee.OH_Code = "BUYER";
			consignee.OH_IsConsignee = true;

			var deliveryAddress = consignee.MainAddress;
			deliveryAddress.OA_Address1 = "65 Katoomba St";
			deliveryAddress.OA_City = "Katoomba";
			deliveryAddress.OA_PostCode = "2780";
			deliveryAddress.OA_State = "NSW";

			var ctoAddress = Helper.NewOrgHeader().MainAddress;
			ctoAddress.OA_PostCode = "2018";

			var zoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, 0, 50, 250);

			var clientRate = Helper.NewClientRate(consignor);
			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AUSYD", "AUMEL");
			var orgLine = orgEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var dstEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "AUMEL");
			var dstLine = dstEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var originCalculator = orgLine.GetCalculator<CartageZoneDistanceCalculator>();
			originCalculator.EquipmentType = EquipmentNeeded.Any;
			originCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 5m);
			originCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, zoneSet.Zones[0].PK);
			originCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 20m, zoneSet.Zones[1].PK);

			var dstCalculator = dstLine.GetCalculator<CartageZoneDistanceCalculator>();
			dstCalculator.EquipmentType = EquipmentNeeded.Any;
			dstCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 11m);
			dstCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 22m, zoneSet.Zones[0].PK);
			dstCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 33m, zoneSet.Zones[1].PK);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "AUMEL", 10m);
			shipment.JS_UniqueConsignRef = "S000955";

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.PK;

			Factory.Save();

			var message = "Expected to match the City Town specific Zones rather than the Australian Zone as there is enough info in the UNLOCO to match";

			var expected = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 100m,
						RevenueCalculationDescription = "OCART: 10 Kilogram(s) @ AUD 10.00/KG"
					},
				new AssertionCharge
					{
						JR_OSSellAmt = 330m,
						RevenueCalculationDescription = "DCART: 10 Kilogram(s) @ AUD 33.00/KG"
					}
			};

			var expectedLogLines = new[] { @"Information: Matched '50 to 249' for RateLine DCART-CTZ-KG-Client Rate SUPPLIER
	- Delivery Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: 85.588 (Consignee Delivery Address to CTO/Wharf)
	- '50 to 249' matched by distance (85.588)",
@"Information: Matched '0 to 49' for RateLine OCART-CTZ-KG-Client Rate SUPPLIER
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: 4.927 (Consignor Pickup Address to CTO/Wharf)
	- '0 to 49' matched by distance (4.927)" };

			AutorateAndAssert(message, expected, shipment, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain distance calculation", expectedLogLines);

			Env.Registry.Rating.UseDistanceCalculationService = true;

			expected = new[]
			{
				new AssertionCharge
					{
						JR_OSSellAmt = 200m,
						RevenueCalculationDescription = "OCART: 10 Kilogram(s) @ AUD 20.00/KG"
					},
				new AssertionCharge
					{
						JR_OSSellAmt = 330m,
						RevenueCalculationDescription = "DCART: 10 Kilogram(s) @ AUD 33.00/KG"
					}
			};

			expectedLogLines = new[] { @"Information: Matched '50 to 249' for RateLine DCART-CTZ-KG-Client Rate SUPPLIER
	- Delivery Distance: empty
	- Distance Calculation Service: 51 (Consignee Delivery Address to CTO/Wharf)
	- '50 to 249' matched by distance (51)",
@"Information: Matched '50 to 249' for RateLine OCART-CTZ-KG-Client Rate SUPPLIER
	- Pickup Distance: empty
	- Distance Calculation Service: 52 (Consignor Pickup Address to CTO/Wharf)
	- '50 to 249' matched by distance (52)" };

			AutorateAndAssert(message, expected, shipment, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain distance calculation details", expectedLogLines);
		}

		public void TestCartageZoneDistanceCalculator_PrefersRatingTypesByMode()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OCART");

			var consignor = Helper.NewOrgHeader();
			var pickupAddress = consignor.MainAddress;
			pickupAddress.OA_Address1 = "389 Crown St";
			pickupAddress.OA_City = "Surry Hills";
			pickupAddress.OA_PostCode = "2010";
			pickupAddress.OA_State = "NSW";
			pickupAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var transportZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.LCL);
			var zone0 = transportZoneSet.CreateRateTransportZoneForTest("0 to 49");
			zone0.CreateRateTransportZoneItemForTest(0, 49);
			var zone1 = transportZoneSet.CreateRateTransportZoneForTest("50 to 99");
			zone1.CreateRateTransportZoneItemForTest(50, 99);
			Factory.Save();

			var clientRate = Helper.NewClientRate(consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Weight.Kilograms);
			var calculator = rateLine.Calculator;
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 100m, ZGuid.Empty);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 20m, zone0.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 30m, zone1.PK);

			var shipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "AUSYD", "AUBTB", 10m);
			var ctoAddress = Helper.NewOrgHeader().MainAddress;
			ctoAddress.OA_PostCode = "2018";

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 200m,
					RevenueCalculationDescription = "OCART: 10 Kilogram(s) @ AUD 20.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, consignor, autorateCosts: false);
		}

		public void TestCartageZoneDistanceCalculator_PrefersMostAppropriateRatingTypesByMode()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OCART");

			var consignor = Helper.NewOrgHeader();
			var pickupAddress = consignor.MainAddress;
			pickupAddress.OA_Address1 = "389 Crown St";
			pickupAddress.OA_City = "Surry Hills";
			pickupAddress.OA_PostCode = "2010";
			pickupAddress.OA_State = "NSW";
			pickupAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var airZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneMode: RateMode.AIR);
			var airZone = airZoneSet.CreateRateTransportZoneForTest("0 to 99");
			airZone.CreateRateTransportZoneItemForTest(0, 99);

			var lseZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneMode: RateMode.LSE);

			var zone = lseZoneSet.CreateRateTransportZoneForTest("0 to 99");
			zone.CreateRateTransportZoneItemForTest(0, 99);

			Factory.Save();

			var clientRate = Helper.NewClientRate(consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Weight.Kilograms);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();

			AssertCollectionNotContains(airZoneSet, calculator.CartageZones);

			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 100m, ZGuid.Empty);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 40m, airZone.PK);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUSYD", "AUBTB", 10m);
			var ctoAddress = Helper.NewOrgHeader().MainAddress;
			ctoAddress.OA_PostCode = "2018";

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 400m,
					RevenueCalculationDescription = "OCART: 10 Kilogram(s) @ AUD 40.00/KG"
				}
			};

			AutorateAndAssert("Should pick the LSE rate for LSE shipment", expected, shipment, consignor, autorateCosts: false);
		}

		public void TestCartageZoneDistanceCalculator_StillUsesStandardRatingTypesWhenNoOtherModesAvailable()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OCART");

			var consignor = Helper.NewOrgHeader();
			var pickupAddress = consignor.MainAddress;
			pickupAddress.OA_Address1 = "389 Crown St";
			pickupAddress.OA_City = "Surry Hills";
			pickupAddress.OA_PostCode = "2010";
			pickupAddress.OA_State = "NSW";
			pickupAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var transportZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.LCL);
			var zone0 = transportZoneSet.CreateRateTransportZoneForTest("0 to 49");
			zone0.CreateRateTransportZoneItemForTest(0, 49);
			var zone1 = transportZoneSet.CreateRateTransportZoneForTest("50 to 99");
			zone1.CreateRateTransportZoneItemForTest(50, 99);

			Factory.Save();

			var clientRate = Helper.NewClientRate(consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Weight.Kilograms);
			var calculator = rateLine.Calculator;
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 100m, ZGuid.Empty);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 20m, zone0.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 30m, zone1.PK);

			var shipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "AUSYD", "AUBTB", 10m);
			var ctoAddress = Helper.NewOrgHeader().MainAddress;
			ctoAddress.OA_PostCode = "2018";

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.PK;

			Factory.Save();

			var expected = new[]
				{
						new AssertionCharge
							{
								JR_OSSellAmt = 200m,
								RevenueCalculationDescription = "OCART: 10 Kilogram(s) @ AUD 20.00/KG"
							}
					};

			AutorateAndAssert(expected, shipment, consignor, autorateCosts: false);
		}

		public void TestCartageZoneDistanceCalculator_DisplayCorrectZoneSet()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OCART");

			var localClient = Helper.NewOrgHeader();
			var pickupAddress = localClient.MainAddress;
			pickupAddress.OA_Address1 = "389 Crown St";
			pickupAddress.OA_City = "Surry Hills";
			pickupAddress.OA_PostCode = "2010";
			pickupAddress.OA_State = "NSW";
			pickupAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var transportZoneSetA = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "RAT AIR A1", "RAT AIR A2" });
			transportZoneSetA.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			transportZoneSetA.TP_ZoneMode = RateMode.AIR;

			var transportZoneSetB = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "RAT LSE B1", "RAT LSE B2" });
			transportZoneSetB.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			transportZoneSetB.TP_ZoneMode = RateMode.LSE;

			var zone = Helper.NewInternationalZone("GT10", null, CountryCodes.Australia);
			zone.FZ_ZoneMode = RateMode.LSE;

			Factory.Save();

			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, zone.Code, "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Weight.Kilograms);
			var transportZones = rateLine.Lookups.Zones;

			AssertEquals("Should Load the most specific zone", 2, transportZones.Count);

			Assert("Should not have Air Zone Sets", !transportZones.ContainsCode("RAT AIR A1"));
			Assert("Should not have Air Zone Sets", !transportZones.ContainsCode("RAT AIR A2"));

			Assert(transportZones.ContainsCode("RAT LSE B1"));
			Assert(transportZones.ContainsCode("RAT LSE B2"));
		}

		public void TestCartageZoneDistanceCalculator_ConsiderNotOnlyPostCodeRangeButCountryCode()
		{
			var pickupFromOrg = Helper.NewOrgHeader();
			var address = pickupFromOrg.MainAddress;
			address.AddAddressType(OrgAddressType.PickupAndDelivery);
			address.OA_Address1 = "Edificio Arango Orillac Piso 1";
			address.OA_City = Helper.GetCityTown("WAREGEM-BELGIE", "BRU", CountryCodes.Belgium).R9_InternationalName;
			address.OA_State = "BRU";
			address.OA_PostCode = "4012";
			address.OA_RN_NKCountryCode = CountryCodes.Belgium;

			var ch4000PostCode = Helper.CreateRefPostCode("4000", CountryCodes.Switzerland);
			var ch4012PostCode = Helper.CreateRefPostCode("4012", CountryCodes.Switzerland);
			var ch4099PostCode = Helper.CreateRefPostCode("4099", CountryCodes.Switzerland);
			var be4000PostCode = Helper.CreateRefPostCode("4000", CountryCodes.Belgium);
			var be4012PostCode = Helper.CreateRefPostCode("4012", CountryCodes.Belgium);
			var be8000PostCode = Helper.CreateRefPostCode("8000", CountryCodes.Belgium);

			var zoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Switzerland);
			var switzerlandZone = zoneSet.CreateRateTransportZoneForTest("Switzerland");
			switzerlandZone.CreateRateTransportZoneItemForTest(ch4000PostCode, ch4099PostCode).TQ_RN_NKCountry = CountryCodes.Switzerland;

			var belgiumZone = zoneSet.CreateRateTransportZoneForTest("Belgium");
			belgiumZone.CreateRateTransportZoneItemForTest(be4000PostCode, be8000PostCode).TQ_RN_NKCountry = CountryCodes.Belgium;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Switzerland))
			{
				var clientRate = Helper.NewClientRate(NewClient);
				var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "CHBSL", "");
				var rateLine = rateEntry.AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
				var ctzCalculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();

				Assert("Pre-condition: standard", ctzCalculator.CartageZones.ContainsZone(""));
				Assert(ctzCalculator.CartageZones.ContainsZone("Switzerland"));
				Assert(ctzCalculator.CartageZones.ContainsZone("Belgium"));

				ctzCalculator.EquipmentType = EquipmentNeeded.Any;
				ctzCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 100m, switzerlandZone.PK);
				ctzCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 80m, belgiumZone.PK);

				var shipment = CreateForwardingShipment(TransportModes.Air, NewClient.PK, ZGuid.Empty, "CHBSL", "TWKHH", 14m);
				shipment.ConsignorPickupAddress.OrganisationPK = pickupFromOrg.PK;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt = 80m,
							RevenueCalculationDescription = $"{DescriptionHelpers.FormatWithTab("Zone:")}Belgium"
						}
				};

				AutorateAndAssert("When matching zone, country of address should be considered", expected, shipment, NewClient);

				shipment.ConsignorPK = ZGuid.Empty;
				shipment.ConsignorPickupAddress.OrganisationPK = ZGuid.Empty;
				Factory.Save();
				expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt = 0m,
							RevenueCalculationDescription = @"Calculation failed due to no transport zone being found for the given details."
						}
				};

				AutorateAndAssert("When matching zone with no country code, no zones should be matched", expected, shipment, NewClient);
			}
		}

		#endregion

		#region AutoRateWithUnitAsFreighted

		public void TestAutoRateWithUnitAsFreighted()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "USLAX", "AUSYD", ZString.Empty, "LD-2");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var rateLine1 = rateEntry.AddRateLine("FRT", HighestRateCalculator.Code);
			var rateLineItem11 = rateLine1.RateLineItems.AddNew();
			rateLineItem11.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem11.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem11.TM_RelevantValue = 5m;

			var rateLineItem12 = rateLine1.RateLineItems.AddNew();
			rateLineItem12.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem12.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem12.TM_RelevantValue = 100m;

			var rateLine2 = rateEntry.AddRateLine("BAF", HighestRateCalculator.Code);
			rateLine2.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.AsFreightedHighestRateWhenMin;
			var rateLineItem21 = rateLine2.RateLineItems.AddNew();
			rateLineItem21.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem21.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem21.TM_RelevantValue = 2m;

			var rateLineItem22 = rateLine2.RateLineItems.AddNew();
			rateLineItem22.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem22.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem22.TM_RelevantValue = 20m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_INCO = "FOB";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 180m;
			shipment.JS_ActualVolume = 2m;

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-2"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			shipment.OuterPackLines[0].SetContainer(consol, container);

			shipment.ConsigneePK = consignee.MainAddress.PK;
			shipment.ConsignorPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 900m,
							RevenueCalculationDescription = "FRT: 180 Kilogram(s) @ USD 5.00/KG"
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 360m,
							RevenueCalculationDescription = "BAF: 180 Kilogram(s) @ USD 2.00/KG (As Freighted)"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestAutoRateWithUnitUnitAndFlatCalculatorAsFreighted()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "USLAX", "AUSYD", ZString.Empty, "40GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var rateLineFRT = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.M3);
			rateLineFRT.GetCalculator<UnitCalculator>().PerUnit = 20;

			var rateLineBAF = rateEntry.AddRateLine("BAF", HighestRateCalculator.Code);
			rateLineBAF.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.AsFreightedHighestRateWhenMin;
			var rateLineItem21 = rateLineBAF.RateLineItems.AddNew();
			rateLineItem21.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem21.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem21.TM_RelevantValue = 2m;

			var rateLineItem22 = rateLineBAF.RateLineItems.AddNew();
			rateLineItem22.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem22.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem22.TM_RelevantValue = 0.5m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_INCO = "FOB";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 180m;
			shipment.JS_ActualVolume = 2m;
			shipment.JS_OuterPacks = 1;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;

			shipment.ConsigneePK = consignee.MainAddress.PK;
			shipment.ConsignorPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 40m,
							RevenueCalculationDescription = "FRT: 2 Cubic Meter(s) @ USD 20.00/M3"
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 1m,
							RevenueCalculationDescription = "BAF: 2 Cubic Meter(s) @ USD 0.50/M3 (As Freighted)"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			rateLineFRT.TL_RateCalculator = FlatCalculator.Code;
			rateLineFRT.GetCalculator<FlatCalculator>().BaseRate = 20;
			rateLineFRT.TL_WeightVolume = "";

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 20m,
							RevenueCalculationDescription = "FRT: Base Rate USD 20.00"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestAutoRateWithUnitAsFreightedUseChargeableInsteadActual()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "USLAX", "AUSYD", ZString.Empty, "40GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var rateLine1 = rateEntry.AddRateLine("FRT", HighestRateCalculator.Code);
			var rateLineItem11 = rateLine1.RateLineItems.AddNew();
			rateLineItem11.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem11.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem11.TM_RelevantValue = 5m;
			rateLine1.UseOnlyActualWeightMeasure = false;

			var rateLine2 = rateEntry.AddRateLine("BAF", HighestRateCalculator.Code);
			rateLine2.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.AsFreightedHighestRateWhenMin;
			var rateLineItem21 = rateLine2.RateLineItems.AddNew();
			rateLineItem21.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem21.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem21.TM_RelevantValue = 2m;

			var rateLineItem22 = rateLine2.RateLineItems.AddNew();
			rateLineItem22.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem22.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem22.TM_RelevantValue = 20m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_INCO = "FOB";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 180m;
			shipment.JS_ActualVolume = 2m;
			shipment.JS_OuterPacks = 1;

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;

			shipment.ConsigneePK = consignee.MainAddress.PK;
			shipment.ConsignorPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 1666.67m,
							RevenueCalculationDescription = "FRT: 333.333 Kilogram(s) @ USD 5.00/KG"
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 360m,
							RevenueCalculationDescription = "BAF: 180 Kilogram(s) @ USD 2.00/KG (As Freighted)"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestAutoRateWithUnitAsFreightedWhenFreightedChargeCodeIsNotSpecified()
		{
			Env.Registry.FreightChargeCode = Guid.Empty;

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "USLAX", "AUSYD", ZString.Empty, "LD-2");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var rateLine1 = rateEntry.AddRateLine("FRT", HighestRateCalculator.Code);
			var rateLineItem11 = rateLine1.RateLineItems.AddNew();
			rateLineItem11.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem11.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem11.TM_RelevantValue = 5m;

			var rateLineItem12 = rateLine1.RateLineItems.AddNew();
			rateLineItem12.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem12.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem12.TM_RelevantValue = 100m;

			var rateLine2 = rateEntry.AddRateLine("BAF", HighestRateCalculator.Code);
			rateLine2.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.AsFreightedHighestRateWhenMin;
			var rateLineItem21 = rateLine2.RateLineItems.AddNew();
			rateLineItem21.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem21.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem21.TM_RelevantValue = 2m;

			var rateLineItem22 = rateLine2.RateLineItems.AddNew();
			rateLineItem22.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem22.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem22.TM_RelevantValue = 20m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_INCO = "FOB";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 180m;
			shipment.JS_ActualVolume = 2m;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-2"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			shipment.OuterPackLines[0].SetContainer(consol, container);

			shipment.ConsigneePK = consignee.MainAddress.PK;
			shipment.ConsignorPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 900m,
							RevenueCalculationDescription = "FRT: 180 Kilogram(s) @ USD 5.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestAutoRateWithUnitAsFreightedWhenHighestRate()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "USLAX", "AUSYD", ZString.Empty, "LD-2");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var rateLine1 = rateEntry.AddRateLine("FRT", HighestRateCalculator.Code);
			var rateLineItem11 = rateLine1.RateLineItems.AddNew();
			rateLineItem11.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem11.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem11.TM_RelevantValue = 5m;

			var rateLineItem12 = rateLine1.RateLineItems.AddNew();
			rateLineItem12.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem12.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem12.TM_RelevantValue = 100m;

			var rateLine2 = rateEntry.AddRateLine("BAF", HighestRateCalculator.Code);
			rateLine2.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.HighestRate;
			var rateLineItem21 = rateLine2.RateLineItems.AddNew();
			rateLineItem21.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem21.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem21.TM_RelevantValue = 2m;

			var rateLineItem22 = rateLine2.RateLineItems.AddNew();
			rateLineItem22.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem22.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem22.TM_RelevantValue = 200m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_INCO = "FOB";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 180m;
			shipment.JS_ActualVolume = 2m;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-2"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			shipment.OuterPackLines[0].SetContainer(consol, container);

			shipment.ConsigneePK = consignee.MainAddress.PK;
			shipment.ConsignorPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 900m,
							RevenueCalculationDescription = "FRT: 180 Kilogram(s) @ USD 5.00/KG"
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 400m,
							RevenueCalculationDescription = "BAF: 2 Cubic Meter(s) @ USD 200.00/M3"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestAutoRateWithUnitAsFreightedAddChargeWhenMinimum()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "USLAX", "AUSYD", ZString.Empty, "LD-2");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var rateLine1 = rateEntry.AddRateLine("FRT", HighestRateCalculator.Code);
			var rateLineItem11 = rateLine1.RateLineItems.AddNew();
			rateLineItem11.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem11.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem11.TM_RelevantValue = 5m;

			var rateLineItem12 = rateLine1.RateLineItems.AddNew();
			rateLineItem12.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem12.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem12.TM_RelevantValue = 100m;

			var rateLineItem13 = rateLine1.RateLineItems.AddNew();
			rateLineItem13.TM_Type = Calculator.Items.Operator.MIN;
			rateLineItem13.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem13.TM_RelevantValue = 3000m;

			var rateLine2 = rateEntry.AddRateLine("BAF", HighestRateCalculator.Code);
			rateLine2.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.AsFreightedHighestRateWhenMin;
			var rateLineItem21 = rateLine2.RateLineItems.AddNew();
			rateLineItem21.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem21.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem21.TM_RelevantValue = 2m;

			var rateLineItem22 = rateLine2.RateLineItems.AddNew();
			rateLineItem22.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem22.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem22.TM_RelevantValue = 200m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_INCO = "FOB";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 180m;
			shipment.JS_ActualVolume = 2m;

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-2"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			shipment.OuterPackLines[0].SetContainer(consol, container);

			shipment.ConsigneePK = consignee.MainAddress.PK;
			shipment.ConsignorPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 3000m,
							RevenueCalculationDescription = "FRT: Minimum USD 3000.00"
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 400m,
							RevenueCalculationDescription = "BAF: 2 Cubic Meter(s) @ USD 200.00/M3"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestAutoRateWithUnitAsFreightedNotAddChargeWhenMinimum()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "USLAX", "AUSYD", ZString.Empty, "LD-2");
			AddParityExchangeRate(rateEntry.Currency);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", HighestRateCalculator.Code);
			var rateLineItem11 = rateLine1.RateLineItems.AddNew();
			rateLineItem11.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem11.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem11.TM_RelevantValue = 5m;

			var rateLineItem12 = rateLine1.RateLineItems.AddNew();
			rateLineItem12.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem12.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem12.TM_RelevantValue = 100m;

			var rateLineItem13 = rateLine1.RateLineItems.AddNew();
			rateLineItem13.TM_Type = Calculator.Items.Operator.MIN;
			rateLineItem13.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem13.TM_RelevantValue = 3000m;

			var rateLine2 = rateEntry.AddRateLine("BAF", HighestRateCalculator.Code);
			rateLine2.GetCalculator<HighestRateCalculator>().RatePickRule = Calculator.Items.AsFreightedDontApplyWhenMin;
			var rateLineItem21 = rateLine2.RateLineItems.AddNew();
			rateLineItem21.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem21.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem21.TM_RelevantValue = 2m;

			var rateLineItem22 = rateLine2.RateLineItems.AddNew();
			rateLineItem22.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem22.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem22.TM_RelevantValue = 200m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_INCO = "FOB";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 180m;
			shipment.JS_ActualVolume = 2m;

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-2"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			shipment.OuterPackLines[0].SetContainer(consol, container);

			shipment.ConsigneePK = consignee.MainAddress.PK;
			shipment.ConsignorPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 3000m,
							RevenueCalculationDescription = "FRT: Minimum USD 3000.00"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestAutoRateWithUnitAsFreightedUseCostChargeableWeightOnMAWB()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FSC");
			var cne = Helper.NewOrgHeader();

			var cost = Factory.New<Costing>();
			var entry = cost.AddRateEntry("AIR", "LSE", "AU", "");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);

			var cmbCalculator = line1.GetCalculator<CombinedCalculator>();
			cmbCalculator["-45"] = (ZDecimal)0.65;
			cmbCalculator["+45"] = (ZDecimal)0.55;
			cmbCalculator.UseHigherChargeableLowerRateRule = true;

			var line2 = entry.AddRateLine("FSC", HighestRateCalculator.Code, QuantityUnit.KG);
			line2.GetCalculator<HighestRateCalculator>().RatePickRule = "HRM";

			var lineItem2 = line2.RateLineItems.AddNew();
			lineItem2.TM_Type = "UNT";
			lineItem2.TM_BreakWeightVolume = "KG";
			lineItem2.TM_RelevantValue = 0.5m;

			line1.UseOnlyActualWeightMeasure = false;
			line2.UseOnlyActualWeightMeasure = false;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_AgentType = "AGT";
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.ConsigneePK = cne.PK;
			shipment.JS_ActualWeight = 40m;
			shipment.JS_ActualChargeable = 44m;

			Factory.Save();

			var expectedCosts = new[]
				{
					new AssertionCost
						{
							CostCalculationDescription = "FSC: 45 Kilogram(s) @ AUD 0.50/KG (As Freighted)",
							ChargeCode = "FSC",
							E6_OSCostAmount = 22.5m,
						},
					new AssertionCost
						{
							CostCalculationDescription = "FRT: 45 Kilogram(s) (HBLR is applied) @ AUD 0.55/KG",
							ChargeCode = "FRT",
							E6_OSCostAmount = 24.75m,
						}
				};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("When ‘Unit As Freighted’ is set on the HRC calculator with only one unit line setup then the measurable unit should be used for this charge exactly as was calculated for the FRT charge."
				, null
				, expectedCosts
				, consol
				, false);
		}

		#endregion

		#region ConsolDoesntAttemptToRateSellOnItself

		[TestDate(2010, 10, 10)]
		public void TestConsolDoesntAttemptToRateSellOnItself()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				var creditor = Helper.NewOrgHeader();
				creditor.OH_IsCreditor = true;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "C00001000";
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.SetDefaultShippingLineAddress(creditor);
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
				consol.JK_PrepaidCollect = PaymentType.Prepaid;

				Factory.Save();

				var expectedNote = @"User:				CargoWise Support
Time:				10-Oct-10 00:00

Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription
Information: AUTORATING COSTS FOR Consol C00001000
Information: CHARGES CALCULATED:
Warning: Consol C00001000 was auto-costed.
	No costs were found.";

				using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
				{
					AutoCostAndAssert("", null, null, consol);
					AssertAutoratingAuditLogNote(consol, expectedNote);
					AutoCostAndAssert("", null, null, consol, false);
					AssertAutoratingAuditLogNote(consol, expectedNote);
				}
			}
		}

		#endregion

		#region CostAutoratedFromDomesticConsol

		public void TestCostAutoratedFromDomesticConsol()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var cnr = Helper.NewOrgHeader(1);
			var cne = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUBNE");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("FRT", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 5;

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CIF";

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate())
			{
				job.JH_OA_LocalChargesAddr = cnr.MainAddress.PK;
				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
						{
							CostCalculationDescription = "FRT: 1500 Kilogram(s) @ AUD 5.00/KG",
							ChargeCode = "FRT",
							E6_OSCostAmount = 7500,
						}
				};

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment, new[]
							{
								new AssertionCharge
									{
										ChargeCode = "FRT",
										JR_OSSellAmt = 7500,
										JR_OSCostAmt = 7500
									}
							}
					}
				};

				AutoCostAndAssert("", expectedCharges, expectedCosts, consol);
			}
		}

		#endregion

		#region Autorating Continue When Cannot Calculate Container With Weight Volume Breaks

		public void TestAutoratingContinueWhenCannotCalculateContainerWithWeightVolumeBreaks()
		{
			var rate = Helper.NewClientRate(Consignee);
			var entry = rate.AddRateEntry("DST", "AIR", "US", "AUBNE");

			var line1 = entry.AddRateLine("DEHC", FlatCalculator.Code);
			line1.GetCalculator<FlatCalculator>().BaseRate = 25;

			var line2 = entry.AddRateLine("DDOC", CartageCalculator.Code, "CN");
			var calc = line2.GetCalculator<CartageCalculator>();
			calc["-250"] = (ZDecimal)45m;
			calc["+250"] = (ZDecimal)35m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "ULD", "FOB", Consignee, Consignor, Consignee, null, "USLAX", "AUBNE", 450m, 1m, QuotedBookingState.BookingOnly);

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-2")).PK;
			container.JC_ContainerCount = 1;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "DEHC",
							JR_OSSellAmt = 25,
						},
					new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_OSSellAmt = 45,
						}
				};

			AutorateAndAssert(expected, quotedBooking, Consignee);

			var rateLineItem = calc.RateLineItems.Cast<RateLineItem>().Single(x => x.TM_Type == "-" && x.TM_Break == 250m);
			rateLineItem.TM_BreakWeightVolume = "KG";

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "DEHC",
						JR_OSSellAmt = 25,
					},
				new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_OSSellAmt = 0,
						RevenueCalculationDescription = "Weight / Volume / Packages information was not specified for Pack Lines on this job. Rating based on container weight / volume / packages cannot be performed.",
					}
			};

			AutorateAndAssert(expected, quotedBooking, Consignee);
		}

		#endregion

		#region TestAutoRateWithAgencyCalculatorWhenValueIsInvalid

		public void TestAutoRateWithAgencyCalculatorWhenValueIsInvalid()
		{
			#region Setup Registry

			var disbCharge = Helper.ChargeCodes.New("DISB", "Disbursement  Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Brokerage);
			disbCharge.AC_ChargeType = ChargeType.Disbursement;
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, disbCharge.PK.ToGuid());

			#endregion

			var cmrCharge = Helper.ChargeCodes.New("CMR", "CMR Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Brokerage);
			cmrCharge.AC_ChargeType = ChargeType.Margin;
			cmrCharge.FillWithValidTestData();
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode(cmrCharge);

			var consignor = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateORGEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, ZString.Empty, "AUSYD");
			rateORGEntry.RateLines.RemoveAndDeleteAll();

			var rateORGLine = rateORGEntry.AddRateLine(cmrCharge.AC_Code, AgencyCalculator.Code);
			rateORGLine.GetCalculator<AgencyCalculator>().AgencyRate = 25m;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, localClient.PK, "CNSHA", "AUSYD", 120m, 0.1m);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = localClient.PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_EntryStatus = "DWC";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CMR",
							JR_OSSellAmt = 25m,
							RevenueCalculationDescription = @"CMR: Base Rate AUD 25.00"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			rateORGLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			rateORGLine.GetCalculator<AgencyCalculator>().MaximumLines = -20;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CMR",
							JR_OSSellAmt = 0m,
							RevenueCalculationDescription = @"Calculation failed due to incorrect or missing data"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			rateORGLine.GetCalculator<AgencyCalculator>().MaximumLines = 0;
			rateORGLine.GetCalculator<AgencyCalculator>().IncludedLines = -50;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CMR",
						JR_OSSellAmt = 0m,
						RevenueCalculationDescription = @"Calculation failed due to incorrect or missing data"
					}
				};

			AutorateAndAssert(expected, shipment, localClient);

			rateORGLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
			rateORGLine.GetCalculator<AgencyCalculator>().MaximumLines = 0;
			rateORGLine.GetCalculator<AgencyCalculator>().IncludedLines = 0;
			rateORGLine.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			rateORGLine.GetCalculator<AgencyCalculator>().IncludedHeaders = -100;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CMR",
						JR_OSSellAmt = 0m,
						RevenueCalculationDescription = @"Calculation failed due to incorrect or missing data"
					}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		#endregion

		#region TestAutoRateAgencyCalculatorWithForwardingShipment

		public void TestAutoRateAgencyCalculatorWithForwardingShipment()
		{
			#region Setup Registry

			var disbCharge = Helper.ChargeCodes.New("DISB", "Disbursement  Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Brokerage);
			disbCharge.AC_ChargeType = ChargeType.Disbursement;
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, disbCharge.PK.ToGuid());

			#endregion

			#region Setup Rate

			var consignor = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader(1);

			var cmrCharge = Helper.ChargeCodes.New("CMR", "CMR Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Brokerage);
			cmrCharge.AC_ChargeType = ChargeType.Margin;
			cmrCharge.FillWithValidTestData();

			var rate = Helper.NewClientRate(localClient);
			var rateORGEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, ZString.Empty, "AUSYD");
			rateORGEntry.RateLines.RemoveAndDeleteAll();

			var rateORGLine = rateORGEntry.AddRateLine(cmrCharge.AC_Code, AgencyCalculator.Code);
			rateORGLine.GetCalculator<AgencyCalculator>().AgencyRate = 25m;
			rateORGLine.GetCalculator<AgencyCalculator>().PerAdditionalLine = 2;

			#endregion

			#region Setup Shipment with Declaration
			// Declaration
			//	|- Entries
			//		|- Entry1
			//			|- Invoice1 - InvoiceLine1
			//			|- Invoice1 - InvoiceLine2
			//		|- Entry2
			//			|- Invoice2 - InvoiceLine1
			//			|- Invoice2 - InvoiceLine2
			//			|- Invoice2 - InvoiceLine3
			//	|- Invoices
			//		|- Invoice1
			//			|- InvoiceLine1
			//			|- InvoiceLine2
			//		|- Invoice2
			//			|- InvoiceLine1
			//			|- InvoiceLine2

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, localClient.PK, "CNSHA", "AUSYD", 120m, 0.1m);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = localClient.PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_EntryStatus = "DWC";

			var header = declaration.ActiveEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1ForEntryLine1 = invoice1.InvoiceLines.AddNew();
			var invoice2ForEntryLine1 = invoice1.InvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			var invoice1ForEntryLine2 = invoice2.InvoiceLines.AddNew();
			var invoice2ForEntryLine2 = invoice2.InvoiceLines.AddNew();
			var invoice3ForEntryLine2 = invoice2.InvoiceLines.AddNew();

			var entryLine1 = header.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoice1ForEntryLine1);
			entryLine1.InvoiceLines.Add(invoice2ForEntryLine1);

			var entryLine2 = header.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invoice1ForEntryLine2);
			entryLine2.InvoiceLines.Add(invoice2ForEntryLine2);
			entryLine2.InvoiceLines.Add(invoice3ForEntryLine2);

			#endregion

			#region FLT: Flat

			rateORGLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CMR",
						JR_OSSellAmt = 25m,
						RevenueCalculationDescription = @"CMR: Base Rate AUD 25.00"
					}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion

			#region INE: PerInvoiceLinePerEntry (2 entires, 2 + 3 lines)
			rateORGLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerEntry;
			rateORGLine.GetCalculator<AgencyCalculator>().PerAdditionalLine = 10;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CMR",
						JR_OSSellAmt = 75m,
						RevenueCalculationDescription = @"CMR: Base Rate AUD 25.00 + 5 invoice lines for each entry @ AUD 10.00/Invoice Line"
					}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion

			#region INI: PerInvoiceLinePerInvoice (2 invoices, 2 + 3 lines)

			rateORGLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerInvoice;
			rateORGLine.GetCalculator<AgencyCalculator>().PerAdditionalLine = 10;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CMR",
						JR_OSSellAmt = 75m,
						RevenueCalculationDescription = @"CMR: Base Rate AUD 25.00 + 5 invoice lines for each invoice @ AUD 10.00/Invoice Line"
					}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion

			#region INS: PerInvoiceLinePerShipment (2 entires, 2 + 3 lines)

			rateORGLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerShipment;
			rateORGLine.GetCalculator<AgencyCalculator>().PerAdditionalLine = 10;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CMR",
						JR_OSSellAmt = 75m,
						RevenueCalculationDescription = @"CMR: Base Rate AUD 25.00 + 5 invoice lines for shipment @ AUD 10.00/Invoice Line"
					}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion

			#region TRE: PerTariffLinePerEntry (2 entries)

			rateORGLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			rateORGLine.GetCalculator<AgencyCalculator>().PerAdditionalLine = 10;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CMR",
						JR_OSSellAmt = 45m,
						RevenueCalculationDescription = @"CMR: Base Rate AUD 25.00 + 2 tariff lines for each entry @ AUD 10.00/Tariff Line"
					}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion

			#region TRS: PerTariffLinePerShipment (2 entries)

			rateORGLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerShipment;
			rateORGLine.GetCalculator<AgencyCalculator>().PerAdditionalLine = 10;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CMR",
						JR_OSSellAmt = 45m,
						RevenueCalculationDescription = @"CMR: Base Rate AUD 25.00 + 2 tariff lines for shipment @ AUD 10.00/Tariff Line"
					}
				};

			AutorateAndAssert(expected, shipment, localClient);

			#endregion
		}

		#endregion

		#region TestAutoRateAgencyCalculatorWithDeclarationWhenFeeTypeIsINV

		public void TestAutoRateAgencyCalculatorWithDeclarationWhenFeeTypeIsINV()
		{
			var localClient = Helper.NewOrgHeader(1);

			var chargeCode = Helper.ChargeCodes.New("ADDINV", "Additional Invoices", FlatCalculator.Code);
			chargeCode.FillWithValidTestData();

			chargeCode.AC_ChargeType = ChargeType.Margin;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, ZString.Empty, CountryCodes.Australia);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(chargeCode.AC_Code, AgencyCalculator.Code);

			rateLine.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerInvoice;
			rateLine.GetCalculator<AgencyCalculator>().IncludedHeaders = 0;
			rateLine.GetCalculator<AgencyCalculator>().AdditionalRate = 13M;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.Containerised;
			declaration.JE_OH_Importer = localClient.PK;
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_MessageType = "IMP";
			declaration.JE_EntryStatus = "DWC";

			declaration.Invoices.AddNew();
			declaration.Invoices.AddNew();

			new JobHeader.Loader(declaration).TryLoadOrCreate();
			declaration.Job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ADDINV",
					JR_OSSellAmt = 26m,
					RevenueCalculationDescription = @"ADDINV: 2 Invoices @ AUD 13.00/Invoice"
				}
			};

			AutorateAndAssert(expected, declaration, localClient);
		}

		#endregion

		#region Test Autorating does not merge Charges with different Currencies

		[ExpectNoExceptions]
		public void TestAutoRatingDoesNotMergeChargesWithDifferentCurrencies()
		{
			var refContainer1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-8").PK;
			var refContainer2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1").PK;

			var origin = "AUSYD";
			var destination = "NZAKL";
			var costing = Helper.NewCosting(null);

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, origin, destination);
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine("FRT", UnitCalculator.Code, "CN", CurrencyCodes.UnitedStates).GetCalculator<UnitCalculator>().PerUnit = 25m;
			rateEntry1.TI_RC = refContainer1;
			rateEntry1.TI_RX_NKCurrency = CurrencyCodes.UnitedStates;

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, origin, destination);
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine("FRT", FlatCalculator.Code, "", CurrencyCodes.EuropeanUnion).GetCalculator<FlatCalculator>().BaseRate = 50m;
			rateEntry2.TI_RC = refContainer2;
			rateEntry2.TI_RX_NKCurrency = CurrencyCodes.EuropeanUnion;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 550m);
			shipment.JS_PackingMode = ContainerModes.ULD;

			var consol = CreateForwardingConsol(TransportModes.Air, origin, destination, Consignor, shipment);

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE8800011";
			container1.JC_RC = refContainer1;
			container1.JC_ContainerMode = ContainerModes.ULD;
			container1.JC_ContainerCount = 1;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE9900011";
			container2.JC_RC = refContainer2;
			container2.JC_ContainerMode = ContainerModes.ULD;
			container2.JC_ContainerCount = 1;

			shipment.OuterPackLines.AddNew().Containers.Add(container1);
			shipment.OuterPackLines.AddNew().Containers.Add(container2);

			AddParityExchangeRate(Helper.Currencies[CurrencyCodes.UnitedStates]);
			AddParityExchangeRate(Helper.Currencies[CurrencyCodes.EuropeanUnion]);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 25m,
							JR_RX_NKCostCurrency = CurrencyCodes.UnitedStates
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 50m,
							JR_RX_NKCostCurrency = CurrencyCodes.EuropeanUnion
						}
				};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);
		}

		#endregion

		#region Freight spot, cost and gateway rates autorating

		public void TestFreightAutorateStandardModeWithNegotiatedBy()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 20;
			shipment.JS_ActualVolume = 2;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  6666.66m,
							JR_OSCostAmt =  6666.66m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20.00m,
							JR_OSCostAmt = 20.00m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestFreightAutorateAllInclusiveModeWithNegotiatetBy()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightCostRate = 20m;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_INCO = "CIF";
			shipment.JS_ActualWeight = 20;
			shipment.JS_ActualVolume = 2;
			shipment.JS_UnitFreightRate = 30;
			shipment.JS_RX_NKFrtRateCurrency = "USD";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  9999.99m,
							JR_OSCostAmt =  6666.66m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		[TestDate(2016, 06, 06)]
		public void TestFreightAutorateFreightPlusModeWithNegotiatedBy()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20m, 2m);
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_UnitFreightRate = 30;
			shipment.JS_FreightCostRate = 10;
			shipment.JS_RX_NKFrtRateCurrency = "USD";
			shipment.JS_UniqueConsignRef = "S100216";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 9999.99m,
							JR_OSCostAmt = 3333.33m,
							CostCalculationDescription = "333.333 Kilogram(s) @ AUD 10.00/KG",
							RevenueCalculationDescription = "333.333 Kilogram(s) @ USD 30.00/KG"
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20m,
							JR_OSCostAmt = 20m,
							RevenueCalculationDescription = "Base Rate AUD 20.00"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			var expectedLogLines = new[] { "Information: RateLine Found FRT-Job Negotiated Cost",
@"Information: RateLine Found BAF-FLT-Client Rate TESTORG1
Information: RateLine Found FRT-UNT-KG-Client Rate TESTORG1
Information: RateLine Found FRT-Job One Off Freight Rate
Information: RateLine Filtered FRT-UNT-KG-Client Rate TESTORG1	reason:	replaced by Job Negotiated Cost/Gateway Sell/One Off Freight Rate" };

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		public void TestFreightAutorateStandardModeWithGatewaySell()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 20;
			shipment.JS_ActualVolume = 2;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  6666.66m,
							JR_OSCostAmt =  6666.66m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20.00m,
							JR_OSCostAmt = 20.00m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		[TestDate(2016, 06, 06)]
		public void TestFreightAutorateAllInclusiveModeWithGatewaySell()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 120m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 120m;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20m, 2m);
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var orgAppointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts);

			shipment.JS_UniqueConsignRef = "S100216";
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 20m;
			shipment.JS_UnitFreightRate = 30;
			shipment.JS_RX_NKFrtRateCurrency = "USD";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  9999.99m,
							JR_OSCostAmt =  6666.66m,
							CostCalculationDescription = "333.333 Kilogram(s) @ AUD 20.00/KG",
							RevenueCalculationDescription = "333.333 Kilogram(s) @ USD 30.00/KG"
						}
			};

			AutorateAndAssert(expected, shipment, localClient);

			var expectedLogLines = new[] { @"Information: RateLine Found FRT-Job Gateway Sell",
			@"Information: RateLine Found FRT-Job One Off Freight Rate" };

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		[TestDate(2016, 06, 06)]
		public void TestFreightAutorateFreightPlusModeWithGatewaySell()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20m, 2m);
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var orgAppointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts);

			shipment.JS_UniqueConsignRef = "S100216";
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_UnitFreightRate = 30;
			shipment.JS_GatewayFreightSellRate = 10;
			shipment.JS_RX_NKFrtRateCurrency = "USD";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  9999.99m,
							JR_OSCostAmt =  3333.33m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt =  20m,
							JR_OSCostAmt =  20m,
						}
				};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutorateAndAssert(expected, shipment, localClient);
			}

			var expectedLogLines = new[] {
@"Information: AUTORATING COSTS FOR Shipment S100216
Information: RateLine Found FRT-Job Gateway Sell",
@"Information: AUTORATING REVENUE FOR Shipment S100216
Information: RatingHeader Found Client Rate TESTORG1 Entries: 1
Information: RateLine Found BAF-FLT-Client Rate TESTORG1
Information: RateLine Found FRT-UNT-KG-Client Rate TESTORG1
Information: RateLine Found FRT-Job One Off Freight Rate
Information: RateLine Filtered FRT-UNT-KG-Client Rate TESTORG1	reason:	replaced by Job Negotiated Cost/Gateway Sell/One Off Freight Rate" };

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		#endregion

		#region Existing Costs with Spot Rating Behaviour

		public void TestAutoCosting_WhereThereIsExistingCostsWithSAAorSBABehaviour_ShouldIgnoreRatesHavingFreightChargeGroup()
		{
			TransportProvider1.OH_IsCreditor = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			costEntry.RateLines.RemoveAndDeleteAll();

			costEntry.AddFlatRateLine(charge1.AC_Code, 100m);

			var originEntry = costing.AddRateEntry("ORG", "LSE", "AUSYD", "SGSIN");
			originEntry.RateLines.RemoveAndDeleteAll();
			originEntry.AddFlatRateLine(charge2.AC_Code, 400m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 1000m);

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00100";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var existingConsolCost = CreateConsolCost(consol, charge3, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.AllInAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m;

			Factory.Save();

			var expected = new[]
			{
				// We expect that CC1 being filtered because there is a cost on consol with SAA rating behaviour
				new AssertionCost { ChargeCode = "CC2", E6_OSCostAmount = 400m },
				new AssertionCost { ChargeCode = "CC3", E6_OSCostAmount = 1234m }, // This charge already existed on consol.
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutoCostAndAssert("", null, expected, consol, false, deleteExistingCosts: false);
			}

			var expectedLogNote = @"Information: AUTORATING COSTS FOR Consol C00100
Information: RatingHeader Found Costing TRASPROV1 Entries: 2
Information: RateLine Found CC1-FLT-Costing TRASPROV1
Information: RateLine Found CC2-FLT-Costing TRASPROV1
Information: RateLine Filtered CC1-FLT-Costing TRASPROV1	reason:	removed due to existing Costs with SAA/SBA Rating Behavior
Information: CHARGES CALCULATED:
	CC2: Base Rate AUD 400.00
Information: Consol C00100 was auto-costed.
	The following costs were found:
	  • CC2 charge from Costing TRASPROV1
	Charges created: CC2";

			AssertAutoratingAuditLogContains(consol, expectedLogNote);
		}

		public void TestAutoCosting_WhereThereIsExistingCostsWithSAForSBFBehaviour_ShouldFilterRatesWithSameChargeCode()
		{
			TransportProvider1.OH_IsCreditor = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			costEntry.RateLines.RemoveAndDeleteAll();

			costEntry.AddFlatRateLine(charge1.AC_Code, 100m);
			costEntry.AddFlatRateLine(charge3.AC_Code, 600m);

			var originEntry = costing.AddRateEntry("ORG", "LSE", "AUSYD", "SGSIN");
			originEntry.RateLines.RemoveAndDeleteAll();
			originEntry.AddFlatRateLine(charge2.AC_Code, 400m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 1000m);

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00100";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var existingConsolCost = CreateConsolCost(consol, charge2, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.FreightAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCost { ChargeCode = "CC1", E6_OSCostAmount = 100m },
				new AssertionCost { ChargeCode = "CC2", E6_OSCostAmount = 1234m }, // This charge already existed on consol, and will be there by its existing amount.
				new AssertionCost { ChargeCode = "CC3", E6_OSCostAmount = 600m },
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutoCostAndAssert("", null, expected, consol, false, deleteExistingCosts: false);
			}

			var expectedLogNote = @"Information: AUTORATING COSTS FOR Consol C00100
Information: RatingHeader Found Costing TRASPROV1 Entries: 2
Information: RateLine Found CC1-FLT-Costing TRASPROV1
Information: RateLine Found CC2-FLT-Costing TRASPROV1
Information: RateLine Found CC3-FLT-Costing TRASPROV1
Information: RateLine Filtered CC2-FLT-Costing TRASPROV1	reason:	removed due to existing Costs with exact charge code and SAF/SBF Rating Behavior
Information: CHARGES CALCULATED:
	CC1: Base Rate AUD 100.00
	CC3: Base Rate AUD 600.00
Information: Consol C00100 was auto-costed.
	The following costs were found:
	  • CC1 charge from Costing TRASPROV1
	  • CC3 charge from Costing TRASPROV1
	Charges created: CC1, CC3";

			AssertAutoratingAuditLogContains(consol, expectedLogNote);
		}

		public void TestAutoRating_WhenExistingCostsWithSpotBehaviour_ShouldRecalculateExistingCostsUsingQuickCalculatorPreservedInformation()
		{
			TransportProvider1.OH_IsCreditor = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			costEntry.RateLines.RemoveAndDeleteAll();

			costEntry.AddFlatRateLine(charge1.AC_Code, 100m);

			var originEntry = costing.AddRateEntry("ORG", "LSE", "AUSYD", "SGSIN");
			originEntry.RateLines.RemoveAndDeleteAll();
			originEntry.AddFlatRateLine(charge2.AC_Code, 400m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 1000m);

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00100";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var existingConsolCost = CreateConsolCost(consol, charge3, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.AllInAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m; //Charge existing value is different from what it should be. Consider it as an old value. 
			var paymentBasis = existingConsolCost.PaymentBases.AddNew();
			paymentBasis.PBS_ChargeableBasis = "Weight";
			paymentBasis.PBS_PerUnitRate = 1.3m;

			Factory.Save();

			var expected = new[]
			{
				// We expect that CC1 being filtered because there is a cost on consol with SAA rating behaviour
				new AssertionCost { ChargeCode = "CC2", E6_OSCostAmount = 400m },
				new AssertionCost { ChargeCode = "CC3", E6_OSCostAmount = 1300m }, // This charge already existed on consol, but we expect that its value being recalculated (1300 = 1.3 * 1000);
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutoCostAndAssert("", null, expected, consol, false, deleteExistingCosts: false);
			}

			var expectedLogNote = @"Information: AUTORATING COSTS FOR Consol C00100
Information: RatingHeader Found Costing TRASPROV1 Entries: 2
Information: RateLine Found CC1-FLT-Costing TRASPROV1
Information: RateLine Found CC2-FLT-Costing TRASPROV1
Information: RateLine Filtered CC1-FLT-Costing TRASPROV1	reason:	removed due to existing Costs with SAA/SBA Rating Behavior
Information: CHARGES CALCULATED:
	CC2: Base Rate AUD 400.00
Information: Consol C00100 was auto-costed.
	The following costs were found:
	  • CC2 charge from Costing TRASPROV1
	Charges created: CC2";

			AssertAutoratingAuditLogContains(consol, expectedLogNote);

			var spotReference = consol.Numbers.GetAllReferenceNumbersByType("SPO")[0];
			AssertEquals("Spot Reference should be created for consol, as its existing spot costs is recalculated", "SAA CC3 AUD 1.30@KG Min: 0", spotReference);
		}

		#endregion

		#region TestAutorateNegotiatedCostSetsConsolCreditorAsCreditor

		public void TestAutorateNegotiatedCostSetsConsolCreditorAsCreditor()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "ZAJNB";
			consol.JK_Phase = "ALL";

			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			var orgAppointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts.O5_PortOrCountry = "AUSYD";
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts);

			var carrier = Helper.NewOrgHeader();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "ZAJNB";
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_FreightCostRate = 20;
			shipment.JS_RX_NKFreightCostRateCurrency = "AUD";
			shipment.JS_ActualWeight = 10m;
			shipment.JS_UnitOfWeight = "KG";

			Factory.Save();

			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));
				var forwardingDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

				shipmentJob.JH_GE = gatewayDepartment.PK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), forwardingDepartment.PK.ToGuid()))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							CostAccountCode = consol.Creditor.OH_Code,
							JR_OSCostAmt = 200m,
							JR_OSSellAmt = 200m,
						}
					};

					AutorateAndAssert(expected, shipment, GlbCompany.CurrentCompany.OrgProxy, null, null, false);
				}
			}
		}

		#endregion

		#region TestCalculationMessageWhenMeasureTypeCannotBeFound

		public void TestCalculationMessageWhenMeasureTypeCannotBeFound()
		{
			var cne = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(cne);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.JP).GetCalculator<UnitCalculator>().PerUnit = 20;
			rateEntry.TI_RX_NKCurrency = "AUD";

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, cne.PK, "USLAX", "AUSYD", 1500);
			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected, shipment, cne);
			AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Log should contain expected line",
				"Information: RateLine Filtered FRT-UNT-JP-Client Rate TESTORG1	reason:	Enter a valid Units.");
		}

		#endregion

		public void TestRateConsolCostRevenue_CostApportionedOnSave_ShipmentRevenueIsCorrect()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var cne = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader();
			var agent = Helper.NewOrgHeader();

			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;

			var cost = Helper.NewCosting(carrier);

			var costEntry = cost.AddRateEntry("AIR", "LSE", "AUSYD", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("FRT", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 3000m;

			var rate = Helper.NewClientRate(cne);

			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 250m;
			rateLine.TL_RX_NKCurrency = "AUD";

			Helper.Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.SetDefaultShippingLineAddress(carrier);
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightCostRateAutoratingMode = "STD";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.ConsigneePK = cne.PK;
			shipment.ConsignorPK = Helper.NewOrgHeader().PK;
			shipment.JS_INCO = "CIF";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_AgentCollectAddr = cne.MainAddress.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;

			Factory.Save();

			var newFactory = NewFactory();
			var newConsol = newFactory.Load<ForwardingConsol>(consol.PK);

			using (var form = new ZForm(newConsol))
			{
				var starter = new AutoRatingStarter(newConsol, new AutoRatingGUIInteractor(form));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue.With(billingType: BillingType.Apportionment));
				newFactory.Save();
			}

			var charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull(charge);
			AssertEquals("Sell amount should be calculated from client rate and its value is 250m.", 250m, charge.JR_OSSellAmt);
			AssertEquals("Cost amount should be calculated from costing and its value is 3000m.", 3000m, charge.JR_OSCostAmt);

			var revenueCalculationDescription = charge.Notes.FindByDescription("AUTORATE_SELL", false, SQLComparisonOperator.StartsWith).FirstOrDefault();
			var costCalculationDescription = charge.Notes.FindByDescription("AUTORATE_COST", false, SQLComparisonOperator.StartsWith).FirstOrDefault();

			AssertNotNull(revenueCalculationDescription);
			AssertNotNull(costCalculationDescription);
		}

		#region TestQuotedBookingsContainerCommodityCodeIsConsisered

		public void TestQuotedBookingsContainerCommodityCodeIsConsisered()
		{
			var rate = Helper.NewClientRate(NewClient);
			var fclEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", ZString.Empty, ZString.Empty, "40HC");
			fclEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(fclEntry.Currency);
			var orgEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", ZString.Empty, ZString.Empty, "40HC");
			orgEntry.RateLines.RemoveAndDeleteAll();

			var frtLine = fclEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 700m;

			var odocLine = orgEntry.AddRateLine("ODOC", FlatCalculator.Code);
			odocLine.GetCalculator<FlatCalculator>().BaseRate = 500m;

			Factory.Save();

			var quickBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", "CIF", NewClient, null, Consignee, null, "AUSYD", "CNSHA", 450m, 1m, QuotedBookingState.BookingOnly);

			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40HC")).PK;
			container.JC_ContainerCount = 1;

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 700,
						},
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt = 500,
						}
				};

			AutorateAndAssert(expected, quickBooking, NewClient);

			fclEntry.TI_RH_NKCommodityCode = "MINL";
			orgEntry.TI_RH_NKCommodityCode = "MINL";
			container.JC_RH_NKContainerCommodityCode = "MINL";

			Factory.Save();

			AutorateAndAssert(expected, quickBooking, NewClient);
		}

		#endregion

		#region TestPercentageCalculatorCorrectWithMinAndUnitCalculators

		public void TestPercentageCalculatorCorrectWithMinAndUnitCalculators()
		{
			var localClient = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", "MIN");
			rateLine1.GetCalculator<MinimumCalculator>().MinimumValue = 20m;
			rateLine1.GetCalculator<MinimumCalculator>().IsChargeCodeMinimum = true;

			var rateLine2 = rateEntry.AddRateLine("CAF", "FLT");
			rateLine2.UseOnlyActualWeightMeasure = true;
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 10;

			var rateLine3 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.UseOnlyActualWeightMeasure = true;
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 0.01m;

			var rateLine4 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine4.UseOnlyActualWeightMeasure = true;
			rateLine4.GetCalculator<UnitCalculator>().PerUnit = 0.01m;

			var rateLine5 = rateEntry.AddRateLine("BAF", "PER");
			rateLine5.GetCalculator<PercentageCalculator>().Percent = 10m;

			RateLineItem rateLineItem = rateLine5.RateLineItems.AddNew();
			rateLineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			rateLineItem.TM_Text = CalculatorConstants.Text.ChargeCode;
			rateLineItem.TM_AC = rateLine1.TL_AC;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_ActualWeight = 120;
			shipment.JS_ActualVolume = 2;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 20m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 2m,
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 10m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 0.1m;
			rateLine4.GetCalculator<UnitCalculator>().PerUnit = 0.1m;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 24m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 2.4m,
						},
					new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSSellAmt = 10m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		#endregion

		#region TestAutoRateResultUseTransitTimeCompareToApplyProperlyRateEntry

		public void TestAutoRateResultUseTransitTimeCompareToApplyProperlyRateEntry()
		{
			var currentDateTime = ZDateTime.Now;

			var consignee = Helper.NewOrgHeader(1);
			var consignor = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry1.RateLines.RemoveAndDeleteAll();
			entry1.TI_TransitTime = "1";
			var rateLine1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1;

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry2.RateLines.RemoveAndDeleteAll();
			entry2.TI_TransitTime = "25";
			var rateLine2 = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = currentDateTime;
			shipment.JS_E_ARV = currentDateTime.AddDays(30);

			Factory.Save();

			var expected = new[]
							{
								new AssertionCharge
									{
										ChargeCode = "FRT",
										JR_OSSellAmt = 300m,
										RevenueCalculationDescription = "Transit Time:" + "\t\t" + "25" + System.Environment.NewLine,
									}
							};

			AutorateAndAssert(expected, shipment, consignee);

			shipment.JS_E_ARV = currentDateTime.AddDays(1);
			Factory.Save();

			expected = new[]
							{
								new AssertionCharge
									{
										ChargeCode = "FRT",
										JR_OSSellAmt = 100m,
										RevenueCalculationDescription = "Transit Time:" + "\t\t" + "1" + System.Environment.NewLine,
									}
							};

			AutorateAndAssert(expected, shipment, consignee);
		}

		#endregion

		#region Test Autorating with IATA City Codes

		public void TestAutoRatingMatchesByIATACityCode()
		{
			var carrier = Helper.NewOrgHeader();
			var cost = Helper.NewCosting(carrier);

			var entry = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "LON", ZString.Empty);
			entry.RateLines.RemoveAndDeleteAll();
			var costLine = entry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "GBLHR", "AUSYD", 200m);
			var consol = CreateForwardingConsol(TransportModes.Air, "GBLHR", "AUSYD", carrier, shipment);
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 2000m } };

			AutorateAndAssert(expected, shipment, Consignee);
		}

		public void TestAutoRatingMatchesByIATACityCode_PrefersIATACityCodeToCountry()
		{
			var carrier = Helper.NewOrgHeader();
			var cost = Helper.NewCosting(carrier);

			var entry1 = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "LON", ZString.Empty);
			entry1.RateLines.RemoveAndDeleteAll();
			var costLine1 = entry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var entry2 = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "GB", ZString.Empty);
			entry2.RateLines.RemoveAndDeleteAll();
			var costLine2 = entry2.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 40m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "GBLHR", "AUSYD", 200m);
			var consol = CreateForwardingConsol(TransportModes.Air, "GBLHR", "AUSYD", carrier, shipment);
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 2000m } };

			AutorateAndAssert("Should prefer IATA Rate to Country Rate", expected, shipment, Consignee);
		}

		public void TestAutoRatingMatchesByIATACityCode_PrefersUNLOCOToIATACityCode()
		{
			var carrier = Helper.NewOrgHeader();
			var cost = Helper.NewCosting(carrier);

			var entry1 = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "LON", ZString.Empty);
			entry1.RateLines.RemoveAndDeleteAll();
			var costLine1 = entry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var entry2 = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "GBLHR", ZString.Empty);
			entry2.RateLines.RemoveAndDeleteAll();
			var costLine2 = entry2.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "GBLHR", "AUSYD", 200m);
			var consol = CreateForwardingConsol(TransportModes.Air, "GBLHR", "AUSYD", carrier, shipment);
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 4000m } };

			AutorateAndAssert("Should prefer UNLOCO Rate to IATA Rate", expected, shipment, Consignee);
		}

		#endregion

		#region TestAutoRatingInternationalZones

		public void TestAutoRatingSupportsAllOrganizationsInInternationalZone()
		{
			var carrier = Helper.NewOrgHeader();
			var zone = Helper.NewInternationalZone("EURO", carrier, CountryCodes.Germany);

			var cost = Helper.NewCosting(carrier);
			var orgEntry = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "EURO", ZString.Empty);
			var desCostLine = orgEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			desCostLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "DEHAM", "AUSYD", 200m);
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = carrier.MainAddress.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 2000m,
				}
			};

			AutorateAndAssert(expected, shipment, Consignor);
		}

		public void TestAutoRatingGenericZonesWhenRateEntryIsClientSpecific()
		{
			var carrier = Helper.NewOrgHeader(1);

			var zone1 = Helper.NewInternationalZone("EURO", carrier, CountryCodes.Germany);
			var zone2 = Helper.NewInternationalZone("TC-3", null, CountryCodes.Germany);

			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "TC-3", "AUSYD");
			entry.TI_OH_TransportProvider = carrier.PK;
			entry.RateLines.RemoveAndDeleteAll();

			var rateLine = entry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "DEHAM", "AUSYD", 200m);
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 2000m,
				}
			};

			AutorateAndAssert(expected, shipment, carrier);
		}

		public void TestAutoRatingCorrectlyCachesFreightLegs()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");

			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader(1);

			var tariff = Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.DST, RateMode.LCL, "AUEC", "AUMEL", "DDOC", 10);

			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUEC", "AUMEL", ZString.Empty, ZString.Empty);
			tariffEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(tariffEntry.Currency);

			var tariffRateLine = tariffEntry.AddRateLine("FRT", FlatCalculator.Code);
			tariffRateLine.GetCalculator<FlatCalculator>().BaseRate = 20m;

			tariff.Factory.Save();

			var clientRate = Helper.NewClientRateWithSingleRateLine(client, RatingConstants.RateCategory.DST, RateMode.LCL, "AUEC", "AUMEL", "DDOC", 15);

			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "AUMEL", ZString.Empty, ZString.Empty);
			clientEntry.RateLines.RemoveAndDeleteAll();

			var clientRateLine = clientEntry.AddRateLine("FRT", FlatCalculator.Code);
			clientRateLine.GetCalculator<FlatCalculator>().BaseRate = 25m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_ActualWeight = 200m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "PPD";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 10m,
				},

				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 25m
				}
			};

			AutorateAndAssert("Should prefer Client Rate over Company Tariff for FRT", expected, shipment, client);
		}

		#endregion

		#region TestInfiniteLoopCSTCMB

		public void TestInfiniteLoopCSTCMB()
		{
			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;

			var cost = Helper.NewCosting(carrier);

			var costEntry1 = cost.AddRateEntry("AIR", "LSE", "AUSYD", "");
			costEntry1.RateLines.RemoveAndDeleteAll();
			var costLine1 = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = cost.AddRateEntry("AIR", "LSE", "", "NZAKL");
			costEntry2.RateLines.RemoveAndDeleteAll();
			var costLine2 = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.SetDefaultShippingLineAddress(carrier);
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			var expectedErrors = new[] { $@"Error Consol {consol.JK_UniqueConsignRef} has encountered the following errors while AutoRating:
	•  Local Cost Amount: Please enter a Local Cost Amount.
	•  Overseas Cost Amount: Please enter an Overseas Cost Amount." };
			var expectedCost = new[] { new AssertionCost { ChargeCode = "FRT", E6_OSCostAmount = 0m } };
			var message = "Cost with empty amount is supposed to be autorated as this situation is not resolvable. Expect no infinite loop";

			void AssertNoInfiniteLoop(string line1Calculator, string line2Calculator)
			{
				costLine1.TL_RateCalculator = line1Calculator;
				costLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10;
				costLine2.TL_RateCalculator = line2Calculator;
				costLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 20;

				Factory.Save();

				AutoCostAndAssert(message, null, expectedCost, consol, expectedErrors: expectedErrors);
			}

			AssertNoInfiniteLoop(CompanyTariffOrCostBasedCalculator.CostBasedCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			AssertNoInfiniteLoop(CompanyTariffOrCostBasedCalculator.CostBasedCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			AssertNoInfiniteLoop(CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			AssertNoInfiniteLoop(CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
		}

		#endregion

		#region TestCalculationLogOnlyCreatedIfRatingApplies

		public void TestCalculationLogOnlyCreatedIfRatingApplies()
		{
			var client = NewClient;
			var clientRate = Helper.NewClientRate(client);
			var entries = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USCHI");
			entries.RateLines.RemoveAndDeleteAll();
			var rateLine = entries.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 125;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;

			var charge = job.Charges.AddNew();
			charge.JR_AC = Helper.ChargeCodes["FRT"].PK;
			charge.JR_OSSellAmt = 125m;
			charge.JR_AgentDeclaredSellAmt = 125m;
			charge.JR_SellRatingOverride = true;

			Factory.Save();

			var revenueLogs = LoadRevenueLogs(job.PK);
			AssertEquals(0, revenueLogs.Length);

			var expectedCharges = new[]
			{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 125
						},
				};

			AutorateAndAssert(expectedCharges, shipment, client, null, job);

			Factory.Save();

			revenueLogs = LoadRevenueLogs(job.PK);
			AssertEquals(0, revenueLogs.Length);

			charge.ApplyCustomQuickCalculator(shipment.RatingAdapter, 0m, 150m);
			charge.JR_SellRatingOverride = false;
			charge.JR_CostRatingOverride = false;

			shipment.JS_RL_NKDestination = "USCHI";

			AutorateAndAssert(expectedCharges, shipment, client, null, job);

			Factory.Save();

			revenueLogs = LoadRevenueLogs(job.PK);
			AssertEquals(1, revenueLogs.Length);
		}

		#endregion

		#region TestCalculationLogsAreDisabledWhenAmountChanged

		public void TestCalculationLogsAreDisabledWhenAmountChanged()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entries = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USCHI");
			entries.RateLines.RemoveAndDeleteAll();
			var rateLine = entries.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 320;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;

			Factory.Save();

			var expectedCharges = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 320
						}
				};

			AutorateAndAssert(expectedCharges, shipment, client, null, job);

			var charge = job.Charges[0];
			Assert(!charge.JR_SellRatingOverride);

			charge.JR_OSSellAmt = 350;
			Assert(charge.JR_SellRatingOverride);

			charge.JR_SellRatingOverride = false;

			Factory.Save();

			var revenueLogs = LoadRevenueLogs(job.PK);
			AssertEquals(1, revenueLogs.Length);
			AssertContains("<IsDisabled>Y</IsDisabled>", revenueLogs[0].ST_NoteDataAsText);
		}

		#endregion

		#region TestConsolCostingPrioritizesCreditor

		public void TestConsolCostingPrioritizesCreditor()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;

			var creditorCosting = Helper.NewCosting(creditor);
			var creditorCostEntry = creditorCosting.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			var creditorCostLine = creditorCostEntry.AddRateLine("FRT", FlatCalculator.Code);
			creditorCostLine.GetCalculator<FlatCalculator>().BaseRate = 20;

			var carrierCosting = Helper.NewCosting(carrier);
			var carrierCostEntry = carrierCosting.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			var carrierCostLine = carrierCostEntry.AddRateLine("FRT", FlatCalculator.Code);
			carrierCostLine.GetCalculator<FlatCalculator>().BaseRate = 30;

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLON";
			consol.Shipments.AddNew();

			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var transport = consol.Transports[0];
			transport.JW_IsLinked = false;
			transport.JW_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			var expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 20m,
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutoCostAndAssert("Creditor should take priority", null, expectedCosts, consol, false);

			var costs = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.CostSupporter.PK));

			foreach (var jobConsolCost in costs)
			{
				jobConsolCost.Delete();
			}

			transport.JW_OA_CreditorAddress = ZGuid.Empty;

			Factory.Save();

			expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 30m,
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutoCostAndAssert("Should fallback to Carrier", null, expectedCosts, consol, false);
		}

		#endregion

		#region AutoratedConsolCostsAreValidatedAndIncludedWithErrors

		public void TestAutoratedConsolCostsAreValidatedAndIncludedWithErrors()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG).GetCalculator<UnitCalculator>().PerUnit = 1;

			cost.Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			AutoCostAndAssert("Consol has no shipments with pack lines to measure", null, null, consol);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 30m);
			consol.Shipments.Add(shipment);

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate())
			{
				job.JH_OA_LocalChargesAddr = shipment.ConsigneeDocumentaryAddress.E2_OA_Address;
				Factory.Save();

				var expectedCost = new[]
				{
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 30m,
					}
				};

				AutoCostAndAssert("", null, expectedCost, consol);
				AssertAutoratingAuditLogNoteContainsLines(consol, "Should contain expected lines", "Information: RateLine Found BAF-UNT-KG-Costing TESTORG1");
			}
		}

		[TestDate(2003, 03, 03)]
		[DisableZeroExchangeRateOverriding]
		public void TestNotificationsEncounteredMessageUsesHumanReadableNames()
		{
			var dofQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "DOF");
			dofQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var dofChargeCode = Factory.LoadTop1<AccChargeCode>(dofQuery);
			dofChargeCode.AC_IsGroupageCharge = true;

			var carrier = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(null);
			var costEntry1 = costing.AddRateEntry("DST", "SEA", "JPAAM", "AUBNE");
			costEntry1.TI_OH_TransportProvider = carrier.PK;

			costEntry1.RateLines.RemoveAndDeleteAll();

			var costLine1 = costEntry1.AddRateLine(dofChargeCode, UnitCalculator.Code, QuantityUnit.LW, CurrencyCodes.NewZealand);
			var costLine2 = costEntry1.AddRateLine(dofChargeCode, UnitCalculator.Code, QuantityUnit.HB, CurrencyCodes.NewZealand);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 40;
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 60;

			var shipment1 = CreateForwardingShipment(TransportModes.Sea, localClient.PK, consignee.PK, "JPAAM", "AUBNE", 100m, 5m);
			var shipment2 = CreateForwardingShipment(TransportModes.Sea, localClient.PK, consignee.PK, "JPOSA", "AUBNE", 150m, 2m);
			var shipment3 = CreateForwardingShipment(TransportModes.Sea, localClient.PK, consignee.PK, "JPAAM", "AUBNE", 500m, 5m);

			var consol = CreateForwardingConsol(TransportModes.Sea, "JPAAM", "AUBNE", carrier, shipment1, PaymentType.Prepaid);
			Factory.Save();

			var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreate();
			var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreate();
			var job3 = new JobHeader.Loader(shipment3).TryLoadOrCreate();

			var consolJobID = consol.JK_UniqueConsignRef;
			var expectedCosts = Array.Empty<AssertionCost>();
			var expectedErrors = new[] { "Apportion Split Charge should not have 0 Cost Amount." };
			var expectedLogLines = new[]
			{
				$@"Information: AUTORATING COSTS FOR Consol {consolJobID}
Information: RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1",
				$@"Information: RateLine Found DOF-UNT-LW-Standard Costs (TACT/General Rates)
Information: RateLine Found DOF-UNT-HB-Standard Costs (TACT/General Rates)
Information: CHARGES CALCULATED:",
				"\tDOF: 0 Lowest Bill(s) @ NZD 40.00/Lowest Bill\n\tDOF: 1 House Bill(s) @ NZD 60.00/House Bill",
				$@"Error: Consol {consolJobID} has encountered the following errors while AutoRating:
	•  Local Cost Amount: Local amount cannot be zero when Overseas Cost Amount is non zero.
	•  Cost Exchange Rate: Cost Exchange Rate for Currency NZD must be greater than 0.
	•  Exchange Rate: Please enter an Exchange Rate.
	•  Exchange Rate: Exchange Rate for Currency NZD must be greater than 0.
	•  Local Cost Amount: Please enter a Local Cost Amount.",
$@"Information: Consol {consolJobID} was auto-costed.
	The following costs were found:
	  • DOF charge from Standard Costs (TACT/General Rates)
	Charges created: DOF"
			};

			try
			{
				AssertExceptionThrown<OnSavingCriticalCheckException<JobCharge>>(() => AutoCostAndAssert("", null, expectedCosts, consol, false, expectedErrors: expectedErrors));
				AssertAutoratingAuditLogNoteContainsLines(consol, "We expect each error to appear only once", expectedLogLines);
			}
			finally
			{
				job1.Dispose();
				job2.Dispose();
				job3.Dispose();
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestDeclarationPullsLCLRatesForNonFCLFreightModes

		public void TestDeclarationPullsLCLRatesForNonFCLFreightModes()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			#region Create Rate

			var cclrQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR");
			cclrQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var cclrChargeCode = Factory.Load<AccChargeCode>(cclrQuery).FirstOrDefault();
			cclrChargeCode.AC_ChargeGroup = "OBR";

			Env.Registry.Rating.SetOriginBrokerageRatedCodes("OBR,BRK");
			Env.Registry.Rating.SetFreightRatedCodes("ORG");

			var rate = Helper.NewClientRate(localClient);
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var rateLine2 = rateEntry1.AddRateLine("CCLR", AgencyCalculator.Code);
			rateLine2.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			rateLine2.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
			rateLine2.GetCalculator<AgencyCalculator>().AgencyRate = 200m;

			#endregion

			#region Create shipment

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = "BBK";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USLAX";

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = "BBK";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "BBK";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			#endregion

			#region Create declaration

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.BreakBulk;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_ShipmentIncoTerm = "CFR";
			declaration.JE_JS = shipment.PK;

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = container.PK;
			cusContainer.CO_FCL_LCL_AIR = ContainerModes.BreakBulk;

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt =  100m,
						},
					new AssertionCharge
						{
							ChargeCode = "CCLR",
							JR_OSSellAmt =  200m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		#endregion

		#region TestAutoratingDeclarationWithCountrySpecificTransportModes

		public void TestPostDeclaration()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CCLR");
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var cclrQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR");
			cclrQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var cclrChargeCode = Factory.Load<AccChargeCode>(cclrQuery).FirstOrDefault();
			cclrChargeCode.AC_ChargeGroup = "OBR";

			Env.Registry.Rating.SetOriginBrokerageRatedCodes("OBR,BRK");

			var rate = Helper.NewClientRate(localClient);
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.MAI, "AUSYD", "USLAX", ZString.Empty);
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("CCLR", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Mail;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_ShipmentIncoTerm = "CFR";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CCLR",
							JR_OSSellAmt =  100m,
						}
				};

			AutorateAndAssert(expected, declaration, localClient);
		}

		#endregion

		#region TestAutoratingDeclarationWithCountrySpecificTransportModes

		public void TestAutoratingDeclarationWithCountrySpecificTransportModes()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			#region Create Rate

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CCLR");
			Factory.Save();

			var cclrQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR");
			cclrQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var cclrChargeCode = Factory.Load<AccChargeCode>(cclrQuery).FirstOrDefault();
			cclrChargeCode.AC_ChargeGroup = "OBR";

			Env.Registry.Rating.SetOriginBrokerageRatedCodes("OBR,BRK");
			Env.Registry.Rating.SetFreightRatedCodes("ORG");

			var rate = Helper.NewClientRate(localClient);
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FTL, "AUBNE", "USLAX");
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var rateLine2 = rateEntry1.AddRateLine("CCLR", AgencyCalculator.Code);
			rateLine2.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			rateLine2.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
			rateLine2.GetCalculator<AgencyCalculator>().AgencyRate = 200m;

			#endregion

			#region Create shipment

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Road;
			consol.JK_ConsolMode = ContainerModes.FTL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USLAX";

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = ContainerModes.FTL;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_PackingMode = ContainerModes.FTL;
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CFR";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			#endregion

			#region Create declaration

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration.JE_TransportMode = "TRK";
			declaration.JE_ContainerMode = ContainerModes.FTL;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_ShipmentIncoTerm = "CFR";
			declaration.JE_JS = shipment.PK;

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = container.PK;
			cusContainer.CO_FCL_LCL_AIR = ContainerModes.FTL;

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSSellAmt =  100m,
						},
					new AssertionCharge
						{
							ChargeCode = "CCLR",
							JR_OSSellAmt =  200m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		#endregion

		#region Auto Rating Declaration With Service Provider

		public void TestAutoRatingDeclarationWithContainerYardAsServiceProvider()
		{
			Env.Registry.Rating.SetOriginBrokerageRatedCodes("BRK");

			var containerYard = Helper.NewOrgHeader();

			var localClient = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US", "CCLR", 100m);
			rateEntry.TI_OH_Supplier = containerYard.PK;

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.AU.IJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Air;
			declaration.JE_ContainerMode = ContainerModes.Loose;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_ShipmentIncoTerm = IncoTerms.DeliveredDutyPaid;
			declaration.ContainerYardDocAddress.OrganisationPK = containerYard.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CCLR",
							JR_OSSellAmt =  100m,
						}
				};

			AutorateAndAssert(expected, declaration, localClient);
		}

		#endregion

		#region TestAutoratingStandardCostsShouldFindDefaultCreditor

		public void TestAutoratingStandardCostsShouldFindDefaultCreditor()
		{
			TransportProvider1.OH_IsCreditor = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_ConsolMode = "LSE";
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Helper.NewOrgHeader(1).PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Helper.NewOrgHeader().PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ActualWeight = 30;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CIF";

			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)costLine.Calculator).BaseRate = 500;

			Factory.Save();

			var expectedCosts = new[]
					{
						new AssertionCost
							{
								ChargeCode = "FRT",
								E6_OSCostAmount = 500m,
							}
					};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("", null, expectedCosts, consol, false);

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			AssertEquals(1, consolCosts.Length);
			AssertEquals("Autorating standard costs should find the default creditor from ChargeCode when providerPK is empty in rateInfo.", TransportProvider1.PK, consolCosts[0].E6_OH_Creditor);
		}

		#endregion

		#region TestAutoratingClientRateShouldFindDefaultCreditor

		public void TestAutoratingClientRateShouldFindDefaultCreditor_ShipmentWithConsol()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var originEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "");
			originEntry.TI_OH_Supplier = creditor.PK;
			var originLine = originEntry.AddRateLine("ODOC", FlatCalculator.Code);
			originLine.GetCalculator<FlatCalculator>().BaseRate = 30;

			var freightEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			freightEntry.TI_OH_Supplier = creditor.PK;
			freightEntry.RateLines.RemoveAndDeleteAll();
			var freightLine = freightEntry.AddRateLine("FRT", FlatCalculator.Code);
			freightLine.GetCalculator<FlatCalculator>().BaseRate = 200;

			var destinationEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "AU", "");
			destinationEntry.TI_OH_Supplier = creditor.PK;
			var destinationLine = destinationEntry.AddRateLine("DDOC", FlatCalculator.Code);
			destinationLine.GetCalculator<FlatCalculator>().BaseRate = 40;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "GBLHR", 100m);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLON";

			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var transport = consol.Transports[0];
			transport.JW_IsLinked = false;

			Factory.Save();

			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "ODOC",
								CostAccountCode = creditor.OH_Code,
								JR_LocalSellAmt = 30m,
							},
						new AssertionCharge
							{
								ChargeCode = "FRT",
								CostAccountCode = creditor.OH_Code,
								JR_LocalSellAmt = 200m,
							},
						new AssertionCharge
							{
								ChargeCode = "DDOC",
								CostAccountCode = creditor.OH_Code,
								JR_LocalSellAmt = 40m,
							},
					};

			AutorateAndAssert("Autorating revenue only should still default the creditor when applicable by charge code group", expected, shipment, client, autorateCosts: false);
		}

		public void TestAutoratingClientRateShouldFindDefaultCreditor_StandAloneShipment()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var originEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "");
			originEntry.TI_OH_Supplier = creditor.PK;
			var originLine = originEntry.AddRateLine("ODOC", FlatCalculator.Code);
			originLine.GetCalculator<FlatCalculator>().BaseRate = 30;

			var freightEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			freightEntry.TI_OH_Supplier = creditor.PK;
			freightEntry.RateLines.RemoveAndDeleteAll();
			var freightLine = freightEntry.AddRateLine("FRT", FlatCalculator.Code);
			freightLine.GetCalculator<FlatCalculator>().BaseRate = 200;

			var destinationEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "AU", "");
			destinationEntry.TI_OH_Supplier = creditor.PK;
			var destinationLine = destinationEntry.AddRateLine("DDOC", FlatCalculator.Code);
			destinationLine.GetCalculator<FlatCalculator>().BaseRate = 40;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "GBLHR", 100m);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = creditor.MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = carrier.MainAddress.PK;

			Factory.Save();

			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "ODOC",
								CostAccountCode = creditor.OH_Code,
								JR_LocalSellAmt = 30m
							}
					};

			var message = @"Autorating revenue only should still default the creditor.
As the pickup cartage matches the creditor all rate entries match the shipment job.
However we only expect to see the creditor default for the origin charges";

			AutorateAndAssert(message, expected, shipment, client, autorateCosts: false);

			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = creditor.MainAddress.PK;

			expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "ODOC",
								CostAccountCode = creditor.OH_Code,
								JR_LocalSellAmt = 30m
							},
						new AssertionCharge
							{
								ChargeCode = "DDOC",
								CostAccountCode = creditor.OH_Code,
								JR_LocalSellAmt = 40m
							},
					};

			message = "Now destination charges should also default the credtior";
			AutorateAndAssert(message, expected, shipment, client, autorateCosts: false);
		}

		#endregion

		#region Job Internal Info

		public void TestNonGateway_RatingSell_InternalFieldsNotToBeSetToCriteriaJob()
		{
			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "ODOC", 100);
			TransportProvider1.OH_IsCreditor = true;

			Helper.NewClientRateWithSingleRateLine(NewClient, RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 250);

			var shipment = CreateForwardingShipment(TransportModes.Air, NewClient.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			var branchA = CreateBranchProxy(TransportProvider1, "AAA");
			var branchB = CreateBranchProxy(NewClient, "BBB");

			Factory.Save();

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				var job = new Job.Loader(shipment).TryLoadOrCreate();
				job.JH_GB = Env.CurrentBranchPK;
				job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES")).PK;
				job.JH_OA_LocalChargesAddr = NewClient.MainAddress.PK;

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_CostRated = true,
						JR_SellRated = false,
						JR_LocalCostAmt = 100m,
						CostAccountCode = TransportProvider1.OH_Code,
						SellAccountCode = "",
						JR_GB_InternalBranch = branchA.PK,
						JR_GE_InternalDept = job.JH_GE,
						JR_JH_InternalJob = job.PK,
					},
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_CostRated = false,
						JR_SellRated = true,
						JR_LocalSellAmt = 250,
						CostAccountCode = "",
						SellAccountCode = NewClient.OH_Code,
						JR_GB_InternalBranch = branchB.PK,
						JR_GE_InternalDept = job.JH_GE,
						JR_JH_InternalJob = job.PK,
					}
				};

				AutorateAndAssert(expected, shipment, NewClient);
			}
		}

		public void TestJobInternalInfosNotSetForNonGatewayShipment()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20, 2);
				Factory.Save();

				var expected = new[]
					{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 6666.66m,
							JR_GE_InternalDept = ZGuid.Empty,
							JR_GB_InternalBranch = ZGuid.Empty,
							JR_JH_InternalJob = ZGuid.Empty
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20.00m,
							JR_GE_InternalDept = ZGuid.Empty,
							JR_GB_InternalBranch = ZGuid.Empty,
							JR_JH_InternalJob = ZGuid.Empty
						}
				};

				AutorateAndAssert(expected, shipment, localClient);
			}
		}

		public void TestAJRJDefaultingDoesNotDefaultBranchIfOrgProxyAppearsInMultipleBranches()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK);

			var currentOrgProxy = GlbBranch.CurrentBranch.OrgProxy;
			currentOrgProxy.OH_IsCreditor = true;

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = Env.CurrentCompanyPK;
			newBranch.GB_OH_OrgProxy = currentOrgProxy.PK;

			var chargeCode = Helper.ChargeCodes["OCART"];
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode(chargeCode);

			var costing = Helper.NewCosting(currentOrgProxy);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "CN");
			var costLine = costEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			costLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Helper.NewOrgHeader().PK, ZGuid.Empty, "AUSYD", "CNSHA", 120);
			shipment.DocsAndCartage.PickupCartageCoPK = currentOrgProxy.PK;

			Factory.Save();

			var job = new Job.Loader(shipment).TryLoadOrCreate();
			job.JH_GB = newBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES")).PK;
			job.JH_OA_LocalChargesAddr = currentOrgProxy.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 100m,
					CostAccountCode = currentOrgProxy.OH_Code,
					SellAccountCode = "",
					JR_GB_InternalBranch = ZGuid.Empty,
					JR_GE_InternalDept = job.JH_GE,
					JR_JH_InternalJob = job.PK,
				}
			};

			AssertEquals(newBranch.PK, job.JH_GB);
			AutorateAndAssert("Should auto-rate and default internal fields from Job. EDICUS is the orgproxy for several branches so it should not be defaulted", expected, shipment, currentOrgProxy);

			var charge = job.Charges[0];
			charge.JR_OH_CostAccount = ZGuid.Empty;

			AssertEquals("Should clear internal job fields as neither cost nor sell are OrgProxies", ZGuid.Empty, charge.JR_GB_InternalBranch);
			AssertEquals(ZGuid.Empty, charge.JR_GE_InternalDept);
			AssertEquals(ZGuid.Empty, charge.JR_JH_InternalJob);

			charge.JR_OH_CostAccount = currentOrgProxy.PK;

			AssertEquals("Should not default branch if OrgProxy applies to several branches in the same company", ZGuid.Empty, charge.JR_GB_InternalBranch);
			AssertEquals(job.JH_GE, charge.JR_GE_InternalDept);
			AssertEquals(job.PK, charge.JR_JH_InternalJob);
		}

		#endregion

		#region Loading Meter as Chargeable

		public void TestLoadingMeterAsChargeableUnit()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6000, Volume.CubicCentimeters, Weight.Kilograms),
				new ConversionFactor(194, Volume.CubicInches, Weight.Pounds)));

			FreightDataRegistry.Instance.InternationalChargeableFactorRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6000, Volume.CubicCentimeters, Weight.Kilograms),
				new ConversionFactor(194, Volume.CubicInches, Weight.Pounds)));

			var client = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry("LCL", "FTL", "USLAX", "USSFO");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.LM);
			rateLine.TL_ConversionFactorString = (new ConversionFactor(1000m, Weight.Kilograms, LoadingLength.LoadingMeters)).ToShortString();
			((UnitCalculator)rateLine.Calculator).PerUnit = 100m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_PackingMode = ContainerModes.FTL;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "USSFO";

			shipment.JS_ActualWeight = 1024m;
			shipment.JS_ActualVolume = 10m;
			shipment.JS_LoadingMeters = 3.2m;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 320m,
							RevenueCalculationDescription = "FRT: 3.2 Loading Meter(s) @ USD 100.00/Loading Meter"
						},
				};

			AutorateAndAssert("Chargeable calculated from LM is bigger than one calculated from weight/volume. LM's actual quantity is used.",
				expected, shipment, client, autorateCosts: false);

			shipment.JS_ActualWeight = 8192m;
			shipment.JS_ActualVolume = 10m;
			shipment.JS_LoadingMeters = 3.2m;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 819.2m,
							RevenueCalculationDescription = "FRT: 8.192 Loading Meter(s) @ USD 100.00/Loading Meter"
						},
				};

			AutorateAndAssert("Chargeable calculated from LM is bigger than one calculated from weight/volume. LM's actual quantity is used.",
				expected, shipment, client, autorateCosts: false);

			shipment.JS_ActualWeight = 8192m;
			shipment.JS_ActualVolume = 100m;
			shipment.JS_LoadingMeters = 3.2m;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 1666.7m,
							RevenueCalculationDescription = "FRT: 16.667 Loading Meter(s) @ USD 100.00/Loading Meter"
						},
				};

			AutorateAndAssert("Chargeable calculated from LM is smaller than one calculated from weight/volume. LM quantity is re-calculated.",
				expected, shipment, client, autorateCosts: false);

			rateLine.TL_ActualPercentage = 50;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 993.35m,
							RevenueCalculationDescription = "FRT: 9.9335 Loading Meter(s) @ USD 100.00/Loading Meter"
						},
				};

			AutorateAndAssert("LM's quantity is re-calculated from calculated chargeable and actual values",
				expected, shipment, client, autorateCosts: false);

			rateLine.TL_ActualPercentage = 100;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 320m,
							RevenueCalculationDescription = "FRT: 3.2 Loading Meter(s) @ USD 100.00/Loading Meter"
						},
				};

			AutorateAndAssert("LM's actual quantity is used",
				expected, shipment, client, autorateCosts: false);
		}

		#endregion

		#region TestAutoRatingCostsOnCrossTrade

		public void TestAutoRatingCostsOnCrossTrade()
		{
			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsCreditor = true;
			var cnr = Helper.NewOrgHeader();

			var consignee = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "");
			costEntry.TI_IsCrossTrade = true;
			var costLine = costEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLine.GetCalculator<FlatCalculator>().BaseRate = 40;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(carrier);
			consol.JK_OA_SendingForwarderAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 40
				}
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutoCostAndAssert("Costing Cross Trade", null, expectedCosts, consol);
		}

		#endregion

		#region TestApplyMultipleChangesFirstOnCompanyTariffOrCostBasedCalculator

		public void TestApplyMultipleChangesFirstOnCompanyTariffOrCostBasedCalculator()
		{
			var client = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffRate = tariffEntry.AddRateLine("FRT", FlatPlusPerUnitCalculator.Code, QuantityUnit.KG);
			tariffRate.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate = 200m;
			tariffRate.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit = 0;

			var clientRate = Helper.NewClientRate(consignee);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var line = clientRateEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;
			AddParityExchangeRate(line.Currency);

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "USLAX", "AUBNE", 10m);

			Factory.Save();

			#region Percentage + Base + Minimum

			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 33m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Minimum = 500m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().CalculationOrder = "PER";
			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 500m,
							RevenueCalculationDescription = "FRT: Minimum USD 500.00"
						}
				};

			AutorateAndAssert("Percent First", expected, shipment, client, autorateCosts: false);

			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().CalculationOrder = "FIX";
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 550m,
							RevenueCalculationDescription = "FRT: Minimum USD 550.00"
						}
				};

			AutorateAndAssert("Increase First", expected, shipment, client);

			#endregion

			#region Percentage + Base + Unit

			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Minimum = 0m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 5m;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 311.3m,
							RevenueCalculationDescription = "FRT: Base Rate USD 36.30 + 10 Kilogram(s) @ USD 5.50/KG + 110.00% of (Base Rate USD 200.00)"
						}
				};

			AutorateAndAssert("Increase First", expected, shipment, client);

			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().CalculationOrder = "PER";
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 303m,
							RevenueCalculationDescription = "FRT: Base Rate USD 33.00 + 10 Kilogram(s) @ USD 5.00/KG + 110.00% of (Base Rate USD 200.00)"
						}
				};

			AutorateAndAssert("Percent First", expected, shipment, client);

			#endregion

			#region Percentage + Minimum + Unit

			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 0m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Minimum = 13m;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 270m,
							RevenueCalculationDescription = "FRT: 110.00% of (Base Rate USD 200.00) + 10 Kilogram(s) @ USD 5.00/KG"
						}
				};

			AutorateAndAssert("Calculate result is greater than minimum", expected, shipment, client);

			#endregion

			#region Percentage + Base + Minimum + Unit

			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 20m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Minimum = 60m;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 290m,
							RevenueCalculationDescription = "FRT: Base Rate USD 20.00 + 10 Kilogram(s) @ USD 5.00/KG + 110.00% of (Base Rate USD 200.00)"
						}
				};

			AutorateAndAssert("Percent First", expected, shipment, client);

			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().CalculationOrder = "FIX";
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 297m,
							RevenueCalculationDescription = "FRT: Base Rate USD 22.00 + 10 Kilogram(s) @ USD 5.50/KG + 110.00% of (Base Rate USD 200.00)"
						}
				};

			AutorateAndAssert("Increase First", expected, shipment, client);

			#endregion
		}

		#endregion

		#region TestAutoratingChargeableRoundingDefaultFromRegistry

		public void TestAutoratingChargeableRoundingDefaultFromRegistry()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var cnr = Helper.NewOrgHeader(1);
			var cne = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("FRT", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_ConsolChargeable = 1600;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CIF";

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate())
			{
				job.JH_OA_LocalChargesAddr = cnr.MainAddress.PK;
				Factory.Save();

				var roundings = new DefaultRoundingsCollection();
				var rounding = roundings.AddNew();
				rounding.Code = "AIR";
				rounding.RoundingType = "NOR";

				var expectedCosts = new[]
				{
					new AssertionCost
						{
							CostCalculationDescription = "FRT: 1500 Kilogram(s) @ AUD 1.00/KG",
							ChargeCode = "FRT",
							E6_OSCostAmount = 1500,
						},
				};

				using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
				{
					AutoCostAndAssert("", null, expectedCosts, consol, true);
				}

				rounding.RoundingType = "CHG";

				expectedCosts = new[]
				{
					new AssertionCost
						{
							CostCalculationDescription = "FRT: 1600 Kilogram(s) @ AUD 1.00/KG",
							ChargeCode = "FRT",
							E6_OSCostAmount = 1600,
						},
				};

				using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
				{
					AutoCostAndAssert("", null, expectedCosts, consol, true);
					AssertEquals("Chargeable is taken from overridden amount when rounding is 'Chargeable'", 1600m, consol.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);
				}
			}
		}

		#endregion

		#region TestHandlingExistingCharges

		//See "Merging with preexisting charges test cases" Excel sheet attached to WI00065720
		public void TestMergingWithPreExistingCharges()
		{
			#region Setup Cost/Rate

			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUFRE");
			costingEntry.TI_RX_NKCurrency = "AUD";
			costingEntry.RateLines.RemoveAndDeleteAll();

			var costLineFRT = costingEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1000m;

			var costLineBAF = costingEntry.AddRateLine("BAF", FlatCalculator.Code);
			costLineBAF.GetCalculator<FlatCalculator>().BaseRate = 500m;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUFRE");
			rateEntry.TI_RX_NKCurrency = "AUD";
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLineFRT = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1200m;

			var rateLineBAF = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLineBAF.GetCalculator<FlatCalculator>().BaseRate = 700m;

			#endregion;

			#region Setup Shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUFRE";
			shipment.JS_INCO = "FOB";
			shipment.ConsignorPK = client.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var ratingAdapter = shipment.RatingAdapter;

			#endregion

			#region Setup Existing Charges

			var expected = new[]
			{
				MakeExpectedCharge(costLineFRT, rateLineFRT),
				MakeExpectedCharge(costLineBAF, rateLineBAF)
			};

			AutorateAndAssert(expected, shipment, client);
			Factory.Save();

			var job = shipment.Job as Job;
			var baseChargeFRT = job.Charges.Where(x => x.ChargeCode.AC_Code == "FRT").First();
			var baseChargeBAF = job.Charges.Where(x => x.ChargeCode.AC_Code == "BAF").First();

			#endregion

			#region FRT and BAF Cost/Rate

			#region Test Case 1

			//1.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, true, true),
				MakeExpectedCharge("FRT", 1000, 1200, false, false),
				MakeExpectedCharge("BAF", 500, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);
			Factory.Save();

			//1.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, true, true),
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
				MakeExpectedCharge("BAF", 500, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);
			Factory.Save();

			//1.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, true, true),
				MakeExpectedCharge("FRT", 1200, 1200, false, false),
				MakeExpectedCharge("BAF", 500, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 2

			costLineBAF.GetCalculator<FlatCalculator>().BaseRate = 800m;
			rateLineBAF.GetCalculator<FlatCalculator>().BaseRate = 900m;

			Factory.Save();

			//2.a
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, true, true),
				MakeExpectedCharge("FRT", 1000, 1200, false, false),
				MakeExpectedCharge("BAF", 800, 900, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//2.c
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, true, true),
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
				MakeExpectedCharge("BAF", 800, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//2.r
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, true, true),
				MakeExpectedCharge("FRT", 1200, 1200, false, false),
				MakeExpectedCharge("BAF", 500, 900, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 3

			costLineFRT.GetCalculator<FlatCalculator>().BaseRate = 900m;
			rateLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1100m;

			Factory.Save();

			//3.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 900, 1200, false, true),
				MakeExpectedCharge("BAF", 500, 900, true, false),
				MakeExpectedCharge("BAF", 800, 800, false, false),
				MakeExpectedCharge("FRT", 1100, 1100, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//3.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 900, 1200, false, true),
				MakeExpectedCharge("BAF", 500, 700, true, false),
				MakeExpectedCharge("BAF", 800, 800, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//3.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, false, true),
				MakeExpectedCharge("BAF", 500, 900, true, false),
				MakeExpectedCharge("FRT", 1100, 1100, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 4

			//4.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1100, true, false),
				MakeExpectedCharge("BAF", 800, 500, false, true),
				MakeExpectedCharge("FRT", 900, 900, false, false),
				MakeExpectedCharge("BAF", 900, 900, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//4.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, false),
				MakeExpectedCharge("BAF", 800, 500, false, true),
				MakeExpectedCharge("FRT", 900, 900, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//4.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1100, true, false),
				MakeExpectedCharge("BAF", 500, 500, false, true),
				MakeExpectedCharge("BAF", 900, 900, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 5

			//5.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 900, 1100, false, false),
				MakeExpectedCharge("BAF", 500, 500, true, true),
				MakeExpectedCharge("BAF", 800, 900, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//5.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 900, 900, false, false),
				MakeExpectedCharge("BAF", 500, 500, true, true),
				MakeExpectedCharge("BAF", 800, 800, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//5.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 1100, 1100, false, false),
				MakeExpectedCharge("BAF", 500, 500, true, true),
				MakeExpectedCharge("BAF", 900, 900, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 6

			//6.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, false, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 900, 1100, false, false),
				MakeExpectedCharge("BAF", 800, 900, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//6.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, false, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 900, 1000, false, false),
				MakeExpectedCharge("BAF", 800, 500, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//6.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, false, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1100, false, false),
				MakeExpectedCharge("BAF", 500, 900, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 7

			//7.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 1000, 1000, true, false, ratingAdapter);

			var chargeFRT2 = job.Charges.AddNew();
			chargeFRT2.JR_AC = baseChargeFRT.JR_AC;
			chargeFRT2.JR_Desc = baseChargeFRT.JR_Desc;
			chargeFRT2.JR_OH_CostAccount = baseChargeFRT.JR_OH_CostAccount;
			chargeFRT2.JR_OH_SellAccount = baseChargeFRT.JR_OH_SellAccount;
			chargeFRT2.ApplyCustomQuickCalculator(ratingAdapter, 500m, 0m);
			chargeFRT2.JR_LocalSellAmt = 500;
			chargeFRT2.JR_CostRatingOverride = false;
			chargeFRT2.JR_SellRatingOverride = true;
			chargeFRT2.JR_AT_CostGSTRate = baseChargeFRT.JR_AT_CostGSTRate;
			chargeFRT2.JR_AT_SellGSTRate = baseChargeFRT.JR_AT_SellGSTRate;

			var chargeBAF2 = job.Charges.AddNew();
			chargeBAF2.JR_AC = baseChargeBAF.JR_AC;
			chargeBAF2.JR_Desc = baseChargeBAF.JR_Desc;
			chargeBAF2.JR_OH_CostAccount = baseChargeBAF.JR_OH_CostAccount;
			chargeBAF2.JR_OH_SellAccount = baseChargeBAF.JR_OH_SellAccount;
			chargeBAF2.ApplyCustomQuickCalculator(ratingAdapter, 0m, 500m);
			chargeBAF2.JR_LocalCostAmt = 500;
			chargeBAF2.JR_CostRatingOverride = false;
			chargeBAF2.JR_SellRatingOverride = true;
			chargeBAF2.JR_AT_CostGSTRate = baseChargeBAF.JR_AT_CostGSTRate;
			chargeBAF2.JR_AT_SellGSTRate = baseChargeBAF.JR_AT_SellGSTRate;

			ResetJobCharges(job, baseChargeFRT, baseChargeBAF, chargeFRT2, chargeBAF2);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1100, true, false),
				MakeExpectedCharge("FRT", 900, 500, false, true),
				MakeExpectedCharge("BAF", 1000, 900, true, false),
				MakeExpectedCharge("BAF", 800, 500, false, true),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//7.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(chargeFRT2, 500, 500, false, true, ratingAdapter);
			SetExistingChargeValues(chargeBAF2, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF, chargeFRT2, chargeBAF2);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, false),
				MakeExpectedCharge("FRT", 900, 500, false, true),
				MakeExpectedCharge("BAF", 1000, 1000, true, false),
				MakeExpectedCharge("BAF", 800, 500, false, true),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//7.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(chargeFRT2, 500, 500, false, true, ratingAdapter);
			SetExistingChargeValues(chargeBAF2, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF, chargeFRT2, chargeBAF2);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1100, true, false),
				MakeExpectedCharge("FRT", 500, 500, false, true),
				MakeExpectedCharge("BAF", 1000, 900, true, false),
				MakeExpectedCharge("BAF", 500, 500, false, true),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 8

			//8.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(chargeFRT2, 500, 500, false, false, ratingAdapter);
			SetExistingChargeValues(chargeBAF2, 500, 500, false, false, ratingAdapter);

			ResetJobCharges(job, baseChargeFRT, baseChargeBAF, chargeFRT2, chargeBAF2);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 900, 1100, false, false),
				MakeExpectedCharge("BAF", 1000, 1000, true, true),
				MakeExpectedCharge("BAF", 800, 900, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//8.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(chargeFRT2, 500, 500, false, false, ratingAdapter);
			SetExistingChargeValues(chargeBAF2, 500, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF, chargeFRT2, chargeBAF2);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 900, 500, false, false),
				MakeExpectedCharge("BAF", 1000, 1000, true, true),
				MakeExpectedCharge("BAF", 800, 500, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//8.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(chargeFRT2, 500, 500, false, false, ratingAdapter);
			SetExistingChargeValues(chargeBAF2, 500, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF, chargeFRT2, chargeBAF2);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 500, 1100, false, false),
				MakeExpectedCharge("BAF", 1000, 1000, true, true),
				MakeExpectedCharge("BAF", 500, 900, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#endregion

			#region FRT Cost/Rate only

			#region Setup Rates

			costLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1000m;
			rateLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1200m;
			costLineBAF.Delete();
			rateLineBAF.Delete();

			Factory.Save();

			#endregion

			#region Test Case 9

			//9.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 1000, 1200, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//9.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
				MakeExpectedCharge("BAF", 700, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//9.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 700, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 1200, 1200, false, false),
				MakeExpectedCharge("BAF", 700, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 10

			//10.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1500, false, true),
				MakeExpectedCharge("FRT", 1200, 1200, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//10.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1500, false, true),
				MakeExpectedCharge("BAF", 700, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//10.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 700, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1500, false, true),
				MakeExpectedCharge("BAF", 700, 700, false, false),
				MakeExpectedCharge("FRT", 1200, 1200, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 11

			//11.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1500, false, true),
				MakeExpectedCharge("FRT", 1200, 1200, false, false),
				MakeExpectedCharge("BAF", 500, 500, false, true),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//11.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1500, false, true),
				MakeExpectedCharge("BAF", 500, 500, false, true),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//11.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1500, false, true),
				MakeExpectedCharge("FRT", 1200, 1200, false, false),
				MakeExpectedCharge("BAF", 500, 500, false, true),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 12

			//12.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, true, false),
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
				MakeExpectedCharge("BAF", 500, 500, true, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//12.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1500, true, false),
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
				MakeExpectedCharge("BAF", 500, 500, true, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//12.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1500, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, true, false),
				MakeExpectedCharge("BAF", 500, 500, true, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 13

			//13.a
			SetExistingChargeValues(baseChargeFRT, 1500, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1500, 1200, true, false),
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//13.c
			SetExistingChargeValues(baseChargeFRT, 1500, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1500, 1000, true, false),
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
				MakeExpectedCharge("BAF", 700, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//13.r
			SetExistingChargeValues(baseChargeFRT, 1500, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 700, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1500, 1200, true, false),
				MakeExpectedCharge("BAF", 700, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 14

			//14.a
			SetExistingChargeValues(baseChargeFRT, 1500, 1250, false, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//14.c
			SetExistingChargeValues(baseChargeFRT, 1500, 1250, false, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1250, false, false),
				MakeExpectedCharge("BAF", 700, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//14.r
			SetExistingChargeValues(baseChargeFRT, 1500, 1250, false, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 700, 500, false, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1500, 1200, false, false),
				MakeExpectedCharge("BAF", 700, 700, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#endregion

			#region Multiple Repeating Rates

			#region Setup Cost/Rate

			costLineFRT.GetCalculator<FlatCalculator>().BaseRate = 900m;
			rateLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1100m;

			var costLineFRT2 = costingEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLineFRT2.GetCalculator<FlatCalculator>().BaseRate = 111m;
			costLineFRT2.TL_RX_NKCurrency = "AUD";

			costLineBAF = costingEntry.AddRateLine("BAF", FlatCalculator.Code);
			costLineBAF.GetCalculator<FlatCalculator>().BaseRate = 800m;
			costLineBAF.TL_RX_NKCurrency = "AUD";

			var costLineBAF2 = costingEntry.AddRateLine("BAF", FlatCalculator.Code);
			costLineBAF2.GetCalculator<FlatCalculator>().BaseRate = 99m;
			costLineBAF2.TL_RX_NKCurrency = "AUD";

			var rateLineFRT2 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLineFRT2.GetCalculator<FlatCalculator>().BaseRate = 222m;
			rateLineFRT2.TL_RX_NKCurrency = "AUD";

			rateLineBAF = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLineBAF.GetCalculator<FlatCalculator>().BaseRate = 900m;
			rateLineBAF.TL_RX_NKCurrency = "AUD";

			var rateLineBAF2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLineBAF2.GetCalculator<FlatCalculator>().BaseRate = 109m;
			rateLineBAF2.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			#endregion;

			#region Test Case 15

			//15.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1011, 1200, false, true),
				MakeExpectedCharge("BAF", 500, 1009, true, false),
				MakeExpectedCharge("BAF", 899, 899, false, false),
				MakeExpectedCharge("FRT", 1322, 1322, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//15.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1011, 1200, false, true),
				MakeExpectedCharge("BAF", 500, 700, true, false),
				MakeExpectedCharge("BAF", 899, 899, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//15.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1200, false, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 700, true, false, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1200, false, true),
				MakeExpectedCharge("BAF", 500, 1009, true, false),
				MakeExpectedCharge("FRT", 1322, 1322, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 16

			//16.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1322, true, false),
				MakeExpectedCharge("FRT", 1011, 1011, false, false),
				MakeExpectedCharge("BAF", 899, 500, false, true),
				MakeExpectedCharge("BAF", 1009, 1009, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//16.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, false),
				MakeExpectedCharge("FRT", 1011, 1011, false, false),
				MakeExpectedCharge("BAF", 899, 500, false, true),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//16.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, false, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, false, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1322, true, false),
				MakeExpectedCharge("BAF", 500, 500, false, true),
				MakeExpectedCharge("BAF", 1009, 1009, false, false)
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#region Test Case 17

			//17.a
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 1011, 1322, false, false),
				MakeExpectedCharge("BAF", 500, 500, true, true),
				MakeExpectedCharge("BAF", 899, 1009, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client);

			//17.c
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 1011, 1011, false, false),
				MakeExpectedCharge("BAF", 500, 500, true, true),
				MakeExpectedCharge("BAF", 899, 899, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateRevenue: false);

			//17.r
			SetExistingChargeValues(baseChargeFRT, 1000, 1000, true, true, ratingAdapter);
			SetExistingChargeValues(baseChargeBAF, 500, 500, true, true, ratingAdapter);
			ResetJobCharges(job, baseChargeFRT, baseChargeBAF);

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, true, true),
				MakeExpectedCharge("FRT", 1322, 1322, false, false),
				MakeExpectedCharge("BAF", 500, 500, true, true),
				MakeExpectedCharge("BAF", 1009, 1009, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);

			#endregion

			#endregion
		}

		public void TestExistingCostsAreNotDeletedWhenRatingRevenue()
		{
			#region Setup Cost/Rate

			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUFRE", ZString.Empty, ZString.Empty);
			costingEntry.RateLines.RemoveAndDeleteAll();

			var costLineFRT = costingEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1000m;
			costLineFRT.TL_RX_NKCurrency = "AUD";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_IsDebtor = true;

			#endregion;

			#region Setup Shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUFRE";
			shipment.JS_INCO = "FOB";
			shipment.ConsignorPK = client.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			#endregion

			#region Setup Existing Charges

			var expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
			};

			AutorateAndAssert(expected, shipment, client, autorateRevenue: false);

			Factory.Save();

			var job = shipment.Job as Job;
			var baseChargeFRT = job.Charges.Where(x => x.ChargeCode.AC_Code == "FRT").First();

			#endregion

			expected = new[]
			{
				MakeExpectedCharge("FRT", 1000, 1000, false, false),
			};

			AutorateAndAssert(expected, shipment, job: job, localClient: client, autorateCosts: false);
		}

		public void TestExistingDisbursementChargesAreNotTickedAsSellRatingOverrideWhenRatingRevenue()
		{
			#region Setup Rate

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUFRE", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLineFRT = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1200m;
			rateLineFRT.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			#endregion;

			#region Setup Shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUFRE";
			shipment.JS_INCO = "FOB";
			shipment.ConsignorPK = client.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			#endregion

			#region Setup Existing Charges

			AccTaxRate.LoadExistingOrCreateNewTaxRate(new BusinessObjectFactory(), "EXEMPT", AccTaxRate.Types.Exempt, 0).Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			var charge = shipmentJob.Charges.AddNew();
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, "CUSDSB");
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var customsChargeCode = Factory.LoadTop1<AccChargeCode>(query);

			charge.JR_AC = customsChargeCode.PK;
			charge.JR_CostRatingOverride = false;
			charge.JR_SellRatingOverride = false;

			#endregion

			var expected = new[]
			{
				MakeExpectedCharge("FRT", 1200, 1200, false, false),
				MakeExpectedCharge("CUSDSB", 0, 0, false, false)
			};

			AutorateAndAssert(expected, shipment, client, job: shipmentJob, autorateCosts: false);
		}

		public void TestPostedCostsAreNotUpdatedWhenAutorating()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "");
			costingEntry.RateLines.RemoveAndDeleteAll();

			var costLineFSC = costingEntry.AddRateLine("FSC", FlatCalculator.Code);
			costLineFSC.GetCalculator<FlatCalculator>().BaseRate = 33m;
			costLineFSC.TL_RX_NKCurrency = "AUD";

			var tariff = Helper.NewCompanyTariff();
			var tariffFRTEntry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "");
			tariffFRTEntry.RateLines.RemoveAndDeleteAll();
			var tariffFRTLine = tariffFRTEntry.AddRateLine("FSC", FlatCalculator.Code);
			((FlatCalculator)tariffFRTLine.Calculator).BaseRate = 33;

			var localClient = Helper.NewOrgHeader(2);
			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader(1);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "NZAKL", 300);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate("USD", 7.1105m);
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = Helper.ChargeCodes["FSC"].PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_LocalCostAmt = 592.93m;

			charge.JR_OSCostAmt = 4216m;
			charge.JR_CostRatingOverride = false;

			var tr = Factory.NewWithValidTestData<AccTaxRate>();

			var th = Factory.NewWithValidTestData<AccTransactionHeader>();
			var tl = Factory.NewWithValidTestData<AccTransactionLines>();
			tl.AL_AH = th.PK;
			tl.AL_LineType = TransactionLineTypes.Cost;
			tl.AL_LineAmount = -200;
			tl.AL_OSAmount = -200;
			tl.AL_RX_NKTransactionCurrency = "AUD";
			tl.AL_RevRecognitionType = "IMM";

			charge.JR_AL_APLine = tl.PK;
			charge.JR_AT_SellGSTRate = tr.PK;

			Assert(charge.IsCostPosted);

			UnitTestUserNotification.Instance.ClearMessages();

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertEquals(4216m, charge.JR_OSCostAmt);
			}
		}

		#endregion

		#region Spot Quotes are filtered by Payment Terms

		[TestDate(2015, 11, 10)]
		public void TestSpotQuotesFilterByPaymentTermsForRevenue()
		{
			Helper.NewClientRateWithSingleRateLine(NewClient, RatingConstants.RateCategory.LCL, RateMode.LCL, "", "AUMEL", "BAF", 10m);
			Helper.NewClientRateWithSingleRateLine(Consignee, RatingConstants.RateCategory.LCL, RateMode.LCL, "", "AUMEL", "BAF", 15m);

			Factory.Save();

			var spotQuote = CreateQuotedBooking(TransportModes.Sea, RateMode.LCL, IncoTerms.ExWorks, Consignee, null, Consignee, null, "CNSHA", "AUMEL", 450m, 1m, QuotedBookingState.QuoteOnly);
			spotQuote.QuotedBookingNumber = "Q0023423";
			spotQuote.StartDate = ZDateTime.Today;
			spotQuote.EndDate = ZDate.Today.AddDays(14);

			var expected = new[] { new AssertionCharge { ChargeCode = "BAF", JR_OSSellAmt = 10m } };
			AutorateAndAssert("Import Collect prefers local client to consignee", expected, spotQuote, NewClient);

			spotQuote.Origin = "AUMEL";
			spotQuote.PaymentTerms = DomesticPaymentTerms.Collect;

			expected = new[] { new AssertionCharge { ChargeCode = "BAF", JR_OSSellAmt = 15m } };
			AutorateAndAssert("Spot quote is now domestic collect, which prefers consignee. It should not include local client rates", expected, spotQuote, NewClient);
		}

		#endregion

		#region TestGivenClientOnOOQIsOverriden_WhenAutoRating_CompanyTariffLevelOverrideShouldBeApplied

		[TestDate(2023, 10, 7)]
		public void TestGivenClientOnOOQIsOverriden_WhenAutoRating_CompanyTariffLevelOverrideShouldBeApplied()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(2);
			consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var tariff1 = Factory.New<CompanyTariff>();
			var rateEntry = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var tariffRate1 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			tariffRate1.GetCalculator<FlatCalculator>().BaseRate = 200m;
			var tariff2 = Factory.New<CompanyTariff>();
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 10m);

			Factory.Save();

			var spotQuote = CreateQuotedBooking(TransportModes.Air, ContainerModes.Loose, DomesticPaymentTerms.Collect, null, consignor, consignee, null, "USLAX", "AUBNE", 450m, 1m, QuotedBookingState.QuoteOnly);
			spotQuote.QuotedBookingNumber = "Q0023423";
			spotQuote.StartDate = ZDateTime.Today;
			spotQuote.EndDate = ZDate.Today.AddDays(14);
			spotQuote.CompanyTariffLevel = "2";
			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 180,
							}
					};

			var expectedBaseCompanyTariff = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 200,
							}
					};

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert("Given OOQ's Client isn't overriden, When AutoRating and not allow override CT Level, Then Job's CompanyTariffLevelOverride should not be applied", expectedBaseCompanyTariff, spotQuote, consignee);
				spotQuote.ClientDocAddress.E2_AddressOverride = true;
				AutorateAndAssert("Given OOQ's Client is overriden, When AutoRating and not allow override CT Level, Then Job's CompanyTariffLevelOverride should be applied", expected, spotQuote, consignee);
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert("Given the registry AllowOverrideCompanyTariffLevel is true, When AutoRating, Then Job's CompanyTariffLevelOverride should be applied regardless Client is checked or not", expected, spotQuote, consignee);
				spotQuote.ClientDocAddress.E2_AddressOverride = true;
				AutorateAndAssert("Given the registry AllowOverrideCompanyTariffLevel is true, When AutoRating, Then Job's CompanyTariffLevelOverride should be applied regardless Client is checked or not", expected, spotQuote, consignee);
			}
		}

		#endregion

		#region Payment Term on Shipment and Consol

		public void TestPaymentTermIsDefinedFromShipmentIfNoPaymentTermOnConsol_CFR()
		{
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var dstChargeCode = Helper.ChargeCodes["DCART"];
			dstChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			var orgChargeCode = Helper.ChargeCodes["OCART"];
			orgChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 900m, CurrencyCodes.Australia);
			var costEntryDST = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "USLAX", dstChargeCode.AC_Code, 400m, CurrencyCodes.Australia);
			var costEntryORG = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", orgChargeCode.AC_Code, 300m, CurrencyCodes.Australia);

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostAndFreight;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSCostAmt = 900m,
				},
				new AssertionCharge
				{
					ChargeCode = orgChargeCode.AC_Code,
					JR_OSCostAmt = 300m
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestPaymentTermIsDefinedFromShipmentIfNoPaymentTermOnConsol_DDP()
		{
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var dstChargeCode = Helper.ChargeCodes["DCART"];
			dstChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			var orgChargeCode = Helper.ChargeCodes["OCART"];
			orgChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 900m, CurrencyCodes.Australia);
			var costEntryDST = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "USLAX", dstChargeCode.AC_Code, 400m, CurrencyCodes.Australia);
			var costEntryORG = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", orgChargeCode.AC_Code, 300m, CurrencyCodes.Australia);

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSCostAmt = 900m,
				},
				new AssertionCharge
				{
					ChargeCode = dstChargeCode.AC_Code,
					JR_OSCostAmt = 400m
				},
				new AssertionCharge
				{
					ChargeCode = orgChargeCode.AC_Code,
					JR_OSCostAmt = 300m
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestPaymentTermIsDefinedFromShipmentIfNoPaymentTermOnConsol_EXW()
		{
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var dstChargeCode = Helper.ChargeCodes["DCART"];
			dstChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 900m, CurrencyCodes.Australia);
			var costEntryDST = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "USLAX", dstChargeCode.AC_Code, 400m, CurrencyCodes.Australia);

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.ExWorks;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;
			consol.JK_PrepaidCollect = ZString.Empty;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSCostAmt = 900m,
				},
			};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region Combined Unit Calculators Auto Rate When Only One Unit Is Present

		public void TestCombinedUnitCalculatorsAutoRateWhenOnlyOneUnitIsPresent()
		{
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var bundleRateLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, PkgUnit.Bundle);
			var bundleMinimumItem = bundleRateLine.RateLineItems.AddNew();
			bundleMinimumItem.TM_Type = Calculator.Items.Operator.MIN;
			bundleMinimumItem.TM_RelevantValue = 60;
			var bundleMinusItem = bundleRateLine.RateLineItems.AddNew();
			bundleMinusItem.TM_Type = Calculator.Items.Operator.Minus;
			bundleMinusItem.TM_Break = 5;
			bundleMinusItem.TM_RelevantValue = 20;
			var bundlePlusItem = bundleRateLine.RateLineItems.AddNew();
			bundlePlusItem.TM_Type = Calculator.Items.Operator.Plus;
			bundlePlusItem.TM_Break = 5;
			bundlePlusItem.TM_RelevantValue = 18;

			var palletRateLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, PkgUnit.Pallet);
			var palletMinimumItem = palletRateLine.RateLineItems.AddNew();
			palletMinimumItem.TM_Type = Calculator.Items.Operator.MIN;
			palletMinimumItem.TM_RelevantValue = 100;
			var palletMinusItem = palletRateLine.RateLineItems.AddNew();
			palletMinusItem.TM_Type = Calculator.Items.Operator.Minus;
			palletMinusItem.TM_Break = 5;
			palletMinusItem.TM_RelevantValue = 60;
			var palletPlusItem = palletRateLine.RateLineItems.AddNew();
			palletPlusItem.TM_Type = Calculator.Items.Operator.Plus;
			palletPlusItem.TM_Break = 5;
			palletPlusItem.TM_RelevantValue = 55;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "USLAX", "AUSYD", 1000m);
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.JL_F3_NKPackType = PkgUnit.Pallet;

			Factory.Save();

			var expected = new[] { new AssertionCharge { JR_OSSellAmt = 660m } };

			AutorateAndAssert("(PLT 12 * $55 = $650), despite having no bundle pack line", expected, shipment, client);
		}

		#endregion

		#region Test correctly applying margin to charges

		public void TestJobChargeMarginCalculationDoesntApplyToAutoratedAmount()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUFRE", ZString.Empty, ZString.Empty);
			costingEntry.RateLines.RemoveAndDeleteAll();

			var costLineFRT = costingEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLineFRT.GetCalculator<FlatCalculator>().BaseRate = 0m;
			costLineFRT.TL_RX_NKCurrency = "AUD";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUFRE", ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLineFRT = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLineFRT.GetCalculator<FlatCalculator>().BaseRate = 1200m;
			rateLineFRT.TL_RX_NKCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUFRE";
			shipment.JS_INCO = "FOB";
			shipment.ConsignorPK = client.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			AssertionCharge expectedCharge = new AssertionCharge()
			{
				ChargeCode = "FRT",
				JR_LocalCostAmt = 0m,
				JR_LocalSellAmt = 1200m,
				RevenueCalculationDescription = "FRT: Base Rate AUD 1200.00",
				CostCalculationDescription = "FRT: Base Rate AUD 0.00"
			};

			AutorateAndAssert(new[] { expectedCharge }, shipment, client);
		}

		#endregion

		#region MultiModal Rating

		public void TestConsolMultiModalRating()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var carrier_Road = Helper.NewOrgHeader();
			var carrier_Sea = Helper.NewOrgHeader();
			carrier_Road.OH_IsCreditor = true;
			carrier_Sea.OH_IsCreditor = true;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var charge = Helper.ChargeCodes.NewConsolChargeCode("CCOST", "Consol Cost", "", ChargeCodeGroupList.Codes.Freight);

			var costing_Road = Factory.New<Costing>();
			costing_Road.TH_OH = carrier_Road.PK;

			var costEntry_Road = costing_Road.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.ROA, "AUMEL", "AUSYD");
			costEntry_Road.TI_RX_NKCurrency = "AUD";
			costEntry_Road.TI_OH_TransportProvider = carrier_Road.PK;
			costEntry_Road.RateLines.RemoveAndDeleteAll();
			costEntry_Road.AddRateLine(charge.AC_Code, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100m;

			var costing_Sea = Factory.New<Costing>();
			costing_Sea.TH_OH = carrier_Sea.PK;

			var costEntry_Sea = costing_Sea.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "SGSIN");
			costEntry_Sea.TI_RX_NKCurrency = "AUD";
			costEntry_Sea.TI_ViaLRC = "NZAKL";
			costEntry_Sea.TI_OH_TransportProvider = carrier_Sea.PK;
			costEntry_Sea.RateLines.RemoveAndDeleteAll();
			costEntry_Sea.AddRateLine(charge.AC_Code, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 250m;

			var costEntry_NoRouteSets = costing_Sea.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUMEL", "SGSIN");
			costEntry_NoRouteSets.TI_RX_NKCurrency = "AUD";
			costEntry_NoRouteSets.RateLines.RemoveAndDeleteAll();
			costEntry_NoRouteSets.AddRateLine(charge.AC_Code, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 350m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00032423";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.SetDefaultShippingLineAddress(carrier_Sea);
			consol.JK_OA_CreditorAddress = carrier_Sea.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var t1 = consol.Transports[0];

			t1.JW_VoyageFlight = "123R";
			t1.JW_RL_NKLoadPort = "AUMEL";
			t1.JW_RL_NKDiscPort = "AUSYD";
			t1.JW_ETD = ZDateTime.Now;
			t1.JW_ETA = ZDateTime.Now.AddDays(1);
			t1.JW_IsLinked = ZBool.True;
			t1.JW_TransportMode = TransportModes.Road;
			t1.JW_TransportType = TransportPlanningType.PreCarriage;
			t1.CarrierPK = carrier_Road.PK;

			var t2 = consol.Transports.AddNew("AUSYD", "NZAKL");
			t2.JW_VoyageFlight = "123S";
			t2.JW_ETD = ZDateTime.Now.AddDays(2);
			t2.JW_ETA = ZDateTime.Now.AddDays(4);
			t2.JW_TransportMode = TransportModes.Sea;
			t2.JW_TransportType = TransportPlanningType.MainVessel;
			t2.CarrierPK = carrier_Sea.PK;

			var t3 = consol.Transports.AddNew("NZAKL", "SGSIN");
			t3.JW_VoyageFlight = "123S";
			t3.JW_ETD = ZDateTime.Now.AddDays(5);
			t3.JW_ETA = ZDateTime.Now.AddDays(7);
			t3.JW_TransportMode = TransportModes.Sea;
			t3.JW_TransportType = TransportPlanningType.Other;
			t3.CarrierPK = carrier_Sea.PK;

			t1.JW_CarrierBookingReference = "";
			t2.JW_CarrierBookingReference = "0001";
			t3.JW_CarrierBookingReference = "0001";

			var shipment = consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var line = shipment.OuterPackLines.AddNew();
			line.JL_JC = container.PK;

			Factory.Save();

			var routeSetNumberDescription = DescriptionHelpers.FormatWithTab("Route Set Number:");

			var expected = new[]
			{
				new AssertionCost { ChargeCode = charge.AC_Code, E6_OSCostAmount = 100m, CostCalculationDescription = $"{routeSetNumberDescription}1" },
				new AssertionCost { ChargeCode = charge.AC_Code, E6_OSCostAmount = 250m, CostCalculationDescription = $"{routeSetNumberDescription}2" }
			};

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert("Should rate by routes", null, expected, consol, false);
			}

			expected = new[]
			{
				new AssertionCost { ChargeCode = charge.AC_Code, E6_OSCostAmount = 350m }
			};

			AutoCostAndAssert("Should rate by most interesting transport", null, expected, consol, false);
		}

		public void TestConsolMultiModalRatingSameCosting()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("CCOST", "Consol Cost", "", ChargeCodeGroupList.Codes.Freight);
			var chargeCode2 = Helper.ChargeCodes["WAR"];
			var refContainerPK = Helper.Containers["20GP"].PK;

			var costing = Helper.NewCosting(null);
			var roadCostEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.ROA, "AUMEL", "AUSYD");
			roadCostEntry.RateLines.RemoveAndDeleteAll();
			roadCostEntry.TI_RC = refContainerPK;
			var roadCostLine = roadCostEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			roadCostLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var seaCostEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "SGSIN");
			seaCostEntry.RateLines.RemoveAndDeleteAll();
			seaCostEntry.TI_ViaLRC = "NZAKL";
			seaCostEntry.TI_RC = refContainerPK;
			var seaCostLine = seaCostEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			seaCostLine.GetCalculator<UnitCalculator>().PerUnit = 250m;

			var seaCostLine2 = seaCostEntry.AddRateLine(chargeCode2, FlatCalculator.Code, "", CurrencyCodes.Australia);
			seaCostLine2.GetCalculator<FlatCalculator>().BaseRate = 80m;

			var seaCostLine3 = seaCostEntry.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.HB, CurrencyCodes.Australia);
			seaCostLine3.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00032423";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = refContainerPK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var t1 = consol.Transports[0];
			t1.JW_IsLinked = false;
			t1.JW_VoyageFlight = "VRRRRMM";
			t1.JW_RL_NKLoadPort = "AUMEL";
			t1.JW_RL_NKDiscPort = "AUSYD";
			t1.JW_ETD = ZDateTime.Now;
			t1.JW_ETA = ZDateTime.Now.AddDays(1);
			t1.JW_TransportMode = TransportModes.Road;
			t1.JW_TransportType = TransportPlanningType.PreCarriage;
			t1.JW_CarrierBookingReference = "Route1";

			var t2 = consol.Transports.AddNew("AUSYD", "NZAKL");
			t2.JW_IsLinked = false;
			t2.JW_VoyageFlight = "123S";
			t2.JW_ETD = ZDateTime.Now.AddDays(2);
			t2.JW_ETA = ZDateTime.Now.AddDays(4);
			t2.JW_TransportMode = TransportModes.Sea;
			t2.JW_TransportType = TransportPlanningType.MainVessel;

			var t3 = consol.Transports.AddNew("NZAKL", "SGSIN");
			t3.JW_IsLinked = false;
			t3.JW_VoyageFlight = "123S";
			t3.JW_ETD = ZDateTime.Now.AddDays(5);
			t3.JW_ETA = ZDateTime.Now.AddDays(7);
			t3.JW_TransportMode = TransportModes.Sea;
			t3.JW_TransportType = TransportPlanningType.Other;

			var shipment = consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var line = shipment.OuterPackLines.AddNew();
			line.JL_JC = container.PK;

			Factory.Save();
			var routeSetNumberDescription = DescriptionHelpers.FormatWithTab("Route Set Number:");

			var expected = new[]
			{
				new AssertionCost { ChargeCode = chargeCode.AC_Code, E6_OSCostAmount = 100m, CostCalculationDescription = $"{routeSetNumberDescription}1" },
				new AssertionCost { ChargeCode = chargeCode.AC_Code, E6_OSCostAmount = 250m, CostCalculationDescription = $"{routeSetNumberDescription}2" },
				new AssertionCost { ChargeCode = chargeCode2.AC_Code, E6_OSCostAmount = 280m }
			};

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var message = "FRT charge codes should not be rolled up as they are being applied to each consol costing adapter individually. WAR charges should be rolled up as they're applied within the same adapter";
				AutoCostAndAssert(message, null, expected, consol, false);
			}
		}

		#endregion

		#region Test Via UNLOCO, Zone, Country

		public void TestTranshipmentFilter_Via_Country()
		{
			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_FullName = "CNR ORG";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_FullName = "CNE ORG";

			var rate = Helper.NewClientRate(consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "USCA");
			rateEntry.TI_ViaLRC = "MY";

			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			((UnitCalculator)rateLine.Calculator).PerUnit = 5m;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "MYPKG";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CFR";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7500.00m,
				},
			};

			AutorateAndAssert(expected, shipment, consignor, job: job);
		}

		public void TestTranshipmentFilter_Via_UNLOCO()
		{
			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_FullName = "CNR ORG";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_FullName = "CNE ORG";

			var rate = Helper.NewClientRate(consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "USCA");
			rateEntry.TI_ViaLRC = "MYPKG";

			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			((UnitCalculator)rateLine.Calculator).PerUnit = 5m;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "MYPKG";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CFR";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7500.00m,
				},
			};

			AutorateAndAssert(expected, shipment, consignor, job: job);
		}

		public void TestTranshipmentFilter_Via_Zone()
		{
			Helper.NewInternationalZone("MYMY", null, CountryCodes.Malaysia);

			var rate = Helper.NewClientRate(Consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "USCA");
			rateEntry.TI_ViaLRC = "MYMY";
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "MYPKG";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CFR";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = Consignor.MainAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7500.00m,
				},
			};

			AutorateAndAssert(expected, shipment, Consignor, job: job);
		}

		#endregion

		#region Landed Costing Autorating

		[TestDate(2014, 01, 01)]
		public void TestAutorating_LandedCosting_Order()
		{
			var costProvider = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.BuyerPK = client.PK;
			order.JD_OH_ReceivingAgent = costProvider.PK;
			order.JD_IncoTerm = IncoTerms.FreeOnBoard;
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKGoodsAvailableAt = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "UAIEV";
			order.JD_RL_NKGoodsDeliveredTo = "UAIEV";
			order.JD_RX_NKOrderCurrency = CurrencyCodes.Australia;
			order.JD_TransportMode = TransportModes.Air;
			order.JD_ContainerMode = ContainerModes.Loose;
			order.JD_RN_NKCountryOfSupply = CountryCodes.Australia;
			order.JD_ActualWeight = 20m;
			order.JD_UnitOfWeight = Weight.Kilograms;

			var costing = Helper.NewCosting(costProvider);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV");
			costEntry.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			costEntry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV");
			clientRateEntry.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			clientRateEntry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)20m;

			Factory.Save();

			var lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.DefaultFromHost(order);
			lcHeader.SynchroniseAll();

			AssertEquals(2, lcHeader.CostInputs.Count);

			AssertEquals(200m, lcHeader.CostInputs[0].LI_CostAmount);
			AssertEquals("FRT: 20 Kilogram(s) @ AUD 10.00/KG", lcHeader.CostInputs[0].LI_ChargeDescription);
			AssertEquals(CurrencyCodes.Australia, lcHeader.CostInputs[0].LI_RX_NKCostCurrency);

			AssertEquals(400m, lcHeader.CostInputs[1].LI_CostAmount);
			AssertEquals("FRT: 20 Kilogram(s) @ AUD 20.00/KG", lcHeader.CostInputs[1].LI_ChargeDescription);
			AssertEquals(CurrencyCodes.Australia, lcHeader.CostInputs[1].LI_RX_NKCostCurrency);
		}

		#endregion

		#region Chargeable Unit sets Break Unit options

		[TestDate(2016, 1, 1)]
		public void TestChargeableUnitIsDistanceSoNoBreakUnitCanBeSet()
		{
			Env.Registry.Rating.UseDistanceCalculationService = true;

			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "MESS";
			consignor.MainAddress.OA_Address1 = "58 Mentmore Ave";
			consignor.MainAddress.OA_City = "Roseberry";
			consignor.MainAddress.OA_PostCode = "2018";
			consignor.MainAddress.OA_State = "NSW";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			Env.Registry.Rating.DefaultCFSAddressRoad = "2753";

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");

			var rate = Helper.NewClientRate(consignor);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LRO, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KM);
			var firstBreak = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 50, 10);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50, 8);

			Factory.Save();

			Assert("Expected no break unit can be set when chargeable unit is distance", firstBreak.TM_BreakWeightVolumeInfo.ReadOnly);

			var shipment = CreateForwardingShipment(TransportModes.Road, consignor.PK, ZGuid.Empty, "AUBNE", "AURCH", 100);
			shipment.JS_UniqueConsignRef = "SH0001010";
			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 424m,
							RevenueCalculationDescription = "ODOC: 53 Kilometer(s) @ AUD 8.00/Kilometer"
						}
				};

			var expectedLogLines = new[] { @"Information: RateLine Found ODOC-CMB-KM-Client Rate TESCLISYD",
@"Information: ODOC-CMB-KM-Client Rate TESCLISYD 53 KM
	- Pickup Distance: empty
	- Distance Calculation Service: 53 (Consignor Pickup Address to CTO/Wharf)
Information: ODOC-CMB-KM-Client Rate TESCLISYD 53 KM
	- Pickup Distance: empty
	- Distance Calculation Service: 53 (Consignor Pickup Address to CTO/Wharf)
Information: ODOC-CMB-KM-Client Rate TESCLISYD 53 KM
	- Pickup Distance: empty
	- Distance Calculation Service: 53 (Consignor Pickup Address to CTO/Wharf)
Information: ODOC-CMB-KM-Client Rate TESCLISYD 53 KM
	- Pickup Distance: empty
	- Distance Calculation Service: 53 (Consignor Pickup Address to CTO/Wharf)" };

			AutorateAndAssert("", expected, shipment, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		[TestDate(2016, 1, 1)]
		public void TestChargeableUnitIsNeitherPkgNorDistanceSoBreakUnitCanBeDistanceUnit()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			var consignor = Helper.NewOrgHeader();
			var consignorAddress = consignor.MainAddress;
			consignorAddress.OA_Address1 = "65 Katoomba St";
			consignorAddress.OA_City = "Katoomba";
			consignorAddress.OA_State = "NSW";
			consignorAddress.OA_PostCode = "2780";

			var rate = Helper.NewClientRate(consignor);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LRO, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 50, 4, QuantityUnit.KM);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50, 8);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100, 10);

			var shipment = CreateForwardingShipment(TransportModes.Road, consignor.PK, ZGuid.Empty, "AUSYD", "AUSYD", 3);
			shipment.JS_UniqueConsignRef = "SH0001010";
			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 12m,
							RevenueCalculationDescription = "ODOC: 3 Kilogram(s) @ AUD 4.00/KG (for 0 KM)"
						}
				};

			AutorateAndAssert("Should pick the rate break by distance but chargeable by kilo", expected, shipment, consignor, autorateCosts: false);

			Env.Registry.Rating.DefaultCFSAddressRoad = "2019";
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 24m,
							RevenueCalculationDescription = "ODOC: 3 Kilogram(s) @ AUD 8.00/KG (for 86.712 KM)"
						}
				};

			var expectedLogLines = new[] { @"Information: RateLine Found ODOC-CMB-KG-Client Rate TESTORG1",
@"Information: ODOC-CMB-KG-Client Rate TESTORG1 86.712 KM
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: 86.712 (Consignor Pickup Address to CTO/Wharf)
Information: ODOC-CMB-KG-Client Rate TESTORG1 86.712 KM
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: 86.712 (Consignor Pickup Address to CTO/Wharf)" };

			AutorateAndAssert("Should pick the rate break by distance but chargeable by kilo", expected, shipment, consignor, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		[TestDate(2016, 1, 1)]
		public void TestChargeableUnitIsNeitherPkgNorDistanceSoBreakUnitCanBeDistanceUnit_AccumulatedRatesOverrideBreakUnit()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			var consignor = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(consignor);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LRO, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			var minusItem = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 50, 4, QuantityUnit.KM);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50, 8);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 80, 9);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 160, 10);

			line.Calculator.IsAccumulated = true;

			Assert("Precondition: Setting is accumulated should clear the break unit", minusItem.TM_BreakWeightVolume.IsEmpty);
			Assert(minusItem.TM_BreakWeightVolumeInfo.ReadOnly);

			var shipment = CreateForwardingShipment(TransportModes.Road, consignor.PK, ZGuid.Empty, "AUSYD", "AUSYD", 95);
			shipment.JS_UniqueConsignRef = "SH0001010";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 575m,
							RevenueCalculationDescription = "ODOC: 15 Kilogram(s) @ AUD 9.00/KG + 30 Kilogram(s) @ AUD 8.00/KG + 50 Kilogram(s) @ AUD 4.00/KG"
						}
				};

			AutorateAndAssert("Should accumulate rates by distance", expected, shipment, consignor, autorateCosts: false);
		}

		[TestDate(2016, 1, 1)]
		public void TestDifferentChargeableAndBreakUnitsWorkWithCompanyTariffBasedCalc()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");

			var consignor = Helper.NewOrgHeader(1);
			var consignorAddress = consignor.MainAddress;
			consignorAddress.OA_Address1 = "65 Katoomba St";
			consignorAddress.OA_City = "Katoomba";
			consignorAddress.OA_State = "NSW";
			consignorAddress.OA_PostCode = "2780";

			Env.Registry.Rating.DefaultCFSAddressRoad = "2019";
			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LRO, "AU", "");
			var tariffLine = tariffEntry.AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			var tariffCalculator = tariffLine.Calculator;
			tariffCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 50, 4, QuantityUnit.KM);
			tariffCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50, 8);
			tariffCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100, 10);
			tariff.Factory.Save();

			var clientRate = Helper.NewClientRate(consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LRO, "AU", "");
			var rateLine = rateEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Road, consignor.PK, ZGuid.Empty, "AUSYD", "AUSYD", 1000);
			shipment.JS_UniqueConsignRef = "SH0001010";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 8800m,
							RevenueCalculationDescription = "ODOC: 1000 Kilogram(s) @ AUD 8.80/KG (for 86.712 KM)"
						}
				};

			AutorateAndAssert("", expected, shipment, consignor, autorateCosts: false);
		}

		public void TestChargeableUnitIsPkgSoBreakUnitCanBeSet()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var consignor = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(consignor);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", CombinedCalculator.Code, PkgUnit.Pallet);
			var minusItem = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10, 100, QuantityUnit.KG);
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 10, 80);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUBNE", "CNSHA", 45);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;

			Factory.Save();

			var packline1 = shipment.OuterPackLines[0];
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = PkgUnit.Pallet;
			packline1.JL_ActualWeight = 40m;
			packline1.JL_ActualWeightUQ = Weight.Kilograms;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 1;
			packline2.JL_F3_NKPackType = PkgUnit.Pallet;
			packline2.JL_ActualWeight = 5m;
			packline2.JL_ActualWeightUQ = Weight.Kilograms;

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 260m,
							RevenueCalculationDescription = "FRT: 1 Pallet(s) @ AUD 100.00/Pallet + 2 Pallet(s) @ AUD 80.00/Pallet"
						}
				};

			var message = "Each pallet should be charged by pack line average rate, so two pallets are more than 10kg, and one less";

			AutorateAndAssert(message, expected, shipment, consignor);

			minusItem.TM_BreakWeightVolume = "";
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 300m,
							RevenueCalculationDescription = "FRT: 3 Pallet(s) @ AUD 100.00/Pallet"
						}
				};

			AutorateAndAssert("When no break unit, chargeable unit is the break", expected, shipment, consignor);
		}

		#endregion

		#region SearchForRates

		[TestDate(2016, 10, 11)]
		public void TestSearchForRatesMethodDoesNotModifyChargesAndDoesntUpdateHost()
		{
			//Ensure that during SearchForRates method the following should be true:
			// 1) Existing charges are not cleared
			// 2) No new charges are added
			// 3) Host is not updated (testing by checking ContractNumbers on consol)
			// 4) No charges are modified
			// 5) RatesFound contain information about found rates
			// 6) Existing AutoRatingLog Notes are not cleared

			var creditor1 = Helper.NewOrgHeader();
			creditor1.OH_Code = "creditor1";

			var sCosting = Helper.NewCosting(null);
			var sEntry = sCosting.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX");
			sEntry.TI_ContractNumber = "1111";
			sEntry.RateLines.RemoveAndDeleteAll();
			var sLine = sEntry.AddRateLine("CAF", FlatCalculator.Code);
			sLine.GetCalculator<FlatCalculator>().BaseRate = 700m;

			var costing = Helper.NewCosting(creditor1);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX");
			entry.TI_ContractNumber = "1111";
			entry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			entry.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 1000;

			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();

			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_CreditorAddress = creditor1.MainAddress.PK;

			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryNum = "123456";
			number1.CE_EntryType = CusEntryNumber.EntryType.ActualArrivalResponseStatus;
			var number2 = consol.Numbers.AddNew();
			number2.CE_EntryNum = "567890";
			number2.CE_EntryType = CusEntryNumber.EntryType.DepartureReportStatus;

			consol.JK_CarrierContractNumber = "1111";

			var shipmentJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			shipmentJob.PlugInData = shipment;
			shipmentJob.JH_OA_AgentCollectAddr = creditor1.MainAddress.PK;

			var existingCharge = shipmentJob.Charges.AddNew();
			existingCharge.JR_AC = Helper.ChargeCodes["FRT"].PK;
			existingCharge.JR_OSCostAmt = 1148;
			existingCharge.JR_OSSellAmt = 1148;
			existingCharge.JR_SellRatingOverride = false;
			existingCharge.JR_CostRatingOverride = false;

			var note = shipment.Notes.AddNew();
			using (var hasChangesSuspender = note.SuspendSettingHasChanges())
			{
				note.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
				note.ST_GC_RelatedCompany = Env.CurrentCompany.PK;
				note.ST_NoteText = "TEST";
			}

			Factory.Save();

			using (RatingDataRegistry.Instance.ClientRateGoingToExpireNotification.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var rateSearchResult = new AutoRatingStarter(shipment, new LoggerDecorator()).SearchForRates(AutoRateOptions.AutorateCostsRevenue);

				AssertEquals("No charges should be deleted or added", 1, shipmentJob.Charges.Count);
				Assert("Autorating Log Note should not be cleared", shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)[0].ST_NoteText != ZString.Empty);

				var expectedResults = new List<RatesAdditionInfo>()
				{
					new RatesAdditionInfo
					{
						Target = "Shipment S00001000 (House Bill='S00001000')",
						CostSell = "cost",
						CreatedCharges = new List<ChargeInfo>().ToArray(),
						ModifiedCharges = new List<ChargeInfo>().ToArray(),
						DeletedChargesCount = 0,
						RatesFound = new List<ChargeInfo>
						{
							new ChargeInfo { ChargeCode = "CAF" },
							new ChargeInfo { ChargeCode = "FRT" }
						}.ToArray()
					},
					new RatesAdditionInfo
					{
						Target = "Shipment S00001000 (House Bill='S00001000')",
						CostSell = "revenue",
						CreatedCharges = new List<ChargeInfo>().ToArray(),
						ModifiedCharges = new List<ChargeInfo>().ToArray(),
						DeletedChargesCount = 0,
						RatesFound = new List<ChargeInfo>().ToArray()
					},
					new RatesAdditionInfo
					{
						Target = "Shipment S00001000 (House Bill='S00001000')",
						CostSell = "revenue",
						CreatedCharges = new List<ChargeInfo>().ToArray(),
						ModifiedCharges = new List<ChargeInfo>().ToArray(),
						DeletedChargesCount = 0,
						RatesFound = new List<ChargeInfo>().ToArray()
					}
				};

				AssertEquals("Results Count", 1, rateSearchResult.Results.Length);
				AssertEquals("Target", shipment.HumanReadableName, rateSearchResult.Results[0].Target);
				AssertMultilineASCIIEquals("", expectedResults.ToJSON(), rateSearchResult.Results[0].Results.ToJSON());
			}
		}

		#endregion

		#region TestCommentChargeCleanup

		public void TestCommentChargeIsNotDeletedWhenAutoratingCostAndRevenue()
		{
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var cmtChargeCode = Helper.ChargeCodes["TSTCMT"];
			cmtChargeCode.AC_ChargeType = ChargeType.Comment;
			cmtChargeCode.AC_AG_WIPAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			costingEntry.RateLines.RemoveAndDeleteAll();

			var frtCostLine = costingEntry.AddRateLine(frtChargeCode, FlatCalculator.Code);
			frtCostLine.GetCalculator<FlatCalculator>().BaseRate = 900m;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = rateEntry.AddRateLine(frtChargeCode, FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 1000m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = cmtChargeCode.PK;
			charge.JR_Desc = "This is a comment";
			charge.ApplyCustomQuickCalculator(shipment.RatingAdapter, 0m, 0m);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSSellAmt = 1000m,
					JR_OSCostAmt = 900m,
				},
				new AssertionCharge
				{
					ChargeCode = cmtChargeCode.AC_Code,
					JR_Desc = "This is a comment"
				}
			};

			AutorateAndAssert(expected, shipment, client, job: shipmentJob);
		}

		public void TestCommentChargeIsNotDuplicatedWhenRatingCostsAndRevenue()
		{
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var glh = Factory.NewWithValidTestData<AccGLHeader>();
			var cmtChargeCode = Helper.ChargeCodes["TSTCMT"];
			cmtChargeCode.AC_ChargeType = ChargeType.Comment;
			cmtChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			cmtChargeCode.AC_Desc = "Comment";
			cmtChargeCode.AC_AG_AccrualAccount = glh.PK;
			cmtChargeCode.AC_AG_WIPAccount = glh.PK;

			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			costingEntry.RateLines.RemoveAndDeleteAll();

			var cmtCostLine = costingEntry.AddRateLine(cmtChargeCode, FlatCalculator.Code);
			cmtCostLine.GetCalculator<FlatCalculator>().BaseRate = 0m;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var cmtRateLine = rateEntry.AddRateLine(cmtChargeCode, FlatCalculator.Code);
			cmtRateLine.GetCalculator<FlatCalculator>().BaseRate = 0m;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = cmtChargeCode.PK;
			charge.JR_Desc = "This is a comment";
			charge.ApplyCustomQuickCalculator(shipment.RatingAdapter, 0m, 0m);
			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
			charge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = cmtChargeCode.AC_Code,
					JR_Desc = "Comment",
					JR_OSSellAmt = 0m,
					JR_OSCostAmt = 0m
				}
			};

			AutorateAndAssert(expected, shipment, client, job: shipmentJob);
		}

		#endregion

		#region TestJobChargeAttributesDuplication

		public void TestJobChargeAttributesAreNotDuplicated()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OSEC");
			Helper.ChargeCodes["OSEC"].AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var clientRateEntry = Helper.NewClientRate(NewClient).AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AU", "US", "", "20GP");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var clientRate1 = clientRateEntry.AddRateLine("OSEC", UnitCalculator.Code, "CN");
			clientRate1.GetCalculator<UnitCalculator>().PerUnit = 80m;
			var clientRate2 = clientRateEntry.AddRateLine("OSEC", UnitCalculator.Code, "CN");
			clientRate2.GetCalculator<UnitCalculator>().PerUnit = 200m;
			clientRate2.TL_Condition = RateLineConditions.DangerousGoods;

			AddParityExchangeRate(clientRate1.Currency);

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_TransportMode = TransportModes.Sea;
			billOfLading.JS_PackingMode = ContainerModes.FCL;
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "USLAX";
			billOfLading.JS_INCO = PaymentType.Prepaid;
			billOfLading.ConsigneeDocumentaryAddress.OrganisationPK = NewClient.MainAddress.PK;
			billOfLading.OuterPackLines.AddNew().UNDGs.AddNew();

			var container = billOfLading.RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "OSEC", JR_OSSellAmt = 280m } };

			AutorateAndAssert("OSEC rate lines merged", expected, billOfLading, NewClient, autorateCosts: false);

			var shipmentJob = new Job.Loader(billOfLading).TryLoadOrCreate();
			var chargeAttributesCount = shipmentJob.Charges[0].JobChargeAttributes.Count;
			AssertEquals("Prerequisite: should be different attributes", 6, chargeAttributesCount);

			Factory.Save();

			AutorateAndAssert("Autorate revenue for same job again", expected, billOfLading, NewClient, job: shipmentJob, autorateCosts: false);
			chargeAttributesCount = shipmentJob.Charges[0].JobChargeAttributes.Count;

			AssertEquals("Should be still attributes", 6, chargeAttributesCount);
		}

		public void TestJobDeletedChargeLogged()
		{
			var client = Helper.NewOrgHeader();
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "AU", "US");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 80m;
			AddParityExchangeRate(rateLine.Currency);
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, client.PK, "AUSYD", "USLAX", 1m);
			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 80m } };

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;

			AutorateAndAssert(expected, shipment, client);
			AutorateAndAssert(expected, shipment, client, job: shipmentJob);
			var expectedLogLines = "Information: FRT Charge deleted by AutoRating.";
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		public void TestJobClearedRevenueChargeLogged()
		{
			var client = Helper.NewOrgHeader();
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "AU", "US");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 80m;
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, client.PK, "AUSYD", "USLAX", 1m);
			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 80m } };

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);
			AutorateAndAssert(expected, shipment, client, job: shipmentJob, autorateCosts: false);
			var expectedLogLines = "Information: FRT Charge Revenue reset by AutoRating.";
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		public void TestJobClearedCostingChargeLogged()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var chargeCode = Helper.ChargeCodes.New("FRTCMT", "Comment Charge", FlatCalculator.Code);
			chargeCode.AC_ChargeType = ChargeType.Comment;

			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costingEntry.RateLines.RemoveAndDeleteAll();

			var cmtCostLine = costingEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			cmtCostLine.GetCalculator<FlatCalculator>().BaseRate = 80m;
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;

			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.ApplyCustomQuickCalculator(shipment.RatingAdapter, 100m, 0m);
			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
			charge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			AssertEquals(100m, charge.JR_OSCostAmt);
			AssertEquals(false, charge.JR_CostRatingOverride);
			AssertEquals(false, charge.JR_SellRatingOverride);
			AssertEquals(true, charge.CostPaymentBases.Count > 0);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			AssertEquals("Pre-condition", JobChargeLookups.ReAutorateCharge, charge.JR_Calc_CostRatingBehavior);
			AssertEquals("Pre-condition", JobChargeLookups.ReAutorateCharge, charge.JR_Calc_SellRatingBehavior);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 80m
				}
			};

			AutorateAndAssert(expected, shipment, client, job: shipmentJob);

			var message = "Quick Calculator charge will be cleared after autorate since Cost and Sell rating behavior is set to REA";
			var expectedLogLines = "Information: FRTCMT Charge Cost reset by AutoRating.";
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		public void TestAutoRatingManuallyCreatedCommentCharges_DoesNotClearCharge()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var chargeCode = Helper.ChargeCodes.New("FRTCMT", "Comment Charge", FlatCalculator.Code);
			chargeCode.AC_ChargeType = ChargeType.Comment;

			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costingEntry.RateLines.RemoveAndDeleteAll();

			var cmtCostLine = costingEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			cmtCostLine.GetCalculator<FlatCalculator>().BaseRate = 200m;
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;

			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_LocalCostAmt = 50m;

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			AssertEquals("Pre-condition", JobChargeLookups.CreateNewCharge, charge.JR_Calc_CostRatingBehavior);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 50m
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 200m
				},
			};

			var message = "Create New Charge should work exactly the same as normal for Comment Charge Codes";
			AutorateAndAssert(message, expected, shipment, client, job: shipmentJob);
		}

		public void TestStrategyPreventsAddingRatesLogged()
		{
			var client = Helper.NewOrgHeader();
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.DST, TransportModes.Air, "AU", "US");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var rateEntry = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateEntry.GetCalculator<UnitCalculator>().PerUnit = 80m;
			AddParityExchangeRate(rateEntry.Currency);
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, client.PK, "AUSYD", "USLAX", 1m);
			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;

			var strategy = new MockInvoicingStrategy(shipment, shipmentJob);
			var starter = new AutoRatingStarter(shipment, new LoggerDecorator(), autoratingStrategy: strategy);
			starter.ExecuteAutorating(AutoRateOptions.AutorateRevenue.With(billingType: BillingType.Invoicing));

			var expectedLogLines = "Information: Rates were found but not added to results due to missing or closed jobs";
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		public void TestChargeCleanerDoesNotThrowExceptionWithChargesWithoutChargeCode()
		{
			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "AUSYD", "USLAX", 5000, 2);
			using (var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate())
			{
				var chargeA = shipmentJob.Charges.AddNew();
				chargeA.ApplyCustomQuickCalculator(shipment.RatingAdapter, 0m, 300m);
				chargeA.JR_CostRatingOverride = true;
				chargeA.JR_SellRatingOverride = false;

				var chargeB = shipmentJob.Charges.AddNew();
				chargeB.ApplyCustomQuickCalculator(shipment.RatingAdapter, 200m, 0m);
				chargeB.JR_CostRatingOverride = false;
				chargeB.JR_SellRatingOverride = true;

				var chargeC = shipmentJob.Charges.AddNew();
				chargeC.ApplyCustomQuickCalculator(shipment.RatingAdapter, 100m, 100m);
				chargeC.JR_CostRatingOverride = false;
				chargeC.JR_SellRatingOverride = false;

				var strategy = new MockInvoicingStrategy(shipment, shipmentJob);
				var logger = new LoggerDecorator();
				var strategiesRepository = new RatingStrategiesRepository(logger, strategy, shipment, BillingType.Invoicing, AdditionalJobsAction.NoAction, additionalJobsToDispose: null);

				AssertEquals(3, shipmentJob.Charges.Count);

				var deletedCharges = ChargeCleaner.CleanupExistingCharges(strategiesRepository, AutoRateOptions.AutorateCostsRevenue.With(billingType: BillingType.Invoicing));

				AssertContainsExactElementsInAnyOrder(new[] { chargeA.PK, chargeB.PK }, shipmentJob.Charges.Select(x => x.PK));
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"Information: Resetting previously auto-rated charges for Shipment EBM22Q33TU475BXH3P60",
					"Information: '' Charge deleted by AutoRating.",
					"Information: '' Charge Cost reset by AutoRating.",
					"Information: '' Charge Revenue reset by AutoRating."
				}, logger.DumpLog());
			}
		}

		class MockInvoicingStrategy : IAutoRatingStrategy
		{
			public MockInvoicingStrategy(IBusiness businessObject, Job job, RatingAdaptersProvider parentProvider = null)
			{
				this.businessObject = businessObject;
				this.job = job;
				ParentProvider = parentProvider;
			}

			readonly IBusiness businessObject;
			readonly Job job;

			public IBusiness HostBusinessEntity => businessObject;

			public Job Job => job;

			public AutoRatesAdditionResult AddAutoRates(IAutoRatingGUIInteractor interactor, AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ZString[] adapterIDs, IEnumerable<IAutoRating> adapters = null)
			{
				var realStrategy = new AutoRateInvoicingStrategy(HostBusinessEntity, Job);
				var result = realStrategy.AddAutoRates(interactor, autoRatingResults, costOrSell, adapterIDs, adapters);
				Job.JH_RatingHasBeenRun = true;
				Job.ReOpenJobStatus();

				return result;
			}

			public bool Supports(CostSell costOrSell) => true;

			public bool ShouldAddAutoRates => false;

			public RatingAdaptersProvider ParentProvider { get; }
		}

		#endregion

		#region CompanyTariffHasZeroMinimum

		public void TestCompanyTariffHasZeroMinimum()
		{
			var client = Helper.NewOrgHeader(1);

			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, TransportModes.Air, "", "AU");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var ddocChargeCode = Helper.ChargeCodes["DDOC"];
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode(ddocChargeCode);

			var tariffLine = tariffEntry.AddRateLine(ddocChargeCode, MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			tariffLine.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 0;
			tariffLine.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 0;
			tariffLine.Factory.Save();

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, TransportModes.Air, "", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(ddocChargeCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Minimum = 10;
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = .06;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "USLAX", "AUSYD", 1m);
			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = ddocChargeCode.AC_Code,
							JR_OSSellAmt = 10m,
							RevenueCalculationDescription = "DDOC: Minimum AUD 10.00",
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region Freight AutoRating Tests

		public void TestGenericCostingRating()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var genericCosting = Helper.NewCosting(null);
			var genericCostingRateEntry = genericCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination);
			genericCostingRateEntry.RateLines.RemoveAndDeleteAll();
			var genericCostingRateLine1 = genericCostingRateEntry.AddRateLine("FRT", UnitCalculator.Code, Weight.Kilograms);
			genericCostingRateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			var genericCostLine2 = genericCostingRateEntry.AddRateLine("BAF", UnitCalculator.Code, Weight.Kilograms);
			genericCostLine2.GetCalculator<UnitCalculator>().PerUnit = 3m;

			var specificCosting = Helper.NewCosting(carrier);
			var specificCostEntry = specificCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination);
			specificCostEntry.RateLines.RemoveAndDeleteAll();
			var specificCostLine = specificCostEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode, Weight.Kilograms);
			specificCostLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 30m;
			specificCostLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 30m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 300);
			var consol = CreateForwardingConsol(TransportModes.Air, origin, destination, carrier, shipment);
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 3900m,
							CostCalculationDescription = "FRT: 300 Kilogram(s) @ AUD 13.00/KG"
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSCostAmt = 900m,
							CostCalculationDescription = "BAF: 300 Kilogram(s) @ AUD 3.00/KG"
						}
				};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestTransportProvidersCostsVsGenericCosts()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";
			var client = Factory.NewWithValidTestData<OrgHeader>();

			SetupNewCostingWithSingleFCLRateEntry(null, origin, destination, 1000m);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, origin, destination, 300m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, TransportProvider2, shipment);
			consol.JK_OA_CreditorAddress = TransportProvider2.MainAddress.PK;
			AddContainersToForwardingConsol(consol, 2);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 2000m,
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 1000.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			SetupNewCostingWithSingleFCLRateEntry(TransportProvider2, origin, destination, 1200m);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 2400m,
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 1200.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestTransportProvidersSpecificCostsVsTransportProviderOwnCosts()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var transportProvider3 = Factory.NewWithValidTestData<OrgHeader>();

			var costing = SetupNewCostingWithSingleFCLRateEntry(TransportProvider1, origin, destination, 1000m);
			var rateEntry = costing.FCLRateEntriesForBinding[0];
			rateEntry.TI_OH_TransportProvider = transportProvider3.PK;
			SetupNewCostingWithSingleFCLRateEntry(TransportProvider2, origin, destination, 1200m);
			SetupNewCostingWithSingleFCLRateEntry(transportProvider3, origin, destination, 1400m);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, origin, destination, 300m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, transportProvider3, shipment);
			consol.Transports[0].CreditorPK = ZGuid.Empty;
			AddContainersToForwardingConsol(consol, 2);

			consol.CreditorPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 2000m,
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 1000.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			consol.CreditorPK = transportProvider3.PK;

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 2800,
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 1400.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestSetStandardFreightCost_StoreULDandLSECalcultitonLogs()
		{
			var standardCost = Helper.NewCosting(null);
			standardCost.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "US", "FRT", 20);
			standardCost.AddRateEntryWithFlatRateLine("AIR", "ULD", "AU", "US", "FRT", 30);

			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var carrierCost = Helper.NewCosting(carrier);
			carrierCost.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "US", "FRT", 1000);
			carrierCost.AddRateEntryWithFlatRateLine("AIR", "ULD", "AU", "US", "FRT", 2000);

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;
			consol.JK_UniqueConsignRef = "ConsolRef";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_TransportMode = TransportModes.Air;

			var t1 = consol.Transports[0];
			t1.JW_IsLinked = false;
			t1.JW_VoyageFlight = "VF01";
			t1.JW_RL_NKLoadPort = "AUSYD";
			t1.JW_RL_NKDiscPort = "FRCDG";
			t1.JW_ETD = ZDate.Today.AddDays(2);
			t1.JW_ETA = ZDate.Today.AddDays(4);
			t1.JW_TransportMode = TransportModes.Air;
			t1.JW_TransportType = TransportPlanningType.Flight1;
			t1.JW_CarrierBookingReference = "Route1";
			t1.CarrierPK = carrier.PK;

			var logger = new ElementaryLogger();

			var testAutoRater = new FreightAutoRater(new RatingContext(logger));
			var proxy = new AutoRatingProxy(consol.RatingAdapter);
			var bizoHost = proxy.StandardFreightCost.GetHost();

			var result = testAutoRater.AutoRate(proxy, CostSell.Cost).RateInfoCollection;
			AssertEquals("The best matching rate is for the carrier rate of $2000", 2000, (int)result[0].Amount);

			var calculationLog = CalculationLogsLoader.Load(bizoHost).Logs;
			AssertEquals("The standard rate, however, is $30", 30, (int)calculationLog[0].BaseRate);
			AssertEquals("ULD", calculationLog[0].RateMode);
			var logWrapperLog = testAutoRater.RatingContext.RateCalculationLogWrapper.Logs;
			AssertEquals("ULD", logWrapperLog[0].RateMode);

			consol.JK_ConsolMode = ContainerModes.Loose;
			shipment.JS_PackingMode = ContainerModes.Loose;
			proxy = new AutoRatingProxy(consol.RatingAdapter);
			result = testAutoRater.AutoRate(proxy, CostSell.Cost).RateInfoCollection;
			calculationLog = CalculationLogsLoader.Load(bizoHost).Logs;
			AssertEquals("The best matching rate is for the carrier rate of $1000", 1000, (int)result[0].Amount);
			AssertEquals("The standard rate, however, is $20", 20, (int)calculationLog[1].BaseRate);
			AssertEquals("ULD", calculationLog[0].RateMode);
			AssertEquals("LSE", calculationLog[1].RateMode);
			AssertEquals("ULD", logWrapperLog[0].RateMode);
			AssertEquals("LSE", logWrapperLog[1].RateMode);
		}

		public void TestSetStandardFreightCost()
		{
			var origin = "AUSYD";
			var destination1 = "USLAX";
			var destination2 = "USSFO";
			var destination3 = "GBLON";
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(null);

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination1);
			var rateLine1 = rateEntry1.RateLines[0];
			rateLine1.TL_RX_NKCurrency = CurrencyCodes.Australia;
			rateLine1.TL_RateCalculator = CombinedCalculator.Code;
			rateLine1.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus);
			var rateLineCalculator = rateLine1.GetCalculator<CombinedCalculator>();
			rateLineCalculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			rateLineCalculator["-45"] = (ZDecimal)10m;
			rateLineCalculator["+45"] = (ZDecimal)9m;
			rateLineCalculator["+100"] = (ZDecimal)8m;
			rateLineCalculator["+250"] = (ZDecimal)7m;

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination2);
			var rateLine2 = rateEntry2.RateLines[0];
			rateLine2.TL_RateCalculator = UnitCalculator.Code;
			rateEntry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 12m;
			rateEntry2.RateLines[0].TL_RX_NKCurrency = CurrencyCodes.Australia;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination1, 200m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1600m,
					CostCalculationDescription = "FRT: 200 Kilogram(s) @ AUD 8.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			shipment.JS_ActualWeight = 300m;
			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 2100m,
					CostCalculationDescription = "FRT: 300 Kilogram(s) @ AUD 7.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			shipment.JS_ActualWeight = 30m;
			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 300m,
					CostCalculationDescription = "FRT: 30 Kilogram(s) @ AUD 10.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			shipment.JS_RL_NKDestination = destination2;
			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 360m,
					CostCalculationDescription = "FRT: 30 Kilogram(s) @ AUD 12.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			shipment.JS_RL_NKDestination = destination3;
			expected = Array.Empty<AssertionCharge>();

			AutorateAndAssert(expected, shipment, client);

			shipment.JS_RL_NKDestination = destination2;
			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 360m,
					CostCalculationDescription = "FRT: 30 Kilogram(s) @ AUD 12.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestSetStandardFreightCost_CostWithDifferentCarriers()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var costing = SetupNewCostingWithSingleFCLRateEntry(null, origin, destination, 100m);
			var rateEntry1 = costing.FCLRateEntriesForBinding[0];
			rateEntry1.TI_OH_TransportProvider = TransportProvider1.PK;

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination, "STD", "20GP");
			rateEntry2.TI_OH_TransportProvider = TransportProvider2.PK;
			var rateLine2 = rateEntry2.RateLines[0];
			rateLine2.TL_RateCalculator = UnitCalculator.Code;
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 200m;
			rateLine2.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, origin, destination, 200m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, TransportProvider1, shipment);
			AddContainersToForwardingConsol(consol, 2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 200m,
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 100.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			consol.Transports[0].CarrierPK = TransportProvider2.PK;
			consol.JK_OA_ShippingLineAddress = TransportProvider2.MainAddress.PK;

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 400m,
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ AUD 200.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		Costing SetupNewCostingWithSingleFCLRateEntry(OrgHeader serviceProvider, string origin, string destination, decimal perUnit)
		{
			var costing = Helper.NewCosting(serviceProvider);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination, "STD", "20GP");
			rateEntry.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = perUnit;
			rateEntry.RateLines[0].TL_RX_NKCurrency = CurrencyCodes.Australia;

			Factory.Save();

			return costing;
		}

		void AddContainersToForwardingConsol(ForwardingConsol consol, int numberOfContainers, string containerCode = "20GP")
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerCode);

			for (int counter = 1; counter <= numberOfContainers; counter++)
			{
				var container = consol.Containers.AddNew();
				container.JC_RC = refContainer.PK;
				container.JC_ContainerNum = $"CONT0000{counter}";
			}
		}

		#endregion

		[TestDate(2023, 7, 27)]
		public void TestGivenExpiredApplyToLineAndValidRelatedRateLineExistInCompanyTariff_WhenAutoRating_ThenShouldFallbackToValidRelatedRateLine()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(2);
			consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			// Given: The accepted rate line is expired, and there is a valid related rate line in Company Tariff
			var tariff1 = Factory.New<CompanyTariff>();
			var expiredEntry = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var tariffRate1 = expiredEntry.AddRateLine("FRT", FlatCalculator.Code);
			expiredEntry.TI_RateStartDate = new ZDate(2020, 2, 10);
			expiredEntry.TI_RateEndDate = new ZDate(2020, 3, 10);
			tariffRate1.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var entry2 = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var rateLine = entry2.AddRateLine("FRT", FlatCalculator.Code);
			entry2.TI_RateStartDate = new ZDate(2020, 4, 10);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var clientRate = Helper.NewClientRate(consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			rateEntry.TI_RateStartDate = new ZDate(2020, 4, 10);
			var line = rateEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().ApplyToLine = tariffRate1.PK.ToString();

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 3000);
			shipment.JS_E_DEP = new ZDate(2023, 4, 10);
			Factory.Save();

			// When: Do auto-rating
			// Then: Should fallback to the valid related rate line
			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 110,
							}
					};

			AutorateAndAssert(expected, shipment, consignee);
		}

		[TestDate(2023, 7, 27)]
		public void TestGivenValidApplyToLineAndValidRelatedRateLineExistInCompanyTariff_WhenAutoRating_ThenShouldFallbackToValidApplyToLine()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(2);
			consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			// Given: The accepted rate line is valid, and there is another related rate line in Company Tariff
			var tariff1 = Factory.New<CompanyTariff>();
			var acceptedValidEntry = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var tariffRate1 = acceptedValidEntry.AddRateLine("FRT", FlatCalculator.Code);
			acceptedValidEntry.TI_RateStartDate = new ZDate(2020, 2, 10);
			acceptedValidEntry.TI_RateEndDate = new ZDate(2023, 7, 31);
			tariffRate1.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var entry2 = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var rateLine = entry2.AddRateLine("FRT", FlatCalculator.Code);
			entry2.TI_RateStartDate = new ZDate(2023, 8, 1);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var clientRate = Helper.NewClientRate(consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			rateEntry.TI_RateStartDate = new ZDate(2020, 4, 10);
			var line = rateEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().ApplyToLine = tariffRate1.PK.ToString();

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 3000);
			shipment.JS_E_DEP = new ZDate(2023, 7, 27);
			Factory.Save();

			// When: Do auto-rating
			// Then: Should fallback to the valid accepted rate line.
			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 220,
							}
					};

			AutorateAndAssert(expected, shipment, consignee);
		}

		[TestDate(2023, 7, 27)]
		public void TestGivenExpiredApplyToLineAndValidRelatedRateLineNotExistInCompanyTariff_WhenAutoRating_ThenShouldFallbackToExpiredApplyToLine()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(2);
			consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			// Given: The accepted rate line is expired, and there is no valid related rate line in Company Tariff
			var tariff1 = Factory.New<CompanyTariff>();
			var expiredEntry = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var tariffRate1 = expiredEntry.AddRateLine("FRT", FlatCalculator.Code);
			expiredEntry.TI_RateStartDate = new ZDate(2020, 2, 10);
			expiredEntry.TI_RateEndDate = new ZDate(2020, 3, 10);
			tariffRate1.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var clientRate = Helper.NewClientRate(consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			rateEntry.TI_RateStartDate = new ZDate(2020, 4, 10);
			var line = rateEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().ApplyToLine = tariffRate1.PK.ToString();

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 3000);
			shipment.JS_E_DEP = new ZDate(2023, 4, 10);
			Factory.Save();

			// When: Do auto-rating
			// Then: Should fallback to the expired accepted rate line.
			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 220,
							}
					};

			AutorateAndAssert(expected, shipment, consignee);
		}

		#region Client Rate uses Company Tariff Based Calculator with Unit Price Change

		[TestDate(2018, 01, 01)]
		public void TestClientRateUsesCTBWithPerUnit_CompanyTariffUsesCSTWithPerUnit()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(carrier);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.SEA, origin, destination);
			var costingLine = costingEntry.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			costingLine.GetCalculator<UnitCalculator>().PerUnit = 1925;

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.SEA, origin, destination);
			var tariffLine = tariffEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.CN, CurrencyCodes.Australia);
			tariffLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 175;

			var sellRate = Helper.NewClientRate(client);
			var sellEntry = sellRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.SEA, origin, destination);
			var sellLine = sellEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, QuantityUnit.CN, CurrencyCodes.Australia);
			sellLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 50;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA1234567";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2150m
				}
			};

			AutorateAndAssert("Should add all nested PerUnits", expected, shipment, client);

			sellLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 0;
			sellLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Minimum = 1000;

			tariffLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 0;
			tariffLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Minimum = 1500;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 2500m
				}
			};

			AutorateAndAssert("Should add all nested Minimum", expected, shipment, client);
		}

		#endregion

		#region TestDetentionGeneratesCorrectRevenue

		public void TestDetentionGeneratesCorrectRevenue()
		{
			var principal = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(1);
			var chargeCode = Helper.ChargeCodes.New("TST", "Test", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.SID, RateMode.SEA, "", "");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine(chargeCode.AC_Code, UnitCalculator.Code, QuantityUnit.DY, CurrencyCodes.EuropeanUnion);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 10;
			tariffEntry.TI_RateStartDate = new ZDate(2018, 1, 1);
			tariff.Factory.Save();

			var voyage = Factory.New<JobVoyage>();
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_TransportMode = TransportModes.Sea;
			billOfLading.JS_PackingMode = ContainerModes.FCL;

			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "FAKE4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var container = billOfLading.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;

			var detention = Factory.New<ContainerDetention>();
			detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			detention.NC_OH_Client = client.PK;
			detention.NC_OH_Principal = principal.PK;

			var movement = stock.Movements.AddNew();
			movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement.E9_MovementDate = new ZDate(2018, 1, 2);
			movement.E9_JV = voyage.PK;
			movement.E9_NC = detention.PK;
			movement.E9_DetentionDays = 16;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 160m,
					ChargeCode = "TST",
				}
			};

			AutorateAndAssert(expected, detention, client);
		}

		#endregion

		#region TestContractNumberFilter

		public void TestForwardingConsolidationCarrierContractNumberFiltering_NoContractNumberOnCosting()
		{
			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100);
			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expectedCosts = Array.Empty<AssertionCost>();

			consol.JK_CarrierContractNumber = "A12345";
			AutoCostAndAssert("FRT charge should NOT come through", null, expectedCosts, consol, false);
			AssertEquals("no change in contract number", "A12345", consol.JK_CarrierContractNumber);

			expectedCosts = new[] {
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 100m
				}
			};

			consol.JK_CarrierContractNumber = "";
			AutoCostAndAssert("FRT charge should come through", null, expectedCosts, consol, false);
			AssertEquals("no change in contract number", "", consol.JK_CarrierContractNumber);
		}

		public void TestForwardingConsolidationCarrierContractNumberFiltering()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100);
			rateEntry.TI_ContractNumber = "A12345";
			rateEntry.AddFlatRateLine("CAF", 150);

			var rateEntry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 200);
			rateEntry2.AddFlatRateLine("BAF", 250);

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expectedCosts = new[] {
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 100m
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_LocalCostAmount = 150m
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 200m
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_LocalCostAmount = 250m
				}
			};

			AutoCostAndAssert("No carrier contract on consol/ No Numbers with empty CON: charges from all rate entries should come through", null, expectedCosts, consol, false);

			consol.JK_CarrierContractNumber = "A12345";

			expectedCosts = new[] {
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 100m
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_LocalCostAmount = 150m
				}
			};

			AutoCostAndAssert("carrier contract on consol/ No Numbers with empty CON: charges from rate entry with contract should come through", null, expectedCosts, consol, false);

			consol.JK_CarrierContractNumber = "B12345"; // no rate entries with this contract no

			expectedCosts = Array.Empty<AssertionCost>();

			AutoCostAndAssert("carrier contract on consol/ No Numbers with empty CON: no rate entries with B12345 contract no, no charges should come through", null, expectedCosts, consol, false);

			consol.JK_CarrierContractNumber = "A12345";
			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			number1.CE_EntryNum = "";

			expectedCosts = new[] {
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 100m
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_LocalCostAmount = 150m
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_LocalCostAmount = 250m
				}
			};

			AutoCostAndAssert("carrier contract on consol/ Numbers with empty CON: charges from rate entry with contract/blank should come through. For same charge code, priority is givent to contract number", null, expectedCosts, consol, false);

			consol.JK_CarrierContractNumber = "";

			expectedCosts = new[] {
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 200m
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_LocalCostAmount = 250m
				}
			};

			AutoCostAndAssert("No carrier contract on consol/ Numbers with empty CON: charges from rate entry with blank should come through", null, expectedCosts, consol, false);
		}

		public void TestForwardingConsolidationCarrierContractNumberWhenMultiRouteAutoCostingIsEnabled()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var carrier_Road = Helper.NewOrgHeader();
			var carrier_Sea = Helper.NewOrgHeader();
			carrier_Road.OH_IsCreditor = true;
			carrier_Sea.OH_IsCreditor = true;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var charge = Helper.ChargeCodes.NewConsolChargeCode("CCOST", "Consol Cost", "", ChargeCodeGroupList.Codes.Freight);

			var costing_Road = Factory.New<Costing>();
			costing_Road.TH_OH = carrier_Road.PK;

			var costEntry_Road = costing_Road.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.ROA, "AUMEL", "AUSYD");
			costEntry_Road.TI_RX_NKCurrency = "AUD";
			costEntry_Road.TI_OH_TransportProvider = carrier_Road.PK;
			costEntry_Road.TI_ContractNumber = "CNT_SYD";
			costEntry_Road.RateLines.RemoveAndDeleteAll();
			costEntry_Road.AddRateLine(charge.AC_Code, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100m;

			var costing_Sea = Factory.New<Costing>();
			costing_Sea.TH_OH = carrier_Sea.PK;

			var costEntry_Sea = costing_Sea.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "SGSIN");
			costEntry_Sea.TI_RX_NKCurrency = "AUD";
			costEntry_Sea.TI_ViaLRC = "NZAKL";
			costEntry_Sea.TI_OH_TransportProvider = carrier_Sea.PK;
			costEntry_Sea.TI_ContractNumber = "CNT_SIN";
			costEntry_Sea.RateLines.RemoveAndDeleteAll();
			costEntry_Sea.AddRateLine(charge.AC_Code, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 250m;

			var costEntry_NoRouteSets = costing_Sea.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUMEL", "SGSIN");
			costEntry_NoRouteSets.TI_RX_NKCurrency = "AUD";
			costEntry_NoRouteSets.RateLines.RemoveAndDeleteAll();
			costEntry_NoRouteSets.AddRateLine(charge.AC_Code, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 350m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "CNT_SYD";

			consol.JK_UniqueConsignRef = "C00032423";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.SetDefaultShippingLineAddress(carrier_Sea);
			consol.JK_OA_CreditorAddress = carrier_Sea.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var t1 = consol.Transports[0];
			t1.JW_VoyageFlight = "123R";
			t1.JW_RL_NKLoadPort = "AUMEL";
			t1.JW_RL_NKDiscPort = "AUSYD";
			t1.JW_ETD = ZDateTime.Now;
			t1.JW_ETA = ZDateTime.Now.AddDays(1);
			t1.JW_IsLinked = ZBool.True;
			t1.JW_TransportMode = TransportModes.Road;
			t1.JW_TransportType = TransportPlanningType.PreCarriage;
			t1.CarrierPK = carrier_Road.PK;

			var t2 = consol.Transports.AddNew("AUSYD", "NZAKL");
			t2.JW_VoyageFlight = "123S";
			t2.JW_ETD = ZDateTime.Now.AddDays(2);
			t2.JW_ETA = ZDateTime.Now.AddDays(4);
			t2.JW_TransportMode = TransportModes.Sea;
			t2.JW_TransportType = TransportPlanningType.MainVessel;
			t2.CarrierPK = carrier_Sea.PK;

			var t3 = consol.Transports.AddNew("NZAKL", "SGSIN");
			t3.JW_VoyageFlight = "123S";
			t3.JW_ETD = ZDateTime.Now.AddDays(5);
			t3.JW_ETA = ZDateTime.Now.AddDays(7);
			t3.JW_TransportMode = TransportModes.Sea;
			t3.JW_TransportType = TransportPlanningType.Other;
			t3.CarrierPK = carrier_Sea.PK;

			t1.JW_CarrierBookingReference = "";
			t2.JW_CarrierBookingReference = "0001";
			t3.JW_CarrierBookingReference = "0001";

			var shipment = consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var line = shipment.OuterPackLines.AddNew();
			line.JL_JC = container.PK;

			Factory.Save();

			// Consol is from AUMEL via road to AUSYD, then via sea to NZAKL and then via sea to SGSIN
			// $100 road rate AUMEL -> AUSYD has contract number CNT_SYD
			// $250 sea rate 1 AUSYD via NZAKL to SGSIN has contract number CNT_SYD
			// $350 sea rate 2 AUMEL to SGSIN has no contract number

			var mockDialogService = new Mock<IDialogService>();
			mockDialogService
				.Setup(x => x.SelectSingleCarrierContractNumber(It.IsAny<IEnumerable<string>>()))
				.Returns(new SingleCarrierContractNumberSelectionResult("CNT_SYD"));

			consol.JK_CarrierContractNumber = "CNT_SYD";

			var routeSetNumberDescription = DescriptionHelpers.FormatWithTab("Route Set Number:");
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCost { ChargeCode = charge.AC_Code, E6_OSCostAmount = 100m, CostCalculationDescription = $"{routeSetNumberDescription}1" },
				};

				AutoCostAndAssert("Should filter costs based on contract number", null, expected, consol, false, deleteExistingCosts: true, dialogService: mockDialogService.Object);
				AssertEquals("Should not change contract number", "CNT_SYD", consol.JK_CarrierContractNumber);
			}

			consol.JK_CarrierContractNumber = "";
			using (FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCost { ChargeCode = charge.AC_Code, E6_OSCostAmount = 100m, CostCalculationDescription = $"{routeSetNumberDescription}1" },
				};
				AutoCostAndAssert("First route will set contract number, so second route rate is filtered away", null, expected, consol, false, deleteExistingCosts: true, dialogService: mockDialogService.Object);
				AssertEquals("Should populate contract number", "CNT_SYD", consol.JK_CarrierContractNumber);
			}
		}

		#endregion

		#region Zero charge + Non-declared Customs Charges Should NOT be created

		[TestDate(2018, 01, 01)]
		public void TestAutoRatingCustomsZeroChargesShouldNotBeCreatedIfLinesAreNotDeclared()
		{
			var client = Helper.NewOrgHeader();
			client.OH_RL_NKClosestPort = "USLAX";

			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_Code = "AMSSUB";
			newChargeCode.AC_Desc = "AMS submit";
			newChargeCode.AC_ChargeGroup = "DST";
			newChargeCode.AC_DepartmentFilterList = "ALL";
			newChargeCode.AC_ChargeType = "REV";
			newChargeCode.AC_ChargeOtherGroups = "PRC";
			newChargeCode.AC_RateCalculator = FlatPlusPerUnitCalculator.Code;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("DFSC", UnitCalculator.Code, QuantityUnit.N3);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var rateLine2 = rateEntry.AddRateLine("DADF", UnitCalculator.Code, QuantityUnit.AF);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 60m;

			var rateLine3 = rateEntry.AddRateLine("CCLR", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateLine4 = rateEntry.AddRateLine("DEHC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine4.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var rateLine5 = rateEntry.AddRateLine("AMSSUB", FlatPlusPerUnitCalculator.Code, QuantityUnit.AM);
			var calculator = rateLine5.GetCalculator<FlatPlusPerUnitCalculator>();
			calculator.BaseRate = 10m;
			calculator.PerUnit = 1m;

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
			Env.Registry.Rating.SetBrokerageRatedCodes("BRK,BON,CDS,FRT,DST");

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Air;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_OH_Importer = client.PK;

			Factory.Save();

			var declarationJob = new Job.Loader(declaration).TryCreateWithMutex();
			declarationJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CCLR",
					JR_OSSellAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "DEHC",
					JR_OSSellAmt = 0m
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutorateAndAssert("Should filtered zero charges", expected, declaration, client);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = (Customs.US.Business.JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			var amsLine = invoiceLine.AMSLines.AddNew();
			var atfLine = invoiceLine.ATFLines.AddNew();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CCLR",
					JR_OSSellAmt = 20m
				},
				new AssertionCharge
				{
					ChargeCode = "AMSSUB",
					JR_OSSellAmt = 11m
				},
				new AssertionCharge
				{
					ChargeCode = "DADF",
					JR_OSSellAmt = 60m
				},
				new AssertionCharge
				{
					ChargeCode = "DEHC",
					JR_OSSellAmt = 0m
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutorateAndAssert("Should filtered zero charges", expected, declaration, client);
		}

		#endregion

		public void TestAPInvoice_ImportCostFromAutoRatedConsolAndSaveIncomplete()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			creditor.OH_IsShippingProvider = true;

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var costing = Factory.New<Costing>();
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LRO, "AU", "");
			costEntry.TI_RX_NKCurrency = "AUD";
			costEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.M3);
			var calculator = rateLine.GetCalculator<UnitCalculator>();
			calculator.PerUnit = 200;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Road;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.IsDomesticFreight = true;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_F3_NKTotalCountPackType = "CNT";
			shipment.JS_ActualVolume = 1M;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_ActualWeight = 300M;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m
				}
			};

			AutoCostAndAssert("Consol should have cost", null, expected, consol, false);

			Factory.Save();

			var incompleteInvoice = Factory.New<APInvoice>();
			incompleteInvoice.AH_TransactionNum = "10001002";
			incompleteInvoice.AH_OH = creditor.PK;
			incompleteInvoice.AH_Ledger = LedgerTypes.IncompleteTransactions;

			Assert("New invoice should have no JobConsolCost", !(incompleteInvoice.ConsolCosting.ConsolCosts.Count > 0));

			JobConsolCost originatingCost = incompleteInvoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			InvoicingBaseConsolCostImporter importer = new InvoicingBaseConsolCostImporter(Factory, originatingCost, incompleteInvoice);

			var costToImport = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).FirstOrDefault();
			importer.ImportCostsIntoCosting(new BusinessObject[] { costToImport });

			Assert("Consol cost should remain", incompleteInvoice.ConsolCosting.ConsolCosts.Count == 1);

			var invoiceConsolCost = incompleteInvoice.ConsolCosting.ConsolCosts[0];
			Assert("Payment bases after importing", invoiceConsolCost.PaymentBases.Count == 1);

			invoiceConsolCost.ParentAPInvoice = incompleteInvoice;
			AssertNoExceptionThrown("Invoice should save as incomplete without error", () => incompleteInvoice.SaveAsIncomplete());

			Assert("JobConsolCost is not saved so its PaymentBases must be cleared while Factor.Save", invoiceConsolCost.PaymentBases.Count == 0);
		}

		public void TestPaymentBasisViewWithConsolHavingMultipleShipmentsAndContainers_ServiceCount()
		{
			TestPaymentBasisViewWithConsolHavingMultipleShipmentsAndContainers(QuantityUnit.SV);
		}

		public void TestPaymentBasisViewWithConsolHavingMultipleShipmentsAndContainers_DurationCount()
		{
			TestPaymentBasisViewWithConsolHavingMultipleShipmentsAndContainers(QuantityUnit.DY);
		}

		void TestPaymentBasisViewWithConsolHavingMultipleShipmentsAndContainers(string quantityUnit)
		{
			var contractor = Helper.CreateCreditor();
			contractor.OH_IsShippingProvider = true;
			contractor.OH_IsShippingLine = true;
			contractor.OH_IsMiscFreightServices = true;
			contractor.OH_IsFumigationContractor = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var mainAddressPK = contractor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = mainAddressPK;
			consol.JK_OA_ShippingLineAddress = mainAddressPK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_RC = GP20.PK;
			var serviceContainer20GP = container20GP.Services.AddNew();
			serviceContainer20GP.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceContainer20GP.ES_ServiceCount = 1;
			serviceContainer20GP.ES_Duration = new TimeSpan(1, 0, 0, 0);
			serviceContainer20GP.ES_References = "20202020";
			serviceContainer20GP.ES_OH_Contractor = contractor.PK;
			serviceContainer20GP.ES_Completed = ZDateTime.Now;
			container20GP.PackLines.Add(shipment.OuterPackLines.AddNew());

			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = GP40.PK;
			var serviceContainer40GP = container40GP.Services.AddNew();
			serviceContainer40GP.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceContainer40GP.ES_ServiceCount = 1;
			serviceContainer40GP.ES_Duration = new TimeSpan(1, 0, 0, 0);
			serviceContainer40GP.ES_References = "40404040";
			serviceContainer40GP.ES_OH_Contractor = contractor.PK;
			serviceContainer40GP.ES_Completed = ZDateTime.Now;
			container40GP.PackLines.Add(shipment.OuterPackLines.AddNew());

			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("OFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);

			var costing = Helper.NewCosting(contractor);
			var rateEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", "", GP20.RC_Code);
			var rateLine20GP = rateEntry20GP.AddRateLine(chargeCode, UnitCalculator.Code, quantityUnit, CurrencyCodes.Australia);
			rateLine20GP.GetCalculator<UnitCalculator>().PerUnit = 300;

			var rateEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", "", GP40.RC_Code);
			var rateLine40GP = rateEntry40GP.AddRateLine(chargeCode, UnitCalculator.Code, quantityUnit, CurrencyCodes.Australia);
			rateLine40GP.GetCalculator<UnitCalculator>().PerUnit = 600;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 300m,
					ChargeCode = "OFUM"
				},
				new AssertionCost
				{
					E6_OSCostAmount = 600m,
					ChargeCode = "OFUM"
				}
			};

			AutoCostAndAssert("AutoRating on consol should produce charges", null, expectedCharges, consol, false);

			var paymentBases = (shipment.Job as Job).Charges.Cast<Charge>().SelectMany(x => x.CostPaymentBases);

			var message = "Chargeable description should be the service reference per container";
			AssertNotNull(message, paymentBases.Single(x => x.PBS_ChargeableDescription == "20202020" && x.Amount == 300));
			AssertNotNull(message, paymentBases.Single(x => x.PBS_ChargeableDescription == "40404040" && x.Amount == 600));
		}

		public void TestPEBCalculatorWithBreaksAreBasedOnValuesRatherThanMeasures_Ticked()
		{
			TestPEBCalculatorWithBreaksAreBasedOnValuesRatherThanMeasures(true);
		}

		public void TestPEBCalculatorWithBreaksAreBasedOnValuesRatherThanMeasures_Unticked()
		{
			TestPEBCalculatorWithBreaksAreBasedOnValuesRatherThanMeasures(false);
		}

		void TestPEBCalculatorWithBreaksAreBasedOnValuesRatherThanMeasures(bool isTicked)
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUSYD", "HKHKG", 250m, 1m);

			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "HK");

			var frtChargeCode = Helper.ChargeCodes["FRT"];

			var rateLineFRT = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLineFRT.GetCalculator<UnitCalculator>().PerUnit = 0.1m;
			var rateLineFSC = rateEntry.AddRateLine("FSC", PercentageBreaksCalculator.Code, QuantityUnit.KG);
			var calculatorPEB = rateLineFSC.GetCalculator<PercentageBreaksCalculator>();

			calculatorPEB.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			calculatorPEB.AddRateLineItem(Calculator.Items.Operator.Minus, 1m, 0m, 0m);
			calculatorPEB.AddRateLineItem(Calculator.Items.Operator.Plus, 1m, 0m, 20m);
			calculatorPEB.AddRateLineItem(Calculator.Items.Operator.Plus, 100.1m, 0m, 25m);
			calculatorPEB.AddRateLineItem(Calculator.Items.Operator.Plus, 200.1m, 0m, 30m);
			calculatorPEB.AddRateLineItem(Calculator.Items.Operator.Plus, 300.1m, 0m, 35m);

			calculatorPEB.Bool4 = isTicked;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 25m
				},
				new AssertionCharge
				{
					ChargeCode = "FSC",
					JR_OSSellAmt = isTicked ? 20m : 30m // value 25 AUD vs 250 KG
				}
			};

			AutorateAndAssert("Charges created from autorating", expectedCharges, shipment, localClient, autorateCosts: false);
		}

		public void TestHRTCalculatorIsBasedOnValuesRatherThanMeasures()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "HK");

			var rateLine = rateEntry.AddRateLine("OBILL", HousebillReleaseTypeCalculator.Code);
			var calculator = rateLine.GetCalculator<HousebillReleaseTypeCalculator>();
			calculator.AddRateLineItem(ShipmentReleaseTypes.OriginalReqSurrender, 0m, 100m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.CashDoc, 0m, 20m);

			var shipment = CreateForwardingShipment(TransportModes.Sea, localClient.PK, consignee.PK, "AUSYD", "HKHKG", 250m, 1m);
			shipment.JS_ReleaseType = ShipmentReleaseTypes.OriginalReqSurrender; // => only OBO should go through

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OBILL",
					JR_OSSellAmt = 100m
				}
			};

			AutorateAndAssert("Charge created from autorating", expectedCharges, shipment, localClient, autorateCosts: false);
		}

		[ExpectNoExceptions]
		public void TestGetWorkflowItems()
		{
			var org = Helper.NewOrgHeader();

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = org.PK;
			clientRate.TH_RateType = "SAL";

			var clientRateProcessTask = Factory.New<ClientRateProcessTask>();
			clientRateProcessTask.P9_ParentID = clientRate.PK;
			clientRateProcessTask.P9_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var processTaskTemplate = Factory.New<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "SAL";
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_IsPartialTemplate = false;
			processTaskTemplate.P0_Name = "Client Rate Process Template Test";

			Factory.Save();

			AssertEquals("Workflow items from ClientRates", 1, clientRate.WorkflowItems.Count);
		}

		public void TestDepartmentChargesCanBeReAutorated()
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			query = query.AddToFilter(new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK));
			var frtChargeCode = Factory.LoadTop1<AccChargeCode>(query);

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			consignee.OH_IsDebtor = true;

			var clientRate = Helper.NewClientRate(consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "NZAKL", ZString.Empty, ZString.Empty);
			rateEntry.TI_RX_NKCurrency = "AUD";
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(frtChargeCode.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 5m;

			query = new ZQuery(GlbDepartmentSchema.GE_Code, "FES");
			var department = Factory.LoadTop1<GlbDepartment>(query);

			var deptCharge = department.DeptCharges.AddNew();
			deptCharge.GD_AC = frtChargeCode.PK;
			deptCharge.GD_GC = frtChargeCode.AC_GC;

			var shipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, consignee.PK, "AUSYD", "NZAKL", 300);
			Factory.Save();

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				shipmentJob.JH_OA_LocalChargesAddr = consignee.MainAddress.PK;
				shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;
				shipmentJob.JH_GE = department.PK;

				var expectedCharges = new[]
				{
					new AssertionCharge()
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 0m,
						JR_OSSellAmt = 0m
					}
				};
				AssertCharges("", expectedCharges, shipmentJob);

				expectedCharges = new[]
				{
					new AssertionCharge()
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 5m,
						JR_OSSellAmt = 5m
					}
				};
				AutorateAndAssert(expectedCharges, shipment, consignee, job: shipmentJob, autorateCosts: false);
			}
		}

		public void TestApportionedChargesCanBeMergedWhenReAutorating()
		{
			TransportProvider1.OH_IsCreditor = true;
			Factory.Save();

			var cost = Helper.NewCosting(null);
			cost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUBNE", "USLAX", "ODOC", 5m, "AUD");

			var odocQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC");
			odocQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var chargeCode = Factory.LoadTop1<AccChargeCode>(odocQuery);
			chargeCode.AC_IsGroupageCharge = true;
			chargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "USLAX", "ODOC", 15m, "AUD");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "USLAX", 300);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_INCO = IncoTerms.ExWorks;

			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "USLAX", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = "FCL";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT00001";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "ODOC",
					E6_OSCostAmount = 5m,
				}
			};

			AutoCostAndAssert("Autorating Cost Consol", null, expectedCosts, consol, false);

			Factory.Save();

			var expectedChargesOnShipmentJob = new[]
			{
				new AssertionCharge()
				{
					ChargeCode = "ODOC",
					JR_OSCostAmt = 5m,
					JR_OSSellAmt = 15m
				}
			};

			AutorateAndAssert("Autorating Cost and Revenue shipment. Charge should be merged, not duplicate", expectedChargesOnShipmentJob, shipment, Consignee);
		}

		public void TestGivenClientRateHasExcludeFromCompanyTariffCalculator_WhenAutoRating_JobCompanyTariffLevelOverrideShouldBeApplied()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(2);
			// Given: The rateline in Client Rate has ExcludeFromCompanyTariffCalculator and Job's CompanyTariffLevelOverride is not zero, and AllowOverrideCompanyTariffLevel is true.

			var tariff1 = Factory.New<CompanyTariff>();
			var rateEntry = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var tariffRate1 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			tariffRate1.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var clientRate = Helper.NewClientRate(consignee);
			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine("FRT", ExcludeCompanyTariffsCalculator.Code);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 3000);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_CompanyTariffLevelOverride = 1;
			Factory.Save();

			// When: Do auto-rating
			// Then: Should apply Company Tariff Level Override.
			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 200,
							}
					};
			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert("Given Client Rate Has ExcludeFromCompanyTariffCalculator, When AutoRating and allow override CT Level, Then Job's CompanyTariffLevelOverride should be applied", expected, shipment, consignee);
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert("Given Client Rate Has ExcludeFromCompanyTariffCalculator, When AutoRating and not allow override CT Level, Then Job's CompanyTariffLevelOverride should not be applied", Array.Empty<AssertionCharge>(), shipment, consignee);
			}
		}

		public void TestGivenClientRateAndCompanyTariffLevelOverrideHasSameCharges_WhenAutoRating_JobCompanyTariffLevelOverrideShouldBeApplied()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(2);
			// Given: The Consignee's Client Rate has the same charge as CompanyTariff's, and AllowOverrideCompanyTariffLevel is true.

			var tariff1 = Factory.New<CompanyTariff>();
			var rateEntry = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var tariffRate1 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			tariffRate1.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var clientRate = Helper.NewClientRate(consignee);
			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var tariffRate2 = rateEntry2.AddRateLine("FRT", FlatCalculator.Code);
			tariffRate2.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 3000);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_CompanyTariffLevelOverride = 1;
			Factory.Save();

			// When: Do auto-rating
			// Then: Should apply Company Tariff Level Override.
			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 200,
							}
					};

			var expectedClientRate = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 100,
							}
					};

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert("Given Client Rate and CompanyTariffLevelOverride has same Charges, When AutoRating and allow override CT Level, Then Job's CompanyTariffLevelOverride should be applied", expected, shipment, consignee);
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert("Given Client Rate and CompanyTariffLevelOverride has same Charges, When AutoRating and not allow override CT Level, Then Job's CompanyTariffLevelOverride should not be applied", expectedClientRate, shipment, consignee);
			}
		}

		public void TestGivenConsigneeCompanyTariffLevelIsDifferentFromJobCompanyTariffLevelOverride_WhenAutoRating_JobCompanyTariffLevelOverrideShouldBeApplied()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(2);
			// Given: The Consignee's Company Tariff Level is different with Job's Company Tariff Level Override, and AllowOverrideCompanyTariffLevel is true.
			consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var tariff1 = Factory.New<CompanyTariff>();
			var rateEntry = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE");
			var tariffRate1 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			tariffRate1.GetCalculator<FlatCalculator>().BaseRate = 200m;
			var tariff2 = Factory.New<CompanyTariff>();
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 10m);

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 3000);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_CompanyTariffLevelOverride = 2;
			Factory.Save();

			// When: Do auto-rating
			// Then: Should apply Company Tariff Level Override.
			var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 180,
								RevenueCalculationDescription = "Charge located in Company Tariff Level 2 (Overridden in job) with the following details:",
							}
					};

			var expectedBaseCompanyTariff = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 200,
							}
					};

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert("Given Consignee CT Level is different from Job's CompanyTariffLevelOverride, When AutoRating and allow override CT Level, Then Job's CompanyTariffLevelOverride should be applied", expected, shipment, consignee);
				AssertAutoratingAuditLogNoteContainsLines(shipment,
				"Log should contain expected lines",
				"Information: Company Tariff Level 2 Override in Shipment SHIPMENT1",
				"Information: RatingHeader Found Company Tariff Level 2 Entries: 1");
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert("Given Consignee CT Level is different from Job's CompanyTariffLevelOverride, When AutoRating and allow override CT Level, Then Job's CompanyTariffLevelOverride should not be applied", expectedBaseCompanyTariff, shipment, consignee);
			}
		}

		#region Load Standard Costing When There Is No Creditors

		public void TestLoadStandardCostingWhenThereIsNoCreditors()
		{
			var carrier = Helper.NewOrgHeader();
			var creditor = Helper.NewOrgHeader();
			var carrierCosting = Helper.NewCosting(carrier);
			var carrierEntry = carrierCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 100m, "AUD");
			var creditorCosting = Helper.NewCosting(creditor);
			var creditorEntry = creditorCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 200m, "AUD");
			var standardCosting = Helper.NewCosting(null);
			var standardEntry = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 300m, "AUD");
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 300m);
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_ConsolMode = ContainerModes.FCL;

			var autoRating = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);
			criteria.ValuesCanBeSet = true;
			criteria.Creditors = Creditors.New();

			var freightAutoRater = new FreightAutoRater(new RatingContext());
			var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
			var charges = result.RateInfoCollection;

			var serviceProviders = charges.Select(x => x.ProviderPK).ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { ZGuid.Empty }, serviceProviders);
		}

		#endregion

		#region Check PaymentTermOverride On Autorating Revenue For Shipment

		[TestDate(2020, 04, 26)]
		public void TestPaymentTermOverrideIsConsideredWhenAutoratingRevenueForShipment()
		{
			var client = Helper.NewOrgHeader(1);
			var consignee = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(client);
			var entry1 = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 100m);
			entry1.TI_PaymentTerm = "PPD";

			var entry2 = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 200m);
			entry2.TI_PaymentTerm = "CCX";

			var entry3 = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 300m);
			entry3.TI_PaymentTerm = "";

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "AUSYD", "USLAX", 5000m, 1.5m);
			shipment.JS_PaymentTermAutoratingOverride = "PPD";

			Factory.Save();

			var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 100m,
							RevenueCalculationDescription = "FRT: Base Rate AUD 100.00"
						},
					};
			AutorateAndAssert("Should match rate 1 and rate 3 and select rate 1 over 3 cause it is more specific", expected, shipment, consignee);

			shipment.JS_PaymentTermAutoratingOverride = "CCX";
			expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 200m,
							RevenueCalculationDescription = "FRT: Base Rate AUD 200.00"
						},
					};
			AutorateAndAssert("Should match rate 2 and rate 3 and select rate 2 over 3 cause it is more specific", expected, shipment, consignee);

			shipment.JS_PaymentTermAutoratingOverride = "";
			expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 300m,
							RevenueCalculationDescription = "FRT: Base Rate AUD 300.00"
						},
					};
			AutorateAndAssert("Should match and select only rate 3 cause shipment has not specified payment term override", expected, shipment, consignee);
		}

		#endregion

		#region Creditor Override Logic

		public void TestAutorateConsol_ChargeCreditorSetupMatchTheJob_DefaultCreditorIsOverriden()
		{
			var carrier = NewCarrierCreditor("CARRIER");
			var agency = NewAgencyCreditor(null);
			var creditor = NewCarrierCreditor("CREDITOR");
			var overridenCreditor = NewCarrierCreditor("CHCREDITOR");
			Factory.Save();
			AddCreditorOverrideToCharge(overridenCreditor.PK, "FRT", "ALL", "", "");
			AddCreditorOverrideToCharge(overridenCreditor.PK, "BAF", JobInvoicingConsumerTypes.ForwardingConsolCode, "IMP", "AIR");
			var consol = SetupConsolHavingCarrier(carrier, agency, creditor);
			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m, E6_OH_Creditor = overridenCreditor.PK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 300m, E6_OH_Creditor = overridenCreditor.PK },
			};

			var message = "Should have rate from carrier and charge creditor should be the charge code overriden creditor";
			AutoCostAndAssert(message, null, expectedResult, consol, false);
		}

		public void TestAutorateConsol_ChargeCreditorSetupDoesNotMatchTheJob_DefaultCreditorIsNotOverriden()
		{
			var carrier = NewCarrierCreditor("CARRIER");
			var agency = NewAgencyCreditor(null);
			var creditor = NewCarrierCreditor("CREDITOR");
			var overridenCreditor = NewCarrierCreditor("CHCREDITOR");

			Factory.Save();

			AddCreditorOverrideToCharge(overridenCreditor.PK, "FRT", JobInvoicingConsumerTypes.WorkItemCode, "", "");
			AddCreditorOverrideToCharge(overridenCreditor.PK, "BAF", JobInvoicingConsumerTypes.WorkItemCode, "", "");

			var consol = SetupConsolHavingCarrier(carrier, agency, creditor);
			Factory.Save();

			AssertEquals("We should set import creditor when we update creditor for import consol", creditor.PK, consol.CarrierImportCreditorAddress.OrganisationPK);

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m, E6_OH_Creditor = creditor.PK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 300m, E6_OH_Creditor = creditor.PK }
			};

			var message = "Should have rate from carrier and charge creditor should be the consol creditor when charge code creditor not matched";
			AutoCostAndAssert(message, null, expectedResult, consol, false);
		}

		public void TestShipmentWithTransportBooking_ChargeCreditorSetupMatchTheJob_DefaultCreditorIsOverriden()
		{
			var rate = Helper.NewClientRate(NewClient);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line.GetCalculator<UnitCalculator>().PerUnit = 5;

			var overridenCreditor = Factory.NewWithValidTestData<OrgHeader>();
			overridenCreditor.OH_IsCreditor = true;
			AddCreditorOverrideToCharge(overridenCreditor.PK, "FRT", "ALL", "", "");
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "AUBNE", "USLAX", 3000);
			var consol = shipment.Consols.AddNew();

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = false;

			var manager = new DtbDeliveryManager(Factory, shipment, DtbBookingDirection.PIC, false);

			try
			{
				manager.CreateTransportBooking();

				var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 15000m,
								JR_OSSellAmt = 15000m,
								CostAccountCode = overridenCreditor.OH_Code,
							}
					};

				AutorateAndAssert("Should assign the correct creditor", expected, shipment, NewClient);
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

		public void TestAutoratingShipment_ChargeCreditorSetupMatchTheJob_DefaultCreditorIsOverriden()
		{
			var odocCreditor = Helper.NewOrgHeader();
			var ocartCreditor = Helper.NewOrgHeader();
			AddCreditorOverrideToCharge(odocCreditor.PK, "ODOC", "ALL", "", "");
			AddCreditorOverrideToCharge(ocartCreditor.PK, "OCART", "ALL", "", "");
			var rate = Helper.NewClientRate(NewClient);
			rate.AddRateEntryWithFlatRateLine("ORG", "SEA", "AUSYD", "USLAX", "ODOC", 200);

			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithFlatRateLine("ORG", "SEA", "AUSYD", "USLAX", "OCART", 180);

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, NewClient.PK, "AUSYD", "USLAX", 50);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			var consol = shipment.Consols.AddNew();
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 200m,
					CostAccountCode = odocCreditor.OH_Code
				},
				new AssertionCharge
				{
					JR_OSCostAmt = 180m,
					CostAccountCode = ocartCreditor.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, NewClient);
		}

		public void TestAutorateCostAndRevenueFromShipment_ShouldTakeConsolidationChargeCreditorOverride_WhenChargeHasCreditorOverride()
		{
			TransportProvider1.OH_IsCreditor = true;
			Consignor.OH_IsDebtor = true;
			var chargeDefaultCreditorForConsolidation = Helper.NewOrgHeader();
			chargeDefaultCreditorForConsolidation.OH_IsCreditor = true;
			var chargeDefaultCreditorForShipment = Helper.NewOrgHeader();
			chargeDefaultCreditorForShipment.OH_IsCreditor = true;

			AddCreditorOverrideToCharge(chargeDefaultCreditorForConsolidation.PK, "BAF", JobInvoicingConsumerTypes.ForwardingConsolCode);
			AddCreditorOverrideToCharge(chargeDefaultCreditorForShipment.PK, "BAF", JobInvoicingConsumerTypes.ShipmentCode);
			var otherCreditor = Helper.CreateCreditor("CREDX");
			AddCreditorOverrideToCharge(otherCreditor.PK, "FRT", "ALL", "ALL", "ALL");

			Factory.Save();

			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 5;
			rateEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 4;

			var cost = Helper.NewCosting(TransportProvider1);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;
			costEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = .5;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1500);
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_UnitOfWeight = "KG";

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					CostCalculationDescription = "BAF: 1500 Kilogram(s) @ AUD 1.00/KG",
					ChargeCode = "BAF",
					E6_OSCostAmount = 1500,
					E6_OH_Creditor = chargeDefaultCreditorForConsolidation.PK,
				},
				new AssertionCost
				{
					CostCalculationDescription = "CAF: 1500 Kilogram(s) @ AUD 0.50/KG",
					ChargeCode = "CAF",
					E6_OSCostAmount = 750,
				}
			};

			AutoCostAndAssert("Should find the charges", null, expectedCosts, consol, autorateRevenue: false);

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				shipmentJob.JH_OA_LocalChargesAddr = Consignor.MainAddress.PK;
				Factory.Save();

				AssertEquals(2, shipmentJob.Charges.Count);
				AssertEquals(true, shipmentJob.Charges[0].JR_IsApportioned);
				AssertEquals("STP", shipmentJob.Charges[0].JR_Calc_CostRatingBehavior);
				AssertEquals(true, shipmentJob.Charges[1].JR_IsApportioned);
				AssertEquals("STP", shipmentJob.Charges[1].JR_Calc_CostRatingBehavior);
				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSSellAmt = 7500,
						JR_OSCostAmt = 1500,
						CostAccountCode = chargeDefaultCreditorForConsolidation.OH_Code,
					},
					new AssertionCharge
					{
						ChargeCode = "CAF",
						JR_OSSellAmt = 6000,
						JR_OSCostAmt = 750
					}
				};
				AutorateAndAssert("Should autorate correctly", expectedCharges, shipment, Consignor, job: shipmentJob);
			}
		}

		public void TestAutorateCostAndRevenueFromConsolidation_ShouldTakeConsolidationChargeCreditorOverride_WhenChargeHasCreditorOverride()
		{
			TransportProvider1.OH_IsCreditor = true;
			Consignor.OH_IsDebtor = true;
			var chargeDefaultCreditorForConsolidation = Helper.NewOrgHeader();
			chargeDefaultCreditorForConsolidation.OH_IsCreditor = true;
			var chargeDefaultCreditorForShipment = Helper.NewOrgHeader();
			chargeDefaultCreditorForShipment.OH_IsCreditor = true;

			AddCreditorOverrideToCharge(chargeDefaultCreditorForConsolidation.PK, "BAF", JobInvoicingConsumerTypes.ForwardingConsolCode);
			AddCreditorOverrideToCharge(chargeDefaultCreditorForShipment.PK, "BAF", JobInvoicingConsumerTypes.ShipmentCode);
			var otherCreditor = Helper.CreateCreditor("CREDX");
			AddCreditorOverrideToCharge(otherCreditor.PK, "BAF", "ALL", "ALL", "ALL");

			Factory.Save();

			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 5;
			rateEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 4;

			var cost = Helper.NewCosting(TransportProvider1);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;
			costEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = .5;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1500);
			shipment.JS_UnitOfWeight = "KG";

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					CostCalculationDescription = "BAF: 1500 Kilogram(s) @ AUD 1.00/KG",
					ChargeCode = "BAF",
					E6_OSCostAmount = 1500,
					E6_OH_Creditor = chargeDefaultCreditorForConsolidation.PK,
				},
				new AssertionCost
				{
					CostCalculationDescription = "CAF: 1500 Kilogram(s) @ AUD 0.50/KG",
					ChargeCode = "CAF",
					E6_OSCostAmount = 750,
				}
			};

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			shipmentJob.JH_OA_LocalChargesAddr = Consignor.MainAddress.PK;
			Factory.Save();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{
					shipment, new[]
						{
							new AssertionCharge
								{
									ChargeCode = "BAF",
									JR_OSSellAmt = 7500,
									JR_OSCostAmt = 1500,
									CostAccountCode = chargeDefaultCreditorForConsolidation.OH_Code,
								},
							new AssertionCharge
								{
									ChargeCode = "CAF",
									JR_OSSellAmt = 6000,
									JR_OSCostAmt = 750
								},
						}
				}
			};

			AutoCostAndAssert("Should find the charges", expectedCharges, expectedCosts, consol);
			Factory.Save();
			AssertEquals(2, shipmentJob.Charges.Count);
			AssertEquals(true, shipmentJob.Charges[0].JR_IsApportioned);
			AssertEquals("STP", shipmentJob.Charges[0].JR_Calc_CostRatingBehavior);
			AssertEquals(true, shipmentJob.Charges[1].JR_IsApportioned);
			AssertEquals("STP", shipmentJob.Charges[1].JR_Calc_CostRatingBehavior);
		}

		public void TestAutorateCostAndRevenueFromShipmentAndThenFromConsolidation_ShouldTakeConsolidationChargeCreditorOverride_WhenChargeHasCreditorOverride()
		{
			TransportProvider1.OH_IsCreditor = true;
			Consignor.OH_IsDebtor = true;

			var chargeDefaultCreditorForConsolidation = Helper.NewOrgHeader();
			chargeDefaultCreditorForConsolidation.OH_IsCreditor = true;
			var chargeDefaultCreditorForShipment = Helper.NewOrgHeader();
			chargeDefaultCreditorForShipment.OH_IsCreditor = true;

			AddCreditorOverrideToCharge(chargeDefaultCreditorForConsolidation.PK, "BAF", JobInvoicingConsumerTypes.ForwardingConsolCode);
			AddCreditorOverrideToCharge(chargeDefaultCreditorForShipment.PK, "BAF", JobInvoicingConsumerTypes.ShipmentCode);
			var otherCreditor = Helper.CreateCreditor("CREDX");
			AddCreditorOverrideToCharge(otherCreditor.PK, "FRT", "ALL", "ALL", "ALL");

			Factory.Save();

			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 5;
			rateEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 4;

			var cost = Helper.NewCosting(TransportProvider1);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;
			costEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = .5;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1500);
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_UnitOfWeight = "KG";

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					CostCalculationDescription = "BAF: 1500 Kilogram(s) @ AUD 1.00/KG",
					ChargeCode = "BAF",
					E6_OSCostAmount = 1500,
					E6_OH_Creditor = chargeDefaultCreditorForConsolidation.PK,
				},
				new AssertionCost
				{
					CostCalculationDescription = "CAF: 1500 Kilogram(s) @ AUD 0.50/KG",
					ChargeCode = "CAF",
					E6_OSCostAmount = 750,
				}
			};

			using (var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				shipmentJob.JH_OA_LocalChargesAddr = Consignor.MainAddress.PK;
				Factory.Save();
				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSSellAmt = 7500,
						JR_OSCostAmt = 1500,
						CostAccountCode = chargeDefaultCreditorForShipment.OH_Code,
					},
					new AssertionCharge
					{
						ChargeCode = "CAF",
						JR_OSSellAmt = 6000,
						JR_OSCostAmt = 750
					}
				};
				var expectedChargesInConsolidation = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment, new[]
							{
								new AssertionCharge
									{
										ChargeCode = "BAF",
										JR_OSSellAmt = 7500,
										JR_OSCostAmt = 1500,
										CostAccountCode = chargeDefaultCreditorForConsolidation.OH_Code,
									},
								new AssertionCharge
									{
										ChargeCode = "CAF",
										JR_OSSellAmt = 6000,
										JR_OSCostAmt = 750
									},
							}
					}
				};
				AutorateAndAssert("Should autorate correctly", expectedCharges, shipment, Consignor, job: shipmentJob);
				Factory.Save();
				AutoCostAndAssert("Should find the charges", expectedChargesInConsolidation, expectedCosts, consol);
				Factory.Save();
				AssertEquals(2, shipmentJob.Charges.Count);
				AssertEquals(true, shipmentJob.Charges[0].JR_IsApportioned);
				AssertEquals("STP", shipmentJob.Charges[0].JR_Calc_CostRatingBehavior);
				AssertEquals(true, shipmentJob.Charges[1].JR_IsApportioned);
				AssertEquals("STP", shipmentJob.Charges[1].JR_Calc_CostRatingBehavior);
			}
		}

		public void TestAutorateCostAndRevenueForShipmentWithTransportBooking_ShouldTakeShipmentCreditorOverride_WhenChargeHasCreditorOverrideForBoth()
		{
			var client = NewClient;
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line.GetCalculator<UnitCalculator>().PerUnit = 5;

			#region Creditor Override

			var tbmCreditorOverrider = Helper.CreateCreditor("CRED1");
			var shpCreditorOverrider = Helper.CreateCreditor("CRED2");
			var otherCreditor = Helper.CreateCreditor("CREDX");
			AddCreditorOverrideToCharge(tbmCreditorOverrider.PK, "FRT", JobInvoicingConsumerTypes.TransportBookingCode, "ALL", "ALL");
			AddCreditorOverrideToCharge(shpCreditorOverrider.PK, "FRT", JobInvoicingConsumerTypes.ShipmentCode, "ALL", "ALL");
			AddCreditorOverrideToCharge(otherCreditor.PK, "FRT", "ALL", "ALL", "ALL");

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "AUBNE", "USLAX", 3000);
			var consol = shipment.Consols.AddNew();
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
				Factory.Save();

				var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 15000m,
								JR_OSSellAmt = 15000m,
								CostAccountCode = shpCreditorOverrider.OH_Code,
							},
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSCostAmt = 2000m,
								JR_OSSellAmt = 0m,
								CostAccountCode = shpCreditorOverrider.OH_Code,
							}
					};

				AutorateAndAssert("Charges will have shipment creditor over transport booking creditor", expected, shipment, client, null, job);
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

		public void TestAutorateCostAndRevenueForShipmentWithTransportBooking_BookingChargesShouldUseBookingCreditorOverride()
		{
			var jobDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			jobDepartment.GE_Code = "ZZ1";

			var client = NewClient;

			var transportChargeCode = Helper.ChargeCodes.New("TBK1", "Transport Booking 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6000, Volume.CubicCentimeters, Weight.Kilograms),
				new ConversionFactor(194, Volume.CubicInches, Weight.Pounds)));

			FreightDataRegistry.Instance.InternationalChargeableFactorRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6000, Volume.CubicCentimeters, Weight.Kilograms),
				new ConversionFactor(194, Volume.CubicInches, Weight.Pounds)));

			var costProvider = Helper.CreateCreditor("COSTCRED");

			var rate = Helper.NewClientRate(client);
			var freightEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "");
			freightEntry.RateLines.RemoveAndDeleteAll();
			var line = freightEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line.GetCalculator<UnitCalculator>().PerUnit = 5;
			rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "", transportChargeCode.AC_Code, 100);

			var transportCreditor1 = Helper.CreateCreditor("TBKCRED1");
			var transportCreditor2 = Helper.CreateCreditor("TBKCRED2");
			var freightCreditor1 = Helper.CreateCreditor("FRTCRED1");
			var freightCreditor2 = Helper.CreateCreditor("FRTCRED2");
			var otherCreditor1 = Helper.CreateCreditor("CREDX1");
			var otherCreditor2 = Helper.CreateCreditor("CREDX2");
			AddCreditorOverrideToCharge(transportCreditor1.PK, "FRT", JobInvoicingConsumerTypes.TransportBookingCode, "ALL", "ALL");
			AddCreditorOverrideToCharge(freightCreditor1.PK, "FRT", JobInvoicingConsumerTypes.ShipmentCode, "ALL", "ALL");
			AddCreditorOverrideToCharge(otherCreditor1.PK, "FRT", "ALL", "ALL", "ALL");

			AddCreditorOverrideToCharge(transportCreditor2.PK, "TBK1", JobInvoicingConsumerTypes.TransportBookingCode, "ALL", "ALL", jobDepartment);
			AddCreditorOverrideToCharge(freightCreditor2.PK, "TBK1", JobInvoicingConsumerTypes.ShipmentCode, "ALL", "ALL");
			AddCreditorOverrideToCharge(otherCreditor2.PK, "TBK1", "ALL", "ALL", "ALL");

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "AUBNE", "USLAX", 3000);
			var consol = shipment.Consols.AddNew();

			CreateRateableTransportBookingForShipment(Factory, shipment, Consignor, client, costProvider);

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.JH_GE = jobDepartment.PK;
			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 15000m,
							JR_OSSellAmt = 15000m,
							CostAccountCode = freightCreditor1.OH_Code,
						},
					new AssertionCharge
						{
							ChargeCode = transportChargeCode.AC_Code,
							JR_OSCostAmt = 100m,
							JR_OSSellAmt = 100m,
							CostAccountCode = transportCreditor2.OH_Code,
						}
				};

			AutorateAndAssert("Should override cost creditor", expected, shipment, client, null, job);
		}

		static DtbBookingConsolidation CreateRateableTransportBookingForShipment(
			BusinessObjectFactory factory,
			ForwardingShipment shipment,
			OrgHeader consignor,
			OrgHeader consignee,
			OrgHeader costProvider)
		{
			var bookingConsolidation = factory.New<DtbBookingConsolidation>();
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
			fromInstruction1.Address.OrganisationPK = consignor.PK;

			var toInstruction1 = booking1.Instructions.AddNew();
			toInstruction1.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction1.KN_IsLooseRateable = true;
			toInstruction1.Address.OrganisationPK = consignee.PK;

			var commodity = factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box1 = CreatePackage(booking1, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet1 = CreatePackage(booking1, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton1 = CreatePackage(booking1, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction1, box1, pallet1, carton1);
			CreateInstructionPkgDivots(toInstruction1, box1, pallet1, carton1);
			return bookingConsolidation;
		}

		void AddCreditorOverrideToCharge(ZGuid creditor, string chargeCode, string jobType, string direction = "ALL", string transportMode = "ALL", GlbDepartment department = null)
		{
			var charge = Helper.ChargeCodes[chargeCode];
			var creditorOverride = charge.CreditorOverrides.AddNew();
			creditorOverride.ACC_JobType = jobType;
			creditorOverride.ACC_Direction = direction;
			creditorOverride.ACC_TransportMode = transportMode;
			creditorOverride.ACC_OH_Creditor = creditor;
			creditorOverride.ACC_PaymentTerm = "ALL";
			if (department != null)
			{
				creditorOverride.ACC_GE_Department = department.PK;
			}
		}

		ForwardingConsol SetupConsolHavingCarrier(OrgHeader carrier, OrgHeader agency, OrgHeader creditor)
		{
			var costingCarrier = Helper.NewCosting(carrier);
			costingCarrier.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "FRT", 100m);

			var costingAgency = Helper.NewCosting(agency);
			costingAgency.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "FRT", 200m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);

			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.SetDefaultShippingLineAddress(carrier);

			if (creditor != null)
			{
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

				var costingCreditor = Helper.NewCosting(creditor);
				costingCreditor.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "BAF", 300m);
			}

			Factory.Save();

			return consol;
		}

		#endregion

		#region Container Quality

		public void TestAccessingToContainerQualityViaRatingCriteria()
		{
			var consol = CreateConsol();

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = refContainer.PK;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_ContainerCount = 1;
			container1.JC_ContainerQuality = ZString.Empty;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = refContainer.PK;
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_ContainerCount = 1;
			container2.JC_ContainerQuality = "GOH";

			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var containers = criteria.RateableMeasures.GetPartList(Rating.Integration.MeasureType.ContainerCount)?.OfType<IRateableContainer>();

			AssertEquals(2, containers.Count());
			AssertContainsExactElementsInAnyOrder(new List<string> { "", "GOH" }, containers.Select(x => x.ContainerQuality));
		}

		#endregion

		#region quick calculator charge testing

		public void TestQuickCalculatorCharge_WhenAutorated_QuickCalcChargeShouldNotBeDeleted()
		{
			var rate = Helper.NewClientRate(NewClient);
			var entry = rate.AddRateEntry("ORG", "SEA", "AUSYD", "USLAX");
			var rateLine = entry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 30m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "AUSYD", "USLAX", 1000m);

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			testJob.PlugInData = shipment;
			testJob.LocalChargesPK = NewClient.PK;

			//Manally entered charge
			var charge1 = testJob.Charges.AddNew();
			charge1.JR_AC = Helper.ChargeCodes["BAF"].PK;
			charge1.JR_OSCostAmt = 40m;
			charge1.JR_OSSellAmt = 50m;

			//Charges enetered using Quick Calculator
			var charge2 = testJob.Charges.AddNew();
			charge2.JR_AC = Helper.ChargeCodes["FRT"].PK;
			charge2.ApplyCustomQuickCalculator(shipment.RatingAdapter, costAmount: 10m, sellAmount: 20m);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt =  50m,
					JR_OSCostAmt =  40m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt =  20m,
					JR_OSCostAmt =  10m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt =  30m,
					JR_OSCostAmt =  30m
				}
			};
			AutorateAndAssert(expected, shipment, NewClient, job: testJob);
			AssertEquals(3, testJob.Charges.Count);
		}

		#endregion

		#region Sliding Calculator Description

		public void TestCombinedCalculatorDescription_WhenAutorate_ThenCalculatorDescriptionAttributeShouldBeSet()
		{
			var consignee = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", CombinedCalculator.Code, Weight.Kilograms);
			var combinedCalculator = line.GetCalculator<CombinedCalculator>();
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 10m, 10m);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 9m, 20m);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 8m, 40m);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 200m, 7m, 50m);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 10m, 0m);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.MIN, 100m, 0m, 0m);
			combinedCalculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 3000m, 0m);
			combinedCalculator.IsAccumulated = true;
			combinedCalculator.UseHigherChargeableLowerRateRule = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "AUSYD", "USLAX", 120, 1);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1528.34m,
				}
			};

			AutorateAndAssert(expected, shipment, client);

			var chargeAttribute = ((Job)shipment.Job).Charges[0];

			var expectedDescription = @"BAS. Rate AUD 10 +
Unit portion <= 45 Kilogram(s) Base AUD 10 + AUD 10/KG
Unit portion > 45 Kilogram(s) Base AUD 20 + AUD 9/KG
Unit portion > 100 Kilogram(s) Base AUD 40 + AUD 8/KG
Unit portion > 200 Kilogram(s) Base AUD 50 + AUD 7/KG
MIN. 100 Kilogram(s), MAX. Rate AUD 3000, Higher Break Lower Rate is applied.";
			AssertContainsExactElementsInAnyOrder(new[] { expectedDescription }, chargeAttribute.JobChargeAttrib_AllCalculatorDescriptions);
		}

		public void TestCombinedCalculatorDescription_WhenAutorate_ThenMultipleCalculatorDescriptionAttributeCouldBeCombined()
		{
			var consignee = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line1 = entry.AddRateLine("FRT", CombinedCalculator.Code, Weight.Kilograms);
			var combinedCalculator1 = line1.GetCalculator<CombinedCalculator>();
			combinedCalculator1.AddRateLineItem(Calculator.Items.Operator.Minus, 60m, 15m, 11m);
			combinedCalculator1.AddRateLineItem(Calculator.Items.Operator.Plus, 60m, 14m, 12m);
			combinedCalculator1.AddRateLineItem(Calculator.Items.Operator.Plus, 80m, 13m, 13m);
			combinedCalculator1.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 12m, 14m);
			combinedCalculator1.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 15m, 0m);
			combinedCalculator1.AddRateLineItem(Calculator.Items.Operator.MIN, 0, 20m, 0m);
			combinedCalculator1.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 2000m, 0m);

			var line2 = entry.AddRateLine("FRT", CombinedCalculator.Code, Weight.Kilograms);
			var combinedCalculator2 = line2.GetCalculator<CombinedCalculator>();
			combinedCalculator2.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 10m, 10m);
			combinedCalculator2.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 9m, 20m);
			combinedCalculator2.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 8m, 40m);
			combinedCalculator2.AddRateLineItem(Calculator.Items.Operator.Plus, 200m, 7m, 50m);
			combinedCalculator2.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 10m, 0m);
			combinedCalculator2.AddRateLineItem(Calculator.Items.Operator.MIN, 100m, 0m, 0m);
			combinedCalculator2.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 3000m, 0m);
			combinedCalculator2.IsAccumulated = true;
			combinedCalculator2.UseHigherChargeableLowerRateRule = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, consignee.PK, "AUSYD", "USLAX", 120, 1);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3528.34m,
				}
			};

			AutorateAndAssert(expected, shipment, client);

			var chargeAttribute = ((Job)shipment.Job).Charges[0];

			var expectedDescription1 = @"BAS. Rate AUD 15 +
Unit total < 60 Kilogram(s) Base AUD 11 + AUD 15/KG
Unit total >= 60 Kilogram(s) Base AUD 12 + AUD 14/KG
Unit total >= 80 Kilogram(s) Base AUD 13 + AUD 13/KG
Unit total >= 100 Kilogram(s) Base AUD 14 + AUD 12/KG
MIN. Rate AUD 20, MAX. Rate AUD 2000.";

			var expectedDescription2 = @"BAS. Rate AUD 10 +
Unit portion <= 45 Kilogram(s) Base AUD 10 + AUD 10/KG
Unit portion > 45 Kilogram(s) Base AUD 20 + AUD 9/KG
Unit portion > 100 Kilogram(s) Base AUD 40 + AUD 8/KG
Unit portion > 200 Kilogram(s) Base AUD 50 + AUD 7/KG
MIN. 100 Kilogram(s), MAX. Rate AUD 3000, Higher Break Lower Rate is applied.";
			AssertContainsExactElementsInAnyOrder(new[] { expectedDescription1, expectedDescription2 }, chargeAttribute.JobChargeAttrib_AllCalculatorDescriptions);

			var line3 = entry.AddRateLine("FRT", CombinedCalculator.Code, Weight.Kilograms);
			var combinedCalculator3 = line3.GetCalculator<CombinedCalculator>();
			combinedCalculator3.AddRateLineItem(Calculator.Items.Operator.Minus, 25m, 20m, 50m);
			combinedCalculator3.AddRateLineItem(Calculator.Items.Operator.Plus, 25m, 19m, 60m);
			combinedCalculator3.AddRateLineItem(Calculator.Items.Operator.Plus, 65m, 18m, 70m);
			combinedCalculator3.AddRateLineItem(Calculator.Items.Operator.Plus, 95m, 17m, 80m);
			combinedCalculator3.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 40m, 0m);
			combinedCalculator3.AddRateLineItem(Calculator.Items.Operator.MIN, 200m, 0m, 0m);
			combinedCalculator3.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 6000m, 0m);
			combinedCalculator3.IsAccumulated = true;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 7233.34m,
				}
			};

			AutorateAndAssert(expected, shipment, client);

			chargeAttribute = ((Job)shipment.Job).Charges[0];

			var expectedDescription3 = @"BAS. Rate AUD 40 +
Unit portion <= 25 Kilogram(s) Base AUD 50 + AUD 20/KG
Unit portion > 25 Kilogram(s) Base AUD 60 + AUD 19/KG
Unit portion > 65 Kilogram(s) Base AUD 70 + AUD 18/KG
Unit portion > 95 Kilogram(s) Base AUD 80 + AUD 17/KG
MIN. 200 Kilogram(s), MAX. Rate AUD 6000.";
			AssertContainsExactElementsInAnyOrder(new[] { expectedDescription1, expectedDescription2, expectedDescription3 }, chargeAttribute.JobChargeAttrib_AllCalculatorDescriptions);
		}

		#endregion

		#region Implementation

		StmNote[] LoadRevenueLogs(ZGuid jobPK)
		{
			const string sql = @"ST_Table = 'JobCharge'
AND ST_ParentID IN
(
	SELECT JR_PK
	FROM dbo.JobCharge
	JOIN dbo.JobHeader on JR_JH = '{0}'
	WHERE ST_Description = 'AutoRating Calculation Log Revenue'
)
";
			var sqlFormatted = string.Format(sql, jobPK);
			var query = new ZDBOnlyQuery(typeof(StmNote)).AddFilterAndZSQLParameterCollection(sqlFormatted, new ZSqlParameterCollection());
			return Factory.Load<StmNote>(query);
		}

		#region Merge Preexisting Charges

		AssertionCharge MakeExpectedCharge(ZString chargeCode, ZDecimal expectedCost, ZDecimal expectedSell, bool costOverriden = false, bool sellOverriden = false)
		{
			AssertionCharge expectedCharge = new AssertionCharge()
			{
				ChargeCode = chargeCode,
				JR_LocalCostAmt = expectedCost,
				JR_LocalSellAmt = expectedSell,
				JR_SellRatingOverride = sellOverriden,
				JR_CostRatingOverride = costOverriden,
			};
			return expectedCharge;
		}

		AssertionCharge MakeExpectedCharge(RateLine costLine, RateLine sellLine)
		{
			AssertionCharge expectedCharge = new AssertionCharge()
			{
				ChargeCode = costLine.ChargeCode.AC_Code,
				JR_LocalCostAmt = costLine.GetCalculator<FlatCalculator>().BaseRate,
				JR_LocalSellAmt = sellLine.GetCalculator<FlatCalculator>().BaseRate,
			};
			return expectedCharge;
		}

		void SetExistingChargeValues(Charge existingCharge, ZDecimal cost, ZDecimal sell, bool costOverriden, bool sellOverriden, IAutoRating ratingAdapter)
		{
			existingCharge.JR_LocalCostAmt = cost;
			existingCharge.JR_LocalSellAmt = sell;

			if (!costOverriden || !sellOverriden)
			{
				existingCharge.ApplyCustomQuickCalculator(ratingAdapter, cost, sell);
			}

			existingCharge.JR_CostRatingOverride = costOverriden;
			existingCharge.JR_SellRatingOverride = sellOverriden;

			if (costOverriden)
			{
				foreach (var costPaymentBasis in existingCharge.CostPaymentBases)
				{
					costPaymentBasis.Delete();
				}
			}

			if (sellOverriden)
			{
				foreach (var sellPaymentBasis in existingCharge.SellPaymentBases)
				{
					sellPaymentBasis.Delete();
				}
			}
		}

		void CopyCharge(Job job, Charge source)
		{
			Charge destination = job.Charges.AddNew();
			destination.JR_AC = source.JR_AC;
			destination.JR_Desc = source.JR_Desc;
			destination.JR_OH_CostAccount = source.JR_OH_CostAccount;
			destination.JR_OH_SellAccount = source.JR_OH_SellAccount;
			destination.JR_LocalCostAmt = source.JR_LocalCostAmt;
			destination.JR_LocalSellAmt = source.JR_LocalSellAmt;
			destination.JR_SellRatingOverride = source.JR_SellRatingOverride;
			destination.JR_CostRatingOverride = source.JR_CostRatingOverride;
			destination.JR_AT_CostGSTRate = source.JR_AT_CostGSTRate;
			destination.JR_AT_SellGSTRate = source.JR_AT_SellGSTRate;
			CopyPaymentBases(destination, source.CostPaymentBases);
			CopyPaymentBases(destination, source.SellPaymentBases);
		}

		void CopyPaymentBases(Charge destination, IEnumerable<JobPaymentBasis> paymentBases)
		{
			foreach (var paymentBasis in paymentBases)
			{
				var clonedCostPaymentBasis = (JobPaymentBasis)paymentBasis.Clone();
				destination.PaymentBases.Add(clonedCostPaymentBasis);
			}
		}

		void ResetJobCharges(Job shipmentJob, params Charge[] charges)
		{
			if (charges.Length > 0)
			{
				shipmentJob.Charges.RemoveAll();
				foreach (var c in charges)
				{
					CopyCharge(shipmentJob, c);
				}
			}
		}

		#endregion

		#endregion
	}
}
