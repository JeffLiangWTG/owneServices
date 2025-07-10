using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.GUI
{
	public partial class RelatedDeclarationsUserControl : BaseRelatedDeclarationsUserControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.RelatedDeclarationsGrid)).BeginInit();
			this.RelatedDeclarationsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// RelatedDeclarationsGrid
			// 
			this.RelatedDeclarationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 212, true);
			// 
			// ButtonEdit
			// 
			this.ButtonEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 237, true);
			// 
			// ButtonNew
			// 
			this.ButtonNew.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 237, true);
			// 
			// ParentButton
			// 
			this.ParentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 237, true);
			//
			// RelatedDeclarationsGrid
			//
			var zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo5.Caption = ResString.GetMultilingualString("778E6DE7-4FFF-4667-AFCD-BFCC75B9A64C", "Type");
			zTextBoxColumnStyleInfo5.ColumnName = "JE_CopyStatusCaption";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RelatedDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			// 
			// RelatedDeclarationsUserControl
			// 
			this.Name = "RelatedDeclarationsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.RelatedDeclarationsGrid)).EndInit();
			this.RelatedDeclarationsGrid.ResumeLayout(false);
			this.RelatedDeclarationsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
