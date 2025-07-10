using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros.GUI;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class MacroEvaluationToolTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestShowForm()
		{
			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			using (var form = new ZForm(bizObj))
			{
				var tool = new MacroEvaluationTool();
				tool.Run(form);

				Assert("MacroEvaluationForm has been opened",
					Application.OpenForms.OfType<MacroEvaluationForm>().Any());
			}
		}

		public void TestDoNotShowForm()
		{
			var tool = new MacroEvaluationTool();
			tool.Run(null);

			Assert("MacroEvaluationForm has not been opened",
				!Application.OpenForms.OfType<MacroEvaluationForm>().Any());

			AssertEquals("Info message has been shown",
				"Macro Evaluator tool cannot run because must be initialized with a parent form.",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
