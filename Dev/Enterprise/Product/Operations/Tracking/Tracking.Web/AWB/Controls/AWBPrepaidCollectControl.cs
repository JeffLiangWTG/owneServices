using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBPrepaidCollectControl runat=server></{0}:AWBPrepaidCollectControl>")]
	public class AWBPrepaidCollectControl : AWBBaseControl
	{
		#region Constructors

		public AWBPrepaidCollectControl()
			: base()
		{
			CollectFieldVisible = true;
			PrepaidFieldVisible = true;
		}

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string PrepaidBindTo { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string CollectBindTo { get; set; }

		[Category("Appearance"), DefaultValue(true), Browsable(true)]
		public bool CollectFieldVisible { get; set; }

		[Category("Appearance"), DefaultValue(true), Browsable(true)]
		public bool PrepaidFieldVisible { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string Caption { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string PrepaidCssClass { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string CollectCssClass { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string PrepaidBoxCssClass { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string CollectBoxCssClass { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string PrepaidCaption { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string CollectCaption { get; set; }

		#endregion

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			PrepaidBox = new ZNumericTextBox();
			PrepaidBox.ID = (NoResString)"Prepaid";

			CollectBox = new ZNumericTextBox();
			CollectBox.ID = (NoResString)"Collect";
		}

		Table GetCaptionTable()
		{
			Table result = new Table();
			result.CellSpacing = 0;
			result.CellSpacing = 0;
			result.CssClass = "AWBCaptionBox";
			TableRow row = new TableRow();
			row.Cells.Add(new TableCell());
			result.Rows.Add(row);
			return result;
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			PrepaidBox.BindTo = PrepaidBindTo;
			PrepaidBox.CssClass = PrepaidBoxCssClass;

			CollectBox.BindTo = CollectBindTo;
			CollectBox.CssClass = CollectBoxCssClass;

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;
			layoutTable.Width = Unit.Percentage(100);

			TableRow captionRow = new TableRow();

			TableCell captionCell = new TableCell();
			captionCell.HorizontalAlign = HorizontalAlign.Center;
			captionCell.VerticalAlign = VerticalAlign.Top;
			ZTextLabel captionLabel = new ZTextLabel()
			{
				Text = Caption
			};
			Table captionTable = GetCaptionTable();
			captionTable.Rows[0].Cells[0].Controls.Add(captionLabel);
			captionCell.Controls.Add(captionTable);

			TableCell prepaidCaptionCell = new TableCell();
			prepaidCaptionCell.HorizontalAlign = HorizontalAlign.Center;
			prepaidCaptionCell.VerticalAlign = VerticalAlign.Top;
			ZTextLabel prepaidCaptionLabel = new ZTextLabel()
			{
				Text = PrepaidCaption
			};
			Table prepaidCaptionTable = GetCaptionTable();
			prepaidCaptionTable.Rows[0].Cells[0].Controls.Add(prepaidCaptionLabel);
			prepaidCaptionCell.Controls.Add(prepaidCaptionTable);

			TableCell collectCaptionCell = new TableCell();
			collectCaptionCell.HorizontalAlign = HorizontalAlign.Center;
			collectCaptionCell.VerticalAlign = VerticalAlign.Top;
			ZTextLabel collectCaptionLabel = new ZTextLabel()
			{
				Text = CollectCaption
			};
			Table collectCaptionTable = GetCaptionTable();
			collectCaptionTable.Rows[0].Cells[0].Controls.Add(collectCaptionLabel);
			collectCaptionCell.Controls.Add(collectCaptionTable);

			TableRow dataRow = new TableRow();
			TableCell prepaidCell = new TableCell();
			prepaidCell.HorizontalAlign = HorizontalAlign.Center;
			prepaidCell.CssClass = "AWBChargeField " + PrepaidCssClass;
			if (PrepaidFieldVisible)
			{
				prepaidCell.Controls.Add(new LiteralControl("&nbsp;"));
				prepaidCell.Controls.Add(PrepaidBox);
				prepaidCell.Controls.Add(new LiteralControl("&nbsp;"));
			}
			else
			{
				prepaidCell.Text = "&nbsp;";
			}
			dataRow.Cells.Add(prepaidCell);

			TableCell collectCell = new TableCell();
			collectCell.HorizontalAlign = HorizontalAlign.Center;
			collectCell.CssClass = "AWBChargeField " + CollectCssClass;
			if (CollectFieldVisible)
			{
				collectCell.Controls.Add(new LiteralControl("&nbsp;"));
				collectCell.Controls.Add(CollectBox);
				collectCell.Controls.Add(new LiteralControl("&nbsp;"));
			}
			else
			{
				collectCell.Text = "&nbsp;";
			}
			dataRow.Cells.Add(collectCell);

			if (string.IsNullOrEmpty(Caption))
			{
				if (!string.IsNullOrEmpty(PrepaidCaption))
				{
					captionRow.Cells.Add(prepaidCaptionCell);
				}
				else
				{
					captionRow.Cells.Add(new TableCell() { Text = "&nbsp;" });
				}
				captionRow.Cells[0].CssClass = "AWBRightBorder";
				if (!string.IsNullOrEmpty(CollectCaption))
				{
					captionRow.Cells.Add(collectCaptionCell);
				}
				else
				{
					captionRow.Cells.Add(new TableCell() { Text = "&nbsp;" });
				}
			}
			else
			{
				captionCell.ColumnSpan = 2;
				if (!string.IsNullOrEmpty(PrepaidCaption) || !string.IsNullOrEmpty(CollectCaption))
				{
					captionRow.Cells.Add(prepaidCaptionCell);
					captionRow.Cells.Add(captionCell);
					captionRow.Cells.Add(collectCaptionCell);
					prepaidCell.ColumnSpan = 2;
					collectCell.ColumnSpan = 2;
				}
				else
				{
					captionRow.Cells.Add(captionCell);
				}
			}

			layoutTable.Rows.Add(captionRow);
			layoutTable.Rows.Add(dataRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		ZNumericTextBox PrepaidBox;
		ZNumericTextBox CollectBox;

		#endregion
	}
}
