using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestConflictResolution()
		{
			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			anotherFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			JobDocAddressPersistentParentForTesting parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			Factory.Save();

			JobDocAddressPersistentParentForTesting parentInAnotherFactory = anotherFactory.Load<JobDocAddressPersistentParentForTesting>(parent.PK);

			JobDocAddress address = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CustomsContainerTerminalOperatorAddress);
			address.E2_AddressOverride = true;
			address.E2_City = "Z1";

			JobDocAddress addressInAnotherFactory = parentInAnotherFactory.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CustomsContainerTerminalOperatorAddress);
			addressInAnotherFactory.E2_AddressOverride = true;
			addressInAnotherFactory.E2_City = "Z2";
			Factory.Save();

			AssertEquals("precondition: Parents should be the same", address.E2_ParentID, addressInAnotherFactory.E2_ParentID);
			Assert("precondition: Addresses should be in seporate factorys", address.Factory != addressInAnotherFactory.Factory);
			Assert("precondition: Should not be the same address", address.PK != addressInAnotherFactory.PK);

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			try
			{
				anotherFactory.Save();
				Fail("First save should not have succeeded.");
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				AssertEquals("User should have been notified of the problem.", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("User should have been notified of the problem.", $@"Another user has changed some address details, please review your changes and save again.
FriendlyName: Container Terminal Operator Address({address.E2_SystemCreateUser} @ {address.E2_SystemCreateTimeUtc.ToSmallDateTimeFloor()})
E2_SystemLastEditTimeUtc
E2_SystemCreateTimeUtc
E2_City", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Address2 should be deleted", true, addressInAnotherFactory.IsDeleted);

				anotherFactory.Save();
			}

			addressInAnotherFactory = parentInAnotherFactory.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CustomsContainerTerminalOperatorAddress);
			AssertEquals("Should be using the existing DocAddress now", address.PK, addressInAnotherFactory.PK);
		}

		public void TestConstructor_WithNullAddress_ShouldThrow()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobDocAddressUniqueIndexFailureHandler(null));
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestConflictResolution_WithNullParent_ShouldNotThrow()
		{
			var notification = new Mock<INotificationHandler>();
			notification.Setup(m => m.ReportError(
				"Another user has changed some address details, please review your changes and save again.\r\nUnspecified\r\n",
				"Save Error",
				It.IsAny<string>(),
				It.IsAny<Exception>()));
			var address = Factory.New<JobDocAddress>();
			AssertNull(address.Parent);
			var handler = new JobDocAddressUniqueIndexFailureHandler(address);
			handler.NotifyUserAndAttemptToResolve(notification.Object, handler.HandledUniqueIndexNames.Single());
			notification.VerifyAll();
		}
	}
}
