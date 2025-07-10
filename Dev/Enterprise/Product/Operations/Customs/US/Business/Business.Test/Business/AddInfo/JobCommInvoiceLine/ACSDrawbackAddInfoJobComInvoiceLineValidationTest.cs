using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACSDrawbackAddInfoJobComInvoiceLineValidationTest : CommonDrawbackAddInfoJobComInvoiceLineValidationTest
	{
		public override void TestCheckUS_ImportEntryNo()
		{
			base.TestCheckUS_ImportEntryNo();
			InvoiceLine.US_DRWCertOfManufacture = "CM123456";
			InvoiceLine.US_ImportEntryNo = "ABC12345678";
			AssertHasMessageErrorContaining(InvoiceLine.US_ImportEntryNoInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ImportEntryOrCMEntered);
			InvoiceLine.US_DRWCertOfManufacture = "";
			AssertNoMessageErrorContaining(InvoiceLine.US_ImportEntryNoInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ImportEntryOrCMEntered);
		}

		public void TestCheckUS_DRWPort()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2407", "Test Name", startDate, endDate);
			newFactory.Save();

			MakeImportLine();
			InvoiceLine.US_DRWPort = "";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWPort();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWPortInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWPort = "2407";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWPortInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWPort = "XXXX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWPortInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_ImportEntryNo = "";
			InvoiceLine.US_DRWCertOfManufacture = "CM123456";
			InvoiceLine.US_DRWPort = "2407";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWPortInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.InvalidCMPort);
			InvoiceLine.US_DRWPort = "0401";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWPortInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.InvalidCMPort);
			foreach (string vaildCMPort in InvoiceLine.Lookups.ValidCMPorts)
			{
				InvoiceLine.US_DRWPort = vaildCMPort;
				AssertNoMessageErrorContaining(InvoiceLine.US_DRWPortInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.InvalidCMPort);
			}
		}

		public void TestCheckUS_DRWCMCDIndicator()
		{
			MakeImportLine();
			InvoiceLine.US_ImportEntryNo = "ABC12345678";
			InvoiceLine.US_DRWCMCDIndicator = "";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWCMCDIndicator();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWCMCDIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWCMCDIndicatorInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWCMCDIndicator = "?";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWCMCDIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWCMCDIndicatorInfo, ListValidation.InvalidCodeMessageError);
			CodeDescriptionPairList list = InvoiceLine.Lookups.DRWCMCDIndicatorCodeList;
			foreach (CodeDescriptionPair pair in list)
			{
				InvoiceLine.US_DRWCMCDIndicator = pair.Code;
				AssertNoMessageErrorContaining(InvoiceLine.US_DRWCMCDIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(InvoiceLine.US_DRWCMCDIndicatorInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_DRWCertOfManufacture()
		{
			MakeImportLine();
			InvoiceLine.US_DRWCertOfManufacture = "CM12345";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.CertificateOfManufactureInvalid);
			InvoiceLine.US_DRWCertOfManufacture = "CM123456";
			AssertNoWarningContaining(InvoiceLine.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.CertificateOfManufactureInvalid);
			InvoiceLine.US_DRWCertOfManufacture = "";
			AssertNoWarningContaining(InvoiceLine.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.CertificateOfManufactureInvalid);
			InvoiceLine.US_DRWCertOfManufacture = "CX123456";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.CertificateOfManufactureInvalid);
			InvoiceLine.US_DRWCertOfManufacture = "CM12X456";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.CertificateOfManufactureInvalid);
			InvoiceLine.US_ImportEntryNo = "ABC12345678";
			InvoiceLine.US_DRWCertOfManufacture = "CM123456";
			AssertHasMessageErrorContaining(InvoiceLine.US_ImportEntryNoInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ImportEntryOrCMEntered);
			InvoiceLine.US_ImportEntryNo = "";
			AssertNoMessageErrorContaining(InvoiceLine.US_ImportEntryNoInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ImportEntryOrCMEntered);
		}

		public void TestCheckCheckUS_DRWDateRcvFrom()
		{
			MakeImportLine();
			declaration.US_DRWSection = "xxxx1313(b)xxxxx";
			InvoiceLine.US_DRWDateRcvFrom = ZDateTime.Empty;
			InvoiceLine.US_DRWDateRcvTo = ZDateTime.Empty;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateRcvTo = new ZDateTime(2008, 12, 1);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateRcvToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateRcvFrom = new ZDateTime(2008, 11, 30);
			InvoiceLine.US_DRWDateRcvTo = ZDateTime.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateRcvTo = new ZDateTime(2008, 12, 1);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateRcvFrom = new ZDateTime(2008, 12, 2);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateRcvToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateRcvFrom = new ZDateTime(2008, 11, 29);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			declaration.US_DRWSection = "xxxx1313(c)xxxxx";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvTo();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DatesMayNotBeEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateRcvToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DatesMayNotBeEntered);
			declaration.US_DRWSection = "xxxx1313(B)xxxxx";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvTo();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DatesMayNotBeEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DatesMayNotBeEntered);
		}

		public void TestCheckCheckUS_DRWDateUsedFrom()
		{
			MakeImportLine();
			declaration.US_DRWSection = "xxxx1313(b)xxxxx";
			InvoiceLine.US_DRWDateUsedFrom = ZDateTime.Empty;
			InvoiceLine.US_DRWDateUsedTo = ZDateTime.Empty;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateUsedFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateUsedTo = new ZDateTime(2008, 12, 1);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateUsedToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateUsedFrom = new ZDateTime(2008, 11, 30);
			InvoiceLine.US_DRWDateUsedTo = ZDateTime.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateUsedTo = new ZDateTime(2008, 12, 1);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateUsedFrom = new ZDateTime(2008, 12, 2);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateUsedToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateUsedFrom = new ZDateTime(2008, 11, 29);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			declaration.US_DRWSection = "xxxx1313(c)xxxxx";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateUsedTo();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DatesMayNotBeEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateUsedToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DatesMayNotBeEntered);
		}

		public void TestCheckCheckUS_DRWDateDelFrom()
		{
			MakeImportLine();
			declaration.US_DRWSection = "xxxx1313(b)xxxxx";
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			InvoiceLine.US_DRWDateDelFrom = ZDateTime.Empty;
			InvoiceLine.US_DRWDateDelTo = ZDateTime.Empty;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateDelFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateDelFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateDelToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateDelTo = new ZDateTime(2008, 12, 1);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateDelFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateDelToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateDelFrom = new ZDateTime(2008, 11, 30);
			InvoiceLine.US_DRWDateDelTo = ZDateTime.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateDelFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateDelToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateDelTo = new ZDateTime(2008, 12, 1);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateDelFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateDelToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateDelFrom = new ZDateTime(2008, 12, 2);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateDelFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateDelToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			InvoiceLine.US_DRWDateDelFrom = new ZDateTime(2008, 11, 29);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateDelFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateDelToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.ToDateBeforeFromDate);
			declaration.US_DRWSection = "xxxx1313(c)xxxxx";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateDelTo();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateDelFromInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DatesMayNotBeEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateDelToInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DatesMayNotBeEntered);
		}

		public void TestCheckCheckUS_DRWExportID()
		{
			MakeExportLine();
			InvoiceLine.US_DRWExportID = "123456";
			AssertNoMessageErrors(InvoiceLine.US_DRWExportIDInfo);
			InvoiceLine.US_DRWExportID = "";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCheckUS_DRWExportDest()
		{
			MakeExportLine();
			InvoiceLine.US_DRWExportDest = "CA";
			AssertNoMessageErrors(InvoiceLine.US_DRWExportDestInfo);
			InvoiceLine.US_DRWExportDest = "";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWExportDest = "??";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWExportAction = "D";
			InvoiceLine.US_DRWExportDest = "";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWCDUse()
		{
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
			InvoiceLine.US_DRWCDUse = CDCMDUseValueCodeList.Codes.CI;
			AssertNoMessageError(InvoiceLine.US_DRWCDUseInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWCDUse = "C1";
			AssertHasMessageError(InvoiceLine.US_DRWCDUseInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWImportEntryLine()
		{
			MakeImportLine();
			InvoiceLine.US_DRWImportEntryLine = 99999;
			AssertHasErrorContaining(InvoiceLine.US_DRWImportEntryLineInfo, ACSDrawbackAddInfoJobComInvoiceLineValidation.DRWImportEntryLineLength);
		}
	}
}
