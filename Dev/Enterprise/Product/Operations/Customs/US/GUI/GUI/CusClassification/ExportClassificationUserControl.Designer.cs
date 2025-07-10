using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportClassificationUserControl
	{
		protected Universal.GUI.TariffFindBox cC_ScheduleBBoundFindBox;

		void InitializeComponent()
		{
			this.cC_ScheduleBBoundFindBox = new Universal.GUI.TariffFindBox();
			this.BaseClassificationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.cC_ScheduleBBoundFindBox);
			this.BaseClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 168, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.cC_ScheduleBBoundFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LookupCodeTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_IsActiveCheckBox, 0);
			// 
			// CC_ScheduleBBoundFindBox
			// 
			this.cC_ScheduleBBoundFindBox.BindTo = "CC_FormattedTariffNum";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.CusClassification)(null)).CC_FormattedTariffNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.CusClassification)(null)).CC_FormattedTariffNum)));
			this.cC_ScheduleBBoundFindBox.BindToList = "Lookups+Tariffs";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Business.CusClassification)(null)).Lookups.Tariffs)));
			this.cC_ScheduleBBoundFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ExportClassificationUserControl|a03e25ba-171a-4c06-8f74-706083103a9f", "Schedule B");
			this.cC_ScheduleBBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 52, true);
			this.cC_ScheduleBBoundFindBox.Name = "CC_ScheduleBBoundFindBox";
			this.cC_ScheduleBBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 21, true);
			this.cC_ScheduleBBoundFindBox.TabIndex = 1;
			// 
			// ExportClassificationUserControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.CusClassification";
			this.Name = "ExportClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 168, true);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
