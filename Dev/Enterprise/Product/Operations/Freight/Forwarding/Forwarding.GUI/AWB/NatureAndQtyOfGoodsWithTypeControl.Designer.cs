using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	partial class NatureAndQtyOfGoodsWithTypeControl
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
			this.natureAndQtyOfGoodsType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// natureAndQtyOfGoodsDetails
			// 
			this.natureAndQtyOfGoodsDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 0, true);
			this.natureAndQtyOfGoodsDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			// 
			// natureAndQtyOfGoodsType
			// 
			this.natureAndQtyOfGoodsType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.natureAndQtyOfGoodsType, "NatureAndQtyOfGoodsType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsType)));
			this.natureAndQtyOfGoodsType.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f094e884-4dd1-4062-a861-18d3e3434726", "Nature and Quantity of Goods Type");
			this.natureAndQtyOfGoodsType.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.natureAndQtyOfGoodsType, false);
			this.natureAndQtyOfGoodsType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.natureAndQtyOfGoodsType.Name = "natureAndQtyOfGoodsType";
			this.natureAndQtyOfGoodsType.PreBoundMaxLength = 1;
			this.natureAndQtyOfGoodsType.ShowDescriptionBox = false;
			this.natureAndQtyOfGoodsType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.natureAndQtyOfGoodsType.TabIndex = 2;
			// 
			// NatureAndQtyOfGoodsWithTypeControl
			// 
			this.Controls.Add(this.natureAndQtyOfGoodsType);
			this.Name = "NatureAndQtyOfGoodsWithTypeControl";
			this.Controls.SetChildIndex(this.natureAndQtyOfGoodsType, 0);
			this.Controls.SetChildIndex(this.natureAndQtyOfGoodsDetails, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		ZArchitecture.GUI.ZDropEdit natureAndQtyOfGoodsType;

		#endregion
	}
}
