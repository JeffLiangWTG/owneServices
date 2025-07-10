using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class OpeningSummaryDeclarationProviderTest : TestCaseWithFactory
	{
		public void TestOpeningSummaryDeclarationMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				var openingSumDecs = sumDec.OpeningSummaryDeclaration.ToArray();
				CombineAssertions("Opening Summary Declaration", () =>
				{
					AssertEquals("Manifestlevel | HowToOpen", ZString.Empty, openingSumDecs[0].HowToOpen);
					AssertEquals("Manifestlevel | InWarehouse", "HAYIR", openingSumDecs[0].InWarehouse);
					AssertEquals("Manifestlevel | DeclarationNo", "manif111", openingSumDecs[0].DeclarationNo);
					AssertEquals("Manifestlevel | WillOpenAnotherRegime", "HAYIR", openingSumDecs[0].WillOpenAnotherRegime);
					AssertEquals("Manifestlevel | Explanation", "Manifestlevel", openingSumDecs[0].Explanation);
					AssertEquals("Manifestlevel | OpeningInternalNumber", "", openingSumDecs[0].OpeningInternalNumber);
					AssertEquals("Billlevel | HowToOpen", "2", openingSumDecs[1].HowToOpen);
					AssertEquals("Billlevel | InWarehouse", "HAYIR", openingSumDecs[1].InWarehouse);
					AssertEquals("Billlevel | DeclarationNo", "manif222", openingSumDecs[1].DeclarationNo);
					AssertEquals("Billlevel | WillOpenAnotherRegime", "HAYIR", openingSumDecs[1].WillOpenAnotherRegime);
					AssertEquals("Billlevel | Explanation", "Billlevel", openingSumDecs[1].Explanation);
					AssertEquals("Billlevel | OpeningInternalNumber", "", openingSumDecs[1].OpeningInternalNumber);
					AssertEquals("Billlinelevel | HowToOpen", "3", openingSumDecs[2].HowToOpen);
					AssertEquals("Billlinelevel | InWarehouse", "HAYIR", openingSumDecs[2].InWarehouse);
					AssertEquals("Billlinelevel | DeclarationNo", "manif333", openingSumDecs[2].DeclarationNo);
					AssertEquals("Billlinelevel | WillOpenAnotherRegime", "HAYIR", openingSumDecs[2].WillOpenAnotherRegime);
					AssertEquals("Billlinelevel | Explanation", "Billlinelevel", openingSumDecs[2].Explanation);
					AssertEquals("Billlinelevel | OpeningInternalNumber", "", openingSumDecs[2].OpeningInternalNumber);
				});
			}
		}
	}
}
