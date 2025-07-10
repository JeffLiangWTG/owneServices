using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class DeltaIECustomsProcedureUniversalReferenceDataFileGeneratorTest : CustomsProcedureUniversalReferenceDataFileGeneratorAbstractTest<DeltaIECustomsProcedureUniversalReferenceDataFileGeneratorForTest>
	{
		[Test]
		public void TestCalcProcedureGroup_UCC6Flags()
		{
			var B1Procedures = new string[] { "10", "11", "23", "31" };
			var B2Procedures = new string[] { "21", "22" };
			var B3Procedures = new string[] { "76", "77" };
			var B4Procedures = new string[] { "10" };
			var C1Procedures = new string[] { "10", "11", "23", "31" };
			var H1Procedures = new string[] { "01", "07", "40", "42", "43", "44", "45", "46", "48", "61", "63", "68" };
			var H2Procedures = new string[] { "71" };
			var H3Procedures = new string[] { "53" };
			var H4Procedures = new string[] { "51" };
			var H5Procedures = new string[] { "40", "42", "61", "63", "95", "96" };
			var H6Procedures = new string[] { "01", "07", "40" };
			var I1Procedures = new string[] { "01", "07", "40", "42", "43", "44", "45", "46", "48", "51", "53", "61", "63", "68" };

			foreach (var procedure in procedureCodeList)
			{
				foreach (var previousProcedure in procedureCodeList)
				{
					if (B1Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("B1"), $"Procedure group  with procedure {procedure} should contain B1");
					}
					if (B2Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("B2"), $"Procedure group  with procedure {procedure} should contain B2");
					}
					if (B3Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("B3"), $"Procedure group  with procedure {procedure} should contain B3");
					}
					if (B4Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("B4"), $"Procedure group  with procedure {procedure} should contain B4");
					}
					if (C1Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("C1"), $"Procedure group  with procedure {procedure} should contain C1");
					}
					if (H1Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("H1"), $"Procedure group  with procedure {procedure} should contain H1");
					}
					if (H2Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("H2"), $"Procedure group  with procedure {procedure} should contain H2");
					}
					if (H3Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("H3"), $"Procedure group  with procedure {procedure} should contain H3");
					}
					if (H4Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("H4"), $"Procedure group  with procedure {procedure} should contain H4");
					}
					if (H5Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("H5"), $"Procedure group  with procedure {procedure} should contain H5");
					}
					if (H6Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("H6"), $"Procedure group  with procedure {procedure} should contain H6");
					}
					if (I1Procedures.Contains(procedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("I1"), $"Procedure group  with procedure {procedure} should contain I1");
					}
					if (procedure == "40" && previousProcedure == "00")
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("H7"), $"Procedure group  with procedure {procedure} should contain H7");
					}
				}
			}
		}


		[Test]
		public void TestCalcProcedureGroup_NonUCC6Flags()
		{
			var expectedNonUCC6Procedures = new string[] { "02", "41", "54", "78", "91", "92" };
			foreach (var procedure in expectedNonUCC6Procedures)
			{
				foreach (var previousProcedure in expectedNonUCC6Procedures)
				{
					var procedureGroup = Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure);
					Assert.True(string.IsNullOrEmpty(procedureGroup), $"Procedure group  with procedure {procedure} should be empty because its UCC6 declaration type is unknown");
				}
			}
		}

		protected override string CustomsProcedureFileName => "UCC6REGIME_DOUANIER.xml";

		protected override string CustomsProcedureOutputFile => ApplicationConfig.Instance.FRDeltaIECustomsProcedureOutputFile;
	}

	class DeltaIECustomsProcedureUniversalReferenceDataFileGeneratorForTest : DeltaIECustomsProcedureUniversalReferenceDataFileGenerator, IProtectedMethod_Exposed
	{
		public string GetProcedureGroupCore_Exposed(string procedureCode, string previousProcedureCode) => GetProcedureGroupCore(procedureCode, previousProcedureCode);
	}
}
