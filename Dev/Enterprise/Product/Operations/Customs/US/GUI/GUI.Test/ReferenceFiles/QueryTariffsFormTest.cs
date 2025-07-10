using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(QueryTariffsForm))]
	sealed class QueryTariffsFormTest : ZFormBasherTest
	{
		public void TestSendButtonClick()
		{
			QueryTariff queryTariff = new QueryTariff();
			queryTariff.FromTariff = "";
			QueryTariffOption option = new QueryTariffOption(Factory);
			option.TariffsToQuery.Add(queryTariff);
			using (QueryTariffsForm form = new QueryTariffsForm(option))
			{
				int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				form.Show();
				form.SendButton.PerformClick();
				AssertEquals(true, option.TariffsToQuery.HasNotifications());
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				queryTariff.FromTariff = "2203.00.0030";
				form.SendButton.PerformClick();
				AssertEquals(++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}

			using (QueryTariffsForm form = new QueryTariffsForm(option))
			{
				int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				queryTariff.FromTariff = "3401";
				queryTariff.ToTariff = "6601";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.Show();
				form.SendButton.PerformClick();
				AssertEquals(++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		public void TestFormVerb()
		{
			using (QueryTariffsForm form = new QueryTariffsForm(new QueryTariffOption(Factory)))
			{
				AssertEquals("", form.FormVerb);
			}
		}

		public void TestFormCaption()
		{
			using (QueryTariffsForm form = new QueryTariffsForm(new QueryTariffOption(Factory)))
			{
				AssertEquals("Query Tariffs", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore() => new QueryTariffsForm(new QueryTariffOption(Factory));
	}
}
