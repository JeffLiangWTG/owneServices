using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackingList))]
	sealed class CusPackingListIPackingParentTest : PackingParentCustomPackTypesTestCase<CusPackingList>
	{
		public void TestPackageSequenceType()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packingList = dec.LoadOrCreateCusPackingList(Factory);
			IPackingParent packingParent = packingList;
			AssertEquals(PackageSequenceType.Standard, packingParent.PackageSequenceType);
		}

		protected override CusPackingList GetNewParent()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packageJob = dec.LoadOrCreateCusPackingList(Factory).PackageJob;
			packageJob.Packages.AddNew();
			return packageJob.PackingList;
		}

		public override void TestPackageJobIsDeletedOnParentJobDelete()
		{
			var parent = GetNewParent();
			var packageJob = CusPackageJob.LoadOrCreatePackageJob(parent);
			packageJob.Packages.AddNew();

			parent.Delete();
			AssertEquals("The related PackageJob was not deleted. Override Delete() on your BusinessObject and delete it.", true, packageJob.IsDeleted);
		}
	}
}
