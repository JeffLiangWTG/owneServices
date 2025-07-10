using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class PeriodicInvoicingValidation : JobStorageValidation
	{
		public PeriodicInvoicingValidation(PeriodicInvoicing parent)
			: base(parent)
		{
		}

		#region Warehouse

		protected override void CheckET_WW()
		{
			base.CheckET_WW();
			MandatoryValidation.CheckEntered(Parent.ET_WWInfo);
			var warehouse = Parent.Warehouse;
			if (warehouse != null && !warehouse.WW_IsActive)
			{
				Parent.ET_WWInfo.AddError(
					warehouse.WW_WarehouseType == WarehouseTypes.Codes.ContainerYard
						? ErrorMessages.CannotSelectInactiveContainerYard
						: ErrorMessages.CannotSelectInactiveWarehouse
				);
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
						string clientStoragePeriod = Parent.ET_StorageType == PeriodicInvoicingStorageTypes.Codes.ContainerYard
							? Parent.Client.CompanyData.YardStorageRatingPeriod
							: Parent.Client.CompanyData.GetWarehouseRatingPeriod();

						if (clientStoragePeriod == Core.Constants.StorageCalculationPeriods.Weekly)
						{
							TimeSpan diff = Parent.ET_StorageToDate - Parent.ET_StorageFromDate;
							if ((diff.TotalDays + 1) % 7 != 0)
							{
								Parent.ET_StorageToDateInfo.AddError(Res.GetString("d90d026c-8dbd-44d1-a46b-ab4bafc1ac2e", "The client's storage calculation period is weekly, but the specified 'To' date does not fall on a week boundary (i.e. 7 days)."));
							}
						}
						else if (clientStoragePeriod == Core.Constants.StorageCalculationPeriods.Fortnightly)
						{
							TimeSpan diff = Parent.ET_StorageToDate - Parent.ET_StorageFromDate;
							if ((diff.TotalDays + 1) % 14 != 0)
							{
								Parent.ET_StorageToDateInfo.AddError(Res.GetString("e24b96d0-fe5f-4572-acf7-ba6b3d965bc9", "The client's storage calculation period is fortnightly, but the specified 'To' date does not fall on a fortnight boundary (i.e. 14 days)."));
							}
						}
						else if (clientStoragePeriod == Core.Constants.StorageCalculationPeriods.Monthly)
						{
							if (!IsStorageToDateDayBeforeStorageFromDateInAnyMonth())
							{
								Parent.ET_StorageToDateInfo.AddError(Res.GetString("1d5b8920-1256-4ee1-ae2e-4ba3351f950e", "The client's storage calculation period is monthly, but the specified 'To' date does not fall on a month boundary."));
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
				get { return Res.GetString("1db5ab19-8712-4714-8601-bb64e03fa5dd", "To Date cannot be before From Date."); }
			}
			public static string FromDateInFuture
			{
				get { return Res.GetString("125447ee-5761-4dc8-b782-7c840e5a77c7", "From Date must be not later than today."); }
			}
			public static string ToDateInFuture
			{
				get { return Res.GetString("a7e51b8f-1faf-47db-a459-ee0b1cb2919e", "To Date must be not later than today."); }
			}
			public static string OverlappingPeriods
			{
				get { return Res.GetString("f0914ddb-6b81-406c-9f0f-da194ccf1317", "You can not have overlapping invoicing periods."); }
			}
			public static string CannotSelectInactiveWarehouse
			{
				get { return Res.GetString("734dd8e5-6bb5-4438-a793-5a99fccd5d45", "This warehouse is set to inactive and cannot be used in transactions"); }
			}
			public static string CannotSelectInactiveContainerYard
			{
				get { return Res.GetString("71cd78d1-5282-4304-9f29-8b235956a1c5", "This container yard is set to inactive and cannot be used in transactions"); }
			}
			public static string FutureDateNotAllowPosting
			{
				get { return Res.GetString("675ee9f2-bfbd-4210-bd1d-31ae4210c325", "To Date must not be later than today in order to post charges and/or costs."); }
			}
		}

		protected new PeriodicInvoicing Parent
		{
			get { return (PeriodicInvoicing)base.Parent; }
		}

		#endregion
	}
}
