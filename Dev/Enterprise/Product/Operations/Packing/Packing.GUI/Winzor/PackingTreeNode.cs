using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Packing.Business;
using static Enterprise.Packing.GUI.PackingTreeNodePainter;

namespace Enterprise.Packing.GUI
{
	partial class PackingTreeNode
	{
		protected override List<string> SummaryTokens => GetSummaryTokens();
		protected override string SummaryStatus => GetSummaryStaus();

		List<string> GetSummaryTokens()
		{
			if ((!IsPackageJob && !IsPackage) || TreeView is null)
			{
				return Enumerable.Empty<string>().ToList();
			}

			var tokensContent = new List<string>();
			var packageSummary = (IPackageSummary)BizO;

			SetTokensStyle(TreeView.NodePainter.Tools);

			tokensContent.Add("(");
			foreach (var summaryToken in TreeView.NodePainter.Summary.GetOrderedVisibleTokens())
			{
				var caption = summaryToken.GetCaption(packageSummary);
				var text = summaryToken.GetText(packageSummary);
				if (!caption.IsEmpty)
				{
					tokensContent.Add($"{caption}: ");
				}
				if (!text.IsEmpty)
				{
					tokensContent.Add($"{text}");
					tokensContent.Add(", ");
				}
			}
			tokensContent.RemoveAt(tokensContent.Count - 1);
			tokensContent.Add(")");

			return tokensContent;
		}

		string GetSummaryStaus()
		{
			if ((!IsPackageJob && !IsPackage) || TreeView is null)
			{
				return string.Empty;
			}

			var tokensContent = new List<string>();
			var packageSummary = (IPackageSummary)BizO;

			SetStatusStyle(TreeView.NodePainter.Tools);

			return packageSummary.Status;
		}

		void SetStatusStyle(GraphicTools tool)
		{
			StatusColor = tool.PackageStatusColor;
			StatusFont = tool.PackageSummaryStatusFont;
		}

		void SetTokensStyle(GraphicTools graphicTools)
		{
			TokensColor = IsPackageJob ? graphicTools.PackageJobSummaryColor : graphicTools.PackageSummaryColor;
			TokensCaptionFont = graphicTools.PackageSummaryCaptionFont;
			TokensFont = graphicTools.PackageSummaryFont;
		}
	}
}
