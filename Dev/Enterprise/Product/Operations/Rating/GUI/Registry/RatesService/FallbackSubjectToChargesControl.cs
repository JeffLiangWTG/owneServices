using Enterprise.Registry.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class FallbackSubjectToChargesControl : RegistryZUserControl
	{
		public FallbackSubjectToChargesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			FallbackSubjectToChargesGrid.ReadOnly = readOnly;
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
		{
			get
			{
				return FallbackSubjectToChargesGrid.ReadOnly;
			}
		}

#endif

	}
}
