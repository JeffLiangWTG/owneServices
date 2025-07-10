using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PackagePackingHelper
	{
		#region PackagePackingHelper

		public PackagePackingHelper(PkgPackage package)
		{
			Package = Argument.NotNull(package, nameof(package));
		}
		readonly PkgPackage Package;

		#endregion

		#region GetString

		string NotPrinted => Res.GetString("4f0e83b4-76ed-4379-a360-1a5b95ecef95", "Not Printed");
		string Packed => Res.GetString("8e524324-ab3e-4a9b-afd6-9507f01e1cf8", "Packed");
		string Printed => Res.GetString("a07d4875-9df9-4aa7-a983-6aaa652e9800", "Printed");
		string Departed => Res.GetString("0bfa7f3a-a32d-43f7-aedc-fea13b9a1cb8", "Departed");
		string Loaded => Res.GetString("769db6a4-f538-4bd1-832a-3971bb204df3", "Loaded");
		string Picked => Res.GetString("e83fc5af-4d64-49f6-8004-4e9e9219664e", "Picked");
		string InPicking => Res.GetString("5ce8943c-f8a4-4b38-88cc-fc4e844fea5e", "In Picking");
		string Staged => Res.GetString("79acfe9f-8e4a-40d1-ae56-478ff921d479", "Staged");
		string ReadyToPack => Res.GetString("185a3122-24e7-4c2b-9432-6b3851916a5d", "Ready To Pack");

		#endregion

		#region GetPickLineFromPackage

		public WhsPickLine GetPickLineFromPackage()
		{
			WhsPickLine result = null;

			if (Package.PackedItemDivots.Count == 1)
			{
				result = GetPickLineFromPackedItem(Package.PackedItemDivots[0].KI_ParentID);
			}
			else
			{
				result = GetPickLinesFromPackage().OrderBy(pl => pl.InventoryLineForAvailableInventory.Location?.WLV_PickPathSequence ?? 0).FirstOrDefault();
			}

			return result;
		}

		WhsPickLine GetPickLineFromPackedItem(ZGuid parentID) => Package.Factory.Load<WhsPickLine>(parentID);

		#endregion

		#region GetPackageLabelStatus

		public string GetPackageLabelStatus()
		{
			// Not Printed => Printed => [In Picking] => Picked => Ready To Pack => Packed => Staged => Loaded => Departed

			var loadPkgPackagePivot = new Lazy<WhsLoadPkgPackagePivot>(() => GetLoadPkgPackagePivot());
			var pickLines = new Lazy<WhsPickLine[]>(() => GetPickLinesFromPackage());

			string result;

			if (IsDeparted(loadPkgPackagePivot.Value))
			{
				result = Departed;
			}
			else if (IsLoaded(loadPkgPackagePivot.Value))
			{
				result = Loaded;
			}
			else if (IsStaged(pickLines.Value))
			{
				result = Staged;
			}
			else if (IsPacked)
			{
				result = Packed;
			}
			else if (IsReadyToPack(pickLines.Value))
			{
				result = ReadyToPack;
			}
			else if (IsPicked(pickLines.Value))
			{
				result = Picked;
			}
			else if (IsInPicking(pickLines.Value))
			{
				result = InPicking;
			}
			else if (IsLabelPrinted)
			{
				result = Printed;
			}
			else
			{
				result = NotPrinted;
			}
			return result;
		}

		bool IsPacked => Package.IsClosed;
		bool IsLabelPrinted => Package.IsLabelPrinted;
		bool IsDeparted(WhsLoadPkgPackagePivot loadPkgPackagePivot) => loadPkgPackagePivot?.Load.WLO_GateOutTime.IsValid ?? false;
		bool IsLoaded(WhsLoadPkgPackagePivot loadPkgPackagePivot) => loadPkgPackagePivot?.IsLoaded ?? false;
		bool IsPicked(WhsPickLine[] pickLines) => pickLines.All(pl => pl.IsPickedFromPutawayLocation);
		bool IsInPicking(WhsPickLine[] pickLines) => pickLines.Any(pl => !pl.WZ_GS_NKAssignedTo.IsEmpty);
		bool IsStaged(WhsPickLine[] pickLines) => pickLines.All(pl => pl.Inventory.WI_InventoryStatus == InventoryStatus.Codes.Staged);
		bool IsReadyToPack(WhsPickLine[] pickLines) => pickLines.All(pl => pl.Inventory.WI_InventoryStatus == InventoryStatus.Codes.ReadyToPack);

		#endregion

		#region GetDocketIDFromPackage

		public ZString GetDocketIDFromPackage() => ((WhsOrder)Package?.PackageJob?.ParentJob)?.WD_DocketID ?? ZString.Empty;

		#endregion

		#region GetPackagePickArea

		public ZString GetPackagePickArea()
		{
			var pickLine = GetPickLineFromPackage();
			return pickLine?.InventoryLineForAvailableInventory.LocationAreaName ?? ZString.Empty;
		}

		#endregion

		#region GetUserAssignedToPackage

		public ZString GetUserAssignedToPackage()
		{
			var pickLinePairs = GetPickLinesFromPackage().Select(pickLine => PickLinePair.New(pickLine).AssignedToCode).Where(o => !o.IsEmpty);
			return pickLinePairs.GetSingleValueOrManyText(o => o, Res.GetString("522386fa-7cda-4480-b132-a3a2223d4934", "Multiple"));
		}

		#endregion

		#region AddFetchHintsWhenDeletingPackageJobFromPickableDockets

		public static void AddFetchHintsWhenDeletingPackageJobFromPickableDockets(BusinessObjectFactory factory, WhsPickableDocket[] pickableDockets)
		{
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WD, pickableDockets.Select(o => o.PK)));

			var docketLine = pickableDockets.SelectMany(o => o.AllLines);
			AddDocketLineFetchHints(factory, docketLine);
			AddPickLineFetchHints(factory, docketLine);

			var ordersToRemove = pickableDockets.Where(o => o is WhsOrder).Cast<WhsOrder>().ToArray();
			AddPackageJobFetchHints(factory, ordersToRemove);

			var packages = ordersToRemove.SelectMany(o => o.PackageJob?.Packages ?? Enumerable.Empty<PkgPackage>());
			AddPackageFetchHints(factory, packages);
			AddPackageDivotFetchHints(factory, packages);
			AddLoadPkgPackagePivotFetchHints(factory, packages);
		}

		static void AddDocketLineFetchHints(BusinessObjectFactory factory, IEnumerable<WhsDocketLine> docketLine)
		{
			var docketLinePKs = docketLine.Select(d => d.PK);
			factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, docketLinePKs));
			factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.Instance, new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, docketLinePKs));
		}

		static void AddPickLineFetchHints(BusinessObjectFactory factory, IEnumerable<WhsDocketLine> docketLine)
		{
			var pickLinePKs = docketLine.SelectMany(dl => dl.PickLines).Select(pl => pl.PK);
			factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.PK, pickLinePKs));

			foreach (var pickLinePK in pickLinePKs)
			{
				AddUniversalCopyFetchHint(factory, WhsPickLineSchema.Constants.Prefix, pickLinePK);
				factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentID, pickLinePK));
				factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentRelatedID, pickLinePK));
			}
		}

		static void AddPackageJobFetchHints(BusinessObjectFactory factory, WhsOrder[] ordersToRemove)
		{
			factory.AddFetchHint(PkgPackageJobSchema.Instance, new ZQuery(PkgPackageJobSchema.KJ_ParentID, ordersToRemove.Select(o => o.PK)));

			var packageJobPKs = ordersToRemove.Select(o => o.PackageJob?.PK ?? ZGuid.Empty).Where(pk => !pk.IsEmpty).ToArray();
			factory.AddFetchHint(JobDocumentDeliverySchema.Instance, new ZQuery(JobDocumentDeliverySchema.JDC_ParentID, packageJobPKs));
			factory.AddFetchHint(StmNoteSchema.Instance, new ZQuery(StmNoteSchema.ST_ParentID, packageJobPKs));
			factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentID, packageJobPKs));
			factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentRelatedID, packageJobPKs));
			factory.AddFetchHint(JobDocumentExclusionSchema.Instance, new ZQuery(JobDocumentExclusionSchema.JDE_ParentID, packageJobPKs));
			factory.AddFetchHint(PkgPackageJobPackageHeaderPivotSchema.Instance, new ZQuery(PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, packageJobPKs));
			foreach (var packageJobPK in packageJobPKs)
			{
				AddUniversalCopyFetchHint(factory, PkgPackageJobSchema.Constants.Prefix, packageJobPK);
			}
		}

		static void AddPackageFetchHints(BusinessObjectFactory factory, IEnumerable<PkgPackage> packages)
		{
			var packagePKs = packages.Select(p => p.PK);
			factory.AddFetchHint(JobDocumentDeliverySchema.Instance, new ZQuery(JobDocumentDeliverySchema.JDC_ParentID, packagePKs));
			factory.AddFetchHint(WhsPickByLabelLabelSchema.Instance, new ZQuery(WhsPickByLabelLabelSchema.WTL_KP_Package, packagePKs));
			factory.AddFetchHint(WhsPickTrolleySlotSchema.Instance, new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, packagePKs));
			factory.AddFetchHint(PkgPackageBookedDetailSchema.Instance, new ZQuery(PkgPackageBookedDetailSchema.KPB_KP_Package, packagePKs));
			factory.AddFetchHint(StmDefaultPrinterSchema.Instance, new ZQuery(StmDefaultPrinterSchema.SDP_SubjectID, packagePKs));
			factory.AddFetchHint(GenAddOnColumnSchema.Instance, new ZQuery(GenAddOnColumnSchema.XA_ParentID, packagePKs));

			var packageHeaderPKs = packages.Select(p => p.KP_KPH_PackageHeader);
			factory.AddFetchHint(JobDocumentDeliverySchema.Instance, new ZQuery(JobDocumentDeliverySchema.JDC_ParentID, packageHeaderPKs));
			factory.AddFetchHint(PkgPackageJobPackageHeaderPivotSchema.Instance, new ZQuery(PkgPackageJobPackageHeaderPivotSchema.KPJ_KPH_PackageHeader, packageHeaderPKs));
			factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentID, packageHeaderPKs));
			factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentRelatedID, packageHeaderPKs));

			foreach (var package in packages)
			{
				ZGuid[] pks = { package.KP_KPH_PackageHeader, package.PK };
				factory.AddFetchHint(JobDocumentExclusionSchema.Instance, new ZQuery(JobDocumentExclusionSchema.JDE_ParentID, pks));
				factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentID, pks));
				factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentRelatedID, pks));
				factory.AddFetchHint(StmNoteSchema.Instance, new ZQuery(StmNoteSchema.ST_ParentID, pks));
				factory.AddFetchHint(JobServiceLinkSchema.Instance, new ZQuery(JobServiceLinkSchema.ESL_ParentID, package.PK));

				AddUniversalCopyFetchHint(factory, PkgPackageSchema.Constants.Prefix, package.PK);
				AddUniversalCopyFetchHint(factory, PkgPackageHeaderSchema.Constants.Prefix, package.KP_KPH_PackageHeader);

				var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, package.PK);
				query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, PkgPackageSchema.Constants.Prefix);
				var addOnColumn = factory.LoadTop1<GenAddOnColumn>(query);
				if (addOnColumn != null)
				{
					AddUniversalCopyFetchHint(factory, GenAddOnColumnSchema.Constants.Prefix, addOnColumn.PK);
					factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentID, addOnColumn.PK));
					factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentRelatedID, addOnColumn.PK));
				}
			}
		}

		static void AddPackageDivotFetchHints(BusinessObjectFactory factory, IEnumerable<PkgPackage> packages)
		{
			var divotPKs = packages.SelectMany(p => p.PackedItemDivots).Select(p => p.PK);
			factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentID, divotPKs));
			factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentRelatedID, divotPKs));
			factory.AddFetchHint(StmNoteSchema.Instance, new ZQuery(StmNoteSchema.ST_ParentID, divotPKs));
			foreach (var divotPK in divotPKs)
			{
				AddUniversalCopyFetchHint(factory, PkgPackageItemDivotSchema.Constants.Prefix, divotPK);
			}
		}

		static void AddLoadPkgPackagePivotFetchHints(BusinessObjectFactory factory, IEnumerable<PkgPackage> packages)
		{
			foreach (var package in packages)
			{
				var pivotQuery = new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, package.PK);
				pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);
				factory.AddFetchHint(WhsLoadPkgPackagePivotSchema.Instance, pivotQuery);
			}
		}

		static void AddUniversalCopyFetchHint(BusinessObjectFactory factory, ZString parentTableCode, ZGuid parentPK)
		{
			var query = new ZQuery(StmUniversalCopySchema.SUC_CopyObjectTableCode, parentTableCode);
			query.AddToFilter(StmUniversalCopySchema.SUC_CopyObjectId, parentPK);
			factory.AddFetchHint(StmUniversalCopySchema.Instance, query);
		}

		#endregion

		#region LoadPkgPackagePivot

		WhsLoadPkgPackagePivot GetLoadPkgPackagePivot()
		{
			var pivotQuery = new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, Package.PK);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);

			return Package.Factory.Load<WhsLoadPkgPackagePivot>(pivotQuery).SingleOrDefault();
		}

		#endregion

		#region PickLines

		public WhsPickLine[] GetPickLinesFromPackage()
		{
			return Package.PackedItemDivots.Select(pi => GetPickLineFromPackedItem(pi.KI_ParentID)).WhereNotNull().ToArray();
		}

		#endregion
	}
}
