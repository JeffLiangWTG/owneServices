using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDebtorGroupBankCurrentOverride : AutoOrgDebtorGroupBankCurrentOverride
	{
		public OrgDebtorGroupBankCurrentOverride(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[CargoWise.ComponentModel.MaxLength(70)]
		public ZString CurrencyDescription
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, PB_RX_NKCurrency);
				if (currency != null)
				{
					fCurrencyDescription = currency.RX_DescMultilingual;
				}

				return fCurrencyDescription;
			}
			set
			{
				fCurrencyDescription = value;
				CurrencyDescriptionInfo.RefreshBinding();
			}
		}
		ZString fCurrencyDescription;

		public ZPropertyInfo CurrencyDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyDescription)); }
		}
	}
}
