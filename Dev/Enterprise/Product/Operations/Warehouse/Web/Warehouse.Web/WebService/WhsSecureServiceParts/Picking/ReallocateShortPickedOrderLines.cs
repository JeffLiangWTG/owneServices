using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region ReallocateShortPickedOrderLines

		[WebMethod(Description = "Reallocate short picked Order Lines")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickWebServiceResponse ReallocateShortPickedOrderLines(Guid pickPK, Guid[] shortedOrderLinePKs, Guid taskPK)
		{
			return HandleWebServiceRequest<WhsPickWebServiceResponse>(response => ReallocateShortPickedOrderLines(response, pickPK, shortedOrderLinePKs, taskPK));
		}

		void ReallocateShortPickedOrderLines(WhsPickWebServiceResponse response, Guid pickPK, Guid[] shortedOrderLinePKs, Guid taskPK)
		{
			var pick = Factory.Load<WhsPick>(pickPK);
			if (pick == null)
			{
				response.LogBusinessValidationError(Res.GetString("115030ed-9aa8-4907-8bda-e4fc2118ad5d", "Pick was not found."));
			}
			else if (taskPK != Guid.Empty && Factory.Load<ProcessTask>(taskPK) == null)
			{
				response.LogBusinessValidationError(Res.GetString("d9e90f05-0f10-47c1-ba79-cb9c118f3797", "Task was not found."));
			}
			else
			{
				var newPickLines = Reallocate(response, shortedOrderLinePKs, pick, taskPK);
				if (newPickLines.Length > 0)
				{
					var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
					WebServiceHelper.AssignPickLinesToUserAndSave(response, pick.Factory, newPickLines, rfUser);

					var assignedPickLines = pick.GetAllPickLines().Where(l => l.WZ_IsPicking == true && l.WZ_GS_NKAssignedTo == rfUser.GS_Code).ToArray();
					AddFetchHintsForReallocatePick(assignedPickLines);

					response.Pick = new WhsPickInfo(pick, assignedPickLines, GetCompletePallets(pick));
				}
			}
		}

		static MultilingualString NoStockReallocatedMessage => ResString.GetMultilingualString("db0daac5-491b-4ad8-90e1-0d0f9698aadd", "No stock could be reallocated for the shorted products.");

		static WhsPickLine[] Reallocate(WebServiceResponse response, Guid[] shortedOrderLinePKs, WhsPick pick, Guid taskPK)
		{
			var newPickLines = Array.Empty<WhsPickLine>();
			var shortedOrderedInventories = pick.OrderedInventories.Where(o => shortedOrderLinePKs.Any(pk => o.Owners.FindByPK(new ZGuid(pk)) != null)).ToArray();
			if (shortedOrderedInventories.Length == 0)
			{
				response.LogBusinessValidationError(Res.GetString("4dfacd5d-61d5-4e02-acd7-e17b618cdf09", "Shorted Ordered Inventories not found."));
			}
			else
			{
				var notifications = new NotificationBuffer();
				var allocationResult = pick.AutoAllocateItems(shortedOrderedInventories, notifications);

				if (allocationResult == AllocationResult.AllocatedStock)
				{
					newPickLines = shortedOrderedInventories.SelectMany(o => o.PickLines.Where(pl => !pl.IsInDatabase)).ToArray();

					RecreatePickByBOMReceiveLine(pick, newPickLines.Select(pl => pl.DocketLine).Cast<WhsPickableDocketLine>());
				}
				else if (allocationResult == AllocationResult.NoStockAllocated)
				{
					response.LogBusinessValidationError(NoStockReallocatedMessage);
				}
				else
				{
					response.LogBusinessValidationError(notifications.AsString);
				}
			}

			if (taskPK != ZGuid.Empty)
			{
				newPickLines.ForEach(pl => pl.WZ_P9_Task = taskPK);
			}

			return newPickLines;
		}

		void AddFetchHintsForReallocatePick(IEnumerable<WhsPickLine> pickLines)
		{
			pickLines.Select(pl => pl.InventoryLine.WE_WD).Distinct()
				.ForEach(pk => Factory.AddFetchHint(typeof(JobDocAddress), FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, pk)));
		}

		static void RecreatePickByBOMReceiveLine(WhsPick pick, IEnumerable<WhsPickableDocketLine> reallocatedDocketLines)
		{
			var kitLinesMightBeReallocated = reallocatedDocketLines.Where(l => l.IsComponentLineOnSalesOrder)
				.DistinctBy(l => l.WE_WE_ParentDocketLine).Select(l => l.ParentLine).Cast<WhsOrderLine>().ToArray();

			var allReleaseLines = new Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>>(() => PackageHelper.GetReleaseLinesByKey(kitLinesMightBeReallocated));
			PickLineUpdater.RecreatePickByBOMReceiveLine(pick, kitLinesMightBeReallocated, allReleaseLines, isShorting: false);
		}

		#endregion
	}
}
