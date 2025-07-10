using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ShipmentGatePassForm : ZForm, ISaveAndPrintUI
	{
		public ShipmentGatePassForm(GatePassShipment shipment)
			: base(shipment)
		{
			InitializeComponent();

			Shipment = shipment;

			if (shipment.JS_IsForwardRegistered)
			{
				WorkflowTabPage.TabVisible = false;
			}
			else
			{
				WorkflowTabPage.Initialize(shipment);
			}

			if ((bool)ObjectFactory.Get<Enterprise.Integration.Customs.CA.ICACustomsDataRegistry>().RNSActive.Value)
			{
				PlugIns.Add(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn);
			}

			PlugIns.AddJobInvoicing(shipment.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia && shipment.IsSea && !shipment.IsExport())
			{
				PlugIns.Add(ControllerIDs.Customs.AU.SeaCargoDepot);
			}

			HookEvents();

			SetupTabPages();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			SetupContextMenus();

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };

			GatePassDetailsUserControl.GatePassShipment = shipment;

			ShipmentDocumentSupporterGuiQueryProvider.Register(shipment.Factory);
			ServicesSelectionGuiProvider.Register(shipment.Factory);
		}

		readonly GatePassShipment Shipment;

		#region Status Provider

		IStatusClassProvider StatusClassProvider
		{
			get { return StatusClassProviderCore(); }
		}

#if DEBUG
		protected virtual
