using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefCurrencyValidation : AutoRefCurrencyValidation
	{
		public RefCurrencyValidation(AutoRefCurrency parent) : base(parent)
		{
			this.Currency = (RefCurrency)parent;
		}

		#region RX_Code

		protected override void CheckRX_Code()
		{
			base.CheckRX_Code();
			MandatoryValidation.CheckEntered(Currency.RX_CodeInfo);

			if (!Currency.RX_Code.IsEmpty && Currency.RX_Code.Length != 3)
			{
				Currency.RX_CodeInfo.AddError(Res.GetString("d07c5d25-b461-46a5-98a7-e935d61ebdf7", "Code must be 3 characters long."));
			}
		}

		#endregion

		#region RX_Symbol

		protected override void CheckRX_Symbol()
		{
			base.CheckRX_Symbol();
			MandatoryValidation.CheckEntered(Currency.RX_SymbolInfo);
		}

		#endregion

		#region RX_UnitName

		protected override void CheckRX_UnitName()
		{
			base.CheckRX_UnitName();
			MandatoryValidation.CheckEntered(Currency.RX_UnitNameInfo);
			TranslatableDataFieldAttribute.Validate(Currency.RX_UnitNameInfo);
		}

		#endregion

		#region RX_SubUnitName

		protected override void CheckRX_SubUnitName()
		{
			base.CheckRX_SubUnitName();
			TranslatableDataFieldAttribute.Validate(Currency.RX_SubUnitNameInfo);
		}

		#endregion

		#region RX_SubUnitRatio

		protected override void CheckRX_SubUnitRatio()
		{
			base.CheckRX_SubUnitRatio();

			List<int> validValues = new List<int>(new int[] { 1, 10, 100, 1000 });
			if (!validValues.Contains(Currency.RX_SubUnitRatio))
			{
				Currency.RX_SubUnitRatioInfo.AddError(InvalidRatioErrorMessage);
			}
		}

		#endregion

		#region RX_SubUnitRatio

		protected override void CheckRX_ISOSubUnitRatio()
		{
			base.CheckRX_ISOSubUnitRatio();

			List<int> validValues = new List<int>(new int[] { 1, 10, 100, 1000 });
			if (!validValues.Contains(Currency.RX_ISOSubUnitRatio))
			{
				Currency.RX_ISOSubUnitRatioInfo.AddError(InvalidRatioErrorMessage);
			}
		}

		#endregion

		#region RX_Desc

		protected override void CheckRX_Desc()
		{
			base.CheckRX_Desc();
			MandatoryValidation.CheckEntered(Currency.RX_DescInfo);
			TranslatableDataFieldAttribute.Validate(Currency.RX_DescInfo);
		}

		#endregion

		string InvalidRatioErrorMessage => Res.GetString("6ecc6acb-4c4a-44c4-b09a-0b932c711601", "Ratio must be 1, 10, 100 or 1000.");

		readonly RefCurrency Currency;
	}
}
