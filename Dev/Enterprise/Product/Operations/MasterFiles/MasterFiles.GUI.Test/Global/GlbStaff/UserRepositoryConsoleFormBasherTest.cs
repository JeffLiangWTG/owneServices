using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UserRepositoryConsoleForm))]
	sealed class UserRepositoryConsoleFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new UserRepositoryConsoleForm(new UserRepository(Factory, "xxx"));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "ObjectDefinitionTextBox";
		}
	}
}
