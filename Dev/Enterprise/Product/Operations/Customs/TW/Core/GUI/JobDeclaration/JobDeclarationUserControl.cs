using System;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				Controls.Remove(SupplierOrganisationControl);
				Controls.Remove(ImporterOrganisationControl);
			}
			RightTabControl.SelectedIndexChanged += RightTabControl_SelectedIndexChanged;
			ReorderRightTabeControlTabs();
		}

		void RightTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (RightTabControl.SelectedTab == OrganisationsTabPage)
			{
				ResetOrganisationsTabPageTabIndexsAndLocation();
				RightTabControl.SelectedIndexChanged -= RightTabControl_SelectedIndexChanged;
			}
		}

		protected override bool JE_ContainerModeBoundDropDownEditVisible => base.JE_ContainerModeBoundDropDownEditVisible || (JobDeclaration != null && JobDeclaration.IsAir && !JobDeclaration.IsNonTransportDeclarationType);

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			bool isSea = JobDeclaration.IsSea;
			TWVesselArrivalRegTextBox.Visible = isSea;
			TWSLDTextBox.Visible = isSea;
			JE_ContainerCountCalcEdit.Visible = isSea;
			JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
			TWSLDTextBox.UpdateCaption();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var declaration = JobDeclaration;
			if (declaration != null)
			{
				PortOfOriginInfo_ValueChanged(this, null);
				FinalDestinationInfo_ValueChanged(this, null);
				declaration.JE_RL_NKOriginInfo.ValueChanged -= PortOfOriginInfo_ValueChanged;
				declaration.JE_RL_NKOriginInfo.ValueChanged += PortOfOriginInfo_ValueChanged;
				declaration.JE_RL_NKFinalDestinationInfo.ValueChanged -= FinalDestinationInfo_ValueChanged;
				declaration.JE_RL_NKFinalDestinationInfo.ValueChanged += FinalDestinationInfo_ValueChanged;
				AllocateEntryNumberButton.Enabled = ModifyEntryNumberButton.Enabled = declaration.ClearanceStatus.IsEmpty;
			}
		}

		protected override void SetFolioNumberVisible()
		{
			FolioNumberTextBox.Visible = false;
		}

		void PortOfOriginInfo_ValueChanged(object sender, EventArgs e)
		{
			var isZ99PortOfOrigin = JobDeclaration.IsZ99PortOfOrigin;
			OriginFindBox.ShowDescriptionBox = !isZ99PortOfOrigin;
			JE_Z99PortOfOriginTextBox.Visible = isZ99PortOfOrigin;
		}

		void FinalDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			var isZ99FinalDestination = JobDeclaration.IsZ99FinalDestination;
			FinalDestinationFindBox.ShowDescriptionBox = !isZ99FinalDestination;
			JE_Z99FinalDestinationTextBox.Visible = isZ99FinalDestination;
		}

		void ResetOrganisationsTabPageTabIndexsAndLocation()
		{
			OrganisationsTopPanel.SuspendLayout();
			OrganisationsTopPanel.Controls.Add(BondedWarehouseDocAddressControl);
			OrganisationsTopPanel.Controls.Add(ContainerYardAddressControl);
			OrganisationsTopPanel.Controls.Add(DepotAddressControl);
			OrganisationsTopPanel.Controls.Add(ContainerTerminalOperatorAddressControl);
			OrganisationsTopPanel.Controls.Add(NotifyOrganisationControl);
			OrganisationsTopPanel.Controls.Add(ForwarderOrganisationControl);
			OrganisationsTopPanel.Controls.Add(ShippingOrAirLineOrganisationControl);
			OrganisationsTopPanel.Controls.Add(ConsigneeOrganisationControl);
			OrganisationsTopPanel.Controls.Add(ConsignorOrganisationControl);
			OrganisationsTopPanel.Controls.Add(ControllingAgentGuidFindBox);
			OrganisationsTopPanel.Controls.Add(ControllingCustomerGuidFindBox);
			OrganisationsTopPanel.Controls.Add(ExternalBrokerGuidFindBox);
			var index = 0;
			ConsignorOrganisationControl.TabIndex = index++;
			ConsigneeOrganisationControl.TabIndex = index++;
			ShippingOrAirLineOrganisationControl.TabIndex = index++;
			ForwarderOrganisationControl.TabIndex = index++;
			NotifyOrganisationControl.TabIndex = index++;
			ContainerTerminalOperatorAddressControl.TabIndex = index++;
			DepotAddressControl.TabIndex = index++;
			ContainerYardAddressControl.TabIndex = index++;
			BondedWarehouseDocAddressControl.TabIndex = index++;
			ControllingAgentGuidFindBox.TabIndex = index++;
			ControllingCustomerGuidFindBox.TabIndex = index++;
			ExternalBrokerGuidFindBox.TabIndex = index++;

			OrganisationsTopPanel.ResumeLayout(true);
			OrganisationsTopPanel.PerformLayout();
		}

		void ReorderRightTabeControlTabs()
		{
			RightTabControl.SuspendLayout();
			RightTabControl.TabPages.Remove(EntryDetailsTabPage);
			RightTabControl.TabPages.Insert(EntryDetailsTabPage, 0);
			RightTabControl.TabPages.Remove(BondedDetailsTabPage);
			RightTabControl.TabPages.Insert(BondedDetailsTabPage, 1);
			RightTabControl.TabPages.Remove(OrganisationsTabPage);
			RightTabControl.TabPages.Insert(OrganisationsTabPage, 2);
			RightTabControl.ResumeLayout(false);
			RightTabControl.PerformLayout();
		}

		protected override void SetRightTabControlSelectTab()
		{
			RightTabControl.SelectedTab = EntryDetailsTabPage;
		}

		void AllocateEntryNumberButton_Click(object sender, EventArgs e)
		{
			if (FindForm() is ZForm mainForm)
			{
				new AllocateNumberButtonClickEventHandler().Allocate(new FormalDeclarationEntryNumberSupporter(JobDeclaration), new AllocateEventHandlerArgs
					((JobDeclaration).EntryHeader?.IsWaitingForResponseOrHasBeenLodgedAtCustoms ?? false,
					mainForm.BusinessEntityForHasChanges,
					() => mainForm.FireSaveButton()), ZString.Empty);
			}
		}

		void ModifyEntryNumberButton_Click(object sender, EventArgs e)
		{
			if (FindForm() is ZForm mainForm)
			{
				new AllocateNumberButtonClickEventHandler().Modify(new FormalDeclarationEntryNumberSupporter(JobDeclaration), new AllocateEventHandlerArgs
					((JobDeclaration).EntryHeader?.IsWaitingForResponseOrHasBeenLodgedAtCustoms ?? false,
					mainForm.BusinessEntityForHasChanges,
					() => mainForm.FireSaveButton()));
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				declaration.JE_RL_NKOriginInfo.ValueChanged -= PortOfOriginInfo_ValueChanged;
				declaration.JE_RL_NKFinalDestinationInfo.ValueChanged -= FinalDestinationInfo_ValueChanged;
			}
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
