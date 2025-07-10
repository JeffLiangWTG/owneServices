using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportCusEntryLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRuleR605()
	{
		const string expectedMessageError = "(R605) Duty/Tax calculations are missing";
		const string import = JobMessageTypeList.Codes.Import;
		const string export = JobMessageTypeList.Codes.Export;
		const bool hasError = true;
		const bool noError = false;

		var testCases = new (ZString, ZString, ZString, ZString, ZBool, ZBool)[]
		{
			("Without duty calculations, customs procedure, procedure details code", import, ZString.Empty, ZString.Empty, false, hasError),
			("Customs procedure = 71", import, ProcedureCodes._71, ZString.Empty, false, noError),
			("Customs procedure = 76", import, ProcedureCodes._76, ZString.Empty, false, noError),
			("Customs procedure != 71 or 76", import, ProcedureCodes._10, ZString.Empty, false, hasError),
			("Procedure details code = 2PL", import, ZString.Empty, ConcessionCodes._2PL, false, noError),
			("Procedure details code != 2PL", import, ZString.Empty, ConcessionCodes._0V1, false, hasError),
			("Duty calculation exists", import, ZString.Empty, ZString.Empty, true, noError),

			("Without duty calculations, customs procedure, procedure details code", export, ZString.Empty, ZString.Empty, false, noError),
			("Customs procedure = 71", export, ProcedureCodes._71, ZString.Empty, false, noError),
			("Customs procedure != 71 or 76", export, ProcedureCodes._10, ZString.Empty, false, noError),
			("Procedure details code = 2PL", export, ZString.Empty, ConcessionCodes._2PL, false, noError),
			("Procedure details code != 2PL", export, ZString.Empty, ConcessionCodes._0V1, false, noError),
			("Duty calculation exists", export, ZString.Empty, ZString.Empty, true, noError),
		};

		CombineAssertions(() =>
		{
			foreach (var testCase in testCases)
			{
				var (description, messageType, procCode, additionalProcCode, createDuty, expectError) = testCase;

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = messageType;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Procedure = procCode;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var procedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();
				procedureCode.CY_Code = additionalProcCode;

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				var entryHeader = declaration.CustomsEntryHeaders.First();
				var entryLine = entryHeader.AllEntryLines.First();
				if (createDuty)
				{
					var fees = entryLine.Fees.AddNew();
				}

				entryLine.Validation.ValidateAll();

				if (expectError)
				{
					AssertHasRowMessageError($"{messageType} - {description}", entryLine, expectedMessageError);
				}
				else
				{
					AssertNoRowMessageError($"{messageType} - {description}", entryLine, expectedMessageError);
				}
			}
		});
	}
}
