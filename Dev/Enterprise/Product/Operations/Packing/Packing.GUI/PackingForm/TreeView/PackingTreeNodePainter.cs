using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI;
using Enterprise.Packing.Business;

namespace Enterprise.Packing.GUI
{
	public class PackingTreeNodePainter : IDisposable
	{
		#region Construction

		public PackingTreeNodePainter(PackingTreeView parentTree)
		{
			ParentTree = Argument.NotNull(parentTree, "parentTree");
			DisposableLeakListener.Instance.RegisterDisposable(this);
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
		GraphicTools Tools;

		#endregion

		#region Graphic Tools

		class GraphicTools : IDisposable
		{
			public void InitialiseTools(PackingTreeView parentTree)
			{
				DisposableLeakListener.Instance.RegisterDisposable(this);

				// fonts
				Font = new Font(parentTree.Font, FontStyle.Regular);
				PackageFont = new Font(Font, FontStyle.Bold);
				PackageSummaryCaptionFont = new Font("Calibri", 9F);
				PackageSummaryFont = new Font("Calibri", 9.25F, FontStyle.Bold);
				PackageSummaryStatusFont = new Font("Calibri", 9.25F, FontStyle.Bold);

				// brushes and pens
				BackgroundBrush = new SolidBrush(parentTree.BackColor);
				DottedLinePen = new Pen(parentTree.LineColor, 1);
				DottedLinePen.DashStyle = DashStyle.Dot;

				// colours
				PackageSummaryColor = Color.FromArgb(18, 97, 225);
				PackageJobSummaryColor = Color.FromArgb(1, 121, 20);
				PackageStatusColor = Color.Gray;

				// other
				TextFlags = TextFormatFlags.NoPadding;
				TextProposedSize = new Size(int.MaxValue, int.MaxValue);
			}

			public Color PackageSummaryColor { get; private set; }
			public Color PackageJobSummaryColor { get; private set; }
			public Color PackageStatusColor { get; private set; }
			public TextFormatFlags TextFlags { get; private set; }
			public Size TextProposedSize { get; private set; }

			// IDisposable
			public Font Font { get; private set; }
			public Font PackageFont { get; private set; }
			public Font PackageSummaryCaptionFont { get; private set; }
			public Font PackageSummaryFont { get; private set; }
			public Font PackageSummaryStatusFont { get; private set; }
			public SolidBrush BackgroundBrush { get; private set; }
			public Pen DottedLinePen { get; private set; }

			#region Dispose

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

				if (BackgroundBrush != null)
				{
					BackgroundBrush.Dispose();
				}

				if (DottedLinePen != null)
				{
					DottedLinePen.Dispose();
				}

				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}

			#endregion
		}

		#endregion

		#region Summary

		public PackingTreeNodeSummary Summary => summary ?? (summary = new PackingTreeNodeSummary());

		public void UpdateSummaryAndRepaint(IReadOnlyList<PackingTreeNodeSummaryToken> proposedTokensInDisplayOrder, bool updateTree = true)
		{
			Summary.UpdateTokens(proposedTokensInDisplayOrder);
			if (updateTree)
			{
				RepaintAllSummaries();
			}
		}

		void RepaintAllSummaries()
		{
			try
			{
				// Make the tree Visible false and then true at the end to prevent the GDI out of memory Microsoft bug.
				// See https://stackoverflow.com/questions/16147635/out-of-memory-exception-in-net-winform-treeview
				ParentTree.Visible = false;
				RepaintAllSummaries(ParentTree.Nodes, Summary.GetOrderedVisibleTokens());
			}
			finally
			{
				ParentTree.Visible = true;
			}

			void RepaintAllSummaries(TreeNodeCollection nodes, IReadOnlyCollection<PackingTreeNodeSummaryToken> summaryTokens)
			{
				foreach (PackingTreeNode node in nodes)
				{
					if (node.IsVisible)
					{
						PaintSummary(node, summaryTokens); // this is much faster than calling Paint() (which repaints the text + dottedline + summary)
					}
					if (node.IsExpanded)
					{
						RepaintAllSummaries(node.Nodes, summaryTokens);
					}
				}
			}
		}

