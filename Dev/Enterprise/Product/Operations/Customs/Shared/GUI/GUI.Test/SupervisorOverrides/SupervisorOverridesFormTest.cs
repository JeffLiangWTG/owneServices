using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class SupervisorOverridesFormTest : TestCaseWithFactory
	{
		public void TestButtonClick()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			SupervisorOverrides supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);

			using (SupervisorOverridesForm form = new SupervisorOverridesForm(supervisorOverrides))
			{
				form.Show();
				var cancelbutton = (ZButton)form.Controls.Find("DisallowButton", true).FirstOrDefault();
				cancelbutton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}

			using (SupervisorOverridesForm form = new SupervisorOverridesForm(supervisorOverrides))
			{
				form.Show();
				var okbutton = (ZButton)form.Controls.Find("AllowButton", true).FirstOrDefault();
				okbutton.PerformClick();
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}
	}

	[TestedType(typeof(SupervisorOverridesForm))]
	sealed class SupervisorOverridesFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			SupervisorOverrides supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);
			return new SupervisorOverridesForm(supervisorOverrides);
		}
	}
}
