#define CODE_ANALYSIS
using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class RefCusTariffModule : ZFilterGridModule, ITariffViewFilterDataSupporter
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.Universal.RefCusTariff;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalTariffs;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefCusTariff);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefCusTariffFilterStripBusinessObject(GridCollection as ChildTariffViewCollection);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefCusTariffFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new TariffViewCollection(Factory);
		}

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			IZForm form = null;
			var tariff = selectedBusinessObject as TariffView;
			if (tariff != null && tariff.ZZ1_IsSystem)
			{
				Globals.Message.ShowError(Res.GetString("35cdc881-e5ec-46fb-bcf4-5d3a65ad22d5", "System defined tariffs cannot be edited."));
			}
			else if (tariff != null && tariff.ZZ1_ZZZ_NKDataGrouping != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				Globals.Message.ShowError(Res.GetString("9b8f9fc9-2abe-4419-b440-7bcd37322a33", "Tariffs that do not belong to your country cannot be edited."));
			}
			else
			{
				form = base.ShowEditForm(selectedBusinessObject);
			}

			return form;
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			IZForm form = null;
			var tariff = selectedBusinessObject as TariffView;
			if (tariff != null && tariff.ZZ1_IsSystem)
			{
				Globals.Message.ShowError(Res.GetString("16546dab-476d-4f0f-adcf-56ed937be46c", "System defined tariffs cannot be deleted."));
			}
			else if (tariff != null && tariff.ZZ1_ZZZ_NKDataGrouping != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				Globals.Message.ShowError(Res.GetString("04bad765-e1d2-45bc-9401-86f5fab01e15", "Tariffs that do not belong to your country cannot be deleted."));
			}
			else
			{
				form = base.ShowDeleteForm(selectedBusinessObject);
			}

			return form;
		}

		public override bool AllowNew => IsSelfManagedTariffCountry;

		public override bool AllowEdit => IsSelfManagedTariffCountry;

		public override bool AllowDelete => IsSelfManagedTariffCountry;

		public override bool AllowUniversalCopy => false;

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			if (IsSelfManagedTariffCountry)
			{
				AddImportDataMenuItem(Res.GetString("942F5192-3045-4337-844F-623701EF325C", "Tariff From CSV"), ImportTariffFromCSV);
				AddImportDataMenuItem(Res.GetString("410F1B16-3BD6-435B-94FF-3E0605858701", "Rate From CSV"), ImportRateFromCSV);
			}
		}

		public ITariffViewFilterData TariffViewFilterData { get; set; }

		static bool IsSelfManagedTariffCountry => ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsSelfManagedTariffCountry(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		#region Import from CSV

		void ImportRateFromCSV(object sender, EventArgs e)
		{
			new ImportRateFromCSVForm().Show();
		}

		void ImportTariffFromCSV(object sender, EventArgs e)
		{
			new ImportTariffFromCSVForm().Show();
		}

		#endregion
	}
}
