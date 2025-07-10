using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgSecurityProfileUpdateForm))]
	sealed class OrgSecurityUpdateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var profile = new OrgSecurityProfileUpdater() { ProfileName = "Default" };
			profile.HasChanges = false;
			return new OrgSecurityProfileUpdateForm(profile);
		}
	}
}
