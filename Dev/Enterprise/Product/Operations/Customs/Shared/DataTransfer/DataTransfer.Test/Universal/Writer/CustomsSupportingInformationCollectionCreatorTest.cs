using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestCusSupportingInfoExtraMappings()
		{
			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("ABC", "ABC DESC");
			var packTypeList = new CodeDescriptionPairList();
			packTypeList.AddPair("UP1", "UP1 DESC");
			var mock = Factory.NewMoq<CusSupportingInfo>();
			var supportingInfo = mock.Object;
			var lookupMock = new Mock<CusSupportingInfoLookups>(supportingInfo);
			lookupMock.SetupGet(x => x.PackTypeList).Returns(packTypeList);
			mock.Protected().Setup<CusSupportingInfoLookups>("GetNewLookups").Returns(lookupMock.Object);
			supportingInfo.CSI_PackQty = 235;
			supportingInfo.CSI_PackType = "UP1";
			supportingInfo.CSI_AdditionalDescription = "TEST ADD DESC";
			supportingInfo.CSI_Quantity3 = 150m;
			supportingInfo.CSI_UnitOfQuantity3 = Core.Constants.PkgUnit.Bag;

			var customsSupportingInformation = CustomsSupportingInformationCollectionCreator.Create(supportingInfo, typeList, new DataWritingManager(new ActionInfo(RecipientRoleType.AFR, supportingInfo)));
			CombineAssertions(() =>
			{
				AssertEquals("PackQuantity", 235, customsSupportingInformation.PackQuantity);
				AssertEquals("PackUnitOfQuantity.Code", "UP1", customsSupportingInformation.PackUnitOfQuantity.Code);
				AssertEquals("PackUnitOfQuantity.Description", "UP1 DESC", customsSupportingInformation.PackUnitOfQuantity.Description);
				AssertEquals("AdditionalDescription", "TEST ADD DESC", customsSupportingInformation.AdditionalDescription);
				AssertEquals("Quantity3", 150m, customsSupportingInformation.Quantity3);
				AssertEquals("UnitOfQuantity3.Code", Core.Constants.PkgUnit.Bag, customsSupportingInformation.UnitOfQuantity3.Code);
			});
		}

		public void TestCusSupportingInfoMappings()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.SaveForTesting();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.GB.IJobDeclaration>();
				var decAdditionalInfo = Factory.BOFactory.New<Integration.Customs.GB.IAdditionalInfo>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)decAdditionalInfo,

					parentTableCode: declaration.TablePrefix,
					parentID: declaration.PK,
					code: "9001",
					country: Core.Constants.CountryCodes.Afghanistan,
					customsOffice: "CD1",

					dateOfIssue: new ZDate(2017, 1, 1),
					description: "DEC SUP DESC 1",
					lineNo: 1,
					procedure: "PD1",
					quantity: 1.1m,

					quantity2: 2.2m,
					referenceNumber: "RD1",
					referenceNumber2: "RD12",
					status: "AB",
					subType: "SD1",

					tariff: "TR1",
					unitOfQuantity: Core.Constants.PkgUnit.Bag,
					unitOfQuantity2: Core.Constants.PkgUnit.BaleCompressed,

					dateOfExpiry: new ZDate(2020, 04, 08),
					valueCurrency: Core.Constants.CurrencyCodes.EuropeanUnion,
					value: 999m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				var decPreviousDocument = Factory.BOFactory.New<Integration.Customs.GB.IPreviousDocument>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)decPreviousDocument,

					parentTableCode: declaration.TablePrefix,
					parentID: declaration.PK,
					code: "9002",
					country: Core.Constants.CountryCodes.AlandIslands,
					customsOffice: "CD2",

					dateOfIssue: new ZDate(2017, 1, 2),
					description: "DEC SUP DESC 2",
					lineNo: 2,
					procedure: "PD2",
					quantity: 3.3m,

					quantity2: 4.4m,
					referenceNumber: "RD2",
					referenceNumber2: "RD22",
					status: "BC",
					subType: "SD2",

					tariff: "TR2",
					unitOfQuantity: Core.Constants.PkgUnit.BaleUncompressed,
					unitOfQuantity2: Core.Constants.PkgUnit.Basket,

					dateOfExpiry: new ZDate(2020, 04, 09),
					valueCurrency: Core.Constants.CurrencyCodes.UnitedStates,
					value: 1000m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				var decSupportingDocument = Factory.BOFactory.New<Integration.Customs.GB.ISupportingDocument>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)decSupportingDocument,

					parentTableCode: declaration.TablePrefix,
					parentID: declaration.PK,
					code: "9002",
					country: Core.Constants.CountryCodes.Albania,
					customsOffice: "CD3",

					dateOfIssue: new ZDate(2017, 1, 3),
					description: "DEC SUP DESC 3",
					lineNo: 3,
					procedure: "PD3",
					quantity: 5.5m,

					quantity2: 6.6m,
					referenceNumber: "RD3",
					referenceNumber2: "RD32",
					status: "CD",
					subType: "SD3",

					tariff: "TR3",
					unitOfQuantity: Core.Constants.PkgUnit.Bottle,
					unitOfQuantity2: Core.Constants.PkgUnit.Box,

					dateOfExpiry: new ZDate(2020, 04, 10),
					valueCurrency: Core.Constants.CurrencyCodes.Australia,
					value: 1001m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				var invoice = declaration.Invoices.AddNew();
				var invoiceAdditionalInfo = Factory.BOFactory.New<Integration.Customs.GB.IAdditionalInfo>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)invoiceAdditionalInfo,

					parentTableCode: invoice.TablePrefix,
					parentID: invoice.PK,
					code: "9011",
					country: Core.Constants.CountryCodes.Bahamas,
					customsOffice: "CI1",

					dateOfIssue: new ZDate(2017, 2, 1),
					description: "INV SUP DESC 1",
					lineNo: 1,
					procedure: "PI1",
					quantity: 2.1m,

					quantity2: 3.2m,
					referenceNumber: "RI1",
					referenceNumber2: "RI12",
					status: "DE",
					subType: "SI1",

					tariff: "TR4",
					unitOfQuantity: Core.Constants.PkgUnit.Carton,
					unitOfQuantity2: Core.Constants.PkgUnit.Case,

					dateOfExpiry: new ZDate(2020, 04, 11),
					valueCurrency: Core.Constants.CurrencyCodes.Singapore,
					value: 1002.2m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				var invoicePreviousDocument = Factory.BOFactory.New<Integration.Customs.GB.IPreviousDocument>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)invoicePreviousDocument,

					parentTableCode: invoice.TablePrefix,
					parentID: invoice.PK,
					code: "9012",
					country: Core.Constants.CountryCodes.Bahrain,
					customsOffice: "CI2",

					dateOfIssue: new ZDate(2017, 1, 2),
					description: "INV SUP DESC 2",
					lineNo: 2,
					procedure: "PI2",
					quantity: 4.3m,

					quantity2: 5.4m,
					referenceNumber: "RI2",
					referenceNumber2: "RI22",
					status: "EF",
					subType: "SI2",

					tariff: "TR5",
					unitOfQuantity: Core.Constants.PkgUnit.Coil,
					unitOfQuantity2: Core.Constants.PkgUnit.Container,

					dateOfExpiry: ZDate.Empty,
					valueCurrency: Core.Constants.CurrencyCodes.Malawi,
					value: 0m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				var invoiceSupportingDocument = Factory.BOFactory.New<Integration.Customs.GB.ISupportingDocument>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)invoiceSupportingDocument,

					parentTableCode: invoice.TablePrefix,
					parentID: invoice.PK,
					code: "9012",
					country: Core.Constants.CountryCodes.Bangladesh,
					customsOffice: "CI3",

					dateOfIssue: new ZDate(2017, 1, 3),
					description: "INV SUP DESC 3",
					lineNo: 3,
					procedure: "PI3",
					quantity: 6.5m,

					quantity2: 7.6m,
					referenceNumber: "RI3",
					referenceNumber2: "RI32",
					status: "FG",
					subType: "SI3",

					tariff: "TR6",
					unitOfQuantity: Core.Constants.PkgUnit.Cradle,
					unitOfQuantity2: Core.Constants.PkgUnit.Crate,

					dateOfExpiry: ZDate.Empty,
					valueCurrency: Core.Constants.CurrencyCodes.Swaziland,
					value: 1m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var invoiceLineAdditionalInfo = Factory.BOFactory.New<Integration.Customs.GB.IAdditionalInfo>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)invoiceLineAdditionalInfo,

					parentTableCode: invoiceLine.TablePrefix,
					parentID: invoiceLine.PK,
					code: "9021",
					country: Core.Constants.CountryCodes.Cambodia,
					customsOffice: "CL1",

					dateOfIssue: new ZDate(2017, 3, 1),
					description: "INVLINE SUP DESC 1",
					lineNo: 1,
					procedure: "PL1",
					quantity: 3.1m,

					quantity2: 4.2m,
					referenceNumber: "RL1",
					referenceNumber2: "RL12",
					status: "GH",
					subType: "SL1",

					tariff: "TR7",
					unitOfQuantity: Core.Constants.PkgUnit.Dozen,
					unitOfQuantity2: Core.Constants.PkgUnit.Drum,

					dateOfExpiry: new ZDate(2020, 01, 01),
					valueCurrency: Core.Constants.CurrencyCodes.Switzerland,
					value: 100.12m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				var invoiceLinePreviousDocument = Factory.BOFactory.New<Integration.Customs.GB.IPreviousDocument>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)invoiceLinePreviousDocument,

					parentTableCode: invoiceLine.TablePrefix,
					parentID: invoiceLine.PK,
					code: "9022",
					country: Core.Constants.CountryCodes.Cameroon,
					customsOffice: "CL2",

					dateOfIssue: new ZDate(2017, 1, 2),
					description: "INVLINE SUP DESC 2",
					lineNo: 2,
					procedure: "PL2",
					quantity: 5.3m,

					quantity2: 6.4m,
					referenceNumber: "RL2",
					referenceNumber2: "RL22",
					status: "HI",
					subType: "SL2",

					tariff: "TR8",
					unitOfQuantity: Core.Constants.PkgUnit.Envelope,
					unitOfQuantity2: Core.Constants.PkgUnit.Gross,

					dateOfExpiry: new ZDate(2020, 01, 02),
					valueCurrency: Core.Constants.CurrencyCodes.Italy,
					value: 10000m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				var invoiceLineSupportingDocument = Factory.BOFactory.New<Integration.Customs.GB.ISupportingDocument>();
				SetupCusSupportingInfo(
					supportingInfoBO: (CusSupportingInfo)invoiceLineSupportingDocument,

					parentTableCode: invoiceLine.TablePrefix,
					parentID: invoiceLine.PK,
					code: "9022",
					country: Core.Constants.CountryCodes.Canada,
					customsOffice: "CL3",

					dateOfIssue: new ZDate(2017, 1, 3),
					description: "INVLINE SUP DESC 3",
					lineNo: 3,
					procedure: "PL3",
					quantity: 7.5m,

					quantity2: 8.6m,
					referenceNumber: "RL3",
					referenceNumber2: "RL32",
					status: "IJ",
					subType: "SL3",

					tariff: "TR9",
					unitOfQuantity: Core.Constants.PkgUnit.Keg,
					unitOfQuantity2: Core.Constants.PkgUnit.Mix,

					dateOfExpiry: new ZDate(2020, 01, 03),
					valueCurrency: "XXX",
					value: 10001m,

					additionalDescription: "Test Desc2",
					issuerType: "ISSUERTYPE",
					itemNumber: 1,
					packQty: 2,
					packType: Core.Constants.PkgUnit.Package,
					quantity3: 3.3m,
					unitOfQuantity3: Core.Constants.PkgUnit.Bottle
				);
				Factory.SaveForTesting();
				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var shipmentData = (UniversalShipment)writer.GetDataObject(declaration);
				AssertEquals("shipmentData.CustomsSupportingInformationCollection.Count", 3, shipmentData.CustomsSupportingInformationCollection.Count);
				AssertContents(
					supportingInformationData: shipmentData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, Common.EU.CusSupportingInfoTypeList.Descriptions.AdditionalInfo),
					type: GetCodeDescriptionPair("9001", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.Afghanistan, "Afghanistan"),
					customsOffice: GetCodeDescriptionPair("CD1", null),
					dateOfIssue: new ZDate(2017, 1, 1),

					description: "DEC SUP DESC 1",
					lineNo: 1,
					procedure: GetCodeDescriptionPair("PD1", null),
					quantity: 1.1m,
					quantity2: 2.2m,

					referenceNumber: "RD1",
					referenceNumber2: "RD12",
					status: GetCodeDescriptionPair("AB", null),
					subType: GetCodeDescriptionPair("SD1", null),
					tariff: "TR1",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bag, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.BaleCompressed, null),

					dateOfExpiry: new ZDate(2020, 04, 08),
					valueCurrency: new Currency { Code = Core.Constants.CurrencyCodes.EuropeanUnion, Description = "Euro" },
					value: 999m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);
				AssertContents(
					supportingInformationData: shipmentData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.PreviousDocument),
					type: GetCodeDescriptionPair("9002", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.AlandIslands, "Aland Islands"),
					customsOffice: GetCodeDescriptionPair("CD2", null),
					dateOfIssue: new ZDate(2017, 1, 2),

					description: "DEC SUP DESC 2",
					lineNo: 2,
					procedure: GetCodeDescriptionPair("PD2", null),
					quantity: 3.3m,
					quantity2: 4.4m,

					referenceNumber: "RD2",
					referenceNumber2: "RD22",
					status: GetCodeDescriptionPair("BC", null),
					subType: GetCodeDescriptionPair("SD2", null),
					tariff: "TR2",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.BaleUncompressed, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.Basket, null),

					dateOfExpiry: new ZDate(2020, 04, 09),
					valueCurrency: new Currency { Code = Core.Constants.CurrencyCodes.UnitedStates, Description = "United States Dollar" },
					value: 1000m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);
				AssertContents(
					supportingInformationData: shipmentData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.SupportingDocument),
					type: GetCodeDescriptionPair("9002", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.Albania, "Albania"),
					customsOffice: GetCodeDescriptionPair("CD3", null),
					dateOfIssue: new ZDate(2017, 1, 3),

					description: "DEC SUP DESC 3",
					lineNo: 3,
					procedure: GetCodeDescriptionPair("PD3", null),
					quantity: 5.5m,
					quantity2: 6.6m,

					referenceNumber: "RD3",
					referenceNumber2: "RD32",
					status: GetCodeDescriptionPair("CD", null),
					subType: GetCodeDescriptionPair("SD3", null),
					tariff: "TR3",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.Box, null),

					dateOfExpiry: new ZDate(2020, 04, 10),
					valueCurrency: new Currency { Code = Core.Constants.CurrencyCodes.Australia, Description = "Australian Dollar" },
					value: 1001m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);

				AssertEquals("shipmentData.CommercialInfo.CommercialInvoiceCollection.Count", 1, shipmentData.CommercialInfo.CommercialInvoiceCollection.Count);
				var invoiceData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0];
				AssertEquals("invoiceData.CustomsSupportingInformationCollection.Count", 3, invoiceData.CustomsSupportingInformationCollection.Count);
				AssertContents(
					supportingInformationData: invoiceData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, Common.EU.CusSupportingInfoTypeList.Descriptions.AdditionalInfo),
					type: GetCodeDescriptionPair("9011", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.Bahamas, "Bahamas"),
					customsOffice: GetCodeDescriptionPair("CI1", null),
					dateOfIssue: new ZDate(2017, 2, 1),

					description: "INV SUP DESC 1",
					lineNo: 1,
					procedure: GetCodeDescriptionPair("PI1", null),
					quantity: 2.1m,
					quantity2: 3.2m,

					referenceNumber: "RI1",
					referenceNumber2: "RI12",
					status: GetCodeDescriptionPair("DE", null),
					subType: GetCodeDescriptionPair("SI1", null),
					tariff: "TR4",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.Carton, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.Case, null),

					dateOfExpiry: new ZDate(2020, 04, 11),
					valueCurrency: new Currency { Code = Core.Constants.CurrencyCodes.Singapore, Description = "Singapore Dollar" },
					value: 1002.2m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);
				AssertContents(
					supportingInformationData: invoiceData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.PreviousDocument),
					type: GetCodeDescriptionPair("9012", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.Bahrain, "Bahrain"),
					customsOffice: GetCodeDescriptionPair("CI2", null),

					dateOfIssue: new ZDate(2017, 1, 2),
					description: "INV SUP DESC 2",
					lineNo: 2,
					procedure: GetCodeDescriptionPair("PI2", null),
					quantity: 4.3m,
					quantity2: 5.4m,

					referenceNumber: "RI2",
					referenceNumber2: "RI22",
					status: GetCodeDescriptionPair("EF", null),
					subType: GetCodeDescriptionPair("SI2", null),
					tariff: "TR5",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.Coil, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.Container, null),

					dateOfExpiry: ZDate.Empty,
					valueCurrency: new Currency { Code = Core.Constants.CurrencyCodes.Malawi, Description = "Malawian Kwacha" },
					value: 0m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);
				AssertContents(
					supportingInformationData: invoiceData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.SupportingDocument),
					type: GetCodeDescriptionPair("9012", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.Bangladesh, "Bangladesh"),
					customsOffice: GetCodeDescriptionPair("CI3", null),
					dateOfIssue: new ZDate(2017, 1, 3),

					description: "INV SUP DESC 3",
					lineNo: 3,
					procedure: GetCodeDescriptionPair("PI3", null),
					quantity: 6.5m,
					quantity2: 7.6m,

					referenceNumber: "RI3",
					referenceNumber2: "RI32",
					status: GetCodeDescriptionPair("FG", null),
					subType: GetCodeDescriptionPair("SI3", null),
					tariff: "TR6",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.Cradle, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.Crate, null),

					dateOfExpiry: ZDate.Empty,
					valueCurrency: new Currency { Code = Core.Constants.CurrencyCodes.Swaziland, Description = "Swazi Lilangeni" },
					value: 1m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);

				AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				AssertEquals("invoiceLineData.CustomsSupportingInformationCollection.Count", 3, invoiceLineData.CustomsSupportingInformationCollection.Count);
				AssertContents(
					supportingInformationData: invoiceLineData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, Common.EU.CusSupportingInfoTypeList.Descriptions.AdditionalInfo),
					type: GetCodeDescriptionPair("9021", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.Cambodia, "Cambodia"),
					customsOffice: GetCodeDescriptionPair("CL1", null),

					dateOfIssue: new ZDate(2017, 3, 1),
					description: "INVLINE SUP DESC 1",
					lineNo: 1,
					procedure: GetCodeDescriptionPair("PL1", null),
					quantity: 3.1m,
					quantity2: 4.2m,

					referenceNumber: "RL1",
					referenceNumber2: "RL12",
					status: GetCodeDescriptionPair("GH", null),
					subType: GetCodeDescriptionPair("SL1", null),
					tariff: "TR7",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.Dozen, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.Drum, null),

					dateOfExpiry: new ZDate(2020, 01, 01),
					valueCurrency: new Currency { Code = Core.Constants.CurrencyCodes.Switzerland, Description = "Swiss Franc" },
					value: 100.12m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);
				AssertContents(
					supportingInformationData: invoiceLineData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.PreviousDocument),
					type: GetCodeDescriptionPair("9022", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.Cameroon, "Cameroon"),
					customsOffice: GetCodeDescriptionPair("CL2", null),

					dateOfIssue: new ZDate(2017, 1, 2),
					description: "INVLINE SUP DESC 2",
					lineNo: 2,
					procedure: GetCodeDescriptionPair("PL2", null),
					quantity: 5.3m,
					quantity2: 6.4m,

					referenceNumber: "RL2",
					referenceNumber2: "RL22",
					status: GetCodeDescriptionPair("HI", null),
					subType: GetCodeDescriptionPair("SL2", null),
					tariff: "TR8",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.Envelope, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.Gross, null),

					dateOfExpiry: new ZDate(2020, 01, 02),
					valueCurrency: new Currency { Code = Core.Constants.CurrencyCodes.Italy, Description = "Euro" },
					value: 10000m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);
				AssertContents(
					supportingInformationData: invoiceLineData.CustomsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument),

					category: GetCodeDescriptionPair(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.SupportingDocument),
					type: GetCodeDescriptionPair("9022", null),
					country: GetCodeDescriptionPair(Core.Constants.CountryCodes.Canada, "Canada"),
					customsOffice: GetCodeDescriptionPair("CL3", null),
					dateOfIssue: new ZDate(2017, 1, 3),

					description: "INVLINE SUP DESC 3",
					lineNo: 3,
					procedure: GetCodeDescriptionPair("PL3", null),
					quantity: 7.5m,
					quantity2: 8.6m,

					referenceNumber: "RL3",
					referenceNumber2: "RL32",
					status: GetCodeDescriptionPair("IJ", null),
					subType: GetCodeDescriptionPair("SL3", null),
					tariff: "TR9",

					unitOfQuantity: GetCodeDescriptionPair(Core.Constants.PkgUnit.Keg, null),
					unitOfQuantity2: GetCodeDescriptionPair(Core.Constants.PkgUnit.Mix, null),

					dateOfExpiry: new ZDate(2020, 01, 03),
					valueCurrency: null,
					value: 10001m,

					additionalDescription: "Test Desc2",
					issuerType: GetCodeDescriptionPair("ISSUERTYPE", null),
					itemNumber: 1,
					packQty: 2,
					packType: GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null),
					quantity3: 3.3m,
					unitOfQuantity3: GetCodeDescriptionPair(Core.Constants.PkgUnit.Bottle, null)
				);
			}
		}

		void SetupCusSupportingInfo(
			CusSupportingInfo supportingInfoBO,

			ZString parentTableCode,
			ZGuid parentID,
			ZString? code = null,
			ZString? country = null,
			ZString? customsOffice = null,

			ZDate? dateOfIssue = null,
			ZString? description = null,
			ZShort? lineNo = null,
			ZString? procedure = null,
			ZDecimal? quantity = null,
			ZDecimal? quantity2 = null,
			ZDecimal? quantity3 = null,

			ZString? referenceNumber = null,
			ZString? referenceNumber2 = null,
			ZString? status = null,
			ZString? subType = null,

			ZString? tariff = null,
			ZString? unitOfQuantity = null,
			ZString? unitOfQuantity2 = null,
			ZString? unitOfQuantity3 = null,

			ZDate? dateOfExpiry = null,
			ZString? valueCurrency = null,
			ZDecimal? value = null,

			ZString? additionalDescription = null,
			ZString? issuerType = null,
			ZShort? itemNumber = null,
			ZInt? packQty = null,
			ZString? packType = null
		)
		{
			supportingInfoBO.CSI_ParentTableCode = parentTableCode;
			supportingInfoBO.CSI_ParentID = parentID;
			if (code.HasValue)
			{
				supportingInfoBO.CSI_Code = code.Value;
			}
			if (country.HasValue)
			{
				supportingInfoBO.CSI_RN_NKCountryCode = country.Value;
			}
			if (customsOffice.HasValue)
			{
				supportingInfoBO.CSI_CustomsOffice = customsOffice.Value;
			}
			if (dateOfIssue.HasValue)
			{
				supportingInfoBO.CSI_DateOfIssue = dateOfIssue.Value;
			}
			if (description.HasValue)
			{
				supportingInfoBO.CSI_Description = description.Value;
			}
			if (lineNo.HasValue)
			{
				supportingInfoBO.CSI_LineNo = lineNo.Value;
			}
			if (procedure.HasValue)
			{
				supportingInfoBO.CSI_Procedure = procedure.Value;
			}
			if (quantity.HasValue)
			{
				supportingInfoBO.CSI_Quantity = quantity.Value;
			}
			if (quantity2.HasValue)
			{
				supportingInfoBO.CSI_Quantity2 = quantity2.Value;
			}
			if (quantity3.HasValue)
			{
				supportingInfoBO.CSI_Quantity3 = quantity3.Value;
			}
			if (referenceNumber.HasValue)
			{
				supportingInfoBO.CSI_ReferenceNumber = referenceNumber.Value;
			}
			if (referenceNumber2.HasValue)
			{
				supportingInfoBO.CSI_ReferenceNumber2 = referenceNumber2.Value;
			}
			if (status.HasValue)
			{
				supportingInfoBO.CSI_Status = status.Value;
			}
			if (subType.HasValue)
			{
				supportingInfoBO.CSI_SubType = subType.Value;
			}
			if (tariff.HasValue)
			{
				supportingInfoBO.CSI_Tariff = tariff.Value;
			}
			if (unitOfQuantity.HasValue)
			{
				supportingInfoBO.CSI_UnitOfQuantity = unitOfQuantity.Value;
			}
			if (unitOfQuantity2.HasValue)
			{
				supportingInfoBO.CSI_UnitOfQuantity2 = unitOfQuantity2.Value;
			}
			if (unitOfQuantity3.HasValue)
			{
				supportingInfoBO.CSI_UnitOfQuantity3 = unitOfQuantity3.Value;
			}
			if (dateOfExpiry.HasValue)
			{
				supportingInfoBO.CSI_DateOfExpiry = dateOfExpiry.Value;
			}
			if (valueCurrency.HasValue)
			{
				supportingInfoBO.CSI_RX_NKCurrency = valueCurrency.Value;
			}
			if (value.HasValue)
			{
				supportingInfoBO.CSI_Value = value.Value;
			}
			if (additionalDescription.HasValue)
			{
				supportingInfoBO.CSI_AdditionalDescription = additionalDescription.Value;
			}
			if (issuerType.HasValue)
			{
				supportingInfoBO.CSI_IssuerType = issuerType.Value;
			}
			if (itemNumber.HasValue)
			{
				supportingInfoBO.CSI_ItemNumber = itemNumber.Value;
			}
			if (packQty.HasValue)
			{
				supportingInfoBO.CSI_PackQty = packQty.Value;
			}
			if (packType.HasValue)
			{
				supportingInfoBO.CSI_PackType = packType.Value;
			}
		}

		void AssertContents(UniversalCustoms.CustomsSupportingInformation supportingInformationData,

			ICodeDescription category,
			ICodeDescription type = null,
			ICodeDescription country = null,
			ICodeDescription customsOffice = null,
			ZDate? dateOfIssue = null,

			ZString? description = null,
			ZInt? lineNo = null,
			ZInt? itemNumber = null,
			ICodeDescription procedure = null,
			ZDecimal? quantity = null,
			ZDecimal? quantity2 = null,
			ZDecimal? quantity3 = null,

			ZString? referenceNumber = null,
			ZString? referenceNumber2 = null,
			ICodeDescription status = null,
			ICodeDescription subType = null,
			ZString? tariff = null,

			ICodeDescription unitOfQuantity = null,
			ICodeDescription unitOfQuantity2 = null,
			ICodeDescription unitOfQuantity3 = null,

			ZDate? dateOfExpiry = null,
			ICodeDescriptionDataObject valueCurrency = null,
			ZDecimal? value = null,

			ICodeDescription issuerType = null,
			ZString? additionalDescription = null,

			ZInt? packQty = null,
			ICodeDescription packType = null
		)
		{
			AssertNotNull("Precondition: supportingInformationData", supportingInformationData);
			CombineAssertions(delegate
			{
				AssertNotNull("supportingInformationData.Category", supportingInformationData.Category);
				AssertEquals("supportingInformationData.Category.Code", category.Code, supportingInformationData.Category.Code);
				AssertEquals("supportingInformationData.Category.Description", category.Description, supportingInformationData.Category.Description);
				if (type == null)
				{
					AssertNull("supportingInformationData.Type", supportingInformationData.Type);
				}
				else
				{
					AssertNotNull("supportingInformationData.Type", supportingInformationData.Type);
					AssertEquals("supportingInformationData.Type.Code", type.Code, supportingInformationData.Type.Code);
					AssertEquals("supportingInformationData.Type.Description", type.Description, supportingInformationData.Type.Description);
				}
				if (country == null)
				{
					AssertNull("supportingInformationData.Country", supportingInformationData.Country);
				}
				else
				{
					AssertNotNull("supportingInformationData.Country", supportingInformationData.Country);
					AssertEquals("supportingInformationData.Country.Code", country.Code, supportingInformationData.Country.Code);
					AssertEquals("supportingInformationData.Country.Name", country.Description, supportingInformationData.Country.Name);
				}
				if (customsOffice == null)
				{
					AssertNull("supportingInformationData.CustomsOffice", supportingInformationData.CustomsOffice);
				}
				else
				{
					AssertNotNull("supportingInformationData.CustomsOffice", supportingInformationData.CustomsOffice);
					AssertEquals("supportingInformationData.CustomsOffice.Code", customsOffice.Code, supportingInformationData.CustomsOffice.Code);
					AssertEquals("supportingInformationData.CustomsOffice.Description", customsOffice.Description, supportingInformationData.CustomsOffice.Description);
				}
				AssertEquals("supportingInformationData.DateOfIssue", dateOfIssue, supportingInformationData.DateOfIssue);
				AssertEquals("supportingInformationData.Description", description, supportingInformationData.Description);
				AssertEquals("supportingInformationData.LineNo", lineNo, supportingInformationData.LineNo);
				if (procedure == null)
				{
					AssertNull("supportingInformationData.Procedure", supportingInformationData.Procedure);
				}
				else
				{
					AssertNotNull("supportingInformationData.Procedure", supportingInformationData.Procedure);
					AssertEquals("supportingInformationData.Procedure.Code", procedure.Code, supportingInformationData.Procedure.Code);
					AssertEquals("supportingInformationData.Procedure.Description", procedure.Description, supportingInformationData.Procedure.Description);
				}
				AssertEquals("supportingInformationData.Quantity", quantity, supportingInformationData.Quantity);
				AssertEquals("supportingInformationData.Quantity2", quantity2, supportingInformationData.Quantity2);
				AssertEquals("supportingInformationData.Quantity3", quantity3, supportingInformationData.Quantity3);
				AssertEquals("supportingInformationData.ReferenceNumber", referenceNumber, supportingInformationData.ReferenceNumber);
				AssertEquals(
					"supportingInformationData.ReferenceNumber2",
					referenceNumber2,
					supportingInformationData.ReferenceNumberCollection.FirstOrDefault(
						number => number.Type.Code.ToString() == Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber
					)?.ReferenceNumber
				);
				if (status == null)
				{
					AssertNull("supportingInformationData.Status", supportingInformationData.Status);
				}
				else
				{
					AssertNotNull("supportingInformationData.Status", supportingInformationData.Status);
					AssertEquals("supportingInformationData.Status.Code", status.Code, supportingInformationData.Status.Code);
					AssertEquals("supportingInformationData.Status.Description", status.Description, supportingInformationData.Status.Description);
				}
				if (subType == null)
				{
					AssertNull("supportingInformationData.SubType", supportingInformationData.SubType);
				}
				else
				{
					AssertNotNull("supportingInformationData.SubType", supportingInformationData.SubType);
					AssertEquals("supportingInformationData.SubType.Code", subType.Code, supportingInformationData.SubType.Code);
					AssertEquals("supportingInformationData.SubType.Description", subType.Description, supportingInformationData.SubType.Description);
				}
				AssertEquals("supportingInformationData.Tariff", tariff, supportingInformationData.Tariff);
				if (unitOfQuantity == null)
				{
					AssertNull("supportingInformationData.UnitOfQuantity", supportingInformationData.UnitOfQuantity);
				}
				else
				{
					AssertNotNull("supportingInformationData.UnitOfQuantity", supportingInformationData.UnitOfQuantity);
					AssertEquals("supportingInformationData.UnitOfQuantity.Code", unitOfQuantity.Code, supportingInformationData.UnitOfQuantity.Code);
					AssertEquals("supportingInformationData.UnitOfQuantity.Description", unitOfQuantity.Description, supportingInformationData.UnitOfQuantity.Description);
				}
				if (unitOfQuantity2 == null)
				{
					AssertNull("supportingInformationData.UnitOfQuantity2", supportingInformationData.UnitOfQuantity2);
				}
				else
				{
					AssertNotNull("supportingInformationData.UnitOfQuantity2", supportingInformationData.UnitOfQuantity2);
					AssertEquals("supportingInformationData.UnitOfQuantity2.Code", unitOfQuantity2.Code, supportingInformationData.UnitOfQuantity2.Code);
					AssertEquals("supportingInformationData.UnitOfQuantity2.Description", unitOfQuantity2.Description, supportingInformationData.UnitOfQuantity2.Description);
				}
				if (unitOfQuantity3 == null)
				{
					AssertNull("supportingInformationData.UnitOfQuantity3", supportingInformationData.UnitOfQuantity3);
				}
				else
				{
					AssertNotNull("supportingInformationData.UnitOfQuantity3", supportingInformationData.UnitOfQuantity3);
					AssertEquals("supportingInformationData.UnitOfQuantity3.Code", unitOfQuantity3.Code, supportingInformationData.UnitOfQuantity3.Code);
					AssertEquals("supportingInformationData.UnitOfQuantity3.Description", unitOfQuantity3.Description, supportingInformationData.UnitOfQuantity3.Description);
				}
				AssertEquals("supportingInformationData.DateOfExpiry", dateOfExpiry, supportingInformationData.DateOfExpiry);
				if (valueCurrency == null)
				{
					AssertNull("supportingInformationData.ValueCurrency", supportingInformationData.ValueCurrency);
				}
				else
				{
					AssertNotNull("supportingInformationData.ValueCurrency", supportingInformationData.ValueCurrency);
					AssertEquals("supportingInformationData.ValueCurrency.Code", valueCurrency.Code, supportingInformationData.ValueCurrency.Code);
					AssertEquals("supportingInformationData.ValueCurrency.Description", valueCurrency.Description, supportingInformationData.ValueCurrency.Description);
				}
				AssertEquals("supportingInformationData.Value", value, supportingInformationData.Value);
				AssertEquals("supportingInformationData.additionalDescription", additionalDescription, supportingInformationData.AdditionalDescription);
				if (issuerType == null)
				{
					AssertNull("supportingInformationData.issuerType", supportingInformationData.IssuerType);
				}
				else
				{
					AssertNotNull("supportingInformationData.IssuerType", supportingInformationData.IssuerType);
					AssertEquals("supportingInformationData.IssuerType.Code", issuerType.Code, supportingInformationData.IssuerType.Code);
					AssertEquals("supportingInformationData.IssuerType.Description", issuerType.Description, supportingInformationData.IssuerType.Description);
				}
				AssertEquals("supportingInformationData.ItemNumber", itemNumber, supportingInformationData.ItemNumber);
				AssertEquals("supportingInformationData.PackQuantity", packQty, supportingInformationData.PackQuantity);
				if (packType == null)
				{
					AssertNull("supportingInformationData.PackUnitOfQuantity", supportingInformationData.PackUnitOfQuantity);
				}
				else
				{
					AssertNotNull("supportingInformationData.PackUnitOfQuantity", supportingInformationData.PackUnitOfQuantity);
					AssertEquals("supportingInformationData.PackUnitOfQuantity.Code", packType.Code, supportingInformationData.PackUnitOfQuantity.Code);
					AssertEquals("supportingInformationData.PackUnitOfQuantity.Description", packType.Description, supportingInformationData.PackUnitOfQuantity.Description);
				}
			});
		}
	}
}
