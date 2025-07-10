using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgProfitShareForm))]
	sealed class OrgProfitShareFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OrgProfitShareForm(Factory.New<OrgAgentRelationship>());
		}
	}
}
