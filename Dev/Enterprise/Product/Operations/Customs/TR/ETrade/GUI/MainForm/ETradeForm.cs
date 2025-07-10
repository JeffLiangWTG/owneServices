using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public partial class ETradeForm : ZTemplateForm
	{
		public ETradeForm(AsycudaManifestHeader header) : base(header)
		{
			InitializeComponent();
			AddPlugins();
			AddETradeMenu();
			ActionsMenuItem.MenuItems.AddRange(ApplicationGUIProvider.GetApplicationGuiProvider(header).GetActionsExtraMenuItems().ToArray<MenuItem>());
			WorkflowTabPage.Initialize(header);
		}

		public new AsycudaManifestHeader BusinessEntity => (AsycudaManifestHeader)base.BusinessEntity;

		public override string FormCaption
		{
			get
			{
				var caption = Res.GetString("68E02BA6-D096-4A59-9699-F63CE120BFAE", "E-Trade");
				if (!this.IsDesignMode())
				{
					caption = BusinessEntity.HumanReadableName;
				}
				return caption;
			}
		}

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
		}

		void AddETradeMenu()
		{
			var actionMenuItemIndex = MainMenu.MenuItems.IndexOf(ActionsMenuItem);
			var eTradeMenu = new ETradeMenu(BusinessEntity);
			MainMenu.MenuItems.Add(actionMenuItemIndex + 1, eTradeMenu);
		}
	}
}
