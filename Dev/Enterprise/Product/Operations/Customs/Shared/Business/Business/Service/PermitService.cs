using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.Business.Service
{
	class PermitService : IPermitService
	{
		bool IPermitService.ConfirmPermitTransactions(IEnumerable<IPermitWithdrawalRequestDetail> permitTransactions)
		{
			var factory = new BusinessObjectFactory();
			var success = true;
			var mutexList = new List<ZGlobalMutex>();
			foreach (var permitTransaction in permitTransactions)
			{
				var provider = GetProvider(factory, permitTransaction);
				if (provider == null || (!provider.IsExemptForMatchingPermit(permitTransaction) && !ConfirmPermitTransaction(permitTransaction, factory, mutexList)))
				{
					success = false;
					break;
				}
			}
			if (success)
			{
				ZString savingMessage;
				success = Save(factory, out savingMessage);
			}
			UnlockMutex(mutexList);
			return success;
		}

		void UnlockMutex(List<ZGlobalMutex> mutexList)
		{
			foreach (var mutex in mutexList)
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		bool Save(BusinessObjectFactory factory, out ZString message)
		{
			var result = false;
			try
			{
				factory.Save();
				result = true;
				message = ZString.Empty;
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				message = ex.Message;
			}
			return result;
		}

		bool ConfirmPermitTransaction(IPermitWithdrawalRequestDetail permitTransaction, BusinessObjectFactory factory, List<ZGlobalMutex> mutexList)
		{
			var result = false;
			if (IsValidDetails(permitTransaction))
			{
				var permitTransactionRefNumber = permitTransaction.PermitTransactionRefNumber;
				var lines = new BaseCusPermitLineTransaction.Loader(factory).FindPendingLines(PermitTransactionAppIdList.Codes.WarehouseOrder, permitTransactionRefNumber); // TODO: need application to be passed in
				if (lines.Length > 0)
				{
					result = true;
					foreach (var line in lines)
					{
						if (!ConfirmPermitTransaction(permitTransactionRefNumber, permitTransaction.Qty, GetCustomsValue(permitTransaction), line, mutexList))
						{
							result = false;
							break;
						}
					}
				}
			}
			return result;
		}

		bool ConfirmPermitTransaction(ZString permitTransactionRefNumber, ZDecimal qty, ZDecimal customsValue, BaseCusPermitLineTransaction line, List<ZGlobalMutex> mutexList)
		{
			var result = false;
			var lineQty = line.CPL_TranQty;
			lineQty = lineQty.Round(lineQty.DecimalPlaces);
			var checkLineQty = lineQty * -1;
			var lineCustomsValue = line.CPL_TranValue;
			lineCustomsValue = lineCustomsValue.Round(lineCustomsValue.DecimalPlaces);
			var checkLineCustomsValue = lineCustomsValue * -1;
			if (checkLineQty >= qty && checkLineCustomsValue >= customsValue)
			{
				var header = line.PermitHeader;
				if (header != null && header.LockMutex)
				{
					if (!mutexList.Contains(header.Mutex))
					{
						mutexList.Add(header.Mutex);
					}

					if (header.CPH_IsClosed)
					{
						return false;
					}

					result = true;
					line.CPL_TransactionStatus = ZString.Empty;
					var confirmLine = AddNewPermitLineTransaction(permitTransactionRefNumber, header);
					confirmLine.CPL_TranQty = checkLineQty - qty;
					confirmLine.CPL_TranValue = checkLineCustomsValue - customsValue;
					confirmLine.CPL_Comment = ResString.GetMultilingualString("{A7F6D3D6-9805-4565-B85F-DA8EB9A1486E}", "Requested (Qty: {0}, Value: {1}) - Confirmed (Qty: {2}, Value: {3})", checkLineQty, checkLineCustomsValue, qty.Round(qty.DecimalPlaces), customsValue.Round(customsValue.DecimalPlaces));
				}
			}
			return result;
		}

		AutoCusPermitLineTransaction AddNewPermitLineTransaction(ZString permitTransactionRefNumber, BaseCusPermitHeader header)
		{
			var permitLineTransaction = (AutoCusPermitLineTransaction)header.CusPermitLineTransactions.AddNew();
			permitLineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			permitLineTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			permitLineTransaction.CPL_AppId = PermitTransactionAppIdList.Codes.WarehouseOrder;
			permitLineTransaction.CPL_Reference = permitTransactionRefNumber;
			return permitLineTransaction;
		}

		SuccessOrFailure IPermitService.RelinquishPermitTransactions(IEnumerable<IPermitTransactionDetail> permitTransactionRefNumbers)
		{
			var factory = new BusinessObjectFactory();
			var success = true;
			var mutexList = new List<ZGlobalMutex>();
			foreach (var permitTransaction in permitTransactionRefNumbers)
			{
				var provider = GetProvider(factory, permitTransaction);
				if (provider == null || (!provider.IsExemptForMatchingPermit(permitTransaction) && !RelinquishPermitTransactions(permitTransaction.PermitTransactionRefNumber, factory, mutexList)))
				{
					success = false;
					break;
				}
			}
			if (success)
			{
				ZString savingMessage;
				success = Save(factory, out savingMessage);
			}
			UnlockMutex(mutexList);
			return success ? SuccessOrFailure.Success : SuccessOrFailure.Failure;
		}

		bool TryMatchOrganisation(BusinessObjectFactory factory, IOrgAddress warehouse, out OrgAddress warehouseAddress)
		{
			warehouseAddress = null;
			if (warehouse != null)
			{
				warehouseAddress = factory.Load<OrgAddress>(warehouse.PK);
			}
			return warehouseAddress != null;
		}

		bool RelinquishPermitTransactions(ZString permitTransactionRefNumber, BusinessObjectFactory factory, List<ZGlobalMutex> mutexList)
		{
			var result = false;
			if (!permitTransactionRefNumber.IsEmpty)
			{
				var lines = new BaseCusPermitLineTransaction.Loader(factory).FindPendingLines(PermitTransactionAppIdList.Codes.WarehouseOrder, permitTransactionRefNumber);
				if (lines.Length > 0)
				{
					result = true;
					foreach (var line in lines)
					{
						if (!RelinquishPermitTransactions(line, mutexList))
						{
							result = false;
							break;
						}
					}
				}
			}
			return result;
		}

		bool RelinquishPermitTransactions(BaseCusPermitLineTransaction line, List<ZGlobalMutex> mutexList)
		{
			var result = false;
			var header = line.PermitHeader;
			if (header != null && header.LockMutex)
			{
				if (!mutexList.Contains(header.Mutex))
				{
					mutexList.Add(header.Mutex);
				}

				if (header.CPH_IsClosed)
				{
					return false;
				}

				line.CPL_TransactionStatus = ZString.Empty;
				var relinquishLine = AddNewPermitLineTransaction(line.CPL_Reference, header);
				relinquishLine.CPL_TranQty = line.CPL_TranQty * -1;
				relinquishLine.CPL_TranValue = line.CPL_TranValue * -1;
				relinquishLine.CPL_Comment = ResString.GetMultilingualString("{AF0FF854-41E5-43FA-B815-206342CFF914}", "Relinquished");
				result = true;
			}
			return result;
		}

		IPermitWithdrawRequestResponseResult IPermitService.IsPermitAvailable(IEnumerable<IPermitWithdrawRequest> requests)
		{
			return TryGetPermits(requests, false);
		}

		IPermitWithdrawRequestResponseResult IPermitService.TryGetPermits(IEnumerable<IPermitWithdrawRequest> requests)
		{
			return TryGetPermits(requests, true);
		}

		IPermitWithdrawRequestResponseResult TryGetPermits(IEnumerable<IPermitWithdrawRequest> requests, bool allowSaving)
		{
			var responseResults = new Dictionary<PermitWithdrawRequestResponse, BaseCusPermitHeader[]>();
			var factory = new BusinessObjectFactory();
			var mutexList = new List<ZGlobalMutex>();
			foreach (var request in requests)
			{
				var response = new PermitWithdrawRequestResponse(request);
				var permits = FindMatchingPermit(response, factory, mutexList, allowSaving);
				responseResults.Add(response, permits);
			}
			var shouldSave = responseResults.Count > 0;
			foreach (var failedResponse in responseResults.Where(x => x.Key.SuccessOrFailure != SuccessOrFailure.Success))
			{
				shouldSave = false;
				ProcessAvailableData(failedResponse.Key, failedResponse.Value);
			}
			if (allowSaving && shouldSave)
			{
				ZString savingMessage;
				if (!Save(factory, out savingMessage))
				{
					foreach (var response in responseResults.Keys)
					{
						response.SuccessOrFailure = SuccessOrFailure.Failure;
						response.AddFailureReason(savingMessage);
					}
				}
			}

			UnlockMutex(mutexList);
			var result = new PermitWithdrawRequestResponseResult() { Responses = responseResults.Keys.ToArray() };
			result.Success = !result.Responses.Any(x => x.SuccessOrFailure != SuccessOrFailure.Success);
			return result;
		}

		bool IsValidDetails(IPermitWithdrawalRequestDetail details)
		{
			return details.Qty > ZDecimal.Zero && details.ReceiveTotalQty > ZDecimal.Zero && details.ReceiveTotalCustomsValue >= ZDecimal.Zero;
		}

		ZDecimal GetCustomsValue(IPermitWithdrawalRequestDetail details)
		{
			return new ZDecimal((details.Qty / details.ReceiveTotalQty) * details.ReceiveTotalCustomsValue).Round(2);
		}

		BaseCusPermitHeader[] FindMatchingPermit(PermitWithdrawRequestResponse response, BusinessObjectFactory factory, List<ZGlobalMutex> mutexList, bool useMutex)
		{
			BaseCusPermitHeader[] permits = null;
			var details = response.Request;
			if (IsValidDetails(details))
			{
				OrgAddress ownerAddress;
				OrgAddress warehouseAddress;
				OrgAddress manufacturerAddress;
				var criteria = response.Request;
				if (TryMatchOrganisation(criteria, factory, out ownerAddress, out warehouseAddress, out manufacturerAddress, response.AddFailureReason))
				{
					var permitType = criteria.PermitType;
					string permitCountry = GetPermitCountry(warehouseAddress);
					var provider = GetProvider(permitCountry, permitType);
					if (provider == null)
					{
						response.AddFailureReason(ResString.GetMultilingualString("{F77E2628-5E71-4AFB-833E-11BF573E5DE1}", "No Provider found for Country '{0}' and Type '{1}'.", permitCountry, permitType.ToString()));
					}
					else
					{
						if (provider.IsExemptForMatchingPermit(response.Request))
						{
							response.SuccessOrFailure = SuccessOrFailure.Success;
						}
						else
						{
							var permitCriteria = new PermitCriteria(criteria.DetailedTrackingEnabled, ownerAddress, warehouseAddress, manufacturerAddress, criteria.UQ, criteria.CountryOfOrigin, criteria.ProductCode, criteria.Tariff, criteria.ZoneStatus);
							var matchingCriteria = provider.GetMatchingCriteria(permitCriteria);
							permits = provider.FindMatchingPermit(permitCriteria);
							if (permits != null)
							{
								permits = permits.OrderBy(x => x.CPH_StartDate).ThenBy(x => x.CPH_SystemCreateTimeUtc).ToArray();
							}
							ProcessMatchingPermit(response, provider, permits, mutexList, matchingCriteria, useMutex);
						}
					}
				}
			}
			else
			{
				var qty = details.Qty;
				if (qty <= ZDecimal.Zero)
				{
					response.AddFailureReason(ResString.GetMultilingualString("{52421696-2D51-4F4C-B8D4-96C6E8272A03}", "Qty ({0}) should be greater than zero.", qty));
				}
				var receiveTotalQty = details.ReceiveTotalQty;
				if (receiveTotalQty <= ZDecimal.Zero)
				{
					response.AddFailureReason(ResString.GetMultilingualString("{A7633C27-879A-4F3A-9DA8-0A684590841D}", "Receive Total Qty ({0}) should be greater than zero.", receiveTotalQty));
				}
				var receiveTotalCustomsValue = details.ReceiveTotalCustomsValue;
				if (receiveTotalCustomsValue < ZDecimal.Zero)
				{
					response.AddFailureReason(ResString.GetMultilingualString("{849D2697-17C6-49A4-9F5F-8E35705537F0}", "Receive Total Customs Value ({0}) should not be negative.", receiveTotalCustomsValue));
				}
			}

			return permits;
		}

		IPermitWithdrawRequestProvider GetProvider(BusinessObjectFactory factory, IPermitTransactionDetail detail)
		{
			IPermitWithdrawRequestProvider provider = null;
			OrgAddress warehouseAddress;
			if (TryMatchOrganisation(factory, detail.Warehouse, out warehouseAddress))
			{
				provider = GetProvider(GetPermitCountry(warehouseAddress), detail.PermitType);
			}
			return provider;
		}

		IPermitWithdrawRequestProvider GetProvider(string permitCountry, PermitType permitType)
		{
			var types = ObjectFactory.Get<Hashtable>("PermitWithdrawRequestCountryProviders");
			var objectHandle = (ObjectHandle)types[permitCountry];
			var countryProvider = objectHandle == null ? null : (IPermitWithdrawRequestCountryProvider)objectHandle.GetObject();
			return countryProvider == null ? null : countryProvider.FindProviderFor(permitType);
		}

		ZString GetPermitCountry(OrgAddress warehouseAddress)
		{
			return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(warehouseAddress?.RelatedCountry?.RN_Code ?? ZString.Empty);
		}

		void ProcessMatchingPermit(PermitWithdrawRequestResponse response, IPermitWithdrawRequestProvider provider, BaseCusPermitHeader[] permits, List<ZGlobalMutex> mutexList, ZString matchingCriteria, bool useMutex)
		{
			if (permits != null && permits.Length > 0)
			{
				var details = response.Request;
				var requestQty = details.Qty;
				var requestCustomsValue = GetCustomsValue(details);
				var permitTransactionRefNumber = details.PermitTransactionRefNumber;
				var permit = permits.FirstOrDefault(x => x.QuantityBalance >= requestQty && x.ValueBalance >= requestCustomsValue);
				if (permit != null)
				{
					if (!useMutex || permit.LockMutex)
					{
						if (!permit.CusPermitLineTransactions.Any(x => x.IsPending && x.CPL_Reference == permitTransactionRefNumber))
						{
							var permitLine = AddNewPermitLineTransaction(details.PermitTransactionRefNumber, permit);
							permitLine.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
							permitLine.CPL_TranQty = requestQty * -1;
							permitLine.CPL_TranValue = requestCustomsValue * -1;
							permitLine.CPL_Comment = ResString.GetMultilingualString("{27C1CB1E-1905-416F-B132-2F0AE50F93A3}", "Requested");
							response.OutwardEntryNumber = provider.GetOutwardEntryNumber(permit);
							response.SuccessOrFailure = SuccessOrFailure.Success;
						}
						else
						{
							response.AddFailureReason(ResString.GetMultilingualString("{2CBC9689-B87A-4506-AB66-DA3BCC8B224F}", "There is already a pending request for '{0}'.", permitTransactionRefNumber));
						}
						if (useMutex && !mutexList.Contains(permit.Mutex))
						{
							mutexList.Add(permit.Mutex);
						}
					}
					else
					{
						response.AddFailureReason(ResString.GetMultilingualString("{C9DD6F8A-FDBC-4174-8643-8301233831EF}", "{0} is in the middle of modifying this permit '{1}'.", permit.Mutex.GetMutexLockByInfo(), permit.CPH_Number));
					}
				}
				else
				{
					var failure = new ZStringBuilder();
					if (permits.All(p => p.QuantityBalance < requestQty))
					{
						failure.Append(ResString.GetMultilingualString("{43C792DA-5816-45E1-8585-A06EC14D176C}", "Weekly Estimate match found but remaining quantity is insufficient."));
					}
					if (permits.All(p => p.ValueBalance < requestCustomsValue))
					{
						failure.Append(ResString.GetMultilingualString("{E64B976C-43EF-49F1-A096-7286EA9A6DAD}", "Weekly Estimate match found but remaining value is insufficient."));
					}

					if (!failure.IsEmpty)
					{
						failure.Append(ResString.GetMultilingualString("{9531BBE5-9FFE-45D8-A53F-14204EE02D44}", "Matches found:"));

						foreach (var permitHeader in permits)
						{
							failure.Append(ResString.GetMultilingualString("{0F3CE551-8B8F-4757-8151-699BE716363C}", "Permit number: {0}, Quantity left: {1} {2}, Value left: {3} {4}", permitHeader.CPH_Number, permitHeader.QuantityBalance.Round(2).ToString(), details.UQ, permitHeader.ValueBalance.Round(2).ToString(), Core.Constants.CurrencyCodes.UnitedStates));
						}

						response.AddFailureReason(System.Environment.NewLine + failure.ToStringWithNewLineBetweenAppends());
					}
				}
			}
			else
			{
				response.AddFailureReason(System.Environment.NewLine + matchingCriteria + ".");
			}
		}

		void ProcessAvailableData(PermitWithdrawRequestResponse response, BaseCusPermitHeader[] permits)
		{
			var details = response.Request;
			if (IsValidDetails(details) && permits != null && permits.Length > 0)
			{
				var totalQty = 0m;
				var totalCustomsValue = 0m;
				foreach (var permit in permits)
				{
					totalQty += permit.QuantityBalance;
					totalCustomsValue += permit.ValueBalance;
				}
				response.AvailableQty = totalQty;
				if (totalCustomsValue >= GetCustomsValue(details) && totalQty >= details.Qty)
				{
					response.SuccessOrFailure = SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits;
				}
			}
		}

		bool TryMatchOrganisation(IPermitMatchingCriteria criteria, BusinessObjectFactory factory, out OrgAddress ownerAddress, out OrgAddress warehouseAddress, out OrgAddress manufacturerAddress, Action<ZString> addFailureReason)
		{
			var result = false;
			ownerAddress = null;
			warehouseAddress = null;
			manufacturerAddress = null;
			if (criteria != null)
			{
				result = true;
				var owner = criteria.Owner;
				if (owner != null)
				{
					ownerAddress = factory.Load<OrgAddress>(owner.PK);
				}
				if (ownerAddress == null)
				{
					addFailureReason(ResString.GetMultilingualString("{B16658E8-2762-4F6F-95F7-2EC567665B2C}", "Unable to match Owner."));
					result = false;
				}
				var warehouse = criteria.Warehouse;
				if (warehouse != null)
				{
					warehouseAddress = factory.Load<OrgAddress>(warehouse.PK);
				}
				if (warehouseAddress == null)
				{
					addFailureReason(ResString.GetMultilingualString("{7C327052-38CD-43B0-A5EC-1560774626AE}", "Unable to match Warehouse."));
					result = false;
				}

				if (result && criteria.DetailedTrackingEnabled)
				{
					var manufacturer = criteria.Manufacturer;
					if (manufacturer != null)
					{
						manufacturerAddress = factory.Load<OrgAddress>(manufacturer.PK);
						if (manufacturerAddress == null)
						{
							result = false;
							addFailureReason(ResString.GetMultilingualString("{AB7AD065-268E-4DB2-909C-4716A1AA74C8}", "Unable to match Manufacturer."));
						}
					}
				}
			}
			return result;
		}

		public ISimpleLogger Logger => logger ?? (logger = new SimpleLogger());
		SimpleLogger logger;

		class SimpleLogger : ISimpleLogger
		{
			public void Log(LogType type, string message)
			{
				Logs.Add(new SimpleLog(type, message));
			}
			List<SimpleLog> Logs => logs ?? (logs = new List<SimpleLog>());
			List<SimpleLog> logs;

			#region ISimpleLogResult Members

			IEnumerable<ISimpleLog> ISimpleLogResult.Logs => Logs;

			#endregion
		}
	}
}
