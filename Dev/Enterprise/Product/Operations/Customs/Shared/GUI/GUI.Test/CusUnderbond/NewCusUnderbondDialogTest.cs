using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(NewCusUnderbondDialog))]
	sealed class NewCusUnderbondDialogTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestOKPressedIsSet()
		{
			using (var dialog = new NewCusUnderbondDialog())
			{
				dialog.Show();
				AssertEquals("OKPressed", false, dialog.OKPressed);
				dialog.OKButton_Click(this, new EventArgs());
				AssertEquals("OKPressed", true, dialog.OKPressed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var bizo = new NewCusUnderbondNonPersistent(Array.Empty<ICusUnderbondDependentCollectionParent>());
			return new NewCusUnderbondDialog(bizo);
		}
	}
}
