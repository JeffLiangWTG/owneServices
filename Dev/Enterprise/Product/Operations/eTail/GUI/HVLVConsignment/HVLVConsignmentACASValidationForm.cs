using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVConsignmentACASValidationForm : ZChildForm
	{
		public HVLVConsignmentACASValidationForm(HVLVConsignmentForACASWrapperCollection collection, ACASReportAction acasReportAction)
			: base(collection)
		{
			InitializeComponent();
			SetUpContextMenu();

			this.acasReportAction = acasReportAction;

			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			}

			ToolStripSendMessageButton = PostingButtonsUserControl.InsertAdditionalButton(
				"SendMessageButton",
				Res.GetString("d38bf193-ee21-46ca-8094-639640439f93", "Send Message"),
				Icons.GetImage(IconTypes.SendMessage), 3);
			ToolStripSendMessageButton.Click += ToolStripSendMessageButton_Click;
		}

		void ToolStripSendMessageButton_Click(object sender, EventArgs e)
		{
			if (Consignments.Any(c => c.HasChanges))
			{
				Globals.Message.Show(Res.GetString("8b965c96-c9fa-43f1-a8da-6bc21c074fae", "Please save the form before sending ACAS {0}.", ActionName));
			}
			else if (BusinessEntity.HasMessageErrors())
			{
				Globals.Message.Show(Res.GetString("71339ec9-82cd-4fcd-acd8-79cd5af36487", "There are message errors that need to be corrected before sending ACAS messages."));
			}
			else
			{
				if (!Shipment.PK.TryAcquireApplicationLock<ForwardingShipment>(
					(NoResString)"HVLV ACAS Report",
					SendACASReport,
					out var shipmentLockedErrorMessage))
				{
					Globals.Message.ShowError(shipmentLockedErrorMessage);
				}

				Close();
			}
		}

		string ActionName
		{
			get
			{
				switch (acasReportAction)
				{
					case ACASReportAction.SendAmendment:
						return Res.GetString("7af874b6-1351-42ab-9111-610106679489", "Amendment");
					case ACASReportAction.SendAcknowledgement:
						return Res.GetString("d0da72ca-ea59-4bf2-a05e-71bccca09da1", "Acknowledgement");
					default:
						return Res.GetString("4e11fe01-1844-4930-86ad-bf182de6f974", "Report");
				}
			}
		}

		void SendACASReport()
		{
			var result = false;
			var acasMessage = string.Empty;
			using (var progressForm = new ProgressForm())
			{
				progressForm.ShowCancelButton = false;
				progressForm.ShowProgressBar = true;
				progressForm.CaptionResourceString = Res.GetData("E4846FE3-92E9-42EE-8CAC-A6AD79EAE351", "Sending ACAS Message");
				progressForm.ShowModalTo(this);

				var acasSender = ObjectFactory.Get<IHVLVAirCargoAdvanceScreeningMessageSender>(nameof(IHVLVAirCargoAdvanceScreeningMessageSender),
					Shipment,
					(Action<string, int>)((message, progress) =>
					{
						if (!string.IsNullOrEmpty(message))
						{
							progressForm.SetStatusAndPercentComplete(message, progress);
						}
					}));

				using (ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(this))
				{
					result = acasSender.TrySendACASReports(acasReportAction, out acasMessage);
				}
			}

			if (result)
			{
				Globals.Message.ShowInformation(acasMessage, Res.GetString("1cc229d8-50c7-4766-84f2-3652e1ba4d29", "ACAS {0} Sent", ActionName));
			}
			else
			{
				Globals.Message.ShowWarning(acasMessage, Res.GetString("3D419DC9-3483-42D8-958F-18A4F13C56C7", "ACAS {0} Failed", ActionName));
			}
		}

		void SetUpContextMenu()
		{
			GridHVLVConsignments.ContextMenu.MenuItems.Add(new ZMenuItem("-"));

			// on pause until WI00413762 - eTail V2.ACAS Menu action to visualize consignment ACAS report is done
			//var openACASReportMenuItem = HVLVMenuItemHelper.OpenACASReport(OpenACASReport);
			//GridHVLVConsignments.ContextMenu.MenuItems.Add(openACASReportMenuItem);

			//GridHVLVConsignments.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
		}

		readonly ZToolStripButton ToolStripSendMessageButton;
		readonly ACASReportAction acasReportAction;

		IEnumerable<HVLVConsignment> Consignments => (BusinessEntity as HVLVConsignmentForACASWrapperCollection).Select(c => c.Consignment);

		ForwardingShipment Shipment => Consignments.FirstOrDefault()?.ConsignmentHeader?.Shipment;

		protected override bool AllowNew => false;

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(new ITransactionParticipant[] { Shipment.Factory });
		}
	}
}
