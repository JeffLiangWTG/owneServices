using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(BulkDeactivationForm))]
	sealed class BulkDeactivationFormTest : ZFormBasherTest
	{
		public void ContinueBtn_ClickTest()
		{
			OrgSupplierBulkDeactivator businessObject = new OrgSupplierBulkDeactivator(Factory);
			using (BulkDeactivationForm form = new BulkDeactivationForm(businessObject))
			{
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new BulkDeactivationForm(new OrgSupplierBulkDeactivator(Factory));
		}
	}
}
