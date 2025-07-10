using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	public class SailingIFindBoxTest_BaseFreightTest : BaseFreightTest
	{
		#region TestPopulateFilterDefaultsFromTransport

		public void TestPopulateFilterDefaultsFromTransport()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CommonConsol consol = factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			SailingIFindBoxForTest findBox;

			transport.JW_RL_NKLoadPort = "";
			transport.JW_RL_NKDiscPort = "";
			transport.JW_IsCharter = false;
			findBox = new SailingIFindBoxForTest(transport);
			AssertSailingsCollection(typeof(NonCharterSailingCollection), findBox);
			AssertFilters("", "", findBox);

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			findBox = new SailingIFindBoxForTest(transport);
			AssertSailingsCollection(typeof(NonCharterSailingCollection), findBox);
			AssertFilters("AUSYD", "USLAX", findBox);

			transport.JW_IsCharter = true;
			findBox = new SailingIFindBoxForTest(transport);
			AssertSailingsCollection(typeof(CharterSailingCollection), findBox);
			AssertFilters("AUSYD", "USLAX", findBox);
		}

		#endregion

		#region TestPopulateFilterDefaultsFromCommonShipment

		public void TestPopulateFilterDefaultsFromCommonShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			SailingIFindBoxForTest findBox;

			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";
			findBox = new SailingIFindBoxForTest(shipment);
			AssertSailingsCollection(typeof(NonCharterSailingCollection), findBox);
			AssertFilters("", "", findBox);

			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "GBLON";
			findBox = new SailingIFindBoxForTest(shipment);
			AssertSailingsCollection(typeof(NonCharterSailingCollection), findBox);
			AssertFilters("NZAKL", "GBLON", findBox);
		}

		#endregion

		#region TestSailingSecurityRights

		public void TestSailingSecurityRights()
		{
			CommonShipment lCLShipment = Factory.New<CommonShipment>();
			using (ZForm form = new ZForm())
			{
				SailingIFindBoxForTest sailingFindBox = new SailingIFindBoxForTest(lCLShipment, form);
				lCLShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				TestShowModuleForTransportMode(sailingFindBox, Env.Security.FlightSchedule);
				lCLShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				TestShowModuleForTransportMode(sailingFindBox, Env.Security.SailingSchedule);
				lCLShipment.JS_TransportMode = Core.Constants.TransportModes.Road;
				TestShowModuleForTransportMode(sailingFindBox, Env.Security.TruckSchedule);
				lCLShipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
				TestShowModuleForTransportMode(sailingFindBox, Env.Security.RailSchedule);
			}
		}

		void TestShowModuleForTransportMode(SailingIFindBoxForTest sailingFindBox, SecurityCheckpoint checkPoint)
		{
			checkPoint.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull("Preconditions: no message expected to be popped up so far.",
				UnitTestUserNotification.Instance.LastMessage.Text);
			sailingFindBox.ShowModuleFromShipment();
			AssertNull("No message expected to be popped up.", UnitTestUserNotification.Instance.LastMessage.Text);

			checkPoint.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			sailingFindBox.ShowModuleFromShipment();
			AssertNotNull("Error message expected to be popped up.",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestModuleScheduleCreateFromJob

		public void TestModuleScheduleCreateFromJob()
		{
			var shipment = Factory.New<CommonShipment>();
			using (var form = new ZForm())
			{
				var sailingFindBox = new SailingIFindBoxForTest(shipment, form);
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				TestModuleScheduleCreateFromJob(sailingFindBox, Env.Security.FlightSchedule);
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				TestModuleScheduleCreateFromJob(sailingFindBox, Env.Security.SailingSchedule);
				shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
				TestModuleScheduleCreateFromJob(sailingFindBox, Env.Security.TruckSchedule);
				shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
				TestModuleScheduleCreateFromJob(sailingFindBox, Env.Security.RailSchedule);
			}
		}

		void TestModuleScheduleCreateFromJob(SailingIFindBoxForTest sailingFindBox, SecurityCheckpoint checkPointForSchedule)
		{
			var isAllowedSchedule = checkPointForSchedule.IsAllowed;
			using (new DisposableAction(() => { checkPointForSchedule.IsAllowed = isAllowedSchedule; }))
			{
				checkPointForSchedule.IsAllowed = true;
				var jobSailingSchedule = sailingFindBox.ShowModuleFromShipment() as IJobSailingSchedule;
				CombineAssertions($"{checkPointForSchedule.HumanReadableName} is allowed", () =>
				{
					AssertNotNull("Module should implement IJobSailingSchedule", jobSailingSchedule);
					Assert(jobSailingSchedule.ScheduleCreateFromJob);
				});
			}
		}

		#endregion

		#region Implementation

		void AssertFilters(ZString expectedLoadPort, ZString expectedDischargePort, SailingIFindBoxForTest findBox)
		{
			const string LoadKey = "Load / Discharge:Property1";
			const string DischargeKey = "Load / Discharge:Property2";

			if (expectedLoadPort.IsEmpty)
			{
				AssertEquals("Load Port", false,
					findBox.Sailings.FilterBusinessObjectDefaults.ContainsDefaultFor(LoadKey));
			}
			else
			{
				AssertEquals("Load Port", expectedLoadPort,
					findBox.Sailings.FilterBusinessObjectDefaults[LoadKey].Value);
			}

			if (expectedDischargePort.IsEmpty)
			{
				AssertEquals("Discharge Port", false,
					findBox.Sailings.FilterBusinessObjectDefaults.ContainsDefaultFor(DischargeKey));
			}
			else
			{
				AssertEquals("Discharge Port", expectedDischargePort,
					findBox.Sailings.FilterBusinessObjectDefaults[DischargeKey].Value);
			}
		}

		void AssertSailingsCollection(Type collectionType, SailingIFindBoxForTest findBox)
		{
			AssertEquals(collectionType, findBox.Sailings.GetType());
		}

		#endregion
	}
}
