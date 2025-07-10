using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public void TestDutyAndTaxDetailsUserControlType()
	{
		using (var form = new Form())
		using (var entryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(entryLineAdditionalDataUserControl);
			form.Show();
			AssertEquals("Using Correct EntryLineTaxAndFeeUserControl", typeof(EntryLineTaxAndFeeUserControl), entryLineAdditionalDataUserControl.DutyAndTaxDetails.UserControlType);
		}
	}
}
