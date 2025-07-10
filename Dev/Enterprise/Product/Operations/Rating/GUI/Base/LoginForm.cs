using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class LoginForm : ZChildForm
	{
		public LoginForm(SecurityOverridenLogin login)
			: base(login)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("15dcc305-bf78-491e-95f9-8bc51564121c", "Login"); }
		}

		#region IDisposable Members

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

