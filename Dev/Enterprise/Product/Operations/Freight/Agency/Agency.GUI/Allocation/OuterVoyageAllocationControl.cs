using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class OuterVoyageAllocationControl : ZUserControl
	{
		public OuterVoyageAllocationControl()
		{
			InitializeComponent();
		}

		#region Form Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				this.allocationByPrincipalHelpMessageLabel.Text = Res.GetString("OuterVoyageAllocationControl|AllocationByPrincipalHelpMessage", "If you have more than one principal for this schedule and wish to separate your allocations per principal, tick the \"By Principal\" check box and add the principals to the grid. If you only have one principal for this schedule or you want all the principals to use the same allocations then leave the \"By Principal\" check box unticked and the grid empty.");
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			try
			{
#if !WINZOR
				SafeNativeMethods.LockWindowUpdate(ParentForm.Handle);
#endif

				selectedIndex = principalTabControl.SelectedIndex;

				AgencyCountry country = (AgencyCountry)CurrentDataItem;

				if (country != null)
				{
					country.VoyageCountry.J0_AllocationsByPrincipalInfo.ValueChanged -= new EventHandler(J0_AllocationsByPrincipalInfo_ValueChanged);

					principalTabControl.DataCollection = null;
					principalTabControl.TabPages.Remove(genericTab);
					genericTab.Dispose();
					genericTab = null;
				}

				base.OnCurrentDataItemChanging(e);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// If we throw an exception then unlock now, otherwise it can wait for OnCurrentDataItemChanged.
#if !WINZOR
				SafeNativeMethods.UnlockWindowUpdate(ParentForm.Handle);
#endif
				throw;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			try
			{
				base.OnCurrentDataItemChanged(e);

				AgencyCountry country = (AgencyCountry)CurrentDataItem;

				if (country != null)
				{
					InnerVoyageAllocationControl control = new InnerVoyageAllocationControl();
					control.Dock = DockStyle.Fill;

					genericTab = new BindingTab(Schedule.GenericPrincipal);
					genericTab.Text = Res.GetString("OuterVoyageAllocationControl|GenericTab", "Details");
					genericTab.Controls.Add(control);
					genericTab.TabVisible = !country.VoyageCountry.J0_AllocationsByPrincipal;

					principalTabControl.TabPages.Add(genericTab);
					principalTabControl.DataCollection = country.WrappedPrincipals;

					country.RefreshAllUsageData();
					country.VoyageCountry.J0_AllocationsByPrincipalInfo.ValueChanged += new EventHandler(J0_AllocationsByPrincipalInfo_ValueChanged);
				}

				AllocationsByPrincipalChanged();

				principalTabControl.SelectedIndex = selectedIndex;
			}
			finally
			{
#if !WINZOR
				SafeNativeMethods.UnlockWindowUpdate(ParentForm.Handle);
#endif
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && Schedule != null)
				{
					Schedule.VoyageCountry.J0_AllocationsByPrincipalInfo.ValueChanged -= new EventHandler(J0_AllocationsByPrincipalInfo_ValueChanged);
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion

		#region Implementation

		void AllocationsByPrincipalChanged()
		{
			if (Schedule == null)
			{
				principalsBoundModuleButtonGrid.ShowAttachButton = false;
				principalsBoundModuleButtonGrid.ShowDetachButton = false;
			}
			else if (Schedule.VoyageCountry.J0_AllocationsByPrincipal)
			{
				principalsBoundModuleButtonGrid.ShowAttachButton = true;
				principalsBoundModuleButtonGrid.ShowDetachButton = true;
				genericTab.TabVisible = false;
			}
			else
			{
				principalsBoundModuleButtonGrid.ShowAttachButton = false;
				principalsBoundModuleButtonGrid.ShowDetachButton = false;
				genericTab.TabVisible = true;
			}
		}

		bool QueryUserRemoveAllPrincipals()
		{
			return Globals.Message.Show(Res.GetString("cc2900f1-55b1-47e1-9352-98f87bcb36fa", "Are you sure you want to remove all Principals with their allocations?"),
				Res.GetString("cacb653d-990a-4759-92d7-578aabbe34f6", "Removing Principals"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
		}

		void J0_AllocationsByPrincipalInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Schedule != null && !Schedule.VoyageCountry.J0_AllocationsByPrincipal && Schedule.Principals.Count > 0)
			{
				if (QueryUserRemoveAllPrincipals())
				{
					Schedule.Principals.RemoveAll();
				}
				else
				{
					Schedule.VoyageCountry.J0_AllocationsByPrincipal = true;
					return;
				}
			}

			AllocationsByPrincipalChanged();
		}

		AgencyCountry Schedule
		{
			get { return (AgencyCountry)CurrentDataItem; }
		}

		int selectedIndex;

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Members

namespace Enterprise.Freight.Agency.GUI
{
	partial class OuterVoyageAllocationControl
	{
		public class TestHelper
		{
			public readonly OuterVoyageAllocationControl Control;

			public TestHelper(OuterVoyageAllocationControl control)
			{
				this.Control = control;
			}

			public BindingTabControl TabControl
			{
				get { return Control.principalTabControl; }
			}

			public ZTabPage GenericTab
			{
				get { return Control.genericTab; }
			}
		}
	}
}

#endregion


#endif
#endregion
