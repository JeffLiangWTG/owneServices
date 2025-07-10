using System;
using CargoWise.Windows.UI;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVConsignmentForm : ZTemplateForm
	{
		public HVLVConsignmentForm(HVLVConsignment consignment)
			: base(consignment)
		{
			InitializeComponent();
			workflowTabPage.Initialize(consignment);
			AddPlugins();
			if (!DesignModeFinder.IsDesigning)
			{
				MainTabPage.RunWhenBindingOrFirstShown((_, __) =>
				{
					AddAdditionalBindings();
				});
			}

			AddActionMenuItems();
		}

		HVLVConsignment Consignment => BusinessEntity as HVLVConsignment;

		public override string FormCaption => Consignment.HVC_ConsignmentId.IsEmpty ?
			ResString.GetMultilingualString("1dadfa85-86d4-4abe-ae90-b3360a030eb3", "Consignment") :
			ResString.GetMultilingualString("42976675-19c2-44c1-ad09-9e3be49eb915", "Consignment {0}", Consignment.HVC_ConsignmentId);

		void AddAdditionalBindings()
		{
			const string IsVisibleForBindingString = "IsVisibleForBinding";

			vesselTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			vesselTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(HVLVConsignment.TransportModeIsSea), false, System.Windows.Forms.DataSourceUpdateMode.Never));
		}

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		void AddActionMenuItems()
		{
			var calculateChargeableMenuItem = HVLVMenuItemHelper.CalculateChargeableMenuItem(CalculateChargeable);
			ActionsMenuItem.MenuItems.Add(calculateChargeableMenuItem);

			var calculateLMCDepotDetailsMenuItem = HVLVMenuItemHelper.CalculateLMCDepotDetailsMenuItem(CalculateLMCDepotDetails);
			ActionsMenuItem.MenuItems.Add(calculateLMCDepotDetailsMenuItem);

			convertToStandAloneDeclarationMenuItem = HVLVMenuItemHelper.ConvertToStandAloneDeclarationMenuItem(ConvertToStandAloneDeclaration);
			ActionsMenuItem.MenuItems.Add(convertToStandAloneDeclarationMenuItem);

			lastMileCarrierBookingMenuItem = new LastMileCarrierBookingMenuItem(() => Consignment);
			ActionsMenuItem.MenuItems.Add(lastMileCarrierBookingMenuItem);

			cancelLastMileCarrierBookingMenuItem = new CancelLastMileCarrierBookingMenuItem(() => Consignment);
			ActionsMenuItem.MenuItems.Add(cancelLastMileCarrierBookingMenuItem);

			var transportBookingMenuItem = HVLVMenuItemHelper.TransportBooking(() => Consignment);
			ActionsMenuItem.MenuItems.Add(transportBookingMenuItem);

			ActionsMenuItem.Popup += ActionsMenuItem_Popup;
		}

		ZMenuItem convertToStandAloneDeclarationMenuItem;
		LastMileCarrierBookingMenuItem lastMileCarrierBookingMenuItem;
		CancelLastMileCarrierBookingMenuItem cancelLastMileCarrierBookingMenuItem;

		void ActionsMenuItem_Popup(object sender, EventArgs args)
		{
			UpdateConvertToStandAloneDeclarationMenuItemVisible();
			lastMileCarrierBookingMenuItem.UpdateVisibilityAndCaption();
			cancelLastMileCarrierBookingMenuItem.UpdateVisibilityAndCaption();
		}

		void CalculateChargeable(object sender, EventArgs e)
		{
			Consignment.CalculateChargeable();
		}

		void ConvertToStandAloneDeclaration(object sender, EventArgs args)
		{
			ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclaration(Consignment);
		}

		void CalculateLMCDepotDetails(object sender, EventArgs e)
		{
			LMCDepotDetailsCalculator.UpdateConsignmentsDestinationDetails(new[] { Consignment.PK });
			Consignment.Reload();
			Consignment.Factory.ForcePublishForDataRefresh(Consignment);
			Consignment.RefreshBindingForLMCProperties();
		}

		HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator LMCDepotDetailsCalculator => lmcDepotDetailsCalculator ?? (lmcDepotDetailsCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator());
		HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator lmcDepotDetailsCalculator;

		void UpdateConvertToStandAloneDeclarationMenuItemVisible()
		{
			convertToStandAloneDeclarationMenuItem.Visible = ConvertToStandAloneDeclarationHelper.ShouldShowConvertToStandAloneDeclarationMenuItem(Consignment);
		}

		protected override bool AllowNew => false;

		protected override bool ShowAuditTab => true;
	}
}
