using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public sealed class MovementReferenceNumber : IAWBMovementReferenceNumberMessageDetailsProvider
	{
		public List<ZString> Numbers
		{
			get { return numbers ?? (numbers = new List<ZString>()); }
		}

		List<ZString> numbers;
		public ZString CountryOfIssue { get; set; }
		public ZString MovementCode { get; set; }
		public ZString CommunityTransitStatusCode { get; set; }

		public List<EntryNumber> RelatedNumbers
		{
			get { return relatedNumbers ?? (relatedNumbers = new List<EntryNumber>()); }
		}

		List<EntryNumber> relatedNumbers;
		IReadOnlyList<ZString> IAWBMovementReferenceNumberMessageDetailsProvider.Numbers => Numbers.AsReadOnly();

		IReadOnlyCollection<IAWBEntryNumberMessageDetailsProvider> IAWBMovementReferenceNumberMessageDetailsProvider.RelatedNumbers => RelatedNumbers.Cast<IAWBEntryNumberMessageDetailsProvider>().ToList().AsReadOnly();
	}
}
