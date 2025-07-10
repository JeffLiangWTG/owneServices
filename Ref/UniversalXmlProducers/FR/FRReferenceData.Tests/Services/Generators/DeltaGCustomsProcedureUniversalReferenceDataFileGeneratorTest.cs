using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class DeltaGCustomsProcedureUniversalReferenceDataFileGeneratorTest : CustomsProcedureUniversalReferenceDataFileGeneratorAbstractTest<DeltaGCustomsProcedureUniversalReferenceDataFileGeneratorForTest>
	{
		protected override string CustomsProcedureFileName => "REGIME_DOUANIER.xml";
		protected override string CustomsProcedureOutputFile => ApplicationConfig.Instance.FRCustomsProcedureOutputFile;

		

		[Test]
		public void TestCalcProcedureGroupEconomicImpactFlag()
		{
			foreach (var procedure in procedureCodeList)
			{
				foreach (var previousProcedure in procedureCodeList)
				{
					if (CustomsProcedureUniversalReferenceDataFileGenerator.HasEconomicImpact(procedure) || CustomsProcedureUniversalReferenceDataFileGenerator.HasEconomicImpact(previousProcedure))
					{
						Assert.That(Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("P"), $"Procedure group for tuple ({procedure}, {previousProcedure}) should contain P because one of its terms has economic impact.");
					}
					else
					{
						Assert.That(!Generator.GetProcedureGroupCore_Exposed(procedure, previousProcedure).Contains("P"), $"Procedure group for tuple ({procedure}, {previousProcedure}) should not contain P because none of its terms has economic impact.");
					}
				}
			}
		}
	}

	class DeltaGCustomsProcedureUniversalReferenceDataFileGeneratorForTest : DeltaGCustomsProcedureUniversalReferenceDataFileGenerator, IProtectedMethod_Exposed
	{
		public string GetProcedureGroupCore_Exposed(string procedureCode, string previousProcedureCode) => GetProcedureGroupCore(procedureCode, previousProcedureCode);
	}
}
