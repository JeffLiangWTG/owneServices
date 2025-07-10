using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckJustification()
	{
		var messageSendingObject = GetNewDepartureSendingObject();
		CombineAssertions(() =>
		{
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.INV;
			AssertNullOrEmpty("Justification is empty", messageSendingObject.Justification);
			AssertHasMessageErrorContaining("Message type is INV", messageSendingObject.JustificationInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			AssertNoMessageErrorContaining("Message type is not INV", messageSendingObject.JustificationInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.INV;
			messageSendingObject.Justification = "Justification entered";
			AssertNoMessageErrorContaining("Message type INV, Justification entered", messageSendingObject.JustificationInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckAmendmentType()
	{
		var messageSendingObject = GetNewDepartureSendingObject();
		CombineAssertions(() =>
		{
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			AssertNullOrEmpty("AmendmentType is empty", messageSendingObject.AmendmentType);
			messageSendingObject.Validation.ValidateAmendmentType();
			AssertHasMessageErrorContaining("Message type is AMD", messageSendingObject.AmendmentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.DEC;
			messageSendingObject.Validation.ValidateAmendmentType();
			AssertNoMessageErrorContaining("Message type is not AMD", messageSendingObject.AmendmentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			messageSendingObject.AmendmentType = AmendmentTypeList.Codes._0DeclarationAmendment;
			AssertNoMessageErrorContaining("Message type AMD, AmendmentType set", messageSendingObject.AmendmentTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckGoodsLocation()
	{
		const string prefixCheck = "NR0032";
		var messageSendingObject = GetNewDepartureSendingObject();
		var nctsHeader = messageSendingObject.NctsHeader;
		nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
		var message = nctsHeader.Messages.AddNew();
		CombineAssertions(() =>
		{
			message.EM_MessageSubType = Constants.MessageSubTypeCodes.IE015;
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.PRN;
			messageSendingObject.Validation.ValidateGoodsLocation();
			AssertHasMessageErrorContaining(messageSendingObject.GoodsLocationInfo, prefixCheck);

			nctsHeader.MovementHeader.GoodsLocation.CGL_Qualifier = "A";
			messageSendingObject.Validation.ValidateGoodsLocation();
			AssertNoMessageErrorContaining(messageSendingObject.GoodsLocationInfo, prefixCheck);

			nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			nctsHeader.MovementHeader.GoodsLocation.CGL_Qualifier = ZString.Empty;
			messageSendingObject.Validation.ValidateGoodsLocation();
			AssertNoMessageErrorContaining(messageSendingObject.GoodsLocationInfo, prefixCheck);

			nctsHeader.Messages.Remove(message);
			nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			messageSendingObject.Validation.ValidateGoodsLocation();
			AssertNoMessageErrorContaining(messageSendingObject.GoodsLocationInfo, prefixCheck);
		});
	}

	public void TestCheckTransportIdentification()
	{
		const string prefixCheck = "NR0033";
		var messageSendingObject = GetNewDepartureSendingObject();
		var nctsHeader = messageSendingObject.NctsHeader;
		nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
		var message = nctsHeader.Messages.AddNew();

		CombineAssertions(() =>
		{
			message.EM_MessageSubType = Constants.MessageSubTypeCodes.IE015;
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.PRN;
			nctsHeader.MovementHeader.TransportAtDeparture = "te";
			messageSendingObject.Validation.ValidateTransportIdentification();
			AssertHasMessageErrorContaining(messageSendingObject.TransportIdentificationInfo, prefixCheck);

			nctsHeader.MovementHeader.TransportAtDeparture = "TE";
			messageSendingObject.Validation.ValidateTransportIdentification();
			AssertNoMessageErrorContaining(messageSendingObject.TransportIdentificationInfo, prefixCheck);

			nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			nctsHeader.MovementHeader.TransportAtDeparture = "te";
			messageSendingObject.Validation.ValidateTransportIdentification();
			AssertNoMessageErrorContaining(messageSendingObject.TransportIdentificationInfo, prefixCheck);

			nctsHeader.Messages.Remove(message);
			nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			messageSendingObject.Validation.ValidateTransportIdentification();
			AssertNoMessageErrorContaining(messageSendingObject.TransportIdentificationInfo, prefixCheck);
		});
	}

	public void TestCheckPresentationDateTime()
	{
		var invalidDateTimeMessage = "Presentation Date and Time cannot be in the past";
		var messageSendingObject = GetNewDepartureSendingObject();
		CombineAssertions(() =>
		{
			messageSendingObject.PresentationDateTime = CargoWise.Types.ZDateTime.Now.AddDays(-1);
			AssertNoMessageError("Message type is not AMD, PresentationDateTime is in the past", messageSendingObject.PresentationDateTimeInfo, invalidDateTimeMessage);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			messageSendingObject.Validation.ValidatePresentationDateTime();
			AssertHasMessageError("Message type AMD, PresentationDateTime is in the past", messageSendingObject.PresentationDateTimeInfo, invalidDateTimeMessage);

			messageSendingObject.PresentationDateTime = CargoWise.Types.ZDateTime.Now.AddDays(1);
			AssertNoMessageError("Message type AMD, valid PresentationDateTime set", messageSendingObject.AmendmentTypeInfo, invalidDateTimeMessage);
		});
	}

	public void TestCheckDepartureOfficeOfEnquiry()
	{
		var messageSendingObject = GetNewDepartureSendingObject();
		messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.RNM;
		var customsOffice = messageSendingObject.NctsHeader.MovementHeader.CustomsOffices.AddNew();

		CombineAssertions(() =>
		{
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain;
			customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
			messageSendingObject.DepartureOfficeOfEnquiry = ZString.Empty;
			AssertHasMessageErrorContaining(messageSendingObject.DepartureOfficeOfEnquiryInfo, MandatoryValidation.YouHaveNotEntered);
			messageSendingObject.DepartureOfficeOfEnquiry = "eee";
			AssertNoMessageErrorContaining(messageSendingObject.DepartureOfficeOfEnquiryInfo, MandatoryValidation.YouHaveNotEntered);

			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry;
			customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
			customsOffice.CY_Data = "eee";
			messageSendingObject.Validation.ValidateDepartureOfficeOfEnquiry();
			AssertNoMessageErrorContaining(messageSendingObject.DepartureOfficeOfEnquiryInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckTirPageNumber()
	{
		var messageSendingObject = GetNewArrivalSendingObject();
		var movementHeader = messageSendingObject.NctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			messageSendingObject.MessageType = ArrivalMessageSendingObjectTypeList.Codes.URM;
			movementHeader.BM_UnloadingCompleted = false;
			messageSendingObject.Validation.ValidateTirPageNumber();
			AssertNoMessageErrorContaining("Unloading is not completed", messageSendingObject.TirPageNumberInfo, MandatoryValidation.YouHaveNotEntered);

			movementHeader.BM_UnloadingCompleted = true;
			messageSendingObject.Validation.ValidateTirPageNumber();
			AssertHasWarningContaining("Empty declaration with empty TirPageNumber", messageSendingObject.TirPageNumberInfo, "[RP22] – for TIR declaration type, it’s required to enter TIR Page Number.");

			messageSendingObject.MessageType = ArrivalMessageSendingObjectTypeList.Codes.ARN;
			messageSendingObject.TirPageNumber = "00";
			AssertNoMessageErrorContaining("Not URM message", messageSendingObject.TirPageNumberInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.MessageType = ArrivalMessageSendingObjectTypeList.Codes.URM;
			messageSendingObject.TirPageNumber = "00";
			messageSendingObject.Validation.ValidateTirPageNumber();
			AssertNoMessageErrorContaining("TirPageNumber is not empty", messageSendingObject.TirPageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("Invalid code for URM", messageSendingObject.TirPageNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

			messageSendingObject.TirPageNumber = TirPageNumberTypeList.Codes._04;
			AssertNoMessageErrorContaining("Valid code for URM", messageSendingObject.TirPageNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			messageSendingObject.TirPageNumber = string.Empty;
			messageSendingObject.Validation.ValidateTirPageNumber();
			AssertHasMessageErrorContaining("TirPageNumber is empty", messageSendingObject.TirPageNumberInfo, "[RP22] You have not entered TIR Page Number.");
			AssertHasMessageErrorContaining("YouHaveNotEntered message appears", messageSendingObject.TirPageNumberInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.TirPageNumber = "00";
			messageSendingObject.Validation.ValidateTirPageNumber();
			AssertHasMessageErrorContaining("Invalid TirPageNumber", messageSendingObject.TirPageNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

			messageSendingObject.TirPageNumber = TirPageNumberTypeList.Codes._04;
			messageSendingObject.Validation.ValidateTirPageNumber();
			AssertNoMessageErrorContaining("Valid TirPageNumber", messageSendingObject.TirPageNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

			movementHeader.BM_InBondEntryType = string.Empty;
			messageSendingObject.TirPageNumber = string.Empty;
			messageSendingObject.Validation.ValidateTirPageNumber();
			AssertHasWarningContaining("Empty declaration with empty TirPageNumber", messageSendingObject.TirPageNumberInfo, "[RP22] – for TIR declaration type, it’s required to enter TIR Page Number.");
		});
	}

	public void TestCheckTirUnloadingNumber()
	{
		var messageSendingObject = GetNewArrivalSendingObject();
		var movementHeader = messageSendingObject.NctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			messageSendingObject.MessageType = ArrivalMessageSendingObjectTypeList.Codes.URM;
			movementHeader.BM_UnloadingCompleted = false;
			messageSendingObject.Validation.ValidateTirUnloadingNumber();
			AssertNoMessageErrorContaining("Unloading is not completed", messageSendingObject.TirUnloadingNumberInfo, MandatoryValidation.YouHaveNotEntered);

			movementHeader.BM_UnloadingCompleted = true;
			messageSendingObject.Validation.ValidateTirUnloadingNumber();
			AssertHasWarningContaining("Empty declaration with empty TirUnloadingNumber", messageSendingObject.TirUnloadingNumberInfo, "[RP22] – for TIR declaration type, it’s required to enter TIR Unloading Number.");

			messageSendingObject.MessageType = ArrivalMessageSendingObjectTypeList.Codes.RNM;
			messageSendingObject.Validation.ValidateTirUnloadingNumber();
			AssertNoMessageErrorContaining("Message is not URM", messageSendingObject.TirUnloadingNumberInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.MessageType = ArrivalMessageSendingObjectTypeList.Codes.URM;
			messageSendingObject.TirUnloadingNumber = "AB";
			AssertNoMessageErrorContaining("TirUnloadingNumber is not empty", messageSendingObject.TirUnloadingNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("Invalid code for URM", messageSendingObject.TirUnloadingNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

			messageSendingObject.TirUnloadingNumber = TirUnloadingNumberTypeList.Codes.D1;
			AssertNoMessageErrorContaining("Valid code for URM", messageSendingObject.TirUnloadingNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			messageSendingObject.TirUnloadingNumber = string.Empty;
			messageSendingObject.Validation.ValidateTirUnloadingNumber();
			AssertHasMessageErrorContaining("TirUnloadingNumber is empty", messageSendingObject.TirUnloadingNumberInfo, "[RP22] You have not entered TIR Unloading Number.");
			AssertHasMessageErrorContaining("YouHaveNotEntered message appears", messageSendingObject.TirUnloadingNumberInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.TirUnloadingNumber = "AB";
			messageSendingObject.Validation.ValidateTirUnloadingNumber();
			AssertHasMessageErrorContaining("Invalid TirUnloadingNumber", messageSendingObject.TirUnloadingNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

			messageSendingObject.TirUnloadingNumber = TirUnloadingNumberTypeList.Codes.D1;
			messageSendingObject.Validation.ValidateTirUnloadingNumber();
			AssertNoMessageErrorContaining("Valid TirUnloadingNumber", messageSendingObject.TirUnloadingNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

			movementHeader.BM_InBondEntryType = string.Empty;
			messageSendingObject.TirUnloadingNumber = string.Empty;
			messageSendingObject.Validation.ValidateTirUnloadingNumber();
			AssertHasWarningContaining("Empty declaration with empty TirUnloadingNumber", messageSendingObject.TirUnloadingNumberInfo, "[RP22] – for TIR declaration type, it’s required to enter TIR Unloading Number.");
		});
	}

	MessageSendingObject GetNewArrivalSendingObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		return new MessageSendingObject(nctsHeader);
	}

	MessageSendingObject GetNewDepartureSendingObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		return new MessageSendingObject(nctsHeader);
	}
}
