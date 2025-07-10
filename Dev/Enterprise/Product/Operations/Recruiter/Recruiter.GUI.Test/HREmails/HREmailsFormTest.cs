using System.Windows.Forms;
using Enterprise.Recruiter.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HREmailsForm))]
	public class HREmailsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new HREmailsForm(new HREmailToContactBusinessObjectTest().NewObjectForTest);
	}
}
