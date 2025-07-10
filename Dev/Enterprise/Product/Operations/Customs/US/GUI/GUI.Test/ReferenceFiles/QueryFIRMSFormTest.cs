using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(QueryFIRMSForm))]
	sealed class QueryFIRMSFormTest : ZFormBasherTest
	{
		public void TestSendButtonClick()
		{
			QueryFIRMSOption option = new QueryFIRMSOption(Factory);
			using (QueryFIRMSForm form = new QueryFIRMSForm(option))
			{
				int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				form.Show();
				option.US_FIRMSCode = "123";
				form.SendButton.PerformClick();
				AssertEquals(true, option.HasNotifications());
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				option.US_FIRMSCode = "1234";
				form.SendButton.PerformClick();
				AssertEquals(++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		public void TestFormVerb()
		{
			using (QueryFIRMSForm form = new QueryFIRMSForm(new QueryFIRMSOption(Factory)))
			{
				AssertEquals("Query", form.FormVerb);
			}
		}

		public void TestFormCaption()
		{
			using (QueryFIRMSForm form = new QueryFIRMSForm(new QueryFIRMSOption(Factory)))
			{
				AssertEquals("FIRMS", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore() => new QueryFIRMSForm(new QueryFIRMSOption(Factory));
	}
}
