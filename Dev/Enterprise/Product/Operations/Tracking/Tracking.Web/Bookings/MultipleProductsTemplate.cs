using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public class MultipleProductsTemplate : ZItemTemplate
	{
		public MultipleProductsTemplate(ZNewRowColumn column)
			: base(column)
		{
		}

		new ZNewRowColumn Column
		{
			get { return base.Column as ZNewRowColumn; }
		}

		protected override ISelfBindingWebControl GetControl()
		{
			ZDataGrid control = new ZDataGrid();
			control.BindTo = Column.BindTo;
			SetupGrid(control);
			return control;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected void SetupGrid(ZDataGrid grid)
		{
			grid.CssClass = "InnerDetailsTable";
			grid.ItemStyle.CssClass = "InnerDetailsCell";
			grid.HeaderStyle.CssClass = "InnerDetailsHeader";
			grid.FooterStyle.CssClass = "InnerDetailsCell";

			grid.GridLines = System.Web.UI.WebControls.GridLines.None;
			grid.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
			grid.BorderWidth = System.Web.UI.WebControls.Unit.Pixel(0);
			grid.CellSpacing = 4;

			grid.AllowAdd = !Column.ReadOnly;
			grid.AllowDelete = !Column.ReadOnly;
			grid.AllowEdit = !Column.ReadOnly;
			grid.ReadOnly = Column.ReadOnly;
			grid.InitialRowsToDisplay = (grid.AllowAdd ? 1 : 0);
			grid.DisplayAdditionalNewRow = grid.AllowAdd && WebDataRegistry.Instance.BookingOrderGridsEnableExtraRowMode.Value;
			grid.BOAnalyzer = new TrackingPackProductAnalyzer();

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((OrgSupplierPartCollection)(((PackProduct)(null)).Lookups.OrgSupplierPartProductCodes)));
			ZCodeFindBoxColumn productColumn = new ZCodeFindBoxColumn(Res.GetString("206c5763-a91c-4a49-a9cb-b1e115eebb9b", "Product"), PackProduct.Schema.D2_ProductCode, "Lookups+OrgSupplierPartProductCodes");
			productColumn.ModuleID = WebModuleIDs.OrgSupplierPartTracking;
			productColumn.AutoPostBack = true;
			productColumn.DisplayNotifications = true;
			productColumn.ValueFieldName = PackProduct.Schema.D2_ProductCode;
			productColumn.TextFieldName = PackProduct.Schema.D2_ProductCode;
			grid.Columns.Add(productColumn);

			ZCalcEditColumn quantityColumn = new ZCalcEditColumn(Res.GetString("3855e7f4-35f4-4d69-a826-83177004a739", "Quantity"), PackProduct.Schema.D2_ProductQuantity);
			grid.Columns.Add(quantityColumn);

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CodeDescriptionPairList)(((PackProduct)(null)).Lookups.UnitOfQuantity)));
			ZDropDownListColumn quanitityUnitColumn = new ZDropDownListColumn(Res.GetString("050b098f-61ca-4596-879f-6f5b9ba2805d", "UQ"), PackProduct.Schema.D2_ProductUnitOfQty, "Lookups+UnitOfQuantity");
			grid.Columns.Add(quanitityUnitColumn);

			ZTextEditColumn descriptionColumn = new ZTextEditColumn(Res.GetString("907a4fb3-2c0d-4d7a-b8eb-28dd4c445dc7", "Description"), "Product+" + OrgSupplierPart.Schema.OP_Desc) { ReadOnly = true };
			grid.Columns.Add(descriptionColumn);

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((OrderLineCollection)(((PackProduct)(null)).Lookups.OrderLines_List)));
			ZFindBoxColumn orderLineColumn = new ZFindBoxColumn(Res.GetString("23771353-4979-43bd-9607-0fe4f58cf632", "Order Line"), PackProduct.Schema.D2_JO, "Lookups+OrderLines_List");
			orderLineColumn.ModuleID = WebModuleIDs.TrackingOrderLines;
			orderLineColumn.AutoPostBack = true;
			orderLineColumn.DisplayNotifications = true;
			orderLineColumn.ValueFieldName = PackProduct.Schema.D2_JO;
			orderLineColumn.TextFieldName = OrderLine.Schema.OrderAndOrderLineNumber;
			grid.Columns.Add(orderLineColumn);
		}
	}
}
