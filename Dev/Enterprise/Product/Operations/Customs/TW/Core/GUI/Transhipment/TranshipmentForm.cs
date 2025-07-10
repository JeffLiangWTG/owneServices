using CargoWise.Common;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.GUI
{
	public partial class TranshipmentForm : ZTemplateForm
	{
		public TranshipmentForm(CusInBondHeader header)
		: base(header)
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			var actionMenuItemIndex = MainMenu.MenuItems.IndexOf(ActionsMenuItem);
			var transhipmentMenuItem = new TranshipmentMessageMenuItem(header);
			MainMenu.MenuItems.Add(actionMenuItemIndex + 1, transhipmentMenuItem);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get
			{
				var aCaption = Res.GetString("TWInBondForm|FormCaption", "Transhipment");
				if (!this.IsDesignMode())
				{
					aCaption += " - " + BusinessEntity.HumanReadableName;
				}
				return aCaption;
			}
		}
	}
}
