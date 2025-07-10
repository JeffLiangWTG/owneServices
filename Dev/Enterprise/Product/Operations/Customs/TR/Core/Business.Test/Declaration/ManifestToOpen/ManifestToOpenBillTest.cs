using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(ManifestToOpenBill))]
	class ManifestToOpenBillTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ClusterKey = 1;
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			manifest.FillWithValidTestData();
			var bill = manifest.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.TPD_ClusterKey = 1;
			return bill;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ClusterKey = 1;
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			manifest.FillWithValidTestData();
			var bill = manifest.Bills.AddNew();
			bill.FillWithValidTestData();
			return bill;
		}

		public void TestTPD_DocumentNumber_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenBill), nameof(ManifestToOpenBill.TPD_DocumentNumber), false, x => x.Caption == "Bill No");
		}

		public void TestTPD_IncludeAllItems_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenBill), nameof(ManifestToOpenBill.TPD_IncludeAllItems), false, x => x.Caption == "All");
		}

		public void TestTPD_IsInWarehouse_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenBill), nameof(ManifestToOpenBill.TPD_IsInWarehouse), false, x => x.Caption == "In Warehouse?");
		}

		public void TestTPD_IsOtherProcedure_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenBill), nameof(ManifestToOpenBill.TPD_IsOtherProcedure), false, x => x.Caption == "Other Procedure?");
		}

		public void TestSetDefaultValues()
		{
			var bill = Factory.New<ManifestToOpenBill>();
			CombineAssertions(() =>
			{
				Assert(bill.TPD_IsInWarehouse);
				Assert(bill.TPD_IncludeAllItems);
			});
		}
	}

	[TestedType(typeof(ManifestToOpenBill))]
	class ManifestToOpenBillClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			if (manifestToOpenBill == null)
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}

				var manifest = declaration.ManifestToOpenHeaders.AddNew();
				manifest.FillWithValidTestData();
				manifestToOpenBill = manifest.Bills.AddNew();
				manifestToOpenBill.TPD_DocumentNumber = "001";
			}

			return manifestToOpenBill;
		}

		protected override EnterpriseBusinessObject NewParentObject() => declaration = Factory.New<JobDeclaration>();

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() =>
			new IClusterKeyWorker[]
			{
				((ManifestToOpenBill)NewClusterKeyEntity()).Packs.AddNew()
			};

		JobDeclaration declaration;
		ManifestToOpenBill manifestToOpenBill;
	}
}
