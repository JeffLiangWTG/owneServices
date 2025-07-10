using System;
using System.Collections.Generic;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NZ.Business
{
	public sealed class RateCalcData : IUniversalRateCalcData
	{
		public RateCalcData(IHaveTheParametersRequiredToCalculateDuty dutyParameters)
		{
			this.dutyParameters = dutyParameters;
		}
		readonly IHaveTheParametersRequiredToCalculateDuty dutyParameters;

		public DateTime DateOfValuation => dutyParameters.DateForDutyRate.ToDateTime();

		public decimal ValueForDuty => dutyParameters.ValueForDuty;

		public decimal CustomsValue => 0M;

		public IDictionary<string, decimal> UnitOfMeasureValueList
		{
			get
			{
				if (unitOfMeasureValueList == null)
				{
					unitOfMeasureValueList = new Dictionary<string, decimal>();
					var uq = dutyParameters.StatUQ;
					if (!uq.IsEmpty)
					{
						unitOfMeasureValueList.AddNewKeyOrAccumulateValue(uq, dutyParameters.StatQty);
					}
					uq = dutyParameters.SuppUQ;
					if (!uq.IsEmpty)
					{
						unitOfMeasureValueList.AddNewKeyOrAccumulateValue(uq, dutyParameters.SuppQty);
					}
				}
				return unitOfMeasureValueList;
			}
		}
		IDictionary<string, decimal> unitOfMeasureValueList;

		public IDictionary<string, decimal> CountrySpecificValueList
		{
			get
			{
				if (countrySpecificValueList == null)
				{
					countrySpecificValueList = new Dictionary<string, decimal>();
					countrySpecificValueList.Add(UOMTypeList.Codes.CU1, dutyParameters.StatQty);
				}

				return countrySpecificValueList;
			}
		}
		IDictionary<string, decimal> countrySpecificValueList;

		public IDictionary<string, string> MeursingExpressionList => new Dictionary<string, string>();

		public IList<Tuple<string, string>> AdditionalInformationList => new List<Tuple<string, string>>();
	}
}
