using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingOrderLineTemplate : ZItemTemplate
	{
		public TrackingOrderLineTemplate(ZNewRowColumn column)
			: base(column)
		{
		}

		#region Column

		new ZNewRowColumn Column
		{
			get { return base.Column as ZNewRowColumn; }
		}

		#endregion

		#region GetWhsOrderLinesExcelExportColumns

		public static List<ExcelExportColumnBase> GetWhsOrderLinesExcelExportColumns(TrackingWhsReleaseLineCollection collectionToExport)
		{
			var grid = new ZDataGrid { BindTo = nameof(WebTracker.Grids.WarehouseDocketLine.ReleaseDetails) };
			SetupGridForExportToExcel(grid);
			var exportHelper = new DataGridExcelExportHelper(collectionToExport, grid.Columns);

			return exportHelper.CanContinueWithExport
					? exportHelper.GetExcelExportColumns()
					: new List<ExcelExportColumnBase>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		static void SetupGridForExportToExcel(ZDataGrid grid)
		{
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReleaseLine)null).ProductCode);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("c78c8692-b3f5-4f7f-8166-5c83fdcaf22c", "Product"), TrackingWhsReleaseLine.Constants.ProductCode));
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("23608436-c2a4-4b60-9b19-3beb22f9a10a", "Description"), TrackingWhsReleaseLine.Constants.ProductDescription));

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsReleaseLine)null).Packs);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("a6afeadf-c172-4d62-8f50-7899efecba9e", "Packs"), TrackingWhsReleaseLine.Constants.Packs));
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("f84b39bd-a2d5-4153-bbcc-c47c6ba0c4a3", "Packs UQ"), TrackingWhsReleaseLine.Constants.PacksUQ));
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("ffdbf08a-634c-458d-bae7-7120224e1b9c", "Qty Ordered"), TrackingWhsReleaseLine.Constants.QtyOrdered));
			SetupGrid(grid);
		}

		#endregion

		#region GetControl

		protected override ISelfBindingWebControl GetControl()
		{
			var control = new ZDataGrid();
			control.BindTo = Column.BindTo;
			SetupGrid(control);
			return control;
		}

		static void SetupGrid(ZDataGrid grid)
		{
			grid.Style[System.Web.UI.HtmlTextWriterStyle.Width] = "100%";
			grid.CssClass = "InnerDetailsTable";
			grid.ItemStyle.CssClass = "InnerDetailsCell";
			grid.HeaderStyle.CssClass = "InnerDetailsHeader";

			grid.GridLines = System.Web.UI.WebControls.GridLines.None;
			grid.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
			grid.BorderWidth = System.Web.UI.WebControls.Unit.Pixel(0);
			grid.CellSpacing = 4;

			var columns = grid.Columns;

			// quantity met
			ZBindToChecker.CheckBindTo(((TrackingWhsReleaseLine)null).W1_Units);
			columns.Add(new ZTextEditColumn(Res.GetString("13db6756-680c-48da-a08b-631267baba11", "Qty Met"), TrackingWhsReleaseLine.Constants.W1_Units));
			columns[0].ItemStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Right;
			columns[0].HeaderStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Right;

			// quantity unit
			ZBindToChecker.CheckBindTo(((TrackingWhsReleaseLine)null).UnitsQName);
			columns.Add(new ZTextEditColumn(Res.GetString("463e3164-6c28-4f00-9f7d-d1b64e3505e2", "UQ"), TrackingWhsReleaseLine.Constants.UnitsQName));

			TrackingSiteUser siteUser = WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null;
			PartAttributeManager partManager = siteUser != null && siteUser.IsLoggedIn ? siteUser.LoggedInOrganisation.PartAttributeManager : null;

			// part attribtues (1, 2, 3), expiry and packing dates
			ZBindToChecker.CheckBindTo(((TrackingWhsReleaseLine)null).W1_PartAttrib1);
			ZBindToChecker.CheckBindTo(((TrackingWhsReleaseLine)null).W1_PartAttrib2);
			ZBindToChecker.CheckBindTo(((TrackingWhsReleaseLine)null).W1_PartAttrib3);
			ZBindToChecker.CheckBindTo(((TrackingWhsReleaseLine)null).W1_SerialNumber);
			ZBindToChecker.CheckBindTo(((TrackingWhsReleaseLine)null).W1_PackingDate);
			ZBindToChecker.CheckBindTo(((TrackingWhsReleaseLine)null).W1_ExpiryDate);
			if (partManager != null)
			{
				if (partManager.IsPartAttributeUsedByOrganisation(1))
				{
					columns.Add(new ZTextEditColumn(partManager.PartAttributeName1, TrackingWhsReleaseLine.Constants.W1_PartAttrib1));
				}

				if (partManager.IsPartAttributeUsedByOrganisation(2))
				{
					columns.Add(new ZTextEditColumn(partManager.PartAttributeName2, TrackingWhsReleaseLine.Constants.W1_PartAttrib2));
				}

				if (partManager.IsPartAttributeUsedByOrganisation(3))
				{
					columns.Add(new ZTextEditColumn(partManager.PartAttributeName3, TrackingWhsReleaseLine.Constants.W1_PartAttrib3));
				}

				if (partManager.IsSerialNumberUsedByOrganisation)
				{
					columns.Add(new ZTextEditColumn(Res.GetString("4791bf5c-3756-49f1-a9ca-a427f04d0c32", "Serial Number"), TrackingWhsReleaseLine.Constants.W1_SerialNumber));
				}

				if (partManager.IsPackingDateUsedByOrganisation)
				{
					columns.Add(new ZTextEditColumn(Res.GetString("e490e9c1-90af-4a1c-9802-4230ed81b618", "Packing Date"), TrackingWhsReleaseLine.Constants.W1_PackingDate));
				}

				if (partManager.IsExpiryDateUsedByOrganisation)
				{
					columns.Add(new ZTextEditColumn(Res.GetString("ca670333-a18f-4367-9af7-39a22a51f8d6", "Expiry Date"), TrackingWhsReleaseLine.Constants.W1_ExpiryDate));
				}
			}
		}

		#endregion
	}
}
