using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingContainer))]
	internal class BillOfLadingContainerBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			return shipment.RealContainers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.NewWithValidTestData<BillOfLadingContainer>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var booking = factory.New<BillOfLading>();
			return booking.BookedContainers.AddNew();
		}
		#endregion
	}
}
