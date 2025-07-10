using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class PreviousDocumentsProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationOpeningAndClosingInfoMember()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var entrylines = declaration.EntryLines.ToArray();

				var openingAndClosingInfo = entrylines[0].DeclarationOpeningAndClosingInfo.ToArray();
				CombineAssertions("Opening And Closing Info Test", () =>
				{
					AssertEquals("ClosedDeclarationNo", "11111", openingAndClosingInfo[0].ClosedDeclarationNo);
					AssertEquals("ClosedDeclarationLineNo", 1, openingAndClosingInfo[0].ClosedDeclarationLineNo);
					AssertEquals("ClosedQuantity", 20m, openingAndClosingInfo[0].ClosedQuantity);
					AssertEquals("Description", "pre desc1 pre desc2", openingAndClosingInfo[0].Description);
				});

				var openingAndClosingInfo2 = entrylines[1].DeclarationOpeningAndClosingInfo.ToArray();
				CombineAssertions("Opening And Closing Info Test", () =>
				{
					AssertEquals("ClosedDeclarationNo", "11111", openingAndClosingInfo2[0].ClosedDeclarationNo);
					AssertEquals("ClosedDeclarationLineNo", 1, openingAndClosingInfo2[0].ClosedDeclarationLineNo);
					AssertEquals("ClosedQuantity", 8m, openingAndClosingInfo2[0].ClosedQuantity);
					AssertEquals("Description", "pre desc1", openingAndClosingInfo2[0].Description);

					AssertEquals("ClosedDeclarationNo", "22222", openingAndClosingInfo2[1].ClosedDeclarationNo);
					AssertEquals("ClosedDeclarationLineNo", 1, openingAndClosingInfo2[1].ClosedDeclarationLineNo);
					AssertEquals("ClosedQuantity", 7m, openingAndClosingInfo2[1].ClosedQuantity);
					AssertEquals("Description", "pre desc4", openingAndClosingInfo2[1].Description);
				});

				headerJobDeclaration.JE_MessageType = "EXP";
				var declaration2 = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var entrylines2 = declaration2.EntryLines.ToArray();
				var openingAndClosingInfo3 = entrylines2[0].DeclarationOpeningAndClosingInfo.ToArray();

				AssertEquals("No Opening And Closing expected", 0, openingAndClosingInfo3.Length);
			}
		}
	}
}
