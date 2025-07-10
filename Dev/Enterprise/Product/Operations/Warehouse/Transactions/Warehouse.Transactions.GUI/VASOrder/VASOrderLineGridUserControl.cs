using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class VASOrderLineGridUserControl : ZUserControl
	{
		public VASOrderLineGridUserControl()
		{
			InitializeComponent();
		}

		#region Properties

		WhsVASOrder VASOrder => CurrentDataItem as WhsVASOrder;

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnhookEvents();

			base.OnCurrentDataItemChanged(e);

			HookEvents();
		}

		#endregion

		#region OnVisibleChanged

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				SetPartAttributeColumns();
			}
		}

		#endregion

		#region Hook / Unhook Events

		void HookEvents()
		{
			var vasOrder = VASOrder;
			if (vasOrder != null)
			{
				vasOrder.WVO_OH_ClientInfo.ValueChanged += OnClientChanged;
			}
		}

		void UnhookEvents()
		{
			var vasOrder = VASOrder;
			if (vasOrder != null)
			{
				vasOrder.WVO_OH_ClientInfo.ValueChanged -= OnClientChanged;
			}
		}

		#endregion

		#region Actions

		void OnClientChanged(object sender, EventArgs e)
		{
			SetPartAttributeColumns();
		}

		void SetPartAttributeColumns()
		{
			var vasOrder = VASOrder;
			PartAttributeColumnManager.SetColumns(vasOrder?.Client);  
		}

		PartAttributeColumnManager PartAttributeColumnManager
		{
			get
			{
				if (partAttributeColumnManager == null)
				{
					partAttributeColumnManager = new PartAttributeColumnManager(this.VASOrderLineGrid,
						 WhsVASOrderLineSchema.WVL_ExpiryDate.Name,
						 WhsVASOrderLineSchema.WVL_PackingDate.Name,
						 WhsVASOrderLineSchema.WVL_PartAttrib1.Name,
						 WhsVASOrderLineSchema.WVL_PartAttrib2.Name,
						 WhsVASOrderLineSchema.WVL_PartAttrib3.Name,
						 WhsVASOrderLineSchema.WVL_SerialNumber.Name);
				}
				return partAttributeColumnManager;
			}
		}
		PartAttributeColumnManager partAttributeColumnManager;

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if ((components != null))
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
