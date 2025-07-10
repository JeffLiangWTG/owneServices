using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class CompanyCredentialsControlBag : ControlBag
{
	CompanyCredentialsControlBag()
	{
		CompanyCredentialsDetailsUserControl = RegisterControl(nameof(CompanyCredentialsUserControl.CompanyCredentialsDetailsUserControl));
	}

	public static CompanyCredentialsControlBag Instance => instance ??= new CompanyCredentialsControlBag();

	[ThreadStatic]
	static CompanyCredentialsControlBag instance;

	protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

	public ControlReference CompanyCredentialsDetailsUserControl { get; }
}
