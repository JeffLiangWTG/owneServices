using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public class OrganisationsControlBag : ControlBag
{
	protected OrganisationsControlBag()
	{
		IntracomReceiverAddressControl = RegisterControl(nameof(IntracomReceiverAddressControl));
		DefermentPartyDocAddressControl = RegisterControl(nameof(DefermentPartyDocAddressControl));
		ExporterDocAddressControl = RegisterControl(nameof(ExporterDocAddressControl));
	}

	public static OrganisationsControlBag Instance => instance ?? (instance = new OrganisationsControlBag());

	[ThreadStatic]
	static OrganisationsControlBag instance;

	protected override Control CreateTemplate() => new OrganisationsUserControl();

	public ControlReference IntracomReceiverAddressControl { get; }
	public ControlReference DefermentPartyDocAddressControl { get; }
	public ControlReference ExporterDocAddressControl { get; }
}
