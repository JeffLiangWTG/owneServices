using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.Module
{
	public partial class LoadListModeFilterControl : ZUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.zLabel2 = new ZArchitecture.ZLabel();
			this.zDropEdit1 = new ZDropEdit();
			this.zDropEdit2 = new ZDropEdit();
			this.SuspendLayout();
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 5, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = Res.GetString("8f526467-007b-450d-865b-0cb728e9bfce", "Trans. Mode");
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 5, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
			this.zLabel2.TabIndex = 0;
			this.zLabel2.Text = Res.GetString("ac07e7e7-e300-4cbe-852e-872fd68e5644", "Cont. Mode");
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.BindTo = "Property1";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((LoadListModeFilter)(null)).Property1Info)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((LoadListModeFilter)(null)).Property1)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 1, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.ShowDescriptionBox = false;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.BindTo = "Property2";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((LoadListModeFilter)(null)).Property2Info)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((LoadListModeFilter)(null)).Property2)));
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 1, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.zDropEdit2.TabIndex = 1;
			// 
			// LoadListModeFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zDropEdit2);
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.DataSourceAssemblyName = "Enterprise.Freight.CFS.Module";
			this.DataSourceTypeName = "Enterprise.Freight.CFS.Module.LoadListModeFilter";
			this.Name = "LoadListModeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 22, true);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZDropEdit zDropEdit1;
		private ZDropEdit zDropEdit2;
	}
}
