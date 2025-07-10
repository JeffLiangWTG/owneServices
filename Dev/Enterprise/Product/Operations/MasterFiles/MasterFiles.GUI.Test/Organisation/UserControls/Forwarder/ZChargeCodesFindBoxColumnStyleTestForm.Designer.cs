namespace Enterprise.MasterFiles.GUI.Testing
{
	public partial class ZChargeCodesFindBoxColumnStyleTestForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			ZChargeCodesFindBoxColumnStyleInfo zMultiFindBoxColumnStyleInfo1 = new ZChargeCodesFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "Collection";
			this.zGrid1.CaptionVisible = false;
			zMultiFindBoxColumnStyleInfo1.BindToList = "Collection";
			zMultiFindBoxColumnStyleInfo1.Caption = "GuidFindBox";
			zMultiFindBoxColumnStyleInfo1.ColumnName = "Z0_NVarCharMax";
			zMultiFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Collection";
			zCodeFindBoxColumnStyleInfo1.Caption = "Code";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Z0_Code";

			if (this.addMultiFindColumnFirst)
			{
				this.zGrid1.ColumnStyles.Add(zMultiFindBoxColumnStyleInfo1);
				this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			}
			else
			{
				this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
				this.zGrid1.ColumnStyles.Add(zMultiFindBoxColumnStyleInfo1);
			}

			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = new System.Drawing.Point(16, 8);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = new System.Drawing.Size(528, 352);
			this.zGrid1.TabIndex = 0;
			// 
			// TestForm
			// 

			this.ClientSize = new System.Drawing.Size(560, 374);
			this.Controls.Add(this.zGrid1);
			this.Name = "TestForm";
			this.Text = "TestForm";
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		public Enterprise.ZArchitecture.ZGrid zGrid1;
	}
}
