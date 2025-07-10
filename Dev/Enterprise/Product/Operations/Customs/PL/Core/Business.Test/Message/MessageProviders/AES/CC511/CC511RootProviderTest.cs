using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC511RootProviderTest : AESBaseProviderTest<CC511RootProvider>
{
	public override void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC511RootProvider(sendingObject: null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration", "Value cannot be null.\r\nParameter name: EntryHeader.Declaration", () => new CC511RootProvider(new BaseMessageSendingObject(Factory.New<CusEntryHeader>())));
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction", "Value cannot be null.\r\nParameter name: EntryHeader.EntryInstruction", () => new CC511RootProvider(new BaseMessageSendingObject(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew())));
		AssertNoExceptionThrown("Valid data", () => new CC511RootProvider(new BaseMessageSendingObject(entryHeader)));
	});

	public void TestExportOperation() => AssertNotNull(Provider.ExportOperation);

	public void TestCustomsOfficeOfPresentationReferenceNumber() => CombineAssertions(() =>
	{
		const string testDataFirst = "PL1";
		const string testDataSecond = "PL2";
		AssertEquals("Empty Value", string.Empty, GetProvider().CustomsOfficeOfPresentationReferenceNumber);

		var customsOfficeFirst = declaration.CustomsOfficesForBinding.AddNew();
		customsOfficeFirst.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		customsOfficeFirst.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
		customsOfficeFirst.CY_Data = testDataFirst;
		AssertEquals("Matched with CY_Data", testDataFirst, GetProvider().CustomsOfficeOfPresentationReferenceNumber);

		customsOfficeFirst.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
		AssertEquals("Return empty if not matched", string.Empty, GetProvider().CustomsOfficeOfPresentationReferenceNumber);

		var customsOfficeSecond = declaration.CustomsOfficesForBinding.AddNew();
		customsOfficeSecond.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		customsOfficeSecond.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
		customsOfficeSecond.CY_Data = testDataSecond;
		AssertEquals("Matched with CY_Data", testDataSecond, GetProvider().CustomsOfficeOfPresentationReferenceNumber);

		customsOfficeSecond.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
		AssertEquals("Return empty if not matched", string.Empty, GetProvider().CustomsOfficeOfPresentationReferenceNumber);
	});

	public void TestCustomsOfficeOfExportReferenceNumber() => CombineAssertions(() =>
	{
		const string testData = "PL1";
		AssertEquals("The CustomsOffice Of Export ReferenceNumber should be empty", string.Empty, Provider.CustomsOfficeOfExportReferenceNumber);

		declaration.JE_CustomsOffice = testData;
		AssertEquals("The CustomsOffice Of Export ReferenceNumber should not be null", testData, Provider.CustomsOfficeOfExportReferenceNumber);
	});

	public void TestGoodsShipment() => AssertNotNull(Provider.GoodsShipment);

	public void TestDeclarant() => CombineAssertions(() =>
	{
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNull("No valid declarant", GetProvider().Declarant);

		var declarantHeader = Factory.New<OrgHeader>();
		var declarantAddress = declarantHeader.Addresses.AddNew();
		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		AssertNotNull("Valid declarant address exists", GetProvider().Declarant);
	});

	public void TestRepresentative() => CombineAssertions(() =>
	{
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertNull("The representative is null", GetProvider().Representative);

		var representativeHeader = Factory.New<OrgHeader>();
		var representativeAddress = representativeHeader.Addresses.AddNew();
		declaration.JE_OA_Representative = representativeAddress.PK;
		AssertNotNull("Valid representative address exists", GetProvider().Representative);
	});

	protected override string ExpectedMessageType => Constants.MessageType.AES.CC511C;

	protected override CC511RootProvider GetProvider() => new CC511RootProvider(sendingObject);
}
