using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(QueryCarrierForm))]
	sealed class QueryCarrierFormTest : ZFormBasherTest
	{
		public void TestSendButtonClick()
		{
			QueryCarrierOption option = new QueryCarrierOption(Factory);
			using (QueryCarrierForm form = new QueryCarrierForm(option))
			{
				int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				form.Show();
				option.US_CarrierCode = "1";
				form.SendButton.PerformClick();
				AssertEquals(true, option.HasNotifications());
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				option.US_CarrierCode = "1234";
				form.SendButton.PerformClick();
				AssertEquals(++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		public void TestFormVerb()
		{
			using (QueryCarrierForm form = new QueryCarrierForm(new QueryCarrierOption(Factory)))
			{
				AssertEquals("Query", form.FormVerb);
			}
		}

		public void TestFormCaption()
		{
			using (QueryCarrierForm form = new QueryCarrierForm(new QueryCarrierOption(Factory)))
			{
				AssertEquals("Carrier", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore() => new QueryCarrierForm(new QueryCarrierOption(Factory));
	}
}
