using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentConversationProviderTest : TestCaseWithFactory
	{
		public void TestConversation()
		{
			var provider = (IConversationProvider)shipment;
			Factory.Save();

			var conversation = provider.eConversation;
			var expectedConversation = JobConversation.GetConversation(shipment);

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestConversation_NotSaved()
		{
			var provider = (IConversationProvider)shipment;

			AssertNull(provider.eConversation);
		}

		public void TestConversation_AlreadyExists()
		{
			var provider = (IConversationProvider)shipment;
			Factory.Save();

			var expectedConversation = JobConversation.GetOrCreate(shipment);

			var conversation = provider.eConversation;

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestParentModule()
		{
			var provider = (IConversationProvider)shipment;

			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = false;
			AssertEquals(ModuleIDs.JobShipment, provider.ParentModule);

			shipment.JS_IsBooking = true;
			AssertEquals(ModuleIDs.QuotedBookings, provider.ParentModule);

			shipment.JS_IsForwardRegistered = true;
			AssertEquals(ModuleIDs.JobShipment, provider.ParentModule);
		}

		public void TestParentController()
		{
			var provider = (IConversationProvider)shipment;

			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = false;
			AssertEquals(ControllerIDs.JobShipment, provider.ParentController);

			shipment.JS_IsBooking = true;
			AssertEquals(ControllerIDs.QuotedBookings, provider.ParentController);

			shipment.JS_IsForwardRegistered = true;
			AssertEquals(ControllerIDs.JobShipment, provider.ParentController);
		}

		public void TestAdditionalParticipants()
		{
			var provider = (IConversationProvider)shipment;

			AssertSequencesEqual(Enumerable.Empty<EConversation.Business.RelatedParty>(), provider.AdditionalParticipants);
		}

		public void TestSendEmailNotificationsOnSave()
		{
			var provider = (IConversationProvider)shipment;

			Assert(provider.SendEmailNotificationsOnSave);
		}

		public void TestEmailSubjectContentOverride()
		{
			var provider = (IConversationProvider)shipment;

			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = false;
			AssertEquals($"Shipment {shipment.JS_UniqueConsignRef}", provider.EmailSubjectContentOverride);

			shipment.JS_IsBooking = true;
			AssertEquals($"Forwarding Booking {shipment.JS_UniqueConsignRef}", provider.EmailSubjectContentOverride);

			shipment.JS_IsForwardRegistered = true;
			AssertEquals($"Shipment {shipment.JS_UniqueConsignRef}", provider.EmailSubjectContentOverride);
		}

		public void TestFromAddressOverride()
		{
			var provider = (IConversationProvider)shipment;
			AssertNull(provider.FromAddressOverride);

			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.NeoConversationsEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "conversations@neo.cargowise.com"))
			{
				AssertEquals("conversations@neo.cargowise.com", provider.FromAddressOverride);
			}
		}

		public void TestGetAdditionalParticipants_NoStaffSubscribers()
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var provider = (IConversationAdditionalParticipantProvider)shipment;

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var additionalParticipants = new List<IConversationParticipant> { staff };
				var mockParticipantProvider = new Mock<IForwardingShipmentParticipantProvider>();
				ObjectFactory.Substitute(mockParticipantProvider.Object);

				var contact = Factory.NewWithValidTestData<OrgContact>();
				var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { contact });
				var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
				participant.JCP_ParticipantTableCode = contact.TablePrefix;
				participant.ParentKey = contact.PK.ToString();

				mockParticipantProvider.Setup(x => x.GetAdditionalParticipants(shipment, participant)).Returns(additionalParticipants);

				AssertSequencesEqual(additionalParticipants, provider.GetAdditionalParticipants(subscribedParticipants, participant));
			}
		}

		public void TestGetAdditionalParticipants_NeoConversationsDisabled()
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var provider = (IConversationAdditionalParticipantProvider)shipment;

				var mockParticipantProvider = new Mock<IForwardingShipmentParticipantProvider>(MockBehavior.Strict);
				ObjectFactory.Substitute(mockParticipantProvider.Object);

				var contact = Factory.NewWithValidTestData<OrgContact>();
				var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { contact });

				AssertSequencesEqual(Enumerable.Empty<IConversationParticipant>(), provider.GetAdditionalParticipants(subscribedParticipants, Factory.NewWithValidTestData<JobConversationParticipant>()));
			}
		}

		public void TestGetAdditionalParticipants_StaffSubscriber()
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var provider = (IConversationAdditionalParticipantProvider)shipment;

				var additionalParticipants = new List<IConversationParticipant>();
				var mockParticipantProvider = new Mock<IForwardingShipmentParticipantProvider>();
				ObjectFactory.Substitute(mockParticipantProvider.Object);

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { staff });

				var contact = Factory.NewWithValidTestData<OrgContact>();
				var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
				participant.JCP_ParticipantTableCode = contact.TablePrefix;
				participant.ParentKey = contact.PK.ToString();

				mockParticipantProvider.Setup(x => x.GetAdditionalParticipants(shipment, participant)).Returns(additionalParticipants);

				AssertSequencesEqual(Enumerable.Empty<IConversationParticipant>(), provider.GetAdditionalParticipants(subscribedParticipants, participant));
			}
		}

		public void TestShouldUseThisProviderForHyperlink()
		{
			var provider = (IConversationParentHyperlinkProvider)shipment;

			Assert(provider.ShouldUseThisProviderForHyperlink(null));
			Assert(provider.ShouldUseThisProviderForHyperlink(Factory.New<OrgContact>()));
			Assert(!provider.ShouldUseThisProviderForHyperlink(Factory.New<GlbStaff>()));
		}

		public void TestGetHyperlinkToConversationParent()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals/"))
			{
				var provider = (IConversationParentHyperlinkProvider)shipment;

				AssertEquals($"https://glowdev/Portals/NEO/Desktop#/formFlow/default/IJobShipment/{shipment.PK}", provider.GetHyperlinkToConversationParent());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		}
		ForwardingShipment shipment;
	}
}
