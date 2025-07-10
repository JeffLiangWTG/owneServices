using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccOrgTaxConfigurationTemplateModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.AccOrgTaxConfigurationTemplate;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.TaxConfigurationTemplate;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccOrgTaxConfigurationTemplate);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccOrgTaxConfigurationTemplateFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccOrgTaxConfigurationTemplateFilterControl(GridCollection, (AccOrgTaxConfigurationTemplateFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccOrgTaxConfigurationTemplateCollection(Factory);
		}

		protected override bool CanBeCopied()
		{
			return true;
		}

		#region GetNewMenuItems

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			menuItems.Add(
				new ZMenuItem(ResString.GetMultilingualString("7D79A325-405A-4810-956D-6FEA94C91B25", "Apply Template"),
				delegate
				{
					ApplyTemplate(GetSelectedBusinessObjects()
						.Where(template => template.IsInDatabase)
						.OfType<AccOrgTaxConfigurationTemplate>()
						.ToArray()
					);
				}));

			return menuItems.ToArray();
		}

		void ApplyTemplate(AccOrgTaxConfigurationTemplate[] templates)
		{
			if (!templates.Any())
			{
				Globals.Message.Show(Res.GetString("F3504396-6956-4289-951A-AB4A8EEADEFD", "Please select a Tax Configuration Template"));
			}
			else if (templates.Length > 1)
			{
				Globals.Message.Show(Res.GetString("52732313-9C15-4623-8747-3585A1048065", "Please only select one Tax Configuration Template"));
			}
			else
			{
				var template = templates.First();
				var propMsg = Res.GetString(
					"687D05EE-0976-4528-9C49-46BC90280776",
					@"Applying this Tax Configuration Template will update and re-default the Tax Configurations of all attached Organizations.
Do you want to proceed ? Select Yes to re-default Tax Configurations on all attached organizations.Select No to cancel this action.");
				var result = Globals.Message.Show(
					propMsg,
					Res.GetString("88088495-ECD0-4156-9042-FE7608B5DF64", "Warning"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					using (new ZWaitCursorChanger())
					{
						TemplateDataHelper.UpdateTaxConfigurationsForMultipleOrgnizations(template.PK.ToGuid());
					}
					Globals.Message.ShowInformation(Res.GetString("A9D7D0CD-F113-4950-87B7-60AE8C57C9C7", "Successfully apply template tax configuration to linked organizations"));
				}
			}
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = base.GetNewStandardMenuItems();
			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("AccOrgTaxConfigurationTemplateModule|c57925d8-24af-44f4-9ede-0827f37b6b4d", "New A/R - Receivables Organizations Template"), delegate
			{ ShowNewFormForTemplateType(); }));
			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("AccOrgTaxConfigurationTemplateModule|92d08dbd-9d31-480f-8d74-48c47df001fb", "New A/P - Payables Organizations Template"), delegate
			{ ShowNewFormForTemplateType(false); }));
			return menuItems;
		}

		void ShowNewFormForTemplateType(bool isReceivable = true)
		{
			var controller = GetNewController();
			var newBizo = controller.Factory.New<AccOrgTaxConfigurationTemplate>();
			newBizo.OCT_IsReceivable = isReceivable;
			controller.ShowFormForNewEntity(newBizo);
		}

		#endregion

		IAccOrgTaxConfigurationTemplateDataHelper TemplateDataHelper => templateHelper ?? (templateHelper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper());
		IAccOrgTaxConfigurationTemplateDataHelper templateHelper;
	}
}
