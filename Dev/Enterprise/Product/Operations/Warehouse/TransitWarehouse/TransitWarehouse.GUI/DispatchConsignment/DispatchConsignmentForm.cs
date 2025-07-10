using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.GUI
{
	public partial class DispatchConsignmentForm : ZTemplateForm
	{
		public DispatchConsignmentForm(WhsItemDispatchConsignment dispatchConsignment) : base(dispatchConsignment)
		{
			InitializeComponent();
			if (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.Value)
			{
				InitialiseMessageMenu();
			}
			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)DispatchConsignment).InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.DtbBooking);
			PlugIns.Add(ControllerIDs.DocumentVisualizer);
			zWorkflowTabPage.Initialize(DispatchConsignment);
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		#endregion

		#region Open in Browser

		void GlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			TransitWarehouseGUIHelper.OpenInBrowser("Goto/DispatchConsignment", DispatchConsignment.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", DispatchConsignment.PK.ToString()) });
		}

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region MessageMenu

		void InitialiseMessageMenu()
		{
			if (DispatchConsignment != null)
			{
				messageMenu = new ZMenuItem(ResString.GetMultilingualString("e67be962-d2d3-4b45-b8e1-8c6faf8a45f9", "Message"));
				MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, messageMenu);

				messageMenu.AddFormsMenuItems(DispatchConsignment,
					ModuleIDs.WhsTransitDispatchConsignment,
					new List<SystemMenuItemInfo>()
					{
						new SystemMenuItemInfo()
						{
							ID = TransitFormMenuItems.CIN750DeconsFromDCN
						},
						new SystemMenuItemInfo()
						{
							ID = TransitFormMenuItems.CIN750ConsFromDCN
						},
						new SystemMenuItemInfo()
						{
							ID = TransitFormMenuItems.CIN750OutFromDCN
						}
					});
			}
		}

		MenuItem messageMenu;

		#endregion

		#region DispatchConsignment

		WhsItemDispatchConsignment DispatchConsignment
		{
			get { return (WhsItemDispatchConsignment)DataSource; }
		}

		#endregion
	}
}
