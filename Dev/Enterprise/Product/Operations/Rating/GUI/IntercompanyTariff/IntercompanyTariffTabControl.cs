using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class IntercompanyTariffTabControl : BaseTabControl
	{
		public IntercompanyTariffTabControl() => InitializeComponent();

		public override ZTabControl TopLevelTabControl => TabControl;

		protected override string[] GetInapplicableGroups() => new[]
		{
			Groups.LinerAndAgency,
			Groups.CFS,
			Groups.Warehouse,
			Groups.Transport
		};
	}
}
