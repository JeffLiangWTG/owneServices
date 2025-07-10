using Enterprise.Core;
using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AutoRateDateByChargeGroupConfigurationControl : RegistryZUserControl
	{
		public AutoRateDateByChargeGroupConfigurationControl()
			: base()
		{
			InitializeComponent();

			filterTypeDropEdit.SelectedIndexChanged += (s, e) =>
			{
				autoRateDateByChargeGroupControl.Visible = filterTypeDropEdit.Text == Constants.RatingDateFilterTypes.Codes.Custom;
			};
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			filterTypeDropEdit.ReadOnly = readOnly;
			autoRateDateByChargeGroupControl.ReadOnly = readOnly;
		}
	}
}
