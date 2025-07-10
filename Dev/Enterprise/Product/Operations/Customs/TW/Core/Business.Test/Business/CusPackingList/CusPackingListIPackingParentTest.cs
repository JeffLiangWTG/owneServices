using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackingList))]
	sealed class CusPackingListIPackingParentTest : PackingParentCustomPackTypesTestCase<CusPackingList>
	{
		protected override CusPackingList GetNewParent()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var cusPackingList = (CusPackingList)dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			packageJob.Packages.AddNew();
			return cusPackingList;
		}

		[ExpectNoExceptions]
		public override void TestPackageJobIsDeletedOnParentJobDelete()
		{
			var parent = GetNewParent();
			var packageJob = CusPackageJob.LoadOrCreatePackageJob(parent);
			packageJob.Packages.AddNew();

			parent.Delete();
			NUnit.Framework.Assert.That(packageJob.IsDeleted, NUnit.Framework.Is.EqualTo(true), "The related PackageJob was not deleted. Override Delete() on your BusinessObject and delete it.");
		}
	}
}
