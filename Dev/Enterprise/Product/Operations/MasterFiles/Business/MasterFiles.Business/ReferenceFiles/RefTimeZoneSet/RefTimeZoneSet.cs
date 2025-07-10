using System.Collections;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefTimeZoneSet.Schema.R3_TimeZoneSetName), DescriptionProperty(RefTimeZoneSet.Schema.R3_TimeZoneSetName)]
	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	public class RefTimeZoneSet : AutoRefTimeZoneSet
	{
		public RefTimeZoneSet(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CreateStandardZoneIfNotExists();
		}

		#region ReadOnly

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			return property.Name != nameof(R3_IsActive) && R3_IsSystem;
		}

		#endregion

		#region Related Business Objects

		#region Standard Zone

		internal void CreateStandardZoneIfNotExists()
		{
			if (StandardZone == null)
			{
				using (SuspendSettingHasChanges())
				{
					StandardTimeZone newZone = Factory.New<StandardTimeZone>();
					using (newZone.SuspendSettingHasChanges())
					{
						R3_R2_StandardZone = newZone.PK;
					}
				}
			}

			RegisterEditableChildObject(StandardZone);
		}

		public new StandardTimeZone StandardZone
		{
			get
			{
				return Factory.Load<StandardTimeZone>(R3_R2_StandardZone);
			}
		}

		#endregion

		#region Daylight Saving Zone

		internal void CreateDaylightSavingZoneIfNotExists()
		{
			if (DaylightSavingZone == null)
			{
				DaylightSavingTimeZone savingZone = DaylightSavingZones.AddNew();
				savingZone.R2_OffsetMinutesFromUTC = StandardZone.R2_OffsetMinutesFromUTC + 60;
				R3_R2_DaylightSavingZone = savingZone.PK;
				DaylightSavingZones.AdditionalFilter = new ZQuery(RefTimeZoneSchema.PK, R3_R2_DaylightSavingZone);
			}
		}

		[ChildEditable(false)]
		public DaylightSavingTimeZoneCollection DaylightSavingZones
		{
			get
			{
				if (fDSTZCollection == null)
				{
					fDSTZCollection = new DaylightSavingTimeZoneCollection(Factory);
					UpdateDaylightSavingZonesAdditionalFilter();
					RegisterEditableChildObject(DaylightSavingZones);
				}

				return fDSTZCollection;
			}
		}

		DaylightSavingTimeZoneCollection fDSTZCollection;

		void UpdateDaylightSavingZonesAdditionalFilter()
		{
			if (!R3_R2_DaylightSavingZone.IsEmpty)
			{
				DaylightSavingZones.AdditionalFilter = new ZQuery(RefTimeZoneSchema.PK, R3_R2_DaylightSavingZone);
			}
			else
			{
				DaylightSavingZones.AdditionalFilter = ZQuery.NoResultQuery;
			}
		}

		public new DaylightSavingTimeZone DaylightSavingZone
		{
			get
			{
				DaylightSavingTimeZone result = null;

				if (DaylightSavingZones.Count != 0)
				{
					result = DaylightSavingZones[0];
					result.StartDateRules?.SetReadOnlyIncludingChildren(R3_IsSystem);
					result.EndDateRules?.SetReadOnlyIncludingChildren(R3_IsSystem);
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Properties

		public override ZGuid R3_R2_DaylightSavingZone
		{
			get { return base.R3_R2_DaylightSavingZone; }
			set
			{
				var originalValue = R3_R2_DaylightSavingZone;
				base.R3_R2_DaylightSavingZone = value;

				if (originalValue != value)
				{
					UpdateDaylightSavingZonesAdditionalFilter();
				}
			}
		}

		#region HasDaylightSavings

		public ZBool HasDaylightSavings
		{
			get { return DaylightSavingZone != null; }
			set
			{
				if (value)
				{
					CreateDaylightSavingZoneIfNotExists();
				}
				else
				{
					DaylightSavingZones.DeleteAll();
					R3_R2_DaylightSavingZone = ZGuid.Empty;
				}
				Validation.ValidateHasDaylightSavings();
				HasDaylightSavingsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HasDaylightSavingsInfo
		{
			get { return GetZPropertyInfo(nameof(HasDaylightSavings)); }
		}

		#endregion

		#endregion

		#region CalculationTimeZone

		public ITimeZone GetCalculationTimeZone()
		{
			string zoneSetName = this.R3_TimeZoneSetName;
			decimal utcOffsetStandard = (this.StandardZone != null && this.StandardZone.R2_OffsetMinutesFromUTC.IsValid) ? ((decimal)this.StandardZone.R2_OffsetMinutesFromUTC) / 60m : 0;
			decimal utcOffsetDst = (this.DaylightSavingZone != null && this.DaylightSavingZone.R2_OffsetMinutesFromUTC.IsValid) ? ((decimal)this.DaylightSavingZone.R2_OffsetMinutesFromUTC) / 60m : 0;

			return new CalculationTimeZone(this, zoneSetName, utcOffsetStandard, utcOffsetDst);
		}

		class CalculationTimeZone : TimeZoneBase
		{
			public CalculationTimeZone(RefTimeZoneSet timeZoneSet, string zoneName, decimal utcOffsetStandard, decimal utcOffsetDst)
				: base(zoneName, utcOffsetStandard, utcOffsetDst)
			{
				this.timeZoneSet = timeZoneSet;
			}

			public override string StandardName
			{
				get { return timeZoneSet.StandardZone.R2_CivilianTimeZoneFullName; }
			}

			protected override bool HasDaylightSaving
			{
				get
				{
					if (hasDaylightSaving == null)
					{
						hasDaylightSaving = (
							timeZoneSet.HasDaylightSavings
							&& timeZoneSet.DaylightSavingZone != null
							&& timeZoneSet.DaylightSavingZone.StartDateRules != null
							&& timeZoneSet.DaylightSavingZone.StartDateRules.Count > 0
							&& timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_DaylightSavingDate.IsValid);
					}

					return hasDaylightSaving.Value;
				}
			}

			bool? hasDaylightSaving;

			protected override RefTimeZoneRuleInfoStartAndEndPair GetStartAndEndDstRuleInfo(int year)
			{
				RefTimeZoneRuleInfo startDstInfo = null;
				RefTimeZoneRuleInfo endDstInfo = null;

				RefTimeZoneRule startRule = timeZoneSet.GetRuleForSpecifiedYear(year, timeZoneSet.DaylightSavingZone.StartDateRules);

				if (startRule != null)
				{
					startDstInfo = new RefTimeZoneRuleInfo(
						year, startRule.R4_StartOrEndRule,
						startRule.R4_DaylightSavingDayWeekDate, startRule.R4_TypeOfTime, startRule.R4_DaylightSavingDate.ToDateTime(),
						startRule.R4_DaylightSavingDayCount, startRule.R4_DaylightSavingDayName, startRule.R4_DaylightSavingMonth);
				}

				RefTimeZoneRule endRule = timeZoneSet.GetRuleForSpecifiedYear(year, timeZoneSet.DaylightSavingZone.EndDateRules);

				if (endRule != null)
				{
					endDstInfo = new RefTimeZoneRuleInfo(
						year, endRule.R4_StartOrEndRule,
						endRule.R4_DaylightSavingDayWeekDate, endRule.R4_TypeOfTime, endRule.R4_DaylightSavingDate.ToDateTime(),
						endRule.R4_DaylightSavingDayCount, endRule.R4_DaylightSavingDayName, endRule.R4_DaylightSavingMonth);
				}

				RefTimeZoneRuleInfoStartAndEndPair result = new RefTimeZoneRuleInfoStartAndEndPair(startDstInfo, endDstInfo);
				return result;
			}

			readonly RefTimeZoneSet timeZoneSet;
		}

		#endregion

		#region GetRuleForSpecifiedYear

		/// <summary>
		/// Gets the rule that applies for the input Year and in the input collection.
		/// </summary>
		/// <param name="year">The Year which the rule must cover</param>
		/// <param name="ruleCollection">The collection which the rule belongs to. I.e. Either StartDateRules or EndDateRules</param>
		/// <returns>A RefTimeZoneRule - either a start rule or end rule depending on which collection was input</returns>
		internal RefTimeZoneRule GetRuleForSpecifiedYear(int year, RefTimeZoneRuleCollection ruleCollection)
		{
			RefTimeZoneRule result = null;

			if (ruleCollection != null)
			{
				foreach (RefTimeZoneRule rule in ruleCollection)
				{
					if (year >= rule.R4_FromYear && (year <= rule.R4_ToYear || rule.R4_ToYear == 0))
					{
						result = rule;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region GetDaylightSavingStartOrEndDateInYear

		public ZDateTime GetDaylightSavingStartOrEndDateInYear(int year, RefTimeZoneRuleCollection startOrEndRuleCollection)
		{
			ZDateTime result = ZDateTime.Empty;
			RefTimeZoneRule rule = GetRuleForSpecifiedYear(year, startOrEndRuleCollection);

			if (rule != null)
			{
				result = rule.GetDaylightSavingLocalDateTimeInYear(
					year, ((decimal)StandardZone.R2_OffsetMinutesFromUTC) / 60m, ((decimal)DaylightSavingZone.R2_OffsetMinutesFromUTC) / 60m);
			}

			return result;
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList list = new ArrayList();
				list.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				list.Add(StandardZone);
				if (DaylightSavingZone != null)
				{
					list.Add(DaylightSavingZone);
				}

				return (BusinessObject[])list.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("27e08db7-5998-4499-b2ba-6ddf3720aa63", "Time Zone"); }
		}

		#endregion
	}
}
