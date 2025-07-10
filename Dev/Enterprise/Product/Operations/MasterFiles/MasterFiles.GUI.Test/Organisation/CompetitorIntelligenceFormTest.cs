using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ZCompetitorIntelligenceForm))]
	sealed class CompetitorIntelligenceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			return new ZCompetitorIntelligenceForm(organisation);
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}
	}
}
