using System.IO;
using System;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	abstract class CustomsProcedureUniversalReferenceDataFileGeneratorAbstractTest<T> where T : CustomsProcedureUniversalReferenceDataFileGenerator, IProtectedMethod_Exposed, new()
	{
		protected T Generator => generator ?? (generator = new T());
		T generator;

		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.ProcedureFileName = "REGIME_SOLLICITE.xml";
			ApplicationConfig.Instance.PreviousProcedureFileName = "REGIME_PRECEDENT.xml";
			ApplicationConfig.Instance.ConcessionFileName = "REGIME_CODE_COMM.xml";
			ApplicationConfig.Instance.CustomsProcedureFileName = CustomsProcedureFileName;

			var error = Errors.No;

			Generator.GenerateFiles(new DateTime(2022, 02, 09), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, CustomsProcedureOutputFile);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, CustomsProcedureOutputFile));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.AreEqual(expectedFileContent, actualFileContent);
			File.Delete(outputFileName);
		}

		protected abstract string CustomsProcedureFileName { get; }
		protected abstract string CustomsProcedureOutputFile { get; }

		[Test]
		public void TestDataWithInconsistentDatesSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.ProcedureFileName = @"InconsistentDatesTest\REGIME_SOLLICITE_FORTEST.xml";
			ApplicationConfig.Instance.PreviousProcedureFileName = @"InconsistentDatesTest\REGIME_PRECEDENT_FORTEST.xml";
			ApplicationConfig.Instance.ConcessionFileName = @"InconsistentDatesTest\REGIME_CODE_COMM_FORTEST.xml";
			ApplicationConfig.Instance.CustomsProcedureFileName = @"InconsistentDatesTest\REGIME_DOUANIER_FORTEST.xml";

			var error = Errors.No;
			Generator.GenerateFiles(new DateTime(2022, 02, 22), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, CustomsProcedureOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.True(actualFileContent.Contains("<ZZ6_ProcedureCode>96</ZZ6_ProcedureCode>"), "96 97 999 procedure has valid start date and end date.");
			Assert.False(actualFileContent.Contains("<ZZ6_ProcedureCode>98</ZZ6_ProcedureCode>"), "98 96 998 procedure start date is greater than end date.");
			File.Delete(outputFileName);
		}

		protected string[] procedureCodeList = new string[] { "00", "01", "02", "07", "10", "11", "21", "22", "23", "31", "40", "41", "42", "43", "44", "45", "46", "48", "51", "53", "54", "61", "63", "68", "71", "76", "77", "78", "91", "92", "95", "96" };
	}

	interface IProtectedMethod_Exposed
	{
		string GetProcedureGroupCore_Exposed(string procedureCode, string previousProcedureCode);
	}
}
