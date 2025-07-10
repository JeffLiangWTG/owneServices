using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.NCTS.Business.Testing;

[TestedType(typeof(NctsConfiguration))]
sealed class NctsConfigurationTest : EU.NCTS.Business.Testing.NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
{
	public override void TestDocDataPlugInSupport()
	{
		var header = SetUpHeader();
		CombineAssertions(() =>
		{
			AssertEquals("When nctsHeader is null", false, configuration.DocDataPlugInSupport(null));
			AssertEquals("When nctsHeader is departure", false, configuration.DocDataPlugInSupport(header));
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertEquals("When nctsHeader is arrival", false, configuration.DocDataPlugInSupport(header));
		});
	}

	public override void TestFullLoadPortSupport()
	{
		AssertEquals(false, configuration.FullLoadPortSupport);
	}

	public override void TestMiscAdditionalInfosSupport()
	{
		AssertEquals(true, configuration.MiscAdditionalInfosSupport(SetUpHeader()));
	}

	public override void TestMiscGuaranteesSupport()
	{
		AssertEquals(false, configuration.MiscGuaranteesSupport(SetUpHeader()));
	}

	public override void TestMiscPreviousDocumentsSupport()
	{
		AssertEquals(true, configuration.MiscPreviousDocumentsSupport(SetUpHeader()));
	}

	public override void TestMiscSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.MiscSupportingDocumentsSupport(SetUpHeader()));
	}

	public override void TestMiscTabPageSupport()
	{
		AssertEquals(false, configuration.MiscTabPageSupport(SetUpHeader()));
	}

	public override void TestReceiveIE043UnloadingPermissionDetailsMessage()
	{
		AssertEquals(true, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);
	}

	public override void TestUseAdditionalDeclarationType()
	{
		AssertEquals(false, configuration.UseAdditionalDeclarationType);
	}

	public override void TestUsePresentationDateTime()
	{
		AssertEquals(false, configuration.UsePresentationDateTime);
	}

	public override void TestUseUniversalFeeCalculation()
	{
		AssertEquals(false, configuration.UseUniversalFeeCalculation);
	}

	protected override Type GetMessageSendingConfigurationTypeForTest() => typeof(MessageSendingConfiguration);

	NctsHeader SetUpHeader()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		return header;
	}
}
