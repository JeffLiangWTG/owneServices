using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageLegValidation : JobContainerLegsValidation
	{
		public CommonCartageLegValidation(CommonCartageLeg parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateQuickGSDriver();
			ValidateQuickRQTruck();
			ValidateQuickOHTransportCompany();
			ValidateQuickPlannedPickupTime();
			ValidateQuickEstimatedDeliveryTime();
		}

		static string LegNotAuthorizedError
		{
			get { return Res.GetString("62b3f3a7-6efc-409a-8d2a-d37b86e18d34", "This leg is not authorized to be allocated to a Run Sheet."); }
		}

		public void ValidateQuickPlannedPickupTime()
		{
			ValidateCalculatedProperty(CartageLeg.QuickPlannedPickupTimeInfo);
		}

		protected virtual void CheckQuickPlannedPickupTime()
		{
			CartageLeg.Validation.ValidateJU_PlannedPickupTime();
			CartageLeg.QuickPlannedPickupTimeInfo.AddAllNotificationsFrom(CartageLeg.JU_PlannedPickupTimeInfo);
		}

		public void ValidateQuickEstimatedDeliveryTime()
		{
			ValidateCalculatedProperty(CartageLeg.QuickEstimatedDeliveryTimeInfo);
		}

		protected virtual void CheckQuickEstimatedDeliveryTime()
		{
			CartageLeg.Validation.ValidateJU_EstimatedDeliveryTime();
			CartageLeg.QuickEstimatedDeliveryTimeInfo.AddAllNotificationsFrom(CartageLeg.JU_EstimatedDeliveryTimeInfo);
		}

		public void ValidateQuickGSDriver()
		{
			ValidateCalculatedProperty(CartageLeg.QuickGSDriverInfo);
		}

		protected virtual void CheckQuickGSDriver()
		{
			ListValidation.ErrorIfInvalidCode(CartageLeg.QuickGSDriverInfo);

			if (!CartageLeg.QuickGSDriver.IsEmpty && !CartageLeg.IsRunSheetAuthorised)
			{
				CartageLeg.QuickGSDriverInfo.AddError(LegNotAuthorizedError);
			}
			else if (CartageLeg.WorkSheet == null && !CartageLeg.QuickGSDriver.IsEmpty && CartageLeg.QuickEstimatedRunSheetTime.IsEmpty)
			{
				CartageLeg.QuickGSDriverInfo.AddError(Res.GetString("0016508e-8563-485f-84a7-606fe3bb7da4", "This leg cannot be assigned to the Drivers Run Sheet without a Planned Pickup or Delivery Time."));
			}
		}

		public void ValidateQuickRQTruck()
		{
			ValidateCalculatedProperty(CartageLeg.QuickRQTruckInfo);
		}

		protected virtual void CheckQuickRQTruck()
		{
			TypeValidation.CheckValidGuid(CartageLeg.QuickRQTruckInfo);
			ListValidation.ErrorIfInvalidPK(CartageLeg.QuickRQTruckInfo);

			if (!CartageLeg.QuickRQTruck.IsEmpty && !CartageLeg.IsRunSheetAuthorised)
			{
				CartageLeg.QuickRQTruckInfo.AddError(LegNotAuthorizedError);
			}
			else if (CartageLeg.WorkSheet == null && !CartageLeg.QuickRQTruck.IsEmpty && CartageLeg.QuickEstimatedRunSheetTime.IsEmpty)
			{
				CartageLeg.QuickRQTruckInfo.AddError(Res.GetString("317a52c1-eac3-48b3-8465-eed8510ae2b5", "This leg cannot be assigned to the Vehicles Run Sheet without a Planned Pickup or Delivery Time."));
			}
		}

		public void ValidateQuickOHTransportCompany()
		{
			ValidateCalculatedProperty(CartageLeg.QuickOHTransportCompanyInfo);
		}

		protected virtual void CheckQuickOHTransportCompany()
		{
			TypeValidation.CheckValidGuid(CartageLeg.QuickOHTransportCompanyInfo);
			ListValidation.ErrorIfInvalidPK(CartageLeg.QuickOHTransportCompanyInfo);

			if (!CartageLeg.QuickOHTransportCompany.IsEmpty && !CartageLeg.IsRunSheetAuthorised)
			{
				CartageLeg.QuickOHTransportCompanyInfo.AddError(LegNotAuthorizedError);
			}
			else if (CartageLeg.WorkSheet == null && !CartageLeg.QuickOHTransportCompany.IsEmpty && CartageLeg.QuickEstimatedRunSheetTime.IsEmpty)
			{
				CartageLeg.QuickOHTransportCompanyInfo.AddError(Res.GetString("e04c9715-3b28-4d0f-8761-71238d3366a7", "This leg cannot be assigned to the Transport Companies Run Sheet without a Planned Pickup or Delivery Time"));
			}
		}

		protected override void CheckJU_PlannedPickupTime()
		{
			base.CheckJU_PlannedPickupTime();
			HasQuickRunSheetTimeValidation(CartageLeg.JU_PlannedPickupTimeInfo);

			if (CartageLeg.JU_PlannedPickupTime.IsValid && CartageLeg.JU_EstimatedDeliveryTime.IsValid && CartageLeg.JU_PlannedPickupTime > CartageLeg.JU_EstimatedDeliveryTime)
			{
				CartageLeg.JU_PlannedPickupTimeInfo.AddError(Res.GetString("671da430-6a48-48da-beac-d951c48b4045", "Planned Pickup Time cannot be after Estimated Delivery Time."));
			}

			var runSheet = CartageLeg.WorkSheet;
			if (!CartageLeg.HasDeliveryStarted &&
				runSheet != null &&
				CartageLeg.JU_PlannedPickupTime.IsValid &&
				!CartageLeg.JU_EstimatedDeliveryTime.IsValid &&
				(CartageLeg.JU_PlannedPickupTime < runSheet.EY_StartTime || CartageLeg.JU_PlannedPickupTime > runSheet.EY_EndTime))
			{
				CartageLeg.JU_PlannedPickupTimeInfo.AddError(Res.GetString("198ff739-8025-41e1-b257-e61d5afd7484", "If Estimated Delivery Time is not entered, Planned Pickup time is required to be in the time range of {0} to {1} for the Run Sheet '{0}' attached.", runSheet.EY_StartTime.ToLongTimeString(), runSheet.EY_EndTime.ToLongTimeString(), runSheet.EY_RunSheetNumber));
			}
		}

		protected override void CheckJU_EstimatedDeliveryTime()
		{
			base.CheckJU_EstimatedDeliveryTime();
			HasQuickRunSheetTimeValidation(CartageLeg.JU_EstimatedDeliveryTimeInfo);

			if (CartageLeg.JU_PlannedPickupTime.IsValid && CartageLeg.JU_EstimatedDeliveryTime.IsValid && CartageLeg.JU_PlannedPickupTime > CartageLeg.JU_EstimatedDeliveryTime)
			{
				CartageLeg.JU_EstimatedDeliveryTimeInfo.AddError(Res.GetString("2530e337-fc58-4633-ba31-302da6a47577", "Estimated Delivery Time cannot be before Planned Pickup Time."));
			}

			var runSheet = CartageLeg.WorkSheet;
			if (!CartageLeg.HasDeliveryStarted &&
				runSheet != null &&
				CartageLeg.JU_EstimatedDeliveryTime.IsValid &&
				(CartageLeg.JU_EstimatedDeliveryTime < runSheet.EY_StartTime || CartageLeg.JU_EstimatedDeliveryTime > runSheet.EY_EndTime))
			{
				CartageLeg.JU_EstimatedDeliveryTimeInfo.AddError(Res.GetString("f8894d03-5372-424f-be33-836241409734", "Estimated Delivery is required to be in the time range of {0} to {1} for the Run Sheet '{2}' attached.", runSheet.EY_StartTime.ToLongTimeString(), runSheet.EY_EndTime.ToLongTimeString(), runSheet.EY_RunSheetNumber));
			}
		}

		void HasQuickRunSheetTimeValidation(ZPropertyInfo info)
		{
			if (!CartageLeg.HasDeliveryStarted && !CartageLeg.QuickEstimatedRunSheetTime.IsValid && ((CartageLeg.WorkSheet == null && CartageLeg.HasQuickWorkSheetAllocation) || CartageLeg.WorkSheet != null))
			{
				info.AddError(Res.GetString("aa336404-4186-453b-aaf5-b205f430759f", "Estimated Delivery or Planned Pickup Time are required to allocate this leg to a Driver/Vehicle/Transport Company Run Sheet."));
			}
		}

		protected override void CheckJU_PickupTimeIn()
		{
			base.CheckJU_PickupTimeIn();

			var info = CartageLeg.JU_PickupTimeInInfo;
			AddErrorIfLater(info, CartageLeg.JU_PickupTimeOutInfo);

			if (CartageLeg.HasWaitPoint)
			{
				AddWarningIfLater(info, CartageLeg.JU_WaitPointTimeInInfo);
				AddWarningIfLater(info, CartageLeg.JU_WaitPointTimeOutInfo);
			}
			AddWarningIfLater(info, CartageLeg.JU_DeliverTimeInInfo);
			AddWarningIfLater(info, CartageLeg.JU_DeliverTimeOutInfo);
		}

		protected override void CheckJU_PickupTimeOut()
		{
			base.CheckJU_PickupTimeOut();

			var info = CartageLeg.JU_PickupTimeOutInfo;
			AddErrorIfEarlier(info, CartageLeg.JU_PickupTimeInInfo);

			if (CartageLeg.HasWaitPoint)
			{
				AddWarningIfLater(info, CartageLeg.JU_WaitPointTimeInInfo);
				AddWarningIfLater(info, CartageLeg.JU_WaitPointTimeOutInfo);
			}
			AddWarningIfLater(info, CartageLeg.JU_DeliverTimeInInfo);
			AddWarningIfLater(info, CartageLeg.JU_DeliverTimeOutInfo);
		}

		protected override void CheckJU_DeliverTimeIn()
		{
			base.CheckJU_DeliverTimeIn();

			var info = CartageLeg.JU_DeliverTimeInInfo;
			AddErrorIfLater(info, CartageLeg.JU_DeliverTimeOutInfo);

			AddWarningIfEarlier(info, CartageLeg.JU_PickupTimeInInfo);
			AddWarningIfEarlier(info, CartageLeg.JU_PickupTimeOutInfo);
			if (CartageLeg.HasWaitPoint)
			{
				AddWarningIfEarlier(info, CartageLeg.JU_WaitPointTimeInInfo);
				AddWarningIfEarlier(info, CartageLeg.JU_WaitPointTimeOutInfo);
			}
		}

		protected override void CheckJU_DeliverTimeOut()
		{
			base.CheckJU_DeliverTimeOut();

			var info = CartageLeg.JU_DeliverTimeOutInfo;
			AddErrorIfEarlier(info, CartageLeg.JU_DeliverTimeInInfo);

			AddWarningIfEarlier(info, CartageLeg.JU_PickupTimeInInfo);
			AddWarningIfEarlier(info, CartageLeg.JU_PickupTimeOutInfo);
			if (CartageLeg.HasWaitPoint)
			{
				AddWarningIfEarlier(info, CartageLeg.JU_WaitPointTimeInInfo);
				AddWarningIfEarlier(info, CartageLeg.JU_WaitPointTimeOutInfo);
			}
		}

		protected override void CheckJU_WaitPointTimeIn()
		{
			base.CheckJU_WaitPointTimeIn();

			if (CartageLeg.HasWaitPoint)
			{
				var info = CartageLeg.JU_WaitPointTimeInInfo;
				AddErrorIfLater(info, CartageLeg.JU_WaitPointTimeOutInfo);

				AddWarningIfEarlier(info, CartageLeg.JU_PickupTimeInInfo);
				AddWarningIfEarlier(info, CartageLeg.JU_PickupTimeOutInfo);
				AddWarningIfLater(info, CartageLeg.JU_DeliverTimeInInfo);
				AddWarningIfLater(info, CartageLeg.JU_DeliverTimeOutInfo);
			}
		}

		protected override void CheckJU_WaitPointTimeOut()
		{
			base.CheckJU_WaitPointTimeOut();

			if (CartageLeg.HasWaitPoint)
			{
				var info = CartageLeg.JU_WaitPointTimeOutInfo;
				AddErrorIfEarlier(info, CartageLeg.JU_WaitPointTimeInInfo);

				AddWarningIfEarlier(info, CartageLeg.JU_PickupTimeInInfo);
				AddWarningIfEarlier(info, CartageLeg.JU_PickupTimeOutInfo);
				AddWarningIfLater(info, CartageLeg.JU_DeliverTimeInInfo);
				AddWarningIfLater(info, CartageLeg.JU_DeliverTimeOutInfo);
			}
		}

		protected override void CheckJU_E2PickupAddressID()
		{
			base.CheckJU_E2PickupAddressID();

			var pickupAddressLine = PickupAddressLine;
			if (!pickupAddressLine.IsEmpty)
			{
				var waitPointAddressLine = WaitPointAddressLine;
				if (!waitPointAddressLine.IsEmpty && pickupAddressLine == waitPointAddressLine)
				{
					CartageLeg.JU_E2PickupAddressIDInfo.AddError(Res.GetString("adfd0e0f-bbcf-4de2-aace-0fa0a840d1f8", "Pickup Address cannot be the same as Wait Point Address"));
				}
				else if (waitPointAddressLine.IsEmpty && pickupAddressLine == DeliveryAddressLine)
				{
					CartageLeg.JU_E2PickupAddressIDInfo.AddError(Res.GetString("c4173410-beae-4eae-ac91-ff63e5bb4719", "Pickup Address cannot be the same as Delivery Address"));
				}
			}
		}

		protected override void CheckJU_E2WaitPointAddressID()
		{
			base.CheckJU_E2WaitPointAddressID();

			var waitPointAddressLine = WaitPointAddressLine;
			if (!waitPointAddressLine.IsEmpty)
			{
				if (waitPointAddressLine == PickupAddressLine)
				{
					CartageLeg.JU_E2WaitPointAddressIDInfo.AddError(Res.GetString("5ce54d00-0fc2-430d-85ff-ab8d29cde99c", "Wait Point Address cannot be the same as Pickup Address"));
				}
				else if (waitPointAddressLine == DeliveryAddressLine)
				{
					CartageLeg.JU_E2WaitPointAddressIDInfo.AddError(Res.GetString("72d8b1da-193c-4e88-b18f-2865435f3a4b", "Wait Point Address cannot be the same as Delivery Address"));
				}
			}
		}

		protected override void CheckJU_E2DeliveryAddressID()
		{
			base.CheckJU_E2DeliveryAddressID();

			var deliveryAddressLine = DeliveryAddressLine;
			if (!deliveryAddressLine.IsEmpty)
			{
				var waitPointAddressLine = WaitPointAddressLine;
				if (!waitPointAddressLine.IsEmpty && deliveryAddressLine == waitPointAddressLine)
				{
					CartageLeg.JU_E2DeliveryAddressIDInfo.AddError(Res.GetString("26e3ef89-04e1-4bb4-8c10-91bd04ebcd2c", "Delivery Address cannot be the same as Wait Point Address"));
				}
				else if (waitPointAddressLine.IsEmpty && deliveryAddressLine == PickupAddressLine)
				{
					CartageLeg.JU_E2DeliveryAddressIDInfo.AddError(Res.GetString("d0d28ecd-e08e-4f2b-ad5a-f9cc84af65e0", "Delivery Address cannot be the same as Pickup Address"));
				}
			}
		}

		protected override void CheckJU_EY_RunSheet()
		{
			base.CheckJU_EY_RunSheet();

			if (CartageLeg.JU_EY_RunSheet.IsValid && !CartageLeg.IsRunSheetAuthorised)
			{
				CartageLeg.JU_EY_RunSheetInfo.AddError(LegNotAuthorizedError);
			}
			else
			{
				var runSheet = CartageLeg.WorkSheet;
				if (runSheet != null && runSheet.HasRunSheetError && (!CartageLeg.IsInDatabase || (ZGuid)CartageLeg.JU_EY_RunSheetInfo.OriginalValue != CartageLeg.JU_EY_RunSheet))
				{
					CartageLeg.JU_EY_RunSheetInfo.AddError(Res.GetString("66e03d5f-e778-4afe-a2ff-e55df9e4bb79", "Cannot attach a Port Transport Leg to a Run Sheet with Status: {0}", runSheet.StatusDescription));
				}

				if (runSheet != null && !CartageLeg.JU_EY_RunSheetInfo.HasErrors())
				{
					runSheet.Validation.CheckTruckOrDriverDoesntOverrlapOtherRunSheets(CartageLeg.JU_EY_RunSheetInfo, CommonWorkSheetValidation.DriverTruckCheck.CheckBoth);
				}
			}
		}

		protected override void CheckJU_CartagePickupDemurrage()
		{
			base.CheckJU_CartagePickupDemurrage();
			CheckTimeIsValid(CartageLeg.JU_CartagePickupDemurrageInfo, Res.GetString("1680A02C-11D9-4695-AE34-63EFFA7980B0", "Pick Up Time should be within 00:00 and 999:59."));
		}

		protected override void CheckJU_CartageDeliveryDemurrage()
		{
			base.CheckJU_CartageDeliveryDemurrage();
			CheckTimeIsValid(CartageLeg.JU_CartageDeliveryDemurrageInfo, Res.GetString("54BAEC36-F465-11E0-8468-4D334924019B", "Delivery Time should be within 00:00 and 999:59."));
		}

		protected override void CheckJU_MessageStatus()
		{
			base.CheckJU_MessageStatus();

			ListValidation.ErrorIfInvalidCode(CartageLeg.JU_MessageStatusInfo);
		}

		void CheckTimeIsValid(ZPropertyInfo propertyInfo, string errorMessage)
		{
			if (!propertyInfo.Value.IsEmpty && !propertyInfo.HasErrors())
			{
				var timeInfo = (ZDateTime)propertyInfo.Value;
				var timeSpan = timeInfo.TimeSpan6MonthsFromStartOfYear;

				if (timeSpan.TotalMinutes < 0 || timeSpan.TotalMinutes > 59999) // 999:59 is the max valid value.
				{
					propertyInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckJU_DistanceUnit()
		{
			base.CheckJU_DistanceUnit();
			if (CartageLeg.JU_DistanceUnit.IsEmpty)
			{
				MandatoryValidation.CheckEntered(CartageLeg.JU_DistanceUnitInfo);
			}
			ListValidation.ErrorIfInvalidCode(CartageLeg.JU_DistanceUnitInfo, BindToLists.GetCachedLists(CartageLeg.Factory).DistanceUnits);
		}

		CommonCartageLeg CartageLeg
		{
			get { return (CommonCartageLeg)Parent; }
		}

		void AddErrorIfEarlier(ZPropertyInfo info, ZPropertyInfo compareToInfo)
		{
			if ((ZDateTime)info.Value < (ZDateTime)compareToInfo.Value)
			{
				info.AddError(Res.GetString("bdc25926-c885-4fc0-9a0d-f57042843ec9", "{0} must be the same or later than {1}.", info.HumanReadableName, compareToInfo.HumanReadableName));
			}
		}

		void AddErrorIfLater(ZPropertyInfo info, ZPropertyInfo compareToInfo)
		{
			if ((ZDateTime)info.Value > (ZDateTime)compareToInfo.Value)
			{
				info.AddError(Res.GetString("506433eb-2d73-49fb-abdb-dca6e7b4cf47", "{0} must be the same or earlier than {1}.", info.HumanReadableName, compareToInfo.HumanReadableName));
			}
		}

		void AddWarningIfEarlier(ZPropertyInfo info, ZPropertyInfo compareToInfo)
		{
			if ((ZDateTime)info.Value < (ZDateTime)compareToInfo.Value)
			{
				info.AddWarning(Res.GetString("62B55FD8-DEE3-495B-ACA7-BF45CD726BD0", "{0} is earlier than {1}.", info.HumanReadableName, compareToInfo.HumanReadableName));
			}
		}

		void AddWarningIfLater(ZPropertyInfo info, ZPropertyInfo compareToInfo)
		{
			if ((ZDateTime)info.Value > (ZDateTime)compareToInfo.Value)
			{
				info.AddWarning(Res.GetString("2AC155AC-3601-4193-BBA6-B331D5C12C6A", "{0} is later than {1}.", info.HumanReadableName, compareToInfo.HumanReadableName));
			}
		}

		ZString PickupAddressLine
		{
			get
			{
				var pickupFromDocAddress = CartageLeg.PickupFromDocAddress;
				return pickupFromDocAddress != null ? pickupFromDocAddress.E2_CompanyNameTruncated + pickupFromDocAddress.AddressAsASingleLine : "";
			}
		}

		ZString WaitPointAddressLine
		{
			get
			{
				var waitPointDocAddress = CartageLeg.WaitPointDocAddress;
				return waitPointDocAddress != null ? waitPointDocAddress.E2_CompanyNameTruncated + waitPointDocAddress.AddressAsASingleLine : "";
			}
		}

		ZString DeliveryAddressLine
		{
			get
			{
				var deliverToDocAddress = CartageLeg.DeliverToDocAddress;
				return deliverToDocAddress != null ? deliverToDocAddress.E2_CompanyNameTruncated + deliverToDocAddress.AddressAsASingleLine : "";
			}
		}
	}
}
