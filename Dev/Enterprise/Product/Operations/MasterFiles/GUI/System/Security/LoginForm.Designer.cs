namespace Enterprise.MasterFiles.GUI
{
	public partial class LoginForm
	{
		#region Designer generated code

		void InitializeComponent()
		{
			this.oGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LoginTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.oLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelXButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.oGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SecurityMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.oGroupBox1.SuspendLayout();
			this.oGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// oGroupBox1
			// 
			this.oGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.oGroupBox1.Controls.Add(this.PasswordTextBox);
			this.oGroupBox1.Controls.Add(this.LoginTextBox);
			this.oGroupBox1.Controls.Add(this.oLabel3);
			this.oGroupBox1.Controls.Add(this.oLabel2);
			this.oGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 138, true);
			this.oGroupBox1.Name = "oGroupBox1";
			this.oGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 87, true);
			this.oGroupBox1.TabIndex = 0;
			this.oGroupBox1.TabStop = false;
			this.oGroupBox1.Text = Enterprise.MasterFiles.GUI.Res.GetString("afbe3652-04ed-43ad-a748-06a3276e9d1d", "Authorizing User");
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 55, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 20, true);
			this.PasswordTextBox.TabIndex = 3;
			// 
			// LoginTextBox
			// 
			this.LoginTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LoginTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 30, true);
			this.LoginTextBox.Name = "LoginTextBox";
			this.LoginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 20, true);
			this.LoginTextBox.TabIndex = 2;
			// 
			// oLabel3
			// 
			this.oLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 54, true);
			this.oLabel3.Name = "oLabel3";
			this.oLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.oLabel3.TabIndex = 1;
			this.oLabel3.Text = Enterprise.MasterFiles.GUI.Res.GetString("16eaa12f-1f21-4987-811a-a7f84511499D", "Password:");
			// 
			// oLabel2
			// 
			this.oLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 30, true);
			this.oLabel2.Name = "oLabel2";
			this.oLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.oLabel2.TabIndex = 0;
			this.oLabel2.Text = Enterprise.MasterFiles.GUI.Res.GetString("a552350d-c133-4f12-8aa8-bab09410aa66", "Username:");
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 234, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Text = Enterprise.MasterFiles.GUI.Res.GetString("e7f32209-d0cd-4d7c-83a6-d52616c7a7c1", "OK");
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelXButton
			// 
			this.CancelXButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelXButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelXButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 234, true);
			this.CancelXButton.Name = "CancelXButton";
			this.CancelXButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
			this.CancelXButton.TabIndex = 2;
			this.CancelXButton.Text = Enterprise.MasterFiles.GUI.Res.GetString("c3a55815-f02c-4e6b-8f47-79451842b500", "Cancel");
			this.CancelXButton.Click += new System.EventHandler(this.CancelXButton_Click);
			// 
			// oGroupBox2
			// 
			this.oGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.oGroupBox2.Controls.Add(this.SecurityMessageTextBox);
			this.oGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.oGroupBox2.Name = "oGroupBox2";
			this.oGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 136, true);
			this.oGroupBox2.TabIndex = 10;
			this.oGroupBox2.TabStop = false;
			// 
			// SecurityMessageTextBox
			// 
			this.SecurityMessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SecurityMessageTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.SecurityMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SecurityMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 13, true);
			this.SecurityMessageTextBox.Multiline = true;
			this.SecurityMessageTextBox.Name = "SecurityMessageTextBox";
			this.SecurityMessageTextBox.ReadOnly = true;
			this.SecurityMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.SecurityMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 118, true);
			this.SecurityMessageTextBox.TabIndex = 0;
			this.SecurityMessageTextBox.TabStop = false;
			// 
			// LoginForm
			// 
			this.AcceptButton = this.OKButton;

			this.CancelButton = this.CancelXButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 264, true);
			this.Controls.Add(this.oGroupBox2);
			this.Controls.Add(this.CancelXButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.oGroupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoginForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = Enterprise.MasterFiles.GUI.Res.GetString("f5076ae1-ed3c-4fa2-8c3c-b628819d9c09", "Security Override Login");
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.oGroupBox1.ResumeLayout(false);
			this.oGroupBox1.PerformLayout();
			this.oGroupBox2.ResumeLayout(false);
			this.oGroupBox2.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox oGroupBox1;
		private Enterprise.ZArchitecture.ZLabel oLabel2;
		private Enterprise.ZArchitecture.ZLabel oLabel3;
		internal protected Enterprise.ZArchitecture.ZTextBox LoginTextBox;
		internal protected Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		internal protected Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox oGroupBox2;
		private Enterprise.ZArchitecture.ZTextBox SecurityMessageTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelXButton;

	}
}
