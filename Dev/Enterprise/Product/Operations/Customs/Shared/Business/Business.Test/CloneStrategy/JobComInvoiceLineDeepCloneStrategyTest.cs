using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public class JobComInvoiceLineDeepCloneStrategyTest : TestCaseWithFactory
	{
		protected virtual ZString AddInfoTestData => "XXX";

		public void TestCloneInvoiceLine()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			BaseCusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_AddInfo = AddInfoTestData;
			invoiceLine.JI_Tariff = "0000";
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CL = ZGuid.NewZGuid();
			invoiceLine.JI_PartNo = "TEST";
			invoiceLine.JI_OP = ZGuid.NewZGuid();
			invoiceLine.JI_CL = ZGuid.NewZGuid();
			var containerPivot = invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CONT2");
			containerPivot.IsForInvoiceLine = true;
			containerPivot.GrossWeightInKG = 10m;
			containerPivot.NetWeightInKG = 11m;
			containerPivot.PackQty = 2;
			containerPivot.SplitValue = 3m;

			BaseJobDeclaration clonedDec = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals("one invoice", 1, clonedDec.Invoices.Count);
			AssertEquals("one invoice line", 1, clonedDec.InvoiceLines.Count);
			AssertEquals(AddInfoTestData, clonedDec.InvoiceLines[0].JI_AddInfo);
			AssertEquals("0000", clonedDec.InvoiceLines[0].JI_Tariff);
			AssertEquals("NO", clonedDec.InvoiceLines[0].JI_CustomsUnitQty);
			AssertEquals(ZGuid.Empty, clonedDec.InvoiceLines[0].JI_CL);
			AssertEquals(invoiceLine.JI_PartNo, clonedDec.InvoiceLines[0].JI_PartNo);
			AssertEquals(invoiceLine.JI_OP, clonedDec.InvoiceLines[0].JI_OP);
			AssertEquals(clonedDec.Invoices[0].PK, clonedDec.InvoiceLines[0].JI_JZ);
			AssertEquals(ZGuid.Empty, clonedDec.InvoiceLines[0].JI_CL);
			AssertEquals(0, clonedDec.InvoiceLines[0].ContainersPivot.Count);

			BaseJobDeclaration clonedDec2 = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy).Clone();
			AssertEquals("one invoice", 1, clonedDec2.Invoices.Count);
			AssertEquals("one invoice line", 1, clonedDec2.InvoiceLines.Count);
			AssertEquals("", clonedDec2.InvoiceLines[0].JI_AddInfo);
			AssertEquals("", clonedDec2.InvoiceLines[0].JI_Tariff);
			AssertEquals("", clonedDec2.InvoiceLines[0].JI_CustomsUnitQty);
			AssertEquals(ZGuid.Empty, clonedDec2.InvoiceLines[0].JI_CL);
			AssertEquals(invoiceLine.JI_PartNo, clonedDec2.InvoiceLines[0].JI_PartNo);
			AssertEquals(ZGuid.Empty, clonedDec2.InvoiceLines[0].JI_OP);
			AssertEquals(clonedDec2.Invoices[0].PK, clonedDec2.InvoiceLines[0].JI_JZ);
			AssertEquals(ZGuid.Empty, clonedDec2.InvoiceLines[0].JI_CL);
			AssertEquals(1, clonedDec2.InvoiceLines[0].ContainersPivot.Count);
			NonPersistentCusContainer clonedPivot = clonedDec2.InvoiceLines[0].ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CONT2");
			AssertNotNull(clonedPivot);
			AssertEquals(true, clonedPivot.IsForInvoiceLine);
			AssertEquals(10m, clonedPivot.GrossWeightInKG);
			AssertEquals(11m, clonedPivot.NetWeightInKG);
			AssertEquals(2, clonedPivot.PackQty);
			AssertEquals(3m, clonedPivot.SplitValue);
		}

		public void TestSetEntryInstructionForJobComInvoiceLine_ShouldNotThrowExceptionWhenEntryInstructionNotProvided()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var cloneStrategy1 = new JobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, new Dictionary<ZString, Dictionary<ZGuid, ZGuid>>());
			var cloneInvoiceLine1 = (BaseJobComInvoiceLine)cloneStrategy1.Clone();
			Assert(cloneInvoiceLine1.JI_CEI.IsEmpty);

			var cloneStrategy2 = new JobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, new Dictionary<ZString, Dictionary<ZGuid, ZGuid>>
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
	}
}
