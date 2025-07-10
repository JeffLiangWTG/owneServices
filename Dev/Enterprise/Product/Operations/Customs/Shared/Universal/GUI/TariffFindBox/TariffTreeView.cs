using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public class TariffTreeView : ZTreeView
	{
		public TariffTreeView(RefCusTariffFilterStripBusinessObject filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;
		}

		public readonly RefCusTariffFilterStripBusinessObject filterBusinessObject;

		protected override bool UseDrawNodeFontOverride => false;

#if !WINZOR

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			base.OnDrawNode(e);

			var nodeFont = e.Node.NodeFont ?? e.Node.TreeView.Font;
			DrawSelectionHighlightingRectangle(e, nodeFont);

			var matches = ExtractMatches(e);
			string txt = e.Node.Text;

			var lastEnd = 0;
			var currentBoundOffset = ControlDpiScalingHelper.NewScaledSize(0, 0, false);
			var flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
			foreach (var match in matches)
			{
				var normalTxt = txt.Substring(lastEnd, match.Item1 - lastEnd);
				var boldTxt = txt.Substring(match.Item1, match.Item2 - match.Item1);
				lastEnd = match.Item2;

				Size proposedSize = ControlDpiScalingHelper.NewScaledSize(int.MaxValue, int.MaxValue, false);
				TextRenderer.DrawText(e.Graphics, normalTxt, nodeFont, e.Bounds.Location + currentBoundOffset, Color.Black, flags);
				currentBoundOffset += ControlDpiScalingHelper.NewScaledSize((int)Math.Floor((double)TextRenderer.MeasureText(e.Graphics, normalTxt, nodeFont, proposedSize, flags).Width), 0, false);

				if (!string.IsNullOrEmpty(boldTxt))
				{
					Rectangle boldRect = new Rectangle(e.Bounds.Location + currentBoundOffset, ControlDpiScalingHelper.NewScaledSize((int)Math.Floor((double)TextRenderer.MeasureText(e.Graphics, boldTxt, nodeFont, proposedSize, flags).Width), e.Bounds.Height, false));
					e.Graphics.FillRectangle(Brushes.Yellow, boldRect);
					TextRenderer.DrawText(e.Graphics, boldTxt, nodeFont, e.Bounds.Location + currentBoundOffset, Color.Black, flags);
					currentBoundOffset += ControlDpiScalingHelper.NewScaledSize(boldRect.Width, 0, false);
				}
			}

			if (lastEnd < txt.Length)
			{
				var normalTxt = txt.Substring(lastEnd);
				TextRenderer.DrawText(e.Graphics, normalTxt, nodeFont, e.Bounds.Location + currentBoundOffset, Color.Black, flags);
			}
		}

		void DrawSelectionHighlightingRectangle(DrawTreeNodeEventArgs e, Font nodeFont)
		{
			var isSelected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
			var isFocused = e.Node.TreeView.Focused;

			var size = TextRenderer.MeasureText(e.Node.Text, nodeFont, e.Bounds.Size, TextFormatFlags.NoPrefix);
			var backColor =
						!isSelected
								? BrushProvider.FromColor(e.Node.BackColor)
								: isFocused
										? SelectedNodeBackBrushWhenFocused
										: SelectedNodeBackBrushWhenUnfocused;

			e.Graphics.FillRectangle(backColor, e.Bounds.X, e.Bounds.Y, size.Width, e.Bounds.Height);
		}

		List<Tuple<int, int>> ExtractMatches(DrawTreeNodeEventArgs e)
		{
			var descriptions = filterBusinessObject.ActiveModuleFilters.OfType<ModuleTextFilter>().Where(filter => filter.Description.StartsWith(Constants.RefCusTariffFilters.DefaultLanguageDescription, StringComparison.OrdinalIgnoreCase));
			string txt = e.Node.Text;
			var pattern = new ZStringBuilder();
			foreach (var description in descriptions)
			{
				if (!description.Property.IsEmpty)
				{
					pattern.Append("(" + Regex.Escape(description.Property) + ")");
				}
			}

			var tuples = new List<Tuple<int, int>>();
			if (!pattern.IsEmpty)
			{
				var regex = new Regex(pattern.ToStringWithDelimiterBetweenAppends("|"), RegexOptions.IgnoreCase);
				var matches = regex.Matches(txt);
				foreach (Match match in matches)
				{
					var overlaps = tuples.Where(existing =>
					{
						return (existing.Item1 < match.Index && existing.Item2 > match.Index) ||
								(match.Index < existing.Item1 && match.Index + match.Length > existing.Item1);
					});

					if (overlaps.Any())
					{
						foreach (var overlap in overlaps)
						{
							tuples.Remove(overlap);
						}

						var start = Math.Min(overlaps.Min(x => x.Item1), match.Index);
						var end = Math.Max(overlaps.Max(x => x.Item2), match.Index);
						tuples.Add(new Tuple<int, int>(start, end));
					}
					else
					{
						tuples.Add(new Tuple<int, int>(match.Index, match.Index + match.Length));
					}
				}
			}
			return tuples;
		}

#endif
	}
}
