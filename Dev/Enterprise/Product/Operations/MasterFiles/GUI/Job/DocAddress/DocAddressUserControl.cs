using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Internal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DocAddressUserControl : ZUserControl
	{
		#region Auto

		IBusiness HostBusinessEntity { get; }

		#endregion

		public DocAddressUserControl() : this(null)
		{
		}

		public DocAddressUserControl(IBusiness hostBusinessEntity)
		{
			InitializeComponent();
			var addressOverrideSupporter = hostBusinessEntity as IJobDocAddressOverrideSupporter;
			if (addressOverrideSupporter != null)
			{
				var docAddresstype = addressOverrideSupporter.ZDocAddressControlType;
				if (docAddresstype != null)
				{
					OverrideAddressControl(docAddresstype);
				}
			}

			AddressGrid.AddressAdded += new DocAddressGrid.AddressAddedEventHandler(AddressGrid_AddressAdded);
			AddressControl.OrgChanged += new EventHandler(AddressControl_OrgChanged);
			AddressGrid.OnRemovingBizOFromList = AddressGridRemoveBizOFromList;
			AddressGrid.ContextMenu.Popup += AddressGridContextMenu_Popup;

#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(AddressControl);
			TypeDescriptor.AddAttributes(zLabel21, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel20, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel19, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel18, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel17, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel16, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel15, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel14, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel13, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel12, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zLabel1, new SuppressFormsLocalizedTestAttribute());
#endif
			SetUpContextMenu();

			if (hostBusinessEntity != null)
			{
				if ((hostBusinessEntity is IScreeningPartyProvider && !(hostBusinessEntity is Enterprise.Integration.Customs.IBaseJobDeclaration))
					|| hostBusinessEntity is Enterprise.Integration.Customs.US.IJobDeclaration)
				{ }
				else
				{
					this.TabControl.Controls.Remove(ScreeningLogsTabPage);
				}
			}

			HostBusinessEntity = hostBusinessEntity;

			groupBox1.AllowOutsideOfParent();
			AddressControl.AllowOutsideOfParent();
			AddressesGroupBox.AllowOutsideOfParent();
			zLabel1.AllowOverlap(groupBox1);
			zLabel16.AllowOverlap(zLabel15);
			zLabel17.AllowOverlap(zLabel16);
			zLabel17.AllowOverlap(zLabel9);
			zLabel17.AllowOverlap(groupBox1);

			zLabel2.AllowOutsideOfParent();
			zLabel9.AllowOutsideOfParent();
			zLabel10.AllowOutsideOfParent();
			zLabel11.AllowOutsideOfParent();
			zLabel18.AllowOutsideOfParent();
			zLabel19.AllowOutsideOfParent();
			zLabel20.AllowOutsideOfParent();
			zLabel21.AllowOutsideOfParent();
		}

		void OverrideAddressControl(Type docAddresstype)
		{
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			this.DetailsTabPage.Controls.Remove(this.AddressControl);
			AddressControl.Dispose();
			AddressControl = (ZDocAddressControl)Activator.CreateInstance(docAddresstype);
			this.DetailsTabPage.Controls.Add(this.AddressControl);
			//
			// AddressControl
			//
			this.AddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressControl, ".");
			this.AddressControl.BindToContacts = "Organisation+ContactsActive";
			this.AddressControl.BindToOrganisations = "Lookups+OrgHeader_List";
			this.AddressControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|bd998914-73e6-4b39-9c9c-ef6d0a77505d", "Organization");
			this.AddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6);
			this.AddressControl.Name = "AddressControl";
			this.AddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182);
			this.AddressControl.TabIndex = 36;

			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.ResumeLayout(false);
			this.TabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		ZGrid.ContinueWithRemove AddressGridRemoveBizOFromList(BusinessObject bizOToRemoveOrDelete)
		{
			var docAddress = (JobDocAddress)bizOToRemoveOrDelete;
			var parent = docAddress.Parent;
			ZGrid.ContinueWithRemove result = ZGrid.ContinueWithRemove.CancelRemoval;
			if (parent != null && parent.CanDeleteAddress(docAddress))
			{
				result = ZGrid.ContinueWithRemove.Remove;
			}
			if (result == ZGrid.ContinueWithRemove.CancelRemoval)
			{
				IBusinessObjectCollection docAddresses = AddressGrid.List as IBusinessObjectCollection;
				if (docAddresses != null)
				{
					docAddress.E2_AddressOverride = false;
					docAddress.OrganisationPK = ZGuid.Empty;
					docAddresses.RemoveFromRelationship(docAddress);
				}
			}
			return result;
		}

		void SetUpContextMenu()
		{
			MenuItem screeningMenuItem = new ZMenuItem(ResString.GetMultilingualString("DocAddressUserControl|Screen", "Screen"), async delegate
			{
				if (AddressGrid.ListManager.Count <= 0)
				{
					return;
				}

				JobDocAddress selectedAddress = (JobDocAddress)AddressGrid.ListManager.GetCurrent();
				if (CheckHasChanges(selectedAddress))
				{
					return;
				}

				var parties = new[] { new ScreeningParty(GetScreeningParent(selectedAddress), "DocAddress", selectedAddress) };

				await new DeniedPartyScreeningPresentationManager().PerformScreening(ParentForm, DpsSourceWithParties.GetSingleSourceList(selectedAddress, parties), parties, false, true, false);
			}
			);

			MenuItem screeningMenuItem_ForceAllLists = null;
			if (DeniedPartyScreenerAsync.HasExcludedList(HostBusinessEntity?.Factory))
			{
				screeningMenuItem_ForceAllLists = new ZMenuItem(ResString.GetMultilingualString("DocAddressUserControl|Screen_ForceFull", "Screen (Full List)"), async delegate
				{
					if (AddressGrid.ListManager.Count <= 0)
					{
						return;
					}

					var selectedAddress = (JobDocAddress)AddressGrid.ListManager.GetCurrent();
					if (CheckHasChanges(selectedAddress))
					{
						return;
					}

					var parties = new[] { new ScreeningParty(GetScreeningParent(selectedAddress), "DocAddress", selectedAddress) };

					await new DeniedPartyScreeningPresentationManager().PerformScreening(ParentForm, DpsSourceWithParties.GetSingleSourceList(selectedAddress, parties), parties, false, true, true);
				}
				);
			}

			MenuItem dividerMenuItem = new ZMenuItem("-");
			if (AddressGrid.ContextMenu.MenuItems.Count > 0 && AddressGrid.ContextMenu.MenuItems[AddressGrid.ContextMenu.MenuItems.Count - 1].Text != "-")
			{
				AddressGrid.ContextMenu.MenuItems.Add(dividerMenuItem);
			}
			AddressGrid.ContextMenu.MenuItems.Add(screeningMenuItem);

			if (screeningMenuItem_ForceAllLists != null)
			{
				AddressGrid.ContextMenu.MenuItems.Add(screeningMenuItem_ForceAllLists);
			}

			AddressGrid.ContextMenu.MenuItems.Add(dividerMenuItem);

			bool CheckHasChanges(JobDocAddress jobDocAddress)
			{
				if (jobDocAddress.HasChanges || !jobDocAddress.IsInDatabase || ((jobDocAddress.Parent as BusinessObject)?.HasChanges ?? false))
				{
					Globals.Message.ShowError(Res.GetString("7d04aa4e-773d-4a8c-94b6-22dbc86c30ae", "Please save before screening for denied parties."));
					return true;
				}

				return false;
			}
		}

		BusinessObject GetScreeningParent(JobDocAddress jobDocAddress)
		{
			return jobDocAddress.Parent is IScreeningPartyProvider && jobDocAddress.Parent is BusinessObject businessObject ? businessObject : jobDocAddress;
		}

		#region Address change hooks

		void AddressControl_OrgChanged(object sender, EventArgs e)
		{
			AddressGrid.RefreshAddresses();
		}

		void AddressGrid_AddressAdded(DocAddressGrid.AddAddressEventArgs e)
		{
			this.AddressControl.Focus();
		}

		protected void AddressGridContextMenu_Popup(object sender, EventArgs e)
		{
			var jobDocAddress = (JobDocAddress)AddressGrid.List[AddressGrid.CurrentRowIndex];
			if (jobDocAddress != null && jobDocAddress.Parent != null)
			{
				AddressGrid.DeleteMenuItem.Enabled = jobDocAddress.Parent.CanDeleteAddress(jobDocAddress);
				AddressGrid.AllowReadOnlyRowsToBeDeleted = true;
				AddressGrid.DeleteMenuItem.Text = jobDocAddress.E2_AddressOverride ?
					Res.GetString("7e24fd76-c42f-4e64-8d67-93ae964ba048", "Delete") :
					Res.GetString("0f9a4a98-8621-440e-9f4c-b3b99505fa18", "Remove");
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				AddressGrid.AddressAdded -= new DocAddressGrid.AddressAddedEventHandler(AddressGrid_AddressAdded);
				AddressControl.OrgChanged -= new EventHandler(AddressControl_OrgChanged);
				AddressGrid.OnRemovingBizOFromList = null;
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
