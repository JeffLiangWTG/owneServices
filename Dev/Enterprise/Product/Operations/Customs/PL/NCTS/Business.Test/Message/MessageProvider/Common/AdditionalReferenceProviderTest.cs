using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class AdditionalReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalReferenceProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsAdditionalInfo", "Value cannot be null.\r\nParameter name: additionalReference",
			() => new AdditionalReferenceProvider(1, null, false));
		AssertNoExceptionThrown("All ok", () => new AdditionalReferenceProvider(1, nctsAdditionalInfo, false));
	});

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestReferenceType() => AssertEquals("123", Provider.ReferenceType);

	public void TestReferenceNumber()
	{
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			AssertEquals("When the validation related to G0321 is not met", "referenceNumber", GetProvider().ReferenceNumber);
			nctsAdditionalInfo.CSI_Code = "aA";
			nctsAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			nctsAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
			AssertEquals("When the validation related to G0321 is met", "0", GetProvider().ReferenceNumber);
		});
	}

	public void TestReferenceNumberMaxLength()
	{
		const int inNCTSTPPeriod = 35;
		const int outNCTSTPPeriod = 70;

		CombineAssertions(() =>
		{
			AssertEquals("ReferenceNumber max length in transition period", inNCTSTPPeriod, new AdditionalReferenceProvider(99, nctsAdditionalInfo, true).ReferenceNumberMaxLength);
			AssertEquals("ReferenceNumber max length outside transition period", outNCTSTPPeriod, GetProvider().ReferenceNumberMaxLength);
		});
	}

	protected override AdditionalReferenceProvider GetProvider() => new AdditionalReferenceProvider(99, nctsAdditionalInfo, false);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		nctsAdditionalInfo = header.AdditionalDocuments.AddNew();
		nctsAdditionalInfo.CSI_Code = "123";
		nctsAdditionalInfo.CSI_ReferenceNumber = "referenceNumber";
	}
	NctsAdditionalInfo nctsAdditionalInfo;
	NctsHeader header;
}
