using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class CustomsEntryLineDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryLineDataObjectWriter
	{
		internal CustomsEntryLineDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override void PopulateCountrySpecificCusEntryLineChargeData(ECB.CusEntryLine baseEntryLineBO, EntryLine entryLineData)
		{
			base.PopulateCountrySpecificCusEntryLineChargeData(baseEntryLineBO, entryLineData);
			var entryLineBO = baseEntryLineBO as CusEntryLine;

			var provisionalPayments = entryLineBO?.ProvisionalPayments;
			if (provisionalPayments != null && provisionalPayments.Count > 0)
			{
				var entryLineChargeCollection = new List<EntryLineCharge>();
				var entryChargeTypeList = entryLineBO.Factory.GetCachedValue<LineLevelProvisionalPayments>();
				foreach (ProvisionalPaymentAmountCodeData ppBO in provisionalPayments)
				{
					entryLineChargeCollection.Add(new EntryLineCharge()
					{
						Amount = ppBO.CY_Value,
						Type = ListHelper.GetWithDescription<CodeDescriptionPair>(ppBO.CY_Code, entryChargeTypeList)
					});
				}
				if (entryLineData.EntryLineChargeCollection == null)
				{
					entryLineData.EntryLineChargeCollection = entryLineChargeCollection;
				}
				else
				{
					entryLineData.EntryLineChargeCollection.AddRange(entryLineChargeCollection);
				}
			}
		}
	}
}
