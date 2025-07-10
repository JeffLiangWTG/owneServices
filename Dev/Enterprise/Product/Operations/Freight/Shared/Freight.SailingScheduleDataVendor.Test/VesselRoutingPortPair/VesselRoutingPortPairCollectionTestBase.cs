using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	abstract class VesselRoutingPortPairCollectionTestBase : BusinessObjectCollectionTestCase
	{
		#region AllowNew / AllowSort

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestAllowSort()
		{
			AssertEquals("AllowSort", false, Collection.AllowSort);
		}

		#endregion

		#region Test Classes

		internal class TestVesselRoutingPortPairCollection : VesselRoutingPortPairCollection
		{
			public TestVesselRoutingPortPairCollection(VesselRoutingVoyage voyage)
				: base(voyage)
			{
			}

			public new bool AllowSort
			{
				get { return base.AllowSort; }
			}
		}

		#endregion

		#region Implementation

		protected void AssertPortPairCount(VesselRoutingVoyage voyage, int expectedCount)
		{
			AssertEquals("Count; all port pairs:\r\n\r\n" + GetPortPairsAsString(voyage), expectedCount, voyage.PortPairs.Count);
		}

		protected void AssertPortPairValues(ZString message, VesselRoutingPortPair portPair, ZString loadPort, ZString dischargePort, ZDateTime eTD, ZDateTime eTA, PortPairTypes portPairType)
		{
			ZString completeMessage = message + "; all port pairs:\r\n\r\n" + GetPortPairsAsString(portPair.Voyage);
			AssertEquals("E9_RL_NKLoadPort;      " + completeMessage, loadPort, portPair.E9_RL_NKLoadPort);
			AssertEquals("E9_RL_NKDischargePort; " + completeMessage, dischargePort, portPair.E9_RL_NKDischargePort);
			AssertEquals("E9_ETD;                " + completeMessage, eTD, portPair.E9_ETD);
			AssertEquals("E9_ETA;                " + completeMessage, eTA, portPair.E9_ETA);

			if (portPairType == PortPairTypes.Export)
			{
				AssertEquals("Availability date empty for an export leg", ZDateTime.Empty, portPair.E9_ImportAvailability);
				AssertEquals("Storage date empty for an export leg", ZDateTime.Empty, portPair.E9_ImportStorageCommences);
			}
			if (portPairType == PortPairTypes.Import)
			{
				AssertEquals("Receival Start date empty for an import leg", ZDateTime.Empty, portPair.E9_ExportReceivalCommences);
				AssertEquals("Cut Off date empty for an import leg", ZDateTime.Empty, portPair.E9_CargoCutOff);
			}
		}

		protected ZString GetPortPairsAsString(VesselRoutingVoyage voyage)
		{
			ZString result = "Load   Discharge\r\n";
			foreach (VesselRoutingPortPair portPair in voyage.PortPairs)
			{
				result += portPair.E9_RL_NKLoadPort.PadRight(5) + "  " + portPair.E9_RL_NKDischargePort + "\r\n";
			}

			return result;
		}

		protected JobVesselRouting NewJobVesselRouting(ZString portCode, ZString voyage, ZGuid schedulePK, ZDateTime eTA, ZDateTime eTD)
		{
			JobVesselRouting result = Factory.New<JobVesselRouting>();
			result.E1_RL_NKDischargePortCode = portCode;
			result.E1_LloydsID = "Lloyds";
			result.E1_VoyageNumber = voyage;
			result.E1_ETA = eTA;
			result.E1_ETD = eTD;
			result.E1_EV = schedulePK;

			if (DataProvider != FreightConstants.VesselDataProviders.OneStop)
			{
				//1-Stop data would get confused with identical routings
				CloneJobVesselRoutingWithEmptyFK(result);
			}

			return result;
		}

		protected JobVesselRouting NewJobVesselRouting(ZString portCode, ZString voyage)
		{
			return NewJobVesselRouting(portCode, voyage, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty);
		}

		protected JobVesselSchedule NewJobVesselSchedule(ZString portName, ZDateTime eTA, ZDateTime eTD,
			ZString lloydsNumber, ZString voyageIn, ZString voyageOut,
			ZString vesselName = default, ZString lineOperator = default,
			ZString operatorsDescription = default)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portName;
			result.EV_ETA = eTA;
			result.EV_ETD = eTD;
			result.EV_IMOLloydsNumber = lloydsNumber;
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			result.EV_DataProvider = DataProvider;
			result.EV_ShipName = vesselName;
			result.EV_LineOperator = lineOperator;
			result.EV_OperatorsDescription = operatorsDescription;

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

		protected JobVesselRouting CloneJobVesselRoutingWithEmptyFK(JobVesselRouting routing)
		{
			JobVesselRouting result = Factory.New<JobVesselRouting>();
			result.CopyPersistentValuesFrom(routing);
			result.E1_EV = ZGuid.Empty;
			return result;
		}

		ZString DataProviderFromOtherCountry(ZString provider)
		{
			switch (provider)
			{
				case FreightConstants.VesselDataProviders.DBH:
				case FreightConstants.VesselDataProviders.DAKOSY:
					return FreightConstants.VesselDataProviders.OneStop;

				case FreightConstants.VesselDataProviders.OneStop:
					return FreightConstants.VesselDataProviders.DBH;
			}
			return "XXX";
		}

		protected abstract ZString DataProvider { get; }

		protected VesselRoutingVoyage LoadVoyageByVoyageNumber(string voyage, string dataProvider)
		{
			ZQuery query = new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, voyage);
			query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_DataProvider, dataProvider);
			return LoadVoyage(query);
		}

		protected VesselRoutingVoyage LoadVoyageByLloyds(string lloyds, string dataProvider)
		{
			ZQuery query = new ZQuery(ViewVesselRoutingVoyagesSchema.E8_LloydsNumber, lloyds);
			query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_DataProvider, dataProvider);
			return LoadVoyage(query);
		}

		protected VesselRoutingVoyage LoadVoyage(ZQuery query)
		{
			VoyageCollection.Load(query);
			return VoyageCollection[0];
		}

		protected VesselRoutingVoyageCollection VoyageCollection
		{
			get
			{
				if (voyageCollection == null)
				{
					voyageCollection = new VesselRoutingVoyageCollection(Factory);
				}

				return voyageCollection;
			}
		}
		VesselRoutingVoyageCollection voyageCollection;

		new TestVesselRoutingPortPairCollection Collection
		{
			get { return (TestVesselRoutingPortPairCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			VesselRoutingVoyage voyage = Factory.New<VesselRoutingVoyage>();
			return new TestVesselRoutingPortPairCollection(voyage);
		}

		#endregion
	}
}
