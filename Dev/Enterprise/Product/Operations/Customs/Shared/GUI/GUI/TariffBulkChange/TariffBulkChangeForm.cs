using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TariffBulkChangeForm : ZChildForm
	{
		public TariffBulkChangeForm(TariffBulkChange businessEntity, bool automaticConvert)
			: base(businessEntity)
		{
			this.automaticConvert = automaticConvert;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			DisableNewAction();
		}
		readonly bool automaticConvert;

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			BusinessEntity.HasChanges = true;
		}
		public new TariffBulkChange BusinessEntity
		{
			get { return base.BusinessEntity as TariffBulkChange; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("f0a4e197-f24a-4d49-a79a-c512ca598b68", "Tariff Bulk Change"); }
		}

		void RefreshGrids()
		{
			int oldGridPosi = OldTariffsZGrid.ListManager.Position;
			int newGridPosi = NewTariffsZGrid.ListManager.Position;
			OldTariffsZGrid.ListManager.Position = oldGridPosi < 1 ? 1 : 0;
			OldTariffsZGrid.ListManager.Position = oldGridPosi;
			NewTariffsZGrid.ListManager.Position = newGridPosi;
		}

		public static string UpdateOriginalSelectionErrorText
		{
			get { return Res.GetString("731cf23c-296f-496c-81ac-440ea9e0728d", "Please select one or more Original Lookups (use Ctrl + Right Click to select a row) and one (only) New Tariff."); }
		}

		public static string UpdateOriginalLookupsSelectionErrorText
		{
			get { return Res.GetString("b2f195d8-40d9-47f0-b7a2-8dc53cbdf63e", "Update Original Lookups Selection Error"); }
		}

		void UpdateOriginalLookups(object sender, EventArgs e)
		{
			if (OldClassificationsZGrid.SelectedElements.Length < 1 || NewTariffsZGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(UpdateOriginalSelectionErrorText, UpdateOriginalLookupsSelectionErrorText);
			}
			else
			{
				BusinessEntity.UpdateOriginalLookups(OldTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeOldTariff,
						NewTariffsZGrid.SelectedElements[0] as TariffBulkChange.TariffBulkChangeNewTariff,
						OldClassificationsZGrid.SelectedElements);
				RefreshGrids();
			}
		}

		public static string UpdateProductsSelectionErrorText
		{
			get { return Res.GetString("fd5d685f-7082-4f3b-a3ad-90b0f56d1da7", "Please select one or more Products (use Ctrl + Right Click to select a row) and one (only) New Lookup."); }
		}

		public static string UpdateProductsGridErrorsText
		{
			get { return Res.GetString("d440e1de-73ee-470c-80c1-51bac2b1cd29", "Please correct error on New Classifications."); }
		}

		public static string UpdateProductsSelectionErrorCaption
		{
			get { return Res.GetString("5bb78631-cac6-4de7-8623-0dd86f222cc7", "Update Products Selection Error"); }
		}

		public static string UpdateProductsGridErrorsCaption
		{
			get { return Res.GetString("27373992-de04-4436-b0d8-3e9db1e2c830", "Update Products Grid Error"); }
		}

		void UpdateProductsZButton_Click(object sender, EventArgs e)
		{
			if (PartsZGrid.SelectedElements.Length < 1 || NewClassificationsZGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(UpdateProductsSelectionErrorText, UpdateProductsSelectionErrorCaption);
			}
			else
			{
				if (NewClassificationsZGrid.SelectedElements[0].HasErrors)
				{
					Globals.Message.ShowError(UpdateProductsGridErrorsText, UpdateProductsGridErrorsCaption);
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

		public static string MakeNewClassificationsErrorText
		{
			get { return Res.GetString("ca66c11e-a72e-46b1-954f-4bc59e8a3d5c", "Please select one or more New Tariffs (use Ctrl + Right Click to select a row)."); }
		}
		public static string MakeNewClassificationsSelectionErrorText
		{
			get { return Res.GetString("144de8c6-9100-469c-b835-c7a60f423f09", "Make Classifications Selection Error"); }
		}

		void MakeNewClassZButton_Click(object sender, EventArgs e)
		{
			if (NewTariffsZGrid.SelectedElements.Length < 1)
			{
				Globals.Message.ShowError(MakeNewClassificationsErrorText, MakeNewClassificationsSelectionErrorText);
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
				result = BusinessEntity.AdditionalContinueWithSave(automaticConvert);
			}
			return result;
		}
	}
}
