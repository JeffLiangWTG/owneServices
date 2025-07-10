using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.GUI
{
	[CodeAlive("CreditReportInformationWindow, deleted in WI00756942, see respective WI's e-doc for details.")]
	public class BrandsModel : ModelBase<BrandsModel>
	{
		public BrandsModel(string relatedBrandOrCompanyName)
		{
			RelatedBrandOrCompanyName = relatedBrandOrCompanyName;
			SelectedMergeAction = MergeAction.Codes.Add;
			MergeActions = MergeAction.GetAddActions();
		}

		public string RelatedBrandOrCompanyName { get; }

		public string SelectedMergeAction { get; set; }

		public Dictionary<string, string> MergeActions { get; }
	}
}
