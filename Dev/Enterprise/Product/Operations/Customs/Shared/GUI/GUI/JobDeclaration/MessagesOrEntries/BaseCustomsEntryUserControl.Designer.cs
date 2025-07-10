using Enterprise.ZArchitecture;
using System.ComponentModel;

namespace Enterprise.Customs.GUI
{
	partial class BaseCustomsEntryUserControl
	{
		void InitializeComponent()
		{
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			RequiresMergeLabel = new ZLabel();
			RequiresMergeLabel.Name = "RequiresMergeLabel";
			RequiresMergeLabel.IsFontBold = true;
			RequiresMergeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			RequiresMergeLabel.Visible = false;
			Controls.Add(RequiresMergeLabel);

			// 
			// BaseCustomsEntryUserControl
			// 
			this.Name = "BaseCustomsEntryUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 156, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
