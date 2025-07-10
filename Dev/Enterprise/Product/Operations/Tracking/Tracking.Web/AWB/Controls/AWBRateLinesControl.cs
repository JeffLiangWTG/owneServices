using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBRateLinesControl runat=server></{0}:AWBRateLinesControl>")]
	public class AWBRateLinesControl : AWBBaseControl
	{
		#region Constructors

		public AWBRateLinesControl()
			: base()
		{
			LinesCount = 11;
		}

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue(11), Browsable(true)]
		public int LinesCount { get; set; }

		#endregion

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			PiecesBoxes = new List<ZTextBox>();
			GrossWeightBoxes = new List<ZNumericTextBox>();
			WeightUnitBoxes = new List<ZDropEditList>();
			RateClassBoxes = new List<ZDropEditList>();
			CommodityItemBoxes = new List<ZTextBox>();
			ChargeableWeightBoxes = new List<ZNumericTextBox>();
			RateBoxes = new List<ZNumericTextBox>();
			TotalBoxes = new List<ZNumericTextBox>();
			DescriptionBoxes = new List<ZTextBox>();

			for (var i = 0; i < LinesCount; i++)
			{
				PiecesBoxes.Add(new ZTextBox());
				SetInternalControlID(PiecesBoxes[i], (NoResString)"Pieces", i);

				GrossWeightBoxes.Add(new ZNumericTextBox());
				SetInternalControlID(GrossWeightBoxes[i], "GrossWeight", i);

				WeightUnitBoxes.Add(new ZDropEditList());
				SetInternalControlID(WeightUnitBoxes[i], "WeightUnit", i);

				RateClassBoxes.Add(new ZDropEditList());
				SetInternalControlID(RateClassBoxes[i], "RateClass", i);

				CommodityItemBoxes.Add(new ZTextBox());
				SetInternalControlID(CommodityItemBoxes[i], "CommodityItem", i);

				ChargeableWeightBoxes.Add(new ZNumericTextBox());
				SetInternalControlID(ChargeableWeightBoxes[i], "ChargeableWeight", i);

				RateBoxes.Add(new ZNumericTextBox());
				SetInternalControlID(RateBoxes[i], (NoResString)"Rate", i);

				TotalBoxes.Add(new ZNumericTextBox());
				SetInternalControlID(TotalBoxes[i], (NoResString)"Total", i);

				DescriptionBoxes.Add(new ZTextBox());
				SetInternalControlID(DescriptionBoxes[i], (NoResString)"Description", i);
			}

			DescriptionBoxes.Add(new ZTextBox());
			SetInternalControlID(DescriptionBoxes[11], (NoResString)"Description", 11);

			TotalPiecesBox = new AWBNumericTextBox();
			TotalPiecesBox.ID = "TotalPieces";

			TotalGrossWeightBox = new AWBNumericTextBox();
			TotalGrossWeightBox.ID = "TotalGrossWeight";

			ShippersLoadBox = new AWBNumericTextBox();
			ShippersLoadBox.ID = "ShippersLoad";

			GrandTotalBox = new AWBNumericTextBox();
			GrandTotalBox.ID = "GrandTotal";

			UpdateTotalsButton = new ZButton();
			UpdateTotalsButton.ID = "UpdateTotals";
		}

		void SetInternalControlID(WebControl control, string idPrefix, int number)
		{
			control.ID = string.Format("{0}{1}", idPrefix, number);
		}

		protected override void BindToAWBHeader(ExportAWBHeader source)
		{
			for (var i = 0; i < LinesCount; i++)
			{
				PiecesBoxes[i].Bind(source.AWBRateLines[i]);
				GrossWeightBoxes[i].Bind(source.AWBRateLines[i]);
				WeightUnitBoxes[i].Bind(source.AWBRateLines[i]);
				RateClassBoxes[i].Bind(source.AWBRateLines[i]);
				CommodityItemBoxes[i].Bind(source.AWBRateLines[i]);
				ChargeableWeightBoxes[i].Bind(source.AWBRateLines[i]);
				RateBoxes[i].Bind(source.AWBRateLines[i]);
				TotalBoxes[i].Bind(source.AWBRateLines[i]);
			}
			for (var i = 0; i < 12; i++)
			{
				DescriptionBoxes[i].Bind(source);
			}
			TotalPiecesBox.Bind(source);
			TotalGrossWeightBox.Bind(source);
			ShippersLoadBox.Bind(source);
			GrandTotalBox.Bind(source);
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			for (var i = 0; i < LinesCount; i++)
			{
				PiecesBoxes[i].BindTo = "ER_NoOfPiecesOrRCP";
				PiecesBoxes[i].CssClass = "AWBText5";

				GrossWeightBoxes[i].BindTo = "ER_GrossWeight";
				GrossWeightBoxes[i].CssClass = "AWBText10";

				WeightUnitBoxes[i].BindTo = "ER_WeightInLBsOrKGs";
				WeightUnitBoxes[i].MaxLength = 2;
				WeightUnitBoxes[i].Width = 45;

				RateClassBoxes[i].BindTo = "ER_RateClass";
				RateClassBoxes[i].MaxLength = 2;
				RateClassBoxes[i].Width = 45;

				CommodityItemBoxes[i].BindTo = "ER_CommodityItemNumber";
				CommodityItemBoxes[i].CssClass = "AWBText10";

				ChargeableWeightBoxes[i].BindTo = "ER_ChargeableWeight";
				ChargeableWeightBoxes[i].CssClass = "AWBText10";

				RateBoxes[i].BindTo = "ER_RateChargeOrDiscount";
				RateBoxes[i].CssClass = "AWBText10";

				TotalBoxes[i].BindTo = "ER_Total";
				TotalBoxes[i].CssClass = "AWBText10";

				DescriptionBoxes[i].BindTo = string.Format("AWBRateLine{0}.ER_NatureAndQtyOfGoods", i + 1);
				DescriptionBoxes[i].CssClass = "AWBText20";
			}

			ZBindToChecker.CheckBindTo(((ExportAWBRateLine)null).ER_NatureAndQtyOfGoods);
			DescriptionBoxes[11].BindTo = "AWBRateLine12.ER_NatureAndQtyOfGoods";
			DescriptionBoxes[11].CssClass = "AWBText20";

			TotalPiecesBox.Caption = "&nbsp;";
			TotalPiecesBox.BindTo = "EH_TotalNoOfPieces";
			TotalPiecesBox.FieldCaptionVisible = false;
			TotalPiecesBox.FieldCssClass = "AWBText5";

			TotalGrossWeightBox.Caption = "&nbsp;";
			TotalGrossWeightBox.BindTo = "EH_TotalGrossWeight";
			TotalGrossWeightBox.FieldCaptionVisible = false;
			TotalGrossWeightBox.FieldCssClass = "AWBText10";

			ShippersLoadBox.Caption = Res.GetString("57d2b6c1-fea5-4249-86ca-bf8f88162c89", "Shippers Load and Count (FWB/FHL)");
			ShippersLoadBox.BindTo = "EH_ShippingLoadAndCount";
			ShippersLoadBox.FieldCaptionVisible = false;
			ShippersLoadBox.FieldCssClass = "AWBText10";
			ShippersLoadBox.DataFieldAlign = HorizontalAlign.Center;

			GrandTotalBox.Caption = "&nbsp;";
			GrandTotalBox.BindTo = "EH_TotalLineTotals";
			GrandTotalBox.FieldCaptionVisible = false;
			GrandTotalBox.FieldCssClass = "AWBText10";

			UpdateTotalsButton.Text = Res.GetString("3f0a9707-9e5e-4ff7-b90a-ad1be96a4566", "Update Totals");
			UpdateTotalsButton.Width = Unit.Percentage(95);

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;
			layoutTable.Width = Unit.Percentage(100);

			#region Header

			TableRow headerRow1 = new TableRow();
			TableRow headerRow2 = new TableRow();

			#region PiecesHeader

			ZTextLabel piecesLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("fafd1c0d-e062-4a87-bab5-d8f93d833df9", "No. of")
			};
			ZTextLabel piecesLabel2 = new ZTextLabel()
			{
				Text = Res.GetString("2fdc5987-0982-4a43-9c37-1c3b896c6bc0", "Pieces")
			};
			ZTextLabel piecesLabel3 = new ZTextLabel()
			{
				Text = "RCP"
			};
			TableCell piecesHeaderCell = new TableCell();
			piecesHeaderCell.RowSpan = 2;
			headerRow1.Cells.Add(piecesHeaderCell);
			piecesHeaderCell.Controls.Add(piecesLabel1);
			piecesHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			piecesHeaderCell.Controls.Add(piecesLabel2);
			piecesHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			piecesHeaderCell.Controls.Add(piecesLabel3);

			#endregion

			#region GrossWeightHeader

			ZTextLabel grossWeightLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("b22af711-e3bb-401e-b8c4-062e4e5fc011", "Gross")
			};
			ZTextLabel grossWeightLabel2 = new ZTextLabel()
			{
				Text = Res.GetString("ef3414ad-1e3f-4610-af70-2c645799e79d", "Weight")
			};
			TableCell grossWeightHeaderCell = new TableCell();
			grossWeightHeaderCell.RowSpan = 2;
			headerRow1.Cells.Add(grossWeightHeaderCell);
			grossWeightHeaderCell.Controls.Add(grossWeightLabel1);
			grossWeightHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			grossWeightHeaderCell.Controls.Add(grossWeightLabel2);

			#endregion

			#region WeightUnitHeader

			ZTextLabel weightUnitLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("eb8aba78-19ae-4641-8232-8162a402eab3", "kg")
			};
			ZTextLabel weightUnitLabel2 = new ZTextLabel()
			{
				Text = Res.GetString("1f497282-4670-4099-83ef-034115b9e631", "lb")
			};
			TableCell weightUnitHeaderCell = new TableCell();
			weightUnitHeaderCell.RowSpan = 2;
			headerRow1.Cells.Add(weightUnitHeaderCell);
			weightUnitHeaderCell.Controls.Add(weightUnitLabel1);
			weightUnitHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			weightUnitHeaderCell.Controls.Add(weightUnitLabel2);

			#endregion

			headerRow1.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn", RowSpan = 2 });

			#region RateClassHeader and Commodity Item

			ZTextLabel rateClassLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("db4519d9-66d9-4996-a0e5-e874c23c51ba", "Rate Class")
			};
			ZTextLabel commodityItemLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("274c354f-69f3-4b71-bfdf-c3fe176dfd62", "Commodity")
			};
			ZTextLabel commodityItemLabel2 = new ZTextLabel()
			{
				Text = Res.GetString("dbea2914-61c5-4e2a-b9c7-12908c6dc339", "Item No.")
			};
			TableCell rateClassHeaderCell = new TableCell();
			rateClassHeaderCell.ColumnSpan = 2;
			rateClassHeaderCell.Style.Add("border-bottom", (NoResString)"0px");
			headerRow1.Cells.Add(rateClassHeaderCell);
			rateClassHeaderCell.Controls.Add(rateClassLabel1);

			TableCell commodityItemHeaderCell = new TableCell();
			commodityItemHeaderCell.CssClass = (NoResString)"AWBLineHeader AWBCommodityHeader";
			headerRow2.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = (NoResString)"AWBLineHeader AWBCommodityEmptyHeader" });
			headerRow2.Cells.Add(commodityItemHeaderCell);
			commodityItemHeaderCell.Controls.Add(commodityItemLabel1);
			commodityItemHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			commodityItemHeaderCell.Controls.Add(commodityItemLabel2);

			#endregion

			headerRow1.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn", RowSpan = 2 });

			#region ChargeableWeightHeader

			ZTextLabel chargeableWeightLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("27f62d64-66e6-457b-9318-874f9b332f54", "Chargeable")
			};
			ZTextLabel chargeableWeightLabel2 = new ZTextLabel()
			{
				Text = Res.GetString("ef3414ad-1e3f-4610-af70-2c645799e79d", "Weight")
			};
			TableCell chargeableWeightHeaderCell = new TableCell();
			chargeableWeightHeaderCell.RowSpan = 2;
			headerRow1.Cells.Add(chargeableWeightHeaderCell);
			chargeableWeightHeaderCell.Controls.Add(chargeableWeightLabel1);
			chargeableWeightHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			chargeableWeightHeaderCell.Controls.Add(chargeableWeightLabel2);

			#endregion

			headerRow1.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn", RowSpan = 2 });

			#region RateChargeHeader

			ZTextLabel rateChargeLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("7e62cac9-bad9-4ca4-942f-bd27935ae5b4", "Rate")
			};
			ZTextLabel rateChargeLabel2 = new ZTextLabel()
			{
				Text = "/"
			};
			ZTextLabel rateChargeLabel3 = new ZTextLabel()
			{
				Text = Res.GetString("bf0b5e76-8b5c-411f-b8f2-16893e7d56f6", "Charge")
			};
			TableCell rateChargeHeaderCell = new TableCell();
			rateChargeHeaderCell.RowSpan = 2;
			headerRow1.Cells.Add(rateChargeHeaderCell);
			rateChargeHeaderCell.Controls.Add(rateChargeLabel1);
			rateChargeHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			rateChargeHeaderCell.Controls.Add(rateChargeLabel2);
			rateChargeHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			rateChargeHeaderCell.Controls.Add(rateChargeLabel3);

			#endregion

			headerRow1.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn", RowSpan = 2 });

			#region TotalHeader

			ZTextLabel totalLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("94e0119f-c51b-44b0-b7cd-d6103cf91643", "Total")
			};
			TableCell totalHeaderCell = new TableCell();
			totalHeaderCell.RowSpan = 2;
			headerRow1.Cells.Add(totalHeaderCell);
			totalHeaderCell.Controls.Add(totalLabel1);

			#endregion

			headerRow1.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn", RowSpan = 2 });

			#region DescriptionHeader

			ZTextLabel descriptionLabel1 = new ZTextLabel()
			{
				Text = Res.GetString("83403b42-cbf0-443d-9a8d-fc4612296cbd", "Nature and Quantity of Goods")
			};
			ZTextLabel descriptionLabel2 = new ZTextLabel()
			{
				Text = Res.GetString("410e6c99-013c-4f2c-b4e9-8ed327aa7f90", "(incl. Dimensions or Volume)")
			};
			TableCell descriptionHeaderCell = new TableCell();
			descriptionHeaderCell.RowSpan = 2;
			headerRow1.Cells.Add(descriptionHeaderCell);
			descriptionHeaderCell.Controls.Add(descriptionLabel1);
			descriptionHeaderCell.Controls.Add(new LiteralControl((NoResString)"<br/>"));
			descriptionHeaderCell.Controls.Add(descriptionLabel2);

			#endregion

			SetupHeaderCells(headerRow1);

			layoutTable.Rows.Add(headerRow1);
			layoutTable.Rows.Add(headerRow2);

			#endregion

			#region Data Lines

			for (var i = 0; i < LinesCount; i++)
			{
				TableRow dataRow = new TableRow();

				TableCell piecesCell = new TableCell();
				piecesCell.Controls.Add(PiecesBoxes[i]);

				TableCell grossWeightCell = new TableCell();
				grossWeightCell.Controls.Add(GrossWeightBoxes[i]);

				TableCell weightUnitCell = new TableCell();
				weightUnitCell.Controls.Add(WeightUnitBoxes[i]);

				TableCell rateClassCell = new TableCell();
				rateClassCell.Controls.Add(RateClassBoxes[i]);

				TableCell commodityItemCell = new TableCell();
				commodityItemCell.Controls.Add(CommodityItemBoxes[i]);

				TableCell chargeableWeightCell = new TableCell();
				chargeableWeightCell.Controls.Add(ChargeableWeightBoxes[i]);

				TableCell rateCell = new TableCell();
				rateCell.Controls.Add(RateBoxes[i]);

				TableCell totalCell = new TableCell();
				totalCell.Controls.Add(TotalBoxes[i]);

				TableCell descriptionCell = new TableCell();
				descriptionCell.Controls.Add(DescriptionBoxes[i]);

				dataRow.Cells.Add(piecesCell);
				dataRow.Cells.Add(grossWeightCell);
				dataRow.Cells.Add(weightUnitCell);
				dataRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn" });
				dataRow.Cells.Add(rateClassCell);
				dataRow.Cells.Add(commodityItemCell);
				dataRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn" });
				dataRow.Cells.Add(chargeableWeightCell);
				dataRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn" });
				dataRow.Cells.Add(rateCell);
				dataRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn" });
				dataRow.Cells.Add(totalCell);
				dataRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn" });
				dataRow.Cells.Add(descriptionCell);

				SetupDataCells(dataRow);

				layoutTable.Rows.Add(dataRow);
			}

			#endregion

			#region Grand Totals

			TableRow grandTotalRow = new TableRow();

			TableCell totalPiecesCell = new TableCell();
			totalPiecesCell.CssClass = (NoResString)"AWBSection AWBLineFirstColumn AWBLinesTotal";
			totalPiecesCell.Controls.Add(TotalPiecesBox);
			grandTotalRow.Cells.Add(totalPiecesCell);

			TableCell totalGrossWeightCell = new TableCell();
			totalGrossWeightCell.CssClass = (NoResString)"AWBSection AWBLinesTotal";
			totalGrossWeightCell.Controls.Add(TotalGrossWeightBox);
			grandTotalRow.Cells.Add(totalGrossWeightCell);

			grandTotalRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineData" });
			grandTotalRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn" });
			grandTotalRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineData" });

			TableCell shippersLoadtCell = new TableCell();
			shippersLoadtCell.ColumnSpan = 4;
			shippersLoadtCell.CssClass = (NoResString)"AWBSection AWBLinesTotal";
			shippersLoadtCell.Controls.Add(ShippersLoadBox);
			grandTotalRow.Cells.Add(shippersLoadtCell);

			//GrandTotalRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineData" });
			TableCell updateTotalsCell = new TableCell();
			updateTotalsCell.CssClass = "AWBLineData";
			updateTotalsCell.HorizontalAlign = HorizontalAlign.Center;
			updateTotalsCell.VerticalAlign = VerticalAlign.Middle;
			updateTotalsCell.Controls.Add(UpdateTotalsButton);
			grandTotalRow.Cells.Add(updateTotalsCell);
			grandTotalRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn" });

			TableCell grandTotalCell = new TableCell();
			grandTotalCell.CssClass = (NoResString)"AWBSection AWBLinesTotal";
			grandTotalCell.Controls.Add(GrandTotalBox);
			grandTotalRow.Cells.Add(grandTotalCell);

			grandTotalRow.Cells.Add(new TableCell() { Text = "&nbsp;", CssClass = "AWBLineEmptyColumn" });

			TableCell lastDescriptionCell = new TableCell();
			lastDescriptionCell.Controls.Add(DescriptionBoxes[11]);
			lastDescriptionCell.CssClass = (NoResString)"AWBLineData AWBLineLastColumn";
			grandTotalRow.Cells.Add(lastDescriptionCell);

			layoutTable.Rows.Add(grandTotalRow);

			#endregion

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		void SetupHeaderCells(TableRow row)
		{
			SetupCells(row, "AWBLineHeader");
		}

		void SetupDataCells(TableRow row)
		{
			SetupCells(row, "AWBLineData");
		}

		void SetupCells(TableRow row, string defaultCssClass)
		{
			for (var i = 0; i < row.Cells.Count; i++)
			{
				TableCell cell = row.Cells[i];
				if (string.IsNullOrEmpty(cell.CssClass))
				{
					cell.CssClass = defaultCssClass;
					if (i == 0)
					{
						cell.CssClass += " AWBLineFirstColumn";
					}
					if (i == row.Cells.Count - 1)
					{
						cell.CssClass += " AWBLineLastColumn";
					}
				}
			}
		}

		#endregion

		#region Implementation

		List<ZTextBox> PiecesBoxes;
		List<ZNumericTextBox> GrossWeightBoxes;
		List<ZDropEditList> WeightUnitBoxes;
		List<ZDropEditList> RateClassBoxes;
		List<ZTextBox> CommodityItemBoxes;
		List<ZNumericTextBox> ChargeableWeightBoxes;
		List<ZNumericTextBox> RateBoxes;
		List<ZNumericTextBox> TotalBoxes;
		List<ZTextBox> DescriptionBoxes;
		AWBNumericTextBox TotalPiecesBox;
		AWBNumericTextBox TotalGrossWeightBox;
		AWBNumericTextBox ShippersLoadBox;
		AWBNumericTextBox GrandTotalBox;
		ZButton UpdateTotalsButton;

		#endregion
	}
}
