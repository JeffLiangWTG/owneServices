using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USInvoiceLineFSISLineCollection))]
	class USInvoiceLineFSISLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDetaultNewElement()
		{
			var refCusCode = Factory.New<ZZRefCusCodeListCombined>();
			refCusCode.ZZD_Code = "1P222";

			var date = new ZDateTime(2019, 3, 25);
			InvLine.Declaration.US_InspecDate = date;
			InvLine.Declaration.US_FSISInspec = "1P222";
			InvLine.JI_Description = "GOODS DESCRIPTION";
			InvLine.FSISLines.RemoveAndDeleteAll();
			var fsisLine = InvLine.FSISLines.AddNew();
			AssertEquals("GOODS DESCRIPTION", fsisLine.US_CommercialDescription);
			AssertEquals(date, fsisLine.US_DateOfInspection);
			AssertEquals("1P222", fsisLine.US_ImportingEstNo);

			fsisLine.US_CommercialDescription = "MORE DETAILED GOODS DESCRIPTION";
			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Mexico;
			fsisLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			fsisLine.US_ExportingEstNo = "555A";
			fsisLine.US_ProductID = "AI";
			fsisLine.US_IntendedUseCode = "200";

			AssertEquals("MORE DETAILED GOODS DESCRIPTION", fsisLine.US_CommercialDescription);
			AssertEquals(Core.Constants.CountryCodes.Mexico, fsisLine.US_UC_NKCertificateIssuerCountry);
			AssertEquals(Core.Constants.CountryCodes.Australia, fsisLine.US_UC_NKCountryOfOrigin);
			AssertEquals("555A", fsisLine.US_ExportingEstNo);
			AssertEquals("AI", fsisLine.US_ProductID);
			AssertEquals("200", fsisLine.US_IntendedUseCode);
			AssertEquals("1P222", fsisLine.US_ImportingEstNo);

			AssertEquals(1, InvLine.FSISLines.Count);
			AssertEquals(PartyTypeList.Codes.Importer, fsisLine.US_CertifyingIndividual);
			fsisLine.US_CertifyingIndividual = EntityRoleCodeList.Codes.CustomsBroker;
			fsisLine.US_PGAContactName = "KNZ TST";
			fsisLine.US_PGAContactPhoneNo = "+86120141414";
			fsisLine.US_PGAContactEmail = "KNZ@TT.COM";

			var fsisLine2 = InvLine.FSISLines.AddNew();
			AssertEquals("KNZ TST", fsisLine2.US_PGAContactName);
			AssertEquals("+86120141414", fsisLine2.US_PGAContactPhoneNo);
			AssertEquals("KNZ@TT.COM", fsisLine2.US_PGAContactEmail);
			AssertEquals(EntityRoleCodeList.Codes.CustomsBroker, fsisLine2.US_CertifyingIndividual);
		}

		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.FSISLines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.FSISLines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return InvLine.FSISLines;
		}

		JobComInvoiceLine InvLine
		{
			get { return fInvLine ?? (fInvLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew()); }
		}
		JobComInvoiceLine fInvLine;
	}
}
