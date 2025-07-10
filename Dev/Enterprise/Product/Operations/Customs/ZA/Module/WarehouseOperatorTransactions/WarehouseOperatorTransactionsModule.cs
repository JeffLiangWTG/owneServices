using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Customs.ZA.GUI;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class WarehouseOperatorTransactionsModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public WarehouseOperatorTransactionsModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		WarehouseOperatorTransactionsModule(WarehouseOperatorTransactionsModule sourceModule)
		{
			isShowMaintenanceMenu = false;
			SetupFindBatchModuleFilters(sourceModule);
		}

		public override ModuleIdentifier ID => ZAModuleIDs.WarehouseOperatorTransactions;

		public override SecurityCheckpoint SecurityCheckpoint => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions);

		public OperationalActionSupporter OperationalActionSupporter => new WarehouseOperatorTransactionsOperationalActionSupporter();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ZAControllerIDs.WarehouseOperatorTransactions);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new WarehouseOperatorTransactionsFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new WarehouseOperatorTransactionsFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ModuleCusWHSOperatorTransactionsCollection(Factory, GlbCompany.CurrentCompany);

		readonly bool isShowMaintenanceMenu = true;

		public override MenuItem[] FormActionMenu
		{
			get
			{
				var result = base.FormActionMenu.ToList();
				if (isShowMaintenanceMenu)
				{
					var menuIndex = result.Count > 0 ? result.Count - 1 : 0;
					result.Insert(menuIndex, CreateMaintenanceMenu());
				}
				return result.ToArray();
			}
		}

		ZMenuItem CreateMaintenanceMenu()
		{
			var result = new ZMenuItem(Res.GetData("123895A8-8770-4615-9CF9-B631444199CD", "Maintenance"), IconTypes.EditButtonActive, IconTypes.EditButtonRest);
			result.MenuItems.Add(new ZMenuItem(Res.GetData("BE50A9D1-3050-4950-A1DA-5C8F8057F54F", "Cancel Unallocated Order"), ShowCancelOrdersForm));
			if (ZACustomsRegistry.Instance.AllowReversalOfCusWarehouseBatch.Value)
			{
				result.MenuItems.Add(new ZMenuItem(Res.GetData("325E8550-D02C-42FF-AB58-6FA749220C3D", "Reverse a Batch"), ShowReverseBatchPopup));
			}
			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				result.MenuItems.Add(new ZMenuItem(Res.GetData("B0EF642F-7C7E-4762-A1E4-FD8BF10BFFDC", "Reset Receipts for a Batch"), ShowResetBatchReceiptPopup));
			}
			return result;
		}

		void ShowCancelOrdersForm(object sender, EventArgs e)
		{
			VaildateFiltersAndShowForm(() => CancelOrdersForm.ShowDialog(Factory, WarehouseFilter.Property, ProductOwnerFilter.Property));
		}

		void ShowReverseBatchPopup(object sender, EventArgs e)
		{
			VaildateFiltersAndShowForm(() => FindBatchModulePopup.ShowDialog(new WarehouseOperatorTransactionsModule(this), FindBatchModulePopup.Operation.ReverseBatch));
		}

		void ShowResetBatchReceiptPopup(object sender, EventArgs e)
		{
			VaildateFiltersAndShowForm(() => FindBatchModulePopup.ShowDialog(new WarehouseOperatorTransactionsModule(this), FindBatchModulePopup.Operation.ResetBatchReceipt));
		}

		void VaildateFiltersAndShowForm(Action showFormAction)
		{
			if (ValidateFilters())
			{
				showFormAction();
			}
			else
			{
				ShowFilterValidationError();
			}
		}

		void ShowFilterValidationError()
		{
			Globals.Message.ShowError(Res.GetString("00CDA602-DD90-4635-86DD-8C82F33AFA7B", "There are errors that need to be corrected before this operation can be performed."));
		}

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		protected override void PostProcessActionMenuItems(MenuItem[] actionMenuItems)
		{
			var operationalActionMenu = actionMenuItems.Where(x => ((ZMenuItem)x).Caption == "Operational Actions").FirstOrDefault();
			if (operationalActionMenu != null)
			{
				operationalActionMenu.Enabled = ValidateFilters();
			}
		}

		ModuleGuidFilter WarehouseFilter => (ModuleGuidFilter)FilterBusinessObject[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.Warehouse];

		ModuleGuidFilter ProductOwnerFilter => (ModuleGuidFilter)FilterBusinessObject[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.ProductOwner];

		bool ValidateFilters()
		{
			var result = true;
			var requiredModuleGuidFilters = new[] { WarehouseFilter, ProductOwnerFilter };

			foreach (var requiredFilter in requiredModuleGuidFilters)
			{
				requiredFilter.Validation.ValidateProperty();

				if (requiredFilter.PropertyInfo.HasErrors())
				{
					result = false;
				}
			}

			return result;
		}

		void SetupFindBatchModuleFilters(WarehouseOperatorTransactionsModule sourceModule)
		{
			WarehouseFilter.Property = sourceModule.WarehouseFilter.Property;
			WarehouseFilter.ReadOnly = true;
			ProductOwnerFilter.Property = sourceModule.ProductOwnerFilter.Property;
			ProductOwnerFilter.ReadOnly = true;

			var requiredFilters = new ZString[]
			{
				WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.BatchNumber,
				WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.Warehouse,
				WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.ProductOwner
			};
			foreach (var filter in FilterBusinessObject.ModuleFilters.ToArray())
			{
				if (!requiredFilters.Contains(filter.Description) && filter.Category != FilterCategories.AuditInformation)
				{
					FilterBusinessObject.ModuleFilters.RemoveFilter(filter);
				}
			}
		}
	}
}
