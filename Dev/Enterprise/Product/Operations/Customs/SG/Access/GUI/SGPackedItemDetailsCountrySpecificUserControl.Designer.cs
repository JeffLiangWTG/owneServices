using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	partial class SGPackedItemDetailsCountrySpecificUserControl
	{
		void InitializeComponent()
		{
			this.GoodsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SGEdiTariffFindBox = new Enterprise.Customs.SG.V4.GUI.TariffFindBox();
			this.GSTPaidDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsTypeDropEdit.SuspendLayout();
			this.GSTPaidDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.Access.Business.AsycudaPack);
			// 
			// GoodsTypeDropEdit
			// 
			this.GoodsTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "PackedItem.GoodsType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.AsycudaPack)(null)).PackedItem.GoodsType)));
			this.GoodsTypeDropEdit.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("F3C0ACD3-688A-4F20-A0E4-A5B06F42D96F", "Goods Type");
			this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
			this.GoodsTypeDropEdit.PreBoundMaxLength = 10;
			this.GoodsTypeDropEdit.ShouldResizeByMaxLength = true;
			this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.GoodsTypeDropEdit.TabIndex = 6;
			// 
			// SGEdiTariffFindBox
			// 
			this.SGEdiTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SGEdiTariffFindBox, "PackedItem.API_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.AsycudaPack)(null)).PackedItem.API_FormattedTariff)));
			this.SGEdiTariffFindBox.GetEffectiveDate = null;
			this.SGEdiTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 3, true);
			this.SGEdiTariffFindBox.Name = "SGEdiTariffFindBox";
			this.SGEdiTariffFindBox.SelectNomenclatureModes = null;
			this.SGEdiTariffFindBox.ShowDescriptionBox = false;
			this.SGEdiTariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.SGEdiTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.SGEdiTariffFindBox.TabIndex = 0;
			this.SGEdiTariffFindBox.TariffType = "HSN";
			// 
			// GSTPaidDropEdit
			// 
			this.GSTPaidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GSTPaidDropEdit, "PackedItem.GSTPaid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.AsycudaPack)(null)).PackedItem.GSTPaid)));
			this.GSTPaidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 70, true);
			this.GSTPaidDropEdit.Name = "GSTPaidDropEdit";
			this.GSTPaidDropEdit.PreBoundMaxLength = 1;
			this.GSTPaidDropEdit.ShowDescriptionBox = false;
			this.GSTPaidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 38, true);
			this.GSTPaidDropEdit.TabIndex = 9;
			// 
			// SGPackedItemDetailsCountrySpecificUserControl
			// 
			this.Controls.Add(this.SGEdiTariffFindBox);
			this.Controls.Add(this.GoodsTypeDropEdit);
			this.Controls.Add(this.GSTPaidDropEdit);
			this.Name = "SGPackedItemDetailsCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 101, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsTypeDropEdit.ResumeLayout(true);
			this.GoodsTypeDropEdit.PerformLayout();
			this.GSTPaidDropEdit.ResumeLayout(true);
			this.GSTPaidDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZDropEdit GoodsTypeDropEdit;
		internal Enterprise.Customs.SG.V4.GUI.TariffFindBox SGEdiTariffFindBox;
		internal ZDropEdit GSTPaidDropEdit;
	}
}


