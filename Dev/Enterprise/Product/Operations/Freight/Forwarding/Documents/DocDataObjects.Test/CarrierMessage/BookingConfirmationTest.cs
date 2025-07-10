using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(BookingConfirmation))]
	sealed class BookingConfirmationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new CommonContext(Factory);

			return new BookingConfirmation(ZGuid.Empty)
			{
				Carrier = AddressBuilder.Create(context, (object)null),
				Shipper = AddressBuilder.Create(context, (object)null),
				Containers = new List<ImportableWrapper<Container>>(),
				CarrierBookingReference = new ImportableZTypeWrapper<ZString>(ZString.Empty),
				Transports = new ImportableWrapper<IReadOnlyCollection<Transport>>(new List<Transport>()),
				CarrierConfirmationNotes = new ImportableZTypeWrapper<ZString>(ZString.Empty),
				BillOfLadingNumber = new ImportableZTypeWrapper<ZString>(ZString.Empty),
				CarrierContractNumber = new ImportableZTypeWrapper<ZString>(ZString.Empty),
				ContainerTerminalOperator = new ImportableWrapper<Address>(AddressBuilder.Create(context, (object)null)),
			};
		}
	}
}
