using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business
{
	public class ProvisionalPaymentAmountCodeDataCollection : CusCodeDataCollection<ProvisionalPaymentAmountCodeData>
	{
		public ProvisionalPaymentAmountCodeDataCollection(CusEntryLine master)
			: base(master, CusCodeDataTypeList.Codes.PPAmount)
		{
		}

		public CusEntryLine EntryLine => (CusEntryLine)Master;

		internal ZDecimal GetAmount(ZString feeType)
		{
			var result = ZDecimal.Zero;
			foreach (ProvisionalPaymentAmountCodeData item in this)
			{
				if (item.CY_Code == feeType)
				{
					result += item.CY_Value;
				}
			}
			return result;
		}

		internal (ZDecimal Amount, IEnumerable<IDutyFeeInformation> Details) GetAmountOfRateType(ZString rateType)
		{
			var feeTypes = ProvisionalPaymentTypesHelper.GetTypesForRateType(rateType);
			var details = GetDutyFeeInformations(feeTypes).ToArray();
			var amount = details.Sum(x => x.Value);
			return (amount, details);
		}

		IEnumerable<IDutyFeeInformation> GetDutyFeeInformations(IEnumerable<ZString> provisionalPaymentTypes)
		{
			var lineNumber = EntryLine.CL_LineNumber;
			var headerPPCollection = EntryLine?.Header?.ProvisionalPaymentPayInfos;
			var feeTypes = provisionalPaymentTypes.ToArray();
			foreach (ProvisionalPaymentAmountCodeData item in this)
			{
				var ppType = item.CY_Code;
				if (feeTypes.Contains(ppType) && (!headerPPCollection?.HasPPTypeBeenLiquidated(ppType, lineNumber) ?? true))
				{
					yield return new DutyFeeInformationDocWrapper(ppType, item.CY_Value);
				}
			}
		}

		internal ProvisionalPaymentAmountCodeData AddNew(ZString code, ZDecimal value)
		{
			return this.AddNew(code, value.ToString());
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			((CusEntryLineValidation)EntryLine?.Validation)?.ValidateDiamondLevyValueAndAmount();
		}
	}
}
