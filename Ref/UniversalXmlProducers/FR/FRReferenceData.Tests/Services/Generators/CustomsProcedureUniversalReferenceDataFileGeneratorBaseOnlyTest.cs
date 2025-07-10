using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;
using System.IO;
using System;
using System.Linq;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class CustomsProcedureUniversalReferenceDataFileGeneratorBaseOnlyTest
	{

		[Test]
		public void TestHasEconomicImpact()
		{
			var expectedProcedureHavingEconomicImpact = new string[] { "02", "11", "21", "22", "23", "31", "41", "51", "53", "54", "71", "76", "77", "78" };
			foreach (var procedure in procedureCodeList)
			{
				if (expectedProcedureHavingEconomicImpact.Contains(procedure))
				{
					Assert.That(CustomsProcedureUniversalReferenceDataFileGenerator.HasEconomicImpact(procedure), $"Procedure {procedure} is supposed to have economic impact.");
				}
				else
				{
					Assert.That(!CustomsProcedureUniversalReferenceDataFileGenerator.HasEconomicImpact(procedure), $"Procedure {procedure} is not supposed to have any economic impact.");
				}
			}
		}

		[Test]
		public void RequiresIntoEndUseAttribute()
		{
			Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresIntoEndUseAttribute("44"));
			Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresIntoEndUseAttribute("44P"));
			Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresIntoEndUseAttribute(""));
			Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresIntoEndUseAttribute("01"));
			Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresIntoEndUseAttribute("01P"));
		}

		[Test]
		public void TestRequiresVATNumberExemptionAttribute()
		{
			var concession = "F48";
			Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresVATNumberExemptionAttribute("", concession, ""));

			var VATNumberExemptedProcedureCodes = new string[] { "42", "51", "53", "63", "71", "78" };
			foreach (var procedure in procedureCodeList)
			{
				if (VATNumberExemptedProcedureCodes.Contains(procedure))
				{
					Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresVATNumberExemptionAttribute(procedure, "", ""));
					Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresVATNumberExemptionAttribute(procedure, "D51", ""));
					Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresVATNumberExemptionAttribute(procedure, "", "53P"));
					Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresVATNumberExemptionAttribute(procedure, "D51", "53P"));
				}
				else
				{
					Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.RequiresVATNumberExemptionAttribute(procedure, "", ""));
				}
			}
		}

		[Test]
		public void TestIsGuaranteeReleased()
		{
			foreach (var procedure in procedureCodeList)
			{
				if (proceduresWithGuarantee.Contains(procedure))
				{
					Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.IsGuaranteeReleased(procedure));
				}
				else
				{
					Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.IsGuaranteeReleased(procedure));
				}
			}
		}

		[Test]
		public void TestIsGuaranteeConsumed()
		{
			foreach (var previousProcedure in procedureCodeList)
			{
				if (proceduresWithGuarantee.Contains(previousProcedure))
				{
					Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.IsGuaranteeConsumed(previousProcedure));
				}
				else
				{
					Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.IsGuaranteeConsumed(previousProcedure));
				}
			}
		}

		[Test]
		public void TestGetCategory()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.ProcedureFileName = "REGIME_SOLLICITE.xml";

			Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.IsImportProcedure("40"));
			Assert.That(CustomsProcedureUniversalReferenceDataFileGenerator.GetCategory("40") == "IM");

			Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.IsImportProcedure("10"));
			Assert.That(CustomsProcedureUniversalReferenceDataFileGenerator.GetCategory("10") == "EX");
		}

		[Test]
		public void TestGetCalculateVAT()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.ProcedureFileName = "REGIME_SOLLICITE.xml";
			ApplicationConfig.Instance.ConcessionFileName = "REGIME_CODE_COMM.xml";

			var exemptedOfVATCalculationProcedures = new string[] { "42", "45", "49", "63" };
			foreach (var procedure in procedureCodeList)
			{
				if (CustomsProcedureUniversalReferenceDataFileGenerator.IsImportProcedure(procedure) && !exemptedOfVATCalculationProcedures.Contains(procedure))
				{
					Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.GetCalculateVAT(procedure, "", ""), $"{procedure}");
				}
				else
				{
					Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.GetCalculateVAT(procedure, "", ""), $"{procedure}");
				}
			}

			Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.GetCalculateVAT("61", "22", "B02"), $"ZZ6_CalculateVAT should be false for procedure 61-22-B02.");
		}

		[Test]
		public void TestGetCalculateDuties()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.ProcedureFileName = "REGIME_SOLLICITE.xml";

			var exemptedOfDutiesCalculationProcedures = new string[] { "63" };
			foreach (var procedure in procedureCodeList)
			{
				if (CustomsProcedureUniversalReferenceDataFileGenerator.IsImportProcedure(procedure) && !exemptedOfDutiesCalculationProcedures.Contains(procedure))
				{
					Assert.True(CustomsProcedureUniversalReferenceDataFileGenerator.GetCalculateDuties(procedure), $"{procedure}");
				}
				else
				{
					Assert.False(CustomsProcedureUniversalReferenceDataFileGenerator.GetCalculateDuties(procedure), $"{procedure}");
				}
			}
		}

		string[] procedureCodeList = new string[] { "00", "01", "02", "07", "10", "11", "21", "22", "23", "31", "40", "41", "42", "43", "44", "45", "46", "48", "51", "53", "54", "61", "63", "68", "71", "76", "77", "78", "91", "92", "95", "96" };
		string[] proceduresWithGuarantee = new string[] { "07", "48", "51", "53", "71", "78", "91" };
	}
}
