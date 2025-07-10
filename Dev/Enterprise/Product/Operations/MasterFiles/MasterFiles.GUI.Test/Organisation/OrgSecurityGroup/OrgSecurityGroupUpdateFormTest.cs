using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgSecurityGroupUpdateForm))]
	sealed class OrgSecurityGroupUpdateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var form = new OrgSecurityGroupUpdateForm(new[] { org.MiscServ });
			return form;
		}

		public void TestSave()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = group1.PK;
			org2.MiscServ.OM_GG_OrgSecurityGroup = group2.PK;
			org3.MiscServ.OM_GG_OrgSecurityGroup = ZGuid.Empty;
			Factory.Save();

			using (var form = new OrgSecurityGroupUpdateForm(new[] { org1.MiscServ, org2.MiscServ, org3.MiscServ }))
			{
				form.Show();
				Application.DoEvents();
				org1.MiscServ.OM_GG_OrgSecurityGroup = group2.PK;
				form.FireSaveButton();
				form.Close();
			}

			AssertEquals(group2.PK, org1.MiscServ.OM_GG_OrgSecurityGroup);
			AssertEquals(group2.PK, org2.MiscServ.OM_GG_OrgSecurityGroup);
			AssertEquals(group2.PK, org3.MiscServ.OM_GG_OrgSecurityGroup);
		}
	}
}
