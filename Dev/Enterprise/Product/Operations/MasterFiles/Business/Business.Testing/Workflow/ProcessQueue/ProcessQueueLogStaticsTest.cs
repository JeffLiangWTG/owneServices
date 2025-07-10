using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessQueueLogStaticsTest : TestCase
	{
		public void TestSchema()
		{
			AssertEquals("Queue", ProcessQueueLog.Schema.Queue);
			AssertEquals("Status", ProcessQueueLog.Schema.Status);
			AssertEquals("SubStatus", ProcessQueueLog.Schema.SubStatus);
			AssertEquals("Reason", ProcessQueueLog.Schema.Reason);
			AssertEquals("AssignedTo", ProcessQueueLog.Schema.AssignedTo);
		}

		public void TestGetEncodedLogReference()
		{
			AssertEquals("COM\"Q01\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "Q01"));
			AssertEquals("COM\"\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, ""));
			AssertEquals("CUS\"LL\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "LL"));

			AssertEquals("CUS\"Q01\",\"XX\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "Q01", "XX"));
			AssertEquals("COM\"QQQ\",\"\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "QQQ", ""));
			AssertEquals("CUS\"\",\"sS\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "", "sS"));

			AssertEquals("CUS\"RRR\",\"XX\",\"RR\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "RRR", "XX", "RR"));
			AssertEquals("COM\"EE\",\"\",\"S1\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "EE", "", "S1"));
			AssertEquals("CUS\"A\",\"sS\",\"\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "A", "sS", ""));

			AssertEquals("COM\"RRR\",\"XX\",\"RR\",\"This\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "RRR", "XX", "RR", "This"));
			AssertEquals("CUS\"EE\",\"\",\"S1\",\"  reason , .!@# whatever $$$\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "EE", "", "S1", "  reason , .!@# whatever $$$"));
			AssertEquals("COM\"A\",\"sS\",\"\",\"\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "A", "sS", "", ""));

			AssertEquals("COM\"RRR\",\"XX\",\"RR\",\"This\",\"AS\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "RRR", "XX", "RR", "This", "AS"));
			AssertEquals("CUS\"EE\",\"\",\"S1\",\"  reason , .!@# whatever $$$\",\"HK\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "EE", "", "S1", "  reason , .!@# whatever $$$", "HK"));
			AssertEquals("COM\"A\",\"sS\",\"\",\"\",\"\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "A", "sS", "", "", ""));
			AssertEquals("CUS\"\",\"\",\"\",\"\",\"\"", ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "", "", "", "", ""));
		}
	}
}
