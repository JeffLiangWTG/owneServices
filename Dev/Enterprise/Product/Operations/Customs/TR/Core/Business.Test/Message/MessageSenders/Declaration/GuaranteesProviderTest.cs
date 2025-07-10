using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class GuaranteesProviderTest : TestCaseWithFactory
	{
		public void TestGuaranteeMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var guarantes = declaration.Guarantees.ToArray();

				CombineAssertions("Guarantees Provider GLOBAL Test", () =>
				{
					AssertEquals("GuaranteesType", "GLOBAL", guarantes[0].GuaranteesType);
					AssertEquals("GuaranteesRatio", 0m, guarantes[0].GuaranteesRatio);
					AssertEquals("LetterOfBankGuaranteeAmount", 100m, guarantes[0].LetterOfBankGuaranteeAmount);
					AssertEquals("GuaranteeAmountForCash", 0m, guarantes[0].GuaranteeAmountForCash);
					AssertEquals("OtherAmount", 0m, guarantes[0].OtherAmount);
					AssertEquals("GlobalGuaranteeNo", "string", guarantes[0].GlobalGuaranteeNo);
					AssertEquals("Description", "string", guarantes[0].Description);
					AssertEquals("OtherAmountReference", string.Empty, guarantes[0].OtherAmountReference);
				});

				CombineAssertions("Guarantees Provider DIGER Test", () =>
				{
					AssertEquals("GuaranteesType", "DIGER", guarantes[1].GuaranteesType);
					AssertEquals("GuaranteesRatio", 0m, guarantes[1].GuaranteesRatio);
					AssertEquals("LetterOfBankGuaranteeAmount", 0m, guarantes[1].LetterOfBankGuaranteeAmount);
					AssertEquals("GuaranteeAmountForCash", 0m, guarantes[1].GuaranteeAmountForCash);
					AssertEquals("OtherAmount", 100m, guarantes[1].OtherAmount);
					AssertEquals("GlobalGuaranteeNo", "string", guarantes[1].GlobalGuaranteeNo);
					AssertEquals("Description", "string", guarantes[1].Description);
					AssertEquals("OtherAmountReference", "string", guarantes[1].OtherAmountReference);
				});
			}
		}
	}
}
