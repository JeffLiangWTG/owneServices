using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USDeclarationFSISLine))]
	class USDeclarationFSISLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<USDeclarationFSISLine>
	{
		public void TestUS_CertifyingIndividual()
		{
			var iorOrgHeader = Factory.New<OrgHeader>();
			iorOrgHeader.OH_Code = "TESTIOR";

			DeclarationTestHelper.AddPGAContact(iorOrgHeader, "FIRST", "LAST", "123456", "ior@ian.com", null);

			var declaration = Declaration;
			declaration.IOROrgPK = iorOrgHeader.PK;

			var ownerOrgHeader = Factory.New<OrgHeader>();
			ownerOrgHeader.OH_Code = "TESTOWN";
			DeclarationTestHelper.AddPGAContact(ownerOrgHeader, "OWNER", "TEST", "234567", "owner@ian.com", null);
			var fsisLine = Declaration.FSISLines.AddNew();
			fsisLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("FIRST LAST", fsisLine.US_PGAContactName);
			AssertEquals("123456", fsisLine.US_PGAContactPhoneNo);
			AssertEquals("ior@ian.com", fsisLine.US_PGAContactEmail);
		}

		public void TestUpdateValueOnInvoiceLineIfRequired()
		{
			var fsisLine = Declaration.FSISLines.AddNew();
			fsisLine.US_HealthCertificateNumber = "KZN01";

			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var fsisLine1 = invoiceLine.FSISLines.AddNew();
			fsisLine1.US_HealthCertificateNumber = "KZN01";

			var fsisLine2 = invoiceLine.FSISLines.AddNew();
			fsisLine2.US_HealthCertificateNumber = "KZN01";

			var fsisLine3 = invoiceLine.FSISLines.AddNew();
			fsisLine3.US_HealthCertificateNumber = "KZN02";

			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals(Core.Constants.CountryCodes.Australia, fsisLine1.US_UC_NKCertificateIssuerCountry);
			AssertEquals(Core.Constants.CountryCodes.Australia, fsisLine2.US_UC_NKCertificateIssuerCountry);
			AssertEquals("", fsisLine3.US_UC_NKCertificateIssuerCountry);
		}

		public void TestReadOnlyForECertificated()
		{
			var fsisLine = Declaration.FSISLines.AddNew();
			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			Assert(fsisLine.IsElectronicallyCertificated);
			Assert(fsisLine.US_ExportingEstNoInfo.ReadOnly);

			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Mexico;
			Assert(!fsisLine.IsElectronicallyCertificated);
			Assert(!fsisLine.US_ExportingEstNoInfo.ReadOnly);

			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.NewZealand;
			Assert(fsisLine.IsElectronicallyCertificated);
			Assert(fsisLine.US_ExportingEstNoInfo.ReadOnly);

			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Netherlands;
			Assert(fsisLine.IsElectronicallyCertificated);
			Assert(fsisLine.US_ExportingEstNoInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Declaration.FSISLines.AddNew();
		}

		protected override IEnumerable<USDeclarationFSISLine> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (USDeclarationFSISLine)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().FSISLines.AddNew();
		}

		JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration fDeclaration;
	}
}
