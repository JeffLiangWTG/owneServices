using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsConfiguration))]
sealed class NctsConfigurationTest : EU.NCTS.Business.Testing.NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
{
	public override void TestDocDataPlugInSupport() => CombineAssertions(() =>
	{
		AssertEquals("When nctsHeader is null, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(nctsHeader: null));
		AssertEquals("When nctsHeader is not null, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(header));
	});

	public void TestHeaderDeparturePhase5ValidationDecider()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType(typeof(NctsHeaderDeparturePhase5ValidationDecider), configuration.GetValidationDecider(header));
	}

	public override void TestFullLoadPortSupport() => AssertEquals(false, configuration.FullLoadPortSupport);

	public override void TestMiscAdditionalInfosSupport() => AssertEquals(true, configuration.MiscAdditionalInfosSupport(header));

	public override void TestMiscGuaranteesSupport() => AssertEquals(false, configuration.MiscGuaranteesSupport(header));

	public override void TestMiscPreviousDocumentsSupport() => AssertEquals(true, configuration.MiscPreviousDocumentsSupport(header));

	public override void TestMiscSupportingDocumentsSupport() => AssertEquals(true, configuration.MiscSupportingDocumentsSupport(header));

	public override void TestMiscTabPageSupport() => CombineAssertions(() =>
	{
		AssertEquals("When nctsHeader is null, MiscTabPageSupport", false, configuration.MiscTabPageSupport(nctsHeader: null));
		AssertEquals("When nctsHeader is not null, MiscTabPageSupport", false, configuration.MiscTabPageSupport(header));
	});

	public override void TestReceiveIE043UnloadingPermissionDetailsMessage() => AssertEquals(true, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);

	public override void TestUseAdditionalDeclarationType() => AssertEquals(true, configuration.UseAdditionalDeclarationType);

	public override void TestUsePresentationDateTime() => AssertEquals(false, configuration.UsePresentationDateTime);

	public override void TestUseUniversalFeeCalculation() => AssertEquals(false, configuration.UseUniversalFeeCalculation);

	protected override Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

	protected override Type GetMessageSendingConfigurationTypeForTest() => typeof(MessageSendingConfiguration);

	protected override Type GetLocationOfGoodsFromAuthorisationDefaulterConfigurationTypeForTest() => typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration);

	protected override Type GetBillConfigurationTypeForTest() => typeof(BillConfiguration);

	protected override Type GetNctsPackageConfigurationTypeForTest() => typeof(NctsPackageConfiguration);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<EU.NCTS.Business.NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}

	EU.NCTS.Business.NctsHeader header;
}
