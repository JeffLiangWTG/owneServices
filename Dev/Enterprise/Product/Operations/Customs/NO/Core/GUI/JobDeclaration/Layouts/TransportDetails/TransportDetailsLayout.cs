using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public sealed class TransportDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout => TransportDetails;

		public TransportDetailsLayout()
		{
			TransportDetails = CreateTransportDetailsLayout();
		}

		PanelLayout CreateTransportDetailsLayout()
		{
			var builder = new TransportDetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			var noBag = TransportDetailsControlBag.Instance;
			builder.AddControlBag(noBag);

			builder.AddColumn();
			builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);

			builder.Add(noBag.TransportDetailsFlightUserControl, ControlWidthClass.Auto);
			builder.Add(noBag.TransportDetailsNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(noBag.TransportDetailsVoyageUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);

			builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.OverrideValuesCheckBox, h => !h.IsStandAlone, h => h.JE_JSInfo);
			builder.SetVisibility(commonBag.OceanBillTextBox, h => h.IsSea || Env.Registry.IsExpress, h => h.JE_TransportModeInfo);
			builder.SetVisibility(noBag.TransportDetailsVoyageUserControl, h => h.IsSea, h => h.JE_TransportModeInfo);
			builder.SetVisibility(commonBag.VesselCodeFindBox, h => h.IsSea, h => h.JE_TransportModeInfo);
			builder.SetVisibility(commonBag.MasterBillTextBox, h => h.IsAir && !Env.Registry.IsExpress, h => h.JE_TransportModeInfo);
			builder.SetVisibility(noBag.TransportDetailsFlightUserControl, h => h.IsAir, h => h.JE_TransportModeInfo);
			builder.SetVisibility(commonBag.TransportIDAndNationalityUserControl, h => h.IsRoad, h => h.JE_TransportModeInfo);
			builder.SetVisibility(noBag.TransportDetailsNationalityUserControl, h => !h.IsAir && !h.IsFixedInstallation && !h.IsPost && !h.IsRail && !h.IsRoad && !h.IsSea, h => h.JE_TransportModeInfo);

			return builder.Build();
		}

		PanelLayout TransportDetails { get; }
	}
}
