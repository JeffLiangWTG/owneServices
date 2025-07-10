using Enterprise.Registry.GUI;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class DateAndReferenceControl : RegistryZUserControl
	{
		public DateAndReferenceControl()
		{
			InitializeComponent();
		}

		#region ReadOnly

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DateAndReferenceGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}

