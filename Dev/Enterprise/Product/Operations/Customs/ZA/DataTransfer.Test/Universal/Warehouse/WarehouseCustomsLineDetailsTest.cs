using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var procedureCode = Factory.New<RefCusProcedure>();
			procedureCode.ZZ6_ProcedureCode = "DS";
			procedureCode.ZZ6_PreviousProcedureCode = "10";
			procedureCode.ZZ6_Concession = "000";
			procedureCode.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedureCode.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedureCode.ZZ6_Description = "DS DESC";
			var invoiceLine = CreateInvoiceLine();
			var rooCertAddInfo = new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_ROOCert.Substring(3), Value = "ROO32423" };
			invoiceLine.AddInfoCollection.Add(rooCertAddInfo);
			invoiceLine.EntryInstructionLink = 1;
			invoiceLine.Procedure = "20";
			var fallbackDetail = new WarehouseCustomsFallbackDetailWithEntryInstruction()
			{
				EntryInstructionProcedureMap = new Dictionary<ZInt, ZString>(),
				EntryInstructionEntryHeaderMap = new Dictionary<ZInt, List<EntryHeader>>(),
				IsExport = false,
				InvoiceLineAddInfosApplicableForInwardWarehousing = Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing()
			};
			fallbackDetail.EntryInstructionProcedureMap.Add(1, "DS");
			fallbackDetail.EntryInstructionProcedureMap.Add(2, "KD");
			var bondValidToDate = new ZDateTime(2020, 6, 30);
			var bondValidToDate2 = bondValidToDate.AddMonths(1);
			fallbackDetail.EntryInstructionEntryHeaderMap.Add(1, new List<EntryHeader>() { new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { BondValidToDate = bondValidToDate } });
			fallbackDetail.EntryInstructionEntryHeaderMap.Add(2, new List<EntryHeader>() { new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { BondValidToDate = bondValidToDate2 } });

			var fallbackAddInfos = new Dictionary<ZString, UniversalAddInfo>();
			fallbackAddInfos.Add(JobComInvoiceHeader.Schema.JZ_ROOType.Substring(3), new UniversalAddInfo() { Key = JobComInvoiceHeader.Schema.JZ_ROOType.Substring(3), Value = UniversalReferenceConstants.TradeAgreement.EUTRADE });
			fallbackAddInfos.Add(JobComInvoiceHeader.Schema.JZ_ROOCert.Substring(3), new UniversalAddInfo() { Key = JobComInvoiceHeader.Schema.JZ_ROOCert.Substring(3), Value = "ROOH6994" });
			fallbackDetail.FallbackAddInfos = fallbackAddInfos;
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", UniversalReferenceConstants.PrimaryPreference.PreferentialRate, "1020304050", 1000m, "12345000000023", 2, "CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG*EngineNumber=ENG32425*NewUsed=S*ROOCert=ROO32423*VIN=VIN23423*OriginalProcedureCode=DS", null, null, bondValidToDate, "DS", "20");

			invoiceLine.Procedure = "DS10000";
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", UniversalReferenceConstants.PrimaryPreference.PreferentialRate, "1020304050", 1000m, "12345000000023", 2, "", "12345000000012", 3, bondValidToDate, "DS", "DS10000");

			invoiceLine.EntryInstructionLink = 2;
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", UniversalReferenceConstants.PrimaryPreference.PreferentialRate, "1020304050", 1000m, "12345000000023", 2, "CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG*EngineNumber=ENG32425*NewUsed=S*ROOCert=ROO32423*VIN=VIN23423*OriginalProcedureCode=KD", null, null, bondValidToDate2, "KD", "DS10000");

			invoiceLine.EntryInstructionLink = 3;
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", UniversalReferenceConstants.PrimaryPreference.PreferentialRate, "1020304050", 1000m, "12345000000023", 2, "CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG*EngineNumber=ENG32425*NewUsed=S*ROOCert=ROO32423*VIN=VIN23423", null, null, null, "", "DS10000");

			invoiceLine.AddInfoCollection.Remove(rooCertAddInfo);
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", UniversalReferenceConstants.PrimaryPreference.PreferentialRate, "1020304050", 1000m, "12345000000023", 2, "CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG*EngineNumber=ENG32425*NewUsed=S*ROOCert=ROOH6994*VIN=VIN23423", null, null, null, "", "DS10000");

			invoiceLine.PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", UniversalReferenceConstants.PrimaryPreference.Standard, "1020304050", 1000m, "12345000000023", 2, "CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG*EngineNumber=ENG32425*NewUsed=S*ROOCert=ROOH6994*VIN=VIN23423", null, null, null, "", "DS10000");

			invoiceLine.PrimaryPreference = null;
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", null, "1020304050", 1000m, "12345000000023", 2, "CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG*EngineNumber=ENG32425*NewUsed=S*ROOCert=ROOH6994*VIN=VIN23423", null, null, null, "", "DS10000");

			fallbackDetail.IsExport = true;
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", null, "1020304050", 1000m, "12345000000023", 2, "CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG*EngineNumber=ENG32425*NewUsed=S*ROOCert=ROOH6994*VIN=VIN23423", null, null, null, "", "DS10000");

			var shipment = CreateShipment();

			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail, shipment);
			AssertWarehouseCustomsLineDetails(lineDetails, "NZ", 10m, "BX", 50m, "KG", null, "1020304050", 100.5, "12345000000023", 2, "CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG*EngineNumber=ENG32425*NewUsed=S*ROOCert=ROOH6994*VIN=VIN23423", null, null, null, "", "DS10000");
		}

		Shipment CreateShipment()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.WarehouseReceive, null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
			};
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[]
				{
					new EntryInstruction()
					{
						Link = 1
					},
					new EntryInstruction()
					{
						Link = 2
					}
				}));

			shipment.SetEntryHeaderCollection(() => new List<EntryHeader>(new[]
				{
					new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Reference = "Entry1",
						EntryInstructionLink = 3,
						EntryLineCollection = new List<EntryLine>(new[]
						{
							new EntryLine()
							{
								LineNumber = 1,
								CustomsValue = 50.1
							},
							new EntryLine()
							{
								LineNumber = 2,
								CustomsValue = 100.5
							}
						})
					},
					new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Reference = "Entry2",
						EntryInstructionLink = 2,
					},
				}));
			return shipment;
		}

		CommercialInvoiceLine CreateInvoiceLine()
		{
			var invoiceLine = new CommercialInvoiceLine()
			{
				LineNo = 1,
				EntryLineNumber = 2,
				EntryNumber = "12345000000023",
				PreviousEntryNumber = "12345000000012",
				PreviousEntryLineNumber = 3,
				PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.PreferentialRate,
				HarmonisedCode = "1020304050",
				CountryOfOrigin = new Country() { Code = "NZ" },
				CustomsValue = 1000m,
				CustomsQuantity = 10m,
				CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "BX" },
				CustomsSecondQuantity = 50m,
				CustomsSecondQuantityUnit = new CodeDescriptionPair6Char() { Code = "KG" },
				CustomsThirdQuantity = 20m,
				CustomsThirdQuantityUnit = new CodeDescriptionPair6Char() { Code = "BAG" },
				AddInfoCollection = new List<UniversalAddInfo>(new[]
				{
					new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_ImportTariff.Substring(3), Value = "9902" },
					new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride.Substring(3), Value = "USD" },
					new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_VIN.Substring(3), Value = "VIN23423" },
					new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_EngineNumber.Substring(3), Value = "ENG32425" },
					new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_NewUsed.Substring(3), Value = "S" }
				})
			};
			return invoiceLine;
		}

		void AssertWarehouseCustomsLineDetails(WarehouseCustomsLineDetails lineDetails, ZString countryOfOrigin, ZDecimal customsQty, ZString customsUQ, ZDecimal customsSecondQty, ZString customsSecondUQ,
			ZString? primaryPreference, ZString tariff, ZDecimal valueForDuty, ZString entryNumber, ZShort entryLineNumber, ZString addInfos, ZString? previousEntryNumber, ZShort? previousEntryLineNumber,
			ZDateTime? customsDeadline, ZString? inwardStyle, ZString? inwardProcedure)
		{
			AssertEquals("CountryOfOrigin", countryOfOrigin, lineDetails.CountryOfOrigin.GetCodeAsUpperCase());
			AssertEquals("CustomsQuantity", customsQty, lineDetails.CustomsQuantity);
			AssertEquals("CustomsQuantityUnit", customsUQ, lineDetails.CustomsQuantityUnit.GetCodeAsUpperCase());
			AssertEquals("CustomsSecondQuantity", customsSecondQty, lineDetails.CustomsSecondQuantity);
			AssertEquals("CustomsSecondQuantityUnit", customsSecondUQ, lineDetails.CustomsSecondQuantityUnit.GetCodeAsUpperCase());
			AssertEquals("EntryNumber", entryNumber, lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", entryLineNumber, lineDetails.EntryLineNumber);
			AssertEquals("PrimaryPreference", primaryPreference, lineDetails.PrimaryPreference);
			AssertEquals("Tariff", tariff, lineDetails.Tariff);
			AssertEquals("ValueForDuty", valueForDuty, lineDetails.ValueForDuty);
			AssertEquals("AddInfos", addInfos, lineDetails.AddInfos);
			AssertEquals("PreviousEntryNumber", previousEntryNumber, lineDetails.PreviousEntryNumber);
			AssertEquals("PreviousEntryLineNumber", previousEntryLineNumber, lineDetails.PreviousEntryLineNumber);
			AssertEquals("CustomsDeadline", customsDeadline, lineDetails.CustomsDeadline);
			AssertEquals("InwardStyle", inwardStyle, lineDetails.Style);
			AssertEquals("InwardProcedure", inwardProcedure, lineDetails.Procedure);
		}
	}
}
