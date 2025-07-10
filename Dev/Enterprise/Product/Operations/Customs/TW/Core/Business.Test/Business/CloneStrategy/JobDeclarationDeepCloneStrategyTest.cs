using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobDeclarationDeepCloneStrategyTest : Customs.Business.Testing.JobDeclarationDeepCloneStrategyAbstractTest<JobDeclaration>
	{
		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetJobDeclarationDeepCloneStrategyToTest(JobDeclaration declarationToClone, Customs.Business.CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			return new JobDeclarationDeepCloneStrategy(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
		}

		protected override ZString AddInfoTestData => "OtherBankAccount=11";

		[ExpectNoExceptions]
		public void TestCloneInvoiceLineJI_DeclarationGoodsDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_DeclarationGoodsDescription = "XXX";
			var clonedDec = (JobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines[0].JI_DeclarationGoodsDescription, NUnit.Framework.Is.EqualTo("XXX").Using(CustomComparers.TypeComparison));
			var clonedDec2 = (JobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines[0].JI_DeclarationGoodsDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCloneJI_Group()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Group = "ZZZZ";
			Factory.Save();
			var clonedDec = (JobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines[0].JI_Group, NUnit.Framework.Is.EqualTo("ZZZZ").Using(CustomComparers.TypeComparison));
			var clonedDec2 = (JobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines[0].JI_AddInfo, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines[0].JI_NAddInfo, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCloneCusEntryInstructionTW_TradersRemarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.TW_TradersRemarks = "AABBCC";
			var clonedDec = (JobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.CusEntryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("AABBCC").Using(CustomComparers.TypeComparison));
			var clonedDec2 = (JobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.CusEntryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}
	}
}
