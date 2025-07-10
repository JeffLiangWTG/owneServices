using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsVASOrderDataObjectReader : WhsDataObjectReader<WhsVASOrder>
	{
		internal WhsVASOrderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			Logger = logger;
		}

		IXmlImportLogger Logger { get; }

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseVASOrder; }
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsVASOrder targetBO)
		{
			var warehousePK = WarehousePK;
			var serviceAreaPK = GetServiceArea(warehousePK, dataObject.Order.StagingArea);
			var clientAddress = ClientAddress;
			var clientPK = clientAddress.GetValue(OrgAddressSchema.OA_OH);

			var vasOrderRow = GetColumnIndexerFromRow(targetBO);
			if (!IsNewBO && HasTransferStarted(vasOrderRow))
			{
				throw new DataObjectReadFailureException(Res.GetString("f1fa5319-f21a-4964-8092-5ef41b81daac", "{0} could not be updated because it has been commenced.", targetBO.HumanReadableName));
			}

			if (!IsOrderCancelled(targetBO))
			{
				var orderDataObject = dataObject.Order;
				var setOrderCancelled = false;

				if (orderDataObject != null)
				{
					setOrderCancelled = ShouldCancelOrder(orderDataObject, targetBO);
				}

				if (!IsNewBO)
				{
					// updating rows where the BizO is already loaded in the same factory causes issues
					// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
					targetBO.HasChanges = true;
				}

				if (setOrderCancelled)
				{
					SetValue(vasOrderRow, WhsVASOrderSchema.WVO_GS_NKCancelledBy, GlbStaff.CurrentUser.GS_Code);
					SetValue(vasOrderRow, WhsVASOrderSchema.WVO_CancelledTimeUtc, ZDateTime.UtcNow);
				}
				else
				{
					SetValue(vasOrderRow, WhsVASOrderSchema.WVO_WA_ServiceArea, serviceAreaPK);
					SetValue(vasOrderRow, WhsVASOrderSchema.WVO_OH_Client, clientPK);
					SetValue(vasOrderRow, WhsVASOrderSchema.WVO_CustomerReferenceNo, dataObject.Order.OrderNumber);
					AddServiceRequestedEvent(targetBO); // Create after setting Whs and Client

					var client = factory.Load<OrgHeader>(clientPK); // need Client OrgHeader BizO for Product Matcher on Lines Reader
					PopulateRelatedEntitites(vasOrderRow, client);
					DeleteInitialTransferIfExists(vasOrderRow);
				}
			}
		}

		#region OrderCancellation

		bool IsOrderCancelled(WhsVASOrder order)
		{
			if (order.IsCancelled)
			{
				var errorMessage = Res.GetString("AA73A507-C2DB-4DAF-89E9-58B38C66E821", "The job {0} - {1} has already been canceled and can not be updated.", order.GetType().Name, order.WVO_JobID);
				Logger.Log(LogType.Error, errorMessage);
				return true;
			}

			return false;
		}

		bool ShouldCancelOrder(Order orderDataObject, WhsVASOrder targetBO)
		{
			var status = orderDataObject.Status.GetCodeAsUpperCase();

			if (status == WhsVASOrderStatuses.Codes.Cancelled)
			{
				if (IsNewBO)
				{
					throw new DataObjectReadFailureException(Res.GetString("39C84EE1-BBE5-4BAB-99C9-FACBDA0A74A3", "Cannot cancel a new VAS Order '{0}'.", orderDataObject.OrderNumber));
				}
				else if (targetBO != null && !string.Equals(targetBO.Status, WhsVASOrderStatuses.Descriptions.Entered, System.StringComparison.OrdinalIgnoreCase))
				{
					throw new DataObjectReadFailureException(Res.GetString("092DE349-3164-4DE7-9083-5F0B7214993F", "You can only cancel VAS Orders with {0} status.", WhsVASOrderStatuses.Descriptions.Entered));
				}

				var errorMessage = Res.GetString("A2D58B33-6E51-4BE8-A659-9C7CB6983AD4", "The job {0} - {1} has been canceled and no other updates were made.", targetBO.GetType().Name, targetBO.WVO_JobID);
				Logger.Log(LogType.Information, errorMessage);

				return true;
			}

			return false;
		}

		#endregion

		#region HasTransferStarted

		bool HasTransferStarted(IColumnIndexer vasOrderRow)
		{
			bool result = false;

			var initialTransferPK = vasOrderRow.GetValue(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea);
			if (!initialTransferPK.IsEmpty)
			{
				var transfer = factory.RowFactory.LoadFromPK(WhsDocketSchema.Constants.TableName, initialTransferPK);
				result = transfer != null && (DoesTransferHaveServiceCommencedLog(initialTransferPK) || DoesTransferHaveFinalisedLine(initialTransferPK));
			}

			return result;
		}

		bool DoesTransferHaveServiceCommencedLog(ZGuid initialTransferPK)
		{
			var logQuery = new ZQuery();
			logQuery.AddToFilter(StmALogSchema.SL_Parent, initialTransferPK);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceCommencedCode);
			logQuery.MaximumRows = 1;

			return factory.RowFactory.Load(StmALogSchema.Constants.TableName, logQuery).Length > 0;
		}

		bool DoesTransferHaveFinalisedLine(ZGuid initialTransferPK)
		{
			var finalisedTransferLineQuery = new ZQuery();
			finalisedTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, initialTransferPK);
			finalisedTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, DocketLineStatus.Codes.Finalised);
			finalisedTransferLineQuery.MaximumRows = 1;

			return factory.RowFactory.Load(WhsDocketLineSchema.Constants.TableName, finalisedTransferLineQuery).Length > 0;
		}

		#endregion

		#region GetServiceArea

		ZGuid GetServiceArea(ZGuid warehouse, string areaName)
		{
			var query = new ZQuery(WhsAreaSchema.WA_WW_Whs, warehouse);
			query.AddToFilter(WhsAreaSchema.WA_Name, areaName);
			query.MaximumRows = 1;

			var serviceArea = factory.RowFactory.Load(WhsAreaSchema.Constants.TableName, query).SingleOrDefault();
			if (serviceArea == null)
			{
				var errorMessage = Res.GetString("a50ddc38-fa32-423f-ab0a-7207a607f88c", "Unable to match Service Area: {0}", areaName);
				throw new DataObjectReadFailureException(errorMessage);
			}

			return GetColumnIndexerFromRow(serviceArea).GetValue(WhsAreaSchema.PK);
		}

		#endregion

		#region AddServiceRequestedEvent

		void AddServiceRequestedEvent(WhsVASOrder vasOrder)
		{
			vasOrder.Logs.AddNew(Events.ServiceRequested); // Workflow etc. relies on BizOs, so the log must be added this way.
		}

		#endregion

		#region PopulateRelatedEntitites

		void PopulateRelatedEntitites(IColumnIndexer targetBO, OrgHeader client)
		{
			var orderLineCollection = dataObject.Order.OrderLineCollection;
			if (orderLineCollection != null)
			{
				var vasOrderLineCollectionReader = new WhsVASOrderLinesDataObjectCollectionReader(targetBO, orderLineCollection.ToArray(), this, client);
				vasOrderLineCollectionReader.ReadIntoCollection();
			}
		}

		#endregion

		#region DeleteInitialTransferIfExists

		void DeleteInitialTransferIfExists(IColumnIndexer vasOrderRow)
		{
			var initialTransferPK = vasOrderRow.GetValue(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea);
			if (!initialTransferPK.IsEmpty)
			{
				SetValue(vasOrderRow, WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, ZGuid.Empty);

				var transfer = factory.RowFactory.LoadFromPK(WhsDocketSchema.Constants.TableName, initialTransferPK);
				if (transfer != null)
				{
					// Will be brought back once architecture support running deferred triggers on delete of Rows, not on delete of Business objects (WI00126094)
					//DeleteTransferLines(initialTransferPK);

					// Architecture does many things like Deleting Notes and calling Delete Strategies which cannot easily be replicated here
					// so we load the transfer as a BizO to delete the remaining things until such time that Universal can Delete Parent Entities properly.
					var transferBO = factory.Load<WhsTransfer>(initialTransferPK);
					transferBO.Delete();
				}
			}
		}

		// Will be brought back once architecture support running deferred triggers on delete of Rows, not on delete of Business objects (WI00126094)
		//void DeleteTransferLines(ZGuid initialTransferPK)
		//{
		//	var query = new ZQuery();
		//	query.AddToFilter(WhsDocketLineSchema.WE_WD, initialTransferPK);

		//	var transferLines = factory.RowFactory.Load(WhsDocketLineSchema.Constants.TableName, query);
		//	foreach (var transferLine in transferLines)
		//	{
		//		var transferLineRow = GetColumnIndexerFromRow(transferLine);
		//		DeletePickLines(transferLineRow);
		//		DeleteRowAndSetHasChanges<WhsTransferLine>(transferLineRow, WhsDocketLineSchema.PK);
		//	}
		//}

		//void DeletePickLines(IColumnIndexer transferLine)
		//{
		//	var query = new ZQuery();
		//	query.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, transferLine.GetValue(WhsDocketLineSchema.PK));

		//	var pickLines = factory.RowFactory.Load(WhsPickLineSchema.Constants.TableName, query);

		//	foreach (var pickLine in pickLines)
		//	{
		//		DeleteRowAndSetHasChanges<WhsPickLine>(GetColumnIndexerFromRow(pickLine), WhsPickLineSchema.PK);
		//	}
		//}

		/// <summary>
		/// The Factory does not handle Deletes properly when a mix of Data Rows and BizOs are deleted, so until
		/// Universal does not require BizOs, we will load the BizO and manually set HasChanges.
		/// </summary>
		void DeleteRowAndSetHasChanges<T>(IColumnIndexer columnIndexer, SchemaPKColumn pkColumn)
			where T : BusinessObject
		{
			var pk = columnIndexer.GetValue(pkColumn);
			factory.Load<T>(pk).HasChanges = true;
			columnIndexer.Delete();
		}

		#endregion

		#endregion

		#region VASOrderLinesDataObjectCollectionReader

		class WhsVASOrderLinesDataObjectCollectionReader : DataObjectCollectionReader<OrderLine, WhsVASOrderLine>
		{
			internal WhsVASOrderLinesDataObjectCollectionReader(IColumnIndexer vasOrder, OrderLine[] orderLineDataObjects, WhsVASOrderDataObjectReader reader, OrgHeader client)
				: base(orderLineDataObjects)
			{
				VASOrder = Argument.NotNull(vasOrder, "vasOrder");
				Reader = Argument.NotNull(reader, "reader");
				Client = Argument.NotNull(client, "client");
			}

			readonly IColumnIndexer VASOrder;
			readonly WhsVASOrderDataObjectReader Reader;
			readonly OrgHeader Client;

			protected override void AddToCollection(WhsVASOrderLine vasOrderLine)
			{
				var row = GetColumnIndexerFromRow(vasOrderLine);
				Reader.SetValue(row, WhsVASOrderLineSchema.WVL_WVO_VASOrder, VASOrder.GetValue(WhsVASOrderSchema.PK));
			}

			protected override void RemoveFromCollection(WhsVASOrderLine vasOrderLine)
			{
				var vasOrderLineRow = GetColumnIndexerFromRow(vasOrderLine);
				Reader.DeleteRowAndSetHasChanges<WhsVASOrderLine>(vasOrderLineRow, WhsVASOrderLineSchema.PK);
			}

			protected override WhsVASOrderLine[] BusinessObjects
			{
				get { return Reader.factory.Load<WhsVASOrderLine>(new ZQuery(WhsVASOrderLineSchema.WVL_WVO_VASOrder, VASOrder.GetValue(WhsVASOrderSchema.PK))); }
			}

			protected override WhsVASOrderLine ReadIntoBusinessObject(OrderLine orderLineDataObject, WhsVASOrderLine vasOrderLine)
			{
				return new WhsVASOrderLineDataObjectReader(orderLineDataObject, Client, Reader.logger, Reader.factory).ReadIntoBusinessObject();
			}

			protected override WhsVASOrderLine FindMatchingBusinessObject(OrderLine orderLineDataObject)
			{
				return null;
			}
		}

		#endregion

		#region GetCombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<WhsVASOrder> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override WhsVASOrder GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			WhsVASOrder result = null;

			var clientAddress = ClientAddress;
			if (clientAddress != null)
			{
				result = VASOrderMatchingHelper.GetMatchingVASOrder(factory, dataObject, clientAddress.GetValue(OrgAddressSchema.OA_OH));
			}

			return result;
		}

		#endregion
	}
}
