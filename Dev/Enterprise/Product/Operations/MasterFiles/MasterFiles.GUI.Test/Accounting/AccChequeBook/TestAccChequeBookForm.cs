using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccChequeBookForm))]
	sealed class TestAccChequeBookForm : ZFormBasherTest
	{
		public void TestAuditPluginIsAdded()
		{
			using (var form = (AccChequeBookForm)GetFormToBashCore())
			{
				AssertNotNull("Cheque Book form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		protected override Form GetFormToBashCore()
		{
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();
			return new AccChequeBookForm(chequeBook);
		}
	}
}
