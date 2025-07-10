using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public class PeriodFilterValidation : ModuleTextFilterValidation
	{
		public PeriodFilterValidation(PeriodFilter parent)
			: base(parent) { }

		public void ValidatePeriodYear()
		{
			ValidateCalculatedProperty(Parent.PeriodYearInfo);
		}

		protected virtual void CheckPeriodYear()
		{
			if (Parent.PeriodYear != ZInt.Zero && (Parent.PeriodYear < ZDateTime.MinSmallDateTimeValue.Year || Parent.PeriodYear > ZDateTime.MaxSmallDateTimeValue.Year))
			{
				Parent.PeriodYearInfo.AddError(Res.GetString("4a4e1aaf-e4ce-436e-a6e7-ee0799cb2efc", "Year should bigger than {0} and smaller than {1}", ZDateTime.MinSmallDateTimeValue.Year, ZDateTime.MaxSmallDateTimeValue.Year));
			}
		}

		public void ValidatePeriodMonth()
		{
			ValidateCalculatedProperty(Parent.PeriodMonthInfo);
		}

		protected virtual void CheckPeriodMonth()
		{
			if (Parent.PeriodMonth != ZInt.Zero && (Parent.PeriodMonth < 1 || Parent.PeriodMonth > 12))
			{
				Parent.PeriodMonthInfo.AddError(Res.GetString("db63fd12-c544-4d24-bfb3-088e23fc0890", "Month should bigger than 1 and smaller than 12"));
			}
		}

		#region Implementation

		new PeriodFilter Parent
		{
			get { return (PeriodFilter)base.Parent; }
		}

		#endregion
	}
}
