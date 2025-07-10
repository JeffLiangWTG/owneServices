using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVBookingHeaderForm : ZTemplateForm
	{
		public HVLVBookingHeaderForm(HVLVBookingHeader bookingHeader)
			: base(bookingHeader)
		{
			InitializeComponent();
			Customs.Business.JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(bookingHeader.Factory, bookingHeader.PK);
			PlugIns.Add(ControllerIDs.DtbBooking);
			AddActionMenuItems();
			workflowTabPage.Initialize(bookingHeader);
			BookingHeader.BillToPartyChanged += BillToPartyChanged;
			if (bookingHeader.HVH_IsProcessedAtOriginDepot)
			{
				bookingHeader.SetReadOnlyIncludingChildren(true);
			}
		}

		void AddActionMenuItems()
		{
			ActionsMenuItem.MenuItems.Add(HVLVMenuItemHelper.CalculateLMCDepotDetailsMenuItem(CalculateLMCDepotDetails));
			ActionsMenuItem.MenuItems.Add(HVLVMenuItemHelper.NavigateToEcommerceWebPortalsMenuItem());
			ActionsMenuItem.MenuItems.Add(new PreScreenMenuItem(BookingHeader));
			ActionsMenuItem.MenuItems.Add(HVLVMenuItemHelper.CreateTestConsignmentsMenuItem(CreateTestConsignments));
			ActionsMenuItem.MenuItems.Add(HVLVMenuItemHelper.CreateTestLoadlistMenuItem(CreateTestLoadlist));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
			AddDeniedPartyScreeningMenuItems();
		}

		void AddDeniedPartyScreeningMenuItems()
		{
			new DeniedPartyScreeningPresentationManager().CreateMenusForJob(this, GetScreeningNotEnabledMessage);
			new DeniedPartyScreeningActionsProvider(this, BookingHeader).AddJobsMenuItem();
		}

		void CreateTestLoadlist(object sender, EventArgs e)
		{
			var saveBeforeCreatingTestConsignmentsMessage = Res.GetString("0463b755-91cf-4b82-a5ad-c7f9d5f393b5", "Please save the form before creating test load list");
			if (UserPromptCheckingHelper.CheckBusinessObjectHasNoChangesOrNotify(BookingHeader, saveBeforeCreatingTestConsignmentsMessage))
			{
				var creatorForm = new HVLVBookingHeaderCreateTestLoadListForm(BookingHeader);
				var dialogResult = ZFormModaliser.ShowDialogAndDispose(creatorForm);
				if (dialogResult == DialogResult.OK)
				{
					if (creatorForm.CreatedLoadList != null)
					{
						ZFormModaliser.Show(new HVLVOriginLoadListForm(creatorForm.CreatedLoadList), this);
					}
				}
			}
		}

		void CreateTestConsignments(object sender, EventArgs e)
		{
			var saveBeforeCreatingTestConsignmentsMessage = Res.GetString("fe41f3bb-b9cd-411f-a533-f8aa0219c7bd", "Please save the form before creating test consignments");
			if (UserPromptCheckingHelper.CheckBusinessObjectHasNoChangesOrNotify(BookingHeader, saveBeforeCreatingTestConsignmentsMessage))
			{
				var creatorForm = new HVLVBookingHeaderCreateTestConsignmentForm(BookingHeader.HVH_BookingReference, BookingHeader.Consignments.Any());
				var dialogResult = ZFormModaliser.ShowDialogAndDispose(creatorForm);
				if (dialogResult == DialogResult.OK)
				{
					ReloadForm();
				}
			}
		}

		async void DeniedPartyingScreeningButton_Click(object sender, EventArgs e)
		{
			var provider = BookingHeader as IScreeningPartyProvider;
			if (provider != null)
			{
				await new DeniedPartyScreeningPresentationManager().PerformScreening(this, false, !DeniedPartyScreenerAsync.HasExcludedList(BookingHeader.Factory), BookingHeader, () => BookingHeader.HasChanges, GetScreeningNotEnabledMessage);
			}
		}

		Func<string> GetScreeningNotEnabledMessage => () =>
		{
			var message = string.Empty;
			if (!HVLVDataRegistry.Instance.HVLVEnablePartyScreening.Value.EnableHVLVPartyScreening)
			{
				message = Res.GetString("7f9353b7-c6dc-4efd-841a-e8f28630f0e2", @"HVLV Party Screening has not been enabled.
To enable go to Registry > Master Data > Organizations > Denied Party Screening > Enable HVLV Party Screening.");
			}

			return message;
		};

		void CalculateLMCDepotDetails(object sender, EventArgs e)
		{
			LMCDepotDetailsCalculator.UpdateConsignmentsDestinationDetails(BookingHeader.Consignments.GetPKs());
			BookingHeader.Consignments.Reload(true);
			BookingHeader.Consignments.OfType<HVLVConsignment>().ForEach(consignment => consignment.RefreshBindingForLMCProperties());
		}

		HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator LMCDepotDetailsCalculator => lmcDepotDetailsCalculator ?? (lmcDepotDetailsCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator());
		HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator lmcDepotDetailsCalculator;

		public override string FormCaption => BookingHeader.HumanReadableName;

		void BillToPartyChanged(object sender, EventArgs e)
		{
			var consignments = BookingHeader.Consignments.OfType<HVLVConsignment>().Where(x => x.HVC_PreScreeningStatus != HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown);
			if (ETailPreScreeningProvider.IsPreScreeningHVLVDetailsEnabled && consignments.Any())
			{
				var messageContent = Res.GetString("b7a29b12-f3c2-4e26-a79e-b4017c322510", "HVLV Pre-Screening is enabled, changing the eTailer will set the Pre-Screening Status on all HVLV Consignments to Unknown.");
				var caption = Res.GetString("327f745d-2127-40cd-ad7d-bf4078877f08", "eTailer");

				Globals.Message.Show(messageContent, caption, MessageBoxButtons.OK, DialogResult.OK);

				consignments.ForEach(x => x.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown);
			}
		}

		HVLVBookingHeader BookingHeader => (HVLVBookingHeader)BusinessEntity;

		protected override bool ShowAuditTab => true;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (BookingHeader != null)
			{
				Customs.Business.JobComInvoiceLinePartSynchronisationManager.StopManagingWhenActiveDeciderPKWasDisposed(BookingHeader.Factory, BookingHeader.PK);
			}
			base.Dispose(disposing);
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			if (DisplayMode == ODisplayMode.Delete && BookingHeader.HVH_IsActive && BookingHeader.IsCancelledHasChanged)
			{
				var message = Res.GetString("b406f6d2-dc74-4d97-847b-df8f75c24578", "This header has inactive consignments attached to it do you want to reactivate them?");
				var caption = Res.GetString("618e7793-814e-427b-bd78-168dc01717ce", "Activate");
				var promptResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (promptResult == DialogResult.Yes)
				{
					BookingHeader.ReactivateAllInactiveConsignments();
				}
			}
			base.Save(factories);
		}
	}
}
