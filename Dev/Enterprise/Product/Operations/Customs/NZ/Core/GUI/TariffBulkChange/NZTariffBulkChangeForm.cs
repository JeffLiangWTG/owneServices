using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	public partial class NZTariffBulkChangeForm : ZChildForm
	{
		public NZTariffBulkChangeForm(NZTariffBulkChange businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			DisableNewAction();
			OldTariffsZGrid.GridId = "GridLayoutKX4qV9Ye2v3cyDdknN+4+Q==";
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			BusinessEntity.HasChanges = true;
		}
		public new NZTariffBulkChange BusinessEntity
		{
			get { return base.BusinessEntity as NZTariffBulkChange; }
		}

		public override string FormCaption
		{
			get { return Enterprise.Customs.NZ.GUI.Res.GetString("8120EA39-1FA9-44B1-AC16-FEB80A5C4DD0", "Tariff Bulk Change"); }
		}

		void RefreshGrids()
		{
			int oldGridPosi = OldTariffsZGrid.ListManager.Position;
			int newGridPosi = NewTariffsZGrid.ListManager.Position;
			OldTariffsZGrid.ListManager.Position = oldGridPosi < 1 ? 1 : 0;
			OldTariffsZGrid.ListManager.Position = oldGridPosi;
			NewTariffsZGrid.ListManager.Position = newGridPosi;
		}

		public const string UpdateOriginalSelectionErrorText = "Please select one or more Original Lookups (use Ctrl + Right Click to select a row) and one (only) New Tariff.";
		void UpdateOriginalLookups(object sender, EventArgs e)
		{
			if (OldClassificationsZGrid.SelectedElements.Length < 1 || NewTariffsZGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(UpdateOriginalSelectionErrorText, "Update Original Lookups Selection Error");
			}
			else
			{
				BusinessEntity.UpdateOriginalLookups(OldTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeOldTariff,
						NewTariffsZGrid.SelectedElements[0] as TariffBulkChange.TariffBulkChangeNewTariff,
						OldClassificationsZGrid.SelectedElements);
				RefreshGrids();
			}
		}

		public const string UpdateProductsSelectionErrorText = "Please select one or more Products (use Ctrl + Right Click to select a row) and one (only) New Lookup.";
		public const string UpdateProductsGridErrorsText = "Please correct error on New Classifications.";
		void UpdateProductsZButton_Click(object sender, EventArgs e)
		{
			if (PartsZGrid.SelectedElements.Length < 1 || NewClassificationsZGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(UpdateProductsSelectionErrorText, "Update Products Selection Error");
			}
			else
			{
				if (NewClassificationsZGrid.SelectedElements[0].HasErrors)
				{
					Globals.Message.ShowError(UpdateProductsGridErrorsText, "Update Products Grid Error");
				}
				else
				{
					BusinessEntity.UpdateProducts(OldTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeOldTariff,
								NewClassificationsZGrid.SelectedElements[0] as BaseCusClassification, null,
								PartsZGrid.SelectedElements);
					RefreshGrids();
				}
			}
		}

		public const string MakeNewClassificationsErrorText = "Please select one or more New Tariffs (use Ctrl + Right Click to select a row).";
		void MakeNewClassZButton_Click(object sender, EventArgs e)
		{
			if (NewTariffsZGrid.SelectedElements.Length < 1)
			{
				Globals.Message.ShowError(MakeNewClassificationsErrorText, "Make Classifications Selection Error");
			}
			else
			{
				BusinessEntity.MakeNewClassifications(OldTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeOldTariff,
							NewTariffsZGrid.SelectedElements);
				RefreshGrids();
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = BusinessEntity.ApplyAdditionalContinueWithSave();
			}
			return result;
		}
	}
}
