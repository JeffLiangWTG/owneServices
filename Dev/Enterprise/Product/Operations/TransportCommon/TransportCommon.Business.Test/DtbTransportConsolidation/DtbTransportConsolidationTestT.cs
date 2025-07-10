using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportConsolidationTest : DtbTransportBusinessObjectTestCase
	{
		#region TestLoadingAsWrongTypeWithoutFactorySave

		public void TestLoadingAsWrongTypeWithoutFactorySave()
		{
			var typesToLoad = new[] { ObjectFactory.GetType<IDtbBookingConsolidation>(), ObjectFactory.GetType<IDtbConsignmentConsolidation>() };
			AssertLoadingAsWrongType(typesToLoad, factorySaveBeforeLoad: false);
		}

		#endregion

		#region TestLoadingAsWrongTypeWithFactorySave

		public void TestLoadingAsWrongTypeWithFactorySave()
		{
			var typesToLoad = new[] { ObjectFactory.GetType<IDtbBookingConsolidation>(), ObjectFactory.GetType<IDtbConsignmentConsolidation>() };
			AssertLoadingAsWrongType(typesToLoad, factorySaveBeforeLoad: true);
		}

		#endregion

		#region SetDefaultValues

		#region TestSetDefaultValues_SetsJobType

		public void TestSetDefaultValues_SetsJobType()
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();

			AssertEquals(ExpectedJobType, consolidation.KB_JobType);
		}

		protected abstract string ExpectedJobType { get; }

		#endregion

		#endregion

		#region Related Entities

		#region TestBookings

		public void TestBookings()
		{
			var consolidationSingleJob = (DtbTransportConsolidation)GetNewBusinessObject();

			consolidationSingleJob.Bookings.AddNew();
			AssertEquals(ExpectedTransportCollectionType, consolidationSingleJob.Bookings.GetType());
			AssertEquals(true, consolidationSingleJob.IsRegisteredEditableChildObject(consolidationSingleJob.Bookings));
		}

		protected abstract Type ExpectedTransportCollectionType { get; }

		#endregion

		#region TestBookedByAddress

		public void TestBookedByAddress()
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();

			AssertNotNull(consolidation.BookedByAddress);
			AssertEquals(DocAddressType.BookingPartyDocumentaryAddress, consolidation.BookedByAddress.DocAddressType);
		}

		#endregion

		#endregion

		#region Properties

		#region TestParentJobDescription

		public void TestParentJobDescription()
		{
			TestParentJobDescriptionCore();
		}

		protected abstract void TestParentJobDescriptionCore();

		#endregion

		#region TestBookedByOrganisationPK

		public void TestBookedByOrganisationPK()
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();

			AssertEquals(ZGuid.Empty, consolidation.BookedByOrganisationPK);

			consolidation.BookedByAddress.E2_AddressOverride = true;
			AssertEquals(ZGuid.Empty, consolidation.BookedByOrganisationPK);

			var org = Factory.New<OrgHeader>();
			consolidation.BookedByAddress.OrganisationPK = org.PK;
			AssertEquals(org.PK, consolidation.BookedByOrganisationPK);
		}

		#endregion

		#endregion

		#region Save

		#region TestJobIDIsPopulatedOnSave

		public void TestJobIDIsPopulatedOnSave()
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();

			AssertEquals("", consolidation.KB_JobID);

			Factory.Save();
			AssertEquals(ExpectedJobNumber, consolidation.KB_JobID);
			TestJobIDIsPopulatedOnSaveCore();
		}

		protected virtual void TestJobIDIsPopulatedOnSaveCore()
		{
		}

		protected abstract ZString ExpectedJobNumber { get; }

		#endregion

		#endregion

		#region TestDeleteTrasnports

		public void TestDelete()
		{
			var consolidationJob = (DtbTransportConsolidation)GetNewBusinessObject();
			var transport1 = consolidationJob.Bookings.AddNew();
			var transport2 = consolidationJob.Bookings.AddNew();

			consolidationJob.Delete();
			AssertEquals(0, consolidationJob.Bookings.Count);
			AssertEquals(true, transport1.IsDeleted);
			AssertEquals(true, transport1.IsDeleted);

			TestDeleteTransportsCore();
		}

		protected virtual void TestDeleteTransportsCore()
		{
		}

		#endregion

		#region IJobNumber Members

		public void TestIJobNumber()
		{
			var transportConsolidation = (DtbTransportConsolidation)GetNewBusinessObject();

			transportConsolidation.KB_JobID = "FOM12341298";
			AssertEquals("((IJobNumber)transportConsolidation).JobNumber", "FOM12341298", ((IJobNumber)transportConsolidation).JobNumber);
		}

		#endregion

		#region TestNotificationManager

		public void TestNotificationManager()
		{
			var consolidationSingleJob = (DtbTransportConsolidation)GetNewBusinessObject();

			AssertNotNull(consolidationSingleJob.NotificationManager);
		}

		#endregion

		#region TestNotificationSubscriber

		public void TestNotificationSubscriber()
		{
			var buffer = new TestNotificationBuffer();
			var consolidationSingleJob = (DtbTransportConsolidation)GetNewBusinessObject();

			consolidationSingleJob.NotificationManager.Push(buffer);
			AssertEquals(buffer, consolidationSingleJob.NotificationSubscriber);

			consolidationSingleJob.NotificationManager.Pop();
			AssertNotNull(consolidationSingleJob.NotificationSubscriber);
		}

		#endregion

		#region TestNotification

		public void TestNotification()
		{
			var notify = new TestNotify();
			var consolidationSingleJob = (DtbTransportConsolidation)GetNewBusinessObject();

			consolidationSingleJob.NotificationManager.Push(notify);

			consolidationSingleJob.NotificationSubscriber.Notify(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "You're doing something crazy!"));
			AssertEquals("You're doing something crazy!", notify.LastNotification.Message);
		}

		#endregion

		#region TestNotify

		class TestNotify : INotifications
		{
			void INotifications.Add(INotification notification)
			{
				Notifications.Add(notification);
				if (notification != null)
				{
					LastNotification = notification;
				}
			}

			public List<INotification> Notifications = new List<INotification>();
			public INotification LastNotification { get; private set; }
		}

		#endregion

		#region IDocAddresses Members

		#region TestDocAddresses

		public void TestDocAddresses()
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();

			consolidation.BookedByAddress.E2_OA_Address = Helper.CreateOrganisation("ABCSYD").MainAddress.PK;
			AssertContainsExactElementsInAnyOrder(new JobDocAddress[] { consolidation.BookedByAddress }, consolidation.DocAddresses);
			Assert(consolidation.IsRegisteredEditableChildObject(consolidation.DocAddresses));
		}

		#endregion

		#region TestChangingBookingParty

		public void TestChangingBookingParty()
		{
			// data setup
			// debtor
			var debtor = Helper.CreateOrganisation("DEBTOR", true);

			// non debtor with IFT
			var nonDebtorWithIFT = Helper.CreateOrganisation("NONDEBTORIFT");
			var pickupParty = Helper.CreateOrganisation("PICKUP");
			var deliveryParty = Helper.CreateOrganisation("DELIVERY");
			var pickupAndDeliveryParty = Helper.CreateOrganisation("DELIVERY");
			var otherParty = Helper.CreateOrganisation("OTHER");
			var nonIFT = Helper.CreateOrganisation("NONIFT");
			CreateRelatedParty(nonDebtorWithIFT, pickupParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup);
			CreateRelatedParty(nonDebtorWithIFT, deliveryParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			CreateRelatedParty(nonDebtorWithIFT, pickupAndDeliveryParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			CreateRelatedParty(nonDebtorWithIFT, nonIFT, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup);

			// non debtor without IFT
			var nonDebtorWithNoIFT = Helper.CreateOrganisation("NOIFT");
			CreateRelatedParty(nonDebtorWithIFT, nonIFT, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup);

			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();
			var transport = (DtbTransport)consolidation.Bookings.AddNew();

			consolidation.BookedByAddress.E2_OA_Address = debtor.MainAddress.PK;
			AssertEquals(debtor.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);

			transport.BillingPartyAddress.E2_OA_Address = ZGuid.Empty;
			consolidation.BookedByAddress.E2_OA_Address = nonDebtorWithIFT.MainAddress.PK;
			AssertEquals(pickupParty.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);

			transport.BillingPartyAddress.E2_OA_Address = ZGuid.Empty;
			consolidation.BookedByAddress.E2_OA_Address = nonDebtorWithNoIFT.MainAddress.PK;
			AssertEquals(ZGuid.Empty, transport.BillingPartyAddress.E2_OA_Address);

			nonDebtorWithIFT.OH_IsDebtor = true;
			transport.BillingPartyAddress.E2_OA_Address = ZGuid.Empty;
			consolidation.BookedByAddress.E2_OA_Address = nonDebtorWithIFT.MainAddress.PK;
			AssertEquals(pickupParty.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);

			transport.BillingPartyAddress.E2_AddressOverride = true;
			consolidation.BookedByAddress.E2_OA_Address = debtor.MainAddress.PK;
			AssertEquals(true, transport.BillingPartyAddress.E2_AddressOverride);
			AssertEquals(OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress, transport.BillingPartyAddress.E2_OA_Address);

			var billingParty = Helper.CreateOrganisation("BILLINGPARTY");
			transport.BillingPartyAddress.E2_AddressOverride = false;
			transport.BillingPartyAddress.E2_OA_Address = billingParty.MainAddress.PK;
			consolidation.BookedByAddress.E2_OA_Address = nonDebtorWithIFT.MainAddress.PK;
			AssertEquals(billingParty.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);
		}

		static void CreateRelatedParty(OrgHeader nonDebtorWithIFT, OrgHeader relatedPartyOrg, string partyType, string direction)
		{
			var relatedParty = nonDebtorWithIFT.AllRelatedParties.AddNew();
			relatedParty.PR_OH_RelatedParty = relatedPartyOrg.PK;
			relatedParty.PR_PartyType = partyType;
			relatedParty.PR_FreightDirection = direction;
		}

		#endregion

		#region TestSupportedAddressTypes

		public void TestSupportedAddressTypes()
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;

			var expectedAddressTypes = new List<DocAddressType>() { DocAddressType.BookingPartyDocumentaryAddress };
			expectedAddressTypes.AddRange(GetSupportedAddressTypesCore());
			AssertContainsExactElementsInAnyOrder(expectedAddressTypes, iConsolidation.SupportedAddressTypes);
		}

		protected virtual IEnumerable<DocAddressType> GetSupportedAddressTypesCore()
		{
			return Array.Empty<DocAddressType>();
		}

		#endregion

		#region TestCanDeleteAddress

		public void TestCanDeleteAddress()
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;

			AssertEquals(true, iConsolidation.CanDeleteAddress(consolidation.BookedByAddress));
		}

		#endregion

		#region TestGetCanOverrideCheckpoint

		public void TestGetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;

			AssertEquals(Env.Security.TransportJobMISCDetails, iConsolidation.GetCanOverrideCheckpoint(null));
		}

		#endregion

		#region TestGetDocAddressRequirement

		public void TestGetDocAddressRequirement(DocAddressType addressType)
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;
			var requirement = iConsolidation.GetDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress);

			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, requirement.DefaultDocAddressType);
			AssertEquals(ContactType.LocalTransport, requirement.DefaultContactType);
			AssertEquals(AddressType.OFC, requirement.DefaultAddressType);
			AssertEquals(true, requirement.SaveEvenIfBlank);
			AssertEquals(false, requirement.IsMandatory);
		}

		#endregion

		#region TestPiggyBackedDocAddressValidation

		public void TestPiggyBackedDocAddressValidation()
		{
			var consolidation = (DtbTransportConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;

			AssertNull(iConsolidation.PiggyBackedDocAddressValidation(null));
		}

		#endregion

		#endregion
	}
}
