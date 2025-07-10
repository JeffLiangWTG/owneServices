using Enterprise.Registry.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateCommodityDefaultingRuleControl : RegistryZUserControl
	{
		public RateCommodityDefaultingRuleControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			RateCommodityGrid.ReadOnly = readOnly;
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
		{
			get
			{
				return RateCommodityGrid.ReadOnly;
			}
		}

#endif

	}
}
