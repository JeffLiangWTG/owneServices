using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolWithTransaction : TransactionedTestCase
	{
		public void TestNctsHeaderForDocuments_CorrectNctsHeaderForCurrentCompany()
		{
			var factory = new BusinessObjectFactory();
			var company1 = factory.NewWithValidTestData<GlbCompany>();
			var company2 = factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company1.Branches.AddNew();
			branch1.FillWithValidTestData();
			var branch2 = company2.Branches.AddNew();
			branch2.FillWithValidTestData();
			var branch3 = company2.Branches.AddNew();
			branch3.FillWithValidTestData();
			factory.Save();

			var consol = factory.New<CommonConsol>();
			var nctsHeader1 = CreateNctsHeader(branch1, consol);
			var nctsHeader2 = CreateNctsHeader(branch2, consol);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company1", branch1.GB_GC, GlbCompany.CurrentCompany.PK);
				AssertEquals("NctsHeaderForDocuments is nctsHeader1 when branch is branch1", nctsHeader1.PK, consol.NctsHeaderForDocuments.PK);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company2", branch2.GB_GC, GlbCompany.CurrentCompany.PK);
				AssertEquals("NctsHeaderForDocuments is nctsHeader2 when branch is branch2", nctsHeader2.PK, consol.NctsHeaderForDocuments.PK);
			}

			AssertEquals("Branches 2 and 3 have common company", branch2.GB_GC, branch3.GB_GC);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company2", branch3.GB_GC, GlbCompany.CurrentCompany.PK);
				AssertEquals("NctsHeaderForDocuments is nctsHeader2 when branch is branch3", nctsHeader2.PK, consol.NctsHeaderForDocuments.PK);
			}

			BusinessObject CreateNctsHeader(GlbBranch branch, CommonConsol commonConsol)
			{
				var nctsHeader = (BusinessObject)factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
				nctsHeader[CusInBondHeaderSchema.BH_ParentID] = commonConsol.PK;
				nctsHeader[CusInBondHeaderSchema.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader[CusInBondHeaderSchema.BH_GB] = branch.PK;
				var nctsDepartureMovement = (BusinessObject)factory.New<Enterprise.Integration.Customs.EU.NCTS.IDepartureMovementHeader>();
				nctsDepartureMovement[CusInBondMoveHeaderSchema.BM_BH] = nctsHeader.PK;
				return nctsHeader;
			}
		}
	}
}
