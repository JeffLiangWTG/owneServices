using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HRJobRoleForm))]
	public class TestHRJobRoleForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new HRJobRoleForm(Factory.New<HRJobRole>());
		}
	}
}
