using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDecRefs))]
	sealed class JobDecRefsBaseOnlyTest : EnterpriseBusinessObjectTestCase
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
				AssertEquals("From CurrentCompany", "ER", (Factory.New<JobDecRefs>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var declarationRefs = declaration.DeclarationRefs.AddNew();
				AssertEquals("From Declaration", "NZ", (declarationRefs as ITypeDeciderContext).Country);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return DecRefs;
		}

		JobDecRefs DecRefs
		{
			get
			{
				if (decRefs == null)
				{
					var dec = Factory.New<BaseJobDeclaration>();
					decRefs = dec.DeclarationRefs.AddNew();
				}
				return decRefs;
			}
		}
		JobDecRefs decRefs;
	}
}
