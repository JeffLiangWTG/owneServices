using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderProcessTasksUpdateDatesFromAttachedShipmentOrDeclarationTest : BaseOrderProcessTasksUpdateDatesTest
	{
		public void TestUpdateDepartureActualDate()
		{
			OrderProcessTasks milestone = GetMilestone(Events.Departure);
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = shipment.PK;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should not default to date on the shipment as there is no transport ATD", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			transport.JW_ATD = TestShipmentDate;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment (as there is transport ATD)", TestShipmentDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			Order.JD_JE = declaration.PK;

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the declaration date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			var newTransport = ((IRoutingSupport)declaration).Transports.AddNew();
			newTransport.JW_RL_NKLoadPort = "AUSYD";
			newTransport.JW_RL_NKDiscPort = "NZAKL";
			newTransport.JW_ATD = TestDeclarationDate;

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestUpdateDepartureScheduledDate()
		{
			OrderProcessTasks task = GetMilestone(Events.Departure);
			task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(TestOrderDate));
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			Factory.Save();
			shipment.Consols.Remove(consol);
			Order.JD_JS = shipment.PK;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should not default to date on the shipment as there is no transport ATD", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			shipment.JS_E_DEP = TestShipmentDate;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment as there are no transports on shipment", TestShipmentDate, task.P9_ScheduledDate.ToZDateTime());

			shipment.Consols.Add(consol);
			consol.Transports[0].JW_ETD = ZDateTime.Empty;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date on transport should override date from shipment", TestShipmentDate, task.P9_ScheduledDate.ToZDateTime());

			shipment.Consols.Remove(consol);
			shipment.Consols.Add(CreateConsolWithTransports());
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the last transport's ETA on the shipment", shipment.TransportsIncludingRelated[0].JW_ETD, task.P9_ScheduledDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(TestOrderDate));
			Order.JD_JE = declaration.PK;

			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the declaration date is empty", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			declaration[JobDeclarationSchema.JE_DateAtOrigin.Name] = TestDeclarationDate;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, task.P9_ScheduledDate.ToZDateTime());
		}

		public void TestUpdateArrivalActualDate()
		{
			OrderProcessTasks milestone = GetMilestone(Events.Arrival);
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = shipment.PK;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should not default to date on the shipment as there is no transport ATA", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			transport.JW_ATA = TestShipmentDate;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment despite transport ATA", TestShipmentDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			Order.JD_JE = declaration.PK;

			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the declaration date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			var newTransport = ((IRoutingSupport)declaration).Transports.AddNew();
			newTransport.JW_RL_NKLoadPort = "NZAKL";
			newTransport.JW_RL_NKDiscPort = "AUSYD";
			newTransport.JW_ATA = TestDeclarationDate;

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestUpdateArrivalScheduledDate()
		{
			OrderProcessTasks task = GetMilestone(Events.Arrival);
			task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(TestOrderDate));
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			Factory.Save();
			shipment.Consols.Remove(consol);
			Order.JD_JS = shipment.PK;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as there are no transports on shipment", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			shipment.JS_E_ARV = TestShipmentDate;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment as there are no transports", TestShipmentDate, task.P9_ScheduledDate.ToZDateTime());

			shipment.Consols.Add(consol);
			consol.Transports[0].JW_ETA = ZDateTime.Empty;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment as transport sate is empty", TestShipmentDate, task.P9_ScheduledDate.ToZDateTime());

			shipment.Consols.Remove(consol);
			shipment.Consols.Add(CreateConsolWithTransports());
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the first transport's ETD on the shipment", shipment.TransportsIncludingRelated[2].JW_ETA, task.P9_ScheduledDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(TestOrderDate));
			Order.JD_JE = declaration.PK;

			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the declaration date is empty", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			declaration[JobDeclarationSchema.JE_DateAtFinalDestination.Name] = TestDeclarationDate;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, task.P9_ScheduledDate.ToZDateTime());
		}

		public void TestUpdateCustomsCommencedActualDate()
		{
			PerformUpdateCustomsDatesTest(Events.CustomsCommenced, Events.ExportCustomsCommenced);
		}

		public void TestUpdateCustomsClearedActualDate()
		{
			PerformUpdateCustomsDatesTest(Events.CustomsCleared, Events.ExportCustomsCleared);
		}

		void PerformUpdateCustomsDatesTest(Event customsEvent, Event exportCustomsEvent)
		{
			OrderProcessTasks milestone = GetMilestone(customsEvent);
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = shipment.PK;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the shipment date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			shipment.Logs.AddNew(customsEvent, TestShipmentDate.ToOffset());
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment", TestShipmentDate, milestone.P9_ActualDate.ToZDateTime());

			shipment.Logs.RemoveAndDeleteAll();

			shipment.Logs.AddNew(exportCustomsEvent, TestShipmentDate.ToOffset().AddDays(1));
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the updated date on the shipment", TestShipmentDate.AddDays(1), milestone.P9_ActualDate.ToZDateTime());

			OrderProcessTasks exportMilestone = GetMilestone(exportCustomsEvent);
			exportMilestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.SetMilestoneActualDateForTest(TestOrderDate);

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			exportMilestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);

			AssertEquals("Task date should default to user defined date as the shipment date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());
			AssertEquals("Task date should default to date on the shipment", TestShipmentDate.AddDays(1), exportMilestone.P9_ActualDate.ToZDateTime());

			shipment.Logs.AddNew(customsEvent, TestShipmentDate.ToOffset());

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			exportMilestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);

			AssertEquals("Task date should default to date on the shipment", TestShipmentDate, milestone.P9_ActualDate.ToZDateTime());
			AssertEquals("Task date should default to date on the shipment", TestShipmentDate.AddDays(1), exportMilestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			exportMilestone.SetMilestoneActualDateForTest(TestOrderDate);
			Order.JD_JE = declaration.PK;

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the declaration date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			declaration.Logs.AddNew(customsEvent, TestDeclarationDate.ToOffset());
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, milestone.P9_ActualDate.ToZDateTime());

			declaration.Logs.RemoveAndDeleteAll();
			declaration.Logs.AddNew(exportCustomsEvent, TestDeclarationDate.ToOffset().AddDays(1));
			milestone.SetMilestoneActualDateForTest(TestOrderDate);

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			exportMilestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);

			AssertEquals("Task date should default to user defined date as the declaration log date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate.AddDays(1), exportMilestone.P9_ActualDate.ToZDateTime());

			declaration.Logs.AddNew(customsEvent, TestDeclarationDate.ToOffset());

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			exportMilestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);

			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, milestone.P9_ActualDate.ToZDateTime());
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate.AddDays(1), exportMilestone.P9_ActualDate.ToZDateTime());
		}

		public void TestUpdateCargoAvailableActualDate()
		{
			OrderProcessTasks milestone = GetMilestone(Events.CargoAvailable);
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = shipment.PK;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the shipment date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			shipment.DocsAndCartage.JP_FCLAvailable = TestShipmentDate;
			shipment.DocsAndCartage.JP_LCLAvailable = TestShipmentDate.AddDays(1);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment with CTO Available date", TestShipmentDate, milestone.P9_ActualDate.ToZDateTime());

			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment with CTO Available date", TestShipmentDate, milestone.P9_ActualDate.ToZDateTime());

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment with CFS Available date", TestShipmentDate.AddDays(1), milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			Order.JD_JE = declaration.PK;

			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestUpdateDeliveryCartageAdvisedActualDate()
		{
			OrderProcessTasks milestone = GetMilestone(Events.DeliveryCartageAdvised);
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = shipment.PK;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the shipment date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = TestShipmentDate;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment", TestShipmentDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			Order.JD_JE = declaration.PK;

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			declaration[JobDeclarationSchema.JE_MessageType] = "IMP";
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			(((IShipmentWithDocsAndCartage)declaration).DocsAndCartage).JP_DeliveryCartageAdvised = TestDeclarationDate;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestUpdateDeliveryCartageCompleteFinalizedScheduledDate()
		{
			OrderProcessTasks task = GetMilestone(Events.DeliveryCartageCompleteFinalised);
			task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(TestOrderDate));
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			Order.JD_JS = shipment.PK;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the shipment date is empty", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			shipment.DocsAndCartage.JP_EstimatedDelivery = TestShipmentDate;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment", TestShipmentDate, task.P9_ScheduledDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(TestOrderDate));
			Order.JD_JE = declaration.PK;

			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			declaration[JobDeclarationSchema.JE_MessageType] = "IMP";
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, task.P9_ScheduledDate.ToZDateTime());

			(((IShipmentWithDocsAndCartage)declaration).DocsAndCartage).JP_EstimatedDelivery = TestDeclarationDate;
			task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, task.P9_ScheduledDate.ToZDateTime());
		}

		public void TestUpdateDeliveryCartageCompleteFinalizedActualDate()
		{
			OrderProcessTasks milestone = GetMilestone(Events.DeliveryCartageCompleteFinalised);
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = shipment.PK;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to user defined date as the shipment date is empty", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = TestShipmentDate;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to date on the shipment", TestShipmentDate, milestone.P9_ActualDate.ToZDateTime());

			Order.JD_JS = ZGuid.Empty;
			milestone.SetMilestoneActualDateForTest(TestOrderDate);
			Order.JD_JE = declaration.PK;

			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			declaration[JobDeclarationSchema.JE_MessageType] = "IMP";
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should be the user defined date", TestOrderDate, milestone.P9_ActualDate.ToZDateTime());

			(((IShipmentWithDocsAndCartage)declaration).DocsAndCartage).JP_DeliveryCartageCompleted = TestDeclarationDate;
			milestone.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
			AssertEquals("Task date should default to the declaration date", TestDeclarationDate, milestone.P9_ActualDate.ToZDateTime());
		}
	}
}
