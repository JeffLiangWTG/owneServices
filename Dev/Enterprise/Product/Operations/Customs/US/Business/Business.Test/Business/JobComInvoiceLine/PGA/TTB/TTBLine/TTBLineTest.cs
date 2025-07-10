using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TTBLine))]
	public class TTBLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<TTBLine>
	{
		public void TestPGALineReadOnly()
		{
			var ttbLine = InvoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
			Factory.Save();
			ttbLine.OnLoaded();
			Assert(!ttbLine.ReadOnly);

			ttbLine.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			ttbLine.OnLoaded();
			Assert(ttbLine.ReadOnly);

			ttbLine.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			ttbLine.OnLoaded();
			Assert(!ttbLine.ReadOnly);

			ttbLine.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			ttbLine.OnLoaded();
			Assert(ttbLine.ReadOnly);

			ttbLine.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			ttbLine.OnLoaded();
			Assert(ttbLine.ReadOnly);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var ttbLine = InvoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ICusAddInfoTypeSupporter supporter = ttbLine;
			supporter.AssertType(typeof(TTBCigar), CusAddInfoTypeAttribute.Codes.USTTBCigar);
			supporter.AssertType(typeof(TTBCOLAAndCertificate), CusAddInfoTypeAttribute.Codes.USTTBCOLAAndCertificate);
			supporter.AssertType(null, "ZZ!");

			var cigar = ttbLine.Cigars.AddNew();
			cigar.US_Quantity = 1000;
			cigar.US_UnitPrice = 0.3454m;

			var cola = ttbLine.COLAAndCertificates.AddNew();
			cola.US_ForeignCertificateCountry = Core.Constants.CountryCodes.NewZealand;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(cigar.PK);
			AssertEquals(typeof(TTBCigar), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(cola.PK);
			AssertEquals(typeof(TTBCOLAAndCertificate), addInfo.GetType());
		}

		public void TestUpdateTTBPermitNumber()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TTB Permit Number";
			org.MainAddress.OA_Address1 = "MAIN ADD 1";

			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.USACodeTypes.TTBPermitNumber;
			customsCode.OK_CustomsRegNo = "AA-A-123";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_OH_Importer = org.PK;
			declaration.IOROrgPK = org.PK;
			declaration.JE_MasterBill = "12599675660";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			AssertEquals("AA-A-123", ttbLine.US_PermitNumber);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = org.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_TTBIndicator = OGAIndicatorList.Codes.Declared;

			var productTTB = pivot.TTBLines.AddNew();
			productTTB.US_ProgramCode = "BBC";
			productTTB.US_PermitNumber = "BB-B-123";

			invoiceLine.JI_PartNo = "INCTEST";
			AssertEquals(1, invoiceLine.TTBLines.Count);
			AssertEquals("BB-B-123", invoiceLine.TTBLines[0].US_PermitNumber);

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals(0, invoiceLine.TTBLines.Count);

			customsCode.OK_CustomsRegNo = "AA-A-1234567890";
			productTTB.US_PermitNumber = ZString.Empty;
			invoiceLine.JI_PartNo = "INCTEST";
			AssertEquals(1, invoiceLine.TTBLines.Count);
			AssertEquals("AA-A-123456789", invoiceLine.TTBLines[0].US_PermitNumber);
		}

		public void TestClearBondDetailsIfNotNeeded()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "MAIN ADD 1";
			var ttbLine = InvoiceLine.TTBLines.AddNew();
			ttbLine.US_LineNo = 1;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttbLine.US_ProcessingCode = TTBWINProcessingCodeList.Codes.T08;
			ttbLine.US_IsReleaseUnderBond = ZBool.False;
			ttbLine.US_NumberForIRC = "234242";
			ttbLine.US_OA_ConsigneeAddress = org.MainAddress.PK;
			ttbLine.US_IsReleaseUnderBond = ZBool.True;
			AssertEquals("ConsigneeOrgPK", org.PK, ttbLine.ConsigneeOrgPK);
			AssertEquals("US_OA_ConsigneeAddress", org.MainAddress.PK, ttbLine.US_OA_ConsigneeAddress);
			AssertEquals("US_OA_ConsigneeAddressInfo.ReadOnly", false, ttbLine.US_OA_ConsigneeAddressInfo.ReadOnly);
			AssertEquals("US_NumberForIRC", "234242", ttbLine.US_NumberForIRC);
			AssertEquals("US_NumberForIRCInfo.ReadOnly", false, ttbLine.US_NumberForIRCInfo.ReadOnly);
			ttbLine.US_IsReleaseUnderBond = ZBool.False;
			AssertEquals("ConsigneeOrgPK", ZGuid.Empty, ttbLine.ConsigneeOrgPK);
			AssertEquals("US_OA_ConsigneeAddress", ZGuid.Empty, ttbLine.US_OA_ConsigneeAddress);
			AssertEquals("US_OA_ConsigneeAddressInfo.ReadOnly", true, ttbLine.US_OA_ConsigneeAddressInfo.ReadOnly);
			AssertEquals("US_NumberForIRC", ZString.Empty, ttbLine.US_NumberForIRC);
			AssertEquals("US_NumberForIRCInfo.ReadOnly", true, ttbLine.US_NumberForIRCInfo.ReadOnly);
		}

		public void TestClearCigarsOrCOLAAndCertificatesIfNotNeeded()
		{
			var ttbLine = InvoiceLine.TTBLines.AddNew();
			var cigar = ttbLine.Cigars.AddNew();
			cigar.US_Quantity = 1000;
			var cola = ttbLine.COLAAndCertificates.AddNew();
			cola.US_COLA = "123";
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			AssertEquals("ttbLine.Cigars.Count", 1, ttbLine.Cigars.Count);
			AssertEquals("ttbLine.COLAAndCertificates.Count", 0, ttbLine.COLAAndCertificates.Count);

			cola = ttbLine.COLAAndCertificates.AddNew();
			cola.US_COLA = "123";
			ttbLine.US_ProgramCode = ZString.Empty;
			AssertEquals("ttbLine.Cigars.Count", 0, ttbLine.Cigars.Count);
			AssertEquals("ttbLine.COLAAndCertificates.Count", 1, ttbLine.COLAAndCertificates.Count);

			var list = new TTBProgramCodeList();
			list.RemoveCode(TTBProgramCodeList.Codes.Tobacco);

			foreach (ICodeDescription pair in list)
			{
				cigar = ttbLine.Cigars.AddNew();
				cigar.US_Quantity = 1000;
				ttbLine.US_ProgramCode = pair.Code;
				AssertEquals("ttbLine.Cigars.Count", 0, ttbLine.Cigars.Count);
				AssertEquals("ttbLine.COLAAndCertificates.Count", 1, ttbLine.COLAAndCertificates.Count);
			}
		}

		public void TestTTIRegistrationNumber()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "MAIN ADD 1";
			ZString numberIRC = "DSP-CA-90210";
			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.TTIRegistrationNumber, numberIRC, Core.Constants.CountryCodes.UnitedStates);
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "ADD 2";

			var ttbLine = InvoiceLine.TTBLines.AddNew();
			ttbLine.US_LineNo = 1;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttbLine.US_ProcessingCode = TTBWINProcessingCodeList.Codes.T08;
			ttbLine.US_IsReleaseUnderBond = ZBool.True;
			ttbLine.US_OA_ConsigneeAddress = address2.PK;
			Assert("Excepted:IRC Number is empty", ttbLine.US_NumberForIRC.IsEmpty);
			ttbLine.US_OA_ConsigneeAddress = org.MainAddress.PK;
			Assert("Excepted:IRC Number is the default value", ttbLine.US_NumberForIRC.Equals(numberIRC));
		}

		public void TestITTBLineMembers()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "MAIN ADD 1";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "2342323", Core.Constants.CountryCodes.UnitedStates);
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "ADD 1";
			InvoiceLine.JI_Description = "Still Wine Not More Than 14% alcohol by volume";
			var ttbLine = InvoiceLine.TTBLines.AddNew();
			ttbLine.US_LineNo = 1;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttbLine.US_ProcessingCode = TTBWINProcessingCodeList.Codes.T08;
			ttbLine.US_IsReleaseUnderBond = ZBool.True;
			ttbLine.US_NumberForIRC = "234242";
			ttbLine.US_OA_ConsigneeAddress = address2.PK;
			ttbLine.US_PermitNumber = "KD323";
			ttbLine.US_PermitExemptionCode = TTBExemptionCodeList.Codes.TTBEX1;
			ttbLine.US_QuantityInPCS = 100m;

			var colaAndCertificate1 = ttbLine.COLAAndCertificates.AddNew();
			colaAndCertificate1.US_ForeignCertificateCountry = Core.Constants.CountryCodes.NewZealand;
			colaAndCertificate1.US_COLA = "11419999999999";

			var colaAndCertificate2 = ttbLine.COLAAndCertificates.AddNew();
			colaAndCertificate2.US_ForeignCertificateCountry = Core.Constants.CountryCodes.Mexico;
			colaAndCertificate2.US_COLA = "11419999999934";

			var cigar1 = ttbLine.Cigars.AddNew();
			cigar1.US_Quantity = 100;
			cigar1.US_UnitPrice = 43.534m;
			var cigar2 = ttbLine.Cigars.AddNew();
			cigar2.US_Quantity = 500;
			cigar2.US_UnitPrice = 98.534m;
			var cigar3 = ttbLine.Cigars.AddNew();
			cigar3.US_IsSmall = true;
			cigar3.US_Quantity = 300;

			ITTBLine line = ttbLine;
			AssertEquals("LineNo", 1, line.LineNo);
			AssertEquals("PGALineStatusProgramCode", TTBProgramCodeList.Codes.Wine, line.ProgramCode);
			AssertEquals("ProcessingCode", TTBWINProcessingCodeList.Codes.T08, line.ProcessingCode);
			AssertEquals("NumberForIRC", "234242", line.NumberForIRC);
			AssertEquals("Consignee", address2, line.Consignee);
			AssertEquals("ConsigneeEIN", "2342323", line.ConsigneeEIN);
			AssertEquals("PermitNumber", "KD323", line.PermitNumber);
			AssertEquals("ExemptionCode", TTBExemptionCodeList.Codes.TTBEX1, line.ExemptionCode);
			AssertEquals("QuantityInPCS", ZDecimal.Zero, line.QuantityInPCS);

			var colaAndCertificates = line.COLAAndCertificates.ToList();
			AssertEquals("COLAAndCertificates", 2, colaAndCertificates.Count);
			AssertCOLAAndCertificate(colaAndCertificates[0], Core.Constants.CountryCodes.NewZealand, "11419999999999");
			AssertCOLAAndCertificate(colaAndCertificates[1], Core.Constants.CountryCodes.Mexico, "11419999999934");

			var cigars = line.Cigars.ToList();
			AssertEquals("Cigars", 0, cigars.Count);

			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			ttbLine.US_QuantityInPCS = 100m;
			AssertEquals("QuantityInPCS", ZDecimal.Zero, line.QuantityInPCS);

			cigars = line.Cigars.ToList();
			AssertEquals("Cigars", 3, cigars.Count);
			AssertCigar(cigars[0], 100, 43.534m, false);
			AssertCigar(cigars[1], 500, 98.534m, false);
			AssertCigar(cigars[2], 300, 0m, true);

			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
			ttbLine.US_QuantityInPCS = 100m;
			AssertEquals("QuantityInPCS", 100m, line.QuantityInPCS);
		}

		public void TestTobaccoRelatedData()
		{
			var ttbLine = InvoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
			AssertEquals("ttbLine.US_QuantityInPCSInfo.ReadOnly", true, ttbLine.US_QuantityInPCSInfo.ReadOnly);
			AssertEquals("ttbLine.Cigars.AllowNew", false, ttbLine.Cigars.AllowNew);
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			AssertEquals("ttbLine.US_QuantityInPCSInfo.ReadOnly", false, ttbLine.US_QuantityInPCSInfo.ReadOnly);
			AssertEquals("ttbLine.Cigars.AllowNew", false, ttbLine.Cigars.AllowNew);
			ttbLine.US_QuantityInPCS = 10m;
			var cigar = ttbLine.Cigars.AddNew();
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T52;
			AssertEquals("ttbLine.US_QuantityInPCSInfo.ReadOnly", false, ttbLine.US_QuantityInPCSInfo.ReadOnly);
			AssertEquals("ttbLine.Cigars.AllowNew", false, ttbLine.Cigars.AllowNew);
			AssertEquals("ttbLine.US_QuantityInPCS", 10m, ttbLine.US_QuantityInPCS);
			AssertEquals("ttbLine.Cigars.Count", 0, ttbLine.Cigars.Count);
			cigar = ttbLine.Cigars.AddNew();
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T30;
			AssertEquals("ttbLine.US_QuantityInPCSInfo.ReadOnly", true, ttbLine.US_QuantityInPCSInfo.ReadOnly);
			AssertEquals("ttbLine.Cigars.AllowNew", true, ttbLine.Cigars.AllowNew);
			AssertEquals("ttbLine.US_QuantityInPCS", ZDecimal.Zero, ttbLine.US_QuantityInPCS);
			AssertEquals("ttbLine.Cigars.Count", 1, ttbLine.Cigars.Count);
			AssertEquals("cigar.IsDeleted", false, cigar.IsDeleted);
			ttbLine.US_QuantityInPCS = 10m;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T34;
			AssertEquals("ttbLine.US_QuantityInPCSInfo.ReadOnly", true, ttbLine.US_QuantityInPCSInfo.ReadOnly);
			AssertEquals("ttbLine.Cigars.AllowNew", true, ttbLine.Cigars.AllowNew);
			AssertEquals("ttbLine.US_QuantityInPCS", ZDecimal.Zero, ttbLine.US_QuantityInPCS);
			AssertEquals("ttbLine.Cigars.Count", 1, ttbLine.Cigars.Count);
			AssertEquals("cigar.IsDeleted", false, cigar.IsDeleted);
		}

		public void TestClone()
		{
			var ttbLine = InvoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.DistilledSpirits;
			ttbLine.US_IsReleaseUnderBond = ZBool.True;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T34;
			ttbLine.ConsigneeOrgPK = ZGuid.Empty;
			ttbLine.US_OA_ConsigneeAddress = ZGuid.Empty;
			ttbLine.US_NumberForIRC = "NUM";
			ttbLine.US_PermitNumber = "A12";
			ttbLine.US_PermitExemptionCode = TTBExemptionCodeList.Codes.TTBEX14;

			var cert = ttbLine.COLAAndCertificates.AddNew();
			cert.US_COLA = "CERT";
			cert.US_COLAExemptionCode = TTBExemptionCodeList.Codes.TTBEX7;
			cert.HasForeignCertificate = ZBool.True;
			cert.US_ForeignCertificateCountry = "AR";

			InvoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var ttbLineNew = InvoiceLine.TTBLines.AddNew();

			AssertEquals("PGALineStatusProgramCode must be equal", ttbLine.US_ProgramCode, ttbLineNew.US_ProgramCode);
			AssertEquals("IsReleaseUnderBond must be equal", ttbLine.US_IsReleaseUnderBond, ttbLineNew.US_IsReleaseUnderBond);
			AssertEquals("ProcessingCode must be equal", ttbLine.US_ProcessingCode, ttbLineNew.US_ProcessingCode);
			AssertEquals("ConsigneeOrgPK must be equal", ttbLine.ConsigneeOrgPK, ttbLineNew.ConsigneeOrgPK);
			AssertEquals("OA_ConsigneeAddress must be equal", ttbLine.US_OA_ConsigneeAddress, ttbLineNew.US_OA_ConsigneeAddress);
			AssertEquals("NumberForIRC must be equal", ttbLine.US_NumberForIRC, ttbLineNew.US_NumberForIRC);
			AssertEquals("PermitNumber must be equal", ttbLine.US_PermitNumber, ttbLineNew.US_PermitNumber);
			AssertEquals("PermitExemptionCode must be equal", ttbLine.US_PermitExemptionCode, ttbLineNew.US_PermitExemptionCode);

			AssertEquals("Certificates: COLA must be equal", ttbLine.COLAAndCertificates[0].US_COLA, ttbLineNew.COLAAndCertificates[0].US_COLA);
			AssertEquals("Certificates: COLAExemptionCode must be equal", ttbLine.COLAAndCertificates[0].US_COLAExemptionCode, ttbLineNew.COLAAndCertificates[0].US_COLAExemptionCode);
			AssertEquals("Certificates: HasForeignCertificate must be equal", ttbLine.COLAAndCertificates[0].HasForeignCertificate, ttbLineNew.COLAAndCertificates[0].HasForeignCertificate);
			AssertEquals("Certificates: ForeignCertificateCountry must be equal", ttbLine.COLAAndCertificates[0].US_ForeignCertificateCountry, ttbLineNew.COLAAndCertificates[0].US_ForeignCertificateCountry);
		}

		public void TestIAESTTBMembers()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var exportTTBLine = InvoiceLine.TTBLines.AddNew();
			exportTTBLine.US_NumberForIRC = "IRC NUM";
			exportTTBLine.US_Date = new ZDate(2016, 9, 22);
			exportTTBLine.US_SerialNumber = "1111";

			var aesTTB = (IAESTTB)exportTTBLine;
			AssertEquals("IRC NUM", aesTTB.IRCNumber);
			AssertEquals(new ZDate(2016, 9, 22), aesTTB.Date);
			AssertEquals("1111", aesTTB.SerialNumber);
			AssertEquals(ZString.Empty, aesTTB.Disclaimer);

			InvoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("1", aesTTB.Disclaimer);
		}

		public void TestUS_DateGetDataFromInvoiceLineExportDate()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var expectedDate = ZDateTime.Today;

			Declaration.US_DateOfExport = expectedDate;
			var exportTTBLine = InvoiceLine.TTBLines.AddNew();
			AssertEquals(expectedDate, exportTTBLine.US_Date);

			Declaration.US_DateOfExport = ZDateTime.Empty;
			exportTTBLine = InvoiceLine.TTBLines.AddNew();
			AssertEquals(ZDateTime.Empty, exportTTBLine.US_Date);

			Declaration.US_DateOfExport = ZDateTime.Empty;
			exportTTBLine = InvoiceLine.TTBLines.AddNew();
			exportTTBLine.US_Date = expectedDate;
			AssertEquals(expectedDate, exportTTBLine.US_Date);

			Declaration.US_DateOfExport = ZDateTime.BrettsBirthday;
			exportTTBLine = InvoiceLine.TTBLines.AddNew();
			exportTTBLine.US_Date = expectedDate;
			AssertEquals(expectedDate, exportTTBLine.US_Date);
		}

		public void TestUS_NumberForIRCGetDataFromTTENumber()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TTB Permit Number";
			org.MainAddress.OA_Address1 = "MAIN ADD 1";

			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.USACodeTypes.TTEPermitNumber;
			customsCode.OK_CustomsRegNo = "AA-A-123";

			InvoiceLine.InvoiceHeader.US_USPPI.ZO_OH_Organisation = org.PK;

			customsCode.OK_CustomsRegNo = "AA-A-123";
			var exportTTBLine = InvoiceLine.TTBLines.AddNew();
			AssertEquals("AA-A-123", exportTTBLine.US_NumberForIRC);

			customsCode.OK_CustomsRegNo = ZString.Empty;
			exportTTBLine = InvoiceLine.TTBLines.AddNew();
			AssertEquals(ZString.Empty, exportTTBLine.US_NumberForIRC);

			customsCode.OK_CustomsRegNo = ZString.Empty;
			exportTTBLine = InvoiceLine.TTBLines.AddNew();
			exportTTBLine.US_NumberForIRC = "AA-A-888";
			AssertEquals("AA-A-888", exportTTBLine.US_NumberForIRC);

			customsCode.OK_CustomsRegNo = "AA-A-123";
			exportTTBLine = InvoiceLine.TTBLines.AddNew();
			exportTTBLine.US_NumberForIRC = "AA-A-888";
			AssertEquals("AA-A-888", exportTTBLine.US_NumberForIRC);
		}

		public void TestIsExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoideLineTTB = invoiceLine.TTBLines.AddNew();
			AssertEquals(false, invoideLineTTB.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, invoideLineTTB.IsExport);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productTTB = pivot.TTBLines.AddNew();
			AssertEquals(false, productTTB.IsExport);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(true, productTTB.IsExport);
		}
		#region Implementation

		void AssertCOLAAndCertificate(ITTBCOLAAndCertificate colaAndCertificate, ZString country, ZString cola)
		{
			AssertEquals("colaAndCertificate.ForeignCertificateCountry", country, colaAndCertificate.ForeignCertificateCountry);
			AssertEquals("colaAndCertificate.COLA", cola, colaAndCertificate.COLA);
		}

		void AssertCigar(ITTBCigar cigar, ZInt quantity, ZDecimal unitPrice, ZBool isSmall)
		{
			AssertEquals("IsSmall", isSmall, cigar.IsSmall);
			AssertEquals("Quantity", quantity, cigar.Quantity);
			AssertEquals("UnitPrice", unitPrice, cigar.UnitPrice);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			return ttbLine;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLine.TTBLines.AddNew();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_EnableCRL = true;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
