using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public sealed class Phase5TraderDetailsLayout : IPanelLayoutProvider
{
	public Phase5TraderDetailsLayout()
	{
		Layout = CreateDepartureDetailsLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	PanelLayout CreateDepartureDetailsLayout()
	{
		var builder = new TraderDetailsLayoutBuilder<EU.NCTS.Business.NctsHeader>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.PrincipalDocAddressControl, ControlWidthClass.LongControl);
		builder.Add(commonBag.ConsignorDocAddressControl, ControlWidthClass.LongControl);
		builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.LongControl);
		builder.Add(commonBag.RepresentativeDocAddressControl, ControlWidthClass.LongControl);

		builder.AddControlBehaviour(commonBag.PrincipalDocAddressControl, new CompactDisplayModeBehaviour());
		builder.AddControlBehaviour(commonBag.ConsignorDocAddressControl, new CompactDisplayModeBehaviour());
		builder.AddControlBehaviour(commonBag.ConsigneeDocAddressControl, new CompactDisplayModeBehaviour());

		return builder.Build();
	}
}
