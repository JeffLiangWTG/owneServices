using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class GlobalRatesIntergrationTest : BaseRatingIntegrationTest
	{
		public void TestGlobalCostingsAreAppliedToJob()
		{
			var localCosting = Helper.NewCosting(TransportProvider1);
			var costEntry = localCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, TransportModes.Air, "AUSYD", "", "ODOC", 15);

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBORG", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var globalCosting = Helper.NewGlobalCosting(TransportProvider1);
			var globalCostingEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.ORG, TransportModes.Air, "AUSYD", "");
			var globalCostingLine = globalCostingEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			globalCostingLine.GetCalculator<UnitCalculator>().PerUnit = 25m;

			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRateWithSingleRateLine(client, RatingConstants.RateCategory.ORG, TransportModes.Air, "AUSYD", "", "ODOC", 35);

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "USLAX", 100m, 0.5m);
			shipment.JS_OH_ExportBroker = TransportProvider1.PK;

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
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
					ChargeCode = "GLBORG",
					JR_OSCostAmt = 2500,
					CostCalculationDescription = "Charge located in TRASPROV1 (Shipment EBM22Q33TU475BXH3P60 --> Export Broker) global cost with the following details:"
				},
			};

			var expectedLogLines = new[] { @"Information: RateLine Found GLBORG-UNT-KG-Global Costing TRASPROV1
Information: RateLine Found ODOC-FLT-Costing TRASPROV1",
				@"	The following costs were found:
	  • GLBORG charge from Global Costing TRASPROV1
	  • ODOC charge from Costing TRASPROV1"
			};

			AutorateAndAssert(expected, shipment, client);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "should contain indication of global rate", expectedLogLines);
		}

		public void TestGlobalCostings_OnConsol()
		{
			TransportProvider1.OH_IsCreditor = true;

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			var globalCosting = Helper.NewGlobalCosting(TransportProvider1);
			var globalCostingEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "");
			globalCostingEntry.RateLines.RemoveAndDeleteAll();
			var globalCostingLine = globalCostingEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			globalCostingLine.GetCalculator<UnitCalculator>().PerUnit = 25m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, ZGuid.Empty, "AUSYD", "SGSIN", 30m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "SGSIN", TransportProvider1, shipment, PaymentType.Prepaid);
			consol.CreditorPK = TransportProvider1.PK;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "GLBFRT",
					E6_OSCostAmount = 750,
				},
			};

			AutoCostAndAssert("JobConsolCosts should be created", null, expectedCosts, consol, false);
		}

		public void TestGlobalCostings_ShouldBeReplacedWithLocalCostingForTheSameCharge()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBORG", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var localChargeCode = GetLocal(globalChargeCode);

			var localCosting = Helper.NewCosting(TransportProvider1);
			var costEntry = localCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, TransportModes.Air, "AUSYD", "", "ODOC", 15m);
			var costLine2 = costEntry.AddRateLine(localChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var globalCosting = Helper.NewGlobalCosting(TransportProvider1);
			var globalCostingEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.ORG, TransportModes.Air, "AUSYD", "");
			var globalCostingLine = globalCostingEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			globalCostingLine.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRateWithSingleRateLine(client, RatingConstants.RateCategory.ORG, TransportModes.Air, "AUSYD", "", "ODOC", 35);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, ZGuid.Empty, "AUSYD", "USLAX", 100m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;
			Factory.Save();

			var expectedMessage = "RateLine Filtered GLBORG-UNT-KG-Global Costing TRASPROV1	reason:	overridden by GLBORG-UNT-KG-Costing TRASPROV1 due to Local Rates overriding Global Rates";
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
					ChargeCode = "GLBORG",
					JR_OSCostAmt = 1000,
				},
			};

			AutorateAndAssert(expected, shipment, client);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain information about filtered Global Cost", expectedMessage);
		}

		public void TestGlobalCostings_ShouldReplaceGlobalChargeCodeWithIntercompanyMappedChargeCode()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";

			TransportProvider1.OH_IsCreditor = true;

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT");
			var globalCosting = Helper.NewGlobalCosting(TransportProvider1);
			var globalCostingEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, "");
			globalCostingEntry.RateLines.RemoveAndDeleteAll();
			var globalCostingLine = globalCostingEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, Weight.Kilograms);
			globalCostingLine.GetCalculator<UnitCalculator>().PerUnit = 25m;

			Factory.Save();

			var intercompanyMappedChargeCode = Helper.ChargeCodes["FRT"];

			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode.AC_Code;

			var globalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = intercompanyMappedChargeCode.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = null;
			using (globalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsPayable;

				var client = Helper.NewOrgHeader();
				var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, origin, destination, 30m);
				var consol = CreateForwardingConsol(TransportModes.Air, origin, destination, TransportProvider1, shipment);
				consol.CreditorPK = TransportProvider1.PK;
				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = intercompanyMappedChargeCode.AC_Code,
						JR_OSCostAmt = 750,
					}
				};

				AutorateAndAssert(expected, shipment, client);

				var overriddenIntercompanyMappedChargeCode = Helper.ChargeCodes["BAF"];

				var globalChargeCodeMapPivotIntercompany2 = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
				globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
				globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_AC] = overriddenIntercompanyMappedChargeCode.PK;
				globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = client.PK;
				using (globalChargeCodeMapPivotIntercompany2.GetValidationSuspender())
				{
					globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsPayable;

					Factory.ClearCachedValue<AccChargeCode>($"{globalChargeCode.PK}|AP");
					Factory.Save();

					expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = overriddenIntercompanyMappedChargeCode.AC_Code,
							JR_OSCostAmt = 750,
						}
					};

					AutorateAndAssert(expected, shipment, client);

					var globalCostingRateLineFromDB = Factory.Load<RateLine>(globalCostingLine.PK);
					AssertEquals("Global Costing Rate Line Charge Code should not be changed", globalCostingRateLineFromDB.ChargeCode.PK, globalChargeCode.PK);
					AssertNotEquals("Global Charge Code Calculator should not be same as Global Costing Rate Line Calculator", globalCostingRateLineFromDB.TL_RateCalculator, globalChargeCode.AC_RateCalculator);
					AssertEquals("Global Costing Rate Line Calculator should be Unit Calcultor as per initial setup in this Test", globalCostingRateLineFromDB.TL_RateCalculator, UnitCalculator.Code);
				}
			}
		}

		public void TestGlobalCostings_DoesNotUseInactiveIntercompanyChargeCodeMapping()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBORG", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			globalChargeCode.Factory.Save();

			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, origin, destination);
			var globalCostingLine = globalRateEntry.AddRateLine(globalChargeCode, FlatCalculator.Code, Weight.Kilograms);
			globalCostingLine.GetCalculator<FlatCalculator>().BaseRate = 75m;
			Factory.Save();

			var intercompanyMappedChargeCode = Helper.ChargeCodes["OAQF"];
			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode.AC_Code;

			var globalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = intercompanyMappedChargeCode.PK;
			using (globalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsReceivable;

				var consignorPK = Helper.NewOrgHeader(1).PK;
				var consigneePK = Helper.NewOrgHeader().PK;
				var shipment = CreateForwardingShipment(TransportModes.Air, consignorPK, consigneePK, origin, destination, 30m);
				shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Today.AddHours(3);
				Factory.Save();

				var expected = new[] { new AssertionCharge { ChargeCode = intercompanyMappedChargeCode.AC_Code } };
				var expectedLogMessage = "Information: ChargeCode Mapping Source Intercompany Charge Code Mappings (GLBORG -> OAQF)";
				AutorateAndAssert(expected, shipment, client, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Expected a message", expectedLogMessage);

				globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_IsActive] = false;
				Factory.ClearCachedValue<AccChargeCode>($"{globalChargeCode.PK}|AR");

				shipment = CreateForwardingShipment(TransportModes.Air, consignorPK, consigneePK, origin, destination, 30m);
				shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Today.AddHours(3);

				Factory.Save();

				expected = new[] { new AssertionCharge { ChargeCode = "GLBORG" } };

				AutorateAndAssert("Should not use intercompany mapping when mapping is inactive", expected, shipment, client, autorateCosts: false);

				var logNote = shipment.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)[0].ST_NoteData.ToUTF8();
				AssertNotContains(expectedLogMessage, logNote);
			}
		}

		public void TestGlobalCostings_GlobalRateEntryDeepCloneToLocalGlobalRateEntry_PrefersIntercompanyMapping()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBCHRG");
			globalChargeCode.Factory.Save();
			var locallyLinkedChargeCode = globalChargeCode.ChildChargeCodes.Single(x => x.AC_GC == Env.CurrentCompanyPK);

			var intercompanyMappedChargeCode = Helper.ChargeCodes["OAQF"];
			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode.AC_Code;

			var globalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = intercompanyMappedChargeCode.PK;
			using (globalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsPayable;
				Factory.Save();

				var globalClientRate = Helper.NewGlobalCosting(null);
				var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
				globalRateEntry.RateLines.RemoveAndDeleteAll();
				globalRateEntry.AddRateLine(globalChargeCode, FlatCalculator.Code);

				var localClientRate = Helper.NewCosting(null);
				var localRateEntry = globalRateEntry.DeepClone(localClientRate.LCLRateEntriesForBinding);

				CombineAssertions(() =>
				{
					var clonedRateLine = localRateEntry.RateLines.Cast<RateLine>().Single();
					AssertEquals("Should prefer intercompany charge code to local", intercompanyMappedChargeCode.PK, clonedRateLine.TL_AC);
				});
			}
		}

		public void TestGlobalCostings_DoesNotUseIntercompanyChargeCodeMappingForAnotherCompanyChargeCode()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBORG", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			globalChargeCode.Factory.Save();

			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, origin, destination);
			var globalCostingLine = globalRateEntry.AddRateLine(globalChargeCode, FlatCalculator.Code);
			globalCostingLine.GetCalculator<FlatCalculator>().BaseRate = 75m;
			Factory.Save();

			var intercompanyMappedChargeCode = Helper.ChargeCodes["OAQF"];
			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode.AC_Code;

			var globalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = intercompanyMappedChargeCode.PK;
			using (globalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsReceivable;

				var consignorPK = Helper.NewOrgHeader(1).PK;
				var consigneePK = Helper.NewOrgHeader().PK;
				var shipment = CreateForwardingShipment(TransportModes.Air, consignorPK, consigneePK, origin, destination, 30m);
				shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Today.AddHours(3);
				Factory.Save();

				var expected = new[] { new AssertionCharge { ChargeCode = intercompanyMappedChargeCode.AC_Code } };
				var expectedLogMessage = "Information: ChargeCode Mapping Source Intercompany Charge Code Mappings (GLBORG -> OAQF)";

				AutorateAndAssert(expected, shipment, client, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Expected a message", expectedLogMessage);

				var anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
				var chargeCodeForAnotherCompany = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCodeForAnotherCompany.AC_Code = "NOPE";
				chargeCodeForAnotherCompany.AC_GC = anotherCompany.PK;
				chargeCodeForAnotherCompany.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
				chargeCodeForAnotherCompany.AC_RateCalculator = FlatCalculator.Code;

				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = chargeCodeForAnotherCompany.PK;
				Factory.ClearCachedValue<AccChargeCode>($"{globalChargeCode.PK}|AR");

				shipment = CreateForwardingShipment(TransportModes.Air, consignorPK, consigneePK, origin, destination, 30m);
				shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Today.AddHours(3);

				Factory.Save();

				expected = new[] { new AssertionCharge { ChargeCode = "GLBORG" } };

				AutorateAndAssert("Should not use intercompany mapping when mapping is inactive", expected, shipment, client, autorateCosts: false);

				var logNote = shipment.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)[0].ST_NoteData.ToUTF8();
				AssertNotContains(expectedLogMessage, logNote);
			}
		}

		public void TestGlobalClientRatesAreAppliedToJob()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBORG", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var client = Helper.NewOrgHeader();
			var localRate = Helper.NewClientRate(client);
			var localRateEntry = localRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, TransportModes.Air, "AUSYD", "", "ODOC", 15);

			var globalRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = globalRate.AddRateEntry(RatingConstants.RateCategory.ORG, TransportModes.Air, "AUSYD", "");
			var globalRateLine = globalRateEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			globalRateLine.GetCalculator<UnitCalculator>().PerUnit = 25m;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "CNSHA", 100m, 0.5m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "CNSHA", TransportProvider1, shipment);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 15,
				},
				new AssertionCharge
				{
					ChargeCode = "GLBORG",
					JR_OSSellAmt = 2500,
				},
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestGlobalClientRates_ShouldBeReplacedWithLocalRateForTheSameCharge()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBORG", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var localChargeCode = GetLocal(globalChargeCode);

			var client = Helper.NewOrgHeader();
			var localRate = Helper.NewClientRate(client);
			var localRateEntry = localRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, TransportModes.Air, "AU", "", "ODOC", 15m);

			var localRateLine = localRateEntry.AddRateLine(localChargeCode, FlatCalculator.Code);
			localRateLine.GetCalculator<FlatCalculator>().BaseRate = 16m;

			var globalRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = globalRate.AddRateEntry(RatingConstants.RateCategory.ORG, TransportModes.Air, "AU", "");
			var globalRateLine = globalRateEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			globalRateLine.GetCalculator<UnitCalculator>().PerUnit = 25m;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "AUSYD", "CNSHA", 100m, 0.5m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "CNSHA", TransportProvider1, shipment);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 15,
				},
				new AssertionCharge
				{
					ChargeCode = "GLBORG",
					JR_OSSellAmt = 16,
				},
			};

			AutorateAndAssert(expected, shipment, client);

			var expectedMessage = "RateLine Filtered GLBORG-UNT-KG-Global Client Rate TESTORG1	reason:	overridden by GLBORG-FLT-Client Rate TESTORG1 due to Local Rates overriding Global Rates";
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Local should override Global", expectedMessage);
		}

		public void TestGlobalClientRates_ShouldReplaceLocalRateForTheSameCharge_WhenRegistryIsEnabled()
		{
			using (RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBORG", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				var localChargeCode = GetLocal(globalChargeCode);

				var client = Helper.NewOrgHeader();
				var localRate = Helper.NewClientRate(client);
				var localRateEntry = localRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, TransportModes.Air, "AU", "", "ODOC", 15m);

				var localRateLine = localRateEntry.AddRateLine(localChargeCode, FlatCalculator.Code);
				localRateLine.GetCalculator<FlatCalculator>().BaseRate = 16m;

				var globalRate = Helper.NewGlobalClientRate(client);
				var globalRateEntry = globalRate.AddRateEntry(RatingConstants.RateCategory.ORG, TransportModes.Air, "AU", "");
				var globalRateLine = globalRateEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
				globalRateLine.GetCalculator<UnitCalculator>().PerUnit = 25m;

				Factory.Save();

				var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "AUSYD", "CNSHA", 100m, 0.5m);
				var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "CNSHA", TransportProvider1, shipment);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSSellAmt = 15,
					},
					new AssertionCharge
					{
						ChargeCode = "GLBORG",
						JR_OSSellAmt = 2500,
					},
				};

				AutorateAndAssert(expected, shipment, client);

				var expectedMessage = "RateLine Filtered GLBORG-FLT-Client Rate TESTORG1	reason:	overridden by GLBORG-UNT-KG-Global Client Rate TESTORG1 due to Global Sell Rates to overriding Local Sell Rates (Global Sell Rates override Local Registry enabled)";
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Registry means we should expect Global", expectedMessage);
			}
		}

		public void TestGlobalTariffsAreAppliedToJob()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			var localChargeCode = GetLocal(globalChargeCode);

			var tariff = Helper.NewGlobalTariff();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 5.1612m;

			tariff.Factory.Save();

			var consignee = Helper.NewOrgHeader(1);
			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, consignee.PK, "USBOS", "AUSYD", 1000m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 5161.2m,
					RevenueCalculationDescription = "GLBFRT: 1000 Kilogram(s) @ AUD 5.1612/KG"
				}
			};

			AutorateAndAssert(expected, shipment, consignee);
		}

		public void TestGlobalTariffs_AreIncludedInCTBCalcForLocalRates()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			var localChargeCode = GetLocal(globalChargeCode);

			var tariff = Helper.NewGlobalTariff();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 5.1612m;

			tariff.Factory.Save();

			var consignee = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(localChargeCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, "", CurrencyCodes.Australia);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 20m;
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, consignee.PK, "USBOS", "AUSYD", 1000m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 6193.44m,
					RevenueCalculationDescription = "GLBFRT: 1000 Kilogram(s) @ AUD 6.1934/KG"
				}
			};

			AutorateAndAssert(expected, shipment, consignee);
		}

		#region Autorating Revenue Priority

		[TestDate(2000, 07, 01)]
		public void TestAutoratingRevenuePriority_RegistryDisabled()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", FlatCalculator.Code);
			var localChargeCode = GetLocal(globalChargeCode);

			var consignee = Helper.NewOrgHeader(1);

			var localClientRate = Helper.NewClientRate(consignee);
			var localClientRateEntry = localClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU", localChargeCode.AC_Code, 10m, description: "Local ClientRate");

			var globalClientRate = Helper.NewGlobalClientRate(consignee);
			var globalClientRateEntry = globalClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU", globalChargeCode.AC_Code, 20m, description: "Global ClientRate");

			var localTariff = Helper.NewCompanyTariff();
			var localTariffEntry = localTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU", localChargeCode.AC_Code, 30m, description: "Local Tariff");
			localTariff.Factory.Save();

			var globalTariff = Helper.NewGlobalTariff();
			globalTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU", globalChargeCode.AC_Code, 40m, description: "Global Tariff");
			globalTariff.Factory.Save();

			using (RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, consignee.PK, "USBOS", "AUSYD", 1000m);
				shipment.JS_E_DEP = ZDateTime.Now;
				shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);
				Factory.Save();
				AutorateAndAssert
				(
					message: "Priority 1: Local ClientRate",
					expected: new[] { new AssertionCharge { JR_OSSellAmt = 10m, RevenueCalculationDescription = "GLBFRT: Base Rate USD 10.00" } },
					shipment,
					consignee,
					autorateCosts: false
				);

				((Job)shipment.Job).Charges.RemoveAndDeleteAll();
				localClientRateEntry.TI_RateStartDate = ZDate.Today.AddMonths(1);
				localClientRateEntry.TI_RateEndDate = ZDate.Today.AddMonths(2);
				Factory.Save();
				AutorateAndAssert
				(
					message: "Priority 2: Global ClientRate",
					expected: new[] { new AssertionCharge { JR_OSSellAmt = 20m, RevenueCalculationDescription = "GLBFRT: Base Rate USD 20.00" } },
					shipment,
					consignee,
					autorateCosts: false
				);

				((Job)shipment.Job).Charges.RemoveAndDeleteAll();
				globalClientRateEntry.TI_RateStartDate = ZDate.Today.AddMonths(1);
				globalClientRateEntry.TI_RateEndDate = ZDate.Today.AddMonths(2);
				Factory.Save();
				AutorateAndAssert
				(
					message: "Priority 3: Local CompanyTariff",
					expected: new[] { new AssertionCharge { JR_OSSellAmt = 30m, RevenueCalculationDescription = "GLBFRT: Base Rate USD 30.00" } },
					shipment,
					consignee,
					autorateCosts: false
				);

				((Job)shipment.Job).Charges.RemoveAndDeleteAll();
				localTariffEntry.TI_RateStartDate = ZDate.Today.AddMonths(1);
				localTariffEntry.TI_RateEndDate = ZDate.Today.AddMonths(2);
				localTariff.Factory.Save();
				AutorateAndAssert
				(
					message: "Priority 4: Global CompanyTariff",
					expected: new[] { new AssertionCharge { JR_OSSellAmt = 40m, RevenueCalculationDescription = "GLBFRT: Base Rate USD 40.00" } },
					shipment,
					consignee,
					autorateCosts: false
				);
			}
		}

		[TestDate(2000, 07, 01)]
		public void TestAutoratingRevenuePriority_RegistryEnabled()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", FlatCalculator.Code);
			var localChargeCode = GetLocal(globalChargeCode);

			var consignee = Helper.NewOrgHeader(1);

			var localClientRate = Helper.NewClientRate(consignee);
			var localClientRateEntry = localClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU", localChargeCode.AC_Code, 10m, description: "Local ClientRate");

			var globalClientRate = Helper.NewGlobalClientRate(consignee);
			var globalClientRateEntry = globalClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU", globalChargeCode.AC_Code, 20m, description: "Global ClientRate");

			var localTariff = Helper.NewCompanyTariff();
			var localTariffEntry = localTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU", localChargeCode.AC_Code, 30m, description: "Local Tariff");
			localTariff.Factory.Save();

			var globalTariff = Helper.NewGlobalTariff();
			var globalTariffEntry = globalTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU", globalChargeCode.AC_Code, 40m, description: "Global Tariff");
			globalTariff.Factory.Save();

			using (RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, consignee.PK, "USBOS", "AUSYD", 1000m);
				shipment.JS_E_DEP = ZDateTime.Now;
				shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);
				Factory.Save();
				AutorateAndAssert
				(
					message: "Priority 1: Global ClientRate",
					expected: new[] { new AssertionCharge { JR_OSSellAmt = 20m, RevenueCalculationDescription = "GLBFRT: Base Rate USD 20.00" } },
					shipment,
					consignee,
					autorateCosts: false
				);

				((Job)shipment.Job).Charges.RemoveAndDeleteAll();
				globalClientRateEntry.TI_RateStartDate = ZDate.Today.AddMonths(1);
				globalClientRateEntry.TI_RateEndDate = ZDate.Today.AddMonths(2);
				Factory.Save();
				AutorateAndAssert
				(
					message: "Priority 2: Local ClientRate",
					expected: new[] { new AssertionCharge { JR_OSSellAmt = 10m, RevenueCalculationDescription = "GLBFRT: Base Rate USD 10.00" } },
					shipment,
					consignee,
					autorateCosts: false
				);

				((Job)shipment.Job).Charges.RemoveAndDeleteAll();
				localClientRateEntry.TI_RateStartDate = ZDate.Today.AddMonths(1);
				localClientRateEntry.TI_RateEndDate = ZDate.Today.AddMonths(2);
				Factory.Save();
				AutorateAndAssert
				(
					message: "Priority 3: Global CompanyTariff",
					expected: new[] { new AssertionCharge { JR_OSSellAmt = 40m, RevenueCalculationDescription = "GLBFRT: Base Rate USD 40.00" } },
					shipment,
					consignee,
					autorateCosts: false
				);

				((Job)shipment.Job).Charges.RemoveAndDeleteAll();
				globalTariffEntry.TI_RateStartDate = ZDate.Today.AddMonths(1);
				globalTariffEntry.TI_RateEndDate = ZDate.Today.AddMonths(2);
				globalTariff.Factory.Save();
				AutorateAndAssert
				(
					message: "Priority 4: Local CompanyTariff",
					expected: new[] { new AssertionCharge { JR_OSSellAmt = 30m, RevenueCalculationDescription = "GLBFRT: Base Rate USD 30.00" } },
					shipment,
					consignee,
					autorateCosts: false
				);
			}
		}

		#endregion

		public void TestGlobalTariffs_ShouldBeReplacedByLocalCompanyTariffWithSameChargeCode()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			var localChargeCode = GetLocal(globalChargeCode);

			var globalTariff = Helper.NewGlobalTariff();
			var tariffEntry = globalTariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "", "AUSYD");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(tariffEntry.Currency);
			var tariffLine = tariffEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 5.1612m;

			var localTariff = globalTariff.Factory.New<CompanyTariff>();
			var localTariffEntry = localTariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "", "AUSYD");
			localTariffEntry.RateLines.RemoveAndDeleteAll();
			var localTariffLine = localTariffEntry.AddRateLine(localChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			localTariffLine.GetCalculator<UnitCalculator>().PerUnit = 8.1612m;

			globalTariff.Factory.Save();

			var consignee = Helper.NewOrgHeader(1);
			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, consignee.PK, "CNCAN", "AUSYD", 1000m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 8161.2m,
					RevenueCalculationDescription = "GLBFRT: 1000 Kilogram(s) @ USD 8.1612/KG"
				}
			};

			AutorateAndAssert(expected, shipment, consignee);

			var expectedMessage = "RateLine Filtered GLBFRT-UNT-KG-Global Base Tariff	reason:	overridden by GLBFRT-UNT-KG-Base Company Tariff due to Local Rates overriding Global Rates";
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Local should override Global", expectedMessage);
		}

		public void TestGlobalTariffs_ShouldReplaceLocalRateForTheSameCharge_WhenRegistryIsEnabled()
		{
			using (RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
				var localChargeCode = GetLocal(globalChargeCode);

				var globalTariff = Helper.NewGlobalTariff();
				var tariffEntry = globalTariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
				tariffEntry.RateLines.RemoveAndDeleteAll();
				var tariffLine = tariffEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
				tariffLine.GetCalculator<UnitCalculator>().PerUnit = 20m;

				var localTariff = globalTariff.Factory.New<CompanyTariff>();
				var localTariffEntry = localTariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
				localTariffEntry.RateLines.RemoveAndDeleteAll();
				var localTariffLine = localTariffEntry.AddRateLine(localChargeCode, UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
				localTariffLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

				globalTariff.Factory.Save();

				var consignee = Helper.NewOrgHeader(1);
				var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, consignee.PK, "AUSYD", "GBSUN", 10m);
				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 200m,
						RevenueCalculationDescription = "GLBFRT: 10 Kilogram(s) @ AUD 20.00/KG"
					}
				};

				AutorateAndAssert(expected, shipment, consignee);

				var expectedMessage = "RateLine Filtered GLBFRT-UNT-KG-Base Company Tariff	reason:	overridden by GLBFRT-UNT-KG-Global Base Tariff due to Global Sell Rates to overriding Local Sell Rates (Global Sell Rates override Local Registry enabled)";
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Registry means we should expect global rate", expectedMessage);
			}
		}

		public void TestGlobalTariffs_ShouldReplaceGlobalChargeCodeWithIntercompanyMappedChargeCode()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			globalChargeCode.Factory.Save();

			var globalTariff = Helper.NewGlobalTariff();
			var tariffEntry = globalTariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "AU");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(tariffEntry.Currency);
			var tariffLine = tariffEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, Weight.Kilograms);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 5.1612m;

			globalTariff.Factory.Save();

			var intercompanyMappedChargeCode = Helper.ChargeCodes["FRT"];

			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode.AC_Code;

			var globalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = intercompanyMappedChargeCode.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = null;
			using (globalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsReceivable;

				var client = Helper.NewOrgHeader(1);
				var consignee = Helper.NewOrgHeader(1);
				var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, consignee.PK, "USBOS", "AUSYD", 1000m);
				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 5161.2m,
						RevenueCalculationDescription = intercompanyMappedChargeCode.AC_Code + ": 1000 Kilogram(s) @ USD 5.1612/KG"
					}
				};

				AutorateAndAssert(expected, shipment, client);
			}
		}

		public void TestGlobalTariffs_ShouldNotReplaceGlobalChargeCodeWithIntercompanyMappedChargeCodeWhenMultipleARMappingsExist()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			globalChargeCode.Factory.Save();

			var globalTariff = Helper.NewGlobalTariff();
			var tariffEntry = globalTariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, origin, destination);
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, Weight.Kilograms, CurrencyCodes.Australia);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			globalTariff.Factory.Save();

			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode.AC_Code;

			var globalChargeCodeMapPivotIntercompany1 = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany1[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany1[AccGlobalChargeCodeMapPivotSchema.YP_AC] = Helper.ChargeCodes["FRT"].PK;
			globalChargeCodeMapPivotIntercompany1[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = null;
			using (globalChargeCodeMapPivotIntercompany1.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany1[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsReceivable;

				var globalChargeCodeMapPivotIntercompany2 = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
				globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
				globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_AC] = Helper.ChargeCodes["WAR"].PK;
				globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = null;
				using (globalChargeCodeMapPivotIntercompany2.GetValidationSuspender())
				{
					globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsReceivable;

					var consignee = Helper.NewOrgHeader(1);
					var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, consignee.PK, origin, destination, 25);
					Factory.Save();

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "GLBFRT",
							JR_OSSellAmt = 250m
						}
					};

					var message = "Should not prefer Intercompany mapping when mutliple mappings exist";
					AutorateAndAssert(message, expected, shipment, consignee);
				}
			}
		}

		public void TestGlobalClientRates_CostOrTariffBasedCalculator()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			globalChargeCode.Factory.Save();

			var client = Helper.NewOrgHeader(1);

			var globalCosting = Helper.NewGlobalCosting(TransportProvider1);
			var costEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "KRSEL");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var globalTariff = Helper.NewGlobalTariff();
			var tariffEntry = globalTariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "KRSEL");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffLine1 = tariffEntry.AddRateLine(globalChargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.KG);
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 50m;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 50m;
			globalTariff.Factory.Save();

			var globalRate = Helper.NewGlobalClientRate(client);
			var rateEntry = globalRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "KRSEL");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine(globalChargeCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "KRSEL", 23000);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "KRSEL", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 115000,
					JR_OSSellAmt = 189750,
					RevenueCalculationDescription = "GLBFRT: 23000 Kilogram(s) @ AUD 8.25/KG",
					CostCalculationDescription = "GLBFRT: 23000 Kilogram(s) @ AUD 5.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestGlobalClientRates_CostOrTariffBasedCalculator_MixedGlobalAndLocalRates()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			globalChargeCode.Factory.Save();

			var globalCosting = Helper.NewGlobalCosting(TransportProvider1);
			var costEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "KRSEL");
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine = costEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var globalTariff = Helper.NewGlobalTariff();
			var tariffEntry = globalTariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "KRSEL");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffLine1 = tariffEntry.AddRateLine(globalChargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.KG);
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 50m;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 50m;
			globalTariff.Factory.Save();

			var client = Helper.NewOrgHeader(1);
			var localRate = Helper.NewClientRate(client);
			var rateEntry = localRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "KRSEL");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine(GetLocal(globalChargeCode), CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10m;

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "KRSEL", 23000);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "KRSEL", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 115000,
					JR_OSSellAmt = 189750,
					RevenueCalculationDescription = "GLBFRT: 23000 Kilogram(s) @ AUD 8.25/KG",
					CostCalculationDescription = "GLBFRT: 23000 Kilogram(s) @ AUD 5.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestGlobalClientRates_CostOrTariffBasedCalculator_ViewResults()
		{
			var globalChargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("GLBCHG1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			var globalChargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("GLBCHG2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			globalChargeCode1.Factory.Save();

			//Set up Level 1 Global Tariff
			var globalTariff = Helper.NewGlobalTariff();
			var tariffEntry1 = globalTariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "CN", "AU");
			var tariffLine1 = tariffEntry1.AddRateLine(globalChargeCode1);
			tariffLine1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var tariffLine2 = tariffEntry1.AddRateLine(globalChargeCode2);
			tariffLine2.GetCalculator<FlatCalculator>().BaseRate = 110m;
			globalTariff.Factory.Save();

			Assert("Pre-condition: should be the first level 1 global tariff", globalTariff.IsLevelOneTariff());

			//Set up Client Rate for Level 1 Organisation
			var client = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "CN", "AU");
			var rateLine1 = rateEntry.AddRateLine(GetLocal(globalChargeCode1), CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;

			var rateLine2 = rateEntry.AddRateLine(GetLocal(globalChargeCode2), CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 12m;

			rateLine1.ViewResults = true;
			var resultsLine1 = rateLine1.ResultsRateLine;
			AssertNotNull("Should be able to find matching line on the global calculator", resultsLine1);
			AssertEquals(110m, resultsLine1.GetCalculator<FlatCalculator>().BaseRate);

			rateLine2.ViewResults = true;
			var resultsLine2 = rateLine2.ResultsRateLine;
			AssertNotNull(resultsLine2);
			AssertEquals(123.2m, resultsLine2.GetCalculator<FlatCalculator>().BaseRate);

			//Set up Level 1 Company Tariff
			var companyTariff = Helper.NewCompanyTariff();
			var tariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "CN", "AU");
			var tariffLine = tariffEntry.AddRateLine(GetLocal(globalChargeCode1));
			tariffLine.GetCalculator<FlatCalculator>().BaseRate = 200m;
			companyTariff.Factory.Save();

			Assert("Pre-condition: should be the first level 1 company tariff", companyTariff.IsLevelOneTariff());

			rateLine1.ViewResults = false;
			rateLine1.ViewResults = true;
			resultsLine1 = rateLine1.ResultsRateLine;

			AssertNotNull("Should be able to find matching line on the company calculator", resultsLine1);
			AssertEquals(220m, resultsLine1.GetCalculator<FlatCalculator>().BaseRate);

			rateLine2.ViewResults = false;
			rateLine2.ViewResults = true;
			resultsLine2 = rateLine2.ResultsRateLine;
			AssertEquals("Should remain the same as there no globalChargeCode2 rate line for the company tariff", 123.2m, resultsLine2.GetCalculator<FlatCalculator>().BaseRate);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, client.PK, "CNSHA", "AUBTB", 150);
			Factory.Save();

			//Not strickly necessary but since a lot of company tariff loading is cached and autorating will be done separatly
			//in a new form, it's more accurate to load the shipment in a new factory
			var newFactory = new BusinessObjectFactory();
			var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 220m,
					RevenueCalculationDescription = @"GLBCHG1: 110.00% of (Base Rate AUD 200.00)"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 123.2m,
					RevenueCalculationDescription = @"GLBCHG2: 112.00% of (Base Rate AUD 110.00)"
				}
			};

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);
		}

		public void TestGlobalCostings_DependentCalculator_WhenGlobalChargeCodeIsMappedByIntercompanyChargeCodeMapping()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";

			var globalChargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("GLB1");
			var globalChargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("GLB2");
			var intercompanyMappedChargeCode = Helper.ChargeCodes["FRT"];

			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode1.AC_Code;

			var globalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = intercompanyMappedChargeCode.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = null;
			using (globalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsPayable;

				Factory.Save();

				var globalCosting = Helper.NewGlobalCosting(TransportProvider1);
				var globalCostingEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, "");
				globalCostingEntry.RateLines.RemoveAndDeleteAll();

				var globalCostingLine1 = globalCostingEntry.AddRateLine(globalChargeCode1, FlatCalculator.Code);
				globalCostingLine1.GetCalculator<FlatCalculator>().BaseRate = 100m;

				var globalCostingLine2 = globalCostingEntry.AddRateLine(globalChargeCode2, PercentageCalculator.Code);
				var rateLineItem = globalCostingLine2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
				rateLineItem.TM_AC = globalChargeCode1.PK;
				globalCostingLine2.GetCalculator<PercentageCalculator>().Percent = 20m;

				var client = Helper.NewOrgHeader();

				var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 100m);
				var consol = CreateForwardingConsol(TransportModes.Air, origin, destination, TransportProvider1, shipment);
				consol.CreditorPK = TransportProvider1.PK;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						JR_OSCostAmt = 100m,
						CostCalculationDescription = "FRT: Base Rate AUD 100.00"
					},

					new AssertionCharge
					{
						JR_OSCostAmt = 20m,
						CostCalculationDescription = "GLB2: 20.00% of (AUD 100.00 (FRT))"
					}
				};

				AutorateAndAssert(expected, shipment, client);

				globalCostingEntry.RateLines.RemoveAndDelete(globalCostingLine2);
				var globalCostingLine3 = globalCostingEntry.AddRateLine(globalChargeCode2, PercentageBreaksCalculator.Code, QuantityUnit.KG);
				globalCostingLine3.GetCalculator<PercentageBreaksCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode1.PK;
				globalCostingLine3.GetCalculator<PercentageBreaksCalculator>()["+75"] = ZInt.Zero;
				globalCostingLine3.RateLineItems[globalCostingLine3.RateLineItems.Count - 1].TM_BreakMinimum = 15m;
				globalCostingLine3.GetCalculator<PercentageBreaksCalculator>()["-75"] = ZInt.Zero;
				globalCostingLine3.RateLineItems[globalCostingLine3.RateLineItems.Count - 1].TM_BreakMinimum = 25m;
				Factory.Save();

				expected = new[]
				{
					new AssertionCharge
					{
						JR_OSCostAmt = 100m,
						CostCalculationDescription = "FRT: Base Rate AUD 100.00"
					},

					new AssertionCharge
					{
						JR_OSCostAmt = 15m,
						CostCalculationDescription = "GLB2: 15.00% of (AUD 100.00 (FRT))"
					}
				};

				AutorateAndAssert(expected, shipment, client);

				globalCostingEntry.RateLines.RemoveAndDelete(globalCostingLine3);
				var globalCostingLine4 = globalCostingEntry.AddRateLine(globalChargeCode2, DisbursementInterestCalculator.Code).GetCalculator<DisbursementInterestCalculator>();
				globalCostingLine4.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode1.PK;
				globalCostingLine4.Uplift = 10;
				globalCostingLine4.AdjustmentDays = 5;
				globalCostingLine4.OutstandingDays = true;
				Factory.Save();

				expected = new[]
				{
					new AssertionCharge
					{
						JR_OSCostAmt = 100m,
						CostCalculationDescription = "FRT: Base Rate AUD 100.00"
					},

					new AssertionCharge
					{
						JR_OSCostAmt = 0.14m,
						CostCalculationDescription = "GLB2: AUD 100.00 (FRT) @ 10 % pa - 0 Days + 5 Adjustment Days (5 effective outstanding days) For Transport Provider 1"
					}
				};

				AutorateAndAssert(expected, shipment, client);
			}
		}

		public void TestAutorating_PercentageCalculator_ShouldNotFail_WhenRelatedChargeCodeIsEmpty()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLB2");
			var intercompanyMappedChargeCode = Helper.ChargeCodes["FRT"];

			var globalCosting = Helper.NewGlobalCosting(TransportProvider1);
			var globalCostingEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, "");
			globalCostingEntry.RateLines.RemoveAndDeleteAll();

			var globalCostingLine = globalCostingEntry.AddRateLine(globalChargeCode, PercentageCalculator.Code);
			var rateLineItem = globalCostingLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rateLineItem.TM_AC = Guid.Empty;
			globalCostingLine.GetCalculator<PercentageCalculator>().Percent = 20m;
			Factory.Save();

			var client = Helper.NewOrgHeader();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 100m);
			var consol = CreateForwardingConsol(TransportModes.Air, origin, destination, TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
					new AssertionCharge
					{
						JR_OSCostAmt = 0m,
						CostCalculationDescription = "GLB2: 20.00% of (AUD 0.00 (Charge Code))"
					},
			};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestAutorating_ShouldFilter_WhenNoLocalChargeCodeFound()
		{
			var globalChargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("HASLOCAL");
			var globalChargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("NOLOCAL");
			var localCharge1 = GetLocal(globalChargeCode1);
			var localCharge2 = GetLocal(globalChargeCode2);

			AssertNotNull("Local Charge Code should be created by default", localCharge1);
			AssertNotNull("Local Charge Code should be created by default", localCharge2);

			localCharge2.Delete();
			Factory.Save();

			var globalCosting = Helper.NewGlobalCosting(null);
			var globalCostingEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
			globalCostingEntry.RateLines.RemoveAndDeleteAll();
			globalCostingEntry.AddRateLine(globalChargeCode1).GetCalculator<FlatCalculator>().BaseRate = 10m;
			globalCostingEntry.AddRateLine(globalChargeCode2).GetCalculator<FlatCalculator>().BaseRate = 20m;

			var globalClientRate = Helper.NewGlobalClientRate(NewClient);
			var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
			globalRateEntry.RateLines.RemoveAndDeleteAll();
			globalRateEntry.AddRateLine(globalChargeCode1).GetCalculator<FlatCalculator>().BaseRate = 30m;
			globalRateEntry.AddRateLine(globalChargeCode2).GetCalculator<FlatCalculator>().BaseRate = 40m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "AUSYD", "CNSHA", 100m);
			Factory.Save();

			var message = "Expected to only include the Rate Line where the Global Charge Could be matched to a Local Charge";
			var expected = new[]
			{
					new AssertionCharge
					{
						JR_OSCostAmt = 10m,
						JR_OSSellAmt = 30m,
						ChargeCode = "HASLOCAL"
					}
			};

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1",
				"Information: RateLine Filtered NOLOCAL-FLT-Standard Costs (TACT/General Rates)	reason:	Charge Code NOLOCAL cannot be found or is not valid in the current company",
				"Information: RateLine Found HASLOCAL-FLT-Standard Costs (TACT/General Rates)",
				"Information: RatingHeader Found Global Client Rate NEWTESSYD Entries: 1",
				"Information: RateLine Filtered NOLOCAL-FLT-Global Client Rate NEWTESSYD	reason:	Charge Code NOLOCAL cannot be found or is not valid in the current company",
				"Information: RateLine Found HASLOCAL-FLT-Global Client Rate NEWTESSYD"
			};

			AutorateAndAssert(message, expected, shipment, NewClient);
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		public void TestAutorating_PercentageCalculator_ShouldFilter_WhenNoLocalChargeCodeFound()
		{
			var globalChargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("GLBFLT1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var globalChargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("GLBFLT2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var globalChargeCode3 = Helper.ChargeCodes.CreateGlobalCharge("GLBPER", PercentageCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var localCharge1 = GetLocal(globalChargeCode1);
			var localCharge2 = GetLocal(globalChargeCode2);
			var localCharge3 = GetLocal(globalChargeCode3);

			var message = "Precondition: Local Charge Code should be created by default";
			AssertNotNull(message, localCharge1);
			AssertNotNull(message, localCharge2);
			AssertNotNull(message, localCharge3);

			localCharge2.Delete();
			Factory.Save();

			var globalCosting = Helper.NewGlobalCosting(null);
			var globalCostingEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			globalCostingEntry.AddRateLine(globalChargeCode1).GetCalculator<FlatCalculator>().BaseRate = 100m;
			globalCostingEntry.AddRateLine(globalChargeCode2).GetCalculator<FlatCalculator>().BaseRate = 120m;
			var costPercentageCalc = globalCostingEntry.AddRateLine(globalChargeCode3).GetCalculator<PercentageCalculator>();
			costPercentageCalc.Percent = 20m;
			costPercentageCalc.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode1.PK;
			costPercentageCalc.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode2.PK;

			var globalClientRate = Helper.NewGlobalClientRate(NewClient);
			var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			globalRateEntry.AddRateLine(globalChargeCode1).GetCalculator<FlatCalculator>().BaseRate = 200m;
			globalRateEntry.AddRateLine(globalChargeCode2).GetCalculator<FlatCalculator>().BaseRate = 240m;
			var ratePercentage = globalRateEntry.AddRateLine(globalChargeCode3).GetCalculator<PercentageCalculator>();
			ratePercentage.Percent = 25m;
			ratePercentage.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode1.PK;
			ratePercentage.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode2.PK;

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "AUSYD", "CNSHA", 100m);
			Factory.Save();

			message = "The percentage calculator should handle finding an unmappable charge code and log this.";
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "GLBFLT1",
					JR_OSCostAmt = 100m,
					JR_OSSellAmt = 200m,
				},
				new AssertionCharge
				{
					ChargeCode = "GLBPER",
					JR_OSCostAmt = 20m,
					JR_OSSellAmt = 50m,
					CostCalculationDescription = "GLBPER: 20.00% of (AUD 100.00 (GLBFLT1 + GLBFLT2))",
					RevenueCalculationDescription = "GLBPER: 25.00% of (AUD 200.00 (GLBFLT1 + GLBFLT2))",
				}
			};

			var expectedLogLines = new[]
			{
				"Information: GLBPER-PER-Standard Costs (TACT/General Rates) could not be applied to Charge Code GLBFLT2 as it could not be found or is not valid in the current company",
				"Information: GLBPER-PER-Global Client Rate NEWTESSYD could not be applied to Charge Code GLBFLT2 as it could not be found or is not valid in the current company",
			};

			AutorateAndAssert(message, expected, shipment, NewClient);
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		public void TestPublishingLocalRateEntry()
		{
			TestPublishLocalRateEntryCore();
		}

		public void TestPublishingLocalRateEntryWithGlobalTariff()
		{
			var globalTariff = Helper.NewGlobalTariff();
			globalTariff.Factory.Save();
			TestPublishLocalRateEntryCore();
		}

		void TestPublishLocalRateEntryCore()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			var localChargeCode = GetLocal(globalChargeCode);
			var additionalLocalChargeCode = Helper.ChargeCodes["BAF"];

			var localTariff = Factory.New<CompanyTariff>();
			var localTariffEntry = localTariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
			localTariffEntry.RateLines.RemoveAndDeleteAll();

			var localTariffLine1 = localTariffEntry.AddRateLine(localChargeCode, FlatCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			localTariffLine1.GetCalculator<FlatCalculator>().BaseRate = 10m;

			var localTariffLine2 = localTariffEntry.AddRateLine(additionalLocalChargeCode, PercentageCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			var percentageCalculator = localTariffLine2.GetCalculator<PercentageCalculator>();
			percentageCalculator.Percent = 10m;

			var rateLineItem = percentageCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rateLineItem.TM_AC = localChargeCode.PK;

			localTariffEntry.IsPublished = true;
			rateLineItem.Validation.ValidateTM_AC();

			CombineAssertions(
				() =>
				{
					Assert("Pre-condition: entry should be published", localTariffEntry.IsPublished);
					AssertNoErrors("Expect no errors as global rate entry should have global charge code", rateLineItem.TM_ACInfo);
				});

			localTariffEntry.IsPublished = false;
			rateLineItem.Validation.ValidateTM_AC();

			CombineAssertions(
				() =>
				{
					Assert("Pre-condition: entry should be not published", !localTariffEntry.IsPublished);
					AssertNoErrors("Expect no errors as local rate entry should have local charge code", rateLineItem.TM_ACInfo);
				});
		}

		#region Implementation

		AccChargeCode GetLocal(AccChargeCode globalChargeCode)
		{
			if (!globalChargeCode.IsInDatabase)
			{
				globalChargeCode.Factory.Save();
			}

			return globalChargeCode.ChildChargeCodes.Single(x => x.AC_GC == Env.CurrentCompanyPK);
		}

		#endregion
	}
}
