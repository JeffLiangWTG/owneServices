using System;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(GoodsItemsConfiguration))]
class GoodsItemsConfigurationTest : EU.NCTS.Business.Testing.GoodsItemsConfigurationAbstractTest<GoodsItemsConfiguration>
{
	protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsDepartureCargoDescPhase5ValidationDecider);

	public override void TestImportMethodOfPaymentVisible()
	{
		AssertEquals(false, configuration.ImportMethodOfPaymentVisible(header));
	}

	public override void TestAdditionalInfosSupport()
	{
		AssertEquals(true, configuration.AdditionalInfosSupport(header));
	}

	public override void TestSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.SupportingDocumentsSupport(header));
	}

	public override void TestPreviousDocumentsSupport()
	{
		AssertEquals(true, configuration.PreviousDocumentsSupport(header));
	}

	public override void TestTaxSupport()
	{
		AssertEquals(true, configuration.TaxSupport(header));
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}
	NctsHeader header;
}
