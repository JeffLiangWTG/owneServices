//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefTransitTimeValidation
//
//    This class should be used for overriding validation in AutoRefTransitTimeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Schema;

	public class RefTransitTimeValidation : AutoRefTransitTimeValidation
	{
		public RefTransitTimeValidation(AutoRefTransitTime parent) : base(parent)
		{
		}

		public new RefTransitTime Parent
		{
			get { return (RefTransitTime)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTransitDays();
			ValidateTransitHours();
		}

		protected override void CheckRTT_RS_NKServiceLevel()
		{
			base.CheckRTT_RS_NKServiceLevel();

			MandatoryValidation.CheckEntered(Parent.RTT_RS_NKServiceLevelInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RTT_RS_NKServiceLevelInfo);

			var existingTransitTimeQuery = new ZQuery(RefTransitTimeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			existingTransitTimeQuery.AddToFilter(RefTransitTimeSchema.RTT_Mode, Parent.RTT_Mode);
			existingTransitTimeQuery.AddToFilter(RefTransitTimeSchema.RTT_RS_NKServiceLevel, Parent.RTT_RS_NKServiceLevel);
			existingTransitTimeQuery.AddToFilter(RefTransitTimeSchema.RTT_TZ_OriginDomesticZone, !Parent.RTT_TZ_OriginDomesticZone.IsEmpty ? Parent.RTT_TZ_OriginDomesticZone : null);
			existingTransitTimeQuery.AddToFilter(RefTransitTimeSchema.RTT_TZ_DestinationDomesticZone, !Parent.RTT_TZ_DestinationDomesticZone.IsEmpty ? Parent.RTT_TZ_DestinationDomesticZone : null);
			existingTransitTimeQuery.AddToFilter(RefTransitTimeSchema.RTT_FZ_OriginInternationalZone, !Parent.RTT_FZ_OriginInternationalZone.IsEmpty ? Parent.RTT_FZ_OriginInternationalZone : null);
			existingTransitTimeQuery.AddToFilter(RefTransitTimeSchema.RTT_FZ_DestinationInternationalZone, !Parent.RTT_FZ_DestinationInternationalZone.IsEmpty ? Parent.RTT_FZ_DestinationInternationalZone : null);

			if (Parent.Factory.LoadTop1<RefTransitTime>(existingTransitTimeQuery) != null)
			{
				Parent.RTT_RS_NKServiceLevelInfo.AddError(Res.GetString("f8e3121a-9289-4e55-ada8-d0bce45af0e3", "Transit Time for this Zone combination has already been configured for this Service Level."));
			}

			var serviceLevel =
				Parent.Factory.LoadTop1<RefServiceLevel>(new ZQuery(RefServiceLevelSchema.RS_Code,
					Parent.RTT_RS_NKServiceLevel));

			if (serviceLevel?.DeliverOnWeekend == true)
			{
				Parent.RTT_RS_NKServiceLevelInfo.AddError(Res.GetString("EA7828FC-00AA-45D8-9D47-5217E12A5A5F",
					"Weekend Delivery Service Level cannot be used for Transit Time."));
			}
		}

		protected override void CheckRTT_Mode()
		{
			base.CheckRTT_Mode();

			MandatoryValidation.CheckEntered(Parent.RTT_ModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RTT_ModeInfo, Parent.Lookups.Modes);
		}

		public void ValidateTransitDays()
		{
			ValidateCalculatedProperty(Parent.TransitDaysInfo);
		}

		protected virtual void CheckTransitDays()
		{
			MandatoryValidation.CheckNotNegative(Parent.TransitDaysInfo);

			if (!FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive && Parent.TransitDays == 0 && Parent.TransitHours == 0)
			{
				Parent.TransitDaysInfo.AddError(TransitTimeGreaterThanZeroError);
			}
		}

		public void ValidateTransitHours()
		{
			ValidateCalculatedProperty(Parent.TransitHoursInfo);
		}

		protected virtual void CheckTransitHours()
		{
			MandatoryValidation.CheckNotNegative(Parent.TransitHoursInfo);

			if (!FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive && Parent.TransitDays == 0 && Parent.TransitHours == 0)
			{
				Parent.TransitHoursInfo.AddError(TransitTimeGreaterThanZeroError);
			}

			if (Parent.TransitHours >= 24)
			{
				Parent.TransitHoursInfo.AddError(Res.GetString("4132839b-5630-447e-b27b-7eec99d8f07a", "Hours must be less than 24. Please use the 'Days' component for longer times."));
			}
		}

		static string TransitTimeGreaterThanZeroError
		{
			get { return Res.GetString("4fa8c52a-06b6-490b-89bd-c34301868b94", "Please specify a Transit time greater than zero."); }
		}
	}
}
