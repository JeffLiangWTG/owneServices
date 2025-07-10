using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	abstract class VesselRoutingPortPairTestBase : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnlyOfProperties()
		{
			AssertEquals("E9_IsSelected should be editable", false, Voyage.PortPairs[0].E9_IsSelectedInfo.ReadOnly);
			AssertEquals("E9_Publish should be editable", false, Voyage.PortPairs[0].E9_PublishInfo.ReadOnly);
			AssertEquals("Other properties should be read only (E8_RL_NKLoadPort)", true, Voyage.PortPairs[0].E9_RL_NKLoadPortInfo.ReadOnly);
			AssertEquals("Other properties should be read only (E8_RL_NKDischargePort)", true, Voyage.PortPairs[0].E9_RL_NKDischargePortInfo.ReadOnly);
		}

		#region Business Object Overrides

		public void TestClone()
		{
			VesselRoutingPortPair portPair = Voyage.PortPairs[1];

			portPair.E9_IsSelected = true;
			portPair.E9_CargoCutOff = new ZDateTime(2005, 1, 1);
			portPair.E9_ExportReceivalCommences = new ZDateTime(2005, 2, 2);
			portPair.E9_ImportAvailability = new ZDateTime(2005, 3, 3);
			portPair.E9_ImportStorageCommences = new ZDateTime(2005, 4, 4);
			portPair.E9_ATA = new ZDateTime(2005, 5, 5);
			portPair.E9_ATD = new ZDateTime(2005, 6, 6);

			VesselRoutingPortPair clonedPortPair = portPair.Clone();
			AssertEquals("E9_IsSelected", portPair.E9_IsSelected, clonedPortPair.E9_IsSelected);
			AssertEquals("E9_CargoCutOff", portPair.E9_CargoCutOff, clonedPortPair.E9_CargoCutOff);
			AssertEquals("E9_ExportReceivalCommences", portPair.E9_ExportReceivalCommences, clonedPortPair.E9_ExportReceivalCommences);
			AssertEquals("E9_ImportAvailability", portPair.E9_ImportAvailability, clonedPortPair.E9_ImportAvailability);
			AssertEquals("E9_ImportStorageCommences", portPair.E9_ImportStorageCommences, clonedPortPair.E9_ImportStorageCommences);
			AssertEquals("E9_ATA", portPair.E9_ATA, clonedPortPair.E9_ATA);
			AssertEquals("E9_ATD", portPair.E9_ATD, clonedPortPair.E9_ATD);
		}

		#endregion

		#region Related Business Objects

		public void TestVoyage()
		{
			AssertEquals("Correct voyage should be returned", Voyage, Voyage.PortPairs[0].Voyage);
		}

		#endregion

		#region New Properties

		public abstract void TestPortPairType();

		public void TestE9_IsSelected()
		{
			AssertEquals("E9_IsSelected should be false by default", false, Voyage.PortPairs[0].E9_IsSelected);
			Voyage.PortPairs[0].E9_IsSelected = true;
			AssertEquals("E9_IsSelected when set to true", true, Voyage.PortPairs[0].E9_IsSelected);
		}

		public void TestE9_IsSelected_WithEmptyLoadPort()
		{
			Voyage.PortPairs[0].E9_RL_NKLoadPort = "";
			AssertEquals("E9_IsSelected false when empty load port", false, Voyage.PortPairs[0].E9_IsSelected);
			AssertEquals("E9_IsSelected ReadOnly when empty load port", true, Voyage.PortPairs[0].E9_IsSelectedInfo.ReadOnly);
		}

		public void TestE9_IsSelected_WithEmptyDischargePort()
		{
			Voyage.PortPairs[0].E9_RL_NKDischargePort = "";
			AssertEquals("E9_IsSelected false when empty discharge port", false, Voyage.PortPairs[0].E9_IsSelected);
			AssertEquals("E9_IsSelected ReadOnly when empty discharge port", true, Voyage.PortPairs[0].E9_IsSelectedInfo.ReadOnly);
		}

		public void TestE9_IsRegistered()
		{
			foreach (VesselRoutingPortPair portPair in Voyage.PortPairs)
			{
				AssertEquals("Not registered initially", false, portPair.E9_IsRegistered);
				portPair.E9_IsSelected = true;
			}

			Assert("Prerequisite - unable to import sea/rail schedules without valid VesselName", Voyage.Vessel != null);

			VesselRoutingVoyageImporter importer = new VesselRoutingVoyageImporter(new VesselRoutingVoyage[] { Voyage });
			importer.Import(Notifications);
			AssertEquals("Expected no errors; " + Notifications.AsString, false, Notifications.HasErrors);
			Voyage.PortPairs.Load();

			foreach (VesselRoutingPortPair portPair in Voyage.PortPairs)
			{
				AssertEquals("Is registered after import", true, portPair.E9_IsRegistered);
			}
		}

		public void TestE9_Publish()
		{
			Voyage.PortPairs[0].E9_Publish = true;
			Voyage.PortPairs[1].E9_Publish = false;
			AssertEquals(true, Voyage.PortPairs[0].E9_Publish);
			AssertEquals(false, Voyage.PortPairs[1].E9_Publish);
		}

		public void TestE9_Publish_WhenPortPairAlreadyRegistered()
		{
			foreach (VesselRoutingPortPair portPair in Voyage.PortPairs)
			{
				portPair.E9_IsSelected = true;
			}

			VesselRoutingVoyageImporter importer = new VesselRoutingVoyageImporter(new VesselRoutingVoyage[] { Voyage });
			importer.Import(new NotificationBuffer());
			Voyage.PortPairs.Load();
			AssertEquals("Port Pair should be registered for the test", true, Voyage.PortPairs[0].E9_IsRegistered);

			AssertEquals("E9_Publish ReadOnly when port pair registered", true, Voyage.PortPairs[0].E9_PublishInfo.ReadOnly);
			AssertEquals("E9_Publish == JobSailing.JX_IsPublished when port pair registered", true, Voyage.PortPairs[0].E9_Publish);
			Voyage.PortPairs[0].JobSailing.JX_IsPublished = false;
			AssertEquals("E9_Publish == JobSailing.JX_IsPublished when port pair registered", false, Voyage.PortPairs[0].E9_Publish);
		}

		#endregion

		#region Implementation

		protected JobVesselSchedule NewJobVesselSchedule(ZString portName, ZDateTime eTA, ZDateTime eTD, ZString voyageIn, ZString voyageOut)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portName;
			result.EV_ETA = eTA;
			result.EV_ETD = eTD;
			result.EV_IMOLloydsNumber = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, string.Empty)).RV_LloydsNumber;
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			result.EV_DataProvider = DataProvider;
			return result;
		}

		protected abstract VesselRoutingVoyage Voyage { get; }
		protected VesselRoutingVoyage voyage;

		protected abstract ZString DataProvider { get; }

		NotificationBuffer Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = new NotificationBuffer();
				}
				return notifications;
			}
		}
		NotificationBuffer notifications;

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot save or delete rows in a view", true);
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert("This business object is always loaded and never created", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Voyage.PortPairs[1];
		}

		#endregion
	}
}
