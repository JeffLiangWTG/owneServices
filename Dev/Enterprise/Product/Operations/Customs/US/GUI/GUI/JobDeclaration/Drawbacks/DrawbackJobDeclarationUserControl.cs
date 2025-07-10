using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.EntryNumber;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class DrawbackJobDeclarationUserControl : Customs.GUI.FrontPageUserControl
	{
		public DrawbackJobDeclarationUserControl()
		{
			InitializeComponent();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ShowOrHideBondAndCalculationCode();
			ChangeJE_ApplicationCode();
		}

		void RefreshBondButton_Click(object sender, EventArgs e)
		{
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				if (Globals.Message.Show((declaration.IsACEDrawback && !declaration.US_AcceleratedClaimInd ? BondNotRequiredForACEDrawback : string.Empty) + RefreshBondDetailsWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					new BondDetailsDefaulter().Default(JobDeclaration, JobDeclaration.US_BondType);
				}
			}
		}
		const string RefreshBondDetailsWarning = "Are you sure you want to refresh the Bond details?";
		const string BondNotRequiredForACEDrawback = "Bond details are not required if 'ACC. Payment Request Ind.' is not ticked.\r\n";

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			US_DRWPurposeInfo_ValueChanged(null, null);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (JobDeclaration != null)
			{
				JobDeclaration.US_DRWPurposeInfo.ValueChanged -= new EventHandler(US_DRWPurposeInfo_ValueChanged);
				JobDeclaration.JE_ApplicationCodeInfo.ValueChanged -= new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
				JobDeclaration.US_BondTypeInfo.ValueChanged -= new EventHandler(US_BondTypeInfo_ValueChanged);
				JobDeclaration.US_BondType2Info.ValueChanged -= new EventHandler(US_BondTypeInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null && JobDeclaration != null)
			{
				JobDeclaration.US_DRWPurposeInfo.ValueChanged += new EventHandler(US_DRWPurposeInfo_ValueChanged);
				JobDeclaration.JE_ApplicationCodeInfo.ValueChanged += new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
				JobDeclaration.US_BondTypeInfo.ValueChanged += new EventHandler(US_BondTypeInfo_ValueChanged);
				JobDeclaration.US_BondType2Info.ValueChanged += new EventHandler(US_BondTypeInfo_ValueChanged);
			}
		}

		void US_DRWPurposeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlVisibility();
		}

		void ControlVisibility()
		{
			var is7552 = JobDeclaration != null && JobDeclaration.Is7552;
			var isACEDrawback = JobDeclaration != null && JobDeclaration.IsACEDrawback;
			this.AdditionalDetailsGroupBox.Visible = is7552 && !isACEDrawback;
			this.ContractNumberszGroupBox.Visible = !is7552 && !isACEDrawback;
			this.BondGroupBox.Visible = !is7552 && !isACEDrawback;
			this.ImporterOrganisationControl.Visible = !is7552 || isACEDrawback;
			this.TransferorOrganizationControl.Visible = is7552 && !isACEDrawback;
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeJE_ApplicationCode();
			ControlVisibility();
		}

		void US_BondTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideBondAndCalculationCode();
		}

		void ChangeJE_ApplicationCode()
		{
			var isACEDrawback = JobDeclaration != null && JobDeclaration.IsACEDrawback;
			ACSDrawbackPanel.Visible = !isACEDrawback;
			ACEDrawbackPanel.Visible = isACEDrawback;
			ACERightPanel.Visible = isACEDrawback;
			StatusTextBox.Visible = !isACEDrawback;
			MessageStatusDescriptionTextBox.Visible = !isACEDrawback;
			IntendedPortOfExportCodeFindBox.Visible = !isACEDrawback;

			if (isACEDrawback)
			{
				MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 44, true);
				DrawbackDelcarationPurposeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 44, true);
			}
			else
			{
				MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 98, true);
				DrawbackDelcarationPurposeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 98, true);
			}
		}

		void ShowOrHideBondAndCalculationCode()
		{
			bool isVisible = JobDeclaration != null && JobDeclaration.IsSingleTransactionBond;
			bool isContinuousBond = JobDeclaration != null && JobDeclaration.US_BondType == BondTypeList.Codes.ContinuousBond;
			bool bondProducerAccNoVisible = isVisible || isContinuousBond;

			ACEBondAmountCalcEdit.Visible = isVisible;
			ACEBondDesignationCodeDropEdit.Visible = isVisible;
			ACEAccountNoTextBox.Visible = bondProducerAccNoVisible;
		}

		void AllocateImportEntryNumberButton_Click(object sender, EventArgs e)
		{
			ZForm mainForm = FindForm() as ZForm;
			if (mainForm != null)
			{
				var args = new AllocateEventHandlerArgs();
				args.FireSaveButton = () => mainForm.FireSaveButton();
				args.TopBizObjForFormHasChanges = mainForm.BusinessEntityForHasChanges;
				args.PerformActionBeforeShowingForm = null;
				new AllocateNumberButtonClickEventHandler().Allocate(JobDeclaration.GetAllocateNumberSupporter(), args);
			}
		}
	}
}
