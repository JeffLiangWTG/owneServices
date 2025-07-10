using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCustomsNumberViewStmNumsEditorForm))]
	sealed class USCustomsNumberViewStmNumsEditorFormTest : ZFormBasherTest
	{
		public void TestFountainNameTextBoxIsNotVisible()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var fountainNameTextBox = (ZTextBox)form.Controls.Find("FountainNameTextBox", true)[0];
				AssertEquals(false, fountainNameTextBox.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var provider = Company.CustomsNumberProvider;
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = provider;
			return new USCustomsNumberViewStmNumsEditorForm(new USCustomsNumberViewStmNumsWrapper(stmNum));
		}

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
	}
}
