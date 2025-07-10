using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public partial class CusUSLVConsignmentForm : ZTemplateForm
	{
		IContainer components;

		public CusUSLVConsignmentForm(CusUSLVConsignment cusUSLVConsignment)
			: base(cusUSLVConsignment)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				MainTabPage.RunWhenBindingOrFirstShown((_, __) =>
				{
					AddAdditionalBindings(cusUSLVConsignment);
				});
			}

			AddMessagingMenuIfNeeded();
			AddActionMenuItems();
		}

		void AddMessagingMenuIfNeeded()
		{
			if (Consignment.ULB_IsActive)
			{
				var messagingMenuItem = new EDIMenuForSingleBill { Consignment = Consignment };
				MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenuItem);
			}
		}

		protected CusUSLVConsignment Consignment => BusinessEntity as CusUSLVConsignment;

		void AddActionMenuItems()
		{
			ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
			openParentJob = new ZMenuItem(Res.GetString("831655CA-5F1B-490F-BF6F-06D0844DB7BC", "Open Low Value Entries parent job"), OpenParentClearanceForm);
			if (IsParentClearanceFormOpen())
			{
				openParentJob.Enabled = false;
			}
			ActionsMenuItem.MenuItems.Add(openParentJob);
		}

		ZMenuItem openParentJob;

		void OpenParentClearanceForm(object sender, EventArgs e)
		{
			if (!IsParentClearanceFormOpen())
			{
				var canOpenParentForm = true;
				if (Consignment.HasChanges)
				{
					if ((Globals.Message.Show(Res.GetString("33a48051-e07d-484a-b415-08dc339952be", "This consignment has not yet been saved. Do you want to save and proceed?"),
						Res.GetString("b8cd1aa8-f4dc-449f-a236-9ccb366bddc3", "Save Consignment"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes))
					{
						canOpenParentForm = FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes;
					}
					else
					{
						canOpenParentForm = false;
					}
				}

				if (canOpenParentForm)
				{
					ZControllerFactory.Create(ControllerIDs.Customs.US.USLowValueEntries).ShowFormOfGivenDisplayType(Consignment.Shipment, this.DisplayMode);
					this.Close();
				}
			}
			else
			{
				openParentJob.Enabled = false;
				Globals.Message.ShowError(Res.GetString("E553BB7C-C867-4CEC-9CC7-28A0BDE89493", "You have already opened parent job."));
			}
		}

		ZBool IsParentClearanceFormOpen()
		{
			ZBool result = false;
			var parentForm = CargoWise.Windows.UI.ZApplication.GetOpenForms().OfType<CusUSLVClearanceForm>().FirstOrDefault(x => ((CusUSLVClearance)x.DataSource)?.PK == Consignment.Shipment.PK);
			if (parentForm != null)
			{
				result = true;
			}
			return result;
		}

		public override string FormCaption => Res.GetString("4ba02be2-8c29-4229-982e-11244a64c5c8", "House Bill {0}", Consignment.ULB_HouseBill);

		protected override void ShowNewForm()
		{
			if (ControllerID == ControllerIDs.Customs.US.USLowValueEntriesBill)
			{
				var controller = ZControllerFactory.Create(ControllerID);
				var clearance = controller.Factory.Load<CusUSLVClearance>(Consignment.Shipment.PK);
				if (clearance != null)
				{
					var consignment = clearance.CusUSLVConsignments.AddNew();
					controller.ShowFormForNewEntity(consignment);
				}
			}
			else
			{
				base.ShowNewForm();
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			groupBoxTransportDetails.SetReadOnlyIncludingChildren(true);
			orgFindBoxMainTabClient.ReadOnly = true;
			orgFindBoxMainTabImporter.ReadOnly = true;
			codeFindBoxCentralizedExamSite.ReadOnly = true;
			codeFindBoxLocationOfGoods.ReadOnly = true;

			AllowOnlyOneUserEditThisForm();
		}

		void AddAdditionalBindings(CusUSLVConsignment consignment)
		{
			const string IsVisibleForBindingString = "IsVisibleForBinding";

			panelTransportDetailGroup.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsNotEmptyForBinding", false, DataSourceUpdateMode.Never));
			panelLocationDate.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsNotEmptyForBinding", false, DataSourceUpdateMode.Never));
			textBoxFlightNo.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsAirForBinding", false, DataSourceUpdateMode.Never));
			masterBillControl.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsAirForBinding", false, DataSourceUpdateMode.Never));
			codeFindBoxVessel.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsSeaForBinding", false, DataSourceUpdateMode.Never));
			textBoxVoyageNo.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsSeaForBinding", false, DataSourceUpdateMode.Never));
			textBoxOceanBill.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsSeaForBinding", false, DataSourceUpdateMode.Never));
			textBoxMailReference.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsMailForBinding", false, DataSourceUpdateMode.Never));
			textBoxJourney.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsRailForBinding", false, DataSourceUpdateMode.Never));
			textBoxMasterBill.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsRailOrRoadForBinding", false, DataSourceUpdateMode.Never));
			textBoxTripID.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsRailOrRoadOrMailForBinding", false, DataSourceUpdateMode.Never));
			dropEditContainerMode.DataBindings.Add(new KBinding(IsVisibleForBindingString, consignment, "Shipment.IsContainerSupported", false, DataSourceUpdateMode.Never));
		}

		void AllowOnlyOneUserEditThisForm()
		{
			mutex = new ZGlobalMutex(MutexIDs.CusUSLVConsignmentForm, GetMutexKey(Consignment.PK));
			if (mutex.IsLocked)
			{
				var lockInfo = mutex.GetLockInfo();
				var who = mutex.GetMutexLockByInfo();
				var when = lockInfo != null ? lockInfo.LockStartTime.ToDateTime().ToLocalTime().ToLongTimeString() : Res.GetString("7e67b047-e0e8-4adc-bbc1-1d5a70f941ac", "*unknown time*");

				string message = Res.GetString("CusUSLVConsignmentForm|UserActionHeader", "The Consignment is currently being edited by user '{0}' since {1}, you cannot edit it before the other user closes the form.{2}{2}", who, when, System.Environment.NewLine);
				Globals.Message.ShowInformation(message, Res.GetString("CusUSLVConsignmentForm|UserActionCaption", "Another session is editing the same information"));

				DisplayMode = ODisplayMode.ReadOnly;
				SetReadOnlyIncludingChildren();
			}
			else
			{
				mutex.Lock();
			}
		}

		ZGlobalMutex mutex;

		static ZString GetMutexKey(ZGuid pk)
		{
			return pk + "_" + GlbCompany.CurrentCompany.GC_Code;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (disposing && (components != null))
				{
					components.Dispose();
				}

				if (mutex != null)
				{
					((IDisposable)mutex).Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
