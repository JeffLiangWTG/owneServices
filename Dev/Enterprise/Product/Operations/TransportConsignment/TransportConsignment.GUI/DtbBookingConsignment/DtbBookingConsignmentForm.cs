using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.GUI;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.GUI
{
	public partial class DtbBookingConsignmentForm : ZTemplateForm, INotifications
	{
		public DtbBookingConsignmentForm(DtbBookingConsignment consignment)
			: base(consignment)
		{
			InitializeComponent();
			this.servicesControl.IsContextVisibleInGrid = true;
			ControllerID = ControllerIDs.DtbBookingConsignment;

			AddPlugins();
			WorkflowTabPage.Initialize(consignment);
			AddActionsMenu();

			HookEvents();

			new UNDGDataItemFormManager(PackingGrid).Initialize();
		}

		#region OnVisible -- Update Booking Link

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				OnVisible();
			}
		}

		void OnVisible()
		{
			if (Consignment.ConsolidationSingleJob.Parent == null)
			{
				HeaderGroupBox.Text = Res.GetString("16e630ca-3ea6-4bf9-afcc-be3c32486cc5", "Consignment"); // remove the "for"
			}
		}

		#endregion

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				DomesticTransportGridHelper.HookContextMenuShowSignature(ConfirmationsGrid);
				HookSplitConsignmentMenuItemToPackingGrid();
			}
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			ConfirmationsGrid.Leave += delegate
			{
				MainTabControl.Focus();
			};

			DomesticTransportGridHelper.HookDoubleClickToOpenRunSheetFromConfirmation(ConfirmationsGrid);

			DistanceButton.Click += delegate
			{ if (Consignment != null) { Consignment.SetCalculatedDistance(this); } };
		}

		void HookEventsAfterBinding()
		{
			// when changing an org that has a diff type to the previous org, the instruction org type on the confirmation updates
			// once, but will not update on subsequent org changes. refreshing the confirmations grid resolves this.
			if (Consignment != null)
			{
				Consignment.PickupInstruction.OrganisationTypeInfo.ValueChanged += RefreshConfirmationsGrid;
				Consignment.DeliveryInstruction.OrganisationTypeInfo.ValueChanged += RefreshConfirmationsGrid;
			}
		}

		#endregion

		#region UnhookEvents

		void UnhookEvents()
		{
			if (Consignment != null)
			{
				Consignment.PickupInstruction.OrganisationTypeInfo.ValueChanged -= RefreshConfirmationsGrid;
				Consignment.DeliveryInstruction.OrganisationTypeInfo.ValueChanged -= RefreshConfirmationsGrid;
			}
		}

		#endregion

		#region HookSplitConsignmentMenuItemToPackingGrid

		void HookSplitConsignmentMenuItemToPackingGrid()
		{
			var splitConsignmentMenuItem = new ZMenuItem(SplitConsignmentMenuItemCaption, SplitConsignment_Click);

			var contextMenu = PackingGrid.ContextMenu;
			contextMenu.Popup += delegate
			{ UpdateContextMenu(splitConsignmentMenuItem); };

			var menuItems = contextMenu.MenuItems;
			menuItems.Add("-");
			menuItems.Add(splitConsignmentMenuItem);
		}

		void UpdateContextMenu(ZMenuItem showSignatureMenuItem)
		{
			var selectedPackages = PackingGrid.SelectedElements;
			var isPackageSelected = (selectedPackages.Length > 0);

			showSignatureMenuItem.Enabled = isPackageSelected;
			showSignatureMenuItem.Caption = isPackageSelected
				? SplitConsignmentMenuItemCaption
				: ResString.GetMultilingualString("6f72cf3e-adf5-4377-a1fe-9dd4d238ef1e", "No Packages Selected");
		}

		ResourceString SplitConsignmentMenuItemCaption
		{
			get { return ResString.GetMultilingualString("83bb4c17-da48-4bd4-99af-c986cda278d1", "Split into new Consignment"); }
		}

		#endregion

		#endregion

		#region SetDataBinding -- Auto-Set Template

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var consignment = dataSource as DtbBookingConsignment;
			if (consignment != null && !consignment.IsInDatabase && consignment.KM_KT_NKBookingTemplate.IsEmpty)
			{
				consignment.KM_KT_NKBookingTemplate = DtbBookingConsignment.TemplateCode;
			}
			base.SetDataBinding(dataSource, dataMember);

			HookEventsAfterBinding();
		}

		#endregion

		#region Consignment

		DtbBookingConsignment Consignment
		{
			get { return (DtbBookingConsignment)DataSource; }
		}

		#endregion

		#region Plugins

		void AddPlugins()
		{
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)Consignment).InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.PackingPlugIn, 1);
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		#endregion

		#region ActionsMenu

		void AddActionsMenu()
		{
			addressPopup = new AddressPopup();
			EventHandler overrideAddress = delegate
			{ addressPopup.ShowPopupAddress(this, DocAddressType.BookingPartyDocumentaryAddress); };
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(OverrideBookingPartyAddressMenuCaption, overrideAddress));
		}

		AddressPopup addressPopup;

		static ResourceString OverrideBookingPartyAddressMenuCaption { get { return ResString.GetMultilingualString("DtbConsignmentForm|OverrideBookingPartyAddressMenuCaption", "Override Booking Party Address"); } }

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region Actions

		#region Booking Link Label

		void ParentBookingLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var booking = Consignment.ConsolidationSingleJob.Parent;
			if (booking != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
				controller.ShowEditForm(booking);
			}
		}

		#endregion

		#region SplitConsignment

		void SplitConsignment_Click(object sender, EventArgs e)
		{
			SplitConsignment();
		}

		void SplitConsignment()
		{
			var consignment = Consignment;
			if (consignment != null)
			{
				var selectedPackages = PackingGrid.SelectedElements;
				var splitConsignment = consignment.SplitConsignment(Array.ConvertAll(selectedPackages, p => (PkgPackage)p), this);
				if (splitConsignment != null)
				{
					var controller = ZControllerFactory.Create(ControllerIDs.DtbBookingConsignment);
					controller.SetFormsModalTo(this);
					controller.ShowFormForNewEntity(splitConsignment);
				}
			}
		}

		#endregion

		#region RefreshConfirmationsGrid

		void RefreshConfirmationsGrid(object sender, EventArgs e)
		{
			ConfirmationsGrid.Refresh();
		}

		#endregion

		#endregion

		// interfaces

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isDispose)
		{
			if (isDispose)
			{
				UnhookEvents();

				if (addressPopup != null)
				{
					addressPopup.Dispose();
				}
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(isDispose);
		}

		#endregion
	}
}
