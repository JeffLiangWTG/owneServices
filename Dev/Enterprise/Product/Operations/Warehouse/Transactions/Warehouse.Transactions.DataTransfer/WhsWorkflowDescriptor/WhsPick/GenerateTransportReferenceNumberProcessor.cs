using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class GenerateTransportReferenceNumberProcessor : IProcessor
	{
		public GenerateTransportReferenceNumberProcessor(WhsOrder order)
		{
			Order = Argument.NotNull(order, nameof(order));
		}

		readonly WhsOrder Order;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var transportReferenceGenerated = false;
			BusinessObjectFactory.SavingEventHandler savingEventHandler = (orderFactory) =>
			{
				if (string.IsNullOrEmpty(Order.WD_TransportReference))
				{
					var transportCo = Order.GetTransportCo();
					if (transportCo != null && Order.WD_WW_Whs.IsValid && Order.WD_OH_Client.IsValid)
					{
						var ranker = new List<ColumnValueRanker.ColumnValuesPair>();
						ranker.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_OH_Client, Order.WD_OH_Client));
						ranker.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_WW_Whs, Order.WD_WW_Whs));

						var linkedFountain = transportCo.GetMatchingNumberRange(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, ranker)?.LinkedFountain;
						if (linkedFountain != null)
						{
							Order.WD_TransportReference = linkedFountain.TryGetNumberFountain().GetNextFormatted(orderFactory);
							transportReferenceGenerated = !string.IsNullOrEmpty(Order.WD_TransportReference);
						}
					}
				}
			};

			var factory = Order.Factory;
			if (factory.IsInTransaction)
			{
				savingEventHandler(factory);
			}
			else
			{
				using (var manager = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
				{
					savingEventHandler(factory);
					if (transportReferenceGenerated)
					{
						ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
					}
					manager.CommitTransaction();
				}
			}
		}
	}
}
