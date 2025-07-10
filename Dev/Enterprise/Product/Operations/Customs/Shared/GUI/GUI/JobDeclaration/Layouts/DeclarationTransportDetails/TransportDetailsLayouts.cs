using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class TransportDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout TransportDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => TransportDetails;

		public TransportDetailsLayouts()
		{
			TransportDetails = CreateTransportDetailsLayout();
		}

		PanelLayout CreateTransportDetailsLayout()
		{
			var builder = new TransportDetailsLayoutBuilder<BaseJobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.VehicleRegistrationNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.FlightUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.VoyageNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
