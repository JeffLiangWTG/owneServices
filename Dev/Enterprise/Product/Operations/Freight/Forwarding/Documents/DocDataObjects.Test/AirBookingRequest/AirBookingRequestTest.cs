using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(AirBookingRequest))]
	sealed class AirBookingRequestTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AirBookingRequest("zzz", "zzz")
			{
				Dimensions = Array.Empty<PackingLine>(),
				FlightDetails = Array.Empty<FlightDetail>(),
				Ulds = Array.Empty<ULD>(),
				CarrierContractNumbers = Array.Empty<ReferenceNumber>(),
				SpecialHandlingItems = Array.Empty<CodeDescription>()
			};
		}
	}
}
