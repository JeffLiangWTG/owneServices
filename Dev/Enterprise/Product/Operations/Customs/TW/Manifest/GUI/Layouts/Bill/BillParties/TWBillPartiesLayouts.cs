using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public sealed class TWBillPartiesLayouts : IPanelLayoutProvider
	{
		PanelLayout BillParties { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillParties;

		public TWBillPartiesLayouts()
		{
			BillParties = CreateBillPartiesLayout();
		}

		PanelLayout CreateBillPartiesLayout()
		{
			var builder = new TWBillPartiesLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.ShipperAddressUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(common.ConsigneeAddressUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(common.NotifyPartyAddressUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}
