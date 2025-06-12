using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal_2011_11.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests.UniversalInterchange2UniversalTransactionTest
{
	[TestClass]
	public class Universal_2011_11_Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Universal_2011_11_UniversalInterchangeInclude2UniversalInterchangeEnvelope()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchangeInclude2UniversalInterchangeEnvelope_input.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchangeInclude2UniversalInterchangeEnvelope_output.xml";
			mapTester.Execute<UniversalInterchangeInclude2UniversalInterchangeEnvelope>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchangeCleaner()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchangeCleaner_input.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchangeCleaner_output.xml";
			mapTester.Execute<UniversalInterchangeCleaner>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchange2UniversalInterchangeEnvelope()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchange2UniversalInterchangeEnvelope_EmptyBody_input.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchange2UniversalInterchangeEnvelope_EmptyBody_output.xml";
			mapTester.Execute<UniversalInterchange2UniversalInterchangeEnvelope>(sourceFile, expectedFile);

			sourceFile = "Universal_2011_11.TestFiles.UniversalInterchange2UniversalInterchangeEnvelope_input.xml";
			expectedFile = "Universal_2011_11.TestFiles.UniversalInterchange2UniversalInterchangeEnvelope_output.xml";
			mapTester.Execute<UniversalInterchange2UniversalInterchangeEnvelope>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalEvent2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Universal_2011_11.TestFiles.UniversalEvent2UniversalInterchange_input.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalEvent2UniversalInterchange_output.xml";
			mapTester.Execute<UniversalEvent2UniversalInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchange2UniversalEvent()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalEvent2UniversalInterchange_output.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalEvent2UniversalInterchange_input.xml";
			mapTester.Execute<UniversalInterchange2UniversalEvent>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchangeInclude2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchangeInclude.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchange.xml";
			mapTester.Execute<UniversalInterchangeInclude2UniversalInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchange2UniversalInterchangeInclude()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchange.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchangeInclude.xml";
			mapTester.Execute<UniversalInterchange2UniversalInterchangeInclude>(sourceFile, expectedFile);
		}

		#region UI2XXX	// The following 4 transformations are identical and can be tested with the same pair of files

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchange2UniversalSchedule()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchange.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalXxx.xml";
			mapTester.Execute<UniversalInterchange2UniversalSchedule>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchange2UniversalShipment()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchange.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalXxx.xml";
			mapTester.Execute<UniversalInterchange2UniversalShipment>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchange2UniversalTransaction()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchange.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalXxx.xml";
			mapTester.Execute<UniversalInterchange2UniversalTransaction>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchange2UniversalTransactionBatch()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalInterchange.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalXxx.xml";
			mapTester.Execute<UniversalInterchange2UniversalTransactionBatch>(sourceFile, expectedFile);
		}

		#endregion

		#region XXX2UI	// The following 4 transformations are identical and can be tested with the same pair of files

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalSchedule2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalXxx.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchange_EmptyHeader.xml";
			mapTester.Execute<UniversalSchedule2UniversalInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalXxx.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchange_EmptyHeader.xml";
			mapTester.Execute<UniversalShipment2UniversalInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalTransaction2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalXxx.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchange_EmptyHeader.xml";
			mapTester.Execute<UniversalTransaction2UniversalInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalTransactionBatch2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			string sourceFile = "Universal_2011_11.TestFiles.UniversalXxx.xml";
			string expectedFile = "Universal_2011_11.TestFiles.UniversalInterchange_EmptyHeader.xml";
			mapTester.Execute<UniversalTransactionBatch2UniversalInterchange>(sourceFile, expectedFile);
		}

		#endregion
	}
}
