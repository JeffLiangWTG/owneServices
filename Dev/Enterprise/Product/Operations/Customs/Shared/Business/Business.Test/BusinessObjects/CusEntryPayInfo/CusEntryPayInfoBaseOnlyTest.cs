using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryPayInfoBaseOnlyTest : TestCaseWithFactory
	{
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
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusEntryPayInfo>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				var payInfo = entry.EntryPayInfos.AddNew();
				AssertEquals("From EntryHeader", "NZ", (payInfo as ITypeDeciderContext).Country);
			});
		}
	}
}
