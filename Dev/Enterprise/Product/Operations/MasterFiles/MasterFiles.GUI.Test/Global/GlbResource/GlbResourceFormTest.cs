using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbResourceForm))]
	sealed class GlbResourceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			GlbStaff resource = Factory.New<GlbStaff>();
			using (resource.SuspendSettingHasChanges())
			{
				resource.GS_IsResource = true;
			}
			return new GlbResourceForm(resource);
		}
	}
}
