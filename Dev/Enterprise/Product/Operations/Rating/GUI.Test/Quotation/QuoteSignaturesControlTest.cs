using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Testing
{
	public class QuoteSignaturesControlTest : TestCaseWithFactory
	{
		public void TestSignatureButtons()
		{
			var rep = Factory.New<GlbStaff>();
			rep.GS_FullName = "Test Rep";
			rep.GS_Code = "TR";

			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();

			using (QuotationForm testForm = new QuotationForm(testQuote))
			{
				testForm.Show();

				QuoteTabControl tabControl = (QuoteTabControl)testForm.BaseTabControl;
				ZTabPage quoteFormatTabPage = GetQuoteFormatTabPage(tabControl);
				tabControl.TopLevelTabControl.SelectedTab = quoteFormatTabPage;
				QuoteSignaturesControl signaturesControl = (QuoteSignaturesControl)quoteFormatTabPage.Controls[0];

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				signaturesControl.OverallRepButton1.PerformClick();
				AssertEquals("There is no Overall Rep for this client.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				signaturesControl.OverallRepButton2.PerformClick();
				AssertEquals("There is no Overall Rep for this client.", UnitTestUserNotification.Instance.LastMessage.Text);

				testQuote.Header.StaffAssignments.OverallSalesRep = rep.GS_Code;

				signaturesControl.OverallRepButton1.PerformClick();
				AssertEquals(rep.GS_Code, testQuote.TH_GS_NKFirstSignatory);
				signaturesControl.CurrentUserButton1.PerformClick();
				AssertEquals(GlbStaff.CurrentUser.GS_Code, testQuote.TH_GS_NKFirstSignatory);

				signaturesControl.OverallRepButton2.PerformClick();
				AssertEquals(rep.GS_Code, testQuote.TH_GS_NKSecondSignatory);
				signaturesControl.CurrentUserButton2.PerformClick();
				AssertEquals(GlbStaff.CurrentUser.GS_Code, testQuote.TH_GS_NKSecondSignatory);
			}
		}

		ZTabPage GetQuoteFormatTabPage(QuoteTabControl tabControl)
		{
			foreach (ZTabPage page in tabControl.TopLevelTabControl.TabPages)
			{
				if (page.Name == "QuoteFormatTabPage")
				{
					return page;
				}
			}

			return null;
		}

		#region Helper

		protected Business.Testing.TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new Business.Testing.TestHelper(Factory);
				}

				return fHelper;
			}
		}

		Business.Testing.TestHelper fHelper;

		#endregion
	}
}
