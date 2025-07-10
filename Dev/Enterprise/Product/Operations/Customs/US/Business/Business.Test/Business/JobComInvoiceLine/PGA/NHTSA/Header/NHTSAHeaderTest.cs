using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSAHeader))]
	public class NHTSAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NHTSAHeader>
	{
		public void TestGetCusAddInfoTypes()
		{
			var nhtsaHeader = Factory.New<NHTSAHeader>();
			var supporter = nhtsaHeader as ICusAddInfoTypeSupporter;

			Type type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USNHTSADetails, out type);
			AssertEquals(typeof(NHTSADetails), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USNHTSADocument, out type);
			AssertEquals(typeof(NHTSADocument), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue("ZZ!", out type);
			AssertNull(type);
		}

		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<NHTSAHeader>();
			originalBO.NHTSADetails.AddNew();
			originalBO.NHTSADocuments.AddNew();

			var newBO = (NHTSAHeader)originalBO.Clone();

			Assert(newBO.NHTSADetails.Count > 0);
			Assert(newBO.NHTSADocuments.Count > 0);

			var fac = new BusinessObjectFactory();
			var newFacClone = (NHTSAHeader)originalBO.Clone(new BusinessObjectCloneArgs(fac, Array.Empty<string>(), typeof(NHTSAHeader), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.NHTSADetails[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.NHTSADocuments[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.NHTSADetails[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.NHTSADocuments[0].Factory.GetHashCode());
		}

		public void TestPGALineReadOnly()
		{
			var invoiceLine = Header.InvoiceLine;

			var nhtsaHeader = invoiceLine.NHTSALines.AddNew();
			nhtsaHeader.US_NHTProgramCode = "MVS";
			nhtsaHeader.US_NHTBoxNumber = "03";
			nhtsaHeader.US_NHTElectronicImage = ZBool.True;
			nhtsaHeader.US_CertifyingIndividual = "CB";

			Factory.Save();
			nhtsaHeader.OnLoaded();
			Assert(!nhtsaHeader.ReadOnly);

			nhtsaHeader.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			nhtsaHeader.OnLoaded();
			Assert(nhtsaHeader.ReadOnly);

			nhtsaHeader.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			nhtsaHeader.OnLoaded();
			Assert(!nhtsaHeader.ReadOnly);

			nhtsaHeader.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			nhtsaHeader.OnLoaded();
			Assert(nhtsaHeader.ReadOnly);

			nhtsaHeader.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			nhtsaHeader.OnLoaded();
			Assert(nhtsaHeader.ReadOnly);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(Header.NHTSADocuments.Count, 1);
			AssertEquals(NHTSADocumentTypeList.Codes._946, Header.NHTSADocuments[0].US_NHTDocumentType);
			AssertEquals(NHTSAOrganizationTypeList.Codes.CertifyingIndividual, Header.NHTSADocuments[0].US_NHTDocumentOwner);
		}

		public void TestFabricatingManufacturerAddress()
		{
			Header.US_NHTFabricatingMFROrgPK = ManufacturerOrg.PK;
			AssertEquals(ManufacturerMainAddress.PK, Header.US_NHTFabricatingMFRAddress);

			Header.US_NHTFabricatingMFRAddress = ManufacturerNewAddress.PK;
			AssertEquals(ManufacturerNewAddress.PK, Header.US_NHTFabricatingMFRAddress);
		}

		public void TestOriginalVehicleManufacturerAddress()
		{
			Header.US_NHTOriginalMFROrgPK = ManufacturerOrg.PK;
			AssertEquals(ManufacturerMainAddress.PK, Header.US_NHTOriginalMFRAddress);

			Header.US_NHTOriginalMFRAddress = ManufacturerNewAddress.PK;
			AssertEquals(ManufacturerNewAddress.PK, Header.US_NHTOriginalMFRAddress);
		}

		public void TestCloneNHTSAHeader()
		{
			var newDocument = Header.NHTSADocuments.AddNew();
			newDocument.US_NHTDocumentType = NHTSADocumentTypeList.Codes._875;
			newDocument.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Owner;

			var clonedHeader = (NHTSAHeader)Header.Clone();
			AssertEquals(2, clonedHeader.NHTSADocuments.Count);
			AssertEquals(NHTSADocumentTypeList.Codes._946, clonedHeader.NHTSADocuments[0].US_NHTDocumentType);
			AssertEquals(NHTSAOrganizationTypeList.Codes.CertifyingIndividual, clonedHeader.NHTSADocuments[0].US_NHTDocumentOwner);
			AssertEquals(NHTSADocumentTypeList.Codes._875, clonedHeader.NHTSADocuments[1].US_NHTDocumentType);
			AssertEquals(NHTSAOrganizationTypeList.Codes.Owner, clonedHeader.NHTSADocuments[1].US_NHTDocumentOwner);
		}

		public void TestUS_CertifyingIndividual()
		{
			var iorOrgHeader = Factory.New<OrgHeader>();
			iorOrgHeader.OH_Code = "TESTIOR";
			DeclarationTestHelper.AddPGAContact(iorOrgHeader, "FIRST", "LAST", "123456", "ior@ian.com", null);

			var declaration = Header.InvoiceLine.Declaration;
			declaration.IOROrgPK = iorOrgHeader.PK;

			var invoice = Header.InvoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "Broker NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@ian.com";

			var ownerOrgHeader = Factory.New<OrgHeader>();
			ownerOrgHeader.OH_Code = "TESTOWN";
			DeclarationTestHelper.AddPGAContact(ownerOrgHeader, "OWNER", "TEST", "234567", "owner@ian.com", null);
			Header.US_OA_NHTOwner = ownerOrgHeader.MainAddress.PK;

			Header.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("FIRST LAST", Header.US_PGAContactName);
			AssertEquals("123456", Header.US_PGAContactPhoneNo);
			AssertEquals("ior@ian.com", Header.US_PGAContactEmail);

			Header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("Broker NAME", Header.US_PGAContactName);
			AssertEquals("2345678", Header.US_PGAContactPhoneNo);
			AssertEquals("broker@ian.com", Header.US_PGAContactEmail);

			Header.US_CertifyingIndividual = PartyTypeList.Codes.Owner;
			AssertEquals("OWNER TEST", Header.US_PGAContactName);
			AssertEquals("234567", Header.US_PGAContactPhoneNo);
			AssertEquals("owner@ian.com", Header.US_PGAContactEmail);
		}
		public void TestUS_CertifyingIndividualWhenContactPhoneN0IsTooLarge()
		{
			var iorOrgHeader = Factory.New<OrgHeader>();
			iorOrgHeader.OH_Code = "TESTIOR";
			DeclarationTestHelper.AddPGAContact(iorOrgHeader, "FIRST12345123456789012345", "LAST12345612345678901234", "12345678901234567890", null, null);
			var emailname = "";
			for (int i = 0; i < 23; i++)
			{
				emailname += "1234567890";
			}
			DeclarationTestHelper.AddPGAContact(iorOrgHeader, null, null, null, emailname + "1234567890123ior@ian.com", null);

			var declaration = Header.InvoiceLine.Declaration;
			declaration.IOROrgPK = iorOrgHeader.PK;

			var invoice = Header.InvoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "Broker NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@ian.com";

			var ownerOrgHeader = Factory.New<OrgHeader>();
			ownerOrgHeader.OH_Code = "TESTOWN";
			DeclarationTestHelper.AddPGAContact(ownerOrgHeader, "OWNER", "TEST", "32345678903234567890", "owner@ian.com", null);
			Header.US_OA_NHTOwner = ownerOrgHeader.MainAddress.PK;

			Header.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("FIRST12345123456789012345 LAST12345612345678901234", Header.US_PGAContactName);
			AssertEquals("123456789012345", Header.US_PGAContactPhoneNo);
			AssertEquals("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123ior@ian.com", Header.US_PGAContactEmail);

			Header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("Broker NAME", Header.US_PGAContactName);
			AssertEquals("2345678", Header.US_PGAContactPhoneNo);
			AssertEquals("broker@ian.com", Header.US_PGAContactEmail);

			Header.US_CertifyingIndividual = PartyTypeList.Codes.Owner;
			AssertEquals("OWNER TEST", Header.US_PGAContactName);
			AssertEquals("323456789032345", Header.US_PGAContactPhoneNo);
			AssertEquals("owner@ian.com", Header.US_PGAContactEmail);

			var newOrgHeader = Factory.New<OrgHeader>();
			newOrgHeader.OH_Code = "TESTNEW";
			DeclarationTestHelper.AddPGAContact(newOrgHeader, "NEW", "TEST", "42345678904234567890", "new@ian.com", null);

			Header.US_OA_NHTOwner = newOrgHeader.MainAddress.PK;
			AssertEquals("NEW TEST", Header.US_PGAContactName);
			AssertEquals("423456789042345", Header.US_PGAContactPhoneNo);
			AssertEquals("new@ian.com", Header.US_PGAContactEmail);
		}

		public void TestCertifySignatureDate()
		{
			Header.InvoiceHeader.US_NHTSASignDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, ((INHTSAHeader)Header).CertifySignatureDate);

			var expectedDate = new ZDate(2017, 01, 01);
			Header.InvoiceHeader.US_NHTSASignDate = expectedDate;
			AssertEquals(expectedDate, ((INHTSAHeader)Header).CertifySignatureDate);

			((INHTSAHeader)Header).CertifySignatureDate = new ZDate(2017, 01, 02);
			AssertEquals(new ZDate(2017, 01, 02), ((INHTSAHeader)Header).CertifySignatureDate);
		}

		public void TestDeclarationCertificate()
		{
			Header.InvoiceHeader.US_NHTSASignDate = ZDateTime.Empty;
			AssertEquals("", ((INHTSAHeader)Header).DeclarationCertificate);

			Header.InvoiceHeader.US_NHTSASignDate = ZDateTime.Today;
			AssertEquals("Y", ((INHTSAHeader)Header).DeclarationCertificate);
		}

		public void TestClone()
		{
			var invoiceLine = Header.InvoiceLine;

			var nhtsaHeader = invoiceLine.NHTSALines.AddNew();
			nhtsaHeader.US_NHTProgramCode = "MVS";
			nhtsaHeader.US_NHTBoxNumber = "03";
			nhtsaHeader.US_NHTElectronicImage = ZBool.True;
			nhtsaHeader.US_CertifyingIndividual = "CB";
			nhtsaHeader.US_OA_NHTOwner = ZGuid.Empty;

			var nhtsaDetails = nhtsaHeader.NHTSADetails.AddNew();
			nhtsaDetails.US_NHTBrandName = "BRAND";
			nhtsaDetails.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS4;
			nhtsaDetails.US_NHTModel = "A2015";
			nhtsaDetails.US_NHTYearOfMFR = "2015";
			nhtsaDetails.US_NHTMonthOfMFR = MonthList.Codes._02;
			nhtsaDetails.US_NHTDriveSide = DriverSideList.Codes.Right;
			nhtsaDetails.US_NHTModelYear = "2015";
			nhtsaDetails.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			nhtsaDetails.US_NHTIdentityNumber = "CHARACTERS1754879";

			var nhtsaDocuments = nhtsaHeader.NHTSADocuments.AddNew();
			nhtsaDocuments.US_NHTDocumentType = "946";
			nhtsaDocuments.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Consignee;

			invoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var nhtsaHeaderNew = invoiceLine.NHTSALines.AddNew();

			AssertEquals("NHTProgramCode must be the same", nhtsaHeader.US_NHTProgramCode, nhtsaHeaderNew.US_NHTProgramCode);
			AssertEquals("NHTBoxNumber must be the same", nhtsaHeader.US_NHTBoxNumber, nhtsaHeaderNew.US_NHTBoxNumber);
			AssertEquals("NHTElectronicImage must be false", ZBool.False, nhtsaHeaderNew.US_NHTElectronicImage);
			AssertEquals("CertifyingIndividual must be the same", nhtsaHeader.US_CertifyingIndividual, nhtsaHeaderNew.US_CertifyingIndividual);
			AssertEquals("NHTOwner must be the same", nhtsaHeader.US_OA_NHTOwner, nhtsaHeaderNew.US_OA_NHTOwner);

			AssertEquals("Details: NHTBrandName must be the same", nhtsaHeader.NHTSADetails[0].US_NHTBrandName, nhtsaHeaderNew.NHTSADetails[0].US_NHTBrandName);
			AssertEquals("Details: NHTCategoryCode must be the same", nhtsaHeader.NHTSADetails[0].US_NHTCategoryCode, nhtsaHeaderNew.NHTSADetails[0].US_NHTCategoryCode);
			AssertEquals("Details: NHTModel must be the same", nhtsaHeader.NHTSADetails[0].US_NHTModel, nhtsaHeaderNew.NHTSADetails[0].US_NHTModel);
			AssertEquals("Details: NHTYearOfMFR must be the same", nhtsaHeader.NHTSADetails[0].US_NHTYearOfMFR, nhtsaHeaderNew.NHTSADetails[0].US_NHTYearOfMFR);
			AssertEquals("Details: NHTMonthOfMFR must be the same", nhtsaHeader.NHTSADetails[0].US_NHTMonthOfMFR, nhtsaHeaderNew.NHTSADetails[0].US_NHTMonthOfMFR);
			AssertEquals("Details: NHTDriveSide must be the same", nhtsaHeader.NHTSADetails[0].US_NHTDriveSide, nhtsaHeaderNew.NHTSADetails[0].US_NHTDriveSide);
			AssertEquals("Details: NHTModelYear must be the same", nhtsaHeader.NHTSADetails[0].US_NHTModelYear, nhtsaHeaderNew.NHTSADetails[0].US_NHTModelYear);
			AssertEquals("Details: NHTIdentityNumQualifier must be the same", nhtsaHeader.NHTSADetails[0].US_NHTIdentityNumQualifier, nhtsaHeaderNew.NHTSADetails[0].US_NHTIdentityNumQualifier);
			AssertEquals("Details: NHTIdentityNumber must be the same", nhtsaHeader.NHTSADetails[0].US_NHTIdentityNumber, nhtsaHeaderNew.NHTSADetails[0].US_NHTIdentityNumber);

			AssertEquals("Documents: NHTDocumentType must be the same", nhtsaHeader.NHTSADocuments[0].US_NHTDocumentType, nhtsaHeaderNew.NHTSADocuments[0].US_NHTDocumentType);
		}

		public void TestINHTSAHeaderMembers()
		{
			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			Header.US_NHTElectronicImage = true;
			Header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._03;
			Header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForOtherUse;
			Header.US_IntendedUseDesc = "THIS IS VERY LONG DESC";
			Header.US_NHTEmbassyNationality = Core.Constants.CountryCodes.UnitedStates;
			Header.US_NHTTravelDocType = TravelDocumentTypeCodeList.Codes._1;
			Header.US_NHTTravelDocNationality = Core.Constants.CountryCodes.China;
			Header.US_NHTTravelDocNumber = "111111";

			var iHeader = (INHTSAHeader)Header;
			AssertEquals("iHeader.PGALineStatusProgramCode", NHTSAProgramCodeList.Codes.MVS, iHeader.ProgramCode);
			AssertEquals("iHeader.ElectronicImageSubmitted", true, iHeader.ElectronicImageSubmitted);
			AssertEquals("iHeader.BoxNumber", "3", iHeader.BoxNumber);
			AssertEquals("iHeader.IntendedUseCode", IntendedUseCodesList.Codes.ForOtherUse, iHeader.IntendedUseCode);
			AssertEquals("iHeader.IntendedUseDesc", "THIS IS VERY LONG DES", iHeader.IntendedUseDesc);
			AssertEquals("iHeader.EmbassyNationality", Core.Constants.CountryCodes.UnitedStates, iHeader.EmbassyNationality);
			AssertEquals("iHeader.DocumentType", TravelDocumentTypeCodeList.Codes._1, iHeader.DocumentType);
			AssertEquals("iHeader.DocumentNationality", Core.Constants.CountryCodes.China, iHeader.DocumentNationality);
			AssertEquals("iHeader.DocumentNumber", "111111", iHeader.DocumentNumber);
		}

		#region Implementation

		OrgHeader ManufacturerOrg
		{
			get
			{
				if (fManufacturerOrg == null)
				{
					fManufacturerOrg = Factory.New<OrgHeader>();
					fManufacturerOrg.OH_Code = "TESTMFR";
				}

				return fManufacturerOrg;
			}
		}
		OrgHeader fManufacturerOrg;

		OrgAddress ManufacturerMainAddress
		{
			get
			{
				if (fManufacturerMainAddress == null)
				{
					fManufacturerMainAddress = ManufacturerOrg.MainAddress;
					fManufacturerMainAddress.OA_Address1 = "TEST MAIN ADDRESS 1";
					fManufacturerMainAddress.OA_City = "HONGKONG";
					fManufacturerMainAddress.OA_RL_NKRelatedPortCode = "HKHKG";
				}
				return fManufacturerMainAddress;
			}
		}
		OrgAddress fManufacturerMainAddress;

		OrgAddress ManufacturerNewAddress
		{
			get
			{
				if (fManufacturerNewAddress == null)
				{
					fManufacturerNewAddress = ManufacturerOrg.Addresses.AddNew();
					fManufacturerNewAddress.OA_Address1 = "TEST NEW ADDRESS 1";
					fManufacturerNewAddress.OA_City = "SYDNEY";
					fManufacturerNewAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				}
				return fManufacturerNewAddress;
			}
		}
		OrgAddress fManufacturerNewAddress;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine.NHTSALines.AddNew();
		}

		protected override IEnumerable<NHTSAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			yield return invoiceLine.NHTSALines.AddNew();
		}

		NHTSAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					header = invoiceLine.NHTSALines.AddNew();
				}
				return header;
			}
		}

		NHTSAHeader header;

		#endregion
	}
}
