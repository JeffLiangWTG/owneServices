using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.DocumentWrappers;

namespace Enterprise.Customs.ZA.Business
{
	public class DA63AdditionalDutyCollection
		: CusCodeDataCollection<DA63AdditionalDuty>
	{
		public DA63AdditionalDutyCollection(JobComInvoiceLine parent)
			: base(parent, CusCodeDataTypeList.Codes.DA63AdditionalDuty)
		{
		}

		JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Master;

		public void AddOrUpdate(ZString code, ZDecimal value)
		{
			var item = GetFirstElementHaving(code)
				?? AddNew(code);

			item.CY_Value = value;
		}

		void AddNewIfNotExists(ZString code)
		{
			if (!ContainsCode(code))
			{
				AddNew(code);
			}
		}

		public void AutoPopulateIfNeeded()
		{
			if (Count == 0)
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine.IsDA63WithOriginalEntry)
				{
					var fees = invoiceLine.ImportBOEntryLine.GetCalcFeeValues();

					var s12B = fees.S1P2BDuty;
					if (!s12B.IsEmpty)
					{
						AddNewIfNotExists(DA63AdditionalDuty.S1P2BDuty);
					}

					var customsDutyTypesExcluding12B = CustomsDutyTypesExcluding12B.ToArray();
					var excluding12B = fees.CustomsDutiesExcluding12B
						.Where(x => customsDutyTypesExcluding12B.Contains(x.Code))
						.GroupBy(x => x.Code)
						.Select(x => new DutyFeeInformationDocWrapper(x.Key, x.Sum(y => y.Value)))
						.OrderBy(x => x.Code);

					foreach (var dutyFeeInformation in excluding12B)
					{
						var code = dutyFeeInformation.Code;
						if (!dutyFeeInformation.Value.IsEmpty)
						{
							AddNewIfNotExists(code);
						}
					}
				}
			}
		}

		public DA63AdditionalDuty S1P2BDuty => GetFirstElementHaving(DA63AdditionalDuty.S1P2BDuty);

		public IEnumerable<DA63AdditionalDuty> Penalties
		{
			get => GetPPs(Universal.Constants.RateTypes.Penalty);
		}

		public IEnumerable<DA63AdditionalDuty> ProvisionalPayments
		{
			get => GetPPs(Universal.Constants.RateTypes.ProvisionalPayment);
		}

		IEnumerable<ZString> CustomsDutyTypesExcluding12B
		{
			get
			{
				return Factory.GetCachedValue("E695B305-3BD1-4081-8281-C1F339CEE4C7|CustomsDutyTypesExcluding12B", () =>
				{
					var pps = Factory.GetAllProvisionalPaymentTypes();
					var types = Factory.GetDA63PartList().GetAllCodes()
						.Except(pps)
						.Except(new[] { DA63AdditionalDuty.S1P2BDuty });

					return types.Select(x => new ZString(x));
				});
			}
		}

		public IEnumerable<DA63AdditionalDuty> CustomsDutiesExcluding12B
		{
			get
			{
				var types = CustomsDutyTypesExcluding12B;
				return Find(x => types.Contains(x.CY_Code));
			}
		}

		IEnumerable<DA63AdditionalDuty> GetPPs(ZString rateType)
		{
			var types = ProvisionalPaymentTypesHelper.GetTypesForRateType(rateType);
			return Find(x => types.Contains(x.CY_Code));
		}
	}
}
