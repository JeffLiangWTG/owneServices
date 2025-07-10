using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Customs.Business.BusinessObjects.CusPermit
{
	public abstract class CusPermitCusDecProcessor<T> : ICusPermitCusDecProcessor<T>
		where T : BusinessObject, ISavingProvider<T>
	{
		protected CusPermitCusDecProcessor(IAllowPermitProcessing header)
		{
			this.header = header;
		}

		protected readonly IAllowPermitProcessing header;
		protected IList<PermitRecord> PermitRecords { get; private set; }

		public ZString PermitErrors(ZString messageText)
		{
			if (PermitRecords != null)
			{
				var permitMessages = new ZStringBuilder();
				foreach (var errorMessage in ErrorList)
				{
					permitMessages.Append(errorMessage);
				}
				messageText += permitMessages.ToStringWithNewLineBetweenAppends();
			}

			return messageText;
		}

		public IEnumerable<ZString> ErrorList => PermitRecords?.SelectMany(x => x.ErrorMessages);

		public void AddPermitTransactions(T parentBizO, Func<T, ZString> getPermitAppId, string status = PermitTransactionStatusList.Codes.Pending)
		{
			if (PermitRecords != null)
			{
				void Handler(T bizzy)
				{
					foreach (var permitRecord in PermitRecords)
					{
						if (permitRecord.ErrorMessages.Count == 0)
						{
							permitRecord.PermitHeader?.AddTransaction(PermitHelper.GetPermitReferenceForEntry(header), PermitHelper.GetPermitComment(header, permitRecord), getPermitAppId(parentBizO), permitRecord.Procedure, permitRecord.Value, permitRecord.Quantity, status, PermitHelper.GetPermitReferenceNumberLineForEntry(header));
						}
					}
					parentBizO.Saving -= Handler;
				}
				parentBizO.Saving += Handler;
			}
		}

		public void AddPermitRecordsAndLockMutexIfNeeded()
		{
			if (ShouldAddPermitRecordsAndLockMutexIfNeeded())
			{
				PermitRecords = GetPermitRecordsWithTransactionsApplicable();

				foreach (var permitRecord in PermitRecords)
				{
					var permitHeader = permitRecord.PermitHeader;
					if (permitHeader != null)
					{
						var permitNumber = permitHeader.CPH_Number;

						void GatherData()
						{
							var existingTransactionsForEntry = permitHeader.GetTransactions().Where(x => x.MatchEntry(header, permitRecord) && x.CPL_TransactionStatus != PermitTransactionStatusList.Codes.Deleted).ToArray();
							var existingValue = existingTransactionsForEntry.Sum(x => x.CPL_TranValue);
							var existingQuantity = existingTransactionsForEntry.Sum(x => x.CPL_TranQty);

							var value = permitRecord.Value + existingValue;
							var quantity = permitRecord.Quantity + existingQuantity;

							var valueBalance = permitHeader.ValueBalance;
							var quantityBalance = permitHeader.QuantityBalance;

							var lastSentPermitRecord = GetLastSentPermitRecord(header, permitNumber);
							if (lastSentPermitRecord != null)
							{
								valueBalance += lastSentPermitRecord.Value;
								quantityBalance += lastSentPermitRecord.Quantity;
							}

							if (permitHeader.IsVAL && (valueBalance - value < ZDecimal.Zero))
							{
								var decimalPlaces = header.PermitValueDecimalPlaceCount;
								permitRecord.ErrorMessages.Add(EntryGreaterThanPermit(Res.GetString("7423A61F-156C-45D1-990D-F7C2ECFAF3DC", "value"), permitRecord.Value.ToString(decimalPlaces), permitNumber, new ZDecimal(valueBalance + existingValue).ToString(decimalPlaces)));
							}
							if (permitHeader.IsQTY && (quantityBalance - quantity < ZDecimal.Zero))
							{
								var decimalPlaces = header.PermitValueDecimalPlaceCount;
								permitRecord.ErrorMessages.Add(EntryGreaterThanPermit(Res.GetString("7423A61F-156C-45D1-990D-F7C2ECAF3A2B", "quantity"), permitRecord.Quantity.ToString(decimalPlaces), permitNumber, new ZDecimal(quantityBalance + existingQuantity).ToString(decimalPlaces)));
							}

							permitRecord.Value = AllowNegativeAdjustments || value > ZDecimal.Zero ? new ZDecimal(ZDecimal.Zero - value) : ZDecimal.Zero;
							permitRecord.Quantity = AllowNegativeAdjustments || quantity > ZDecimal.Zero ? new ZDecimal(ZDecimal.Zero - quantity) : ZDecimal.Zero;
						}

						void AddMutexText(string mutexText) => permitRecord.ErrorMessages.Add(mutexText);
						string GetMutexText() => Res.GetString("7423A61F-156C-45D1-990D-F7C2ECFCE2CE", "Cannot reserve the requested value/quantity for Permit {0}; {1} is already in the process of reserving data.", permitNumber, permitHeader.GetMutexLockInfo());

						permitHeader.DoActionWithMutexLock(GatherData, AddMutexText, GetMutexText);
					}
				}
			}
		}

		protected virtual bool ShouldAddPermitRecordsAndLockMutexIfNeeded() => true;

		public void UnlockPermitMutexes()
		{
			if (PermitRecords != null)
			{
				foreach (var permitRecord in PermitRecords)
				{
					var permitHeader = permitRecord.PermitHeader;
					if (permitHeader != null)
					{
						var mutex = permitHeader.Mutex;
						if (mutex.HasLock)
						{
							mutex.Unlock();
						}
					}
				}
			}
		}

		string EntryGreaterThanPermit(string type, string entryValue, string permitNumber, string permitBalance)
		{
			return Res.GetString("EAA8529C-9EC2-4283-BEF9-DC974340A135", "The declared customs {3}, {0}, for permit {1} is greater than the permit {3} balance of {2}.", entryValue, permitNumber, permitBalance, type);
		}

		protected virtual PermitRecord GetLastSentPermitRecord(IAllowPermitProcessing header, ZString permitNumber)
		{
			return null;
		}

		protected IList<PermitRecord> GetPermitRecordsWithTransactionsApplicable() => GetPermitRecords().Where(x => x.PermitHeader?.IsTransactionsApplicable() ?? false).ToList();
		protected abstract IList<PermitRecord> GetPermitRecords();

		protected virtual bool AllowNegativeAdjustments => false;
	}
}
