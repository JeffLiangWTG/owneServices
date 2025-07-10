using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class InBondMenuItemMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckInBondNumber()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader.MovementHeaders.AddNew();
			var movementHeader2 = inBondHeader.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "111111111";
			var movementHeader3 = inBondHeader.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111111";
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First(x => x.InBondNumber.IsEmpty);
			inBondMenuItemMessageSendingObject.Validation.ValidateInBondNumber();
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, "In-Bond Number cannot be empty.");
			inBondMenuItemMessageSendingObject.InBondNumber = "ABC";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, "In-Bond Number cannot be empty.");
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, US.Business.ValidationConstants.AllocateInBondNumber.MustBeNumeric);
			inBondMenuItemMessageSendingObject.InBondNumber = "1111111";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, US.Business.ValidationConstants.AllocateInBondNumber.MustBeNumeric);
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, US.Business.ValidationConstants.AllocateInBondNumber.InBondNumberLengthShouldBeNineOrEleven);
			inBondMenuItemMessageSendingObject.InBondNumber = "111111111";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, US.Business.ValidationConstants.AllocateInBondNumber.InBondNumberLengthShouldBeNineOrEleven);
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, US.Business.ValidationConstants.AllocateInBondNumber.AlreadyExists);
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, string.Format(InBondMenuItemMessageSendingObjectValidation.DuplicateInBondNumberFound, "111111111"));

			inBondMenuItemMessageSendingObject.InBondNumber = "111111110";
			var inBondNumber = Common.CusEntryNumber.Load(movementHeader2, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
			var oldTime = inBondNumber.CE_SystemCreateTimeUtc;
			inBondNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddYears(-4);
			inBondMenuItemMessageSendingObject.InBondNumber = "111111111";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, US.Business.ValidationConstants.AllocateInBondNumber.InBondNumberLengthShouldBeNineOrEleven);
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, US.Business.ValidationConstants.AllocateInBondNumber.AlreadyExists);
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, string.Format(InBondMenuItemMessageSendingObjectValidation.DuplicateInBondNumberFound, "111111111"));

			inBondNumber.CE_SystemCreateTimeUtc = oldTime;

			inBondMenuItemMessageSendingObject.InBondNumber = "111111112";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, US.Business.ValidationConstants.AllocateInBondNumber.AlreadyExists);
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, string.Format(InBondMenuItemMessageSendingObjectValidation.DuplicateInBondNumberFound, "111111111"));
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.InBondNumber = "111111113";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, Env.Security.USInBondNew.ErrorMessageForNotAllowed.ToString());
			Env.Security.USInBondNew.IsAllowed = false;
			inBondMenuItemMessageSendingObject.Validation.ValidateInBondNumber();
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, Env.Security.USInBondNew.ErrorMessageForNotAllowed.ToString());
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects[0];
			AssertEquals(ZString.Empty, inBondMenuItemMessageSendingObject.InBondNumber);
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, InBondMenuItemMessageSendingObjectValidation.MatchingMovementHeaderNoFound);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.InBondNumber = "111111112";
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, InBondMenuItemMessageSendingObjectValidation.MatchingMovementHeaderNoFound);
			inBondMenuItemMessageSendingObject.InBondNumber = "111111111";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, InBondMenuItemMessageSendingObjectValidation.MatchingMovementHeaderNoFound);

			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.PrintDocument);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects[0];
			AssertEquals(ZString.Empty, inBondMenuItemMessageSendingObject.InBondNumber);
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, InBondMenuItemMessageSendingObjectValidation.MatchingMovementHeaderNoFound);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.InBondNumber = "111111112";
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, InBondMenuItemMessageSendingObjectValidation.MatchingMovementHeaderNoFound);
			inBondMenuItemMessageSendingObject.InBondNumber = "111111111";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.InBondNumberInfo, InBondMenuItemMessageSendingObjectValidation.MatchingMovementHeaderNoFound);
		}

		public void TestCheckUSDestinationPortCode()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var validCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "Test Name", startDate, endDate);
			newFactory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(inBondMenuItemMessageSendingObject.USDestinationPortCodeInfo, "????", validCode.ZZD_Code);
		}

		public void TestCheckForeignDestinationPortCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "R!456", "R!456", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeMessageError(inBondMenuItemMessageSendingObject.ForeignDestinationPortCodeInfo, "????", "R!456");
		}

		public void TestCheckInBondCarrierCodeSCAC()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SC1Z";
			ValidationTestHelper.AssertInvalidCodeMessageError(inBondMenuItemMessageSendingObject.InBondCarrierCodeSCACInfo, "????", "SC1Z");
		}

		public void TestCheckQPMessageStatus()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			var movementHeader = inBondHeader.MovementHeaders.AddNew();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First();
			inBondMenuItemMessageSendingObject.Validation.ValidateQPMessageStatus();
			AssertNoMessageErrorContaining(inBondMenuItemMessageSendingObject.QPMessageStatusInfo, InBondMenuItemMessageSendingObjectValidation.WaitForResponseMessageText);
			movementHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First();
			inBondMenuItemMessageSendingObject.Validation.ValidateQPMessageStatus();
			AssertHasMessageErrorContaining(inBondMenuItemMessageSendingObject.QPMessageStatusInfo, InBondMenuItemMessageSendingObjectValidation.WaitForResponseMessageText);
		}

		public void TestCheckWPMessageStatus()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			var movementHeader = inBondHeader.MovementHeaders.AddNew();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First();
			inBondMenuItemMessageSendingObject.Validation.ValidateWPMessageStatus();
			AssertNoMessageErrorContaining(inBondMenuItemMessageSendingObject.WPMessageStatusInfo, InBondMenuItemMessageSendingObjectValidation.WaitForResponseMessageText);
			movementHeader.BM_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First();
			inBondMenuItemMessageSendingObject.Validation.ValidateWPMessageStatus();
			AssertHasMessageErrorContaining(inBondMenuItemMessageSendingObject.WPMessageStatusInfo, InBondMenuItemMessageSendingObjectValidation.WaitForResponseMessageText);
		}

		public void TestCheckEntryType()
		{
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			inBondMenuItemMessageSendingObject.InBondNumber = "111111114";
			inBondMenuItemMessageSendingObject.EntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertHasMessageErrorContaining(inBondMenuItemMessageSendingObject.EntryTypeInfo, "Entry type should be 62 or 63.");
			inBondMenuItemMessageSendingObject.EntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertNoMessageErrorContaining(inBondMenuItemMessageSendingObject.EntryTypeInfo, "Entry type should be 62 or 63.");
			inBondMenuItemMessageSendingObject.EntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertNoMessageErrorContaining(inBondMenuItemMessageSendingObject.EntryTypeInfo, "Entry type should be 62 or 63.");
		}

		public void TestCheckPedimentoNumber()
		{
			var inBondMenuItemMessageData_Pedimento = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento);
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData_Pedimento.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.Validation.ValidatePedimentoNumber();
			AssertHasErrorContaining(inBondMenuItemMessageSendingObject.PedimentoNumberInfo, MandatoryValidation.MustBeEntered);
			inBondMenuItemMessageSendingObject.PedimentoNumber = "A";
			AssertNoErrorContaining(inBondMenuItemMessageSendingObject.PedimentoNumberInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckArrivalFirmsCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "1234", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData, movementHeader1);
			inBondMenuItemMessageSendingObject.Validation.ValidateArrivalFirmsCode();
			AssertNoMessageErrorContaining(inBondMenuItemMessageSendingObject.ArrivalFirmsCodeInfo, "The code you have selected is not in the list.(In-Bond: 111111111)");

			movementHeader1.BM_FIRMS = "1235";
			inBondMenuItemMessageSendingObject.Validation.ValidateArrivalFirmsCode();
			AssertHasMessageErrorContaining(inBondMenuItemMessageSendingObject.ArrivalFirmsCodeInfo, "The code you have selected is not in the list.(In-Bond: 111111111)");

			movementHeader1.BM_FIRMS = "1234";
			inBondMenuItemMessageSendingObject.Validation.ValidateArrivalFirmsCode();
			AssertNoMessageErrorContaining(inBondMenuItemMessageSendingObject.ArrivalFirmsCodeInfo, "The code you have selected is not in the list.(In-Bond: 111111111)");

			movementHeader1.BM_FIRMS = "1235";
			inBondHeader1.BH_ImportTransportMode = US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			inBondMenuItemMessageSendingObject.Validation.ValidateArrivalFirmsCode();
			AssertNoMessageErrorContaining(inBondMenuItemMessageSendingObject.ArrivalFirmsCodeInfo, "The code you have selected is not in the list.(In-Bond: 111111111)");
		}

		protected override void SetUp()
		{
			base.SetUp();
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			inBondMenuItemMessageSendingObject.InBondNumber = "111111114";
		}

		InBondMenuItemMessageSendingObject inBondMenuItemMessageSendingObject;
		InBondMenuItemMessageData inBondMenuItemMessageData;
	}
}
