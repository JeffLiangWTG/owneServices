using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Testing
{
	sealed class EmailDeliveryTest : TestCaseWithFactory
	{
		public void TestDeliver()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var emailDelivery = new EmailDelivery();

			var context = new DeliveryContext(Factory);
			context.ParentInfo = EntityInfo.New(dummy);
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Destination = "test@test.com";
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				emailDelivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(stream));
				Factory.Save();
			}

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			email.SetupBusinessEntityInfo(dummy.PK, dummy.TablePrefix, dummy.HumanReadableName);
		}

		public void TestDeliverToMultipleDestinations()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var emailDelivery = new EmailDelivery();

			var context = new DeliveryContext(Factory);
			context.ParentInfo = EntityInfo.New(dummy);
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Destination = "test1@test1.com, test2@test2.com,test3@test3.com,		test4@test4.com		 ";
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				emailDelivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(stream));
				Factory.Save();
			}

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals(4, email.Recipients.Count);
			AssertCollectionContains("test1@test1.com", email.Recipients);
			AssertCollectionContains("test2@test2.com", email.Recipients);
			AssertCollectionContains("test3@test3.com", email.Recipients);
			AssertCollectionContains("test4@test4.com", email.Recipients);
		}

		public void TestDeliverToMultipleDestinations_OneIsBlank()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var emailDelivery = new EmailDelivery();

			var context = new DeliveryContext(Factory);
			context.ParentInfo = EntityInfo.New(dummy);
			context.Notifications = new NotificationsForTest();
			context.ActionDescription = "Action Name";
			context.ParentHumanReadableName = "BusinessObject BO1001";
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Destination = "test1@test1.com, test2@test2.com,,test4@test4.com";
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				emailDelivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(stream));
				Factory.Save();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];

				AssertEquals(3, email.Recipients.Count);
				AssertCollectionContains("test1@test1.com", email.Recipients);
				AssertCollectionContains("test2@test2.com", email.Recipients);
				AssertCollectionContains("test4@test4.com", email.Recipients);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestDeliveryWithEmptyRecipients()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var emailDelivery = new EmailDelivery();
			var context = new DeliveryContext(Factory);
			context.ParentInfo = EntityInfo.New(dummy);
			context.Notifications = new NotificationsForTest();
			context.ActionDescription = "Action Name";
			context.ParentHumanReadableName = "BusinessObject BO1001";
			var mode = Factory.New<EDICommunicationsMode>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "JNC@test.com";
			staff.GS_Code = "JNC";
			Factory.Save();

			using (NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var resultDateTime = emailDelivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(stream));
				AssertNotEquals(ZDateTime.Empty, resultDateTime);

				var warning = "The evaluated email destination of Action Name in BusinessObject BO1001 is empty, please check the Email Address macro you used.";
				AssertContains(warning, ((NotificationsForTest)context.Notifications).GetWarnings());

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContains(warning, email.Body);
				Assert(email.Recipients.Contains("JNC@test.com"));
			}
		}
	}
}
