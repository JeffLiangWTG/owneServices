using System.Windows.Forms;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(LoginForm))]
	public class LoginFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new LoginForm(new SecurityOverridenLogin());
		}
	}
}
