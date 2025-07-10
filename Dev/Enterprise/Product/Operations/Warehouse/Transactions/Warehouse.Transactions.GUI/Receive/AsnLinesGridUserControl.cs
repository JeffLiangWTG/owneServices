using System;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class AsnLinesGridUserControl : ZUserControl, IBindTo
	{
		#region Constructors

		public AsnLinesGridUserControl()
		{
			InitializeComponent();
		}

		void Grid_AfterBind(object sender, EventArgs e)
		{
			if (Receive != null && this.Grid.ListManager != null &&
				WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				this.Grid.ListManager.CurrentChanged += ListManager_CurrentChanged;
				if (Grid.ListManager.Position == -1)
				{
					Grid.ListManager.Position = 0; // Display the serial number for the first line, if it exists for the first time the tab is selected.
				}
				SetSerialNumberControlVisiblity();
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e) => SetSerialNumberControlVisiblity();

		void SetSerialNumberControlVisiblity()
		{
			var parent = Grid.ListManager.GetCurrent() as ISerialNumberParent;
			if (parent != null)
			{
				SerialNumberControl.Visible = parent.IsSerialNumberUsed;
			}
		}

		#endregion

		#region Properties

		public WhsReceive Receive => (WhsReceive)CurrentDataItem;

		#endregion

		#region Binding

		public string BindTo
		{
			get => Grid.BindTo;
			set => Grid.BindTo = value;
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Receive != null)
			{
				Receive.ClientChanged -= OnClientChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Receive != null)
			{
				Receive.ClientChanged += OnClientChanged;
				OnClientChanged(this, e);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Receive != null)
				{
					Receive.ClientChanged -= OnClientChanged;
					if (Grid.ListManager != null && WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
					{
						Grid.ListManager.CurrentChanged -= ListManager_CurrentChanged;
					}
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Events

		void OnClientChanged(object sender, EventArgs e)
		{
			PartAttributeColumnManager.SetColumns(Receive.Client);
		}

		#endregion

		#region Implementation

		PartAttributeColumnManager PartAttributeColumnManager
		{
			get
			{
				if (partAttributeColumnManager == null)
				{
					partAttributeColumnManager = new PartAttributeColumnManager(
						Grid,
						WhsAsnLineSchema.Constants.WN_ExpiryDate,
						WhsAsnLineSchema.Constants.WN_PackingDate,
						WhsAsnLineSchema.Constants.WN_PartAttrib1,
						WhsAsnLineSchema.Constants.WN_PartAttrib2,
						WhsAsnLineSchema.Constants.WN_PartAttrib3,
						WhsAsnLineSchema.Constants.WN_SerialNumber);
				}
				return partAttributeColumnManager;
			}
		}
		PartAttributeColumnManager partAttributeColumnManager;

		#endregion
	}
}
