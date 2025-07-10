using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class OrgSupplierPartFormCustomsControl : Customs.GUI.OrgSupplierPartFormCustomsControl
	{
		public OrgSupplierPartFormCustomsControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (PivotGrid.ListManager != null)
			{
				PivotGrid.ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
				ListManager_CurrentChanged(this, new EventArgs());
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (currentPivot != null && !currentPivot.IsDeleted)
			{
				currentPivot.CI_ChildTypeInfo.ValueChanged -= new EventHandler(CI_ChildTypeInfo_ValueChanged);
				currentPivot.OnTariffTypeChanging -= pivot_OnTariffTypeChanging;
			}

			if (PivotGrid.ListManager != null && PivotGrid.CurrentRowIndex >= 0)
			{
				currentPivot = (CusClassPartPivot)PivotGrid.ListManager.GetCurrent();

				if (currentPivot != null && !currentPivot.IsDeleted)
				{
					currentPivot.CI_ChildTypeInfo.ValueChanged += new EventHandler(CI_ChildTypeInfo_ValueChanged);
					currentPivot.CI_TariffNumInfo.ValueChanged += new EventHandler(CI_ChildTypeInfo_ValueChanged);
					currentPivot.OnTariffTypeChanging += pivot_OnTariffTypeChanging;
				}
			}

			SetVisibility();
		}

		CusClassPartPivot currentPivot;

		void pivot_OnTariffTypeChanging(object sender, System.ComponentModel.CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("4A526265-4340-425C-921C-DBC406DA1C4D", "You are changing the tariff type from Import to Export.All associated Import PGA data will be removed.\r\nDo you wish to proceed?"), Res.GetString("63A95FC1-23E5-4209-BD73-DDE4EE0364E4", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		void CI_ChildTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetVisibility();
		}

		void SetVisibility()
		{
			var isHTIPivotRelated = currentPivot != null && !currentPivot.IsDeleted && currentPivot.IsImportClassification;
			var isExportPivotRelated = currentPivot != null && !currentPivot.IsDeleted && currentPivot.IsExportTariff;
			importClassificationUserControl.CurrentPivot = isHTIPivotRelated ? currentPivot : null;
			importClassificationUserControl.Visible = isHTIPivotRelated;
			exportClassificationUserControl.CurrentPivot = isExportPivotRelated ? currentPivot : null;
			exportClassificationUserControl.Visible = isExportPivotRelated;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (PivotGrid != null && PivotGrid.ListManager != null)
				{
					PivotGrid.ListManager.CurrentChanged -= new EventHandler(ListManager_CurrentChanged);
				}

				if (currentPivot != null && !currentPivot.IsDeleted)
				{
					currentPivot.CI_ChildTypeInfo.ValueChanged -= new EventHandler(CI_ChildTypeInfo_ValueChanged);
				}
			}
			base.Dispose(disposing);
		}

		#region Click Events

		void pivotGrid_Click(object sender, EventArgs e)
		{
			CusClassPartPivot pivot = null;

			if (PivotGrid.ListManager != null && PivotGrid.CurrentRowIndex >= 0)
			{
				pivot = (CusClassPartPivot)PivotGrid.ListManager.GetCurrent();
			}

			if (pivot == null)
			{
				Globals.Message.ShowInformation("Please select (highlight) an Import Classification line.", "Edit Import Classification Line");
			}
			else if (pivot.CI_ChildType != ClassificationTypeList.Codes.HTI)
			{
				Globals.Message.ShowInformation("Only Import Classifications may have multiple classification (tariff) lines.", "Edit Import Classification Line");
			}
			else
			{
				using (Form form = new PartImportClassificationForm(pivot))
				{
					form.ShowDialog();
				}
			}
		}

		#endregion

	}
}
