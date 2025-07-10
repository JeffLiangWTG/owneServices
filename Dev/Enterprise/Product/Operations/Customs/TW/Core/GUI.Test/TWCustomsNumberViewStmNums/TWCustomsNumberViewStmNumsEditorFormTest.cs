using System.Windows.Forms;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(TWCustomsNumberViewStmNumsEditorForm))]
	sealed class TWCustomsNumberViewStmNumsEditorFormTest : ZFormBasherTest
	{
		public void TestFountainNameTextBoxIsNotVisible()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var fountainNameTextBox = (ZTextBox)form.Controls.Find("FountainNameTextBox", true)[0];
				AssertEquals(false, fountainNameTextBox.Visible);
				var topPanel = (ZPanel)form.Controls.Find("TopPanel", true)[0];
				AssertEquals(false, topPanel.Visible);
			}
		}

		public void TestCharacterCasingOfTextBox()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var textBox = (ZTextBox)form.Controls.Find("StartNumberTextBox", true)[0];
				AssertEquals(CharacterCasing.Upper, textBox.CharacterCasing);
				textBox = (ZTextBox)form.Controls.Find("CurrentValueTextBox", true)[0];
				AssertEquals(CharacterCasing.Upper, textBox.CharacterCasing);
				textBox = (ZTextBox)form.Controls.Find("EndNumberTextBox", true)[0];
				AssertEquals(CharacterCasing.Upper, textBox.CharacterCasing);
			}
		}

		#region Implementation
		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
		protected override Form GetFormToBashCore()
		{
			var provider = Company.CustomsNumberProvider;
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = provider;
			return new TWCustomsNumberViewStmNumsEditorForm(new TWCustomsNumberViewStmNumsWrapper(stmNum));
		}
		#endregion
	}
}
