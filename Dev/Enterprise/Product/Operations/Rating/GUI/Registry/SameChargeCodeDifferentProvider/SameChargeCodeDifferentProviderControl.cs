using Enterprise.Registry.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class SameChargeCodeDifferentProviderControl : RegistryZUserControl
	{
		public SameChargeCodeDifferentProviderControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			SameChargeCodeDifferentProviderGrid.ReadOnly = readOnly;
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
		{
			get
			{
				return SameChargeCodeDifferentProviderGrid.ReadOnly;
			}
		}

#endif

	}
}
