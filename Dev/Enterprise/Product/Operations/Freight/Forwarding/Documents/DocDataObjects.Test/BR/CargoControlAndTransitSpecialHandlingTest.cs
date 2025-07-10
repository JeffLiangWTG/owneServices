using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.BR.Testing
{
	[TestedType(typeof(CargoControlAndTransitSpecialHandling))]
	sealed class CargoControlAndTransitSpecialHandlingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new CargoControlAndTransitSpecialHandling(0);
	}
}
