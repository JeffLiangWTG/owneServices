using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class PaymentProviderTest : TestCaseWithFactory
	{
		public void TestPaymentTypesMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var entryLines = declaration.EntryLines.ToArray()[0];
				var paymentTypes = entryLines.PaymentTypes.ToArray();

				CombineAssertions("Aviation Fuel Type Provider Test", () =>
				{
					AssertEquals("PaymentTypeCode", "XX", paymentTypes[0].PaymentTypeCode);
					AssertEquals("PaymentAmount", 200m, paymentTypes[0].PaymentAmount);
					AssertEquals("TBFID", "111", paymentTypes[0].TBFID);
				});

				headerJobDeclaration.JE_MessageType = "EXP";
				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				entryLines = declaration.EntryLines.ToArray()[0];
				paymentTypes = entryLines.PaymentTypes.ToArray();

				CombineAssertions("Aviation Fuel Type Provider Test", () =>
				{
					AssertEquals("PaymentTypeCode", "XX", paymentTypes[0].PaymentTypeCode);
					AssertEquals("PaymentAmount", 200m, paymentTypes[0].PaymentAmount);
					AssertEquals("TBFID", ZString.Empty, paymentTypes[0].TBFID);
				});
			}
		}
	}
}
