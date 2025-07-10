using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsContainersWithTrackingUserControl : BaseCustomsCusContainersWithTrackingUserControl
	{
		public CustomsContainersWithTrackingUserControl()
			: base()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetupContainerSizeColumn();
				AddColumnsToGrid();
			}
			SendMCDContainerQuarantineDeclarationCheckBox.AllowOutsideOfParent();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		void AddColumnsToGrid()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo5.Caption = "TSW Container Size/Type";
			zDropEditColumnStyleInfo5.ColumnName = CusContainer.Schema.CO_MAF_ContainerType;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsContainerUserControl|9F449158-EAF8-4D00-A4E3-D72A7FD1FAD2", "TSW Size/Type", "TSW Container Size/Type", "Must be transmitted to state the size and type of container/pallet. Use UN/EDIFACT 8155 Equipment size and type description codes. For pallets use code 16 (exchangeable pallet).");
			ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo5, 130, true);
			zDropEditColumnStyleInfo5.IsVisible = true;
			this.CusContainersBoundGrid.ColumnStyles.Insert(7, zDropEditColumnStyleInfo5);

			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo11.Caption = "Sealing Party Name";
			zTextBoxColumnStyleInfo11.ColumnName = CusContainer.Schema.CO_SealingParty;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo11, 130, true);
			zTextBoxColumnStyleInfo11.IsVisible = true;
			zTextBoxColumnStyleInfo11.CharacterCasing = CharacterCasing.Upper;
			this.CusContainersBoundGrid.ColumnStyles.Insert(8, zTextBoxColumnStyleInfo11);

			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo12.Caption = "MPI Approved System Number";
			zTextBoxColumnStyleInfo12.ColumnName = CusContainer.Schema.CO_MPIApprovedSystemNumber;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo12, 155, true);
			zTextBoxColumnStyleInfo12.IsVisible = true;
			zTextBoxColumnStyleInfo12.CharacterCasing = CharacterCasing.Upper;
			this.CusContainersBoundGrid.ColumnStyles.Insert(9, zTextBoxColumnStyleInfo12);

			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyle1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			zOrganisationFindBoxColumnStyle1.Caption = "Container Pack Location";
			zOrganisationFindBoxColumnStyle1.ColumnName = CusContainer.Schema.PackingLocationOrgPK;
			ControlDpiScalingHelper.SetWidth(ref zOrganisationFindBoxColumnStyle1, 130, true);
			zOrganisationFindBoxColumnStyle1.IsVisible = true;
			zOrganisationFindBoxColumnStyle1.CharacterCasing = CharacterCasing.Upper;
			zOrganisationFindBoxColumnStyle1.GroupName = Res.GetData("CustomsContainersWithTrackingUserControl|4F5C9472-B16C-457F-A0AA-7BF91CF0D5CB", "Container Pack Location");
			this.CusContainersBoundGrid.ColumnStyles.Insert(10, zOrganisationFindBoxColumnStyle1);

			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			zGuidDropEditColumnStyleInfo1.Caption = "Address";
			zGuidDropEditColumnStyleInfo1.ColumnName = CusContainer.Schema.CO_OA_PackingLocation;
			ControlDpiScalingHelper.SetWidth(ref zGuidDropEditColumnStyleInfo1, 130, true);
			zGuidDropEditColumnStyleInfo1.IsVisible = true;
			zGuidDropEditColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.GroupName = Res.GetData("CustomsContainersWithTrackingUserControl|4F5C9472-B16C-457F-A0AA-7BF91CF0D5CB", "Container Pack Location");
			this.CusContainersBoundGrid.ColumnStyles.Insert(11, zGuidDropEditColumnStyleInfo1);
		}

		void SetupContainerSizeColumn()
		{
			ZDropEditColumnStyleInfo containerSizeDropEditColumnStyle = new ZDropEditColumnStyleInfo();

			this.SuspendLayout();

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CusContainer)(((object)(((JobDeclaration)(null)).CusContainers)))).Lookups.CO_ContainerSizeList);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CusContainer)(((object)(((JobDeclaration)(null)).CusContainers)))).CO_ContainerSizeInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CusContainer)(((object)(((JobDeclaration)(null)).CusContainers)))).CO_ContainerSize);
			containerSizeDropEditColumnStyle.BindToList = "Lookups.CO_ContainerSizeList";
			containerSizeDropEditColumnStyle.Caption = "Container Size";
			containerSizeDropEditColumnStyle.ColumnName = CusContainer.Schema.CO_ContainerSize;
			containerSizeDropEditColumnStyle.ToolTip = "Container Size Declared";
			this.CusContainersBoundGrid.InnerGrid.ColumnStyles.Add(containerSizeDropEditColumnStyle);

			this.ResumeLayout();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			QuarantineGroupBox.Visible = JobDeclaration.IsQuarantineGroupBoxVisible;
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			bool isTSW = JobDeclaration.IsTSWDeclaration;
			CusContainersBoundGrid.InnerGrid.SetAvailability(!isTSW && JobDeclaration.IsECIWriteoff, CusContainer.Schema.CO_ContainerSize);
			CusContainersBoundGrid.InnerGrid.SetAvailability(isTSW, CusContainer.Schema.CO_MAF_ContainerType);
			CusContainersBoundGrid.InnerGrid.SetAvailability(isTSW, CusContainer.Schema.CO_SealingParty);
			CusContainersBoundGrid.InnerGrid.SetAvailability(isTSW, CusContainer.Schema.CO_MPIApprovedSystemNumber);
			CusContainersBoundGrid.InnerGrid.SetAvailability(isTSW, CusContainer.Schema.PackingLocationOrgPK);
			CusContainersBoundGrid.InnerGrid.SetAvailability(isTSW, CusContainer.Schema.CO_OA_PackingLocation);
		}

		protected override void LoadPluginsCore()
		{
			base.LoadPluginsCore();
			if (!JobDeclaration.IsTSWDeclaration)
			{
				this.containersUserControl1.DetailTabControl.PlugIns.Add(ControllerIDs.Customs.NZ.MAFeBACCaContainerPlugin);
			}
		}
	}
}

