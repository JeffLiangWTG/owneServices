using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Vehicle))]
	public class VehicleTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<Vehicle>
	{
		public void TestPGALineReadOnly()
		{
			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			Vehicle.US_VehicleModel = "2012";
			Vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._04;
			Factory.Save();
			Vehicle.OnLoaded();
			Assert(!Vehicle.ReadOnly);

			Vehicle.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			Vehicle.OnLoaded();
			Assert(Vehicle.ReadOnly);

			Vehicle.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			Vehicle.OnLoaded();
			Assert(!Vehicle.ReadOnly);

			Vehicle.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			Vehicle.OnLoaded();
			Assert(Vehicle.ReadOnly);

			Vehicle.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			Vehicle.OnLoaded();
			Assert(Vehicle.ReadOnly);
		}

		public void TestUS_CertifyingIndividual()
		{
			var iorOrgHeader = Factory.New<OrgHeader>();
			iorOrgHeader.OH_Code = "TESTIOR";
			DeclarationTestHelper.AddPGAContact(iorOrgHeader, "FIRST", "LAST", "123456", "ior@ian.com", null);

			var declaration = Vehicle.InvoiceLine.Declaration;
			declaration.IOROrgPK = iorOrgHeader.PK;

			var invoice = Vehicle.InvoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "Broker NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@ian.com";

			var ownerOrgHeader = Factory.New<OrgHeader>();
			ownerOrgHeader.OH_Code = "TESTOWN";
			DeclarationTestHelper.AddPGAContact(ownerOrgHeader, "OWNER", "TEST", "234567", "owner@ian.com", null);
			Vehicle.US_OA_Owner = ownerOrgHeader.MainAddress.PK;

			Vehicle.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("FIRST LAST", Vehicle.US_ContactName);
			AssertEquals("123456", Vehicle.US_ContactPhoneNo);
			AssertEquals("ior@ian.com", Vehicle.US_ContactEmail);

			Vehicle.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("Broker NAME", Vehicle.US_ContactName);
			AssertEquals("2345678", Vehicle.US_ContactPhoneNo);
			AssertEquals("broker@ian.com", Vehicle.US_ContactEmail);

			Vehicle.US_CertifyingIndividual = PartyTypeList.Codes.Owner;
			AssertEquals("OWNER TEST", Vehicle.US_ContactName);
			AssertEquals("234567", Vehicle.US_ContactPhoneNo);
			AssertEquals("owner@ian.com", Vehicle.US_ContactEmail);
		}

		public void TestUS_CertifyingIndividualWhenContactPhoneN0IsTooLarge()
		{
			var iorOrgHeader = Factory.New<OrgHeader>();
			iorOrgHeader.OH_Code = "TESTIOR";
			var emailname = "";
			for (int i = 0; i < 23; i++)
			{
				emailname += "1234567890";
			}

			DeclarationTestHelper.AddPGAContact(iorOrgHeader, "FIRST12345123456789012345", "LAST12345612345678901234", "12345678901234567890", emailname + "1234567890123ior@ian.com", null);

			var declaration = Vehicle.InvoiceLine.Declaration;
			declaration.IOROrgPK = iorOrgHeader.PK;

			var invoice = Vehicle.InvoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "Broker NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@ian.com";

			var ownerOrgHeader = Factory.New<OrgHeader>();
			ownerOrgHeader.OH_Code = "TESTOWN";
			DeclarationTestHelper.AddPGAContact(ownerOrgHeader, "OWNER", "TEST", "32345678903234567890", "owner@ian.com", null);
			Vehicle.US_OA_Owner = ownerOrgHeader.MainAddress.PK;

			Vehicle.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("FIRST12345123456789012345 LAST12345612345678901234", Vehicle.US_ContactName);
			AssertEquals("123456789012345", Vehicle.US_ContactPhoneNo);
			AssertEquals("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123ior@ian.com", vehicle.US_ContactEmail);

			Vehicle.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("Broker NAME", Vehicle.US_ContactName);
			AssertEquals("2345678", Vehicle.US_ContactPhoneNo);
			AssertEquals("broker@ian.com", Vehicle.US_ContactEmail);

			Vehicle.US_CertifyingIndividual = PartyTypeList.Codes.Owner;
			AssertEquals("OWNER TEST", Vehicle.US_ContactName);
			AssertEquals("323456789032345", Vehicle.US_ContactPhoneNo);
			AssertEquals("owner@ian.com", Vehicle.US_ContactEmail);

			var newOrgHeader = Factory.New<OrgHeader>();
			newOrgHeader.OH_Code = "TESTNEW";
			DeclarationTestHelper.AddPGAContact(newOrgHeader, "NEW", "TEST", "42345678904234567890", "new@ian.com", null);

			Vehicle.US_OA_Owner = newOrgHeader.MainAddress.PK;
			AssertEquals("NEW TEST", Vehicle.US_ContactName);
			AssertEquals("423456789042345", Vehicle.US_ContactPhoneNo);
			AssertEquals("new@ian.com", Vehicle.US_ContactEmail);
		}

		public void TestUS_BondPolicyNo_ReadOnly()
		{
			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertEquals(true, Vehicle.US_BondPolicyNo_ReadOnly);

			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			AssertEquals(false, Vehicle.US_BondPolicyNo_ReadOnly);

			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertEquals(true, Vehicle.US_BondPolicyNo_ReadOnly);

			Vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.G;
			AssertEquals(false, Vehicle.US_BondPolicyNo_ReadOnly);

			Vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			AssertEquals(true, Vehicle.US_BondPolicyNo_ReadOnly);
		}

		public void TestUS_EnginePower_ReadOnly()
		{
			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertEquals(true, Vehicle.US_EnginePower_ReadOnly);

			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			AssertEquals(false, Vehicle.US_EnginePower_ReadOnly);

			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertEquals(true, Vehicle.US_EnginePower_ReadOnly);

			Vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.U;
			AssertEquals(false, Vehicle.US_EnginePower_ReadOnly);

			Vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			AssertEquals(true, Vehicle.US_EnginePower_ReadOnly);
		}

		public void TestUS_EnginePowerUQ_ReadOnly()
		{
			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertEquals(true, Vehicle.US_EnginePowerUQ_ReadOnly);

			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			AssertEquals(false, Vehicle.US_EnginePowerUQ_ReadOnly);

			Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertEquals(true, Vehicle.US_EnginePowerUQ_ReadOnly);

			Vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.U;
			AssertEquals(false, Vehicle.US_EnginePowerUQ_ReadOnly);

			Vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			AssertEquals(true, Vehicle.US_EnginePowerUQ_ReadOnly);
		}

		public void TestCertifySignatureDate()
		{
			Vehicle.InvoiceHeader.US_VNESignDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, ((IVNEData)Vehicle).CertifySignatureDate);

			var expectedDate = new ZDate(2017, 01, 01);
			Vehicle.InvoiceHeader.US_VNESignDate = expectedDate;
			AssertEquals(expectedDate, ((IVNEData)Vehicle).CertifySignatureDate);

			((IVNEData)Vehicle).CertifySignatureDate = new ZDate(2017, 01, 02);
			AssertEquals(new ZDate(2017, 01, 02), ((IVNEData)Vehicle).CertifySignatureDate);
		}

		public void TestDeclarationCertificate()
		{
			Vehicle.InvoiceHeader.US_VNESignDate = ZDateTime.Empty;
			AssertEquals("", ((IVNEData)Vehicle).DeclarationCertificate);

			Vehicle.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			AssertEquals("Y", ((IVNEData)Vehicle).DeclarationCertificate);
		}

		public void TestClone()
		{
			var invoiceLine = Vehicle.InvoiceLine;

			var vehicle = invoiceLine.VehicleLines.AddNew();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_VehicleModel = "2012";
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._04;
			vehicle.US_IndustryCode = IndustryCodesList.Codes.D;
			vehicle.US_ModelYear = "2012";
			vehicle.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicle.US_VNEElectronicImage = ZBool.True;

			invoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var vehicleNew = invoiceLine.VehicleLines.AddNew();

			AssertEquals("FormType must be equal", vehicle.US_FormType, vehicleNew.US_FormType);
			AssertEquals("VehicleModel must be equal", vehicle.US_VehicleModel, vehicleNew.US_VehicleModel);
			AssertEquals("ImportCode must be equal", vehicle.US_ImportCode, vehicleNew.US_ImportCode);
			AssertEquals("IndustryCode must be equal", vehicle.US_IndustryCode, vehicleNew.US_IndustryCode);
			AssertEquals("ModelYear must be equal", vehicle.US_ModelYear, vehicleNew.US_ModelYear);
			AssertEquals("CertifyingIndividual must be equal", vehicle.US_CertifyingIndividual, vehicleNew.US_CertifyingIndividual);
			AssertEquals("VNEElectronicImage must be false", ZBool.False, vehicleNew.US_VNEElectronicImage);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return Vehicle;
		}

		#endregion

		#region Implementation

		protected override IEnumerable<Vehicle> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (Vehicle)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine.VehicleLines.AddNew();
		}

		Vehicle Vehicle
		{
			get
			{
				if (vehicle == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					vehicle = invoiceLine.VehicleLines.AddNew();
				}
				return vehicle;
			}
		}
		Vehicle vehicle;

		#endregion
	}
}
