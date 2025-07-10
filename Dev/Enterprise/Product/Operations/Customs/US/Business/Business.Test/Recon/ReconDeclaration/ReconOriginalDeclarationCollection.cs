using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconOriginalDeclarationCollection : TestCaseWithFactory
	{
		public void TestLoadAndFind()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_DeclarationReference = "DEC1";
			declaration1.US_EntryFilerCode = "XJ5";
			declaration1.ImportEntryNumber = "ENT00001";
			var declaration1Entry = declaration1.CustomsEntryHeaders.AddNew();
			declaration1Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "DEC2";
			declaration2.US_EntryFilerCode = "XJ5";
			declaration2.ImportEntryNumber = "ENT00002";
			var declaration2Entry = declaration2.CustomsEntryHeaders.AddNew();
			declaration2Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_DeclarationReference = "DEC3";
			declaration3.US_EntryFilerCode = "XJ5";
			declaration3.ImportEntryNumber = "ENT00003";
			var declaration3Entry = declaration3.CustomsEntryHeaders.AddNew();
			declaration3Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			var factory1 = new BusinessObjectFactory();
			var reconDeclaration = new ReconDeclaration(factory1.New<JobDeclaration>());
			var entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.CH_OrigEntryReference = "XJ5ENT00003";
			AssertEquals(declaration3Entry.PK, entry1.CH_CH_OriginalEntry);
			var entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.CH_OrigEntryReference = "XJ5ENT00004";
			AssertEquals(ZGuid.Empty, entry2.CH_CH_OriginalEntry);
			var entry3 = reconDeclaration.OriginalEntries.AddNew();
			entry3.CH_OrigEntryReference = "XJ5ENT00001";
			AssertEquals(declaration1Entry.PK, entry3.CH_CH_OriginalEntry);
			var entry4 = reconDeclaration.OriginalEntries.AddNew();
			entry4.CH_OrigEntryReference = "XJ5ENT00002";
			AssertEquals(declaration2Entry.PK, entry4.CH_CH_OriginalEntry);
			var entry5 = reconDeclaration.OriginalEntries.AddNew();
			entry5.CH_OrigEntryReference = ZString.Empty;
			AssertEquals(ZGuid.Empty, entry5.CH_CH_OriginalEntry);
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			var reconDeclarationInFactory2 = new ReconDeclaration(factory2.Load<JobDeclaration>(reconDeclaration.JE_PK));
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 1);
			var originalDeclarations = reconDeclarationInFactory2.OriginalDeclarations;
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 2);
			AssertTableHit(factory2, CusEntryHeaderSchema.Constants.TableName, 2);
			AssertNotNull(originalDeclarations.Find(declaration1Entry.PK));
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 2);
			AssertTableHit(factory2, CusEntryHeaderSchema.Constants.TableName, 2);
			AssertNull(originalDeclarations.Find(ZGuid.NewZGuid()));
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 2);
			AssertTableHit(factory2, CusEntryHeaderSchema.Constants.TableName, 3);
			AssertNull(originalDeclarations.Find(ZGuid.Empty));
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 2);
			AssertTableHit(factory2, CusEntryHeaderSchema.Constants.TableName, 3);
			AssertNotNull(originalDeclarations.Find(declaration2Entry.PK));
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 2);
			AssertTableHit(factory2, CusEntryHeaderSchema.Constants.TableName, 3);
			AssertNotNull(originalDeclarations.Find(declaration3Entry.PK));
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 2);
			AssertTableHit(factory2, CusEntryHeaderSchema.Constants.TableName, 3);
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.US_EntryFilerCode = "XJ5";
			declaration4.ImportEntryNumber = "ENTA0004";
			declaration4.JE_DeclarationReference = "DEC4";
			var declaration4Entry = declaration4.CustomsEntryHeaders.AddNew();
			declaration4Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			var entry5InFactory2 = reconDeclarationInFactory2.OriginalEntries.FindEntryBy(ZString.Empty);
			AssertEquals(entry5.CH_PK, entry5InFactory2.CH_PK);
			entry5InFactory2.CH_OrigEntryReference = "XJ5ENTA0004";
			var entry5InFactory2OriginalDeclaration = entry5InFactory2.OriginalDeclaration;
			AssertEquals("DEC4", entry5InFactory2OriginalDeclaration.JE_DeclarationReference);
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 3);
			AssertTableHit(factory2, CusEntryHeaderSchema.Constants.TableName, 4);
			AssertEquals(entry5InFactory2OriginalDeclaration, originalDeclarations.Find(entry5InFactory2.CH_CH_OriginalEntry));
			AssertTableHit(factory2, JobDeclarationSchema.Constants.TableName, 3);
			AssertTableHit(factory2, CusEntryHeaderSchema.Constants.TableName, 4);
		}

		void AssertTableHit(BusinessObjectFactory factory, string tableName, int hitCount)
		{
			var tableHit = factory.TableSelects.FirstOrDefault(x => x.TableName == tableName);
			AssertEquals(tableName, hitCount, tableHit.Value);
		}
	}
}
