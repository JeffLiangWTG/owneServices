using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBMovementReferenceNumberMessageDetailsProvider
	{
		IReadOnlyList<ZString> Numbers { get; }
		ZString CountryOfIssue { get; }
		ZString MovementCode { get; }
		IReadOnlyCollection<IAWBEntryNumberMessageDetailsProvider> RelatedNumbers { get; }
		ZString CommunityTransitStatusCode { get; }
	}
}
