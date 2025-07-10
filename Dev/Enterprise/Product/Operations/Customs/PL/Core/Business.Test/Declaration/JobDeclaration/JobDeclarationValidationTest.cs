using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class JobDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
{
	public void TestDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(declaration.Validation.Declaration, declaration);
	}

	public void TestValidateAll()
	{
		var validationMock = new Mock<JobDeclarationValidation>(jobDeclaration);
		validationMock.Setup(x => x.ValidateAll()).CallBase();
		var validation = validationMock.Object;
		validation.ValidateAll();
		validationMock.Protected().Verify("CheckJE_OfficeOfEntryExit", Times.Once());
		Assert(true);
	}

	public void TestCheckJE_CustomsOffice()
	{
		var msg_invalid = ListValidation.InvalidCodeMessageError;

		CombineAssertions(() =>
		{
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.JE_CustomsOffice = "invld_Cod1";
			AssertHasMessageError("Import, invalid message", jobDeclaration.JE_CustomsOfficeInfo, msg_invalid);
			jobDeclaration.JE_CustomsOffice = ZString.Empty;
			AssertEquals("Import, only one notification if empty", 1, jobDeclaration.JE_CustomsOfficeInfo.Notifications.Count());
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.JE_CustomsOffice = "invld_Cod2";
			AssertHasMessageError("Export", jobDeclaration.JE_CustomsOfficeInfo, msg_invalid);
			jobDeclaration.JE_CustomsOffice = ZString.Empty;
			AssertEquals("Export, only one notification if empty", 1, jobDeclaration.JE_CustomsOfficeInfo.Notifications.Count());
		});
	}

	public void TestCheckJE_GoodsDestination()
	{
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceline1 = invoiceHeader.JobComInvoiceLines.AddNew();
		var invoiceline2 = invoiceHeader.JobComInvoiceLines.AddNew();

		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		invoiceline1.ZG_CountryOfDestination = "PL";
		invoiceline2.ZG_CountryOfDestination = "PL";
		jobDeclaration.JE_GoodsDestination = "DE";
		AssertNoMessageError("All Inv. Line destinations are the same and different from Declaration destination, but it's an Import Declaration.", jobDeclaration.JE_GoodsDestinationInfo, "The Countries of Destination for Inv.Lines are different from the Declaration destination country.");

		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		invoiceline1.ZG_CountryOfDestination = "PL";
		invoiceline2.ZG_CountryOfDestination = "PL";
		jobDeclaration.JE_GoodsDestination = "DE";
		AssertHasMessageError("All Inv. Line destinations are the same and different from the Export Declaration destination.", jobDeclaration.JE_GoodsDestinationInfo, "The Countries of Destination for Inv.Lines are different from the Declaration destination country.");

		invoiceline1.ZG_CountryOfDestination = "PL";
		invoiceline2.ZG_CountryOfDestination = "PL";
		jobDeclaration.JE_GoodsDestination = "PL";
		AssertNoMessageError("Inv. Line destinations are the same as the Declaration destination.", jobDeclaration.JE_GoodsDestinationInfo, "The Countries of Destination for Inv.Lines are different from the Declaration destination country.");

		invoiceline1.ZG_CountryOfDestination = "AL";
		invoiceline2.ZG_CountryOfDestination = "PL";
		jobDeclaration.JE_GoodsDestination = "PL";
		AssertNoMessageError("Inv. Line destinations are not all the same.", jobDeclaration.JE_GoodsDestinationInfo, "The Countries of Destination for Inv.Lines are different from the Declaration destination country.");
	}

	public void TestCheckJE_OfficeOfEntryExit() => CombineAssertions(() =>
	{
		var invalidCodeErrorMessage = "The code you have selected is not in the list.";

		jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		jobDeclaration.JE_OfficeOfEntryExit = ZString.Empty;
		AssertHasMessageErrorContaining("Import Declaration, Empty Code", jobDeclaration.JE_OfficeOfEntryExitInfo, MandatoryValidation.YouHaveNotEntered);
		jobDeclaration.JE_OfficeOfEntryExit = "invld_Code";
		AssertHasMessageError("Import Declaration, Invalid Code", jobDeclaration.JE_OfficeOfEntryExitInfo, invalidCodeErrorMessage);

		jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		jobDeclaration.JE_OfficeOfEntryExit = ZString.Empty;
		AssertHasMessageErrorContaining("Export Declaration, Empty Code", jobDeclaration.JE_OfficeOfEntryExitInfo, MandatoryValidation.YouHaveNotEntered);
		jobDeclaration.JE_OfficeOfEntryExit = "invld_Code";
		AssertHasMessageError("Export Declaration, Invalid Code", jobDeclaration.JE_OfficeOfEntryExitInfo, invalidCodeErrorMessage);

		jobDeclaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		jobDeclaration.JE_OfficeOfEntryExit = ZString.Empty;
		AssertNoMessageErrorContaining("Exit Summary Declaration, Empty Code", jobDeclaration.JE_OfficeOfEntryExitInfo, MandatoryValidation.YouHaveNotEntered);
		jobDeclaration.JE_OfficeOfEntryExit = "invld_Code";
		AssertHasMessageError("Exit Summary Declaration, Invalid Code", jobDeclaration.JE_OfficeOfEntryExitInfo, invalidCodeErrorMessage);

		jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		jobDeclaration.JE_OfficeOfEntryExit = ZString.Empty;
		AssertNoMessageErrorContaining("Miscellaneous Declaration, Empty Code", jobDeclaration.JE_OfficeOfEntryExitInfo, MandatoryValidation.YouHaveNotEntered);
		jobDeclaration.JE_OfficeOfEntryExit = "invld_Code";
		AssertHasMessageError("Miscellaneous Declaration, Invalid Code", jobDeclaration.JE_OfficeOfEntryExitInfo, invalidCodeErrorMessage);
	});

	public void TestCheckJE_ContainerMode()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.JE_ContainerMode = "AAA";
			AssertHasMessageError("ivalid code", jobDeclaration.JE_ContainerModeInfo, ListValidation.InvalidCodeMessageError);

			jobDeclaration.JE_ContainerMode = "BBK";
			AssertNoNotifications("valid code", jobDeclaration.JE_ContainerModeInfo);
		});
	}

	public void TestCheckJE_ApplicationCode()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.JE_ApplicationCode = "AHA";
			AssertHasMessageError("invalid code", jobDeclaration.JE_ApplicationCodeInfo, ListValidation.InvalidCodeMessageError);

			jobDeclaration.JE_ApplicationCode = "BLT";
			AssertNoNotifications("valid code", jobDeclaration.JE_ApplicationCodeInfo);
		});
	}

	public void TestCheckRuleR530()
	{
		var errorMsg = "Can contain only digits 0 to 9, letters from a to Z and signs '-', '_', '.', '@', '#', '/', '\\', '='.";

		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

		jobDeclaration.JE_UCR = string.Empty;
		AssertNoMessageError(jobDeclaration.JE_UCRInfo, errorMsg);

		jobDeclaration.JE_UCR = "0123456789";
		AssertNoNotifications(jobDeclaration.JE_UCRInfo);

		jobDeclaration.JE_UCR = "-_.@#/\\=";
		AssertNoNotifications(jobDeclaration.JE_UCRInfo);

		jobDeclaration.JE_UCR = "qwertyuiopasdfghjklzxcvbnm";
		AssertNoNotifications(jobDeclaration.JE_UCRInfo);

		jobDeclaration.JE_UCR = "QWERTYUIOPASDFGHJKLZXCVBNM";
		AssertNoNotifications(jobDeclaration.JE_UCRInfo);

		jobDeclaration.JE_UCR = "abc!";
		AssertHasMessageError(jobDeclaration.JE_UCRInfo, errorMsg);
	}

	public void TestCheckJE_OA_DeclarantAddress()
	{
		var eoriMessageError = "Selected Organization doesn't have a valid EOR number.";

		var declarantAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();

		declarantAddress.OA_OH = orgHeader.PK;
		declarantAddress.OA_Address1 = "Some street";

		var orgCusCodeSrt = Factory.New<OrgCusCode>();
		orgCusCodeSrt.OK_OA_PremisesAddress = declarantAddress.PK;
		orgCusCodeSrt.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCodeSrt.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		orgCusCodeSrt.OK_OH = declarantAddress.OA_OH;

		orgHeader.CustomsCodes.Add(orgCusCodeSrt);
		CombineAssertions(() =>
		{
			jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNoMessageError("Empty Declaration - EORI", jobDeclaration.JE_OA_DeclarantAddressInfo, eoriMessageError);

			jobDeclaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			AssertHasMessageError("No EORI", jobDeclaration.JE_OA_DeclarantAddressInfo, eoriMessageError);

			orgCusCodeSrt.OK_CustomsRegNo = "12345";
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError("orgCusCode is not EORI", jobDeclaration.JE_OA_DeclarantAddressInfo, eoriMessageError);

			orgCusCodeSrt.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("orgCusCode is EORI", jobDeclaration.JE_OA_DeclarantAddressInfo, eoriMessageError);
		});
	}

	public void TestCheckJE_EntryStyle()
	{
		const string expectedErrorMessage = "Entry Style is required for EXS - Exit Summary Declaration";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = string.Empty;
			AssertHasMessageError("JE_EntryStyle is empty.", declaration.JE_EntryStyleInfo, expectedErrorMessage);
			declaration.JE_EntryStyle = "A1";
			AssertNoMessageError("JE_EntryStyle is not empty.", declaration.JE_EntryStyleInfo, expectedErrorMessage);
		});
	}

	public void TestCheckGoodsLocationUNLocode()
	{
		var messageError = ListValidation.InvalidCodeMessageError.ToString();
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.X;
			declaration.Validation.ValidateGoodsLocationUNLocode();
			AssertNoNotifications("QualifierOfTheIdentificationList.Codes.X", declaration.GoodsLocationUNLocodeInfo);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.U;
			declaration.Validation.ValidateGoodsLocationUNLocode();
			AssertHasMessageErrorContaining("QualifierOfTheIdentificationList.Codes.U", declaration.GoodsLocationUNLocodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.GoodsLocationUNLocode = "BEAAA";
			AssertHasMessageErrorContaining("Valid UNLocode", declaration.GoodsLocationUNLocodeInfo, messageError);

			declaration.GoodsLocationUNLocode = "BEBRU";
			AssertNoMessageErrorContaining("Invalid UNLocode", declaration.GoodsLocationUNLocodeInfo, messageError);
		});
	}

	public void TestCheckRuleR0013E()
	{
		var declaration = Factory.New<JobDeclaration>();
		var messageError = $"[R0013E] {MandatoryValidation.YouHaveNotEntered}";
		var propertyInfo = declaration.JE_RN_NKTransportNationalityInlandInfo;
		var transportMeansTypeList = new[]
		{
			string.Empty
			, ModeOfTransportList.Codes._2_RailTransport
			, ModeOfTransportList.Codes._5_PostalConsignment
			, ModeOfTransportList.Codes._7_FixedTransportInstallations
		};

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty Setup", propertyInfo, messageError);

			foreach (var transportMeansType in transportMeansTypeList)
			{
				declaration.JE_TransportMeans = transportMeansType;
				declaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
				AssertNoMessageErrorContaining($"Leading character '{transportMeansType}'", propertyInfo, messageError);

				declaration.JE_TransportMeans = $"1{transportMeansType}";
				declaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
				AssertHasMessageErrorContaining($"Not Leading character {transportMeansType} - 1{transportMeansType}", propertyInfo, messageError);
			}

			declaration.JE_TransportMeans = "11";
			declaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
			AssertHasMessageErrorContaining($"not 2 / 5 / 7 transportMeansType", propertyInfo, messageError);
		});
	}

	public void TestCheckJE_RN_NKTrailer1Nationality()
	{
		var propertyInfo = declaration.JE_RN_NKTrailer1NationalityInfo;
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty setup", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportModeInland = RefTransportModeList.Codes.ROA;
			declaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
			AssertNoMessageErrorContaining("Road but empty trailer", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportModeInland = RefTransportModeList.Codes.SEA;
			declaration.JE_Trailer1RegNo = "abc";
			declaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
			AssertNoMessageErrorContaining("Not Road and not empty trailer", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportModeInland = RefTransportModeList.Codes.ROA;
			declaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
			AssertHasMessageErrorContaining("Road and not empty trailer", propertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_RN_NKTrailer2Nationality()
	{
		var propertyInfo = declaration.JE_RN_NKTrailer2NationalityInfo;
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty setup", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportModeInland = RefTransportModeList.Codes.ROA;
			declaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
			AssertNoMessageErrorContaining("Road but empty trailer", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportModeInland = RefTransportModeList.Codes.SEA;
			declaration.JE_Trailer2RegNo = "abc";
			declaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
			AssertNoMessageErrorContaining("Not Road and not empty trailer", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportModeInland = RefTransportModeList.Codes.ROA;
			declaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
			AssertHasMessageErrorContaining("Road and not empty trailer", propertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_LloydsIMO()
	{
		const string errorMessage = "You have not entered an IMO number.";
		jobDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Sea;

		CombineAssertions(() =>
		{
			var targetInfo = jobDeclaration.JE_LloydsIMOInfo;

			jobDeclaration.Validation.ValidateJE_LloydsIMO();
			AssertNoMessageErrorContaining("IMO is not required when Border Transport Means is empty", targetInfo, errorMessage);

			jobDeclaration.ZG_BorderTransportMeans = MeansOfTransportList.Codes.NameOfTheSeaGoingVessel;
			jobDeclaration.Validation.ValidateJE_LloydsIMO();
			AssertNoMessageErrorContaining("IMO is not required when Border Transport Means is not 10", targetInfo, errorMessage);

			jobDeclaration.ZG_BorderTransportMeans = MeansOfTransportList.Codes.ImoShipIdentificationNumber;
			jobDeclaration.Validation.ValidateJE_LloydsIMO();
			AssertHasMessageErrorContaining("IMO is required when Border Transport Means is 10", targetInfo, errorMessage);

			jobDeclaration.JE_LloydsIMO = "1234567";
			AssertNoMessageErrorContaining("IMO is required when Border Transport Means is 10. No error message - validation passed.", targetInfo, errorMessage);
		});
	}

	public void TestCheckJE_RN_NKTransportNationality_Sea()
	{
		var vessel = Factory.New<RefVessel>();
		vessel.RV_Code = "abc123";
		vessel.RV_LloydsNumber = "123456";
		vessel.RV_RN_NKCountryOfReg = CountryCodes.Germany;

		var messageError = "The Vessel Registration Country is different from the Nationality code entered.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_RN_NKTransportNationality = CountryCodes.Tuvalu;

		CombineAssertions(() =>
		{
			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			declaration.JE_VesselName = "abc123";
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageError("JE_TransportIDInland is vessel but not a sea transport mode", declaration.JE_RN_NKTransportNationalityInfo, messageError);

			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_VesselName = "abc123";
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageError("JE_TransportModeInland is sea with lloyd number - valid Country", declaration.JE_RN_NKTransportNationalityInfo, messageError);

			declaration.JE_RN_NKTransportNationality = CountryCodes.Tuvalu;
			AssertHasMessageError("JE_TransportModeInland is sea with lloyd number - invalid Country", declaration.JE_RN_NKTransportNationalityInfo, messageError);

			vessel.RV_RN_NKCountryOfReg = ZString.Empty;
			jobDeclaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
			AssertNoMessageError("RV_RN_NKCountryOfReg is empty", jobDeclaration.JE_RN_NKTransportNationalityInfo, messageError);

			vessel.RV_RN_NKCountryOfReg = CountryCodes.Germany;
			jobDeclaration.JE_RN_NKTransportNationality = ZString.Empty;
			AssertNoMessageError("JE_RN_NKTransportNationalityInland is empty", jobDeclaration.JE_RN_NKTransportNationalityInfo, messageError);
		});
	}

	public void TestCheckJE_RN_NKTransportNationalityInland_Sea()
	{
		var vessel = Factory.New<RefVessel>();
		vessel.RV_Code = "abc123";
		vessel.RV_LloydsNumber = "123456";
		vessel.RV_RN_NKCountryOfReg = CountryCodes.Germany;

		var messageError = "The Vessel Registration Country is different from the Nationality code entered.";
		jobDeclaration.JE_RN_NKTransportNationalityInland = CountryCodes.Tuvalu;

		CombineAssertions(() =>
		{
			jobDeclaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			jobDeclaration.JE_TransportIDInland = "abc123";
			jobDeclaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
			AssertNoMessageError("JE_TransportIDInland is vessel but not a sea transport mode", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, messageError);

			jobDeclaration.JE_TransportModeInland = TransportModes.Sea;
			jobDeclaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._10;
			jobDeclaration.JE_TransportIDInland = "123456";
			AssertNoMessageError("transport mode is sea with lloyd number - valid Country", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, messageError);

			jobDeclaration.JE_RN_NKTransportNationalityInland = CountryCodes.Tuvalu;
			AssertHasMessageError("transport mode is sea with lloyd number - invalid Country", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, messageError);

			jobDeclaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			jobDeclaration.JE_TransportIDInland = "abc123";
			AssertNoMessageError("transport mode is sea with vessel name - valid Country", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, messageError);

			jobDeclaration.JE_RN_NKTransportNationalityInland = CountryCodes.Tuvalu;
			AssertHasMessageError("transport mode is sea with vessel name - invalid Country", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, messageError);

			vessel.RV_RN_NKCountryOfReg = ZString.Empty;
			jobDeclaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
			AssertNoMessageError("RV_RN_NKCountryOfReg is empty", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, messageError);

			vessel.RV_RN_NKCountryOfReg = CountryCodes.Germany;
			jobDeclaration.JE_RN_NKTransportNationalityInland = ZString.Empty;
			AssertNoMessageError("JE_RN_NKTransportNationalityInland is empty", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, messageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
	}

	JobDeclaration jobDeclaration;

	string MessageType => EUJobMessageTypeList.Codes.MiscellaneousCustoms;
}
