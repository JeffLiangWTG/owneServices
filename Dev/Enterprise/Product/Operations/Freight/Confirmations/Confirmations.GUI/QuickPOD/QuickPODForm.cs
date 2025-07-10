using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class QuickPODForm : ZChildForm, IDoDisplayModeBrowseOverride
	{
		public QuickPODForm(QuickPODs businessEntity)
			: base(businessEntity)
		{
			SetupContextMenu();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			TypeDescriptor.AddAttributes(EditSelectedShipmentButton, new SuppressControlRequiresTextBasherAttribute());
			MissingResourceStringChecker.ExcludeFromTest(this);
		}

		#region InternalMembers
#if DEBUG
		protected internal ZArchitecture.ZGrid QuickPODGridInternal => QuickPODGrid;
		protected internal ZController lastControllerInternal => lastController;
#endif
		#endregion

		#region SetupContextMenu

		void SetupContextMenu()
		{
			MenuItem shipmentMenuItem = new ZMenuItem(ResString.GetMultilingualString("Forwarding.QuickPOD.EditShipmentJob", "Edit Shipment Job"));
			shipmentMenuItem.Click += new EventHandler(ShipmentMenuItem_Click);
			QuickPODGrid.ContextMenu.MenuItems.Add(0, new ZMenuItem("-"));
			QuickPODGrid.ContextMenu.MenuItems.Add(0, shipmentMenuItem);
		}

		#endregion

		#region Events

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (QuickPODs != null)
			{
				QuickPODs.HouseBillMatchesOnMultipleShipments -= new QuickPODMultipleShipmentsEventHandler(QuickPODs_HouseBillMatchesOnMultipleShipments);
				QuickPODs.QuickPODsCollection.ReleaseShipmentJobHeaderMutexes();
			}
			base.SetDataBinding(dataSource, dataMember);
			if (QuickPODs != null)
			{
				QuickPODs.HouseBillMatchesOnMultipleShipments += new QuickPODMultipleShipmentsEventHandler(QuickPODs_HouseBillMatchesOnMultipleShipments);
			}
		}

		#region HouseBillMatchesOnMulitpleShipments

		void QuickPODs_HouseBillMatchesOnMultipleShipments(object sender, QuickPODMultipleShipmentsEventArgs e)
		{
			using (QuickPODShipmentSelectionForm multiShipmentSelectForm = new QuickPODShipmentSelectionForm(e))
			{
				ZFormModaliser.ShowDialogAndDispose(multiShipmentSelectForm);
			}
		}

		#endregion

		#region EditShipment

		internal void ShipmentMenuItem_Click(object sender, EventArgs e)
		{
			EditSelectedShipment();
		}

		void EditSelectedShipmentButton_Click(object sender, EventArgs e)
		{
			EditSelectedShipment();
		}

		void EditSelectedShipment()
		{
			ZController controller = null;
			BusinessObject[] pods = QuickPODGrid.SelectedElements;
			int maximumAllowedEdits = 5;

			if (pods.Length == 0)
			{
				Globals.Message.ShowWarning(Res.GetString("4350f7c0-5ec9-4fee-80ed-28c97041f188", "Please select Shipments."));
			}
			else if (pods.Length > maximumAllowedEdits)
			{
				Globals.Message.ShowWarning(Res.GetString("9f2241be-86b2-48a7-b986-a030bfdd03ce", "Please select no more than {0} shipments to edit. Editing more at the same time can put unnecessary strain on the system.", maximumAllowedEdits));
			}
			else
			{
				bool hasPodsWithNoHouseBill = false;
				StringBuilder warningMessage = new StringBuilder();

				foreach (QuickPOD pod in pods)
				{
					if (pod.Shipment != null)
					{
						ChildEditableService.SetState(pod.Factory, ChildEditableServiceStates.Shipment);
						controller = ZControllerFactory.Create(ControllerIDs.JobShipment);
						controller.SetFormsModalTo(this);
						IZForm shipmentForm = controller.ShowEditForm(pod.Shipment);
						if (shipmentForm != null)
						{
							shipmentForm.Closed += delegate
							{
								pod.RefreshShipment();
							};
						}
					}
					else if (pod.HouseBill.IsEmpty)
					{
						if (!hasPodsWithNoHouseBill)
						{
							hasPodsWithNoHouseBill = true;
							warningMessage.Insert(0, "- " + Res.GetString("c8a0dcbb-e058-41b0-a28b-5e8fc489e69b", "Not all selected shipments have a House Bill #.") + System.Environment.NewLine);
						}
					}
					else
					{
						warningMessage.AppendLine("- " + Res.GetString("7e6f1c73-0149-48ce-9e43-823957542993", "Shipment with House Bill '{0}' not found.", pod.HouseBill));
					}
				}

				if (warningMessage.Length > 0)
				{
					warningMessage.Insert(0, Res.GetString("32d06ef5-73a8-4002-abf2-724121c1fa23", "Can not edit all selected shipments:") + System.Environment.NewLine);
					Globals.Message.ShowWarning(warningMessage.ToString().Trim());
				}
			}
#if DEBUG
			lastController = controller;
#endif
		}

#if DEBUG
		ZController lastController;
#endif

		#endregion

		#endregion

		#region Form Overrides

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}

		void IDoDisplayModeBrowseOverride.DoDisplayModeBrowse()
		{
			ZFormStrategy.DoDisplayModeNew(this);
		}

		#endregion

		#region Implementation

		QuickPODs QuickPODs
		{
			get { return (QuickPODs)DataSource; }
		}

		#endregion
	}
}
