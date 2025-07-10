using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(TaskSkillsForm))]
	sealed class TaskSkillsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var task = Factory.New<ProcessTask>();
			TaskSkillsForm result = new TaskSkillsForm(task);
			return result;
		}
	}
}
