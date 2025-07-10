using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefServiceLevel : AutoRefServiceLevel, IDocManagerSupport, IRefServiceLevel
	{
		public new class Schema : AutoRefServiceLevel.Schema
		{
			public const string DefaultTransitDays = "DefaultTransitDays";
			public const string DefaultTransitHours = "DefaultTransitHours";
			public const string DefaultTransitTimeFormatted = "DefaultTransitTimeFormatted";
		}

		public RefServiceLevel(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			defaultTransitDays = RS_DefaultTransitHours / 24;
			defaultTransitHours = RS_DefaultTransitHours % 24;
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RS_ServiceDeliveryType = ServiceLevelDeliveryTypeList.Codes.DIFOT;
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ServiceLevel);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region Properties

		[TranslatableDataField(Schema.TableName, Schema.RS_Description, DataXmlFilePaths.RefTables, MaxLength = Schema.RS_DescriptionMaxLength, Type = typeof(RefServiceLevel), SecurityCheckpoint = "ServiceLevelsModify", Asmid = ResString.AssemblyId)]
		public override ZString RS_Description
		{
			get { return base.RS_Description; }
			set { base.RS_Description = value; }
		}

		public MultilingualString RS_DescriptionMultilingual
		{
			get { return GetMultilingual(RS_DescriptionInfo); }
		}

		[ReadOnlyMember(nameof(IsServiceLevelNonDIFOT))]
		public override ZByte RS_ServiceDeliveryPercentage
		{
			get { return base.RS_ServiceDeliveryPercentage; }
			set { base.RS_ServiceDeliveryPercentage = value; }
		}

		[List("Lookups.ServiceDeliveryTypeList")]
		public override ZString RS_ServiceDeliveryType
		{
			get { return base.RS_ServiceDeliveryType; }
			set
			{
				base.RS_ServiceDeliveryType = value;

				if (IsServiceLevelNonDIFOT)
				{
					RS_ServiceDeliveryPercentage = 0;
				}
			}
		}

		[ResourceStringData("RefServiceLevel|ServiceDeliveryTypeDescription", ShortCaption = "Agreement", Caption = "Service Agreement", FullDescription = "Indicates the delivery agreement for this service level. This can either be Guaranteed, or a Delivery-in-Full-on-Time (DIFOT) based percentage.")]
		public ZString ServiceDeliveryTypeDescription
		{
			get { return Lookups.ServiceDeliveryTypeList.GetDescriptionFromCode(RS_ServiceDeliveryType); }
		}

		public ZBool IsServiceLevelNonDIFOT
		{
			get { return RS_ServiceDeliveryType != ServiceLevelDeliveryTypeList.Codes.DIFOT; }
		}

		public ZInt DefaultTransitDays
		{
			get { return defaultTransitDays; }
			set
			{
				if (defaultTransitDays != value)
				{
					SetNonPersistentPropertyValue(DefaultTransitDaysInfo, ref defaultTransitDays, value);
					CalculateTotalHours();

					if (!IsValidationSuspended)
					{
						Validation.ValidateDefaultTransitDays();
						Validation.ValidateDefaultTransitHours();
					}
				}
			}
		}

		ZInt defaultTransitDays;

		public ZPropertyInfo DefaultTransitDaysInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultTransitDays); }
		}

		public ZInt DefaultTransitHours
		{
			get { return defaultTransitHours; }
			set
			{
				if (defaultTransitHours != value)
				{
					SetNonPersistentPropertyValue(DefaultTransitHoursInfo, ref defaultTransitHours, value);
					CalculateTotalHours();

					if (!IsValidationSuspended)
					{
						Validation.ValidateDefaultTransitDays();
						Validation.ValidateDefaultTransitHours();
					}
				}
			}
		}

		ZInt defaultTransitHours;

		public ZPropertyInfo DefaultTransitHoursInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultTransitHours); }
		}

		void CalculateTotalHours()
		{
			RS_DefaultTransitHours = (DefaultTransitDays * 24) + DefaultTransitHours;
		}

		[ResourceStringData("RefServiceLevel|DefaultTransitTimeFormatted", ShortCaption = "Transit Time", Caption = "Default Transit Time", FullDescription = "The Default Transit Time of this service level.")]
		public ZString DefaultTransitTimeFormatted
		{
			get
			{
				var days = DefaultTransitDays == 1
					? Res.GetString("2d0c2e4f-0c6a-4c30-88f3-0b3004de8d96", "1 day")
					: Res.GetString("c2463817-0411-47eb-a007-369290283753", "{0} days", DefaultTransitDays);
				var hours = DefaultTransitHours == 1
					? Res.GetString("7fb82151-c734-4417-a6db-b2f1c6522987", "1 hour")
					: Res.GetString("60291a17-273f-421c-9523-a1dc561dcaac", "{0} hours", DefaultTransitHours);

				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", days, hours);
			}
		}

		public override ZDateTime RS_DefaultArrivalTime
		{
			get
			{
				return base.RS_DefaultArrivalTime;
			}
			set
			{
				ZDateTime defaultArrivalTime = value;
				if (value.IsValid)
				{
					defaultArrivalTime = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, 0);
				}
				base.RS_DefaultArrivalTime = defaultArrivalTime;
			}
		}

		public override ZDateTime RS_DefaultDeliveryDueTime
		{
			get
			{
				return base.RS_DefaultDeliveryDueTime;
			}
			set
			{
				ZDateTime defaultDeliveryDueTime = value;
				if (value.IsValid)
				{
					defaultDeliveryDueTime = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, 0);
				}
				base.RS_DefaultDeliveryDueTime = defaultDeliveryDueTime;
			}
		}

		[ResourceStringData("RefServiceLevel|DeliverOnWeekend", ShortCaption = "On Weekend", Caption = "Deliver On Weekend", FullDescription = "The flag of Deliver On Weekend of this service level.")]
		public ZBool DeliverOnWeekend => RS_DeliverOnSaturday || RS_DeliverOnSunday;

		[ResourceStringData("RefServiceLevel|DefaultArrivalTime", ShortCaption = "Arrival Time", Caption = "Default Arrival Time", FullDescription = "The Default Arrival Time of this service level.")]
		public ZString DefaultArrivalTime
		{
			get
			{
				if (RS_DefaultArrivalTime.IsValid)
				{
					return RS_DefaultArrivalTime.ToShortTimeString();
				}
				return ZString.Empty;
			}
		}

		[ResourceStringData("RefServiceLevel|ServiceDeliveryDueTime", ShortCaption = "Delivery Due Time", Caption = "Service Delivery Due Time", FullDescription = "The Default Service Delivery Due Time of this service level.")]
		public ZString ServiceDeliveryDueTime
		{
			get
			{
				if (RS_DefaultDeliveryDueTime.IsValid)
				{
					return RS_DefaultDeliveryDueTime.ToShortTimeString();
				}
				return ZString.Empty;
			}
		}

		#endregion
	}
}
