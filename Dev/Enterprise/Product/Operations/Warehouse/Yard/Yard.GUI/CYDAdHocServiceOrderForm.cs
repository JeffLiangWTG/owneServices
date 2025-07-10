using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.GUI
{
	public partial class CYDAdHocServiceOrderForm : ZTemplateForm
	{
		public CYDAdHocServiceOrderForm(CYDAdHocServiceOrder adHocServiceOrder) : base(adHocServiceOrder)
		{
			InitializeComponent();
			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)adHocServiceOrder).InvoicingSupporter);
			zWorkflowTabPage.Initialize(adHocServiceOrder);
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region CYDAdHocServiceOrder

		CYDAdHocServiceOrder adHocServiceOrder
		{
			get { return (CYDAdHocServiceOrder)DataSource; }
		}

		#endregion
	}
}
