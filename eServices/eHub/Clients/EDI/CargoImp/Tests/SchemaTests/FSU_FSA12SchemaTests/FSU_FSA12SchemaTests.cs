using System.Xml.Linq;
using CargoWise.eHub.Clients.EDI.Schemas.CargoIMP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests.SchemaTests.FSU_FSA12SchemaTests
{
	[TestClass]
	public class FSU_FSA12SchemaTests : BaseSchemaTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEmptyARRFlightElement()
		{
			using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_FSA12_Arrival_FlatFile_Input.txt"))
			using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
			{
				Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_FSA12_Arrival.xml"), XDocument.Load(outputMsg).ToString());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEmptBKDFlightDateOfScheduledDepartureElement()
		{
            using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_BKD_FlatFile_EmptyDate_Input.txt"))
			using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
			{
				Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_BKD_EmptyDate.xml"), XDocument.Load(outputMsg).ToString());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEmptyDEPFlightElement()
		{
			using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_FSA12_Departure_FlatFile_Input.txt"))
			using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
			{
				Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_FSA12_Departure.xml"), XDocument.Load(outputMsg).ToString());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSU_FSA_OSIEvent()
		{
			using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_OSI_FlatFile_Input.txt"))
			using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
			{
				Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_OSI.xml"), XDocument.Load(outputMsg).ToString());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPartial_ARR()
		{
			using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_Arrival_Partial_FlatFile_Input.txt"))
			using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
			{
				Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_Arrival_Partial.xml"), XDocument.Load(outputMsg).ToString());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPartial_DEP()
		{
			using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_Departure_Partial_FlatFile_Input.txt"))
			using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
			{
				Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_Departure_Partial.xml"), XDocument.Load(outputMsg).ToString());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPartial_RCF()
		{
			using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_RCF_Partial_FlatFile_Input.txt"))
			using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
			{
				Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_RCF_Partial.xml"), XDocument.Load(outputMsg).ToString());
			}
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEmptyFlightNumber()
        {
            using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_FlatFile_EmptyFlightNumber_Input.txt"))
            using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
            {
                Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_EmptyFlightNumber.xml"), XDocument.Load(outputMsg).ToString());
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFSU_FSA_FOHWithVolumeDetails()
        {
	        using (var inputMsg = GetEmbeddedResource("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_FOHWithVolumeDetails.txt"))
	        using (var outputMsg = SchemaTester<FSU_FSA12>.ParseFF(inputMsg))
	        {
		        Assert.AreEqual(GetResourceAsString("SchemaTests.FSU_FSA12SchemaTests.TestFiles.FSU_FOHWithVolumeDetails.xml"), XDocument.Load(outputMsg).ToString());
	        }
        }
	}
}
