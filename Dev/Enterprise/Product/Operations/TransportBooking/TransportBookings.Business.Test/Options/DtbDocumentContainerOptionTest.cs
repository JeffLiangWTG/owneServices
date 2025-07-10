
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business.Options;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbDocumentContainerOption))]
	public class DtbDocumentContainerOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var documentContainer = new DtbDocumentContainerOption("CONT123456", "20GP", "1238", 10, "RELEASENUM123");
			AssertEquals("CONT123456", documentContainer.ContainerNumber);
			AssertEquals("20GP", documentContainer.ContainerType);
			AssertEquals("1238", documentContainer.Seal);
			AssertEquals(10, documentContainer.Link);
			AssertEquals("RELEASENUM123", documentContainer.ReleaseNumber);
			AssertEquals(true, documentContainer.DeliverContainer);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new DtbDocumentContainerOption("CONT123456", "20GP", "1238", 10, "RELEASENUM123");
		}
	}
}
