using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingPlannedContainerCollection))]
	sealed class JobSupplierBookingPlannedContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestJobSupplierBookingContainer()
		{
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var container1 = booking.PlannedContainers.AddNew();
			container1.J1_RC = refContainer.PK;
			container1.J1_ContainerCount = 2;
			AssertEquals(booking.PK, container1.J1_ParentID);
			AssertEquals(booking.TablePrefix, container1.J1_ParentTableCode);

			var container2 = Factory.New<JobSupplierBookingPlannedContainer>();
			container2.J1_RC = refContainer.PK;
			container2.J1_ContainerCount = 2;
			AssertEquals(ZGuid.Empty, container2.J1_ParentID);
			AssertEquals(ZString.Empty, container2.J1_ParentTableCode);

			booking.PlannedContainers.Add(container2);
			AssertEquals(booking.PK, container2.J1_ParentID);
			AssertEquals(booking.TablePrefix, container2.J1_ParentTableCode);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			booking = otherFactory.Load<JobSupplierBooking>(booking.PK);
			AssertEquals(2, booking.PlannedContainers.Count);
			Assert(booking.PlannedContainers.Contains(container1.PK));
			Assert(booking.PlannedContainers.Contains(container2.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var booking = Factory.New<JobSupplierBooking>();
			return new JobSupplierBookingPlannedContainerCollection(booking, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobSupplierBookingPlannedContainer>();
		}
	}
}
