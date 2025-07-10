using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC583RootProviderTest : AESBaseProviderTest<CC583RootProvider>
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject",
			"Value cannot be null.\r\nParameter name: sendingObject",
			() => new CC583RootProvider(null, null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration",
			"Value cannot be null.\r\nParameter name: EntryHeader.Declaration",
			() => new CC583RootProvider(new(Factory.New<CusEntryHeader>()), null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction",
			"Value cannot be null.\r\nParameter name: EntryHeader.EntryInstruction",
			() => new CC583RootProvider(new(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()), null));
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObjectParent",
			"Value cannot be null.\r\nParameter name: sendingObjectParent",
			() => new CC583RootProvider(sendingObject, null));
		AssertNoExceptionThrown("Valid data", () => new CC583RootProvider(sendingObject, sendingObjectParent));
	}

	protected override string ExpectedMessageType => Constants.MessageType.AES.CC583C;

	protected override CC583RootProvider GetProvider() => new(sendingObject, sendingObjectParent);

	public void TestExportOperation() => AssertType<CC583ExportOperationProvider>(Provider.ExportOperation);

	public void TestExitCarrier() => AssertType<CC583ExitCarrierProvider>(Provider.ExitCarrier);

	public void TestExitCarrier_RuleC0681() => CombineAssertions(() =>
	{
		string[] ruleC0681EnquiryInformationCodes = ["1", "2"];
		foreach (var enquiryInformationCode in ruleC0681EnquiryInformationCodes)
		{
			sendingObjectParent.EnquiryInformationCode = enquiryInformationCode;
			AssertNull($"ExitCarrier is empty for enquiry information code {enquiryInformationCode}.", GetProvider().ExitCarrier);
		}
		sendingObjectParent.EnquiryInformationCode = "3";
		AssertType<CC583ExitCarrierProvider>("ExitCarrier is defined for other enquiry information codes.", GetProvider().ExitCarrier);
	});

	public void TestDeclarant() => AssertType<AESDeclarantWithIdentificationNumbersProvider>(Provider.Declarant);

	public void TestRepresentative() => CombineAssertions(() =>
	{
		var representativeHeader = Factory.New<OrgHeader>();
		var representativeAddress = representativeHeader.Addresses.AddNew();
		declaration.JE_OA_Representative = representativeAddress.PK;
		declaration.JE_DeclarantType = PLRepresentationTypeList.Codes._4Direct;
		AssertType<AESRepresentativeProvider>("Representative is defined for valid declarant type.", Provider.Representative);

		declaration.JE_DeclarantType = PLRepresentationTypeList.Codes._3Consignee;
		AssertNull("Representative is empty for unsupported declarant type.", GetProvider().Representative);
	});

	public void TestCustomsOfficeOfExportReferenceNumber() => CombineAssertions(() =>
	{
		sendingObjectParent.CustomsOffice = null;
		AssertEquals("CustomsOffice is not defined.", string.Empty, Provider.CustomsOfficeOfExportReferenceNumber);

		sendingObjectParent.CustomsOffice = "TestCustomsOffice";
		AssertEquals("CustomsOffice is specified.", "TestCustomsOffice", Provider.CustomsOfficeOfExportReferenceNumber);
	});

	public void TestCustomsOfficeOfExitActualReferenceNumber() => CombineAssertions(() =>
	{
		sendingObjectParent.OfficeOfExitActual = null;
		AssertEquals("OfficeOfExitActual is not defined.", string.Empty, Provider.CustomsOfficeOfExitActualReferenceNumber);

		sendingObjectParent.OfficeOfExitActual = "TestOfficeOfExitActual";
		AssertEquals("OfficeOfExitActual is specified.", "TestOfficeOfExitActual", Provider.CustomsOfficeOfExitActualReferenceNumber);
	});

	public void TestAlternativeEvidences() => CombineAssertions(() =>
	{
		sendingObjectParent.AlternativeEvidences.Add(new AlternativeEvidence(sendingObjectParent));
		AssertType<List<AlternativeEvidenceProvider>>("Provider type.", Provider.AlternativeEvidences);
		AssertEquals("Items count.", GetProvider().AlternativeEvidences.Count, 1);
		AssertEquals("The sequence starts from number 1.", 1, GetProvider().AlternativeEvidences.Single().SequenceNumber);
	});

	protected override void SetUp()
	{
		base.SetUp();

		sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
	}
}
