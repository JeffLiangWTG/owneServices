using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.GUI
{
	[CodeAlive("CreditReportInformationWindow, deleted in WI00756942, see respective WI's e-doc for details.")]
	public class WebsiteModel : ModelBase<WebsiteModel>
	{
		public WebsiteModel(string originalMainUrl, string newUrl)
		{
			OriginalMainUrl = originalMainUrl;
			NewUrl = newUrl;
			if (OriginalMainUrl == NewUrl)
			{
				SelectedMergeAction = MergeAction.Codes.Ignore;
				MergeActions = MergeAction.GetIgnoreActions();
			}
			else
			{
				SelectedMergeAction = MergeAction.Codes.Add;
				MergeActions = MergeAction.GetAddActions();
			}
		}

		public string OriginalMainUrl { get; }

		public string NewUrl { get; }

		public bool IsMainUrl { get; set; }

		public string SelectedMergeAction { get; set; }

		public Dictionary<string, string> MergeActions { get; }
	}
}
