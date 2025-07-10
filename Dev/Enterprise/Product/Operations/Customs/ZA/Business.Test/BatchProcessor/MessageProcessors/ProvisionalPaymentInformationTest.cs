using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	sealed class ProvisionalPaymentAdditionalInfoTestCase : TestCase
	{
		public void TestProvisionalPaymentAdditionalInfo()
		{
			ProvisionalPaymentAdditionalInfo tester = null;
			CombineAssertions("empty string", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo(ZString.Empty, ZString.Empty);
				AssertEquals(false, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals(ZString.Empty, tester.PPNo);
				AssertEquals(ZString.Empty, tester.DutyType);
				AssertEquals(ZDecimal.Zero, tester.PPAmount);
				AssertEquals(ZDateTime.Empty, tester.ExpiryDate);
			});
			CombineAssertions("random string", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("XXX", ZString.Empty);
				AssertEquals(false, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals(ZString.Empty, tester.PPNo);
				AssertEquals(ZString.Empty, tester.DutyType);
				AssertEquals(ZDecimal.Zero, tester.PPAmount);
				AssertEquals(ZDateTime.Empty, tester.ExpiryDate);
			});
			CombineAssertions("normal", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100662286;!DutyType=PEN;!PPAmount=56.25;!Expiry Date=2016/10/01;'", ZString.Empty);
				AssertEquals(true, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals("100662286", tester.PPNo);
				AssertEquals("PEN", tester.DutyType);
				AssertEquals(56.25m, tester.PPAmount);
				AssertEquals(new ZDateTime(2016, 10, 01), tester.ExpiryDate);
			});
			CombineAssertions("normal with trim of key/value", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=501.75;!Expiry Date =2016/09/23;'", ZString.Empty);
				AssertEquals(true, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals("100516447", tester.PPNo);
				AssertEquals("PEN", tester.DutyType);
				AssertEquals(501.75m, tester.PPAmount);
				AssertEquals(new ZDateTime(2016, 09, 23), tester.ExpiryDate);
			});
			CombineAssertions("missing !", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;PPAmount=501.75;!Expiry Date=2016/09/23;'", ZString.Empty);
				AssertEquals(false, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals("100516447", tester.PPNo);
				AssertEquals("PEN", tester.DutyType);
				AssertEquals(ZDecimal.Zero, tester.PPAmount);
				AssertEquals(new ZDateTime(2016, 09, 23), tester.ExpiryDate);
			});
			CombineAssertions("Zero Amount", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=0.0;!Expiry Date=2016/09/23;'", ZString.Empty);
				AssertEquals(false, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals("100516447", tester.PPNo);
				AssertEquals("PEN", tester.DutyType);
				AssertEquals(ZDecimal.Zero, tester.PPAmount);
				AssertEquals(new ZDateTime(2016, 09, 23), tester.ExpiryDate);
			});
			CombineAssertions("trim of key/value", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=  100516447 ; !DutyType =PEN; !PPAmount=10.0;!Expiry Date=2016/XX/23;'", ZString.Empty);
				AssertEquals(true, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals("100516447", tester.PPNo);
				AssertEquals("PEN", tester.DutyType);
				AssertEquals(10m, tester.PPAmount);
				AssertEquals(ZDateTime.Empty, tester.ExpiryDate);
			});
			CombineAssertions("missing !", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("PPNo=100516447;!DutyType=PEN;PPAmount=501.75;!Expiry Date=2016/09/23;'", ZString.Empty);
				AssertEquals(false, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals(ZString.Empty, tester.PPNo);
				AssertEquals("PEN", tester.DutyType);
				AssertEquals(ZDecimal.Zero, tester.PPAmount);
				AssertEquals(new ZDateTime(2016, 09, 23), tester.ExpiryDate);
			});
			CombineAssertions("missing tailing ;", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=0.10", ZString.Empty);
				AssertEquals(true, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals("100516447", tester.PPNo);
				AssertEquals("PEN", tester.DutyType);
				AssertEquals(0.1m, tester.PPAmount);
				AssertEquals(ZDateTime.Empty, tester.ExpiryDate);
			});
			CombineAssertions("ending with ;", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=0.10;", ZString.Empty);
				AssertEquals(true, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals("100516447", tester.PPNo);
				AssertEquals("PEN", tester.DutyType);
				AssertEquals(0.1m, tester.PPAmount);
				AssertEquals(ZDateTime.Empty, tester.ExpiryDate);
			});
			CombineAssertions("case sensitive", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!Ppno=100516447;!DUTYtype=PEN;!PPAmount=0.10;", ZString.Empty);
				AssertEquals(false, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
				AssertEquals(ZString.Empty, tester.PPNo);
				AssertEquals(ZString.Empty, tester.DutyType);
				AssertEquals(0.1m, tester.PPAmount);
				AssertEquals(ZDateTime.Empty, tester.ExpiryDate);
			});
			CombineAssertions("LineNumber Empty", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=0.10;", ZString.Empty);
				AssertEquals(true, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
			});
			CombineAssertions("LineNumber 0", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=0.10;", "0");
				AssertEquals(true, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
			});
			CombineAssertions("LineNumber 1", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=0.10;", "1");
				AssertEquals(true, tester.IsValid);
				AssertEquals(true, tester.IsHeaderLevelInfo);
			});
			CombineAssertions("LineNumber 2", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=0.10;", "2");
				AssertEquals(true, tester.IsValid);
				AssertEquals(false, tester.IsHeaderLevelInfo);
			});
			CombineAssertions("LineNumber 1 with space", () =>
			{
				tester = new ProvisionalPaymentAdditionalInfo("!PPNo=100516447;!DutyType=PEN;!PPAmount=0.10;", " 1 ");
				AssertEquals(true, tester.IsValid);
				AssertEquals(true, tester.IsHeaderLevelInfo);
			});
		}
	}
}
