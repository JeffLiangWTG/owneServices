using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(TransportToPrintForm))]
	sealed class TransportToPrintFormTest : ZFormBasherTest
	{
		public void TestFormText()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var transport = declaration.TransportsIncludingRelated.AddNew();
			AssertNull("Precondition:", declaration.TransportToPrint);
			using (TransportToPrintForm form = new TransportToPrintForm(declaration))
			{
				form.Show();
				AssertEquals("Text", "Select Routing", form.Text);
				Application.DoEvents();
				var grid = (ZGrid)form.Controls.Find("TransportsGrid", true)[0];
				grid.Select(0);
				form.AcceptButton.PerformClick();
			}

			AssertEquals("The transport should be selected to print.", transport, declaration.TransportToPrint);
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.TransportsIncludingRelated.AddNew();
			return new TransportToPrintForm(declaration);
		}
	}
}
