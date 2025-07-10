using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(InstructionToSelectFromForPrintingCollection))]
	sealed class InstructionToSelectFromForPrintingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InstructionToSelectFromForPrintingCollection>
	{
		protected override InstructionToSelectFromForPrintingCollection GetCollectionToTest()
		{
			return new InstructionToSelectFromForPrintingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InstructionToSelectFromForPrinting(Factory.New<DtbBookingInstruction>());
		}

		public void TestCreateNonPersistentBusinessObjectNotSupported()
		{
			var collection = new InstructionToSelectFromForPrintingCollection(Factory);
			AssertExceptionThrown(typeof(NotSupportedException), () => collection.AddNew());
		}
	}
}
