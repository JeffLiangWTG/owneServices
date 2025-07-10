using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI.General;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CreateEventRuleControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBinding()
		{
			using (var control = new CreateEventRuleControl())
			using (var form = new ZForm(control))
			{
				form.SetDataBinding(Factory.New<GenCustomAddOnRule>(), "");
				Application.DoEvents();
			}
		}

		[RequiresSTA]
		public void TestHasNoErrors()
		{
			using (var control = new CreateEventRuleControl())
			using (var form = new ZForm(control))
			{
				control.Dock = DockStyle.Fill;
				form.Show();
				control.Show();
				Application.DoEvents();

				var bizo = Factory.New<GenCustomAddOnRule>();
				control.SetDataBinding(bizo, "");
				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder(System.Array.Empty<object>(), bizo.NotificationsIncludingChildren);
			}
		}
	}
}
