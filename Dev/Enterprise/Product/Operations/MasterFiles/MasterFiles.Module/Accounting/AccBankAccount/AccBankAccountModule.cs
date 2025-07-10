using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccBankAccountModule : ZFilterGridModule
	{
		public AccBankAccountModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccBankAccount; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccBankAccount);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccBankAccountFilterControl(GridCollection, (AccBankAccountFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccBankAccountFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.BankAccounts;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			var accountTypes = AccBankAccountLookups.GetBankAccountTypesList();
			foreach (CodeDescriptionPair accountType in accountTypes)
			{
				var code = accountType.Code;
				var description = ResString.GetMultilingualString("1FC18C7F-1482-448B-9220-73A42BAE1070", "New {0}", accountType.Description);
				NewMenuItem.MenuItems.Add(new ZMenuItem(description, new EventHandler((sender, e) => HandleNewBankAccount(sender, e, code))));
			}

			return menuItems.ToArray();
		}

		protected void HandleNewBankAccount(object sender, EventArgs e, ZString typeCode)
		{
			var controller = (AccBankAccountController)ZControllerFactory.Create(ControllerIDs.AccBankAccount);
			controller.ShowNewForm(typeCode);
		}
	}
}
