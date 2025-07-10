using CargoWise.Types;
using Enterprise.Customs.NL.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NLNctsFallbackEntryNumberGeneratorTarget : NumberGeneratorTarget
{
	protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore() => Context.AccessRegistry(NLCustomsRegistry.Instance.NLNctsFallbackEntryNumberCustomisation);

	protected override int GetMaxLengthCore() => NctsDepartureMovementHeader.Schema.FallbackEntryNumberLength;

	protected override ZString GetNameCore() => "NLNctsFallbackEntryNumberCustomisation";

	public override string NumberCustomisationLocation => ((IRegistryItemInternals)NLCustomsRegistry.Instance.NLNctsFallbackEntryNumberCustomisation).Location;
}
