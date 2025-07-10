using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public sealed class PutawayHelper
	{
		PutawayHelper()
		{
		}

		public static void SetReceiveArrivalDate(IEnumerable<WhsDocket> receives)
		{
			var arrivalDate = ZDateTimeOffset.Now;
			receives.ForEach(r =>
			{
				if (r.WD_ArrivalDate.IsEmpty)
				{
					r.WD_ArrivalDate = arrivalDate;
				}
			});
		}

		public static bool AreAllPutawayTransfersUnfinalized(WebServiceResponse response, IEnumerable<WhsInventoryView> inventoryCollection, string palletID)
		{
			return AreAllPutawayTransfersUnfinalized(response, inventoryCollection.Select(inventory => GetPutawayTransferLineFromInventory(inventory)), palletID);
		}

		public static bool AreAllPutawayTransfersUnfinalized(WebServiceResponse response, IEnumerable<WhsTransferLine> transferLines, string palletID)
		{
			return AreAllPutawayTransfersUnfinalizedInternal(response, transferLines,
				Res.GetString("a872328c-cefa-4244-9281-0ea66b5cd677", "Putaway has been already completed for the Pallet ID {0}.", palletID));
		}

		public static bool AreAllPutawayTransfersUnfinalized(WebServiceResponse response, IEnumerable<WhsTransferLine> transferLines)
		{
			return AreAllPutawayTransfersUnfinalizedInternal(response, transferLines,
				Res.GetString("76b505a8-f690-4f4b-ae8d-6b8e029a6d3d", "Putaway has been already completed for one or more pallet ids"));
		}

		static bool AreAllPutawayTransfersUnfinalizedInternal(WebServiceResponse response, IEnumerable<WhsTransferLine> transferLines, string errorMessage)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage) && transferLines.Any(x => x?.IsFinalised ?? false))
			{
				response.ErrorMessage = errorMessage;
				response.Error = ErrorTypes.BusinessValidationError;
			}
			return string.IsNullOrEmpty(response.ErrorMessage);
		}

		public static WhsTransferLine GetPutawayTransferLineFromInventory(WhsInventoryView inventory)
		{
			WhsTransferLine result = null;
			var docketLine = inventory.InDocketLine;

			if (docketLine is WhsReceiveLine receiveLine)
			{
				result = receiveLine.PutawayTransferLine;
			}
			else if (docketLine is WhsTransferLine transferLine)
			{
				result = transferLine.IsPutawayTransferLine ? transferLine : null;
			}

			return result;
		}

		public static WhsPutawayJob CreateFinalisedPutawayJobWithPutawayLine(BusinessObjectFactory factory, WhsWarehouse warehouse, GlbStaff staff, string palletID)
		{
			var putawayJob = CreatePutawayJob(factory, warehouse, staff);
			var putawayLine = factory.New<WhsPutawayLine>();
			putawayLine.WPL_WPJ_PutawayJob = putawayJob.PK;
			putawayLine.WPL_PalletID = palletID;
			putawayLine.WPL_IsFinalized = true;

			// Finalise Job after complete job and line creation.
			putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.UtcNow;

			return putawayJob;
		}

		public static WhsPutawayJob CreatePutawayJob(BusinessObjectFactory factory, WhsWarehouse warehouse, GlbStaff staff)
		{
			var putawayJob = factory.New<WhsPutawayJob>();
			putawayJob.WPJ_GS_NKUser = staff.GS_Code;
			putawayJob.WPJ_WW_Warehouse = warehouse.PK;
			return putawayJob;
		}

		public static WhsPutawayJob LoadUnfinalisedPutawayJob(BusinessObjectFactory factory, WhsWarehouse warehouse, GlbStaff staff)
			=> factory.LoadTop1<WhsPutawayJob>(AddUnfinalisedPutawayJobFilters(warehouse, staff, new ZQuery()));

		public static IEnumerable<WhsPutawayLine> LoadUnfinalisedPutawayLines(BusinessObjectFactory factory, WhsWarehouse warehouse, GlbStaff staff)
		{
			var jobQuery = GetPutawayJobQuery(warehouse, staff);
			var lineQuery = new ZDBOnlyQuery(typeof(WhsPutawayLine));
			lineQuery.AddToFilter(WhsPutawayLineSchema.WPL_IsFinalized, false);
			lineQuery.AddSubQuery(WhsPutawayLineSchema.WPL_WPJ_PutawayJob, jobQuery, JoinCondition.And);
			return factory.Load<WhsPutawayLine>(lineQuery);
		}

		public static ZDBOnlySubQuery GetPutawayJobQuery(WhsWarehouse warehouse, GlbStaff staff)
			=> AddUnfinalisedPutawayJobFilters(warehouse, staff, new ZDBOnlySubQuery(typeof(WhsPutawayJob), WhsPutawayJobSchema.PK));

		public static T AddUnfinalisedPutawayJobFilters<T>(WhsWarehouse warehouse, GlbStaff staff, T jobQuery) where T : ZQuery
		{
			jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_WW_Warehouse, warehouse.PK);
			jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_GS_NKUser, staff.GS_Code);
			jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_FinalizedTimeUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);
			return jobQuery;
		}

		public static ZQuery FindReceiveLineQuery(IEnumerable<string> palletIDs, ZGuid whsPK)
		{
			var docketQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK);
			docketQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, whsPK);

			var query = new ZDBOnlyQuery(typeof(WhsDocketLine));
			AddCommonFiltersToReceiveLineQuery(query, palletIDs);
			query.AddSubQuery(WhsDocketLineSchema.WE_WD, docketQuery, JoinCondition.And);
			return query;
		}

		public static ZQuery FindInventoryQuery(string[] palletIDs, ZGuid whsPK)
		{
			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_InDocketLineType, DocketType.Codes.Receive);
			inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, whsPK);

			var receiveLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.PK);

			AddCommonFiltersToReceiveLineQuery(receiveLineSubQuery, palletIDs);
			inventoryQuery.AddSubQuery(receiveLineSubQuery, JoinCondition.And);
			inventoryQuery.ReLoadExistingRows = true;

			return inventoryQuery;
		}

		static void AddCommonFiltersToReceiveLineQuery(ZDBOnlyQuery receiveLineQuery, IEnumerable<string> palletIDs)
		{
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_PalletID, palletIDs);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, SQLComparisonOperator.Equal, DocketType.Codes.Receive);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Cancelled);
		}

		public static void CreatePutawayLinesOnPutawayJob(WhsWarehouse warehouse, GlbStaff staff, WhsPutawayJob putawayJob, IEnumerable<WhsInventoryView> inventories)
		{
			foreach (var inv in inventories)
			{
				var putawayLine = putawayJob.Lines.AddNew();
				putawayLine.WPL_PalletID = inv.WI_PalletID;
				putawayLine.WPL_IsPuttingAway = true;
			}
		}

		public static void CheckIfPalletIDsNotOnAnotherPutawayJob(WebServiceResponse response, IEnumerable<WhsPutawayLine> lines, bool isReassigning)
		{
			if (lines.Any())
			{
				var linesInPuttingAway = lines.Where(line => line.WPL_IsPuttingAway);
				if (linesInPuttingAway.Any())
				{
					response.LogBusinessValidationError(Res.GetString("18bb6367-d76b-4395-9193-c2986bdc691f", "Entered Pallet ID(s) '{0}' are already assigned to another user and are currently being put away.", string.Join(", ", linesInPuttingAway.Select(l => l.WPL_PalletID))));
				}
				else if (!isReassigning)
				{
					response.LogError(ErrorTypes.YesNoEnquiry, Res.GetString("725a0fb3-7073-48ba-b444-1dabd75813ce", "Entered Pallet ID(s) '{0}' are already assigned to another user. Do you want to reassign the ID(s) to your Putaway Job?", string.Join(", ", lines.Select(l => l.WPL_PalletID))));
				}
			}
		}

		public static IEnumerable<WhsPutawayLine> LoadPutawayLinesFromDifferentJobsWithMatchingPalletIDs(BusinessObjectFactory factory, WhsWarehouse warehouse, GlbStaff staff, string[] palletIDs)
		{
			var jobQuery = PreparePutawayJobQuery(warehouse);
			jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_GS_NKUser, SQLComparisonOperator.NotEqual, staff.GS_Code);
			return LoadPutawayLinesForPalletIDs(factory, palletIDs, jobQuery);
		}
		public static IEnumerable<WhsPutawayLine> LoadPutawayLinesWithMatchingPalletIDs(BusinessObjectFactory factory, WhsWarehouse warehouse, string[] palletIDs)
		{
			var jobQuery = PreparePutawayJobQuery(warehouse);
			return LoadPutawayLinesForPalletIDs(factory, palletIDs, jobQuery);
		}

		static ZDBOnlySubQuery PreparePutawayJobQuery(WhsWarehouse warehouse)
		{
			var jobQuery = new ZDBOnlySubQuery(typeof(WhsPutawayJob), WhsPutawayJobSchema.PK);
			jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_WW_Warehouse, warehouse.PK);
			jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_FinalizedTimeUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);
			return jobQuery;
		}

		static IEnumerable<WhsPutawayLine> LoadPutawayLinesForPalletIDs(BusinessObjectFactory factory, string[] palletIDs, ZDBOnlySubQuery jobQuery)
		{
			var lineQuery = new ZDBOnlyQuery(typeof(WhsPutawayLine));
			lineQuery.AddToFilter(WhsPutawayLineSchema.WPL_PalletID, palletIDs);
			lineQuery.AddSubQuery(WhsPutawayLineSchema.WPL_WPJ_PutawayJob, jobQuery, JoinCondition.And);
			return factory.Load<WhsPutawayLine>(lineQuery);
		}

		public static void DeleteLinesWhichAreNotPuttingAway(IEnumerable<WhsPutawayLine> lines)
		{
			lines.Where(line => !line.WPL_IsPuttingAway).ForEach(l => l.Delete());
		}

		public static void PreparePutawayTransferLinesForReallocation(WhsReceiveLine[] lines, BusinessObjectFactory factory, IEnumerable<ZGuid> skipLocationPKs)
		{
			lines.ForEach(x =>
			{
				var putawayTransferLine = x.PutawayTransferLine;
				if (putawayTransferLine != null)
				{
					if (!putawayTransferLine.WE_PalletID.EqualsIgnoringCase(x.WE_PalletID))
					{
						putawayTransferLine.WE_PalletID = x.WE_PalletID;
					}
					putawayTransferLine.WE_WL = ZGuid.Empty;
				}
			});

			var createCycleCount = lines
				.DistinctBy(l => l.WE_WD)
				.Select(l => l.Docket)
				.DistinctBy(d => new { d.WD_WW_Whs, d.WD_OH_Client, d.WD_ReceiveCategory })
				.Any(d => WhsClientParams.GetClientParams(d.Client).ClientParametersByWarehouse
					.FindWithEmptyFallback(d.WD_OH_Client, d.WD_WW_Whs, d.WD_ReceiveCategory)?.WY_CycleCountOnAlternatePutaway ?? false);

			if (createCycleCount)
			{
				var cycleCountTaskCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>();
				cycleCountTaskCreator.CreateCycleCountLocations(factory, skipLocationPKs, priority: 1);
			}
		}

		public static void AllocateInventoryLocationsWithConcurrencyHandling(
			Action<WebServiceResponse, BusinessObjectFactory, bool> allocateAction,
			WebServiceResponse response,
			BusinessObjectFactory originalFactory)
		{
			const int maxRetry = 5;
			var retryAttempt = 0;

			while (retryAttempt < maxRetry)
			{
				try
				{
					var isRetry = retryAttempt > 0;
					using (isRetry ? Db.DisposableActionForDbConnection() : null)
					{
						var factory = isRetry ? new BusinessObjectFactory() : originalFactory;
						allocateAction(response, factory, isRetry);

						if (response.NoError() || (response.Error == ErrorTypes.WarningOnly && isRetry))
						{
							try
							{
								factory.Save();
							}
							catch (ZCannotSaveException ex)
							{
								response.LogBusinessValidationError(ex.Message);
							}
							catch (ZSaveConcurrencyException)
							{
								throw new PutawayAllocateLocationConcurrencyException();
							}
						}
						else if (!isRetry) // On the first attempt, the cache may be stale so we should rebuild and retry if any errors or warnings occur.
						{
							retryAttempt++;
							response.Error = ErrorTypes.None;
							response.ErrorMessage = "";
							continue;
						}
					}
				}
				catch (PutawayAllocateLocationConcurrencyException)
				{
					retryAttempt++;
					continue;
				}

				break;
			}

			if (retryAttempt == maxRetry)
			{
				response.LogBusinessValidationError(PutawayHelper.ReceiveConcurrencyError);
			}
		}

		internal static void CheckIfPalletIDsIsHeldForPutaway(WebServiceResponse response, WhsInventoryView[] inventories)
		{
			if (response.NoError())
			{
				var heldPutawayReceives = inventories?.DistinctBy(i => i.WI_WD).Select(i => i.Docket).Where(d => d.WD_HoldPalletIDPutaway).Select(d => d.WD_DocketID).ToArray();
				if (heldPutawayReceives != null && heldPutawayReceives.Length > 0)
				{
					var allHeldPutawayReceiveIDs = string.Join("\r\n", heldPutawayReceives);
					response.LogBusinessValidationError(Res.GetString("094FACBD-E2CF-441C-953A-AC916AF263DB", "A desktop user will need to audit the unloaded pallets for the following receive(s):\r\n{0}", allHeldPutawayReceiveIDs));
				}
			}
		}

		#region Errors

		static ZString ReceiveConcurrencyError => Res.GetString("e04f4444-4938-45e7-b81a-de074f590c65", "Another user has changed the Receive Job while you have been working on it. Please restart the operation and try again.");

		#endregion
	}
}
