using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconCustomsValueCalculationManager))]
	sealed class ReconCustomsValueCalculationManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateRecalculationType()
		{
			var calculator = ReconDeclaration.ReconCustomsValueCalculationManager;
			calculator.RecalculationType = "";
			AssertHasErrorContaining(calculator.RecalculationTypeInfo, "Please enter a Calculation Type.");
			calculator.RecalculationType = "XXX";
			AssertHasError(calculator.RecalculationTypeInfo, "Enter a valid selection.");
			calculator.RecalculationType = "INC";
			AssertNoError(calculator.RecalculationTypeInfo, "Enter a valid selection.");
		}

		public void TestValidateRecalculationPercentage()
		{
			var calculator = ReconDeclaration.ReconCustomsValueCalculationManager;
			calculator.RecalculationPercentage = -10m;
			AssertHasError(calculator.RecalculationPercentageInfo, "Please enter a number greater than 0.");
			calculator.RecalculationPercentage = 10m;
			AssertNoError(calculator.RecalculationPercentageInfo, "Please enter a number greater than 0.");
		}

		public void TestCalculateCustomsValuesForAllEntryLines()
		{
			var calculator = ReconDeclaration.ReconCustomsValueCalculationManager;
			calculator.RecalculationType = "DCR";
			calculator.RecalculationPercentage = 15m;
			var entry = ReconDeclaration.OriginalEntries.AddNew();
			var line = entry.Invoice.JobComInvoiceLines.AddNew();
			line.US_R_OrigCV = 16.20m;
			var line2 = entry.Invoice.JobComInvoiceLines.AddNew();
			line2.US_R_OrigCV = 159.38m;
			var line3 = entry.Invoice.JobComInvoiceLines.AddNew();
			line3.US_R_OrigCV = 583.42m;
			ReconDeclaration.CalculateCustomsValues();
			AssertEquals(13.77m, entry.Invoice.JobComInvoiceLines[0].JI_LinePrice);
			AssertEquals(135.47m, entry.Invoice.JobComInvoiceLines[1].JI_LinePrice);
			AssertEquals(495.91m, entry.Invoice.JobComInvoiceLines[2].JI_LinePrice);
			AssertEquals("Recon Customs Values Decrease 15%", ReconDeclaration.Logs.GetAllLogs()[0].SL_Reference);
		}

		protected override BusinessObject GetNewBusinessObject() => new ReconCustomsValueCalculationManager(ReconDeclaration);

		ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					var dec = Factory.New<JobDeclaration>();
					dec.JE_MessageType = JobMessageTypeList.Codes.Recon;
					dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					reconDeclaration = new ReconDeclaration(dec);
				}

				return reconDeclaration;
			}
		}

		ReconDeclaration reconDeclaration;
	}
}
