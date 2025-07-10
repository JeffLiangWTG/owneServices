using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCollection))]
	sealed class CusEntryHeaderCollectionTest : Customs.Business.Testing.CusEntryHeaderCollectionTest
	{
		sealed class JobDeclarationForTesting : JobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public int DeriveExportDeclarationStatusCalledCount { get; set; }

			protected override void DeriveExportDeclarationStatus()
			{
				DeriveExportDeclarationStatusCalledCount++;
			}
		}

		public void TestDeriveExportDirectStatusWhenCountChanged()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var collection = declaration.CustomsEntryHeaders;
			var entry = Factory.New<CusEntryHeader>();
			collection.Add(entry);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			_ = collection.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			ZGuid decPK = declaration.PK;
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclarationForTesting>(decPK);
			declaration.DeriveExportDeclarationStatusCalledCount = 0;   // Reset count.
			_ = declaration.CustomsEntryHeaders;
			AssertEquals($"DeriveExportDeclarationStatus should never be called.", 0, declaration.DeriveExportDeclarationStatusCalledCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryHeaderCollection(Factory.New<JobDeclaration>(), Factory);
	}
}
