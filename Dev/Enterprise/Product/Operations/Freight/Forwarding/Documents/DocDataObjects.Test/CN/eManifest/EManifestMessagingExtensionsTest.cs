using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class EManifestMessagingExtensionsTest : TestCaseWithFactory
	{
		#region TestContinueWithSendingMessage_NoBookingsSent

		public void TestContinueWithSendingMessage_NoBookingsSent()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					BookingNumber = "BBB",
					Send = true
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("allow to send 2 originals", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("The following SLD numbers cannot be sent because responses have not yet been received. AAA, BBB", "Information"), Times.Never);
		}

		#endregion

		#region TestContinueWithSendingMessage_OneBookingSentNoReply

		public void TestContinueWithSendingMessage_OneBookingSentNoReply()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					BookingNumber = "BBB",
					Send = true
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("disallow sending eManifest when a response has not been received for a booking", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("The following SLD numbers cannot be sent because responses have not yet been received. AAA", "Information"), Times.Once);
		}

		#endregion

		#region TestContinueWithSendingMessage_OneBookingSentReceivedAcceptance

		public void TestContinueWithSendingMessage_OneBookingSentReceivedAcceptance()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.InterchangeSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					BookingNumber = "BBB",
					Send = true
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest)

				;

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("allow to send 1 orignal and 1 amendment", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("The following SLD numbers cannot be sent because responses have not yet been received. AAA", "Information"), Times.Never);
		}

		#endregion

		#region TestContinueWithSendingMessage_OneBookingSentReceivedRejection

		public void TestContinueWithSendingMessage_OneBookingSentReceivedRejection()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.InterchangeRejected,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					BookingNumber = "BBB",
					Send = true
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("allow to send 2 orignals", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("The following SLD numbers cannot be sent because responses have not yet been received. AAA", "Information"), Times.Never);
		}

		#endregion

		#region TestContinueWithSendingMessage_PromptForValidCarrierOnOceanCarrierMessaging

		public void TestContinueWithSendingMessage_PromptForValidCarrierOnOceanCarrierMessaging()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dynamicData = new Mock<IDynamicData>();
				var document = new Mock<IDocument>();
				var notifications = new Mock<IUserNotifications>();
				var messageInstructions = new Mock<IMessageInstructions>();

				var shipment = CreateShipment();

				shipment.Logs.CreateOrRecreateEventLog(
					Events.MessageSent,
					EstimateActual.Actual,
					ZDateTimeOffset.Now,
					ZString.Empty,
					GetParametersForEvent("AAA"));

				var eManifest = new EManifest("zzz", "zzz", "zzz");
				eManifest.Bookings = new[]
				{
				new Booking("AAA", shipment)
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", shipment)
				{
					BookingNumber = "BBB",
					Send = true
				}
			};

				dynamicData
					.SetupGet(d => d.Value)
					.Returns(eManifest);

				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);

				var extensions = new EManifestMessagingExtensions(document.Object, shipment, messageInstructions.Object);
				var res = extensions.ContinueWithSendingMessage(notifications.Object);

				AssertEquals("disallow sending eManifest when the carrier doesn't have messaging capability", false, res);

				dynamicData.Verify(fake => fake.Value, Times.Once);
				document.Verify(fake => fake.Data, Times.Once);
				notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the eManifest message to MAERSK that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);
			}
		}

		public void TestContinueWithSendingMessage_PromptForInvalidRoutingRule()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dynamicData = new Mock<IDynamicData>();
				var document = new Mock<IDocument>();
				var notifications = new Mock<IUserNotifications>();
				var messageInstructions = new Mock<IMessageInstructions>();
				var routingRuleValidator = new Mock<IRoutingRuleValidator>();

				var shipment = CreateShipment(true);

				shipment.Logs.CreateOrRecreateEventLog(
					Events.MessageSent,
					EstimateActual.Actual,
					ZDateTimeOffset.Now,
					ZString.Empty,
					GetParametersForEvent("AAA"));

				var eManifest = new EManifest("zzz", "zzz", "zzz");
				eManifest.Bookings = new[]
				{
					new Booking("AAA", shipment)
					{
						BookingNumber = "AAA",
						Send = true
					},
					new Booking("BBB", shipment)
					{
						BookingNumber = "BBB",
						Send = true
					}
				};

				dynamicData
					.SetupGet(d => d.Value)
					.Returns(eManifest);

				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);

				routingRuleValidator
					.Setup(r => r.IsValid(It.IsAny<string>()))
					.Returns(false);

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = new EManifestMessagingExtensions(document.Object, shipment, messageInstructions.Object);
					var res = extensions.ContinueWithSendingMessage(notifications.Object);

					AssertEquals("disallow sending eManifest when the routing rule validation is invalid", false, res);

					notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the eManifest message to MAERSK, however your Organization/Branch is not registered with this carrier for this message type.
