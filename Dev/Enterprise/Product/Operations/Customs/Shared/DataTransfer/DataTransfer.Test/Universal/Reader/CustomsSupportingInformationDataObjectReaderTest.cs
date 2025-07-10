using System.Collections.Generic;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestCusSupportingInfoMapping_PackQty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var supportingDocumentDataObject = new UniversalCustoms.CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
					PackQuantity = 154,
				};
				var reader = new CustomsSupportingInformationDataObjectReader(supportingDocumentDataObject, logger, Factory, invoiceLine.PK, invoiceLine.TablePrefix);
				var doc = reader.ReadIntoDataRow();
				AssertEquals("CusSupportingInfo.CSI_PackQty", new ZShort(154), doc.GetValue(CusSupportingInfoSchema.CSI_PackQty));
			}
		}

		public void TestCusSupportingInfoMapping_PackUnitOfQuantity()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var supportingDocumentDataObject = new UniversalCustoms.CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
					PackUnitOfQuantity = new CodeDescriptionPair { Code = "UP1" },
				};
				var reader = new CustomsSupportingInformationDataObjectReader(supportingDocumentDataObject, logger, Factory, invoiceLine.PK, invoiceLine.TablePrefix);
				var doc = reader.ReadIntoDataRow();
				AssertEquals("CusSupportingInfo.CSI_PackType", "UP1", doc.GetValue(CusSupportingInfoSchema.CSI_PackType));
			}
		}

		public void TestCusSupportingInfoMapping_AdditionalDescription()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var supportingDocumentDataObject = new UniversalCustoms.CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
					AdditionalDescription = "ADD DESC 1",
				};
				var reader = new CustomsSupportingInformationDataObjectReader(supportingDocumentDataObject, logger, Factory, invoiceLine.PK, invoiceLine.TablePrefix);
				var doc = reader.ReadIntoDataRow();
				AssertEquals("CusSupportingInfo.CSI_AdditionalDescription", "ADD DESC 1", doc.GetValue(CusSupportingInfoSchema.CSI_AdditionalDescription));
			}
		}

		public void TestBasicCusSupportingInfoLevelFieldMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.GB.IJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var supportingDocumentDataObject = new UniversalCustoms.CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
					Type = new CodeDescriptionPair6Char() { Code = "CK3" },
					Country = new Country() { Code = Core.Constants.CountryCodes.Singapore },
					CustomsOffice = new CodeDescriptionPair10Char() { Code = "COF1" },
					DateOfIssue = new ZDate(2017, 11, 2),
					Description = "BOB THE BUILDER",
					LineNo = 3,
					Procedure = new CodeDescriptionPair7Char() { Code = "PR3" },
					Quantity = 150m,
					Quantity2 = 300m,
					Quantity3 = 400m,
					ReferenceNumber = "REF3232",
					ReferenceNumberCollection = new List<Reference>
					{
						new Reference
						{
							Type = new EntryType
							{
								Code = Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber,
								Description = Constants.ReferenceNumberTypes.Descriptions.LocalReferenceNumber
							},
							ReferenceNumber = "REF3238"
						}
					},
					ItemNumber = 1,
					Status = new CodeDescriptionPair() { Code = "ST1" },
					Tariff = "T001",
					SubType = new CodeDescriptionPair5Char() { Code = "ST32" },
					UnitOfQuantity = new CodeDescriptionPair4Char() { Code = "UOQ1" },
					UnitOfQuantity2 = new CodeDescriptionPair4Char() { Code = "UOQ2" },
					UnitOfQuantity3 = new CodeDescriptionPair4Char() { Code = "UOQ3" },
					DateOfExpiry = new ZDate(2020, 04, 08),
					ValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.EuropeanUnion, Description = "Euro" },
					Value = 999m,
					AdditionalDescription = "Test Desc2",
					IssuerType = new CodeDescriptionPair10Char() { Code = "ISSUERTYPE" },
					PackQuantity = 200,
					PackUnitOfQuantity = new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Package },
				};
				var previousDocumentDataObject = new UniversalCustoms.CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument },
					Type = new CodeDescriptionPair6Char() { Code = "CK4" },
					Country = new Country() { Code = Core.Constants.CountryCodes.NewZealand },
					CustomsOffice = new CodeDescriptionPair10Char() { Code = "COF2" },
					DateOfIssue = new ZDate(2017, 11, 4),
					Description = "WENDT THE DESTROYER",
					LineNo = 1,
					Procedure = new CodeDescriptionPair7Char() { Code = "PR2" },
					Quantity = 420m,
					Quantity2 = 600m,
					ReferenceNumber = "REF8734",
					ReferenceNumberCollection = new List<Reference>
					{
						new Reference
						{
							Type = new EntryType
							{
								Code = Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber,
								Description = Constants.ReferenceNumberTypes.Descriptions.LocalReferenceNumber
							},
							ReferenceNumber = "REF8736"
						}
					},
					ItemNumber = 2,
					Status = new CodeDescriptionPair() { Code = "ST2" },
					SubType = new CodeDescriptionPair5Char() { Code = "ST65" },
					Tariff = "T002",
					UnitOfQuantity = new CodeDescriptionPair4Char() { Code = "UOQ4" },
					UnitOfQuantity2 = new CodeDescriptionPair4Char() { Code = "UOQ6" }
				};
				var invoiceLineDataObject = new UniversalCustoms.CommercialInvoiceLine()
				{
					CustomsSupportingInformationCollection = new List<UniversalCustoms.CustomsSupportingInformation>(new[] { supportingDocumentDataObject, previousDocumentDataObject })
				};
				var cusSupportingInfoBOs = new CustomsSupportingInformationCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom)).ReadIntoDataRows(invoiceLine.PK, invoiceLine.TablePrefix, invoiceLine.IsInDatabase, invoiceLineDataObject);

				AssertNotNull(cusSupportingInfoBOs);

				CombineAssertions(delegate
				{
					AssertEquals("CusSupportingInfo", 2, cusSupportingInfoBOs.Length);
					var supportingDocumentBO = cusSupportingInfoBOs[0];
					var previousDocumentBO = cusSupportingInfoBOs[1];
					if (Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument.Equals(previousDocumentBO[CusSupportingInfoSchema.Constants.CSI_Type]))
					{
						supportingDocumentBO = cusSupportingInfoBOs[1];
						previousDocumentBO = cusSupportingInfoBOs[0];
					}
					AssertCusSupportingInfoContents(
						cusSupportingInfoBO: supportingDocumentBO,

						parentTableCode: invoiceLine.TablePrefix,
						parentID: invoiceLine.PK,
						type: Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument,
						code: "CK3",
						country: Core.Constants.CountryCodes.Singapore,

						customsOffice: "COF1",
						dateOfIssue: new ZDate(2017, 11, 2),
						description: "BOB THE BUILDER",
						lineNo: 3,
						procedure: "PR3",

						quantity: 150m,
						quantity2: 300m,
						quantity3: 400m,
						referenceNumber: "REF3232",
						referenceNumber2: "REF3238",
						itemNumber: 1,
						status: "ST1",
						subType: "ST32",

						tariff: "T001",
						unitOfQuantity: "UOQ1",
						unitOfQuantity2: "UOQ2",
						unitOfQuantity3: "UOQ3",

						dateOfExpiry: new ZDate(2020, 04, 08),
						valueCurrency: "EUR",
						value: 999m,

						additionalDescription: "Test Desc2",
						issuerType: "ISSUERTYPE",
						packQty: 200,
						packType: "PKG"
					);

					AssertCusSupportingInfoContents(
						cusSupportingInfoBO: previousDocumentBO,

						parentTableCode: invoiceLine.TablePrefix,
						parentID: invoiceLine.PK,
						type: Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument,
						code: "CK4",
						country: Core.Constants.CountryCodes.NewZealand,

						customsOffice: "COF2",
						dateOfIssue: new ZDate(2017, 11, 4),
						description: "WENDT THE DESTROYER",
						lineNo: 1,
						procedure: "PR2",

						quantity: 420m,
						quantity2: 600m,
						quantity3: 0m,
						referenceNumber: "REF8734",
						referenceNumber2: "REF8736",
						itemNumber: 2,
						status: "ST2",
						subType: "ST65",

						tariff: "T002",
						unitOfQuantity: "UOQ4",
						unitOfQuantity2: "UOQ6",
						unitOfQuantity3: ZString.Empty,

						dateOfExpiry: ZDate.Empty,
						valueCurrency: ZString.Empty,
						value: 0m,

						additionalDescription: ZString.Empty,
						issuerType: ZString.Empty,
						packQty: 0,
						packType: ZString.Empty
					);
				});
			}
		}
	}
}
