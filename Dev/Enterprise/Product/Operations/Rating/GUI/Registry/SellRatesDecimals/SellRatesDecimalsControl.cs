using Enterprise.Registry.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class SellRatesDecimalsControl : RegistryZUserControl
	{
		public SellRatesDecimalsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			NumberOfDecimalsGrid.ReadOnly = readOnly;
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
		{
			get
			{
				return NumberOfDecimalsGrid.ReadOnly;
			}
		}

#endif

	}
}

