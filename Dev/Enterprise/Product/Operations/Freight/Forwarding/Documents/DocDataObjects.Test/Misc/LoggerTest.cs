using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class LoggerTest : TestCaseWithFactory
	{
		#region TestCreateMessageSentLog

		public void TestCreateMessageSentLog()
		{
			var documentData = Factory.New<VisualizerDocumentData>();

			var res = documentData.CreateMessageSentLog("Air Booking");

			AssertEquals("logger indicated that sent log has been created", true, res);

			var logs = documentData
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
					"MSN |DEP=Carrier|MST=Air Booking"
				},
				logs);
		}

		#endregion

		#region TestCreateMessageWithdrawalLog

		public void TestCreateMessageWithdrawalLog()
		{
			var documentData = Factory.New<VisualizerDocumentData>();

			var res = documentData.CreateMessageWithdrawalLog("Air Booking", "reason");

			AssertEquals("logger indicated that sent log has been created", true, res);

			var logs = documentData
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
					"MWR |DEP=Carrier|MST=Air Booking|RES=reason"
				},
				logs);
		}

		#endregion

		#region TestCreateStatusUpdateLog

		public void TestCreateStatusUpdateLog()
		{
			var documentData = Factory.New<VisualizerDocumentData>();

			var res = documentData.CreateStatusUpdateLog("Air Booking");

			AssertEquals("logger indicated that sent log has been created", true, res);

			var logs = documentData
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
					"STU |DEP=CargoWise Support|MST=Air Booking|TYP=Reset To Original"
				},
				logs);
		}

		#endregion

		#region TestCreateDataExportLog

		public void TestCreateDataExportLog()
		{
			var documentData = Factory.New<VisualizerDocumentData>();

			const string interchangeContent = "<UniversalInterchange><Body><UniversalShipment></UniversalShipment></Body></UniversalInterchange>";
			const string messageContent = "<UniversalShipment></UniversalShipment>";

			var res = documentData.CreateDataExportEventLog(interchangeContent);

			AssertEquals("logger indicated that sent log has been created", true, res);

			var logs = documentData
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.ToArray();

			var logsFormatted = logs
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
					"DEX"
				},
				logsFormatted);

			var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, logs[0].PK);
			var pivots = Factory.Load<IGenPivot>(pivotQuery);

			AssertEquals("Pivot has been created", 1, pivots.Length);

			var message = Factory.Load<IEDIMessage>(pivots[0].XX_Relation2ID);

			AssertNotNull("message was created", message);
			AssertMultilineASCIIEquals("message content", messageContent, message.EM_MessageText);
		}

		#endregion
	}
}
