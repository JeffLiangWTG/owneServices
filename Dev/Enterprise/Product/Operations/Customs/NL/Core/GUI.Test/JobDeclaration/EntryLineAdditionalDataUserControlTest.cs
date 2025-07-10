using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public static void TestDutyAndTaxDetailsUserControlType()
	{
		using (var form = new ZForm())
		using (var entryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(entryLineAdditionalDataUserControl);
			form.Show();
			AssertEquals("Using Correct EntryLineTaxAndFeeUserControl", typeof(EntryLineTaxAndFeeUserControl), entryLineAdditionalDataUserControl.DutyAndTaxDetails.UserControlType);
		}
	}
}
