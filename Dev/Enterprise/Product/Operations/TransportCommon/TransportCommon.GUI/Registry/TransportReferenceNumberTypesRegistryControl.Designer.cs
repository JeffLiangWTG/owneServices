using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.TransportCommon.Registry;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class TransportReferenceNumberTypesRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.transportReferenceNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.transportReferenceNumbersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TransportReferenceNumberTypeCollection);
			// 
			// transportReferenceNumbersGrid
			// 
			this.transportReferenceNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.transportReferenceNumbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((TransportReferenceNumberType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TransportReferenceNumberType)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TransportReferenceNumberType)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((TransportReferenceNumberType)(null)).IsUnique)));
			this.transportReferenceNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("TransportReferenceNumberTypesRegistryControl|AB63D1E2-447A-4608-B4DF-1424C52D4273", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("TransportReferenceNumberTypesRegistryControl|7880F979-8A49-4153-8FA4-8A9CBD033FC5", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("TransportReferenceNumberTypesRegistryControl|C8DE5DD9-27C3-4304-88F8-8C53248CA2F8", "Is Unique");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsUnique";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.transportReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.transportReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.transportReferenceNumbersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.transportReferenceNumbersGrid.GridId = "7F852C5B-D8F5-4A73-A08E-E0BF87690142";
			this.transportReferenceNumbersGrid.CopySelectedRowsAllowed = true;
			this.transportReferenceNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.transportReferenceNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.transportReferenceNumbersGrid.LayoutKey = "transportReferenceNumbersGrid";
			this.transportReferenceNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.transportReferenceNumbersGrid.Name = "transportReferenceNumbersGrid";
			this.transportReferenceNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			this.transportReferenceNumbersGrid.TabIndex = 0;
			// 
			// TransportReferenceNumberTypesRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.transportReferenceNumbersGrid);
			this.Name = "TransportReferenceNumberTypesRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.transportReferenceNumbersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid transportReferenceNumbersGrid;
	}
}
