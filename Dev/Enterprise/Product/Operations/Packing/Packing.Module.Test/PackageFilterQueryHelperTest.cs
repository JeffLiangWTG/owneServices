using System;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	public class PackageFilterQueryHelperTest : TransactionedTestCase
	{
		/// Note there are tests that actually load the bizOs in Warehouse -- DocketFilterBusinessObject and PickFilterBusinessObject.

		#region TestQueryDelegate_WhenModuleBizOIsPackageJobParent

		public void TestQueryDelegate_WhenModuleBizOIsPackageJobParent() // test with the diff constructors
		{
			var helper = new PackageFilterQueryHelper(typeof(DummyWithPacking), PkgPackageHeaderSchema.KPH_PackageID);
			var query = helper.QueryDelegate(SQLComparisonOperator.StartsWith, "abc");
			AssertEquals("Z0_PK IN (SELECT KJ_ParentID FROM dbo.PkgPackageJob WHERE KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_KPH_PackageHeader IN (SELECT KPH_PK FROM dbo.PkgPackageHeader WHERE KPH_PackageID like 'abc%')))", query.LiteralTextADO);

			var queryWithoutOperator = helper.QueryDelegateWithoutOperator("abc");
			AssertEquals("Z0_PK IN (SELECT KJ_ParentID FROM dbo.PkgPackageJob WHERE KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_KPH_PackageHeader IN (SELECT KPH_PK FROM dbo.PkgPackageHeader WHERE KPH_PackageID = 'abc')))", queryWithoutOperator.LiteralTextADO);

			var queryWithHandlingUnitID = helper.QueryDelegateWithHandlingUnitID(SQLComparisonOperator.StartsWith, "abc");
			AssertEquals("Z0_PK IN " +
				"(SELECT KJ_ParentID FROM dbo.PkgPackageJob WHERE KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_KP_TopHandlingUnitPackage IN " +
				"(SELECT KP_PK FROM dbo.PkgPackage WHERE KP_KPH_PackageHeader IN (SELECT KPH_PK FROM dbo.PkgPackageHeader WHERE KPH_PackageID like 'abc%'))) or (KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_PK IN " +
				"(SELECT KPD_KP_Package FROM dbo.PkgPackageHandlingUnitDivot WHERE KPD_KP_HandlingUnit IN (SELECT KP_PK FROM dbo.PkgPackage WHERE KP_KPH_PackageHeader IN (SELECT KPH_PK FROM dbo.PkgPackageHeader WHERE KPH_PackageID like 'abc%')) " +
				"and CONVERT(CONVERT(KPD_UnpackedTime, System.String), System.DateTime) is null))))", queryWithHandlingUnitID.LiteralTextADO);
		}

		#endregion

		#region TestQueryDelegate_WithPkgPackageColumn

		public void TestQueryDelegate_WithPkgPackageColumn()
		{
			var helper = new PackageFilterQueryHelper(typeof(DummyWithPacking), PkgPackageSchema.KP_F3_NKPackType);
			var query = helper.QueryDelegate(SQLComparisonOperator.StartsWith, "ab");
			AssertEquals("Z0_PK IN (SELECT KJ_ParentID FROM dbo.PkgPackageJob WHERE KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_F3_NKPackType like 'ab%'))", query.LiteralTextADO);

			var queryWithoutOperator = helper.QueryDelegateWithoutOperator("abc");
			AssertEquals("Z0_PK IN (SELECT KJ_ParentID FROM dbo.PkgPackageJob WHERE KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_F3_NKPackType = 'abc'))", queryWithoutOperator.LiteralTextADO);
		}

		#endregion

		#region TestQueryDelegate_WhenModuleBizOIsRelatedToPackageJobParent

		public void TestQueryDelegate_WhenModuleBizOIsRelatedToPackageJobParent()
		{
			var helper = new PackageFilterQueryHelper(typeof(PkgPackageContainer), typeof(PkgPackage), PkgPackageContainerSchema.K0_KP_Package, PkgPackageHeaderSchema.KPH_PackageID);
			var query = helper.QueryDelegate(SQLComparisonOperator.Equal, "abc");
			AssertEquals(
				"K0_KP_Package IN (SELECT KP_PK FROM dbo.PkgPackage WHERE KP_PK IN (SELECT KJ_ParentID FROM dbo.PkgPackageJob WHERE KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_KPH_PackageHeader IN (SELECT KPH_PK FROM dbo.PkgPackageHeader WHERE KPH_PackageID = 'abc'))))",
				query.LiteralTextADO);

			var queryWithoutOperator = helper.QueryDelegateWithoutOperator("abc");
			AssertEquals(
				"K0_KP_Package IN (SELECT KP_PK FROM dbo.PkgPackage WHERE KP_PK IN (SELECT KJ_ParentID FROM dbo.PkgPackageJob WHERE KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_KPH_PackageHeader IN (SELECT KPH_PK FROM dbo.PkgPackageHeader WHERE KPH_PackageID = 'abc'))))",
				queryWithoutOperator.LiteralTextADO);

			var queryWithHandlingUnitID = helper.QueryDelegateWithHandlingUnitID(SQLComparisonOperator.Equal, "abc");
			AssertEquals("K0_KP_Package IN " +
				"(SELECT KP_PK FROM dbo.PkgPackage WHERE KP_PK IN (SELECT KJ_ParentID FROM dbo.PkgPackageJob WHERE KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_KP_TopHandlingUnitPackage IN (SELECT KP_PK FROM dbo.PkgPackage WHERE KP_KPH_PackageHeader IN " +
				"(SELECT KPH_PK FROM dbo.PkgPackageHeader WHERE KPH_PackageID = 'abc'))) or (KJ_PK IN (SELECT KP_KJ_ParentPackageJob FROM dbo.PkgPackage WHERE KP_PK IN (SELECT KPD_KP_Package FROM dbo.PkgPackageHandlingUnitDivot WHERE KPD_KP_HandlingUnit IN " +
				"(SELECT KP_PK FROM dbo.PkgPackage WHERE KP_KPH_PackageHeader IN (SELECT KPH_PK FROM dbo.PkgPackageHeader WHERE KPH_PackageID = 'abc')) and CONVERT(CONVERT(KPD_UnpackedTime, System.String), System.DateTime) is null)))))", queryWithHandlingUnitID.LiteralTextADO);
		}

		#endregion

		#region TestExceptionIsThrownWhenInvalidColumtIsSupplied

		public void TestExceptionIsThrownWhenInvalidColumtIsSupplied()
		{
			AssertNoExceptionThrown(() => new PackageFilterQueryHelper(typeof(DummyWithPacking), PkgPackageHeaderSchema.KPH_PackageID));
			AssertNoExceptionThrown(() => new PackageFilterQueryHelper(typeof(DummyWithPacking), PkgPackageSchema.KP_DimensionUQ));
			// if we specify anything but PkgPackageHeader or PkgPackage column - an exception should be thrown
			AssertExceptionThrown<ArgumentException>(() => new PackageFilterQueryHelper(typeof(DummyWithPacking), OrgSupplierPartSchema.OP_Brand));
		}

		#endregion

	}
}
