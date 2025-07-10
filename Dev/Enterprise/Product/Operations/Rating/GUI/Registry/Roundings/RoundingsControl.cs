using Enterprise.Registry.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RoundingsControl : RegistryZUserControl
	{
		public RoundingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultRoundingsGrid.ReadOnly = readOnly;
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
		{
			get
			{
				return DefaultRoundingsGrid.ReadOnly;
			}
		}

#endif

	}
}

