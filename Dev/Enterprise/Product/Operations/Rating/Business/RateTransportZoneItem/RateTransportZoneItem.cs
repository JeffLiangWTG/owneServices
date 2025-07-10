using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[DependentBusinessObject(typeof(RateTransportZone), "Items")]
	public class RateTransportZoneItem : AutoRateTransportZoneItem, IRateTransportZoneItem
	{
		public new class Schema : AutoRateTransportZoneItem.Schema
		{
			public const string BeyondDays = "BeyondDays";
			public const string BeyondHours = "BeyondHours";
			public const string BeyondTimeFormatted = "BeyondTimeFormatted";
		}

		public RateTransportZoneItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CalculateBeyongDaysAndHours();
		}

		void CalculateBeyongDaysAndHours()
		{
			beyondDays = TQ_BeyondHours / 24;
			beyondHours = TQ_BeyondHours % 24;
		}

		#region Properties

		public override ZDateTime TQ_DeliveryDueTime
		{
			get { return base.TQ_DeliveryDueTime; }
			set
			{
				ZDateTime deliveryDueTime = value;
				if (value.IsValid)
				{
					deliveryDueTime = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, value.Second);
				}

				base.TQ_DeliveryDueTime = deliveryDueTime;
			}
		}

		public string ToDescriptiveString()
		{
			var result = string.Empty;
			if (CityTown != null)
			{
				if (TQ_FromPostCode == ZString.Empty)
				{
					result = Res.GetString("431548d2-d711-4051-8960-fa210b1d9425", "country/region ({0}) + city/town ({1})", TQ_RN_NKCountry, CityTown.R9_InternationalName);
				}
				else
				{
					result = Res.GetString("fc04e5f5-029d-464d-a6a2-91e02cf96f28", "country/region ({0}) + city/town ({1}) + postcode ({2})", TQ_RN_NKCountry, CityTown.R9_InternationalName, TQ_FromPostCode);
				}
			}
			else if (TQ_ToDistance > 0)
			{
				result = Res.GetString("c92d493f-2046-49c1-8ac0-ada76adff41c", "country/region ({0}) + distance range ({1} km to {2} km)", TQ_RN_NKCountry, TQ_FromDistance, TQ_ToDistance);
			}
			else if (TQ_FromPostCode != ZString.Empty)
			{
				if (TQ_ToPostCode != ZString.Empty)
				{
					result = Res.GetString("d427680b-db1b-4afc-9262-1d524da19bdd", "country/region ({0}) + postcode range ({1} to {2})", TQ_RN_NKCountry, TQ_FromPostCode, TQ_ToPostCode);
				}
				else
				{
					result = Res.GetString("e067399b-6892-4f62-8dee-5eb78a820302", "country/region ({0}) + postcode ({1})", TQ_RN_NKCountry, TQ_FromPostCode);
				}
			}

			return result;
		}

		[List("Lookups.CityTowns")]
		[MaxLength(AutoRefCityTown.Schema.R9_InternationalNameMaxLength)]
		public override ZGuid TQ_R9_CityTown
		{
			get
			{
				return base.TQ_R9_CityTown;
			}
			set
			{
				if (base.TQ_R9_CityTown != value)
				{
					base.TQ_R9_CityTown = value;
					if (value == ZGuid.Invalid)
					{
						TQ_FromPostCode = ZString.Empty;
					}
					if (value != ZGuid.Empty)
					{
						TQ_ToPostCode = ZString.Empty;
					}
					Validation.ValidateTQ_FromPostCode();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(RateTransportZoneItemLookups.PostCodes))]
		public override ZString TQ_FromPostCode
		{
			get
			{
				return base.TQ_FromPostCode;
			}
			set
			{
				if (base.TQ_FromPostCode != value)
				{
					base.TQ_FromPostCode = value;

					if (value == ZString.Empty)
					{
						TQ_R9_CityTown = ZGuid.Empty;
					}
					Validation.ValidateTQ_R9_CityTown();
				}
			}
		}

		/// <summary>
		/// Returns the UI FieldType to use for FromPostCode
		/// If there is a City/Town then uses a drop list, otherwise a code lookup.
		/// </summary>
		public ZString PostCodeControlType
		{
			get
			{
				return (TQ_R9_CityTown == ZGuid.Empty || TQ_R9_CityTown == ZGuid.Invalid || !Factory.Exists(typeof(RefCityPCodePivot), new ZQuery(RefCityPCodePivotSchema.R0_R9, TQ_R9_CityTown)))
					? nameof(FieldType.TextCodeFindBox)
					: nameof(FieldType.TextDropEdit);
			}
		}

		public RefPostCode FromPostCode
			=> LoadRefPostCode(TQ_FromPostCode);

		public RefPostCode ToPostCode
			=> LoadRefPostCode(TQ_ToPostCode);

		internal RefPostCode LoadRefPostCode(ZString postCode)
			=> LoadRefPostCode(Factory, postCode, TQ_RN_NKCountry);

		static RefPostCode LoadRefPostCode(BusinessObjectFactory factory, ZString postCode, ZString countryCode)
		{
			if (!postCode.IsEmpty && !countryCode.IsEmpty)
			{
				var query = new ZQuery(RefPostCodeSchema.RK_RN_NKCountry, countryCode)
					.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, postCode);
				// prefer active since the system allows both active and inactive for the same code
				query.OrderBy = RefPostCode.Schema.RK_IsActive + " desc";
				return factory.LoadTop1<RefPostCode>(query);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Returns the UI FieldType to use for City/Town.
		/// If the from-post-code is linked to any City/Town via RefCityPCodePivot then returns FieldType.GuidDropEdit
		/// otherwise a FieldType.Guid.
		/// </summary>
		public ZString CityTownControlType
		{
			get
			{
				var fromPostCode = FromPostCode;
				return IsLinkedToCity(fromPostCode)
					? nameof(FieldType.GuidDropEdit)
					: nameof(FieldType.Guid);
			}
		}

		bool IsLinkedToCity(RefPostCode postCode)
		{
			return postCode != null
				&& Factory.Exists(typeof(RefCityPCodePivot), new ZQuery(RefCityPCodePivotSchema.R0_RK, postCode.PK));
		}

		[List("Lookups.PostCodes")]
		public override ZString TQ_ToPostCode
		{
			get { return base.TQ_ToPostCode; }
			set { base.TQ_ToPostCode = value; }
		}

		public bool TQ_ToPostCode_ReadOnly
		{
			get { return TQ_R9_CityTown != ZGuid.Empty; }
		}

		public bool TQ_R9_CityTown_ReadOnly
		{
			get { return TQ_FromPostCode != ZString.Empty && TQ_ToPostCode != ZString.Empty; }
		}

		public bool TQ_IsExcludingPostCode_ReadOnly
			=> !IsRatingZoneType(Zone?.TransportProvider?.TP_ZoneType);

		static bool IsRatingZoneType(string zoneType)
			=> zoneType == RatingConstants.RatingZoneTypes.Rating || zoneType == RatingConstants.RatingZoneTypes.All;

		#region BeyondHours

		public override ZBool TQ_IsBeyond
		{
			get => base.TQ_IsBeyond;
			set
			{
				base.TQ_IsBeyond = value;
				if (!value)
				{
					TQ_BeyondHours = 0;
					CalculateBeyongDaysAndHours();
				}
			}
		}

		public ZInt BeyondDays
		{
			get { return beyondDays; }
			set
			{
				if (beyondDays != value)
				{
					SetNonPersistentPropertyValue(BeyondDaysInfo, ref beyondDays, value);
					CalculateTotalHours();

					if (!IsValidationSuspended)
					{
						Validation.ValidateBeyondDays();
						Validation.ValidateBeyondHours();
					}
				}
			}
		}

		protected bool BeyondDays_ReadOnly
		{
			get
			{
				return !TQ_IsBeyond;
			}
		}

		ZInt beyondDays;

		public ZPropertyInfo BeyondDaysInfo
		{
			get { return GetZPropertyInfo(Schema.BeyondDays); }
		}

		public ZInt BeyondHours
		{
			get { return beyondHours; }
			set
			{
				if (beyondHours != value)
				{
					SetNonPersistentPropertyValue(BeyondHoursInfo, ref beyondHours, value);
					CalculateTotalHours();

					if (!IsValidationSuspended)
					{
						Validation.ValidateBeyondHours();
						Validation.ValidateBeyondDays();
					}
				}
			}
		}

		protected bool BeyondHours_ReadOnly
		{
			get
			{
				return !TQ_IsBeyond;
			}
		}

		ZInt beyondHours;

		public ZPropertyInfo BeyondHoursInfo
		{
			get { return GetZPropertyInfo(Schema.BeyondHours); }
		}

		void CalculateTotalHours()
		{
			var beyondHoursCandidate = (long)BeyondDays * 24 + BeyondHours;
			if (beyondHoursCandidate > int.MaxValue)
			{
				TQ_BeyondHours = int.MaxValue;
			}
			else if (beyondHoursCandidate < int.MinValue)
			{
				TQ_BeyondHours = int.MinValue;
			}
			else
			{
				TQ_BeyondHours = (ZInt)beyondHoursCandidate;
			}
		}

		public ZString BeyondTimeFormatted
		{
			get
			{
				var days = BeyondDays == 1
					? Res.GetString("ac6cc5fd-dbd8-450c-8256-d234fee7a802", "1 day")
					: Res.GetString("cb61a141-0657-44e5-93b5-15b8e2b3b08e", "{0} days", BeyondDays);
				var hours = BeyondHours == 1
					? Res.GetString("50cfa655-3ed2-4a0d-8c0b-d68c33818e80", "1 hour")
					: Res.GetString("15d63b86-e1f3-48af-a32d-7b122c65b48f", "{0} hours", BeyondHours);

				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", days, hours);
			}
		}

		#endregion

		#endregion

		#region Related Business Objects

		public RateTransportZone Zone
		{
			get { return Factory.Load<RateTransportZone>(TQ_TZ_DomesticZone); }
		}

		[RelatedBusinessObject("Zone")]
		public override ZGuid TQ_TZ_DomesticZone
		{
			get { return base.TQ_TZ_DomesticZone; }
			set
			{
				base.TQ_TZ_DomesticZone = value;

				var provider = Zone?.TransportProvider;
				if (provider != null)
				{
					TQ_RN_NKCountry = !provider.TP_RN_NKCountry.IsEmpty ? provider.TP_RN_NKCountry : provider.ZoneHubLocation != null ? provider.ZoneHubLocation.R9_RN_NKCountry : ZString.Empty;
				}
			}
		}

		#endregion

		public void CheckOrCreatePostCodeFromWebIfNotInDatabase(string postCode)
		{
			if (!string.IsNullOrWhiteSpace(postCode) && LoadRefPostCode(postCode) == null)
			{
				var helper = new RefCityTownPostcodeHelper(Lookups.PostCodes, TQ_RN_NKCountry, string.Empty, CityTown?.R9_InternationalName, CityTown?.R9_RW_NKState);
				helper.GetPostcodePKFromCode(postCode);
			}
		}
	}
}
