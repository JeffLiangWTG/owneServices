using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public sealed class CompanyCredentialsControlBag : ControlBag
	{
		public CompanyCredentialsControlBag()
		{
			CredentialGroupBox = RegisterControl(nameof(CompanyCredentialsUserControl.CredentialGroupBox));
		}

		public static CompanyCredentialsControlBag Instance => instance ?? (instance = new CompanyCredentialsControlBag());

		[ThreadStatic]
		static CompanyCredentialsControlBag instance;

		protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

		public ControlReference CredentialGroupBox { get; }
	}
}
