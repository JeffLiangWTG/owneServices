using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconEntryLine))]
	class CusReconEntryLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusReconEntryLineTypeDecider>(CusReconEntryLine.TypeDecider);
		}

		public void TestCRL_LineNumber_Caption()
		{
			AssertEquals("Line Number", DataBoundResourceStrings.GetDataForProperty(reconEntryLine.CRL_LineNumberInfo).Caption);
		}

		public void TestCRL_OriginalEntryLineNumber_Caption()
		{
			AssertEquals("Invoice Line", DataBoundResourceStrings.GetDataForProperty(reconEntryLine.CRL_OriginalEntryLineNumberInfo).Caption);
		}

		public void TestCRL_Description_Caption()
		{
			AssertEquals("Description", DataBoundResourceStrings.GetDataForProperty(reconEntryLine.CRL_DescriptionInfo).Caption);
		}

		public void TestCRL_CustomsStatus_Caption()
		{
			AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(reconEntryLine.CRL_CustomsStatusInfo).Caption);
		}

		public void TestCusReconSnapshots()
		{
			var entryLineSnapshots = reconEntryLine.CusReconSnapshots;
			var snapshot1 = entryLineSnapshots.AddNew();
			var snapshot2 = entryLineSnapshots.AddNew();
			reconEntry.CusReconSnapshots.AddNew();

			CombineAssertions(() =>
			{
				AssertType<CusReconSnapshotCollection>("Type", entryLineSnapshots);
				AssertEquals("IsRegisteredEditableChildObject", true, reconEntryLine.IsRegisteredEditableChildObject(entryLineSnapshots));
				AssertContainsExactElementsInAnyOrder("Elements", new[] { snapshot1.PK, snapshot2.PK }, entryLineSnapshots.Select(x => x.PK));
			});
		}

		public void TestCusReconSnapshotsAreDeleted()
		{
			var snapshot1 = reconEntryLine.CusReconSnapshots.AddNew();
			var snapshot2 = reconEntryLine.CusReconSnapshots.AddNew();

			var reconEntryLine2 = reconEntry.CusReconEntryLines.AddNew();
			var snapshot3 = reconEntryLine2.CusReconSnapshots.AddNew();
			var snapshot4 = reconEntry.CusReconSnapshots.AddNew();

			reconEntryLine.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Line is deleted", true, reconEntryLine.IsDeleted);
				AssertEquals("Snapshot1 from deleted line", true, snapshot1.IsDeleted);
				AssertEquals("Snapshot2 from deleted line", true, snapshot2.IsDeleted);
				AssertEquals("Sibling line is not deleted", false, reconEntryLine2.IsDeleted);
				AssertEquals("Snapshot from non-deleted sibling line", false, snapshot3.IsDeleted);
				AssertEquals("Parent entry is not deleted", false, reconEntry.IsDeleted);
				AssertEquals("Snapshot from non-deleted parent entry", false, snapshot4.IsDeleted);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusReconEntryLine(factory);

		CusReconEntryLine GetCusReconEntryLine(BusinessObjectFactory factory)
		{
			var address = factory.NewWithValidTestData<OrgAddress>();
			factory.Save();

			var declaration = factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = address.PK;
			var entryLine = factory.New<CusReconEntryLine>();
			entryLine.CRL_LineNumber = 1;
			entryLine.CRL_CustomsStatus = "AA";
			entryLine.CRL_Description = "AA";
			entryLine.CRL_OriginalEntryLineNumber = 1;
			entryLine.CRL_CRE = reconEntry.PK;
			return entryLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			reconEntry = Factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = address.PK;
			reconEntryLine = reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_LineNumber = 1;
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
		}
		CusReconEntry reconEntry;
		CusReconEntryLine reconEntryLine;
	}
}
