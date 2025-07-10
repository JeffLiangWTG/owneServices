using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class ACEFDAEditForm : ZChildForm
	{
		public ACEFDAEditForm()
		{
		}

		public ACEFDAEditForm(ACEFDA header)
			: base(header)
		{
		}

		public override string FormCaption
		{
			get
			{
				var result = base.FormCaption;
				if (FDA != null && FDA.InvoiceLine != null)
				{
					result = result + ": Invoice: " + FDA.InvoiceLine.InvoiceNumber + ", LNO: " + FDA.InvoiceLine.JI_LineNo.ToString();
				}
				return result;
			}
		}

		ACEFDA FDA
		{
			get { return (ACEFDA)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			UpdateInchPanelVisibility();
			UpdateUS_ContainerDim1CalcEditCaption();
			if (FDA != null)
			{
				FDA.US_ContainerDimTypeInfo.ValueChanged += DimType_ValueChanged;
				FDA.US_ProgramCodeInfo.ValueChanged += US_ProgramCodeInfo_ValueChanged;
				FDA.US_DimUQInfo.ValueChanged += US_DimUQInfo_ValueChanged;
			}
			SetItemIdentityNumberVisible();
			SetAOCGridAndLicensesGridVisible();

			LicenseGroupBox.AllowOverlap(ContainerDimensionsGroupBox);
		}

		void US_ProgramCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetItemIdentityNumberVisible();
		}

		void SetItemIdentityNumberVisible()
		{
			var isItemIdentityVisible = FDA != null && FDA.US_ProgramCode == FDAProgramCodeList.Codes.RAD;
			this.ItemIdentityNumberTextBox.Visible = isItemIdentityVisible;
			this.ItemNumberQualifierTextBoxDropEdit.Visible = isItemIdentityVisible;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var fda = FDA;
				if (fda != null)
				{
					fda.US_ContainerDimTypeInfo.ValueChanged -= DimType_ValueChanged;
					fda.US_ProgramCodeInfo.ValueChanged -= US_ProgramCodeInfo_ValueChanged;
					fda.US_DimUQInfo.ValueChanged -= US_DimUQInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		void US_DimUQInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateInchPanelVisibility();
		}

		void UpdateInchPanelVisibility()
		{
			if (FDA != null && FDA.US_DimUQ == FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals)
			{
				InchPanel.Visible = true;
			}
			else
			{
				InchPanel.Visible = false;
			}
		}

		void DimType_ValueChanged(object sender, EventArgs e)
		{
			UpdateUS_ContainerDim1CalcEditCaption();
		}

		void UpdateUS_ContainerDim1CalcEditCaption()
		{
			if (FDA != null)
			{
				US_ContainerDim1CalcEdit.CaptionResourceString = FDA.US_ContainerDimType == CylindricalRectangularList.Codes.Cylindrical ? Res.GetData("69759E5A-16FF-41BC-9C22-BE3BEFBB755B", "Diameter") : Res.GetData("31415141-0AEE-4CF2-B801-93DC85039ECA", "Width");
				US_ContainerDim1CalcEdit.UpdateCaption();

				CanDim1InchCalcEdit.CaptionResourceString = FDA.US_ContainerDimType == CylindricalRectangularList.Codes.Cylindrical ? Res.GetData("69759E5A-16FF-41BC-9C22-BE3BEFBB755B", "Diameter") : Res.GetData("31415141-0AEE-4CF2-B801-93DC85039ECA", "Width");
				CanDim1InchCalcEdit.UpdateCaption();
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		#region Control Visible for Product

		void SetAOCGridAndLicensesGridVisible()
		{
			if (FDA != null)
			{
				if (FDA.Parent is CusClassPartPivot)
				{
					this.AOCGrid.Visible = true;
					this.LicensesGrid.Visible = false;
					this.LicenseGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3D1977B8-22BB-4C58-A354-D2A41496DC3D", "Affirmation Of Compliance");
					this.ForcePNCheckBox.Visible = false;
					this.PNDisclCheckBox.Visible = false;
					this.PNConfNoTextBox.Visible = false;

					this.FoodFacRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 112, true);
					this.ExemptDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(767, 112, true);

					this.ItemNumberQualifierTextBoxDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 138, true);
					this.ItemIdentityNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 138, true);

					this.PackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 171, true);

					this.ActiveIngredientGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 227, true);

					this.LicenseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 360, true);
					this.LicenseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 110, true);
				}
				else
				{
					this.AOCGrid.Visible = false;
					this.LicensesGrid.Visible = true;
					this.LicenseGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("057ADF3C-28B8-4724-89BF-2C0032985E0B", "Privately Owned Vehicle License Information");
					this.ForcePNCheckBox.Visible = true;
					this.PNDisclCheckBox.Visible = true;
					this.PNConfNoTextBox.Visible = true;
				}
			}
		}

		#endregion
	}
}
