using System.Windows.Forms;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GenCustomAddOnRuleForm))]
	sealed class GenCustomAddOnRuleFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new GenCustomAddOnRuleForm(Factory.New<GenCustomAddOnRule>());
		}
	}
}
