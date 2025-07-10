using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

class MessageProviderHelperTest : TestCaseWithFactory
{
	public void TestReturnNullIfEmpty()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty String", MessageProviderHelper.ReturnNullIfEmpty(ZString.Empty));
			AssertEquals("Not empty String", "ABC", MessageProviderHelper.ReturnNullIfEmpty(new ZString("ABC")));

			AssertNull("Empty Decimal", MessageProviderHelper.ReturnNullIfEmpty(ZDecimal.Zero));
			AssertEquals("Not empty Decimal", 123.1m, MessageProviderHelper.ReturnNullIfEmpty(new ZDecimal(123.1m)));
		});
	}

	public void TestReturnNullIfInvalid()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty DateTime", MessageProviderHelper.ReturnNullIfInvalid(ZDateTime.Empty));
			AssertEquals("Not empty DateTime", new DateTime(2022, 01, 01), MessageProviderHelper.ReturnNullIfInvalid(new(2022, 01, 01)));
		});
	}

	public void TestIntReturnNullIfEmpty()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty Int String", MessageProviderHelper.IntReturnNullIfEmpty(ZString.Empty));
			AssertNull("Not empty String without Int value", MessageProviderHelper.IntReturnNullIfEmpty(new ZString("ABC")));
			AssertNull("Not empty String with Mixed value case 1", MessageProviderHelper.IntReturnNullIfEmpty(new ZString("231zxc")));
			AssertNull("Not empty String with Mixed value case 2", MessageProviderHelper.IntReturnNullIfEmpty(new ZString("qwe231")));
			AssertNull("Not empty String with Decimal value", MessageProviderHelper.IntReturnNullIfEmpty(new ZString("1.32")));

			AssertEquals("Not empty String with Int value", 231, MessageProviderHelper.IntReturnNullIfEmpty(new ZString("231")));
		});
	}

	public void TestGetLabelingOfTransportAtArrival()
	{
		CombineAssertions(() =>
		{
			AssertNull("Declaration is null", MessageProviderHelper.GetLabelingOfTransportAtArrival(null));

			var declaration = Factory.New<JobDeclaration>();
			AssertNull("Declaration is null", MessageProviderHelper.GetLabelingOfTransportAtArrival(declaration));

			declaration.JE_TransportIDInland = "ABC";
			declaration.JE_Trailer1RegNo = "123";
			declaration.JE_Trailer2RegNo = "456";
			AssertEquals("Not Road type", "ABC", MessageProviderHelper.GetLabelingOfTransportAtArrival(declaration));

			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
			AssertEquals("Not Road type", "ABC/123/456", MessageProviderHelper.GetLabelingOfTransportAtArrival(declaration));

			declaration.JE_TransportIDInland = ZString.Empty;
			AssertEquals("Not Road type", "123/456", MessageProviderHelper.GetLabelingOfTransportAtArrival(declaration));

			declaration.JE_Trailer1RegNo = ZString.Empty;
			AssertEquals("Not Road type", "456", MessageProviderHelper.GetLabelingOfTransportAtArrival(declaration));
		});
	}
}
