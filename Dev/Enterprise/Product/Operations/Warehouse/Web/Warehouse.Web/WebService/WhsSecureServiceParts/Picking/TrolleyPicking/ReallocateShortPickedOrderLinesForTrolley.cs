using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region ReallocateShortPickedOrderLinesForTrolley

		[WebMethod(Description = "Reallocate short picked Order Lines for Trolley Picking")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickTrolleyWebServiceResponse ReallocateShortPickedOrderLinesForTrolley(Guid trolleyJobPK, Guid packagePK, Guid[] shortedOrderLinePKs)
		{
			return HandleWebServiceRequest<WhsPickTrolleyWebServiceResponse>(response => ReallocateShortPickedOrderLinesForTrolley(response, trolleyJobPK, packagePK, shortedOrderLinePKs));
		}

		void ReallocateShortPickedOrderLinesForTrolley(WhsPickTrolleyWebServiceResponse response, Guid trolleyJobPK, Guid packagePK, Guid[] shortedOrderLinePKs)
		{
			var pickTrolleyJob = Factory.Load<WhsPickTrolleyJob>(trolleyJobPK);
			if (pickTrolleyJob == null)
			{
				response.LogBusinessValidationError(Res.GetString("60b2e551-456b-4b2d-afa1-8eb757c89075", "Trolley job was not found."));
			}
			else
			{
				var package = Factory.Load<PkgPackage>(packagePK);
				if (package == null)
				{
					response.LogBusinessValidationError(Res.GetString("98e09a6d-e43d-4ac0-86f8-07a809dfedf5", "Package was not found."));
				}
				else
				{
					var order = (WhsOrder)package.PackageJob.ParentJob;
					var newPickLines = Reallocate(response, shortedOrderLinePKs, order.Pick, Guid.Empty);
					if (newPickLines.Length > 0)
					{
						var newPickLinesPK = newPickLines.Select(pl => pl.PK).ToHashSet();
						var shortedOrderLinePKsSet = shortedOrderLinePKs.ToHashSet();
						var shortedOrderLines = order.Lines.Cast<WhsOrderLine>().Where(ol => shortedOrderLinePKsSet.Contains(ol.PK.ToGuid())).ToArray();

						AddFetchHintsForPackPickLines(order);
						PackPickLinesIntoPackage(shortedOrderLines, newPickLines, package);
						var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);

						var pickLinesToAssign = shortedOrderLines.SelectMany(l => l.PickLines).Where(l => newPickLinesPK.Contains(l.PK));
						WebServiceHelper.AssignPickLinesToUserAndSave(response, pickTrolleyJob.Factory, pickLinesToAssign, rfUser);

						var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);

						AddFetchHintsForCreatingTrolleyJob(pickTrolleyJob);

						var trolleyJobInfo = CreateTrolleyJobInfo(warehouse, pickTrolleyJob, pickTrolleyJob.Equipment.RQ_Registration, pickTrolleyJob.PickingType);

						var assignedPickLines = pickTrolleyJob.Slots.SelectMany(s => s.Package.GetPickLines())
							.Where(l => l.WZ_IsPicking == true && l.WZ_GS_NKAssignedTo == rfUser.GS_Code);

						var assignedPickLinesWithPackageIDAndSlots = GetPickLinesWithPackageIDAndSlots(Factory, assignedPickLines);
						trolleyJobInfo.SetLines(new WhsPickLineInfoCollection(assignedPickLinesWithPackageIDAndSlots, trolleyJobInfo));

						response.Job = trolleyJobInfo;
					}
				}
			}
		}

		void PackPickLinesIntoPackage(WhsOrderLine[] shortedOrderLines, IEnumerable<WhsPickLine> newPickLines, PkgPackage package)
		{
			var releaseLines = shortedOrderLines.SelectMany(l => l.ReleaseLines.Cast<WhsReleaseLine>());

			var releaseLinesByPackableItemKey = releaseLines.ToDictionary(r => r.KeyForPacking);
			foreach (var packableItem in newPickLines.Cast<IPackableItem>())
			{
				if (releaseLinesByPackableItemKey.TryGetValue(packableItem.Key, out var releaseLine) && releaseLine != null)
				{
					package.Pack(packableItem, releaseLine);
				}
			}
		}

		void AddFetchHintsForCreatingTrolleyJob(WhsPickTrolleyJob pickTrolleyJob)
		{
			pickTrolleyJob.Slots.ForEach(s =>
			{
				Factory.AddFetchHint(typeof(PkgPackage), PkgPackageSchema.PK, s.WTS_KP_Package);

				var query = new ZDBOnlyQuery(typeof(PkgPackageJob));
				var subQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
				subQuery.AddToFilter(PkgPackageSchema.PK, s.WTS_KP_Package);
				query.AddSubQuery(subQuery, JoinCondition.And);
				pickTrolleyJob.Factory.AddFetchHint(PkgPackageJobSchema.Instance, query);
			});

			pickTrolleyJob.Slots.ForEach(s => Factory.AddFetchHint(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, s.Package.PK));
		}

		void AddFetchHintsForPackPickLines(WhsOrder order)
		{
			foreach (var packableItem in order.PackageJob.PackableItemParents.Select(w => w.PackableItemParent).SelectMany(p => p.PackableItems))
			{
				Factory.AddFetchHint(PkgPackageItemDivotSchema.KI_ParentID, packableItem.PK);
			}
		}

		#endregion
	}
}
