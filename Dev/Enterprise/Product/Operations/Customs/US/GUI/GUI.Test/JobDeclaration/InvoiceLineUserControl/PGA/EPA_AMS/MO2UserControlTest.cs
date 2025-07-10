using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class MO2UserControlTest : TestCaseWithFactory
	{
		public void TestAMSGridHasCertNumberIssueDateIsDocSubmitted()
		{
			using (var control2 = new MO2UserControl())
			{
				var grid = (ZGrid)control2.Controls[0].Controls.Find("MO2Grid", true)[0];
				var column4 = grid.GetColumnStyle("US_CertNumber");
				AssertEquals("AMSLine.Schema.US_CertNumber", true, column4.IsVisible);
				var column5 = grid.GetColumnStyle("US_IssueDate");
				AssertEquals("AMSLine.Schema.US_IssueDate", true, column5.IsVisible);
				var column6 = grid.GetColumnStyle("US_IsDocSubmitted");
				AssertEquals("AMSLine.Schema.US_IsDocSubmitted", true, column6.IsVisible);
				var column7 = grid.GetColumnStyle("US_NetWeight");
				AssertEquals("AMSLine.Schema.US_NetWeight", true, column7.IsVisible);
				var column8 = grid.GetColumnStyle("US_NetWeightUQ");
				AssertEquals("AMSLine.Schema.US_NetWeightUQ", true, column8.IsVisible);
			}
		}
	}
}
