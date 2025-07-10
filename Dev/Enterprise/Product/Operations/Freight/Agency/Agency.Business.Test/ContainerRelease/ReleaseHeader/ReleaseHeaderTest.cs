using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ReleaseHeader))]
	internal class ReleaseHeaderTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2008, 5, 12, 14, 30, 24)]
		public void TestUpdateContainerReleaseLog()
		{
			const string firstResultString = "2008-05-12 14:30 V00000001-1, V00000001-2\r\nSome Text";
			const string secondResultString = "2008-05-12 14:30 V00000001-2\r\nNew Text\r\n---------------\r\n" + "2008-05-12 14:30 V00000001-1, V00000001-2\r\nSome Text";
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_CFSReference = "V00000001";
			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_OA_DepartureContainerYardAddress = orgHeader.MainAddress.PK;
			AgencyShipmentContainer container2 = shipment.BookedContainers.AddNew();
			container2.JC_ContainerCount = 1;
			AssertEquals(0, shipment.Notes.GetAllNotes().Count);
			header = new ReleaseHeader(shipment, false);
			header.Init();
			header.ReleaseNumber = "Ref-1";
			header.ContainerReleaseNote = "Some Text";
			header.Release(container1, 1);
			header.Release(container2, 1);
			header.DoRelease(new NotificationsHandler());
			StmNote[] notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.ContainerReleaseNote.Description);
			AssertEquals(1, notes.Length);
			AssertMultilineASCIIEquals("Right Container Release Note log should be written", firstResultString, notes[0].ST_NoteText);
			header = new ReleaseHeader(shipment, true);
			header.Init();
			header.ContainerReleaseNote = "New Text";
			header.DoRelease(new NotificationsHandler());
			notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.ContainerReleaseNote.Description);
			AssertEquals(1, notes.Length);
			AssertMultilineASCIIEquals("Right Container Release Note log should be written", secondResultString, notes[0].ST_NoteText);
		}

		public void TestDoSelectAll()
		{
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerCount = 3;
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container2.JC_ContainerCount = 4;
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container3.JC_ContainerCount = 5;
			Header.Init();
			foreach (ReleaseDetail detail in Header.Details)
			{
				detail.ReleaseCount = 0;
			}

			foreach (ReleaseDetail detail in Header.Details)
			{
				AssertEquals(detail.Container.JC_ContainerCode, (short)0, detail.ReleaseCount);
			}

			Header.DoSelectAll();
			foreach (ReleaseDetail detail in Header.Details)
			{
				AssertEquals(detail.Container.JC_ContainerCode, detail.ContainerCount, detail.ReleaseCount);
			}
		}

		public void TestNotReplacement()
		{
			Shipment.JS_CFSReference = "Ref";
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			container1.JC_ReleaseNum = "Ref-1";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerCount = 3;
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			container2.JC_ReleaseNum = "Ref-2";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container2.JC_ContainerCount = 3;
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();
			container3.JC_ReleaseNum = "";
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container3.JC_ContainerCount = 3;
			header = new ReleaseHeader(Shipment, false);
			Header.Init();
			AssertEquals("", Header.ReleaseNumber);
			AssertEquals(true, Header.ReleaseNumberInfo.ReadOnly);
			AssertContainsExactElementsInAnyOrder("", (c) => c.JC_ContainerCode + " " + c.JC_ReleaseNum, new AgencyShipmentContainer[] { container3 }, Array.ConvertAll(Header.Details.ToArray(), (b) => ((ReleaseDetail)b).Container));
		}

		public void TestReplacement()
		{
			Shipment.JS_CFSReference = "Ref";
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			container1.JC_ReleaseNum = "Ref-1";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerCount = 3;
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			container2.JC_ReleaseNum = "Ref-2";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container2.JC_ContainerCount = 3;
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();
			container3.JC_ReleaseNum = "";
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container3.JC_ContainerCount = 3;
			header = new ReleaseHeader(Shipment, true);
			Header.Init();
			AssertEquals("Ref-2", Header.ReleaseNumber);
			AssertEquals(false, Header.ReleaseNumberInfo.ReadOnly);
			AssertContainsExactElementsInAnyOrder("", (c) => c.JC_ContainerCode + " " + c.JC_ReleaseNum, new AgencyShipmentContainer[] { container2, container3 }, Array.ConvertAll(Header.Details.ToArray(), (b) => ((ReleaseDetail)b).Container));
			Header.ReleaseNumber = "Ref-1";
			AssertContainsExactElementsInAnyOrder("", (c) => c.JC_ContainerCode + " " + c.JC_ReleaseNum, new AgencyShipmentContainer[] { container1, container3 }, Array.ConvertAll(Header.Details.ToArray(), (b) => ((ReleaseDetail)b).Container));
		}

		public void TestReprintMessageTypeGetExpectedPurpose()
		{
			var portMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort = portMessagingPortCollection.AddNew();
			portMessagingPort.Port = "NZAKL";
			portMessagingPort.SenderID = "SenderID";
			portMessagingPort.Enabled = true;

			using (Factory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				var sailing = CreateSailing("NZAKL", "AUBNE", ZDateTime.Now);

				var shipment = Factory.New<AgencyBooking>();
				shipment.JS_NKLoadPort = "NZAKL";
				shipment.JS_JX = sailing.PK;

				var voyage = sailing.Voyage;
				var vessel = RefVessel.LookupVesselByName("AMERICA STAR", Factory).First();
				vessel.RV_LloydsNumber = "123456";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				var container = shipment.BookedContainers.AddNew();
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_ContainerCount = 2;

				Factory.Save();

				var releaseHeader = new ReleaseHeader(shipment, false);
				releaseHeader.Init();
				releaseHeader.Release(container, 2);
				releaseHeader.DoRelease(new NotificationsHandler());

				var replacementHeader = new ReleaseHeader(shipment, true);
				replacementHeader.Init();
				replacementHeader.Release(container, 2);
				replacementHeader.DoRelease(new NotificationsHandler());

				var message = Factory.Load<IEDIMessage>(new ZQuery { FetchOnlyFromLocalCache = true })
					.OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc])
					.First();

				var xml = XDocument.Parse(message.EM_MessageText);
				var ns = xml.Root.GetDefaultNamespace();
				var dataContext = xml.Root.Descendants(ns + "DataContext").First();
				var expectedDataContext = $@"<DocumentaryOverride>
    <DataVersion>1</DataVersion>
    <DocumentName>Export Pre-Advice Replacement</DocumentName>
    <IsSystemDefined>true</IsSystemDefined>
    <Purpose>AMD</Purpose>
    <SubmissionVersion>1</SubmissionVersion>
  </DocumentaryOverride>";

				AssertContains(expectedDataContext, dataContext.ToString());
			}
		}

		public void TestReleaseNumberMaxLength()
		{
			AssertEquals(JobContainerSchema.JC_ReleaseNum.MaxLength, Header.ReleaseNumberInfo.MaxLength);
		}

		public void TestReleaseNumberSuffixAlwaysGetsLastSuffix()
		{
			var notificationshandler = new NotificationsHandler();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "yard";
			Shipment.JS_CFSReference = "Ref";
			var container1 = Shipment.BookedContainers.AddNew();
			container1.JC_OA_DepartureContainerYardAddress = orgHeader.MainAddress.PK;
			container1.JC_ContainerCount = 10;
			Header.Init();
			Header.Release(container1, 1);
			Header.DoRelease(notificationshandler);
			AssertEquals("Ref-1", container1.JC_ReleaseNum);
			Header.DoRelease(notificationshandler);
			AssertEquals("Ref-2", container1.JC_ReleaseNum);
			Shipment.JS_CFSReference = "New";
			Header.DoRelease(notificationshandler);
			AssertEquals("New-3", container1.JC_ReleaseNum);
			var parameters = new[] { new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.ContainerYard) };
			Shipment.Logs.AddNew(Events.ReleaseRequested, "New-7", parameters);
			Header.DoRelease(notificationshandler);
			AssertEquals("New-8", container1.JC_ReleaseNum);
			Header.DoRelease(notificationshandler);
			AssertEquals("New-9", container1.JC_ReleaseNum);
			Shipment.JS_CFSReference = "Ref-1-2-3";
			var container2 = Shipment.BookedContainers.AddNew();
			container2.JC_OA_DepartureContainerYardAddress = orgHeader.MainAddress.PK;
			container2.JC_ContainerCount = 2;
			Header.Init();
			Header.Release(container2, 1);
			Header.DoRelease(notificationshandler);
			AssertEquals("Ref-1-2-3-10", container2.JC_ReleaseNum);
			Header.DoRelease(notificationshandler);
			AssertEquals("Ref-1-2-3-11", container2.JC_ReleaseNum);
			Shipment.JS_CFSReference = "Ref-";
			var container3 = Shipment.BookedContainers.AddNew();
			container3.JC_OA_DepartureContainerYardAddress = orgHeader.MainAddress.PK;
			container3.JC_ContainerCount = 2;
			Header.Init();
			Header.Release(container3, 1);
			Header.DoRelease(notificationshandler);
			AssertEquals("Ref--12", container3.JC_ReleaseNum);
			Header.DoRelease(notificationshandler);
			AssertEquals("Ref--13", container3.JC_ReleaseNum);
		}

		public void TestRunningContainerReleaseWizardCreatesReleaseRequestedEvent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var yard = org.MainAddress;
			yard.OA_RL_NKRelatedPortCode = "AUSYD";
			Shipment.JS_CFSReference = "Ref";
			var container = Shipment.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ReleaseNum = "";
			container.JC_OA_DepartureContainerYardAddress = yard.PK;
			container.JC_ContainerCount = 3;
			Factory.Save();
			Header.Init();
			Header.Release(container, 2);
			Header.DoRelease(new NotificationsHandler());
			Factory.Save();
			var createdEvent = Shipment.Logs.MostRecentLogByEventTime(Events.ReleaseRequested);
			AssertEquals("Release Requested Event should be created.", "RLQ", createdEvent.SL_SE_NKEvent);
			AssertEquals("REF (Free Text) should be the release number.", "Ref-1", StmALog.GetFreeTextFromReference(createdEvent.SL_Reference));
			string facility;
			StmALog.GetParametersFromReference(createdEvent.SL_Reference).TryGetValue(Constants.EventReferenceParameters.Codes.Facility, out facility);
			AssertEquals("FAC parameter should be Container Yard", "CY", facility);
			string location;
			StmALog.GetParametersFromReference(createdEvent.SL_Reference).TryGetValue(Constants.EventReferenceParameters.Codes.Location, out location);
			AssertEquals("LOC parameter should be the same as address on container.", "AUSYD", location);
			string type;
			StmALog.GetParametersFromReference(createdEvent.SL_Reference).TryGetValue(Constants.EventReferenceParameters.Codes.Type, out type);
			AssertEquals("TYP parameter should be ORG if it is original.", "ORG", type);
		}

		public void TestReleaseNumberWillNotBeReusedAfterCancellation()
		{
			var yard = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			yard.Header.OH_FullName = "yard";
			Shipment.JS_CFSReference = "Ref";
			var container1 = Shipment.BookedContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ReleaseNum = "Ref-1";
			container1.JC_OA_DepartureContainerYardAddress = yard.PK;
			Header.Init();
			Header.ReleaseNumber = "Ref-1";
			Header.Release(container1, 0);
			Header.DoRelease(new NotificationsHandler());
			Factory.Save();
			AssertEquals("Precondition: Release Requested Event Created.", 1, Shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReleaseRequested.Code)).Length);
			var container2 = Shipment.BookedContainers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container2.JC_OA_DepartureContainerYardAddress = yard.PK;
			container2.JC_ContainerCount = 4;
			Header.Init();
			Header.Release(container2, 4);
			Header.DoRelease(new NotificationsHandler());
			AssertEquals("release number should not be reusing the number from canceled release.", "Ref-2", container2.JC_ReleaseNum);
		}

		public void TestDoRelease_MultipleContainerYards()
		{
			OrgAddress yard1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			yard1.Header.OH_FullName = "yard1";
			OrgAddress yard2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			yard2.Header.OH_FullName = "yard2";
			Shipment.JS_CFSReference = "Ref";
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ReleaseNum = "";
			container1.JC_OA_DepartureContainerYardAddress = yard1.PK;
			container1.JC_ContainerCount = 3;
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container2.JC_ReleaseNum = "";
			container2.JC_OA_DepartureContainerYardAddress = yard1.PK;
			container2.JC_ContainerCount = 4;
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container3.JC_ReleaseNum = "Ref-1";
			container3.JC_OA_DepartureContainerYardAddress = yard1.PK;
			container3.JC_ContainerCount = 5;
			AgencyShipmentContainer container4 = Shipment.BookedContainers.AddNew();
			container4.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container4.JC_ReleaseNum = "";
			container4.JC_OA_DepartureContainerYardAddress = yard2.PK;
			container4.JC_ContainerCount = 6;
			Header.Init();
			Header.Release(container1, 3);
			Header.Release(container2, 4);
			Header.Release(container4, 6);
			Header.DoRelease(new NotificationsHandler());
			AssertContainsExactElementsInAnyOrder("", new string[] { "20GP (3) Ref-2", "40GP (4) Ref-2", "20RE (5) Ref-1", "40RE (6) Ref-3", }, Array.ConvertAll(header.Shipment.BookedContainers.ToArray<AgencyBookingContainer>(), (c) => c.JC_ContainerCode + " " + c.JC_ReleaseNum));
			AssertContainsExactElementsInAnyOrder("", new string[] { "Ref-2 yard1 ORG", "Ref-3 yard2 ORG", }, Array.ConvertAll(header.Instances.ToArray<ReleaseInstance>(), (i) => i.ReleaseNumber + " " + i.ContainerYard.Header.OH_FullName + " " + i.ReleaseType));
		}

		public void TestDoRelease_Split()
		{
			OrgAddress yard = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			yard.Header.OH_FullName = "yard";
			Shipment.JS_CFSReference = "Ref";
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ReleaseNum = "Ref-1";
			container1.JC_OA_DepartureContainerYardAddress = yard.PK;
			container1.JC_ContainerCount = 4;
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container2.JC_ReleaseNum = "Ref-1";
			container2.JC_OA_DepartureContainerYardAddress = yard.PK;
			container2.JC_ContainerCount = 4;
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container3.JC_ReleaseNum = "";
			container3.JC_OA_DepartureContainerYardAddress = yard.PK;
			container3.JC_ContainerCount = 4;
			AgencyShipmentContainer container4 = Shipment.BookedContainers.AddNew();
			container4.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container4.JC_ReleaseNum = "";
			container4.JC_OA_DepartureContainerYardAddress = yard.PK;
			container4.JC_ContainerCount = 4;
			header = new ReleaseHeader(Shipment, true);
			Header.Init();
			Header.ReleaseNumber = "Ref-1";
			Header.Release(container1, 0);
			Header.Release(container2, 2);
			Header.Release(container3, 2);
			Header.Release(container4, 4);
			Header.DoRelease(new NotificationsHandler());
			AssertContainsExactElementsInAnyOrder("", new string[] { "20GP (4) ", "40GP (2) Ref-1", "40GP (2) ", "20RE (2) Ref-1", "20RE (2) ", "40RE (4) Ref-1", }, Array.ConvertAll(header.Shipment.BookedContainers.ToArray<AgencyBookingContainer>(), (c) => c.JC_ContainerCode + " " + c.JC_ReleaseNum));
			AssertContainsExactElementsInAnyOrder("", new string[] { "Ref-1 yard RVS", }, Array.ConvertAll(header.Instances.ToArray<ReleaseInstance>(), (i) => i.ReleaseNumber + " " + i.ContainerYard.Header.OH_FullName + " " + i.ReleaseType));
		}

		public void TestDoRelease_SplitOriginal()
		{
			OrgAddress yard = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			yard.Header.OH_FullName = "yard";
			Shipment.JS_CFSReference = "Ref";
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ReleaseNum = "";
			container1.JC_OA_DepartureContainerYardAddress = yard.PK;
			container1.JC_ContainerCount = 4;
			header = new ReleaseHeader(Shipment, false);
			Header.Init();
			Header.ReleaseNumber = "";
			Header.Release(container1, 1);
			Header.DoRelease(new NotificationsHandler());
			AssertContainsExactElementsInAnyOrder("", new string[] { "20GP (1) Ref-1", "20GP (3) ", }, Array.ConvertAll(header.Shipment.BookedContainers.ToArray<AgencyBookingContainer>(), (c) => c.JC_ContainerCode + " " + c.JC_ReleaseNum));
			AssertContainsExactElementsInAnyOrder("", new string[] { "Ref-1 yard ORG", }, Array.ConvertAll(header.Instances.ToArray<ReleaseInstance>(), (i) => i.ReleaseNumber + " " + i.ContainerYard.Header.OH_FullName + " " + i.ReleaseType));
		}

		public void TestDoRelease_Reverse()
		{
			OrgAddress yard = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			yard.Header.OH_FullName = "yard";
			Shipment.JS_CFSReference = "Ref";
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ReleaseNum = "Ref-1";
			container1.JC_OA_DepartureContainerYardAddress = yard.PK;
			container1.JC_ContainerCount = 4;
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container2.JC_ReleaseNum = "Ref-2";
			container2.JC_OA_DepartureContainerYardAddress = yard.PK;
			container2.JC_ContainerCount = 4;
			Header.Init();
			Header.ReleaseNumber = "Ref-1";
			Header.Release(container1, 0);
			Header.DoRelease(new NotificationsHandler());
			AssertContainsExactElementsInAnyOrder("", new string[] { "20GP (4) ", "40GP (4) Ref-2", }, Array.ConvertAll(header.Shipment.BookedContainers.ToArray<AgencyBookingContainer>(), (c) => c.JC_ContainerCode + " " + c.JC_ReleaseNum));
			AssertContainsExactElementsInAnyOrder("", new string[] { "Ref-1 yard CAN", }, Array.ConvertAll(header.Instances.ToArray<ReleaseInstance>(), (i) => i.ReleaseNumber + " " + i.ContainerYard.Header.OH_FullName + " " + i.ReleaseType));
		}

		public void TestDetails()
		{
			header = new ReleaseHeader(Shipment, true);
			AssertEquals(false, header.HasChanges);
			AssertEquals(false, header.Details.HasChanges);
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			container1.JC_ReleaseNum = "Ref-1";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerCount = 3;
			header.Details.RePopulate("Ref-1");
			AssertEquals(true, header.Details.HasChanges);
			AssertEquals(true, header.HasChanges);
		}

		[TestDate(2015, 4, 1, 9, 0, 0)]
		public void TestContainerMessaging_RegistryEnabled_MatchCompanyLevelPortSenderIDWithPrincipal()
		{
			var systemLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort1 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort1.Port = "NZAKL";
			portMessagingPort1.SenderID = "SenderID1";
			portMessagingPort1.Enabled = true;
			var portMessagingPort2 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort2.Port = "NZAKL";
			portMessagingPort2.SenderID = "SenderID2";
			portMessagingPort2.Enabled = true;
			portMessagingPort2.PrincipalPK = Principal.PK;

			var companyLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort3 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort3.Port = "NZAKL";
			portMessagingPort3.SenderID = "SenderID3";
			portMessagingPort3.Enabled = true;
			var portMessagingPort4 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort4.Port = "NZAKL";
			portMessagingPort4.SenderID = "SenderID4";
			portMessagingPort4.Enabled = true;
			portMessagingPort4.PrincipalPK = Principal.PK;

			TestContainerMessaging_RegistryEnabled(systemLevelPortMessagingPortCollection, companyLevelPortMessagingPortCollection, "SenderID4");
		}

		[TestDate(2015, 4, 1, 9, 0, 0)]
		public void TestContainerMessaging_RegistryEnabled_MatchCompanyLevelPortSenderIDWithEmptyPrincipal()
		{
			var systemLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort1 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort1.Port = "NZAKL";
			portMessagingPort1.SenderID = "SenderID1";
			portMessagingPort1.Enabled = true;
			var portMessagingPort2 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort2.Port = "NZAKL";
			portMessagingPort2.SenderID = "SenderID2";
			portMessagingPort2.Enabled = true;
			portMessagingPort2.PrincipalPK = Principal.PK;

			var companyLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort3 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort3.Port = "NZAKL";
			portMessagingPort3.SenderID = "SenderID3";
			portMessagingPort3.Enabled = true;
			var portMessagingPort4 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort4.Port = "NZAKL";
			portMessagingPort4.SenderID = "SenderID4";
			portMessagingPort4.Enabled = true;
			portMessagingPort4.PrincipalPK = Principal2.PK;

			TestContainerMessaging_RegistryEnabled(systemLevelPortMessagingPortCollection, companyLevelPortMessagingPortCollection, "SenderID3");
		}

		[TestDate(2015, 4, 1, 9, 0, 0)]
		public void TestContainerMessaging_RegistryEnabled_MatchSystemLevelPortSenderIDWithPrincipal()
		{
			var systemLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort1 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort1.Port = "NZAKL";
			portMessagingPort1.SenderID = "SenderID1";
			portMessagingPort1.Enabled = true;
			var portMessagingPort2 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort2.Port = "NZAKL";
			portMessagingPort2.SenderID = "SenderID2";
			portMessagingPort2.Enabled = true;
			portMessagingPort2.PrincipalPK = Principal.PK;

			var companyLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort3 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort3.Port = "NZAKL";
			portMessagingPort3.SenderID = "SenderID3";
			portMessagingPort3.Enabled = true;
			portMessagingPort3.PrincipalPK = Principal2.PK;
			var portMessagingPort4 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort4.Port = "NZAKL";
			portMessagingPort4.SenderID = "SenderID4";
			portMessagingPort4.Enabled = false;

			TestContainerMessaging_RegistryEnabled(systemLevelPortMessagingPortCollection, companyLevelPortMessagingPortCollection, "SenderID2");
		}

		[TestDate(2015, 4, 1, 9, 0, 0)]
		public void TestContainerMessaging_RegistryEnabled_MatchSystemLevelPortSenderIDWithEmptyPrincipal()
		{
			var systemLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort1 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort1.Port = "NZAKL";
			portMessagingPort1.SenderID = "SenderID1";
			portMessagingPort1.Enabled = true;
			var portMessagingPort2 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort2.Port = "NZAKL";
			portMessagingPort2.SenderID = "SenderID2";
			portMessagingPort2.Enabled = true;
			portMessagingPort2.PrincipalPK = Principal2.PK;

			var companyLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort3 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort3.Port = "NZAKL";
			portMessagingPort3.SenderID = "SenderID3";
			portMessagingPort3.Enabled = false;
			portMessagingPort3.PrincipalPK = Principal2.PK;
			var portMessagingPort4 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort4.Port = "NZAKL";
			portMessagingPort4.SenderID = "SenderID4";
			portMessagingPort4.Enabled = false;

			TestContainerMessaging_RegistryEnabled(systemLevelPortMessagingPortCollection, companyLevelPortMessagingPortCollection, "SenderID1");
		}

		void TestContainerMessaging_RegistryEnabled(PortMessagingPortCollection systemLevelPortMessagingPortCollection, PortMessagingPortCollection companyLevelPortMessagingPortCollection, string expectSenderID)
		{
			using (Factory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLevelPortMessagingPortCollection))
			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyLevelPortMessagingPortCollection))
			{
				var sailing = CreateSailing("NZAKL", "AUBNE", ZDateTime.Now);

				var voyage = sailing.Voyage;
				var vessel = RefVessel.LookupVesselByName("AMERICA STAR", Factory).First();
				vessel.RV_LloydsNumber = "123456";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				Shipment.JS_CFSReference = "Ref";
				Shipment.JS_NKLoadPort = "NZAKL";
				Shipment.JS_JX = sailing.PK;
				Shipment.JS_OH_DeliveryAgent = Principal.PK;
				Factory.Save();

				header = new ReleaseHeader(Shipment, false);

				var container = Shipment.BookedContainers.AddNew();
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_ContainerCount = 3;

				Factory.Save();

				Header.Init();
				Header.Release(container, 1);
				Header.DoRelease(new NotificationsHandler());

				var message = Factory.Load<IEDIMessage>(new ZQuery { FetchOnlyFromLocalCache = true })
					.OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc])
					.First();
				Factory.Save();

				var xml = XDocument.Parse(message.EM_MessageText);

				var ns = xml.Root.GetDefaultNamespace();
				AssertEquals("Uses 2012 namespace", @"http://www.cargowise.com/Schemas/Universal/2012/11", ns.ToString());

				var dataContext = xml.Root.Descendants(ns + "DataContext").First();
				AssertMultilineASCIIEquals("", expectedContainerMessagingDataContext, dataContext.ToString());

				dataContext = xml.Root.Descendants(ns + "AddInfoCollection").First();
				AssertMultilineASCIIEquals("", @"<AddInfoCollection xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <AddInfo>
    <Key>SenderID</Key>
    <Value>" + expectSenderID + @"</Value>
  </AddInfo>
  <AddInfo>
    <Key>OperationalPort_Code</Key>
    <Value>NZAKL</Value>
  </AddInfo>
  <AddInfo>
    <Key>OperationalPort_Name</Key>
    <Value>Auckland</Value>
  </AddInfo>
</AddInfoCollection>", dataContext.ToString());

				var messageSentEvent = Shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).First();
				AssertEquals("Export Pre-Advice Message Sent to Terminal, Auckland, AUK, NZ", messageSentEvent.DisplayEventReference);
				AssertEquals("|DEP=Terminal|LOC=NZAKL|MST=Export Pre-Advice", messageSentEvent.SL_ReferenceForBinding);
			}
		}

		[TestDate(2015, 4, 1, 9, 0, 0)]
		public void TestContainerMessaging_RegistryDisable()
		{
			var systemLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort1 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort1.Port = "NZAKL";
			portMessagingPort1.SenderID = "SenderID1";
			portMessagingPort1.Enabled = false;
			var portMessagingPort2 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort2.Port = "NZAKL";
			portMessagingPort2.SenderID = "SenderID2";
			portMessagingPort2.Enabled = false;
			portMessagingPort2.PrincipalPK = Principal.PK;

			var companyLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort3 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort3.Port = "NZAKL";
			portMessagingPort3.SenderID = "SenderID3";
			portMessagingPort3.Enabled = false;
			portMessagingPort3.PrincipalPK = Principal.PK;
			var portMessagingPort4 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort4.Port = "NZAKL";
			portMessagingPort4.SenderID = "SenderID4";
			portMessagingPort4.Enabled = false;

			TestContainerMessaging_RegistryDisable(systemLevelPortMessagingPortCollection, companyLevelPortMessagingPortCollection);
		}

		[TestDate(2015, 4, 1, 9, 0, 0)]
		public void TestContainerMessaging_RegistryDisable_PortIsDisabledAtCompanyLevel()
		{
			var systemLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort1 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort1.Port = "NZAKL";
			portMessagingPort1.SenderID = "SenderID1";
			portMessagingPort1.Enabled = true;
			var portMessagingPort2 = systemLevelPortMessagingPortCollection.AddNew();
			portMessagingPort2.Port = "NZAKL";
			portMessagingPort2.SenderID = "SenderID2";
			portMessagingPort2.Enabled = true;
			portMessagingPort2.PrincipalPK = Principal.PK;

			var companyLevelPortMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort3 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort3.Port = "NZAKL";
			portMessagingPort3.SenderID = "SenderID3";
			portMessagingPort3.Enabled = false;
			portMessagingPort3.PrincipalPK = Principal.PK;
			var portMessagingPort4 = companyLevelPortMessagingPortCollection.AddNew();
			portMessagingPort4.Port = "NZAKL";
			portMessagingPort4.SenderID = "SenderID4";
			portMessagingPort4.Enabled = false;

			TestContainerMessaging_RegistryDisable(systemLevelPortMessagingPortCollection, companyLevelPortMessagingPortCollection);
		}

		void TestContainerMessaging_RegistryDisable(PortMessagingPortCollection systemLevelPortMessagingPortCollection, PortMessagingPortCollection companyLevelPortMessagingPortCollection)
		{
			using (Factory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLevelPortMessagingPortCollection))
			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyLevelPortMessagingPortCollection))
			{
				Shipment.JS_CFSReference = "Ref";
				Shipment.JS_NKLoadPort = "NZAKL";
				Shipment.JS_OH_DeliveryAgent = Principal.PK;

				header = new ReleaseHeader(Shipment, false);
				header.IncludeMessage = true;

				AssertHasError(header.IncludeMessageInfo, "Principal is not configured for sending Export Pre-Advice message to NZAKL");
			}
		}

		#region Expected Container Messaging DataContext

		const string expectedContainerMessagingDataContext = @"<DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <DataSource>
    <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
    <Key>V00001000</Key>
    <Type>AgencyBooking</Type>
  </DataSource>
  <DocumentaryOverride>
    <DataVersion>1</DataVersion>
    <DocumentName>Export Pre-Advice</DocumentName>
    <IsSystemDefined>true</IsSystemDefined>
    <Purpose>ORG</Purpose>
    <SubmissionVersion>1</SubmissionVersion>
  </DocumentaryOverride>
  <Workflow>
    <Company>
      <Code>EDI</Code>
      <Country Name=""New Zealand"">NZ</Country>
      <Name>Eagle Datamation International</Name>
    </Company>
    <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
    <EventDepartment Name=""Branch"">BRN</EventDepartment>
    <EventReference>Ref-1|FAC=CY|TYP=ORG</EventReference>
    <EventType Description=""Release Requested"">RLQ</EventType>
    <EventUser Name=""CargoWise Support"">E</EventUser>
    <TriggerCount>1</TriggerCount>
    <TriggerDate>2015-04-01T09:00:00.000+00:00</TriggerDate>
    <TriggerDescription></TriggerDescription>
    <TriggerType>Manual</TriggerType>
    <RecipientRoleCollection>
      <RecipientRole Description=""Port For Export Release"">PER</RecipientRole>
    </RecipientRoleCollection>
  </Workflow>
