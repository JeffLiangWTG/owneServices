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
	public partial class ReceiveConsignmentForm : ZTemplateForm
	{
		public ReceiveConsignmentForm(WhsItemReceiveConsignment receiveConsignment) : base(receiveConsignment)
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
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)ReceiveConsignment).InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.DocumentVisualizer); 
			zWorkflowTabPage.Initialize(ReceiveConsignment);
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		#endregion

		#region Open in Browser

		void GlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			TransitWarehouseGUIHelper.OpenInBrowser("Goto/ReceiveConsignment", ReceiveConsignment.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", ReceiveConsignment.PK.ToString()) });
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
			if (ReceiveConsignment != null)
			{
				messageMenu = new ZMenuItem(ResString.GetMultilingualString("e8b48769-7ede-4222-a05b-25c9e41195e1", "Message"));
				MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, messageMenu);

				messageMenu.AddFormsMenuItems(ReceiveConsignment,
					ModuleIDs.WhsTransitReceiveConsignment,
					new List<SystemMenuItemInfo>()
					{
						new SystemMenuItemInfo()
						{
							ID = TransitFormMenuItems.CIN750InFromRCN
						},
						new SystemMenuItemInfo()
						{
							ID = TransitFormMenuItems.CIN750CorFromRCN
						}
					});
			}
		}

		MenuItem messageMenu;

		#endregion

		#region ReceiveConsignment

		WhsItemReceiveConsignment ReceiveConsignment
		{
			get { return (WhsItemReceiveConsignment)DataSource; }
		}

		#endregion
	}
}
