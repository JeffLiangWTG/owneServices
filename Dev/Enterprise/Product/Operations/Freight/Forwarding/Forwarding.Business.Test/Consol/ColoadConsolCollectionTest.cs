using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ColoadConsolCollection))]
	sealed class ColoadConsolCollectionTest : ActiveBusinessObjectCollectionTestCase<ColoadConsolCollection>
	{
		public void TestCtor()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();
			var consol4 = Factory.New<ForwardingConsol>();
			var consol5 = Factory.New<ForwardingConsol>();

			var collection = new ColoadConsolCollection(consol1);
			AssertEquals(0, collection.Count);

			consol2.JK_JK_MasterConsol = consol1.PK;
			consol3.JK_JK_MasterConsol = consol1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { consol2, consol3 }, collection);

			consol5.JK_JK_MasterConsol = consol4.PK;
			AssertContainsExactElementsInAnyOrder(new[] { consol2, consol3 }, collection);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet_IsAWBColoadOrDirect()
		{
			var masterConsol = Factory.New<ForwardingConsol>();

			var coloadConsol = Factory.New<ForwardingConsol>();
			coloadConsol.JK_AgentType = Constants.AgentType.AWBCoload;

			var agentConsol = Factory.New<ForwardingConsol>();
			agentConsol.JK_AgentType = Constants.AgentType.Agent;

			var directConsol = Factory.New<ForwardingConsol>();
			directConsol.JK_AgentType = Constants.AgentType.Direct;

			var collection = new ColoadConsolCollection(masterConsol);
			var errorMessage = "Only AWB Coload consols and Direct consols can be attached to Multi AWB Master.";

			var notifications = ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(agentConsol);
			AssertEquals(errorMessage, notifications);

			notifications = ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(directConsol);
			AssertNotContains(errorMessage, notifications);

			notifications = ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(coloadConsol);
			AssertNotContains(errorMessage, notifications);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet_ColoadIsAlreadyAttachedToMaster()
		{
			var masterConsol = Factory.New<ForwardingConsol>();
			masterConsol.JK_UniqueConsignRef = "MASTA";

			var coloadConsol = Factory.New<ForwardingConsol>();
			coloadConsol.JK_AgentType = Constants.AgentType.AWBCoload;
			coloadConsol.JK_JK_MasterConsol = masterConsol.PK;

			var anotherMasterConsol = Factory.New<ForwardingConsol>();

			var collection = new ColoadConsolCollection(anotherMasterConsol);
			var errorMessage = "This consol is already attached to Multi AWB Master MASTA.";

			var notifications = ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(coloadConsol);
			AssertEquals(errorMessage, notifications);

			coloadConsol.JK_JK_MasterConsol = ZGuid.Empty;

			notifications = ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(coloadConsol);
			AssertNotContains(errorMessage, notifications);
		}

		public void TestGetColoadConsolsList_AdditionalFilter()
		{
			var agentConsol = Factory.New<ForwardingConsol>();
			agentConsol.JK_AgentType = Constants.AgentType.Agent;

			var coloadConsol1 = Factory.New<ForwardingConsol>();
			coloadConsol1.JK_AgentType = Constants.AgentType.AWBCoload;

			var coloadConsol2 = Factory.New<ForwardingConsol>();
			coloadConsol2.JK_AgentType = Constants.AgentType.AWBCoload;

			var directConsol1 = Factory.New<ForwardingConsol>();
			directConsol1.JK_AgentType = Constants.AgentType.Direct;

			var directConsol2 = Factory.New<ForwardingConsol>();
			directConsol2.JK_AgentType = Constants.AgentType.Direct;

			var collection = ColoadConsolCollection.GetColoadConsolsList(Factory, ZString.Empty, ZDateTime.Now.AddYears(1));
			AssertContainsExactElementsInAnyOrder("Only AWB coloads without master", new[] { coloadConsol1, coloadConsol2, directConsol1, directConsol2 }, collection);

			coloadConsol1.JK_JK_MasterConsol = Factory.New<ForwardingConsol>().PK;
			directConsol1.JK_JK_MasterConsol = Factory.New<ForwardingConsol>().PK;
			collection = ColoadConsolCollection.GetColoadConsolsList(Factory, ZString.Empty, ZDateTime.Now.AddYears(1));
			AssertContainsExactElementsInAnyOrder(new[] { coloadConsol2, directConsol2 }, collection);
		}

		public void TestGetExtraNotification_NoFreightNO()
		{
			var masterConsol = Factory.New<ForwardingConsol>();
			masterConsol.JK_AgentType = Constants.AgentType.AWBMaster;

			var coloadConsol = Factory.New<ForwardingConsol>();
			coloadConsol.JK_AgentType = Constants.AgentType.AWBCoload;

			var collection = masterConsol.ColoadConsols_List;

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			AssertNotNull(notificationProvider);
			AssertNull(notificationProvider.GetExtraNotification(coloadConsol));
		}

		public void TestGetExtraNotification_DifferentFreightNO()
		{
			var awbMaster = Factory.New<ForwardingConsol>();
			awbMaster.JK_AgentType = Constants.AgentType.AWBMaster;

			var masterTransport = awbMaster.MostInterestingTransportForBinding.FirstOrDefault() as Transport;
			masterTransport.JW_VoyageFlight = "FR384";

			var awbCoload = Factory.New<ForwardingConsol>();
			awbCoload.JK_AgentType = Constants.AgentType.AWBCoload;

			var coloadTransport = awbCoload.MostInterestingTransportForBinding.FirstOrDefault() as Transport;
			coloadTransport.JW_VoyageFlight = "FR355";

			var collection = awbMaster.ColoadConsols_List;

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			AssertEquals("Only AWB on the same flight can be consolidated together.", notificationProvider.GetExtraNotification(awbCoload).Message);
		}

		[TestDate(2020, 06, 22, 0, 0, 0)]
		public void TestGetExtraNotification_CutOffDatePassed_ConsolAttachDetachShipmentAfterCutOffDate_NotAllowed()
		{
			var awbMaster = Factory.New<ForwardingConsol>();
			awbMaster.JK_AgentType = Constants.AgentType.AWBMaster;
			awbMaster.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddDays(-1);

			AssertLessThan(awbMaster.JK_ConsolCutOffDate, ZDateTime.Now);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;
			var awbCoload = Factory.New<ForwardingConsol>();
			awbCoload.JK_AgentType = Constants.AgentType.AWBCoload;

			var collection = awbMaster.ColoadConsols_List;

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			AssertEquals("Cannot attach this consol to the AWB Master consol as the Cut Off Date (21 Jun 2020 00:00) has passed.", notificationProvider.GetExtraNotification(awbCoload).Message);
		}

		[TestDate(2020, 06, 22, 0, 0, 0)]
		public void TestGetExtraNotification_CutOffDatePassed_ConsolAttachDetachShipmentAfterCutOffDate_Allowed()
		{
			var awbMaster = Factory.New<ForwardingConsol>();
			awbMaster.JK_AgentType = Constants.AgentType.AWBMaster;
			awbMaster.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddDays(-1);

			AssertLessThan(awbMaster.JK_ConsolCutOffDate, ZDateTime.Now);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
			var awbCoload = Factory.New<ForwardingConsol>();
			awbCoload.JK_AgentType = Constants.AgentType.AWBCoload;

			var collection = awbMaster.ColoadConsols_List;

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			AssertNull(notificationProvider.GetExtraNotification(awbCoload));
		}

		public void TestGetExtraNotification_SameFreightNO()
		{
			var awbMaster = Factory.New<ForwardingConsol>();
			awbMaster.JK_AgentType = Constants.AgentType.AWBMaster;

			var masterTransport = awbMaster.MostInterestingTransportForBinding.FirstOrDefault() as Transport;
			masterTransport.JW_VoyageFlight = "FR384";

			var awbCoload = Factory.New<ForwardingConsol>();
			awbCoload.JK_AgentType = Constants.AgentType.AWBCoload;

			var coloadTransport = awbCoload.MostInterestingTransportForBinding.FirstOrDefault() as Transport;
			coloadTransport.JW_VoyageFlight = "FR384";

			var collection = awbMaster.ColoadConsols_List;

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			AssertNull(notificationProvider.GetExtraNotification(awbCoload));
		}

		public void TestGetExtraNotification_MultipleTransports()
		{
			var awbMaster = Factory.New<ForwardingConsol>();
			awbMaster.JK_AgentType = Constants.AgentType.AWBMaster;

			var masterTransport = awbMaster.MostInterestingTransportForBinding.FirstOrDefault() as Transport;
			masterTransport.JW_VoyageFlight = "FR384";

			var awbCoload = Factory.New<ForwardingConsol>();
			awbCoload.JK_TransportMode = Constants.TransportModes.Air;
			awbCoload.JK_AgentType = Constants.AgentType.AWBCoload;

			var coloadTransport1 = awbCoload.Transports.FirstOrDefault() as Transport;
			coloadTransport1.JW_VoyageFlight = "FR384";
			coloadTransport1.JW_RL_NKLoadPort = "AUBNE";
			coloadTransport1.JW_RL_NKDiscPort = "CNCAN";

			var coloadTransport2 = awbCoload.Transports.AddNew();
			coloadTransport2.JW_VoyageFlight = "FR376";

			var collection = awbMaster.ColoadConsols_List;

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			AssertNull(notificationProvider.GetExtraNotification(awbCoload));
		}

		#region Implementation

		protected override ColoadConsolCollection GetCollectionToTest()
		{
			var parentConsol = Factory.New<ForwardingConsol>();
			return new ColoadConsolCollection(parentConsol);
		}

		#endregion
	}
}
