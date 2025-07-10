using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusInBondMoveHeaderTest : TestCaseWithFactory
	{
		public void TestHeaderBranch()
		{
			var header = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
			header.BH_GB = branch.PK;
			var moveHeader = (BaseCusInBondMoveHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader>();
			CombineAssertions(() =>
			{
				AssertNull("header branch not set", moveHeader.HeaderBranch);
				moveHeader.BM_BH = header.PK;
				AssertEquals("header branch is set", branch.PK, moveHeader.HeaderBranch.PK);
			});
		}

		public void TestRegistryBranchPK()
		{
			var header = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
			header.BH_GB = branch.PK;

			CombineAssertions(() =>
			{
				var moveHeader = (BaseCusInBondMoveHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader>();
				AssertEquals("default to current branch", GlbBranch.CurrentBranch.PK.ToGuid(), moveHeader.RegistryBranchPK);
				var moveHeader1 = (BaseCusInBondMoveHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader>();
				moveHeader1.BM_BH = header.PK;
				AssertEquals("branch is set", branch.PK, moveHeader1.RegistryBranchPK);
			});
		}

		public void TestRegistryCompanyPK()
		{
			var header = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
			header.BH_GB = branch.PK;

			CombineAssertions(() =>
			{
				var moveHeader = (BaseCusInBondMoveHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader>();
				AssertEquals("default to current company", GlbCompany.CurrentCompany.PK.ToGuid(), moveHeader.RegistryCompanyPK);
				var moveHeader1 = (BaseCusInBondMoveHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader>();
				moveHeader1.BM_BH = header.PK;
				AssertEquals("company is set", branch.GB_GC.ToGuid(), moveHeader1.RegistryCompanyPK);
			});
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			var baseCusInBondMoveHeaderForTest = Factory.New<BaseCusInBondMoveHeaderForTest>();
			AssertEquals(typeof(CusGoodsLocation), (baseCusInBondMoveHeaderForTest as ICusGoodsLocationTypeSupporter).GoodsLocationType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();
		}

		GlbBranch branch;
	}

	sealed class BaseCusInBondMoveHeaderForTest : BaseCusInBondMoveHeader
	{
		public BaseCusInBondMoveHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type MovementDetailTypeCore => throw new NotImplementedException();
	}
}
