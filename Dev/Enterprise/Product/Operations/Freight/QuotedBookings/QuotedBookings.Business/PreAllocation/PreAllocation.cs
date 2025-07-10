using System.ComponentModel;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class PreAllocation : NonPersistentBusinessObjectWithLogsAndNotes, IDocumentSupportable
	{
		#region Schema

		public static class Schema
		{
			public const string HouseBillCount = "HouseBillCount";
			public const string HouseBillFrom = "HouseBillFrom";
			public const string HouseBillTo = "HouseBillTo";
			public const string IsPrePrinted = "IsPrePrinted";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public PreAllocation(QuotedBooking quotedBooking, PreAllocationState preAllocationState)
			: base(quotedBooking.Factory)
		{
			QuotedBooking = quotedBooking;
			RegisterEditableChildObject(QuotedBooking);
			State = preAllocationState;

			SetDefaults();
		}

		#region SetDefaults

		void SetDefaults()
		{
			IsPrePrinted = true;
			QuotedBooking.DisableLoadDischargeDefaulting();
			QuotedBooking.Booking.SuppressShipmentNumberValidation = true;
		}

		#endregion

		#region QuotedBooking

		public QuotedBooking QuotedBooking
		{
			get { return quotedBooking; }
			private set { quotedBooking = value; }
		}
		QuotedBooking quotedBooking;

		#endregion

		#region State

		public enum PreAllocationState
		{
			None = 0,
			New = 1,
			Existing = 2,
			PrintOnly = 3,
		}

		public PreAllocationState State
		{
			get { return preAllocationState; }
			set
			{
				preAllocationState = value;

				if (preAllocationState == PreAllocationState.Existing)
				{
					QuotedBooking.TransportMode_ReadOnly = true;
					QuotedBooking.ContainerMode_ReadOnly = true;
					QuotedBooking.ClientAddrPK_ReadOnly = true;
					QuotedBooking.ConsignorDocumentaryAddress.ReadOnly = true;
					QuotedBooking.ConsigneeDocumentaryAddress.ReadOnly = true;
					QuotedBooking.Origin_ReadOnly = true;
					QuotedBooking.Destination_ReadOnly = true;
					QuotedBooking.ServiceLevel_ReadOnly = true;
					QuotedBooking.Booking.JS_GoodsDescription_ReadOnly = true;
				}
				else if (preAllocationState == PreAllocationState.PrintOnly)
				{
					QuotedBooking.ReadOnly = true;
					QuotedBooking.ConsignorDocumentaryAddress.ReadOnly = true;
					QuotedBooking.ConsigneeDocumentaryAddress.ReadOnly = true;
					HouseBillCount_ReadOnly = true;
					this.RefreshBindingIncludingChildren();
				}
			}
		}
		PreAllocationState preAllocationState;

		#endregion

		#region Pre-Allocation Properties

		#region HouseBillPrefix

		ZString HouseBillPrefix
		{
			get
			{
				string clientCode = QuotedBooking.Client != null && QuotedBooking.Client.MiscServ != null && !QuotedBooking.Client.MiscServ.OM_EXPreAllocPrefix.IsEmpty ? QuotedBooking.Client.MiscServ.OM_EXPreAllocPrefix.ToString() : "???";
				return Enterprise.Freight.Business.CommonShipment.PreAllocatedHouseBillPrefix + clientCode;
			}
		}

		#endregion

		#region HouseBillNumberFrom

		public ZInt HouseBillNumberFrom
		{
			get { return houseBillNumberFrom; }
			set
			{
				houseBillNumberFrom = value;
				HouseBillFrom = FormatHouseBillNumber(value);
			}
		}
		ZInt houseBillNumberFrom;

		#endregion

		#region HouseBillNumberTo

		public ZInt HouseBillNumberTo
		{
			get { return houseBillNumberTo; }
			set
			{
				houseBillNumberTo = value;
				HouseBillTo = FormatHouseBillNumber(value);
			}
		}
		ZInt houseBillNumberTo;

		#endregion

		#region HouseBillCount

		public ZInt HouseBillCount
		{
			get { return houseBillCount; }
			set
			{
				houseBillCount = value;

				GenerateHouseBills();

				HouseBillCountInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateHouseBillCount();
				}
			}
		}
		ZInt houseBillCount;

		public ZPropertyInfo HouseBillCountInfo
		{
			get { return GetZPropertyInfo(Schema.HouseBillCount); }
		}

		public bool HouseBillCount_ReadOnly
		{
			get; set;
		}

		#endregion

		#region HouseBillFrom

		[ReadOnly(true)]
		[MaxLength(14)]
		public ZString HouseBillFrom
		{
			get { return houseBillFrom; }
			set
			{
				CheckMaximumLength(HouseBillFromInfo, value);
				houseBillFrom = value;
				HouseBillFromInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateHouseBillFrom();
				}
			}
		}
		ZString houseBillFrom;

		public ZPropertyInfo HouseBillFromInfo
		{
			get { return GetZPropertyInfo(Schema.HouseBillFrom); }
		}

		#endregion

		#region HouseBillTo

		[ReadOnly(true)]
		[MaxLength(14)]
		public ZString HouseBillTo
		{
			get { return houseBillTo; }
			set
			{
				CheckMaximumLength(HouseBillToInfo, value);
				houseBillTo = value;
				HouseBillToInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateHouseBillTo();
				}
			}
		}
		ZString houseBillTo;

		public ZPropertyInfo HouseBillToInfo
		{
			get { return GetZPropertyInfo(Schema.HouseBillTo); }
		}

		#endregion

		#region IsPrePrinted

		public ZBool IsPrePrinted
		{
			get { return isPrePrinted; }
			set
			{
				isPrePrinted = value;
				IsPrePrintedInfo.RefreshBinding();
			}
		}
		ZBool isPrePrinted;

		public ZPropertyInfo IsPrePrintedInfo
		{
			get { return GetZPropertyInfo(Schema.IsPrePrinted); }
		}

		#endregion

		#endregion

		public const int MaxHouseBillCount = 10000;

		#region Generate HouseBills

		public void GenerateHouseBills()
		{
			if (HouseBillCount > 0 && HouseBillCount <= MaxHouseBillCount)
			{
				ForwardingShipment[] shipments = ShipmentsThatMatchHouseBillPrefix(false);

				int max = 0;
				foreach (ForwardingShipment shipment in shipments)
				{
					if (shipment.JS_HouseBill.Length == 14 && shipment.JS_HouseBill.SubstringSafe(HouseBillPrefix.Length).IsNumbersOnlyOrEmpty)
					{
						string numberAsString = shipment.JS_HouseBill.SubstringSafe(HouseBillPrefix.Length);
						int number;
						if (int.TryParse(numberAsString, out number) && number > max)
						{
							max = number;
						}
						break;
					}
				}

				if (max + HouseBillCount < 99999999)
				{
					HouseBillNumberFrom = max + 1;
					HouseBillNumberTo = HouseBillNumberFrom + HouseBillCount - 1;
				}
				else
				{
					HouseBillFrom = "";
					HouseBillTo = "";
				}
			}
			else
			{
				HouseBillFrom = "";
				HouseBillTo = "";
			}
		}

		public ForwardingShipment[] ShipmentsThatMatchHouseBillPrefix(bool includeBookingOnly)
		{
			var factory = new BusinessObjectFactory();
			ZQuery houseBillQuery = new ZQuery(JobShipmentSchema.JS_HouseBill, SQLComparisonOperator.StartsWith, HouseBillPrefix);
			if (includeBookingOnly)
			{
				houseBillQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false);
				houseBillQuery.AddToFilter(JobShipmentSchema.JS_IsBooking, true);
			}

			houseBillQuery.OrderBy = JobShipmentSchema.JS_HouseBill.Name + " DESC";
			return factory.Load<ForwardingShipment>(houseBillQuery);
		}

		public ZString FormatHouseBillNumber(ZInt number)
		{
			return HouseBillPrefix + number.ToString("00000000", CultureInfo.InvariantCulture);
		}

		#endregion

		#region Bookings Created

		public ForwardingShipmentCollection Bookings
		{
			get { return bookings ?? (bookings = new ForwardingShipmentCollection(Factory)); }
		}
		ForwardingShipmentCollection bookings;

		public void Create()
		{
			if (State != PreAllocationState.PrintOnly && HouseBillCount > 0 && !HouseBillFrom.IsEmpty && !HouseBillTo.IsEmpty)
			{
				for (int i = 0; i < HouseBillCount; i++)
				{
					var booking = i == 0 && State == PreAllocationState.New ? QuotedBooking.Booking : QuotedBooking.CreateNewBooking(Factory);
					var copyQuotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

					using (new DisposableAction(() => copyQuotedBooking.DisableLoadDischargeDefaulting(), () => copyQuotedBooking.EnableLoadDischargeDefaulting()))
					{
						copyQuotedBooking.ClientPK = QuotedBooking.ClientPK;
						copyQuotedBooking.TransportMode = QuotedBooking.TransportMode;
						copyQuotedBooking.ContainerMode = QuotedBooking.ContainerMode;
						copyQuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address;
						copyQuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address;
						copyQuotedBooking.Origin = QuotedBooking.Origin;
						copyQuotedBooking.Destination = QuotedBooking.Destination;
						copyQuotedBooking.ServiceLevel = QuotedBooking.ServiceLevel;
						copyQuotedBooking.Booking.JS_GoodsDescription = QuotedBooking.Booking.JS_GoodsDescription;
						copyQuotedBooking.Booking.DetailedGoodsDescriptionNoteText = QuotedBooking.Booking.DetailedGoodsDescriptionNoteText;
						copyQuotedBooking.Booking.JS_HouseBillOfLadingType = QuotedBooking.Booking.JS_HouseBillOfLadingType;

						booking.SuppressShipmentNumberValidation = true;
						ClientShipmentTmpLink.Store(QuotedBooking.ClientPK, booking.PK, Factory);
						copyQuotedBooking.Booking.JS_HouseBill = FormatHouseBillNumber(HouseBillNumberFrom + i);
						Bookings.Add(booking);
					}
				}

				State = PreAllocationState.PrintOnly;
			}
		}

		#endregion

		#region Overrides

		protected override BusinessObject LogsAndNotesTarget
		{
			get { return QuotedBooking.Booking; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("56ccdca5-dde3-4c4c-886f-440c8eb29d32", "Pre-Allocation"); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public PreAllocationValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual PreAllocationValidation GetNewValidation()
		{
			return new PreAllocationValidation(this);
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new PreAllocationDocumentSupporter(this); }
		}

		#endregion
	}
}