Please raise an eRequest and we will guide you through the registration process.", "Confirmation"), Times.Once);
				}
			}
		}

		public void TestContinueWithSendingMessageWithdrawal_PromptForInvalidRoutingRule()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dynamicData = new Mock<IDynamicData>();
				var document = new Mock<IDocument>();
				var notifications = new Mock<IUserNotifications>();
				var messageInstructions = new Mock<IMessageInstructions>();
				var routingRuleValidator = new Mock<IRoutingRuleValidator>();

				var shipment = CreateShipment(true);

				shipment.Logs.CreateOrRecreateEventLog(
					Events.MessageSent,
					EstimateActual.Actual,
					ZDateTimeOffset.Now,
					ZString.Empty,
					GetParametersForEvent("AAA"));

				var eManifest = new EManifest("zzz", "zzz", "zzz");
				eManifest.Bookings = new[]
				{
					new Booking("AAA", shipment)
					{
						BookingNumber = "AAA",
						Send = true
					},
					new Booking("BBB", shipment)
					{
						BookingNumber = "BBB",
						Send = true
					}
				};

				dynamicData
					.SetupGet(d => d.Value)
					.Returns(eManifest);

				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);

				routingRuleValidator
					.Setup(r => r.IsValid(It.IsAny<string>()))
					.Returns(false);

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = new EManifestMessagingExtensions(document.Object, shipment, messageInstructions.Object);
					var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

					AssertEquals("disallow sending eManifest withdrawal when the routing rule validation is invalid", false, res);

					notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the eManifest message to MAERSK, however your Organization/Branch is not registered with this carrier for this message type.
