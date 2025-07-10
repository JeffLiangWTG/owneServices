using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DetentionDatesUpdaterTest : TestCaseWithFactory
	{
		[TestDate(2014, 1, 1)]
		public void TestUpdateContainerDetentionDateFromSailing()
		{
			var today = ZDateTime.Today;
			var updater = new DetentionDatesUpdater();
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_UniqueConsignRef = "V00000100";
			billOfLading.ConsigneeDocumentaryAddress.OrganisationPK = ConsigneeWithDetentionDays.PK;
			billOfLading.JS_OH_DeliveryAgent = Carrier.PK;
			billOfLading.JS_JX = ImportSailing.PK;
			var transport = billOfLading.Transports[0];
			transport.JW_JX = ImportSailing.PK;
			var container = billOfLading.RealContainers.AddNew();
			using (AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				container.JC_ContainerNum = "REAL0000008";
				container.JC_RC = containerRCGuid;
				container.JC_EmptyReturnedBy = today.AddDays(4);
				Factory.Save();
				AssertEquals("Container date should remain manually overriden after saving", today.AddDays(4), container.JC_EmptyReturnedBy);
				Destination.JB_AvailabilityDate = today;
				updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				AssertEquals("Expected NOT to have recalculated the date as the registry setting is off", today.AddDays(4), container.JC_EmptyReturnedBy);
			}

			using (AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Destination.JB_AvailabilityDate = today.AddDays(1);
				updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				Factory.Save();
				AssertEquals("Expected to have updated the Returned By date as the registry is now on", today.AddDays(10), container.JC_EmptyReturnedBy);
				Destination.JB_AvailabilityDate = today.AddDays(20);
				updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				Factory.Save();
				AssertEquals("Expected to have re-calculated the date from the transport availabilty change", today.AddDays(20 + 9), container.JC_EmptyReturnedBy);
				Destination.JB_AvailabilityDate = ZDateTime.Empty;
				updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				Factory.Save();
				AssertEquals("Expected NOT to have re-calculated the date as the updater will not try to replace it with an empty date", today.AddDays(20 + 9), container.JC_EmptyReturnedBy);
				Destination.JB_AvailabilityDate = today.AddDays(1);
				updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				Factory.Save();
				AssertEquals("Expected to have updated it again because the calculated date is valid", today.AddDays(10), container.JC_EmptyReturnedBy);
			}
		}

		[TestDate(2014, 1, 1)]
		public void TestUpdateContainerDetentionDateFromSailing_CreatesLog()
		{
			using (AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var today = ZDateTime.Today;
				var billOfLading = Factory.New<BillOfLading>();
				billOfLading.JS_UniqueConsignRef = "V00000100";
				billOfLading.ConsigneeDocumentaryAddress.OrganisationPK = ConsigneeWithDetentionDays.PK;
				billOfLading.JS_OH_DeliveryAgent = Carrier.PK;
				billOfLading.JS_JX = ImportSailing.PK;
				var transport = billOfLading.Transports[0];
				transport.JW_JX = ImportSailing.PK;
				var container = billOfLading.RealContainers.AddNew();
				container.JC_ContainerNum = "REAL0000008";
				container.JC_RC = containerRCGuid;
				container.JC_EmptyReturnedBy = today.AddDays(4);
				container.JC_OH_ShippingLine = Carrier.PK;
				Factory.Save();
				AssertEquals("Container date should remain manually overriden after saving", today.AddDays(4), container.JC_EmptyReturnedBy);
				Destination.JB_AvailabilityDate = today.AddDays(20);
				Updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				Factory.Save();
				AssertEquals("Expected to have updated the Returned By date", today.AddDays(20 + 9), container.JC_EmptyReturnedBy);
				var query = EditedLogQuery.AddToFilter(StmALogSchema.SL_Reference, "Container 'REAL0000008' 'Empty Return By' re-calculated from 05-Jan-14 to 30-Jan-14");
				AssertNotNull("Expected an edit log on the container with the correct reference", container.Logs.Find(query));
				var detentionDate = ConsigneeWithDetentionDays.ConsigneeContainerPenalties[0];
				detentionDate.PD_FreeDays = 5;
				detentionDate.PD_OH_Carrier = Carrier.PK;
				Destination.JB_AvailabilityDate = today;
				Updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				Factory.Save();
				AssertEquals("Expected to have updated the Returned By date", today.AddDays(4), container.JC_EmptyReturnedBy);
				query = EditedLogQuery.AddToFilter(StmALogSchema.SL_Reference, "Container 'REAL0000008' 'Empty Return By' re-calculated from empty to 05-Jan-14");
				AssertNotNull("Expected an edit log on the container with the reference to the new date", container.Logs.Find(query));
				Destination.JB_AvailabilityDate = ZDateTime.Empty;
				Updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				Factory.Save();
				AssertEquals("Expected to NOT to have deleted the Returned By date but left it untouched", today.AddDays(4), container.JC_EmptyReturnedBy);
				AssertNotNull("Expected the last log to remain current as we have not changed the date", container.Logs.Find(query));
			}
		}

		[TestDate(2014, 1, 1)]
		public void TestUpdateContainerDetentionDateFromSailing_OnlyRealBillOfLadingContainersAreChanged()
		{
			using (AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = Factory.New<BillOfLading>();
				billOfLading.JS_UniqueConsignRef = "V00000100";
				billOfLading.ConsigneeDocumentaryAddress.OrganisationPK = ConsigneeWithDetentionDays.PK;
				billOfLading.JS_OH_DeliveryAgent = Carrier.PK;
				billOfLading.JS_JX = ImportSailing.PK;
				var billOfLadingContainer = billOfLading.RealContainers.AddNew();
				billOfLadingContainer.JC_ContainerNum = "REAL0000008";
				billOfLadingContainer.JC_Purpose = "REL";
				billOfLadingContainer.JC_RC = containerRCGuid;
				var agencyShipment = Factory.New<AgencyShipment>();
				agencyShipment.JS_UniqueConsignRef = "S00000222";
				agencyShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				agencyShipment.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
				agencyShipment.JS_JX = ImportSailing.PK;
				var agencyContainer = Factory.New<AgencyBookingContainer>();
				agencyContainer.JC_JS_FCLBookingOnlyLink = agencyShipment.PK;
				agencyContainer.JC_RC = containerRCGuid;
				agencyContainer.JC_ContainerNum = "FAKE0000009";
				var commonShipment = Factory.New<CommonShipment>();
				commonShipment.JS_UniqueConsignRef = "S00000333";
				commonShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				commonShipment.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
				commonShipment.JS_JX = ImportSailing.PK;
				var commonContainer = Factory.New<CommonContainer>();
				commonContainer.JC_JS_FCLBookingOnlyLink = commonShipment.PK;
				commonContainer.JC_RC = containerRCGuid;
				commonContainer.JC_ContainerNum = "FAKE0000009";
				Factory.Save();
				Assert("Pre-condition: expected Returned By Date to empty", billOfLadingContainer.JC_EmptyReturnedBy.IsEmpty);
				Assert("Pre-condition: expected Returned By Date to empty", agencyContainer.JC_EmptyReturnedBy.IsEmpty);
				Assert("Pre-condition: expected Returned By Date to empty", commonContainer.JC_EmptyReturnedBy.IsEmpty);
				billOfLading.Transports[0].JW_TerminalAvailabilityDate = ZDateTime.Today;
				Updater.UpdateContainerDetentionDateFromSailing(ImportSailing);
				Assert("Expected Return By Date on the agency container to remain unchanged", commonContainer.JC_EmptyReturnedBy.IsEmpty);
				Assert("Expected Return By Date on the common container to remain unchanged", agencyContainer.JC_EmptyReturnedBy.IsEmpty);
				AssertEquals("Expected date to be updated for BOL container", ZDateTime.Today.AddDays(9), billOfLadingContainer.JC_EmptyReturnedBy);
				var log = billOfLadingContainer.Logs.MostRecentLogByEventTime(AutoEvents.EditedARecord);
				AssertNotNull(log);
				AssertEquals("Container 'REAL0000008' 'Empty Return By' recalculated from empty to 10-Jan-14", log.SL_Reference);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			Updater = new DetentionDatesUpdater();
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_OH_Line = Carrier.PK;
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("CONDOR", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "444";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "GBLON";
			Destination = voyage.Destinations.AddNew();
			Destination.JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			ImportSailing = voyage.Sailings[0];
		}

		JobSailing ImportSailing;
		VoyageDestination Destination;
		DetentionDatesUpdater Updater;
		ZGuid containerRCGuid
		{
			get
			{
				return Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			}
		}

		static ZQuery EditedLogQuery
		{
			get
			{
				return new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.EditedARecord.Code);
			}
		}

		OrgHeader Carrier
		{
			get
			{
				if (carrier == null)
				{
					carrier = Factory.NewWithValidTestData<OrgHeader>();
					carrier.OH_Code = "Carrier";
					carrier.OH_IsShippingLine = true;
				}

				return carrier;
			}
		}

		OrgHeader carrier;
		OrgHeader ConsigneeWithDetentionDays
		{
			get
			{
				if (consignee == null)
				{
					consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_Code = "Consignee";
					consignee.OH_IsConsignee = true;
					var detention = consignee.ConsigneeContainerPenalties.AddNew();
					detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					detention.PD_FreeDays = 10;
				}

				return consignee;
			}
		}

		OrgHeader consignee;
		#endregion
	}
}
