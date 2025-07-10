using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FlattenEntryLineAndBillForImmediateDelivery))]
	sealed class FlattenEntryLineAndBillForImmediateDeliveryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestItBlAwbCode()
		{
			DocEntryLine.ItBlAwbCode = "O";
			AssertEquals("O", DocEntryLine.ItBlAwbCode);
		}

		public void TestItBlAwbNumber()
		{
			DocEntryLine.ItBlAwbNumber = "ADRE3457869";
			AssertEquals("ADRE3457869", DocEntryLine.ItBlAwbNumber);
		}

		public void TestPortOfLading()
		{
			DocEntryLine.PortOfLading = "GENOA";
			AssertEquals("GENOA", DocEntryLine.PortOfLading);
		}

		public void TestManifestQuantityAndUQ()
		{
			DocEntryLine.ManifestQuantity = 2564;
			AssertEquals("2564 ", DocEntryLine.ManifestQuantityAndUQ);
			DocEntryLine.ManifestQuantity = 2564.123456;
			AssertEquals("2564.12346 ", DocEntryLine.ManifestQuantityAndUQ);
		}

		public void TestManifestQuantity()
		{
			DocEntryLine.ManifestQuantity = 2564;
			AssertEquals((ZDecimal)2564, DocEntryLine.ManifestQuantity);
			DocEntryLine.ManifestQuantityUQ = ABIUnitOfMeasureList.Codes.Kilograms;
			AssertEquals(ABIUnitOfMeasureList.Codes.Kilograms, DocEntryLine.ManifestQuantityUQ);
		}

		public void TestTariff()
		{
			AssertEquals("8471704065", DocEntryLine.Tariff);
		}

		public void TestCountryOfOrigin()
		{
			AssertEquals("AD", DocEntryLine.CountryOfOrigin);
		}

		public void TestManufacturerID()
		{
			AssertEquals("XYBEREQU6LON", DocEntryLine.ManufacturerID);
		}

		protected override BusinessObject GetNewBusinessObject() => new FlattenEntryLineAndBillForImmediateDelivery(ZString.Empty, ZString.Empty, ZString.Empty);

		FlattenEntryLineAndBillForImmediateDelivery docEntryLine;
		FlattenEntryLineAndBillForImmediateDelivery DocEntryLine
		{
			get
			{
				if (docEntryLine == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.US_EnableCRL = true;
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "8471704065";
					invoiceLine.JI_CountryOfOrigin = "AD";
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
					CusEntryLine entryLine = invoiceLine.CusEntryLine;
					OrgHeader manufacturer = Factory.New<OrgHeader>();
					invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
					manufacturer.OH_FullName = "Manufacturer";
					manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
					OrgContact contact = manufacturer.Contacts.AddNew();
					contact.OC_ContactName = "Walter Doodleberry";
					contact.OC_Phone = "(847) 364 5600";
					OrgCusCode manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
					docEntryLine = new FlattenEntryLineAndBillForImmediateDelivery(entryLine.ImportTariff.UE_Tariff, entryLine.CountryOfOrigin.Code, manufacturer.CustomsCodes[0].OK_CustomsRegNo);
				}

				return docEntryLine;
			}
		}
	}
}
