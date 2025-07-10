using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Module
{
	public class PackageFilterQueryHelper
	{
		#region Construction

		/// <summary>
		/// Use this constructor if your module BizO is the parent of the PackageJob.
		/// </summary>
		public PackageFilterQueryHelper(Type moduleBizOType, SchemaColumn packageColumnToFilterOn)
		{
			Argument.NotNull(moduleBizOType, "moduleBizOType");
			Argument.NotNull(packageColumnToFilterOn, "packageColumnToFilterOn");

			if (packageColumnToFilterOn.TableName != PkgPackageSchema.Constants.TableName
				&& packageColumnToFilterOn.TableName != PkgPackageHeaderSchema.Constants.TableName)
			{
				throw new ArgumentException("packageColumnToFilterOn must belong to PkgPackageSchema or PkgPackageHeaderSchema", nameof(packageColumnToFilterOn));
			}

			ModuleBizOType = moduleBizOType;
			PackageColumnToFilterOn = packageColumnToFilterOn;
		}

		/// <summary>
		/// Use this constructor if your module BizO is NOT the parent of the PackageJob. For example -- Warehouse Order has
		/// a PackageJob, however the PICK MODULE displays Warehouse Picks and allows a search on the child Orders' PackageIDs.
		/// </summary>
		public PackageFilterQueryHelper(Type moduleBizOType, Type packageJobParentJobBizOType, SchemaGuidColumn subQueryfK, SchemaColumn packageColumnToFilterOn)
			: this(moduleBizOType, packageColumnToFilterOn)
		{
			Argument.NotNull(packageJobParentJobBizOType, "packageJobParentJobBizOType");
			Argument.NotNull(subQueryfK, "subQueryfK");

			PackageJobParentJobBizOType = packageJobParentJobBizOType;
			SubQueryfK = subQueryfK;
		}

		readonly Type ModuleBizOType;
		readonly Type PackageJobParentJobBizOType;
		readonly SchemaGuidColumn SubQueryfK;
		readonly SchemaColumn PackageColumnToFilterOn;

		#endregion

		#region Query

		public GetTextQueryWithOperator QueryDelegate
		{
			get { return GetPackageValueQuery; }
		}

		public GetTextQuery QueryDelegateWithoutOperator
		{
			get { return GetPackageValueQueryWithoutOperator; }
		}

		public GetTextQueryWithOperator QueryDelegateWithHandlingUnitID
		{
			get { return GetPackageValueQueryWithHandlingUnitID; }
		}

		ZQuery GetPackageValueQueryWithoutOperator(ZString packageValue)
		{
			return GetPackageValueQuery(SQLComparisonOperator.Equal, packageValue);
		}

		ZQuery GetPackageValueQuery(SQLComparisonOperator comparisonOperator, ZString packageValue)
		{
			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);

			if (PackageColumnToFilterOn.TableName == PkgPackageSchema.Constants.TableName)
			{
				packageSubQuery.AddToFilter(PackageColumnToFilterOn, comparisonOperator, packageValue);
			}
			else
			{
				var packageIDSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
				packageIDSubQuery.AddToFilter(PackageColumnToFilterOn, comparisonOperator, packageValue);
				packageSubQuery.AddSubQuery(packageIDSubQuery, JoinCondition.And);
			}

			var outerQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			outerQuery.AddSubQuery(packageSubQuery, JoinCondition.And);

			if (PackageJobParentJobBizOType != null)
			{
				var packageJobParentSubQuery = new ZDBOnlySubQuery(PackageJobParentJobBizOType, SubQueryfK);
				packageJobParentSubQuery.AddSubQuery(outerQuery, JoinCondition.And);

				outerQuery = packageJobParentSubQuery;
			}

			var query = new ZDBOnlyQuery(ModuleBizOType);
			query.AddSubQuery(outerQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetPackageValueQueryWithHandlingUnitID(SQLComparisonOperator comparisonOperator, ZString handlingUnitID)
		{
			var handlingUnitSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			handlingUnitSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, comparisonOperator, handlingUnitID);

			var handlingUnitPkgPackageQueryForSingleLevel = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.PK);
			handlingUnitPkgPackageQueryForSingleLevel.AddSubQuery(handlingUnitSubQuery, JoinCondition.And);

			var singleLevelHandlingUnitPkgPackageQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			singleLevelHandlingUnitPkgPackageQuery.AddSubQuery(PkgPackageSchema.KP_KP_TopHandlingUnitPackage, handlingUnitPkgPackageQueryForSingleLevel, JoinCondition.And);

			var handlingUnitPkgPackageQueryForSecondLevel = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit);
			handlingUnitPkgPackageQueryForSecondLevel.AddSubQuery(handlingUnitSubQuery, JoinCondition.And);

			var pkgPackageHandlingUnitDivotQueryForSecondLevel = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
			pkgPackageHandlingUnitDivotQueryForSecondLevel.AddSubQuery(handlingUnitPkgPackageQueryForSecondLevel, JoinCondition.And);

			pkgPackageHandlingUnitDivotQueryForSecondLevel.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);

			var secondLevelHandlingUnitPkgPackageQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			secondLevelHandlingUnitPkgPackageQuery.AddSubQuery(pkgPackageHandlingUnitDivotQueryForSecondLevel, JoinCondition.And);

			var pkgPackageJobQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			pkgPackageJobQuery.AddSubQuery(singleLevelHandlingUnitPkgPackageQuery, JoinCondition.Or);
			pkgPackageJobQuery.AddSubQuery(secondLevelHandlingUnitPkgPackageQuery, JoinCondition.Or);

			if (PackageJobParentJobBizOType != null)
			{
				var packageJobParentSubQuery = new ZDBOnlySubQuery(PackageJobParentJobBizOType, SubQueryfK);
				packageJobParentSubQuery.AddSubQuery(pkgPackageJobQuery, JoinCondition.And);

				pkgPackageJobQuery = packageJobParentSubQuery;
			}

			var query = new ZDBOnlyQuery(ModuleBizOType);
			query.AddSubQuery(pkgPackageJobQuery, JoinCondition.And);

			return query;
		}

		#endregion
	}
}
