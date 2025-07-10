using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AdvancedReportMessageLogCreatorTest : TestCaseWithFactory
	{
		#region TestCreateMessageSentLog

		public void TestCreateMessageSentLog()
		{
			var dynamicData = new Mock<IDynamicData>();

			var consol = Factory.New<ForwardingConsol>();
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
			documentData.JDD_ParentID = consol.PK;

			var logCreator = new AdvancedReportMessageLogCreator(DocumentNames.AdvancedCargoReport, Core.Constants.CountryCodes.UnitedStates);
			var res = logCreator.CreateMessageSentLog(documentData, dynamicData.Object, "zzz", "recipient");

			AssertEquals("log creator indicated that sent log has been created", true, res);

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
					"MSN |DEP=recipient|LOC=US|MST=Advanced Cargo Report"
				},
				logs);
		}

		#endregion
	}
}
