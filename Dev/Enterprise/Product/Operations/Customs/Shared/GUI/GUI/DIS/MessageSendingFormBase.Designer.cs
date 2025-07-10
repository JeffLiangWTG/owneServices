using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GUI
{
	public abstract partial class MessageSendingFormBase<T1, T2> where T1 : NonPersistentBusinessObject, IMessageSendingActionBase
		where T2 : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        protected new void InitializeComponent()
        {
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 241, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.MessageSendingActionCollectionBase<T1, T2>);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainGroupBox.Controls.Add(this.DocumentsGrid);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 205, true);
			this.MainGroupBox.TabIndex = 0;
			this.MainGroupBox.TabStop = false;
			this.MainGroupBox.CaptionResourceString = Res.GetData("212006d7-2b3c-4710-b2d8-3c22f51713ef", "Documents");
			// 
			// DocumentsGrid
			// 
			this.DocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((T1)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((T1)(null)).Send)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((T1)(null)).SendWithdrawal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((T1)(null)).MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((T1)(null)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((T1)(null)).DocumentDescription)));
			this.DocumentsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("3c3e8dff-dc55-43bb-9b80-d813c1716624", "Send Add/Replace");
			zCheckBoxColumnStyleInfo1.ColumnName = "Send";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("45d2b09b-be50-4427-a2b0-0b1ddee0e84c", "Send Withdrawal");
			zCheckBoxColumnStyleInfo2.ColumnName = "SendWithdrawal";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("ecc4b322-c94a-4a1c-95da-de38ea4ff0ed", "Message");
			zTextBoxColumnStyleInfo1.ColumnName = "MessageType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("1ae65371-72ac-4952-a1fe-bdc699c60cc6", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("9e8ce9c9-96e6-44a0-9744-5b0f412e1312", "Document");
			zTextBoxColumnStyleInfo3.ColumnName = "DocumentDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.DocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DocumentsGrid.CopySelectedRowsAllowed = true;
			this.DocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGrid.GridId = "c716b2c4-ffc2-4bf9-b3af-c556c27f2605";
			this.DocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentsGrid.LayoutKey = "zGrid1";
			this.DocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DocumentsGrid.Name = "DocumentsGrid";
			this.DocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 186, true);
			this.DocumentsGrid.TabIndex = 0;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 213, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.CaptionResourceString = Res.GetData("3959fa0f-1e1e-46ad-b905-aff8c1df8016", "Send");
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton1
			// 
			this.CancelButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 213, true);
			this.CancelButton1.Name = "CancelButton1";
			this.CancelButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.CancelButton1.TabIndex = 4;
			this.CancelButton1.CaptionResourceString = Res.GetData("65d1ca76-0654-44a5-9d26-626f493a3db6", "Cancel");
			this.CancelButton1.UseVisualStyleBackColor = true;
			this.CancelButton1.Click += new System.EventHandler(this.CancelButton1_Click);
			// 
			// MessageSendingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 265, true);
			this.Controls.Add(this.CancelButton1);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.MainGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.Business.MessageSendingActionCollectionBase<T1, T2>);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 308, true);
			this.Name = "MessageSendingForm";
			this.Text = "MessageSendingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CancelButton1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		protected ZArchitecture.ZGrid DocumentsGrid;
		internal ZArchitecture.GUI.ZButton SendButton;
		internal ZArchitecture.GUI.ZButton CancelButton1;
    }
}
