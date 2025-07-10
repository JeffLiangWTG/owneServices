using System.Windows.Forms;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.GUI
{
	public partial class CYDDeliveryHeaderForm : ZTemplateForm
	{
		public CYDDeliveryHeaderForm(CYDDeliveryHeader deliveryHeader) : base(deliveryHeader)
		{
			InitializeComponent();
			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			zWorkflowTabPage.Initialize(deliveryHeader);
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region GLOW link open in browser

		void GlowLinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/CYDDeliveryHeader", deliveryHeader.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", deliveryHeader.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		#endregion

		#region CYDDeliveryHeader

		CYDDeliveryHeader deliveryHeader
		{
			get { return (CYDDeliveryHeader)DataSource; }
		}

		#endregion
	}
}
