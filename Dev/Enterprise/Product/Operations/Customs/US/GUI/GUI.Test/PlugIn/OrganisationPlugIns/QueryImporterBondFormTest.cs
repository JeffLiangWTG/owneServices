using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(QueryImporterBondForm))]
	sealed class QueryImporterBondFormTest : ZFormBasherTest
	{
		public void TestQueryImporterBondForm()
		{
			using (QueryImporterBondForm form = (QueryImporterBondForm)GetFormToBashCore())
			{
				AssertEquals(typeof(QueryImporterBondForm), form.GetType());
				AssertEquals(NumberToQueryForTest, form.numberToQuery);
			}
		}

		public void TestbtnCancel_Click()
		{
			using (QueryImporterBondForm form = (QueryImporterBondForm)GetFormToBashCore())
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				form.btnCancel_Click(form.btnCancel, EventArgs.Empty);
				AssertEquals("IsOKToSendMessage", false, form.IsOKToSendMessage);
			}
		}

		public void TestbtnSend_Click()
		{
			using (QueryImporterBondForm form = (QueryImporterBondForm)GetFormToBashCore())
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				form.btnSend_Click(form.btnSend, EventArgs.Empty);
				AssertEquals("IsOKToSendMessage", true, form.IsOKToSendMessage);
			}
		}

		protected override Form GetFormToBashCore() => new QueryImporterBondForm(NumberToQueryForTest);

		const string NumberToQueryForTest = "1234567890";
	}
}
