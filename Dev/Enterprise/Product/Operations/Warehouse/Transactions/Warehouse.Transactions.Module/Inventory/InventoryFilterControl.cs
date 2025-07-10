using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class InventoryFilterControl : ZFilterStripControl
	{
		#region Constructors

		public InventoryFilterControl()
		{
			InitializeComponent();
		}

		public InventoryFilterControl(IBusinessObjectCollection gridCollection, InventoryFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			this.FiltersCleared += InventoryFilterControl_FiltersCleared;
			SetColumnsAvailability();
		}

		void SetColumnsAvailability()
		{
			if (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				Grid.SetAvailability(false, WhsInventoryViewSchema.WI_SerialNumber.Name);
			}
		}

		public InventoryFilterControl(WhsDocket docket)
			: this(docket.InventoryFilter, new InventoryFilterBusinessObject(docket))
		{
			Docket = docket;

			var docketType = docket.GetType();
			DataSourceAssemblyName = docketType.Assembly.GetName().Name;
			DataSourceTypeName = docketType.FullName;
			FilteredGrid.BindTo = "InventoryFilter";
			PerformSearch += InventoryFilterControl_PerformSearch;
		}

		readonly WhsDocket Docket;

		#endregion

		#region Properties

		InventoryFilterBusinessObject InvFilterBusinessObject
		{
			get { return (InventoryFilterBusinessObject)FilterBusinessObject; }
		}

		#endregion

		#region ZFilterControl Overloads

		protected override void BindCore()
		{
			base.BindCore();

			this.PartAttributeColumnManager = new PartAttributeColumnManager(this.FilteredGrid,
				WhsInventoryViewSchema.WI_ExpiryDate.Name,
				WhsInventoryViewSchema.WI_PackingDate.Name,
				WhsInventoryViewSchema.WI_PartAttrib1.Name,
				WhsInventoryViewSchema.WI_PartAttrib2.Name,
				WhsInventoryViewSchema.WI_PartAttrib3.Name,
				WhsInventoryViewSchema.WI_SerialNumber.Name,
				WhsInventoryView.Schema.IsExpired);

			InvFilterBusinessObject.WarehouseChanged += WarehouseChanged;
			InvFilterBusinessObject.ClientChanged += ClientChanged;
			OnWarehouseChanged();
			OnClientChanged();
		}

		PartAttributeColumnManager PartAttributeColumnManager;

		#endregion

		#region Events

		#region InventoryFilterControl_FiltersCleared

		void InventoryFilterControl_FiltersCleared(object sender, EventArgs e)
		{
			OnWarehouseChanged();
			OnClientChanged();
		}

		#endregion

		#region WarehouseChanged

		void WarehouseChanged(object sender, EventArgs e)
		{
			OnWarehouseChanged();
		}

		void OnWarehouseChanged()
		{
			var whs = InvFilterBusinessObject.Warehouse;
			FilteredGrid.SetColumnVisible(whs == null || whs.IsWarehouseBondEnabled, WhsInventoryViewSchema.WI_BondedEntryKey.Name);

			bool showPackageIdColumn = whs.IsBondedEnabledAndUSJurisdiction();
			FilteredGrid.SetAvailability(showPackageIdColumn, WhsInventoryView.Schema.PackageGroupId);
		}

		#endregion

		#region ClientChanged

		void ClientChanged(object sender, EventArgs e)
		{
			OnClientChanged();
		}

		void OnClientChanged()
		{
			var client = InvFilterBusinessObject.Client;
			PartAttributeColumnManager.SetColumns(client);
		}

		#endregion

		#region InventoryFilterControl_PerformSearch

		void InventoryFilterControl_PerformSearch(object sender, EventArgs e)
		{
			if (Docket != null && FilterBusinessObject != null)
			{
				Docket.InventoryFilter.Load(FilterBusinessObject.Filter);
			}
		}

		#endregion

		#endregion
	}
}
