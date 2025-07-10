using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneRuleLookups : AutoRefTimeZoneRuleLookups
	{
		public RefTimeZoneRuleLookups(AutoRefTimeZoneRule parent) : base(parent)
		{
		}

		#region Zone Type Codes

		public CodeDescriptionPairList ZoneTypes
		{
			get
			{
				if (fZoneTypes == null)
				{
					fZoneTypes = new CodeDescriptionPairList();
					fZoneTypes.AddPair(TimeZoneConstants.DstRuleDayOfMonth, Res.GetString("aaf4024f-8065-4fbe-97af-52ad54e7998a", "Based on Dates"));
					fZoneTypes.AddPair(TimeZoneConstants.DstRuleWeekday, Res.GetString("6ae8fabf-ab8b-4b1f-8b55-0717912b45a4", "Based on a certain week day"));
				}
				return fZoneTypes;
			}
		}

		CodeDescriptionPairList fZoneTypes;

		#endregion

		#region TypeOfTime Codes and List

		public CodeDescriptionPairList TypeOfTimeList
		{
			get
			{
				if (fTypesOfTime == null)
				{
					fTypesOfTime = new CodeDescriptionPairList();
					fTypesOfTime.AddPair(TimeZoneConstants.DstTimeBaseStandard, Res.GetString("cb509d9c-8f5e-4283-9897-2cd6eb3da452", "Based on the Standard Zone time"));
					fTypesOfTime.AddPair(TimeZoneConstants.DstTimeBaseLocal, Res.GetString("d41e3e24-3c02-482a-bf4a-a8b1eb60a1d0", "Based on the local time"));
					fTypesOfTime.AddPair(TimeZoneConstants.DstTimeBaseUtc, Res.GetString("45f583fc-b63b-4854-9aea-ac30a632d548", "Based on the UTC time"));
				}
				return fTypesOfTime;
			}
		}

		CodeDescriptionPairList fTypesOfTime;

		#endregion

		#region NthDayOfMonthList

		public CodeDescriptionPairList NthDayOfMonthList
		{
			get
			{
				if (fNthDayOfMonthList == null)
				{
					fNthDayOfMonthList = new CodeDescriptionPairList();
					fNthDayOfMonthList.AddPair("1", (NoResString)"1st");
					fNthDayOfMonthList.AddPair("2", (NoResString)"2nd");
					fNthDayOfMonthList.AddPair("3", (NoResString)"3rd");
					fNthDayOfMonthList.AddPair("4", (NoResString)"4th");
					fNthDayOfMonthList.AddPair("5", "LAST");
				}
				return fNthDayOfMonthList;
			}
		}

		CodeDescriptionPairList fNthDayOfMonthList;

		#endregion

		#region ZoneDays

		public CodeDescriptionPairList ZoneDays
		{
			get { return new CodeDescriptionPairList(new DayOfWeekCodeList()); }
		}

		#endregion

		#region ZoneMonths

		public CodeDescriptionPairList ZoneMonths
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Months); }
		}

		#endregion

		#region DaysOfMonthList

		public CodeDescriptionPairList DaysOfMonthList
		{
			get
			{
				if (fDaysOfMonthList == null)
				{
					fDaysOfMonthList = new CodeDescriptionPairList();
					for (int i = 1; i < 32; i++)
					{
						if (i == 1 || i == 21 || i == 31)
						{
							fDaysOfMonthList.AddPair(i.ToString(), ((ZString)(i + (NoResString)"st")));
						}
						else if (i == 2 || i == 22)
						{
							fDaysOfMonthList.AddPair(i.ToString(), (ZString)(i + (NoResString)"nd"));
						}
						else if (i == 3 || i == 23)
						{
							fDaysOfMonthList.AddPair(i.ToString(), (ZString)(i + (NoResString)"rd"));
						}
						else
						{
							fDaysOfMonthList.AddPair(i.ToString(), (ZString)(i + (NoResString)"th"));
						}
					}
				}
				return fDaysOfMonthList;
			}
		}
		CodeDescriptionPairList fDaysOfMonthList;

		#endregion
	}
}
