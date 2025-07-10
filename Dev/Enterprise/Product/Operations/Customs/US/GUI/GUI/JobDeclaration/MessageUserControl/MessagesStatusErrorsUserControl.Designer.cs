using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class MessagesStatusErrorsUserControl
	{
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.StatusesAndErrorsGrid = new ZArchitecture.ZGrid();
			this.blockInterpretationGroupBox = new ZGroupBox();
			this.blockTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StatusesAndErrorsGrid)).BeginInit();
			this.StatusesAndErrorsGrid.SuspendLayout();
			this.blockInterpretationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.StatusErrorsDataViewCollection);
			// 
			// StatusesAndErrorsGrid
			// 
			this.StatusesAndErrorsGrid.AllowNavigation = false;
			this.StatusesAndErrorsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusesAndErrorsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Messaging.Business.ErrorsRecord)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.ErrorsRecord)(null)).LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.ErrorsRecord)(null)).TariffNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.ErrorsRecord)(null)).PGAAgencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.ErrorsRecord)(null)).PGALine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.ErrorsRecord)(null)).ErrorMessageIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.ErrorsRecord)(null)).NarrativeMessage)));
			this.StatusesAndErrorsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Line No.";
			zTextBoxColumnStyleInfo1.ColumnName = "LineNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.Caption = "Tariff";
			zTextBoxColumnStyleInfo2.ColumnName = "TariffNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4620713a-211d-4590-8095-8d7a6862f720", "PGA");
			zTextBoxColumnStyleInfo3.ColumnName = "PGAAgencyCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("52a381d9-08ff-425d-8fe9-6c8da1bb9f2f", "PGA Line");
			zTextBoxColumnStyleInfo4.ColumnName = "PGALine";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Error/Status Code";
			zTextBoxColumnStyleInfo5.ColumnName = "ErrorMessageIdentifier";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.Caption = "Narrative";
			zTextBoxColumnStyleInfo6.ColumnName = "NarrativeMessage";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(235);
			this.StatusesAndErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StatusesAndErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StatusesAndErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.StatusesAndErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.StatusesAndErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.StatusesAndErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.StatusesAndErrorsGrid.CopySelectedRowsAllowed = true;
			this.StatusesAndErrorsGrid.GridId = "c5cadbc9-4682-4de8-b609-b802611e85e7";
			this.StatusesAndErrorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StatusesAndErrorsGrid.LayoutKey = "StatusesAndErrorsGrid";
			this.StatusesAndErrorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 1, true);
			this.StatusesAndErrorsGrid.Name = "StatusesAndErrorsGrid";
			this.StatusesAndErrorsGrid.ReadOnly = true;
			this.StatusesAndErrorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 195, true);
			this.StatusesAndErrorsGrid.TabIndex = 19;
			// 
			// BlockInterpretationGroupBox
			// 
			this.blockInterpretationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.blockInterpretationGroupBox.Controls.Add(this.blockTextBox);
			this.blockInterpretationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 200, true);
			this.blockInterpretationGroupBox.Name = "BlockInterpretationGroupBox";
			this.blockInterpretationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 257, true);
			this.blockInterpretationGroupBox.TabIndex = 20;
			this.blockInterpretationGroupBox.TabStop = false;
			this.blockInterpretationGroupBox.Text = "Message Block Text";
			// 
			// BlockTextBox
			// 
			this.BindingSource.SetBindingMember(this.blockTextBox, "BlockText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.ErrorsRecord)(null)).BlockText)));
			this.blockTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.blockTextBox, false);
			this.blockTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.blockTextBox.Multiline = true;
			this.blockTextBox.Name = "BlockTextBox";
			this.blockTextBox.ReadOnly = true;
			this.blockTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.blockTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 238, true);
			this.blockTextBox.TabIndex = 1;
			this.blockTextBox.WordWrap = false;
			// 
			// MessagesStatusErrorsUserControl
			// 
			this.Controls.Add(this.StatusesAndErrorsGrid);
			this.Controls.Add(this.blockInterpretationGroupBox);
			this.Name = "MessagesStatusErrorsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 460, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 330, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StatusesAndErrorsGrid)).EndInit();
			this.StatusesAndErrorsGrid.ResumeLayout(false);
			this.StatusesAndErrorsGrid.PerformLayout();
			this.blockInterpretationGroupBox.ResumeLayout(false);
			this.blockInterpretationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.ZGrid StatusesAndErrorsGrid;
		private ZGroupBox blockInterpretationGroupBox;
		private ZArchitecture.ZTextBox blockTextBox;
	}
}
