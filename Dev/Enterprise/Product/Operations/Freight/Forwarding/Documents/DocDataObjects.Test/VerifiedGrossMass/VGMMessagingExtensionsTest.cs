using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	class VGMMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessage_NoMessageSent()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();
			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			Assert(!extensions.IsSendingAmendment().Value);
			Assert("should allow to send", res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestContinueWithSendingMessage_OneContainerSentNoReply()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");

			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();

			Factory.Save();

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			Assert(extensions.IsSendingAmendment().Value);
			Assert("should not allow to send", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The following container numbers cannot be sent because responses have not yet been received. 00001", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessage_OneContainerSentAndAccepted()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");
			CreateEvent(bizObj1, Events.InterchangeSent, "00001");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			Assert(extensions.IsSendingAmendment().Value);
			Assert("should not allow to send", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The following container numbers cannot be sent because responses have not yet been received. 00001", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessage_PromptForValidCarrierOnOceanCarrierMessaging()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dynamicData = new Mock<IDynamicData>();
				var document = new Mock<IDocument>();
				var notifications = new Mock<IUserNotifications>();
				var messageInstructions = new Mock<IMessageInstructions>();

				var consol = CreateConsol();

				var vgm = new VerifiedGrossMass("aaa", "aaa")
				{
					Containers = new[]
					{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), consol)
					{
						Number = "00001"
					}
				}
				};

				dynamicData
					.SetupGet(d => d.Value)
					.Returns(vgm);

				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);

				var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
				var res = extensions.ContinueWithSendingMessage(notifications.Object);

				AssertEquals("disallow sending VGM when the carrier doesn't have messaging capability", false, res);

				dynamicData.Verify(fake => fake.Value, Times.Once);
				document.Verify(fake => fake.Data, Times.Once);
				notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Verified Gross Container Weight message to MAERSK that cannot receive this message electronically.
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

				var consol = CreateConsol(true);

				var vgm = new VerifiedGrossMass("aaa", "aaa")
				{
					Containers = new[]
					{
						new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), consol)
						{
							Number = "00001"
						}
					}
				};

				dynamicData
					.SetupGet(d => d.Value)
					.Returns(vgm);

				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);

				routingRuleValidator
					.Setup(r => r.IsValid(It.IsAny<string>()))
					.Returns(false);

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
					var res = extensions.ContinueWithSendingMessage(notifications.Object);

					AssertEquals("disallow sending VGM when the routing rule validation is invalid", false, res);

					notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Verified Gross Container Weight message to MAERSK, however your Organization/Branch is not registered with this carrier for this message type.
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

				var consol = CreateConsol(true);

				var vgm = new VerifiedGrossMass("aaa", "aaa")
				{
					Containers = new[]
					{
						new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), consol)
						{
							Number = "00001"
						}
					}
				};

				dynamicData
					.SetupGet(d => d.Value)
					.Returns(vgm);

				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);

				routingRuleValidator
					.Setup(r => r.IsValid(It.IsAny<string>()))
					.Returns(false);

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
					var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

					AssertEquals("disallow sending VGM withdrawal when the routing rule validation is invalid", false, res);

					notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Verified Gross Container Weight message to MAERSK, however your Organization/Branch is not registered with this carrier for this message type.
