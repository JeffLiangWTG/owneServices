using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccTaxOverrideGroupModule : ZFilterGridModule
	{
		public AccTaxOverrideGroupModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccTaxOverrideGroup; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			if ((selectedBusinessObject is AccTaxOverrideGroup taxGroup) && taxGroup.IsTaxFrameworkRelated)
			{
				return ZControllerFactory.Create(ControllerIDs.TaxFrameworkAccTaxOverrideGroup);
			}

			return ZControllerFactory.Create(ControllerIDs.AccTaxOverrideGroup);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccTaxOverrideGroupFilterControl(GridCollection, (AccTaxOverrideGroupFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccTaxOverrideGroupCollectionCompany(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccTaxOverrideGroupFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TaxOverrideGroups; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Menu Items

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			var menuItem = new ZMenuItem(ResString.GetMultilingualString("45D32981-BB86-448C-A1E5-FD706952D694", "New {0} Tax Override Group", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription), OnNewTaxOverrideGroupMenuItemClick);
			menuItem.DefaultItem = true;
			NewMenuItem.MenuItems.Add(menuItem);

			if (GlbCompany.CurrentCompany.IsEnabledForTaxFrameworkConfiguration(Factory))
			{
				var taxFrameworkTaxOverrideGroupMenuItem = new ZMenuItem(ResString.GetMultilingualString("75A2ACB2-7E17-4FE6-B64F-B595EA5918D4", "New Tax Configuration Override Group"), OnNewTaxFrameworkTaxOverrideGroupMenuItemClick);
				NewMenuItem.MenuItems.Add(taxFrameworkTaxOverrideGroupMenuItem);
			}

			return menuItems.ToArray();
		}

		#endregion

		void OnNewTaxOverrideGroupMenuItemClick(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.AccTaxOverrideGroup);
			controller.ShowNewForm();
		}

		void OnNewTaxFrameworkTaxOverrideGroupMenuItemClick(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.TaxFrameworkAccTaxOverrideGroup);
			controller.ShowNewForm();
		}
	}
}
