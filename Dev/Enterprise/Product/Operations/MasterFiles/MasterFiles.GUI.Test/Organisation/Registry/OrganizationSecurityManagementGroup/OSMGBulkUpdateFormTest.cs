using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OSMGBulkUpdateForm))]
	sealed class OSMGBulkUpdateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OSMGBulkUpdateForm(new OSMGBulkUpdater());
		}

		[RequiresSTA]
		public void TestUpdateButton()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var osmgGroup = Factory.NewWithValidTestData<GlbGroup>();
			osmgGroup.GG_Code = "OG1";
			osmgGroup.GG_Desc = "osmgGroup1";
			Factory.Save();

			var osmgBulkUpdater = new OSMGBulkUpdater();

			using (var form = new OSMGBulkUpdateForm(osmgBulkUpdater))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddUserResponse("Yes");
				UnitTestUserNotification.Instance.AddOKAnswer();

				form.Show();
				osmgBulkUpdater.BulkOrgSecurityGroup = osmgGroup.PK;
				var button = form.Controls.Find("UpdateButton", true).OfType<ZButton>().Single();
				button.PerformClick();

				AssertType<ProgressForm>(ZFormModaliser.LastFormShownForTest);
				var orgMiscServ = new BusinessObjectFactory().LoadTop1<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_OH, org1.PK));
				AssertEquals(orgMiscServ.OM_GG_OrgSecurityGroup, osmgGroup.PK);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.EndsWith(" organization(s) have been updated."));
			}
		}
	}
}
