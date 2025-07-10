using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseJobComInvoiceLine;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceLine))]
	sealed class BaseJobComInvoiceLineBaseOnlyTest : BaseJobComInvoiceLineAbstractTest
	{
		public void TestIInvoiceLinePartDetailsEnable()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLinePartDetails = invoiceLine as IInvoiceLinePartDetails;
			Assert(invoiceLinePartDetails.Enabled);
			declaration.JE_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			Assert(!invoiceLinePartDetails.Enabled);
		}

		public void TestProvProgAdditionalTariffThatUsedInInvoiceLineReport()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals("ProvProgTariff", ZString.Empty, invoiceLine.ProvProgTariff);
			AssertEquals("ProvProgDutyRate", ZString.Empty, invoiceLine.ProvProgDutyRate);
			AssertEquals("ProvProgCustomsQty", ZString.Empty, invoiceLine.ProvProgCustomsQty);
			AssertEquals("ProvProgTariff1", ZString.Empty, invoiceLine.ProvProgTariff1);
			AssertEquals("ProvProgDutyRate1", ZString.Empty, invoiceLine.ProvProgDutyRate1);
			AssertEquals("ProvProgCustomsQty1", ZString.Empty, invoiceLine.ProvProgCustomsQty1);
			AssertEquals("ProvProgTariff2", ZString.Empty, invoiceLine.ProvProgTariff2);
			AssertEquals("ProvProgDutyRate2", ZString.Empty, invoiceLine.ProvProgDutyRate2);
			AssertEquals("ProvProgCustomsQty2", ZString.Empty, invoiceLine.ProvProgCustomsQty2);
			AssertEquals("ProvProgTariff3", ZString.Empty, invoiceLine.ProvProgTariff3);
			AssertEquals("ProvProgDutyRate3", ZString.Empty, invoiceLine.ProvProgDutyRate3);
			AssertEquals("ProvProgCustomsQty3", ZString.Empty, invoiceLine.ProvProgCustomsQty3);
			AssertEquals("ProvProgTariff4", ZString.Empty, invoiceLine.ProvProgTariff4);
			AssertEquals("ProvProgDutyRate4", ZString.Empty, invoiceLine.ProvProgDutyRate4);
			AssertEquals("ProvProgCustomsQty4", ZString.Empty, invoiceLine.ProvProgCustomsQty4);
			AssertEquals("ProvProgTariff5", ZString.Empty, invoiceLine.ProvProgTariff5);
			AssertEquals("ProvProgDutyRate5", ZString.Empty, invoiceLine.ProvProgDutyRate5);
			AssertEquals("ProvProgCustomsQty5", ZString.Empty, invoiceLine.ProvProgCustomsQty5);
		}

		[ExpectNoExceptions]
		public void TestCalculateLinePriceFromUnitPrice()
		{
			AssertEquals(25075m, invoiceLine.CalculateLinePriceFromUnitPrice(100, 250.75));
			AssertEquals(0m, invoiceLine.CalculateLinePriceFromUnitPrice(Decimal.MaxValue, Decimal.MaxValue));
		}

		public void TestFirstVehicle_VehicleRelationshipType_None()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
			invoiceLineMock.Setup(x => x.VehicleRelationship).Returns(VehicleRelationshipType.None);
			var invoiceLine = invoiceLineMock.Object;
			invoice.JobComInvoiceLines.Add(invoiceLine);
			AssertEquals("Vehicles count is 0", 0, invoiceLine.Vehicles.Count);
			AssertNull("invoiceLine.FirstVehicle for VehicleRelationshipType.None", invoiceLine.FirstVehicle);
		}

		public void TestFirstVehicle_CreateNew()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
				invoiceLineMock.Setup(x => x.VehicleRelationship).Returns(VehicleRelationshipType.Many);
				var invoiceLine = invoiceLineMock.Object;
				invoice.JobComInvoiceLines.Add(invoiceLine);
				AssertEquals("Vehicles count is 0", 0, invoiceLine.Vehicles.Count);

				var vehicle = invoiceLine.FirstVehicle;
				AssertNotNull("InvoiceLine.FirstVehicle", vehicle);
				AssertEquals("FirstVehicle is automatically created", 1, invoiceLine.Vehicles.Count);
				AssertEquals("Getting the FirstVehicle again returns the existing vehicle", vehicle, invoiceLine.FirstVehicle);
			});
		}

		public void TestFirstVehicle_UseOldest()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
				invoiceLineMock.Setup(x => x.VehicleRelationship).Returns(VehicleRelationshipType.Many);
				var invoiceLine = invoiceLineMock.Object;
				invoiceLine.JI_JZ = invoice.PK;
				invoiceLine.JI_ClusterKey = invoice.JZ_ClusterKey;
				invoice.JobComInvoiceLines.Add(invoiceLine);

				var vehicle1 = invoiceLine.Vehicles.AddNew();
				vehicle1.CVH_VehicleIdentificationNumber = "VIN2";
				Factory.Save();

				var vehicle2 = invoiceLine.Vehicles.AddNew();
				vehicle2.CVH_VehicleIdentificationNumber = "VIN1";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();

				invoiceLineMock = newFactory.LoadMoq<BaseJobComInvoiceLine>(invoiceLine.PK);
				invoiceLineMock.Setup(x => x.VehicleRelationship).Returns(VehicleRelationshipType.Many);
				invoiceLine = invoiceLineMock.Object;
				AssertEquals("Vehicles count is 2", 2, invoiceLine.Vehicles.Count);
				AssertEquals("invoiceLine.FirstVehicle.PK", vehicle1.PK, invoiceLine.FirstVehicle.PK);
			});
		}

		public void TestDeleteVehicles()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
				invoiceLineMock.Setup(x => x.VehicleRelationship).Returns(VehicleRelationshipType.Many);
				var invoiceLine = invoiceLineMock.Object;
				invoiceLine.JI_JZ = invoice.PK;
				invoiceLine.JI_ClusterKey = invoice.JZ_ClusterKey;
				invoice.JobComInvoiceLines.Add(invoiceLine);
				var vehicle = invoiceLine.Vehicles.AddNew();
				vehicle.CVH_VehicleIdentificationNumber = "VI32423";

				Factory.Save();
				AssertEquals("Vehicles count is 1 after save", 1, invoiceLine.Vehicles.Count);
				AssertEquals("Vehicle is saved in database", true, vehicle.IsInDatabase);
				AssertEquals("Vehicle is not deleted", false, vehicle.IsDeleted);

				invoiceLine.Delete();
				AssertEquals("Vehicles count is 0 when invoice line is deleted", 0, invoiceLine.Vehicles.Count);
				AssertEquals("Vehicle is deleted", true, vehicle.IsDeleted);

				Factory.Save();
				AssertEquals("Vehicle is no longer in database", false, vehicle.IsInDatabase);
			});
		}

		public void TestJI_TariffForComplianceWise()
		{
			var factory = new BusinessObjectFactory();
			var invoiceLine = factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_Tariff = "1234.56";
			AssertEquals("Precondition: ", "1234.56", invoiceLine.JI_Tariff);
			AssertEquals("1234.56", invoiceLine.JI_TariffForComplianceWise);
		}

		public void TestClearJI_MatchingKeyOnSavingFail_NewInvoiceLine()
		{
			var factory = new BusinessObjectFactory();
			var invoiceLine = factory.New<BaseJobComInvoiceLine>();
			AssertExceptionThrown<Exception>(() => factory.Save());
			AssertEquals(string.Empty, invoiceLine.JI_MatchingKey);
		}

		public void TestWouldNotClearJI_MatchingKeyOnSavingFail_ExistedInvoiceLine()
		{
			var factory = new BusinessObjectFactory();
			var invoiceLine = factory.NewWithValidTestData<BaseJobComInvoiceLine>();
			factory.Save();

			var savedJI_MatchingKey = invoiceLine.JI_MatchingKey;

			factory.Saving += (f) => throw new Exception("simulate concurrency error");
			AssertExceptionThrown<Exception>(() => factory.Save());
			AssertEquals(savedJI_MatchingKey, invoiceLine.JI_MatchingKey);
		}

		public void TestWouldNotClearJI_MatchingKeyOnSavingFail_NotPopulatedByNumberFountain()
		{
			var factory = new BusinessObjectFactory();
			var invoiceLine = factory.NewWithValidTestData<BaseJobComInvoiceLine>();
			var savedJI_MatchingKey = invoiceLine.JI_MatchingKey;

			factory.Saving += (f) => { throw new Exception("simulate concurrency error"); };
			AssertExceptionThrown<Exception>(() => factory.Save());
			AssertEquals(savedJI_MatchingKey, invoiceLine.JI_MatchingKey);
		}

		public void TestLinkedModuleCommodityRiskStatusDescription()
		{
			AssertLinkedModuleCommodityRiskStatusDescription("CLR", "Clear");
			AssertLinkedModuleCommodityRiskStatusDescription("PSK", "Potential Risk");
			AssertLinkedModuleCommodityRiskStatusDescription("NCH", "Not Checked");
			AssertLinkedModuleCommodityRiskStatusDescription("PRS", "Possible Risk");
			AssertLinkedModuleCommodityRiskStatusDescription("HSK", "High Risk");

			void AssertLinkedModuleCommodityRiskStatusDescription(string code, string description)
			{
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "1234";
					invoiceLine.JI_CountryOfOrigin = "US";

					((ISupportInteractionWithComplianceWiseCommodities)declaration).Helper = new InteractionWithComplianceWiseCommoditiesHelper(declaration);

					var commodityInfo = new ComplianceResultFromCpw
					{
						HarmonizedCode = "1234",
						GroupingOrCountry = "WCO",
						GoodsDescription = "",
						OriginOfGoods = "",
						LinkVisible = true,
						HarmonizedBorderWiseTextual = "Test Harmonized Border Wise Textual",
						RiskStatus = code,
						RiskNotes = "",
						AssessmentInitialized = true
					};
					invoiceLine.LinkedModuleCommodity.BatchInitialize(commodityInfo);

					AssertEquals(code, invoiceLine.LinkedModuleCommodity.RiskStatus);
					AssertEquals(description, invoiceLine.LinkedModuleCommodity.RiskStatusDescription);
				}
			}
		}

		public void TestReCalculateTariffDescriptionWhenAttachedToDec()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoiceLine1.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine1);
			invoiceLine1.JI_Tariff = "123456789";

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoiceLine2.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine2);
			invoiceLine2.JI_Tariff = "123456789";
			invoiceLine2.JI_Description = ZString.Empty;

			var invoiceLine3 = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoiceLine3.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine3);
			invoiceLine3.JI_Tariff = "123456789";
			invoiceLine3.JI_Description = "XXX";

			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(true)))
			{
				invoiceLine1.ReCalculateTariffDescriptionWhenAttachedToDec();
				invoiceLine2.ReCalculateTariffDescriptionWhenAttachedToDec();
				invoiceLine3.ReCalculateTariffDescriptionWhenAttachedToDec();
				AssertEquals("When EnableCustomsDeclaration is true, default tariff description should be kept.", "TEST DESCRIPTION", invoiceLine1.JI_Description);
				AssertEquals("When EnableCustomsDeclaration is true, empty JI_Description should be updated with default tariff description.", "TEST DESCRIPTION", invoiceLine2.JI_Description);
				AssertEquals("When EnableCustomsDeclaration is true, manually entered JI_Description should be kept.", "XXX", invoiceLine3.JI_Description);
			}

			invoiceLine2.JI_Description = ZString.Empty;
			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(false)))
			{
				invoiceLine1.ReCalculateTariffDescriptionWhenAttachedToDec();
				invoiceLine2.ReCalculateTariffDescriptionWhenAttachedToDec();
				invoiceLine3.ReCalculateTariffDescriptionWhenAttachedToDec();
				AssertEquals("When EnableCustomsDeclaration is false, default tariff description should be removed.", string.Empty, invoiceLine1.JI_Description);
				AssertEquals("When EnableCustomsDeclaration is false, empty JI_Description should be kept.", string.Empty, invoiceLine2.JI_Description);
				AssertEquals("When EnableCustomsDeclaration is false, manually entered JI_Description should be kept.", "XXX", invoiceLine3.JI_Description);
			}

			AutomatedTariffDescriptionPopulation GetConfiguredRegistry(bool value)
			{
				var registryValue = new AutomatedTariffDescriptionPopulation();
				registryValue.EnableCustomsDeclaration = value;
				return registryValue;
			}
		}

		public void TestShouldSetDescriptionWhenTariffChanges_ShouldBeDependentOfRegistryEnableAutoTariffDescriptionPopulation()
		{
			CombineAssertions("When the invoice line is the child of an invoice header attached to a declaration:", () =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine_AttachedToDec = Factory.New<BaseJobComInvoiceLineForTesting>();
				invoiceLine_AttachedToDec.JI_JZ = invoice.PK;
				invoice.InvoiceLines.Add(invoiceLine_AttachedToDec);

				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(true, true)))
				{
					Assert("invoiceLine_AttachedToDec.JI_Description should be populated when Registry.EnableAutoTariffDescriptionPopulation.EnableCustomsDeclaration is true, regardless the value of EnableCommercialInvoice.", DescriptionCanBePopulated(ref invoiceLine_AttachedToDec));
				}
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(true, false)))
				{
					Assert("invoiceLine_AttachedToDec.JI_Description should be populated when Registry.EnableAutoTariffDescriptionPopulation.EnableCustomsDeclaration is true, regardless the value of EnableCommercialInvoice.", DescriptionCanBePopulated(ref invoiceLine_AttachedToDec));
				}
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(false, true)))
				{
					Assert("invoiceLine_AttachedToDec.JI_Description should NOT be populated when Registry.EnableAutoTariffDescriptionPopulation.EnableCustomsDeclaration is false, regardless the value of EnableCommercialInvoice.", !DescriptionCanBePopulated(ref invoiceLine_AttachedToDec));
				}
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(false, false)))
				{
					Assert("invoiceLine_AttachedToDec.JI_Description should NOT be populated when Registry.EnableAutoTariffDescriptionPopulation.EnableCustomsDeclaration is false, regardless the value of EnableCommercialInvoice.", !DescriptionCanBePopulated(ref invoiceLine_AttachedToDec));
				}
			});

			CombineAssertions("When the invoice line is the child of a stand-alone invoice header:", () =>
			{
				var invoiceHeader_StandAlone = Factory.New<BaseJobComInvoiceHeader>();
				new FakeDeclarationCreatorForInvoice(invoiceHeader_StandAlone);
				var invoiceLine_AttachedNoDec = Factory.New<BaseJobComInvoiceLineForTesting>();
				invoiceLine_AttachedNoDec.JI_JZ = invoiceHeader_StandAlone.PK;
				invoiceHeader_StandAlone.InvoiceLines.Add(invoiceLine_AttachedNoDec);

				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(true, true)))
				{
					Assert("invoiceLine_AttachedNoDec.JI_Description should be populated when Registry.EnableAutoTariffDescriptionPopulation.EnableCommercialInvoice is true, regardless of the value of EnableCustomsDeclaration.", DescriptionCanBePopulated(ref invoiceLine_AttachedNoDec));
				}
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(true, false)))
				{
					Assert("invoiceLine_AttachedNoDec.JI_Description should NOT be populated when Registry.EnableAutoTariffDescriptionPopulation.EnableCommercialInvoice is false, regardless of the value of EnableCustomsDeclaration..", !DescriptionCanBePopulated(ref invoiceLine_AttachedNoDec));
				}
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(false, true)))
				{
					Assert("invoiceLine_AttachedNoDec.JI_Description should be populated when Registry.EnableAutoTariffDescriptionPopulation.EnableCommercialInvoice is true, regardless of the value of EnableCustomsDeclaration..", DescriptionCanBePopulated(ref invoiceLine_AttachedNoDec));
				}
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(false, false)))
				{
					Assert("invoiceLine_AttachedNoDec.JI_Description should NOT be populated when Registry.EnableAutoTariffDescriptionPopulation.EnableCommercialInvoice is false, regardless of the value of EnableCustomsDeclaration..", !DescriptionCanBePopulated(ref invoiceLine_AttachedNoDec));
				}
			});

			bool DescriptionCanBePopulated(ref BaseJobComInvoiceLineForTesting invoiceLine)
			{
				invoiceLine.JI_Tariff = "123456789";
				var result = invoiceLine.JI_Description == "TEST DESCRIPTION";

				invoiceLine.JI_Tariff = ZString.Empty;
				invoiceLine.JI_Description = ZString.Empty;
				return result;
			}

			AutomatedTariffDescriptionPopulation GetConfiguredRegistry(bool enableDecValue, bool enableInvoiceValue)
			{
				var registryValue = new AutomatedTariffDescriptionPopulation();
				registryValue.EnableCustomsDeclaration = enableDecValue;
				registryValue.EnableCommercialInvoice = enableInvoiceValue;
				return registryValue;
			}
		}

		public void TestJI_DataModel_SetOnSaving()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			var invoieLine = Factory.New<BaseJobComInvoiceLine>();
			AssertEquals(ZString.Empty, invoiceLine.JI_DataModel);
			invoiceLine.OnSaving();
			AssertEquals("ER", invoiceLine.JI_DataModel);
		}

		public void TestJI_DataModel_SetOnFactorySave()
		{
			AssertJI_DataModel(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public void TestJI_DataModel_Jurisdiction()
		{
			AssertJI_DataModel(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.Australia);
			AssertJI_DataModel(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland);
			AssertJI_DataModel(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			AssertJI_DataModel(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France);

			AssertJI_DataModel(Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates);
			AssertJI_DataModel(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Switzerland);
			AssertJI_DataModel(Core.Constants.CountryCodes.FrenchGuyana, Core.Constants.CountryCodes.France);
			AssertJI_DataModel(Core.Constants.CountryCodes.Guadeloupe, Core.Constants.CountryCodes.France);
			AssertJI_DataModel(Core.Constants.CountryCodes.Martinique, Core.Constants.CountryCodes.France);
			AssertJI_DataModel(Core.Constants.CountryCodes.Mayotte, Core.Constants.CountryCodes.France);
			AssertJI_DataModel(Core.Constants.CountryCodes.Reunion, Core.Constants.CountryCodes.France);
			AssertJI_DataModel(Core.Constants.CountryCodes.SaintMartin, Core.Constants.CountryCodes.France);
			AssertJI_DataModel(Core.Constants.CountryCodes.SaintBarthelemy, Core.Constants.CountryCodes.France);
		}

		void AssertJI_DataModel(string currentCountry, string expectedJI_DataModel)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(currentCountry))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				AssertEquals("Not set", ZString.Empty, invoiceLine.JI_DataModel);
				Factory.Save();
				AssertEquals("set", expectedJI_DataModel, invoiceLine.JI_DataModel);
			}
		}

		public void TestJI_DataModel_ReportErrorWhenUpdated() =>
			DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<BaseJobComInvoiceLine>(Factory);

		public void TestJI_DataModel_CanSaveTwice() =>
			DataModelTestHelper.RunDataModelTest_CanSaveTwice<BaseJobComInvoiceLine>(Factory);

		public void TestIDataModelSupporter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusSupportingInfoParent = (IDataModelSupporter)invoiceLine;
			AssertEquals(ZString.Empty, cusSupportingInfoParent.DataModel);
			cusSupportingInfoParent.PopulateDataModelIfNeeded();
			AssertEquals("ER", cusSupportingInfoParent.DataModel);
		}

		public void TestIsOutOfOutwardProcessing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_OutofOutwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");
			procedure2.ZZ6_IntoOutwardProcessing = "Y";
			procedure2.ZZ6_OutofOutwardProcessing = "Y";

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "AABB";
			AssertEquals(true, invoiceLine.IsOutOfOutwardProcessing);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, invoiceLine.IsOutOfOutwardProcessing);
		}

		public void TestIsIntoOutwardProcessing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_IntoOutwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");
			procedure2.ZZ6_IntoOutwardProcessing = "Y";
			procedure2.ZZ6_OutofOutwardProcessing = "Y";

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "AABB";
			AssertEquals(true, invoiceLine.IsIntoOutwardProcessing);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, invoiceLine.IsIntoOutwardProcessing);
		}

		public void TestIsOutOfInwardProcessing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_OutOfInwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");
			procedure2.ZZ6_IntoInwardProcessing = "Y";
			procedure2.ZZ6_OutOfInwardProcessing = "Y";

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "AABB";
			AssertEquals(true, invoiceLine.IsOutOfInwardProcessing);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, invoiceLine.IsOutOfInwardProcessing);
		}

		public void TestIsIntoInwardProcessing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_IntoInwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");
			procedure2.ZZ6_IntoInwardProcessing = "Y";
			procedure2.ZZ6_OutOfInwardProcessing = "Y";

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "AABB";
			AssertEquals(true, invoiceLine.IsIntoInwardProcessing);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, invoiceLine.IsIntoInwardProcessing);
		}

		public void TestHasIntoOutwardProcessingProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_IntoOutwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "AABB";
			AssertEquals(true, invoiceLine.HasIntoOutwardProcessingProcedure);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, invoiceLine.HasIntoOutwardProcessingProcedure);
		}

		public void TestHasIntoInwardProcessingProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_IntoInwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "AABB";
			AssertEquals(true, invoiceLine.HasIntoInwardProcessingProcedure);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, invoiceLine.HasIntoInwardProcessingProcedure);
		}

		public void TestPopulateMatchingKeyIfNeeded()
		{
			var jobComInvoiceLine = invoice.InvoiceLines.AddNew();
			jobComInvoiceLine.JI_MatchingKey = "";
			Factory.Save();
			AssertEquals(true, new Regex($"^{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}\\d{{8}}$").IsMatch(jobComInvoiceLine.JI_MatchingKey));
		}

		public void TestPopulateMatchingKeyIfNeeded_DuplicateInFactory()
		{
			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			var fountain = Env.NumberFountains.JobComInvoiceLineMatchingKey(prefix, JobComInvoiceLineSchema.JI_MatchingKey.MaxLength);
			var peekedNumber = fountain.PeekPreliminaryFormatted(Db.Connection);
			var jobComInvoiceLine = invoice.InvoiceLines.AddNew();
			jobComInvoiceLine.JI_MatchingKey = peekedNumber;
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestPopulateMatchingKeyIfNeeded_DuplicateInDB()
		{
			Factory.Save();

			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			var fountain = Env.NumberFountains.JobComInvoiceLineMatchingKey(prefix, JobComInvoiceLineSchema.JI_MatchingKey.MaxLength);
			var peekedNumber = fountain.PeekPreliminaryFormatted(Db.Connection);

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.JobComInvoiceLine SET JI_MatchingKey = '{peekedNumber}', JI_SystemLastEditTimeUtc = GetUtcDate(), JI_SystemLastEditUser = '~BP' WHERE JI_PK = '{invoiceLine.PK}'");

			var jobComInvoiceLine = invoice.InvoiceLines.AddNew();
			AssertNoExceptionThrown(Factory.Save);
		}

		[SnailTest]
		[DeveloperOnlyTest]
		public void TestPopulateMatchingKeyIfNeeded_Performance()
		{
			for (var i = 0; i < 10000; i++)
			{
				var jobComInvoiceLine = invoice.InvoiceLines.AddNew();
				jobComInvoiceLine.JI_MatchingKey = "";
			}

			var stopWatch = new Stopwatch();
			stopWatch.Start();
			Factory.Save();
			stopWatch.Stop();

			AssertLessThan("stopWatch time should be satisfied", stopWatch.ElapsedMilliseconds, 3000);
		}

		[SnailTest]
		[DeveloperOnlyTest]
		public void TestPopulateMatchingKeyIfNeeded_DbHits()
		{
			for (var i = 0; i < 10000; i++)
			{
				var jobComInvoiceLine = invoice.InvoiceLines.AddNew();
				jobComInvoiceLine.JI_MatchingKey = "";
			}

			var expectedHits = new Dictionary<string, int>();

			using (AssertDbHitsForAllFactories("Performance - set 10000 JI_MatchingKey and Save", expectedHits, acceptableVariance: 1))
			{
				Factory.Save();
			}
		}

		public void TestCopyDetachedUNDGDataItem()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				CombineAssertions(() =>
				{
					undgs = invoiceLine.UNDGs;
					undg = undgs.AddNew();
					var undg2 = undgs.AddNew();
					undgs.CollectionCountChange += Undgs_CollectionCountChange;
					var product = Factory.New<OrgSupplierPart>();
					var undgProduct = product.UNDGs.AddNew();
					undgProduct.DI_DGWeight = 11m;
					AssertNoExceptionThrown(() => invoiceLine.CopyUNDGsIfSupported(product.UNDGs));
					AssertEquals("undg.IsDeleted", true, undg.IsDeleted);
					AssertEquals("undg2.IsDeleted", true, undg2.IsDeleted);
					AssertEquals("invoiceLine.UNDGs.Count", 1, invoiceLine.UNDGs.Count);
					AssertEquals("DI_DGWeight", 11m, invoiceLine.UNDGs[0].DI_DGWeight);
				});
			}
		}

		void Undgs_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved)
			{
				if (undgs != null)
				{
					undgs.CollectionCountChange -= Undgs_CollectionCountChange;
				}
				undg?.Delete();
			}
		}
		UNDGDataItemCollection undgs;
		UNDGDataItem undg;

		public void TestInitialiseCustomsUnitDefaultingStrategy()
		{
			var tariffCustomsUnitDefaultingStrategyMock = new Mock<ICustomsUnitDefaultingStrategy>();
			var quotaCustomsUnitDefaultingStrategyMock = new Mock<ICustomsUnitDefaultingStrategy>();

			int callOrder = 0;
			tariffCustomsUnitDefaultingStrategyMock.Setup(x => x.Initialise(It.IsAny<BaseJobComInvoiceLine>()))
				.Callback(() => AssertEquals("Tariff was called first", 0, callOrder++));
			quotaCustomsUnitDefaultingStrategyMock.Setup(x => x.Initialise(It.IsAny<BaseJobComInvoiceLine>()))
				.Callback(() => AssertEquals("Quota was called second", 1, callOrder++));

			var tariffCustomsUnitDefaultingStrategy = tariffCustomsUnitDefaultingStrategyMock.Object;
			var quotaCustomsUnitDefaultingStrategy = quotaCustomsUnitDefaultingStrategyMock.Object;

			var compositeStrategy =
				new CompositeCustomsUnitDefaultingStrategy(tariffCustomsUnitDefaultingStrategy, quotaCustomsUnitDefaultingStrategy);

			var invoiceLine = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoiceLine.GetCustomsUnitDefaultingStrategyReturns = compositeStrategy;
			invoiceLine.ResetCustomsUnitDefaultingStrategyExposed();

			quotaCustomsUnitDefaultingStrategyMock.Verify(x => x.Initialise(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Quota has not been called");
			tariffCustomsUnitDefaultingStrategyMock.Verify(x => x.Initialise(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Tariff has not been called");
		}

		public void TestExecuteCustomsUnitDefaultingStrategy()
		{
			var tariffCustomsUnitDefaultingStrategyMock = new Mock<ICustomsUnitDefaultingStrategy>();
			var quotaCustomsUnitDefaultingStrategyMock = new Mock<ICustomsUnitDefaultingStrategy>();

			int callOrder = 0;
			tariffCustomsUnitDefaultingStrategyMock.Setup(x => x.DefaultUOMs(It.IsAny<BaseJobComInvoiceLine>()))
				.Callback(() => AssertEquals("Tariff was called first", 0, callOrder++));
			quotaCustomsUnitDefaultingStrategyMock.Setup(x => x.DefaultUOMs(It.IsAny<BaseJobComInvoiceLine>()))
				.Callback(() => AssertEquals("Quota was called second", 1, callOrder++));

			var tariffCustomsUnitDefaultingStrategy = tariffCustomsUnitDefaultingStrategyMock.Object;
			var quotaCustomsUnitDefaultingStrategy = quotaCustomsUnitDefaultingStrategyMock.Object;

			var compositeStrategy =
				new CompositeCustomsUnitDefaultingStrategy(tariffCustomsUnitDefaultingStrategy, quotaCustomsUnitDefaultingStrategy);

			var invoiceLine = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoiceLine.GetCustomsUnitDefaultingStrategyReturns = compositeStrategy;
			invoiceLine.ResetCustomsUnitDefaultingStrategyExposed();
			invoiceLine.ExecuteCustomsUnitDefaultingStrategy();

			quotaCustomsUnitDefaultingStrategyMock.Verify(x => x.DefaultUOMs(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Quota has not been called");
			tariffCustomsUnitDefaultingStrategyMock.Verify(x => x.DefaultUOMs(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Tariff has not been called");
		}

		public void TestHasOutOfRegimeProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "AA", "BB", ZString.Empty, "OP DESC1", "EXW", outOfWarehouse: true);
			var procedure2 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "CC", "DD", ZString.Empty, "OP DESC2", "EXW");
			procedure2.ZZ6_OutOfInwardProcessing = "Y";
			var procedure3 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "EE", "FF", ZString.Empty, "OP DESC3", "EXW");
			procedure3.ZZ6_OutofOutwardProcessing = "Y";
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "HH", "II", ZString.Empty, "OP DESC4", "EXW");
			Factory.Save();

			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_Procedure = "AABB";
			Assert(invoiceLine.HasOutOfRegimeProcedure);

			invoiceLine.JI_Procedure = "CCDD";
			Assert(invoiceLine.HasOutOfRegimeProcedure);

			invoiceLine.JI_Procedure = "EEFF";
			Assert(invoiceLine.HasOutOfRegimeProcedure);

			invoiceLine.JI_Procedure = "GGHH";
			Assert(!invoiceLine.HasOutOfRegimeProcedure);
		}

		public void TestHasIntoRegimeProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "AA", "BB", ZString.Empty, "OP DESC1", "EXW", intoWarehouse: true);
			var procedure2 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "CC", "DD", ZString.Empty, "OP DESC2", "EXW");
			procedure2.ZZ6_IntoInwardProcessing = "Y";
			var procedure3 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "EE", "FF", ZString.Empty, "OP DESC3", "EXW");
			procedure3.ZZ6_IntoOutwardProcessing = "Y";
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "HH", "II", ZString.Empty, "OP DESC4", "EXW");
			Factory.Save();

			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_Procedure = "AABB";
			Assert(invoiceLine.HasIntoRegimeProcedure);

			invoiceLine.JI_Procedure = "CCDD";
			Assert(invoiceLine.HasIntoRegimeProcedure);

			invoiceLine.JI_Procedure = "EEFF";
			Assert(invoiceLine.HasIntoRegimeProcedure);

			invoiceLine.JI_Procedure = "GGHH";
			Assert(!invoiceLine.HasIntoRegimeProcedure);
		}

		public void TestJI_Calc_OtherTaxesAmount()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20m;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeAmount = 100m;
			fee1.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeAmount = 200m;
			fee2.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;

			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_ChargeAmount = 400m;
			AssertEquals("JI_Calc_OtherTaxesAmount should be 1/3 of the sum amount of fees whose type is not DTY or GST", 133.33333333m, invoiceLine1.JI_Calc_OtherTaxesAmount);
			AssertEquals("JI_Calc_OtherTaxesAmount should be 2/3 of the sum amount of fees whose type is not DTY or GST", 266.66666667m, invoiceLine2.JI_Calc_OtherTaxesAmount);
		}

		public void TestJI_Calc_ValueForVat()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = dec.Invoices.AddNew();

			var invoiceLine = Factory.New<Testing.BaseJobComInvoiceLineForTesting>();
			invoiceLine.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine);

			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 500m;
			invoiceLine.JI_LinePrice = 1000m;

			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var rate = usdCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Now.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
			rate.RE_SellRate = 2;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;

			DoMerge(dec);

			AssertEquals("JI_Calc_ValueForVat", 2000m, invoiceLine.JI_Calc_ValueForVat);
		}

		public void TestDefaultDataGroupingForTaxOrFee()
		{
			CombineAssertions(() =>
			{
				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Angola;
				var branch = company.Branches.AddNew();
				var currentCountry = GlbCompany.CurrentCompany.Country.Code;
				var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
				var defaultDataGroupingField = typeof(BaseJobComInvoiceLine).GetProperty("DefaultDataGroupingForTaxOrFee", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertEquals("No binded Declaration", currentCountry, defaultDataGroupingField.GetValue(invoiceLine));

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_GB = branch.PK;
				var invoice = declaration.Invoices.AddNew();
				invoiceLine.JI_JZ = invoice.PK;
				AssertEquals("Declaration DefaultDataGrouping", Core.Constants.CountryCodes.Angola, declaration.GetDefaultDataGroupingCode());
				AssertEquals("Has binded Declaration and use Declaration DefaultDataGrouping for invoiceLine", Core.Constants.CountryCodes.Angola, defaultDataGroupingField.GetValue(invoiceLine));
			});
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			var companyLV = Factory.New<GlbCompany>();
			companyLV.GC_Code = "CL1";
			companyLV.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var companyLVBranch = companyLV.Branches.AddNew();
			companyLVBranch.GB_Code = "BL1";
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IInvoiceLinePartDetails partDetails = invoiceLine;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, partDetails.CustomsCountryCode);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.US.IOrgSupplierPart>(), partDetails.TypeOfPartUsed);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(Core.Constants.CountryCodes.Australia, partDetails.CustomsCountryCode);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.AU.IOrgSupplierPart>(), partDetails.TypeOfPartUsed);
			}
			invoice.JZ_GB = companyLVBranch.PK;
			AssertEquals(Core.Constants.CountryCodes.Latvia, partDetails.CustomsCountryCode);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.IOrgSupplierPart>(), partDetails.TypeOfPartUsed);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert($"Covered by {nameof(FetchStrategies.Testing.JobComInvoiceLineFetchStrategyTest)}.", true);
		}

		public void TestJI_Procedure_Caption()
		{
			AssertEquals("Procedure Code", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_ProcedureInfo).Caption);
		}

		public void TestJI_ZZF_NKTaxType_Caption()
		{
			AssertEquals("Tax Type Code", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_ZZF_NKTaxTypeInfo).Caption);
		}

		public void TestJI_ZZF_NKTaxType_MaxLength()
		{
			AssertEquals("JI_ZZF_NKTaxType should handle up to 4 chars", 4, invoiceLine.JI_ZZF_NKTaxTypeInfo.MaxLength);
		}

		public void TestJI_PrimaryPreference_Caption()
		{
			AssertEquals("Preference", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_PrimaryPreferenceInfo).Caption);
		}

		public void TestJI_CustomsSecondQuantity_Caption()
		{
			AssertEquals("Additional Qty 1", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_CustomsSecondQuantityInfo).Caption);
		}

		public void TestJI_CustomsThirdQuantity_Caption()
		{
			AssertEquals("Additional Qty 2", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_CustomsThirdQuantityInfo).Caption);
		}

		public void TestJI_CustomsFourthQuantity_Caption()
		{
			AssertEquals("Additional Qty 3", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_CustomsFourthQuantityInfo).Caption);
		}

		public void TestJI_CustomsFifthQuantity_Caption() => CombineAssertions(() =>
			AssertEntity<BaseJobComInvoiceLine>()
				.HasProperty(x => x.JI_CustomsFifthQuantity)
				.WithCaption("Additional Qty 4")
				.WithFullDescription("Additional Quantity 4")
		);

		public void TestJI_CustomsThirdUnitQty_MaxLength()
		{
			AssertEquals(4, invoiceLine.JI_CustomsThirdUnitQtyInfo.MaxLength);
		}

		public void TestJI_CustomsFourthUnitQty_MaxLength()
		{
			AssertEquals(4, invoiceLine.JI_CustomsFourthUnitQtyInfo.MaxLength);
		}

		public void TestISetterSuspenderSupporter_SupportedFields()
		{
			ISetterSuspenderSupporter supporter = invoiceLine;
			AssertArrayEqualsByElements(new[]
			{
				BaseJobComInvoiceLine.Schema.JI_Tariff,
				BaseJobComInvoiceLine.Schema.JI_InvoiceUQ,
				BaseJobComInvoiceLine.Schema.JI_CustomsQuantity,
				BaseJobComInvoiceLine.Schema.JI_CustomsUnitQty,
				BaseJobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
				BaseJobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
				BaseJobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
				BaseJobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
				BaseJobComInvoiceLine.Schema.JI_CustomsFourthQuantity,
				BaseJobComInvoiceLine.Schema.JI_CustomsFourthUnitQty
			}, supporter.SupportedFields.ToArray());
		}

		public void TestISetterSuspenderSupporter_SetterSuspender()
		{
			ISetterSuspenderSupporter supporter = invoiceLine;
			AssertSame(supporter.SetterSuspender, supporter.SetterSuspender);
		}

		public void TestJI_TariffSetterSuspending()
		{
			using (invoiceLine.SetterSuspender.SuspendSetting(BaseJobComInvoiceLine.Schema.JI_Tariff))
			{
				invoiceLine.JI_Tariff = "1020304050";
				AssertEquals("Suspend Setting", ZString.Empty, invoiceLine.JI_Tariff);
				using (invoiceLine.SetterSuspender.ResumeSetting(BaseJobComInvoiceLine.Schema.JI_Tariff))
				{
					invoiceLine.JI_Tariff = "1020304050";
					AssertEquals("Resume Setting", "1020304050", invoiceLine.JI_Tariff);
				}
			}
		}

		public void TestAllApplicableRates()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);

			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountryCode, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);
			Factory.Save();

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountryCode, "HSN");
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(currentCountryCode, hsnTariffType.PK, "DUMMYTRF", date1, date4, "dummy Description 0");
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(currentCountryCode, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var rateCode2 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC2", dutyRateType.PK);
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO Matched", currentCountryCode);
			Factory.Save();

			var testRate2 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceTWO.PK);
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4);
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceTWO.PK);
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4);
			Factory.Save();

			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_PrimaryPreference = "TWO";
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { testRate2.PK, testRate3.PK }, invoiceLine.AllApplicableRates.Select(x => x.PK));
		}

		public void TestRatesSelectionCriteria_Constructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<NullReferenceException>("", () => new RateSelectionCriteria<BaseJobComInvoiceLine>(null, ZString.Empty, ZString.Empty));

				var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
				invoiceLine.Delete();
				new RateSelectionCriteria<BaseJobComInvoiceLine>(invoiceLine, ZString.Empty, ZString.Empty);
				AssertContains("Should have DeveloperNotificationException", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestConditionSelectionCriteria_Constructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<NullReferenceException>("", () => new ZZConditionSelectionCriteria<BaseJobComInvoiceLine>(null));
				AssertExceptionThrown<NullReferenceException>("", () => new ZZConditionSelectionCriteria<BaseJobComInvoiceLine>(null, ZString.Empty, ZString.Empty));

				var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
				invoiceLine.Delete();
				new ZZConditionSelectionCriteria<BaseJobComInvoiceLine>(invoiceLine);
				AssertContains("Should have DeveloperNotificationException", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				new ZZConditionSelectionCriteria<BaseJobComInvoiceLine>(invoiceLine, ZString.Empty, ZString.Empty);
				AssertContains("Should have DeveloperNotificationException for constructor with conditonClass and conditionType", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestVATSelectionCriteria_Constructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<NullReferenceException>("", () => new ZZVATSelectionCriteria<BaseJobComInvoiceLine>(null));

				var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
				invoiceLine.Delete();
				new ZZVATSelectionCriteria<BaseJobComInvoiceLine>(invoiceLine);
				AssertContains("Should have DeveloperNotificationException", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestTariffAdditionalCodeSelectionCriteria_Constructor()
		{
			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2024, 03, 22, 0, 0, 0);
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "US";

			var tariffAdditionalCodeSelectionCriterias = invoiceLine.TariffAdditionalCodeSelectionCriteria;
			AssertNull("Criterias should be defined in sub classes.", invoiceLine.TariffAdditionalCodeSelectionCriteria);
		}

		public void TestRatesSelectionCriteria()
		{
			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_SecondaryPreference = "01";
			invoiceLine.JI_ConcessionOrder = "TestOrder";

			var rateSelectionCriteria = invoiceLine.AllApplicableRatesSelectionCriteria;
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("SecondTradeGroups", 0, rateSelectionCriteria.SecondTradeGroups.Count);
			AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code, rateSelectionCriteria.DataGrouping);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCodes", rateSelectionCriteria.AdditionalCodes.Contains(""));
			AssertEquals("ConcessionOrder", "TestOrder", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", "", rateSelectionCriteria.RateType);
			AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);

			rateSelectionCriteria = invoiceLine.DutyRateSelectionCriteria;
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("SecondTradeGroups", 0, rateSelectionCriteria.SecondTradeGroups.Count);
			AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code, rateSelectionCriteria.DataGrouping);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCodes", rateSelectionCriteria.AdditionalCodes.Contains(""));
			AssertEquals("ConcessionOrder", "TestOrder", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", Universal.Constants.RateTypes.Duty, rateSelectionCriteria.RateType);
			AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);

			declaration.TariffDataGrouping = "ABC";
			Factory.InvalidateCachedProperties();
			rateSelectionCriteria = invoiceLine.DutyRateSelectionCriteria;
			AssertEquals("DataGrouping", "ABC", rateSelectionCriteria.DataGrouping);
		}

		public void TestDefaultConditionSelectionCriteria()
		{
			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_SecondaryPreference = "01";
			invoiceLine.JI_ConcessionOrder = "TestOrder";

			var conditionSelectionCriteria = invoiceLine.ConditionSelectionCriterias.First();
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), conditionSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", conditionSelectionCriteria.TradeGroupCountry);
			AssertEquals("PrimaryPreference", "STD", conditionSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, conditionSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCodes", conditionSelectionCriteria.AdditionalCodes.Contains(""));
			AssertEquals("ConcessionOrder", "TestOrder", conditionSelectionCriteria.ConcessionOrder);
			AssertEquals("DataGrouping", declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff), conditionSelectionCriteria.DataGrouping);
			AssertEquals("Direction", ConditionChecker.ConditionDirection.Import, conditionSelectionCriteria.Direction);
			AssertEquals("SecondTradeGroups", 0, conditionSelectionCriteria.SecondTradeGroups.Count);

			declaration.TariffDataGrouping = "ABC";
			Factory.InvalidateCachedProperties();
			conditionSelectionCriteria = invoiceLine.ConditionSelectionCriterias.First();
			AssertEquals("DataGrouping", "ABC", conditionSelectionCriteria.DataGrouping);
		}

		public void TestVATSelectionCriteria()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = "tax1";

			var vatSelectionCriteria = invoiceLine.VATSelectionCriteria;
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), vatSelectionCriteria.EffectiveDate);
			AssertEquals("DataGrouping", declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff), vatSelectionCriteria.DataGrouping);
			AssertEquals("AdditionalCodes count", 1, vatSelectionCriteria.AdditionalCodes.Count);
			AssertEquals("AdditionalCodes", ZString.Empty, vatSelectionCriteria.AdditionalCodes.FirstOrDefault());
			AssertEquals("SecondTradeGroups", 0, vatSelectionCriteria.TradeGroups.Count);
			AssertEquals("TaxOrFeeCode", "tax1", vatSelectionCriteria.TaxOrFeeCode);
		}

		[ExpectNoExceptions]
		public void TestVATSelectionCriteriaForDeletedInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = "tax1";
			invoiceLine.Delete();
			AssertNull("should be null after delete", invoiceLine.VATSelectionCriteria);
		}

		public void TestChangeIsLinkedOnLinkPackageAndSyncTheValueToContainerPivot()
		{
			var declaration = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			declaration.JE_MasterBill = "123";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "ANYTHING";

			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = declaration.JE_MasterBill;
			package.CW_PackQty = 10;
			package.CW_PackType = "PK";
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;

			var linePack = new BaseCusLinkPackage(invoiceLine) { Package = package, IsLinked = true };
			Assert("Should be true as the container is linked.", invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			linePack.IsLinked = false;
			Assert("Should be false as the container is not linked.", !invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
		}

		public void TestSupportsChcPivotBetweenInvoiceLineAndPackingCore_AndCanDeleteChcEvenWhenNoDeclaration()
		{
			var dec = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			dec.JE_MasterBill = "123";
			var package = dec.Packages.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = Factory.New<PivotBetweenCWandJITest.BaseJobComInvoiceLineWhichSupportsPackagesPivot>();
			invLine.JI_JZ = inv.PK;
			AssertEquals(true, invLine.SupportsChcPivotBetweenInvoiceLineAndPacking);
			invLine.WipeJzForTesting();
			AssertEquals("Pre req", null, invLine.Declaration);
			AssertEquals("Still supports even when dec is null", true, invLine.SupportsChcPivotBetweenInvoiceLineAndPacking);
			var chc = Factory.New<InvoiceLinePackagePivot>();
			chc.CHC_JI = invLine.PK;
			chc.CHC_CW = package.PK;
			chc.CHC_NumberOfPacks = 1;
			invLine.Delete();
			AssertEquals(true, chc.IsDeleted);
			Factory.Save();
			Assert("Saved OK", true);
		}

		public void TestMarkAsNonPersistentOnAssigningCeiOnlyIfTheEntryInstrucitonIsNonPersistent()
		{
			var cei = Factory.New<CusEntryInstruction>();
			var ji = Factory.New<BaseJobComInvoiceLine>();
			ji.JI_CEI = cei.PK;
			AssertEquals(true, ji.IsSavedByFactory);

			cei.MakeNonPersistent();
			ji = Factory.New<BaseJobComInvoiceLine>();
			ji.JI_CEI = cei.PK;
			AssertEquals(false, ji.IsSavedByFactory);
		}

		public void TestToggleLinkageWithPackage()
		{
			var dec = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			dec.JE_MasterBill = "123";
			var cw = dec.Packages.AddNew();
			cw.CW_HouseBill = dec.JE_MasterBill;
			cw.CW_PackQty = 10;
			cw.CW_PackType = "PK";
			dec.Invoices.AddNew();
			var invLine = dec.Invoices[0].InvoiceLines.AddNew();
			var pivot = invLine.ToggleLinkageWithPackage(cw, true);
			AssertEquals(10, pivot.NumberOfPacks);
		}

		public void TestToggleLinkageWithPackageUpdateLinkageWithContainer_Ryken()
		{
			var dec = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			dec.JE_MasterBill = "123";
			var container = dec.CusContainers.AddNew();
			var houseBill = dec.Bills.AddNew();
			var packingGroup = houseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;

			var package = dec.Packages.AddNew();
			package.CW_HouseBill = dec.JE_MasterBill;
			package.CW_PackQty = 10;
			package.CW_PackType = "PK";
			package.CW_CR_HouseContainer = packingGroup.PK;
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;

			invLine.ToggleLinkageWithPackage(package, true);
			Assert(invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			invLine.ToggleLinkageWithPackage(package, false);
			Assert(!invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
		}

		public void TestToggleLinkageWithPackageUpdateLinkageWithContainer_Daniel()
		{
			var dec = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			dec.JE_MasterBill = "123";
			var container = dec.CusContainers.AddNew();
			container.CO_ContainerNumber = "ANYTHING";
			var package = dec.Packages.AddNew();
			package.CW_HouseBill = dec.JE_MasterBill;
			package.CW_PackQty = 10;
			package.CW_PackType = "PK";
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;

			invLine.ToggleLinkageWithPackage(package, true);
			Assert(invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			invLine.ToggleLinkageWithPackage(package, false);
			Assert(!invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
		}

		public void TestSetDefaultValuesForNewPackableItem()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_Description = "desc line 1";
			line1.JI_InvoiceQuantity = 2m;
			line1.JI_InvoiceUQ = "BAG";
			line1.JI_NetWeight = 50m;
			line1.JI_NetWeightUQ = "KG";

			var line2 = invoice.InvoiceLines.AddNew();
			ZString descForTesting = "A1" + ZString.Replicate('Z', line2.JI_DescriptionInfo.MaxLength - 2);
			line2.JI_Description = descForTesting;
			line2.JI_InvoiceQuantity = 3m;
			line2.JI_InvoiceUQ = "BBG";
			line2.JI_NetWeight = 100m;
			line2.JI_NetWeightUQ = "LB";

			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var package = packingList.PackageJob.Packages.AddNew();
			_ = package.PackableItemRelataions;
			var items = packingList.PackableItems;
			AssertEquals(2, items.Count);
			var item = items.First();
			AssertEquals("desc line 1", item.CUI_GoodsDescription);
			AssertEquals(2m, item.CUI_PackableQty);
			AssertEquals("BAG", item.CUI_PackableUQ);
			AssertEquals(50m, item.CUI_NetWeight);
			AssertEquals("KG", item.CUI_NetWeightUQ);
			item = items.ElementAt(1);
			var expectedGoodsDesc = descForTesting.Left(CusPackableItem.Schema.CUI_GoodsDescriptionMaxLength);
			AssertEquals(expectedGoodsDesc, item.CUI_GoodsDescription);
			AssertEquals(3m, item.CUI_PackableQty);
			AssertEquals("BBG", item.CUI_PackableUQ);
			AssertEquals(100m, item.CUI_NetWeight);
			AssertEquals("LB", item.CUI_NetWeightUQ);
		}

		public void TestCustomsCountryOfJurisdictionIsUsed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				var org1 = Factory.New<OrgHeader>();
				org1.OH_Code = "KD23423";
				org1.OH_FullName = "BOB THE BUILDER";
				org1.OH_RL_NKClosestPort = "USLAX";
				org1.MainAddress.OA_Address1 = "ADD 1";
				org1.OH_IsConsignee = true;
				var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
				part.OP_PartNum = "TSD234";
				part.OP_Desc = "TEST DATA";
				part.RelatedOrganisations.AddOwner(org1);

				var classification = (BaseCusClassification)Factory.New<Integration.Customs.US.ICusClassification>();
				classification.CC_TariffNum = "101010";
				classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				classification.CC_LookupCode = "KD2433";
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				classification.CC_Description = "HELLO WORLD";

				var pivot = (BaseCusClassPartPivot)Factory.New<Integration.Customs.US.ICusClassPartPivot>();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part.PK;
				pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = org1.PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = dec.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "TSD234";
				AssertEquals("JI_OP", part.PK, invoiceLine.JI_OP);
				AssertEquals("Pivot", pivot, invoiceLine.Pivot);
				AssertEquals("Classification", classification, invoiceLine.Classification);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var invoiceLineInDiff = newFactory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
				var decInDiff = invoiceLineInDiff.Declaration;
				AssertEquals(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>(), decInDiff.GetType());
				AssertEquals("CountryCode", Core.Constants.CountryCodes.PuertoRico, decInDiff.CountryCode);
				AssertEquals("CountryCode", Core.Constants.CountryCodes.UnitedStates, ((IUnitConverterDataProvider)invoiceLineInDiff).CountryCode);
				AssertEquals("TypeOfPartUsed", ObjectFactory.GetType<Integration.Customs.US.IOrgSupplierPart>(), invoiceLineInDiff.TypeOfPartUsed);
				AssertEquals("Pivot", ObjectFactory.GetType<Integration.Customs.US.ICusClassPartPivot>(), invoiceLineInDiff.Pivot.GetType());
				AssertEquals("Classification", ObjectFactory.GetType<Integration.Customs.US.ICusClassification>(), invoiceLineInDiff.Classification.GetType());
			}
		}

		public void TestIsChangeOfRegimeWarehousing()
		{
			var helper = new WhsDataTestHelper(Factory);
			CombineAssertions(() =>
			{
				var data = helper.CreateChangeOfRegimeEntryData();
				var procedure = helper.ChangeOfOwnershipCusProcedure;
				var invoiceLine = data.InvoiceLine;
				AssertEquals("IsChangeOfRegimeWarehousing", true, invoiceLine.IsChangeOfRegimeWarehousing);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
				AssertEquals("procedure.ZZ6_IntoWarehouse is No", false, invoiceLine.IsChangeOfRegimeWarehousing);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
				AssertEquals("procedure.ZZ6_OutOfWarehouse is No", false, invoiceLine.IsChangeOfRegimeWarehousing);

				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
				var importerCompanyData = helper.Importer.CompanyData;
				importerCompanyData.OB_CusInventoryForInwardProcessing = false;
				AssertEquals("importerCompanyData.OB_CusInventoryForInwardProcessing is false", false, invoiceLine.IsChangeOfRegimeWarehousing);

				importerCompanyData.OB_CusInventoryForInwardProcessing = true;
				importerCompanyData.OB_IMUsedBondedWhs = false;
				AssertEquals("importerCompanyData.OB_IMUsedBondedWhs is false", false, invoiceLine.IsChangeOfRegimeWarehousing);

				importerCompanyData.OB_IMUsedBondedWhs = true;
				var warehouse2CompanyData = helper.Warehouse2.CompanyData;
				warehouse2CompanyData.OB_CusInventoryForInwardProcessing = false;
				AssertEquals("warehouse2CompanyData.OB_CusInventoryForInwardProcessing is false", false, invoiceLine.IsChangeOfRegimeWarehousing);

				warehouse2CompanyData.OB_CusInventoryForInwardProcessing = true;
				var warehouseCompanyData = helper.Warehouse.CompanyData;
				warehouseCompanyData.OB_IMUsedBondedWhs = false;
				AssertEquals("warehouseCompanyData.OB_IMUsedBondedWhs is false", false, invoiceLine.IsChangeOfRegimeWarehousing);

				data.Entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreated;
				AssertEquals("entry.HasWHSChangeOfRegimeTransaction is true", true, invoiceLine.IsChangeOfRegimeWarehousing);
			});
		}

		public void TestWarehouseFlagsAndComponentPKs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var helper = new WhsDataTestHelper(Factory);
				helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
				helper.Owner.CompanyData.OB_IMUsedBondedWhs = true;
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = false;
				helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = false;
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_PreviousProcedureCode = "10";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Eritrea;
				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

				var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = helper.Importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				entryInstruction.CEI_OH_Owner = helper.Owner.PK;
				entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				entryInstruction.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", false, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", false, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", false, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", false, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				invoiceLine.JI_CEI = entryInstruction.PK;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", false, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", false, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", false, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", false, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				invoiceLine.JI_Procedure = "AB10";
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", true, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", true, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", true, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", true, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", true, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", true, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
				helper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", true, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", true, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", true, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", true, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", true, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", true, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = false;
				helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
				helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", true, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", true, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", true, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", false, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", true, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", true, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", false, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", true, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", false, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", true, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", false, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", false, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", false, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", false, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", false, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", false, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", false, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", false, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", false, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", false, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", false, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", true, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				procedure.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoVATWarehouse = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, invoiceLine.HasBothOutOfAndIntoRegimeProcedure);
				AssertEquals("HasIntoWarehouseProcedure", false, invoiceLine.HasIntoWarehouseProcedure);
				AssertEquals("HasIntoVATWarehouseProcedure", true, invoiceLine.HasIntoVATWarehouseProcedure);
				AssertEquals("HasOutOfWarehouseProcedure", false, invoiceLine.HasOutOfWarehouseProcedure);
				AssertEquals("IsIntoOrOutOfWarehouseProcedure", false, invoiceLine.IsIntoOrOutOfRegimeProcedure);
				AssertEquals("IsChangeOfOwnershipWarehousing", false, invoiceLine.IsChangeOfOwnershipWarehousing);
				AssertEquals("IsIntoWarehouse", false, invoiceLine.IsIntoWarehouseWarehousing);
				AssertEquals("IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
				AssertEquals("IsGoingIntoBondedWarehouse", false, invoiceLine.IsGoingIntoBondedWarehouse);
				AssertEquals("UseBondedWarehouseAutomation", false, invoiceLine.UseBondedWarehouseAutomation);

				procedure.ZZ6_IntoVATWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = "";
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = false;
				helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
				AssertEquals("HasOutOfInwardProcessingProcedure", false, invoiceLine2.HasOutOfInwardProcessingProcedure);
				AssertEquals("HasOutOfOutwardProcessingProcedure", false, invoiceLine2.HasOutOfOutwardProcessingProcedure);
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
				AssertEquals("HasOutOfInwardProcessingProcedure", false, invoiceLine2.HasOutOfInwardProcessingProcedure);
				AssertEquals("HasOutOfOutwardProcessingProcedure", false, invoiceLine2.HasOutOfOutwardProcessingProcedure);
				invoiceLine2.JI_Procedure = "AB10";
				AssertEquals("HasOutOfInwardProcessingProcedure", true, invoiceLine2.HasOutOfInwardProcessingProcedure);
				AssertEquals("HasOutOfOutwardProcessingProcedure", true, invoiceLine2.HasOutOfOutwardProcessingProcedure);
			}
		}

		public void TestInventorySelections()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var helper = new WhsDataTestHelper(Factory);
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = helper.Importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				entryInstruction.CEI_OH_Owner = helper.Owner.PK;
				entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				entryInstruction.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				AssertEquals("IWarehouseIntegrationSupporter WarehouseAddress", helper.Warehouse.MainAddress, invoiceLine.WarehouseForComponentInventories);
			}
		}

		public void TestCusProcedure()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("ProcedureCode", "", invoiceLine.ProcedureCode);
			invoiceLine.JI_Procedure = "ABCD";
			AssertEquals("ProcedureCode", "AB", invoiceLine.ProcedureCode);
			AssertEquals("JI_Calc_PreviousProcedure", "CD", invoiceLine.JI_Calc_PreviousProcedure);
			invoiceLine.JI_Procedure = "CD00";
			AssertEquals("ProcedureCode", "CD", invoiceLine.ProcedureCode);
			AssertEquals("JI_Calc_PreviousProcedure", "00", invoiceLine.JI_Calc_PreviousProcedure);
		}

		public void TestPreviousProcedureIsNothingToDoWithCeiStyle()
		{
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ProcedureCode = "AB";
			procedure1.ZZ6_PreviousProcedureCode = "CD";
			procedure1.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ProcedureCode = "CD";
			procedure2.ZZ6_PreviousProcedureCode = "10";
			procedure2.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var procedure3 = Factory.New<RefCusProcedure>();
			procedure3.ZZ6_ProcedureCode = "CD";
			procedure3.ZZ6_PreviousProcedureCode = "";
			procedure3.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.Country.Code;

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "Not CPC";  // something other than a procedure code
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "Anyfing";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction1.PK;
			AssertNull(invoiceLine.PreviousProcedure);
			invoiceLine.JI_Procedure = "1000";
			AssertNull(invoiceLine.PreviousProcedure);
			invoiceLine.JI_Procedure = "10CD";
			AssertEquals("PreviousProcedure", procedure3, invoiceLine.PreviousProcedure);
		}

		public void TestImportExport()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			Assert(invoiceLine.IsImport);
			Assert(!invoiceLine.IsExport);

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			Assert(!invoiceLine.IsImport);
			Assert(invoiceLine.IsExport);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_JE = declaration.PK;
			Assert(invoiceLine.IsImport);
			Assert(!invoiceLine.IsExport);
		}

		public void TestInvoiceLineRefsIsNotLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ref1 = Factory.New<JobComInvLineRefs>();
			ref1.JG_JI = invoiceLine.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedInvoiceLine = newFactory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
			loadedInvoiceLine.LoadChildEditableObjects();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(JobComInvLineRefs.Schema.TableName));
			AssertEquals(0, loadedInvoiceLine.InvoiceLineRefs.Count);
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(JobComInvLineRefs.Schema.TableName));
			loadedInvoiceLine.InvoiceLineRefs.Load();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(JobComInvLineRefs.Schema.TableName));
		}

		public void TestSupportsBondedWarehousing()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				AssertEquals(false, invoiceLine.SupportsBondedWarehousing);
				declaration.SetSupportsBondedWarehousingForTesting(true);
				AssertEquals(true, invoiceLine.SupportsBondedWarehousing);
				invoice.JZ_JE = ZGuid.Missing;
				invoiceLine.JI_JZ = ZGuid.Empty;
				invoiceLine.JI_JZ = invoice.PK;
				AssertEquals(false, invoiceLine.SupportsBondedWarehousing);
			}
		}

		public void TestLightValidationIsValid()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declarationMock = Factory.New<BaseJobDeclarationForTesting>();
			declarationMock.HasNotificationsNotIncludingChildrenReturns = false;
			declaration = declarationMock;

			var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
			var invoiceLineMockProtected = invoiceLineMock.Protected();
			invoiceLineMockProtected.Setup<bool>("HasNotificationsNotIncludingChildren", ItExpr.IsAny<INotificationType>()).Returns<INotificationType>((notificationType) => false);

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceLineMock.Object;
			invoiceLine.JI_JZ = invoiceHeader.PK;
			invoiceHeader.JobComInvoiceLines.Add(invoiceLine);
			declaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoiceHeader.JZ_OH_Buyer = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoiceLine.JI_LineNo = 2;
			var declarationAddInfo = GetAddInfoAndPopulateWithData(declaration);
			var invoiceLineAddInfo = GetAddInfoAndPopulateWithData(invoiceLine);

			AssertEquals("Declaration.IsValid initially", false, ((ILightValidationInternals)declaration).IsValid);
			AssertEquals("InvoiceLine.IsValid initially", false, ((ILightValidationInternals)invoiceLine).IsValid);
			if (declarationAddInfo is ILightValidationInternals)
			{
				AssertEquals("Declaration.AddInfo.IsValid initially", false, ((ILightValidationInternals)declarationAddInfo).IsValid);
			}
			if (invoiceLineAddInfo is ILightValidationInternals)
			{
				AssertEquals("InvoiceLine.AddInfo.IsValid initially", false, ((ILightValidationInternals)invoiceLineAddInfo).IsValid);
			}

			declaration.LoadChildEditableObjects();
			declaration.RunPreSaveValidation();
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<BaseJobDeclaration>(declaration.PK);
			invoiceLine = declaration.InvoiceLines[0];

			declarationAddInfo = GetAddInfoAndPopulateWithData(declaration); // accessing the AddInfos may cause a MarkAsNeedingValidation to occur
			invoiceLineAddInfo = GetAddInfoAndPopulateWithData(invoiceLine);
			AssertEquals("InvoiceLine.HasChanges after save", false, invoiceLine.HasChanges);
			AssertEquals("Declaration.IsValid after save", true, ((ILightValidationInternals)declaration).IsValid);
			AssertEquals("InvoiceLine.IsValid after save", true, ((ILightValidationInternals)invoiceLine).IsValid);
			if (declarationAddInfo is ILightValidationInternals)
			{
				AssertEquals("Declaration.AddInfo.IsValid after save", true, ((ILightValidationInternals)declarationAddInfo).IsValid);
			}
			if (invoiceLineAddInfo is ILightValidationInternals)
			{
				AssertEquals("InvoiceLine.AddInfo.IsValid after save", true, ((ILightValidationInternals)invoiceLineAddInfo).IsValid);
			}
		}

		public void TestCloneDoesNotSynchroniseInvoiceLineWithProduct()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "PART1";
			var ou = product.RelatedOrganisations.AddNew();
			ou.OU_OH = org.PK;
			ou.OU_Relationship = "SUP";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_OH = ou.OU_OH;
			pivot.CI_TariffNum = "6969696969";
			var classification = product.ClassificationsForBinding.AddNew();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = "EXP";
			pivot.CI_CC = classification.PK;
			var sourceDec = Factory.New<BaseJobDeclaration>();
			sourceDec.JE_OH_Supplier = org.PK;
			var sourceInv = sourceDec.Invoices.AddNew();
			var sourceInvLine = sourceInv.InvoiceLines.AddNew();
			Factory.Save();
			sourceInvLine.JI_PartNo = product.OP_PartNum;
			sourceInvLine.JI_Tariff = "111111";

			var cloned = sourceDec.GetNewRelatedDeclaration(Factory);
			AssertEquals("Should get JI's own tariff WITHOUT synching from Product", "111111", cloned.InvoiceLines[0].JI_Tariff);
		}

		public void TestCloneDeclarationWithUnexpectedVolume()
		{
			ErrorReporter.Clear();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "TestRef";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.879m;
			invoice.JZ_InvoiceNumber = "Inv123";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "UIYRYUI345";
			invoiceLine.JI_Description = "Socks";
			invoiceLine.JI_CountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "1111";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KG";

			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_InvoiceQuantity = 300m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_Weight = 400m;
			invoiceLine.JI_WeightUQ = "KG";

			invoiceLine.JI_Volume = 1000001m;
			invoiceLine.JI_VolumeUQ = "L";

			var clonedDeclaration = declaration.GetNewRelatedDeclaration(Factory);
			clonedDeclaration.JE_OperationalStatus = "OK";
			AssertEquals("TestRef", clonedDeclaration.ClonedFromDeclarationReference);
			AssertNoExceptionThrown(() => { clonedDeclaration.Factory.Save(); });
		}

		public void TestCloneDeclarationWithTooLargeWeight()
		{
			ErrorReporter.Clear();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "TestRef";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.879m;
			invoice.JZ_InvoiceNumber = "Inv123";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "UIYRYUI345";
			invoiceLine.JI_Description = "Socks";
			invoiceLine.JI_CountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "1111";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KG";

			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_InvoiceQuantity = 300m;
			invoiceLine.JI_InvoiceUQ = "NO";

			invoiceLine.JI_Weight = 1000001m;
			invoiceLine.JI_WeightUQ = "KG";

			invoiceLine.JI_NetWeight = 1000m;
			invoiceLine.JI_NetWeightUQ = "KG";

			invoiceLine.JI_Volume = 1000m;
			invoiceLine.JI_VolumeUQ = "L";

			var clonedDeclaration = declaration.GetNewRelatedDeclaration(Factory);
			clonedDeclaration.JE_OperationalStatus = "OK";
			AssertEquals("TestRef", clonedDeclaration.ClonedFromDeclarationReference);
			AssertNoExceptionThrown(() => { clonedDeclaration.Factory.Save(); });
		}

		public void TestCloneDeclarationWithTooLargeNetWeight()
		{
			ErrorReporter.Clear();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "TestRef";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.879m;
			invoice.JZ_InvoiceNumber = "Inv123";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "UIYRYUI345";
			invoiceLine.JI_Description = "Socks";
			invoiceLine.JI_CountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "1111";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KG";

			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_InvoiceQuantity = 300m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_Weight = 400m;
			invoiceLine.JI_WeightUQ = "KG";

			invoiceLine.JI_NetWeight = 1000001m;
			invoiceLine.JI_NetWeightUQ = "KG";

			invoiceLine.JI_Volume = 1000m;
			invoiceLine.JI_VolumeUQ = "L";

			var clonedDeclaration = declaration.GetNewRelatedDeclaration(Factory);
			clonedDeclaration.JE_OperationalStatus = "OK";
			AssertEquals("TestRef", clonedDeclaration.ClonedFromDeclarationReference);
			AssertNoExceptionThrown(() => { clonedDeclaration.Factory.Save(); });
		}

		public void TestRulingConfigurationsIsNotLoaded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var config = Factory.New<CusRulingConfigCombined>();
			config.ZZY_Type = "Min";
			config.ZZY_Category = "DTY";
			config.ZZY_JI_InvoiceLine = invoiceLine.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedInvoiceLine = newFactory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
			loadedInvoiceLine.LoadChildEditableObjects();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusRulingConfigCombined.Schema.TableName));
			AssertEquals(0, loadedInvoiceLine.RulingConfigurations.Count);
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusRulingConfigCombined.Schema.TableName));
			loadedInvoiceLine.RulingConfigurations.Load();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusRulingConfigCombined.Schema.TableName));
		}

		public void TestCopyCommodityFromProduct()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AMYT";
			commodity.RH_IsPerishable = true;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART1";
			product.OP_Weight = 100m;
			product.OP_WeightUQ = "HG";
			product.RelatedOrganisations.AddSupplier(consignor);
			product.RelatedOrganisations.AddOwner(consignee);
			product.OP_RH_NKCommodityCode = commodity.RH_Code;

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);

			AssertEquals(product.OP_RH_NKCommodityCode, invoiceLine.JI_RH_NKCommodity_Code);
		}

		public void TestCopyCustomFieldsFromProduct()
		{
			var lineWorkflow = CreateWorkflowTemplate(WorkflowDescriptors.CommericalInvoiceLineWorkflowDescriptorCode);
			DefineCustomField<ZString>(lineWorkflow, "String Value");
			DefineCustomField<ZInt>(lineWorkflow, "Integer Value");
			DefineCustomField<ZDecimal>(lineWorkflow, "Decimal Value");
			DefineCustomField<ZDateTime>(lineWorkflow, "DateTime Value");
			DefineCustomField<ZBool>(lineWorkflow, "Boolean Value");
			DefineCustomField<ZString>(lineWorkflow, "String Value - Check No Overwrite");
			DefineCustomField<ZString>(lineWorkflow, "String Value - For Line Only");

			var partWorkflow = CreateWorkflowTemplate(WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode);
			DefineCustomField<ZString>(partWorkflow, "String Value");
			DefineCustomField<ZInt>(partWorkflow, "Integer Value");
			DefineCustomField<ZDecimal>(partWorkflow, "Decimal Value");
			DefineCustomField<ZDateTime>(partWorkflow, "DateTime Value");
			DefineCustomField<ZBool>(partWorkflow, "Boolean Value");
			DefineCustomField<ZString>(partWorkflow, "String Value - Check No Overwrite");
			DefineCustomField<ZString>(partWorkflow, "String Value - For Product Only");

			Factory.Save();

			var product = SetupProduct();
			product.OP_PartNum = "PART WITH CUSTOM VALUES";
			SetCustomField<ZString>(product, "String Value", "AAA");
			SetCustomField<ZInt>(product, "Integer Value", 111);
			SetCustomField<ZDecimal>(product, "Decimal Value", 2.333);
			SetCustomField<ZDateTime>(product, "DateTime Value", new DateTime(2024, 01, 02));
			SetCustomField<ZBool>(product, "Boolean Value", true);
			SetCustomField<ZString>(product, "String Value - Check No Overwrite", "BBB");
			SetCustomField<ZString>(product, "String Value - For Product Only", "CCC");

			var invoiceLine = invoice.InvoiceLines.AddNew();
			SetCustomField<ZString>(invoiceLine, "String Value - Check No Overwrite", "XXX");
			SetCustomField<ZString>(invoiceLine, "String Value - For Line Only", "ZZZ");
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertCustomField<ZString>(invoiceLine, "String Value", "AAA");
			AssertCustomField<ZInt>(invoiceLine, "Integer Value", 111);
			AssertCustomField<ZDecimal>(invoiceLine, "Decimal Value", 2.333);
			AssertCustomField<ZDateTime>(invoiceLine, "DateTime Value", new DateTime(2024, 01, 02));
			AssertCustomField<ZBool>(invoiceLine, "Boolean Value", true);
			AssertCustomField<ZString>(invoiceLine, "String Value - Check No Overwrite", "XXX");
			AssertCustomField<ZString>(invoiceLine, "String Value - For Line Only", "ZZZ");

			ProcessTaskTemplate CreateWorkflowTemplate(ZString processType)
			{
				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = processType;
				template.P0_IsActive = true;
				return template;
			}

			void DefineCustomField<T>(ProcessTaskTemplate template, ZString name)
				where T : IZType
			{
				var field = template.GenCustomColumnDefinitions.AddNew();
				field.XC_Name = name;
				field.XC_Type = AddOnColumnDataType.GetCodeFromType(typeof(T));
			}

			void SetCustomField<T>(ICustomFieldProvider bizo, ZString name, T value)
				where T : IZType
			{
				var identifier = CustomPropertyHelper.GeneratePropertyIdentifier(name, typeof(T));
				var cusBizo = bizo.GetCustomBusinessObject();
				cusBizo[identifier] = value;
			}

			void AssertCustomField<T>(ICustomFieldProvider bizo, ZString name, T value)
				where T : IZType
			{
				var identifier = CustomPropertyHelper.GeneratePropertyIdentifier(name, typeof(T));
				var cusBizo = bizo.GetCustomBusinessObject();
				AssertEquals(name, value, (T)cusBizo[identifier]);
			}
		}

		public void TestUnitConvertWhenNetAndGrossWeightAreEntered()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";

			product.OP_StockKeepingUnit = "UNT";
			product.OP_Weight = 0.52m;
			product.OP_WeightUQ = "KG";
			product.OP_NetWeight = 0.51m;

			var productUnitConversion = product.PartUnits.AddNew();
			productUnitConversion.OF_QuantityInParent = 0.13m;
			productUnitConversion.OF_PackType = "KG";
			productUnitConversion.OF_ParentPackType = "NO";

			productUnitConversion = product.PartUnits.AddNew();
			productUnitConversion.OF_QuantityInParent = 4m;
			productUnitConversion.OF_PackType = "NO";
			productUnitConversion.OF_ParentPackType = "UNT";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Net weight is calculated", 5.1m, invoiceLine.JI_NetWeight);
			AssertEquals("Net weight UQ", "KG", invoiceLine.JI_NetWeightUQ);
		}

		public void TestUnitConvertWhenNetWeightAloneIsEntered()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";

			product.OP_StockKeepingUnit = "UNT";
			//no gross weight is entered
			product.OP_WeightUQ = "KG";
			product.OP_NetWeight = 0.51m;

			var productUnitConversion = product.PartUnits.AddNew();
			productUnitConversion.OF_QuantityInParent = 0.13m;
			productUnitConversion.OF_PackType = "KG";
			productUnitConversion.OF_ParentPackType = "NO";

			productUnitConversion = product.PartUnits.AddNew();
			productUnitConversion.OF_QuantityInParent = 4m;
			productUnitConversion.OF_PackType = "NO";
			productUnitConversion.OF_ParentPackType = "UNT";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Net weight is calculated", 5.1m, invoiceLine.JI_NetWeight);
			AssertEquals("Net weight UQ", "KG", invoiceLine.JI_NetWeightUQ);
		}

		public void TestProductSpecificConversion()
		{
			var cusRefPacks = Factory.New<CusRefPacks>();
			cusRefPacks.RP_CommercialPack = "PCE";
			cusRefPacks.RP_CustomsPack = "NO";
			cusRefPacks.RP_ConversionFactor = 1m;

			cusRefPacks = Factory.New<CusRefPacks>();
			cusRefPacks.RP_CommercialPack = "UNT";
			cusRefPacks.RP_CustomsPack = "NO";
			cusRefPacks.RP_ConversionFactor = 1m;

			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";

			product.OP_StockKeepingUnit = "UNT";

			var productUnitConversion = product.PartUnits.AddNew();
			productUnitConversion.OF_QuantityInParent = 0.13m;
			productUnitConversion.OF_PackType = "KG";
			productUnitConversion.OF_ParentPackType = "PCE";

			productUnitConversion = product.PartUnits.AddNew();
			productUnitConversion.OF_QuantityInParent = 4m;
			productUnitConversion.OF_PackType = "PCE";
			productUnitConversion.OF_ParentPackType = "UNT";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_InvoiceQuantity = 1m;

			AssertEquals("PreCondition", "UNT", invoiceLine.JI_InvoiceUQ);
			AssertEquals("It should use product-specific conversion first", 0.52m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestUnitConvertWhenNoWeightConversionIsEntered()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";

			product.OP_StockKeepingUnit = "UNT";
			//no gross weight is entered
			product.OP_WeightUQ = "KG";
			product.OP_NetWeight = 0.51m;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Net weight is calculated", 5.1m, invoiceLine.JI_NetWeight);
			AssertEquals("Net weight UQ", "KG", invoiceLine.JI_NetWeightUQ);
		}

		public void TestConvertFromNetWeight()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 100m;
			invoiceLine.JI_NetWeightUQ = "KG";

			invoiceLine.JI_CustomsUnitQty = "T";
			AssertEquals(0.1m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestICommonNonApportionedChargeProvider()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ICommonNonApportionedChargeProvider<BaseInvoiceLineCharge> provider = invoiceLine;
			AssertEquals(0, invoiceLine.Charges.Count);
			var charge1 = provider.CreateNew();
			AssertEquals(1, invoiceLine.Charges.Count);
			AssertEquals(charge1, invoiceLine.Charges[0]);
			var charge2 = provider.CreateNew();
			AssertEquals(2, invoiceLine.Charges.Count);
			AssertEquals(charge1, invoiceLine.Charges[0]);
			AssertEquals(charge2, invoiceLine.Charges[1]);
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge1.J7_Amount = ZDecimal.Zero;
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge2.J7_Amount = ZDecimal.Zero;
			AssertEquals(charge1, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasInsurance));
			AssertEquals(charge2, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasFreight));
			charge1.J7_Amount = 10m;
			AssertNull(provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasInsurance));
			AssertEquals(charge2, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasFreight));
		}

		public void TestExtendedCommercialDescription()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";
			part.OP_Desc = "Description";
			part.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description, "EXTENDED DESCRIPTION");
			part.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Both);
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = owner.PK;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoiceLine.IsExtendedCommercialDescriptionEnabledReturns = false;
			invoiceLine.JI_JZ = invoice.PK;
			AssertEquals("", invoiceLine.PartExtendedCommercialDescription);
			AssertEquals("", invoiceLine.JI_ExtraInfoForClassification);
			invoiceLine.JI_PartNo = "PART";
			AssertEquals("", invoiceLine.PartExtendedCommercialDescription);
			AssertEquals("", invoiceLine.JI_ExtraInfoForClassification);

			invoiceLine.JI_PartNo = "";
			AssertEquals("", invoiceLine.PartExtendedCommercialDescription);
			AssertEquals("", invoiceLine.JI_ExtraInfoForClassification);

			invoiceLine.IsExtendedCommercialDescriptionEnabledReturns = true;
			invoiceLine.JI_PartNo = "PART";
			AssertEquals("EXTENDED DESCRIPTION", invoiceLine.PartExtendedCommercialDescription);
			AssertEquals("EXTENDED DESCRIPTION", invoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestCalculatingUnitPriceWhenQuantityChanges()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 2000m;
			invoiceLine.JI_InvoiceUQ = "M2";

			invoiceLine.JI_LinePrice = 100000m;
			AssertEquals("UnitPrice calculated", 50m, invoiceLine.UnitPrice);

			invoiceLine.JI_InvoiceQuantity = 1999m;
			AssertEquals("Unit Price should be updated, not invoice line price", 100000m, invoiceLine.JI_LinePrice);
			AssertEquals("Unit price updated", 50.0250m, invoiceLine.UnitPrice);

			invoiceLine.UnitPrice = 60m;
			AssertEquals("Line Price should be updated", 119940m, invoiceLine.JI_LinePrice);
		}

		public void TestUNDGsAreDefaultedFromProduct()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var contact1 = owner.Contacts.AddNew();
			contact1.OC_ContactName = "BOB";
			var contact2 = owner.Contacts.AddNew();
			contact2.OC_ContactName = "JOE";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTZ234";
			part.OP_Desc = "Description";
			part.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Both);
			var undg1 = part.UNDGs.TryGetOrCreate("3208A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			undg1.DI_DGFlashPoint = 10m;
			undg1.DI_OC_DGContact = contact1.PK;
			undg1.DI_TechnicalName = "TECH METALLIC SUBSTANCE";
			undg1.DI_MPMarinePollutant = YesNoList.Codes.Yes;
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.IsUNDGSupportedOnInvoiceLinesReturns = false;

			declaration.JE_OH_Importer = owner.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.UNDGs.Count);
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertEquals(0, invoiceLine.UNDGs.Count);

			invoiceLine.JI_PartNo = "";
			AssertEquals(0, invoiceLine.UNDGs.Count);

			declaration.IsUNDGSupportedOnInvoiceLinesReturns = true;
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertEquals(1, invoiceLine.UNDGs.Count);
			AssertUNDG(invoiceLine.UNDGs[0], "3208a", 10m, contact1.PK, "TECH METALLIC SUBSTANCE", YesNoList.Codes.Yes);

			invoiceLine.JI_PartNo = "";
			invoiceLine.UNDGs.DeleteAll();
			var undg2 = part.UNDGs.TryGetOrCreate("2015A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			undg2.DI_DGFlashPoint = -14m;
			undg2.DI_OC_DGContact = contact2.PK;
			Factory.Save();
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertEquals(0, invoiceLine.UNDGs.Count);
		}

		public void TestIsUNDGSupported()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var contact1 = owner.Contacts.AddNew();
			contact1.OC_ContactName = "BOB";
			var contact2 = owner.Contacts.AddNew();
			contact2.OC_ContactName = "JOE";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTZ234";
			part.OP_Desc = "Description";
			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Both);
			var undg1 = part.UNDGs.TryGetOrCreate("3208A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			undg1.DI_DGFlashPoint = 10m;
			undg1.DI_OC_DGContact = contact1.PK;
			undg1.DI_TechnicalName = "TECH METALLIC SUBSTANCE";
			undg1.DI_MPMarinePollutant = YesNoList.Codes.Yes;
			Factory.Save();

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_OH_Supplier = owner.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertNoExceptionThrown("No exceptions for standalone Commercial Invoice Imported from .csv file",
				delegate
				{ invoiceLine.JI_PartNo = part.OP_PartNum; });
		}

		public void TestClearingPartNoClearsAttributes()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartAttrib1 = "AT1";
			invoiceLine.JI_PartAttrib2 = "AT2";
			invoiceLine.JI_PartAttrib3 = "AT3";
			invoiceLine.JI_SerialNumber = "SN1";
			invoiceLine.JI_PartNo = "PART1";
			AssertEquals("AT1", invoiceLine.JI_PartAttrib1);
			AssertEquals("AT2", invoiceLine.JI_PartAttrib2);
			AssertEquals("AT3", invoiceLine.JI_PartAttrib3);
			AssertEquals("SN1", invoiceLine.JI_SerialNumber);
			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.JI_PartAttrib1);
			AssertEquals(ZString.Empty, invoiceLine.JI_PartAttrib2);
			AssertEquals(ZString.Empty, invoiceLine.JI_PartAttrib3);
			AssertEquals(ZString.Empty, invoiceLine.JI_SerialNumber);
		}

		public void TestJI_SerialNumberReadOnly()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Read only if parent id is empty", false, invoiceLine.JI_SerialNumberInfo.ReadOnly);

			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			AssertEquals("Read only if parent id not empty", true, invoiceLine.JI_SerialNumberInfo.ReadOnly);
		}

		public void TestJI_ExtraInfoForClassification()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("", invoiceLine.JI_ExtraInfoForClassification);
			AssertEquals(true, invoiceLine.JI_ExtraInfoForClassificationInfo.ReadOnly);
			invoiceLine.JI_Description = "HELLO WORLD";
			AssertEquals("", invoiceLine.JI_ExtraInfoForClassification);
			AssertEquals(false, invoiceLine.JI_ExtraInfoForClassificationInfo.ReadOnly);
			invoiceLine.JI_ExtraInfoForClassification = "BYE WORLD";
			AssertEquals("BYE WORLD", invoiceLine.JI_ExtraInfoForClassification);
			AssertEquals(false, invoiceLine.JI_ExtraInfoForClassificationInfo.ReadOnly);
			invoiceLine.JI_Description = "";
			AssertEquals("", invoiceLine.JI_ExtraInfoForClassification);
			AssertEquals(true, invoiceLine.JI_ExtraInfoForClassificationInfo.ReadOnly);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IRegistryAccessingSupporter supporter = invoiceLine;

			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			invoice.JZ_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), supporter.RegistryBranchPK);

			invoice.JZ_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);
		}

		public void TestIUltimateDistributee()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "Supply All";
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.879m;
			invoice.JZ_InvoiceNumber = "Inv123";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "UIYRYUI345";
			invoiceLine.JI_Description = "Socks";
			invoiceLine.JI_CountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "1111";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KG";

			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_InvoiceQuantity = 300m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_Weight = 400m;
			invoiceLine.JI_WeightUQ = "KG";

			invoiceLine.JI_Volume = 500m;
			invoiceLine.JI_VolumeUQ = "L";

			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_DutyPercent = 10m;
			entryLine.CL_FlatAmount = 2m;
			entryLine.CL_FlatAmountUQ = "KG";

			IUltimateDistributee distributee = invoiceLine;
			AssertEquals("INV123", distributee.InvoiceNumber);
			AssertEquals("Supply All", distributee.SupplierName);
			AssertEquals("KRW", distributee.InvoiceCurrencyCode);
			AssertEquals("Socks", distributee.LineDescription);
			AssertEquals("KR", distributee.CountryOfOriginCode);
			AssertEquals(100m, distributee.CustomsQuantity);
			AssertEquals("KG", distributee.CustomsUQ);
			AssertEquals("10.00%+2.00/KG", distributee.DutyRateDescription);
			AssertEquals("NO", distributee.InvoiceUQ);
			AssertEquals(200m, distributee.LinePriceInInvoiceCurrency);
			AssertEquals("1111", distributee.TariffNumber);
			AssertEquals(400m, distributee.Weight);
			AssertEquals("KG", distributee.WeightUQ);
			AssertEquals(500m, distributee.Volume);
			AssertEquals("L", distributee.VolumeUQ);
		}

		public void TestWeightApportionment()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_Weight = 1000m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			AssertEquals("InvoiceLine1 Weight", 1000m, invoiceLine1.JI_Weight);
			AssertEquals("InvoiceLine1 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine1.JI_WeightUQ);

			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("InvoiceLine1 Weight", 1000m, invoiceLine1.JI_Weight);
			AssertEquals("InvoiceLine1 WeightUQ", Core.Constants.Weight.Grams, invoiceLine1.JI_WeightUQ);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_WeightUQ = ZString.Empty;
			invoiceLine2.JI_LinePrice = 300m;
			AssertEquals("InvoiceLine1 Weight", 250000m, invoiceLine1.JI_Weight);
			AssertEquals("InvoiceLine1 WeightUQ", Core.Constants.Weight.Grams, invoiceLine1.JI_WeightUQ);
			AssertEquals("InvoiceLine2 Weight", 750m, invoiceLine2.JI_Weight);
			AssertEquals("InvoiceLine2 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine2.JI_WeightUQ);

			declaration.JE_AutoWeightApportion = false;
			invoiceLine2.JI_LinePrice = 700m;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("InvoiceLine1 Weight", 250000m, invoiceLine1.JI_Weight);
			AssertEquals("InvoiceLine1 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine1.JI_WeightUQ);
			AssertEquals("InvoiceLine2 Weight", 750m, invoiceLine2.JI_Weight);
			AssertEquals("InvoiceLine2 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine2.JI_WeightUQ);

			declaration.JE_AutoWeightApportion = true;
			AssertEquals("InvoiceLine1 Weight", 125m, invoiceLine1.JI_Weight);
			AssertEquals("InvoiceLine1 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine1.JI_WeightUQ);
			AssertEquals("InvoiceLine2 Weight", 875m, invoiceLine2.JI_Weight);
			AssertEquals("InvoiceLine2 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine2.JI_WeightUQ);

			invoiceLine1.JI_LinePrice = 200m;
			invoiceLine2.JI_LinePrice = 300m;

			var invoiceLine3 = (BaseJobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			invoiceLine3.JI_JZ = invoice.PK;
			invoiceLine3.JI_WeightUQ = ZString.Empty;
			invoiceLine3.JI_LinePrice = 500m;
			AssertEquals(false, invoice.JobComInvoiceLines.Contains(invoiceLine3));
			AssertEquals("InvoiceLine1 Weight", 200m, invoiceLine1.JI_Weight);
			AssertEquals("InvoiceLine1 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine1.JI_WeightUQ);
			AssertEquals("InvoiceLine2 Weight", 300m, invoiceLine2.JI_Weight);
			AssertEquals("InvoiceLine2 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine2.JI_WeightUQ);
			AssertEquals("InvoiceLine3 Weight", 500m, invoiceLine3.JI_Weight);
			AssertEquals("InvoiceLine3 WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine3.JI_WeightUQ);
		}

		public void TestIWeightApportioneeMembers()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usd.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.5m);
			invoice.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine.JI_LinePrice = 2500m;
			invoiceLine.JI_Weight = 15m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;

			IWeightApportionee weightApportionee = invoiceLine;
			AssertEquals("Amount", 2500m, weightApportionee.Amount);
			AssertEquals("Weight", 15m, weightApportionee.Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Grams, weightApportionee.WeightUQ);

			weightApportionee.Weight = 20m;
			weightApportionee.WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Weight", 20m, invoiceLine.JI_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine.JI_WeightUQ);
		}

		public void TestSettingLinePrice()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";
			part.OP_Desc = "Description";
			part.OP_StockKeepingUnit = "BOX";
			part.OP_LastCost = 100m;
			var uNTtoCTN = part.PartUnits.AddNew();
			uNTtoCTN.OF_QuantityInParent = 25m;
			uNTtoCTN.OF_PackType = "UNT";
			uNTtoCTN.OF_ParentPackType = "BOX";
			part.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Both);
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_OH_Importer = owner.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_InvoiceQuantity = 50m;
			AssertEquals("LineCost", 200m, invoiceLine.JI_LinePrice);
		}

		public void TestChargeWithPercentageRecalculatedWhenLinePriceUpdated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2000m;

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			charge.J7_Percentage = 10m;
			AssertEquals("Amount is calculated", 200m, charge.J7_Amount);

			invoiceLine.JI_LinePrice = 2200m;
			AssertEquals("Amount is calculated", 220m, charge.J7_Amount);
		}

		public void TestJI_Calc_SupplierPartNo()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_IsConsignor = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART NO";
			part.OP_Desc = "Part no 1";
			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			relation.OU_LocalPartNumber = "SUPPLIER1PARTNO";

			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_IsConsignor = true;
			relation = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			relation.OU_LocalPartNumber = "SUPPLIER2PARTNO";

			var supplier3 = Factory.New<OrgHeader>();
			supplier3.OH_IsConsignor = true;
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = supplier1.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PART NO";
			AssertEquals("JI_Calc_SupplierPartNo", "SUPPLIER1PARTNO", invoiceLine.JI_Calc_SupplierPartNo);

			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertEquals("JI_Calc_SupplierPartNo", "SUPPLIER2PARTNO", invoiceLine.JI_Calc_SupplierPartNo);

			invoice.JZ_OH_Supplier = supplier3.PK;
			AssertEquals("JI_Calc_SupplierPartNo", "PART NO", invoiceLine.JI_Calc_SupplierPartNo);
		}

		public void TestJI_Calc_OwnerPartNo()
		{
			var buyer1 = Factory.New<OrgHeader>();
			buyer1.OH_IsConsignee = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART NO";
			part.OP_Desc = "Part no 1";
			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation.OU_LocalPartNumber = "BUYER1PARTNO";

			var buyer2 = Factory.New<OrgHeader>();
			buyer2.OH_IsConsignee = true;
			relation = part.RelatedOrganisations.AddOrganisationIfNotExist(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation.OU_LocalPartNumber = "BUYER2PARTNO";

			var buyer3 = Factory.New<OrgHeader>();
			buyer3.OH_IsConsignee = true;
			part.RelatedOrganisations.AddOrganisationIfNotExist(buyer3.PK, OrgPartRelation.RelationshipTypes.Owner);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = buyer1.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PART NO";
			AssertEquals("JI_Calc_OwnerPartNo", "BUYER1PARTNO", invoiceLine.JI_Calc_OwnerPartNo);

			invoice.JZ_OH_Buyer = buyer2.PK;
			AssertEquals("JI_Calc_OwnerPartNo", "BUYER2PARTNO", invoiceLine.JI_Calc_OwnerPartNo);

			invoice.JZ_OH_Buyer = buyer3.PK;
			AssertEquals("JI_Calc_OwnerPartNo", "PART NO", invoiceLine.JI_Calc_OwnerPartNo);
		}

		/// <summary>
		/// Issue 00059446
		/// </summary>
		public void TestForBalanceValidation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2000m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 2000m;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2000m;

			invoice.RunPreSaveValidation();
			AssertEquals("LightValidationIsValid", true, invoice.LightValidationIsValid);

			invoiceLine2.JI_Calc_Invoice = "1";

			AssertEquals("LightValidationIsValid", false, invoice.LightValidationIsValid);
		}

		public void TestClone()
		{
			invoiceLine.JI_CL = ZGuid.NewZGuid();
			invoiceLine.JI_CEI = ZGuid.NewZGuid();
			invoiceLine.JI_AddInfo = "Hello World";
			invoiceLine.JI_NAddInfo = "Hi World";
			invoiceLine.JI_GS_NKClassUsageCommentReviewer = "TST";

			var clonedInvoiceLine = invoiceLine.Clone();
			AssertEquals("JI_CL is empty on cloned invoice line", ZGuid.Empty, clonedInvoiceLine.JI_CL);
			AssertEquals("JI_CEI is empty on cloned invoice line", ZGuid.Empty, clonedInvoiceLine.JI_CEI);
			AssertEquals("JI_AddInfo on cloned invoice line", "Hello World", clonedInvoiceLine.JI_AddInfo);
			AssertEquals("JI_NAddInfo on cloned invoice line", "Hi World", clonedInvoiceLine.JI_NAddInfo);
			AssertEquals("JI_GS_NKClassUsageCommentReviewer on cloned invoice line", ZString.Empty, clonedInvoiceLine.JI_GS_NKClassUsageCommentReviewer);
		}

		public void TestCloneNAddInfoFields()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_AddInfo = "TariffAdditionalCode=1234";
				invoiceLine.JI_NAddInfo = "Compositions=組成*Group=群組";

				var clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
				CombineAssertions("Template Copy", () =>
				{
					AssertEquals("JI_AddInfo", invoiceLine.JI_AddInfo, clonedDeclaration.InvoiceLines[0].JI_AddInfo);
					AssertEquals("JI_NAddInfo", "Compositions=組成*Group=群組", clonedDeclaration.InvoiceLines[0].JI_NAddInfo);
					AssertEquals("JI_TariffAdditionalCode", "1234", clonedDeclaration.InvoiceLines[0]["JI_TariffAdditionalCode"]);
					AssertEquals("JI_Compositions", "組成", clonedDeclaration.InvoiceLines[0]["JI_Compositions"]);
					AssertEquals("JI_Group", "群組", clonedDeclaration.InvoiceLines[0]["JI_Group"]);
				});

				clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy).Clone();
				CombineAssertions("CountryToCountry Copy", () =>
				{
					AssertEquals("JI_AddInfo", "", clonedDeclaration.InvoiceLines[0].JI_AddInfo);
					AssertEquals("JI_NAddInfo", "", clonedDeclaration.InvoiceLines[0].JI_NAddInfo);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestMaxLengthofJI_AddInfo()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLineWithBaseAddInfoForTesting>();
			invoiceLine.JI_AddInfo = "NameOfGoods=" + new string('名', AutoJobComInvoiceLine.Schema.JI_NAddInfoMaxLength - 12)
														+ "*DutyMode=" + new string('X', AutoJobComInvoiceLine.Schema.JI_AddInfoMaxLength - 9);
		}

		public void TestCloneWithPart()
		{
			// Checks that refreshing of part manager is claled on new line, not source line
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TestImporter";
			importer.OH_IsConsignor = true;
			var partApple = Factory.New<OrgSupplierPart>();
			partApple.FillWithValidTestData();
			partApple.OP_PartNum = "APPLE";
			partApple.RelatedOrganisations.RemoveAndDeleteAll();
			partApple.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = partApple.PK;
			pivot.CI_TariffNum = "12345678";
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			pivot.CI_ChildType = invoiceLine.GetClassificationTypeProvider().HTICode;
			invoiceLine.JI_PartNo = "APPLE";
			invoiceLine.JI_InvoiceQuantity = 144;
			invoiceLine.JI_Tariff = "87654321";
			Factory.Save();
			var clonedInvoiceLine = invoiceLine.Clone();
			AssertEquals("Refreshed from part, not from original line", "12345678", clonedInvoiceLine.JI_Tariff);
			AssertEquals("Unchanged", "87654321", invoiceLine.JI_Tariff);
		}

		public void TestGetCustomsWeight()
		{
			invoiceLine.JI_CustomsQuantity = 23.45m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			AssertEquals("invoiceLine.CustomsWeight.Amount", 23.45m, invoiceLine.CustomsWeight.Amount);
			AssertEquals("invoiceLine.CustomsWeight.Unit", Core.Constants.Weight.Kilograms, invoiceLine.CustomsWeight.Unit);
		}

		public void TestICommonInvoiceAllCharges()
		{
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var charge1 = invoiceLine.Charges.AddNew();
			var charge2 = invoiceLine.ApportionedCharges.AddNew();

			var charge3 = invoiceLine2.Charges.AddNew();
			var charge4 = invoiceLine2.ApportionedCharges.AddNew();

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var invoiceLineLoaded = factory2.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
			factory2.Load(typeof(BaseJobComInvoiceLine), invoiceLine2.PK);

			AssertEquals("ICommonInvoice.AllCharges for InvoiceLine1 should have Charge1", true, ((ICommonInvoice)invoiceLineLoaded).AllCharges.Contains(charge1.PK));
			AssertEquals("ICommonInvoice.AllCharges for InvoiceLine1 should have Charge2", true, ((ICommonInvoice)invoiceLineLoaded).AllCharges.Contains(charge2.PK));
			AssertEquals("ICommonInvoice.AllCharges for InvoiceLine1 should not have Charge3", false, ((ICommonInvoice)invoiceLineLoaded).AllCharges.Contains(charge3.PK));
			AssertEquals("ICommonInvoice.AllCharges for InvoiceLine1 should not have Charge4", false, ((ICommonInvoice)invoiceLineLoaded).AllCharges.Contains(charge4.PK));
		}

		public void TestIUltimateDistributeeCustomsValue()
		{
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			invoiceLine.JI_LinePrice = 10000m;

			AssertEquals("PreCondition:JI_Calc_FOB", 10000m, invoiceLine.JI_Calc_FOB_InLocalCurrency);
			AssertEquals("CustomsValue", 10000m, ((IUltimateDistributee)invoiceLine).CustomsValue);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration1 = Factory.New<BaseJobDeclaration>();
				declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				var invoice1 = declaration1.Invoices.AddNew();
				invoice1.JZ_InvoiceAmount = 10000m;
				var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
				var entry1 = declaration1.CustomsEntryHeaders.AddNew();
				var entryLine1 = entry1.MergedLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;
				entryLine1.CL_CustomsValue = 20000m;

				AssertEquals("CustomsValue", 20000m, ((IUltimateDistributee)invoiceLine1).CustomsValue);
			}
		}

		public void TestIUltimateDistributeeForGSTVATAmount()
		{
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			invoiceLine.JI_LinePrice = 10000m;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryLine.Fees.AddOrUpdate(declaration.GSTOrVATCode, 10m);
			AssertEquals(10m, ((IUltimateDistributee)invoiceLine).GSTVATAmount);
		}

		public void TestHumanReadableCodeInfos()
		{
			invoice.JZ_InvoiceNumber = "TestInvoice";

			invoiceLine.JI_LineNo = (short)2;

			var infos = ((IUltimateDistributee)invoiceLine).HumanReadableCodeInfos;
			AssertEquals("First Info to sort LC Lines by on HumanReadableCode", invoice.JZ_InvoiceNumberInfo, infos[0]);
			AssertEquals("Second Info to sort LC Lines by on HumanReadableCode", invoiceLine.JI_LineNoInfo, infos[1]);
		}

		public void TestOverseasInsuranceOFT()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				invoiceLine.JI_LinePrice = 10000m;
				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("Overseas Freight", 100m, invoiceLine.JI_Calc_FreightInInvoiceCurr);
				AssertEquals("Overseas Insurance", 10m, invoiceLine.JI_Calc_InsuranceInInvoiceCurr);
				AssertEquals("Overseas Freight for Invoice", 100m, invoice.JZ_Calc_OFTInInvoiceCurrency);
				AssertEquals("Overseas Insurance for Invoice", 10m, invoice.JZ_Calc_ONSInInvoiceCurrency);
			}
		}

		public void TestIChargeApportionee()
		{
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_Volume = 2m;
			invoiceLine.JI_VolumeUQ = Core.Constants.Volume.CubicMetres;
			invoiceLine.JI_InvoiceQuantity = 156m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("AllApportionees", 0, ((IChargeApportionee)invoiceLine).AllApportionees.Length);
			AssertEquals("ApportionedCharges", invoiceLine.ApportionedCharges, ((IChargeApportionee)invoiceLine).ApportionedCharges);
			AssertEquals("GetBaseValueToApportionOn for Value", 10000m, ((IChargeApportionee)invoiceLine).GetBaseValueToApportionOn(invoiceLine.CurrencyConverter, ChargeDistributeByList.Codes.Value));
			AssertEquals("GetBaseValueToApportionOn for Weight", 10m, ((IChargeApportionee)invoiceLine).GetBaseValueToApportionOn(invoiceLine.CurrencyConverter, ChargeDistributeByList.Codes.Weight));
			AssertEquals("GetBaseValueToApportionOn for Volume", 2m, ((IChargeApportionee)invoiceLine).GetBaseValueToApportionOn(invoiceLine.CurrencyConverter, ChargeDistributeByList.Codes.Volume));
			AssertEquals("GetBaseValueToApportionOn for Quantity", 156m, ((IChargeApportionee)invoiceLine).GetBaseValueToApportionOn(invoiceLine.CurrencyConverter, ChargeDistributeByList.Codes.Quantity));

			var cOM = invoice.Charges.AddNew();
			cOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			cOM.J7_Amount = 100m;
			cOM.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			cOM.J7_IsIncludedInITOT = true;

			AssertEquals("IChargeHolder.Charges", invoiceLine.Charges, ((IChargeHolder)invoiceLine).Charges);
			AssertEquals("IChargeHolder.CurrencyConverter", invoiceLine.CurrencyConverter, ((IChargeHolder)invoiceLine).CurrencyConverter);
			AssertEquals("IChargeHolder.ImmediateChargeHolderChildren", 0, ((IChargeHolder)invoiceLine).ImmediateChargeHolderChildren.Length);
			AssertEquals("IChargeHolder.ImmediateChargeHolderParent", invoice, ((IChargeHolder)invoiceLine).ImmediateChargeHolderParent);
		}

		public void TestChargeApportionedAndJ7_IsIncluedInITOT()
		{
			using (Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoiceLine.JI_LinePrice = 10000m;

				var packingCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				packingCharge.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
				packingCharge.J7_Amount = 100m;
				packingCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

				var freightCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				freightCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				freightCharge.J7_Amount = 100m;
				freightCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
				declaration.ResumeApportionment();

				var charge = invoiceLine.ApportionedCharges
					.Cast<BaseInvoiceLineApportionedCharge>()
					.FirstOrDefault(c => c.J7_ChargeType == CustomsChargeTypeList.Codes.PackingCost);

				AssertNotNull("PreCondition:Packing Charge", charge);
				AssertEquals("IsIncludedInITOT", false, charge.J7_IsIncludedInITOT);

				charge = invoiceLine.ApportionedCharges
					.Cast<BaseInvoiceLineApportionedCharge>()
					.FirstOrDefault(c => c.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight);

				AssertNotNull("PreCondition:Overseas Freight Charge", charge);
				AssertEquals("IsIncludedInITOT", false, charge.J7_IsIncludedInITOT);

				invoice.JZ_IncoTerm = "CIF";
				declaration.ResumeApportionment();

				charge = invoiceLine.ApportionedCharges
					.Cast<BaseInvoiceLineApportionedCharge>()
					.FirstOrDefault(c => c.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight);

				AssertNotNull("PreCondition:Overseas Freight Charge", charge);
				AssertEquals("IsIncludedInITOT", true, charge.J7_IsIncludedInITOT);
			}
		}

		public void TestBondedWarehouseTransactionLineProvider()
		{
			var creator = new MergedDeclarationCreator<BaseJobDeclaration>(Factory);

			var line1 = ((IBondedWarehouseTransactionLineProvider)creator.InvoiceLine1).TransactionLine;
			var line2 = ((IBondedWarehouseTransactionLineProvider)creator.InvoiceLine1).TransactionLine;

			CombineAssertions(() =>
			{
				AssertSame("Same value returned each call", line1, line2);
				AssertNotNull("BondedWarehouseTransaction provided", line1);
			});
		}

		public void TestUseBondedWarehouseAutomation()
		{
			AssertEquals(false, invoiceLine.UseBondedWarehouseAutomation);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestUseBondedWarehouseAutomationSetterThrowsExceptionWhenNotOverriden()
		{
			invoiceLine.UseBondedWarehouseAutomation = true;
		}

		public void TestICommonInvoiceImmediateCommonInvoiceParent()
		{
			invoice.JZ_InvoiceNumber = "TESTINVOICE";
			invoiceLine.JI_LineNo = (short)1;
			AssertEquals("ICommonInvoice.ImmediateCommonInvoiceParent", invoice, ((ICommonInvoice)invoiceLine).ImmediateCommonInvoiceParent);
			AssertEquals("ICommonInvoice.UserFriendlyCode", "1/TESTINVOICE", ((ICommonInvoice)invoiceLine).UserFriendlyCode);
		}

		public void TestMarkApportionmentDirtyWhenInvoiceChanges()
		{
			invoice.JZ_InvoiceNumber = "TESTINVOICE1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TESTINVOICE2";

			AssertEquals("PreCondition:InvoiceLine's InvoiceHeader", invoice, invoiceLine.InvoiceHeader);

			AssertEquals("Apportionment is not dirty", false, declaration.ApportionmentDirty);
			invoiceLine.JI_Calc_Invoice = "TESTINVOICE2";
			AssertEquals("Invoice line invoice header is changed", invoice2, invoiceLine.InvoiceHeader);
			AssertEquals("Apportionment is dirty now", true, declaration.ApportionmentDirty);
		}

		public void TestJI_LinePriceMoneyForUserEnterableInvoiceCurrExRate()
		{
			SetupUserEnterableInvoiceCurrExRate();
			invoiceLine.JI_LinePrice = 100m;
			var money = invoiceLine.JI_LinePriceMoney;
			AssertEquals("Amount", 200m, money.Amount);
			AssertEquals("Currency", declaration.LocalCurrencyCode, money.Currency.Code);

			RemoveUserEnterableInvoiceCurrExRate();

			money = invoiceLine.JI_LinePriceMoney;
			AssertEquals("Amount", 100m, money.Amount);
			AssertEquals("Currency", invoice.Invoice_Currency.RX_Code, money.Currency.Code);
		}

		public void TestJI_FOBForUserEnterableInvoiceCurrExRate()
		{
			SetupUserEnterableInvoiceCurrExRate();
			invoiceLine.JI_LinePrice = 100m;
			var charge = invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 60m);
			charge.J7_IsDutiable = true;

			var money = invoiceLine.JI_FOB;
			AssertEquals("Amount", 320m, money.Amount);
			AssertEquals("Currency", declaration.LocalCurrencyCode, money.Currency.Code);

			RemoveUserEnterableInvoiceCurrExRate();

			money = invoiceLine.JI_FOB;
			AssertEquals("Amount", 160m, money.Amount);
			AssertEquals("Currency", invoice.Invoice_Currency.RX_Code, money.Currency.Code);
		}

		public void TestJI_CIFForUserEnterableInvoiceCurrExRate()
		{
			SetupUserEnterableInvoiceCurrExRate();
			invoiceLine.JI_LinePrice = 100m;
			var charge = invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 60m);
			charge.J7_IsGSTApplicable = true;

			var money = invoiceLine.JI_CIF;
			AssertEquals("Amount", 320m, money.Amount);
			AssertEquals("Currency", declaration.LocalCurrencyCode, money.Currency.Code);
			AssertEquals("local amount", money.Amount, invoiceLine.JI_Calc_CIF_InLocalCurrency);

			RemoveUserEnterableInvoiceCurrExRate();

			money = invoiceLine.JI_CIF;
			AssertEquals("Amount", 160m, money.Amount);
			AssertEquals("Currency", invoice.Invoice_Currency.RX_Code, money.Currency.Code);
		}

		public void TestGrossWeightInKG()
		{
			AssertEquals(0m, invoiceLine.GrossWeightInKG);

			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_WeightUQ = "";
			AssertEquals(0m, invoiceLine.GrossWeightInKG);

			invoiceLine.JI_WeightUQ = "KG";
			AssertEquals(10m, invoiceLine.GrossWeightInKG);

			invoiceLine.JI_WeightUQ = "T";
			AssertEquals(10000m, invoiceLine.GrossWeightInKG);
		}

		public void TestCustomsFirstQuantityInKG()
		{
			AssertEquals(0m, invoiceLine.CustomsFirstQuantityInKG);

			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(0m, invoiceLine.CustomsFirstQuantityInKG);

			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(10m, invoiceLine.CustomsFirstQuantityInKG);

			invoiceLine.JI_CustomsUnitQty = "T";
			AssertEquals(10000m, invoiceLine.CustomsFirstQuantityInKG);
		}

		public void TestCustomsSecondQuantityInKG()
		{
			AssertEquals(0m, invoiceLine.CustomsSecondQuantityInKG);

			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.JI_CustomsSecondUnitQty = "";
			AssertEquals(0m, invoiceLine.CustomsSecondQuantityInKG);

			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			AssertEquals(10m, invoiceLine.CustomsSecondQuantityInKG);

			invoiceLine.JI_CustomsSecondUnitQty = "T";
			AssertEquals(10000m, invoiceLine.CustomsSecondQuantityInKG);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthExceptionWhenSynchroniseFromForwardingOrderLine()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var order = Factory.New<Freight.Forwarding.Orders.Business.Order>();
			var orderLine = order.OrderLines.AddNew();

			order.JD_OrderNumber = "ORDER NUMBER ORDER NUMBE";

			((Integration.Customs.IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
		}

		public void TestIsAdvanceShippingNoticeInvoice()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			Assert("IsAdvanceShippingNoticeInvoice", invoiceLine.IsAdvanceShippingNoticeInvoice);
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsAdvanceShippingNoticeInvoice", !invoiceLine.IsAdvanceShippingNoticeInvoice);
		}

		public void TestPartSyncRefershForASNInvoice()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.RelatedOrganisations.AddSupplier(consignor);
			part.RelatedOrganisations.AddOwner(consignee);

			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			invoice1.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoice1.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice1.JZ_OH_Buyer = consignee.PK;
			invoice1.JZ_OH_Supplier = consignor.PK;

			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			invoice2.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;

			var invoiceLine = invoice1.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PART1";
			Assert("Precondition:PartSyncManager.Enabled", invoiceLine.PartSyncManager.Enabled);
			AssertEquals("Precondition:JI_OP", part.PK, invoiceLine.JI_OP);
			AssertNotNull("Precondition:Part", invoiceLine.Part);
			invoiceLine.JI_JZ = invoice2.PK;
			Assert("invoiceLine.PartSyncManager.Enabled should be false for ASN invoice", !invoiceLine.PartSyncManager.Enabled);
			AssertEquals("JI_OP should be null for ASN invoice", ZGuid.Empty, invoiceLine.JI_OP);
			AssertNull("Part should be null for ASN invoice", invoiceLine.Part);
		}

		public void TestUpdatePartSyncManagerAndRefresh()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.RelatedOrganisations.AddSupplier(consignor);
			part.RelatedOrganisations.AddOwner(consignee);

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PART1";
			Assert("Precondition:", invoiceLine.PartSyncManager.Enabled);
			AssertNotNull("Precondition:", invoiceLine.PartSyncManager.Part);
			AssertEquals("Precondition:", part.PK, invoiceLine.JI_OP);

			invoiceLine.UpdatePartSyncManagerAndRefresh(false);
			Assert("PartSyncManager.Enabled should be set to false", !invoiceLine.PartSyncManager.Enabled);
			AssertNull("PartSyncManager.Part should be cleared", invoiceLine.PartSyncManager.Part);
			AssertEquals("JI_OP should be cleared", ZGuid.Empty, invoiceLine.JI_OP);

			invoiceLine.UpdatePartSyncManagerAndRefresh(true);
			Assert("PartSyncManager.Enabled should be set to true", invoiceLine.PartSyncManager.Enabled);
			AssertEquals("PartSyncManager.Part should be set", part.PK, invoiceLine.PartSyncManager.Part.PK);
			AssertEquals("JI_OP should be set", part.PK, invoiceLine.JI_OP);
		}

		public void TestProductAudit()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var pivot = part.PivotsForBinding.AddNew();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = Factory.New<OrgHeader>().PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			pivot.CI_CC = classification.PK;
			invoice.JZ_OH_Supplier = relation.OU_OH;
			invoiceLine.JI_PartNo = part.OP_PartNum;

			AssertEquals(pivot.PK, invoiceLine.Pivot.PK);
			AssertEquals(classification.PK, invoiceLine.Classification.PK);

			Assert("Is Non Audited Product", !invoiceLine.IsProductAuditedOrInvalid());
			pivot.CI_LastAuditedDate = ZDateTime.Now;
			Assert("Is Audited Product", invoiceLine.IsProductAuditedOrInvalid());

			Assert("Is Non Audited Lookup", !invoiceLine.IsLookupAudited());
			classification.CC_LastAuditedDate = ZDateTime.Now;
			Assert("Is Audited Lookup", invoiceLine.IsLookupAudited());
		}

		public void TestParentTariffLineDoNotRaiseDeletedBOException()
		{
			var childLine = invoice.InvoiceLines.AddNew();
			childLine.JI_ParentID = invoiceLine.PK;
			childLine.Delete();
			AssertNoExceptionThrown(delegate
			{ var accessed = childLine.ParentTariffLine; });
		}

		public void TestDebugLog()
		{
			// clear debugLogSent flag (some other test could set it)
			BaseJobComInvoiceLine.debugLogSent = false;

			var childLine1 = invoice.InvoiceLines.AddNew();
			var childLine2 = invoice.InvoiceLines.AddNew();
			childLine1.JI_ParentID = invoiceLine.PK;
			childLine2.JI_ParentID = invoiceLine.PK;

			AssertSame("(pre-condition) parent was set for line 1", invoiceLine, childLine1.ParentTariffLine);
			AssertSame("(pre-condition) parent was set for line 2", invoiceLine, childLine2.ParentTariffLine);

			using (invoiceLine.EnableDebugLog())
			{
				childLine1.Delete();
				Assert(childLine1.IsDeleted);
				Assert(!invoiceLine.IsDeleted);
				_ = childLine1.ParentTariffLine;

				AssertContains("DebugLog", "JobComInvoiceLine delete.", childLine1.DebugLog.ToString());
				AssertEquals("DebugLog 2", string.Empty, childLine2.DebugLog.ToString());

				AssertEquals("both standard error and debug log were sent", 2, ExceptionReporterTestListener.Instance.Count);
				AssertContains("Deleted row information cannot be accessed through the row.", (ExceptionReporterTestListener.Instance[0] as InvalidOperationException)?.Message);
				AssertContains(FormattableString.Invariant(
					$@"Developer Error: Should not be accessing a property on a deleted business object
TableName: JobComInvoiceLine
Property name: JI_ParentID
Business Object Type: Enterprise.Customs.Business.BaseJobComInvoiceLine
DataRowState: Detached
IsDataRowInDataTable: True
Version: Default
VersionToUse: Default
PK: {childLine1.PK}
CollectionListChangedSuspender index on factory: 0"), (ExceptionReporterTestListener.Instance[1] as DeveloperNotificationException)?.Message);

				ExceptionReporterTestListener.Instance.Clear();

				childLine2.Delete();
				Assert(childLine2.IsDeleted);
				Assert(!invoiceLine.IsDeleted);
				_ = childLine2.ParentTariffLine;

				AssertContains("DebugLog", "JobComInvoiceLine delete.", childLine2.DebugLog.ToString());

				AssertEquals("neither both standard error nor debug log were sent", 0, ExceptionReporterTestListener.Instance.Count);

				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestDebugLog_WhenDisabled()
		{
			// clear debugLogSent flag (some other test could set it)
			BaseJobComInvoiceLine.debugLogSent = false;

			var childLine = invoice.InvoiceLines.AddNew();
			childLine.JI_ParentID = invoiceLine.PK;

			_ = childLine.ParentTariffLine;
			childLine.Delete();
			_ = childLine.ParentTariffLine;

			Assert("DebugLog is empty", childLine.DebugLog.IsEmpty);

			AssertEquals("only standard error was sent", 1, ExceptionReporterTestListener.Instance.Count);
			AssertContains(FormattableString.Invariant(
				$@"Developer Error: Should not be accessing a property on a deleted business object
TableName: JobComInvoiceLine
Property name: JI_ParentID
Business Object Type: Enterprise.Customs.Business.BaseJobComInvoiceLine
DataRowState: Detached
IsDataRowInDataTable: True
Version: Default
VersionToUse: Default
PK: {childLine.PK}
CollectionListChangedSuspender index on factory: 0"), (ExceptionReporterTestListener.Instance[0] as DeveloperNotificationException)?.Message);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalCopyEntityCopyNode()
		{
			var elementType = typeof(BaseJobComInvoiceLine);
			var copyTemplateTree = new CopyTemplateTree(elementType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var innerNode = copyTemplateTree.InnerNode as EntityCopyTemplateNode;

			var cusEntryLineNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "CusEntryLine") as PropertyCopyTemplateNode;
			AssertNull("CusEntryLine should be excluded from Universal Copy", cusEntryLineNode);

			var cusEntryLinePKNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "JI_CL") as PropertyCopyTemplateNode;
			AssertNull("JI_CL should be excluded from Universal Copy", cusEntryLinePKNode);

			var cusEntryInstructionNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "JI_CEI") as PropertyCopyTemplateNode;
			AssertNull("JI_CEI should be excluded from Universal Copy", cusEntryInstructionNode);

			var messageTypeNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "JI_PartNo") as PropertyCopyTemplateNode;
			AssertNotNull("JI_PartNo is expected as proof that Universal Copy is functioning", messageTypeNode);

			var chargesNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "Charges") as CollectionCopyTemplateNode;
			AssertNotNull("Charges is expected as proof that Universal Copy is functioning", chargesNode);
		}

		public void TestIsDataImportInProgress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine1.IsDataImportInProgress);

			BaseJobComInvoiceLine invoiceLine2;

			using (DataImportIndicatorService.StartDataImport(Factory))
			{
				AssertEquals(true, invoiceLine1.IsDataImportInProgress);

				invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				AssertEquals(true, invoiceLine2.IsDataImportInProgress);
			}

			AssertEquals(false, invoiceLine1.IsDataImportInProgress);
			AssertEquals(false, invoiceLine2.IsDataImportInProgress);
		}

		public void TestTariffViewFilterData()
		{
			CombineAssertions("Invoice line without a parent declaration", () =>
			{
				var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
				AssertEquals("Expected empty", ZString.Empty, invoiceLine.TariffViewFilterData.RatesApplyToCountry);
			});

			CombineAssertions("Invoice line with a parent declaration", () =>
			{
				var refUNLOCO = Factory.New<RefUNLOCO>();
				refUNLOCO.RL_Code = "FIN";
				refUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Denmark;

				var declaration = Factory.New<BaseJobDeclaration>();
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				AssertEquals("Expected empty", ZString.Empty, invoiceLine.TariffViewFilterData.RatesApplyToCountry);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
				AssertEquals("Expected JI_CountryOfOrigin", Core.Constants.CountryCodes.Italy, invoiceLine.TariffViewFilterData.RatesApplyToCountry);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_RL_NKFinalDestination = "FIN";
				AssertEquals("Expected EXP JE_RL_NKFinalDestination country code", Core.Constants.CountryCodes.Denmark, invoiceLine.TariffViewFilterData.RatesApplyToCountry);
				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals("Expected MSC JE_RL_NKFinalDestination country code", Core.Constants.CountryCodes.Denmark, invoiceLine.TariffViewFilterData.RatesApplyToCountry);
			});
		}

		public void TestJI_BondedWhsUnitQty()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<Testing.BaseJobDeclarationForTesting>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = Factory.New<Testing.BaseJobComInvoiceLineForTesting>();
				invoiceLine.JI_JZ = invoiceHeader.PK;
				invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_InvoiceQuantity = 1m;

				invoiceLine.JI_BondedWhsUnitQty = Core.Constants.Volume.CubicCentimeters;
				Assert(invoiceLine.JI_BondedWhsQuantity.IsEmpty);

				invoiceLine.JI_BondedWhsUnitQty = Core.Constants.Weight.Grams;
				AssertEquals(1000m, invoiceLine.JI_BondedWhsQuantity);

				invoiceLine.JI_BondedWhsUnitQty = Core.Constants.Weight.Kilograms;
				AssertEquals(1m, invoiceLine.JI_BondedWhsQuantity);
			}
		}

		public void TestDefaultJI_BondedWhsQuantity()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<Testing.BaseJobDeclarationForTesting>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = Factory.New<Testing.BaseJobComInvoiceLineForTesting>();
				invoiceLine.JI_JZ = invoiceHeader.PK;
				invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_InvoiceQuantity = 1m;
				invoiceLine.JI_BondedWhsUnitQty = Core.Constants.Weight.Kilograms;
				AssertEquals(1m, invoiceLine.JI_BondedWhsQuantity);

				invoiceLine.JI_BondedWhsUnitQty = Core.Constants.Weight.Grams;
				invoiceLine.JI_InvoiceQuantity = 2m;
				AssertEquals(2000m, invoiceLine.JI_BondedWhsQuantity);
			}
		}

		public void TestHasGuaranteeConsumingProcedure()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var procedure = GenerateProcedure("No", YesNoList.Codes.No, YesNoList.Codes.No, "No description", false);
			var procedure2 = GenerateProcedure("Ye", YesNoList.Codes.Yes, YesNoList.Codes.No, "Yes description", false);
			Factory.Save();

			var invoiceLine = Factory.New<Testing.BaseJobComInvoiceLineForTesting>();
			invoiceLine.JI_Procedure = "Ye";
			Assert("invoiceline HasGuaranteeConsumingProcedure = true", invoiceLine.HasGuaranteeConsumingProcedure);

			invoiceLine.JI_Procedure = "No";
			Assert("invoiceline HasGuaranteeConsumingProcedure = false", !invoiceLine.HasGuaranteeConsumingProcedure);

			invoiceLine.JI_Procedure = "ER";
			Assert("invoiceline HasGuaranteeConsumingProcedure = false", !invoiceLine.HasGuaranteeConsumingProcedure);

			invoiceLine.JI_Procedure = "";
			Assert("invoiceline HasGuaranteeConsumingProcedure = false", !invoiceLine.HasGuaranteeConsumingProcedure);
		}

		public void TestHasGuaranteeReleasedProcedure()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var procedure = GenerateProcedure("No", YesNoList.Codes.No, YesNoList.Codes.No, "No description", false);
			var procedure2 = GenerateProcedure("Ye", YesNoList.Codes.No, YesNoList.Codes.Yes, "Yes description", false);
			Factory.Save();

			var invoiceLine = Factory.New<Testing.BaseJobComInvoiceLineForTesting>();
			invoiceLine.JI_Procedure = "Ye";
			Assert("invoiceline HasGuaranteeReleasedProcedure = true", invoiceLine.HasGuaranteeReleasedProcedure);

			invoiceLine.JI_Procedure = "No";
			Assert("invoiceline HasGuaranteeReleasedProcedure = false", !invoiceLine.HasGuaranteeReleasedProcedure);

			invoiceLine.JI_Procedure = "ER";
			Assert("invoiceline HasGuaranteeReleasedProcedure = false", !invoiceLine.HasGuaranteeReleasedProcedure);

			invoiceLine.JI_Procedure = "";
			Assert("invoiceline HasGuaranteeReleasedProcedure = false", !invoiceLine.HasGuaranteeReleasedProcedure);
		}

		public void TestHasAnyProcedureWithSuspendedVat()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var procedure = GenerateProcedure("Ye", YesNoList.Codes.No, YesNoList.Codes.No, "No description", true);
			Factory.Save();

			var invoiceLine = Factory.New<Testing.BaseJobComInvoiceLineForTesting>();
			invoiceLine.JI_Procedure = "Ye";
			Assert("ProcedureIndicatesVATNotApply is not empty", !invoiceLine.ProcedureIndicatesVATNotApply.IsEmpty);
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = true", invoiceLine.HasAnyProcedureWithSuspendedVat);

			invoiceLine.JI_Procedure = "";
			Assert("ProcedureIndicatesVATNotApply is empty", invoiceLine.ProcedureIndicatesVATNotApply.IsEmpty);
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = false", !invoiceLine.HasAnyProcedureWithSuspendedVat);
		}

		public void TestDrawbackClaimQuantity()
		{
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_CustomsQuantity = 0m;
			invoiceLine.JI_CustomsUnitQty = "";
			AssertEquals("If no customs qty and units then invoice qty selected", 1m, invoiceLine.DrawbackClaimQuantity);
			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("Customs units specified so customs quantity selected", 0m, invoiceLine.DrawbackClaimQuantity);
			invoiceLine.JI_CustomsQuantity = 2m;
			AssertEquals("Customs qty specified so is selected", 2m, invoiceLine.DrawbackClaimQuantity);
			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsQuantity = 2m;
			AssertEquals("Customs qty specified so is still selected", 2m, invoiceLine.DrawbackClaimQuantity);
		}

		public void TestPartSyncManagerActiveDeciderPK()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			AssertEquals(ZGuid.Empty, invoiceLine.PartSyncManagerActiveDeciderPK);

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoiceLine.JI_JZ = invoice.PK;
			AssertEquals(invoice.PK, invoiceLine.PartSyncManagerActiveDeciderPK);

			var dec = Factory.New<BaseJobDeclaration>();
			invoice.JZ_JE = dec.PK;
			AssertEquals(dec.PK, invoiceLine.PartSyncManagerActiveDeciderPK);
		}

		public void TestIsGoingIntoBondedWarehouse()
		{
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);

			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsGoingIntoBondedWarehouse", false, invoiceLine.IsGoingIntoBondedWarehouse);

			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("IsGoingIntoBondedWarehouse", true, invoiceLine.IsGoingIntoBondedWarehouse);
		}

		public void TestIsGoingIntoBondedWarehouse_Orphaned()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("IsGoingIntoBondedWarehouse", false, invoiceLine.IsGoingIntoBondedWarehouse);
		}

		public void TestIsVolumeWeightCalculableFromInvoiceQty()
		{
			invoiceLine.JI_InvoiceUQ = "";
			AssertEquals("IsVolumeCalculableFromInvoiceQty", false, invoiceLine.IsVolumeCalculableFromInvoiceQty);
			AssertEquals("IsWeightCalculableFromInvoiceQty", false, invoiceLine.IsGrossWeightCalculableFromInvoiceQty);

			invoiceLine.JI_InvoiceUQ = "M3";
			AssertEquals("IsVolumeCalculableFromInvoiceQty", true, invoiceLine.IsVolumeCalculableFromInvoiceQty);
			AssertEquals("IsWeightCalculableFromInvoiceQty", false, invoiceLine.IsGrossWeightCalculableFromInvoiceQty);

			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("IsVolumeCalculableFromInvoiceQty", false, invoiceLine.IsVolumeCalculableFromInvoiceQty);
			AssertEquals("IsWeightCalculableFromInvoiceQty", true, invoiceLine.IsGrossWeightCalculableFromInvoiceQty);
		}

		public void TestIsVolumeWeightCalculableFromProduct()
		{
			var product = SetupProduct();
			product.OP_StockKeepingUnit = "BOX";
			product.OP_Cubic = 0.1m;
			product.OP_CubicUQ = "M3";
			product.OP_Weight = 2m;
			product.OP_WeightUQ = "KG";
			product.OP_NetWeight = 2m;

			invoiceLine.JI_PartNo = product.OP_PartNum;

			invoiceLine.JI_InvoiceUQ = "";
			AssertEquals("IsVolumeCalculableFromProduct", false, invoiceLine.IsVolumeCalculableFromProduct);
			AssertEquals("IsGrossWeightCalculableFromProduct", false, invoiceLine.IsGrossWeightCalculableFromProduct);
			AssertEquals("IsNetWeightCalculableFromProduct", false, invoiceLine.IsNetWeightCalculableFromProduct);

			invoiceLine.JI_InvoiceUQ = "BOX";
			AssertEquals("IsVolumeCalculableFromProduct", true, invoiceLine.IsVolumeCalculableFromProduct);
			AssertEquals("IsGrossWeightCalculableFromProduct", true, invoiceLine.IsGrossWeightCalculableFromProduct);
			AssertEquals("IsNetWeightCalculableFromProduct", true, invoiceLine.IsNetWeightCalculableFromProduct);

			product.OP_Cubic = 0m;
			product.OP_Weight = 0m;
			product.OP_NetWeight = 0m;
			AssertEquals("IsVolumeCalculableFromProduct", false, invoiceLine.IsVolumeCalculableFromProduct);
			AssertEquals("IsGrossWeightCalculableFromProduct", false, invoiceLine.IsGrossWeightCalculableFromProduct);
			AssertEquals("IsNetWeightCalculableFromProduct", false, invoiceLine.IsNetWeightCalculableFromProduct);

			product.OP_Cubic = 20m;
			product.OP_Weight = 20m;
			product.OP_StockKeepingUnit = "KG";
			AssertEquals("IsVolumeCalculableFromProduct", false, invoiceLine.IsVolumeCalculableFromProduct);
			AssertEquals("IsWeightCalculableFromProduct", false, invoiceLine.IsGrossWeightCalculableFromProduct);
			AssertEquals("IsNetWeightCalculableFromProduct", false, invoiceLine.IsNetWeightCalculableFromProduct);

			product.OP_StockKeepingUnit = "M3";
			AssertEquals("IsVolumeCalculableFromProduct", false, invoiceLine.IsVolumeCalculableFromProduct);
			AssertEquals("IsWeightCalculableFromProduct", false, invoiceLine.IsGrossWeightCalculableFromProduct);
			AssertEquals("IsNetWeightCalculableFromProduct", false, invoiceLine.IsNetWeightCalculableFromProduct);
		}

		public void TestCalculateVolumeWeightFromInvoiceQty()
		{
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.Volume.CubicFeet;

			AssertEquals("JI_Volume/UQ should be calculated from Invoice Qty", 100m, invoiceLine.JI_Volume);
			AssertEquals("JI_Volume/UQ should be calculated from Invoice Qty", Core.Constants.Volume.CubicFeet, invoiceLine.JI_VolumeUQ);

			invoiceLine.JI_InvoiceUQ = Core.Constants.Volume.CubicInches;
			AssertEquals("JI_Volume/UQ should be calculated from Invoice Qty", 100m, invoiceLine.JI_Volume);
			AssertEquals("JI_Volume/UQ should be calculated from Invoice Qty", Core.Constants.Volume.CubicInches, invoiceLine.JI_VolumeUQ);

			//Weight
			invoiceLine.JI_WeightUQ = "";
			invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.LongTons;
			AssertEquals(0m, invoiceLine.JI_Weight);
			AssertEquals("", invoiceLine.JI_WeightUQ);

			using (CustomsDataRegistry.Instance.DefaultWeightFromInvoiceQty.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("JI_Weight/UQ should be calculated from Invoice Qty", 100m, invoiceLine.JI_Weight);
				AssertEquals("JI_Weight/UQ should be calculated from Invoice Qty", Core.Constants.Weight.Kilograms, invoiceLine.JI_WeightUQ);

				invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.LongTons;
				AssertEquals("JI_Weight/UQ should be calculated from Invoice Qty", 100m, invoiceLine.JI_Weight);
				AssertEquals("JI_Weight/UQ should be calculated from Invoice Qty", Core.Constants.Weight.LongTons, invoiceLine.JI_WeightUQ);
			}
		}

		public void TestCalculateAllocatedQuantityFromInvoiceQuantity()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = Factory.New<Testing.BaseJobComInvoiceLineForTesting>();
				invoiceLine.JI_JZ = invoiceHeader.PK;

				var whsInventory = Factory.New<IWhsInventoryView>();
				whsInventory.WI_AllocationKey = "WI123";
				whsInventory.WI_F3_NKPackType = Core.Constants.Weight.Kilograms;
				var inventory = Factory.New<JobComInvLineComponentInventory>();
				inventory.JIV_AllocationKey = whsInventory.WI_AllocationKey;
				invoiceLine.ComponentInventoryCollection.Add(inventory);

				AssertEquals("Value before setting JI_InvoiceQuantity", 0m, inventory.JIV_QuantityToDraw);

				invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_InvoiceQuantity = 10m;
				AssertEquals("Value after setting JI_InvoiceQuantity", 10m, inventory.JIV_QuantityToDraw);
			}
		}

		public void TestNetWeightCalculatedFromProductFile()
		{
			var product = SetupProduct();
			var boxToNo = product.PartUnits.AddNew();
			boxToNo.OF_QuantityInParent = 10m;
			boxToNo.OF_PackType = "NUM";
			boxToNo.OF_ParentPackType = "BOX";

			product.OP_StockKeepingUnit = "BOX";
			product.OP_NetWeight = 150m;

			product.OP_Weight = 2m;
			product.OP_WeightUQ = "KG";

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 2000m;
			invoiceLine.JI_InvoiceUQ = "NUM";
			AssertEquals("Net Weight", 30000m, invoiceLine.JI_NetWeight);
			AssertEquals("Weight should be calculated from Product details", "KG", invoiceLine.JI_WeightUQ);
		}

		[ExpectNoExceptions]
		public void TestInvalidWeightUnitDoesNotCauseException()
		{
			var product = SetupProduct();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_WeightUQ = "EA";
			invoiceLine.JI_InvoiceQuantity = 3000m;
		}

		public void TestCalculateVolumeWeightFromProduct()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var product = SetupProduct();
			var boxToNo = product.PartUnits.AddNew();
			boxToNo.OF_QuantityInParent = 10m;
			boxToNo.OF_PackType = "NUM";
			boxToNo.OF_ParentPackType = "BOX";

			product.OP_StockKeepingUnit = "BOX";
			product.OP_Cubic = 0.1m;
			product.OP_CubicUQ = "M3";

			product.OP_Weight = 2m;
			product.OP_WeightUQ = "KG";

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 2000m;
			invoiceLine.JI_InvoiceUQ = "NUM";
			AssertEquals("Volume should be calculated from Product details", 20m, invoiceLine.JI_Volume);
			AssertEquals("Volume should be calculated from Product details", "M3", invoiceLine.JI_VolumeUQ);

			AssertEquals("Weight should be calculated from Product details", 400m, invoiceLine.JI_Weight);
			AssertEquals("Weight should be calculated from Product details", "KG", invoiceLine.JI_WeightUQ);

			var factory2 = new BusinessObjectFactory();
			var productInDiffFactory = factory2.New<OrgSupplierPart>();
			productInDiffFactory.OP_PartNum = "TestPartNumInDiffFactory";
			productInDiffFactory.OP_StockKeepingUnit = "NO";
			productInDiffFactory.OP_Cubic = 1m;
			productInDiffFactory.OP_CubicUQ = "M3";
			OrgPartRelation relation = productInDiffFactory.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "SUP";
			relation.OU_OH = supplier.PK;
			factory2.Save();

			invoiceLine.JI_InvoiceQuantity = 2000m;
			invoiceLine.JI_InvoiceUQ = "NUM";

			invoiceLine.JI_PartNo = productInDiffFactory.OP_PartNum;
			AssertEquals("Volume should be calculated from Product details", 2000m, invoiceLine.JI_Volume);
			AssertEquals("Volume should be calculated from Product details", "M3", invoiceLine.JI_VolumeUQ);

			productInDiffFactory.OP_Cubic = 10m;
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(invoiceLine.Factory, invoiceLine.PartSyncManagerActiveDeciderPK);
			factory2.Save();
			AssertEquals("Volume should be calculated from Product details", 20000m, invoiceLine.JI_Volume);
			AssertEquals("Volume should be calculated from Product details", "M3", invoiceLine.JI_VolumeUQ);
		}

		public void TestEmptyOverseasFreightExpressedInItsCurrency()
		{
			using (Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var koreanCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "GHS");
				Declaration.JE_ExportDate = ZDateTime.Today;
				SetExchangeRate(Declaration.JE_ExportDate.AddDays(-1), Declaration.JE_ExportDate.AddDays(1), RatesAreReciprocal ? 2m : 0.5m, koreanCurrency);

				InvoiceHeader.JZ_InvoiceAmount = 1000;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = koreanCurrency.RX_Code;
				InvoiceHeader.JZ_IncoTerm = "CIF";

				InvoiceHeader.Charges.RemoveAndDeleteAll();
				Declaration.TopGroupInvoice.Charges.RemoveAndDeleteAll();

				InvoiceHeader.Charges.AddNew(OverseasFreightCode, 0, koreanCurrency.RX_Code);
				InvoiceHeader.Charges.AddNew(OverseasInsuranceCode, 20, Declaration.LocalCurrencyCode);

				InvoiceLine.JI_LinePrice = 980m;
				Declaration.ResumeApportionment();
				CombineAssertions(() =>
				{
					AssertEquals("Two apportioned charges", 2, InvoiceLine.ApportionedCharges.Count);
					AssertEquals("Line OFT amount", 0m, InvoiceLine.JI_OverseasFreight.Amount);
					AssertEquals("Line OFT currency", koreanCurrency.RX_Code, InvoiceLine.JI_OverseasFreight.Currency.Code);
					AssertEquals("Line ONS currency", Declaration.LocalCurrencyCode, InvoiceLine.JI_OverseasInsurance.Currency.Code);
					AssertEquals("JI_Calc_Insurance", 10m, InvoiceLine.JI_Calc_InsuranceInInvoiceCurr);
					AssertEquals("JI_Calc_Freight", 0m, InvoiceLine.JI_Calc_FreightInInvoiceCurr);
				});
			}
		}

		public void TestJI_OverseasFreight()
		{
			using (Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				InvoiceHeader.JZ_InvoiceAmount = 1000m;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
				InvoiceHeader.Charges.AddNew(OverseasFreightCode, 100m, Declaration.LocalCurrencyCode);

				InvoiceLine.JI_LinePrice = 400m;
				var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 600m;
				Declaration.ResumeApportionment();
				CombineAssertions(() =>
				{
					AssertApportionedCharge(InvoiceLine, "Line 1", 40m, 40m);
					AssertApportionedCharge(invoiceLine2, "Line 2", 60m, 60m);
				});

				void AssertApportionedCharge(BaseJobComInvoiceLine invoiceLine, string testMessage, decimal chargeAmount, decimal overseasFreightAmount)
				{
					AssertEquals($"Single Apportioned Charge {testMessage}", 1, invoiceLine.ApportionedCharges.Count);
					var apportionedCharge = invoiceLine.ApportionedCharges[0];
					AssertEquals($"Charge Type {testMessage}", OverseasFreightCode, apportionedCharge.J7_ChargeType);
					AssertEquals($"Charge Amount {testMessage}", chargeAmount, apportionedCharge.J7_Amount);
					AssertEquals($"Overseas Freight Amount {testMessage}", overseasFreightAmount, invoiceLine.JI_OverseasFreight.Amount);
				}
			}
		}

		public void TestSetDefaultTaxOrFeeCode_NoRelatedTaxOrFeeCode()
		{
			var invoiceLine = CreateTariffAndFeeDetails(0);
			AssertEquals("No related TaxOrFeeCode and get tariff ZZ1_ZZF_NKTaxOrFeeCode: ZZT", invoiceLine.UseUniversalTariff ? "ZZT" : string.Empty, invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestSetDefaultTaxOrFeeCode_OneRelatedTaxOrFeeCode()
		{
			var invoiceLine = CreateTariffAndFeeDetails(1);
			AssertEquals("Has only one related TaxOrFeeCode: ZZ1", invoiceLine.UseUniversalTariff ? "ZZ1" : string.Empty, invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestSetDefaultTaxOrFeeCode_MorenThanOneRelatedTaxOrFeeCode()
		{
			var invoiceLine = CreateTariffAndFeeDetails(2);
			AssertEquals("Has more than one related TaxOrFeeCode and get the one with highest Value : ZZ2", invoiceLine.UseUniversalTariff ? "ZZ2" : string.Empty, invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestDefaultUniversalTariffProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("UseUniversalTariff", UseUniversalTariff, invoiceLine.UseUniversalTariff);
				AssertEquals("UniversalTariffType", UniversalTariffType, invoiceLine.UniversalTariffType);
			});
		}

		public void TestGetTopBusinessObject()
		{
			CombineAssertions(() =>
			{
				var controllerFactory = System.Reflection.Assembly.Load("Enterprise.ZArchitecture.GUI").GetType("Enterprise.ZArchitecture.Modules.ZControllerFactory").GetField("Instance").GetValue(null);
				var controller = controllerFactory.GetType().GetMethod("GetControllerForBizo").Invoke(controllerFactory, new[] { InvoiceLine.GetTopBusinessObject() });
				AssertNotNull("We got the controller, so we got the Form", controller);
				var baseControllerType = System.Reflection.Assembly.Load("Enterprise.Customs.Module").GetType("Enterprise.Customs.Module.JobDeclarationController");
				AssertEquals("It is a JobDeclarationController", true, baseControllerType.IsAssignableFrom(controller.GetType()));
			});
		}

		public void TestGetPivotByTypeAndOwnerSupplier_ImportWithAttributes_Attrib1()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MiscServ.OM_IMPartAttrib1Name = "PA 1";
			importer.MiscServ.OM_IMPartAttrib1Type = "MAN";
			AssertGetPivotByTypeAndOwnerSupplier_ImportWithAttributes(importer, (pivot) => pivot.Attributes1.AddNew(), (line, attribValue) => line.JI_PartAttrib1 = attribValue);
		}

		public void TestGetPivotByTypeAndOwnerSupplier_ImportWithAttributes_Attrib2()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MiscServ.OM_IMPartAttrib2Name = "PA 2";
			importer.MiscServ.OM_IMPartAttrib2Type = "MAN";
			AssertGetPivotByTypeAndOwnerSupplier_ImportWithAttributes(importer, (pivot) => pivot.Attributes2.AddNew(), (line, attribValue) => line.JI_PartAttrib2 = attribValue);
		}

		public void TestGetPivotByTypeAndOwnerSupplier_ImportWithAttributes_Attrib3()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MiscServ.OM_IMPartAttrib3Name = "PA 3";
			importer.MiscServ.OM_IMPartAttrib3Type = "MAN";
			AssertGetPivotByTypeAndOwnerSupplier_ImportWithAttributes(importer, (pivot) => pivot.Attributes3.AddNew(), (line, attribValue) => line.JI_PartAttrib3 = attribValue);
		}

		public void TestAttributeUpdatesPivot_Attrib1()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MiscServ.OM_IMPartAttrib1Name = "PA 1";
			importer.MiscServ.OM_IMPartAttrib1Type = "MAN";
			AssertAttributeUpdatesPivot(importer, (pivot) => pivot.Attributes1.AddNew(), (line, attribValue) => line.JI_PartAttrib1 = attribValue);
		}

		public void TestAttributeUpdatesPivot_Attrib2()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MiscServ.OM_IMPartAttrib2Name = "PA 2";
			importer.MiscServ.OM_IMPartAttrib2Type = "MAN";
			AssertAttributeUpdatesPivot(importer, (pivot) => pivot.Attributes2.AddNew(), (line, attribValue) => line.JI_PartAttrib2 = attribValue);
		}

		public void TestAttributeUpdatesPivot_Attrib3()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MiscServ.OM_IMPartAttrib3Name = "PA 3";
			importer.MiscServ.OM_IMPartAttrib3Type = "MAN";
			AssertAttributeUpdatesPivot(importer, (pivot) => pivot.Attributes3.AddNew(), (line, attribValue) => line.JI_PartAttrib3 = attribValue);
		}

		public void TestAddAddFetchHintForInitialiseData()
		{
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_OP = ZGuid.NewZGuid();
			Factory.ClearQueryCache();
			invoiceLine.AddAddFetchHintForInitialiseData();
			CombineAssertions(() =>
			{
				AssertEquals("OrgSupplierPart 0 all", 0, Factory.ActiveFetchHintsForTable(OrgSupplierPartSchema.Constants.TableName));

				invoiceLine.JI_ParentID = ZGuid.Empty;
				invoiceLine.JI_PartNo = ZString.Empty;
				invoiceLine.JI_OP = ZGuid.NewZGuid();
				Factory.ClearQueryCache();
				invoiceLine.AddAddFetchHintForInitialiseData();
				AssertEquals("OrgSupplierPart 0 JI_PartNo JI_OP", 0, Factory.ActiveFetchHintsForTable(OrgSupplierPartSchema.Constants.TableName));

				invoiceLine.JI_ParentID = ZGuid.Empty;
				invoiceLine.JI_PartNo = "123";
				invoiceLine.JI_OP = ZGuid.NewZGuid();
				Factory.ClearQueryCache();
				invoiceLine.AddAddFetchHintForInitialiseData();
				AssertEquals("OrgSupplierPart 0 JI_OP", 0, Factory.ActiveFetchHintsForTable(OrgSupplierPartSchema.Constants.TableName));

				invoiceLine.JI_ParentID = ZGuid.Empty;
				invoiceLine.JI_PartNo = "123";
				invoiceLine.JI_OP = ZGuid.Empty;
				Factory.ClearQueryCache();
				Factory.ExecuteAllFetchHints();
				invoiceLine.AddAddFetchHintForInitialiseData();
				AssertEquals("OrgSupplierPart 1", 1, Factory.ActiveFetchHintsForTable(OrgSupplierPartSchema.Constants.TableName));
			});
		}

		public void TestOnLoadedInitialiseData()
		{
			typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(invoiceLine, false);
			typeof(BaseJobComInvoiceLine).GetField("fPartSyncManager", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(invoiceLine, null);
			CombineAssertions(() =>
			{
				using (declaration.SuspendInvoiceLineDataInitialization())
				{
					invoiceLine.OnLoaded();
					AssertNull("InitialiseData not called after loaded when InitialiseInvoiceLineData suspended", invoiceLine.PartSyncManager);
				}

				invoiceLine.OnLoaded();
				AssertNotNull("InitialiseData called after loaded", invoiceLine.PartSyncManager);
				var hasInitialisedData = (bool)typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(invoiceLine);
				Assert("hasInitialisedData should be set to true after loaded", hasInitialisedData);
			});
		}

		public void TestSetDefaultValues_InitialisedData()
		{
			var hasInitialisedData = (bool)typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(invoiceLine);
			Assert("hasInitialisedData should be set to true in SetDefaultValues()", hasInitialisedData);
		}

		public void TestJI_RN_NKCountryOfExport_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_RN_NKCountryOfExportInfo, multipleResourceKey: null, "Country/Region Of Export", "Ctry./Rgn. of Exp.");
		}

		public void TestBondedWHSOrderNumberLength()
		{
			AssertEquals("WI00352209 : WD_DocketID && JI_BondedWHSOrderLineNumber should be equal as we want to copy value from  WD_DocketID in JI_BondedWHSOrderLineNumber.", WhsDocketSchema.WD_DocketID.MaxLength, AutoJobComInvoiceLine.Schema.JI_BondedWHSOrderNumberMaxLength);
		}

		public void TestIncludedInUniversalXML()
		{
			AssertEquals(true, invoiceLine.IncludedInUniversalXML);
		}

		public void TestIncludeEntryDetailsInUniversalXML()
		{
			AssertEquals(true, invoiceLine.IncludeEntryDetailsInUniversalXML);
		}

		public void TestIsWarehouseOrderNumberAndLinesVisible()
		{
			AssertIsWarehouseOrderLinesVisible("CD12", ji_BondedWarehouseOrderNumber: ZString.Empty, ji_BondedWarehouseOrderLineNumber: ZShort.Zero, isWarehouseOrderEnabled: false, expectedIsWarehouseOrderLinesVisible: false);
			AssertIsWarehouseOrderLinesVisible("CD13", ji_BondedWarehouseOrderNumber: ZString.Empty, ji_BondedWarehouseOrderLineNumber: ZShort.Zero, isWarehouseOrderEnabled: true, expectedIsWarehouseOrderLinesVisible: true);
			AssertIsWarehouseOrderLinesVisible("CD14", ji_BondedWarehouseOrderNumber: "10", ji_BondedWarehouseOrderLineNumber: ZShort.Zero, isWarehouseOrderEnabled: true, expectedIsWarehouseOrderLinesVisible: true);
			AssertIsWarehouseOrderLinesVisible("CD14", ji_BondedWarehouseOrderNumber: "10", ji_BondedWarehouseOrderLineNumber: ZShort.Zero, isWarehouseOrderEnabled: false, expectedIsWarehouseOrderLinesVisible: true);
			AssertIsWarehouseOrderLinesVisible("CD15", ji_BondedWarehouseOrderNumber: ZString.Empty, ji_BondedWarehouseOrderLineNumber: 1, isWarehouseOrderEnabled: false, expectedIsWarehouseOrderLinesVisible: true);
			AssertIsWarehouseOrderLinesVisible("CD15", ji_BondedWarehouseOrderNumber: "10", ji_BondedWarehouseOrderLineNumber: 1, isWarehouseOrderEnabled: false, expectedIsWarehouseOrderLinesVisible: true);
		}

		void AssertIsWarehouseOrderLinesVisible(string proc, string ji_BondedWarehouseOrderNumber, ZShort ji_BondedWarehouseOrderLineNumber, bool isWarehouseOrderEnabled, bool expectedIsWarehouseOrderLinesVisible)
		{
			var direction = isWarehouseOrderEnabled ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;
			var whsDataTestHelper = new WhsDataTestHelper(Factory);
			whsDataTestHelper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			whsDataTestHelper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			whsDataTestHelper.Warehouse.CompanyData.OB_IMUsedBondedWhs = isWarehouseOrderEnabled;
			whsDataTestHelper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = isWarehouseOrderEnabled;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, proc.Substring(0, 2), proc.Substring(2, 2), ZString.Empty, "description", direction);
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			var declaration = declarationMock.Object;
			declarationMock.Protected()
				.Setup<bool>("IsWarehouseOrderFunctionActivatedCore")
				.Returns(true);

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "AB";
			instruction.CEI_OH_Owner = whsDataTestHelper.Owner.PK;
			instruction.CEI_OA_Warehouse = whsDataTestHelper.Warehouse.MainAddress.PK;

			instruction.CEI_OA_Warehouse2 = whsDataTestHelper.Warehouse2.MainAddress.PK;
			declaration.JE_MessageType = direction;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
			invoiceLine.JI_BondedWHSOrderNumber = ji_BondedWarehouseOrderNumber;
			invoiceLine.JI_BondedWHSOrderLineNumber = ji_BondedWarehouseOrderLineNumber;

			var message = $"JI_BondedWHSOrderNumber" +
				$" is {ji_BondedWarehouseOrderNumber} and JI_BondedWHSOrderLineNumber is {ji_BondedWarehouseOrderLineNumber} And IsWarehouseOrderEnabled is {isWarehouseOrderEnabled} so invoiceline.IsWarehouseOrderVisible should be equal to {expectedIsWarehouseOrderLinesVisible}.";
			AssertEquals(message, expectedIsWarehouseOrderLinesVisible, invoiceLine.IsBondedWHSOrderNumberVisible);
		}

		public void TestIsWarehouseOrderEnabled()
		{
			AssertIsWarehouseOrderEnabled("AB71", WarehouseMoveStatus.Codes.Yes, isWarehouseOrderFunctionActivated: true, JobMessageTypeList.Codes.Import, entryInstructionIsBondedWarehouse: true, expectedIsWarehouseOrderEnabled: true);
			AssertIsWarehouseOrderEnabled("AB74", WarehouseMoveStatus.Codes.Yes, isWarehouseOrderFunctionActivated: true, JobMessageTypeList.Codes.Import, entryInstructionIsBondedWarehouse: false, expectedIsWarehouseOrderEnabled: false);
			AssertIsWarehouseOrderEnabled("AB72", WarehouseMoveStatus.Codes.Yes, isWarehouseOrderFunctionActivated: true, JobMessageTypeList.Codes.Export, entryInstructionIsBondedWarehouse: true, expectedIsWarehouseOrderEnabled: true);
			AssertIsWarehouseOrderEnabled("AB73", WarehouseMoveStatus.Codes.Yes, isWarehouseOrderFunctionActivated: false, JobMessageTypeList.Codes.Import, entryInstructionIsBondedWarehouse: true, expectedIsWarehouseOrderEnabled: false);
			AssertIsWarehouseOrderEnabled("CD12", WarehouseMoveStatus.Codes.No, isWarehouseOrderFunctionActivated: true, JobMessageTypeList.Codes.Import, entryInstructionIsBondedWarehouse: true, expectedIsWarehouseOrderEnabled: false);
		}

		void AssertIsWarehouseOrderEnabled(string proc, string hasOutOfWarehouseProcedure, bool isWarehouseOrderFunctionActivated, string direction, bool entryInstructionIsBondedWarehouse, bool expectedIsWarehouseOrderEnabled)
		{
			var whsDataTestHelper = new WhsDataTestHelper(Factory);
			whsDataTestHelper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			whsDataTestHelper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			whsDataTestHelper.Warehouse.CompanyData.OB_IMUsedBondedWhs = entryInstructionIsBondedWarehouse;
			whsDataTestHelper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = entryInstructionIsBondedWarehouse;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, proc.Substring(0, 2), proc.Substring(2, 2), ZString.Empty, "description", direction);
			procedure.ZZ6_OutOfWarehouse = hasOutOfWarehouseProcedure;
			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			var declaration = declarationMock.Object;
			declarationMock.Protected()
				.Setup<bool>("IsWarehouseOrderFunctionActivatedCore")
				.Returns(isWarehouseOrderFunctionActivated);

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "AB";
			instruction.CEI_OH_Owner = whsDataTestHelper.Owner.PK;
			instruction.CEI_OA_Warehouse = whsDataTestHelper.Warehouse.MainAddress.PK;
			instruction.CEI_OA_Warehouse2 = whsDataTestHelper.Warehouse2.MainAddress.PK;

			declaration.JE_MessageType = direction;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;

			var message = $"Direction is {direction} and EntryInstruction.WarehouseIsBOndedWarehousing  is {entryInstructionIsBondedWarehouse} and HasOutOfWarehouseProcedure is {hasOutOfWarehouseProcedure} and isWarehouseOrderFunctionActivated is {isWarehouseOrderFunctionActivated} so invoiceline.IsWarehouseOrderEnabled should be equal to {expectedIsWarehouseOrderEnabled}.";
			AssertEquals(message, expectedIsWarehouseOrderEnabled, invoiceLine.IsWarehouseOrderEnabled);
		}

		public void TestIsPreviousEntryNumberVisible_AtLeastOneBwhPropertyHasValue() => AssertBwhPropertyVisibleBecauseAtLeastOneBwhPropertyHasValue((x) => x.IsPreviousEntryNumberVisible);

		public void TestIsPreviousEntryNumberVisible_IsPreviousEntryNumberVisibleCore()
		{
			var invoiceLineForTest = invoice.JobComInvoiceLines.AddNew(typeof(BaseJobComInvoiceLineForTesting)) as BaseJobComInvoiceLineForTesting;
			Assert(invoiceLineForTest.IsPreviousEntryNumberVisible);
		}

		public void TestIsBondedWhsQuantityVisible_AtLeastOneBwhPropertyHasValue() => AssertBwhPropertyVisibleBecauseAtLeastOneBwhPropertyHasValue((x) => x.IsBondedWhsQuantityVisible);

		public void TestIsBondedWhsQuantityVisible_IsBondedWhsQuantityVisibleCore()
		{
			var invoiceLineForTest = invoice.JobComInvoiceLines.AddNew(typeof(BaseJobComInvoiceLineForTesting)) as BaseJobComInvoiceLineForTesting;
			Assert(invoiceLineForTest.IsBondedWhsQuantityVisible);
		}

		public void TestIsInvoiceLineComponentsVisible_ComponentInventoryCollectionHasAtLeastOneRecord()
		{
			var componentInventory = invoiceLine.ComponentInventoryCollection.AddNew();
			var invoiceLineWithoutComponentInventory = invoice.JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("invoiceLine: IsInvoiceLineComponentsVisible", expected: true, invoiceLine.IsInvoiceLineComponentsVisible);
				AssertEquals("invoiceLine2: IsInvoiceLineComponentsVisible", expected: false, invoiceLineWithoutComponentInventory.IsInvoiceLineComponentsVisible);
			});
		}

		public void TestJI_AddInfoSet()
		{
			var bo = Factory.New<BaseJobComInvoiceLine>();

			AssertEquals(ZString.Empty, bo.JI_AddInfo);
			AssertEquals(ZString.Empty, bo.JI_NAddInfo);

			bo.JI_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.JI_AddInfo);
			AssertEquals(ZString.Empty, bo.JI_NAddInfo);
		}

		public void TestJI_AddInfoSet_WithBaseAddInfo()
		{
			var bo = Factory.New<BaseJobComInvoiceLineWithBaseAddInfoForTesting>();

			AssertEquals(ZString.Empty, bo.JI_AddInfo);
			AssertEquals(ZString.Empty, bo.JI_NAddInfo);

			bo.JI_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.JI_AddInfo);
			AssertEquals("NString=def", bo.JI_NAddInfo);
		}

		public void TestJI_AddInfoSet_WithAddInfoWrapper()
		{
			var bo = Factory.New<BaseJobComInvoiceLineWithAddInfoWrapperForTesting>();

			AssertEquals(ZString.Empty, bo.JI_AddInfo);
			AssertEquals(ZString.Empty, bo.JI_NAddInfo);
			AssertEquals(ZString.Empty, bo.JI_String);
			AssertEquals(ZString.Empty, bo.JI_NString);

			bo.JI_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.JI_AddInfo);
			AssertEquals("", bo.JI_NAddInfo);
			AssertEquals("abc", bo.JI_String);
			AssertEquals("", bo.JI_NString);

			bo.JI_NAddInfo = "String=ghi*NString=jkl";

			AssertEquals("String=abc*NString=def", bo.JI_AddInfo);
			AssertEquals("String=ghi*NString=jkl", bo.JI_NAddInfo);
			AssertEquals("abc", bo.JI_String);
			AssertEquals("jkl", bo.JI_NString);
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			AssertEquals("Precondition", "ER", declaration.Branch.Country.Code);
			AssertEquals("ER", (invoiceLine as ITypeDeciderContext).Country);

			declaration.JE_GB = nzBranch.PK;
			AssertEquals("NZ", (invoiceLine as ITypeDeciderContext).Country);
		}

		public void TestCountryOfOriginFallback()
		{
			CreateEUCountry();

			var bo = Factory.New<BaseJobComInvoiceLine>();
			bo.JI_CountryOfOrigin = "EU";

			CombineAssertions(() =>
			{
				AssertEquals("Code of CountryOfOriginFallback of the invoice line should refer to EU", "EU", bo.CountryOfOriginFallback.RN_Code);
				AssertEquals("Description of CountryOfOriginFallback of the invoice line should refer to European Union", "European Union", bo.CountryOfOriginFallback.RN_Desc);
			});
		}

		public void TestCountryOfExportFallback()
		{
			CreateEUCountry();

			var bo = Factory.New<BaseJobComInvoiceLine>();
			bo.JI_RN_NKCountryOfExport = "EU";

			CombineAssertions(() =>
			{
				AssertEquals("Code of CountryOfExportFallback of the invoice line should refer to EU", "EU", bo.CountryOfExportFallback.RN_Code);
				AssertEquals("Description of CountryOfExportFallback of the invoice line should refer to European Union", "European Union", bo.CountryOfExportFallback.RN_Desc);
			});
		}

		void CreateEUCountry()
		{
			var refCountry = Factory.New<RefCountry>();
			refCountry.RN_Code = "EU";
			refCountry.RN_Desc = "European Union";
		}

		BaseJobDeclaration declaration => fDeclaration ?? (fDeclaration = Factory.New<BaseJobDeclaration>());
		BaseJobDeclaration fDeclaration;

		BaseJobComInvoiceHeader invoice => fInvoice ?? (fInvoice = declaration.Invoices.AddNew());
		BaseJobComInvoiceHeader fInvoice;

		BaseJobComInvoiceLine invoiceLine => fInvoiceLine ?? (fInvoiceLine = invoice.JobComInvoiceLines.AddNew());
		BaseJobComInvoiceLine fInvoiceLine;

		void AssertBwhPropertyVisibleBecauseAtLeastOneBwhPropertyHasValue(Func<BaseJobComInvoiceLine, bool> visibilityPropertyToCheck)
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_BondedWhsQuantity empty; JI_PreviousEntryNumber empty; JI_PreviousEntryLineNumber empty; IsBondedWhsQuantityVisibleCore false", expected: false, visibilityPropertyToCheck(invoiceLine));

				invoiceLine.JI_BondedWhsQuantity = 1;
				AssertEquals("JI_BondedWhsQuantity has value; JI_PreviousEntryNumber empty; JI_PreviousEntryLineNumber empty; IsBondedWhsQuantityVisibleCore false", expected: true, visibilityPropertyToCheck(invoiceLine));

				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				invoiceLine.JI_PreviousEntryNumber = "a";
				AssertEquals("JI_BondedWhsQuantity empty; JI_PreviousEntryNumber has value; JI_PreviousEntryLineNumber empty; IsBondedWhsQuantityVisibleCore false", expected: true, visibilityPropertyToCheck(invoiceLine));

				invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
				invoiceLine.JI_PreviousEntryLineNumber = 1;
				AssertEquals("JI_BondedWhsQuantity empty; JI_PreviousEntryNumber empty; JI_PreviousEntryLineNumber has value; IsBondedWhsQuantityVisibleCore false", expected: true, visibilityPropertyToCheck(invoiceLine));
			});
		}

		RefCusProcedure GenerateProcedure(string procedureCode, string isGuaranteeConsumed, string isGuaranteeReleased, string description, ZBool isCalculateVAT)
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = procedureCode;
			procedure.ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed;
			procedure.ZZ6_IsGuaranteeReleased = isGuaranteeReleased;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Description = description;
			procedure.ZZ6_CalculateVAT = isCalculateVAT;
			return procedure;
		}

		BusinessObject GetAddInfoAndPopulateWithData(BusinessObject addInfoParent)
		{
			BusinessObject result = null;
			if (ZCustomTypeDescriptor.GetProperties(addInfoParent.GetType())["AddInfo"] != null)
			{
				result = (BusinessObject)addInfoParent["AddInfo"];
				result.FillWithValidTestData(TestBusinessObjectKind.PopulateStrings, Array.Empty<PropertyDescriptor>());
			}
			return result;
		}

		void AssertUNDG(UNDGDataItem undg, ZString code, ZDecimal flashpoint, ZGuid contactPK, ZString technicalName, ZString pollutant)
		{
			AssertEquals(code, undg.UNDGSubstance.DG_Code);
			AssertEquals(flashpoint, undg.DI_DGFlashPoint);
			AssertEquals(contactPK, undg.DI_OC_DGContact);
			AssertEquals(technicalName, undg.DI_TechnicalName);
			AssertEquals(pollutant, undg.DI_MPMarinePollutant);
		}

		void RemoveUserEnterableInvoiceCurrExRate()
		{
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
		}

		void SetupUserEnterableInvoiceCurrExRate()
		{
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Belarus;
			invoice.JZ_InvoiceCurrExRateType = "FIX";
			invoice.JZ_InvoiceCurrExRate = 0.5m;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
		}

		OrgSupplierPart SetupProduct()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			invoice.JZ_OH_Supplier = supplier.PK;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TestPartNum";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "SUP";
			relation.OU_OH = supplier.PK;
			return product;
		}

		BaseJobComInvoiceLine CreateTariffAndFeeDetails(int vatApplicablityNumber)
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var countryCode = Core.Constants.CountryCodes.Eritrea;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(countryCode, UniversalTariffTypeForDefaultTaxOrFeeCode).PK;
			for (int i = 1; i <= vatApplicablityNumber; i++)
			{
				var rate = 0.01m * (i + 1m);
				var taxOrFee = helper.CreateTaxOrFee("ZZ" + i.ToString(), rate, countryCode);
				taxOrFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			}
			Factory.Save();
			var tariff = helper.CreateTariff(countryCode, tariffTypePK, "99999999", startDate, endDate, taxOrFeeCode: "ZZT");

			for (int i = 1; i <= vatApplicablityNumber; i++)
			{
				helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "ZZ" + i.ToString(), "AdditionalCode" + i.ToString(), startDate, endDate);
				if (i > 10)
				{
					throw new Exception("It can only create up to 9 VAT applicabilitty records....");
				}
			}

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			return invoiceLine;
		}

		void AssertGetPivotByTypeAndOwnerSupplier_ImportWithAttributes(OrgHeader importer, Func<BaseCusClassPartPivot, CusAttributeFilter> getNewPivotAttribFilter, Action<BaseJobComInvoiceLine, string> assignInvoiceLineAttribute)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.JI_PartNo = "1234";

			var part = Factory.New<OrgSupplierPart>();
			var relatedOrg = part.RelatedOrganisations.AddNew();
			relatedOrg.OU_OH = supplier.PK;
			relatedOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part.OP_PartNum = "1234";
			line.SetPartForTesting(part);
			AssertNull("No Pivot for Suppplier", line.GetPivot());

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_OH = importer.PK;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var attrib = getNewPivotAttribFilter(pivot);
			attrib.BG_AttributeValue1 = "1";
			AssertNull("No Pivot for Importer", line.GetPivot());

			assignInvoiceLineAttribute(line, "1");
			AssertEquals("Assign Attribute", pivot, line.GetPivot());
		}

		void AssertAttributeUpdatesPivot(OrgHeader importer, Func<BaseCusClassPartPivot, CusAttributeFilter> getNewPivotAttribFilter, Action<BaseJobComInvoiceLine, string> assignInvoiceLineAttribute)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.JI_PartNo = "1234";

			var part = Factory.New<OrgSupplierPart>();
			var relatedOrg = part.RelatedOrganisations.AddNew();
			relatedOrg.OU_OH = supplier.PK;
			relatedOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part.OP_PartNum = "1234";
			line.SetPartForTesting(part);
			AssertNull("No Pivot for Suppplier", line.Pivot);

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_OH = importer.PK;
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "123";
			var attrib1 = getNewPivotAttribFilter(pivot1);
			attrib1.BG_AttributeValue1 = "1";
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_OH = importer.PK;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = "456";
			var attrib2 = getNewPivotAttribFilter(pivot2);
			attrib2.BG_AttributeValue1 = "2";
			AssertNull("No Pivot for Importer", line.Pivot);

			assignInvoiceLineAttribute(line, "1");
			AssertEquals("Assign Attribute", "123", line.JI_Tariff);
			AssertEquals("Assign Attribute", pivot1, line.Pivot);

			assignInvoiceLineAttribute(line, "2");
			AssertEquals("Assign Attribute", "456", line.JI_Tariff);
			AssertEquals("Assign Attribute", pivot2, line.Pivot);
		}

		sealed class BaseJobComInvoiceLineForTesting : Testing.BaseJobComInvoiceLineForTesting
		{
			public BaseJobComInvoiceLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategyReturns { get; set; }
			public bool IsExtendedCommercialDescriptionEnabledReturns { get; set; }

			protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => GetCustomsUnitDefaultingStrategyReturns;
			protected override ZBool IsBondedWhsQuantityVisibleCore => true;
			protected override ZBool IsPreviousEntryNumberVisibleCore => true;
			public override bool IsExtendedCommercialDescriptionEnabled => IsExtendedCommercialDescriptionEnabledReturns;
		}

		sealed class BaseJobDeclarationForTesting : BaseJobDeclaration
		{
			public BaseJobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool HasNotificationsNotIncludingChildrenReturns { get; set; }
			public bool IsUNDGSupportedOnInvoiceLinesReturns { get; set; }

			protected override bool HasNotificationsNotIncludingChildren(INotificationType notificationType) => HasNotificationsNotIncludingChildrenReturns;
			protected internal override bool IsUNDGSupportedOnInvoiceLines => IsUNDGSupportedOnInvoiceLinesReturns;

			protected override bool SupportInwardProcessingCore => true;

			protected internal override bool IsAllocatedQuantityRequiredForBondedWarehouse => true;
		}

		sealed class BaseJobComInvoiceLineWithBaseAddInfoForTesting : BaseJobComInvoiceLine, IAddInfoManager, INAddInfoSupporter
		{
			public BaseJobComInvoiceLineWithBaseAddInfoForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public TestAddInfo AddInfo => addInfo ?? (addInfo = new TestAddInfo(this));
			TestAddInfo addInfo;

			IAddInfo IAddInfoManager.AddInfo => AddInfo;
			public ZPropertyInfoString NAddInfoProperty => JI_NAddInfoInfo as ZPropertyInfoString;
		}

		sealed class BaseJobComInvoiceLineWithAddInfoWrapperForTesting : BaseJobComInvoiceLine, IAddInfoManagerWithSchema
		{
			public BaseJobComInvoiceLineWithAddInfoWrapperForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				addInfo = new AddInfoWrapper<BaseJobComInvoiceLineWithAddInfoWrapperForTesting>(
					this,
					Schema.JI_AddInfo,
					() => AddInfoNamesMapping,
					Schema.JI_NAddInfo,
					() => NAddInfoNamesMapping
				);
			}

			public ZString JI_String
			{
				get => JI_StringData.Value;
				set
				{
					SetNonPersistentPropertyValue(JI_StringInfo, ref JI_StringData.Value, value);
				}
			}
			public ZPropertyInfo JI_StringInfo => GetZPropertyInfo(nameof(JI_String));
			AddInfoPropertyData<ZString> JI_StringData => ji_String ?? (ji_String = new AddInfoPropertyData<ZString>(nameof(JI_String)));
			AddInfoPropertyData<ZString> ji_String;

			public ZString JI_NString
			{
				get => JI_NStringData.Value;
				set
				{
					SetNonPersistentPropertyValue(JI_NStringInfo, ref JI_NStringData.Value, value);
				}
			}
			public ZPropertyInfo JI_NStringInfo => GetZPropertyInfo(nameof(JI_NString));
			AddInfoPropertyData<ZString> JI_NStringData => ji_NString ?? (ji_NString = new AddInfoPropertyData<ZString>(nameof(JI_NString)));
			AddInfoPropertyData<ZString> ji_NString;

			IDictionary<string, IAddInfoPropertyData> AddInfoNamesMapping => addInfoNamesMapping ?? (addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData> { { "String", JI_StringData } });
			IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping;

			IDictionary<string, IAddInfoPropertyData> NAddInfoNamesMapping => nAddInfoNamesMapping ?? (nAddInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData> { { "NString", JI_NStringData } });
			IDictionary<string, IAddInfoPropertyData> nAddInfoNamesMapping;

			IAddInfo IAddInfoManager.AddInfo => addInfo;
			readonly IAddInfo addInfo;

			ITableSchema IAddInfoManagerWithSchema.AddInfoSchema => TWJobDeclarationSchema.Instance;
		}
	}
}
