using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrganisationViewStmNumsEditorForm))]
	sealed class OrganisationViewStmNumsEditorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			return new OrganisationViewStmNumsEditorForm(stmNum);
		}

		public void TestControlVisibility()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			using (var form = new OrganisationViewStmNumsEditorForm(stmNum))
			{
				form.Show();
				AssertEquals(true, form.ZoneIDPrefixTextBox.Visible);
				AssertEquals(true, form.ClientPrefexTextBox.Visible);
				AssertEquals(false, form.PrefixTextBox.Visible);

				stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
				AssertEquals(true, form.ZoneIDPrefixTextBox.Visible);
				AssertEquals(false, form.ClientPrefexTextBox.Visible);
				AssertEquals(false, form.PrefixTextBox.Visible);

				stmNum.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				AssertEquals(false, form.ZoneIDPrefixTextBox.Visible);
				AssertEquals(false, form.ClientPrefexTextBox.Visible);
				AssertEquals(true, form.PrefixTextBox.Visible);
			}
		}
	}
}
