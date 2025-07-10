using System;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public class ConvertToLocalCurrencyControl : ZCalcFindBox, IDynamicToolTip
	{
		public delegate CurrencyConverter CurrencyConverterGetter();

		public event QueryToolTipEventHandler QueryToolTip;

		public void GetToolTip(ToolTipInfo info)
		{
			if (CurrencyConverterGetterMethod != null)
			{
				CurrencyConverter currencyConverter = CurrencyConverterGetterMethod();
				if (currencyConverter != null)
				{
					RefCurrency foreignCurrency = (RefCurrency)currencyConverter.Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, UnitFindBox.CurrentCode);
					Money foreignAmount = new Money((Decimal)AmountCalcEdit.CalcValue, foreignCurrency);

					Money localPrice = currencyConverter.ConvertRounded(foreignAmount, currencyConverter.LocalCurrency);

					info.ToolTipText += "\n" + localPrice.Currency.Code + " " + String.Format("{0,10:C}", localPrice.Amount);
					info.DefaultLocation = true;

					if (QueryToolTip != null)
					{
						QueryToolTip(this, info);
					}
				}
			}
		}

		public CurrencyConverterGetter CurrencyConverterGetterMethod;

		public void SetCurrencyCodeFindBoxReadOnly(bool value)
		{
			this.UnitFindBox.SetReadOnly(value);
		}
	}
}
