using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageItemDivotValidationTest : PackingBusinessObjectValidationTestCase
	{
		public void TestCheckPkgNetWeight()
		{
			var error = "value cannot be negative.";
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			var info = packageItemDivot.PkgNetWeightInfo;
			packageItemDivot.PkgNetWeight = -1;
			AssertHasError(info, error);

			packageItemDivot.PkgNetWeight = 0;
			AssertNoError(info, error);

			packageItemDivot.PkgNetWeight = 1;
			AssertNoError(info, error);
		}

		public void TestCheckPkgNetWeightUQ()
		{
			var warning = "You have not entered a valid code.";
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			var info = packageItemDivot.PkgNetWeightUQInfo;
			var weightUQs = packageItemDivot.Lookups.WeightUQs.GetAllCodes();
			foreach (var code in weightUQs)
			{
				packageItemDivot.PkgNetWeightUQ = code;
				AssertNoWarning(info, warning);
			}

			packageItemDivot.PkgNetWeightUQ = "A";
			AssertHasWarning(info, warning);
		}

		public void TestShouldValidateFKToCancelledRecord()
		{
			var packageDivot = Factory.New<PkgPackageItemDivot>();
			var validation = new TestPkgPackageValidation(packageDivot);
			AssertEquals("Parent Package cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(packageDivot.KI_KP_PackageInfo));
			AssertEquals("Packed Item cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(packageDivot.KI_ParentIDInfo));

			foreach (var propertyInfo in packageDivot.ZPropertyInfoHash
				.Cast<ZPropertyInfo>()
				.Where(p => p.IsPersistent && p.Name != nameof(packageDivot.KI_KP_Package) && p.Name != nameof(packageDivot.KI_ParentID)))
			{
				AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
			}
		}

		class TestPkgPackageValidation : PkgPackageItemDivotValidation
		{
			public TestPkgPackageValidation(PkgPackageItemDivot parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}
	}
}
