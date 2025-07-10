using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconSnapshot))]
	class CusReconSnapshotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusReconSnapshotTypeDecider>(CusReconSnapshot.TypeDecider);
		}

		public void TestSupportsClone()
		{
			AssertEquals(true, GetCusReconSnapshot(Factory).SupportsClone());
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusReconSnapshot(factory);

		CusReconSnapshot GetCusReconSnapshot(BusinessObjectFactory factory)
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
			var reconEntryLine = reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
			var reconEntrySnapshot = factory.New<CusReconSnapshot>();
			reconEntrySnapshot.CRS_CRL_Line = reconEntryLine.PK;
			return reconEntrySnapshot;
		}
	}

	[TestedType(typeof(CusReconSnapshot.Loader))]
	class CusReconSnapshotLoaderTest : LoaderTestCase
	{
		public void TestGetSingleCusReconSnapshot_CusReconEntry()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			snapshot.CRS_CRE_Entry = reconEntry.PK;
			snapshot.CRS_Type = "LDG";
			AssertEquals(snapshot, CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", reconEntry));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryInvalid()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			var reconEntry2 = Factory.New<CusReconEntry>();
			snapshot.CRS_CRE_Entry = reconEntry.PK;
			snapshot.CRS_Type = "LDG";
			AssertNull(CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", reconEntry2));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntry_InvalidType()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			snapshot.CRS_CRE_Entry = reconEntry.PK;
			snapshot.CRS_Type = "LDG";
			AssertNull(CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "CUR", reconEntry));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntry_EmptyType()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			snapshot.CRS_CRE_Entry = reconEntry.PK;
			snapshot.CRS_Type = "LDG";
			AssertNull(CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, ZString.Empty, reconEntry));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntry_NullFactory()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			snapshot.CRS_CRE_Entry = reconEntry.PK;
			snapshot.CRS_Type = "LDG";
			AssertExceptionThrown<ArgumentNullException>(() => CusReconSnapshot.Loader.GetSingleCusReconSnapshot(null, "LDG", reconEntry));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryNull()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			snapshot.CRS_CRE_Entry = reconEntry.PK;
			snapshot.CRS_Type = "LDG";
			AssertExceptionThrown<ArgumentNullException>(() => CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", (CusReconEntry)null));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntry_MultipleSnapshots()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			snapshot.CRS_CRE_Entry = reconEntry.PK;
			snapshot.CRS_Type = "LDG";
			var snapshot2 = Factory.New<CusReconSnapshot>();
			snapshot2.CRS_CRE_Entry = reconEntry.PK;
			snapshot2.CRS_Type = "LDG";
			AssertEquals(snapshot, CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", reconEntry));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntry_NoSnapshots()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			snapshot.CRS_Type = "LDG";
			AssertNull(CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", reconEntry));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryLine()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			snapshot.CRS_CRL_Line = reconEntryLine.PK;
			snapshot.CRS_Type = "LDG";
			AssertEquals(snapshot, CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", reconEntryLine));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryLineInvalid()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			var reconEntryLine2 = Factory.New<CusReconEntryLine>();
			snapshot.CRS_CRL_Line = reconEntryLine.PK;
			snapshot.CRS_Type = "LDG";
			AssertNull(CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", reconEntryLine2));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryLine_InvalidType()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			snapshot.CRS_CRL_Line = reconEntryLine.PK;
			snapshot.CRS_Type = "LDG";
			AssertNull(CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "CUR", reconEntryLine));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryLine_EmptyType()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			snapshot.CRS_CRL_Line = reconEntryLine.PK;
			snapshot.CRS_Type = "LDG";
			AssertNull(CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, ZString.Empty, reconEntryLine));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryLine_NullFactory()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			snapshot.CRS_CRL_Line = reconEntryLine.PK;
			snapshot.CRS_Type = "LDG";
			AssertExceptionThrown<ArgumentNullException>(() => CusReconSnapshot.Loader.GetSingleCusReconSnapshot(null, "LDG", reconEntryLine));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryLineNull()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			snapshot.CRS_CRL_Line = reconEntryLine.PK;
			snapshot.CRS_Type = "LDG";
			AssertExceptionThrown<ArgumentNullException>(() => CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", (CusReconEntryLine)null));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryLine_MultipleSnapshots()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			snapshot.CRS_CRL_Line = reconEntryLine.PK;
			snapshot.CRS_Type = "LDG";
			var snapshot2 = Factory.New<CusReconSnapshot>();
			snapshot2.CRS_CRL_Line = reconEntryLine.PK;
			snapshot2.CRS_Type = "LDG";
			AssertEquals(snapshot, CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", reconEntryLine));
		}

		public void TestGetSingleCusReconSnapshot_CusReconEntryLine_NoSnapshots()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			snapshot.CRS_Type = "LDG";
			AssertNull(CusReconSnapshot.Loader.GetSingleCusReconSnapshot(Factory, "LDG", reconEntryLine));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusReconSnapshot.Loader(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			snapshot = Factory.New<CusReconSnapshot>();
		}
		CusReconSnapshot snapshot;
	}
}
