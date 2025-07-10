namespace Enterprise.Customs.NL.GUI
{
	partial class FallbackControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zfallbackGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDateEditEnd = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEditRegularisation = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCalcEditPeriod = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zTextBoxInvocation = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBoxRevocation = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEditStart = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCalcEditCount = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEditBatchSize = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zfallbackGroupbox.SuspendLayout();
			this.zDateEditEnd.SuspendLayout();
			this.zDateEditRegularisation.SuspendLayout();
			this.zDateEditStart.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.FallbackConfiguration);
			// 
			// zfallbackGroupbox
			// 
			this.zfallbackGroupbox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FallbackControl|FAF137C7-47DC-4024-8923-7C1470D61610", "Fallback settings");
			this.zfallbackGroupbox.Controls.Add(this.zDateEditEnd);
			this.zfallbackGroupbox.Controls.Add(this.zDateEditRegularisation);
			this.zfallbackGroupbox.Controls.Add(this.zCalcEditPeriod);
			this.zfallbackGroupbox.Controls.Add(this.zTextBoxInvocation);
			this.zfallbackGroupbox.Controls.Add(this.zTextBoxRevocation);
			this.zfallbackGroupbox.Controls.Add(this.zDateEditStart);
			this.zfallbackGroupbox.Controls.Add(this.zCalcEditCount);
			this.zfallbackGroupbox.Controls.Add(this.zCalcEditBatchSize);
			this.zfallbackGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zfallbackGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zfallbackGroupbox.Name = "zfallbackGroupbox";
			this.zfallbackGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.zfallbackGroupbox.TabIndex = 0;
			this.zfallbackGroupbox.TabStop = false;
			// 
			// zDateEditEnd
			// 
			this.zDateEditEnd.AllowDrop = true;
			this.zDateEditEnd.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEditEnd, "End");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.FallbackConfiguration)(null)).End)));
			this.zDateEditEnd.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FallbackControl|B7BD3791-D480-418E-96C7-EB8AE5A8FF7D", "End");
			this.zDateEditEnd.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditEnd.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 27, true);
			this.zDateEditEnd.Name = "zDateEditEnd";
			this.zDateEditEnd.TabIndex = 2;
			// 
			// zDateEditRegularisation
			// 
			this.zDateEditRegularisation.AllowDrop = true;
			this.zDateEditRegularisation.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEditRegularisation, "Regularisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.FallbackConfiguration)(null)).Regularisation)));
			this.zDateEditRegularisation.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FallbackControl|4AFF468B-4097-4682-A13B-2E7EACEEB20E", "Regularization date");
			this.zDateEditRegularisation.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditRegularisation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 54, true);
			this.zDateEditRegularisation.Name = "zDateEditRegularisation";
			this.zDateEditRegularisation.TabIndex = 3;
			// 
			// zCalcEditPeriod
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditPeriod, "RegularisationPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NL.Business.FallbackConfiguration)(null)).RegularisationPeriod)));
			this.zCalcEditPeriod.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("2CA8726C-B7B6-4F6B-B6B6-820AEA560F54", "Period");
			this.zCalcEditPeriod.DecimalPlaces = 0;
			this.zCalcEditPeriod.Decimals = 0;
			this.zCalcEditPeriod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 54, true);
			this.zCalcEditPeriod.Name = "zCalcEditPeriod";
			this.zCalcEditPeriod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.zCalcEditPeriod.TabIndex = 4;
			this.zCalcEditPeriod.Text = "0";
			this.zCalcEditPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBoxInvocation
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxInvocation, "InvocationReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.FallbackConfiguration)(null)).InvocationReason)));
			this.zTextBoxInvocation.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FallbackControl|6FF79D0B-3982-4F7C-9C24-A1BA26E4FE1F", "Invocation");
			this.zTextBoxInvocation.IsDynamicMultiline = true;
			this.zTextBoxInvocation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 81, true);
			this.zTextBoxInvocation.Multiline = true;
			this.zTextBoxInvocation.Name = "zTextBoxInvocation";
			this.zTextBoxInvocation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 30, true);
			this.zTextBoxInvocation.TabIndex = 5;
			// 
			// zTextBoxRevocation
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxRevocation, "RevocationReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.FallbackConfiguration)(null)).RevocationReason)));
			this.zTextBoxRevocation.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FallbackControl|2C51F460-D409-410E-9073-1A037D6C9EA4", "Revocation");
			this.zTextBoxRevocation.IsDynamicMultiline = true;
			this.zTextBoxRevocation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 116, true);
			this.zTextBoxRevocation.Multiline = true;
			this.zTextBoxRevocation.Name = "zTextBoxRevocation";
			this.zTextBoxRevocation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 30, true);
			this.zTextBoxRevocation.TabIndex = 6;
			// 
			// zDateEditStart
			// 
			this.zDateEditStart.AllowDrop = true;
			this.zDateEditStart.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEditStart, "Start");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.FallbackConfiguration)(null)).Start)));
			this.zDateEditStart.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FallbackControl|A8869D18-A2CD-489A-90E1-D0FF0413D37B", "Start");
			this.zDateEditStart.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditStart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 27, true);
			this.zDateEditStart.Name = "zDateEditStart";
			this.zDateEditStart.TabIndex = 1;
			// 
			// zCalcEditCount
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditCount, "RegularisationCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NL.Business.FallbackConfiguration)(null)).RegularisationCount)));
			this.zCalcEditCount.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FallbackControl|F16A57B3-0A7F-4348-AF1F-B7AB0C3689CA", "Count");
			this.zCalcEditCount.DecimalPlaces = 2;
			this.zCalcEditCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 150, true);
			this.zCalcEditCount.Name = "zCalcEditCount";
			this.zCalcEditCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEditCount.TabIndex = 7;
			this.zCalcEditCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEditCount.Visible = false;
			// 
			// zCalcEditBatchSize
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditBatchSize, "RegularisationBatchSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NL.Business.FallbackConfiguration)(null)).RegularisationBatchSize)));
			this.zCalcEditBatchSize.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FallbackControl|13E0C50B-A255-4E38-8DD6-C54AA1E75864", "Size");
			this.zCalcEditBatchSize.DecimalPlaces = 2;
			this.zCalcEditBatchSize.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 174, true);
			this.zCalcEditBatchSize.Name = "zCalcEditBatchSize";
			this.zCalcEditBatchSize.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEditBatchSize.TabIndex = 8;
			this.zCalcEditBatchSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEditBatchSize.Visible = false;
			// 
			// FallbackControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zfallbackGroupbox);
			this.Name = "FallbackControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zfallbackGroupbox.ResumeLayout(false);
			this.zfallbackGroupbox.PerformLayout();
			this.zDateEditEnd.ResumeLayout(true);
			this.zDateEditEnd.PerformLayout();
			this.zDateEditRegularisation.ResumeLayout(true);
			this.zDateEditRegularisation.PerformLayout();
			this.zDateEditStart.ResumeLayout(true);
			this.zDateEditStart.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zfallbackGroupbox;
		private ZArchitecture.GUI.ZDateEdit zDateEditEnd;
		private ZArchitecture.GUI.ZDateEdit zDateEditRegularisation;
		private ZArchitecture.ZCalcEdit zCalcEditPeriod;
		private ZArchitecture.ZTextBox zTextBoxInvocation;
		private ZArchitecture.ZTextBox zTextBoxRevocation;
		private ZArchitecture.GUI.ZDateEdit zDateEditStart;
		private ZArchitecture.ZCalcEdit zCalcEditCount;
		private ZArchitecture.ZCalcEdit zCalcEditBatchSize;
	}
}
