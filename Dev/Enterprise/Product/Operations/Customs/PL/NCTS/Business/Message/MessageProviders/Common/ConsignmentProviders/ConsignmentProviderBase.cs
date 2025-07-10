using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.PL.NCTS.Business;

public abstract class ConsignmentProviderBase
{
	protected ConsignmentProviderBase(IDepartureTransportMeansProvider transportMeansProvider)
	{
		this.transportMeansProvider = Argument.NotNull(transportMeansProvider, nameof(transportMeansProvider));
	}

	protected readonly IDepartureTransportMeansProvider transportMeansProvider;

	protected abstract string GetDepartureTransportMeansTransportMode();

	public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??= GetDepartureTransportMeans();
	IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

	IReadOnlyCollection<IDepartureTransportMeans> GetDepartureTransportMeans()
		=> GetDepartureTransportMeansTransportMode() is string mode && !string.IsNullOrWhiteSpace(mode)
			? GenerateDepartureTransportMeans(mode)
			: Array.Empty<IDepartureTransportMeans>();

	IReadOnlyCollection<IDepartureTransportMeans> GenerateDepartureTransportMeans(string inlandTransportMode)
	{
		var result = new List<IDepartureTransportMeans>();

		switch (inlandTransportMode)
		{
			case ModeOfTransportList.Codes._1_SeaTransport:
				GenerateSeaTransport();
				break;
			case ModeOfTransportList.Codes._2_RailTransport:
				GenerateRailTransport();
				break;
			case ModeOfTransportList.Codes._3_RoadTransport:
				GenerateRoadTransport();
				break;
			case ModeOfTransportList.Codes._4_AirTransport:
				GenerateAirTransport();
				break;
			case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
				GenerateInlandWaterwayTransport();
				break;
			case ModeOfTransportList.Codes._9_OwnPropulsion:
				GenerateOwnPropulsionTransport();
				break;
		}
		return result;

		void GenerateSeaTransport()
		{
			TryAdd(transportMeansProvider.TransportTypeAtDeparture, transportMeansProvider.VesselNameAtDeparture, transportMeansProvider.VesselCountryAtDeparture);
		}

		void GenerateRailTransport()
		{
			TryAdd(transportMeansProvider.TransportTypeAtDeparture, transportMeansProvider.TransportAtDeparture, transportMeansProvider.TransportCountryAtDeparture);
			transportMeansProvider.AdditionalWagons.ForEach(
				wagon => TryAdd(transportMeansProvider.TransportTypeAtDeparture, wagon.WagonNumber, wagon.WagonNationality));
		}

		void GenerateRoadTransport()
		{
			TryAdd(transportMeansProvider.TransportTypeAtDeparture, transportMeansProvider.TransportAtDeparture, transportMeansProvider.TransportCountryAtDeparture);
			TryAdd(NctsTransportTypeOfIdList.Codes._31, transportMeansProvider.Trailer1IDAtDeparture, transportMeansProvider.Trailer1NationalityAtDeparture);
			TryAdd(NctsTransportTypeOfIdList.Codes._31, transportMeansProvider.Trailer2IDAtDeparture, transportMeansProvider.Trailer2NationalityAtDeparture);
		}

		void GenerateAirTransport()
		{
			TryAdd(transportMeansProvider.TransportTypeAtDeparture, transportMeansProvider.TransportAtDeparture, transportMeansProvider.TransportCountryAtDeparture);
		}

		void GenerateInlandWaterwayTransport()
		{
			TryAdd(transportMeansProvider.TransportTypeAtDeparture, transportMeansProvider.TransportAtDeparture, transportMeansProvider.TransportCountryAtDeparture);
		}

		void GenerateOwnPropulsionTransport()
		{
			TryAdd(transportMeansProvider.TransportTypeAtDeparture, transportMeansProvider.TransportAtDeparture, transportMeansProvider.TransportCountryAtDeparture);
		}

		void TryAdd(ZString identificationType, ZString identificationNumber, string nationality)
		{
			if (!identificationNumber.IsEmpty)
			{
				result.Add(new DepartureTransportMeansProvider(result.Count + 1, identificationType, identificationNumber, GetTransportMeanNationality(inlandTransportMode, nationality)));
			}
		}
	}

	protected virtual string GetTransportMeanNationality(string transportMode, string value) => value;

	protected bool InPhase5TransitionPeriod => CachedValueHelper.GetValue(ref inPhase5TransitionPeriod, () => transportMeansProvider.IsInPhase5TransitionPeriod);
	CachedValue<bool> inPhase5TransitionPeriod;
}
