using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntrySnapshot))]
	sealed class CusEntrySnapshotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();

			var pivot = Factory.New<CusEntrySnapshot>();
			pivot.CES_CH_EntryHeader = entryheader.PK;

			return pivot;
		}

		public void TestRelatedEntryHeaderProperty()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "11";

			var pivot = Factory.New<CusEntrySnapshot>();
			pivot.CES_CH_EntryHeader = entryHeader.PK;
			AssertEquals("EntryInstruction should be linked to CusEntrySnapshot", pivot.CES_CH_EntryHeader, pivot.EntryHeader.PK);
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusEntrySnapshot>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				var pivot = entry.Snapshots.AddNew();
				AssertEquals("From EntryHeader", "NZ", (pivot as ITypeDeciderContext).Country);
			});
		}
	}
}
