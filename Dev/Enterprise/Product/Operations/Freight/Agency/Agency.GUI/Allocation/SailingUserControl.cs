using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class SailingUserControl : ZUserControl
	{
		public SailingUserControl()
		{
			InitializeComponent();
		}

		#region SetButtonsEnabled

		public void SetButtonsEnabled(bool enabled)
		{
			createSailingButton.Enabled = enabled;
			selectSailingButton.Enabled = enabled;
			editSailingButton.Enabled = enabled;
			clearSailingButton.Enabled = enabled;
		}

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Shipment != null)
			{
				Shipment.JS_JXInfo.ValueChanged -= new EventHandler(SailingChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				Shipment.JS_JXInfo.ValueChanged += new EventHandler(SailingChanged);
			}
			SailingChanged(null, new EventArgs());
		}

		AgencyShipment Shipment
		{
			get { return (AgencyShipment)CurrentDataItem; }
		}

		#endregion

		#region Events

		#region SelectSailingButton

		void SelectSailingButton_Click(object sender, EventArgs e)
		{
			if (SailingCannotBeModifiedDueToInvoicedCharges)
			{
				Globals.Message.Show(SailingScheduleInvoiced);
			}
			else
			{
				SailingIFindBox helper = new SailingIFindBox((ISailingParentFindBox)CurrentDataItem, ParentZForm);
				helper.ShowModuleFromISailingParent();
			}
		}

		#endregion

		#region CreateSailingButton

		void CreateSailingButton_Click(object sender, EventArgs e)
		{
			if (SailingCannotBeModifiedDueToInvoicedCharges)
			{
				Globals.Message.Show(SailingScheduleInvoiced);
			}
			else
			{
				IVoyageFinderParent voyageFinderParent = Shipment;
				if (voyageFinderParent != null && !voyageFinderParent.LoadPort.IsEmpty && !voyageFinderParent.DischargePort.IsEmpty && !Shipment.JS_A_BKD.IsEmpty)
				{
					VoyageFinder voyageFinder = new VoyageFinder(Shipment);
					VesselVoyageForm voyageForm = new VesselVoyageForm(voyageFinder, Shipment.JS_TransportMode);
					ZFormModaliser.Show(voyageForm, ParentZForm);

					voyageForm.Closed += new EventHandler(VoyageForm_Closed);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("0726d5f1-fc13-4d09-8491-86e1d4566cdc", "An Origin & Destination or Load & Discharge must exist before a new Sailing can be created."), Res.GetString("b8b430f7-eae7-4f78-8fc8-d333fe141d1f", "Add Sailing Error"));
				}
			}
		}

		void VoyageForm_Closed(object sender, EventArgs e)
		{
			var form = (VesselVoyageForm)sender;

			if (Shipment != null && !Shipment.JS_A_BKD.IsEmpty && !Shipment.JS_NKDischargePort.IsEmpty
				&& !Shipment.JS_NKLoadPort.IsEmpty && form.NewSailingCreated)
			{
				if (form.RequiredSailing != null)
				{
					Shipment.JS_JX = form.RequiredSailing.PK;
				}
			}
		}

		#endregion

		#region EditSailingButton

		void EditSailingButton_Click(object sender, EventArgs e)
		{
			if (Shipment.Sailing != null && Shipment.Sailing.Voyage != null)
			{
				if (Shipment.Sailing.IsInDatabase)
				{
					ZController controller = ZControllerFactory.Create(ControllerIDs.JobSeaVoyage);
					controller.ShowEditForm(Shipment.Sailing.Voyage);
#if DEBUG
					LastCreatedControllerForTesting = controller;
#endif
				}
				else
				{
					Globals.Message.Show(Res.GetString("5d5bead3-32f1-4151-aaa5-9fdbc50f85b6", "Please save this Booking before attempting to edit the Sailing."));
				}
			}
			else
			{
				Globals.Message.Show(SailingScheduleNotCreated);
			}
		}

		#endregion

		#region ClearSailingButton_Click

		void ClearSailingButton_Click(object sender, EventArgs e)
		{
			if (SailingCannotBeModifiedDueToInvoicedCharges)
			{
				Globals.Message.Show(SailingScheduleInvoiced);
			}
			else
			{
				Shipment.JS_JX = ZGuid.Empty;
			}
		}

		#endregion

		#region SailingChanged

		void SailingChanged(object sender, EventArgs e)
		{
			ZString origin;
			ZString destination;

			if (Shipment != null && Shipment.Sailing != null)
			{
				origin = Shipment.Sailing.Origin.JA_RL_NKPortOfLoading;
				destination = Shipment.Sailing.Destination.JB_RL_NKPortOfDischarge;
			}
			else
			{
				origin = ZString.Empty;
				destination = ZString.Empty;
			}

			if (ImportExportHelper.IsImport(origin, destination))
			{
				SetImportExportPanel(ImportPanel);
			}
			else
			{
				SetImportExportPanel(ExportPanel);
			}
		}

		#endregion

		bool SailingCannotBeModifiedDueToInvoicedCharges
		{
			get
			{
				return Shipment != null
					&& !LinerAgencyDataRegistry.Instance.AllowSailingChangeWhenInvoiceIsPosted.Value
					&& Shipment.HasPostedCharges;
			}
		}

		internal static string SailingScheduleInvoiced
		{
			get { return Res.GetString("8ab92db4-1e5f-47d7-867c-5dc4a3e19732", "Unable to create/select/clear Sailing Schedule because job has been invoiced."); }
		}

		internal static string SailingScheduleNotCreated
		{
			get { return Res.GetString("44c678a7-e451-44a5-91f4-4a54805ded73", "Unable to edit Sailing Schedule because it was not created. Use \"Create Sailing\" or \"Select Sailing\" buttons."); }
		}

		#endregion

		#region Implementation

		protected ZForm ParentZForm
		{
			get { return (ZForm)this.FindForm(); }
		}

		#region ImportExportPanel

		void SetImportExportPanel(ZPanel value)
		{
			if (importExportPanel != value)
			{
				SuspendLayout();
				try
				{
					if (importExportPanel != null)
					{
						importExportPanel.Visible = false;
					}

					importExportPanel = value;

					if (importExportPanel != null)
					{
						importExportPanel.Visible = true;
					}
				}
				finally
				{
					ResumeLayout();
				}
			}
		}
		ZPanel importExportPanel;

		#endregion

		#endregion
	}
}

#region Test
#if DEBUG

#region TestMethods

namespace Enterprise.Freight.Agency.GUI
{
	partial class SailingUserControl
	{
		public void PerformClickSelectSailingForTest()
		{
			selectSailingButton.PerformClick();
		}

		public void PerformClickEditSailingForTest()
		{
			editSailingButton.PerformClick();
		}

		public void PerformClickCreateSailingForTest()
		{
			createSailingButton.PerformClick();
		}

		public void PerformClickClearSailingForTest()
		{
			clearSailingButton.PerformClick();
		}

		public ZController LastCreatedControllerForTesting;

		public List<Control> AllButtonsExposedForTesting
		{
			get
			{
				return new List<Control>
					{
						createSailingButton,
						selectSailingButton,
						editSailingButton,
						clearSailingButton
					};
			}
		}
	}
}

#endregion


#endif
#endregion
