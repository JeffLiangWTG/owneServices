using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconEntry))]
	class CusReconEntryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportsClone()
		{
			AssertEquals(true, reconEntry.SupportsClone());
		}

		public void TestTypeDecider()
		{
			AssertType<CusReconEntryTypeDecider>(CusReconEntry.TypeDecider);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GlbBranch.CurrentBranch.PK, reconEntry.CRE_GB_Branch);
		}

		public void TestReconDeclaration()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconEntry.CRE_CRD = reconDeclaration.PK;
			AssertEquals(reconEntry.CRE_CRD, reconEntry.ReconDeclaration.PK);
		}

		public void TestCusReconEntryLines()
		{
			AssertType<CusReconEntryLineCollection>(reconEntry.CusReconEntryLines);
		}

		public void TestCRE_EntryType_Caption()
		{
			AssertEquals("Entry Type", DataBoundResourceStrings.GetDataForProperty(reconEntry.CRE_EntryTypeInfo).Caption);
		}

		public void TestCRE_EntryType_List()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CusReconEntry), CusReconEntry.Schema.CRE_EntryType, true, attrib => attrib.ListDataSourceMember == "Lookups.EntryTypeList");
		}

		public void TestDeclarantCode()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "Dec";
			reconEntry.CRE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertEquals("Dec", reconEntry.DeclarantCode);
		}

		public void TestDeclarantCode_Caption()
		{
			var propertyInfo = reconEntry.GetType().GetProperty(CusReconEntry.Schema.DeclarantCode);
			AssertEquals("Declarant", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestImporterCode()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Imp";
			reconEntry.CRE_OA_ImporterAddress = importer.MainAddress.PK;
			AssertEquals("Imp", reconEntry.ImporterCode);
		}

		public void TestImporterCode_Caption()
		{
			var propertyInfo = reconEntry.GetType().GetProperty(CusReconEntry.Schema.ImporterCode);
			AssertEquals("Importer", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestRepresentativeCode()
		{
			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "Rep";
			reconEntry.CRE_OA_RepresentativeAddress = representative.MainAddress.PK;
			AssertEquals("Rep", reconEntry.RepresentativeCode);
		}

		public void TestRepresentativeCode_Caption()
		{
			var propertyInfo = reconEntry.GetType().GetProperty(CusReconEntry.Schema.RepresentativeCode);
			AssertEquals("Representative", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestBuyingAgentCode()
		{
			var buyingAgent = Factory.New<OrgHeader>();
			buyingAgent.OH_Code = "Buy";
			reconEntry.CRE_OA_BuyingAgentAddress = buyingAgent.MainAddress.PK;
			AssertEquals("Buy", reconEntry.BuyingAgentCode);
		}

		public void TestBuyingAgentCode_Caption()
		{
			var propertyInfo = reconEntry.GetType().GetProperty(CusReconEntry.Schema.BuyingAgentCode);
			AssertEquals("Represented Party", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestRepresentationType()
		{
			CombineAssertions(() =>
			{
				reconEntry.CRE_CH_OriginalEntry = ZGuid.Empty;
				AssertEquals("EntryHeader is null", ZString.Empty, reconEntry.RepresentationType);

				var entryHeader = Factory.New<CusEntryHeader>();
				reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
				AssertEquals("EntryHeader.Declaration is null", ZString.Empty, reconEntry.RepresentationType);

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_DeclarantType = "DIR";
				entryHeader.CH_JE = declaration.PK;
				AssertEquals("EntryHeader.Declaration.JE_DeclarantType = 'DIR'", "DIR", reconEntry.RepresentationType);
			});
		}

		public void TestRepresentationType_Caption()
		{
			var propertyInfo = reconEntry.GetType().GetProperty(CusReconEntry.Schema.RepresentationType);
			AssertEquals("Rep. Type", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestCusReconSnapshots()
		{
			var reconEntrySnapshots = reconEntry.CusReconSnapshots;
			var snapshot1 = reconEntrySnapshots.AddNew();
			var snapshot2 = reconEntrySnapshots.AddNew();
			var reconEntryLine = reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CusReconSnapshots.AddNew();

			CombineAssertions(() =>
			{
				AssertType<CusReconSnapshotCollection>("Type", reconEntrySnapshots);
				AssertEquals("IsRegisteredEditableChildObject", true, reconEntry.IsRegisteredEditableChildObject(reconEntrySnapshots));
				AssertContainsExactElementsInAnyOrder("Elements", new[] { snapshot1.PK, snapshot2.PK }, reconEntrySnapshots.Select(x => x.PK));
			});
		}

		public void TestCusReconEntryLinesAreDeleted()
		{
			var entryLine1 = reconEntry.CusReconEntryLines.AddNew();
			var entryLine2 = reconEntry.CusReconEntryLines.AddNew();

			var reconEntry2 = Factory.New<CusReconEntry>();
			var entryLine3 = reconEntry2.CusReconEntryLines.AddNew();

			reconEntry.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Entry is deleted", true, reconEntry.IsDeleted);
				AssertEquals("Line1 from deleted entry", true, entryLine1.IsDeleted);
				AssertEquals("Line2 from deleted entry", true, entryLine2.IsDeleted);
				AssertEquals("Line from non-deleted entry", false, entryLine3.IsDeleted);
			});
		}

		public void TestCusReconSnapshotsAreDeleted()
		{
			var snapshot1 = reconEntry.CusReconSnapshots.AddNew();
			var snapshot2 = reconEntry.CusReconSnapshots.AddNew();

			var reconEntry2 = Factory.New<CusReconEntry>();
			var snapshot3 = reconEntry2.CusReconSnapshots.AddNew();

			reconEntry.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Entry is deleted", true, reconEntry.IsDeleted);
				AssertEquals("Snapshot1 from deleted entry", true, snapshot1.IsDeleted);
				AssertEquals("Snapshot2 from deleted entry", true, snapshot2.IsDeleted);
				AssertEquals("Snapshot from non-deleted entry", false, snapshot3.IsDeleted);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => reconEntry;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
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
			return reconEntry;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			reconEntry = Factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
		}
		CusReconEntry reconEntry;
	}
}
