using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.Common;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class EnquiryProviderTest : Customs.Business.Testing.DataProviderTestCase<EnquiryProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new EnquiryProvider(null));
	}

	[TestDate(2023, 5, 17)]
	public void TestTC11DeliveryDate()
	{
		CombineAssertions(() =>
		{
			AssertNull(GetProvider().TC11DeliveryDate);

			messageSendingObject.TC11DeliveryDate = ZDateTime.Now;
			AssertEquals("17/05/2023 12:00:00 AM", GetProvider().TC11DeliveryDate.ToString());
		});
	}

	public void TestText()
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, GetProvider().Text);

			messageSendingObject.AdditionalText = "ad";
			AssertEquals("ad", GetProvider().Text);
		});
	}
	protected override EnquiryProvider GetProvider() => new EnquiryProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader);
	}
	MessageSendingObject messageSendingObject;
}
