using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Services.ServiceHost
{
	public class TrackingMapUrlService : ITrackingMapUrlService
	{
		public (Uri url, string errorMessage) GetActiveTransportMapUrl(TransportOrderHelper helper, OrgContact contact = null, CancellationToken ct = default)
		{
			Argument.NotNull(helper, nameof(helper));
			Argument.NotNull(ct, nameof(ct));

			var activeTransport = helper.LastLegMatching(x => (x.IsSea || x.IsAir) && (x.JW_ATD_UTC <= ZDateTime.UtcNow || (x.JW_ATD_UTC.IsEmpty && x.JW_ETD_UTC <= ZDateTime.UtcNow))) ?? helper.FirstLeg;

			if (activeTransport == null)
			{
				return (null, Res.GetString("3A98A863-7B90-4D32-B7FC-33A87B9ADDC7", "Transport not found."));
			}

			var transportMode = activeTransport.TransportMode;

			switch (transportMode)
			{
				case Core.Constants.TransportModes.Sea:
					return GenerateSeaMapUrl(activeTransport.Parent.PK.ToString(), activeTransport, contact, ct);
				case Core.Constants.TransportModes.Air:
					if (activeTransport.JW_ParentType == Core.Constants.TransportParentTypes.Consol && activeTransport.Parent is CommonConsol consol)
					{
						var consignmentNumber = consol.JK_UniqueConsignRef;

						return GenerateAirMapUrl(activeTransport.Parent.PK.ToString(), consignmentNumber, contact, ct);
					}
					else
					{
						return (null, Res.GetString("7804076a-281b-4807-82bb-91f0817e46f3", "Cargo Tracker is only available for AIR consols."));
					}
				default:
					return (null, Res.GetString("5cd9c82d-3bb5-4258-9ef4-81cad000ed78", "The shipment is in transit via {0} mode.", GetTransportModeDescription(activeTransport.TransportMode)));
			}
		}

		string GetTransportModeDescription(String transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return Core.Constants.TransportModeDescriptions.Air;
				case Core.Constants.TransportModes.Rail:
					return Core.Constants.TransportModeDescriptions.Rail;
				case Core.Constants.TransportModes.Road:
					return Core.Constants.TransportModeDescriptions.Road;
				case Core.Constants.TransportModes.Sea:
					return Core.Constants.TransportModeDescriptions.Sea;
				default:
					return transportMode;
			}
		}

		public (Uri url, string errorMessage) GetOrderMapUrl(Order order, OrgContact contact = null, CancellationToken ct = default)
		{
			Argument.NotNull(order, nameof(order));
			Argument.NotNull(ct, nameof(ct));

			var helper = new OrderVesselMovementsUrlHelper(order);

			return GenerateSeaMapUrl(order.PK.ToString(), helper, contact, ct);
		}

		(Uri url, string errorMessage) GenerateSeaMapUrl(string correlationID, IVesselMovementsUrlSupporter supporter, OrgContact contact = null, CancellationToken ct = default)
		{
			var generator = ObjectFactory.Get<IVesselMovementsUrlGenerator>();
			var options = VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel;
			var tokenResult = generator.GetTokenAsync(correlationID, contact, ct).Result;
			if (!string.IsNullOrEmpty(tokenResult.ErrorMessage))
			{
				return (null, FormatMapErrorMessage(tokenResult.ErrorMessage));
			}
			if (tokenResult.RedirectUrl != null)
			{
				return (tokenResult.RedirectUrl, null);
			}

			var (mapUrl, mapValidationMessage) = generator.Generate(tokenResult.Token, supporter, options);

			if (!string.IsNullOrEmpty(mapValidationMessage))
			{
				return (null, FormatMapErrorMessage(mapValidationMessage));
			}

			return (mapUrl, null);
		}

		(Uri url, string errorMessage) GenerateAirMapUrl(string correlationID, string consignmentNumber, OrgContact contact = null, CancellationToken ct = default)
		{
			var generator = ObjectFactory.Get<ICargoTrackerUrlGenerator>();
			var tokenResult = generator.GetToken(correlationID, contact, ct);
			if (!string.IsNullOrEmpty(tokenResult.ErrorMessage))
			{
				return (null, FormatMapErrorMessage(tokenResult.ErrorMessage));
			}
			if (tokenResult.RedirectUrl != null)
			{
				return (tokenResult.RedirectUrl, null);
			}

			var licenseCode = GlbCompany.CurrentCompany.GetLicenceCode();
			var (mapUrl, mapValidationMessage) = generator.Generate(tokenResult.Token, licenseCode, consignmentNumber);

			if (!string.IsNullOrEmpty(mapValidationMessage))
			{
				return (null, FormatMapErrorMessage(mapValidationMessage));
			}

			return (mapUrl, null);
		}

		string FormatMapErrorMessage(string errorMessage) => Res.GetString("656783F3-1EA3-4DB1-A249-5E829591C709", "Unable to show map: {0}", errorMessage);
	}
}
