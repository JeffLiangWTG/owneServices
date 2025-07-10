using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest
	{
		public void TestPopulateAddInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			invoiceLine.CertItemDescription = "TEST CERTIFICATE ITEM DESCRIPTION";
			invoiceLine.MarksAndNumbers = "TEST MARKS AND NUMBERS";
			Factory.SaveForTesting();
			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				var invoices = declarationData.CommercialInfo.CommercialInvoiceCollection;
				AssertEquals(1, invoices.Count);
				var invoiceLineData = invoices.First().CommercialInvoiceLineCollection.First();

				AssertEquals("Certificate Item Description", "TEST CERTIFICATE ITEM DESCRIPTION", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description).Value);
				AssertEquals("Marks and Numbers", "TEST MARKS AND NUMBERS", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description).Value);
			});
		}

		public void TestEmptyValuesDoNotPopulateAddInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			invoiceLine.JI_BrandName = ZString.Empty;
			invoiceLine.JI_Model = ZString.Empty;
			invoiceLine.CertItemDescription = ZString.Empty;
			invoiceLine.MarksAndNumbers = ZString.Empty;
			Factory.SaveForTesting();
			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				var invoices = declarationData.CommercialInfo.CommercialInvoiceCollection;
				AssertEquals(1, invoices.Count);
				var invoiceLineData = invoices.First().CommercialInvoiceLineCollection.First();
				AssertNull("Certificate Item Description", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description));
				AssertNull("Marks and Numbers", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description));
			});
		}

		public void TestPopulateAdditionalLineTariffDetailCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var product = invoiceLine.ProductCodes.AddNew();
			product.BZ_Tariff = "ZBP0BA0QVDG";
			product.BZ_Qty1 = 12m;
			product.BZ_UQ1 = UnitOfQuantityCodeList.Codes.LTR;
			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var additionalLineTariffDetails = declarationData.CommercialInfo.CommercialInvoiceCollection.First().CommercialInvoiceLineCollection.First().AdditionalLineTariffDetailCollection;
			AssertNotNull(additionalLineTariffDetails);
			AssertEquals(1, additionalLineTariffDetails.Count);
			var additionalLineTariffDetail = additionalLineTariffDetails[0];
			CombineAssertions(() =>
			{
				AssertEquals("Tariff", "ZBP0BA0QVDG", additionalLineTariffDetail.Tariff.Value);
				AssertEquals("Quantity", 12m, additionalLineTariffDetail.CustomsQuantity.Value);
				AssertEquals("Unit Of Quantity", UnitOfQuantityCodeList.Codes.LTR, additionalLineTariffDetail.CustomsQuantityUnit.Code);
				Assert("Value", additionalLineTariffDetail.Value.Value.IsEmpty);
			});
		}

		public void TestMarksAndNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			invoiceLine.MarksAndNumbers = "Invoice Marks and Numbers";
			invoiceLine.CertItemDescription = "I/L Cert. Item Desc.";
			Factory.SaveForTesting();
			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			var invoices = declarationData.CommercialInfo.CommercialInvoiceCollection;
			AssertEquals(1, invoices.Count);
			var invoiceLineData = invoices.First().CommercialInvoiceLineCollection.First();

			AssertEquals("Marks and Numbers is output in file", "Invoice Marks and Numbers", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description).Value);
			AssertEquals("Certificate Item Description is output in file", "I/L Cert. Item Desc.", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description).Value);
		}

		public void TestAddInfoData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.MarksAndNumbers = "SG Marks";
			invoiceLine.CertItemDescription = "SG Cert Item Desc";
			invoiceLine.SG_LotNo = "P17";
			invoiceLine.SG_PreviousLotNo = "K110";
			invoiceLine.SG_InwardHAWB = "HB4029";
			invoiceLine.SG_InwardMAWB = "61800429487";
			invoiceLine.SG_OutwardMAWB = "08100239287";
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 840m;
			invoiceLine.SG_TotalDutiableWGTVOLQTYUnit = "BOX";

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var additionalInfoDetails = declarationData.CommercialInfo.CommercialInvoiceCollection.First().CommercialInvoiceLineCollection.First().AddInfoCollection;
			AssertNotNull(additionalInfoDetails);

			bool hasLotNo = false;
			bool hasPreviousLotNo = false;
			bool hasInwardHB = false;
			bool hasInwardMawb = false;
			bool hasOutwardMawb = false;
			bool hasMarks = false;
			bool hasCertItem = false;
			bool hasTotalDutiableWGTVOLQTY = false;
			bool hasTotalDutiableWGTVOLQTYUnit = false;
			foreach (var addInfo in additionalInfoDetails)
			{
				if (addInfo.Key.ToString() == "SG50")
				{
					hasLotNo = addInfo.Value.ToString() == "P17";
				}

				if (addInfo.Key.ToString() == "SG70")
				{
					hasPreviousLotNo = addInfo.Value.ToString() == "K110";
				}

				if (addInfo.Key.ToString() == "SG47")
				{
					hasInwardHB = addInfo.Value.ToString() == "HB4029";
				}

				if (addInfo.Key.ToString() == "SG48")
				{
					hasInwardMawb = addInfo.Value.ToString() == "61800429487";
				}

				if (addInfo.Key.ToString() == "SG63")
				{
					hasOutwardMawb = addInfo.Value.ToString() == "08100239287";
				}

				if (addInfo.Key.ToString() == PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description)
				{
					hasMarks = addInfo.Value.ToString() == "SG Marks";
				}

				if (addInfo.Key.ToString() == PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description)
				{
					hasCertItem = addInfo.Value.ToString() == "SG Cert Item Desc";
				}

				if (addInfo.Key.ToString() == "SG89")
				{
					hasTotalDutiableWGTVOLQTY = addInfo.Value.ToString() == "840";
				}

				if (addInfo.Key.ToString() == "SG90")
				{
					hasTotalDutiableWGTVOLQTYUnit = addInfo.Value.ToString() == "BOX";
				}
			}

			Assert("Add info for SG_LotNo is being populated", hasLotNo);
			Assert("Add info for SG_PreviousLotNo is being populated", hasPreviousLotNo);
			Assert("Add info for SG_InwardHAWB is being populated", hasInwardHB);
			Assert("Add info for SG_InwardMAWB is being populated", hasInwardMawb);
			Assert("Add info for SG_OutwardMAWB is being populated", hasOutwardMawb);
			Assert("Marks and Numbers is being populated", hasMarks);
			Assert("Cert Item Description is being populated", hasCertItem);
			Assert("Add info for SG_TotalDutiableWGTVOLQTY is being populated", hasTotalDutiableWGTVOLQTY);
			Assert("Add info for SG_TotalDutiableWGTVOLQTYUnit is being populated", hasTotalDutiableWGTVOLQTYUnit);
		}

		public void TestAddInfoNumericData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SG_UnitDutiableWGTVOLQTY = 590.75m;
			invoiceLine.SG_UnitDutiableWGTVOLQTYUnit = "STK";
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 840m;
			invoiceLine.SG_TotalDutiableWGTVOLQTYUnit = "BOX";

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var additionalInfoDetails = declarationData.CommercialInfo.CommercialInvoiceCollection.First().CommercialInvoiceLineCollection.First().AddInfoCollection;
			AssertNotNull(additionalInfoDetails);

			bool hasSG_UnitDutiableWGTVOLQTY = false;
			bool hasSG_UnitDutiableWGTVOLQTYUnit = false;
			bool hasTotalDutiableWGTVOLQTY = false;
			bool hasTotalDutiableWGTVOLQTYUnit = false;
			foreach (var addInfo in additionalInfoDetails)
			{
				if (addInfo.Key.ToString() == "SG92")
				{
					hasSG_UnitDutiableWGTVOLQTY = addInfo.Value.ToString() == "590.75";
				}

				if (addInfo.Key.ToString() == "SG93")
				{
					hasSG_UnitDutiableWGTVOLQTYUnit = addInfo.Value.ToString() == "STK";
				}

				if (addInfo.Key.ToString() == "SG89")
				{
					hasTotalDutiableWGTVOLQTY = addInfo.Value.ToString() == "840";
				}

				if (addInfo.Key.ToString() == "SG90")
				{
					hasTotalDutiableWGTVOLQTYUnit = addInfo.Value.ToString() == "BOX";
				}
			}

			Assert("Add info for SG_UnitDutiableWGTVOLQTY is being populated", hasSG_UnitDutiableWGTVOLQTY);
			Assert("Add info for SG_UnitDutiableWGTVOLQTYUnit is being populated", hasSG_UnitDutiableWGTVOLQTYUnit);
			Assert("Add info for SG_TotalDutiableWGTVOLQTY is being populated", hasTotalDutiableWGTVOLQTY);
			Assert("Add info for SG_TotalDutiableWGTVOLQTYUnit is being populated", hasTotalDutiableWGTVOLQTYUnit);
		}
	}
}
