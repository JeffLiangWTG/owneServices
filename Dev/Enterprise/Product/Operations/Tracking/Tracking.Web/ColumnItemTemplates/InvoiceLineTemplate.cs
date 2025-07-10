using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.ColumnItemTemplates
{
	public class InvoiceLineTemplate : ZItemTemplate
	{
		public InvoiceLineTemplate(ZPage page, ZNewRowColumn column, ZString countryCode) : base(column)
		{
			this.Page = page;
		}

		readonly ZPage Page;

		protected override void ChangeCell(TableCell cell)
		{
			base.ChangeCell(cell);
			DataGridItem item = cell.NamingContainer as DataGridItem;
			if (((BaseJobComInvoiceHeader)item.DataItem).InvoiceLines.Count > 0)
			{
				((ZDataGrid)Control).Visible = true;
				((BaseJobComInvoiceHeader)item.DataItem).InvoiceLines.Sort(BaseJobComInvoiceLine.Schema.JI_LineNo);
			}
			else
			{
				((ZDataGrid)Control).Visible = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override ISelfBindingWebControl GetControl()
		{
			ZDataGrid grid = new ZDataGrid();
			grid.Style[System.Web.UI.HtmlTextWriterStyle.Width] = "100%";
			grid.ItemStyle.CssClass = "DetailsCell";
			grid.HeaderStyle.CssClass = "DetailsHeader";
			grid.CssClass = "DetailsTable";
			grid.AllowEdit = false;

			ZBindToChecker.CheckBindTo((ZShort)((BaseJobComInvoiceLine)null).JI_LineNo);
			grid.Columns.Add(new ZCalcEditColumn("LNO", BaseJobComInvoiceLine.Schema.JI_LineNo) { Decimals = 0 });

			grid.Columns.Add(new ZHyperLinkColumn(Res.GetString("21abc971-4398-4d78-8715-0f5bc1f558f3", "Product"), BaseJobComInvoiceLine.Schema.JI_PartNo)
			{
				DataNavigateUrlFormatString = (((Global)Page.AppInstance).ProductProfileDetailsPage + (NoResString)"?Ref={0}&OrderRef={1}"), // Non-semantic text
				DataNavigateUrlFields = new[] { "Part.PK", "Declaration.PK" }
			});

			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceLine)null).JI_FormattedTariff);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("8a034a7c-01fc-466c-8f3d-62d2c8b50cdd", "Tariff"), BaseJobComInvoiceLine.Schema.JI_FormattedTariff));

			ZBindToChecker.CheckBindTo((ZDecimal)((BaseJobComInvoiceLine)null).JI_InvoiceQuantity);
			grid.Columns.Add(new ZCalcEditColumn("Inv.Qty", BaseJobComInvoiceLine.Schema.JI_InvoiceQuantity) { Decimals = 3 });
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((BaseJobComInvoiceLine)null).Lookups.InvoiceUQList);
			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceLine)null).JI_InvoiceUQ);
			grid.Columns.Add(new ZDropDownListColumn(Res.GetString("e2d3f7be-df85-45f9-a593-30e533fedcde", "UQ"), "JI_InvoiceUQ", "Lookups.InvoiceUQList"));

			ZBindToChecker.CheckBindTo((ZDecimal)((BaseJobComInvoiceLine)null).JI_LinePrice);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("3a3cff94-da32-4e02-906f-a942e7d11073", "Line Price"), BaseJobComInvoiceLine.Schema.JI_LinePrice) { Decimals = 2 });

			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceLine)null).JI_Description);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("07ef78a5-7009-44d0-8a2d-c861db34a8c6", "Description"), BaseJobComInvoiceLine.Schema.JI_Description));

			ZBindToChecker.CheckBindTo((ZDecimal)((BaseJobComInvoiceLine)null).JI_Weight);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("47f4a2e0-8658-474d-bf6a-f87379752ec6", "Weight"), BaseJobComInvoiceLine.Schema.JI_Weight) { Decimals = 3 });

			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((BaseJobComInvoiceLine)null).Lookups.WeightUQList);
			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceLine)null).JI_WeightUQ);
			grid.Columns.Add(new ZDropDownListColumn(Res.GetString("6a333b52-f9a4-4811-b038-0ae1a8442d99", "UW"), "JI_WeightUQ", "Lookups.WeightUQList"));

			ZBindToChecker.CheckBindTo((ZDecimal)((BaseJobComInvoiceLine)null).JI_Volume);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("bcd354eb-26e7-4736-83f8-21ef672eabee", "Volume"), BaseJobComInvoiceLine.Schema.JI_Volume) { Decimals = 3 });

			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((BaseJobComInvoiceLine)null).Lookups.VolumeUQList);
			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceLine)null).JI_VolumeUQ);
			grid.Columns.Add(new ZDropDownListColumn(Res.GetString("eff24a63-94c4-47ce-9042-6dde634f2803", "UV"), "JI_VolumeUQ", "Lookups.VolumeUQList"));

			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceLine)null).JI_OrderNumber);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("aed5b0e9-0cd7-48d8-a783-447a47cdf655", "Order #"), BaseJobComInvoiceLine.Schema.JI_OrderNumber));

			ZBindToChecker.CheckBindTo((ZDecimal)((BaseJobComInvoiceLine)null).JI_Calc_DutyAmount);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("e6809b43-4bb6-443d-b0ca-17c3a1c4ba75", "Duty"), BaseJobComInvoiceLine.Schema.JI_Calc_DutyAmount));

			return grid;
		}
	}
}
