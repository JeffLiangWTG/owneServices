using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(StampDutyLedgerDocumentEntryForm))]
	public class StampDutyLedgerDocumentEntryFormTest : ZFormBasherTest
	{
		public void TestInitializeDefaultDate()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var stampDutyLedgerDocumentEntryForm = new StampDutyLedgerDocumentEntryForm(statement))
			{
				var printDateDateEdit = (ZDateEdit)(stampDutyLedgerDocumentEntryForm.Controls.Find("PrintDateDateEdit", true).Single());

				stampDutyLedgerDocumentEntryForm.Show();
				AssertEquals("DateEdit has the default today", ZDateTime.Today, printDateDateEdit.DateTimeValue);
			}
		}

		public void TestBtnOk()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.FindSingle<Button>("btnOk").PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCloseButton()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.FindSingle<Button>("btnClose").PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var statement = Factory.New<CusStatementHeader>();
			return new StampDutyLedgerDocumentEntryForm(statement);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "PrintDateDateEdit";
		}
	}
}
