using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class RateLinesTest : RatingTestCase
	{
		public void TestRateCalculatorType()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "AU", "CN");
			var rateLine = rateEntry.AddRateLine("DDOC", FlatCalculator.Code);
			AssertEquals(CalculatorType.Flat, rateLine.RateCalculatorType);
			AssertType<FlatCalculator>(rateLine.Calculator);

			rateLine.RateCalculatorType = CalculatorType.Unit;
			AssertEquals("setting RateCalculatorType updates TL_RateCalculator", UnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertType<UnitCalculator>("setting RateCalculatorType resets Calculator", rateLine.Calculator);

			rateLine.TL_RateCalculator = FlatCalculator.Code;
			AssertEquals("setting TL_RateCalculator updates RateCalculatorType", CalculatorType.Flat, rateLine.RateCalculatorType);
		}

		public void TestUnitFactor()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "AU", "CN");
			var rateLine = rateEntry.AddRateLine("DDOC", FlatCalculator.Code);

			rateLine.TL_UnitFactor = UnitFactorList.Codes.BCN;
			AssertNoExceptionThrown(() => { Factory.Save(); });

			rateLine.TL_UnitFactor = "PW";
			AssertNoExceptionThrown(() => { Factory.Save(); });
		}

		public void TestUseOnlyActualWeightMeasureReadOnly_BasedOnUnitFactor_WarehouseProductLine()
			=> AssertUseOnlyActualWeightMeasureReadOnly_BasedOnUnitFactor
			(
				unitFactor: UnitFactorList.Codes.ProductLine,
				actualPercentage: 50,
				expectedActualPercentageReadonly: false,
				expectedActualPercentage: 50,
				expectedUseOnlyActualWeightMeasureReadOnly: false,
				expectedUseOnlyActualWeightMeasure: false
			);

		public void TestUseOnlyActualWeightMeasureReadOnly_BasedOnUnitFactor_PacksWeight()
			=> AssertUseOnlyActualWeightMeasureReadOnly_BasedOnUnitFactor
			(
				unitFactor: UnitFactorList.Codes.PacksWeight,
				actualPercentage: 50,
				expectedActualPercentageReadonly: true,
				expectedActualPercentage: 100,
				expectedUseOnlyActualWeightMeasureReadOnly: true,
				expectedUseOnlyActualWeightMeasure: true
			);

		public void TestUseOnlyActualWeightMeasureReadOnly_BasedOnUnitFactor_WarehousePackageLine()
			=> AssertUseOnlyActualWeightMeasureReadOnly_BasedOnUnitFactor
			(
				unitFactor: UnitFactorList.Codes.PackageLine,
				actualPercentage: 50,
				expectedActualPercentageReadonly: false,
				expectedActualPercentage: 50,
				expectedUseOnlyActualWeightMeasureReadOnly: false,
				expectedUseOnlyActualWeightMeasure: false
			);

		void AssertUseOnlyActualWeightMeasureReadOnly_BasedOnUnitFactor(string unitFactor, byte actualPercentage, bool expectedActualPercentageReadonly, byte expectedActualPercentage, bool expectedUseOnlyActualWeightMeasureReadOnly, bool expectedUseOnlyActualWeightMeasure)
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, "", "");
			var rateLine = rateEntry.AddRateLine("ODOC", CombinedCalculator.Code, Weight.Pounds);

			rateLine.UseOnlyActualWeightMeasure = true;
			CombineAssertions($"WHEN UnitFactor is not {unitFactor} THEN should able to edit ActualWeightVolume and ActualPercentage", () =>
			{
				AssertNotEquals("Precondition: UnitFactor", UnitFactorList.Codes.PacksWeight, rateLine.TL_UnitFactor);
				AssertEquals("Actual-Percentage readonly", false, rateLine.TL_ActualPercentageInfo.ReadOnly);
				AssertEquals("Actual-Weight/Volume readonly", false, rateLine.UseOnlyActualWeightMeasureInfo.ReadOnly);
			});

			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.TL_ActualPercentage = actualPercentage;
			rateLine.TL_UnitFactor = unitFactor;
			CombineAssertions($"WHEN UnitFactor is {unitFactor} THEN should not able to edit ActualWeightVolume and ActualPercentage", () =>
			{
				AssertEquals("Actual-Percentage readonly", expectedActualPercentageReadonly, rateLine.TL_ActualPercentageInfo.ReadOnly);
				AssertEquals("Actual-Percentage value", expectedActualPercentage, rateLine.TL_ActualPercentage);
				AssertEquals("Actual-Weight/Volume readonly", expectedUseOnlyActualWeightMeasureReadOnly, rateLine.UseOnlyActualWeightMeasureInfo.ReadOnly);
				AssertEquals("Actual-Weight/Volume value", expectedUseOnlyActualWeightMeasure, rateLine.UseOnlyActualWeightMeasure);
			});
		}

		#region Get Cartage Location for Zones

		public void TestGetCartageLocationForZones_Destination()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "AU", "CN");
			var rateLine = rateEntry.AddRateLine("DDOC", FlatCalculator.Code);

			var expectedCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.China);
			AssertEquals("DST lines should get location from destination", expectedCountry, rateLine.GetCartageLocationForZones());

			rateEntry.TI_DestinationLRC = "CNCAN";

			var expectedLocation = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CNCAN");
			AssertEquals("Should prefer more specific location, not just country", expectedLocation, rateLine.GetCartageLocationForZones());

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "CNSHA";
			var deliveryAddress = consignee.MainAddress;
			deliveryAddress.OA_Address1 = "The Bund";
			deliveryAddress.OA_City = "Shanghai";
			deliveryAddress.OA_State = "31";

			rateEntry.TI_OA_CartageDeliveryAddressOverride = deliveryAddress.PK;

			AssertEquals("Should prefer the delivery address to the rate destination", deliveryAddress, rateLine.GetCartageLocationForZones());
		}

		public void TestGetCartageLocationForZones_Origin()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "CN");
			var rateLine = rateEntry.AddRateLine("ODOC", FlatCalculator.Code);

			var expectedCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia);
			AssertEquals("Origin should take unloco from origin", expectedCountry, rateLine.GetCartageLocationForZones());

			rateEntry.TI_OriginLRC = "AUMEL";

			var expectedLocation = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			AssertEquals("Should prefer more specific location, not just country", expectedLocation, rateLine.GetCartageLocationForZones());

			var consignor = Helper.NewOrgHeader(1);

			rateEntry.TI_OA_CartagePickupAddressOverride = consignor.MainAddress.PK;

			AssertEquals("Should prefer the pickup address to the rate origin", consignor.MainAddress, rateLine.GetCartageLocationForZones());
		}

		public void TestGetCartageLocationForZones_Warehouse()
		{
			AssertGetCartageLocationForZones_Warehouse(RatingConstants.RateCategory.WHS);
			AssertGetCartageLocationForZones_Warehouse(RatingConstants.RateCategory.TRW);
			AssertGetCartageLocationForZones_Warehouse(RatingConstants.RateCategory.TWU);
		}

		void AssertGetCartageLocationForZones_Warehouse(string ratingCategory)
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var whsRateEntry = clientRate.AddRateEntry(ratingCategory, Core.Constants.RateMode.ALL, "", "");
			var whsRateLine = whsRateEntry.AddRateLine("ODOC", FlatCalculator.Code);

			AssertEquals("Without a warehouse, should fall back to current company country", GlbCompany.CurrentCompany.Country, whsRateLine.GetCartageLocationForZones());

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse[WhsWarehouseSchema.WW_WarehouseCode] = "W1";
			warehouse[WhsWarehouseSchema.WW_OA_WarehouseAddress] = client.MainAddress.PK;
			whsRateEntry.TI_WW_Warehouse = warehouse.PK;

			AssertEquals("WHS lines should match warehouse address to choose either the city town then country if available", client.MainAddress, whsRateLine.GetCartageLocationForZones());
		}

		#endregion

		public void TestUniversalCopyRateLine()
		{
			var orgHeader = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(orgHeader);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], MinimumOrPerUnitCalculator.Code, "KG");

			Factory.Save();

			var copyTemplateTree = new CopyTemplateTree();
			var entityNode = new EntityCopyTemplateNode { Name = "RateEntry" };
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateEntrySchema.Constants.TI_OriginLRC, CopyMethod = CopyMethod.Copy });

			var rateLineEntityNode = new EntityCopyTemplateNode { Name = "RateLine" };
			var collectionNode = new CollectionCopyTemplateNode
			{
				Name = "RateLines",
				ItemPropertyName = "RateLines",
				ItemsTableName = RateLinesSchema.Constants.TableName,
				InnerNode = rateLineEntityNode,
				CopyMethod = CollectionCopyMethod.All
			};
			entityNode.Nodes.Add(collectionNode);

			copyTemplateTree.InnerNode = entityNode;

			var copyManager = new BusinessObjectCopyManager();

			var copiedItem = copyManager.Copy(rateEntry, copyTemplateTree).Object as RateEntry;
			AssertNotNull("New RateEntry created by copy BusinessObjectCopyManager", copiedItem);
			AssertNotEquals(rateEntry.PK, copiedItem.PK);

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals(rateEntry.TI_OriginLRC, copiedItem.TI_OriginLRC);
			AssertEquals(rateEntry.RateLines.Count, copiedItem.RateLines.Count);

			foreach (var line in copiedItem.RateLines)
			{
				var item = line as RateLine;
				AssertNotNull("New RateLine created by copy BusinessObjectCopyManager", item);
				AssertNotNull("New RateLine parent", item.Parent);
			}
		}

		public void TestDecimalPlaces()
		{
			var decimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals
				{
					Code = RatingConstants.RateCategory.ORG,
					Decimals = "2"
				},
				new SellRatesDecimals
				{
					Code = RatingConstants.RateCategory.DST,
					Decimals = "3"
				}
			};

			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, decimals))
			{
				var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
				var orgRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
				var dstRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "", "AU");
				var orgRateLine = orgRateEntry.RateLines.AddNew();
				var dstRateLine = dstRateEntry.RateLines.AddNew();

				AssertEquals(2, orgRateLine.DecimalPlaces());
				AssertEquals(3, dstRateLine.DecimalPlaces());

				var costing = Helper.NewCosting(Helper.NewOrgHeader());
				var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
				var costLine = costEntry.RateLines.AddNew();

				AssertEquals("Costs are unaffected by the SellRatesDecimals registry", 4, costLine.DecimalPlaces());
			}
		}

		public void TestIsFeeChargeSame()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine1 = rateEntry.RateLines.AddNew();

			Assert(!rateLine1.IsFeeChargeSame(null));

			var rateLine2 = rateEntry.RateLines.AddNew();
			Assert(rateLine1.IsFeeChargeSame(rateLine2));

			rateLine1.TL_FeeChargeType = "DWY";
			rateLine1.TL_FeeChargeLevel = "STD";

			rateLine2.TL_FeeChargeType = "INW";
			rateLine2.TL_FeeChargeLevel = "STA";
			Assert(!rateLine1.IsFeeChargeSame(rateLine2));

			rateLine2.TL_FeeChargeType = "DWY";
			Assert(!rateLine1.IsFeeChargeSame(rateLine2));

			rateLine2.TL_FeeChargeLevel = "STD";
			Assert(rateLine1.IsFeeChargeSame(rateLine2));
		}

		#region ApplicableToOrg

		public void TestApplicableToOrg()
		{
			var criteria = new TestRatingCriteria();
			criteria.JobDirection = Directions.Export;
			criteria.FreightMode = FreightMode.LCL;

			var org1 = Factory.New<OrgHeader>();
			org1.CompanyData.RateTariffLevels.SetLevel("FRT", "EXP", "LCL", 2);

			var org2 = Factory.New<OrgHeader>();

			var tariff1 = Factory.New<CompanyTariff>();
			var tariff1RateEntry = tariff1.AddRateEntry("LCL");
			var tariff1RateLine = tariff1RateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var tariff2 = Factory.New<CompanyTariff>();
			var tariff2RateEntry = tariff2.AddRateEntry("LCL");
			var tariff2RateLine = tariff2RateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var tariff3 = Factory.New<CompanyTariff>();
			var tariff3RateEntry = tariff3.AddRateEntry("LCL");
			var tariff3RateLine = tariff3RateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			Assert(!tariff1RateLine.IsApplicableToOrg(org1, criteria));
			Assert(tariff2RateLine.IsApplicableToOrg(org1, criteria));
			Assert(!tariff3RateLine.IsApplicableToOrg(org1, criteria));

			criteria.JobDirection = Directions.Import;

			Assert(!tariff1RateLine.IsApplicableToOrg(org1, criteria));
			Assert(!tariff2RateLine.IsApplicableToOrg(org1, criteria));
			Assert(!tariff3RateLine.IsApplicableToOrg(org1, criteria));

			var rate = Factory.New<ClientRate>();
			rate.TH_OH = org1.PK;
			var clientRateEntry = rate.AddRateEntry("LCL");
			var clientRateLine = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var quote = Factory.New<Quote>();
			quote.TH_OH = org2.PK;
			var quoteRateEntry = quote.AddRateEntry("LCL");
			var quoteRateLine = quoteRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			Assert(clientRateLine.IsApplicableToOrg(org1, criteria));
			Assert(!clientRateLine.IsApplicableToOrg(org2, criteria));
			Assert(!quoteRateLine.IsApplicableToOrg(org1, criteria));
			Assert(quoteRateLine.IsApplicableToOrg(org2, criteria));
		}

		public void TestIsFeeChargeApplicable()
		{
			var orgHeader = Factory.New<OrgHeader>();

			var tariff = Factory.New<CompanyTariff>();
			var tariffRateEntry = tariff.AddRateEntry("LCL");
			var tariffRateLine = tariffRateEntry.RateLines.AddNew();
			tariffRateLine.TL_FeeChargeType = "FSE";
			tariffRateLine.TL_FeeChargeLevel = "STD";

			AssertEquals(false, tariffRateLine.IsFeeChargeApplicable(orgHeader));
			AssertNotNull(orgHeader.CompanyData);

			var feeChargeLevel = orgHeader.CompanyData.RateFeeChargeLevels.AddNew();
			feeChargeLevel.ORF_ServiceType = "FSE";
			feeChargeLevel.ORF_Level = "STD";

			AssertEquals(true, tariffRateLine.IsFeeChargeApplicable(orgHeader));
		}

		[ExpectNoExceptions]
		public void TestNoInfiniteCycleWhenLoadingApplicableOrganizations()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var testRateClient = Helper.NewClientRate(NewClient);
			var testRateConsignor = Helper.NewClientRate(Consignor);
			var testRateConsignee = Helper.NewClientRate(Consignee);

			NewClient.RelatedManagementSubsidiaryRelations.AddOrganisationWithoutCheckingValid(Consignee);
			Consignee.RelatedManagementSubsidiaryRelations.AddOrganisationWithoutCheckingValid(NewClient);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignee = Consignee;
			testCriteria.Consignor = Consignor;

			var tariff1 = Factory.New<CompanyTariff>();
			var tariff1RateEntry = tariff1.AddRateEntry("LCL");
			var tariff1RateLine = tariff1RateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			Assert(!tariff1RateLine.IsApplicableToOrg(Consignee, testCriteria));

			var rate = Factory.New<ClientRate>();
			rate.TH_OH = Consignee.PK;
			var clientRateEntry = rate.AddRateEntry("LCL");
			var clientRateLine = clientRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			Assert(!clientRateLine.IsApplicableToOrg(Consignor, testCriteria));
		}

		#endregion

		public void TestUNTRateLineItemIsUpdatedWhenRaquiresWeightVolumeAndWeightVolumeChanged()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "USLAX").RateLines[0];
			Assert(line.TL_ContainerOwnership.IsEmpty);
			line.TL_RateCalculator = HighestRateCalculator.Code;

			var rateLineItem = line.RateLineItems.AddNew();
			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals(string.Empty, rateLineItem.TM_BreakWeightVolume);

			line.TL_WeightVolume = RatingConstants.Units.KG;
			AssertEquals(RatingConstants.Units.KG, rateLineItem.TM_BreakWeightVolume);
		}

		public void TestUNTRateLineItemIsUpdatedWhenRaquiresWeightVolumeAndWeightVolumeMultipleChanged()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "USLAX").RateLines[0];
			Assert(line.TL_ContainerOwnership.IsEmpty);
			line.TL_RateCalculator = HighestRateCalculator.Code;

			var rateLineItem = line.RateLineItems.AddNew();
			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals(1, rateLineItem.TM_UnitMultiple);

			line.TL_WeightVolumeMultiple = 100m;
			AssertEquals(100, rateLineItem.TM_UnitMultiple);
		}

		public void TestCMBCalculatorBreaksPerIsEmpty_WhenWeightVolumeChanged()
		{
			var rateEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR");
			var rateLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, Weight.Kilograms);
			rateLine.Calculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;

			rateLine.TL_WeightVolume = Weight.Grams;
			AssertEquals(Calculator.Items.BreaksPerContainerTypeOrClass, rateLine.Calculator.BreaksPer);

			rateLine.TL_WeightVolume = Weight.Pounds;
			AssertEquals(Calculator.Items.BreaksPerContainerTypeOrClass, rateLine.Calculator.BreaksPer);

			rateLine.TL_WeightVolume = Volume.CubicMetres;
			AssertEquals(string.Empty, rateLine.Calculator.BreaksPer);

			rateLine.TL_WeightVolume = Weight.Pounds;
			rateLine.Calculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			AssertEquals(Calculator.Items.BreaksPerContainerTypeOrClass, rateLine.Calculator.BreaksPer);

			rateLine.TL_WeightVolume = Volume.Litre;
			AssertEquals(string.Empty, rateLine.Calculator.BreaksPer);
		}

		public void TestCMBCalculatorBreaksPerIsEmpty_WhenRoundingChanged()
		{
			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.AIR;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				var rateEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR");
				var rateLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, Weight.Kilograms);
				rateLine.Calculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;

				rateLine.TL_Rounding = RatingRoundingTypes.NoRounding;
				AssertEquals(Calculator.Items.BreaksPerContainerTypeOrClass, rateLine.Calculator.BreaksPer);

				rateLine.TL_Rounding = RatingRoundingTypes.Bankers;
				AssertEquals(Calculator.Items.BreaksPerContainerTypeOrClass, rateLine.Calculator.BreaksPer);

				rateLine.TL_Rounding = RatingRoundingTypes.Chargeable;
				AssertEquals(string.Empty, rateLine.Calculator.BreaksPer);

				rateLine.TL_Rounding = RatingRoundingTypes.NoRounding;
				rateLine.Calculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
				AssertEquals(Calculator.Items.BreaksPerContainerTypeOrClass, rateLine.Calculator.BreaksPer);

				rateLine.TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;
				AssertEquals(string.Empty, rateLine.Calculator.BreaksPer);
			}
		}

		#region TestUnitFactorIsUpdatedWhenCalculatorChangedFromWPK

		public void TestUnitFactorIsUpdatedWhenCalculatorChangedFromWPK()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "USLAX").RateLines[0];
			line.TL_RateCalculator = WarehousePackCalculator.Code;
			line.TL_WeightVolume = QuantityUnit.PK;
			line.TL_UnitFactor = UnitFactorList.Codes.LoadedPackagesOnly;
			AssertEquals("Precondition: Unit Factor is correct", UnitFactorList.Codes.LoadedPackagesOnly, line.TL_UnitFactor);

			line.TL_RateCalculator = CombinedCalculator.Code;
			AssertEquals("Unit Factor is set to empty after Calculator changed", string.Empty, line.TL_UnitFactor);
		}

		#endregion

		#region RateLineDeletionShouldNotCreateNewRateLineItemsWhichLaterFailOnSave

		[ExpectNoExceptions]
		public void TestRateLineDeletionShouldNotCreateNewRateLineItemsWhichLaterFailOnSave()
		{
			var header = Factory.NewWithValidTestData<Costing>();
			var entry = header.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "US");
			entry.TI_RateStartDate = ZDate.Today.AddMonths(-6);

			var line1 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line1.GetCalculator<UnitCalculator>().PerUnit = 7;

			var line2 = entry.AddRateLine("BAF", PercentageCalculator.Code);
			line2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			line2.GetCalculator<PercentageCalculator>().Percent = 10m;

			Factory.Save();

			line2.RateCalculatorChanged = true; //need this hack to force Calculator to be reinitialized when touched
			line2.Delete();

			//deleting Rateline will delete RateLineItems. Previously, RateLineItems would touch Calculator while being deleted and trigger RateLieItems recreation. Those new items would later blow the TM_TL FK on save
			Factory.Save();
		}

		#endregion

		#region RateLineItemsMarkedForValidation

		public void TestRateLineItemsMarkedForValidation()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			line.TL_RateCalculator = CombinedCalculator.Code;
			var calc = (CombinedCalculator)line.Calculator;
			var item1 = calc.AddRateLineItem(Calculator.Items.Operator.Minus, 100m, 0m, 0m);

			item1.MarkLightValidationAsValidForTesting();
			Assert(!item1.ShouldValidateOnSave);

			line.TL_WeightVolume = RatingConstants.Units.CN;
			Assert(item1.ShouldValidateOnSave);
		}

		#endregion

		#region Conditions

		public void TestConditionalExpression()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			Assert(line.TL_Condition.IsEmpty);
			Assert(line.TL_ConditionalExpression.IsEmpty);
			Assert(line.TL_ConditionalExpressionInfo.ReadOnly);

			line.TL_Condition = RateLineConditions.OwnBrokerage;
			Assert(line.TL_ConditionalExpressionInfo.ReadOnly);
			Assert(line.TL_ConditionalExpression.IsEmpty);

			line.TL_Condition = RateLineConditions.OwnCFS;
			Assert(line.TL_ConditionalExpressionInfo.ReadOnly);
			Assert(line.TL_ConditionalExpression.IsEmpty);

			line.TL_Condition = RateLineConditions.ForwardingAndBrokerage;
			Assert(line.TL_ConditionalExpressionInfo.ReadOnly);
			Assert(line.TL_ConditionalExpression.IsEmpty);

			line.TL_Condition = RateLineConditions.HandOver;
			Assert(line.TL_ConditionalExpressionInfo.ReadOnly);
			Assert(line.TL_ConditionalExpression.IsEmpty);

			line.TL_Condition = RateLineConditions.UserDefined;
			Assert(!line.TL_ConditionalExpressionInfo.ReadOnly);
			Assert(line.TL_ConditionalExpression.IsEmpty);

			line.TL_ConditionalExpression = "bla==bla";
			AssertEquals("bla==bla", line.TL_ConditionalExpression);

			line.TL_Condition = RateLineConditions.OwnBrokerage;
			Assert(line.TL_ConditionalExpression.IsEmpty);
			Assert(line.TL_ConditionalExpressionInfo.ReadOnly);

			// Although TL_ConditionalExpression is read only, it can still be set during ADAW importing.
			// Once set however, it becomes write-able with a validation error so the user can clear the validation error
			// or once the TL_Condition is updated during the importing it can resolve itself according to the value set
			// for TL_Condition.
			line.TL_Condition = string.Empty;
			Assert(line.TL_ConditionalExpressionInfo.ReadOnly);
			line.TL_ConditionalExpression = "potato==potato";
			Assert(!line.TL_ConditionalExpressionInfo.ReadOnly);
			AssertHasError(line.TL_ConditionalExpressionInfo, "User defined expression can only be set when TL_Condition is set to USR mode");

			// When the condition changes, then the existing text in the conditional expression is cleared and the validation error goes away.
			line.TL_Condition = RateLineConditions.OwnGateway;
			Assert(line.TL_ConditionalExpression.IsEmpty);
			Assert(line.TL_ConditionalExpressionInfo.ReadOnly);
			AssertNoErrors(line.TL_ConditionalExpressionInfo);
		}

		[ExpectNoExceptions]
		public void TestConditionalExpression_ConstraintCheck()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			var rateLine = entry.RateLines[0];
			Factory.Save();

			AssertThrowsException("", "2!=1");
			AssertThrowsException(RateLineConditions.UserDefined, "");
			AssertThrowsException(RateLineConditions.DangerousGoods, "1==1");
			AssertThrowsException(RateLineConditions.DangerousGoods, " ");

			void AssertThrowsException(string tL_Condition, string tL_ConditionalExpression)
			{
				var sql = $@"
UPDATE {rateLine.TableName}
SET
	{RateLinesSchema.Constants.TL_Condition} = '{tL_Condition}',
	{RateLinesSchema.Constants.TL_ConditionalExpression} = '{tL_ConditionalExpression}'
WHERE
	{RateLinesSchema.Constants.PK} = '{rateLine.PK}'";

				NUnit.Framework.Assert.That(delegate
				{
					TestConnection.ExecuteNonQuery(sql);
				}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The UPDATE statement conflicted with the CHECK constraint \"Constraint_TL_ConditionalExpression\"", true), "Should not be able to save");
			}
		}

		public void TestConditionalExpressionDesc()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			line.TL_Condition = RateLineConditions.ForwardingAndBrokerage;
			Assert(line.TL_ConditionalExpressionDescriptionInfo.ReadOnly);

			line.TL_Condition = RateLineConditions.UserDefined;
			Assert(line.TL_ConditionalExpressionDescriptionInfo.ReadOnly);

			line.TL_ConditionalExpression = "\"<Blah>\" == \"ABC\"";
			Assert(!line.TL_ConditionalExpressionDescriptionInfo.ReadOnly);

			line.TL_ConditionalExpressionDescription = "This is a macro";
			line.TL_ConditionalExpressionDescription = "This is a macro";

			AssertEquals("This is a macro", line.TL_ConditionalExpressionDescription);
		}

		#endregion

		#region TestCalculationOrder

		public void TestCalculationOrder()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			line.SetCalculationOrder(5);
			AssertEquals(0, line.GetCalculationOrder());

			line.TL_RateCalculator = PercentageCalculator.Code;
			line.SetCalculationOrder(6);
			AssertEquals(6, line.GetCalculationOrder());
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry01 = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var line11 = entry01.RateLines.AddNew();

			var entry02 = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var line021 = entry02.RateLines.AddNew();

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Break Bulk";

			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry11 = rate1.AddRateEntry("ORG");
			var line111 = entry11.RateLines.AddNew();

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry21 = rate2.AddRateEntry("ORG");
			var line211 = entry21.RateLines.AddNew();

			var rate3 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry31 = rate3.AddRateEntry("ORG");
			var line311 = entry31.AddFlatRateLine(chargeCode.AC_Code, 100m);
			line11.ChargeInternalNoteText = "aaa";
			line021.ChargeInternalNoteText = "aaa";
			line111.ChargeInternalNoteText = "aaa";
			line211.ChargeInternalNoteText = "aaa";
			line311.ChargeInternalNoteText = "aaa";

			line11.ChargeInformationNoteText = "bbb";
			line021.ChargeInformationNoteText = "bbb";
			line111.ChargeInformationNoteText = "bbb";
			line211.ChargeInformationNoteText = "bbb";
			line311.ChargeInformationNoteText = "bbb";

			AssertEquals("aaa", line11.ChargeInternalNoteText);
			AssertEquals("bbb", line11.ChargeInformationNoteText);
			AssertEquals("aaa", line021.ChargeInternalNoteText);
			AssertEquals("bbb", line021.ChargeInformationNoteText);
			AssertEquals("aaa", line111.ChargeInternalNoteText);
			AssertEquals("bbb", line111.ChargeInformationNoteText);
			AssertEquals("aaa", line211.ChargeInternalNoteText);
			AssertEquals("bbb", line211.ChargeInformationNoteText);
			AssertEquals("aaa", line311.ChargeInternalNoteText);
			AssertEquals("bbb", line311.ChargeInformationNoteText);

			line11.Delete();
			entry02.Delete();
			line111.Delete();
			entry21.Delete();

			AssertEquals("", line11.ChargeInternalNoteText);
			AssertEquals("", line11.ChargeInformationNoteText);
			AssertEquals("", line021.ChargeInternalNoteText);
			AssertEquals("", line021.ChargeInformationNoteText);
			AssertEquals("", line111.ChargeInternalNoteText);
			AssertEquals("", line111.ChargeInformationNoteText);
			AssertEquals("", line211.ChargeInternalNoteText);
			AssertEquals("", line211.ChargeInformationNoteText);

			rate3.Factory.Save();
			AssertEquals("aaa", FindNote(line311.PK, PredefinedNoteTypes.Instance.TradeLaneChargeInternalNote));
			AssertEquals("bbb", FindNote(line311.PK, PredefinedNoteTypes.Instance.TradeLaneChargeInformation));

			rate3.Delete();
			rate3.Factory.Save();
			AssertEquals("", FindNote(line311.PK, PredefinedNoteTypes.Instance.TradeLaneChargeInternalNote));
			AssertEquals("", FindNote(line311.PK, PredefinedNoteTypes.Instance.TradeLaneChargeInformation));

			string FindNote(ZGuid parentID, PredefinedNoteType predefinedNoteType)
			{
				Factory.ClearQueryCache();

				var zQuery = new ZDBOnlyQuery(typeof(StmNote));
				zQuery.AddToFilter(StmNoteSchema.ST_Description, predefinedNoteType.MultilingualDescription.GetUnresolvedString());
				zQuery.AddToFilter(StmNoteSchema.ST_ParentID, parentID);
				return Factory.LoadTop1<StmNote>(zQuery)?.ST_NoteText ?? ZString.Empty;
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteDoesNotProvokesConcurrencyError()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Test";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			var item1 = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 0m, 100m);

			Factory.Save();

			item1.TM_FlatAmount = 4.56;
			line.Delete();

			Assert(item1.HasChanges);
			Assert(item1.IsDeleted);

			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestDeleteWithPostponedChangeDoesNotProvokesConcurrencyError()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Test";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			var item1 = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 0m, 100m);

			Factory.Save();

			((ILightValidationInternals)item1).IsValid = true;

			Factory.Save();

			item1.MarkAsNeedingValidation();
			line.Delete();

			Assert(item1.HasChanges);
			Assert(item1.IsDeleted);

			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestDeleteWithNewAddedRateLineItems()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Test";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			line.TL_RateCalculator = FlatCalculator.Code;
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 20m);
			Factory.Save();

			line.TL_RateCalculator = UnitCalculator.Code;
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0m, 3m);
			line.Delete();
			Factory.Save();
		}

		public void TestRelatedEntitiesAreDeletedWithoutLoadingWhenDeletingLine()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			var line = entry.RateLines[0];
			line.TL_RateCalculator = "FLT";
			line.GetCalculator<FlatCalculator>().BaseRate = 11;
			line.TL_Condition = RateLineConditions.UserDefined;
			line.TL_ConditionalExpression = "MOD=FSA";

			var lineItemPKs = Factory.Load<RateLineItem>(new ZQuery(RateLineItemsSchema.TM_TL, line.PK))
				.Select(x => x.PK)
				.ToList();

			var stmNotePKs = Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, line.PK).AddToFilter(StmNoteSchema.ST_Table, line.TableName))
				.Select(x => x.PK)
				.ToList();

			Assert(lineItemPKs.Count > 0);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var lineInNewFactory = newFactory.Load<RateLine>(line.PK);

			lineInNewFactory.Delete();
			newFactory.Save();

			AssertNoRelatedEntityIsLoaded(newFactory);
			AssertRelatedEntitiesAreDeleted(newFactory, lineItemPKs, stmNotePKs);
		}

		#endregion

		#region Charge Codes

		public void TestIsFreightChargeCode()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var testRateLine = rateEntry.RateLines.AddNew();
			var dummyChargeCode = Factory.New<AccChargeCode>();
			testRateLine.TL_AC = dummyChargeCode.PK;

			dummyChargeCode.AC_ChargeGroup = "ORG";
			AssertEquals("Not Freight Charge Code", false, testRateLine.IsFreightChargeCodeLine);

			dummyChargeCode.AC_ChargeGroup = "FRT";
			AssertEquals("Freight Charge Code", true, testRateLine.IsFreightChargeCodeLine);
		}

		#endregion

		#region Description

		public void TestDescription_Quote_EnableLocalChargeCodeDescription_LocalClient_EmptyLocalChargeDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: Description", ZString.Empty, rateLine.TL_RateDesc);

				rateLine.TL_AC = chargeCode.PK;
				AssertEquals
				(
					$"GIVEN enableLocalChargeCodeDescriptionDefault and isLocalClient but charge.LocalChargeDescription is empty THEN Description",
					"Charge Description 1",
					rateLine.TL_RateDesc
				);
			}
		}

		public void TestGetRateDescOrRateDescLocal_EmptyRateDescLocal()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_AC = chargeCode.PK;
				rateLine.OverrideChargeDescription = true;
				rateLine.TL_RateDesc += " (updated)";
				rateLine.TL_RateDescLocal = "";
				AssertEquals
				(
					$"GIVEN empty RateLine LocalDesription THEN should get RateLine Description",
					"Charge Description 1 (updated)",
					rateLine.GetRateDescOrRateDescLocal()
				);
			}
		}

		public void TestDescription_Quote_EnableLocalChargeCodeDescription_LocalClient()
		{
			AssertDescription_EnableLocalChargeCodeDescription
			(
				enableLocalChargeCodeDescriptionDefaultValue: true,
				isLocalClient: true,
				expectedDescription: "Charge Local Description 1"
			);
		}

		public void TestDescription_Quote_EnableLocalChargeCodeDescription_NonLocalClient()
		{
			AssertDescription_EnableLocalChargeCodeDescription
			(
				enableLocalChargeCodeDescriptionDefaultValue: true,
				isLocalClient: false,
				expectedDescription: "Charge Description 1"
			);
		}

		public void TestGetRateDescOrRateDescLocal()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals("RateLine Description", ZString.Empty, rateLine.TL_RateDesc);
					AssertEquals("AccChargeCode Description", ZString.Empty, chargeCode.AC_LocalLanguageDescription);
				});

				rateLine.TL_AC = chargeCode.PK;
				AssertEquals
				(
					"GIVEN AccChargeCode.AC_LocalLanguageDescription is empty THEN RateLine.GetRateDescOrRateDescLocal() should get AccChargeCode.AC_Desc",
					"Charge Description 1",
					rateLine.GetRateDescOrRateDescLocal()
				);
			}
		}

		public void TestGetRateDescOrRateDescLocal_Overriden_OverrideChargeDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals("RateLine Description", ZString.Empty, rateLine.TL_RateDesc);
					AssertEquals("AccChargeCode Description", ZString.Empty, chargeCode.AC_LocalLanguageDescription);
				});

				rateLine.TL_AC = chargeCode.PK;
				rateLine.OverrideChargeDescription = true;

				AssertEquals
				(
					"GIVEN OverrideChargeDescription=TRUE and AccChargeCode.AC_LocalLanguageDescription is empty THEN RateLine.GetRateDescOrRateDescLocal() should get AccChargeCode.AC_Desc",
					"Charge Description 1",
					rateLine.GetRateDescOrRateDescLocal()
				);
			}
		}

		public void TestDescription_Quote_DisableLocalChargeCodeDescription()
		{
			AssertDescription_EnableLocalChargeCodeDescription
			(
				enableLocalChargeCodeDescriptionDefaultValue: false,
				isLocalClient: true,
				expectedDescription: "Charge Description 1"
			);
		}

		void AssertDescription_EnableLocalChargeCodeDescription(bool enableLocalChargeCodeDescriptionDefaultValue, bool isLocalClient, string expectedDescription)
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			if (isLocalClient)
			{
				orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			}

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableLocalChargeCodeDescriptionDefaultValue))
			{
				AssertEquals("Precondition: Description", ZString.Empty, rateLine.TL_RateDesc);

				rateLine.TL_AC = chargeCode.PK;
				AssertEquals
				(
					$"GIVEN enableLocalChargeCodeDescriptionDefault={enableLocalChargeCodeDescriptionDefaultValue} and isLocalClient={isLocalClient} THEN Description",
					expectedDescription,
					rateLine.GetRateDescOrRateDescLocal()
				);
			}
		}

		public void TestDescription_CompanyTariff_EnableLocalChargeCodeDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");

			Factory.Save();

			var ratingHeader = Helper.NewCompanyTariff();
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: Description", ZString.Empty, rateLine.TL_RateDesc);

				rateLine.TL_AC = chargeCode.PK;
				AssertEquals
				(
					$"GIVEN enableLocalChargeCodeDescriptionDefault THEN Description",
					"Charge Local Description 1",
					rateLine.GetRateDescOrRateDescLocal()
				);
			}
		}

		public void TestDescription_Quote_EnableLocalChargeCodeDescription_LocalClient_OverrideChargeDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: Description", ZString.Empty, rateLine.GetRateDescOrRateDescLocal());

				rateLine.TL_AC = chargeCode.PK;
				rateLine.OverrideChargeDescription = true;
				AssertEquals
				(
					$"GIVEN EnableLocalChargeCodeDescriptionDefault with LocalClient and OverrideChargeDescription THEN Description should be LocalLanguageDescription",
					"Charge Local Description 1",
					rateLine.GetRateDescOrRateDescLocal()
				);

				rateLine.OverrideChargeDescription = false;
				AssertEquals
				(
					$"GIVEN EnableLocalChargeCodeDescriptionDefault with LocalClient and disable OverrideChargeDescription THEN Description should be LocalLanguageDescription",
					"Charge Local Description 1",
					rateLine.GetRateDescOrRateDescLocal()
				);
			}
		}

		public void TestDescription_EnableLocalChargeCodeDescription_NonLocalClient_ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: Description", ZString.Empty, rateLine.GetRateDescOrRateDescLocal());

				rateLine.TL_AC = chargeCode.PK;

				var applyLocalChargeCodeDescriptionDefaultToForeignDebtors = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors");

				AssertEquals("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is false", false, applyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);
				AssertEquals
				(
					$"GIVEN EnableLocalChargeCodeDescriptionDefault with not LocalClient then Description should be LanguageDescription",
					"Charge Description 1",
					rateLine.GetRateDescOrRateDescLocal(false)
				);

				using (applyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is true", true, applyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);
					AssertEquals
					(
						$"GIVEN EnableLocalChargeCodeDescriptionDefault with not LocalClient and ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors then Description should be LocalLanguageDescription",
						"Charge Local Description 1",
						rateLine.GetRateDescOrRateDescLocal(false)
					);
				}
			}
		}

		public void TestDescription_Quote_EnableLocalChargeCodeDescription_LocalClient_EmptyChargeCodeLocalLanguageDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertNullOrEmpty("ChargeCode LocalLanguageDescription", chargeCode.AC_LocalLanguageDescription);
					AssertEquals("RateLine Description", ZString.Empty, rateLine.TL_RateDesc);
				});

				rateLine.TL_AC = chargeCode.PK;
				AssertEquals
				(
					$"GIVEN EnableLocalChargeCodeDescriptionDefault with LocalClient and OverrideChargeDescription THEN Description should be LocalLanguageDescription",
					"Charge Description 1",
					rateLine.TL_RateDesc
				);
			}
		}

		#region Multilingual

		public void TestDescription_Multilingual_EnableLocalChargeCodeDescriptionDefault()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");
			AssertDescription_Multilingual
			(
				chargeCode,
				enableLocalChargeCodeDescriptionDefaultValue: true,
				chargeCodeMultiLingualDescription: "Mein Testgebührencode",
				expectedDescription: "Charge Local Description 1"
			);
		}

		public void TestDescription_Multilingual_DisableLocalChargeCodeDescriptionDefault()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");
			AssertDescription_Multilingual
			(
				chargeCode,
				enableLocalChargeCodeDescriptionDefaultValue: false,
				chargeCodeMultiLingualDescription: "Mein Testgebührencode",
				expectedDescription: "Mein Testgebührencode"
			);
		}

		void AssertDescription_Multilingual(AccChargeCode chargeCode, bool enableLocalChargeCodeDescriptionDefaultValue, string chargeCodeMultiLingualDescription, string expectedDescription)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableLocalChargeCodeDescriptionDefaultValue))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, chargeCodeMultiLingualDescription);
				AssertEquals
				(
					$"GIVEN enableLocalChargeCodeDescriptionDefaultValue={enableLocalChargeCodeDescriptionDefaultValue} THEN description",
					expectedDescription,
					rateLine.GetMultilingualRateDesc()
				);
			}
		}

		static void SetChargeCodeDescriptionMultilingual(AccChargeCode chargeCode, IMockResourceStringCache mockRes, string multilingualDescription)
		{
			var resKey = chargeCode.AC_DescInfo.CustomizableDataResourceStrings.GetMultilingualString(chargeCode, chargeCode.AC_Desc).ResourceKey;
			mockRes.Put(resKey, new ResourceStringData(resKey, multilingualDescription));
		}

		public void TestDescription_Multilingual_AddTextToDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");

			var ratingHeader = Helper.NewQuote(Helper.NewOrgHeader());
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, "Mein Testgebührencode");

				rateLine.TL_RateDesc += " (updated)";

				AssertEquals
				(
					"WHEN text is added at the end of description, THEN should show multilingual description with the added text",
					"Mein Testgebührencode (updated)",
					rateLine.GetMultilingualRateDesc()
				);
			}
		}

		public void TestDescription_Multilingual_UpdatingDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");

			var ratingHeader = Helper.NewQuote(Helper.NewOrgHeader());
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, "Mein Testgebührencode");

				rateLine.TL_RateDesc = "User updated description";

				AssertEquals
				(
					"WHEN rateLine description is updated, THEN should show updated text",
					"User updated description",
					rateLine.GetMultilingualRateDesc()
				);
			}
		}

		#endregion

		public void TestDescriptionSetOnNonFreightEntries()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_Desc = "Break Bulk";

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testEntry = testRate.AddRateEntry("ORG");
			var testLine = testEntry.RateLines.AddNew();

			AssertEquals("Description", ZString.Empty, testLine.TL_RateDesc);
			AssertEquals("Local Description", ZString.Empty, testLine.TL_RateDescLocal);
			testLine.TL_AC = dummyChargeCode.PK;
			AssertEquals("Description", (ZString)"BREAK BULK", testLine.TL_RateDesc.ToUpper());
			AssertEquals("Local Description", (ZString)"BREAK BULK", testLine.TL_RateDescLocal.ToUpper());
		}

		public void TestDescriptionReadOnlyIfNotOverriding()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("ORG");
			var rateLine = entry.RateLines.AddNew();

			rateLine.OverrideChargeDescription = true;
			AssertEquals("Description editable", false, rateLine.TL_RateDescInfo.ReadOnly);
			AssertEquals("Local Description editable", false, rateLine.TL_RateDescLocalInfo.ReadOnly);

			rateLine.OverrideChargeDescription = false;
			AssertEquals("Description NOT editable", true, rateLine.TL_RateDescInfo.ReadOnly);
			AssertEquals("Local Description NOT editable", true, rateLine.TL_RateDescLocalInfo.ReadOnly);
		}

		public void TestOverrideChargeDescription_ReadOnly()
		{
			var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
			ratingHeader.TH_RateType = "XXX";

			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "", "AU");
			var rateLine = entry.RateLines[0];
			AssertEquals("For unexpected RateType, it should not readOnly", false, rateLine.OverrideChargeDescriptionInfo.ReadOnly);
		}

		public void TestDescriptionReadOnlySecurityRights_ClientRate()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			AssertChargeDescriptionReadOnly(rate, Env.Security.ClientRatesChargeDescriptionOverride);
		}

		public void TestDescriptionReadOnlySecurityRights_Costing()
		{
			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			AssertChargeDescriptionReadOnly(cost, Env.Security.CostingRatesChargeDescriptionOverride);
		}

		public void TestDescriptionReadOnlySecurityRights_Tariff()
		{
			var tariff = Factory.New<CompanyTariff>();
			AssertChargeDescriptionReadOnly(tariff, Env.Security.CompanyTariffRatesChargeDescriptionOverride);
		}

		public void TestDescriptionReadOnlySecurityRights_Quotation()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			AssertChargeDescriptionReadOnly(quote, Env.Security.QuotationChargeDescriptionOverride);
		}

		public void TestDescriptionReadOnlySecurityRights_IntercompanyTariffs()
		{
			var intercompany = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			AssertChargeDescriptionReadOnly(intercompany, Env.Security.IntercompanyTariffsChargeDescriptionOverride);
		}

		void AssertChargeDescriptionReadOnly(RatingHeader ratingHeader, SecurityCheckpoint securityCheckpoint)
		{
			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "", "AU");
			var rateLine = entry.RateLines[0];

			AssertEquals("Pre-condition", true, securityCheckpoint.IsAllowed);
			AssertEquals("Pre-condition", false, rateLine.OverrideChargeDescription);
			AssertEquals("Pre-condition", false, rateLine.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals("Pre-condition", true, rateLine.TL_RateDescInfo.ReadOnly);
			AssertEquals("Pre-condition", true, rateLine.TL_RateDescLocalInfo.ReadOnly);

			rateLine.OverrideChargeDescription = true;
			AssertEquals("Can now edit description", false, rateLine.TL_RateDescInfo.ReadOnly);
			AssertEquals("Can now edit local description", false, rateLine.TL_RateDescLocalInfo.ReadOnly);

			rateLine.TL_RateDesc = "some desc";
			rateLine.TL_RateDescLocal = "some local desc";
			AssertEquals("some desc", rateLine.TL_RateDesc);
			AssertEquals("some local desc", rateLine.TL_RateDescLocal);

			securityCheckpoint.IsAllowed = false;
			Factory.Save();
			rateLine = new BusinessObjectFactory().Load<RateLine>(rateLine.PK);

			AssertEquals("Once reloaded should be true (even security settings aren't set, it should affect OverrideChargeDescription)", true, rateLine.OverrideChargeDescription);
			AssertEquals("But should not be overrideable", true, rateLine.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals("Description still NOT editable", true, rateLine.TL_RateDescInfo.ReadOnly);
			AssertEquals("Local Description still NOT editable", true, rateLine.TL_RateDescLocalInfo.ReadOnly);
			AssertEquals("Should not clear existing description", "some desc", rateLine.TL_RateDesc);
			AssertEquals("Should not clear existing local description", "some local desc", rateLine.TL_RateDescLocal);

			securityCheckpoint.IsAllowed = true;
			Factory.Save();
			rateLine = new BusinessObjectFactory().Load<RateLine>(rateLine.PK);

			AssertEquals(true, rateLine.OverrideChargeDescription);
			AssertEquals(false, rateLine.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals(false, rateLine.TL_RateDescInfo.ReadOnly);
			AssertEquals(false, rateLine.TL_RateDescLocalInfo.ReadOnly);
			AssertEquals("Should not clear existing description", "some desc", rateLine.TL_RateDesc);
			AssertEquals("Should not clear existing local description", "some local desc", rateLine.TL_RateDescLocal);
		}

		#endregion

		#region Rate Calculator

		public void TestCompanyTariffCalcShowsResultsWeightVolFields()
		{
			var newFactory = new BusinessObjectFactory();

			var org = newFactory.New<OrgHeader>();
			org.OH_Code = "TESZUBS";
			org.OH_FullName = "ZUBIN ORG 123";
			org.MainAddress.OA_Address1 = "26 Myrtle Street";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var testChargeCode = newFactory.New<AccChargeCode>();
			testChargeCode.AC_Code = "TEZZ";
			testChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			var tariff = newFactory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var tariffLine = tariffEntry.RateLines.AddNew();
			tariffLine.TL_AC = testChargeCode.PK;
			tariffLine.TL_RateCalculator = UnitCalculator.Code;
			tariffLine.TL_WeightVolume = "CN";
			tariffLine.TL_ContainerOwnership = "SHP";
			tariffLine.UseOnlyActualWeightMeasure = true;
			tariffLine.TL_WeightVolumeMultiple = 30m;
			tariffLine.TL_RX_NKCurrency = "INR";

			var rate = newFactory.New<ClientRate>();
			rate.TH_OH = org.PK;
			var entry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line = entry.RateLines.AddNew();
			line.TL_AC = testChargeCode.PK;

			newFactory.Save();

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			line.TL_RX_NKCurrency = "USD";
			AssertEquals("Weight Volume Field Disabled", true, line.TL_WeightVolumeInfo.ReadOnly);
			Assert(line.UnitMultipleAsStringInfo.ReadOnly);
			Assert(line.UseOnlyActualWeightMeasureInfo.ReadOnly);
			Assert(line.TL_ContainerOwnershipInfo.ReadOnly);
			Assert(!line.TL_RX_NKCurrencyInfo.ReadOnly);

			AssertEquals("Values Blank for Comp Tariff Calc", ZString.Empty, line.TL_WeightVolume);
			AssertEquals("Values Blank for Comp Tariff Calc", 0m, line.TL_WeightVolumeMultiple);
			AssertEquals("Values Blank for Comp Tariff Calc", "", line.TL_ContainerOwnership);
			AssertEquals("Values Blank for Comp Tariff Calc", false, line.UseOnlyActualWeightMeasure);
			AssertEquals("Currency shown from this line", "USD", line.TL_RX_NKCurrency);

			line.ViewResults = true;
			AssertNotNull("Results Line expected", line.ResultsRateLine);
			Assert(line.TL_WeightVolumeInfo.ReadOnly);
			Assert(line.UnitMultipleAsStringInfo.ReadOnly);
			Assert(line.UseOnlyActualWeightMeasureInfo.ReadOnly);
			Assert(line.TL_RX_NKCurrencyInfo.ReadOnly);
			Assert(line.TL_ContainerOwnershipInfo.ReadOnly);

			AssertEquals("Values shown from Company Tariff Line", "CN", line.TL_WeightVolume);
			AssertEquals("Values shown from Company Tariff Line", "SHP", line.TL_ContainerOwnership);
			AssertEquals("Values shown from Company Tariff Line", 30m, line.TL_WeightVolumeMultiple);
			AssertEquals("Values shown from Company Tariff Line", true, line.UseOnlyActualWeightMeasure);
			AssertEquals("Currency shown from Company Tariff line", "INR", line.TL_RX_NKCurrency);
		}

		public void TestViewResults_UseContractNumberFilter()
		{
			AssertUseColumnInViewResults(
				RatingConstants.RateCategory.AIR,
				RateMode.LSE,
				RateEntrySchema.TI_ContractNumber,
				new ZString("123"),
				new ZString("456")
			);
		}

		public void TestViewResults_UseContractNumberFilter_CSTCalculator()
		{
			AssertUseColumnInViewResults(
				RatingConstants.RateCategory.AIR,
				RateMode.LSE,
				RateEntrySchema.TI_ContractNumber,
				new ZString("123"),
				new ZString("456"),
				costingBased: true
			);
		}

		public void TestViewResults_UseFMCTariffIDFilter()
		{
			AssertUseColumnInViewResults(
				RatingConstants.RateCategory.AIR,
				RateMode.LSE,
				RateEntrySchema.TI_FMCTariffID,
				new ZString("123"),
				new ZString("456")
			);
		}

		public void TestViewResults_UseFMCTariffIDFilter_NonFreightCategory()
		{
			AssertUseColumnInViewResults(
				RatingConstants.RateCategory.ORG,
				RateMode.FCL,
				RateEntrySchema.TI_FMCTariffID,
				new ZString("123"),
				new ZString("456")
			);
		}

		public void TestViewResults_UsesNonOperatingReeferFilter()
		{
			var container = Factory.New<RefContainer>();
			container.RC_Code = "Z1";

			AssertUseColumnInViewResults(
				RatingConstants.RateCategory.FCL,
				RateMode.SEA,
				RateEntrySchema.TI_IsNonOperatedReefer,
				new ZString("Y"),
				new ZString("N"),
				container: container.PK
			);
		}

		public void TestViewResults_UsesNonOperatingReeferFilter_NonFreightCategory()
		{
			var container = Factory.New<RefContainer>();
			container.RC_Code = "Z1";

			AssertUseColumnInViewResults(
				RatingConstants.RateCategory.ORG,
				RateMode.FCL,
				RateEntrySchema.TI_IsNonOperatedReefer,
				new ZString("Y"),
				new ZString("N"),
				container: container.PK
			);
		}

		void AssertUseColumnInViewResults(string category, string rateMode, SchemaColumn column, IZType value1, IZType value2, ZGuid container = default, bool costingBased = false)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESZUBS";
			org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var header = costingBased ? Factory.New<Costing>() : SetupCompanyTariff();

			// Empty entry to check for priority aswell
			var emptyEntry = header.AddRateEntryWithFlatRateLine(category, rateMode, "AUSYD", "USLAX", "FRT", 300m, "NZD");
			emptyEntry.TI_RC = container;

			var entry1 = header.AddRateEntryWithFlatRateLine(category, rateMode, "AUSYD", "USLAX", "FRT", 100m, "AUD");
			entry1.TI_RC = container;
			entry1.SetValue(column, value1);

			var entry2 = header.AddRateEntryWithFlatRateLine(category, rateMode, "AUSYD", "USLAX", "FRT", 200m, "USD");
			entry2.TI_RC = container;
			entry2.SetValue(column, value2);

			var rate = Factory.New<ClientRate>();
			rate.TH_OH = org.PK;
			var entry = rate.AddRateEntry(category, rateMode, "AUSYD", "USLAX", removeLines: true);
			entry.TI_RC = container;

			Factory.Save();

			var calculatorCode = costingBased ? CompanyTariffOrCostBasedCalculator.CostBasedCode : CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var line = entry.AddRateLine("FRT", calculatorCode);
			line.TL_RX_NKCurrency = "AUD";

			entry.SetValue(column, value1);
			line.ViewResults = true;
			AssertNotNull(line.ResultsRateLine);
			AssertEquals("AUD", line.ResultsRateLine.TL_RX_NKCurrency);
			AssertEquals(100m, (line.ResultsRateLine.Calculator as FlatCalculator).BaseRate);
			line.ViewResults = false;

			entry.SetValue(column, value2);
			line.ViewResults = true;
			AssertNotNull(line.ResultsRateLine);
			AssertEquals("USD", line.ResultsRateLine.TL_RX_NKCurrency);
			AssertEquals(200m, (line.ResultsRateLine.Calculator as FlatCalculator).BaseRate);
		}

		RatingHeader SetupCompanyTariff()
		{
			var tariff = Factory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			return tariff;
		}

		public void TestFlatCalcClearsWeightVolFields()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();

			line.TL_RateCalculator = UnitCalculator.Code;
			AssertEquals("Weight Volume Field Enabled", false, line.TL_WeightVolumeInfo.ReadOnly);
			AssertEquals("Weight Volume Multiple Field Enabled", false, line.UnitMultipleAsStringInfo.ReadOnly);
			AssertEquals("Use Actuals Field Enabled", false, line.UseOnlyActualWeightMeasureInfo.ReadOnly);
			AssertEquals("Currency Field Enabled", false, line.TL_RX_NKCurrencyInfo.ReadOnly);

			line.TL_WeightVolume = "KG";
			line.TL_WeightVolumeMultiple = 10m;
			line.UseOnlyActualWeightMeasure = true;

			line.TL_RateCalculator = FlatCalculator.Code;
			AssertEquals("Weight Volume Field Disabled", true, line.TL_WeightVolumeInfo.ReadOnly);
			AssertEquals("Weight Volume Multiple Field Disabled", true, line.UnitMultipleAsStringInfo.ReadOnly);
			AssertEquals("Use Actuals Field Disabled", true, line.UseOnlyActualWeightMeasureInfo.ReadOnly);
			AssertEquals("Currency Field Enabled as it still applies", false, line.TL_RX_NKCurrencyInfo.ReadOnly);

			AssertEquals("Values Cleared", ZString.Empty, line.TL_WeightVolume);
			AssertEquals("Values Cleared", 0m, line.TL_WeightVolumeMultiple);
			AssertEquals("Values Cleared", false, line.UseOnlyActualWeightMeasure);
		}

		public void TestTL_RateCalculator_WarehousePackTypeCalculatorDefaultsWeightVolume()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();

			line.TL_RateCalculator = WarehousePackCalculator.Code;
			AssertEquals("Default weight volume is UNT.", "UNT", line.TL_WeightVolume);
			AssertEquals("Weight Volume Field should be editable.", false, line.TL_WeightVolumeInfo.ReadOnly);
		}

		public void TestRateCaculatorSetOnRateLine()
		{
			var dummyChargeCode1 = Factory.New<AccChargeCode>();
			dummyChargeCode1.AC_RateCalculator = PercentageCalculator.Code;

			var dummyChargeCode2 = Factory.New<AccChargeCode>();
			dummyChargeCode2.AC_RateCalculator = FlatCalculator.Code;

			var testQuote = Factory.New<Quote>();
			var oRGEntry = testQuote.AddRateEntry("ORG");
			var oRGLine = oRGEntry.RateLines.AddNew();

			oRGLine.TL_AC = dummyChargeCode1.PK;
			AssertEquals("Rate Calc", PercentageCalculator.Code, oRGLine.TL_RateCalculator);

			oRGLine.TL_AC = dummyChargeCode2.PK;
			AssertEquals("Rate Calc", FlatCalculator.Code, oRGLine.TL_RateCalculator);
		}

		public void TestRateCaculatorCanBeOverridden()
		{
			var dummyChargeCode1 = Factory.New<AccChargeCode>();
			dummyChargeCode1.AC_RateCalculator = FlatCalculator.Code;

			var testQuote = Factory.New<Quote>();
			var oRGEntry = testQuote.AddRateEntry("ORG");
			var oRGLine = oRGEntry.RateLines.AddNew();

			oRGLine.TL_AC = dummyChargeCode1.PK;
			AssertEquals("Default Rate Calc Set from Charge Code", FlatCalculator.Code, oRGLine.TL_RateCalculator);

			oRGLine.TL_RateCalculator = AgencyCalculator.Code;
			AssertEquals("Rate Calc on Rate Line can be overridden", AgencyCalculator.Code, oRGLine.TL_RateCalculator);
			AssertEquals("Overriding Rate calc on Rate Line doesn't affect charge code", FlatCalculator.Code, oRGLine.ChargeCode.AC_RateCalculator);
		}

		public void TestViewCalculator()
		{
			var testCost = Factory.New<Costing>();
			testCost.TH_OH = TransportProvider1.PK;
			var costEntry = testCost.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var costLine = costEntry.RateLines[0];
			costLine.TL_RateCalculator = UnitCalculator.Code;
			((UnitCalculator)costLine.Calculator).PerUnit = 5m;

			Factory.Save();

			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = NewClient.PK;
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = TransportProvider1.PK;
			var line = entry.RateLines[0];
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			((CompanyTariffOrCostBasedCalculator)line.Calculator).Percent = 10M;

			AssertEquals(typeof(CompanyTariffOrCostBasedCalculator), line.Calculator.GetType());
			AssertEquals(typeof(CompanyTariffOrCostBasedCalculator), line.ViewCalculator.GetType());

			line.ViewResults = true;
			AssertEquals(typeof(CompanyTariffOrCostBasedCalculator), line.Calculator.GetType());
			AssertEquals(typeof(UnitCalculator), line.ViewCalculator.GetType());

			line.ViewResults = false;
			AssertEquals(typeof(CompanyTariffOrCostBasedCalculator), line.Calculator.GetType());
			AssertEquals(typeof(CompanyTariffOrCostBasedCalculator), line.ViewCalculator.GetType());
		}

		public void TestViewCalculatorShowsCorrectValuesWhenCaclulatorIsCTBAndOrderIsPercentFirst()
		{
			var testCost = Factory.New<Costing>();
			testCost.TH_OH = TransportProvider1.PK;
			var costEntry = testCost.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var costLine = costEntry.RateLines[0];
			costLine.TL_RateCalculator = CombinedCalculator.Code;

			var combinedCalc = costLine.GetCalculator<CombinedCalculator>();
			combinedCalc.PerUnit = 100m;
			combinedCalc.Minimum = 100m;
			combinedCalc.BaseRate = 100m;

			Factory.Save();

			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = NewClient.PK;
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = TransportProvider1.PK;
			var line = entry.RateLines[0];
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var ctbCalc = line.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			ctbCalc.Percent = 10M;
			ctbCalc.PerUnitPercent = 10M;
			ctbCalc.Minimum = 100m;
			ctbCalc.BaseRate = 400m;
			ctbCalc.PerUnit = 300m;
			ctbCalc.PerUnitPercent = 10M;
			ctbCalc.CalculationOrder = Calculator.Items.PercentFirst;

			line.ViewResults = true;
			AssertEquals(typeof(CompanyTariffOrCostBasedCalculator), line.Calculator.GetType());
			AssertEquals(typeof(CombinedCalculator), line.ViewCalculator.GetType());

			var minimum = line.ViewCalculator[Calculator.Items.Operator.MIN];
			var baseRate = line.ViewCalculator[Calculator.Items.Operator.BAS];
			var perUnit = line.ViewCalculator[Calculator.Items.Operator.UNT];

			CombineAssertions(() =>
			{
				AssertEquals("Minimum value is incorrect", 210m, minimum);
				AssertEquals("Base rate value is incorrect", 510m, baseRate);
				AssertEquals("Per unit value is incorrect", 410m, perUnit);
			});
		}

		public void TestViewCalculatorShowsCorrectValuesWhenCaclulatorIsCTBAndOrderIsIncreaseFirst()
		{
			var testCost = Factory.New<Costing>();
			testCost.TH_OH = TransportProvider1.PK;
			var costEntry = testCost.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var costLine = costEntry.RateLines[0];
			costLine.TL_RateCalculator = CombinedCalculator.Code;

			var combinedCalc = costLine.GetCalculator<CombinedCalculator>();
			combinedCalc.PerUnit = 100m;
			combinedCalc.Minimum = 100m;
			combinedCalc.BaseRate = 100m;

			Factory.Save();

			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = NewClient.PK;
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = TransportProvider1.PK;
			var line = entry.RateLines[0];
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var ctbCalc = line.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			ctbCalc.Percent = 10M;
			ctbCalc.PerUnitPercent = 10M;
			ctbCalc.Minimum = 100m;
			ctbCalc.BaseRate = 400m;
			ctbCalc.PerUnit = 300m;
			ctbCalc.PerUnitPercent = 10M;
			ctbCalc.CalculationOrder = Calculator.Items.IncreaseFirst;

			line.ViewResults = true;
			AssertEquals(typeof(CompanyTariffOrCostBasedCalculator), line.Calculator.GetType());
			AssertEquals(typeof(CombinedCalculator), line.ViewCalculator.GetType());

			var minimum = line.ViewCalculator[Calculator.Items.Operator.MIN];
			var baseRate = line.ViewCalculator[Calculator.Items.Operator.BAS];
			var perUnit = line.ViewCalculator[Calculator.Items.Operator.UNT];

			CombineAssertions("View calculator values are incorrect when we set base rate", () =>
			{
				AssertEquals("Minimum value is incorrect", 220m, minimum);
				AssertEquals("Base rate value is incorrect", 550m, baseRate);
				AssertEquals("Per unit value is incorrect", 440m, perUnit);
			});
		}

		public void TestLockCalculator()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			AssertEquals(FlatCalculator.Code, line.TL_RateCalculator);

			line.TL_AC = Helper.ChargeCodes["OCART"].PK;
			AssertEquals(CartageCalculator.Code, line.TL_RateCalculator);

			line.LockCalculator = true;
			line.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			AssertEquals(CartageCalculator.Code, line.TL_RateCalculator);
		}

		[ExpectNoExceptions]
		public void TestViewResultsWithNullChargeCode()
		{
			var supplier = Helper.NewOrgHeader();
			Helper.NewCosting(supplier).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Helper.NewCompanyTariff().AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var line = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			line.Parent.TI_OH_Supplier = supplier.PK;
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			Factory.Save();

			line.TL_AC = ZGuid.Empty;
			line.ViewResults = true;

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			line.ViewResults = true;
		}

		public void TestViewResultsVisible()
		{
			var testCost = Helper.NewCosting(Helper.NewOrgHeader());
			testCost.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 10m;

			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry.TI_OH_TransportProvider = testCost.TH_OH;
			var line = entry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			Assert(line.ViewResultsVisible);

			line.ViewResults = true;
			AssertEquals(typeof(FlatCalculator), line.ViewCalculator.GetType());
			Assert(line.ViewResultsVisible);

			line.ViewResults = false;
			AssertEquals(typeof(CompanyTariffOrCostBasedCalculator), line.ViewCalculator.GetType());
			line.IsBulkRateUpdateActionLine = true;
			Assert(!line.ViewResultsVisible);
		}

		#endregion

		#region Cartage Equipment

		public void TestCartageEquipmentDefaulting()
		{
			var testClient = Helper.NewOrgHeader();

			var destAddr = testClient.Addresses.AddNew();
			destAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			destAddr.OA_FCLEquipmentNeeded = "DFC";
			destAddr.OA_LCLEquipmentNeeded = "DLC";
			destAddr.OA_AIREquipmentNeeded = "DAI";

			var testRate = Helper.NewClientRate(testClient);
			var entry = testRate.AddRateEntry("DST", "FCL", "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CartageCalculator.Code;

			// Import FCL
			AssertEquals("Equipment set to IMPORT, FCL", "DFC", line.Calculator.FindRateLineItem(CartageCalculator.Items.EquipmentType).TM_Text);

			// Import LCL
			entry = testRate.AddRateEntry("DST", "LCL", "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CartageCalculator.Code;
			AssertEquals("Equipment set to IMPORT, LCL", "DLC", line.Calculator.FindRateLineItem(CartageCalculator.Items.EquipmentType).TM_Text);

			// Import AIR
			entry = testRate.AddRateEntry("DST", "AIR", "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CartageCalculator.Code;
			AssertEquals("Equipment set to IMPORT, AIR", "DAI", line.Calculator.FindRateLineItem(CartageCalculator.Items.EquipmentType).TM_Text);

			// Export FCL
			entry = testRate.AddRateEntry("DST", "FCL", GlbBranch.CurrentBranch.GB_RL_NKHomePort, "USLAX");
			line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CartageCalculator.Code;
			AssertEquals("Equipment set to EXPORT, FCL", "DFC", line.Calculator.FindRateLineItem(CartageCalculator.Items.EquipmentType).TM_Text);

			// Export LCL
			entry = testRate.AddRateEntry("DST", "LCL", GlbBranch.CurrentBranch.GB_RL_NKHomePort, "USLAX");
			line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CartageCalculator.Code;
			AssertEquals("Equipment set to EXPORT, LCL", "DLC", line.Calculator.FindRateLineItem(CartageCalculator.Items.EquipmentType).TM_Text);

			// Export AIR
			entry = testRate.AddRateEntry("DST", "AIR", GlbBranch.CurrentBranch.GB_RL_NKHomePort, "USLAX");
			line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CartageCalculator.Code;
			AssertEquals("Equipment set to EXPORT, AIR", "DAI", line.Calculator.FindRateLineItem(CartageCalculator.Items.EquipmentType).TM_Text);
		}

		#endregion

		#region Clone

		public void TestClone()
		{
			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("AIR");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = "ABC";
			line.TL_RateDesc = "Zubin";
			line.TL_RateDescLocal = "Zubin Local";
			line.TL_WeightVolumeMultiple = 9m;
			line.TL_ActualPercentage = 100;
			line.TL_WeightVolume = "M3";
			line.ChargeInformationNoteText = "Information Note Text";
			line.ChargeInternalNoteText = "Internal Note Text";
			line.TL_Condition = RateLineConditions.UserDefined;
			line.TL_ConditionalExpression = "MOD=FAS";

			line.RateLineItems.AddNew();

			var clonedLine = line.Clone(entry.RateLines);

			Assert("Different objects", line != clonedLine);
			AssertEquals("RateLineItems.Count", 0, clonedLine.RateLineItems.Count);
			AssertEquals("TL_RateCalculator", "ABC", clonedLine.TL_RateCalculator);
			AssertEquals("TL_RateDesc", "Zubin", clonedLine.TL_RateDesc);
			AssertEquals("TL_RateDescLocal", "Zubin Local", clonedLine.TL_RateDescLocal);
			AssertEquals("TL_WeightVolumeMultiple", 9m, clonedLine.TL_WeightVolumeMultiple);
			AssertEquals("TL_ActualPercentage", (ZByte)100, clonedLine.TL_ActualPercentage);
			AssertEquals("TL_WeightVolume", "M3", clonedLine.TL_WeightVolume);
			AssertEquals("ChargeInformationNoteText", "Information Note Text", clonedLine.ChargeInformationNoteText);
			AssertEquals("ChargeInternalNoteText", "Internal Note Text", clonedLine.ChargeInternalNoteText);
			AssertEquals(RateLineConditions.UserDefined, clonedLine.TL_Condition);
			AssertEquals("MOD=FAS", clonedLine.TL_ConditionalExpression);
		}

		public void TestCloneForGlobalRate()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry("AIR");
			var line1 = entry.RateLines.AddNew();
			line1.TL_AC = Helper.ChargeCodes["FRT"].PK;

			var line2 = entry.RateLines.AddNew();
			line2.TL_AC = normalChargeCodeLinked.PK;

			var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
			var globalEntry = globalClientRate.AddRateEntry("AIR");

			var clonedLine1 = line1.Clone(globalEntry.RateLines);
			AssertEquals("Cannot be mapped so should be left with an error", line1.TL_AC, clonedLine1.TL_AC);

			var clonedLine2 = line2.Clone(globalEntry.RateLines);
			AssertEquals("Should be global charge PK", globalChargeCode.PK, clonedLine2.TL_AC);
		}

		public void TestCopyTo_Description_NotOverriden()
		{
			var baf = Helper.ChargeCodes["BAF"];
			baf.AC_LocalLanguageDescription = "Local Bunker Adjustment Factor";

			var costing = Helper.NewCosting(NewClient);
			var costingRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			var costingRateLine = (RateLine)costingRateEntry.RateLines.Single();
			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Charge Code description", "Bunker Adjustment Factor", baf.AC_Desc);

				AssertEquals("Source OverrideChargeDescription", false, costingRateLine.OverrideChargeDescription);

				AssertEquals("Source description", "Bunker Adjustment Factor", costingRateLine.TL_RateDesc);
				AssertNullOrEmpty("Source base description", costingRateLine.BaseRateDesc);

				AssertEquals("Source local description", "Local Bunker Adjustment Factor", costingRateLine.TL_RateDescLocal);
				AssertNullOrEmpty("Source base local description", costingRateLine.BaseRateDescLocal);
			});

			var clientRate = Helper.NewClientRate(NewClient);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX");
			var clientRateLine = clientRateEntry.RateLines.AddNew();
			costingRateLine.CopyTo(clientRateLine);
			CombineAssertions("GIVEN not overriden WHEN CopyTo THEN should get ChargeCode description/localDescription", () =>
			{
				AssertEquals("Target OverrideChargeDescription", false, clientRateLine.OverrideChargeDescription);

				AssertEquals("Target description", "Bunker Adjustment Factor", clientRateLine.TL_RateDesc);
				AssertNullOrEmpty("Target base description", clientRateLine.BaseRateDesc);

				AssertEquals("Target local description", "Local Bunker Adjustment Factor", clientRateLine.TL_RateDescLocal);
				AssertNullOrEmpty("Target base local description", clientRateLine.BaseRateDescLocal);
			});
		}

		public void TestCopyTo_Description_Overriden()
		{
			var costing = Helper.NewCosting(NewClient);
			var costingRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			var costingRateLine = (RateLine)costingRateEntry.RateLines.Single();
			costingRateLine.OverrideChargeDescription = true;
			costingRateLine.TL_RateDesc = "BAF (updated)";
			costingRateLine.TL_RateDescLocal = "Local BAF (updated)";

			var clientRate = Helper.NewClientRate(NewClient);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX");
			var clientRateLine1 = clientRateEntry.RateLines.AddNew();
			costingRateLine.CopyTo(clientRateLine1);
			CombineAssertions
			(
				"GIVEN overriden description WHEN CopyTo THEN should copy override flag and description",
				() =>
				{
					AssertEquals("OverrideChargeDescription", true, clientRateLine1.OverrideChargeDescription);

					AssertEquals("Description", "BAF (updated)", clientRateLine1.TL_RateDesc);
					AssertEquals("Base Description", "BAF (updated)", clientRateLine1.BaseRateDesc);

					AssertEquals("Local Description", "Local BAF (updated)", clientRateLine1.TL_RateDescLocal);
					AssertEquals("Base Local Description", "Local BAF (updated)", clientRateLine1.BaseRateDescLocal);
				}
			);

			costingRateLine.TL_RateDescLocal = "";
			var clientRateLine2 = clientRateEntry.RateLines.AddNew();
			costingRateLine.CopyTo(clientRateLine2);
			CombineAssertions
			(
				"GIVEN empty overriden local description WHEN CopyTo THEN should copy override flag with empty local description",
				() =>
				{
					AssertEquals("OverrideChargeDescription", true, clientRateLine2.OverrideChargeDescription);
					AssertNullOrEmpty("Local Description", clientRateLine2.TL_RateDescLocal);
					AssertNullOrEmpty("Base Local Description", clientRateLine2.BaseRateDescLocal);
				}
			);
		}

		public void TestCopyTo_Description_OverridenFromChargeCodeDescription()
		{
			var baf = Helper.ChargeCodes["BAF"];
			baf.AC_LocalLanguageDescription = "Local Bunker Adjustment Factor";

			var costing = Helper.NewCosting(NewClient);
			var costingRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			var costingRateLine = (RateLine)costingRateEntry.RateLines.Single();
			costingRateLine.OverrideChargeDescription = true;

			var clientRate = Helper.NewClientRate(NewClient);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX");
			var clientRateLine1 = clientRateEntry.RateLines.AddNew();
			costingRateLine.CopyTo(clientRateLine1);
			CombineAssertions
			(
				"GIVEN overriden description WHEN CopyTo THEN should copy override flag and description",
				() =>
				{
					AssertEquals("OverrideChargeDescription", true, clientRateLine1.OverrideChargeDescription);

					AssertEquals("Description", "Bunker Adjustment Factor", clientRateLine1.TL_RateDesc);
					AssertEquals("Base Description", "Bunker Adjustment Factor", clientRateLine1.BaseRateDesc);

					AssertEquals("Local Description", "Local Bunker Adjustment Factor", clientRateLine1.TL_RateDescLocal);
					AssertEquals("Base Local Description", "Local Bunker Adjustment Factor", clientRateLine1.BaseRateDescLocal);
				}
			);
		}

		public void TestLocalRateLineCopyToGlobalRateLine()
		{
			AccChargeCode localChargeCode, localChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out localChargeCode, out localChargeCodeLinked, out globalChargeCode);

			var client = Helper.NewOrgHeader();
			var localClientRate = Helper.NewClientRate(client);
			var localRateEntry = localClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var localRateLine1 = localRateEntry.AddRateLine(localChargeCodeLinked, FlatCalculator.Code);
			var localRateLine2 = localRateEntry.AddRateLine(localChargeCode, FlatCalculator.Code);

			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			Factory.Save();

			var globalRateLine1 = globalRateEntry.RateLines.AddNew();
			localRateLine1.CopyTo(globalRateLine1);

			var globalRateLine2 = globalRateEntry.RateLines.AddNew();
			localRateLine2.CopyTo(globalRateLine2);

			CombineAssertions(() =>
			{
				AssertEquals("Should map to gobal charge code", globalChargeCode.PK, globalRateLine1.TL_AC);
				AssertEquals(FlatCalculator.Code, globalRateLine1.TL_RateCalculator);

				AssertEquals("Should not keep global charge code which cannot be mapped", localChargeCode.PK, globalRateLine2.TL_AC);
				AssertEquals(FlatCalculator.Code, globalRateLine2.TL_RateCalculator);
			});
		}

		public void TestGlobalRateLineCopyToLocalRateLine()
		{
			AccChargeCode localChargeCode, localChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out localChargeCode, out localChargeCodeLinked, out globalChargeCode);

			var unmappedGlobalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			unmappedGlobalChargeCode.AC_GC = ZGuid.Empty;
			unmappedGlobalChargeCode.AC_RateCalculator = FlatCalculator.Code;

			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var globalRateLine1 = globalRateEntry.AddRateLine(globalChargeCode, FlatCalculator.Code);
			var globalRateLine2 = globalRateEntry.AddRateLine(unmappedGlobalChargeCode, FlatCalculator.Code);

			var localClientRate = Helper.NewClientRate(client);
			var localRateEntry = localClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var localRateLine1 = localRateEntry.RateLines.AddNew();
			globalRateLine1.CopyTo(localRateLine1);

			var localRateLine2 = localRateEntry.RateLines.AddNew();
			globalRateLine2.CopyTo(localRateLine2);

			CombineAssertions(() =>
			{
				AssertEquals("Should map to local charge code", localChargeCodeLinked.PK, localRateLine1.TL_AC);
				AssertEquals(FlatCalculator.Code, localRateLine1.TL_RateCalculator);

				AssertEquals("Should not keep global charge code which cannot be mapped", unmappedGlobalChargeCode.PK, localRateLine2.TL_AC);
				AssertEquals(FlatCalculator.Code, localRateLine2.TL_RateCalculator);
			});
		}

		public void TestCloneWithConversionFactor()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "TAAA";
			chargeCode.AC_RateCalculator = FlatCalculator.Code;

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			line.TL_RateCalculator = CartageCalculator.Code;
			line.ConversionFactor = new ConversionFactor(166.67m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var clonedLine = line.Clone(rateEntry.RateLines);
			AssertEquals(CartageCalculator.Code, clonedLine.TL_RateCalculator);
			AssertEquals(line.ConversionFactor, clonedLine.ConversionFactor);
		}

		public void TestCloneWithOverrideConversionFactor()
		{
			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("AIR");
			var line = entry.RateLines.AddNew();
			line.ConversionFactor = new ConversionFactor(166.67m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var clonedLine = line.Clone(entry.RateLines);
			AssertEquals(line.ConversionFactor, clonedLine.ConversionFactor);
		}

		public void TestCloneWithIDependentCalculator()
		{
			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("AIR");
			var line = entry.AddRateLine(Helper.ChargeCodes["FRT"].AC_Code, PercentageCalculator.Code);
			var applyToItem = line.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem.TM_AC = Helper.ChargeCodes["BAF"].PK;

			var clonedLine = line.Clone(entry.RateLines);
			Assert("Should not have row errors", !clonedLine.HasRowErrors);
		}

		#endregion

		#region MayGSTBeApplicable

		public void TestMayGSTBeApplicable()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TEST";
			Factory.Save();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("ORG", "ALL", "", "").AddRateLine("TEST", FlatCalculator.Code);

			Assert(!line.MayGSTBeApplicable("FOB"));

			chargeCode.AC_AT_GSTRate = CreateTaxRate(10).PK;
			chargeCode.ClearGSTRateCacheForTesting();
			Factory.Save();
			Assert(line.MayGSTBeApplicable("FOB"));

			CreateTaxOverride(chargeCode, CreateTaxRate(0), "REV", "ALL", "FOB", "SHP");
			chargeCode.ClearGSTRateCacheForTesting();
			Factory.Save();
			Assert(!line.MayGSTBeApplicable("FOB"));

			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			chargeCode.ClearGSTRateCacheForTesting();
			Factory.Save();
			Assert(line.MayGSTBeApplicable("FOB"));
			CreateTaxOverride(chargeCode, CreateTaxRate(0), "REV", "ALL", "FOB", "BRK");
			chargeCode.ClearGSTRateCacheForTesting();
			Factory.Save();
			Assert(!line.MayGSTBeApplicable("FOB"));

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var costLine = costing.AddRateEntry("ORG", "ALL", "", "").AddRateLine("TEST", FlatCalculator.Code);
			Assert(costLine.MayGSTBeApplicable("FOB"));
			CreateTaxOverride(chargeCode, CreateTaxRate(0), "COS", "ALL", "FOB", "BRK");
			chargeCode.ClearGSTRateCacheForTesting();
			Factory.Save();
			Assert(!costLine.MayGSTBeApplicable("FOB"));

			GlbCompany.CurrentCompany.SetCountry("AU");

			line.Parent.TI_OriginLRC = "AUSYD";
			Assert(line.MayGSTBeApplicable("EXW"));
			CreateTaxOverride(chargeCode, CreateTaxRate(0), "REV", "EXP", "EXW", "BRK");
			chargeCode.ClearGSTRateCacheForTesting();
			Factory.Save();
			Assert(!line.MayGSTBeApplicable("EXW"));

			line.Parent.TI_OriginLRC = "USLAX";
			line.Parent.TI_DestinationLRC = "AUMEL";
			Assert(line.MayGSTBeApplicable("EXW"));
			CreateTaxOverride(chargeCode, CreateTaxRate(0), "REV", "IMP", "EXW", "BRK");
			chargeCode.ClearGSTRateCacheForTesting();
			Factory.Save();
			Assert(!line.MayGSTBeApplicable("EXW"));
		}

		AccTaxRate CreateTaxRate(ZInt rate)
		{
			return Helper.ChargeCodes.CreateTaxRate(rate);
		}

		AccChargeTaxOverride CreateTaxOverride(AccChargeCode chargeCode, AccTaxRate taxRate, ZString costSell, ZString direction, ZString incoTerm, ZString jobType)
		{
			var @override = chargeCode.TaxOverrides.AddNew();
			@override.AO_AT = taxRate.PK;
			@override.AO_CostSellAll = costSell;
			@override.AO_Direction = direction;
			@override.AO_IncoTerm = incoTerm;
			@override.AO_JobType = jobType;
			@override.AO_Origin = "ALL";
			@override.AO_Destination = "ALL";
			@override.AO_TaxRegCntryOrGroup = "ALL";

			return @override;
		}

		#endregion

		#region SetReadOnlyExcludinChargeCode

		public void TestSetReadOnlyExcludingChargeCode()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			line.OverrideChargeDescription = true;

			Assert(!line.TL_ACInfo.ReadOnly);
			Assert(!line.TL_RateCalculatorInfo.ReadOnly);
			Assert(!line.TL_RateDescInfo.ReadOnly);
			Assert(!line.TL_RateDescLocalInfo.ReadOnly);

			line.SetReadOnlyExcludingChargeCode(true);
			Assert(!line.TL_ACInfo.ReadOnly);
			Assert(line.TL_RateCalculatorInfo.ReadOnly);
			Assert(line.TL_RateDescInfo.ReadOnly);
			Assert(line.TL_RateDescLocalInfo.ReadOnly);

			line.SetReadOnlyExcludingChargeCode(false);
			Assert(!line.TL_ACInfo.ReadOnly);
			Assert(!line.TL_RateCalculatorInfo.ReadOnly);
			Assert(!line.TL_RateDescInfo.ReadOnly);
			Assert(!line.TL_RateDescLocalInfo.ReadOnly);
		}

		public void TestSetReadOnlyExcludingViewAgentRates()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);

			Assert(!line.ViewAgentRatesInfo.ReadOnly);
			Assert(!line.TL_ACInfo.ReadOnly);

			line.SetReadOnlyExcludingViewAgentRates(true);
			Assert(!line.ViewAgentRatesInfo.ReadOnly);
			Assert(line.TL_ACInfo.ReadOnly);

			line.SetReadOnlyExcludingViewAgentRates(false);
			Assert(!line.ViewAgentRatesInfo.ReadOnly);
			Assert(!line.TL_ACInfo.ReadOnly);
		}

		#endregion

		#region Remove With DataRefreshBus

		public void TestRemoveWithDataRefreshBus()
		{
			var rate = Factory.New<ClientRate>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry = rate.AddRateEntry("AIR");
			entry.RateLines[0].InitializeCalculator();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var entry2 = factory2.Load<RateEntry>(entry.PK);

			AssertEquals(1, entry2.RateLines.Count);
			AssertEquals(7, entry2.RateLines[0].Calculator.RateLineItems.Count);

			entry.RateLines[0].RateLineItems.RemoveAndDelete(entry.RateLines[0].RateLineItems[entry.RateLines[0].RateLineItems.Count - 1]);
			entry.RateLines.RemoveAndDelete(entry.RateLines[0]);

			Factory.Save();
		}

		#endregion

		#region UsesCalculator
		public void TestUsesCalculator()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", PercentageCalculator.Code);
			Assert(line.Uses(CalculatorType.Percentage));

			line.TL_RateCalculator = CartageCalculator.Code;

			line.TL_RateCalculator = DisbursementInterestCalculator.Code;
			Assert(line.Uses(CalculatorType.DisbursementInterest));
		}

		#endregion

		#region TL_ConversionFactorString

		public void TestTL_ConversionFactorString_IsDefaultedBlankForLoadingMeter()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.LM);

			AssertEquals(ConversionFactor.Empty, rateLine.ConversionFactor);
		}

		public void TestWhenCreateNewRateLine_ConversionFactorString_ShouldBeBlank()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			AssertEquals(ConversionFactor.Empty, rateLine.ConversionFactor);
		}

		public void TestTL_ConversionFactor()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("ORG", "LCL", "AU", "").AddRateLine("OCART", CartageCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<CartageCalculator>().PerUnit = 1m;
			rateLine.ConversionFactor = new ConversionFactor(333m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			AssertEquals(333m, (ZDecimal)rateLine.TL_ConversionFactorInfo.Value);
			AssertEquals(Constants.Weight.Kilograms, rateLine.TL_FactorNumeratorInfo.Value.ToString());
			AssertEquals(Constants.Volume.CubicMetres, rateLine.TL_FactorDenominatorInfo.Value.ToString());

			rateLine.ConversionFactor = ConversionFactor.Standard.Metric.Air;
			AssertEquals(6000m, (ZDecimal)rateLine.TL_ConversionFactorInfo.Value);
			AssertEquals(Constants.Volume.CubicCentimeters, rateLine.TL_FactorNumeratorInfo.Value.ToString());
			AssertEquals(Constants.Weight.Kilograms, rateLine.TL_FactorDenominatorInfo.Value.ToString());
		}

		#endregion

		#region ConversionFactor

		public void TestConversionFactorGetter_ReturnValueFromConversionFactorForBinding()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.ConversionFactorForBinding.ConversionFactor = new ConversionFactor(100m, "KG", "M3");

			AssertEquals("ConversionFactor", new ConversionFactor(100m, "KG", "M3"), rateLine.ConversionFactor);
		}

		public void TestConversionFactorSetter_UpdateConversionFactorForBinding()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.ConversionFactor = new ConversionFactor(100m, "KG", "M3");

			AssertEquals("ConversionFactorForBinding.ConversionFactor", new ConversionFactor(100m, "KG", "M3"), rateLine.ConversionFactorForBinding.ConversionFactor);
		}

		public void TestFactorySave_PersistConversionFactorFields()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var header = Helper.NewCosting(org);
			var rateEntry = header.AIRRateEntriesForBinding.AddNew();
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery()).PK;
			rateLine.ConversionFactor = new ConversionFactor(100m, "KG", "M3");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newRateLine = newFactory.Load<RateLine>(rateLine.PK);

			AssertEquals("ConvertionFactor", new ConversionFactor(100m, "KG", "M3"), newRateLine.ConversionFactor);
		}

		public void TestFactoryLoad_SetConversionFactoryReadonlyStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var header = Helper.NewCosting(org);
			var rateEntry = header.AIRRateEntriesForBinding.AddNew();
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery()).PK;
			rateLine.TL_RateCalculator = FlatCalculator.Code;
			rateLine.TL_WeightVolume = ZString.Empty;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newRateLine = newFactory.Load<RateLine>(rateLine.PK);

			AssertEquals("Is Conversion Factor readonly", true, newRateLine.ConversionFactorForBinding.ReadOnly);
		}

		public void TestLoad_ConversionFactorChanged_DoesNoSetHasChanges()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 10m);
			var rateLine = rateEntry.RateLines[0];
			rateLine.ConversionFactor = new ConversionFactor(100m, "KG", "M3");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newRateLine = newFactory.Load<RateLine>(rateLine.PK);
			AssertEquals("Load does not set HasChanges", false, newRateLine.HasChanges);
		}

		#endregion

		#region ConversionFactorForBindingReadOnly

		public void TestConversionFactorForBindingReadOnly()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_RateCalculator = UnitCalculator.Code;

			rateLine.UseOnlyActualWeightMeasure = true;
			AssertEquals(true, rateLine.ConversionFactorForBinding.ReadOnly);

			rateLine.UseOnlyActualWeightMeasure = false;
			AssertEquals(false, rateLine.ConversionFactorForBinding.ReadOnly);
		}

		#endregion

		#region TL_IsOnPallets

		public void TestTL_IsOnPalletsReadOnly()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			Assert(rateLine.TL_IsOnPalletsInfo.ReadOnly);

			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = RatingConstants.Units.CN;
			Assert(!rateLine.TL_IsOnPalletsInfo.ReadOnly);

			rateLine.TL_WeightVolume = RatingConstants.Units.KG;
			Assert(rateLine.TL_IsOnPalletsInfo.ReadOnly);
		}

		#endregion

		#region TL_OP_ProductNumber

		public void TestTL_OP_ProductNumberReadOnly()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			AssertEquals(false, rateLine.TL_OP_ProductNumberInfo.ReadOnly);

			rateLine.TL_RateCalculator = WarehouseLocationTypeCalculator.Code;
			AssertEquals(true, rateLine.TL_OP_ProductNumberInfo.ReadOnly);

			rateLine.TL_RateCalculator = UnitCalculator.Code;
			AssertEquals(false, rateLine.TL_OP_ProductNumberInfo.ReadOnly);

			rateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertEquals(true, rateLine.TL_OP_ProductNumberInfo.ReadOnly);
		}

		public void TestTL_OP_ProductNumberClearedOnCalcChange()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			rateLine.TL_OP_ProductNumber = ZGuid.NewZGuid();

			rateLine.TL_RateCalculator = UnitCalculator.Code;
			AssertEquals(false, rateLine.TL_OP_ProductNumber.IsEmpty);

			rateLine.TL_RateCalculator = WarehouseLocationTypeCalculator.Code;
			AssertEquals(true, rateLine.TL_OP_ProductNumber.IsEmpty);
		}

		public void TestTL_OP_ProductNumberRelatedBusinessObjectIsProductNumber()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			string relatedBizObjName = RelatedBusinessObjectAttribute.GetRelatedBizObjName(rateLine.TL_OP_ProductNumberInfo.PropertyDescriptor);

			AssertEquals("ProductNumber", relatedBizObjName);
		}

		#endregion

		#region TL_RateDesc

		public void TestTL_RateDesc()
		{
			var chargeCode = Helper.ChargeCodes.New("FDDD", "Freight Charge", UnitCalculator.Code);

			var rate = Factory.New<ClientRate>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry = rate.AddRateEntry("FCL");
			var line = entry.RateLines[0];

			line.TL_AC = chargeCode.PK;
			AssertEquals("Freight Charge", line.TL_RateDesc);
			AssertEquals("Freight Charge", line.TL_RateDescLocal);

			chargeCode.AC_Desc = "Freight Sub Charge";
			AssertEquals("Freight Sub Charge", line.TL_RateDesc);
			AssertEquals("Freight Sub Charge", line.TL_RateDescLocal);

			line.OverrideChargeDescription = true;
			AssertEquals("Freight Sub Charge", line.TL_RateDesc);
			AssertEquals("Freight Sub Charge", line.TL_RateDescLocal);

			line.TL_RateDesc = "Freight Charge (Discounted)";
			AssertEquals("Freight Charge (Discounted)", line.TL_RateDesc);

			line.TL_RateDescLocal = "Freight Charge (Discounted) Local";
			AssertEquals("Freight Charge (Discounted) Local", line.TL_RateDescLocal);

			chargeCode.AC_Desc = "Freight Sub Charge (FCL)";
			AssertEquals("Freight Charge (Discounted)", line.TL_RateDesc);
			AssertEquals("Freight Charge (Discounted) Local", line.TL_RateDescLocal);

			Factory.Save();
			line = new BusinessObjectFactory().Load<RateLine>(line.PK);
			AssertEquals("Freight Charge (Discounted)", line.TL_RateDesc);
			AssertEquals("Freight Charge (Discounted) Local", line.TL_RateDescLocal);

			line.OverrideChargeDescription = false;
			AssertEquals("Freight Sub Charge (FCL)", line.TL_RateDesc);
			AssertEquals("Freight Sub Charge (FCL)", line.TL_RateDescLocal);

			line.Factory.Save();
			line = new BusinessObjectFactory().Load<RateLine>(line.PK);
			AssertEquals("Freight Sub Charge (FCL)", line.TL_RateDesc);
			AssertEquals("Freight Sub Charge (FCL)", line.TL_RateDescLocal);

			chargeCode = line.Factory.Load<AccChargeCode>(chargeCode.PK);
			chargeCode.AC_Desc = "Freight Charge";
			AssertEquals("Freight Charge", line.TL_RateDesc);
			AssertEquals("Freight Charge", line.TL_RateDescLocal);
		}

		#endregion

		#region Job Level

		public void TestJobLevelFlag()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			var testRateLine = rateEntry.RateLines.AddNew();
			AssertEquals(false, testRateLine.TL_IsWhsJobLevelChargeInfo.ReadOnly);

			testRateLine.TL_RateCalculator = MinimumCalculator.Code;
			AssertEquals(true, testRateLine.TL_IsWhsJobLevelChargeInfo.ReadOnly);

			testRateLine.TL_RateCalculator = UnitCalculator.Code;
			AssertEquals(false, testRateLine.TL_IsWhsJobLevelChargeInfo.ReadOnly);

			testRateLine.TL_IsWhsJobLevelCharge = true;
			testRateLine.TL_RateCalculator = MinimumCalculator.Code;
			AssertEquals(false, testRateLine.TL_IsWhsJobLevelCharge);
			AssertEquals(true, testRateLine.TL_IsWhsJobLevelChargeInfo.ReadOnly);
		}

		#endregion

		#region TL_BreakWeightVolume

		public void TestIsBreakWeightVolumeAvailable()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			AssertEquals(false, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_RateCalculator = CombinedCalculator.Code;
			AssertEquals(false, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_WeightVolume = RatingConstants.Units.CN;
			AssertEquals(true, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_RateCalculator = CartageCalculator.Code;
			AssertEquals(true, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_WeightVolume = RatingConstants.Units.KG;
			AssertEquals(true, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_WeightVolume = QuantityUnit.KM;
			AssertEquals("Distance chargeable units cannot break units", false, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_RateCalculator = CartageZoneDistanceCalculator.Code;
			AssertEquals(false, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_WeightVolume = RatingConstants.Units.CN;
			AssertEquals(true, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_WeightVolume = RatingConstants.Units.KG;
			AssertEquals(true, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_RateCalculator = TimeCalculator.Code;
			AssertEquals(true, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_RateCalculator = EquipmentHireCalculator.Code;
			AssertEquals(true, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_RateCalculator = UnitCalculator.Code;
			AssertEquals(false, rateLine.IsBreakWeightVolumeAvailable);

			rateLine.TL_RateCalculator = HighestRateCalculator.Code;
			AssertEquals(true, rateLine.IsBreakWeightVolumeAvailable);
		}

		#endregion

		#region UseOnlyActualWeightMeasure

		public void TestUseOnlyActualWeightMeasure()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();

			line.TL_ActualPercentage = 100;
			AssertEquals("UseOnlyActualWeightMeasure changes if TL_ActualPercentage changes", true, line.UseOnlyActualWeightMeasure);

			line.TL_ActualPercentage = 0;
			AssertEquals("UseOnlyActualWeightMeasure changes if TL_ActualPercentage changes", false, line.UseOnlyActualWeightMeasure);

			line.TL_ActualPercentage = 50;
			AssertEquals("UseOnlyActualWeightMeasure changes if TL_ActualPercentage changes", false, line.UseOnlyActualWeightMeasure);

			line.TL_ActualPercentage = 100;
			AssertEquals("UseOnlyActualWeightMeasure changes if TL_ActualPercentage changes", true, line.UseOnlyActualWeightMeasure);
		}

		#endregion

		#region TL_ActualPercentage

		public void TestTL_ActualPercentage()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();

			line.UseOnlyActualWeightMeasure = true;
			AssertEquals("Actual Percentage changes if UseOnlyActualWeightMeasure changes", (ZByte)100, line.TL_ActualPercentage);

			line.UseOnlyActualWeightMeasure = false;
			AssertEquals("Actual Percentage changes if UseOnlyActualWeightMeasure changes", (ZByte)0, line.TL_ActualPercentage);

			line.UseOnlyActualWeightMeasure = true;
			AssertEquals("Actual Percentage changes if UseOnlyActualWeightMeasure changes", (ZByte)100, line.TL_ActualPercentage);

			line.UseOnlyActualWeightMeasure = false;
			AssertEquals("Actual Percentage changes if UseOnlyActualWeightMeasure changes", (ZByte)0, line.TL_ActualPercentage);
		}

		#endregion

		#region TestTL_AC_IsSetingDefaultActualFlag

		public void TestTL_AC_IsSetingDefaultActualFlag()
		{
			var warehouseStorageChargeCode = Helper.ChargeCodes.New("WWHSSTO", "Warehouse Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage, "");
			var warehouseReceiveHandlingChargeCode = Helper.ChargeCodes.New("WRECHAN", "Warehouse Receive Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, "");
			var warehouseReceiveStorageChargeCode = Helper.ChargeCodes.New("WRECSTO", "Warehouse Receive Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			AssertEquals("Precondition:", false, rateLine.UseOnlyActualWeightMeasure);

			rateLine.TL_AC = warehouseStorageChargeCode.PK;
			AssertEquals("Charge Code with Warehouse Storage group should default Actuals Flag to True.", true, rateLine.UseOnlyActualWeightMeasure);

			rateLine.TL_AC = warehouseReceiveHandlingChargeCode.PK;
			AssertEquals("Using Charge Code that is not Warehouse Storage should not default Actuals Flag to false.", true, rateLine.UseOnlyActualWeightMeasure);
			rateLine.TL_AC = warehouseStorageChargeCode.PK; // clear up
			rateLine.UseOnlyActualWeightMeasure = false; // clear up

			rateLine.TL_AC = warehouseReceiveHandlingChargeCode.PK;
			AssertEquals("Other Charge Codes should not change.", false, rateLine.UseOnlyActualWeightMeasure);

			rateLine.TL_AC = warehouseReceiveStorageChargeCode.PK;
			AssertEquals("Charge Code with Warehouse Storage SUB group should NOT default Actuals Flag.", false, rateLine.UseOnlyActualWeightMeasure);
		}

		#endregion

		#region TestDefaultRounding

		public void TestDefaultRounding()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();

			AssertEquals(RatingRoundingTypes.DefaultFromRegistry, rateLine.TL_Rounding);
			AssertEquals(false, rateLine.OverrideChargeDescription);
		}

		#endregion

		#region Test Trade Lane Charge Notes

		public void TestTradeLaneChargeNotes_Concurrency_ChargeInformationNoteText() => TestTradeLaneChargeNotes_Concurrency((RateLine rateLine, string value) => rateLine.ChargeInformationNoteText = value);

		public void TestTradeLaneChargeNotes_Concurrency_ChargeInternalNoteText() => TestTradeLaneChargeNotes_Concurrency((RateLine rateLine, string value) => rateLine.ChargeInternalNoteText = value);

		void TestTradeLaneChargeNotes_Concurrency(Action<RateLine, string> setRateLineNote)
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			var rateLine = Helper.AddRateLineWithFlatCalculatorToRateEntry(rateEntry, "FRT", 100m);
			setRateLineNote(rateLine, "Note 1");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedRateLine = newFactory.Load<RateLine>(rateLine.PK);

			setRateLineNote(rateLine, "");
			setRateLineNote(rateLine, "Note 2");

			setRateLineNote(loadedRateLine, "");
			setRateLineNote(loadedRateLine, "Note 3");
			newFactory.Save();

			AssertNoExceptionThrown("GIVEN note is modified on other factory WHEN updating note THEN should not throw 'Sequence contains more than one element' exception", () =>
			{
				setRateLineNote(rateLine, "");
			});
		}

		public void TestTradeLaneChargeNotes()
		{
			ZString expected1 = "Some Text 1";
			ZString expected2 = "Some Text 2";

			var header = Factory.NewWithValidTestData<ClientRate>();
			var entry = Factory.NewWithValidTestData<RateEntry>();
			entry.TI_TH = header.PK;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;

			// Trade Lane Charge Information Note
			var description = PredefinedNoteTypes.Instance.TradeLaneChargeInformation.Description;

			AssertEquals(0, line.Notes.FindByDescription(description).Length);
			AssertEquals(ZString.Empty, line.ChargeInformationNoteText);

			line.ChargeInformationNoteText = expected1;
			AssertEquals(expected1, line.Notes.FindByDescription(description)[0].ST_NoteText);
			AssertEquals(expected1, line.ChargeInformationNoteText);

			line.ChargeInformationNoteText = expected2;
			AssertEquals(expected2, line.Notes.FindByDescription(description)[0].ST_NoteText);
			AssertEquals(expected2, line.ChargeInformationNoteText);

			line.ChargeInformationNoteText = ZString.Empty;
			AssertEquals(0, line.Notes.FindByDescription(description).Length);
			AssertEquals(ZString.Empty, line.ChargeInformationNoteText);

			// Trade Lane Charge Internal Note
			description = PredefinedNoteTypes.Instance.TradeLaneChargeInternalNote.Description;

			AssertEquals(0, line.Notes.FindByDescription(description).Length);
			AssertEquals(ZString.Empty, line.ChargeInternalNoteText);

			line.ChargeInternalNoteText = expected1;
			AssertEquals(expected1, line.Notes.FindByDescription(description)[0].ST_NoteText);
			AssertEquals(expected1, line.ChargeInternalNoteText);

			line.ChargeInternalNoteText = expected2;
			AssertEquals(expected2, line.Notes.FindByDescription(description)[0].ST_NoteText);
			AssertEquals(expected2, line.ChargeInternalNoteText);

			line.ChargeInternalNoteText = ZString.Empty;
			AssertEquals(0, line.Notes.FindByDescription(description).Length);
			AssertEquals(ZString.Empty, line.ChargeInternalNoteText);
		}

		#endregion

		#region Test Universal Charge Code

		public void TestUniversalChargeCode()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var mapping = chargeCode.UniversalChargeCodeMappingsCollection.AddNew();
			mapping.AUP_Code = "BAF";

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;

			AssertEquals(line.UniversalChargeCodes, mapping.AUP_Code);

			line = entry.RateLines.AddNew();
			AssertEquals(line.UniversalChargeCodes, string.Empty);
		}

		#endregion

		#region Test Carrier Charge Code

		public void TestCarrierChargeCode()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var mapping = chargeCode.UniversalChargeCodeMappingsCollection.AddNew();
			mapping.AUP_Code = "BAF";

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;

			AssertEquals(line.CarrierChargeCode, string.Empty);
		}

		#endregion

		#region Contract Number

		public void TestContractNumber()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			AssertEquals("", rateLine.ContractNumber);

			rateEntry.TI_ContractNumber = "C00485203";
			AssertEquals("C00485203", rateLine.ContractNumber);
		}

		#endregion

		#region TACT Rate

		public void TestIsTACTRate()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			AssertEquals("Default value", false, rateEntry.TI_IsTact);
			AssertEquals("Should be no items", 0, rateLine.RateLineItems.Count);

			rateEntry.TI_IsTact = true;
			AssertEquals(1, rateEntry.RateLines.Count);
			AssertEquals(0, rateLine.RateLineItems.Count);

			rateEntry.TI_IsTact = false;
			AssertEquals(1, rateEntry.RateLines.Count);
			AssertEquals("RateLineItems should not be affected", 0, rateLine.RateLineItems.Count);
		}

		#endregion

		#region JobServiceSpotRateDescription

		public void TestJobServiceSpotRateDescription()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "AU");
			var line = entry.AddRateLine("ODOC", FlatCalculator.Code);
			line.SpotRateDescription = "Import Demurrage";

			AssertEquals("Import Demurrage", line.SpotRateDescription);
		}

		#endregion

		#region Fees And Charges

		public void TestTL_FeeChargeTypeSetsLevel()
		{
			var tariff = Factory.New<CompanyTariff>();
			var rateLine = tariff.AddRateEntry("FCL").RateLines.AddNew();

			Assert("Pre-condition", rateLine.TL_FeeChargeType.IsEmpty);
			Assert("Expected to be read only", rateLine.TL_FeeChargeLevelInfo.ReadOnly);

			rateLine.TL_FeeChargeType = ServiceTypeFromRegistry.Code;

			Assert("Expected not to be read only as type is entered", !rateLine.TL_FeeChargeLevelInfo.ReadOnly);

			rateLine.TL_FeeChargeLevel = "STD";

			AssertEquals("Should have set a level", "STD", rateLine.TL_FeeChargeLevel);

			rateLine.TL_FeeChargeType = "";

			Assert("Expected to be empty as the type has changed", rateLine.TL_FeeChargeLevel.IsEmpty);
			Assert("Expected to be read only as there is no type entered", rateLine.TL_FeeChargeLevelInfo.ReadOnly);

			rateLine.TL_FeeChargeType = ServiceTypeFromRegistry.Code;
			rateLine.TL_FeeChargeLevel = "STD";

			Assert("Should have set a level", !rateLine.TL_FeeChargeLevel.IsEmpty);
		}

		public void TestGettingFeeChargeType()
		{
			var tariff = Factory.New<CompanyTariff>();
			var rateLine = tariff.AddRateEntry("FCL").RateLines.AddNew();
			rateLine.TL_CompanyTariffLevel = 1;
			rateLine.TL_FeeChargeType = ServiceTypeFromRegistry.Code;

			AssertEquals(ServiceTypeFromRegistry.Code, rateLine.GetFeeChargeType().Code);
			AssertEquals(ServiceTypeFromRegistry.Description, rateLine.GetFeeChargeType().Description);

			rateLine.TL_FeeChargeType = "BAD";

			AssertNull(rateLine.GetFeeChargeType());
		}

		public void TestSettingFeeChargeTypeDisablesTariffLevelDiscounting()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			tariff1.TH_GlobalRateLevel = 1;
			var entry1 = tariff1.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			entry1.RateLines.RemoveAndDeleteAll();

			var line1 = entry1.AddRateLine("WAR", FlatCalculator.Code);
			line1.GetCalculator<FlatCalculator>().BaseRate = 100m;
			var line2 = entry1.AddRateLine("PSS", FlatCalculator.Code);
			line2.GetCalculator<FlatCalculator>().BaseRate = 100m;
			line2.TL_FeeChargeType = ServiceTypeFromRegistry.Code;
			line2.TL_FeeChargeLevel = "STD";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var tariff2 = newFactory.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = 2;
			var entry2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0];

			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 20M);

			var entry2Line1Item = entry2.RateLines[0].RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS);
			var entry2Line2Item = entry2.RateLines[1].RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS);

			AssertEquals("Discount reflected in tariff 2 charge", 80M, entry2Line1Item.TM_RelevantValue);
			AssertEquals("Discount not applied as there are fees and charges on this line", 100M, entry2Line2Item.TM_RelevantValue);

			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 50M);

			AssertEquals("Discount reflected in tariff 2 charge", 50M, entry2Line1Item.TM_RelevantValue);
			AssertEquals("Discount not applied as there are fees and charges on this line", 100M, entry2Line2Item.TM_RelevantValue);
		}

		static FeeChargeType ServiceTypeFromRegistry
		{
			get
			{
				var registrySetting = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (registrySetting != null)
				{
					return registrySetting.FeeChargeTypes.FirstOrDefault() as FeeChargeType;
				}

				return null;
			}
		}

		#endregion

		#region Importing through ADAW

		public void TestDuringImporting_RateLineCalculator_NotChangedByChargeCode()
		{
			var chargeCode1 = Helper.ChargeCodes.New("CC1", "CC1", CombinedCalculator.Code);
			var chargeCode2 = Helper.ChargeCodes.New("CC2", "CC2", AgencyCalculator.Code);
			Factory.Save();

			var costing = Factory.New<Costing>();
			var costingSupport = costing as ISupportDataImporting;
			var rateEntry = costing.AddRateEntry("FCL", "FCL", "AU", "DE");

			var rateLine = rateEntry.AddRateLine(chargeCode1.AC_Code, FlatCalculator.Code);
			AssertEquals("RateLine made with specific calculator has it after making", FlatCalculator.Code, rateLine.TL_RateCalculator);

			costingSupport.IsImportingData = false;
			rateLine.TL_AC = chargeCode2.PK;
			AssertEquals("When not importing, changing the charge code changes the calculator to the charge code default", chargeCode2.AC_RateCalculator, rateLine.TL_RateCalculator);

			costingSupport.IsImportingData = true;
			rateLine.TL_AC = chargeCode1.PK;
			AssertEquals("When importing, changing the charge code does not change the calculator", chargeCode2.AC_RateCalculator, rateLine.TL_RateCalculator);
		}

		public void TestDuringImporting_WeightVolumeReset_WhenFlatCalculatorPicked()
		{
			var costing = Factory.New<Costing>();
			var costingSupport = costing as ISupportDataImporting;
			var rateEntry = costing.AddRateEntry("FCL", "FCL", "AU", "DE");
			rateEntry.Unit = Weight.Kilograms;

			var rateLine = rateEntry.RateLines.AddNew();
			AssertEquals(
				"Precondition: A new RateLine has its weight the same as its parent's Unit",
				Weight.Kilograms,
				rateLine.TL_WeightVolume);

			costingSupport.IsImportingData = true;
			rateLine.TL_RateCalculator = FlatCalculator.Code;
			AssertEquals(
				"After setting a calculator that does not need TL_WeightVolume. It resets to empty",
				ZString.Empty,
				rateLine.TL_WeightVolume);
		}

		public void TestDuringImportingFreightRate_ConversionFactorBinding_IsUpdated()
		{
			TestDuringImporting_ConversionFactorBinding_IsUpdated(RatingConstants.RateCategory.FCL);
		}

		public void TestDuringImportingNonFreightRate_ConversionFactorBinding_IsUpdated()
		{
			TestDuringImporting_ConversionFactorBinding_IsUpdated(RatingConstants.RateCategory.ORG);
		}

		void TestDuringImporting_ConversionFactorBinding_IsUpdated(string rateCategory)
		{
			var costing = Factory.New<Costing>();
			var rateEntry = costing.AddRateEntry(rateCategory, "FCL", "AU", "DE");
			var rateLine = rateEntry.RateLines.AddNew();

			using (SupportDataImportingHelper.DataImporting(rateEntry))
			using (SupportDataImportingHelper.DataImporting(rateLine))
			{
	#pragma warning disable CS0618 // Type or member is obsolete
					rateLine.TL_ConversionFactor = new ZDecimal(999);
					rateLine.TL_FactorDenominator = "KG";
					rateLine.TL_FactorNumerator = "M3";
	#pragma warning restore CS0618
			}

			AssertEquals(
				"Setting TL_ConversionFactor when importing correctly updates ConversionFactor.",
				999m,
				rateLine.ConversionFactor.Factor);
			AssertEquals(
				"KG",
				rateLine.ConversionFactor.DenominatorUnit);
			AssertEquals(
				"M3",
				rateLine.ConversionFactor.NumeratorUnit);
		}

		#endregion

		#region IsPivotBreakOverrideApplicable - Pivot Break Override Support

		public void TestIsPivotBreakOverrideApplicable()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var companyTariff = Helper.NewCompanyTariff();
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			//Rating Header
			AssertPivotBreakOverride("not supported for Client Rate, only supported for Costing", clientRate, false, calculatorCode: CombinedCalculator.Code);
			AssertPivotBreakOverride("not supported for Company Tariff, only supported for Costing", companyTariff, false, calculatorCode: CombinedCalculator.Code);

			//CalculatorType
			AssertPivotBreakOverride("supported for CombinedCalculator", costing, true, calculatorCode: CombinedCalculator.Code);
			AssertPivotBreakOverride("supported for CartageCalculator", costing, true, calculatorCode: CartageCalculator.Code);
			AssertPivotBreakOverride("supported for CartageZoneDistanceCalculator", costing, true, calculatorCode: CartageZoneDistanceCalculator.Code);
			AssertPivotBreakOverride("not supported for TimeCalculator", costing, false, calculatorCode: TimeCalculator.Code);
			AssertPivotBreakOverride("not supported for PercentageBreaksCalculator", costing, false, calculatorCode: PercentageBreaksCalculator.Code);

			//Weight Unit
			AssertPivotBreakOverride("not supported as unit is not weight unit", costing, false, unit: Volume.CubicFeet);
			AssertPivotBreakOverride("supported as unit is weight unit", costing, true, unit: Weight.Pounds);

			//Freight Charge Code defined in registry
			AssertPivotBreakOverride("not supported as BAF is not defined in registry as Freight Charge Code", costing, false, chargeCode: "BAF");
			DataRegistry.Instance.FreightChargeCode = Helper.ChargeCodes["BAF"].PK.ToGuid();
			AssertPivotBreakOverride("supported as BAF is defined in registry as Freight Charge Code", costing, true, chargeCode: "BAF");

			void AssertPivotBreakOverride(string message, RatingHeader ratingHeader, bool shouldOverride, string calculatorCode = CombinedCalculator.Code, string unit = "KG", bool addMoreThan2LineItems = false, string chargeCode = "FRT")
			{
				var entry = ratingHeader.AddRateEntry("AIR", "ULD", container: "LD-6");
				var line = entry.AddRateLine(chargeCode, calculatorCode, unit);
				var calc = line.Calculator;
				calc.AddRateLineItem(Calculator.Items.Operator.Minus, 45M, 20M, 200M);
				calc.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 10m, 100m);
				if (addMoreThan2LineItems)
				{
					calc.AddRateLineItem(Calculator.Items.Operator.Plus, 90m, 5m, 50m);
				}
				calc.IsAccumulated = true;

				AssertEquals(message, shouldOverride, line.IsPivotBreakOverrideApplicable());
			}
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestMarkupPercentage_GlobalCosting()
		{
			var line = Helper.NewGlobalCosting(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "USLAX").RateLines[0];
			var percentage = line.DefaultMarkupPercentage();
		}

		public static void AssertNoRelatedEntityIsLoaded(BusinessObjectFactory factory)
		{
			var inMemoryLineItemsCount = factory.Load<RateLineItem>(new ZQuery() { FetchOnlyFromLocalCache = true }).Length;
			AssertEquals(0, inMemoryLineItemsCount);

			var inMemorystmNotesCount = factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, "RateLines") { FetchOnlyFromLocalCache = true }).Length;
			AssertEquals(0, inMemorystmNotesCount);
		}

		public static void AssertRelatedEntitiesAreDeleted(BusinessObjectFactory factory, IEnumerable<ZGuid> lineItemPKs, IEnumerable<ZGuid> stmNotePKs)
		{
			var lineItemsCount = factory.Load<RateLineItem>(new ZQuery(RateLineItemsSchema.PK, lineItemPKs)).Length;
			AssertEquals(0, lineItemsCount);

			var stmNotesCount = factory.Load<StmNote>(new ZQuery(StmNoteSchema.PK, stmNotePKs)).Length;
			AssertEquals(0, stmNotesCount);
		}

		public void TestRateLineMustbePartOfCollection()
		{
			var testObjectFactory = new BusinessObjectFactory();
			var rateEntry = testObjectFactory.New<RateEntry>();
			var line1 = rateEntry.RateLines.AddNew();

			AssertEquals("Rate line should be created via Collection.AddNew()", rateEntry.RateLines.LastOrDefault().PK, line1.PK);
			AssertEquals("No developer exception should be raised", 0, ExceptionReporterTestListener.Instance.Count);

			var line2 = testObjectFactory.New<RateLine>();
			AssertEquals("Developer Exception should be raised when you a create a rateline using Factory.New()", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		#region Calculated RoundingFactor

		public void TestCalcRoundingFactor_DefaultValuesFromRegistryOrDefaultRoundings()
		{
			var clientRate = Helper.NewClientRate(null);
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.AddRateLine("ODOC", FlatCalculator.Code);

			var expectedValues = new Dictionary<ZString, (bool isReadOnly, ZDecimal defaultValue)>
			{
				{ RatingRoundingTypes.DefaultFromRegistry, (true, 0m) },
				{ RatingRoundingTypes.NoRounding, (true, 0m) },
				{ RatingRoundingTypes.Bankers, (true, 1m) },
				{ RatingRoundingTypes.UpToHalf, (true, 0.5m) },
				{ RatingRoundingTypes.UpTo1, (true, 1) },
				{ RatingRoundingTypes.UpTo1IfLessThanOne, (true, 1) },
				{ RatingRoundingTypes.Chargeable, (true, 0m) },
				{ RatingRoundingTypes.Custom, (false, 0) },
			};

			var ratingRoundingTypeList = rateLine.Lookups.Roundings;
			var roundingTypeCodes = ratingRoundingTypeList.GetAllCodes();

			foreach (var roundingType in roundingTypeCodes)
			{
				rateLine.TL_Rounding = roundingType;

				var expectedRounding = expectedValues[roundingType];
				var message = $"With RoundingType={roundingType}, RoundingFactor should be read only";
				AssertEquals(message, expectedRounding.isReadOnly, rateLine.RoundingFactorInfo.ReadOnly);
				message = $"With RoundingType={roundingType}, RoundingFactor should have default value";
				AssertEquals(message, expectedRounding.defaultValue, rateLine.RoundingFactor);
			}

			rateLine.TL_Rounding = RatingRoundingTypes.Custom;
			rateLine.TL_RoundingFactor = 0.3m;
			AssertEquals("TL_RoundingFactor with CUS", 0.3m, rateLine.RoundingFactor);

			rateLine.TL_Rounding = RatingRoundingTypes.Bankers;
			AssertEquals("Despite TL_RoundingFactor being set, calculated value should show default rounding", 1m, rateLine.RoundingFactor);

			var systemRoundings = new DefaultRoundingsCollection();
			DefaultRoundings systemRounding = systemRoundings.AddNew();
			systemRounding.Code = RatingConstants.RateCategory.ORG;
			systemRounding.RoundingType = RatingRoundingTypes.UpToHalf;
			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemRoundings))
			{
				rateLine.TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;
				AssertEquals("Rounding Factor field should show default value from registry", expectedValues[RatingRoundingTypes.UpToHalf].defaultValue, rateLine.RoundingFactor);
			}

			systemRounding.RoundingType = RatingRoundingTypes.Custom;
			systemRounding.RoundingFactor = 0.75m;
			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemRoundings))
			{
				AssertEquals("Rounding Factor field should show custom value from registry", 0.75m, rateLine.RoundingFactor);
			}
		}

		public void TestOnFactorySaving_DoesNotInitializeCalculator()
		{
			var startDate = ZDate.Today;
			var clientRate = Helper.NewClientRate(null);
			var entry = clientRate.AddRateEntry("ORG");
			entry.TI_RateStartDate = startDate;
			entry.TI_RateEndDate = startDate.AddDays(30);
			entry.AddRateLine("ODOC", FlatCalculator.Code);

			var line2 = entry.AddRateLine("FRT", DisbursementInterestCalculator.Code, "", Constants.CurrencyCodes.Australia);
			line2.GetCalculator<DisbursementInterestCalculator>().AddApplyToItem(CalculatorConstants.Text.AllCharges);
			line2.GetCalculator<DisbursementInterestCalculator>().Uplift = 1;
			line2.GetCalculator<DisbursementInterestCalculator>().AdjustmentDays = 45;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var rateInNewFactory = factory2.Load<ClientRate>(clientRate.PK);
			var entryInNewFactory = rateInNewFactory.AllEntries.First();
			entryInNewFactory.TI_RateStartDate = startDate.AddDays(10);
			AssertEquals("PRE: all rate lines are loaded", 2, rateInNewFactory.AllEntries.Select(x => x.RateLines).Sum(x => x.Count));
			factory2.Save();
			AssertEquals("no hits on RateLineItems", 0, factory2.GetTableHitCount(AutoRateLineItems.Schema.TableName));
			var allRateLines = rateInNewFactory.AllEntries.SelectMany(x => x.RateLines.Cast<RateLine>());
			AssertEquals("No calculators initialized", true, allRateLines.All(x => !x.IsCalculatorInitialized));
		}

		#endregion

		#region Property Setter For RelatedRateLine DoesNotThrow

		public void TestTL_AC_SetterForRelatedRateLine_DoesNotThrow()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RelatedRateLines.AddNew();

			AssertNull("Precondition: line.Parent should be null.", line.Parent);
			AssertNoExceptionThrown("TL_AC", () => line.TL_AC = Helper.ChargeCodes["WAR"].PK);
		}

		public void TestTL_IsWhsJobLevelCharge_SetterForRelatedRateLine_DoesNotThrow()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RelatedRateLines.AddNew();

			AssertNull("Precondition: line.Parent should be null.", line.Parent);
			AssertNoExceptionThrown("TL_IsWhsJobLevelCharge", () => line.TL_IsWhsJobLevelCharge = !line.TL_IsWhsJobLevelCharge);
		}

		public void TestTL_RX_NKCurrency_SetterForRelatedRateLine_DoesNotThrow()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RelatedRateLines.AddNew();

			AssertNull("Precondition: line.Parent should be null.", line.Parent);
			AssertNoExceptionThrown("TL_RX_NKCurrency", () => line.TL_RX_NKCurrency = Constants.CurrencyCodes.India);
		}

		public void TestTL_RateCalculator_SetterForRelatedRateLine_DoesNotThrow()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RelatedRateLines.AddNew();

			AssertNull("Precondition: line.Parent should be null.", line.Parent);
			AssertNoExceptionThrown("TL_RateCalculator", () => line.TL_RateCalculator = ExcludeCompanyTariffsCalculator.Code);
		}

		#endregion
	}

	#region BusinessObject Test Case

	[TestedType(typeof(RateLine))]
	public class RateLineBizObjTest : BizObjectRateLineTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<ClientRate>();
			var entry = header.AddRateEntry("AIR");
			var line = entry.RateLines[0];
			line.OverrideChargeDescription = true;
			return line;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<ClientRate>();

			var entry = header.AddRateEntry(RatingConstants.RateCategory.AIR);
			entry.TI_RateStartDate = ZDate.Today.AddMonths(-6);

			return entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
		}
		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.Parent.TI_RateStartDate = ZDate.Today.AddMonths(-6);

			return rateLine;
		}
	}

	#endregion
}
