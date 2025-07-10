using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(MessageSendingObject))]
sealed class MessageSendingObjectTest : NctsHeaderMessageSendingObjectTest
{
	public new void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new MessageSendingObject(null));
	}

	public void TestMovementReferenceNumber_DefaultValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default MovementReferenceNumber is empty", ZString.Empty, messageSendingObject.MovementReferenceNumber);

			var header = Factory.New<NctsHeader>();
			header.MovementReferenceEntryNumber.CE_EntryNum = "789";
			var messageSendingObject1 = new MessageSendingObject(header);
			AssertEquals("Default MovementReferenceNumber is set", "789", messageSendingObject1.MovementReferenceNumber);
		});
	}

	public void TestMovementReferenceNumber_Caption() => NCTSTestHelper.AssertCaptions(messageSendingObject.MovementReferenceNumberInfo, "Movement Reference Number", string.Empty, "MRN");

	public void TestMovementReferenceNumber_ReadOnly()
	{
		CombineAssertions(() =>
		{
			var movementReferenceNumberInfo = messageSendingObject.MovementReferenceNumberInfo;
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			AssertNullOrEmpty("MRN is empty", messageSendingObject.MRN);
			AssertEquals("Movement Reference Number is editable", expected: false, movementReferenceNumberInfo.ReadOnly);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.DEC;
			AssertEquals("Message Type is not AMD, Movement Reference Number is readonly", expected: true, movementReferenceNumberInfo.ReadOnly);

			var header = Factory.New<NctsHeader>();
			header.MovementReferenceEntryNumber.CE_EntryNum = "789";
			var messageSendingObject1 = new MessageSendingObject(header);
			messageSendingObject1.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			movementReferenceNumberInfo = messageSendingObject1.MovementReferenceNumberInfo;
			AssertEquals("Message Type is AMD, MRN has value, Movement Reference Number is readonly", expected: true, movementReferenceNumberInfo.ReadOnly);
		});
	}

	public void TestMovementReferenceNumber_MaxLength() => AssertEquals(AutoNctsHeaderMessageSendingObject.Schema.MRNMaxLength, messageSendingObject.MovementReferenceNumberInfo.MaxLength);

	public void TestMovementReferenceNumber_MessageTypeChanged()
	{
		var header = Factory.New<NctsHeader>();
		var messageSendingObject = new MessageSendingObject(header);
		header.MovementReferenceEntryNumber.CE_EntryNum = "123";
		CombineAssertions(() =>
		{
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			messageSendingObject.MovementReferenceNumber = "456";
			AssertEquals("MovementReferenceNumber", "456", messageSendingObject.MovementReferenceNumber);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.DEC;
			AssertEquals("Message type changed, MovementReferenceNumber reset to default value", "123", messageSendingObject.MovementReferenceNumber);

			messageSendingObject.MovementReferenceNumber = "789";
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.DEC;
			AssertEquals("Same Message type set, MovementReferenceNumber unchanged", "789", messageSendingObject.MovementReferenceNumber);
		});
	}

	public void TestDepartureOfficeOfEnquiry_ReadOnly()
	{
		CombineAssertions(() =>
		{
			var customsOffice = messageSendingObject.NctsHeader.MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.AeoCompetentCustomsAuthorities;
			customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.RNM;
			var departureOfficeOfEnquiryInfo = messageSendingObject.DepartureOfficeOfEnquiryInfo;
			AssertEquals("Departure Office Of Enquiry is editable", expected: false, departureOfficeOfEnquiryInfo.ReadOnly);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.DEC;
			AssertEquals("Departure Office Of Enquiry is read only", expected: true, departureOfficeOfEnquiryInfo.ReadOnly);
		});
	}

	public void TestDescription_Captions() => NCTSTestHelper.AssertCaptions(messageSendingObject.DescriptionInfo, "Description", string.Empty, "Descr.");

	public void TestDescription()
	{
		var departureCode = DepartureMessageSendingObjectTypeList.Codes.AMD;
		var expectedDepartureDescription = DepartureMessageSendingObjectTypeList.Descriptions.AMD;
		var arrivalCode = ArrivalMessageSendingObjectTypeList.Codes.ARN;
		var expectedArrivalDescription = ArrivalMessageSendingObjectTypeList.Descriptions.ARN;

		var departureHeader = Factory.New<NctsHeader>();
		departureHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var departureObject = new MessageSendingObject(departureHeader);

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		var arrivalObject = new MessageSendingObject(arrivalHeader);

		CombineAssertions(() =>
		{
			AssertEquals("Departure message description is empty by default", string.Empty, departureObject.Description);
			departureObject.MessageType = departureCode;
			AssertEquals($"Departure message description for {departureCode}:", expectedDepartureDescription, departureObject.Description);

			AssertEquals("Arrival message description is empty by default", string.Empty, arrivalObject.Description);
			arrivalObject.MessageType = arrivalCode;
			AssertEquals($"Arrival message description for {arrivalCode}:", expectedArrivalDescription, arrivalObject.Description);
		});
	}

	public void TestJustification_MaxLength() => AssertEquals(512, messageSendingObject.JustificationInfo.MaxLength);

	public void TestJustification_Caption() => AssertEquals("Justification", DataBoundResourceStrings.GetDataForProperty(messageSendingObject.JustificationInfo).Caption);

	public void TestJustification_Readonly() => AssertEquals(false, messageSendingObject.JustificationInfo.ReadOnly);

	public void TestJustification_MessageTypeChanged()
	{
		CombineAssertions(() =>
		{
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.INV;
			messageSendingObject.Justification = "ABC";
			AssertEquals("Justification", "ABC", messageSendingObject.Justification);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			AssertNullOrEmpty("Message type changed, Justification reset", messageSendingObject.Justification);

			messageSendingObject.Justification = "ABC";
			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;
			AssertEquals("Same Message type set, Justification unchanged", "ABC", messageSendingObject.Justification);
		});
	}

	public void TestAmendmentType_Caption() => AssertEquals("Amendment Type", DataBoundResourceStrings.GetDataForProperty(messageSendingObject.AmendmentTypeInfo).Caption);

	public void TestGoodsLocation()
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, messageSendingObject.GoodsLocation);
			nctsHeader.MovementHeader.GoodsLocation.CGL_Qualifier = "A";
			AssertEquals("A", messageSendingObject.GoodsLocation);
		});
	}

	public void TestGoodsLocation_HumanReadableName() => AssertEquals(nctsHeader.MovementHeader.GoodsLocationDescriptionInfo.HumanReadableName, messageSendingObject.GoodsLocationInfo.HumanReadableName);

	public void TestTransportIdentification()
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, messageSendingObject.TransportIdentification);
			nctsHeader.MovementHeader.TransportAtDeparture = "TE";
			AssertEquals("TE", messageSendingObject.TransportIdentification);
		});
	}

	public void TestTransportIdentification_HumanReadableName()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertEquals(nctsHeader.MovementHeader.BM_TransportAtDepartureInfo.HumanReadableName, messageSendingObject.TransportIdentificationInfo.HumanReadableName);
	}

	public void TestPresentationDateTime_Caption() => AssertEquals("Presentation Date and Time", DataBoundResourceStrings.GetDataForProperty(messageSendingObject.PresentationDateTimeInfo).Caption);

	public void TestValidation() => AssertType<MessageSendingObjectValidation>(messageSendingObject.Validation);

	public void TestLookups() => AssertType<MessageSendingObjectLookups>(messageSendingObject.Lookups);

	public void TestTirPageNumber_Caption() => AssertEquals("TIR Page Number", DataBoundResourceStrings.GetDataForProperty(messageSendingObject.TirPageNumberInfo).Caption);

	public void TestTirPageNumber_MaxLength() => AssertEquals(2, (messageSendingObject.TirPageNumberInfo.MaxLength));

	public void TestTirPageNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default value", ZString.Empty, messageSendingObject.TirPageNumber);

			messageSendingObject.TirPageNumber = "0A";
			AssertEquals("Not a number", "0A", messageSendingObject.TirPageNumber);

			messageSendingObject.TirPageNumber = "0";
			AssertEquals("on number value should be LeftPad(2)", "0", messageSendingObject.TirPageNumber);

			messageSendingObject.TirPageNumber = "10";
			AssertEquals("> 9 should not be left pad", "10", messageSendingObject.TirPageNumber);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.INV;
			AssertEquals("On Message Type change should be cleared", ZString.Empty, messageSendingObject.TirPageNumber);
		});
	}

	public void TestTirUnloadingNumber_Caption() => AssertEquals("TIR Unloading Number", DataBoundResourceStrings.GetDataForProperty(messageSendingObject.TirUnloadingNumberInfo).Caption);

	public void TestTirUnloadingNumber_MaxLength() => AssertEquals(2, (messageSendingObject.TirUnloadingNumberInfo.MaxLength));

	public void TestTirUnloadingNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default value", ZString.Empty, messageSendingObject.TirUnloadingNumber);

			messageSendingObject.TirUnloadingNumber = "0A";
			AssertEquals("Not default value", "0A", messageSendingObject.TirUnloadingNumber);

			messageSendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.INV;
			AssertEquals("On Message Type change should be cleared", ZString.Empty, messageSendingObject.TirUnloadingNumber);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => messageSendingObject;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader);
	}
	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
}
