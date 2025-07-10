using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SuppressDocsForOrgForm))]
	sealed class SuppressDocsForOrgFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return new SuppressDocsForOrgForm(org);
		}
	}
}
