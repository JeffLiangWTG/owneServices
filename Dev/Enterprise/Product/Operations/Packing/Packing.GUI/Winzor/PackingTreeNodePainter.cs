using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.Packing.Business;

namespace Enterprise.Packing.GUI
{
	public class PackingTreeNodePainter : IDisposable
	{
		public PackingTreeNodePainter(PackingTreeView parentTree)
		{
			ParentTree = Argument.NotNull(parentTree, "parentTree");
		}

		public void Initialise()
		{
			if (Tools == null)
			{
				Tools = new GraphicTools();
				Tools.InitialiseTools(ParentTree);
			}
		}

		readonly PackingTreeView ParentTree;
		internal GraphicTools Tools;

		public void PaintText(PackingTreeNode node)
		{
			if (node.IsPackage)
			{
				PaintPackageText(node);
			}
			else if (node.IsPackedItem)
			{
				PaintPackedItemText(node);
			}
			else if (node.IsPackageJob)
			{
				PaintPackageJobText(node);
			}
			else
			{
				throw new NotSupportedException("Attempt to paint a node with an unsupported BizO -- " + node.BizO + ".");
			}
		}

		void PaintPackageText(PackingTreeNode node)
		{
			if (node.NodeFont != Tools.PackageFont)
			{
				node.NodeFont = Tools.PackageFont;
			}

			node.Text = node.Package.ToStringPackageSummary();
		}

		void PaintPackedItemText(PackingTreeNode node)
		{
			if (node.NodeFont != Tools.Font)
			{
				node.NodeFont = Tools.Font;
			}

			var packedItem = node.PackedItem;
			if (packedItem.Code.IsEmpty)
			{
				node.Text = Res.GetString("2015f561-9e63-44ab-8ecf-66ea4fda8d47", "{0}x {1}", packedItem.PackedQty.ToStringTrimZeros(), packedItem.Description);
			}
			else
			{
				node.Text = Res.GetString("31d26407-3207-4ea1-9c24-0f79ad481f1f", "{0}x {1} - {2}", packedItem.PackedQty.ToStringTrimZeros(), packedItem.Code, packedItem.Description);
			}
		}

		void PaintPackageJobText(PackingTreeNode node)
		{
			if (node.NodeFont != Tools.PackageFont)
			{
				node.NodeFont = Tools.PackageFont;
			}

			var parentJob = node.PackageJob.ParentJob;
			if (parentJob != null)
			{
				var jobWithCustomDescription = parentJob as IPackingParentCustomDescription;
				node.Text = (jobWithCustomDescription != null)
					? jobWithCustomDescription.FullJobDescription.ToString()
					: string.Concat(parentJob.JobDescription, " ", parentJob.JobNo);
			}
			else
			{
				node.Text = Res.GetString("e336a4c7-4004-4d99-b5c3-de1fa675d9df", "<not attached to a Job>");
			}
		}

		public void RepaintTopNode()
		{
		}

		public void PaintSummary(PackingTreeNode node)
		{
			node.Refresh();
		}

		public PackingTreeNodeSummary Summary => summary ?? (summary = new PackingTreeNodeSummary());

		PackingTreeNodeSummary summary;

		public void UpdateSummaryAndRepaint(IReadOnlyList<PackingTreeNodeSummaryToken> proposedTokensInDisplayOrder, bool updateTree = true)
		{
			Summary.UpdateTokens(proposedTokensInDisplayOrder);
			if (updateTree)
			{
				ParentTree.Refresh();
			}
		}

		internal class GraphicTools : IDisposable
		{
			public void InitialiseTools(PackingTreeView parentTree)
			{
				Font = new Font(parentTree.Font, FontStyle.Regular);
				PackageFont = new Font(Font, FontStyle.Bold);
				PackageSummaryCaptionFont = new Font("Calibri", 9F);
				PackageSummaryFont = new Font("Calibri", 9.25F, FontStyle.Bold);
				PackageSummaryStatusFont = new Font("Calibri", 9.25F, FontStyle.Bold);

				PackageSummaryColor = Color.FromArgb(18, 97, 225);
				PackageJobSummaryColor = Color.FromArgb(1, 121, 20);
				PackageStatusColor = Color.Gray;

				TextFlags = TextFormatFlags.NoPadding;
				TextProposedSize = new Size(int.MaxValue, int.MaxValue);
			}

			public Color PackageSummaryColor { get; private set; }
			public Color PackageJobSummaryColor { get; private set; }
			public Color PackageStatusColor { get; private set; }
			public TextFormatFlags TextFlags { get; private set; }
			public Size TextProposedSize { get; private set; }

			public Font Font { get; private set; }
			public Font PackageFont { get; private set; }
			public Font PackageSummaryCaptionFont { get; private set; }
			public Font PackageSummaryFont { get; private set; }
			public Font PackageSummaryStatusFont { get; private set; }

			public void Dispose()
			{
				if (Font != null)
				{
					Font.Dispose();
				}
				if (PackageFont != null)
				{
					PackageFont.Dispose();
				}
				if (PackageSummaryCaptionFont != null)
				{
					PackageSummaryCaptionFont.Dispose();
				}
				if (PackageSummaryFont != null)
				{
					PackageSummaryFont.Dispose();
				}
				if (PackageSummaryStatusFont != null)
				{
					PackageSummaryStatusFont.Dispose();
				}
			}
		}

		public void Dispose()
		{
			if (Tools != null)
			{
				Tools.Dispose();
			}
		}
	}
}
