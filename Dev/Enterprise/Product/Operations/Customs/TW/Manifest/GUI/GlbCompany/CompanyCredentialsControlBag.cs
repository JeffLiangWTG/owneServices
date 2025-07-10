using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public sealed class CompanyCredentialsControlBag : ControlBag
	{
		public CompanyCredentialsControlBag()
		{
			ForwarderCertificateUserControl = RegisterControl(nameof(CompanyCredentialsUserControl.ForwarderCertificateUserControl));
			LicensingCertificateUserControl = RegisterControl(nameof(CompanyCredentialsUserControl.LicensingCertificateUserControl));
		}

		public static CompanyCredentialsControlBag Instance => instance ?? (instance = new CompanyCredentialsControlBag());

		[ThreadStatic]
		static CompanyCredentialsControlBag instance;

		protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

		public ControlReference ForwarderCertificateUserControl { get; }
		public ControlReference LicensingCertificateUserControl { get; }
	}
}
