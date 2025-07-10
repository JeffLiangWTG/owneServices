using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class CompanyCredentialsControlBag : ControlBag
	{
		public CompanyCredentialsControlBag()
		{
			ICS2CredentialUserControl = RegisterControl(nameof(CompanyCredentialsUserControl.ICS2CredentialUserControl));
		}

		public static CompanyCredentialsControlBag Instance => instance ?? (instance = new CompanyCredentialsControlBag());

		[ThreadStatic]
		static CompanyCredentialsControlBag instance;

		protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

		public ControlReference ICS2CredentialUserControl { get; }
	}
}
