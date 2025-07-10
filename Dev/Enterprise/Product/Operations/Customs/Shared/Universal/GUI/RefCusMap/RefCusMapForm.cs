using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusMapForm : ZTemplateForm
	{
		public RefCusMapForm(ZZRefCusMapCombined dataSource)
			: base(dataSource)
		{
			ControllerID = ControllerIDs.Customs.Universal.ZZRefCusMap;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		public override string FormCaption
		{
			get { return (BusinessEntity as ZZRefCusMapCombined)?.HumanReadableName ?? Res.GetString("RefCusMapForm|FormCaption", "Global Data Maps"); }
		}

		protected override void SaveToRecentItems()
		{
		}
	}
}
