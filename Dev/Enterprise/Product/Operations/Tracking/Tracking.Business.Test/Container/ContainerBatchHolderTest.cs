using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(ContainerBatchHolder))]
	public class ContainerBatchHolderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestApply()
		{
			TrackingContainer[] containers = new TrackingContainer[3];
			containers[0] = Factory.NewWithValidTestData<TrackingContainer>();
			containers[1] = Factory.NewWithValidTestData<TrackingContainer>();
			containers[2] = Factory.NewWithValidTestData<TrackingContainer>();

			ContainersForTest.AddRange(containers);

			ZDateTime testDate = ZDateTime.Now.AddDays(-1);

			BatchHolderForTest.RequiredDeliveryDate = testDate;
			BatchHolderForTest.ConfirmedDeliveryDate = testDate;
			BatchHolderForTest.ActualDeliveryDate = testDate;
			BatchHolderForTest.EstimatedDehireDate = testDate;
			BatchHolderForTest.EmptyPickup = testDate;
			BatchHolderForTest.ActualDehireDate = testDate;

			BatchHolderForTest.Apply();

			foreach (TrackingContainer container in containers)
			{
				AssertEquals("RequiredDelivery", testDate, container.RequiredDelivery);
				AssertEquals("ConfirmedDelivery", testDate, container.ConfirmedDelivery);
				AssertEquals("ActualDelivery", testDate, container.ActualDelivery);
				AssertEquals("EstimatedDehire", testDate, container.EmptyReady);
				AssertEquals("Pickup", testDate, container.EmptyPickup);
				AssertEquals("ActualDehire", testDate, container.ActualDehire);
			}

			ZDateTime testDate1 = ZDateTime.Now;

			BatchHolderForTest.RequiredDeliveryDate = ZDateTime.Empty;
			BatchHolderForTest.ConfirmedDeliveryDate = testDate1;
			BatchHolderForTest.ActualDeliveryDate = ZDateTime.Empty;
			BatchHolderForTest.EstimatedDehireDate = testDate1;
			BatchHolderForTest.EmptyPickup = ZDateTime.Empty;
			BatchHolderForTest.ActualDehireDate = testDate1;

			BatchHolderForTest.Apply();

			foreach (TrackingContainer container in containers)
			{
				AssertEquals("RequiredDelivery", testDate, container.RequiredDelivery);
				AssertEquals("ConfirmedDelivery", testDate1, container.ConfirmedDelivery);
				AssertEquals("ActualDelivery", testDate, container.ActualDelivery);
				AssertEquals("EstimatedDehire", testDate1, container.EmptyReady);
				AssertEquals("Pickup", testDate, container.EmptyPickup);
				AssertEquals("ActualDehire", testDate1, container.ActualDehire);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContainerBatchHolder(ContainersForTest);
		}

		protected ContainerBatchHolder BatchHolderForTest
		{
			get
			{
				if (batchHolderForTest == null)
				{
					batchHolderForTest = GetNewBusinessObject() as ContainerBatchHolder;
				}
				return batchHolderForTest;
			}
		}
		ContainerBatchHolder batchHolderForTest;

		protected TrackingContainerStandaloneCollection ContainersForTest
		{
			get
			{
				if (containersForTest == null)
				{
					containersForTest = new TrackingContainerStandaloneCollection(Factory);
				}
				return containersForTest;
			}
		}
		TrackingContainerStandaloneCollection containersForTest;

		#endregion
	}
}
