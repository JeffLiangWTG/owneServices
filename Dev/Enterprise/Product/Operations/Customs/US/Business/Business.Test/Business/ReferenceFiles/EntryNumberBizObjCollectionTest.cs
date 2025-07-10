using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryNumberBizObjCollection))]
	sealed class EntryNumberBizObjCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryNumberBizObjCollection>
	{
		public void TestSetDefaultsForNewChild()
		{
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "SV9";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);

			var entryNumbers = GetCollectionToTest();
			var entryNumber1 = entryNumbers.AddNew();
			AssertEquals("SV9", entryNumber1.EntryFilerCode);

			entryFiler.EntryFilerCode = "SV8";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);

			var entryNumber2 = entryNumbers.AddNew();
			AssertEquals("SV8", entryNumber2.EntryFilerCode);
		}

		protected override EntryNumberBizObjCollection GetCollectionToTest() => new EntryNumberBizObjCollection(queryData);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EntryNumberBizObj(queryData);

		EntrySummaryQueryBizObj queryData;

		protected override void SetUp()
		{
			base.SetUp();
			queryData = new EntrySummaryQueryBizObj(Factory);
		}
	}
}
