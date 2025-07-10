namespace Enterprise.Customs.NL.GUI
{
	public partial class ContainerDetailsUserControl : EU.GUI.ContainersUserControl
	{
		public ContainerDetailsUserControl()
		{
			InitializeComponent();
		}

		new void ImportTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ImportTabPage.SuspendLayout();
			this.ImportDeliverEmptyToAddressControl.SuspendLayout();
			this.ImportTabPage.PerformLayout();
			this.ImportDeliverEmptyToAddressControl.ResumeLayout(true);
			this.ImportDeliverEmptyToAddressControl.PerformLayout();
			this.ImportTabPage.ResumeLayout(true);
		}

		new void ExportTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ExportTabPage.SuspendLayout();
			this.ExportEmptyReqByDateEdit.SuspendLayout();
			this.ExportPickupEmptyFromAddressControl.SuspendLayout();
			this.RelatedContainerLoadListFindBox.SuspendLayout();
			// 
			// RelatedContainerLoadListFindBox
			// 
			this.RelatedContainerLoadListFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 25, true);
			this.ExportTabPage.PerformLayout();
			this.ExportEmptyReqByDateEdit.ResumeLayout(true);
			this.ExportEmptyReqByDateEdit.PerformLayout();
			this.ExportPickupEmptyFromAddressControl.ResumeLayout(true);
			this.ExportPickupEmptyFromAddressControl.PerformLayout();
			this.RelatedContainerLoadListFindBox.ResumeLayout(true);
			this.RelatedContainerLoadListFindBox.PerformLayout();
			this.ExportTabPage.ResumeLayout(true);
		}

		void OutturnTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.OutturnTabPage.SuspendLayout();
			this.OutturnTabPage.PerformLayout();
			this.OutturnTabPage.ResumeLayout(true);
		}

		void VGMTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.VGMTabPage.SuspendLayout();
			this.VGMTabPage.PerformLayout();
			this.VGMTabPage.ResumeLayout(true);
		}

		void FreightRatesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.FreightRatesTabPage.SuspendLayout();
			this.FreightRatesTabPage.PerformLayout();
			this.FreightRatesTabPage.ResumeLayout(true);
		}
	}
}
