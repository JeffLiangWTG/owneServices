using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class ImportAdditionalProcedureCodeValidationTest : CusCodeDataValidationTest
{
	public void TestCheckForRuleR407()
	{
		const string messageError = "R407 - Invalid Procedure/Add.Procedure Code combination.";

		var itemList = new List<ZString>
		{ "C01", "C02", "C03", "C04",
			"C06", "C07", "C08", "C09", "C10", "C11", "C12", "C13", "C14", "C15", "C16", "C17", "C18", "C19", "C20", "C21", "C22", "C23", "C24", "C25", "C26", "C27", "C28", "C29", "C30",
			"C31", "C32", "C33", "C34", "C35", "C36", "C37", "C38", "C39", "C40", "C41", "B02", "B03", "F01", "F02", "F03", "F21", "F22", "3PL", "4PL", "5PL", "6PL", "7PL", "1C1" };
		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			foreach (var item in itemList)
			{
				line.JI_Procedure = "4100";
				additionalProcedure.CY_Code = $"{item}";
				AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

				line.JI_Procedure = "6100";
				additionalProcedure.CY_Code = $"{item}";
				AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

				line.JI_Procedure = "5100";
				additionalProcedure.CY_Code = $"{item}";
				AssertHasMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);
			}
		});
	}

	public void TestCheckForRuleR414()
	{
		const string messageError = "R414 - Invalid Procedure/Add.Procedure Code combination.";

		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			line.JI_Procedure = "4800";
			additionalProcedure.CY_Code = $"B07";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			line.JI_Procedure = "5100";
			additionalProcedure.CY_Code = $"B07";
			AssertHasMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			additionalProcedure.CY_Code = $"B06";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR419()
	{
		const string messageError = "R419 - Invalid CN Code/Add.Procedure Code C01.";

		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			line.JI_Tariff = "9905";
			additionalProcedure.CY_Code = $"C01";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			line.JI_Tariff = "9904";
			additionalProcedure.CY_Code = $"C01";
			AssertHasMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			additionalProcedure.CY_Code = $"C02";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR421()
	{
		const string messageError = "R421 - Invalid Procedure/Add.Procedure Code combination.";

		var itemList = new List<ZString> { "C02", "C03", "C04", "C06", "C41", "C20", "C26" };

		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			foreach (var item in itemList)
			{
				line.JI_Tariff = "9919";
				additionalProcedure.CY_Code = $"{item}";
				AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

				line.JI_Tariff = "9918";
				additionalProcedure.CY_Code = $"{item}";
				AssertHasMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);
			}
		});
	}

	public void TestCheckForRuleR424()
	{
		const string messageError = "R424 - Invalid Procedure/Add.Procedure Code combination.";

		var itemList = new List<ZString> { "4100", "5100", "6100", "7100" };

		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;
		foreach (var item in itemList)
		{
			line.JI_Procedure = item;
			additionalProcedure.CY_Code = $"A31";
			AssertNoMessageError(additionalProcedure.CY_CodeInfo, messageError);

			additionalProcedure.CY_Code = $"D51";
			AssertHasMessageError(additionalProcedure.CY_CodeInfo, messageError);
		}
	}

	public void TestCheckForRuleR480()
	{
		const string messageError = "R480 - Invalid Procedure/Add.Procedure Code combination.";

		var additionalProcedureItemList = new List<ZString> { "B01", "B02", "B03", "B04" };
		var currentCusProcedureItemList = new List<ZString> { "5100", "6100", "6300" };

		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			foreach (var additionalProcedureItem in additionalProcedureItemList)
			{
				foreach (var currentCusProcedureItem in currentCusProcedureItemList)
				{
					line.JI_Procedure = currentCusProcedureItem;
					additionalProcedure.CY_Code = $"{additionalProcedureItem}";
					AssertHasMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

					additionalProcedure.CY_Code = $"B00";
					AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);
				}
			}
		});
	}

	public void TestCheckForRuleR859()
	{
		const string messageError = "R859 - Invalid Procedure/Add.Procedure Code combination.";

		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			line.JI_Procedure = "4100";
			additionalProcedure.CY_Code = $"2PL";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			line.JI_Procedure = "6100";
			additionalProcedure.CY_Code = $"2PL";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			line.JI_Procedure = "5100";
			additionalProcedure.CY_Code = $"2PL";
			AssertHasMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR975()
	{
		const string messageError = "R975 - Invalid Procedure/Add.Procedure Code combination.";

		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			line.JI_Procedure = "0051";
			additionalProcedure.CY_Code = $"F44";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			line.JI_Procedure = "0054";
			additionalProcedure.CY_Code = $"F44";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			line.JI_Procedure = "0050";
			additionalProcedure.CY_Code = $"F44";
			AssertHasMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR1049()
	{
		const string messageError = "R1049 - Invalid Procedure/Add.Procedure Code combination.";

		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			line.JI_Procedure = "6800";
			additionalProcedure.CY_Code = $"F06";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			line.JI_Procedure = "4500";
			additionalProcedure.CY_Code = $"F06";
			AssertNoMessageError(additionalProcedure.CY_Code, additionalProcedure.CY_CodeInfo, messageError);

			var additionalProcedure2 = line.AdditionalProcedureCodes.AddNew();
			var itemList = new List<ZString> { "6A1", "6A2", "6A3", "6A4", "6A5", "6A6", "6A7", "6A8", "6A9", "7A1", "7A2", "7A3", "7A4", "7A5", "7A6", "7A7", "7A8", "7A9", "8A8" };
			foreach (var item in itemList)
			{
				line.JI_Procedure = "4500";
				additionalProcedure.CY_Code = $"F06";
				additionalProcedure2.CY_Code = $"{item}";
				AssertHasMessageError(additionalProcedure2.CY_Code, additionalProcedure2.CY_CodeInfo, messageError);

				line.JI_Procedure = "6800";
				additionalProcedure2.CY_Code = $"{item}";
				AssertHasMessageError(additionalProcedure2.CY_Code, additionalProcedure2.CY_CodeInfo, messageError);

				additionalProcedure.CY_Code = $"F05";
				additionalProcedure2.CY_Code = $"{item}";
				AssertHasMessageError(additionalProcedure2.CY_Code, additionalProcedure2.CY_CodeInfo, messageError);

				line.JI_Procedure = "3300";
				additionalProcedure2.CY_Code = $"{item}";
				AssertNoMessageError(additionalProcedure2.CY_Code, additionalProcedure2.CY_CodeInfo, messageError);
			}

			additionalProcedure2.CY_Code = $"6A0";
			AssertNoMessageError(additionalProcedure2.CY_Code, additionalProcedure2.CY_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR1583()
	{
		const string messageError = "(R1583) If a 2PL additional procedure code is specified, one of procedure codes C30, C31, C32, C34, C35, C36 is also required.";
		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty additional procedure", additionalProcedure.CY_CodeInfo, messageError);

			additionalProcedure.CY_Code = Constants.ConcessionCodes._2PL;
			AssertHasMessageError("2PL additional procedure with no additional concession codes", additionalProcedure.CY_CodeInfo, messageError);

			var additionalProcedure2 = line.AdditionalProcedureCodes.AddNew();
			additionalProcedure2.CY_Code = "AAA";
			additionalProcedure.Validation.ValidateCY_Code();
			AssertHasMessageError("Additional procedure 2PL with No C30, C31, C32, C34, C35, C36", additionalProcedure.CY_CodeInfo, messageError);

			var r1583concessionCodesList = new List<ZString>() { "C30", "C31", "C32", "C34", "C35", "C36" };
			foreach (var concessionCode in r1583concessionCodesList)
			{
				additionalProcedure2.CY_Code = concessionCode;
				additionalProcedure.Validation.ValidateCY_Code();
				AssertNoMessageError($"Additional procedure 2PL with {concessionCode}", additionalProcedure.CY_CodeInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR1584()
	{
		const string messageError = "(R1584) If a 2PL additional procedure code is specified, procedure codes C07 and C08 are not allowed.";
		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty additional procedure", additionalProcedure.CY_CodeInfo, messageError);

			additionalProcedure.CY_Code = Constants.ConcessionCodes._2PL;
			AssertNoMessageError("2PL additional procedure with no additional concession codes", additionalProcedure.CY_CodeInfo, messageError);

			var additionalProcedure2 = line.AdditionalProcedureCodes.AddNew();
			additionalProcedure2.CY_Code = "AAA";
			additionalProcedure.Validation.ValidateCY_Code();
			AssertNoMessageError("Additional procedure 2PL with No C07, C08", additionalProcedure2.CY_CodeInfo, messageError);

			additionalProcedure2.CY_Code = "C07";
			additionalProcedure.Validation.ValidateCY_Code();
			AssertHasMessageError("Additional procedure 2PL with C07", additionalProcedure2.CY_CodeInfo, messageError);

			additionalProcedure2.CY_Code = "C08";
			additionalProcedure.Validation.ValidateCY_Code();
			AssertHasMessageError("Additional procedure 2PL with C08", additionalProcedure2.CY_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR1590()
	{
		const string messageError = "(R1590) Procedure details code C07 or Fiscal role code FR5 is required.";
		var additionalProcedure = GetAdditionalProcedureCode();
		var line = additionalProcedure.ParentAsJobComInvoiceLine;
		var additionalProcedure1 = line.AdditionalProcedureCodes.AddNew();
		additionalProcedure.CY_Type = EU.Business.CusCodeDataTypeList.Codes.AdditionalProcedureCode;
		additionalProcedure.CY_Code = "F48";
		additionalProcedure1.CY_Type = EU.Business.CusCodeDataTypeList.Codes.AdditionalProcedureCode;
		additionalProcedure1.CY_Code = Constants.ConcessionCodes.C07;
		var entryInstruction = line.Declaration.CustomsEntryInstructions.AddNew();
		line.JI_CEI = entryInstruction.PK;
		var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
		cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;

		CombineAssertions(() =>
		{
			additionalProcedure.Validation.ValidateCY_Code();
			AssertNoMessageError("There is Code FR5 or C07 when this line have a code F48", additionalProcedure.CY_CodeInfo, messageError);

			line.JI_CEI = ZGuid.Empty;
			additionalProcedure.Validation.ValidateCY_Code();
			AssertNoMessageError("Entry Instruction is null so there is no Code FR5 but code C07 exists when this line have a code F48", additionalProcedure.CY_CodeInfo, messageError);

			additionalProcedure1.CY_Code = "C06";
			additionalProcedure.Validation.ValidateCY_Code();
			AssertHasMessageError("There is no Code FR5 or C07 when this line have a code F48", additionalProcedure.CY_CodeInfo, messageError);
		});
	}

	AdditionalProcedureCode GetAdditionalProcedureCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		return invoiceLine.AdditionalProcedureCodes.AddNew();
	}
}
