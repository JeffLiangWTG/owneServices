using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoiceValidation : JobStorageValidation
	{
		public WhsInvoiceValidation(WhsInvoice parent)
			: base(parent)
		{
		}

		#region Warehouse

		protected override void CheckET_WW()
		{
			base.CheckET_WW();
			MandatoryValidation.CheckEntered(Parent.ET_WWInfo);
			if (Parent.Warehouse != null && !Parent.Warehouse.WW_IsActive)
			{
				Parent.ET_WWInfo.AddError(ErrorMessages.CannotSelectInactiveWarehouse);
			}
		}

		#endregion

		#region From Date

		protected override void CheckET_StorageFromDate()
		{
			base.CheckET_StorageFromDate();

			if (Parent.IncludeInInvoicing)
			{
				MandatoryValidation.CheckEntered(Parent.ET_StorageFromDateInfo);
				if (!Parent.ET_StorageFromDateInfo.HasErrors())
				{
					if (Parent.ET_StorageFromDate > ZDateTime.Today)
					{
						Parent.ET_StorageFromDateInfo.AddError(ErrorMessages.FromDateInFuture);
					}
					else if (Parent.ET_StorageFromDate > Parent.ET_StorageToDate)
					{
						Parent.ET_StorageFromDateInfo.AddError(ErrorMessages.ToDateBeforeFromDate);
					}
					else
					{
						CheckForOverlappingPeriods(Parent.ET_StorageFromDateInfo);
					}
				}
			}
		}

		#endregion

		#region To Date

		protected override void CheckET_StorageToDate()
		{
			base.CheckET_StorageToDate();

			if (Parent.IncludeInInvoicing)
			{
				MandatoryValidation.CheckEntered(Parent.ET_StorageToDateInfo);
				if (!Parent.ET_StorageToDateInfo.HasErrors())
				{
					if (Parent.ET_StorageToDate < Parent.ET_StorageFromDate)
					{
						Parent.ET_StorageToDateInfo.AddError(ErrorMessages.ToDateBeforeFromDate);
					}
					else
					{
						CheckForOverlappingPeriods(Parent.ET_StorageToDateInfo);
					}

					if (Parent.Client != null && Parent.ET_StorageFromDate.IsValid && Parent.ET_StorageToDate.IsValid)
					{
						string clientStoragePeriod = Parent.Client.CompanyData.GetWarehouseRatingPeriod();

						if (clientStoragePeriod == Core.Constants.StorageCalculationPeriods.Weekly)
						{
							TimeSpan diff = Parent.ET_StorageToDate - Parent.ET_StorageFromDate;
							if ((diff.TotalDays + 1) % 7 != 0)
							{
								Parent.ET_StorageToDateInfo.AddError(Res.GetString("8518dd6c-dcef-4339-8d97-f97fe783ca12", "The client's storage calculation period is weekly, but the specified 'To' date does not fall on a week boundary (i.e. 7 days)."));
							}
						}
						else if (clientStoragePeriod == Core.Constants.StorageCalculationPeriods.Fortnightly)
						{
							TimeSpan diff = Parent.ET_StorageToDate - Parent.ET_StorageFromDate;
							if ((diff.TotalDays + 1) % 14 != 0)
							{
								Parent.ET_StorageToDateInfo.AddError(Res.GetString("c4a63d5c-079d-4402-8b09-56700f5e23f8", "The client's storage calculation period is fortnightly, but the specified 'To' date does not fall on a fortnight boundary (i.e. 14 days)."));
							}
						}
						else if (clientStoragePeriod == Core.Constants.StorageCalculationPeriods.Monthly)
						{
							if (!IsStorageToDateDayBeforeStorageFromDateInAnyMonth())
							{
								Parent.ET_StorageToDateInfo.AddError(Res.GetString("964a3418-f958-447d-9197-f776263829fa", "The client's storage calculation period is monthly, but the specified 'To' date does not fall on a month boundary."));
							}
						}
					}
				}
			}
		}

		bool IsStorageToDateDayBeforeStorageFromDateInAnyMonth()
		{
			int storageToDateLastDay = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(Parent.ET_StorageToDate.Year, Parent.ET_StorageToDate.Month);
			return
					Parent.ET_StorageToDate.Day == Parent.ET_StorageFromDate.Day - 1 ||
					(Parent.ET_StorageFromDate.Day == 1 && Parent.ET_StorageToDate.Day == storageToDateLastDay);
		}

		#endregion

		#region Billing Date

		protected override void CheckET_BillingDate()
		{
			base.CheckET_BillingDate();
			MandatoryValidation.CheckEntered(Parent.ET_BillingDateInfo);
		}

		#endregion

		#region OffBand Processing Status

		protected override void CheckET_OffBandProcessingStatus()
		{
			base.CheckET_OffBandProcessingStatus();
			ListValidation.ErrorIfInvalidCode(Parent.ET_OffBandProcessingStatusInfo);
		}

		#endregion

		#region RequiresDateOverlappingValidation

		protected bool RequiresDateOverlappingValidation =>
			!Parent.IsInDatabase
			|| Parent.ET_StorageFromDateInfo.HasChanges
			|| Parent.ET_StorageToDateInfo.HasChanges
			|| Parent.ET_WWInfo.HasChanges
			|| Parent.ET_OH_ClientInfo.HasChanges;

		#endregion

		#region VerifyJobHeaderNotificationErrors

		public void ValidateVerifyJobHeaderNotificationErrors()
		{
			ValidateCalculatedProperty(Parent.VerifyJobHeaderNotificationErrorsInfo);
		}

		protected void CheckVerifyJobHeaderNotificationErrors()
		{
			if (Parent.VerifyJobHeaderNotificationErrors && (Parent.JobHeader?.Notifications.HasErrors() ?? false))
			{
				Parent.VerifyJobHeaderNotificationErrorsInfo.AddError(Parent.JobHeader.Notifications.GetErrors().ToMessageListString());
			}
		}

		#endregion

		#region Implementation

		void CheckForOverlappingPeriods(ZPropertyInfo propertyInfo)
		{
			if (Parent.ET_StorageFromDate.IsValid && Parent.ET_StorageToDate.IsValid && RequiresDateOverlappingValidation)
			{
				if (Parent.HasOverlappingInvoices())
				{
					propertyInfo.AddError(ErrorMessages.OverlappingPeriods);
				}
			}
		}

		public static class ErrorMessages
		{
			public static string ToDateBeforeFromDate
			{
				get { return Res.GetString("4902d465-eeae-476d-8107-4cad618495a1", "To Date cannot be before From Date."); }
			}
			public static string FromDateInFuture
			{
				get { return Res.GetString("67dcf891-00c6-4d34-bdaa-1600e2e5f802", "From Date must be not later than today."); }
			}
			public static string ToDateInFuture
			{
				get { return Res.GetString("0cad2ba7-188e-4ebc-8a63-c40c31733f19", "To Date must be not later than today."); }
			}
			public static string OverlappingPeriods
			{
				get { return Res.GetString("163b3220-b570-413e-b5d7-1677c8f67805", "You can not have overlapping invoicing periods."); }
			}
			public static string CannotSelectInactiveWarehouse
			{
				get { return Res.GetString("c2b609a0-2718-4a38-b156-4c491e62b200", "This warehouse is set to inactive and cannot be used in transactions"); }
			}
			public static string FutureDateNotAllowPosting
			{
				get { return Res.GetString("363afc59-b696-45b7-9faa-a9ca4b3296d7", "To Date must not be later than today in order to post charges and/or costs."); }
			}
		}

		protected new WhsInvoice Parent
		{
			get { return (WhsInvoice)base.Parent; }
		}

		#endregion
	}
}
