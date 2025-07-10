using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ImportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest
	{
		public void TestPortOfArrival()
		{
			TestCaseHelper.ClearTable(RefUNLOCO.Schema.TableName);
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.Botswana;
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.Lesotho;
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.Swaziland;
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.NewWithValidTestData<RefUNLOCO>().RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			var ports = declaration.Lookups.PortOfArrivals;
			AssertEquals(1, ports.Find(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Botswana)).Count());
			AssertEquals(1, ports.Find(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Lesotho)).Count());
			AssertEquals(1, ports.Find(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Namibia)).Count());
			AssertEquals(1, ports.Find(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Swaziland)).Count());
			AssertEquals(2, ports.Find(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.SouthAfrica)).Count());
			AssertEquals(3, ports.Find(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new string[] { Core.Constants.CountryCodes.Botswana, Core.Constants.CountryCodes.Lesotho, Core.Constants.CountryCodes.Namibia, Core.Constants.CountryCodes.Swaziland, Core.Constants.CountryCodes.SouthAfrica })).Count());
		}

		public void TestMergByListBasedOnPPC()
		{
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertMergeByListDescription(discouraged: false, mergeBy: "NON", previousProcedureCode: "00", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: false, mergeBy: "NOP", previousProcedureCode: "00", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: false, mergeBy: "TRF", previousProcedureCode: "00", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: false, mergeBy: "TRD", previousProcedureCode: "00", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: false, mergeBy: "CLS", previousProcedureCode: "00", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: false, mergeBy: "CLD", previousProcedureCode: "00", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: false, mergeBy: "PNO", previousProcedureCode: "00", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: false, mergeBy: "PNP", previousProcedureCode: "00", instruction, invoiceLine);

			AssertMergeByListDescription(discouraged: false, mergeBy: "NON", previousProcedureCode: "11", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: false, mergeBy: "NOP", previousProcedureCode: "11", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: true, mergeBy: "TRF", previousProcedureCode: "11", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: true, mergeBy: "TRD", previousProcedureCode: "11", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: true, mergeBy: "CLS", previousProcedureCode: "11", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: true, mergeBy: "CLD", previousProcedureCode: "11", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: true, mergeBy: "PNO", previousProcedureCode: "11", instruction, invoiceLine);
			AssertMergeByListDescription(discouraged: true, mergeBy: "PNP", previousProcedureCode: "11", instruction, invoiceLine);
		}

		void AssertMergeByListDescription(bool discouraged, string mergeBy, string previousProcedureCode, CusEntryInstruction instruction, JobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_Procedure = "40" + previousProcedureCode;
			AssertEquals("Pre-req", previousProcedureCode, invoiceLine.JI_Calc_PreviousProcedure);
			declaration.JE_MergeBy = mergeBy;
			AssertEquals(8, declaration.Lookups.MergeByList.Count);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP", declaration.Lookups.MergeByList.CodesAsString);
			if (discouraged)
			{
				AssertContains("[Discouraged]: ", declaration.Lookups.MergeByList.GetDescriptionFromCode(mergeBy));
			}
			else
			{
				AssertNotContains("[Discouraged]: ", declaration.Lookups.MergeByList.GetDescriptionFromCode(mergeBy));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
		}
	}
}
