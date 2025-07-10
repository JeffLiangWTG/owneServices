using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Packing.DataTransfer;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using WTG.RTUS.Interface;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public sealed class WhsCarrierLabelsBatchPrintingSubscriber : IWhsCarrierLabelsBatchPrintingSubscriber
	{
		WhsCarrierLabelsBatchPrintingSubscriber()
		{
		}

		readonly List<ITopLevelDataObject> BatchedRequests = new List<ITopLevelDataObject>();

		public static IWhsCarrierLabelsBatchPrintingSubscriber GetSubscriber()
		{
			return new WhsCarrierLabelsBatchPrintingSubscriber();
		}

		public IReadOnlyCollection<ITopLevelDataObject> GetCarrierLabelsBatchRequests => BatchedRequests;

		public ReturnResult SubscribePackages(WhsPick pick, IEnumerable<WhsOrderToPackageItemNumbers> packageItemNumbers, IReadOnlyCollection<int> labelSeparatorSegmentNumbers)
		{
			var errorMessage = string.Empty;
			try
			{
				var request = new WhsPickUniversalShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, pick)), packageItemNumbers).GetDataObject(pick);
				if (request != null)
				{
					BatchedRequests.AddRange(RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.Instance, request, labelSeparatorSegmentNumbers, ObjectFactory.Get<IRTUSProcessor>()));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errorMessage = ex.Message;
			}

			return new ReturnResult { Success = errorMessage.IsNullOrEmpty(), Message = errorMessage };
		}
	}
}
