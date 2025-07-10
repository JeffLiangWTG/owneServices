namespace Enterprise.Freight.Business
{
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;
	using static Enterprise.Registry.Business.AutoratingViaPortHelper;

	public class ConsolRatingRoute : RatingRoute<CommonConsol>
	{
		public ConsolRatingRoute(CommonConsol parent) : base(parent) { }

		public override OrgHeader Carrier
		{
			get { return parent.ShippingLine; }
		}

		public override Creditors Creditors
		{
			get => creditorsOverride ?? parent.GetCreditors();
			set => creditorsOverride = value;
		}
		Creditors creditorsOverride;

		public override ILocation Destination
		{
			get { return parent.DischargePort; }
		}

		public override ILocation Origin
		{
			get { return parent.LoadPort; }
		}

		public override ZString TransportMode
		{
			get { return parent.JK_TransportMode; }
		}

		public override ILocation GetVia(CostSell costOrSell)
		{
			var miTransport = parent.Transports.MostInterestingTransport;
			var routes = parent.GetRatingRoutes(costOrSell);

			if (routes.Count > 0)
			{
				var loadVia = parent.LoadPort != miTransport.LoadPort ? miTransport.LoadPort : null;
				var dischargeVia = parent.DischargePort != miTransport.DiscPort ? miTransport.DiscPort : null;

				if (loadVia != null && dischargeVia != null)
				{
					if (parent.IsImport())
					{
						return dischargeVia;
					}

					if (parent.IsExport())
					{
						return loadVia;
					}
				}
				else
				{
					return loadVia ?? dischargeVia;
				}

				return null;
			}

			var viaCode = RatingDataRegistry.Instance.AutoratingViaPort.GetViaForForwardingConsol(
				transportMode: parent.TransportMode,
				direction: 
					parent.IsExport() ? DirectionOption.Export.Code :
					parent.IsImport() ? DirectionOption.Import.Code :
					DirectionOption.All.Code,
				origin: parent.JK_RL_NKLoadPort,
				destination: parent.JK_RL_NKDischargePort,
				voyageLoad: miTransport.JW_RL_NKLoadPort,
				voyageDischarge: miTransport.JW_RL_NKDiscPort,
				lastModeRouteSetDischarge: parent.Transports.Where(r => r.TransportMode == parent.TransportMode).LastOrDefault()?.JW_RL_NKDiscPort);

			return LocationHelper.GetCachedLocationFromString(viaCode, parent.Factory);
		}

		public override ILocation GetFirstLoad(CostSell costOrSell) => parent.LoadPort;

		public override ILocation GetLastDischarge(CostSell costOrSell) => parent.DischargePort;

		public override ILocation GetFirstRouteSetLoad(CostSell costOrSell) =>
			parent.Transports.FirstTransportWithTransportMode(parent.TransportMode)?.LoadPort;

		public override ILocation GetLastRouteSetDischarge(CostSell costOrSell) =>
			parent.Transports.LastTransportWithTransportMode(parent.TransportMode)?.DiscPort;

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new ConsolJobDatesProvider(parent); }
		}

		public override ZBool SupportsManualRateSelection
		{
			get => supportsManualRateSelectionOverride;
			set => supportsManualRateSelectionOverride = value;
		}
		ZBool supportsManualRateSelectionOverride = true;
	}
}
