using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.DataTransfer.Universal;

public class CO2eCompleteChainOfLegsProvider
{
	public CO2eCompleteChainOfLegsProvider(ICO2eLegBasedSupporter supporter, bool useRoadForFirstAndLastLegs = default)
	{
		this.supporter = Argument.NotNull(supporter, nameof(supporter));
		this.useRoadForFirstAndLastLegs = useRoadForFirstAndLastLegs;
	}

	readonly ICO2eLegBasedSupporter supporter;
	readonly bool useRoadForFirstAndLastLegs;

	List<ICO2eLegProvider> GetLegsProviders()
	{
		var result = new List<ICO2eLegProvider>();
		if (supporter.Legs is { Count: > 0 })
		{
			if (supporter.Legs[0].GetType() == typeof(Transport))
			{
				var transportOrderHelper = new TransportOrderHelper(supporter.Legs
					.Cast<Transport>()
					.Where(transport => !transport.JW_RL_NKLoadPort.IsEmpty || !transport.JW_RL_NKDiscPort.IsEmpty)
					.ToArray());
				transportOrderHelper.ForEach(result.Add);
			}
			else
			{
				result.AddRange(supporter.Legs.Cast<ICO2eLegProvider>());
			}
		}

		return result;
	}

	public PrePostCarriageLegWrapper[] GetChain()
	{
		var result = new List<PrePostCarriageLegWrapper>();
		var legs = GetLegsProviders();

		var transportModeForVirtualLeg = supporter.ConvertTransportModeForCO2eCalculation();
		if (legs.Count > 0)
		{
			var unlocoLoader = new RefUNLOCO.Loader(supporter.Factory);
			AddLoadPortToFirstLeg(result, useRoadForFirstAndLastLegs ? TransportModes.Road : transportModeForVirtualLeg, unlocoLoader, legs[0]);

			for (var i = 0; i < legs.Count; i++)
			{
				if (!legs[i].LoadPort.IsEmpty && !legs[i].DiscPort.IsEmpty)
				{
					result.Add(new PrePostCarriageLegWrapper(legs[i], supporter.Factory, isMainCarriage: true));
				}

				if (i < legs.Count - 1)
				{
					AddIfPortsAreDifferent(result,
						new PrePostCarriageLocationWrapper(legs[i].DiscPort).FallbackIfEmpty(legs[i].LoadPort),
						new PrePostCarriageLocationWrapper(legs[i + 1].LoadPort).FallbackIfEmpty(legs[i + 1].DiscPort),
						transportModeForVirtualLeg);
				}
			}

			AddLastLegToDischargePort(result, useRoadForFirstAndLastLegs ? TransportModes.Road : transportModeForVirtualLeg, unlocoLoader, legs[legs.Count - 1]);
		}
		else
		{
			AddWhenNoLegs(result, transportModeForVirtualLeg);
		}

		return result.ToArray();
	}

	void AddWhenNoLegs(List<PrePostCarriageLegWrapper> result, ZString transportMode)
	{
		AddIfPortsAreDifferent(result, supporter.LoadPortForCO2eCalc, supporter.AdditionalLoadPortForCO2eCalc.FallbackIfEmpty(supporter.ViaPortForCO2eCalc).FallbackIfEmpty(supporter.AdditionalDischargePortForCO2eCalc), transportMode);
		if (supporter.ViaPortForCO2eCalc != null && !supporter.ViaPortForCO2eCalc.IsEmpty)
		{
			AddIfPortsAreDifferent(result, supporter.AdditionalLoadPortForCO2eCalc, supporter.ViaPortForCO2eCalc, transportMode);
			AddIfPortsAreDifferent(result, supporter.ViaPortForCO2eCalc, supporter.AdditionalDischargePortForCO2eCalc, transportMode);
		}
		else
		{
			AddIfPortsAreDifferent(result, supporter.AdditionalLoadPortForCO2eCalc, supporter.AdditionalDischargePortForCO2eCalc, transportMode);
		}
		AddIfPortsAreDifferent(result, supporter.AdditionalDischargePortForCO2eCalc.FallbackIfEmpty(supporter.ViaPortForCO2eCalc).FallbackIfEmpty(supporter.AdditionalLoadPortForCO2eCalc), supporter.DischargePortForCO2eCalc, transportMode);
	}

	void AddLoadPortToFirstLeg(List<PrePostCarriageLegWrapper> result, ZString transportMode, RefUNLOCO.Loader unlocoLoader, ICO2eLegProvider firstLeg)
	{
		AddIfPortsAreDifferent(result, supporter.LoadPortForCO2eCalc, supporter.AdditionalLoadPortForCO2eCalc, transportMode);
		AddIfPortsAreDifferent(result,
			supporter.AdditionalLoadPortForCO2eCalc.FallbackIfEmpty(supporter.LoadPortForCO2eCalc),
			new PrePostCarriageLocationWrapper(firstLeg.LoadPort).FallbackIfEmpty(firstLeg.DiscPort),
			transportMode);
	}

	void AddLastLegToDischargePort(List<PrePostCarriageLegWrapper> result, ZString transportMode, RefUNLOCO.Loader unlocoLoader, ICO2eLegProvider lastLeg)
	{
		AddIfPortsAreDifferent(result,
			new PrePostCarriageLocationWrapper((lastLeg.DiscPort.FallbackIfEmpty(lastLeg.LoadPort))),
			supporter.AdditionalDischargePortForCO2eCalc.FallbackIfEmpty(supporter.DischargePortForCO2eCalc),
			transportMode);
		AddIfPortsAreDifferent(result, supporter.AdditionalDischargePortForCO2eCalc, supporter.DischargePortForCO2eCalc, transportMode);
	}

	void AddIfPortsAreDifferent(List<PrePostCarriageLegWrapper> list, IPrePostCarriageLocation loadPort, IPrePostCarriageLocation dischargePort, ZString transportMode)
	{
		if (loadPort != null && dischargePort != null && !loadPort.IsEmpty && !dischargePort.IsEmpty && !loadPort.Equals(dischargePort))
		{
			list.Add(new PrePostCarriageLegWrapper(loadPort, dischargePort, transportMode, isMainCarriage: true));
		}
	}
}