		PackingTreeNodeSummary summary;

		#endregion

		#region PaintText

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

		#region PaintPackageText

		void PaintPackageText(PackingTreeNode node)
		{
			if (node.NodeFont != Tools.PackageFont)
			{
				node.NodeFont = Tools.PackageFont;
			}

			node.Text = node.Package.ToStringPackageSummary();
		}

		#endregion

		#region PaintPackedItemText

		void PaintPackedItemText(PackingTreeNode node)
		{
			if (node.NodeFont != Tools.Font)
			{
				node.NodeFont = Tools.Font;
			}

			var packedItem = node.PackedItem;
			node.Text = Res.GetString("2015f561-9e63-44ab-8ecf-66ea4fda8d47", "{0}x {1}", packedItem.PackedQty.ToStringTrimZeros(), packedItem.CodeWithDescription);
		}

		#endregion

		#region PaintPackageJobText

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

		#endregion

		#endregion

		#region PaintSummary

		public void PaintSummary(PackingTreeNode node) => PaintSummary(node, Summary.GetOrderedVisibleTokens());

		void PaintSummary(PackingTreeNode node, IReadOnlyCollection<PackingTreeNodeSummaryToken> summaryTokens)
		{
			if (node.IsVisible)
			{
				using (var g = ParentTree.CreateGraphics())
				{
					var e = new DrawTreeNodeEventArgs(g, node, node.Bounds, TreeNodeStates.Default);
					PaintSummary(e, summaryTokens);
				}
			}
		}

		public void PaintSummary(DrawTreeNodeEventArgs e) => PaintSummary(e, Summary.GetOrderedVisibleTokens());

		void PaintSummary(DrawTreeNodeEventArgs e, IReadOnlyCollection<PackingTreeNodeSummaryToken> summaryTokens)
		{
			if (e.Node.IsVisible)
			{
				var node = (PackingTreeNode)e.Node;

				if (node.IsPackage)
				{
					PaintSummaryCore(e, summaryTokens, Tools.PackageSummaryColor, Tools.PackageStatusColor);
				}
				else if (node.IsPackageJob)
				{
					PaintSummaryCore(e, summaryTokens, Tools.PackageJobSummaryColor, Color.Empty, isPackageJob: true);
				}
			}
		}

		void PaintSummaryCore(DrawTreeNodeEventArgs e, IReadOnlyCollection<PackingTreeNodeSummaryToken> summaryTokens, Color color, Color statusColour, bool isPackageJob = false)
		{
			var node = (PackingTreeNode)e.Node;
			var packageSummary = (IPackageSummary)node.BizO;
			var tokens = GetPackageSummaryTokens(packageSummary, summaryTokens);

			var realTextOnNode = TextRenderer.MeasureText(e.Graphics, node.Text, node.NodeFont, Tools.TextProposedSize, Tools.TextFlags);
			var xPos = e.Node.Bounds.Left + realTextOnNode.Width;
			var yPos = e.Node.Bounds.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(2);

			if (tokens.Count > 0)
			{
				xPos += TokenGap;

				// clear the target area, otherwise redrawing on mouse over will darken the text due to repeated anti-aliasing calculation
				// (Height +2 to allow for Release Tag height, and text that dips below (eg. the "g" in "Wgt"))
				var textArea = ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(xPos),
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(yPos),
					ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ParentTree.Width),
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(realTextOnNode.Height) + 2);
				e.Graphics.FillRectangle(Tools.BackgroundBrush, textArea);

				// draw opening "("
				TextRenderer.DrawText(e.Graphics, "(", Tools.PackageSummaryCaptionFont, ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(xPos), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(yPos)), color);
				xPos += ControlDpiScalingHelper.ScaleToCurrentDpiX(4);

