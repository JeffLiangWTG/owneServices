using System;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ShipmentDetails : ZUserControl
	{
		public ShipmentDetails()
		{
			InitializeComponent();
			SetupWarehouseLocationControls();
			new UNDGDataItemFormManager(PacklinesGrid).Initialize();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				this.NumbersTabPage.TabVisible = true;
			}
			else
			{
				this.NumbersTabPage.TabVisible = false;
			}
		}

		CFSShipment ParentShipment
		{
			get { return (CFSShipment)CurrentDataItem; }
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (ParentShipment != null)
			{
				ParentShipment.JS_ShipmentTypeInfo.ValueChanged -= new EventHandler(JS_ShipmentTypeInfo_ValueChanged);
				ParentShipment.MasterChanged -= new EventHandler<MasterChangedEventArgs>(OnShipment_MasterChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (ParentShipment != null)
			{
				ParentShipment.JS_ShipmentTypeInfo.ValueChanged += new EventHandler(JS_ShipmentTypeInfo_ValueChanged);
				ParentShipment.MasterChanged += new EventHandler<MasterChangedEventArgs>(OnShipment_MasterChanged);
			}
		}

		void OnShipment_MasterChanged(object sender, MasterChangedEventArgs e)
		{
			ShipmentVsConsolGUIMessageHelper.Instance.OnShipmentMasterChanged(CFSShipmentVsConsolMessageHelper.Instance, ParentShipment, null, e);
		}

		#endregion

		#region GUI Setup

		void JS_ShipmentTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ConsignorDocumentaryDocAddressControl.Text = (ParentShipment.IsCoLoadMaster || ParentShipment.IsBlindCoLoadMaster)
				? Res.GetString("67e62497-4f03-4dc5-9a7f-fd724dc7b4da", "Sending Forwarder")
				: Res.GetString("c21c28a4-4cc7-4cee-ba50-801a3fdf1835", "Consignor");

			ConsigneeDocumentaryDocAddressControl.Text = (ParentShipment.IsCoLoadMaster || ParentShipment.IsBlindCoLoadMaster)
				? Res.GetString("e5d2e62c-8ecb-4a81-96a3-b82d8c8b60ca", "Receiving Forwarder")
				: Res.GetString("5011afa6-deb4-4305-a6df-85e983ceb7e0", "Consignee");
		}

		void SetupWarehouseLocationControls()
		{
			SetControlVisibility();
			CheckLocationGridColumns();
		}

		void SetControlVisibility()
		{
			bool useTrueWHSLocation = Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value;

			WarehouseDropEdit.Visible = useTrueWHSLocation;
			LocationTextBox.Visible = useTrueWHSLocation;

			WarehouseTextBox.Visible = !useTrueWHSLocation;
		}

		void CheckLocationGridColumns()
		{
			foreach (ZGridColumnInfo info in LocationsGrid.ColumnStyles.ToArray())
			{
				if (Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value)
				{
					if (info.ColumnName == PackLocation.Schema.JQ_WarehouseLocation)
					{
						LocationsGrid.ColumnStyles.Remove(info);
					}
				}
				else
				{
					if (info.ColumnName == "LocationString" || info.ColumnName == "LocationWhsGuid")
					{
						LocationsGrid.ColumnStyles.Remove(info);
					}
				}
			}
		}

		#endregion
	}
}
