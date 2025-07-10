using System;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(EIDOSendOriginalApplicator))]
	internal sealed class EIDOSendOriginalApplicatorTest : EIDOBaseApplicatorTest
	{
		[EIDOMessagingConfiguration]
		public void TestRunSuccess()
		{
			ZQuery eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);

			const string expectedLog =
				"";

			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			BillOfLadingContainer target = NewContainerTarget("SGSIN", "AUBNE");
			target.Booking.RunPreSaveValidation();
			AssertNoErrors("precondition:", target);
			AssertNoMessageErrors("precondition:", target);
			AssertEquals("precondition:", 0, target.Messages.Count);
			AssertEquals("precondition:", 0, target.Logs.Find(eventFilter).Length);

			ApplyApplicator(new BusinessObject[] { target.Booking }, expectedLog);
			AssertEquals("Should have added a message.", 1, target.Messages.Count);
			AssertEquals("The new message should be queued", EDIMessage.Status.Queued, target.Messages[0].EM_Status);
			AssertEquals("The new message should have the correct application code", EDIMessage.ApplicationCodes.EIDO, target.Messages[0].EM_ApplicationCode);

			Factory.Save();

			StmALog[] events = target.Logs.Find(eventFilter);
			AssertEquals("should have added 1 MessageSent event", 1, events.Length);
			AssertEquals("added event should have a parameter that its name is DEP and value is 1-stop", EIDOBaseApplicator.EventParameterValues.OneStop, events[0].Parameters[Constants.EventReferenceParameters.Codes.Department]);
			AssertEquals("added event should have a parameter that its name is MST and value is E-IDO", EIDOBaseApplicator.EventParameterValues.EIDO, events[0].Parameters[Constants.EventReferenceParameters.Codes.MessageType]);
			AssertEquals("added event should have a empty reference free text", ZString.Empty, events[0].ReferenceFreeText);
		}

		[EIDOMessagingConfiguration]
		public void TestRunSuccess_ByContainer()
		{
			ZQuery eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);

			const string expectedLog =
				"";

			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			BillOfLading shipment = NewShipmentTarget("SGSIN", "AUBNE");
			BillOfLadingContainer container1 = NewContainerTarget(shipment, "FAKE4100011");
			BillOfLadingContainer container2 = NewContainerTarget(shipment, "FAKE4100027");
			BillOfLadingContainer container3 = NewContainerTarget(shipment, "FAKE4100032");

			shipment.RunPreSaveValidation();
			AssertNoErrors("precondition:", shipment);
			AssertNoMessageErrors("precondition:", shipment);

			AssertEquals("precondition: container1.Logs", 0, container1.Logs.Find(eventFilter).Length);
			AssertEquals("precondition: container2.Logs", 0, container2.Logs.Find(eventFilter).Length);
			AssertEquals("precondition: container3.Logs", 0, container3.Logs.Find(eventFilter).Length);

			ApplyApplicator(new BusinessObject[] { container1, container2 }, expectedLog);

			AssertEquals("Should have added a message to container 1.", 1, container1.Messages.Count);
			AssertEquals("The new message on container 1 should be queued", EDIMessage.Status.Queued, container1.Messages[0].EM_Status);
			AssertEquals("The new message on container 1 should have the correct application code", EDIMessage.ApplicationCodes.EIDO, container1.Messages[0].EM_ApplicationCode);

			AssertEquals("Should have added a message to container 2.", 1, container2.Messages.Count);
			AssertEquals("The new message on container 2 should be queued", EDIMessage.Status.Queued, container2.Messages[0].EM_Status);
			AssertEquals("The new message on container 2 should have the correct application code", EDIMessage.ApplicationCodes.EIDO, container2.Messages[0].EM_ApplicationCode);

			AssertEquals("Should not have added a message to container 3.", 0, container3.Messages.Count);
		}

		[EIDOMessagingConfiguration]
		public void TestRunWithPending()
		{
			ZQuery eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);

			const string expectedLog =
				"ERROR: S00000100-[HL TEST4100029] is still waiting on a response.\r\n" +
				"";

			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			BillOfLading shipment = NewShipmentTarget("SGSIN", "AUBNE");
			BillOfLadingContainer container1 = NewContainerTarget(shipment, "TEST4100013");
			BillOfLadingContainer container2 = NewContainerTarget(shipment, "TEST4100029");

			AddMessage(container2, EDIMessage.Status.Queued);

			shipment.RunPreSaveValidation();
			AssertNoErrors("precondition:", shipment);
			AssertNoMessageErrors("precondition:", shipment);

			AssertEquals("precondition: container1.Logs", 0, container1.Logs.Find(eventFilter).Length);
			AssertEquals("precondition: container2.Logs", 0, container2.Logs.Find(eventFilter).Length);

			ZGuid[] containerPKs = new ZGuid[] { container1.PK, container2.PK };
			ZGuid[] existingMessages = Array.ConvertAll(Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, containerPKs)), (bo) => bo.PK);
			ZQuery exclusionFilter = new ZQuery(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, existingMessages);

			ApplyApplicator(new BusinessObject[] { container1, container2 }, expectedLog);
			EDIMessage[] newMessages1 = (EDIMessage[])container1.Messages.Find(exclusionFilter);
			EDIMessage[] newMessages2 = (EDIMessage[])container2.Messages.Find(exclusionFilter);

			AssertEquals("Should have added a message to container 1.", 1, newMessages1.Length);
			AssertEquals("The new message on container 1 should be queued", EDIMessage.Status.Queued, newMessages1[0].EM_Status);
			AssertEquals("The new message on container 1 should have the correct application code", EDIMessage.ApplicationCodes.EIDO, newMessages1[0].EM_ApplicationCode);

			AssertEquals("Should not have added a message to container 2.", 0, newMessages2.Length);
		}

		[EIDOMessagingConfiguration]
		public void TestRunOnExport()
		{
			ZQuery eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);

			const string expectedLog =
				"WARNING: '[HL S00000100]' is released in another country/region and is not eligible for E-IDO messaging, skipping.\n" +
				"";

			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			BillOfLadingContainer target = NewContainerTarget("AUBNE", "SGSIN");
			target.Booking.RunPreSaveValidation();
			AssertNoErrors("precondition:", target.Booking);
			AssertNoMessageErrors("precondition:", target.Booking);
			AssertEquals("precondition:", 0, target.Messages.Count);
			AssertEquals("precondition:", 0, target.Logs.Find(eventFilter).Length);

			ApplyApplicator(new BusinessObject[] { target.Booking }, expectedLog);
			AssertEquals("Should not have added a message.", 0, target.Messages.Count);

			Factory.Save();

			StmALog[] events = target.Logs.Find(eventFilter);
			AssertEquals("should not have added DeliveryOrderSent event", 0, events.Length);
		}

		[EIDOMessagingConfiguration]
		public void TestRunWithErrors_Skip()
		{
			Settings.ErrorBehaviour = OperationalActionErrorBehaviourList.Codes.Skip;

			ZQuery eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);

			const string expectedLog =
				"WARNING: '[HL S00000100]' has errors and/or message errors.\n" +
				"You will need to correct these before an E-IDO message can be sent for it.\n" +
				"";

			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			BillOfLadingContainer target = NewContainerTarget("SGSIN", "AUBNE");
			target.Booking.JS_GoodsDescription = "";
			target.Booking.RunPreSaveValidation();
			AssertHasErrors("precondition:", target.Booking.JS_GoodsDescriptionInfo);
			AssertEquals("precondition:", 0, target.Messages.Count);
			AssertEquals("precondition:", 0, target.Logs.Find(eventFilter).Length);

			ApplyApplicator(new BusinessObject[] { target.Booking }, expectedLog);
			AssertEquals("Should not have added a message.", 0, target.Messages.Count);

			Factory.Save();

			StmALog[] events = target.Logs.Find(eventFilter);
			AssertEquals("should not have added an event", 0, events.Length);
		}

		[EIDOMessagingConfiguration]
		public void TestRunWithErrors_Abort()
		{
			Settings.ErrorBehaviour = OperationalActionErrorBehaviourList.Codes.Abort;

			ZQuery eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);

			const string expectedLog =
				"ERROR: '[HL S00000100]' has errors and/or message errors.\n" +
				"You will need to correct these before an E-IDO message can be sent for it.\n" +
				"";

			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			BillOfLadingContainer target = NewContainerTarget("SGSIN", "AUBNE");
			target.Booking.JS_GoodsDescription = "";
			target.Booking.RunPreSaveValidation();
			AssertHasErrors("precondition:", target.Booking.JS_GoodsDescriptionInfo);
			AssertEquals("precondition:", 0, target.Messages.Count);
			AssertEquals("precondition:", 0, target.Logs.Find(eventFilter).Length);

			ApplyApplicator(new BusinessObject[] { target.Booking }, expectedLog);
			AssertEquals("Should not have added a message.", 0, target.Messages.Count);

			Factory.Save();

			StmALog[] events = target.Logs.Find(eventFilter);
			AssertEquals("should not have added an event", 0, events.Length);
		}
		[EIDOMessagingConfiguration(Principals = new string[] { "XPrincipal" })]

		public void TestRunUnconfiguredPrincipal()
		{
			ZQuery eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);

			const string expectedLog =
				"ERROR: E-IDO messaging has not been enabled for the principal Principal.\n" +
				"You can enable it in the 'Liner & Agency -> E-IDO Messaging' registry option";

			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			BillOfLadingContainer target = NewContainerTarget("SGSIN", "AUBNE");
			target.Booking.RunPreSaveValidation();
			AssertNoErrors("precondition:", target);
			AssertNoMessageErrors("precondition:", target);
			AssertEquals("precondition:", 0, target.Logs.Find(eventFilter).Length);

			ZGuid[] existingMessages = Array.ConvertAll(target.Messages.ToArray(), (bo) => bo.PK);
			ApplyApplicator(new BusinessObject[] { target.Booking }, expectedLog);
			EDIMessage[] newMessages = (EDIMessage[])target.Messages.Find(new ZQuery(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, existingMessages));

			AssertEquals("Should not have added a message.", 0, target.Messages.Count);

			Factory.Save();

			StmALog[] events = target.Logs.Find(eventFilter);
			AssertEquals("should not have added DeliveryOrderSent event", 0, events.Length);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EIDOSendOriginalApplicator(Settings);
		}

		void AddMessage(BillOfLadingContainer container, string status)
		{
			IEIDOMessagingData data = EIDOShipmentMessagingData.NewOriginal(container);
			IEIDOMessageBuilder builder = EIDOMessageBuilderFactory.GetNewBuilder();
			EIDOMessage message = EIDOMessage.New(container, data.MessageFunction, builder.GenerateMessageText(data));
			message.EM_Status = status;
		}

		#endregion
	}
}
