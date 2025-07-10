using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobDeclarationWorkingDate
	{
		public AddInfoJobDeclarationWorkingDate()
		{
		}

		public ZDate GeneratePaymentDueDate(ZDate baseDate)
		{
			const int PymtDueDaysToAdd = 10;
			return GenerateWorkingDate(baseDate, PymtDueDaysToAdd);
		}

		public ZDate GeneratePrelimStmtDate(IPrelimStatementDetailsDefault prelimStatementDetails)
		{
			return GenerateWorkingDate(prelimStatementDetails.BaseDateToCalculateOn, prelimStatementDetails.DaysToAddToStatementDate);
		}

		public ZInt DaysToAddToStatementDate(ZGuid companyPK, ZGuid branchPK, OrgHeader importerOfRecord)
		{
			var daysToAdd = IOROverridenDays(importerOfRecord);

			if (daysToAdd == 0)
			{
				daysToAdd = USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.GetFallBackValueAtAllLevels(companyPK.ToGuid(), branchPK.ToGuid(), Guid.Empty).NumberOfDays;
			}

			return daysToAdd;
		}

		public ZInt IOROverridenDays(OrgHeader importerOfRecord)
		{
			ZInt iORSetting = 0;

			if (importerOfRecord != null)
			{
				iORSetting = OrgHeaderWrapper.New(importerOfRecord).ZO_SPDNumberOfDays;
			}

			return iORSetting;
		}

		ZDate GenerateWorkingDate(ZDate baseDate, int daysToAdd)
		{
			ZDateTime result = WorkingDays.GetAnotherStandardWorkingDay(baseDate.ToDateTime(), daysToAdd);
			return result.Date;
		}

		CustomsWorkingDays WorkingDays
		{
			get { return workingDays ?? (workingDays = CustomsWorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" })); }
		}
		CustomsWorkingDays workingDays;

		public ZDate GenerateDeferredTaxDueDate(JobDeclaration declaration)
		{
			var result = declaration.US_DeferredTaxDueDate;
			if (declaration.TaxDeferred && !declaration.US_FixDefTaxDueDate)
			{
				var dateForCalculation = GetDateToCalculateOn(declaration);
				if (!dateForCalculation.IsEmpty)
				{
					var isDateInFirstSemimonthlyPeriod = dateForCalculation.Day >= 1 && dateForCalculation.Day <= 15;

					result = dateForCalculation.Month == 9 && !isDateInFirstSemimonthlyPeriod ? CalculateDeferredDueDateForExceptions(declaration, dateForCalculation)
						: CalculateDeferredDueDate(dateForCalculation, isDateInFirstSemimonthlyPeriod);

					if (WorkingDays.IsDateACustomsFederalHoliday(result) || WorkingDays.IsDateAWeekend(result.ToDateTime()))
					{
						result = WorkingDays.GetAnotherStandardWorkingDay(result.ToDateTime(), -1);
					}
				}
			}
			return result.Date;
		}

		ZDateTime CalculateDeferredDueDateForExceptions(JobDeclaration declaration, ZDateTime dateForCalculation)
		{
			var year = dateForCalculation.Year;
			return declaration.DeferredTaxToBePaidByEFT
				? GetDate(dateForCalculation.Day >= 16 && dateForCalculation.Day <= 26, year, 29)
				: declaration.TaxToBeDeferred ? GetDate(dateForCalculation.Day >= 16 && dateForCalculation.Day <= 25, year, 28) : ZDateTime.Empty;
		}

		ZDateTime GetDate(bool isFirstPaymentPeriod, int year, int day)
		{
			return isFirstPaymentPeriod ? new ZDateTime(year, 9, day) : new ZDateTime(year, 10, 14);
		}

		ZDateTime CalculateDeferredDueDate(ZDateTime dateForCalculation, bool isDateInFirstSemimonthlyPeriod)
		{
			var result = ZDateTime.Empty;
			if (isDateInFirstSemimonthlyPeriod)
			{
				var day = dateForCalculation.Month == 2 && !DateTime.IsLeapYear(dateForCalculation.Year) ? 28 : 29;
				result = new ZDateTime(dateForCalculation.Year, dateForCalculation.Month, day);
			}
			else
			{
				var isDecember = dateForCalculation.Month == 12;
				var year = isDecember ? dateForCalculation.Year + 1 : dateForCalculation.Year;
				var month = isDecember ? 1 : dateForCalculation.Month + 1;
				result = new ZDateTime(year, month, 14);
			}
			return result;
		}

		ZDateTime GetDateToCalculateOn(JobDeclaration declaration)
		{
			var dateForCalculation = ZDateTime.Empty;
			if (EntryTypeList.IsExWarehouseType(declaration.US_EntryType))
			{
				dateForCalculation = declaration.US_EstimatedEntryDate;
			}
			else
			{
				var importerValue = declaration.IORWrapper != null ? declaration.IORWrapper.ZO_DefTaxDateCalcOption : ZString.Empty;
				var dateConfigValue = !importerValue.IsEmpty ? importerValue.ToString() : DataRegistry.Business.USCustomsDataRegistry.Instance.DefTaxDueDateCalculationOption.GetValueWithoutFallback(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);

				var useCollectionDate = dateConfigValue == DefTaxDueDateCalculationOptionList.Codes.COL;

				var releaseDateFallbackToEntryDate = declaration.JE_EntryAuthorisationDate.IsEmpty ? declaration.US_EntryDate : declaration.JE_EntryAuthorisationDate;
				if (useCollectionDate)
				{
					if (EntryTypeList.IsConsumption(declaration.US_EntryType))
					{
						dateForCalculation = !declaration.US_CollectionDate.IsEmpty ? declaration.US_CollectionDate : declaration.US_PaymentDueDate;
					}
					else //quota entry types
					{
						dateForCalculation = !declaration.US_CollectionDate.IsEmpty ? declaration.US_CollectionDate : releaseDateFallbackToEntryDate;
					}
				}
				else //use release date with fallback to another dates, for consumption and quota entries
				{
					dateForCalculation = releaseDateFallbackToEntryDate;
				}
			}
			return dateForCalculation;
		}

		public ZDateTime Get11thWorkingDayOfMonth(ZDate dateToValidateAgainst)
		{
			if (dateToValidateAgainst.IsValid)
			{
				return GenerateWorkingDate(dateToValidateAgainst.AddDays(-dateToValidateAgainst.Day), 11);
			}
			else
			{
				return ZDateTime.Empty;
			}
		}
	}
}
