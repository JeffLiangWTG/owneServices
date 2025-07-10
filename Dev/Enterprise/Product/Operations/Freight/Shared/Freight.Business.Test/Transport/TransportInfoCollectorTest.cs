using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportInfoCollectorTest : TestCaseWithFactory
	{
		public void TestDeletedFromRowFactory()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			var row = ((INeedRow)transport).Row;
			row.Delete();
			AssertEquals("pre: state is detached", DataRowState.Detached, row.RowState);
			AssertEquals("pre: bizo IsDeleted", true, transport.IsDeleted);

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			var isLinked = transport.JW_IsLinked;
			CombineAssertions(() =>
			{
				AssertEquals("error reporter count", 1, ErrorReporter.TotalErrorCount);
				AssertContains("bizo deletion stack trace is not recorded", "Deletion stack trace never collected.", ErrorReporter.LastMessageReported);
				AssertContains("row deleted stack trace is recorded", "TransportInfoCollector.DataRowDeleting_EventHandler", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}
	}
}
