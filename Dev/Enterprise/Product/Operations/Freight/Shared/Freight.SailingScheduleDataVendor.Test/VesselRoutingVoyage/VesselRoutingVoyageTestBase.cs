using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	abstract class VesselRoutingVoyageTestBase : EnterpriseBusinessObjectTestCase
	{
		public abstract void TestE8_OH_LineOperator_DefaultsOrgFromE8_LineOperator();

		public void TestPortPairs()
		{
			AssertEquals("Expected 5 port pairs", 5, Voyage.PortPairs.Count);

			AssertEquals("1st port pair", "MYPKG", Voyage.PortPairs[0].E9_RL_NKLoadPort);
			AssertEquals("1st port pair", "AUSYD", Voyage.PortPairs[0].E9_RL_NKDischargePort);

			AssertEquals("2nd port pair", "MYPKG", Voyage.PortPairs[1].E9_RL_NKLoadPort);
			AssertEquals("2nd port pair", "AUMEL", Voyage.PortPairs[1].E9_RL_NKDischargePort);

			AssertEquals("3rd port pair", "AUSYD", Voyage.PortPairs[2].E9_RL_NKLoadPort);
			AssertEquals("3rd port pair", "AUMEL", Voyage.PortPairs[2].E9_RL_NKDischargePort);

			AssertEquals("4th port pair", "AUSYD", Voyage.PortPairs[3].E9_RL_NKLoadPort);
			AssertEquals("4th port pair", "MYPKG", Voyage.PortPairs[3].E9_RL_NKDischargePort);

			AssertEquals("5th port pair", "AUMEL", Voyage.PortPairs[4].E9_RL_NKLoadPort);
			AssertEquals("5th port pair", "MYPKG", Voyage.PortPairs[4].E9_RL_NKDischargePort);
		}

		public void TestReadOnlyOfProperties()
		{
			AssertEquals("E8_OH_LineOperator should be editable", false, Voyage.E8_OH_LineOperatorInfo.ReadOnly);

			AssertEquals("Other properties should be read only (E8_IsSelected)", true, Voyage.E8_IsSelectedInfo.ReadOnly);
			AssertEquals("Other properties should be read only (E8_LloydsNumber)", true, Voyage.E8_LloydsNumberInfo.ReadOnly);
			AssertEquals("Other properties should be read only (E8_Voyage)", true, Voyage.E8_VoyageInfo.ReadOnly);
			AssertEquals("Other properties should be read only (E8_FirstArrival)", true, Voyage.E8_FirstArrivalInfo.ReadOnly);
			AssertEquals("Other properties should be read only (E8_LastDeparture)", true, Voyage.E8_LastDepartureInfo.ReadOnly);
		}

		public void TestDeselectAllPortPairs()
		{
			VesselRoutingPortPair portPair1 = Voyage.PortPairs.AddNew();
			VesselRoutingPortPair portPair2 = Voyage.PortPairs.AddNew();
			Voyage.PortPairs[0].E9_IsSelected = true;
			Voyage.PortPairs[1].E9_IsSelected = true;

			AssertEquals("Before deselecting", true, Voyage.PortPairs[0].E9_IsSelected);
			AssertEquals("Before deselecting", true, Voyage.PortPairs[1].E9_IsSelected);
			Voyage.DeselectAllPortPairs();
			AssertEquals("After deselecting", false, Voyage.PortPairs[0].E9_IsSelected);
			AssertEquals("After deselecting", false, Voyage.PortPairs[1].E9_IsSelected);
		}

		public void TestSameLloydsVoyageDifferentVesselName()
		{
			JobVesselSchedule port1 = NewJobVesselScheduleAndCreateClone("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1), "Lloyds", "Voyage", "Voyage", string.Empty, "Vessel");
			JobVesselSchedule port2 = NewJobVesselScheduleAndCreateClone("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1), "Lloyds", "Voyage", "Voyage", string.Empty, "Same Vessel Just Different Name");
			Factory.Save();

			ZQuery query = new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, "Voyage");
			query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_DataProvider, DataProvider);
			VesselRoutingVoyage[] voyages = Factory.Load<VesselRoutingVoyage>(query);
			AssertEquals("Number of voyages returned", NumberOfVoyages_TestSameLloydsVoyageDifferentVesselName, voyages.Length);
		}

		public void TestRunPreSaveValidation()
		{
			VesselRoutingPortPair portPair = Voyage.PortPairs.AddNew();
			portPair.E9_RL_NKLoadPort = "MYPKG";
			portPair.E9_RL_NKDischargePort = "AUSYD";
			portPair.E9_IsSelected = true;
			Voyage.E8_OH_LineOperator = ZGuid.Invalid;

			Voyage.ClearAllNotifications();
			AssertNoErrors("No errors initially for E8_OH_LineOperator", Voyage.E8_OH_LineOperatorInfo);

			Voyage.RunPreSaveValidation();
			AssertHasErrors("Has errors after RunPreSaveValidation() for E8_OH_LineOperator", Voyage.E8_OH_LineOperatorInfo);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot save or delete this business object because it is based on an sql view", true);
		}

		public void TestForeignPorts_SurvivesVoyageReload()
		{
			List<string> foreignPorts = Voyage.ForeignPorts;
			VoyageCollection.Load(new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, Voyage.E8_Voyage));
			AssertEquals(
				"ForeignPorts instance should be the same even after the data has been re-loaded so the data is retained after the voyage is refreshed",
				true, foreignPorts == VoyageCollection[0].ForeignPorts);
		}

		public void TestVessel()
		{
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			Voyage.E8_LloydsNumber = vessel.RV_LloydsNumber;
			AssertEquals("Vessel should be loaded from the Lloyds number", vessel.PK, Voyage.Vessel.PK);
		}

		public void TestVessel_WhenAmbiguousLloydsNumber_MatchOnVesselName()
		{
			RefVessel vessel = NewRefVessel("VesselName", "Lloyds");
			RefVessel decoyVessel = NewRefVessel("DecoyVesselName", "Lloyds");
			Factory.Save();

			Voyage.E8_VesselName = "VesselName";
			Voyage.E8_LloydsNumber = "Lloyds";
			AssertEquals("Vessel should be loaded from the Lloyds AND vessel name if there is a duplicate vessel by lloyds", vessel.PK, Voyage.Vessel.PK);
		}

		public void TestVessel_WhenAmbiguousLloydsNumber_AndNoMatchOnVesselName()
		{
			RefVessel vessel1 = NewRefVessel("Ambiguous Vessel 1", "Lloyds");
			RefVessel vessel2 = NewRefVessel("Ambiguous Vessel 2", "Lloyds");
			Factory.Save();

			Voyage.E8_VesselName = "VesselName";
			Voyage.E8_LloydsNumber = "Lloyds";
			AssertEquals("No vessel should not be loaded because there is an ambiguous match on Lloyds number, and no match on vessel name", null, Voyage.Vessel);
		}

		RefVessel NewRefVessel(ZString vesselName, ZString lloyds)
		{
			RefVessel result = Factory.New<RefVessel>();
			result.RV_Name = vesselName;
			result.RV_LloydsNumber = lloyds;
			return result;
		}

		#region New Properties

		#region E8_IsSelected

		public void TestE8_IsSelected()
		{
			VesselRoutingPortPair portPair1 = Voyage.PortPairs.AddNew();
			VesselRoutingPortPair portPair2 = Voyage.PortPairs.AddNew();
			portPair1.E9_RL_NKLoadPort = "MYPKG";
			portPair1.E9_RL_NKDischargePort = "AUSYD";
			portPair2.E9_RL_NKLoadPort = "MYPKG";
			portPair2.E9_RL_NKDischargePort = "AUMEL";

			AssertEquals("Initial value", false, Voyage.E8_IsSelected);

			portPair1.E9_IsSelected = true;
			AssertEquals("With 1 port pair selected", true, Voyage.E8_IsSelected);
			portPair2.E9_IsSelected = true;
			AssertEquals("With 2 port pairs selected", true, Voyage.E8_IsSelected);

			portPair1.E9_IsSelected = false;
			portPair2.E9_IsSelected = false;
			AssertEquals("With no port pairs selected", false, Voyage.E8_IsSelected);
		}

		#endregion

		#region E8_OH_LineOperator

		public void TestE8_OH_LineOperator_NotReadOnly()
		{
			AssertEquals(false, VoyageWithLineOperators.E8_OH_LineOperatorInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region PortPairTypeFilter

		public void TestPortPairTypeFilter()
		{
			VoyageCollection.PortPairTypeFilter = PortPairTypes.Import | PortPairTypes.Domestic;
			AssertEquals("PortPairTypeFilter", PortPairTypes.Import | PortPairTypes.Domestic, Voyage.PortPairTypeFilter);

			AssertEquals(true, Voyage.IsPortPairTypeInFilter(PortPairTypes.Import));
			AssertEquals(true, Voyage.IsPortPairTypeInFilter(PortPairTypes.Domestic));
			AssertEquals(true, Voyage.IsPortPairTypeInFilter(PortPairTypes.Export));
		}

		#endregion

		#region Lookups

		public void TestE8_OH_LineOperator_List()
		{
			AssertEquals(typeof(SeaShippingProviderCollection), Voyage.E8_OH_LineOperator_List.GetType());
		}

		public void TestPorts()
		{
			AssertNotNull(Voyage.Ports);
		}

		#endregion

		#region Implementation

		protected VesselRoutingVoyageCollection VoyageCollection
		{
			get
			{
				if (voyageCollection == null)
				{
					voyageCollection = new VesselRoutingVoyageCollection(Factory);
					voyageCollection.PortPairTypeFilter = PortPairTypes.All;
				}

				return voyageCollection;
			}
		}
		VesselRoutingVoyageCollection voyageCollection;

		protected abstract OrgHeader LineOperator { get; }
		protected OrgHeader lineOperator;

		protected JobVesselRouting NewJobVesselRouting(ZString portCode, ZString lloyds, ZString voyage)
		{
			return NewJobVesselRouting(portCode, lloyds, voyage, ZGuid.Empty);
		}

		protected JobVesselRouting NewJobVesselRouting(ZString portCode, ZString lloyds, ZString voyage, ZGuid schedulePK)
		{
			JobVesselRouting result = Factory.New<JobVesselRouting>();
			result.E1_RL_NKDischargePortCode = portCode;
			result.E1_LloydsID = lloyds;
			result.E1_VoyageNumber = voyage;
			result.E1_EV = schedulePK;

			return result;
		}

		protected JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZDateTime eTA, ZDateTime eTD, ZString lloyds, ZString voyageIn, ZString voyageOut, ZString dataProviderReference, ZString vesselName)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portCode;
			result.EV_ETA = eTA;
			result.EV_ETD = eTD;
			result.EV_IMOLloydsNumber = lloyds;
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			result.EV_DataProviderReference = dataProviderReference;
			result.EV_DataProvider = DataProvider;
			result.EV_ShipName = vesselName;

			return result;
		}

		protected JobVesselSchedule NewJobVesselScheduleAndCreateClone(ZString portCode, ZDateTime eTA, ZDateTime eTD, ZString lloyds, ZString voyageIn, ZString voyageOut, ZString dataProviderReference, ZString vesselName)
		{
			JobVesselSchedule result = NewJobVesselSchedule(portCode, eTA, eTD, lloyds, voyageIn, voyageOut, dataProviderReference, vesselName);
			CloneJobVesselScheduleWithDifferentDataProvider(result);
			return result;
		}

		protected JobVesselSchedule CloneJobVesselScheduleWithDifferentDataProvider(JobVesselSchedule schedule)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.CopyPersistentValuesFrom(schedule);
			result.EV_DataProvider = DataProviderFromOtherCountry(schedule.EV_DataProvider);
			return result;
		}

		protected JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZDateTime eTA, ZDateTime eTD, ZString lloyds, ZString voyageIn, ZString voyageOut)
		{
			return NewJobVesselSchedule(portCode, eTA, eTD, lloyds, voyageIn, voyageOut, string.Empty, string.Empty);
		}

		protected JobVesselSchedule NewJobVesselScheduleAndCreateClone(ZString portCode, ZDateTime eTA, ZDateTime eTD, ZString lloyds, ZString voyageIn, ZString voyageOut)
		{
			return NewJobVesselScheduleAndCreateClone(portCode, eTA, eTD, lloyds, voyageIn, voyageOut, string.Empty, string.Empty);
		}

		ZString DataProviderFromOtherCountry(ZString provider)
		{
			string result;

			switch (provider)
			{
				case FreightConstants.VesselDataProviders.DBH:
				case FreightConstants.VesselDataProviders.DAKOSY:
					result = FreightConstants.VesselDataProviders.OneStop;
					break;

				case FreightConstants.VesselDataProviders.OneStop:
					result = FreightConstants.VesselDataProviders.DBH;
					break;

				default:
					result = "XXX";
					break;
			}

			return result;
		}

		protected abstract ZString DataProvider { get; }

		protected override BusinessObject GetNewBusinessObject()
		{
			return VoyageCollection.AddNew();
		}

		protected VesselRoutingVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					var foreignPort = NewJobVesselRouting("MYPKG", "Lloyds", "Voyage");
					var domesticPort1 = NewJobVesselScheduleAndCreateClone("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "Voyage", "Voyage");
					var domesticPort2 = NewJobVesselScheduleAndCreateClone("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "Voyage", "Voyage");
					Factory.Save();

					VoyageCollection.Load(new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, "Voyage"));
					voyage = VoyageCollection[0];
				}

				return voyage;
			}
		}
		VesselRoutingVoyage voyage;

		protected VesselRoutingVoyage VoyageWithLineOperators
		{
			get
			{
				if (voyageWithLineOperators == null)
				{
					var domesticPort1 = NewJobVesselScheduleAndCreateClone("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "Voyage", "Voyage");
					domesticPort1.EV_LineOperator = "LO1";
					domesticPort1.EV_OperatorsDescription = "Line Operator 1";
					var domesticPort2 = NewJobVesselScheduleAndCreateClone("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "Voyage", "Voyage");
					domesticPort2.EV_LineOperator = "LO2";
					domesticPort2.EV_OperatorsDescription = "Line Operator 2";

					Factory.Save();
					VoyageCollection.Load(new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, "Voyage"));
					voyageWithLineOperators = VoyageCollection[0];
				}

				return voyageWithLineOperators;
			}
		}
		VesselRoutingVoyage voyageWithLineOperators;

		protected int NumberOfVoyages_TestSameLloydsVoyageDifferentVesselName { get { return 1; } }

		#endregion
	}
}
