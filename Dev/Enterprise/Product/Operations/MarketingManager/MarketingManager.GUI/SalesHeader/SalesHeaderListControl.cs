using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesHeaderListControl : ZUserControl, IReadOnlyToggleControl
	{
		public SalesHeaderListControl()
		{
			InitializeComponent();
#if !WINZOR
			toolRightCellColor = TopToolStrip.BackColor;
#endif
		}

		#region SalesHeaderCollection

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				SetupButtons();
				RefreshStrips();
				RemoveFocus();
			}
		}

		internal SalesHeaderCollection SalesHeaderCollection
		{
			get
			{
				var entity = (ISalesValueAssociatedEntity)base.DataSource;
				return entity != null ? (SalesHeaderCollection)entity.ProspectiveSalesHeaderCollection : null;
			}
		}

		bool DataSourceReadOnly
		{
			get
			{
				var entity = base.DataSource as BusinessObject;
				return entity?.ReadOnly ?? false;
			}
		}

		#endregion

		#region Refresh

		protected virtual void RefreshStrips()
		{
			StripsPanel.Controls.Clear();

			if (SalesHeaderCollection != null)
			{
				var salesHeadersToAdd = SalesHeaderCollection.Cast<SalesHeader>().Where(x => x.EntitySalesCollectionProductView.Any()).ToList();
				foreach (var salesHeader in salesHeadersToAdd)
				{
					SalesHeaderStripControl.AddFetchHintsForView(salesHeader);
				}

				foreach (var salesHeader in salesHeadersToAdd.OrderBy(x => x.SalesProduct.MP_NameMultilingual))
				{
					AddNewStrip(salesHeader);
				}
			}
		}

		SalesHeaderStripControl AddNewStrip(SalesHeader salesHeader)
		{
			var headerStripControl = new SalesHeaderStripControl();
			headerStripControl.Dock = DockStyle.Top;
			headerStripControl.ReadOnly = ReadOnly || DataSourceReadOnly;
			headerStripControl.SetDataBinding(salesHeader, "");
			StripsPanel.Controls.Add(headerStripControl);
			headerStripControl.BringToFront();
			headerStripControl.Enter += HeaderStripControl_Enter;

			return headerStripControl;
		}

		protected void HeaderStripControl_Enter(object sender, EventArgs e)
		{
			var newFocusedStrip = (ISalesHeaderStripControl)sender;
			if (newFocusedStrip.IsDeleteButtonFocused)
			{
				// Do not focus if user pressing delete button
				var controlToRefocusTo = CurrentlyExpandedStrip;
				if (controlToRefocusTo != null)
				{
					ActiveControl = (Control)controlToRefocusTo;
				}
				return;
			}

			CurrentlyExpandedStrip = newFocusedStrip;
		}

		#endregion

		#region Focus

		public virtual void Focus(OrgSalesProduct salesProduct, bool onNewRow)
		{
			Argument.NotNull(salesProduct, "salesProduct");

			var strip = FindOrCreateSalesHeaderStrip(salesProduct);
			strip.Focus(onNewRow);
		}

		public void Focus(EntitySalesWrapper entitySales)
		{
			Argument.NotNull(entitySales, "sales");

			var salesProduct = (OrgSalesProduct)entitySales.Product;
			if (salesProduct != null)
			{
				var strip = FindOrCreateSalesHeaderStrip(salesProduct);
				strip.Focus(entitySales);
			}
		}

		SalesHeaderStripControl FindOrCreateSalesHeaderStrip(OrgSalesProduct salesProduct)
		{
			Argument.NotNull(salesProduct, "salesProduct");

			var stripControls = StripsPanel.Controls.OfType<SalesHeaderStripControl>();
			var strip = stripControls.FirstOrDefault(x => x.CurrentDataItem.SalesProduct.PK == salesProduct.PK);
			if (strip != null)
			{
				CurrentlyExpandedStrip = strip;
			}
			else
			{
				var salesHeader = SalesHeaderCollection.Cast<SalesHeader>().FirstOrDefault(x => x.SalesProduct == salesProduct) ?? SalesHeaderCollection.AddNew(salesProduct);
				SalesHeaderStripControl.AddFetchHintsForView(salesHeader);
				strip = AddNewStrip(salesHeader);
				salesHeader.HasChanges = true;
			}

			return strip;
		}

		#endregion

		#region CurrentlyExpandedStrip

		ISalesHeaderStripControl CurrentlyExpandedStrip
		{
			get { return currentlyExpandedStrip; }
			set
			{
				if (currentlyExpandedStrip != value)
				{
					StripsPanel.SuspendLayout();
					try
					{
						if (currentlyExpandedStrip != null)
						{
							currentlyExpandedStrip.Collapsed = true;
						}

						currentlyExpandedStrip = value;

						if (value != null)
						{
							value.Collapsed = false;
						}
					}
					finally
					{
						StripsPanel.ResumeLayout();
					}
				}
			}
		}
		ISalesHeaderStripControl currentlyExpandedStrip;

		#endregion

		#region ReadOnly

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if (readOnly != value)
				{
					readOnly = value;
					TopToolStrip.Visible = !ReadOnly;
					foreach (var childControl in StripsPanel.Controls.OfType<ISalesHeaderStripControl>())
					{
						childControl.ReadOnly = value;
					}
				}
			}
		}
		bool readOnly;

		#endregion

		#region Buttons

		protected virtual void SetupButtons()
		{
			if (DataSourceReadOnly)
			{
				AddToolStripDropDownButton.Visible = false;
				CreateQuotationToolStripButton.Visible = false;
			}
			else
			{
				AddToolStripDropDownButton.Image = Icons.GetImage(IconTypes.NewButtonRest);
				CreateQuotationToolStripButton.Image = Icons.GetImage(IconTypes.EditButtonRest);

				if (SalesHeaderCollection != null)
				{
					var allProducts = GetProducts();
					foreach (var product in allProducts)
					{
						AddToolStripDropDownButton.DropDownItems.Add(new ZToolStripMenuItem(EscapeMnemonics(product.MP_NameMultilingual), GetAddHeaderMenuItemHandler(product)));
					}
				}
			}
		}

		#region AddToolStripDropDownButton

		IOrderedEnumerable<OrgSalesProduct> GetProducts()
		{
			var factory = SalesHeaderCollection.Factory;
			var allProducts = factory.Load<OrgSalesProduct>(new ZQuery());
			return allProducts.OrderBy(x => x.MP_NameMultilingual);
		}

		void AddToolStripDropDownButton_DropDownOpening(object sender, EventArgs e)
		{
			if (AddToolStripDropDownButton.DropDownItems.Count == 0)
			{
				Globals.Message.ShowInformation(
						Res.GetString("bcfe0d3e-ad60-4bd8-91a8-888324e2a44a", @"No sales products available.
Sales products can be created under Maintain > Sales & Marketing > Sales Products."),
						Res.GetString("075c86ba-1ec7-43a2-a6c9-37f0223b816d", "Cannot add Sales Product"));
			}
		}

		static string EscapeMnemonics(MultilingualString text)
		{
			return text != null ? text.ToString().Replace("&", "&&") : "";
		}

		EventHandler GetAddHeaderMenuItemHandler(OrgSalesProduct salesProduct)
		{
			return (sender, e) =>
			{
				Focus(salesProduct, true);
			};
		}

		#endregion

		#region MainTableLayoutPanel

#if !WINZOR
		readonly Color toolRightCellColor;
		void MainTableLayoutPanel_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
		{
			if (e.Column == 1 && e.Row == 0)
			{
				var graphics = e.Graphics;

				using (var brush = new SolidBrush(toolRightCellColor))
				{
					graphics.FillRectangle(brush, e.CellBounds);
				}
			}
		}

#endif

		#endregion

		#region CreateQuotationToolStripButton_Click

		void CreateQuotationToolStripButton_Click(object sender, EventArgs e)
		{
			var entity = SalesHeaderCollection?.Entity;
			if (entity != null && (entity is OrgOpportunity || entity is OrgHeader))
			{
				var tradeLanes = SalesHeaderCollection.Cast<SalesHeader>().SelectMany(x => x.EntitySales).SelectMany(x => x.EntityTradeDetails);
				if (tradeLanes.Any())
				{
					var generateQuoteController = GenerateQuoteForSalesValueAssociatedEntityController.New();
					generateQuoteController.ParentModalForm = (ZForm)ParentForm;
					generateQuoteController.Execute(entity, tradeLanes);
				}
				else
				{
					Globals.Message.ShowInformation(
						Res.GetString("235740ef-ee68-409d-aebf-bb1ccadb4d0c", "Please enter trade lanes details before creating a quotation."),
						GenerateQuoteForSalesValueAssociatedEntityController.CannotCreateQuoteCaption);
				}
			}
		}

		#endregion

		#endregion

		#region Strips Panel

		public ZPanel StripsPanel
		{
			get { return stripsPanel; }
		}

		#endregion

		#region RemoveFocus

		void RemoveFocus()
		{
			TopToolStrip.Focus();
			CurrentlyExpandedStrip = null;
		}

		void SalesHeaderListControl_Click(object sender, EventArgs e)
		{
			RemoveFocus();
		}

		void StripsPanel_Click(object sender, EventArgs e)
		{
			RemoveFocus();
		}
		#endregion
	}
}
