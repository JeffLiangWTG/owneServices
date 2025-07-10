using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BranchDistrictPort))]
	sealed class BranchDistrictPortTest : RegistryBusinessObjectTemplateTestCase<BranchDistrictPort>
	{
		public void TestValidation()
		{
			var company = GlbCompany.CurrentCompany;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "TST";
			company.Branches.Add(branch);
			var collection = new BranchDistrictPortCollection(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			var item1 = collection.AddNew();
			item1.PortCode = "390";
			AssertHasError(item1.PortCodeInfo, BranchDistrictPort.PortCodeLength);
			item1.PortCode = "3901";
			AssertNoError(item1.PortCodeInfo, BranchDistrictPort.PortCodeLength);
			item1.BranchPK = ZGuid.Invalid;
			AssertHasErrorContaining(item1.BranchPKInfo, ListValidation.InvalidCodeError);
			item1.BranchPK = branch.PK;
			AssertNoErrorContaining(item1.BranchPKInfo, ListValidation.InvalidCodeError);
			var item2 = collection.AddNew();
			item2.PortCode = "39";
			item2.BranchPK = Env.CurrentBranch.PK;
			AssertNoError(item2.PortCodeInfo, BranchDistrictPort.DuplicatesFound);
			item2.BranchPK = ZGuid.Empty;
			AssertHasError(item2.BranchPKInfo, BranchDistrictPort.BranchIsMandatory);
			item2.BranchPK = branch.PK;
			AssertHasError(item2.PortCodeInfo, BranchDistrictPort.DuplicatesFound);
			AssertNoError(item2.BranchPKInfo, BranchDistrictPort.BranchIsMandatory);
			item2.PortCode = "3904";
			AssertNoError(item2.PortCodeInfo, BranchDistrictPort.DuplicatesFound);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new BranchDistrictPortCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override BranchDistrictPort GetBusinessObjectToClone() => (BranchDistrictPort)GetNewBusinessObject();

		protected override BranchDistrictPort GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
