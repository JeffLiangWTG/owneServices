using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class MO4UserControl : ZUserControl, IAMSControlIdentity
	{
		public MO4UserControl()
		{
			InitializeComponent();
		}

		public string IdentityCode
		{
			get => identityCode;
			set
			{
				identityCode = value;
				LinesGroupBox.Text = identityCode + " Details";
			}
		}
		string identityCode = string.Empty;

		string IAMSControlIdentity.IdentityCode => IdentityCode;
	}
}
