using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class ProcedureCodesHelperTest : TestCaseWithFactory
{
	public void TestCheckForRuleR407()
	{
		const string messageError = "R407 - Invalid Procedure/Add.Procedure Code combination.";

		var invalidConcessionCodes = new List<ZString>
		{
			"C01", "C02", "C03", "C04", "C06", "C07", "C08", "C09",
			"C10", "C11", "C12", "C13", "C14", "C15", "C16", "C17", "C18", "C19",
			"C20", "C21", "C22", "C23", "C24", "C25", "C26", "C27", "C28", "C29",
			"C30", "C31", "C32", "C33", "C34", "C35", "C36", "C37", "C38", "C39",
			"C40", "C41",
			"B02", "B03",
			"F01", "F02", "F03", "F21", "F22",
			"3PL", "4PL", "5PL", "6PL", "7PL",
			"1C1"
		}.AsReadOnly();

		CombineAssertions(() =>
		{
			AssertCheckForRuleR407("Procedure code starts with 4", "4100", ZString.Empty, string.Empty);
			AssertCheckForRuleR407("Procedure code starts with 6", "6100", ZString.Empty, string.Empty);
			foreach (var concessionCode in invalidConcessionCodes)
			{
				AssertCheckForRuleR407($"Procedure code doesn't start with 4 or 6; Invalid Concession Code {concessionCode}", "5100", concessionCode, messageError);
			}
			AssertCheckForRuleR407($"Procedure code doesn't start with 4 or 6; Valid Concession Code", "5100", "D01", string.Empty);
		});

		void AssertCheckForRuleR407(string testCase, ZString procedureCode, ZString concessionCode, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR407(procedureCode, concessionCode, null, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}

	public void TestCheckForRuleR414()
	{
		const string messageError = "R414 - Invalid Procedure/Add.Procedure Code combination.";

		CombineAssertions(() =>
		{
			AssertCheckForRuleR414("Procedure code 48", "48", ZString.Empty, string.Empty);
			AssertCheckForRuleR414("Procedure code 51", "51", ZString.Empty, string.Empty);
			AssertCheckForRuleR414("Procedure code 51, Invalid Concession Code B07", "51", "B07", messageError);
			AssertCheckForRuleR414("Procedure code 51, Valid Concession Code", "51", "B06", string.Empty);
		});

		void AssertCheckForRuleR414(string testCase, ZString procedureCode, ZString concessionCode, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR414(procedureCode, concessionCode, null, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}

	public void TestCheckForRuleR419()
	{
		const string messageError = "R419 - Invalid CN Code/Add.Procedure Code C01.";

		CombineAssertions(() =>
		{
			AssertCheckForRuleR419("Tariff 9905", "9905", ZString.Empty, string.Empty);
			AssertCheckForRuleR419("Tariff 9906", "9906", ZString.Empty, string.Empty);
			AssertCheckForRuleR419("Valid Concession Code C02", ZString.Empty, "C02", string.Empty);
			AssertCheckForRuleR419("InValid Concession Code C01", ZString.Empty, "C01", messageError);
			AssertCheckForRuleR419("Tariff 9906, Invalid Concession Code B07", "9905", "C02", string.Empty);
			AssertCheckForRuleR419("Tariff 9906, Valid Concession Code", "9906", "C01", messageError);
		});

		void AssertCheckForRuleR419(string testCase, ZString tariff, ZString concessionCode, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR419(concessionCode, null, tariff, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}

	public void TestCheckForRuleR421()
	{
		const string messageError = "R421 - Invalid Procedure/Add.Procedure Code combination.";

		var validConcessionCodes = new List<ZString>
		{
			"C02", "C03", "C04", "C06", "C41", "C20", "C26"
		}.AsReadOnly();

		CombineAssertions(() =>
		{
			AssertCheckForRuleR421("Tariff 9919", "9919", ZString.Empty, string.Empty);
			AssertCheckForRuleR421("Tariff 9918", "9918", ZString.Empty, string.Empty);
			AssertCheckForRuleR421("Tariff 9918, Invalid Concession Code C01", "9918", "C01", string.Empty);
			foreach (var concession in validConcessionCodes)
			{
				AssertCheckForRuleR421($"Invalid tariff, Valid Concession Code {concession}", ZString.Empty, concession, messageError);
				AssertCheckForRuleR421($"Valid tariff, Valid Concession Code {concession}", "9919", concession, ZString.Empty);
			}
		});

		void AssertCheckForRuleR421(string testCase, ZString tariff, ZString concessionCode, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR421(concessionCode, null, tariff, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}

	public void TestCheckForRuleR424()
	{
		const string messageError = "R424 - Invalid Procedure/Add.Procedure Code combination.";

		CombineAssertions(() =>
		{
			AssertCheckForRuleR424("Procedure code start with 4", "41", ZString.Empty, string.Empty);
			AssertCheckForRuleR424("Procedure code start with 6", "61", ZString.Empty, string.Empty);
			AssertCheckForRuleR424("Procedure code 51", "51", ZString.Empty, string.Empty);
			AssertCheckForRuleR424("Procedure code 71", "71", ZString.Empty, string.Empty);
			AssertCheckForRuleR424("Valid Concession Code not starting with D, C01", ZString.Empty, "C01", string.Empty);
			AssertCheckForRuleR424("Invalid Concession Code start with D", ZString.Empty, "D50", string.Empty);
			AssertCheckForRuleR424("Procedure code 51, Invalid Concession Code start with D", "51", "D01", messageError);
			AssertCheckForRuleR424("Procedure code 66, Invalid Concession Code start with D", "66", "D01", messageError);
			AssertCheckForRuleR424("Procedure code 66, Valid Concession Code C01", "66", "C01", string.Empty);
		});

		void AssertCheckForRuleR424(string testCase, ZString procedureCode, ZString concessionCode, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR424(procedureCode, concessionCode, null, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}

	public void TestCheckForRuleR480()
	{
		const string messageError = "R480 - Invalid Procedure/Add.Procedure Code combination.";

		var invalidConcessionCodes = new List<ZString>
		{
			"B01", "B02", "B03", "B04"
		}.AsReadOnly();

		var invalidProcedureCodes = new List<ZString>
		{
			"51", "61", "63"
		}.AsReadOnly();

		CombineAssertions(() =>
		{
			AssertCheckForRuleR480($"Procedure code 50, Valid Concession Code B00", "50", "B00", string.Empty);
			foreach (var concession in invalidConcessionCodes)
			{
				AssertCheckForRuleR480($"Invalid Concession Code {concession}", ZString.Empty, concession, string.Empty);
				foreach (var procedure in invalidProcedureCodes)
				{
					AssertCheckForRuleR480($"Invalid Procedure Code {procedure}", procedure, ZString.Empty, string.Empty);
					AssertCheckForRuleR480($"Procedure code {procedure}, Valid Concession Code {concession}", procedure, concession, messageError);
				}
			}
		});

		void AssertCheckForRuleR480(string testCase, ZString procedureCode, ZString concessionCode, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR480(procedureCode, concessionCode, null, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}

	public void TestCheckForRuleR859()
	{
		const string messageError = "R859 - Invalid Procedure/Add.Procedure Code combination.";

		CombineAssertions(() =>
		{
			AssertCheckForRuleR859("Procedure code start with 4", "41", ZString.Empty, string.Empty);
			AssertCheckForRuleR859("Procedure code start with 6", "61", ZString.Empty, string.Empty);
			AssertCheckForRuleR859("Procedure code 51", "51", ZString.Empty, string.Empty);
			AssertCheckForRuleR859("Procedure code 61", "71", ZString.Empty, string.Empty);
			AssertCheckForRuleR859("Invalid Concession Code 2PL", ZString.Empty, "2PL", messageError);
			AssertCheckForRuleR859("Valid Concession Code 1PL", ZString.Empty, "1PL", string.Empty);
			AssertCheckForRuleR859("Procedure code start with 4, Invalid Concession Code 2PL", "41", "2PL", string.Empty);
			AssertCheckForRuleR859("Procedure code start with 6, Invalid Concession Code 2PL", "61", "2PL", string.Empty);
			AssertCheckForRuleR859("Procedure code start with 4, Valid Concession Code 1PL", "41", "1PL", string.Empty);
			AssertCheckForRuleR859("Procedure code start with 6, Valid Concession Code 1PL", "61", "1PL", string.Empty);
			AssertCheckForRuleR859("Procedure code 51, Invalid Concession Code 2PL", "51", "2PL", messageError);
			AssertCheckForRuleR859("Procedure code 51, Valid Concession Code 1PL", "51", "1PL", string.Empty);
		});

		void AssertCheckForRuleR859(string testCase, ZString procedureCode, ZString concessionCode, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR859(procedureCode, concessionCode, null, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}

	public void TestCheckForRuleR975()
	{
		const string messageError = "R975 - Invalid Procedure/Add.Procedure Code combination.";

		CombineAssertions(() =>
		{
			AssertCheckForRuleR975("Procedure code 41", "41", ZString.Empty, string.Empty);
			AssertCheckForRuleR975("Procedure code 51", "51", ZString.Empty, string.Empty);
			AssertCheckForRuleR975("Procedure code 54", "54", ZString.Empty, string.Empty);
			AssertCheckForRuleR975("Invalid Concession Code F44", ZString.Empty, "F44", messageError);
			AssertCheckForRuleR975("Valid Concession Code F43", ZString.Empty, "F43", string.Empty);
			AssertCheckForRuleR975("Procedure code 41, Invalid Concession Code F44", "41", "F44", messageError);
			AssertCheckForRuleR975("Procedure code 51, Invalid Concession Code F44", "51", "F44", string.Empty);
			AssertCheckForRuleR975("Procedure code 54, Invalid Concession Code F44", "54", "F44", string.Empty);
			AssertCheckForRuleR975("Procedure code 41, Valid Concession Code F43", "41", "F43", string.Empty);
			AssertCheckForRuleR975("Procedure code 51, Valid Concession Code F43", "51", "F43", string.Empty);
			AssertCheckForRuleR975("Procedure code 54, Valid Concession Code F43", "54", "F43", string.Empty);
		});

		void AssertCheckForRuleR975(string testCase, ZString procedureCode, ZString concessionCode, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR975(procedureCode, concessionCode, null, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}

	public void TestCheckForRuleR1049()
	{
		const string messageError = "R1049 - Invalid Procedure/Add.Procedure Code combination.";

		var invalidConcessionCodes = new List<ZString>
		{
			"6A1", "6A2", "6A3", "6A4", "6A5", "6A6", "6A7", "6A8", "6A9",
			"7A1", "7A2", "7A3", "7A4", "7A5", "7A6", "7A7", "7A8", "7A9",
			"8A8"
		}.AsReadOnly();

		CombineAssertions(() =>
		{
			AssertCheckForRuleR1049("Invoice line is null", "12", "C01", null, string.Empty);
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			AssertCheckForRuleR1049("Procedure code 45", "45", ZString.Empty, invoiceLine, string.Empty);
			AssertCheckForRuleR1049("Procedure code 45", "68", ZString.Empty, invoiceLine, string.Empty);
			AssertCheckForRuleR1049("Invalid Procedure code 51", "51", ZString.Empty, invoiceLine, string.Empty);

			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode.CY_Code = "F06";
			AssertCheckForRuleR1049("AdditionalProcedureCode F06", "51", ZString.Empty, invoiceLine, string.Empty);

			additionalProcedureCode.CY_Code = "F05";
			AssertCheckForRuleR1049("AdditionalProcedureCode F05", "51", ZString.Empty, invoiceLine, string.Empty);

			foreach (var concession in invalidConcessionCodes)
			{
				additionalProcedureCode.CY_Code = "F06";
				AssertCheckForRuleR1049($"Procedure code 45, Concession code {concession}, AdditionalProcedureCode is 0000F06", "45", concession, invoiceLine, messageError);
				AssertCheckForRuleR1049($"Procedure code 68, Concession code {concession}, AdditionalProcedureCode is 0000F06", "68", concession, invoiceLine, messageError);
				AssertCheckForRuleR1049($"Procedure code 51, Concession code {concession}, AdditionalProcedureCode is 0000F06", "51", concession, invoiceLine, messageError);

				additionalProcedureCode.CY_Code = "F05";
				AssertCheckForRuleR1049($"Procedure code 45, Concession code {concession}, AdditionalProcedureCode is 0000F05", "45", concession, invoiceLine, messageError);
				AssertCheckForRuleR1049($"Procedure code 68, Concession code {concession}, AdditionalProcedureCode is 0000F05", "68", concession, invoiceLine, messageError);
				AssertCheckForRuleR1049($"Procedure code 51, Concession code {concession}, AdditionalProcedureCode is 0000F05", "51", concession, invoiceLine, string.Empty);
			}
		});

		void AssertCheckForRuleR1049(string testCase, ZString procedureCode, ZString concessionCode, JobComInvoiceLine invoiceLine, string expectedMessage)
		{
			var message = string.Empty;
			ProcedureCodesHelper.CheckForRuleR1049(procedureCode, concessionCode, null, invoiceLine, (p, m) => message = m);
			AssertEquals(testCase, expectedMessage, message);
		}
	}
}
