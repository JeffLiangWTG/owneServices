using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business
{
	class PrelimStatementPrintDateValidator
	{
		public void ValidatePreliminaryStatementPrintDate(ZPropertyInfo prelimStatementDateInfo, IPrelimStatementDetailsDefault statementDateDefaultee)
		{
			ZDateTime statementPrintDate = (ZDateTime)prelimStatementDateInfo.Value;

			if (statementDateDefaultee.IsManualPayment)
			{
				if (!statementPrintDate.IsEmpty)
				{
					prelimStatementDateInfo.AddMessageError(PreliminaryPrintNotRequired);
				}
			}
			else
			{
				if (statementPrintDate.IsValid)
				{
					if (!statementPrintDate.IsInTheFutureDatePartOnly && statementDateDefaultee.ShouldValidatePastDate)
					{
						prelimStatementDateInfo.AddMessageError(PreliminaryPrintDateShouldBeInTheFuture);
					}

					new WeekendsAndHolidaysValidator().CheckWeekendsAndHolidays(prelimStatementDateInfo);

					if (statementPrintDate.Date > ZDateTime.Today.AddDays(90).Date)
					{
						prelimStatementDateInfo.AddMessageError(PreliminaryPrintDateMoreThan90DaysInTheFuture);
					}

					ValidateWorkingDayWithDay11(prelimStatementDateInfo, statementDateDefaultee.US_PaymentType, statementDateDefaultee.US_PeriodicStatementMM);

					if (statementDateDefaultee.Branch != null && USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.GetFallBackValueAtAllLevels(statementDateDefaultee.Branch.Company.PK.ToGuid(), statementDateDefaultee.Branch.PK.ToGuid(), Guid.Empty).DoDefaultPrelimStatementPrintDate)
					{
						ValidateDefaultedPSD(prelimStatementDateInfo, statementPrintDate, statementDateDefaultee);
					}
				}
				else
				{
					prelimStatementDateInfo.AddMessageError(PreliminaryPrintDateRequired);
				}
			}
		}

		public void ValidateWorkingDayWithDay11(ZPropertyInfo prelimStatementDateInfo, ZString paymentType, ZString periodicStatementMM)
		{
			ZDateTime statementPrintDate = (ZDateTime)prelimStatementDateInfo.Value;

			if (paymentType == PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate ||
				paymentType == PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter ||
				paymentType == PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes)
			{
				if (new MonthList().ContainsCode(periodicStatementMM))
				{
					var psm = ZInt.ParseSafe(periodicStatementMM, 0);
					var psdMonth = statementPrintDate.Month;
					var year = psdMonth > psm ? statementPrintDate.Date.Year + 1 : statementPrintDate.Date.Year;
					var baseDate = new ZDateTime(year, psm, 1).Date;
					if (baseDate.IsValid)
					{
						var workingDayWithDay11 = new AddInfoJobDeclarationWorkingDate().Get11thWorkingDayOfMonth(baseDate);
						if (statementPrintDate > workingDayWithDay11)
						{
							prelimStatementDateInfo.AddMessageError(PSDMustBeLessThanOrEqual11BusinessDay);
						}
					}
				}
			}
		}

		protected virtual void ValidateDefaultedPSD(ZPropertyInfo prelimStatementDateInfo, ZDateTime statementPrintDate, IPrelimStatementDetailsDefault statementDateDefaultee)
		{
		}

		internal const string PreliminaryPrintDateRequired = "Preliminary Print Date is required for the entered payment type";
		internal const string PreliminaryPrintNotRequired = "Preliminary Print Date is NOT required for payment type 1";
		internal const string PreliminaryPrintDateShouldBeInTheFuture = "Preliminary Statement Print Date should be at least one working day in the future.";
		internal const string PreliminaryPrintDateMoreThan90DaysInTheFuture = "Preliminary Statement Print Date (Payment Date) cannot be more than 90 days in the future.";
		internal const string PrelimStatementPrintDateNotDefaulted = "The Preliminary Statement Print Date differs from the calculated default value, ({0}). The Preliminary Statement Print Date default value is calculated based on the following precedence of date values, {1} plus the number of working days, {2}";

		internal const string FormalDeclarationPrecedence = "Release Date, Presentation Date or Estimated Entry Date, (if entered, in precedence), or the later of Estimated Date of Arrival (ETA) and Current Date";
		internal const string ReconDeclarationPrecedence = "Estimated Recon Date if entered, or Current Date";

		internal const string RegistryWorkingDays = " working days), based upon a System Registry setting. \r\nTo alter the registry, (if required), please adjust registry setting: Maintain > System > Registry > Customs > United States of America > Import > ABI > Statement > Default Statement Print Date.";
		internal const string ImporterOfRecordWorkingDays = " working days, for this specific Importer of Record), based upon their Organization setting. \r\nTo alter this setting, (if required), adjust the 'Statement Print Date working days to be added' field on the Importer of Record details in Organization > Details > Config > US Defaults tab.\r\n\r\nTo alter the underlying system registry if necessary, please adjust the registry setting: Maintain > System > Registry > Customs > United States of America > Import > ABI > Statement > Default Statement Print Date?";
		internal const string PSDMustBeLessThanOrEqual11BusinessDay = "If Payment Type = 6, 7 or 8 the PSD must be less than or equal to 11th business day of the Periodic Statement Month (PSM).";
	}
}
