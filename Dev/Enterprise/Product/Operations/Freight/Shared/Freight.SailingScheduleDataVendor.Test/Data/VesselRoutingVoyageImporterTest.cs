using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class VesselRoutingVoyageImporterTest : TestCaseWithFactory
	{
		#region Import

		public void TestImportVoyage()
		{
			VesselRoutingVoyage voyage = NewVesselRoutingVoyage("VesselName", "Lloyds", "Voyage");
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Import | PortPairTypes.Export;
			SetupPortPairData(voyage);

			#region ExpectedNotificationsWhenImportingInitially
			const string ExpectedNotificationsWhenImportingInitially = @"
Vessel 'VesselName' with Lloyds number 'Lloyds' created

Importing Voyage (Vessel Name='VesselName' Voyage='Voyage')
-----------------------------------------------------------
Sailing Port Pair (Load='MYPKG' Discharge='AUSYD') created
Sailing Port Pair (Load='AUSYD' Discharge='NZAKL') created

Sailing schedules imported successfully.
";
			#endregion

			Importer.Import(ImportContext);
			JobVoyage importedVoyage = Factory.LoadTop1<JobVoyage>(new ZQuery(JobVoyageSchema.JV_VoyageFlight, "Voyage"));
			AssertEquals("Notifications when performing first import", ExpectedNotificationsWhenImportingInitially.Trim(), Notifications.AsString.Trim());
			AssertEquals("Schedule should be imported and saved", true, importedVoyage.IsInDatabase);
			AssertEquals("RefVessel should be automatically created and set", "VesselName", importedVoyage.JV_RV_NKVessel);
			AssertEquals("Voyage number", "Voyage", importedVoyage.JV_VoyageFlight);

			AssertEquals("Sailing port pairs should be created", 3, importedVoyage.Sailings.Count);
			AssertImportedPortPairs(importedVoyage);
			Notifications.Clear();

			#region ExpectedNotificationsWhenImportingASecondTime
			const string ExpectedNotificationsWhenImportingASecondTime = @"
Importing Voyage (Vessel Name='VesselName' Voyage='Voyage')
-----------------------------------------------------------
Sailing Port Pair (Load='MYPKG' Discharge='AUSYD') updated
Sailing Port Pair (Load='AUSYD' Discharge='NZAKL') updated

Sailing schedules imported successfully.
";
			#endregion

			Importer.Import(ImportContext);
			importedVoyage = Factory.LoadTop1<JobVoyage>(new ZQuery(JobVoyageSchema.JV_VoyageFlight, "Voyage"));
			AssertEquals("Notifications when performing second, updating import", ExpectedNotificationsWhenImportingASecondTime.Trim(), Notifications.AsString.Trim());
			AssertEquals("Sailing port pairs should be updated and not recreated", 3, importedVoyage.Sailings.Count);
			AssertImportedPortPairs(importedVoyage);
		}

		void SetupPortPairData(VesselRoutingVoyage voyage)
		{
			VesselRoutingPortPair portPair1 = voyage.PortPairs.AddNew();
			portPair1.E9_RL_NKLoadPort = "MYPKG";
			portPair1.E9_RL_NKDischargePort = "AUSYD";
			portPair1.E9_IsSelected = true;

			portPair1.E9_ETD = new ZDateTime(2000, 1, 11);
			portPair1.E9_ATD = new ZDateTime(2000, 1, 12);
			portPair1.E9_ETA = new ZDateTime(2000, 1, 13);
			portPair1.E9_ATA = new ZDateTime(2000, 1, 14);

			portPair1.E9_CargoCutOff = new ZDateTime(2000, 1, 1);
			portPair1.E9_ExportReceivalCommences = new ZDateTime(2000, 1, 2);
			portPair1.E9_ImportAvailability = new ZDateTime(2000, 1, 21);
			portPair1.E9_ImportStorageCommences = new ZDateTime(2000, 1, 22);

			VesselRoutingPortPair portPair2 = voyage.PortPairs.AddNew();
			portPair2.E9_RL_NKLoadPort = "AUSYD";
			portPair2.E9_RL_NKDischargePort = "NZAKL";
			portPair2.E9_IsSelected = true;

			portPair2.E9_ETD = new ZDateTime(2000, 1, 21);
			portPair2.E9_ATD = new ZDateTime(2000, 1, 22);
			portPair2.E9_ETA = new ZDateTime(2000, 1, 23);
			portPair2.E9_ATA = new ZDateTime(2000, 1, 24);
		}

		void AssertImportedPortPairs(JobVoyage importedSchedule)
		{
			JobSailing importedPortPair1 = importedSchedule.Sailings.GetSailingFromLoadAndDischarge("MYPKG", "AUSYD");
			JobSailing importedPortPair2 = importedSchedule.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");

			AssertEquals("Imported ETD", new ZDateTime(2000, 1, 11), importedPortPair1.Origin.JA_E_DEP);
			AssertEquals("Imported ETA", new ZDateTime(2000, 1, 13), importedPortPair1.Destination.JB_E_ARV);
			AssertEquals("Imported ATD", new ZDateTime(2000, 1, 12), importedPortPair1.Origin.JA_A_DEP);
			AssertEquals("Imported ATA", new ZDateTime(2000, 1, 14), importedPortPair1.Destination.JB_A_ARV);

			AssertEquals("Imported Cut-off Date", new ZDateTime(2000, 1, 1), importedPortPair1.JX_JA_CTOCutOff);
			AssertEquals("Imported Receival Commences Date", new ZDateTime(2000, 1, 2), importedPortPair1.JX_JA_CTOReceivalCommences);
			AssertEquals("Imported Availability Date", new ZDateTime(2000, 1, 21), importedPortPair1.JX_JB_CTOAvailabilityDate);
			AssertEquals("Imported Storage Date", new ZDateTime(2000, 1, 22), importedPortPair1.JX_JB_CTOStorageDate);
		}

		#endregion

		#region EnsureVesselsExist

		public void TestEnsureVesselsExist_CreatingNewVessel()
		{
			VesselRoutingVoyage voyage = NewVesselRoutingVoyage("Vessel Name", "Lloyds", "Voyage");
			VesselRoutingPortPair portPair = voyage.PortPairs.AddNew();
			portPair.E9_RL_NKLoadPort = "MYPKG";
			portPair.E9_RL_NKDischargePort = "AUSYD";
			portPair.E9_IsSelected = true;
			AssertEquals("Vessel should not exist initially", null, voyage.Vessel);

			Importer.ImportToFactory(ImportContext);
			AssertEquals("Vessel should be created with correct vessel name", "Vessel Name", voyage.Vessel.RV_Name);
			AssertEquals("Vessel should be created with correct lloyds", "Lloyds", voyage.Vessel.RV_LloydsNumber);
			AssertEquals("Notifications",
@"Vessel 'Vessel Name' with Lloyds number 'Lloyds' created

Importing Voyage (Vessel Name='Vessel Name' Voyage='Voyage')
------------------------------------------------------------
Sailing Port Pair (Load='MYPKG' Discharge='AUSYD') created

", Notifications.AsString);
		}

		public void TestEnsureVesselsExist_ResolvingVesselNameAmbiguity()
		{
			VesselRoutingVoyage voyage = NewVesselRoutingVoyage("New Vessel Name", "Lloyds", "Voyage");
			VesselRoutingPortPair portPair = voyage.PortPairs.AddNew();
			portPair.E9_RL_NKLoadPort = "MYPKG";
			portPair.E9_RL_NKDischargePort = "AUSYD";
			portPair.E9_IsSelected = true;
			RefVessel firstVessel = NewRefVessel("First Vessel Name", "Lloyds");
			RefVessel vesselUserWillSelect = NewRefVessel("Vessel user will select", "Lloyds");
			AssertEquals("Vessel should not be matched initially", null, voyage.Vessel);

			Importer.ImportToFactory(ImportContext);
			AssertEquals("Vessel ambiguity should be resolved", vesselUserWillSelect, voyage.Vessel);
			AssertEquals(
@"Vessel 'New Vessel Name' was chosen as the match for Lloyds number 'Lloyds'

Importing Voyage (Vessel Name='New Vessel Name' Voyage='Voyage')
----------------------------------------------------------------
Sailing Port Pair (Load='MYPKG' Discharge='AUSYD') created

", Notifications.AsString);
		}

		public void TestEnsureVesselsExist_ResolvingVesselNameAmbiguity_WhenVesselNameExists()
		{
			RefVessel existingVessel = NewRefVessel("MatchingVesselName", "WrngLds");
			VesselRoutingVoyage voyage = NewVesselRoutingVoyage("MatchingVesselName", "RigtLds", "Voyage");
			AssertEquals("Vessel should not be matched initially", null, voyage.Vessel);

			Importer.ImportToFactory(ImportContext);
			AssertEquals("Should query the user to select the vessel with the wrong lloyds number", typeof(QueryUserSelectVesselFromLloydsNumber), Notifications.LastUserQuery.GetType());
		}

		public void TestEnsureVesselsExist_WhenVesselNameAmbiguityResolutionCancelled()
		{
			RefVessel firstVessel = NewRefVessel("First Vessel Name", "Lloyds");
			RefVessel secondVessel = NewRefVessel("Second Vessel Name", "Lloyds");
			Factory.Save();

			VesselRoutingVoyage voyage = NewVesselRoutingVoyage("New Vessel Name", "Lloyds", "Voyage");
			AssertEquals("Vessel should not be matched initially", null, voyage.Vessel);

			Notifications.UserCancelMatchOfVessel = true;
			ImportContext.Factory.Saved += delegate
			{ Fail("Didn't expect the factory to be saved when the user cancels"); };
			Importer.Import(Notifications);
			AssertEquals("Vessel should not have been matched after", null, voyage.Vessel);
			AssertEquals("Error: The import was canceled at the user's request", Notifications.AsString.Trim());
		}

		public void TestEnsureVesselsExist_When2VoyagesHaveSameLloydsToBeCreated()
		{
			VesselRoutingVoyage voyage1 = NewVesselRoutingVoyage("Vessel Name", "Lloyds", "Voyage1");
			VesselRoutingPortPair portPair1 = voyage1.PortPairs.AddNew();
			portPair1.E9_RL_NKLoadPort = "MYPKG";
			portPair1.E9_RL_NKDischargePort = "AUSYD";
			portPair1.E9_IsSelected = true;
			AssertEquals("Vessel should not exist initially", null, voyage1.Vessel);

			VesselRoutingVoyage voyage2 = NewVesselRoutingVoyage("Vessel Name", "Lloyds", "Voyage2");
			VesselRoutingPortPair portPair2 = voyage2.PortPairs.AddNew();
			portPair2.E9_RL_NKLoadPort = "MYPKG";
			portPair2.E9_RL_NKDischargePort = "AUSYD";
			portPair2.E9_IsSelected = true;
			AssertEquals("Vessel should not exist initially", null, voyage2.Vessel);

			Importer.ImportToFactory(ImportContext);
			AssertEquals("Vessel should be created with correct vessel name", "Vessel Name", voyage1.Vessel.RV_Name);
			AssertEquals("Vessel should be created with correct lloyds", "Lloyds", voyage1.Vessel.RV_LloydsNumber);
			AssertEquals("Same vessel should be assigned to both voyages", voyage1.Vessel.PK, voyage2.Vessel.PK);

			AssertEquals("Notifications",
@"Vessel 'Vessel Name' with Lloyds number 'Lloyds' created

Importing Voyage (Vessel Name='Vessel Name' Voyage='Voyage1')
-------------------------------------------------------------
Sailing Port Pair (Load='MYPKG' Discharge='AUSYD') created

Importing Voyage (Vessel Name='Vessel Name' Voyage='Voyage2')
-------------------------------------------------------------
Sailing Port Pair (Load='MYPKG' Discharge='AUSYD') created

", Notifications.AsString);
		}

		#endregion

		#region Test Classes

		class TestVesselRoutingVoyageImporter : VesselRoutingVoyageImporter
		{
			public TestVesselRoutingVoyageImporter(VesselRoutingVoyage[] voyages)
				: base(voyages)
			{
			}

			public new void ImportToFactory(ValueObjectImportContext importContext)
			{
				base.ImportToFactory(importContext);
			}
		}

		class TestNotificationSubscriber : NotificationBuffer
		{
			public bool UserCancelMatchOfVessel;
			public IQueryUserEventArgs LastUserQuery;

			protected override void QueryUser(IQueryUserEventArgs e)
			{
				LastUserQuery = e;

				QueryUserSelectVesselFromLloydsNumber vesselSelect = e as QueryUserSelectVesselFromLloydsNumber;
				if (vesselSelect != null)
				{
					if (!UserCancelMatchOfVessel)
					{
						foreach (RefVessel vessel in vesselSelect.AvailableVessels)
						{
							if (vessel.RV_Name == "Vessel user will select")
							{
								vesselSelect.SelectedVessel = vessel;
							}
						}
					}
				}
				else
				{
					throw new NotSupportedException("Not supported for this test");
				}
			}
		}

		#endregion

		#region Implementation

		VesselRoutingVoyage NewVesselRoutingVoyage(ZString vesselName, ZString lloyds, ZString voyage)
		{
			VesselRoutingVoyage result = VoyageCollection.AddNew();
			result.E8_VesselName = vesselName;
			result.E8_LloydsNumber = lloyds;
			result.E8_Voyage = voyage;
			return result;
		}

		RefVessel NewRefVessel(ZString vesselName, ZString lloyds)
		{
			RefVessel result = Factory.New<RefVessel>();
			result.RV_Name = vesselName;
			result.RV_LloydsNumber = lloyds;
			return result;
		}

		TestVesselRoutingVoyageImporter Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = new TestVesselRoutingVoyageImporter((VesselRoutingVoyage[])VoyageCollection.ToArray(typeof(VesselRoutingVoyage)));
				}
				return fImporter;
			}
		}
		TestVesselRoutingVoyageImporter fImporter;

		VesselRoutingVoyageCollection VoyageCollection
		{
			get
			{
				if (fVoyageCollection == null)
				{
					fVoyageCollection = new VesselRoutingVoyageCollection(Factory);
				}
				return fVoyageCollection;
			}
		}
		VesselRoutingVoyageCollection fVoyageCollection;

		ValueObjectImportContext ImportContext
		{
			get
			{
				if (fImportContext == null)
				{
					fImportContext = new ValueObjectImportContext(Factory, Notifications);
				}
				return fImportContext;
			}
		}
		ValueObjectImportContext fImportContext;

		TestNotificationSubscriber Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new TestNotificationSubscriber();
				}
				return fNotifications;
			}
		}
		TestNotificationSubscriber fNotifications;

		#endregion
	}
}
