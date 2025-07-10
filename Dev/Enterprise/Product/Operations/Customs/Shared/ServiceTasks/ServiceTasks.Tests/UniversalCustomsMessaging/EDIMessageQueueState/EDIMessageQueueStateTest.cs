using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	class EDIMessageQueueStateTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var identifier = Guid.NewGuid();
			var messagePK = Guid.NewGuid();
			var parentSystemCreateTimeUtc = new DateTime(2024, 01, 23, 22, 54, 45, 342);
			var keys = new[] { "KEY1", "KEY2" };
			var chainID = Guid.NewGuid();
			var queueState = new EDIMessageQueueState(identifier, messagePK, "ENT212", parentSystemCreateTimeUtc, "ACK", keys, chainID);
			AssertEDIMessageQueueState(queueState, identifier, messagePK, parentSystemCreateTimeUtc, "ENT212", "ACK", keys, chainID, true);

			var message = Factory.New<EDIMessage>();
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			queueState = new EDIMessageQueueState(message, keys, "ENT212");
			AssertEDIMessageQueueState(queueState, queueState.Identifier, message.PK.ToGuid(), message.EM_SystemCreateTimeUtc.ToDateTime(), "ENT212", string.Empty, keys, Guid.Empty, false);
		}

		public void TestUpdateStatus()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var keys = new[] { "KEY1", "KEY2" };
			var queueState = new EDIMessageQueueState(message, keys, "ENT212");
			var chainID = Guid.NewGuid();
			queueState.SetChainId(chainID);
			AssertEquals("queueState.Status - Default", string.Empty, queueState.Status);
			AssertEquals("queueState.StatusChanged - Default", false, queueState.StatusChanged);
			queueState.UpdateStatus("PRE");
			AssertEquals("queueState.Status - PRE", "PRE", queueState.Status);
			AssertEquals("queueState.StatusChanged - PRE", true, queueState.StatusChanged);
			queueState.SetAsDatabaseSynced();
			queueState.UpdateStatus("PRE");
			AssertEquals("queueState.Status - SetAsDatabaseSynced - PRE", "PRE", queueState.Status);
			AssertEquals("queueState.StatusChanged - SetAsDatabaseSynced - PRE", false, queueState.StatusChanged);
			queueState.UpdateStatus(QueueStatusCodes.Codes.Queued);
			AssertEquals("queueState.Status - QUE", "QUE", queueState.Status);
			AssertEquals("queueState.StatusChanged - QUE", true, queueState.StatusChanged);
			AssertEquals("ExceptionReporterTestListener.Instance.Count - QUE", 0, ExceptionReporterTestListener.Instance.Count);
			queueState.UpdateStatus(QueueStatusCodes.Codes.Queued);
			AssertEquals("queueState.Status - 2nd QUE", "QUE", queueState.Status);
			AssertEquals("queueState.StatusChanged - 2nd QUE", true, queueState.StatusChanged);
			AssertEquals("ErrorReporter.LastKeyReported - 2nd QUE", string.Empty, ErrorReporter.LastKeyReported);
			AssertEquals("ExceptionReporterTestListener.Instance.Count - 2nd QUE", 0, ExceptionReporterTestListener.Instance.Count);
			queueState.SetAsDatabaseSynced();
			queueState.UpdateStatus(QueueStatusCodes.Codes.Processed);
			AssertEquals("queueState.Status - PRS", "PRS", queueState.Status);
			AssertEquals("queueState.StatusChanged - PRS", true, queueState.StatusChanged);
			AssertEquals("ExceptionReporterTestListener.Instance.Count - PRS", 1, ExceptionReporterTestListener.Instance.Count);
			var exception = (DeveloperNotificationException)ExceptionReporterTestListener.Instance[0];
			AssertEquals("ExceptionReporterTestListener.Instance.GetExceptionKey(0) - PRS", "Status should never change after a row is queued", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
			AssertEquals("exception.InnerException.Message - PRS", $"EDIMessageQueueState.Status should never change after a row is queued: ParentID:{message.PK}, ChainID:{chainID}", exception.InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSetChainIdAndSetAsDatabaseSynced()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var keys = new[] { "KEY1", "KEY2" };
			var queueState = new EDIMessageQueueState(message, keys, "ENT212");
			AssertEquals("queueState.ChainID - Pre", Guid.Empty, queueState.ChainID);
			AssertEquals("queueState.ChainIdChanged - Pre", false, queueState.ChainIdChanged);
			AssertEquals("queueState.IsInDatabase - Pre", false, queueState.IsInDatabase);
			AssertEquals("queueState.Status - Pre", string.Empty, queueState.Status);
			AssertEquals("queueState.StatusChanged - Pre", false, queueState.StatusChanged);
			var chainID = Guid.NewGuid();
			queueState.SetChainId(chainID);
			AssertEquals("queueState.ChainID - SetChainId", chainID, queueState.ChainID);
			AssertEquals("queueState.ChainIdChanged - SetChainId", true, queueState.ChainIdChanged);
			AssertEquals("queueState.IsInDatabase - SetChainId", false, queueState.IsInDatabase);
			AssertEquals("queueState.Status - SetChainId", string.Empty, queueState.Status);
			AssertEquals("queueState.StatusChanged - SetChainId", false, queueState.StatusChanged);
			queueState.UpdateStatus("QUE");
			AssertEquals("queueState.ChainID - UpdateStatus", chainID, queueState.ChainID);
			AssertEquals("queueState.ChainIdChanged - UpdateStatus", true, queueState.ChainIdChanged);
			AssertEquals("queueState.IsInDatabase - UpdateStatus", false, queueState.IsInDatabase);
			AssertEquals("queueState.Status - UpdateStatus", "QUE", queueState.Status);
			AssertEquals("queueState.StatusChanged - UpdateStatus", true, queueState.StatusChanged);
			queueState.SetAsDatabaseSynced();
			AssertEquals("queueState.ChainID - SetAsDatabaseSynced", chainID, queueState.ChainID);
			AssertEquals("queueState.ChainIdChanged - SetAsDatabaseSynced", false, queueState.ChainIdChanged);
			AssertEquals("queueState.IsInDatabase - SetAsDatabaseSynced", true, queueState.IsInDatabase);
			AssertEquals("queueState.Status - SetAsDatabaseSynced", "QUE", queueState.Status);
			AssertEquals("queueState.StatusChanged - SetAsDatabaseSynced", false, queueState.StatusChanged);
		}

		public void TestGetParentDetails()
		{
			var identifier = Guid.NewGuid();
			var messagePK = Guid.NewGuid();
			var parentSystemCreateTimeUtc = new DateTime(2024, 01, 23, 22, 54, 45, 342);
			var keys = new[] { "KEY1", "KEY2" };
			var chainID = Guid.NewGuid();
			var queueState = new EDIMessageQueueState(identifier, messagePK, "ENT212", parentSystemCreateTimeUtc, "ACK", keys, chainID);
			AssertEquals("queueState.GetParentDetails()", $"Time:{SqlFormatInfo.ToSqlDateTimeString(parentSystemCreateTimeUtc)}-Number:ENT212", queueState.GetParentDetails());

			queueState = new EDIMessageQueueState(identifier, messagePK, null, parentSystemCreateTimeUtc, "ACK", keys, chainID);
			AssertEquals("queueState.GetParentDetails()", $"Time:{SqlFormatInfo.ToSqlDateTimeString(parentSystemCreateTimeUtc)}", queueState.GetParentDetails());
		}

		public void TestIQueueStateMembers()
		{
			var identifier = Guid.NewGuid();
			var messagePK = Guid.NewGuid();
			var parentSystemCreateTimeUtc = new DateTime(2024, 01, 23, 22, 54, 45, 342);
			var keys = new[] { "KEY1", "KEY2" };
			var chainID = Guid.NewGuid();
			IQueueState queueState = new EDIMessageQueueState(identifier, messagePK, "ENT212", parentSystemCreateTimeUtc, "ACK", keys, chainID);
			AssertIQueueState(queueState, identifier, messagePK, parentSystemCreateTimeUtc, "ENT212", "ACK", keys, chainID, true);
		}

		public void TestEquals()
		{
			var message = Factory.New<EDIMessage>();
			var queueState1 = new EDIMessageQueueState(message, Array.Empty<string>(), "ENT212");
			var queueState2 = new EDIMessageQueueState(message, new[] { "ENT" }, "ENT213");
			var queueState3 = new EDIMessageQueueState(Factory.New<EDIMessage>(), Array.Empty<string>(), "ENT212");
			AssertEquals("queueState1.Equals(queueState2)", true, queueState1.Equals(queueState2));
			AssertEquals("queueState1.Equals(queueState3)", false, queueState1.Equals(queueState3));
		}

		public void TestGetHashCode()
		{
			var identifier = Guid.NewGuid();
			var messagePK = Guid.NewGuid();
			var parentSystemCreateTimeUtc = new DateTime(2024, 01, 23, 22, 54, 45, 342);
			var keys = new[] { "KEY1", "KEY2" };
			var chainID = Guid.NewGuid();
			var queueState = new EDIMessageQueueState(identifier, messagePK, "ENT212", parentSystemCreateTimeUtc, "ACK", keys, chainID);
			AssertEquals("queueState.GetHashCode()", messagePK.GetHashCode(), queueState.GetHashCode());
		}

		void AssertEDIMessageQueueState(EDIMessageQueueState queueState, Guid identifier, Guid messagePK,
			DateTime parentSystemCreateTimeUtc, string parentMessageNumber, string status, IEnumerable<string> keys,
			Guid chainID, bool isInDatabase)
		{
			AssertEquals("queueState.ParentSystemCreateTimeUtc", parentSystemCreateTimeUtc, queueState.ParentSystemCreateTimeUtc);
			AssertEquals("queueState.ParentMessageNumber", parentMessageNumber, queueState.ParentMessageNumber);
			AssertIQueueState(queueState, identifier, messagePK, parentSystemCreateTimeUtc, parentMessageNumber, status, keys, chainID, isInDatabase);
		}

		void AssertIQueueState(IQueueState queueState, Guid identifier, Guid messagePK,
			DateTime parentSystemCreateTimeUtc, string parentMessageNumber, string status, IEnumerable<string> keys,
			Guid chainID, bool isInDatabase)
		{
			AssertEquals("queueState.IsInDatabase", isInDatabase, queueState.IsInDatabase);
			AssertEquals("queueState.Identifier", identifier, queueState.Identifier);
			AssertEquals("queueState.ParentID", messagePK, queueState.ParentID);
			AssertEquals("queueState.TableCode", EDIMessageSchema.Constants.Prefix, queueState.TableCode);
			AssertEquals("queueState.ChainID", chainID, queueState.ChainID);
			AssertEquals("queueState.Status", status, queueState.Status);
			AssertEquals("queueState.Keys", keys, queueState.Keys);
			AssertEquals("queueState.OrderInfo", parentSystemCreateTimeUtc.Ticks.ToString().PadLeft(20, '0') + (parentMessageNumber ?? string.Empty), queueState.OrderInfo);
		}
	}
}
