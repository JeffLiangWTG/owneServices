using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(AttributeNeutralPackageInfo))]
	public class AttributeNeutralPackageInfoTest : DataObjectInfoTestCase<AttributeNeutralPackageInfo>
	{
		public void TestConstructor_EmptyConstructor()
		{
			var info = new AttributeNeutralPackageInfo();
			AssertEquals("No serials.", 0, info.SerialNumbers.Length);
			AssertEquals("Expect empty amount.", 0, info.ExpectedQuantityInPackage);
			AssertEquals("Expect empty PackagePK.", Guid.Empty, info.PackagePK);
			AssertEquals("Expect empty PalletID.", string.Empty, info.PalletID);
			AssertEquals("Expect empty PackageType.", string.Empty, info.PackType);
			AssertEquals("Expect empty ProductPK.", Guid.Empty, info.ProductPK);
		}

		public void TestConstructor()
		{
			var pkgPK = new Guid();
			var productPK = new Guid();
			var palletID = "PLTID";
			var expectedAmt = 2;
			var pkgType = "TAN";
			var serials = new[] { "SN1", "SN2" };

			var info = new AttributeNeutralPackageInfo(pkgType, expectedAmt, palletID, productPK, serials);
			info.PackagePK = pkgPK;

			AssertEquals("Serials is correct", serials, info.SerialNumbers);
			AssertEquals("Expect correct amount.", expectedAmt, info.ExpectedQuantityInPackage);
			AssertEquals("Expect correct PackagePK.", pkgPK, info.PackagePK);
			AssertEquals("Expect correct PalletID.", palletID, info.PalletID);
			AssertEquals("Expect correct PackageType.", pkgType, info.PackType);
			AssertEquals("Expect correct ProductPK.", productPK, info.ProductPK);
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new AttributeNeutralPackageInfo();
		}
	}
}