#endif
 IStatusClassProvider StatusClassProviderCore()
		{
			return Shipment;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookEvents();
				UnHookEventsForGatePassDetailsUserControl();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Events

		void HookEvents()
		{
			if (Shipment != null)
			{
				Shipment.DocsAndCartage.IsContingencyReleaseChecked += JobDocsAndCartage_IsContingencyReleaseChecked;
				Shipment.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				Shipment.JS_IsForwardRegisteredInfo.ValueChanged += JS_IsForwardRegisteredInfo_ValueChanged;
			}
		}

		void JS_IsForwardRegisteredInfo_ValueChanged(object sender, EventArgs e)
		{
			WorkflowTabPage.TabVisible = !Shipment.JS_IsForwardRegistered;
			if (!Shipment.JS_IsForwardRegistered && !((IWorkflowTabPage)WorkflowTabPage).Initialized)
			{
				WorkflowTabPage.Initialize(Shipment);
			}
		}

		void UnHookEvents()
		{
			if (Shipment != null)
			{
				Shipment.DocsAndCartage.IsContingencyReleaseChecked -= JobDocsAndCartage_IsContingencyReleaseChecked;
				Shipment.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				Shipment.JS_IsForwardRegisteredInfo.ValueChanged -= JS_IsForwardRegisteredInfo_ValueChanged;
			}
		}

		#endregion

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
			SetColourFromStatusClass();
		}

		#endregion

		#region Save

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			bool succeeded = SpecialGatePassSaveAndPrint();
			Shipment.CleanUpAfterSaveAndPrint(succeeded);

			if (succeeded)
			{
				if (DialogResult.Yes == ConstantsAndReusablesGUI.AskInGUIAboutChangingJobID(Shipment))
				{
					return base.ShowPreSaveDialogs();
				}
				else
				{
					return ContinueWithSave.No;
				}
			}
			else
			{
				return ContinueWithSave.No;
			}
		}

		#endregion
		
		#region Form Setup

		public override string FormCaption
		{
			get { return Res.GetString("c3bce34c-451c-49ef-94a8-ef6ea2932825", "Gate Pass {0}", Shipment.JS_UniqueConsignRef); }
		}

		public override bool IsResizableByTabPageAllowed => true;

		public GatePassDetails GatePassDetailsUserControl
		{
			get { return gatePassDetailsUserControl; }
			set
			{
				if (gatePassDetailsUserControl != value)
				{
					UnHookEventsForGatePassDetailsUserControl();
					gatePassDetailsUserControl = value;
					HookEventsForGatePassDetailsUserControl();
				}
			}
		}
		GatePassDetails gatePassDetailsUserControl;

		protected void SetupTabPages()
		{
			GatePassDetailsUserControl = new GatePassDetails();
			GatePassDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			GatePassTabPage.Controls.Add(GatePassDetailsUserControl);
		}

		void HookEventsForGatePassDetailsUserControl()
		{
			if (gatePassDetailsUserControl != null)
			{
				gatePassDetailsUserControl.DeliverButton.Click += new EventHandler(DeliverButton_Click);
				SetupColorChangingHooks();
			}
		}

		void UnHookEventsForGatePassDetailsUserControl()
		{
			if (gatePassDetailsUserControl != null)
			{
				gatePassDetailsUserControl.DeliverButton.Click -= new EventHandler(DeliverButton_Click);
				gatePassDetailsUserControl.JS_GatePassStatusBoundTextBox.TextChanged += new EventHandler(JS_GatePassStatusBoundTextBox_TextChanged);
				gatePassDetailsUserControl.JS_GatePassStatusBoundTextBox.BackColorChanged += new EventHandler(JS_GatePassStatusBoundTextBox_TextChanged);
			}
		}

		protected bool SpecialGatePassSaveAndPrint()
		{
			Task = null;
			bool result = ((IHasStatusProvider)Shipment).StatusProvider.CanSaveAndPrint(this);

			if (result)
			{
				PostingButtonsUserControl.CloseButton.Enabled = false;

				if (Shipment.JS_PrintNewDeliveriesOnSave)
				{
					if (Shipment.NumberOfNewDeliveries() == 0)
					{
						AskAboutPrintingWhenThereAreNoNewDeliveries();
					}
					else
					{
						result = TryToSetupForPrintAndRunPrintJob();
					}
				}

				if (!Shipment.JS_PrintNewDeliveriesOnSave)
				{
					result = true;
				}

				PostingButtonsUserControl.CloseButton.Enabled = true;
			}

			return result;
		}

		protected void SetupContextMenus()
		{
			MenuItem cancelDeliveryItem = new ZMenuItem(ResString.GetMultilingualString("819d9448-bee7-4260-9bef-9600d1f6293f", "Cancel Delivery"));
			cancelDeliveryItem.Click += new EventHandler(CancelDelivery_Click);
			GatePassDetailsUserControl.DeliveriesGrid.ContextMenu.MenuItems.Add(0, cancelDeliveryItem);
		}

		void CancelDelivery_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedDeliveries = GatePassDetailsUserControl.DeliveriesGrid.SelectedElements;
			if (selectedDeliveries.Length == 0)
			{
				Globals.Message.Show(Res.GetString("472f3967-011c-4bac-b1ca-d9c2e4cf73c3", "Please select deliveries to cancel."));
			}
			else
			{
				DialogResult result = Globals.Message.Show(Res.GetString("6dbc837a-4c5b-4172-8421-c47b538cc498", "Permanently undo these deliveries?\r\nYou will have to save before adding new deliveries."), Res.GetString("94be0d0c-b910-47e4-a525-436d7e118794", "Please confirm"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					foreach (CommonPickupDeliveryConfirm selectedDelivery in selectedDeliveries)
					{
						selectedDelivery.Delete();
					}
				}
			}
		}

		bool SettingColour;
		void JS_GatePassStatusBoundTextBox_TextChanged(object sender, EventArgs e)
		{
			SetColourFromStatusClass();
		}

		void SetColourFromStatusClass()
		{
			if (!SettingColour && StatusClassProvider != null)
			{
				SettingColour = true;

				switch (StatusClassProvider.StatusClass)
				{
					case StatusClass.Clear:
						SetGatePassStatusTextBoxColour(Color.White, Color.Green);
						break;
					case StatusClass.Held:
						SetGatePassStatusTextBoxColour(Color.Yellow, Color.Red);
						break;
					case StatusClass.Underbonded:
						SetGatePassStatusTextBoxColour(Color.Wheat, Color.CornflowerBlue);
						break;
					case StatusClass.Warning:
						SetGatePassStatusTextBoxColour(Color.Red, Color.Yellow);
						break;
					default:
						SetGatePassStatusTextBoxColour(SystemColors.WindowText, SystemColors.Control);
						break;
				}

				SettingColour = false;
			}
		}

		void SetGatePassStatusTextBoxColour(Color foregroundColour, Color backgroundColour)
		{
			GatePassDetailsUserControl.JS_GatePassStatusBoundTextBox.ForeColor = foregroundColour;
			GatePassDetailsUserControl.JS_GatePassStatusBoundTextBox.BackColor = backgroundColour;
		}

		void SetupColorChangingHooks()
		{
			GatePassDetailsUserControl.JS_GatePassStatusBoundTextBox.TextChanged += new EventHandler(JS_GatePassStatusBoundTextBox_TextChanged);
			GatePassDetailsUserControl.JS_GatePassStatusBoundTextBox.BackColorChanged += new EventHandler(JS_GatePassStatusBoundTextBox_TextChanged);
		}

		void DeliverButton_Click(object sender, EventArgs e)
		{
			if (Shipment.OuterPackLines.Count == 0)
			{
				Globals.Message.Show(Res.GetString("55220cd1-c554-4e4d-a506-8a62416dfcdf", "Can't deliver a shipment without any packlines"), Res.GetString("1658a9b6-e442-4744-97c9-68b54537d846", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (Shipment.JS_Calc_TotalInStock == 0)
			{
				Globals.Message.Show(Res.GetString("fc34bc8e-6d05-4c87-8667-6f5371b2c39a", "Nothing left to deliver"), Res.GetString("84f2d86c-f6ce-4325-8d80-51c6bfa040c6", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (!ConfirmTimesSyncHelper.IsValidShipmentForAddingConfirmations(Shipment))
			{
				Globals.Message.Show(Res.GetString("ad18d8d8-6b18-45da-a9ce-01fcbf6e3807", "Co-Load Master Shipments have no packlines – Gate Pass must be done from the linked sub Shipments"), Res.GetString("b2ccaa58-c58f-455b-818b-16cc7303043b", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				if (OkToDeliver &&
					(!Shipment.DocsAndCartage.Services.AreAnyServicesIncomplete ||
					Globals.Message.Show(Res.GetString("83da8704-b836-4793-b262-89c260903924", "There are still incomplete services on this shipment.\r\n Do you still want to deliver the shipment?"),
					Res.GetString("851f9e46-b95a-415f-a83d-02a2d4a8e665", "Incomplete Services on Shipment"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes))
				{
					AddNewConfirm();
				}
			}
		}

		bool OkToDeliver
		{
			get
			{
				bool result = false;
				if (IsCustomsControlled(Shipment) && !Shipment.JS_GatePassStatus.StartsWith("CLEAR", StringComparison.Ordinal))
				{
					if (Shipment.DocsAndCartage.JP_IsContingencyRelease)
					{
						if (Globals.Message.ShowConfirmation(Res.GetString("a6c976c4-b9b6-4774-a179-aad9164ae2e5", "This cargo has not been cleared by customs.  Are you sure you want to release it?"), Res.GetString("69b2fcc9-50ef-4a0b-b282-2132c299b4bd", "Please Confirm"), Res.GetString("aaff2e7c-56cb-4d6c-b0ee-a31762c5a1f8", "Do you wish to continue?"), "YES", MessageBoxIcon.Hand) == DialogResult.OK)
						{
							Shipment.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.SeaCargoDepotEvent, "Contingency Release");
							AddNewConfirm();
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("9e0dd677-bdbe-4a45-b8ce-5a8ab3d6f11f", "This cargo has not been cleared and is not ticked as a contingency release"));
					}
				}
				else
				{
					result = true;
				}
				return result;
			}
		}

		void AddNewConfirm()
		{
			CommonPickupDeliveryConfirm confirm;

			if (Shipment.IsExport())
			{
				confirm = Shipment.DestinationCFSDepartures.AddNew();
			}
			else
			{
				confirm = Shipment.DestinationCFSDepartures.AddNew();
			}

			confirm.EU_PickupDeliveryTime = ZDateTime.Now;
		}

		bool IsCustomsControlled(CFSShipment shipment)
		{
			ZQuery customsMessageFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK);
			if (shipment.ArrivalConsol != null)
			{
				customsMessageFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_LinkUniqueID, SQLComparisonOperator.Equal, shipment.ArrivalConsol.PK);
			}
			if (shipment.ArrivalContainers != null)
			{
				customsMessageFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_LinkUniqueID, shipment.ArrivalContainers.GetPKs());
			}
			customsMessageFilter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "SCA");

			EDIMessage customsMessage = shipment.Factory.LoadTop1<EDIMessage>(customsMessageFilter);
			return customsMessage != null;
		}

		void JobDocsAndCartage_IsContingencyReleaseChecked(object sender, EventArgs e)
		{
			string message = Res.GetString("3c2b9879-9e0b-4515-948f-7768caf836fc", "In manually changing the Sea Cargo Status you must have authority to do so from Customs and your company's management. Consider the reasons and consequence of overriding the status or release of customs controlled cargo they are important and have consequences. Changing the status manually as incorrect or inappropriate changes to customs status could result in fines and or other civil or criminal penalties.");
			string caption = Res.GetString("784926a7-c606-41eb-8977-b83b2cc69308", "Customs Contingency Release");

			Globals.Message.ShowWarning(message, caption);
		}

		protected override bool AllowNew => false;

		#endregion

		#region Document Printing

		#region SuppressResourceStringsCheckRegion

		public static class DocumentNames
		{
			public const string GatePass = "Gate Pass";
			public const string GatePassSingapore = "Gate Pass Singapore";
		}

		#endregion

		protected DocumentPrintSet Task { get; set; }

		protected void AskAboutPrintingWhenThereAreNoNewDeliveries()
		{
			ZString message = Res.GetString("177a96ac-eb4c-4c47-a5a9-6d8f934c9dd2", "The {0} option is ticked but there are no new deliveries to print.\r\nIf you continue saving, no delivery details will be printed.\r\nDo you wish to continue?", GatePassDetailsUserControl.PrintOnSaveCheckBox.Text);
			ZString caption = Res.GetString("529d00b5-1c95-4dc1-a928-671e0ab5423e", "No new deliveries to print");

			DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				Shipment.JS_PrintNewDeliveriesOnSave = false;
			}
		}

		protected bool TryToSetupForPrintAndRunPrintJob()
		{
			Task = PreparePrintTask(Shipment.Factory,
				GlbBranch.CurrentBranch.Country.Code != Core.Constants.CountryCodes.Singapore ? DocumentNames.GatePass : DocumentNames.GatePassSingapore,
				GlbBranch.CurrentBranch.Country.Code != Core.Constants.CountryCodes.Singapore ? CFSDataRegistry.Instance.GatepassCopiesToPrint.Value : 3);

			if (Task == null)
			{
				Globals.Message.Show(Res.GetString("c1940285-f1fa-44b1-ae0d-0122e2f35034", "Document not saved - please print or uncheck the Print On Save check box."));
			}

			return Task != null;
		}

		protected DocumentPrintSet PreparePrintTask(BusinessObjectFactory factory, ZString documentName, ZInt numberOfCopies)
		{
			var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, documentName);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, Shipment.DocumentSupporter.BusinessContext);
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, "");

			var documentCommand = factory.LoadTop1<DocumentCommand>(query);

			DocumentPrintSet printSet = null;

			if (documentCommand != null)
			{
				documentCommand.Parent = Shipment;

				printSet = new DocumentPrintSet(documentCommand, new UserControlProviderList());

				if (printSet.Count > 0 && numberOfCopies > 1)
				{
					CreateAdditionalCopies(printSet[0], numberOfCopies - 1);
				}
			}

			return printSet != null && printSet.Count > 0 ? printSet : null;
		}

		void CreateAdditionalCopies(DocumentPack pack, int numberOfCopies)
		{
			var copies = new List<Report>();

			for (int i = 0; i < numberOfCopies; i++)
			{
				copies.AddRange(
					pack
						.OfType<Report>()
						.Select((report) => new Report(pack, report.Template, report.DataProviderList, report.Name, null, report.Direction, false))
				);
			}

			pack.AddRange(copies);
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (Task != null && savedSuccessfully)
			{
				Task.Run(AllowedDeliveryOptions.All, Env.Security.None);
			}

			Task = null;
		}

		#endregion

		#region ISaveAndPrintUI Members

		bool ISaveAndPrintUI.Ask(string question)
		{
			return Globals.Message.Show(question, Res.GetString("07010869-3c81-41e7-8ba4-b23376121a33", "Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Hand, DialogResult.No) == DialogResult.Yes;
		}

		void ISaveAndPrintUI.ShowError(string message)
		{
			Globals.Message.ShowError(message);
		}

		void ISaveAndPrintUI.ShowWarning(string message)
		{
			Globals.Message.ShowWarning(message);
		}

		#endregion
	}
}
