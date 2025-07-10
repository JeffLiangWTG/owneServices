using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LicenceAndPermitValidationTest : TestCaseWithFactory
	{
		public void TestPermitCottonCertificateNoWith22()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._22, "Organic Product Exemption Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "3333333333";
			tariff.UE_DateFrom = startDate;
			tariff.UE_DateTo = endDate;

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			dutyRate.UD_TaxFeeFlag = "1";

			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			var importer = Factory.New<OrgHeader>();
			var nafDoc = importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Cotton);
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "TS";
			declaration.DoMerge();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			nafDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(10);
			nafDoc.EQ_DocNumber = "123456789";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._22;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The cotton certificate  format is not valid", permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			AssertNoMessageError("Don't match the document number", permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MatchCottonCertificateOnImporter);

			permit.CY_Data = "333444555";
			AssertNoMessageError("The cotton certificate  format is not valid", permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			AssertHasMessageError("Don't match the document number", permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MatchCottonCertificateOnImporter);

			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Avocado;
			permit.Validation.ValidateCY_Data();
			AssertNoMessageError("The cotton certificate  format is not valid", permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			AssertNoMessageError("Don't match the document number", permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MatchCottonCertificateOnImporter);

			permit.CY_Data = "3334445556";
			AssertHasMessageError("The cotton certificate  format is not valid", permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			AssertNoMessageError("Don't match the document number", permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MatchCottonCertificateOnImporter);

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			permit.Validation.ValidateCY_Data();
			AssertHasMessageError("The cotton certificate  format is not valid", permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			AssertNoMessageError("Don't match the document number", permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MatchCottonCertificateOnImporter);

			permit.CY_Data = "333444555";
			AssertNoMessageError("The cotton certificate  format is not valid", permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			AssertNoMessageError("Don't match the document number", permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MatchCottonCertificateOnImporter);

			permit.CY_Data = "123456789";
			AssertNoMessageError("The cotton certificate  format is not valid", permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			AssertNoMessageError("The cotton certificate is valid", permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCottonCertificate);

			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			nafDoc.EQ_ValidToDate = CargoWise.Types.ZDateTime.Now.AddDays(-10);
			permit.CY_Data = "123456789";
			AssertHasMessageError("Don't match the document number", permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCottonCertificate);

			permit.CY_Data = "123456";
			AssertHasMessageError(permit.CY_DataInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MatchCottonCertificateOnImporter);
		}

		[TestDate(2010, 01, 01)]
		public void TestPermit01()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._01, "Steel Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.US_SupTariff = "99106179";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._01;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Steel Import License number is invalid.");
			permit.CY_Data = "1SG123456";
			AssertNoMessageError(permit.CY_DataInfo, "The Steel Import License number is invalid.");
		}

		[TestDate(2009, 1, 1)]
		public void TestPermit02()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._02, "Singapore TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\dSG[13]\d{5}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.US_SupTariff = "99106179";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._02;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Singapore TPL Certificate number is invalid.");
			permit.CY_Data = "1SG123456";
			AssertNoMessageError(permit.CY_DataInfo, "The Singapore TPL Certificate number is invalid.");
		}

		public void TestPermit03()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._03, "Canadian NAFTA TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\dCA\d{6}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.US_SupTariff = "99990050";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._03;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Canadian NAFTA TPL Certificate number is invalid.");
			permit.CY_Data = "1CA123456";
			AssertNoMessageError(permit.CY_DataInfo, "The Canadian NAFTA TPL Certificate number is invalid.");
		}

		public void TestPermit04()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._04, "Mexican NAFTA TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\dMX\d{6}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.US_SupTariff = "99990062";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._04;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Mexican NAFTA TPL Certificate number is invalid.");
			permit.CY_Data = "1MX123456";
			AssertNoMessageError(permit.CY_DataInfo, "The Mexican NAFTA TPL Certificate number is invalid.");
		}

		public void TestPermit05()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._05, "Beef Export Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.JI_Tariff = "7102214000";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._05;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Beef Export Certificate number is invalid.");
			permit.CY_Data = "CG0000123";
			AssertNoMessageErrors(permit.CY_DataInfo);
			AssertNoMessageError(permit.CY_DataInfo, "The Beef Export Certificate number is invalid.");
		}

		public void TestPermit06()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._06, "Diamond Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.JI_Tariff = "7102214000";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._06;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Diamond Certificate number is invalid.");
			permit.CY_Data = "CG0000123";
			AssertNoMessageErrors(permit.CY_DataInfo);
			AssertNoMessageError(permit.CY_DataInfo, "The Diamond Certificate number is invalid.");
		}

		public void TestPermit07()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._07, "Andean Trade Partnership Drug Eradication Act (ATPDEA) Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.JI_Tariff = "7102214000";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._07;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Andean Trade Partnership Drug Eradication Act (ATPDEA) Certificate number is invalid.");
			permit.CY_Data = "CG0000123";
			AssertNoMessageErrors(permit.CY_DataInfo);
			AssertNoMessageError(permit.CY_DataInfo, "The Andean Trade Partnership Drug Eradication Act (ATPDEA) Certificate number is invalid.");
		}

		[TestDate(2009, 1, 1)]
		public void TestPermit08()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._08, "Australia Free Trade Export Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"[0-9]AU[A-Z0-9]{6}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.US_SupTariff = "99130440";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._08;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Australia Free Trade Export Certificate number is invalid.");
			permit.CY_Data = "1AU123456";
			AssertNoMessageErrors(permit.CY_DataInfo);
			AssertNoMessageError(permit.CY_DataInfo, "The Australia Free Trade Export Certificate number is invalid.");
		}

		public void TestPermit09()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._09, "Mexican Cement Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"CEM\d{6}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.JI_Tariff = "2523290000";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._09;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Mexican Cement Import License number is invalid.");
			permit.CY_Data = "CEM123456";
			AssertNoMessageErrors(permit.CY_DataInfo);
			AssertNoMessageError(permit.CY_DataInfo, "The Mexican Cement Import License number is invalid.");
		}

		public void TestPermit10()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._10, "CAFTA TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\dNI[A-Z0-9]{6}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.US_SupTariff = "99156101";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._10;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The CAFTA TPL Certificate number is invalid.");
			permit.CY_Data = "1NI123456";
			AssertNoMessageErrors(permit.CY_DataInfo);
			AssertNoMessageError(permit.CY_DataInfo, "The CAFTA TPL Certificate number is invalid.");
		}

		public void TestPermit11()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._11, "Atlantic Lumber Board (ALB) Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._11;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Atlantic Lumber Board (ALB) Certificate number is invalid.");
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			permit.CY_Data = "123456789";
			AssertNoMessageErrors(permit.CY_DataInfo);
		}

		public void TestPermit12()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._12, "Cotton Shirting Fabric License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._12;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Cotton Shirting Fabric License number is invalid.");
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			permit.CY_Data = "123456789";
			AssertNoMessageErrors(permit.CY_DataInfo);
		}

		public void TestPermit13()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._13, "Haiti Earned Import Allowance", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._13;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Haiti Earned Import Allowance number is invalid.");
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			permit.CY_Data = "123456789";
			AssertNoMessageErrors(permit.CY_DataInfo);
		}

		public void TestPermit14()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._14, "Agricultural License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"[0-9]{1}-([a-z]{2}|[a-z]{1}\s)-[0-9]{3}-[0-9]{1}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._14;
			permit.CY_Data = "1423456789";
			AssertHasMessageError("The format shoud be N-AA-NNN-N or N-AB-NNN-N", permit.CY_DataInfo, "The Agricultural License number is invalid.");
			permit.CY_Data = "3-A -777-7";
			AssertNoMessageError("The format shoud be N-AA-NNN-N or N-AB-NNN-N", permit.CY_DataInfo, "The Agricultural License number is invalid.");
			permit.CY_Data = "3-AU-777-7";
			AssertNoMessageError("The format shoud be N-AA-NNN-N or N-AB-NNN-N", permit.CY_DataInfo, "The Agricultural License number is invalid.");
		}

		public void TestPermit19()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._19, "African Growth and Opportunity Act (AGOA) Textile Provision", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._19;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The African Growth and Opportunity Act (AGOA) Textile Provision number is invalid.");
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			permit.CY_Data = "123456789";
			AssertNoMessageErrors(permit.CY_DataInfo);
		}

		public void TestPermit21()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._21, "USDA Sugar Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._21;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The USDA Sugar Certificate number is invalid.");
			permit.CY_Data = "A3AAAX555";
			AssertNoMessageError(permit.CY_DataInfo, "The USDA Sugar Certificate number is invalid.");
			permit.CY_Data = "A3AAA8555";
			AssertNoMessageError(permit.CY_DataInfo, "The USDA Sugar Certificate number is invalid.");
		}

		public void TestPermit25()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._25, "Dominican Republic Earned Allowance Program Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._25;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Dominican Republic Earned Allowance Program Certificate number is invalid.");
			permit.CY_Data = "123456789";
			AssertNoMessageErrors(permit.CY_DataInfo);
		}

		public void TestPermit26()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._26, "Mexican Sugar Export License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"[A-Z]{2}[0-9]{7}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._26;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Mexican Sugar Export License number is invalid.");
			permit.CY_Data = "AB3456789";
			AssertNoMessageErrors(permit.CY_DataInfo);
		}

		public void TestPermit16()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._16, "Canadian Export Sugar Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,8}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._16;
			permit.CY_Data = "~";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Canadian Export Sugar Certificate number is invalid.");
			permit.CY_Data = "1";
			AssertNoMessageError(permit.CY_DataInfo, "The Canadian Export Sugar Certificate number is invalid.");
			permit.CY_Data = "123456";
			AssertNoMessageError(permit.CY_DataInfo, "The Canadian Export Sugar Certificate number is invalid.");
			permit.CY_Data = "12345678";
			AssertNoMessageError(permit.CY_DataInfo, "The Canadian Export Sugar Certificate number is invalid.");
			permit.CY_Data = "ABCRREFG";
			AssertNoMessageError(permit.CY_DataInfo, "The Canadian Export Sugar Certificate number is invalid.");
		}

		public void TestPermit27()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._27, "General Note 15 (c) Waiver Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._27;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The General Note 15 (c) Waiver Certificate number is invalid.");
			permit.CY_Data = "123456789";
			AssertNoMessageError(permit.CY_DataInfo, "The General Note 15 (c) Waiver Certificate number is invalid.");
		}

		public void TestPermit28()
		{
			ValidateMaxiumLengthOrLessAndMiscPermitNoIsNotRequired(LicencePermitTypeList.Codes._28, "Aluminum Import License", @"\w{1,9}");
		}

		public void TestPermit29()
		{
			ValidateMaxiumLengthOrLessAndMiscPermitNoIsNotRequired(LicencePermitTypeList.Codes._29, "Canadian USMCA TPL Certificate", @"\w{1,9}");
		}

		public void TestPermit30()
		{
			ValidateMaxiumLengthOrLessAndMiscPermitNoIsNotRequired(LicencePermitTypeList.Codes._30, "Mexican USMCA TPL Certificate", @"\w{1,9}");
		}

		public void TestOrganicOrAMSCertificate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._22, "Organic Product Exemption Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,9}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100148";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._22;
			permit.CY_Data = "3334445551";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			permit.CY_Data = "123456780";
			permit.Validation.ValidateCY_Data();
			AssertNoMessageError(permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
			permit.CY_Data = "A12-45-78";
			AssertHasMessageError(permit.CY_DataInfo, "The Organic Product Exemption Certificate number is invalid.");
		}

		public void TestLicenseAndPermitAgainstXline()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._10, "CAFTA TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\dNI[A-Z0-9]{6}");
			Factory.Save();

			var invoiceLine = ACEInvoiceLine;
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			var line1 = invoiceLine.LicenceAndPermits.AddNew();
			line1.CY_Code = LicencePermitTypeList.Codes._10;
			line1.Validation.ValidateCY_Data();
			AssertHasRowMessageError(line1, (string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit)));
			invoiceLine.LicenceAndPermits.RemoveAndDeleteAll();
			line1 = invoiceLine.LicenceAndPermits.AddNew();
			line1.CY_Data = "1234";
			AssertHasRowMessageError(line1, (string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit)));
			invoiceLine.US_SetInd = ZString.Empty;
			line1.CY_Code = LicencePermitTypeList.Codes._10;
			line1.CY_Data = "3334445551";
			line1.Validation.ValidateCY_Data();
			AssertHasMessageError("The format is not valid", line1.CY_DataInfo, "The CAFTA TPL Certificate number is invalid.");
		}

		[TestDate(2015, 01, 12)]
		public void TestCheckUS_MiscPermitNoAndCheckCY_Data()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "7221000075";
			tariff.UE_DateFrom = ZDateTime.Now.AddYears(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(2);
			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_Tariff = "7221000075";
			tariffRule2.U1_RuleCode = TariffRuleList.Codes.EligibleForAGOATextileClaims;
			tariffRule2.U1_DateFrom = ZDateTime.Now.AddYears(-1);
			tariffRule2.U1_DateTo = ZDateTime.Now.AddYears(1);
			Factory.Save();
			var invoiceLine = ACEInvoiceLine;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_MiscPermitNo = "123";
			invoiceLine.US_SPI = "MX";
			Factory.Save();
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._10;
			permit.CY_Data = "ANI123456";
			AssertHasWarnings(FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoIsNotRequired, permit.CY_DataInfo);
		}

		public void TestPermit19WithAGOATextileClaims()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98191103";
			tariff.UE_DateFrom = ZDateTime.Now.AddYears(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(2);
			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_Tariff = "98191103";
			tariffRule2.U1_DateFrom = ZDateTime.Now.AddYears(-1);
			tariffRule2.U1_DateTo = ZDateTime.Now.AddYears(1);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertNotNull(invoiceLine.ImportSupTariff);
			AssertEquals("PreCondition:EligibleForAGOA", true, invoiceLine.ImportSupTariff.IsEligibleForAGOATextileBenefits(invoiceLine.EffectiveDateForDutyRate));
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._19;
			permit.CY_Data = "1";
			AssertNoMessageError(permit.CY_DataInfo, LicenceAndPermitValidation.PleaseEnterANumber);
			invoiceLine.US_SupTariff = "1";
			permit.CY_Data = "";
			AssertHasMessageError(permit.CY_DataInfo, LicenceAndPermitValidation.PleaseEnterANumber);
		}

		public void TestRegexPatternForLicensePermit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var testCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "99", "TEST code", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeListAttribute(testCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, "^\\w{1,6}$");
			helper.CreateNewOrGetExistingCusCodeListAttribute(testCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatErrorText, "1 to 6 alpha-numeric characters");
			Factory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9876543210";
			tariff.UE_DateFrom = ZDateTime.Now.AddYears(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(2);
			tariff.UE_PermitLicenseIndicator = "99";
			Factory.Save();
			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "9876543210";

			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = "99";
			permit.CY_Data = "1234567";
			AssertHasMessageError(permit.CY_DataInfo, "The TEST code number is invalid. The format should be 1 to 6 alpha-numeric characters.");

			permit.CY_Data = "123456";
			AssertNoMessageErrors(permit.CY_DataInfo);
		}

		void ValidateMaxiumLengthOrLessAndMiscPermitNoIsNotRequired(string code, string description, string formatMask)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var licenseCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(licenseCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, formatMask);
			Factory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9876543210";
			tariff.UE_DateFrom = ZDateTime.Now.AddYears(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(2);
			Factory.Save();
			var invoiceLine = ACEInvoiceLine;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "9876543210";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = code;
			permit.CY_Data = "3334445556";
			AssertHasMessageError("The format is not valid", permit.CY_DataInfo, $"The {description} number is invalid.");
			permit.CY_Data = "123456789";
			AssertNoMessageErrors(permit.CY_DataInfo);
			AssertHasWarnings(FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoIsNotRequired, permit.CY_DataInfo);
			tariff.UE_PermitLicenseIndicator = "XX";
			Factory.Save();
			permit.Validation.ValidateCY_Data();
			AssertNoWarnings(FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoIsNotRequired, permit.CY_DataInfo);
		}

		JobComInvoiceLine ACEInvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					var invoice = declaration.Invoices.AddNew();
					invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "4412997000";
				}

				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;
	}
}
