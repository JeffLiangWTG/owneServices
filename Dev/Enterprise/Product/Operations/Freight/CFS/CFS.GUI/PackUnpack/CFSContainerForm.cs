using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.CFS.GUI
{
	#region IContainerCopyAndTransformSupporter Interface

	public interface IContainerCopyAndTransformSupporter
	{
		IZForm ShowCopyFormWithTransform(BusinessObject cFSContainer);
	}

	#endregion

	public partial class CFSContainerForm : ZTemplateForm
	{
		public CFSContainerForm(CFSContainer container)
			: base(container)
		{
			InitializeComponent();

			PlugIns.Add(ControllerIDs.JobInvoicing);
			JobInvoicingPlugin = PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			SetInvoicingPluginState();

			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ABD2D6B2-29D3-4ebb-9510-36815ABD499C", "Delivery Order Handed Over"), new EventHandler(this.DeliveryOrderHandedOver_Click));

			AddRegistrationDetailsTabPage();
			SetNewContainerRegistrationButtonText();

			HookEvents();

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };

			ServicesSelectionGuiProvider.Register(container.Factory);
		}

		#region Registration Details

		void AddRegistrationDetailsTabPage()
		{
			RegistrationDetails = new RegistrationDetails();
			RegistrationDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			MainTabPage.Controls.Add(RegistrationDetails);
		}

		protected RegistrationDetails RegistrationDetails;

		CFSContainer CFSContainer
		{
			get { return (CFSContainer)BusinessEntity; }
		}

		#endregion

		#region Form Overrides

		public override string FormCaption
		{
			get { return Res.GetString("7ab7a991-e5dd-498d-94bf-2fec552554e1", "Container Registration") + (CFSContainer.JC_ContainerJobID.IsEmpty ? "" : " " + CFSContainer.JC_ContainerJobID); }
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			ContinueWithDelete result = ContinueWithDelete.No;

			if (CFSContainer.HasShipments)
			{
				Globals.Message.ShowError(Res.GetString("d4f14f86-5bcf-4aa5-a5d4-2c0e82a6943b", "This container contains at least one Shipment and has been packed.\r\nDeletion is not allowed.\r\nThe container must be unpacked before deletion is allowed."));
			}
			else if (CFSContainer.IsPackedOrUnpacked)
			{
				Globals.Message.ShowError(Res.GetString("2dabc90e-4392-4e03-93a1-de7a183263d8", "This Container cannot be deleted because is has been packed/unpacked."));
			}
			else if (CFSContainer.IsAttachedToLoadList)
			{
				DialogResult doDelete = Globals.Message.Show(Res.GetString("636132ab-72f9-4153-9cd3-96a408a18c4b", "Warning: this Container is currently attached to a load list.\r\nYou are about to permanently delete this Container.\r\nDo you want to proceed?"),
					Res.GetString("0dfbae92-f17a-4d18-9e1b-733befb1b024", "Attached to Load List"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				if (doDelete == DialogResult.Yes)
				{
					result = base.ShowPreDeleteDialogs();
				}
			}

			return result;
		}

		#endregion

		#region Events

		protected void DeliveryOrderHandedOver_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(NewRaiseEventLogForm(ZArchitecture.Business.Events.DeliveryOrderHandedOver), this);
		}

		protected ZStmALogAddForm NewRaiseEventLogForm(Event @event)
		{
			var view = new StmALogCollectionView(CFSContainer);
			var form = new ZStmALogAddForm(view, CFSContainer.HasChanges);
			var newLog = (BaseStmALog)form.BusinessEntity;
			newLog.SL_SE_NKEvent = @event.Code;
			return form;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			UnhookEvents();
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Hook / Unhook Events

		void HookEvents()
		{
			if (CFSContainer != null)
			{
				CFSContainer.JC_PurposeInfo.ValueChanged += new EventHandler(JC_PurposeInfo_ValueChanged);
				CFSContainer.WarningMessageInfo.ValueChanged += new EventHandler(ShowWarningMessage);
			}
		}

		void UnhookEvents()
		{
			if (CFSContainer != null)
			{
				CFSContainer.JC_PurposeInfo.ValueChanged -= new EventHandler(JC_PurposeInfo_ValueChanged);
				CFSContainer.WarningMessageInfo.ValueChanged -= new EventHandler(ShowWarningMessage);
			}
		}

		#endregion

		#region Create New Container Registration

		void SetNewContainerRegistrationButtonText()
		{
			if (CFSContainer.JC_Purpose == ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS)
			{
				CreateNewContainerRegistrationButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("742968FB-6107-49a9-879C-B49BAAAE8146", "Create Storage From CFS");
			}
			else
			{
				CreateNewContainerRegistrationButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("66D9ED3E-F119-4469-8993-4F7DAE673614", "Create CFS From Storage");
			}
		}

		void CreateNewContainerRegistrationButton_Click(object sender, EventArgs e)
		{
			if (!CFSContainer.HasChanges)
			{
				if (CFSContainer.JC_ArrivalTime.IsValid)
				{
					IContainerCopyAndTransformSupporter controller = (IContainerCopyAndTransformSupporter)ZControllerFactory.Create(ControllerIDs.PackContainerRegistration);
					controller.ShowCopyFormWithTransform(CFSContainer);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("9a208cdc-1119-4df5-990f-385fe5e8b5e9", "You must enter an arrival time before you create a new container registration."), Res.GetString("a535828f-b693-4c19-9b99-52cfff2320ac", "Container Registration"));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("9dd85557-aaa1-49e6-89f2-7413fb415ec6", "Please save the form before you create a new container registration."), Res.GetString("e22a28ec-1936-4917-bb14-40c86619b7ac", "Container Registration"));
			}
		}

		#endregion

		#region ShowWarningMessage

		void ShowWarningMessage(object sender, EventArgs e)
		{
			if (CFSContainer.ContinueWithChanging && !CFSContainer.WarningMessage.IsEmpty)
			{
				DialogResult result = Globals.Message.Show(CFSContainer.WarningMessage, CFSContainer.MessageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				CFSContainer.ContinueWithChanging = (result == DialogResult.Yes);
			}
		}

		#endregion

		#region Billing

		void JC_PurposeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetInvoicingPluginState();
			SetNewContainerRegistrationButtonText();
		}

		void SetInvoicingPluginState()
		{
			JobInvoicingPlugin.Enabled = CFSContainer.JC_Purpose == ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
		}

		readonly ZPlugIn JobInvoicingPlugin;

		#endregion

	}
}
