using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class PackageStateColumnIndexerHelper
	{
		public static ZString GetStatus(IColumnIndexer packageState)
		{
			if (packageState != null)
			{
				var statusCodeDescriptionPairs = new TransitWarehouseStatuses();
				return statusCodeDescriptionPairs.GetDescriptionFromCode(packageState.GetValue(WhsItemPackageStateSchema.WPS_Status));
			}
			return "";
		}

		public static IColumnIndexer GetRCN(UniversalObjectFactory factory, IColumnIndexer packageState)
		{
			if (packageState != null)
			{
				var rcnPK = packageState.GetValue(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment);
				var query = new ZQuery(WhsItemReceiveConsignmentSchema.PK, rcnPK);
				return DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(WhsItemReceiveConsignmentSchema.Constants.TableName, query).SingleOrDefault());
			}
			return null;
		}

		public static IColumnIndexer GetDCN(UniversalObjectFactory factory, IColumnIndexer packageState)
		{
			if (packageState != null)
			{
				var dcnPK = packageState.GetValue(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment);
				var query = new ZQuery(WhsItemDispatchConsignmentSchema.PK, dcnPK);
				return DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(WhsItemDispatchConsignmentSchema.Constants.TableName, query).SingleOrDefault());
			}
			return null;
		}

		public static IColumnIndexer GetLoadList(UniversalObjectFactory factory, IColumnIndexer packageState)
		{
			if (packageState != null)
			{
				var ddlPK = packageState.GetValue(WhsItemPackageStateSchema.WPS_WDL_LoadList);
				var query = new ZQuery(WhsItemDispatchLoadListSchema.PK, ddlPK);
				return DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(WhsItemDispatchLoadListSchema.Constants.TableName, query).SingleOrDefault());
			}
			return null;
		}

		public static IColumnIndexer GetPackage(UniversalObjectFactory factory, IColumnIndexer packageState)
		{
			if (packageState != null)
			{
				var packagePK = packageState.GetValue(WhsItemPackageStateSchema.WPS_KP_Package);
				var query = new ZQuery(PkgPackageSchema.PK, packagePK);
				return DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, query).SingleOrDefault());
			}
			return null;
		}

		public static ZString GetPackageReference(UniversalObjectFactory factory, IColumnIndexer packageState)
		{
			if (packageState != null)
			{
				var package = GetPackage(factory, packageState);
				if (package != null)
				{
					var header = PackageColumnIndexerHelper.GetHeader(factory, package);
					if (header != null)
					{
						var packageID = header.GetValue(PkgPackageHeaderSchema.KPH_PackageID);
						if (!packageID.IsEmpty)
						{
							return packageID;
						}
					}
					return new ZString($"{package.GetValue(PkgPackageSchema.KP_PackageQty)} {package.GetValue(PkgPackageSchema.KP_F3_NKPackType)}");
				}
			}
			return "";
		}

		public static ZString GetPackageType(UniversalObjectFactory factory, IColumnIndexer packageState)
		{
			if (packageState != null)
			{
				var package = GetPackage(factory, packageState);
				if (package != null)
				{
					return package.GetValue(PkgPackageSchema.KP_F3_NKPackType);
				}
			}
			return "";
		}

		public static Dictionary<ZGuid, WhsItemPackageState> GetTransportationUnitAsPackageStates_ByTransportationUnitPK(UniversalObjectFactory factory, string parentTableCode, IEnumerable<ZGuid> transportationUnitPKs)
		{
			var res = new Dictionary<ZGuid, WhsItemPackageState>();
			if (transportationUnitPKs == null || !transportationUnitPKs.Any())
			{
				return res;
			}

			transportationUnitPKs = transportationUnitPKs.Distinct();
			var pkgExtensionQuery = new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, transportationUnitPKs);
			pkgExtensionQuery.AddToFilter(PkgPackageExtensionSchema.KPN_ParentTableCode, parentTableCode);

			var pkgExtensions = factory.BOFactory.Load<PkgPackageExtension>(pkgExtensionQuery);

			var transportationUnits_AsPkgPK = pkgExtensions.Select(pe => pe.KPN_KP_Package);
			var transportationUnits_AsPkgStateQuery = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, transportationUnits_AsPkgPK);
			var transportationUnits_AsPkgState = factory.BOFactory.Load<WhsItemPackageState>(transportationUnits_AsPkgStateQuery);

			foreach (var pkgExtension in pkgExtensions)
			{
				var transportationUnitPk = pkgExtension.KPN_ParentID;
				if (!res.ContainsKey(transportationUnitPk))
				{
					var transportationUnit_AsPkgPK = pkgExtension.KPN_KP_Package;
					var transportationUnit_AsPkgState = transportationUnits_AsPkgState.FirstOrDefault(ps => ps.WPS_KP_Package == transportationUnit_AsPkgPK);
					res.Add(transportationUnitPk, transportationUnit_AsPkgState);
				}
			}
			foreach (var transportationUnitPk in transportationUnitPKs)
			{
				if (!res.ContainsKey(transportationUnitPk))
				{
					res.Add(transportationUnitPk, null);
				}
			}
			return res;
		}

		public static IEnumerable<IColumnIndexer> GetAllHandlingUnits(WhsItemPackageState packageState, UniversalObjectFactory factory)
		{
			return GetNextLevelHandlingUnitPackage(packageState, factory).Except(packageState) ?? Enumerable.Empty<WhsItemPackageState>();
		}

		static IEnumerable<IColumnIndexer> GetNextLevelHandlingUnitPackage(WhsItemPackageState packageState, UniversalObjectFactory factory)
		{
			var result = new List<IColumnIndexer> { packageState };
			var handlingUnit = GetHandlingUnit(packageState, factory);
			if (handlingUnit != null)
			{
				result.AddRange(GetNextLevelHandlingUnitPackage(handlingUnit, factory));
			}

			return result;
		}

		static WhsItemPackageState GetHandlingUnit(WhsItemPackageState packageState, UniversalObjectFactory factory)
		{
			var divotQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit);
			divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, packageState[WhsItemPackageStateSchema.Constants.WPS_KP_Package]);
			divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);

			var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			query.AddSubQuery(WhsItemPackageStateSchema.WPS_KP_Package, divotQuery, JoinCondition.And);

			return factory.BOFactory.Load<WhsItemPackageState>(query).SingleOrDefault();
		}

		public static IEnumerable<WhsItemPackageState> GetAllHandlingUnitChildPackages(WhsItemPackageState handlingUnit, UniversalObjectFactory factory)
		{
			IEnumerable<WhsItemPackageState> handlingUnitChildPackages = null;

			if (handlingUnit.WPS_IsHandlingUnit)
			{
				var package = GetPkgPackage(handlingUnit, factory);
				if (package.KP_KP_TopHandlingUnitPackage == ZGuid.Empty)
				{
					var packageQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.PK);
					packageQuery.AddToFilter(PkgPackageSchema.KP_KP_TopHandlingUnitPackage, handlingUnit.WPS_KP_Package);

					var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
					query.AddSubQuery(WhsItemPackageStateSchema.WPS_KP_Package, packageQuery, JoinCondition.And);

					handlingUnitChildPackages = factory.BOFactory.Load<WhsItemPackageState>(query);
				}
				else
				{
					handlingUnitChildPackages = GetChildPackagesForSubLevelHandlingUnit(handlingUnit, factory).Except(handlingUnit);
				}
			}

			return handlingUnitChildPackages ?? Enumerable.Empty<WhsItemPackageState>();
		}

		static IEnumerable<WhsItemPackageState> GetChildPackagesForSubLevelHandlingUnit(WhsItemPackageState handlingUnit, UniversalObjectFactory factory)
		{
			var result = new List<WhsItemPackageState> { handlingUnit };
			var directlyChildPackages = GetHandlingUnitChildPackages(handlingUnit, factory);
			foreach (var child in directlyChildPackages)
			{
				result.AddRange(GetChildPackagesForSubLevelHandlingUnit(child, factory));
			}

			return result;
		}

		static IEnumerable<WhsItemPackageState> GetHandlingUnitChildPackages(WhsItemPackageState handlingUnit, UniversalObjectFactory factory)
		{
			IEnumerable<WhsItemPackageState> handlingUnitChildPackages = null;

			if (handlingUnit.WPS_IsHandlingUnit)
			{
				var divotQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
				divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, handlingUnit.WPS_KP_Package);
				divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);

				var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
				query.AddSubQuery(WhsItemPackageStateSchema.WPS_KP_Package, divotQuery, JoinCondition.And);

				handlingUnitChildPackages = factory.BOFactory.Load<WhsItemPackageState>(query);
			}

			return handlingUnitChildPackages ?? Enumerable.Empty<WhsItemPackageState>();
		}

		public static IColumnIndexer GetIncompleteTransferLine(WhsItemPackageState packageState, UniversalObjectFactory factory)
		{
			var incompleteTransferLineQuery = new ZDBOnlyQuery(typeof(WhsItemTransferLine));
			incompleteTransferLineQuery.AddToFilter(JoinCondition.And, WhsItemTransferLineSchema.WTF_PutTime, null);
			incompleteTransferLineQuery.AddToFilter(JoinCondition.And, WhsItemTransferLineSchema.WTF_WPS_PackageState, packageState.PK);

			var incompleteTransferLine = factory.RowFactory.Load(WhsItemTransferLineSchema.Constants.TableName, incompleteTransferLineQuery).FirstOrDefault();
			if (incompleteTransferLine == null)
			{
				var package = DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.LoadFromPK(PkgPackageSchema.Constants.TableName, packageState.WPS_KP_Package));

				if (package[PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage] != DBNull.Value)
				{
					var pkgPackageQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.PK);
					pkgPackageQuery.AddToFilter(WhsItemPackageStateSchema.WPS_KP_Package, package[PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage]);

					incompleteTransferLineQuery = new ZDBOnlyQuery(typeof(WhsItemTransferLine));
					incompleteTransferLineQuery.AddToFilter(JoinCondition.And, WhsItemTransferLineSchema.WTF_PutTime, null);
					incompleteTransferLineQuery.AddSubQuery(pkgPackageQuery, JoinCondition.And);
				}

				incompleteTransferLine = factory.RowFactory.Load(WhsItemTransferLineSchema.Constants.TableName, incompleteTransferLineQuery).FirstOrDefault();
			}

			return DataObjectReader.GetColumnIndexerFromRow(incompleteTransferLine);
		}

		public static WhsItemReceiveTransportationUnit GetReceiveTransportationUnit(WhsItemPackageState packageState, UniversalObjectFactory factory)
		{
			return factory.BOFactory.Load<WhsItemReceiveTransportationUnit>(packageState.WPS_WRH_TransitReceiveHeader);
		}

		static PkgPackage GetPkgPackage(WhsItemPackageState packageState, UniversalObjectFactory factory)
		{
			return factory.BOFactory.Load<PkgPackage>(packageState.WPS_KP_Package);
		}

		public static List<WhsItemPackageState> GetPackageStateBOs(IEnumerable<IColumnIndexer> packageStateIndexers, UniversalObjectFactory factory)
		{
			var result = new List<WhsItemPackageState>();
			if (packageStateIndexers != null && packageStateIndexers.Any())
			{
				var pkgStateIDs = packageStateIndexers?.Select(c => c.GetValue(WhsItemPackageStateSchema.PK));
				var pkgStateQuery = new ZQuery(WhsItemPackageStateSchema.PK, pkgStateIDs);
				result = factory.BOFactory.Load<WhsItemPackageState>(pkgStateQuery).ToList();
				// updating rows where the BizO is already loaded in the same factory causes issues
				// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
				result.ForEach(p =>
				{
					p.HasChanges = true;
				});
			}
			return result;
		}

		public static IColumnIndexer[] GetPackageStateIndexers(IEnumerable<ZGuid> packageStatePKs, UniversalObjectFactory factory)
		{
			var packageStateQuery = new ZQuery(WhsItemPackageStateSchema.PK, packageStatePKs);
			return Array.ConvertAll(factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, packageStateQuery), DataObjectReader.GetColumnIndexerFromRow);
		}
	}
}
