using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[TestedType(typeof(MovementExporterServiceTask))]
	internal class MovementExporterServiceTaskTest : ServiceTaskTestCase<MovementExporterServiceTask>
	{
		public void TestRunTask()
		{
			Assert("PRECONDITION: should not be any messages in database", Factory.GetDatabaseCount(typeof(EDIMessage)).Equals(0));
			Assert("PRECONDITION: should not be any interchanges in database", Factory.GetDatabaseCount(typeof(EDIInterchange)).Equals(0));
			AssertEquals("PRECONDITION: should not be any movements in database", 0, Factory.GetDatabaseCount(typeof(ContainerMovement)));
			Setup("RecipientID", ZDateTime.Empty);
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var billOfLading = NewBill(voyage, "V01", "AUBNE", "NLAMS");
			var containerAndMapEventDic = new Dictionary<string, string>();
			Action<string, string, string> addNewMovement = (containerNum, movementType, expectedMapEvent) =>
			{
				var stock = NewStock(containerNum, "20GP");
				AddContainer(billOfLading, stock);
				AddMovement(stock, movementType, ZDateTime.Now.AddHours(-1), voyage, false, false);
				containerAndMapEventDic.Add(containerNum, expectedMapEvent);
			};
			addNewMovement("TEST2016121400", ContainerMovementTypes.Codes.ReturnToWharf, AutoEvents.GateInCode);
			addNewMovement("TEST2016121401", ContainerMovementTypes.Codes.YardGateIn, AutoEvents.GateInCode);
			addNewMovement("TEST2016121402", ContainerMovementTypes.Codes.WharfGateIn, AutoEvents.GateInCode);
			addNewMovement("TEST2016121403", ContainerMovementTypes.Codes.ReturnedUnshipped, AutoEvents.GateInCode);
			addNewMovement("TEST2016121404", ContainerMovementTypes.Codes.RePositionIntoYard, AutoEvents.GateInCode);
			addNewMovement("TEST2016121405", ContainerMovementTypes.Codes.OnHire, AutoEvents.GateInCode);
			addNewMovement("TEST2016121406", ContainerMovementTypes.Codes.DepotGateIn, AutoEvents.GateInCode);
			addNewMovement("TEST2016121407", ContainerMovementTypes.Codes.YardGateOut, AutoEvents.GateOutCode);
			addNewMovement("TEST2016121408", ContainerMovementTypes.Codes.WharfGateOut, AutoEvents.GateOutCode);
			addNewMovement("TEST2016121409", ContainerMovementTypes.Codes.ReShipRequested, AutoEvents.GateOutCode);
			addNewMovement("TEST2016121410", ContainerMovementTypes.Codes.RePositionOutOfYard, AutoEvents.GateOutCode);
			addNewMovement("TEST2016121411", ContainerMovementTypes.Codes.OffHire, AutoEvents.GateOutCode);
			addNewMovement("TEST2016121412", ContainerMovementTypes.Codes.DepotGateOut, AutoEvents.GateOutCode);
			addNewMovement("TEST2016121413", ContainerMovementTypes.Codes.Discharge, AutoEvents.FreightUnloadedCode);
			addNewMovement("TEST2016121414", ContainerMovementTypes.Codes.Load, AutoEvents.FreightLoadedCode);
			Factory.Save();
			var task = new MovementExporterServiceTask();

			InitialiseTaskSchedule(task, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(GlbBranch.CurrentBranch.PK.ToGuid());
			RunTaskSchedule(task);

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals(15, messages.Length);
			AssertEquals(15, interchanges.Length);
			CombineAssertions(() =>
			{
				var regex = new Regex(@"<Key>(.+?)</Key>");
				foreach (var ediMessage in messages)
				{
					var messageContent = ediMessage.EM_FormattedMessageText;
					var containerNum = regex.Match(messageContent).Groups[1].Value;
					var expectedEventCode = containerAndMapEventDic[containerNum];
					var pattern = string.Format("<EventType>{0}</EventType>", expectedEventCode);
					var message = string.Format("MovementExporterServiceTask should map event code {0} on container {1}.", expectedEventCode, containerNum);
					Assert(message, Regex.IsMatch(messageContent, pattern));
					AssertEquals("EDIEDIDAT", ediMessage.Interchange.EI_From);
					AssertEquals("RecipientID", ediMessage.Interchange.EI_To);
				}
			});
		}

		[TestDate(2011, 02, 05, 10, 15, 00)]
		public void TestRunTaskNo_Movements()
		{
			Setup("RecipientID", ZDateTime.Empty);
			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, messages.Length);
			var task = new MovementExporterServiceTask();
			InitialiseAndRunTaskSchedule(task);
			messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, messages.Length);
		}

		[TestDate(2011, 02, 05, 10, 15, 00)]
		public void TestHighWaterMark()
		{
			AssertEquals(DateTime.MinValue, AgencyRegistry.Instance.CMMeHubHighWaterMark.Value);
			var task = new MovementExporterServiceTask();
			InitialiseAndRunTaskSchedule(task);
			AssertEquals(new DateTime(2011, 02, 05, 10, 15, 00), AgencyRegistry.Instance.CMMeHubHighWaterMark.Value);
		}

		[TestDate(2011, 01, 21, 10, 15, 00)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRunTaskCutOff()
		{
			RefContainerStock stock1 = NewStock("TEST4100013", "20GP");
			RefContainerStock stock2 = NewStock("TEST4100029", "20RE");
			var cutOff = ZDateTime.Now.AddHours(1);
			ContainerMovement movement1 = AddMovement(stock1, ContainerMovementTypes.Codes.DepotGateIn, ZDateTime.Now.AddDays(-1), null, false, false);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			ContainerMovement movement2 = AddMovement(stock2, ContainerMovementTypes.Codes.DepotGateIn, ZDateTime.Now.AddDays(-1), null, false, false);
			Factory.Save();
			Setup("RecipientID", cutOff);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(15);
			MovementExporterServiceTask task = new MovementExporterServiceTask();

			InitialiseTaskSchedule(task, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(GlbBranch.CurrentBranch.PK.ToGuid());
			RunTaskSchedule(task);

			CombineAssertions(delegate
			{
				AssertEquals("movement1.Messages.Count", 0, movement1.Messages.Count);
				AssertEquals("movement2.Messages.Count", 1, movement2.Messages.Count);
			});
			var message = movement2.Messages[0];
			AssertEquals("EDIEDIDAT", message.Interchange.EI_From);
			AssertEquals("RecipientID", message.Interchange.EI_To);
		}

		[ExpectNoExceptions]
		public void TestRunTaskWithInvalidMovementType()
		{
			Assert("PRECONDITION: should not be any messages in database", Factory.GetDatabaseCount(typeof(EDIMessage)).Equals(0));
			Assert("PRECONDITION: should not be any interchanges in database", Factory.GetDatabaseCount(typeof(EDIInterchange)).Equals(0));
			AssertEquals("PRECONDITION: should not be any movements in database", 0, Factory.GetDatabaseCount(typeof(ContainerMovement)));
			Setup("RecipientID", ZDateTime.Empty);
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var badMovementType = "XXX";
			var billOfLading = NewBill(voyage, "V01", "AUBNE", "NLAMS");
			var stock = NewStock("TEST2016121400", "20GP");
			AddContainer(billOfLading, stock);
			AddMovement(stock, badMovementType, ZDateTime.Now.AddHours(-1), voyage, false, false);
			Factory.Save();
			var task = new MovementExporterServiceTask();
			InitialiseAndRunTaskSchedule(task);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		void Setup(string recipientID, ZDateTime cutoff)
		{
			AgencyRegistry.Instance.CMMeHubCutOff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cutoff.IsEmpty ? DateTime.MinValue : cutoff.ToDateTime());
			OrgHeader proxy = GlbCompany.CurrentCompany.OrgProxy;
			proxy.EDICommunicationsModes.RemoveAll();
			EDICommunicationsMode mode = proxy.EDICommunicationsModes.AddNew();
			mode.EK_Module = EDICommunicationsMode.Modules.ContainerMovements;
			mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			mode.EK_Destination = recipientID;
		}

		RefContainerStock NewStock(string containerNum, string containerType)
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = containerNum;
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			return stock;
		}

		ContainerMovement AddMovement(RefContainerStock stock, ZString movementType, ZDateTime movementDate, JobVoyage voyage, bool withMessage, bool withInterchange)
		{
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementType = movementType;
			movement.E9_MovementDate = movementDate;
			movement.E9_JV = voyage == null ? ZGuid.Empty : voyage.PK;
			return movement;
		}

		BillOfLading NewBill(JobVoyage voyage, string consignRef, string origin, string destination)
		{
			BillOfLading bill = Factory.New<BillOfLading>();
			if (voyage != null)
			{
				bill.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge(origin, destination).PK;
			}

			bill.JS_UniqueConsignRef = consignRef;
			bill.JS_CFSReference = consignRef;
			bill.JS_BookingReference = consignRef;
			bill.JS_RL_NKOrigin = origin;
			bill.JS_RL_NKDestination = destination;
			return bill;
		}

		BillOfLadingContainer AddContainer(BillOfLading bill, RefContainerStock stock)
		{
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;
			container.JC_RC = stock.R6_RC;
			return container;
		}
		#endregion
	}
}
