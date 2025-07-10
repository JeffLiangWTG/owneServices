using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCTariffDutyRateCollection : DependentBusinessObjectCollection<USCTariffDutyRate, USCTariff>
	{
		public USCTariffDutyRateCollection(USCTariff tariff, BusinessObjectFactory factory)
			: base(tariff, factory)
		{
		}

		public bool RequiresFirstQuantity
		{
			get
			{
				foreach (USCTariffDutyRate dutyRate in this)
				{
					if (dutyRate.RequiresFirstQuantity)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool RequiresSecondQuantity
		{
			get
			{
				foreach (USCTariffDutyRate dutyRate in this)
				{
					if (dutyRate.RequiresSecondQuantity)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool RequiresThirdQuantity
		{
			get
			{
				foreach (USCTariffDutyRate dutyRate in this)
				{
					if (dutyRate.RequiresThirdQuantity)
					{
						return true;
					}
				}
				return false;
			}
		}

		public USCTariffDutyRate GetRateForTaxFeeClassCode(ZString taxFeeClassCode)
		{
			foreach (USCTariffDutyRate rate in this)
			{
				if (rate.UD_TaxFeeClassCode == taxFeeClassCode)
				{
					return rate;
				}
			}
			return null;
		}

		public USCTariffDutyRate GetRateForElement(ZString elementCode)
		{
			USCTariffDutyRate result = null;
			foreach (USCTariffDutyRate dutyRate in this)
			{
				if (dutyRate.UD_DutyElement == elementCode)
				{
					result = dutyRate;
					break;
				}
			}
			return result;
		}

		public USCTariffDutyRate GetRateForSPI(ZString spiIndicator)
		{
			USCTariffDutyRate result = null;
			foreach (USCTariffDutyRate dutyRate in this)
			{
				if (dutyRate.UD_ISOCountryCode == spiIndicator)
				{
					result = dutyRate;
					break;
				}
			}
			return result;
		}

		internal void MarkForDelete()
		{
			foreach (USCTariffDutyRate dutyRate in this)
			{
				dutyRate.RemoveOnFactorySaving = true;
			}
		}
	}
}
