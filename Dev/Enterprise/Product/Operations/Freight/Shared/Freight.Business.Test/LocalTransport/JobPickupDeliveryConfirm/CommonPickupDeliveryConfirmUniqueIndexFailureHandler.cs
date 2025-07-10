using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonPickupDeliveryConfirmUniqueIndexFailureHandler : TestCaseWithFactory
	{
		public void TestConflictResolution()
		{
			var (newFactory, containerInNewFactory, confirmInNewFactory) = SetupConflictData();

			try
			{
				newFactory.Save();
				Fail("First save should not have succeeded");
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);

				CombineAssertions(delegate
				{
					AssertEquals("Another user has made changes to the confirmation. Please review your changes and save again.\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert("Second confirmation should be deleted", confirmInNewFactory.IsDeleted);
				});
			}

			newFactory.Save();

			CombineAssertions(delegate
			{
				AssertEquals("Tom", containerInNewFactory.Confirms[0].EU_DriversName);
				AssertEquals(new ZDateTime(2019, 1, 1, 10, 0, 0), containerInNewFactory.Confirms[0].EU_PickupDeliveryTime);
				AssertEquals(ZDateTime.Empty, containerInNewFactory.Confirms[0].EU_RequestedPickupDeliveryTime);
			});
		}

		public void TestConflictResolution_ServiceTask()
		{
			Globals.SetIsUserInteractiveForTest(false);
			var (newFactory, containerInNewFactory, confirmInNewFactory) = SetupConflictData();

			try
			{
				newFactory.Save();
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);

				CombineAssertions(delegate
				{
					AssertEquals("Another user has made changes to the confirmation.\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("Second confirmation should be deleted", confirmInNewFactory.IsDeleted);
				});
			}

			newFactory.Save();

			CombineAssertions(delegate
			{
				AssertEquals("Tom", containerInNewFactory.Confirms[0].EU_DriversName);
				AssertEquals(new ZDateTime(2019, 1, 1, 10, 0, 0), containerInNewFactory.Confirms[0].EU_PickupDeliveryTime);
				AssertEquals(ZDateTime.Empty, containerInNewFactory.Confirms[0].EU_RequestedPickupDeliveryTime);
			});
		}

		#region Helpers

		(BusinessObjectFactory factory, CommonContainer container, CommonPickupDeliveryConfirm confirm) SetupConflictData()
		{
			var newFactory = new BusinessObjectFactory();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = Factory.NewWithValidTestData<CommonContainer>();
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			consol.Containers.Add(container);
			consol.Shipments.Add(shipment);

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 1;
			packline.SetContainer(container.PK);

			Factory.Save();

			var containerInNewFactory = newFactory.Load<CommonContainer>(container.PK);

			var confirm = container.OriginConfirm;
			confirm.EU_PickupDeliveryTime = new ZDateTime(2019, 1, 1, 10, 0, 0);
			confirm.EU_DriversName = "Tom";

			var confirmInNewFactory = containerInNewFactory.OriginConfirm;
			confirmInNewFactory.EU_RequestedPickupDeliveryTime = new ZDateTime(2019, 1, 1, 12, 0, 0);
			confirmInNewFactory.EU_DriversName = "Jerry";

			Factory.Save();

			return (newFactory, containerInNewFactory, confirmInNewFactory);
		}

		#endregion
	}
}