</DataContext>";

		#endregion

		[ExpectNoExceptions]
		public void TestReprintMessageTypeDoesNotThrowException()
		{
			var portMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort = portMessagingPortCollection.AddNew();
			portMessagingPort.Port = "NZAKL";
			portMessagingPort.SenderID = "SenderID";
			portMessagingPort.Enabled = true;

			using (Factory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				var sailing = CreateSailing("NZAKL", "AUBNE", ZDateTime.Now);

				var shipment = Factory.New<AgencyBooking>();
				shipment.JS_NKLoadPort = "NZAKL";
				shipment.JS_JX = sailing.PK;

				var voyage = sailing.Voyage;
				var vessel = RefVessel.LookupVesselByName("AMERICA STAR", Factory).First();
				vessel.RV_LloydsNumber = "123456";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				var container = shipment.BookedContainers.AddNew();
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_ContainerCount = 2;

				Factory.Save();

				var releaseHeader = new ReleaseHeader(shipment, false);
				releaseHeader.Init();
				releaseHeader.Release(container, 2);
				releaseHeader.DoRelease(new NotificationsHandler());

				var replacementHeader = new ReleaseHeader(shipment, true);
				replacementHeader.Init();
				replacementHeader.Release(container, 2);
				replacementHeader.DoRelease(new NotificationsHandler());

				var messageSentEvent = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).ElementAt(1);
				AssertEquals("Export Pre-Advice Replacement Message Sent to Terminal, Auckland, AUK, NZ", messageSentEvent.DisplayEventReference);
				AssertEquals("|DEP=Terminal|LOC=NZAKL|MST=Export Pre-Advice Replacement", messageSentEvent.SL_ReferenceForBinding);
				Factory.Save();
			}
		}

		public void TestIncludeMessageInformation()
		{
			var portMessagingPortCollection = new PortMessagingPortCollection();
			var portMessagingPort1 = portMessagingPortCollection.AddNew();
			portMessagingPort1.Port = "NZAKL";
			portMessagingPort1.SenderID = "NZSenderID";
			portMessagingPort1.Enabled = true;

			var portMessagingPort2 = portMessagingPortCollection.AddNew();
			portMessagingPort2.Port = "NZTRG";
			portMessagingPort2.SenderID = "AUSenderID";
			portMessagingPort2.Enabled = false;

			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portMessagingPortCollection))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var nzShipment = Factory.New<AgencyBooking>();
				nzShipment.JS_NKLoadPort = "NZAKL";

				var nzContainer = nzShipment.BookedContainers.AddNew();
				nzContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				nzContainer.JC_ContainerCount = 1;

				Factory.Save();

				var nzReleaseHeader = new ReleaseHeader(nzShipment, false);

				Assert(nzReleaseHeader.SupportsMessageSending);
				Assert(nzReleaseHeader.IncludeMessage);
				AssertEquals("Send Export Pre-Advice to NZAKL Port", nzReleaseHeader.IncludeMessageCaption);

				var nzReplacementHeader = new ReleaseHeader(nzShipment, true);

				Assert(nzReplacementHeader.SupportsMessageSending);
				Assert(nzReplacementHeader.IncludeMessage);
				AssertEquals("Send Export Pre-Advice to NZAKL Port", nzReplacementHeader.IncludeMessageCaption);

				var auShipment = Factory.New<AgencyBooking>();
				auShipment.JS_NKLoadPort = "NZTRG";

				var auContainer = auShipment.BookedContainers.AddNew();
				auContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				auContainer.JC_ContainerCount = 2;

				Factory.Save();

				var auReleaseHeader = new ReleaseHeader(auShipment, false);

				Assert(auReleaseHeader.SupportsMessageSending);
				Assert(!auReleaseHeader.IncludeMessage);
				AssertEquals("Send Export Pre-Advice to NZTRG Port", auReleaseHeader.IncludeMessageCaption);

				var usShipment = Factory.New<AgencyBooking>();
				usShipment.JS_NKLoadPort = "USCGE";

				var usContainer = usShipment.BookedContainers.AddNew();
				usContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				usContainer.JC_ContainerCount = 3;

				var usReleaseHeader = new ReleaseHeader(usShipment, false);

				Assert(!usReleaseHeader.SupportsMessageSending);
				Assert(!usReleaseHeader.IncludeMessage);
				AssertNullOrEmpty(usReleaseHeader.IncludeMessageCaption);
			}
		}

		public void TestContainerMessaging_RecipientIdIsEqualToShippingPortsMessagingEHubIDRegistryOfSpanishPort()
		{
			using (Factory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var systemLevelPortMessagingPortCollection = new PortMessagingPortCollection();
				var portMessagingPort1 = systemLevelPortMessagingPortCollection.AddNew();
				portMessagingPort1.Port = "ESVLC";
				portMessagingPort1.SenderID = "SenderID1";
				portMessagingPort1.Enabled = true;

				var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();
				var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDCollection.AddNew();
				shippingPortsMessagingEHubID.Port = "ESVLC";
				shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.xHub;
				shippingPortsMessagingEHubID.RecipientID = "ESVLC_RECIPIENT";

				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shippingPortsMessagingEHubIDCollection))
				using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLevelPortMessagingPortCollection))
				{
					var sailing = CreateSailing("ESVLC", "AUBNE", ZDateTime.Now);

					var voyage = sailing.Voyage;
					var vessel = RefVessel.LookupVesselByName("AMERICA STAR", Factory).First();
					vessel.RV_LloydsNumber = "123456";
					voyage.JV_RV_NKVessel = vessel.RV_FK;

					Shipment.JS_CFSReference = "Ref";
					Shipment.JS_NKLoadPort = "ESVLC";
					Shipment.JS_JX = sailing.PK;
					Shipment.JS_OH_DeliveryAgent = Principal.PK;
					Factory.Save();

					header = new ReleaseHeader(Shipment, false);

					var container = Shipment.BookedContainers.AddNew();
					container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					container.JC_ContainerCount = 3;

					Factory.Save();

					Header.Init();
					Header.Release(container, 1);
					Header.DoRelease(new NotificationsHandler());

					var message = Factory.Load<IEDIMessage>(new ZQuery { FetchOnlyFromLocalCache = true })
						.OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc])
						.First();
					Factory.Save();

					AssertEquals("ESVLC_RECIPIENT", message.Interchange.EI_To);

					var messageSentEvent = Shipment.Logs
						.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).First();
					AssertEquals("Export Pre-Advice Message Sent to Terminal, Valencia, ES", messageSentEvent.DisplayEventReference);
					AssertEquals("|DEP=Terminal|LOC=ESVLC|MST=Export Pre-Advice", messageSentEvent.SL_ReferenceForBinding);
				}
			}
		}

		public void TestContainerMessaging_RecipientIdIsEqualToShippingPortsMessagingEHubIDRegistryWithEmptyPort()
		{
			using (Factory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var systemLevelPortMessagingPortCollection = new PortMessagingPortCollection();
				var portMessagingPort1 = systemLevelPortMessagingPortCollection.AddNew();
				portMessagingPort1.Port = "ESVLC";
				portMessagingPort1.SenderID = "SenderID1";
				portMessagingPort1.Enabled = true;

				var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();

				var shippingPortsMessagingEHubID2 = shippingPortsMessagingEHubIDCollection.AddNew();
				shippingPortsMessagingEHubID2.Port = ZString.Empty;
				shippingPortsMessagingEHubID2.Module = ModuleTypes.Codes.eHub;
				shippingPortsMessagingEHubID2.RecipientID = "DEF_RECIPIENT";

				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shippingPortsMessagingEHubIDCollection))
				using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLevelPortMessagingPortCollection))
				{
					var sailing = CreateSailing("ESVLC", "AUBNE", ZDateTime.Now);

					var voyage = sailing.Voyage;
					var vessel = RefVessel.LookupVesselByName("AMERICA STAR", Factory).First();
					vessel.RV_LloydsNumber = "123456";
					voyage.JV_RV_NKVessel = vessel.RV_FK;

					Shipment.JS_CFSReference = "Ref";
					Shipment.JS_NKLoadPort = "ESVLC";
					Shipment.JS_JX = sailing.PK;
					Shipment.JS_OH_DeliveryAgent = Principal.PK;
					Factory.Save();

					header = new ReleaseHeader(Shipment, false);

					var container = Shipment.BookedContainers.AddNew();
					container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					container.JC_ContainerCount = 3;

					Factory.Save();

					Header.Init();
					Header.Release(container, 1);
					Header.DoRelease(new NotificationsHandler());

					var message = Factory.Load<IEDIMessage>(new ZQuery { FetchOnlyFromLocalCache = true })
						.OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc])
						.First();
					Factory.Save();

					AssertEquals("DEF_RECIPIENT", message.Interchange.EI_To);

					var messageSentEvent = Shipment.Logs
						.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).First();
					AssertEquals("Export Pre-Advice Message Sent to Terminal, Valencia, ES", messageSentEvent.DisplayEventReference);
					AssertEquals("|DEP=Terminal|LOC=ESVLC|MST=Export Pre-Advice", messageSentEvent.SL_ReferenceForBinding);
				}
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReleaseHeader(Factory.New<AgencyBooking>(), false);
		}

		ReleaseHeader Header
		{
			get
			{
				return header ?? (header = new ReleaseHeader(Shipment, false));
			}
		}

		ReleaseHeader header;

		AgencyBooking Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking shipment;

		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = Factory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "TST";
					principal.OH_IsShippingProvider = true;

					OrgCompanyData companyData = principal.CompanyData;
					companyData.OB_CRIsShipsAgencyPrincipal = true;
					Factory.Save();
				}

				return principal;
			}
		}

		OrgHeader principal;

		OrgHeader Principal2
		{
			get
			{
				if (principal2 == null)
				{
					principal2 = Factory.NewWithValidTestData<OrgHeader>();
					principal2.OH_Code = "TS2";
					principal2.OH_IsShippingProvider = true;

					OrgCompanyData companyData = principal2.CompanyData;
					companyData.OB_CRIsShipsAgencyPrincipal = true;
					Factory.Save();
				}

				return principal2;
			}
		}

		OrgHeader principal2;

		JobSailing CreateSailing(ZString load, ZString discharge, ZDateTime etd)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		#endregion
	}
}
