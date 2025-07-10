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
	public class AccGlobalChargeCodeModule : ZFilterGridModule
	{
		public AccGlobalChargeCodeModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccGlobalChargeCode; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccGlobalChargeCode);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccChargeCodeFilterControl(GridCollection, (AccGlobalChargeCodeFilterBusinessObject)FilterBusinessObject, AccChargeCodeFilterControl.Mode.Global);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccGlobalChargeCodeCollection(Factory, new ZQuery());
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccGlobalChargeCodeFilterBusinessObject();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("AccGlobalChargeCodeModule|1acf8908-1596-4127-a642-7d4c9cafaebf", "New Global Charge Code"),
				new EventHandler((s, e) => ShowNewForm())));
			NewMenuItem.MenuItems.Add(new ZMenuItem("-"));
			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("AccGlobalChargeCodeModule|18a5c1de-d3ee-498f-8a2d-b300aadf5ae7", "Consolidate Local Charge Codes"),
				new EventHandler(OpenConsolidateChargeCodeForm)));
			return menuItems.ToArray();
		}

		void OpenConsolidateChargeCodeForm(object sender, EventArgs e)
		{
			if (Env.Security.GlobalChargeCodesNew.IsAllowed)
			{
				new ConsolidateChargeCodeForm(Factory).Show();
			}
			else
			{
				Env.Security.GlobalChargeCodesNew.ShowError();
			}
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GlobalChargeCodes; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
