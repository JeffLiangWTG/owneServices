using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class LinkedOrganizationsControl : ZUserControl
	{
		public event EventHandler<ModuleButtonGridOnAttachEventArgs> OnAttached
		{
			add => zModuleButtonGrid.Attached += value;
			remove => zModuleButtonGrid.Attached -= value;
		}

		public event EventHandler<ModuleButtonGridOnAttachEventArgs> BeforeAttach
		{
			add => zModuleButtonGrid.BeforeAttached += value;
			remove => zModuleButtonGrid.BeforeAttached -= value;
		}

		public event ModuleButtonGridOperationCancelEventHandler OnAttaching
		{
			add => zModuleButtonGrid.Attaching += value;
			remove => zModuleButtonGrid.Attaching -= value;
		}

		public event EventHandler<ModuleButtonGridOnDetachedEventArgs> OnDetached
		{
			add => zModuleButtonGrid.Detached += value;
			remove => zModuleButtonGrid.Detached -= value;
		}

		public void ConfigureExportColumnsToExcelMenuItems(bool isEnabled)
		{
			zModuleButtonGrid.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled = isEnabled;
			zModuleButtonGrid.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled = isEnabled;
		}

		public AccOrgTaxConfigurationTemplateLinkedOrganisationCollection BoundedOrgCollection => zModuleButtonGrid.InnerGrid.List as AccOrgTaxConfigurationTemplateLinkedOrganisationCollection;

		public LinkedOrganizationsControl()
		{
			InitializeComponent();
		}
	}
}
