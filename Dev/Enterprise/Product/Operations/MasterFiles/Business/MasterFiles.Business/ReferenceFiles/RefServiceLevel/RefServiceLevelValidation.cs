using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class RefServiceLevelValidation : AutoRefServiceLevelValidation
	{
		public RefServiceLevelValidation(AutoRefServiceLevel parent)
			: base(parent)
		{
		}

		public new RefServiceLevel Parent
		{
			get { return (RefServiceLevel)base.Parent; }
		}

		#region RS_Code

		protected override void CheckRS_Code()
		{
			base.CheckRS_Code();
			MandatoryValidation.CheckEntered(Parent.RS_CodeInfo);
		}

		#endregion

		#region RS_Description

		protected override void CheckRS_Description()
		{
			base.CheckRS_Description();
			MandatoryValidation.CheckEntered(Parent.RS_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.RS_DescriptionInfo);
		}

		#endregion

		#region RS_ServiceDeliveryType

		protected override void CheckRS_ServiceDeliveryType()
		{
			base.CheckRS_ServiceDeliveryType();
			MandatoryValidation.CheckEntered(Parent.RS_ServiceDeliveryTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RS_ServiceDeliveryTypeInfo);
		}

		#endregion

		#region RS_ServiceDeliveryPercentage

		protected override void CheckRS_ServiceDeliveryPercentage()
		{
			base.CheckRS_ServiceDeliveryPercentage();
			if (Parent.RS_ServiceDeliveryPercentage > 100)
			{
				Parent.RS_ServiceDeliveryPercentageInfo.AddError(Res.GetString("8b565021-273c-49ac-8b6c-c83c8e333a7a", "DIFOT Percentage should be between 0 and 100."));
			}

			if (Parent.RS_ServiceDeliveryType != ServiceLevelDeliveryTypeList.Codes.Guaranteed && Parent.RS_ServiceDeliveryPercentage == 100)
			{
				Parent.RS_ServiceDeliveryPercentageInfo.AddError(Res.GetString("7e280417-56b3-4b7a-8671-11fec1ba2cd0", "Entering a DIFOT Percentage of 100 means that you are guaranteeing delivery. You should change the Agreement type to Guaranteed."));
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateDefaultTransitDays();
			ValidateDefaultTransitHours();
		}

		public void ValidateDefaultTransitDays()
		{
			ValidateCalculatedProperty(Parent.DefaultTransitDaysInfo);
		}

		protected virtual void CheckDefaultTransitDays()
		{
			MandatoryValidation.CheckNotNegative(Parent.DefaultTransitDaysInfo);

			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive
				&& (!Parent.RS_DefaultArrivalTime.IsEmpty || !Parent.RS_DefaultDeliveryDueTime.IsEmpty || Parent.DeliverOnWeekend)
				&& (Parent.DefaultTransitDays == 0 && Parent.DefaultTransitHours == 0))
			{
				Parent.DefaultTransitDaysInfo.AddError(TransitTimeGreaterThanZeroError);
			}
		}

		public void ValidateDefaultTransitHours()
		{
			ValidateCalculatedProperty(Parent.DefaultTransitHoursInfo);
		}

		protected virtual void CheckDefaultTransitHours()
		{
			MandatoryValidation.CheckNotNegative(Parent.DefaultTransitHoursInfo);

			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive
				&& (!Parent.RS_DefaultArrivalTime.IsEmpty || !Parent.RS_DefaultDeliveryDueTime.IsEmpty || Parent.DeliverOnWeekend)
				&& Parent.DefaultTransitDays == 0
				&& Parent.DefaultTransitHours == 0)
			{
				Parent.DefaultTransitHoursInfo.AddError(TransitTimeGreaterThanZeroError);
			}

			ValidateArrivalTimeAndTransitHour(Parent.DefaultTransitHoursInfo);

			if (Parent.DefaultTransitHours >= 24)
			{
				Parent.DefaultTransitHoursInfo.AddError(Res.GetString("4132839a-5631-447e-b27b-7eec19d8f07c", "Hours must be less than 24. Please use the 'Days' component for longer times."));
			}
		}

		protected override void CheckRS_DefaultArrivalTime()
		{
			base.ValidateRS_DefaultArrivalTime();
			ValidateArrivalTimeAndTransitHour(Parent.RS_DefaultArrivalTimeInfo);
		}

		public void ValidateArrivalTimeAndTransitHour(ZPropertyInfo info)
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive && Parent.RS_DefaultArrivalTime.IsValid && Parent.DefaultTransitHours > 0)
			{
				info.AddError(Res.GetString("95ab1263-4582-42f6-91b1-df971a3c10c4", "When arrival time has a value, transit hours must be zero, and conversely."));
				return;
			}
		}

		static string TransitTimeGreaterThanZeroError
		{
			get { return Res.GetString("4fa8c52a-02b6-410b-89bd-c34301864192", "Please specify a Transit time greater than zero."); }
		}

		protected override void CheckRS_DefaultArrivalTimeIsValidZDateTimeRange()
		{
		}

		protected override void CheckRS_DefaultDeliveryDueTimeIsValidZDateTimeRange()
		{
		}
	}
}
