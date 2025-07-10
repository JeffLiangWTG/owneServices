using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class DeniedPartyScreeningActionsProvider : IDeniedPartyScreeningActionsProvider
	{
		public DeniedPartyScreeningActionsProvider(ZFilterGridModule moduleFilterGrid, List<MenuItem> moduleActionsMenuItem)
		{
			ParentModuleActionsMenuItem = moduleActionsMenuItem ?? throw new ArgumentNullException(nameof(moduleActionsMenuItem));
			ParentModuleFilterGrid = moduleFilterGrid ?? throw new ArgumentNullException(nameof(moduleFilterGrid));

			ComplianceRiskEnabled = ComplianceRiskHelper.CheckIfComplianceRiskEnabled(moduleFilterGrid.TypeOfTopLevelBusinessObject, allowViewType: true);
		}

		public DeniedPartyScreeningActionsProvider(ZForm parentForm, IBusiness parentIBusiness)
		{
			ParentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
			ParentIBusiness = parentIBusiness ?? throw new ArgumentNullException(nameof(parentIBusiness));

			ComplianceRiskEnabled = ComplianceRiskHelper.CheckIfComplianceRiskEnabled(parentIBusiness.GetType(), allowViewType: false);
		}

		public DeniedPartyScreeningActionsProvider(ZForm parentForm)
		{
			ParentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
		}

		public void AddEntitiesMenuItem()
		{
			DeniedPartyScreeningEntities.AddMenuItem(this);
		}

		public void AddJobsMenuItem()
		{
			DeniedPartyScreeningJobs.AddMenuItem(this);
		}

		internal bool ComplianceRiskEnabled { get; private set; }

		internal ZForm ParentForm { get; private set; }

		internal IBusiness ParentIBusiness { get; private set; }

		public List<MenuItem> ParentModuleActionsMenuItem { get; }

		public IZFilterGridModule ParentModuleFilterGrid { get; }

		internal void AddActionMenuItem(MenuItem menuItem)
		{
			if (IsFormActionsMenuItem)
			{
				ZFormMenuStrategy.AddActionsMenuItem(ParentForm, menuItem);
			}
			else if (IsModuleActionsMenuItem)
			{
				ParentModuleActionsMenuItem.Add(menuItem);
			}
		}

		public void AddParentModuleActionsMenuItem(object item)
		{
			ParentModuleActionsMenuItem.Add((MenuItem)item);
		}

		internal void AddinSeparatorActionsMenuItemIfNeeded()
		{
			if (IsFormActionsMenuItem)
			{
				AddinFormSeparatorActionsMenuItemIfNeeded();
			}
			else if (IsModuleActionsMenuItem)
			{
				AddinModuleSeparatorActionsMenuItemIfNeeded();
			}
		}

		internal ZBool IsFormActionsMenuItem => (ParentForm is ZForm);

		internal ZBool IsModuleActionsMenuItem => (ParentModuleFilterGrid is ZFilterGridModule && ParentModuleActionsMenuItem is List<MenuItem>);

		public ZBool ModuleHasSelectedBusinessObjectsWithShowMessage()
		{
			var result = ParentModuleFilterGrid.GetSelectedBusinessObjects().Length > 0;
			if (!result)
			{
				Globals.Message.Show(Res.GetString("AE2796FE-F528-49A5-9665-06BA6E530CD3", "Please select at least 1 row to Process."));
			}
			return result;
		}

		internal ModuleIdentifier GetModuleIdentifier => ((Core.Modules.INamedModule)ParentModuleFilterGrid).ModuleID;

		internal ZString GetFormIdentifier => ParentForm.BusinessEntity.TableName;

		internal Form GetParentForm() => IsFormActionsMenuItem ? ParentForm : ((ZFilterGridModule)ParentModuleFilterGrid).LocateMainForm();

		void AddinFormSeparatorActionsMenuItemIfNeeded()
		{
			var menuItems = ((IFileMenuItemsProvider)ParentForm).ActionsMenuItem.MenuItems;

			if (menuItems.Count > 0 && IsMenuItemTextValid(menuItems[menuItems.Count - 1].Text))
			{
				ZFormMenuStrategy.AddActionsMenuItem(ParentForm, ZMenuItem.Separator, null);
			}
		}

		void AddinModuleSeparatorActionsMenuItemIfNeeded()
		{
			if (ParentModuleActionsMenuItem.Count > 0 && IsMenuItemTextValid(ParentModuleActionsMenuItem.Last().Text))
			{
				ParentModuleActionsMenuItem.Add(new ZMenuItem(ZMenuItem.Separator));
			}
		}

		bool IsMenuItemTextValid(string text) => text != ZMenuItem.Separator && !string.IsNullOrEmpty(text);
	}
}
