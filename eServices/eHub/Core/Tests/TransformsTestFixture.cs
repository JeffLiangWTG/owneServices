using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Tests the core transformation artefacts
    /// </summary>
    [TestClass]
    public class TransformsTestFixture
    {
        private MapTester _mapTester;
        private Assembly _testFixture;

        [TestInitialize]
        public void TestSetup()
        {
            _testFixture = Assembly.GetExecutingAssembly();
			_mapTester = new MapTester(_testFixture, Comparer);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test_dbo_SelectOutboxEnvelopesBySchedule2OutboxEnvelopes()
        {
            string source = "TestFiles.dbo_SelectOutboxEnvelopesBySchedule2OutboxEnvelopes_Source.xml";
            string expected = "TestFiles.dbo_SelectOutboxEnvelopesBySchedule2OutboxEnvelopes_Expected.xml";
            _mapTester.Execute<dbo_SelectOutboxEnvelopesBySchedule2OutboxEnvelopes>(source, expected);
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_dbo_SelectOutboxMessageByEnvelopeIDResponse2OutboxMessage()
		{
			string source = "TestFiles.dbo_SelectOutboxMessageByEnvelopeIDResponse2OutboxMessage_Source.xml";
			string expected = "TestFiles.dbo_SelectOutboxMessageByEnvelopeIDResponse2OutboxMessage_Expected.xml";
			_mapTester.Execute<dbo_SelectOutboxMessageByEnvelopeIDResponse2OutboxMessage>(source, expected);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_OutboxEnvelope2dbo_SelectOutboxMessageByEnvelopeID()
		{
			
			string source = "TestFiles.OutboxEnvelope2dbo_SelectOutboxMessageByEnvelopeID_Source.xml";
			string expected = "TestFiles.OutboxEnvelope2dbo_SelectOutboxMessageByEnvelopeID_Expected.xml";
			_mapTester.Execute<OutboxEnvelope2dbo_SelectOutboxMessageByEnvelopeID>(source, expected);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_InboxInsert2dbo_InsertInboxXmlContent()
		{
			string source = "TestFiles.InboxInsert2dbo_InsertInboxXmlContent_Source.xml";
			string expected = "TestFiles.InboxInsert2dbo_InsertInboxXmlContent_Expected.xml";
			_mapTester.Execute<InboxInsert2dbo_InsertInboxXmlContent>(source, expected);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_OutboxInsert2dbo_InsertOutboxMessage()
		{
			string source = "TestFiles.OutboxInsert2dbo_InsertOutboxMessage_Source.xml";
			string expected = "TestFiles.OutboxInsert2dbo_InsertOutboxMessage_Expected.xml";
			_mapTester.Execute<OutboxInsert2dbo_InsertOutboxMessage>(source, expected);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = new List<string>();
					exclusionXpaths.Add("/*[local-name()='SelectOutboxMessageByEnvelopeID']/*[local-name()='CurrentDateTimeUTC']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;
    }
}
