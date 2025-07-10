using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GLPeriodValidationProvider : PeriodValidationProvider
	{
		public GLPeriodValidationProvider(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void LedgerPeriodValidation(ZPropertyInfo dateTimeInfo)
		{
			ZDateTime dateTime = (ZDateTime)dateTimeInfo.Value;
			if (PeriodCalculator.IsPostDateValid(dateTime) == PostDateValidationResult.GLPeriodClosed)
			{
				dateTimeInfo.AddError(GeneralLedgerPeriodClosedError);
			}
		}
	}
}
