using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class ContainerSequenceStoreTest : TestCase
	{
		public void TestGetSequenceNumber()
		{
			var testSequenceStore = new ContainerSequenceStore(new Dictionary<string, int>(2)
			{ { "C1", 1 }, { "C2", 30 }, { "C3", 25 } });
			AssertEquals("C1 sequence number", 1, testSequenceStore.GetSequenceNumber("C1"));
			AssertEquals("C2 sequence number", 30, testSequenceStore.GetSequenceNumber("C2"));
			AssertEquals("C3 sequence number", 25, testSequenceStore.GetSequenceNumber("C3"));
			AssertEquals("NewContainer sequence number", -1, testSequenceStore.GetSequenceNumber("NewContainer"));
			testSequenceStore = new ContainerSequenceStore(null);
			AssertEquals("[NO SEQUENCE DATA] C1 sequence number", -1, testSequenceStore.GetSequenceNumber("C1"));
			AssertEquals("[NO SEQUENCE DATA] C2 sequence number", -1, testSequenceStore.GetSequenceNumber("C2"));
			AssertEquals("[NO SEQUENCE DATA] C3 sequence number", -1, testSequenceStore.GetSequenceNumber("C3"));
			AssertEquals("[NO SEQUENCE DATA] NewContainer sequence number", -1, testSequenceStore.GetSequenceNumber("NewContainer"));
		}

		public void TestHighestSequenceNumber()
		{
			var testSequenceStore = new ContainerSequenceStore(new Dictionary<string, int>(2)
			{ { "C1", 1 }, { "C2", 20 }, { "C3", 3 }, { "C4", 14 } });
			AssertEquals("HighestSequenceNumber", 20, testSequenceStore.HighestSequenceNumber);
			testSequenceStore = new ContainerSequenceStore(null);
			AssertEquals("[NO SEQUENCE DATA] HighestSequenceNumber", 0, testSequenceStore.HighestSequenceNumber);
		}
	}
}
