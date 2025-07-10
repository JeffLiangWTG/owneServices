using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class LoadEntryForm : ZTemplateForm, INotifications, ICustomerServiceMenuSectionCodeOverridable
	{
		public LoadEntryForm(WhsLoad load)
			: base(load)
		{
			InitializeComponent();

			if (ControllerID == null)
			{
				ControllerID = ControllerIDs.WhsLoad;
			}

			WorkflowTabPage.Initialize(load);

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		#region FormCaption

		public override string FormCaption => BusinessEntity?.HumanReadableName.ToString() ?? base.FormCaption;

		#endregion

		#region ZForm Overloads

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		#endregion

		#region SectionCode

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode => ModuleTreeCustomerServiceMenuSectionList.Codes.Warehouse;

		#endregion

		#region Properties

		#region IsResizableByTabPageAllowed

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		WhsLoad WarehouseLoad => (WhsLoad)BusinessEntity;

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		void WarehouseLoadGlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			GlowLinksHelper.OpenEnityInGlow(this, ProductWarehouseGlowLinkAliases.ViewProductWarehouseLoad, WarehouseLoad);
		}
	}
}
