using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration.ContractManagement;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.GUI
{
	public partial class FreightContainersUserControl : ContainersUserControl
	{
		ZGuidFindBox JC_RCA_AllocationRouteCodeFindBox;
		ZTextBox ConsolNumberTextBox;
		ZTextBox ContractNumberTextBox;
		readonly bool IsContainerModuleForm;

		public FreightContainersUserControl()
		{
			InitializeComponent();
			IsContainerModuleForm = !GetType().IsSubclassOf(typeof(FreightContainersUserControl));
			// After first binding is neccessary here as the rendering of ContractAllocationInformationBox is conditional on the binding (CurrentContainer)
			AfterFirstBinding += FreightContainersUserControl_AfterFirstBinding;
		}

		void FreightContainersUserControl_AfterFirstBinding(object sender, System.EventArgs e)
		{
			AddContractModuleParentFields();
		}

		protected override void ExportTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			base.ExportTabPage_InitializeTab(sender, e);
			DepartureSlotReferenceTextBox.CaptionResourceString = Res.GetData("FreightContainersUserControl|20ff710c-3550-4e80-a2de-93be895afec0", "Slot Booking Ref");
			ExportPickupEmptyFromAddressControl.CaptionResourceString = Res.GetData("FreightContainersUserControl|f01a9b93-2e43-4c42-b2e9-8811a290b3bf", "Empty Pickup From");
		}

		protected override void ImportTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			base.ImportTabPage_InitializeTab(sender, e);
			ImportDeliverEmptyToAddressControl.CaptionResourceString = Res.GetData("FreightContainersUserControl|3ed3a8a6-c300-4230-a109-c0579d04d0b8", "Empty Return To");
			ImportReleaseNumberTextBox.CaptionResourceString = Res.GetData("FreightContainersUserControl|45bbbd86-4c44-4244-92d8-bc4f0962c999", "Release Number");
			ImportSlotReferenceTextBox.CaptionResourceString = Res.GetData("FreightContainersUserControl|3133107b-f50c-4f42-9e7c-5f93b3e424d8", "Slot Booking Ref");
		}

		void AddContractModuleParentFields()
		{
			var isAllocationVisible = ContractsPermissions.IsAllocationsVisible();

			JC_RCA_AllocationRouteCodeFindBox = (ZGuidFindBox)ObjectFactory.Get<IContractAllocationGuidFindBox>();
			JC_RCA_AllocationRouteCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(77, 107, true);
			JC_RCA_AllocationRouteCodeFindBox.Name = nameof(JC_RCA_AllocationRouteCodeFindBox);
			JC_RCA_AllocationRouteCodeFindBox.ShowDescriptionBox = false;
			JC_RCA_AllocationRouteCodeFindBox.Size = ControlDpiScalingHelper.NewScaledSize(140, 21, true);
			JC_RCA_AllocationRouteCodeFindBox.TabIndex = 13;
			JC_RCA_AllocationRouteCodeFindBox.Visible = isAllocationVisible;
			JC_RCA_AllocationRouteCodeFindBox.CaptionResourceString = Res.GetData("98B54460-6E65-44D3-B740-2897736D7419", "Allocation ID");

			BindingSource.SetBindingMember(JC_RCA_AllocationRouteCodeFindBox, "JC_RCA_AllocationLine");

			if (IsContainerModuleForm)
			{
				JC_RCA_AllocationRouteCodeFindBox.ReadOnly = CurrentContainer?.Consol == null;

				ConsolNumberTextBox = new ZTextBox()
				{
					Location = ControlDpiScalingHelper.NewScaledPoint(77, 133, true),
					Name = nameof(ConsolNumberTextBox),
					ReadOnly = true,
					Size = ControlDpiScalingHelper.NewScaledSize(84, 20, true),
					TabIndex = 14,
					Visible = isAllocationVisible,
					CaptionResourceString = Res.GetData("a28b640e-717c-a890-48e5-019bbcb2d2e3", "Consol No.")
				};
				DetailsGroupBox.Controls.Add(ConsolNumberTextBox);

				ContractNumberTextBox = new ZTextBox()
				{
					Location = ControlDpiScalingHelper.NewScaledPoint(77, 159, true),
					Name = nameof(ContractNumberTextBox),
					ReadOnly = true,
					Size = ControlDpiScalingHelper.NewScaledSize(84, 20, true),
					TabIndex = 15,
					Visible = isAllocationVisible,
					CaptionResourceString = Res.GetData("90BEAF3E-DE87-403C-A215-1A3E4CD25AB8", "Contract No.")
				};
				DetailsGroupBox.Controls.Add(ContractNumberTextBox);

				if (CurrentContainer is IForwardingContainer)
				{
					BindingSource.SetBindingMember(ConsolNumberTextBox, "Consol.JK_UniqueConsignRef");
					BindingSource.SetBindingMember(ContractNumberTextBox, "AllocationLine.Contract.RCT_ContractNumber");
				}
			}

			if (isAllocationVisible && IsContainerModuleForm)
			{
				DetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(320, 185, true);
			}
			else if (isAllocationVisible && !IsContainerModuleForm)
			{
				DetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(320, 140, true);
			}
			DetailsGroupBox.Controls.Add(JC_RCA_AllocationRouteCodeFindBox);
		}
	}
}

