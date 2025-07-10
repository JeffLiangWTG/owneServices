using System.Windows.Forms;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(AddProjectLogPopupForm))]
	public class AddProjectLogPopupFormTest : BaseProjectPopupFormTest
	{
		protected override Form GetFormToBashCore()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);
			return new AddProjectLogPopupForm(action);
		}
	}
}
