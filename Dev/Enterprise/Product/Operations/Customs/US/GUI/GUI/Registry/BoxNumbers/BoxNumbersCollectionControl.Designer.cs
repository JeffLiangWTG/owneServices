namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class BoxNumbersCollectionControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BoxNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BoxNumbersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.BoxNumberCollection);
			// 
			// BoxNumbersGrid
			// 
			this.BoxNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BoxNumbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.BoxNumber)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.BoxNumber)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.BoxNumber)(null)).BoxNo)));
			this.BoxNumbersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("BoxNumbersCollectionControl|09f01901-1495-4d22-91a7-3469aaef7d1a", "Transport Mode");
			zDropEditColumnStyleInfo1.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.MaxDropDownItems = 3;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("BoxNumbersCollectionControl|d8e432f2-55a7-4750-a34d-feb2cbcaa29a", "Box Number");
			zTextBoxColumnStyleInfo1.ColumnName = "BoxNo";
			this.BoxNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.BoxNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BoxNumbersGrid.GridId = "9251e50a-f4cf-4ae4-b5dc-288133515614";
			this.BoxNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BoxNumbersGrid.LayoutKey = "BoxNumbersGrid";
			this.BoxNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BoxNumbersGrid.Name = "BoxNumbersGrid";
			this.BoxNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 159, true);
			this.BoxNumbersGrid.TabIndex = 0;
			// 
			// BoxNumbersCollectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BoxNumbersGrid);
			this.Name = "BoxNumbersCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 162, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BoxNumbersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid BoxNumbersGrid;
	}
}
