using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AESDeclaration))]
	sealed class AESDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIAESDeclarationMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry1.US_IsDeactivated = true;
			AssertEquals(true, entry1.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry1.HasBeenWithdrawn);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals(true, entry2.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry2.HasBeenWithdrawn);

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals(true, entry3.HasBeenLodgedAtCustoms);
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			entry3.US_IsDeactivated = true;
			AssertEquals(true, entry3.HasBeenWithdrawn);

			var entry4 = declaration.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry4.CH_Status = AESDirectCustomsEntryStatus.Codes.NotSent;
			AssertEquals(false, entry4.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry4.HasBeenWithdrawn);

			Factory.Save();

			IAESDeclaration aes = new AESDeclaration(declaration);
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			AssertEquals(messageInitiator, aes.MessageInitiator);
			AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced));
			aes.LogCustomsCommencedIfNeeded();
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced, GetBranchQuery()));
			AssertEquals(3, aes.ActiveEntryHeaders.Count());
			AssertCollectionContains("Deactivated lodged entry", entry1, aes.ActiveEntryHeaders);
			AssertCollectionContains("entry for amendment", entry2, aes.ActiveEntryHeaders);
			AssertCollectionContains("not lodged entry", entry4, aes.ActiveEntryHeaders);
		}

		public ZQuery GetBranchQuery()
		{
			return new ZQuery(StmALogSchema.SL_GB_NKBranch, GlbCompany.CurrentCompany.ActiveBranches.Cast<GlbBranch>().Select(x => x.GB_Code));
		}

		public void TestEntriesAreValidatedWhenLoaded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry1.CH_BGMReference = "BGM1";
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry1.US_IsDeactivated = true;
			AssertEquals(true, entry1.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry1.HasBeenWithdrawn);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry2.CH_BGMReference = "BGM2";
			entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals(true, entry2.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry2.HasBeenWithdrawn);

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry3.CH_BGMReference = "BGM3";
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals(true, entry3.HasBeenLodgedAtCustoms);
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			entry3.US_IsDeactivated = true;
			AssertEquals(true, entry3.HasBeenWithdrawn);

			var entry4 = declaration.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry4.CH_BGMReference = "BGM4";
			entry4.CH_Status = AESDirectCustomsEntryStatus.Codes.NotSent;
			AssertEquals(false, entry4.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry4.HasBeenWithdrawn);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declarationInDifferentFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			var aes = new AESDeclaration(declarationInDifferentFactory);
			var entries = aes.Entries.ToArray<CusEntryHeader>();
			AssertEquals(3, entries.Length);
			entry1 = entries.FirstOrDefault(x => x.CH_BGMReference == "BGM1");
			AssertHasWarning(entry1.CH_BGMReferenceInfo, ValidationConstants.AES.DeactivatedEntryWithActiveCustomsTransaction);
			entry2 = entries.FirstOrDefault(x => x.CH_BGMReference == "BGM2");
			AssertNoNotifications(entry2.CH_BGMReferenceInfo);
			entry3 = entries.FirstOrDefault(x => x.CH_BGMReference == "BGM3");
			AssertNull("No ne", entry3);
			entry4 = entries.FirstOrDefault(x => x.CH_BGMReference == "BGM4");
			AssertNoNotifications(entry4.CH_BGMReferenceInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			return new AESDeclaration(declaration);
		}
	}
}
