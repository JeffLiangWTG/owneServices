using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ZAJobComInvoiceLineDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCopyJI_CEI()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariff1P1 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "07101202", Universal.Constants.RateTypes.Duty, taxOrFeeCode: "VAT");
			var tariff12A = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "1051004", Universal.Constants.RateTypes.Duty);
			helper.CreateTariffRelationship(tariff12A.PK, tariff1P1.ZZ1_ZZI_TariffType, tariff1P1.ZZ1_TariffCode);
			helper.CreateRefCusProcedure("ZA", "A", "XX1", "", "", "XX1_3", "IMP");
			Factory.Save();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "XX1";
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = tariff1P1.ZZ1_TariffCode;
			Assert(invoiceLine.CusLineTariffDetails.Any());
			AssertEquals("Defaulted tariff", tariff12A.ZZ1_TariffCode, invoiceLine.CusLineTariffDetails[0].BZ_Tariff);
			var clonedDeclaration = (JobDeclaration)new ZAJobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			var cloneInstruction = clonedDeclaration.CustomsEntryInstructions[0];
			var clonedInvoiceLine = clonedDeclaration.InvoiceLines[0];
			var clonedTariffDetail = clonedInvoiceLine.CusLineTariffDetails;
			AssertEquals("EntryInstruction", cloneInstruction.PK, clonedInvoiceLine.JI_CEI);
			AssertEquals("CusLineTariffDetails Copy - Count", 1, clonedTariffDetail.Count);
		}

		public void TestCopyCusLineTariffDetail()
		{
			invoiceLine.JI_Description = "TestDescription";
			var additionalDuty = invoiceLine.CusLineTariffDetails.AddNew("12A", "111#");
			var clonedDeclaration = (JobDeclaration)new ZAJobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			var clonedTariffDetail = clonedDeclaration.InvoiceLines[0].CusLineTariffDetails;
			AssertEquals("CusLineTariffDetails Copy - Count", 1, clonedTariffDetail.Count);
			AssertEquals("CusLineTariffDetails Copy - AdditionalDuty Copied", "12A", clonedTariffDetail[0].BZ_Type);
			AssertEquals("CusLineTariffDetails Copy - AdditionalDuty Copied", "111#", clonedTariffDetail[0].BZ_Tariff);
		}

		public void TestSetEntryInstructionForJobComInvoiceLine_ShouldNotThrowExceptionWhenEntryInstructionNotProvided()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var cloneStrategy1 = new ZAJobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, new Dictionary<ZString, Dictionary<ZGuid, ZGuid>>());
			var cloneInvoiceLine1 = (BaseJobComInvoiceLine)cloneStrategy1.Clone();
			Assert(cloneInvoiceLine1.JI_CEI.IsEmpty);
			var cloneStrategy2 = new ZAJobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, new Dictionary<ZString, Dictionary<ZGuid, ZGuid>>
			{
				{
					JobDeclarationDeepCloneStrategy.CusEntryInstructionPKPairsKey,
					new Dictionary<ZGuid, ZGuid>
					{
						{
							entryInstruction.PK,
							entryInstruction.PK
						}
					}
				}
			});
			var cloneInvoiceLine2 = (BaseJobComInvoiceLine)cloneStrategy2.Clone();
			AssertEquals(entryInstruction.PK, cloneInvoiceLine2.JI_CEI);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}
	}
}
