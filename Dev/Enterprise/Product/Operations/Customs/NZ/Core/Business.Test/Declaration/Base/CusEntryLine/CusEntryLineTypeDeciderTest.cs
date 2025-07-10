
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class CusEntryLineTypeDeciderTest : TestCaseWithFactory
	{
		public void TestCompletionGetsFormal()
		{
			JobDeclaration declaration = (JobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.NZ.IJobDeclaration)), TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Factory.Save();
			using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				CusEntryHeader entryHeader = declaration.CusEntryHeader;
				AssertEquals("Precondition", typeof(FormalEntry.CompletionCusEntryHeader), entryHeader.GetType());
				CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
				Factory.Save();

				BusinessObjectFactory secondFactory = new BusinessObjectFactory();
				CusEntryLine loadedEntryLine = secondFactory.Load<CusEntryLine>(entryLine.PK);
				AssertEquals(typeof(CusEntryLine), loadedEntryLine.GetType());
			}
		}

		public void TestFormalGetsFormal()
		{
			JobDeclaration declaration = (JobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.NZ.IJobDeclaration)), TestBusinessObjectKind.MinimumRequiredToSave);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			CusEntryLine loadedEntryLine = secondFactory.Load<CusEntryLine>(entryLine.PK);
			AssertEquals(typeof(CusEntryLine), loadedEntryLine.GetType());
		}

		public void TestECIGetsBase()
		{
			JobDeclaration declaration = (JobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.NZ.IJobDeclaration)), TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			CusEntryLine loadedEntryLine = secondFactory.Load<CusEntryLine>(entryLine.PK);
			AssertEquals(typeof(CusEntryLine), loadedEntryLine.GetType());
		}
	}
}
