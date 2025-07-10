using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(BookingContainer))]
	sealed class BookingContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BookingContainer(new ZGuid())
			{
				PackingLines = System.Array.Empty<PackingLine>(),
				Numbers = System.Array.Empty<ReferenceNumber>(),
				AdditionalServices = System.Array.Empty<AdditionalService>(),
				Milestones = System.Array.Empty<Milestone>()
			};
		}
	}
}
