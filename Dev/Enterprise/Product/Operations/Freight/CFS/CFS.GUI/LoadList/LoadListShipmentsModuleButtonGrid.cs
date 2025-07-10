using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class LoadListShipmentsModuleButtonGrid : ZModuleButtonGrid, IGridControl
	{
		public LoadListShipmentsModuleButtonGrid()
		{
			InitializeComponent();
		}

		protected CFSLoadListConsol ParentConsol { get; private set; }

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				CleanUp();

				ParentConsol = (CFSLoadListConsol)dataSource;
				SetUp();
			}
			else
			{
				CleanUp();
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		void SetUp()
		{
			if (ParentConsol != null)
			{
				ParentConsol.Shipments.CountChanged += new CollectionCountChangedEventHandler(OnShipments_CountChanged);
				foreach (CFSShipment shipment in ParentConsol.Shipments)
				{
					AddShipmentEvents(shipment);
				}

				string message;
				ParentConsol.Shipments.AllowAddNew = CFSShipmentVsConsolMessageHelper.Instance.IsAllowedToAddNewShipment(out message, ParentConsol);
			}
		}

		void CleanUp()
		{
			if (ParentConsol != null)
			{
				ParentConsol.Shipments.CountChanged -= new CollectionCountChangedEventHandler(OnShipments_CountChanged);
				foreach (CFSShipment shipment in ParentConsol.Shipments)
				{
					RemoveShipmentEvents(shipment);
				}

				ParentConsol = null;
			}
		}

		void OnShipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var shipment = (CFSShipment)e.BizObject;
			if (e.ItemAdded)
			{
				AddShipmentEvents(shipment);
			}
			else
			{
				RemoveShipmentEvents(shipment);
			}
		}

		void AddShipmentEvents(CFSShipment shipment)
		{
			if (shipment != null)
			{
				shipment.MasterChanged += new EventHandler<MasterChangedEventArgs>(OnShipment_MasterChanged);
			}
		}

		void RemoveShipmentEvents(CFSShipment shipment)
		{
			if (shipment != null)
			{
				shipment.MasterChanged -= new EventHandler<MasterChangedEventArgs>(OnShipment_MasterChanged);
			}
		}

		void OnShipment_MasterChanged(object sender, MasterChangedEventArgs e)
		{
			ShipmentVsConsolGUIMessageHelper.Instance.OnShipmentMasterChanged(CFSShipmentVsConsolMessageHelper.Instance, (CFSShipment)sender, ParentConsol, e);
		}

		#endregion // Binding

		#region Buttons

		protected override void NewButton_Click(object sender, EventArgs e)
		{
			string errorMessage;
			if (!CFSShipmentVsConsolMessageHelper.Instance.IsAllowedToAddNewShipment(out errorMessage, ParentConsol))
			{
				Globals.Message.ShowError(errorMessage);
				return;
			}

			base.NewButton_Click(sender, e);
		}

		protected override void AttachButton_Click(object sender, EventArgs e)
		{
			var attachRequest = CFSShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(ParentConsol, null);
			if (!attachRequest.Errors.IsEmpty)
			{
				Globals.Message.ShowError(attachRequest.Errors);
				return;
			}

			base.AttachButton_Click(sender, e);
		}

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			SelectFirstRowIfOnlyRowInGrid();

			if (ParentConsol != null && InnerGrid.SelectedElements != null && InnerGrid.SelectedElements.Length > 0)
			{
				var selectedShipments = InnerGrid.SelectedElements.Cast<CommonShipment>();
				if (!ShipmentVsConsolGUIMessageHelper.Instance.IsAllowedToDetachShipments(CFSShipmentVsConsolMessageHelper.Instance, ParentConsol, selectedShipments))
				{
					return;
				}
			}

			base.DetachButton_Click(sender, e);
		}

		#endregion //Buttons

		#region RecordAttacher

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new LoadListShipmentsModuleButtonGridAttacher(ParentConsol, destinationCollection, findBoxList, moduleID);
		}

		internal class LoadListShipmentsModuleButtonGridAttacher : ZRecordAttacher
		{
			public LoadListShipmentsModuleButtonGridAttacher(CFSLoadListConsol parentConsol, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(destinationCollection, findBoxList, moduleID)
			{
				this.parentConsol = parentConsol;
			}

			readonly CFSLoadListConsol parentConsol;

			protected override bool CheckAttaching(List<BusinessObject> businessObjectsToAttach)
			{
				bool result = base.CheckAttaching(businessObjectsToAttach);
				if (result && this.parentConsol != null)
				{
					string caption = Res.GetString("01695006-0441-484b-a963-a07c878560ee", "Attaching Shipments...");
					var shipmentsToRemove = new List<CFSShipment>();
					var shipmentsToCheck = new List<CFSShipment>();
					var shipmentsToSkip = new List<CFSShipment>();
					var checkMessageList = new List<string>();
					var skipMessageList = new List<string>();
					foreach (CFSShipment shipment in businessObjectsToAttach)
					{
						var master = shipment.CoLoadMasterShipment;
						var attachRequest = CFSShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(parentConsol, shipment);
						if (master != null)
						{
							if (businessObjectsToAttach.Contains(master))
							{
								shipmentsToRemove.Add(shipment);
							}
							else if (!attachRequest.Errors.IsEmpty)
							{
								skipMessageList.Add(attachRequest.Errors);
								shipmentsToSkip.Add(shipment);
							}
							else if (!master.Consols.Contains(this.parentConsol))
							{
								checkMessageList.Add(Res.GetString("2929e810-64ee-4830-ad61-cf26a87a279a",
									"{0} is a sub-shipment of master/lead {1}",
									shipment.JS_UniqueConsignRef,
									master.JS_UniqueConsignRef));

								shipmentsToCheck.Add(shipment);
							}
						}
						else if (!attachRequest.Errors.IsEmpty)
						{
							skipMessageList.Add(attachRequest.Errors);
							shipmentsToSkip.Add(shipment);
						}
					}

					if (shipmentsToSkip.Count > 0)
					{
						skipMessageList.Sort();

						string message = Res.GetString("6d7866ab-add3-4ef4-9360-3b727fb939eb",
							"Of the shipments you are trying to attach to the load list {1}, there are shipments that cannot be attached for the following reasons:{2}{0}",
							string.Join(System.Environment.NewLine, skipMessageList.ToArray()),
							this.parentConsol.JK_UniqueConsignRef,
							System.Environment.NewLine);

						if (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel) == DialogResult.Cancel)
						{
							return false;
						}

						shipmentsToRemove.AddRange(shipmentsToSkip);
					}

					if (shipmentsToCheck.Count > 0)
					{
						checkMessageList.Sort();

						string message = Res.GetString("e38ea64f-2bc1-4e84-b187-dc26e926ac96",
							"The shipments that you are trying to attach are sub-shipments:\r\n{0}\r\n\r\nOnly these shipments, without their masters, will be attached to {1} load list.\r\n\r\nIf you would like to attach these shipments, their masters/leads and all sub-shipments of their masters/leads to this load list, you need to attach the master/lead shipments to this load list instead.\r\n\r\nPress [Yes] if you would like to continue.\r\nPress [No] if you would like to skip this shipments and apply all other selected shipments.\r\nPress [Cancel] to cancel operation.",
							string.Join(System.Environment.NewLine, checkMessageList.ToArray()),
							this.parentConsol.JK_UniqueConsignRef);

						var dilogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, DialogResult.Cancel);
						switch (dilogResult)
						{
							case DialogResult.Yes:
								break;
							case DialogResult.No:
								shipmentsToRemove.AddRange(shipmentsToCheck);
								break;
							default:
								return false;
						}
					}

					foreach (var shipment in shipmentsToRemove)
					{
						businessObjectsToAttach.Remove(shipment);
					}

					result = businessObjectsToAttach.Count > 0;
				}

				return result;
			}
		}

		#endregion
	}
}
