using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccGroupsForm))]
	sealed class TestAccGroupsForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccGroups groups = Factory.New<AccGroups>();
			return new AccGroupsForm(groups);
		}
	}
}
