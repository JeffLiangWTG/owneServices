using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USInvoiceLineFSISLine))]
	public class USInvoiceLineFSISLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<USInvoiceLineFSISLine>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<USInvoiceLineFSISLine>();
			originalBO.Lots.AddNew();

			var newBO = (USInvoiceLineFSISLine)originalBO.Clone();

			AssertEquals(1, newBO.Lots.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (USInvoiceLineFSISLine)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(USInvoiceLineFSISLine), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Lots[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Lots[0].Factory.GetHashCode());
		}

		public void TestPGALineReadOnly()
		{
			var fsis = InvLine.FSISLines.AddNew();
			fsis.US_HealthCertificateNumber = "WERWER";
			fsis.US_CommercialDescription = "ERWERWER";
			fsis.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.France;
			fsis.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.France;
			Factory.Save();
			fsis.OnLoaded();
			Assert(!fsis.ReadOnly);

			fsis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			fsis.OnLoaded();
			Assert(fsis.ReadOnly);

			fsis.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			fsis.OnLoaded();
			Assert(!fsis.ReadOnly);

			fsis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			fsis.OnLoaded();
			Assert(fsis.ReadOnly);

			fsis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			fsis.OnLoaded();
			Assert(fsis.ReadOnly);
		}

		public void TestIParentDocManagerSupportMembers()
		{
			var fsisLine = Factory.New<USInvoiceLineFSISLine>();
			IParentDocManagerSupport support = fsisLine;
			AssertEquals("support.ParentTableName", JobDeclarationSchema.Constants.TableName, support.ParentTableName);
			AssertEquals("support.ParentGuid", ZGuid.Empty, support.ParentGuid);
			AssertNull("support.DocManagerInfo", support.DocManagerInfo);

			fsisLine.B7_ParentID = InvLine.PK;
			fsisLine.B7_ParentTableCode = InvLine.TablePrefix;
			AssertEquals("support.ParentTableName", JobDeclarationSchema.Constants.TableName, support.ParentTableName);
			AssertEquals("support.ParentGuid", Declaration.PK, support.ParentGuid);
			AssertEquals("support.DocManagerInfo", Declaration.DocManagerInfo, support.DocManagerInfo);
		}

		public void TestAndOrRemovFromDeclarationFSISLine()
		{
			var fsisLine = Declaration.FSISLines.AddNew();
			fsisLine.US_HealthCertificateNumber = "KZN01";

			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var fsisLine1 = invoiceLine.FSISLines.AddNew();
			fsisLine1.US_HealthCertificateNumber = "KZN01";

			var fsisLine2 = invoiceLine.FSISLines.AddNew();
			fsisLine2.US_HealthCertificateNumber = "KZN01";
			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals(Core.Constants.CountryCodes.Australia, fsisLine1.US_UC_NKCertificateIssuerCountry);
			AssertEquals(Core.Constants.CountryCodes.Australia, fsisLine2.US_UC_NKCertificateIssuerCountry);

			fsisLine2.US_HealthCertificateNumber = "KZN02";
			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, fsisLine1.US_UC_NKCertificateIssuerCountry);
			AssertEquals("", fsisLine2.US_UC_NKCertificateIssuerCountry);
		}

		public void TestDeclarationFSISLines()
		{
			var fsisLine = Factory.New<USInvoiceLineFSISLine>();
			AssertNoExceptionThrown(delegate
			{ var accessed = fsisLine.DeclarationFSISLines; });

			var fsisLine2 = InvLine.FSISLines.AddNew();
			AssertNotNull("used as Lookups for Certificates", fsisLine2.DeclarationFSISLines);
		}

		public void TestForUncommittedFSISLine()
		{
			var fsisLine = Factory.New<USInvoiceLineFSISLine>();
			AssertNoExceptionThrown(delegate
			{ fsisLine.US_SealNumbers = "342098"; });
		}

		public void TestCountryOfOrigin()
		{
			var country = Factory.New<USCCountry>();
			country.UC_Code = "XX";
			country.UC_Name = "XANAXIA";

			Factory.Save();

			var fsisLine = InvLine.FSISLines.AddNew();
			fsisLine.US_UC_NKCountryOfOrigin = "XX";

			AssertEquals("XANAXIA", fsisLine.CountryOfOrigin.UC_Name);
		}

		public void TestImportEstablishment()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers;
			cusCodeList.ZZD_Code = "M46712";
			cusCodeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberCompany, "121 In-Flight Catering LLC");
			cusCodeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberStreet, "45 Rason Road");
			cusCodeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberCity, "Inwood");
			cusCodeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberState, "NY");
			cusCodeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberZip, "11096");

			Factory.Save();

			var fsisLine = InvLine.FSISLines.AddNew();
			fsisLine.US_ImportingEstNo = "M46712";

			AssertEquals("121 In-Flight Catering LLC", fsisLine.ImportEstablishmentName);
			AssertEquals("45 Rason Road, Inwood, NY, 11096", fsisLine.ImportEstablishmentAddress);
		}

		public void TestReadOnlyForECertificated()
		{
			var fsisLine = InvLine.FSISLines.AddNew();
			fsisLine.US_HealthCertificateNumber = "ABC";
			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			Assert(fsisLine.US_ExportingEstNoInfo.ReadOnly);
			Assert(fsisLine.US_IntendedUseCodeInfo.ReadOnly);
			Assert(fsisLine.US_ProductIDQualifierInfo.ReadOnly);
			Assert(fsisLine.Lots.ReadOnly);

			fsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Mexico;
			Assert(!fsisLine.US_ExportingEstNoInfo.ReadOnly);
			Assert(!fsisLine.US_IntendedUseCodeInfo.ReadOnly);
			Assert(!fsisLine.US_ProductIDQualifierInfo.ReadOnly);
			Assert(!fsisLine.Lots.ReadOnly);

			var decFsisLine = (USDeclarationFSISLine)fsisLine.Parent.Declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "ABC");
			AssertEquals(Core.Constants.CountryCodes.Mexico, decFsisLine.US_UC_NKCertificateIssuerCountry);
			Assert(!decFsisLine.IsElectronicallyCertificated);
			Assert(!decFsisLine.US_ExportingEstNoInfo.ReadOnly);
			Assert(!decFsisLine.US_IntendedUseCodeInfo.ReadOnly);
			Assert(!decFsisLine.US_ProductIDQualifierInfo.ReadOnly);

			decFsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			Assert(decFsisLine.IsElectronicallyCertificated);
			Assert(decFsisLine.US_ExportingEstNoInfo.ReadOnly);
			Assert(decFsisLine.US_IntendedUseCodeInfo.ReadOnly);
			Assert(decFsisLine.US_ProductIDQualifierInfo.ReadOnly);

			decFsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Chile;
			Assert(!decFsisLine.IsElectronicallyCertificated);
			Assert(!decFsisLine.US_ExportingEstNoInfo.ReadOnly);
			Assert(!decFsisLine.US_IntendedUseCodeInfo.ReadOnly);
			Assert(!decFsisLine.US_ProductIDQualifierInfo.ReadOnly);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCLeCERT, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				decFsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
				Assert(decFsisLine.IsElectronicallyCertificated);
				Assert(decFsisLine.US_ExportingEstNoInfo.ReadOnly);
				Assert(decFsisLine.US_IntendedUseCodeInfo.ReadOnly);
				Assert(decFsisLine.US_ProductIDQualifierInfo.ReadOnly);

				decFsisLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Chile;
				Assert(decFsisLine.IsElectronicallyCertificated);
				Assert(decFsisLine.US_ExportingEstNoInfo.ReadOnly);
				Assert(decFsisLine.US_IntendedUseCodeInfo.ReadOnly);
				Assert(decFsisLine.US_ProductIDQualifierInfo.ReadOnly);
			}
		}

		public void TestOtherProperitesReadOnlyUntilCertificateIsEntered()
		{
			var fsisLine = InvLine.FSISLines.AddNew();
			Assert(fsisLine.US_ExportingEstNoInfo.ReadOnly);
			Assert(fsisLine.US_IntendedUseCodeInfo.ReadOnly);
			Assert(fsisLine.US_ProductIDQualifierInfo.ReadOnly);
			Assert(fsisLine.US_DateOfInspectionInfo.ReadOnly);
			Assert(!fsisLine.US_HealthCertificateNumberInfo.ReadOnly);

			fsisLine.US_HealthCertificateNumber = "ABCD";
			Assert(!fsisLine.US_ExportingEstNoInfo.ReadOnly);
			Assert(!fsisLine.US_IntendedUseCodeInfo.ReadOnly);
			Assert(!fsisLine.US_ProductIDQualifierInfo.ReadOnly);
			Assert(!fsisLine.US_DateOfInspectionInfo.ReadOnly);

			AssertEquals(1, InvLine.Declaration.FSISLines.Count);
			var fsisLine2 = InvLine.FSISLines.AddNew();
			fsisLine2.US_HealthCertificateNumber = "ABCD";
			Assert(fsisLine.US_ExportingEstNoInfo.ReadOnly);
			Assert(fsisLine.US_IntendedUseCodeInfo.ReadOnly);
			Assert(fsisLine.US_ProductIDQualifierInfo.ReadOnly);
			Assert(fsisLine2.US_ExportingEstNoInfo.ReadOnly);
			Assert(fsisLine2.US_IntendedUseCodeInfo.ReadOnly);
			Assert(fsisLine2.US_ProductIDQualifierInfo.ReadOnly);
		}

		public void TestDateOfInspectionReadOnlyAndCopied()
		{
			var fsisLine1 = InvLine.FSISLines.AddNew();
			fsisLine1.US_HealthCertificateNumber = "ABCD";
			fsisLine1.US_DateOfInspection = ZDateTime.BrettsBirthday;

			//check inv line is added and not read only
			AssertEquals(1, InvLine.Declaration.FSISLines.Count);
			AssertEquals(ZDateTime.BrettsBirthday, InvLine.Declaration.FSISLines[0].US_DateOfInspection);
			Assert(!fsisLine1.US_DateOfInspectionInfo.ReadOnly);

			//check date gets copied to declaration fsis line
			var decFsisLine = (USDeclarationFSISLine)fsisLine1.Parent.Declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "ABCD");
			AssertEquals(fsisLine1.US_DateOfInspection, decFsisLine.US_DateOfInspection);

			//check changes to inv line date reflected on declaration fsis line
			fsisLine1.US_DateOfInspection = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), decFsisLine.US_DateOfInspection);

			//check after removing date in line date, it gets removed on declaration fsis line as well
			fsisLine1.US_DateOfInspection = ZDate.Empty;
			Assert(decFsisLine.US_DateOfInspection.IsEmpty);

			//check that date for second line with same certificate number id read only
			var fsisLine2 = InvLine.FSISLines.AddNew();
			fsisLine2.US_HealthCertificateNumber = "ABCD";
			Assert(fsisLine2.US_DateOfInspectionInfo.ReadOnly);

			//check that changing date on declaration fsis line changes dates on both invoice lines
			decFsisLine.US_DateOfInspection = ZDateTime.BrettsBirthday.AddDays(5);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(5), fsisLine1.US_DateOfInspection);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(5), fsisLine2.US_DateOfInspection);
		}

		//craig says it is most likely to be the same for all lines, but one line could have a different date.
		public void TestDateOfInspectionGetsCopiedToDecForTheFirstTime()
		{
			var fsisLine = InvLine.FSISLines.AddNew();
			fsisLine.US_HealthCertificateNumber = "ABCD";
			fsisLine.US_DateOfInspection = ZDateTime.BrettsBirthday;

			AssertEquals(1, InvLine.Declaration.FSISLines.Count);
			AssertEquals(ZDateTime.BrettsBirthday, InvLine.Declaration.FSISLines[0].US_DateOfInspection);

			var fsisLine2 = InvLine.FSISLines.AddNew();
			fsisLine2.US_HealthCertificateNumber = "ABCD";
			AssertEquals("defaulted", ZDateTime.BrettsBirthday, fsisLine2.US_DateOfInspection);

			fsisLine2.US_DateOfInspection = ZDateTime.BrettsBirthday.AddDays(1);

			AssertEquals(ZDateTime.BrettsBirthday, InvLine.Declaration.FSISLines[0].US_DateOfInspection);
			AssertEquals(ZDateTime.BrettsBirthday, fsisLine.US_DateOfInspection);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), fsisLine2.US_DateOfInspection);
		}

		public void TestOriginCountryAndIssuerCountryCopied()
		{
			//check countries get copied from invoice line when creating new fsis line
			InvLine.US_UC_NKCountryOfOrigin = "OR";
			InvLine.US_UC_NKCountryOfExport = "EX";
			var fsisLine = InvLine.FSISLines.AddNew();
			fsisLine.US_HealthCertificateNumber = "ABCD";
			AssertEquals("OR", fsisLine.US_UC_NKCountryOfOrigin);
			AssertEquals("EX", fsisLine.US_UC_NKCertificateIssuerCountry);

			//check countries get copied to declaration fsis line
			var decFsisLine = (USDeclarationFSISLine)fsisLine.Parent.Declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "ABCD");
			AssertEquals(fsisLine.US_UC_NKCountryOfOrigin, decFsisLine.US_UC_NKCountryOfOrigin);

			//change countries on declaration fsis line
			decFsisLine.US_UC_NKCountryOfOrigin = "UA";
			decFsisLine.US_UC_NKCertificateIssuerCountry = "UA";

			//check data gets copied to fsis line
			AssertEquals("UA", fsisLine.US_UC_NKCountryOfOrigin);
			AssertEquals("UA", fsisLine.US_UC_NKCertificateIssuerCountry);

			//check data gets copied from declaration fsis line when creating new fsis line with same certificate number
			var fsisLine1 = InvLine.FSISLines.AddNew();
			fsisLine1.US_HealthCertificateNumber = "ABCD";
			AssertEquals("UA", fsisLine1.US_UC_NKCountryOfOrigin);
			AssertEquals("UA", fsisLine1.US_UC_NKCertificateIssuerCountry);
		}

		public void TestCertifySignatureDate()
		{
			var fsisLine = InvLine.FSISLines.AddNew();
			fsisLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, ((IFSISLine)fsisLine).CertifySignatureDate);

			var expectedDate = new ZDate(2017, 01, 01);
			fsisLine.InvoiceHeader.US_FSISSignDate = expectedDate;
			AssertEquals(expectedDate, ((IFSISLine)fsisLine).CertifySignatureDate);

			((IFSISLine)fsisLine).CertifySignatureDate = new ZDate(2017, 01, 02);
			AssertEquals(new ZDate(2017, 01, 02), ((IFSISLine)fsisLine).CertifySignatureDate);
		}

		public void TestDeclarationCertificate()
		{
			var fsisLine = InvLine.FSISLines.AddNew();
			fsisLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Empty;
			AssertEquals("", ((IFSISLine)fsisLine).DeclarationCertificate);

			fsisLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Today;
			AssertEquals("Y", ((IFSISLine)fsisLine).DeclarationCertificate);
		}

		public void TestClone()
		{
			var fsis = InvLine.FSISLines.AddNew();
			fsis.US_HealthCertificateNumber = "WERWER";
			fsis.US_CommercialDescription = "ERWERWER";
			fsis.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.France;
			fsis.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.France;
			fsis.US_ProductID = "34234";
			fsis.US_ProductIDQualifier = "AI";
			fsis.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseHumanFood;
			fsis.US_ExportingEstNo = "AS3";
			fsis.US_ImportingEstNo = "DWS1";
			fsis.US_DateOfInspection = new ZDateTime(2016, 4, 7);

			var lot = fsis.Lots.AddNew();
			lot.US_LotNumber = "324";
			lot.US_ShippingMarks = "RET ERT";
			lot.US_NoOfUnit1 = 44;
			lot.US_UQ1 = "AP";
			lot.US_NoOfUnit2 = 43;
			lot.US_UQ2 = "AE";
			lot.US_NetWeight = 33m;
			lot.US_WeightUQ = "DT";
			lot.US_StartDate = new ZDateTime(2016, 4, 12);
			lot.US_EndDate = new ZDateTime(2016, 4, 19);
			lot.US_Species = "4";
			lot.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.HTSS;
			lot.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._1A;
			lot.US_ProducingEstNo = "SAP";
			lot.US_SourceEstNo = "ESTNO";
			lot.US_SourceCountry = Core.Constants.CountryCodes.France;

			InvLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var fsisNew = InvLine.FSISLines.AddNew();

			AssertEquals("CommercialDescription must be the same", fsis.US_CommercialDescription, fsisNew.US_CommercialDescription);
			AssertEquals("UC_NKCertificateIssuerCountry must be the same", fsis.US_UC_NKCertificateIssuerCountry, fsisNew.US_UC_NKCertificateIssuerCountry);
			AssertEquals("UC_NKCountryOfOrigin must be the same", fsis.US_UC_NKCountryOfOrigin, fsisNew.US_UC_NKCountryOfOrigin);
			AssertEquals("ProductID must be the same", fsis.US_ProductID, fsisNew.US_ProductID);
			AssertEquals("ProductIDQualifier must be the same", fsis.US_ProductIDQualifier, fsisNew.US_ProductIDQualifier);
			AssertEquals("IntendedUseCode must be the same", fsis.US_IntendedUseCode, fsisNew.US_IntendedUseCode);
			AssertEquals("ExportingEstNo must be the same", fsis.US_ExportingEstNo, fsisNew.US_ExportingEstNo);
			AssertEquals("ImportingEstNo must be the same", fsis.US_ImportingEstNo, fsisNew.US_ImportingEstNo);

			AssertEquals("Lot: Species must be the same", fsis.Lots[0].US_Species, fsisNew.Lots[0].US_Species);
			AssertEquals("Lot: ProductQualifierCode must be the same", fsis.Lots[0].US_ProductQualifierCode, fsisNew.Lots[0].US_ProductQualifierCode);
			AssertEquals("Lot: ProductCharacteristicQualifier must be the same", fsis.Lots[0].US_ProductCharacteristicQualifier, fsisNew.Lots[0].US_ProductCharacteristicQualifier);
			AssertEquals("Lot: ProducingEstNo must be the same", fsis.Lots[0].US_ProducingEstNo, fsisNew.Lots[0].US_ProducingEstNo);
			AssertEquals("Lot: SourceEstNo must be the same", fsis.Lots[0].US_SourceEstNo, fsisNew.Lots[0].US_SourceEstNo);
			AssertEquals("Lot: SourceCountry must be the same", fsis.Lots[0].US_SourceCountry, fsisNew.Lots[0].US_SourceCountry);
		}

		public void TestUS_CertifyingIndividual()
		{
			var iorOrgHeader = Factory.New<OrgHeader>();
			iorOrgHeader.OH_Code = "TESTIOR";
			DeclarationTestHelper.AddPGAContact(iorOrgHeader, "FIRST", "LAST", "123456", "ior@ian.com", null);

			var fsis = InvLine.FSISLines.AddNew();
			var declaration = fsis.Parent.Declaration;
			declaration.IOROrgPK = iorOrgHeader.PK;

			var invoice = fsis.Parent.InvoiceHeader;
			invoice.US_FDAContactName = "Broker NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@ian.com";

			fsis.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("Broker NAME", fsis.US_PGAContactName);
			AssertEquals("2345678", fsis.US_PGAContactPhoneNo);
			AssertEquals("broker@ian.com", fsis.US_PGAContactEmail);

			fsis.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("FIRST LAST", fsis.US_PGAContactName);
			AssertEquals("123456", fsis.US_PGAContactPhoneNo);
			AssertEquals("ior@ian.com", fsis.US_PGAContactEmail);
		}

		public void TestDocumentPropertyFromRefSysConfig()
		{
			var expirationDateType = Factory.New<RefSysConfigType>();
			expirationDateType.ZRT_ConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540ED;
			expirationDateType.ZRT_Description = "Expiration Statement to print on FSIS Form 9540-1";
			expirationDateType.ZRT_LongDescription = "Expiration Statement to print on FSIS Form 9540-1";

			var revisionStatementDateType = Factory.New<RefSysConfigType>();
			revisionStatementDateType.ZRT_ConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540RD;
			revisionStatementDateType.ZRT_Description = "Revision Statement to print on FSIS Form 9540-1";
			revisionStatementDateType.ZRT_LongDescription = "Revision Statement to print on FSIS Form 9540-1";

			var replacementStatementType = Factory.New<RefSysConfigType>();
			replacementStatementType.ZRT_ConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540RP;
			replacementStatementType.ZRT_Description = "Replacement Statement to print on FSIS Form 9540-1";
			replacementStatementType.ZRT_LongDescription = "Replacement Statement to print on FSIS Form 9540-1";

			var expirationDate = Factory.New<RefSysConfig>();
			expirationDate.ZRC_ZRT_NKConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540ED;
			expirationDate.ZRC_DecimalValue = 0;
			expirationDate.ZRC_StringValue = "EXPIRATION DATE:  12/31/2025";
			expirationDate.ZRC_StartDate = new ZDateTime(2023, 1, 12);
			expirationDate.ZRC_EndDate = new ZDateTime(2079, 6, 6);

			var revisionStatementDate = Factory.New<RefSysConfig>();
			revisionStatementDate.ZRC_ZRT_NKConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540RD;
			revisionStatementDate.ZRC_DecimalValue = 0;
			revisionStatementDate.ZRC_StringValue = "(01/12/2023)";
			revisionStatementDate.ZRC_StartDate = new ZDateTime(2023, 1, 12);
			revisionStatementDate.ZRC_EndDate = new ZDateTime(2079, 6, 6);

			var replacementStatement = Factory.New<RefSysConfig>();
			replacementStatement.ZRC_ZRT_NKConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540RP;
			replacementStatement.ZRC_DecimalValue = 0;
			replacementStatement.ZRC_StringValue = "REPLACES FSIS FORM 9540-1 (5/02) WHICH IS OBSOLETE";
			replacementStatement.ZRC_StartDate = new ZDateTime(2023, 1, 12);
			replacementStatement.ZRC_EndDate = new ZDateTime(2079, 6, 6);
			Factory.Save();

			var fsisLine = (USInvoiceLineFSISLine)GetNewBusinessObject();
			AssertEquals("Expiration Statement to print on FSIS Form 9540-1", "EXPIRATION DATE:  12/31/2025", fsisLine.ExpirationDate);
			AssertEquals("Revision Statement to print on FSIS Form 9540-1", "(01/12/2023)", fsisLine.RevisionStatementDate);
			AssertEquals("Replacement Statement to print on FSIS Form 9540-1", "REPLACES FSIS FORM 9540-1 (5/02) WHICH IS OBSOLETE", fsisLine.ReplacementStatement);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvLine.FSISLines.AddNew();
		}

		protected override IEnumerable<USInvoiceLineFSISLine> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (USInvoiceLineFSISLine)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().FSISLines.AddNew();
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceLine InvLine
		{
			get { return fInvLine ?? (fInvLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew()); }
		}
		JobComInvoiceLine fInvLine;
	}
}