Please raise an eRequest and we will guide you through the registration process.", "Confirmation"), Times.Once);
				}
			}
		}

		public void TestContinueWithAmendmentMessage_OneContainerSent()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			notifications
				.Setup(n => n.ShowMessage("", ""));

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			Assert(extensions.IsSendingAmendment().Value);
			Assert("should not allow to send", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		}

		public void TestContinueWithAmendmentMessage_OneContainerSentAndAccepted()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");
			CreateEvent(bizObj1, Events.InterchangeSent, "00001");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();
			AddShippingLineAddressWithC1CCodeForConsol(consol);

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			Assert(extensions.IsSendingAmendment().Value);
			Assert("should allow to send", res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment_TwoContainerSentWithOneAccepted()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");

			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj2, Events.MessageSent, "00002");
			CreateEvent(bizObj2, Events.InterchangeSent, "00002");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();
			AddShippingLineAddressWithC1CCodeForConsol(consol);

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			Assert(extensions.IsSendingAmendment().Value);
			Assert("should not allow to send", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The following container numbers cannot be sent because responses have not yet been received or original has not yet been sent. 00001", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_TwoContainerSentWithOneAcceptedOrOneRejected()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");
			CreateEvent(bizObj1, Events.InterchangeSent, "00001");

			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj2, Events.MessageSent, "00002");
			CreateEvent(bizObj2, Events.InterchangeRejected, "00002");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();
			AddShippingLineAddressWithC1CCodeForConsol(consol);

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			Assert(extensions.IsSendingAmendment().Value);
			Assert("should not allow to send", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The following container numbers cannot be sent because responses have not yet been received or original has not yet been sent. 00002", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_HasContainerChangedCarrier()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001", "CNCN");

			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj2, Events.MessageSent, "00002", "AUAU");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();
			AddShippingLineAddressWithC1CCodeForConsol(consol, "CNCN");

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			Assert(extensions.IsSendingAmendment().Value);
			Assert("should not allow to send", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("You are trying to send a Verified Gross Container Weight amendment message to a new carrier. Please verify Carrier and Reset to Original if you are sending Verified Gross Container Weight to new carrier.", "Confirmation"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_AnyCarrierChanged_OldMessage()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001", string.Empty);

			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj2, Events.MessageSent, "00002", string.Empty);

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			Assert(extensions.IsSendingAmendment().Value);
			Assert("should not allow to send", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("You are trying to send a Verified Gross Container Weight amendment message to a new carrier. Please verify Carrier and Reset to Original if you are sending Verified Gross Container Weight to new carrier.", "Confirmation"), Times.Never);

			AddShippingLineAddressWithC1CCodeForConsol(consol, "CNCN");

			extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			notifications.Verify(n => n.ShowMessage("You are trying to send a Verified Gross Container Weight amendment message to a new carrier. Please verify Carrier and Reset to Original if you are sending Verified Gross Container Weight to new carrier.", "Confirmation"), Times.Never);
		}

		public void TestContinueWithResetToOriginal_NoMessageSent()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();

			Factory.Save();

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			Assert("should not allow to reset to original", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The following containers cannot be reset to original because they have not yet been sent. 00001, 00002", "Information"), Times.Once);
		}

		public void TestContinueWithResetToOriginal_OneMessageSentOneNot()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");

			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			Assert("should not allow to reset to original", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		}

		public void TestContinueWithResetToOriginal_MessageSentAndAccepted()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");
			CreateEvent(bizObj1, Events.InterchangeSent, "00001");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			Assert("should fallback to default functionality", !res.HasValue);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestContinueWithResetToOriginal_MessageSentAndRejected()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");
			CreateEvent(bizObj1, Events.InterchangeRejected, "00001");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			Assert("should not allow to reset to original", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The following containers cannot be reset to original because they have not yet been sent. 00001", "Information"), Times.Once);
		}

		public void TestContinueWithResetToOriginal_OneMessageSendAndAcceptedAnotherOneSentAndRejected()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");
			CreateEvent(bizObj1, Events.InterchangeRejected, "00001");

			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj2, Events.MessageSent, "00002");
			CreateEvent(bizObj2, Events.InterchangeSent, "00002");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new VGMMessagingExtensions(document.Object, CreateConsol(), messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			Assert("should not allow to reset to original", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The following containers cannot be reset to original because they have not yet been sent. 00001", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_MessageNotSend()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();
			var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = orgAddress.PK;

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			Assert("should not allow to withdraw", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The withdrawal message of the following container numbers cannot be sent because responses have not yet been received or there is no message to withdraw. 00001", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_MessageSendNoResponse()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj, Events.MessageSent, "00001");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();
			var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = orgAddress.PK;

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			Assert("should not allow to withdraw", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The withdrawal message of the following container numbers cannot be sent because responses have not yet been received or there is no message to withdraw. 00001", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_MessageSendWithResponse()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj, Events.MessageSent, "00001");
			CreateEvent(bizObj, Events.InterchangeSent, "00001");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj)
					{
						Number = "00001"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();
			var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = orgAddress.PK;

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			Assert("should allow to withdraw", res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The withdrawal message of the following container numbers cannot be sent because responses have not yet been received or there is no message to withdraw. 00001", "Information"), Times.Never);
		}

		public void TestContinueWithSendingMessageWithdrawal_OneMessageSendAndAcceptedAnotherOneSentAndRejected()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj1 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj1, Events.MessageSent, "00001");
			CreateEvent(bizObj1, Events.InterchangeRejected, "00001");

			var bizObj2 = Factory.New<DummyEnterpriseBusinessObject>();
			CreateEvent(bizObj2, Events.MessageSent, "00002");
			CreateEvent(bizObj2, Events.InterchangeSent, "00002");

			var vgm = new VerifiedGrossMass("aaa", "aaa")
			{
				Containers = new[]
				{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj1)
					{
						Number = "00001"
					},
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj2)
					{
						Number = "00002"
					}
				}
			};

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(vgm);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var consol = CreateConsol();
			var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = orgAddress.PK;

			var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			Assert("should not allow to withdraw", !res.Value);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(n => n.ShowMessage("The withdrawal message of the following container numbers cannot be sent because responses have not yet been received or there is no message to withdraw. 00001", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_IsCarrierChangedSinceLastSent()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dynamicData = new Mock<IDynamicData>();
				var document = new Mock<IDocument>();
				var notifications = new Mock<IUserNotifications>();
				var messageInstructions = new Mock<IMessageInstructions>();

				var bizObj = Factory.New<DummyEnterpriseBusinessObject>();
				CreateEvent(bizObj, Events.MessageSent, "00001");
				CreateEvent(bizObj, Events.InterchangeSent, "00001");

				var vgm = new VerifiedGrossMass("aaa", "aaa")
				{
					Containers = new[]
					{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj)
					{
						Number = "00001"
					}
				}
				};

				dynamicData
					.SetupGet(d => d.Value)
					.Returns(vgm);

				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);

				var consol = CreateConsol();

				var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
				var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
				Assert(!res.Value);
				notifications.Verify(x => x.ShowMessage("You are trying to send a Verified Gross Container Weight withdrawal message to a new carrier. Sending Verified Gross Container Weight withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Once);

				var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_OA_ShippingLineAddress = orgAddress.PK;

				res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
				Assert(res.Value);

				orgAddress.Header.CustomsCodes[0].OK_CustomsRegNo = "BCDF";
				notifications = new Mock<IUserNotifications>();
				res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
				Assert(!res.Value);
				notifications.Verify(x => x.ShowMessage("You are trying to send a Verified Gross Container Weight withdrawal message to a new carrier. Sending Verified Gross Container Weight withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Once);
			}
		}

		public void TestContinueWithSendingMessageWithdrawal_IsCarrierChangedSinceLastSent_OldMessage()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dynamicData = new Mock<IDynamicData>();
				var document = new Mock<IDocument>();
				var notifications = new Mock<IUserNotifications>();
				var messageInstructions = new Mock<IMessageInstructions>();

				var bizObj = Factory.New<DummyEnterpriseBusinessObject>();
				CreateEvent(bizObj, Events.MessageSent, "00001", string.Empty);
				CreateEvent(bizObj, Events.InterchangeSent, "00001", string.Empty);

				var vgm = new VerifiedGrossMass("aaa", "aaa")
				{
					Containers = new[]
					{
					new VGMMessagingContainer(new Container(DefaultDataObjectWriterStrategy.TestInstance), bizObj)
					{
						Number = "00001"
					}
				}
				};

				dynamicData
					.SetupGet(d => d.Value)
					.Returns(vgm);

				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);

				var consol = CreateConsol();

				var extensions = new VGMMessagingExtensions(document.Object, consol, messageInstructions.Object);
				var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
				Assert(res.Value);
				notifications.Verify(x => x.ShowMessage("You are trying to send a Verified Gross Container Weight withdrawal message to a new carrier. Sending Verified Gross Container Weight withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Never);

				var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_OA_ShippingLineAddress = orgAddress.PK;

				res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
				Assert(res.Value);

				orgAddress.Header.CustomsCodes[0].OK_CustomsRegNo = "BCDF";
				notifications = new Mock<IUserNotifications>();
				res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
				Assert(res.Value);
				notifications.Verify(x => x.ShowMessage("You are trying to send a Verified Gross Container Weight withdrawal message to a new carrier. Sending Verified Gross Container Weight withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Never);
			}
		}

		OrgAddress CreateOrgAddressWithCargoWiseOneCarrierCode(string customsRegNo = "ABCD")
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var orgCusCode = orgAddress.Header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			orgCusCode.OK_CustomsRegNo = customsRegNo;

			return orgAddress;
		}

		void CreateEvent(DummyEnterpriseBusinessObject logParent, Event @event, string containerNum, string companyParameterValue = "ABCD")
		{
			logParent.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent(containerNum, @event.Code, companyParameterValue));

			Factory.Save();
			Thread.Sleep(1);
		}

		KeyValuePair<string, string>[] GetParametersForEvent(string containerNum, string eventCode, string companyParameterValue)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				Core.Constants.EventReferenceMessageTypes.VerifiedGrossContainerWeight));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber,
				containerNum));

			HashSet<string> sentMessagesEventCodesHashSet = new HashSet<string>(MessageEventCodes.SentMessagesEventCodes);
			if (eventCode != null && sentMessagesEventCodesHashSet.Contains(eventCode) && !string.IsNullOrEmpty(companyParameterValue))
			{
				result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Company, companyParameterValue));
			}

			return result.ToArray();
		}

		ForwardingConsol CreateConsol(bool enabledVerifiedGrossContainerWeight = false)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "CONSOL";
			consol.JK_BookingReference = "REF";
			consol.JK_MasterBillNum = "WAYBILL";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			if (enabledVerifiedGrossContainerWeight)
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;

				carrier.OH_RSL_ShippingLine = shippingLine.PK;
			}

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			return consol;
		}

		void AddShippingLineAddressWithC1CCodeForConsol(ForwardingConsol consol, string c1CCode = "ABCD")
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var orgCusCode = orgAddress.Header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			orgCusCode.OK_CustomsRegNo = c1CCode;

			consol.JK_OA_ShippingLineAddress = orgAddress.PK;
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
	}
}
