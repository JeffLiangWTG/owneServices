using System.Windows.Forms;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.GUI
{
	public partial class ReceiveASNForm : ZTemplateForm
	{
		public ReceiveASNForm(WhsItemReceiveASN asn) : base(asn)
		{
			InitializeComponent();

			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			zWorkflowTabPage.Initialize(ASN);
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		#endregion

		#region Open in Browser

		void GlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			TransitWarehouseGUIHelper.OpenInBrowser("Goto/ReceiveASN", ASN.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", ASN.PK.ToString()) });
		}

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region ReceiveASN

		WhsItemReceiveASN ASN
		{
			get { return (WhsItemReceiveASN)DataSource; }
		}

		#endregion
	}
}
