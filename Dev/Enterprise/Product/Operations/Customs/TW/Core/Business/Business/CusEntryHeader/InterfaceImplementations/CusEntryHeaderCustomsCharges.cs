using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;

namespace Enterprise.Customs.TW.Business.InterfaceImplementations
{
	class CusEntryHeaderCustomsCharges : Customs.Business.InterfaceImplementations.CusEntryHeaderCustomsCharges
	{
		public CusEntryHeaderCustomsCharges(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		CusEntryHeader EntryHeader => (CusEntryHeader)entryHeader;

		protected override CustomsCharge[] GetCustomsCharges(ILogger logger)
		{
			var customsCharges = new Dictionary<string, CustomsCharge>();
			var declaration = EntryHeader.Declaration;
			if (declaration != null)
			{
				var entryChargeTypeList = EntryHeader.EntryChargeTypeList;
				var isPaidByBroker = declaration.JE_PaidBy != Enterprise.MasterFiles.Business.Customs.PaidByCodeList.Codes.CLI;
				var effectiveEntryPayInfos = EntryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Where(x => x.C9_PaymentAmount > 0).ToList();
				foreach (var entryPayInfo in effectiveEntryPayInfos)
				{
					var stringKey = entryPayInfo.C9_TransactionType;
					var chargeValue = entryPayInfo.C9_PaymentAmount;
					if (!customsCharges.TryGetValue(stringKey, out var customsCharge))
					{
						var entryChargeType = entryChargeTypeList[stringKey];
						customsCharge = new CustomsCharge(GetAccChargeCodeFor(entryChargeType), entryPayInfo.TypeDescription, 0m, 0m
							, isPaidByBroker
							, entryHeader.CreditorPK
							, entryHeader.LocalCurrencyCode)
						{
							EntryReference = entryHeader.EntryReferenceInChargeDescSupported ? entryHeader.ReferenceNumber : ZString.Empty,
							DebtorPK = ZGuid.Empty
						};

						customsCharges.Add(stringKey, customsCharge);
					}
					customsCharge.AddAmount(false, chargeValue);
				}
			}

			return customsCharges.Values.ToArray();
		}
	}
}
