using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackingList))]
	sealed class CusPackingListTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			return dec.CreateCusPackingList(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.NewWithValidTestData<JobDeclaration>();
			return dec.LoadOrCreateCusPackingList(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var bo = base.GetBusinessObjectForFetchForLoad() as CusPackingList;
			var dec = bo.Factory.NewWithValidTestData<JobDeclaration>();
			bo.CUL_JE = dec.PK;
			return bo;
		}

		[ExpectNoExceptions]
		public void TestPackageJobType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			var cusPackingList = (CusPackingList)declaration.LoadOrCreateCusPackingList(Factory);
			NUnit.Framework.Assert.That(cusPackingList.PackageJob, NUnit.Framework.Is.TypeOf(typeof(CusPackageJob)));
		}

		[ExpectNoExceptions]
		public void TestPackageDescriptionHasChanges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_PackageDescription = "package desc";
			Factory.Save();
			var cusPackingList = (CusPackingList)declaration.CreateCusPackingList(Factory);
			NUnit.Framework.Assert.That(cusPackingList.EntryInstruction.CEI_PackageDescription, NUnit.Framework.Is.EqualTo("package desc").Using(CustomComparers.TypeComparison));
			cusPackingList.EntryInstruction.CEI_PackageDescription = "package desc2";
			NUnit.Framework.Assert.That(cusPackingList.HasChanges, NUnit.Framework.Is.True, "HasChanges should set to true");
		}

		[ExpectNoExceptions]
		public void TestChangePackageDescriptionOnPackingListFromInvoiceChangesRefectOnEntryInstruction()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_PackageDescription = "package desc";
			var invoice = declaration.Invoices.AddNew();
			Factory.Save();

			var cusPackingList = invoice.CreateCusPackingList(Factory);
			NUnit.Framework.Assert.That(cusPackingList.EntryInstruction.CEI_PackageDescription, NUnit.Framework.Is.EqualTo("package desc").Using(CustomComparers.TypeComparison));
			cusPackingList.EntryInstruction.CEI_PackageDescription = "package desc2";
			Factory.Save();
			NUnit.Framework.Assert.That(entryInstruction.CEI_PackageDescription, NUnit.Framework.Is.EqualTo("package desc2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChangePackageDescriptionOnPackingListFromDeclarationChangesRefectOnEntryInstruction()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_PackageDescription = "package desc";
			Factory.Save();

			var cusPackingList = (CusPackingList)declaration.CreateCusPackingList(Factory);
			NUnit.Framework.Assert.That(cusPackingList.EntryInstruction.CEI_PackageDescription, NUnit.Framework.Is.EqualTo("package desc").Using(CustomComparers.TypeComparison));
			cusPackingList.EntryInstruction.CEI_PackageDescription = "package desc2";
			Factory.Save();
			NUnit.Framework.Assert.That(entryInstruction.CEI_PackageDescription, NUnit.Framework.Is.EqualTo("package desc2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackageDescriptionWhenCusPackingListInvoiceHasNoDeclaration()
		{
			var invoiceWithoutDeclaration = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			Factory.Save();
			var cusPackingList = invoiceWithoutDeclaration.CreateCusPackingList(Factory);
			NUnit.Framework.Assert.That(cusPackingList.EntryInstruction.CEI_PackageDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestEntryInstructionThroughDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;

			var cusPackingList = (CusPackingList)declaration.CreateCusPackingList(Factory);
			NUnit.Framework.Assert.That(cusPackingList.EntryInstruction.PK, NUnit.Framework.Is.EqualTo(entryInstruction.PK));
		}

		[ExpectNoExceptions]
		public void TestEntryInstructionThroughInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();

			var cusPackingList = invoice.CreateCusPackingList(Factory);
			NUnit.Framework.Assert.That(cusPackingList.EntryInstruction.PK, NUnit.Framework.Is.EqualTo(entryInstruction.PK));
		}

		[ExpectNoExceptions]
		public void TestDocumentSupporter()
		{
			var cusPackingList = Factory.New<CusPackingList>();
			NUnit.Framework.Assert.That(cusPackingList.DocumentSupporter, NUnit.Framework.Is.TypeOf<CusPackingListDocumentSupporter>());
		}
	}
}
