using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SetSQLPasswordForm))]
	sealed class SetSQLPasswordFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new SetSQLPasswordForm(Factory.New<GlbStaff>());
			MissingResourceStringChecker.ExcludeFromTest(form.ErrorLabel);
			return form;
		}
	}
}
