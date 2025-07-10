using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(CargoReceiptAdvice))]
	sealed class CargoReceiptAdviceTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new CommonContext(Factory);

			return new CargoReceiptAdvice(nameof(ForwardingShipment), "S00001001")
			{
				HIRReference = ZString.Empty,
				MarksAndNumbers = ZString.Empty,
				BookingParty = AddressBuilder.Create(context, (object)null),
				DepartureCFSAddress = AddressBuilder.Create(context, (object)null),
				PackingLines = Array.Empty<PackingLine>()
			};
		}
	}
}
