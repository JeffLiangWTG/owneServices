using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[CodeProperty("UniqueCode"), DescriptionProperty("UniqueCode")] // UniqueCode is an unique calculated property
	[DebuggerDisplay("{UniqueCode}")]
	public class RateTransportProvider : AutoRateTransportProvider, IRateTransportProvider
	{
		public RateTransportProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoRateTransportProvider.Schema
		{
			public const string ZoneHubStateLabel = "ZoneHubStateLabel";
		}

		#endregion

		#region IRateTransportProvider

		IRateTransportZonesCollection IRateTransportProvider.Zones
		{
			get { return Zones; }
		}

		#endregion

		#region Zones

		[ChildEditable(true)]
		public RateTransportZonesCollection Zones
		{
			get
			{
				if (fZones == null)
				{
					fZones = new RateTransportZonesCollection(this);
					RegisterEditableChildObject(fZones);
				}

				return fZones;
			}
		}
		RateTransportZonesCollection fZones;

		#endregion

		#region Properties

		[ZDateTimeDurationValue]
		public override ZDateTime TP_DefaultDeliveryDueTime
		{
			get => base.TP_DefaultDeliveryDueTime;
			set => base.TP_DefaultDeliveryDueTime = value.ConvertToDurationBasedDate(TP_DefaultDeliveryDueTimeInfo);
		}

		[ZDateTimeDurationValue]
		public override ZDateTime TP_DefaultHoldForPickupTime
		{
			get => base.TP_DefaultHoldForPickupTime;
			set => base.TP_DefaultHoldForPickupTime = value.ConvertToDurationBasedDate(TP_DefaultHoldForPickupTimeInfo);
		}

		public ZString UniqueCode
		{
			get
			{
				var result = string.Join("-", RelatedPartyCode, TP_ZoneType, TP_ZoneMode, CountryCode);
				if (ZoneHubLocation != null)
				{
					result = string.Join("-", result, ZoneHubLocation.R9_InternationalName).ToUpperInvariant();
				}

				return result.ToUpperInvariant();
			}
		}

		public ZPropertyInfo UniqueCodeInfo
		{
			get { return GetZPropertyInfo(nameof(UniqueCode)); }
		}

		public ZString RelatedPartyCode
		{
			get
			{
				ZString result = ZString.Empty;

				if (RelatedParty != null)
				{
					result = RelatedParty.OH_Code;
				}
				else
				{
					result = Res.GetString("62e5a064-615c-4a62-9c5e-836dfb23683f", "Generic Zone");
				}

				return result;
			}
		}

		[MaxLength(OrgHeader.Schema.OH_FullNameMaxLength)]
		public ZString RelatedPartyFullName
		{
			get
			{
				var result = ZString.Empty;

				if (RelatedParty != null)
				{
					result = RelatedParty.OH_FullNameTruncated;
				}
				else
				{
					result = Res.GetString("62e5a064-615c-4a62-9c5e-836dfb23683f", "Generic Zone");
				}

				return result;
			}
		}

		public ZPropertyInfo RelatedPartyFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedPartyFullName)); }
		}

		#region Zone Type

		[List("Lookups.ZoneTypes")]
		public override ZString TP_ZoneType
		{
			get { return base.TP_ZoneType; }
			set
			{
				if (base.TP_ZoneType != value)
				{
					base.TP_ZoneType = value;

					if (value == RatingConstants.RatingZoneTypes.Reporting)
					{
						TP_OH_RelatedParty = ZGuid.Empty;
					}
					else if (!IsValidationSuspended)
					{
						Validation.ValidateTP_OH_RelatedParty();
					}

					var zoneModeList = Lookups.ZoneModes;
					if (zoneModeList != null && zoneModeList.Count > 0)
					{
						TP_ZoneMode = zoneModeList[0].Code;
					}

					TP_ZoneModeInfo.RefreshBinding();
				}
			}
		}

		public ZBool IsZoneSetOwnerFieldVisible
		{
			get { return TP_ZoneType != RatingConstants.RatingZoneTypes.Reporting; }
		}

		public ZBool IsZoneModeNonEditable
		{
			get { return TP_ZoneType != RatingConstants.RatingZoneTypes.Rating; }
		}

		#endregion

		[List("Lookups.ZoneModes")]
		public override ZString TP_ZoneMode
		{
			get => base.TP_ZoneMode;
			set
			{
				if (base.TP_ZoneMode != value)
				{
					base.TP_ZoneMode = value;
				}
			}
		}

		#region Locations

		[List("Lookups.ZoneHubLocations")]
		public override ZGuid TP_R9_ZoneHubLocation
		{
			get { return base.TP_R9_ZoneHubLocation; }
			set
			{
				if (base.TP_R9_ZoneHubLocation != value || ZoneHubLocation != null && ZoneHubLocation.R9_RN_NKCountry != TP_RN_NKCountry)
				{
					var previousCountryCode = ZoneHubLocation?.R9_RN_NKCountry ?? ZString.Empty;
					base.TP_R9_ZoneHubLocation = value;

					TP_RN_NKCountry = !value.IsValid ? previousCountryCode : ZString.Empty;
				}

				CountryCodeInfo.RefreshBinding();
				TP_R9_ZoneHubLocationInfo.RefreshBinding();
			}
		}

		public override RefCountry Country
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCode); }
		}

		[List("Lookups.Countries")]
		[MaxLength(Schema.TP_RN_NKCountryMaxLength)]
		public ZString CountryCode
		{
			get { return ZoneHubLocation != null ? ZoneHubLocation.R9_RN_NKCountry : TP_RN_NKCountry; }
			set
			{
				if (ZoneHubLocation == null)
				{
					TP_R9_ZoneHubLocation = ZGuid.Empty;
					TP_RN_NKCountry = value;
				}

				CountryCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CountryCode), x => TP_RN_NKCountryInfo); }
		}

		protected bool CountryCode_ReadOnly
		{
			get { return ZoneHubLocation != null; }
		}

		#endregion

		#region IsActive

		public override ZBool TP_IsActive
		{
			get
			{
				return base.TP_IsActive;
			}
			set
			{
				if (value != base.TP_IsActive)
				{
					base.TP_IsActive = value;
					if (!value)
					{
						DeactivateChildZones();
					}
					Zones.SetReadOnlyIncludingChildren(!TP_IsActive);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTP_OH_RelatedParty();
					}
				}
			}
		}

		void DeactivateChildZones()
		{
			foreach (var zone in Zones)
			{
				zone.TZ_IsActive = false;
			}
		}

		#endregion

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Delete

		public override void Delete()
		{
			Zones.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			TP_ZoneMode = RatingConstants.RatingZoneTypes.All;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			TP_RN_NKCountry = Env.CurrentCompany.Country.Code;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		#endregion
	}
}

