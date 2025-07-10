using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class ContainerProviderTest : TestCaseWithFactory
	{
		public void TestContainerInfoMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var entryLines = declaration.EntryLines.ToArray()[0];
				var containerInfo = entryLines.ContainerInfo.ToArray();

				CombineAssertions("CusContainer InvoiceLine Pivot Provider Test", () =>
				{
					AssertEquals("1.Container | ContainerNo", "11111", containerInfo[0].ContainerNo);
					AssertEquals("1.Container | CountryCode", "052", containerInfo[0].CountryCode);

					AssertEquals("2.Container | ContainerNo", "22222", containerInfo[1].ContainerNo);
					AssertEquals("2.Container | CountryCode", "006", containerInfo[1].CountryCode);
				});
			}
		}
	}
}
