using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class FlightDetail : NonPersistentBusinessObject, IObsoleteValidation
	{
		public FlightDetail(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public FlightDetail(BusinessObjectFactory factory, FlightDetailCollection flights)
			: base(factory)
		{
			Flights = flights;
		}

		[BusinessObjectTestExclude]
		public FlightDetailCollection Flights { get; private set; }

		#region Schema

		public static class Schema
		{
			public const string Selected = "Selected";
			public const string FlightNo = "FlightNo";
			public const string FlightArrivalDate = "FlightArrivalDate";
			public const string FlightReference = "FlightReference";

			public const int FlightNoMaxLength = 10;
			public const int FlightReferenceMaxLength = 35;
		}

		#endregion

		public ZBool IsArrival { get; set; }

		#region Selected

		public ZBool Selected
		{
			get { return fSelected; }
			set
			{
				SetNonPersistentPropertyValue(SelectedInfo, ref fSelected, value);
				ValidateSelected();
			}
		}
		ZBool fSelected;

		public ZPropertyInfo SelectedInfo
		{
			get { return GetZPropertyInfo(Schema.Selected); }
		}

		protected void ValidateSelected()
		{
			SelectedInfo.ClearAllNotifications();
			if (Selected
				&& Flights != null
				&& Flights.GetSelectedFlights().Any(x => x != this))
			{
				SelectedInfo.AddError("More than 1 Arrival is selected");
			}
		}

		#endregion

		#region FlightNo

		[MaxLength(Schema.FlightNoMaxLength)]
		public ZString FlightNo
		{
			get { return fFlightNo; }
			set
			{
				CheckMaximumLength(FlightNoInfo, value);
				SetNonPersistentPropertyValue(FlightNoInfo, ref fFlightNo, value);
			}
		}
		ZString fFlightNo;

		public ZPropertyInfo FlightNoInfo
		{
			get { return GetZPropertyInfo(Schema.FlightNo); }
		}

		public bool FlightNo_ReadOnly => true;

		#endregion

		#region FlightArrivalDate

		public ZDate FlightArrivalDate
		{
			get { return fFlightArrivalDate; }
			set { SetNonPersistentPropertyValue(FlightArrivalDateInfo, ref fFlightArrivalDate, value); }
		}
		ZDate fFlightArrivalDate;

		public ZPropertyInfo FlightArrivalDateInfo
		{
			get { return GetZPropertyInfo(Schema.FlightArrivalDate); }
		}

		public bool FlightArrivalDate_ReadOnly => true;

		#endregion

		#region FlightReference

		[MaxLength(Schema.FlightReferenceMaxLength)]
		public ZString FlightReference
		{
			get { return fFlightReference; }
			set
			{
				CheckMaximumLength(FlightReferenceInfo, value);
				SetNonPersistentPropertyValue(FlightReferenceInfo, ref fFlightReference, value);
			}
		}
		ZString fFlightReference;

		public ZPropertyInfo FlightReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.FlightReference); }
		}

		public bool FlightReference_ReadOnly => true;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateSelected();
		}

		#endregion
	}
}
