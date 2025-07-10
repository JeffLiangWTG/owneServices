using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BranchDistrictPortRegistryItem))]
	sealed class BranchDistrictPortRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<BranchDistrictPortCollection>
	{
		protected override StronglyTypedRegistryItem<BranchDistrictPortCollection, BranchDistrictPortCollection> GetNewRegistryItem() => new BranchDistrictPortRegistryItem("", null, null, null, RegistryStorageFlags.Company);
	}

	[TestedType(typeof(BranchDistrictPortCollectionRegistryDataType))]
	sealed class BranchDistrictPortCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BranchDistrictPortCollectionRegistryDataType>
	{
		protected override string ExpectedEditorName => "BranchDistrictPortRegistryItemEditor";

		protected override BranchDistrictPortCollectionRegistryDataType GetNewDataType() => new BranchDistrictPortCollectionRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var coll = new BranchDistrictPortCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);
			var element = coll.AddNew();
			element.PortCode = "3901";
			element.BranchPK = GlbBranch.CurrentBranch.PK;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(coll, new BranchDistrictPortCollectionRegistryDataType().Serialise(coll))
			};
		}
	}
}
