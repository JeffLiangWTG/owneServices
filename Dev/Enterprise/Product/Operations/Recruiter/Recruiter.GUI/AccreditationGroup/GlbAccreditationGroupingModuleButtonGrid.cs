using System;
using System.Linq;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	public class GlbAccreditationGroupingModuleButtonGrid : ZModuleButtonGrid
	{
		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			if (InnerGrid.SelectedElements.Cast<GlbAccreditation>().Any(x => x.HAC_IsRefresher))
			{
				var error = Res.GetString("c1e539c3-93aa-481b-bde8-5d8d70a4bc39", "Refresher Accreditations should not be selected for detach. Detach the Refresher's Main Accreditation and the Refresher will automatically be detached.");
				var caption = Res.GetString("5426bc65-ca99-400c-9bd3-422584ff3953", "Detach Refresher not allowed");

				Globals.Message.ShowError(error, caption);
			}
			else
			{
				base.DetachButton_Click(sender, e);
			}
		}
	}
}