				// draw the tokens
				for (var i = 0; i < tokens.Count - 1; i++)
				{
					xPos += PaintSummaryToken(e.Graphics, tokens[i], xPos, yPos, color);
				}
				xPos += PaintSummaryToken(e.Graphics, tokens[tokens.Count - 1], xPos, yPos, color, suppressComma: true);

				// draw closing ")"
				TextRenderer.DrawText(e.Graphics, ")", Tools.PackageSummaryCaptionFont, ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(xPos), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(yPos)), color);
			}

			// draw status
			PaintSummaryStatus(e.Graphics, packageSummary, statusColour, xPos, yPos, isPackageJob);
		}

		#region Tokens

		int PaintSummaryToken(Graphics g, (string Caption, string Text) token, int xPos, int yPos, Color color, bool suppressComma = false)
		{
			return PaintSummaryToken(g, token, xPos, yPos, color, Tools.PackageSummaryFont, suppressComma);
		}

		int PaintSummaryToken(Graphics g, (string Caption, string Text) token, int xPos, int yPos, Color color, Font font, bool suppressComma = false)
		{
			// draw the caption
			var captionWidth = 0;
			if (token.Caption.Length > 0)
			{
				captionWidth = TextRenderer.MeasureText(g, token.Caption, Tools.PackageSummaryCaptionFont, Tools.TextProposedSize, Tools.TextFlags).Width;
				TextRenderer.DrawText(g, token.Caption, Tools.PackageSummaryCaptionFont, ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(xPos), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(yPos) + 1), color);
			}

			// draw the text (value)
			var textWidth = TextRenderer.MeasureText(g, token.Text, font, Tools.TextProposedSize, Tools.TextFlags).Width;
			TextRenderer.DrawText(g, token.Text, font, ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(xPos + captionWidth), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(yPos)), color);

			// draw the comma
			var commaWidth = 0;
			var tokenGap = 0;
			if (!suppressComma)
			{
				commaWidth = TextRenderer.MeasureText(g, ",", Tools.PackageSummaryCaptionFont, Tools.TextProposedSize, Tools.TextFlags).Width;
				tokenGap = TokenGap;
				TextRenderer.DrawText(g, ",", Tools.PackageSummaryCaptionFont, ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(xPos + captionWidth + textWidth), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(yPos) + 1), color);
			}

			// return the start position for the next drawstring
			return captionWidth + textWidth + commaWidth + tokenGap;
		}

		List<(string Caption, string Text)> GetPackageSummaryTokens(IPackageSummary packageSummary, IReadOnlyCollection<PackingTreeNodeSummaryToken> summaryTokens)
		{
			var tokens = new List<(string, string)>(summaryTokens.Count);

			foreach (var summaryToken in summaryTokens)
			{
				var caption = summaryToken.GetCaption(packageSummary);
				var text = summaryToken.GetText(packageSummary);

				if (!caption.IsEmpty || !text.IsEmpty)
				{
					var updatedCaption = !caption.IsEmpty ? caption + ": " : string.Empty;
					tokens.Add((updatedCaption, text));
				}
			}

			return tokens;
		}

		const int TokenGap = 5;

		#endregion

		#region Status Tag

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-system")]
		void PaintSummaryStatus(Graphics g, IPackageSummary packageSummary, Color statusColour, int xPos, int yPos, bool isPackageJob = false)
		{
			if (isPackageJob)
			{
				PaintPackageJobSummaryStatusTag(g, packageSummary, xPos, yPos);
			}
			else
			{
				var status = packageSummary.Status;
				if (!status.IsEmpty)
				{
					var closedToken = (string.Empty, "   –  " + status);
					PaintSummaryToken(g, closedToken, xPos, yPos, statusColour, Tools.PackageSummaryStatusFont, suppressComma: true);
				}
			}
		}

		void PaintPackageJobSummaryStatusTag(Graphics g, IPackageSummary packageSummary, int xPos, int yPos)
		{
			var status = packageSummary.Status;
			if (!status.IsEmpty)
			{
				var darkTag = packageSummary.IsStatusSignificant;
				var text = (string)status.ToUpper();

				// draw the left edge
				var roundedEdge = darkTag ? Properties.Resources.ReleasedViaPackagesEdge : Properties.Resources.ReleasedViaJobEdge;
				const int gapFromSummary = 17;
				var imageX = xPos + ControlDpiScalingHelper.ScaleToCurrentDpiX(gapFromSummary);
				var imageY = yPos + ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
				var imageW = ControlDpiScalingHelper.ScaleToCurrentDpiX(roundedEdge.Width);
				var imageH = ControlDpiScalingHelper.ScaleToCurrentDpiX(roundedEdge.Height);
				g.DrawImage(roundedEdge, imageX, imageY, imageW, imageH);

				var textEndPadding = ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
				var textWidth = textEndPadding + TextRenderer.MeasureText(g, text, Tools.PackageSummaryFont, Tools.TextProposedSize, Tools.TextFlags).Width;

				if (darkTag)
				{
					// draw the background
					using (var brush = new SolidBrush(Tools.PackageJobSummaryColor))
					{
						g.FillRectangle(brush, imageX + imageW, imageY, textWidth, imageH);
					}
				}
				else
				{
					// draw the dotted lines
					using (var pen = new Pen(Tools.PackageJobSummaryColor))
					using (var penLight = new Pen(Color.FromArgb(199, 225, 203)))
					{
						pen.DashStyle = DashStyle.Dot;
						penLight.DashStyle = DashStyle.Dot;
						g.DrawLine(pen, imageX + imageW + 1, imageY, imageX + imageW + textWidth - textEndPadding, imageY);
						g.DrawLine(penLight, imageX + imageW + 2, imageY, imageX + imageW + textWidth - textEndPadding, imageY);
						g.DrawLine(pen, imageX + imageW + 1, imageY + imageH - 1, imageX + imageW + textWidth - textEndPadding, imageY + imageH - 1);
						g.DrawLine(penLight, imageX + imageW + 2, imageY + imageH - 1, imageX + imageW + textWidth - textEndPadding, imageY + imageH - 1);
					}
				}

				// draw the text
				var textColor = darkTag ? Color.White : Tools.PackageJobSummaryColor;
				PaintSummaryToken(g, (string.Empty, text), imageX, yPos, textColor, suppressComma: true);

				// draw the right edge
				roundedEdge.RotateFlip(RotateFlipType.RotateNoneFlipX);
				g.DrawImage(roundedEdge, imageX + textWidth, imageY, imageW, imageH);
			}
		}

		#endregion

		#endregion

		#region PaintDottedLine

		public void PaintDottedLine(DrawTreeNodeEventArgs e)
		{
			if (e.Node.IsVisible && ParentTree.ShowLines && ParentTree.ImageList != null && e.Node.ImageIndex == 0)
			{
				const int SPACE_BETWEEN_IMAGE_AND_LABEL = 3;

				// image size
				int imgW = ParentTree.ImageList.ImageSize.Width;
				int imgH = ParentTree.ImageList.ImageSize.Height;

				// image center
				int xPos = e.Node.Bounds.Left - SPACE_BETWEEN_IMAGE_AND_LABEL - imgW / 2;
				int yPos = (e.Node.Bounds.Top + e.Node.Bounds.Bottom) / 2;

				// draw the horizontal missing treeline
				e.Graphics.DrawLine(Tools.DottedLinePen, (xPos - imgW / 2) + 1, yPos, (xPos + imgW / 2), yPos);

				if (!ParentTree.CheckBoxes && e.Node.IsExpanded && e.Node.Nodes.Count > 0)
				{
					// draw the vertical missing treeline
					e.Graphics.DrawLine(Tools.DottedLinePen, xPos, yPos, xPos, (yPos + imgH / 2) + 1);
				}
			}
		}

		#endregion

		#region RepaintTopNode

		public void RepaintTopNode()
		{
			ParentTree.TopNode.Paint();
		}

		#endregion

		#region Dispose

		public void Dispose()
		{
			if (Tools != null)
			{
				Tools.Dispose();
			}

			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion
	}
}
