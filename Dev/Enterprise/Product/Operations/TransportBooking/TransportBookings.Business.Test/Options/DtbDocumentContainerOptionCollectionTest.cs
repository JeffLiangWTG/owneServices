using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business.Options;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbDocumentContainerOptionCollection))]
	public class DtbDocumentContainerOptionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DtbDocumentContainerOptionCollection>
	{
		public void TestAllowNew()
		{
			var collection = new DtbDocumentContainerOptionCollection();
			AssertEquals(false, collection.AllowNew);
		}

		protected override DtbDocumentContainerOptionCollection GetCollectionToTest()
		{
			return new DtbDocumentContainerOptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DtbDocumentContainerOption("CONT123456", "20GP", "1238", 10, "RELEASENUM123");
		}
	}
}
