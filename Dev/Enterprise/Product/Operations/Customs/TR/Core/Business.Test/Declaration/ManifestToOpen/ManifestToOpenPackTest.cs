using System.Collections.Generic;
using CargoWise.ComponentModel;
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
	[TestedType(typeof(ManifestToOpenPack))]
	class ManifestToOpenPackTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ClusterKey = 1;
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			manifest.FillWithValidTestData();
			var bill = manifest.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.TPD_ClusterKey = 1;
			var pack = bill.Packs.AddNew();
			pack.FillWithValidTestData();
			pack.TPI_ClusterKey = 1;
			return pack;
		}

		public void TestLookups()
		{
			var pack = Factory.New<ManifestToOpenPack>();
			AssertType<ManifestToOpenPackLookups>(pack.Lookups);
		}

		public void TestValidation()
		{
			var pack = Factory.New<ManifestToOpenPack>();
			AssertType<ManifestToOpenPackValidation>(pack.Validation);
		}

		public void TestTPI_LineNumber_ReadOnlyMember()
		{
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(ManifestToOpenPack), nameof(ManifestToOpenPack.TPI_LineNumber), false, attr => attr.Member == nameof(ManifestToOpenPack.BillIncludeAllItems));
		}

		public void TestTPI_Quantity_ReadOnlyMember()
		{
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(ManifestToOpenPack), nameof(ManifestToOpenPack.TPI_Quantity), false, attr => attr.Member == nameof(ManifestToOpenPack.IsQuantityReadOnly));
		}

		public void TestTPI_WarehouseCode_ReadOnlyMember()
		{
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(ManifestToOpenPack), nameof(ManifestToOpenPack.TPI_WarehouseCode), false, attr => attr.Member == nameof(ManifestToOpenPack.IsWarehouseCodeReadOnly));
		}

		public void TestTPI_WarehouseCode_List()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(ManifestToOpenPack), nameof(ManifestToOpenPack.TPI_WarehouseCode), false, attr => attr.ListDataSourceMember == nameof(ManifestToOpenPack.Lookups) + "." + nameof(ManifestToOpenPackLookups.WarehouseCodeList));
		}

		public void TestTPI_LineNumber_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenPack), nameof(ManifestToOpenPack.TPI_LineNumber), false, x => x.Caption == "Line No.");
		}

		public void TestTPI_Quantity_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenPack), nameof(ManifestToOpenPack.TPI_Quantity), false, x => x.Caption == "Quantity");
		}

		public void TestTPI_WarehouseCode_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenPack), nameof(ManifestToOpenPack.TPI_WarehouseCode), false, x => x.Caption == "Warehouse Code");
		}

		public void TestIncludeAllItems_BillIsNull()
		{
			var pack = Factory.New<ManifestToOpenPack>();
			AssertEquals(nameof(pack.BillIncludeAllItems), false, pack.BillIncludeAllItems);
		}

		public void TestIsInWarehouse_BillIsNull()
		{
			var pack = Factory.New<ManifestToOpenPack>();
			AssertEquals(nameof(pack.BillIsInWarehouse), false, pack.BillIsInWarehouse);
		}

		public void TestIsQuantityReadOnly()
		{
			var bill = Factory.New<ManifestToOpenBill>();
			var pack = bill.Packs.AddNew();
			bill.TPD_IncludeAllItems = true;

			CombineAssertions(() =>
			{
				AssertEquals(nameof(pack.IsQuantityReadOnly), true, pack.IsQuantityReadOnly);

				bill.TPD_IncludeAllItems = false;
				pack.TPI_LineNumber = 1;

				AssertEquals(nameof(pack.IsQuantityReadOnly), false, pack.IsQuantityReadOnly);

				pack.TPI_LineNumber = 0;

				AssertEquals(nameof(pack.IsQuantityReadOnly), true, pack.IsQuantityReadOnly);
			});
		}

		public void TestIsWarehouseCodeReadOnly()
		{
			var bill = Factory.New<ManifestToOpenBill>();
			var pack = bill.Packs.AddNew();
			bill.TPD_IncludeAllItems = true;

			CombineAssertions(() =>
			{
				AssertEquals(nameof(pack.IsWarehouseCodeReadOnly), true, pack.IsWarehouseCodeReadOnly);

				bill.TPD_IncludeAllItems = false;
				bill.TPD_IsInWarehouse = true;
				pack.TPI_LineNumber = 1;

				AssertEquals(nameof(pack.IsWarehouseCodeReadOnly), false, pack.IsWarehouseCodeReadOnly);

				pack.TPI_LineNumber = 0;

				AssertEquals(nameof(pack.IsWarehouseCodeReadOnly), true, pack.IsWarehouseCodeReadOnly);
			});
		}
	}

	[TestedType(typeof(ManifestToOpenPack))]
	class ManifestToOpenPackClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			if (manifestToOpenBill == null)
			{
				manifestToOpenBill = (ManifestToOpenBill)NewParentObject();
			}
			return manifestToOpenBill.Packs.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var manifest = Declaration.ManifestToOpenHeaders.AddNew();
			manifest.FillWithValidTestData();
			var manifestToOpenBill = manifest.Bills.AddNew();
			manifestToOpenBill.TPD_DocumentNumber = "001";

			return manifestToOpenBill;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		ManifestToOpenBill manifestToOpenBill;
	}
}
