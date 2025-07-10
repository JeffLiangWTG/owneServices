using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(ContainerAdviceToBooking))]
	sealed class ContainerAdviceToBookingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var containerAdviceToBooking = new ContainerAdviceToBooking(
				"ForwardingShipment",
				"S0001000");

			containerAdviceToBooking.Containers = new List<BookingContainer>();

			return containerAdviceToBooking;
		}
	}
}