Please raise an eRequest and we will guide you through the registration process.", "Confirmation"), Times.Once);
				}
			}
		}

		#endregion

		#region TestIsSendingAmendment_OneBookingAlreadySentAndAccepted

		public void TestIsSendingAmendment_OneBookingAlreadySentAndAccepted()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.InterchangeSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					BookingNumber = "BBB",
					Send = true
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.IsSendingAmendment();

			AssertEquals("one of the bookings has been sent and accepted there's we're sending an amendment", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}

		#endregion

		#region TestIsSendingAmendment_NoneSent

		public void TestIsSendingAmendment_NoneSent()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					BookingNumber = "BBB",
					Send = true
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.IsSendingAmendment();

			AssertEquals("no bookings have been sent", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}

		#endregion

		#region TestContinueWithSendingMessageWithdrawal_OtherSLDsInTheSameContainer_NoBookingSubmitted_CannotWithdraw
		public void TestContinueWithSendingMessageWithdrawal_OtherSLDsInTheSameContainer_NoBookingSubmitted_CannotWithdraw()
		{
			var context = new CommonContext(Factory);
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNNGB" };
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					Containers = new List<BookingContainer>()
					{
						new BookingContainer(new ZGuid())
						{
							Number = "1234"
						}
					},
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					Containers = new List<BookingContainer>()
					{
						new BookingContainer(new ZGuid())
						{
							Number = "1234"
						}
					},
					BookingNumber = "BBB",
					Send = false
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			Assert("withdrawal cancelled.", !res.GetValueOrDefault());
			AssertEquals("1 bookings are selected", eManifest.Bookings.Where(b => b.Send).Count(), 1);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Exactly(2));
			notifications.Verify(fake => fake.ShowMessage("The following selected booking have not been submitted or received any reply yet. Sending withdrawal message may cause errors and possibly revert to a manual process. Please wait for reply before sending.\r\nAAA", "Information"), Times.Once);
		}
		#endregion

		#region TestContinueWithSendingMessageWithdrawal_OtherSLDsInTheSameContainer_OneBookingSubmitted_CannotWithdraw

		public void TestContinueWithSendingMessageWithdrawal_OtherSLDsInTheSameContainer_OneBookingSubmitted_CanWithdraw()
		{
			var context = new CommonContext(Factory);
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.InterchangeSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now.AddHours(1),
				ZString.Empty,
				GetParametersForEvent("AAA"));

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNNGB" };
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					Containers = new List<BookingContainer>()
					{
						new BookingContainer(new ZGuid())
						{
							Number = "1234"
						}
					},
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					Containers = new List<BookingContainer>()
					{
						new BookingContainer(new ZGuid())
						{
							Number = "1234"
						}
					},
					BookingNumber = "BBB",
					Send = false
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			Assert("withdrawal cancelled.", res.GetValueOrDefault());
			AssertEquals("1 bookings are selected", eManifest.Bookings.Where(b => b.Send).Count(), 1);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Exactly(2));
		}

		#endregion

		#region TestContinueWithSendingMessageWithdrawal_OtherSLDsInTheSameContainer_BothBookingSubmitted_CanWithdraw

		public void TestContinueWithSendingMessageWithdrawal_OtherSLDsInTheSameContainer_BothBookingSubmitted_CanWithdraw()
		{
			var context = new CommonContext(Factory);
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.InterchangeSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now.AddHours(1),
				ZString.Empty,
				GetParametersForEvent("AAA"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("BBB"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.InterchangeSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now.AddHours(1),
				ZString.Empty,
				GetParametersForEvent("BBB"));

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNNGB" };
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					Containers = new List<BookingContainer>()
					{
						new BookingContainer(new ZGuid())
						{
							Number = "1234"
						}
					},
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					Containers = new List<BookingContainer>()
					{
						new BookingContainer(new ZGuid())
						{
							Number = "1234"
						}
					},
					BookingNumber = "BBB",
					Send = false
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			notifications
				.Setup(n => n.ShowConfirmation("SLD/s selected are packed into Container/s with other already submitted SLD's, and as a result all previously submitted SLD/s for this Container/s will also be withdrawn: \r\nAAA, BBB", "Confirmation"))
				.Returns(true);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			Assert("Should be able to withdraw", res.GetValueOrDefault());
			AssertEquals("2 bookings are selected", eManifest.Bookings.Where(b => b.Send).Count(), 2);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Exactly(2));
			notifications.Verify(fake => fake.ShowConfirmation("SLD/s selected are packed into Container/s with other already submitted SLD's, and as a result all previously submitted SLD/s for this Container/s will also be withdrawn: \r\nAAA, BBB", "Confirmation"), Times.Once);
			notifications.Verify(fake => fake.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		#endregion

		#region TestContinueWithSendingMessageWithdrawal_OtherSLDsInTheSameContainer_BothBookingSubmitted_ValidationError_CanNotWithdraw

		public void TestContinueWithSendingMessageWithdrawal_OtherSLDsInTheSameContainer_BothBookingSubmitted_ValidationError_CanNotWithdraw()
		{
			var context = new CommonContext(Factory);
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("AAA"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.InterchangeSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now.AddHours(1),
				ZString.Empty,
				GetParametersForEvent("AAA"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent("BBB"));

			bizObj.Logs.CreateOrRecreateEventLog(
				Events.InterchangeSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now.AddHours(1),
				ZString.Empty,
				GetParametersForEvent("BBB"));

			var eManifest = new EManifest("zzz", "zzz", "zzz");
			eManifest.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNNGB" };
			eManifest.Bookings = new[]
			{
				new Booking("AAA", bizObj)
				{
					Containers = new List<BookingContainer>()
					{
						new BookingContainer(new ZGuid())
						{
							Number = "1234"
						}
					},
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB", bizObj)
				{
					Containers = new List<BookingContainer>()
					{
						new BookingContainer(new ZGuid())
						{
							Number = "1234"
						}
					},
					BookingNumber = "BBB",
					Send = false
				}
			};

			eManifest.Bookings.First().BookingNumberInfo.AddMessageError("Test Error");

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(eManifest);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			notifications
				.Setup(n => n.ShowConfirmation("SLD/s selected are packed into Container/s with other already submitted SLD's, and as a result all previously submitted SLD/s for this Container/s will also be withdrawn: \r\nAAA, BBB", "Confirmation"))
				.Returns(true);

			var extensions = new EManifestMessagingExtensions(document.Object, CreateShipment(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			Assert("Should not be able to withdraw", !res.GetValueOrDefault());
			AssertEquals("2 bookings are selected", eManifest.Bookings.Where(b => b.Send).Count(), 2);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Exactly(2));
			notifications.Verify(fake => fake.ShowConfirmation("SLD/s selected are packed into Container/s with other already submitted SLD's, and as a result all previously submitted SLD/s for this Container/s will also be withdrawn: \r\nAAA, BBB", "Confirmation"), Times.Once);
			notifications.Verify(fake => fake.ShowMessage("The document contains validation errors, please fix them and withdraw again.", "Errors"), Times.Once);
		}

		#endregion

		#region Implementation

		KeyValuePair<string, string>[] GetParametersForEvent(string bookingNumber)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				"eManifest"));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber,
				bookingNumber));

			return result.ToArray();
		}

		ForwardingShipment CreateShipment(bool enabledEManifest = false)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_NoOriginalBills = 3;
			consol1.JK_NoCopyBills = 4;
			consol1.JK_RL_NKLoadPort = "CNSHA";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			if (enabledEManifest)
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_EManifestAvailable = true;

				carrier.OH_RSL_ShippingLine = shippingLine.PK;
			}

			consol1.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "AUBNE";

			return shipment;
		}

		protected override void SetUp()
		{
			base.SetUp();
			FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.DefaultValue);
		}

		#endregion
	}
}
