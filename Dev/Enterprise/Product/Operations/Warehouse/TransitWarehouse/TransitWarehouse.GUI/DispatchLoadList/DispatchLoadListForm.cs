using System.Windows.Forms;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.GUI
{
	public partial class DispatchLoadListForm : ZTemplateForm
	{
		public DispatchLoadListForm(WhsItemDispatchLoadList dll) : base(dll)
		{
			InitializeComponent();

			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Apportionment, 2);
			zWorkflowTabPage.Initialize(DLL);
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		#endregion

		#region Open in Browser

		void GlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			TransitWarehouseGUIHelper.OpenInBrowser("Goto/DispatchLoadList", DLL.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", DLL.PK.ToString()) });
		}

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region DispatchLoadList

		WhsItemDispatchLoadList DLL
		{
			get { return (WhsItemDispatchLoadList)DataSource; }
		}

		#endregion
	}
}
