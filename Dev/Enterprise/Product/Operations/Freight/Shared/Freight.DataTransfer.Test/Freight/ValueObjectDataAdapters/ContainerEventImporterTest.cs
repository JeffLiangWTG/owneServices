using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ContainerEventImporterTest : TestCaseWithFactory
	{
		#region Import

		public void TestImportSlotDateAndReferenceContainerEvents()
		{
			ZDateTime importDate = ZDateTime.Now.ToSmallDateTime();
			ZDateTime exportDate = ZDateTime.Now.AddHours(2).AddMinutes(23).ToSmallDateTime();

			Xsd.ContainerEvent arrivalSlotEvent = new Xsd.ContainerEvent();
			arrivalSlotEvent.EventType = Xsd.ContainerEventEventType.ArrivalSlotTimeBooked;
			arrivalSlotEvent.EventDate = importDate;
			arrivalSlotEvent.EnterpriseJobNumber = Consol.JK_UniqueConsignRef;
			arrivalSlotEvent.ContainerNumber = Container.JC_ContainerNum;

			Xsd.ContainerEvent departureSlotEvent = new Xsd.ContainerEvent();
			departureSlotEvent.EventType = Xsd.ContainerEventEventType.DepartureSlotTimeBooked;
			departureSlotEvent.EventDate = exportDate;
			departureSlotEvent.EnterpriseJobNumber = Consol.JK_UniqueConsignRef;
			departureSlotEvent.ContainerNumber = Container.JC_ContainerNum;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			Container.JC_ArrivalSlotDateTime = ZDateTime.Now.AddDays(-1).ToSmallDateTime();
			Container.JC_ArrivalSlotReference = "A SLOT REF";
			Container.JC_DepartureSlotDateTime = ZDateTime.Now.AddDays(-2).ToSmallDateTime();
			Container.JC_DepartureSlotReference = "D SLOT REF";

			Importer.Import(context, arrivalSlotEvent);
			Importer.Import(context, departureSlotEvent);

			AssertEquals(importDate, Container.JC_ArrivalSlotDateTime);
			AssertEquals("A SLOT REF", Container.JC_ArrivalSlotReference);
			AssertEquals(exportDate, Container.JC_DepartureSlotDateTime);
			AssertEquals("D SLOT REF", Container.JC_DepartureSlotReference);

			arrivalSlotEvent.EventReference = "Arrive slot ref should be trimmed so it does not exceed max length";
			departureSlotEvent.EventReference = "Depart slot ref should be trimmed so it does not exceed max length";

			Importer.Import(context, arrivalSlotEvent);
			Importer.Import(context, departureSlotEvent);

			AssertEquals(importDate, Container.JC_ArrivalSlotDateTime);
			AssertEquals("Arrive slot ref should be trimmed s", Container.JC_ArrivalSlotReference);
			AssertEquals(exportDate, Container.JC_DepartureSlotDateTime);
			AssertEquals("Depart slot ref should be trimmed s", Container.JC_DepartureSlotReference);
		}

		public void TestContainerEventsUpdatedRegardlessOfDeclarationBranch()
		{
			GlbBranch newBranch = Factory.New<GlbBranch>();
			newBranch.GB_Code = "BBB";
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			Declaration[JobDeclarationSchema.JE_GB] = newBranch.PK;
			Declaration.Factory.Save();

			Xsd.ContainerEvent containerEvent = GetXsdContainerEvent("Container");

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, buffer);
			Importer.Import(importContext, containerEvent);
			AssertEquals(new ZDateTime(2005, 1, 2), Container.JC_ArrivalCartageAdvised);
		}

		public void TestImportNotifiesWhenContainerFound()
		{
			Container.Factory.Save();
			Consol.Factory.Save();
			Xsd.ContainerEvent containerEvent = GetXsdContainerEvent("Container");

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, buffer);
			Importer.Import(importContext, containerEvent);
			Assert("Should notify when container processed", buffer.AsString.Contains("Container with Container Number='Container' Voyage='Voyage' Lloyds='TstLlds'"));
		}

		public void TestImportNotifiesWhenContainerNotFound()
		{
			Xsd.ContainerEvent containerEvent = GetXsdContainerEvent("Container");

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, buffer);
			Importer.Import(importContext, containerEvent);
			Assert("Should notify when container not found", buffer.AsString.Contains("Could not find container with Container Number='Container' Voyage='Voyage' Lloyds='TstLlds'"));
		}

		public void TestImportNotifiesInvalidEventDateImported()
		{
			Container.Factory.Save();
			Consol.Factory.Save();
			Xsd.ContainerEvent containerEvent = GetXsdContainerEvent("Container");
			containerEvent.EventDate = new ZDateTime(9999, 1, 30);

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, buffer);
			Importer.Import(importContext, containerEvent);
			AssertContains("Should notify invalid date supplied", "Invalid date provided='30-Jan-99 00:00:00'", buffer.AsString);
		}

		Xsd.ContainerEvent GetXsdContainerEvent(ZString containerNumber)
		{
			Xsd.ContainerEvent result = new Xsd.ContainerEvent();
			result.ContainerNumber = containerNumber;
			result.Vessel.Lloyds = Vessel.RV_LloydsNumber;
			result.Vessel.Voyage = "Voyage";
			result.EventType = Xsd.ContainerEventEventType.ArrivalCartageAdvised;
			result.EventDate = new ZDateTime(2005, 1, 2);
			return result;
		}

		public void TestImport_FindByContainerNumberVesselVoyage()
		{
			Container.Factory.Save();
			Consol.Factory.Save();
			foreach (KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo> date in ContainerDates)
			{
				TestImport_FindByContainerNumberVesselVoyage(date.Key, date.Value);
			}
			AssertEquals("No errors expected during import", false, Notifications.HasErrors);
		}

		public void TestImport_FindByContainerNumberVesselVoyage_ForDeclaration()
		{
			Container.Factory.Save();
			Declaration.Factory.Save();
			foreach (KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo> date in ContainerDates)
			{
				TestImport_FindByContainerNumberVesselVoyage(date.Key, date.Value);
			}
			AssertEquals("No errors expected during import", false, Notifications.HasErrors);
		}

		void TestImport_FindByContainerNumberVesselVoyage(Xsd.ContainerEventEventType eventType, ZPropertyInfo containerEventProperty)
		{
			Xsd.ContainerEvent containerEvent = new Xsd.ContainerEvent();
			containerEvent.ContainerNumber = "Container";
			containerEvent.Vessel.VesselName = "VesselName";
			containerEvent.Vessel.Voyage = "Voyage";
			containerEvent.EventType = eventType;
			containerEvent.EventDate = new ZDateTime(2005, 1, 2);

			var isDurationProperty = containerEventProperty.Name == "ArrivalTruckWaitTime" || containerEventProperty.Name == "DepartureTruckWaitTime";
			var baseYear = isDurationProperty ? ZDateTime.DefaultDurationEpoch.Year : 2005;

			Importer.Import(ImportContext, containerEvent);
			AssertEquals(eventType.ToString(), new ZDateTime(baseYear, 1, 2), containerEventProperty.Value);
		}

		public void TestImport_FindByContainerNumberLloydsVoyage_ForConsol()
		{
			Container.Factory.Save();
			Consol.Factory.Save();
			foreach (KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo> date in ContainerDates)
			{
				TestImport_FindByContainerNumberLloydsVoyage(date.Key, date.Value);
			}
			AssertEquals("No errors expected during import", false, Notifications.HasErrors);
		}

		public void TestImport_FindByContainerNumberLloydsVoyage_ForDeclaration()
		{
			Container.Factory.Save();
			Declaration.Factory.Save();
			foreach (KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo> date in ContainerDates)
			{
				TestImport_FindByContainerNumberLloydsVoyage(date.Key, date.Value);
			}
			AssertEquals("No errors expected during import", false, Notifications.HasErrors);
		}

		void TestImport_FindByContainerNumberLloydsVoyage(Xsd.ContainerEventEventType eventType, ZPropertyInfo containerEventProperty)
		{
			Xsd.ContainerEvent containerEvent = new Xsd.ContainerEvent();
			containerEvent.ContainerNumber = "Container";
			containerEvent.Vessel.Lloyds = Vessel.RV_LloydsNumber;
			containerEvent.Vessel.Voyage = "Voyage";
			containerEvent.EventType = eventType;
			containerEvent.EventDate = new ZDateTime(2005, 1, 2);

			var isDurationProperty = containerEventProperty.Name == "ArrivalTruckWaitTime" || containerEventProperty.Name == "DepartureTruckWaitTime";
			var baseYear = isDurationProperty ? ZDateTime.DefaultDurationEpoch.Year : 2005;

			Importer.Import(ImportContext, containerEvent);
			AssertEquals(eventType.ToString(), new ZDateTime(baseYear, 1, 2), containerEventProperty.Value);
		}

		public void TestImport_FindByJobNumberAndContainerNumber_ForConsol()
		{
			Container.Factory.Save();
			Consol.Factory.Save();
			foreach (KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo> date in ContainerDates)
			{
				TestImport_FindByJobNumberAndContainerNumber_ForConsol(date.Key, date.Value);
			}
			AssertEquals("No errors expected during import", false, Notifications.HasErrors);
		}

		void TestImport_FindByJobNumberAndContainerNumber_ForConsol(Xsd.ContainerEventEventType eventType, ZPropertyInfo containerEventProperty)
		{
			Xsd.ContainerEvent containerEvent = new Xsd.ContainerEvent();
			containerEvent.EnterpriseJobNumber = Consol.JK_UniqueConsignRef;
			containerEvent.ContainerNumber = "Container";
			containerEvent.EventType = eventType;
			containerEvent.EventDate = new ZDateTime(2005, 1, 2);

			var isDurationProperty = containerEventProperty.Name == "ArrivalTruckWaitTime" || containerEventProperty.Name == "DepartureTruckWaitTime";
			var baseYear = isDurationProperty ? ZDateTime.DefaultDurationEpoch.Year : 2005;

			Importer.Import(ImportContext, containerEvent);
			AssertEquals(eventType.ToString(), new ZDateTime(baseYear, 1, 2), containerEventProperty.Value);
		}

		public void TestImport_FindByJobNumberAndContainerNumber_ForDeclaration()
		{
			Container.Factory.Save();
			Declaration.Factory.Save();
			foreach (KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo> date in ContainerDates)
			{
				TestImport_FindByJobNumberAndContainerNumber_ForDeclaration(date.Key, date.Value);
			}
			AssertEquals("No errors expected during import", false, Notifications.HasErrors);
			Factory.Save();

			AssertEquals(new ZDateTime(2005, 1, 2), Declaration["JE_EstimatedDeliveryOrPickup"]);
			AssertEquals(new ZDateTime(2005, 1, 2), Declaration["JE_CartageCompleted"]);

			AssertEquals(new ZDateTime(2005, 1, 2), Container["JC_ArrivalEstimatedDelivery"]);
			AssertEquals(new ZDateTime(2005, 1, 2), Container["JC_ArrivalCartageComplete"]);
		}

		void TestImport_FindByJobNumberAndContainerNumber_ForDeclaration(Xsd.ContainerEventEventType eventType, ZPropertyInfo containerEventProperty)
		{
			Xsd.ContainerEvent containerEvent = new Xsd.ContainerEvent();
			containerEvent.EnterpriseJobNumber = (ZString)Declaration[JobDeclarationSchema.JE_DeclarationReference];
			containerEvent.ContainerNumber = "Container";
			containerEvent.EventType = eventType;
			containerEvent.EventDate = new ZDateTime(2005, 1, 2);

			var isDurationProperty = containerEventProperty.Name == "ArrivalTruckWaitTime" || containerEventProperty.Name == "DepartureTruckWaitTime";
			var baseYear = isDurationProperty ? ZDateTime.DefaultDurationEpoch.Year : 2005;

			Importer.Import(ImportContext, containerEvent);
			AssertEquals(eventType.ToString(), new ZDateTime(baseYear, 1, 2), containerEventProperty.Value);
		}

		public void TestImport_FindByContainerNumberAndArrivalDate_ForConsol()
		{
			Container.Factory.Save();
			Consol.Factory.Save();
			foreach (KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo> date in ContainerDates)
			{
				TestImport_FindByContainerNumberAndArrivalDate(date.Key, date.Value);
			}
			AssertEquals("No errors expected during import", false, Notifications.HasErrors);
		}

		public void TestImport_FindByContainerNumberAndArrivalDate_ForDeclaration()
		{
			Container.Factory.Save();
			Declaration.Factory.Save();
			foreach (KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo> date in ContainerDates)
			{
				TestImport_FindByContainerNumberAndArrivalDate(date.Key, date.Value);
			}
			AssertEquals("No errors expected during import", false, Notifications.HasErrors);
		}

		void TestImport_FindByContainerNumberAndArrivalDate(Xsd.ContainerEventEventType eventType, ZPropertyInfo containerEventProperty)
		{
			containerEventProperty.Value = ZDateTime.Empty;

			Xsd.ContainerEvent containerEvent = new Xsd.ContainerEvent();
			containerEvent.ContainerNumber = "Container";
			containerEvent.EventType = eventType;
			containerEvent.EventDate = new ZDateTime(2005, 1, 2);

			containerEvent.Vessel.ArrivalDate = TestArrivalDate.AddDays(8);
			Importer.Import(ImportContext, containerEvent);
			AssertEquals(eventType.ToString(), ZDateTime.Empty, containerEventProperty.Value);

			containerEvent.Vessel.ArrivalDate = TestArrivalDate.AddDays(-8);
			Importer.Import(ImportContext, containerEvent);
			AssertEquals(eventType.ToString(), ZDateTime.Empty, containerEventProperty.Value);

			var isDurationProperty = containerEventProperty.Name == "ArrivalTruckWaitTime" || containerEventProperty.Name == "DepartureTruckWaitTime";
			var baseYear = isDurationProperty ? ZDateTime.DefaultDurationEpoch.Year : 2005;

			containerEvent.Vessel.ArrivalDate = TestArrivalDate.AddDays(7);
			Importer.Import(ImportContext, containerEvent);
			AssertEquals(eventType.ToString(), new ZDateTime(baseYear, 1, 2), containerEventProperty.Value);
			containerEventProperty.Value = ZDateTime.Empty;

			containerEvent.Vessel.ArrivalDate = TestArrivalDate.AddDays(-7);
			Importer.Import(ImportContext, containerEvent);
			AssertEquals(eventType.ToString(), new ZDateTime(baseYear, 1, 2), containerEventProperty.Value);
			containerEventProperty.Value = ZDateTime.Empty;
		}

		public void TestImport_WithoutSpecifyingKeyInformation()
		{
			Xsd.ContainerEvent containerEvent = new Xsd.ContainerEvent();

			containerEvent.ContainerNumber = "ContainerNumber";
			Importer.Import(ImportContext, containerEvent);
			AssertEquals("Specifying only ContainerNumber is invalid", true, Notifications.HasErrors);
			AssertEquals($"Error: Key information was not specified (Container Number, Vessel Name, Voyage), ({Core.Constants.ProductName} Job Number, Container Number) or (Container Number, Arrival Date).", Notifications.AsString.Trim());
			Notifications.Clear();

			containerEvent.ContainerNumber = "ContainerNumber";
			containerEvent.Vessel.VesselName = "VesselName";
			Importer.Import(ImportContext, containerEvent);
			AssertEquals("Specifying only ContainerNumber/VesselName is invalid", true, Notifications.HasErrors);
			Notifications.Clear();

			containerEvent.ContainerNumber = "";
			containerEvent.Vessel.ArrivalDate = ZDate.Today;
			Importer.Import(ImportContext, containerEvent);
			AssertEquals("Specifying only ArrivalDate is invalid", true, Notifications.HasErrors);
			Notifications.Clear();
		}

		public void TestImport_WhenContainerNotFound()
		{
			Xsd.ContainerEvent containerEvent = new Xsd.ContainerEvent();
			containerEvent.ContainerNumber = "Container";
			containerEvent.Vessel.VesselName = "Vessel";
			containerEvent.Vessel.Voyage = "Voyage";
			containerEvent.Vessel.Lloyds = "Lloyds";
			containerEvent.Vessel.ArrivalDate = new ZDate(2005, 1, 2);

			Importer.Import(ImportContext, containerEvent);
			AssertEquals(
				"An informational only message should be shown",
				"Could not find container with Container Number='Container' Vessel Name='Vessel' Voyage='Voyage' Lloyds='Lloyds' Arrival Date='02-Jan-05'",
				Notifications.AsString.Trim());
		}

		#endregion

		#region Implementation

		readonly ZDate TestArrivalDate = new ZDate(2005, 1, 7);

		ContainerEventImporter Importer
		{
			get
			{
				if (importer == null)
				{
					importer = new ContainerEventImporter();
				}
				return importer;
			}
		}
		ContainerEventImporter importer;

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Factory.New<CommonContainer>();
					container.JC_ContainerNum = "Container";
				}
				return container;
			}
		}
		CommonContainer container;

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
					consol.JK_UniqueConsignRef = "C0001001";
					consol.Transports.MostInterestingTransport.JW_ETA = TestArrivalDate;
					consol.Transports.MostInterestingTransport.JW_Vessel = "VesselName";
					consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Voyage";
					consol.Containers.Add(Container);
				}
				return consol;
			}
		}
		CommonConsol consol;

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					declaration[JobDeclarationSchema.Constants.JE_TransportMode] = Core.Constants.TransportModes.Sea;
					declaration[JobDeclarationSchema.JE_DeclarationReference] = "B0001001";
					declaration[JobDeclarationSchema.JE_DateOfArrival] = (ZDateTime)TestArrivalDate;
					declaration[JobDeclarationSchema.JE_VesselName] = "VesselName";
					declaration[JobDeclarationSchema.JE_VoyageFlightNo] = "Voyage";

					BusinessObjectCollection cusContainers = (BusinessObjectCollection)declaration["CusContainers"];
					BusinessObject cusContainer = cusContainers.AddNew();
					cusContainer[CusContainerSchema.CO_ContainerNumber] = Container.JC_ContainerNum;
					cusContainer[CusContainerSchema.CO_JC] = Container.PK;
				}
				return declaration;
			}
		}
		BusinessObject declaration;

		RefVessel Vessel
		{
			get
			{
				if (vessel == null)
				{
					vessel = Factory.New<RefVessel>();
					vessel.RV_LloydsNumber = "TstLlds";
					vessel.RV_Name = "VesselName";
				}
				return vessel;
			}
		}
		RefVessel vessel;

		ValueObjectImportContext ImportContext
		{
			get
			{
				if (importContext == null)
				{
					importContext = new ValueObjectImportContext(Factory, Notifications);
				}
				return importContext;
			}
		}
		ValueObjectImportContext importContext;

		readonly NotificationBuffer Notifications = new NotificationBuffer();

		IEnumerable<KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>> ContainerDates
		{
			get
			{
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.LCLStorageCommences, Container.JC_LCLStorageCommencesInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.ContainerYardEmptyPickupGateOut, Container.JC_ContainerYardEmptyPickupGateOutInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.DepartureCartageAdvised, Container.JC_DepartureCartageAdvisedInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.DepartureCartageComplete, container.JC_DepartureCartageCompleteInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.DepartureCartageDemurrageTime, container.DepartureTruckWaitTimeInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.ArrivalEstimatedDelivery, container.JC_ArrivalEstimatedDeliveryInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.ArrivalCartageAdvised, container.JC_ArrivalCartageAdvisedInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.ArrivalCartageComplete, container.JC_ArrivalCartageCompleteInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.ArrivalCartageDemurrageTime, container.ArrivalTruckWaitTimeInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.FCLWharfGateIn, container.JC_FCLWharfGateInInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.FCLOnBoardVessel, container.JC_FCLOnBoardVesselInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.FCLUnloadFromVessel, container.JC_FCLUnloadFromVesselInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.FCLAvailable, container.JC_FCLAvailableInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.FCLWharfGateOut, container.JC_FCLWharfGateOutInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.FCLStorageCommences, container.JC_ArrivalCTOStorageStartDateInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.LCLUnpack, container.JC_LCLUnpackInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.LCLAvailable, container.JC_LCLAvailableInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.LCLStorageCommences, container.JC_LCLStorageCommencesInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.EmptyRequired, container.JC_EmptyRequiredInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.EmptyReadyForReturn, container.JC_EmptyReadyForReturnInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.EmptyReturnBy, container.JC_EmptyReturnedByInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.ContainerYardEmptyReturnGateIn, container.JC_ContainerYardEmptyReturnGateInInfo);
				yield return new KeyValuePair<Xsd.ContainerEventEventType, ZPropertyInfo>(Xsd.ContainerEventEventType.DepartureEstimatedPickup, container.JC_DepartureEstimatedPickupInfo);
			}
		}

		#endregion
	}
}
