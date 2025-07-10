using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptForm))]
	public class GlbAccreditationAttemptFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new GlbAccreditationAttemptForm(Factory.New<GlbAccreditationAttempt>())
			{
				ControllerID = ControllerIDs.GlbAccreditationAttempt
			};
			return form;
		}
	}
}
