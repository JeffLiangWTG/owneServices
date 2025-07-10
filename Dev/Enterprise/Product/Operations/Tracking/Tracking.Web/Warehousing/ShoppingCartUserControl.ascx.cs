using System;
using System.Web.UI.WebControls;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public delegate void NewButtonClickedHandler();
	public delegate void ClearButtonClickedHanlder();

	public partial class ShoppingCartUserControl : BaseUserControl
	{
		public event NewButtonClickedHandler NewButtonClicked;
		public event ClearButtonClickedHanlder ClearButtonClicked;

		protected Button NewButton;
		protected Button ClearButton;
		protected System.Web.UI.HtmlControls.HtmlGenericControl OrderLinesGridDiv;
		public ZDataGrid OrderLinesGrid;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
			SetupGrid();
		}

		public void SetupGrid()
		{
			OrderLinesGridDiv.Attributes["class"] = ZCssHelper.Join(CssConstants.ContentSection, CssConstants.Scrollable);
			OrderLinesGrid.CssClass = ZCssHelper.Join(OrderLinesGrid.CssClass, TableCss);
			OrderLinesGrid.HeaderStyle.CssClass = ZCssHelper.Join(OrderLinesGrid.HeaderStyle.CssClass, HeaderCss);
			OrderLinesGrid.ItemStyle.CssClass = ZCssHelper.Join(OrderLinesGrid.ItemStyle.CssClass, ItemCss);
			OrderLinesGrid.AlternatingItemStyle.CssClass = ZCssHelper.Join(OrderLinesGrid.AlternatingItemStyle.CssClass, AlternatingCss);
			OrderLinesGrid.SelectedItemStyle.CssClass = ZCssHelper.Join(OrderLinesGrid.SelectedItemStyle.CssClass, SelectedCss);

			SetupGridColumns();

			OrderLinesGrid.PagerStyle.Mode = PagerMode.NumericPages;
			OrderLinesGrid.PagerStyle.CssClass = ZCssHelper.Join(OrderLinesGrid.CssClass, PagerCss);
			OrderLinesGrid.AllowPaging = true;
			OrderLinesGrid.PageSize = PageSize;
			OrderLinesGrid.AllowSorting = false;
		}

		protected void SetupGridColumns()
		{
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("ef056d58-3c6b-4353-9b69-a6308187603c", "Product"), TrackingWhsOrderLine.WrapperSchema.ProductCode));
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("70df18ca-c6ca-424b-8daa-af0157d7b535", "Description"), TrackingWhsOrderLine.WrapperSchema.ProductDescription));
			ZCalcEditColumn quantityColumn = new ZCalcEditColumn(Res.GetString("7dcae526-de56-42c8-8c82-a88839a99fd1", "Quantity"), TrackingWhsOrderLine.WrapperSchema.WE_TransactionQuantity);
			quantityColumn.BindToDecimals = TrackingWhsOrderLine.GetSchemaPath("SupplierPart.OP_CountDecimalPlaces");
			quantityColumn.ItemTemplate = quantityColumn.GetNewEditItemTemplate();
			OrderLinesGrid.Columns.Add(quantityColumn);
			OrderLinesGrid.Columns.Add(new ZTextEditColumn("UQ", TrackingWhsOrderLine.WrapperSchema.ProductUQ));
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("d5f9a8e0-1c49-42de-9770-1df981039366", "Shortfall Qty."), TrackingWhsOrderLine.WrapperSchema.WE_ShortfallQuantityCached));

			if (Page.SiteUser != null && ((OrgContactWebUser)Page.SiteUser).LoggedInOrganisation != null)
			{
				var partManager = ((TrackingSiteUser)Page.SiteUser).LoggedInOrganisation.PartAttributeManager;

				AddClientDependantColumn(OrderLinesGrid.Columns, partManager.IsPartAttributeUsedByOrganisation(1), new ZTextEditColumn(partManager.PartAttributeName1, TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib1));
				AddClientDependantColumn(OrderLinesGrid.Columns, partManager.IsPartAttributeUsedByOrganisation(2), new ZTextEditColumn(partManager.PartAttributeName2, TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib2));
				AddClientDependantColumn(OrderLinesGrid.Columns, partManager.IsPartAttributeUsedByOrganisation(3), new ZTextEditColumn(partManager.PartAttributeName3, TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib3));
				AddClientDependantColumn(
					OrderLinesGrid.Columns,
					partManager.IsSerialNumberUsedByOrganisation,
					new ZTextEditColumn(Res.GetString("8642b47d-e323-4e6a-95d2-85ec7d05c5e6", "Serial Number"), TrackingWhsOrderLine.WrapperSchema.WE_SerialNumber));
				AddClientDependantColumn(OrderLinesGrid.Columns, partManager.IsPackingDateUsedByOrganisation, new ZDateTimeColumn(Res.GetString("6e4664c3-6be0-498b-a2b1-d688d2752e6e", "Packing Date"), TrackingWhsOrderLine.WrapperSchema.WE_PackingDate, ZDateTimePickerFormat.Short));
				AddClientDependantColumn(OrderLinesGrid.Columns, partManager.IsExpiryDateUsedByOrganisation, new ZDateTimeColumn(Res.GetString("8d8c1e47-7248-4caa-ba3d-914fedc18a32", "Expiry Date"), TrackingWhsOrderLine.WrapperSchema.WE_ExpiryDate, ZDateTimePickerFormat.Short));
			}
		}

		protected void AddClientDependantColumn(DataGridColumnCollection columns, bool isUsed, ZTemplateColumn column)
		{
			if (isUsed)
			{
				columns.Add(column);
			}
		}

		public int PageSize
		{
			get
			{
				if (fPageSize == 0)
				{
					fPageSize = WebDataRegistry.Instance.PageSize.Value;
				}
				return fPageSize;
			}
			set { fPageSize = value; }
		}
		protected int fPageSize;

		#region Css Styles

		protected string TableCss
		{
			get { return CssConstants.DetailsTable; }
		}

		protected string ItemCss
		{
			get { return CssConstants.DetailsCell; }
		}

		protected string AlternatingCss
		{
			get { return CssConstants.DetailsAlternatingCell; }
		}

		protected string SelectedCss
		{
			get { return CssConstants.DetailsSelectedCell; }
		}

		protected string PagerCss
		{
			get { return CssConstants.ResultsTablePager; }
		}

		protected string HeaderCss
		{
			get { return CssConstants.DetailsHeader; }
		}

		#endregion

		protected void InitializeComponent()
		{
			var newButtonText = Res.GetString("f3b9ef09-7fba-4f7f-8955-ba1d775b9f53", "Create New Order");
			this.NewButton.Text = newButtonText;
			this.NewButton.ToolTip = newButtonText;
			this.NewButton.Click += new EventHandler(this.NewButton_Click);

			var clearButtonText = Res.GetString("aae52662-5a9b-437a-b896-56b3ce0191fe", "Clear Allocated Lines");
			this.ClearButton.Text = clearButtonText;
			this.ClearButton.ToolTip = clearButtonText;
			this.ClearButton.Click += new EventHandler(this.ClearButton_Click);
		}

		protected void NewButton_Click(object sender, EventArgs e)
		{
			if (NewButtonClicked != null)
			{
				NewButtonClicked();
			}
		}

		protected void ClearButton_Click(object sender, EventArgs e)
		{
			if (ClearButtonClicked != null)
			{
				ClearButtonClicked();
			}
		}
	}
}
