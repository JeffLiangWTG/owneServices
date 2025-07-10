using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(AdditionalInformationProvider))]
sealed class AdditionalInformationProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalInformationProvider>
{
	public void TestSequenceNumeric()
	{
		cusSupportingInfo.CSI_LineNo = 1;
		AssertEquals(1, Provider.SequenceNumeric);
	}

	public void TestStatementCode()
	{
		AssertEquals("C13", Provider.StatementCode);
	}

	public void TestStatementDescription()
	{
		AssertEquals("AddInfoDescription", Provider.StatementDescription);
	}

	public void TestStatementTypeCode()
	{
		AssertNull(Provider.StatementTypeCode);
	}

	public void TestPointers()
	{
		AssertEquals(0, Provider.Pointers.Count);
	}

	protected override AdditionalInformationProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		cusSupportingInfo = Factory.CreateCusSupportingInfo("OTH", "INF", "AddInfoRefNum", null, "C13", header);
		cusSupportingInfo.CSI_Description = "AddInfoDescription";
		provider = new AdditionalInformationProvider(cusSupportingInfo);
	}

	AdditionalInformationProvider provider;
	CusSupportingInfo cusSupportingInfo;
}
