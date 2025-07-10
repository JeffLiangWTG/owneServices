using System.Diagnostics;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business;

[DebuggerDisplay("{From} -> {To}")]
public class PrePostCarriageLegWrapper
{
	public PrePostCarriageLegWrapper(IPrePostCarriageLocation from, IPrePostCarriageLocation to, string transportMode = default, bool isMainCarriage = false)
	{
		From = Argument.NotNull(from, nameof(from));
		To = Argument.NotNull(to, nameof(to));
		TransportMode = string.IsNullOrEmpty(transportMode) ? Core.Constants.TransportModes.Road : transportMode;
		IsMainCarriage = isMainCarriage;
	}

	public PrePostCarriageLegWrapper(ICO2eLegProvider cO2eProvider, BusinessObjectFactory factory, bool isMainCarriage = false)
	{
		Argument.NotNull(cO2eProvider, nameof(cO2eProvider));
		From = new PrePostCarriageLocationWrapper(cO2eProvider.LoadPort);
		To = new PrePostCarriageLocationWrapper(cO2eProvider.DiscPort);
		ActualLeg = cO2eProvider;
		TransportMode = cO2eProvider.TransportMode;
		IsMainCarriage = isMainCarriage;
	}

	public readonly ICO2eLegProvider ActualLeg;
	public readonly bool IsMainCarriage;
	public IPrePostCarriageLocation From { get; private set; }
	public IPrePostCarriageLocation To { get; private set; }
	public string TransportMode { get; private set; }
	public ZBool IsVirtual => ActualLeg == null;
}
