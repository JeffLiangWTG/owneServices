using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DeniedPartyScreeningStatusDropEditTest : TestCaseWithFactory
	{
		public void TestBackColor()
		{
			var org = Factory.New<OrgHeader>();

			using (var form = new ZForm(org))
			using (var dropEdit = new DeniedPartyScreeningStatusDropEdit())
			{
				form.Controls.Add(dropEdit);
				dropEdit.SetDataBinding(org, OrgHeaderSchema.Constants.OH_ScreeningStatus);
				form.Show();

				AssertEquals("Predcondition: ", ScreeningStatusesList.Codes.NotScreened, org.OH_ScreeningStatus);

				CombineAssertions(() =>
				{
					AssertEquals(DeniedPartyConstants.GridColor.NotScreened, dropEdit.DescriptionBox.BackColor);
					AssertEquals(DeniedPartyConstants.GridColor.NotScreened, dropEdit.CodeBox.BackColor);
				});

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				CombineAssertions(() =>
				{
					AssertEquals(DeniedPartyConstants.GridColor.Clear, dropEdit.DescriptionBox.BackColor);
					AssertEquals(DeniedPartyConstants.GridColor.Clear, dropEdit.CodeBox.BackColor);
				});

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				CombineAssertions(() =>
				{
					AssertEquals(DeniedPartyConstants.GridColor.Matched, dropEdit.DescriptionBox.BackColor);
					AssertEquals(DeniedPartyConstants.GridColor.Matched, dropEdit.CodeBox.BackColor);
				});

				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				CombineAssertions(() =>
				{
					AssertEquals(DeniedPartyConstants.GridColor.Unknown, dropEdit.DescriptionBox.BackColor);
					AssertEquals(DeniedPartyConstants.GridColor.Unknown, dropEdit.CodeBox.BackColor);
				});
			}
		}
	}
}
