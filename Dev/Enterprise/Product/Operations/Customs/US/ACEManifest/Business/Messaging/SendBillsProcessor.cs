using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ACEManifest.Business.Messaging
{
	internal class SendBillsProcessor : IProcessor
	{
		public SendBillsProcessor(AsycudaManifestHeader header)
		{
			this.header = header;
			var messageStatusProvider = new AIMMessageStatusProvider();
			originalMessageSubType = messageStatusProvider.GetMessageFunctionSubTypeForSend(header);
			amendmentMessageSubType = messageStatusProvider.GetMessageFunctionSubTypeForAmend(header);
			messagingHelper = new AIMMessagingHelper();
		}

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			const int BatchSize = 1000;
			using (DisposableEnvironment.ForBranch(header.AMA_GB.ToGuid()))
			{
				var countSuccess = 0;
				var countFailed = 0;
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, $"Starting sending bills for {header.HumanReadableName}");

				var pks = GetBillPKs();
				var nextBatchStartIndex = 0;
				while (pks.Length > nextBatchStartIndex)
				{
					var curBatchSize = Math.Min(BatchSize, pks.Length - nextBatchStartIndex);
					var curBatch = pks.Skip(nextBatchStartIndex).Take(curBatchSize);
					nextBatchStartIndex += curBatchSize;
					if (SendBills(curBatch, notifications))
					{
						countSuccess += curBatchSize;
					}
					else
					{
						// switch to 1-member batch to retry
						foreach (var pk in curBatch)
						{
							if (SendBills(new[] { pk }, notifications))
							{
								countSuccess++;
							}
							else
							{
								countFailed++;
							}
						}
					}
				}

				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, $"Sent {countSuccess} bills for {header.HumanReadableName}");
				if (countFailed > 0)
				{
					notifications.Add(CargoWise.ComponentModel.NotificationType.Information, $"Failing sending {countFailed} bills for {header.HumanReadableName}");
				}
			}
		}

		ZGuid[] GetBillPKs()
		{
			var factory = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "Asycuda Bills' PK Query Factory" // Factory Name For Debugging
			};
			var query = new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(header).CompleteFilter;
			var sqlText = System.FormattableString.Invariant($@"
SELECT {AsycudaBillSchema.Constants.PK}
FROM dbo.AsycudaBill
{query.GetAsWhereClause(false)}
OPTION (RECOMPILE)
");
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sqlText, new ZSqlParameterCollection(query.Params));
			return collection.Select(x => (ZGuid)x[AsycudaBillSchema.Constants.PK]).ToArray();
		}

		bool SendBills(IEnumerable<ZGuid> pks, INotifications notifications)
		{
			var factory = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "Asycuda Bills Send Factory" // Factory Name For Debugging
			};
			foreach (var pk in pks)
			{
				factory.AddFetchHint(AsycudaBillSchema.PK, pk);
			}
			var bills = new List<AsycudaBill>();
			try
			{
				foreach (var pk in pks)
				{
					var bill = factory.Load<AsycudaBill>(pk);
					string messageSubType;
					if (bill.ABL_MessageStatus == ZString.Empty)
					{
						messageSubType = originalMessageSubType;
					}
					else if (bill.ABL_MessageStatus == ASYCUDA.Business.MessageStatusCodeList.Codes.Sent)
					{
						messageSubType = amendmentMessageSubType;
					}
					else
					{
						continue;
					}
					var (success, notification, _) = messagingHelper.SendBillMessage(header, messageSubType, new[] { bill });
					if (success)
					{
						bills.Add(bill);
					}
					else
					{
						if (!pks.Skip(1).Any()) // equivalent to Count() == 1 but no iteration
						{
							// only add log when the current batch is one to avoid double entry
							notifications.AddError(notification);
						}
						return false;
					}
				}
				factory.Save();
				foreach (var bill in bills)
				{
					notifications.Add(CargoWise.ComponentModel.NotificationType.Information, $"Sent {bill.HumanReadableName}");
				}
				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!pks.Skip(1).Any()) // equivalent to Count() == 1 but no iteration
				{
					notifications.AddError($"Failed sending {bills.Single().HumanReadableName}. Reason: {ex.Message}");
				}
				else
				{
					notifications.AddError(ex.Message);
				}
				return false;
			}
		}

		readonly AsycudaManifestHeader header;
		readonly ZString originalMessageSubType;
		readonly ZString amendmentMessageSubType;
		readonly AIMMessagingHelper messagingHelper;
	}
}
