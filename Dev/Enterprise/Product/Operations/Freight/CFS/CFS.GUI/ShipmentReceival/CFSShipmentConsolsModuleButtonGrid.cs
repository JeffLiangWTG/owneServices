using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class CFSShipmentConsolsModuleButtonGrid : ZModuleButtonGrid
	{
		public CFSShipmentConsolsModuleButtonGrid()
			: base()
		{
			InitializeComponent();
		}

		protected CFSShipment ParentShipment
		{
			get { return ((ZForm)ParentForm).BusinessEntity as CFSShipment; }
		}

		#region Buttons

		protected override void NewButton_Click(object sender, EventArgs e)
		{
			string errorMessage;
			if (!CFSShipmentVsConsolMessageHelper.Instance.IsAllowedToAddNewConsol(out errorMessage, ParentShipment))
			{
				Globals.Message.ShowError(errorMessage);
			}
			else if (ParentShipment.CoLoadMasterShipment != null && !AskUserToContinue(GetMessage_AddConsolToSubShipment()))
			{
				return;
			}
			else
			{
				base.NewButton_Click(sender, e);
			}
		}

		protected override void AttachButton_Click(object sender, EventArgs e)
		{
			var attachRequest = CFSShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachConsol(ParentShipment, null);
			if (!attachRequest.Errors.IsEmpty)
			{
				Globals.Message.ShowError(attachRequest.Errors);
			}
			else if (ParentShipment.CoLoadMasterShipment != null && !AskUserToContinue(GetMessage_AddConsolToSubShipment()))
			{
				return;
			}
			else
			{
				base.AttachButton_Click(sender, e);
			}
		}

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			SelectFirstRowIfOnlyRowInGrid();

			if (ParentShipment != null && InnerGrid.SelectedElements != null && InnerGrid.SelectedElements.Length > 0)
			{
				var selectedConsols = InnerGrid.SelectedElements.Cast<CommonConsol>();
				if (!ShipmentVsConsolGUIMessageHelper.Instance.IsAllowedToDetachConsols(CFSShipmentVsConsolMessageHelper.Instance, ParentShipment, selectedConsols))
				{
					return;
				}
			}

			base.DetachButton_Click(sender, e);
		}

		#endregion

		#region Implementation

		bool AskUserToContinue(string message)
		{
			string caption = Res.GetString("f602cdca-14e1-4343-8b6d-2de9bc1b4b5c", "Shipment Load Lists");
			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK;
		}

		string GetMessage_AddConsolToSubShipment()
		{
			return Res.GetString("e65e4d99-7bef-4e72-9634-4e827f9c248d",
				"The shipment {0} is a sub-shipment of the master/lead shipment {1}. A new load list will be attached to this shipment only, without attaching to its master.\r\n\r\nIf you would like to attach a new load list to this shipment, its master/lead and all sub-shipments of its master/lead, you need to attach a new load list to the master/lead shipment {1} instead.",
				ParentShipment.JS_UniqueConsignRef,
				ParentShipment.CoLoadMasterShipment.JS_UniqueConsignRef);
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new CFSShipmentConsolsModuleButtonGridAttacher(ParentShipment, destinationCollection, findBoxList, moduleID);
		}

		internal class CFSShipmentConsolsModuleButtonGridAttacher : ZRecordAttacher
		{
			public CFSShipmentConsolsModuleButtonGridAttacher(CFSShipment parentShipment, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(destinationCollection, findBoxList, moduleID)
			{
				this.parentShipment = parentShipment;
			}

			readonly CFSShipment parentShipment;

			protected override bool CheckAttaching(List<BusinessObject> businessObjectsToAttach)
			{
				bool result = base.CheckAttaching(businessObjectsToAttach);
				if (result && this.parentShipment != null)
				{
					var consolsToSkip = new List<CFSLoadListConsol>();
					var skipMessageList = new List<string>();
					foreach (CFSLoadListConsol consol in businessObjectsToAttach)
					{
						var attachRequest = CFSShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachConsol(parentShipment, consol);
						if (!attachRequest.Errors.IsEmpty)
						{
							skipMessageList.Add(attachRequest.Errors);
							consolsToSkip.Add(consol);
						}
					}

					if (consolsToSkip.Count > 0)
					{
						skipMessageList.Sort();

						string caption = Res.GetString("cbe6f689-ceaf-4e78-990e-745778fd8f5e", "Attaching Load Lists...");
						string message = Res.GetString("3a02597c-c523-418d-b1a1-a9d82dac48e9",
							"Of the load lists you are trying to attach to the shipment {1}, there are load lists that cannot be attached for the following reasons:{2}{0}",
							string.Join(System.Environment.NewLine, skipMessageList.ToArray()),
							this.parentShipment.JS_UniqueConsignRef,
							System.Environment.NewLine);

						if (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel) == DialogResult.Cancel)
						{
							return false;
						}

						foreach (var consol in consolsToSkip)
						{
							businessObjectsToAttach.Remove(consol);
						}
					}

					result = businessObjectsToAttach.Count > 0;
				}

				return result;
			}
		}

		#endregion
	}
}
