using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest
	{
		public void TestPopulateNoteFieldsFromAddInfoCollection()
		{
			var declarationDataObject = SetupDeclaration(ZString.Empty, ZString.Empty);
			declarationDataObject.CommercialInfo = SetupCommercialInfoWithInvoiceLine();
			var invoiceLineDataObject = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			invoiceLineDataObject.AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = PredefinedNoteTypes.Instance.SGCertificateItemDesc.ToString(), Value = "TEST CERTIFICATE ITEM DESCRIPTION" },
					new AddInfo() { Key = PredefinedNoteTypes.Instance.SGMarksAndNumbers.ToString(), Value = "TEST MARKS AND NUMBERS" }
				};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var invoices = declarationBO.Invoices;
				AssertEquals(1, invoices.Count);
				var invoiceLines = invoices.First().InvoiceLines;
				AssertEquals(1, invoiceLines.Count);

				var invoiceLine = (JobComInvoiceLine)invoiceLines.First();
				AssertEquals("Certificate Item Description", "TEST CERTIFICATE ITEM DESCRIPTION", invoiceLine.CertItemDescription);
				AssertEquals("Marks and Numbers", "TEST MARKS AND NUMBERS", invoiceLine.MarksAndNumbers);
			});
		}

		public void TestPopulateFieldsAdditionalLineTariffDetailCollection()
		{
			var declarationDataObject = SetupDeclaration(ZString.Empty, ZString.Empty);
			declarationDataObject.CommercialInfo = SetupCommercialInfoWithInvoiceLine();
			var invoiceLineDataObject = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			invoiceLineDataObject.AdditionalLineTariffDetailCollection = new List<AdditionalLineTariffDetail>()
			{
				new AdditionalLineTariffDetail() { Tariff = "ZBP0BA0QVDG", CustomsQuantity = 34m, CustomsQuantityUnit = new CodeDescriptionPair() { Code = UnitOfQuantityCodeList.Codes.DRM } }
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var invoiceLine = (JobComInvoiceLine)declarationBO.Invoices.First().InvoiceLines.First();
				AssertEquals(1, invoiceLine.ProductCodes.Count);
				var productCode = invoiceLine.ProductCodes[0];
				AssertEquals("Product Code", "ZBP0BA0QVDG", productCode.BZ_Tariff);
				AssertEquals("Quantity", 34m, productCode.BZ_Qty1);
				AssertEquals("Unit Of Quantity", UnitOfQuantityCodeList.Codes.DRM, productCode.BZ_UQ1);
			});
		}

		public void TestAllAddInfoFieldsPopulate()
		{
			var declarationDataObject = SetupDeclaration(ZString.Empty, ZString.Empty);
			declarationDataObject.CommercialInfo = SetupCommercialInfoWithInvoiceLine();
			var invoiceLineDataObject = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			invoiceLineDataObject.AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_InwardHAWB), Value = "HB4029" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_InwardMAWB), Value = "61800429487" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardHAWB), Value = "G1772" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardMAWB), Value = "08100239287" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OuterPackQuantity), Value = "250" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OuterPackQuantityUnit), Value = "BOX" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_EndUseDescription), Value = "FOR HUMANITARIAN AID" },
					new AddInfo() { Key = PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description, Value = "Invoice Marks and Numbers" },
					new AddInfo() { Key = PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description, Value = "I/L Cert. Item Desc." },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTY), Value = "100.00" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTYUnit), Value = "PKT" }
				};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var invoiceLine = (JobComInvoiceLine)declarationBO.Invoices.First().InvoiceLines.First();
			AssertEquals("SG_InwardHAWB", "HB4029", invoiceLine.SG_InwardHAWB);
			AssertEquals("SG_InwardMAWB", "61800429487", invoiceLine.SG_InwardMAWB);
			AssertEquals("SG_OutwardHAWB", "G1772", invoiceLine.SG_OutwardHAWB);
			AssertEquals("SG_OutwardMAWB", "08100239287", invoiceLine.SG_OutwardMAWB);
			AssertEquals("SG_OuterPackQuantity", 250, invoiceLine.SG_OuterPackQuantity);
			AssertEquals("SG_OuterPackQuantityUnit", "BOX", invoiceLine.SG_OuterPackQuantityUnit);
			AssertEquals("SG_EndUseDescription", "FOR HUMANITARIAN AID", invoiceLine.SG_EndUseDescription);
			AssertEquals("Invoice Marks and Numbers", invoiceLine.MarksAndNumbers);
			AssertEquals("I/L Cert. Item Desc.", invoiceLine.CertItemDescription);
			AssertEquals("SG_TotalDutiableWGTVOLQTY", 100.00m, invoiceLine.SG_TotalDutiableWGTVOLQTY);
			AssertEquals("SG_TotalDutiableWGTVOLQTYUnit", "PKT", invoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
		}

		public void TestNumericAddInfoFieldsPopulate()
		{
			var declarationDataObject = SetupDeclaration(ZString.Empty, ZString.Empty);
			declarationDataObject.CommercialInfo = SetupCommercialInfoWithInvoiceLine();
			var invoiceLineDataObject = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			invoiceLineDataObject.AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_UnitDutiableWGTVOLQTY), Value = "93.95" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_UnitDutiableWGTVOLQTYUnit), Value = "STK" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTY), Value = "100.00" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTYUnit), Value = "PKT" }
				};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var invoiceLine = (JobComInvoiceLine)declarationBO.Invoices.First().InvoiceLines.First();
			AssertEquals("SG_UnitDutiableWGTVOLQTY", 93.95m, invoiceLine.SG_UnitDutiableWGTVOLQTY);
			AssertEquals("SG_UnitDutiableWGTVOLQTYUnit", "STK", invoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
			AssertEquals("SG_TotalDutiableWGTVOLQTY", 100.00m, invoiceLine.SG_TotalDutiableWGTVOLQTY);
			AssertEquals("SG_TotalDutiableWGTVOLQTYUnit", "PKT", invoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
		}

		public void TestPopulateFieldsForOVR()
		{
			var declarationDataObject = SetupDeclaration("INP", "APS");

			var declarationAddInfoCollection = new List<AddInfo>()
			{
				new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_US_NKPlaceOfReceipt), Value = "OVR" },
				new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_US_NKPlaceOfCargoRelease), Value = "OVR" }
			};
			declarationDataObject.SetAddInfoCollection(() => declarationAddInfoCollection);
			declarationDataObject.CommercialInfo = SetupCommercialInfoWithInvoiceLine();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("JE_MessageType", "INP", declarationBO.JE_MessageType);
				AssertEquals("JE_MessageSubType", "APS", declarationBO.JE_MessageSubType);
				AssertEquals("SG_US_NKPlaceOfReceipt", "OVR", declarationBO.SG_US_NKPlaceOfReceipt);
				AssertEquals("SG_US_NKPlaceOfCargoRelease", "OVR", declarationBO.SG_US_NKPlaceOfCargoRelease);

				var invoice1Line1 = (JobComInvoiceLine)declarationBO.Invoices[0].InvoiceLines[0];
				AssertEquals("ProductCodes", 0, invoice1Line1.ProductCodes.Count);
				AssertEquals("CASCCode1s", 0, invoice1Line1.CASCCode1s.Count);
			});

			reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				var invoice1Line1 = (JobComInvoiceLine)declarationBO.Invoices[0].InvoiceLines[0];
				AssertEquals("ProductCodes", 0, invoice1Line1.ProductCodes.Count);
				AssertEquals("CASCCode1s", 0, invoice1Line1.CASCCode1s.Count);
			});

			var invoiceDataObject = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0];
			var invoiceLineDataObject = invoiceDataObject.CommercialInvoiceLineCollection[0];

			invoiceLineDataObject.AdditionalLineTariffDetailCollection = new List<AdditionalLineTariffDetail>()
			{
				new AdditionalLineTariffDetail() { Type = new CodeDescriptionPair5Char() { Code = "" }, Tariff = "OVR", Value = 0m }
			};

			invoiceLineDataObject.CustomsReferenceCollection = new List<CustomsReference>()
			{
				new CustomsReference
				{
					Type = new CodeDescriptionPair() { Code = Common.SG.CusCodeDataTypeList.Codes.CASCode1 },
					Reference = "gstn001"
				}
			};

			reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				var invoice1Line1 = (JobComInvoiceLine)declarationBO.Invoices[0].InvoiceLines[0];
				AssertEquals("ProductCodes", 1, invoice1Line1.ProductCodes.Count);
				var productCode = invoice1Line1.ProductCodes[0];
				AssertEquals("Product Code", "OVR", productCode.BZ_Tariff);

				AssertEquals("CASCCode1s", 1, invoice1Line1.CASCCode1s.Count);
				var cascCode1 = invoice1Line1.CASCCode1s[0];
				AssertEquals("CASC Code", "gstn001", cascCode1.CY_Data);
			});
		}

		#region Implementation

		Shipment SetupDeclaration(ZString messageType, ZString messageSubType)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			return new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair() { Code = messageType },
				MessageSubType = new CodeDescriptionPair() { Code = messageSubType }
			};
		}

		CommercialInfo SetupCommercialInfoWithInvoiceLine()
		{
			return new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
						{
							new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
							}
						}))
				}
			};
		}

		#endregion
	}
}
