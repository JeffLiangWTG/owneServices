using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(RepititionSelection))]
	sealed class RepititionSelectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDates_Daily()
		{
			Selection.UseDailyPattern = true;
			Selection.FromDate = new ZDateTime(2010, 2, 5);
			Selection.ToDate = new ZDateTime(2010, 4, 6);
			Selection.RecurrenceInterval = 6;

			AssertContainsExactElementsInAnyOrder(
				new ZDateTime[]
				{
					new ZDateTime(2010, 2, 5, 11, 35, 00),
					new ZDateTime(2010, 2, 11, 11, 35, 00),
					new ZDateTime(2010, 2, 17, 11, 35, 00),
					new ZDateTime(2010, 2, 23, 11, 35, 00),
					new ZDateTime(2010, 3, 1, 11, 35, 00),
					new ZDateTime(2010, 3, 7, 11, 35, 00),
					new ZDateTime(2010, 3, 13, 11, 35, 00),
					new ZDateTime(2010, 3, 19, 11, 35, 00),
					new ZDateTime(2010, 3, 25, 11, 35, 00),
					new ZDateTime(2010, 3, 31, 11, 35, 00),
					new ZDateTime(2010, 4, 6, 11, 35, 00),
				},
				Selection.GetDateTimes(new ZDateTime(2010, 1, 30, 11, 35, 0)));

			AssertContainsExactElementsInAnyOrder(
				new ZDateTime[]
				{
					new ZDateTime(2010, 3, 7, 11, 35, 00),
					new ZDateTime(2010, 3, 13, 11, 35, 00),
					new ZDateTime(2010, 3, 19, 11, 35, 00),
					new ZDateTime(2010, 3, 25, 11, 35, 00),
					new ZDateTime(2010, 3, 31, 11, 35, 00),
					new ZDateTime(2010, 4, 6, 11, 35, 00),
				},
				Selection.GetDateTimes(new ZDateTime(2010, 3, 7, 11, 35, 0)));
		}

		public void TestDates_Weekly()
		{
			Selection.UseWeeklyPattern = true;
			Selection.FromDate = new ZDateTime(2010, 2, 5);
			Selection.ToDate = new ZDateTime(2010, 3, 1);
			Selection.RecurrenceInterval = 2;
			Selection.DayOfTheWeek["Monday"].Value = true;
			Selection.DayOfTheWeek["Wednesday"].Value = true;
			Selection.DayOfTheWeek["Friday"].Value = true;

			AssertContainsExactElementsInAnyOrder(
				new ZDateTime[]
				{
					new ZDateTime(2010, 2, 5, 15, 40, 00),

					//new ZDateTime(2010, 2, 8, 15, 40, 00),
					//new ZDateTime(2010, 2, 10, 15, 40, 00),
					//new ZDateTime(2010, 2, 12, 15, 40, 00),

					new ZDateTime(2010, 2, 15, 15, 40, 00),
					new ZDateTime(2010, 2, 17, 15, 40, 00),
					new ZDateTime(2010, 2, 19, 15, 40, 00),

					//new ZDateTime(2010, 2, 22, 15, 40, 00),
					//new ZDateTime(2010, 2, 24, 15, 40, 00),
					//new ZDateTime(2010, 2, 28, 15, 40, 00),

					new ZDateTime(2010, 3, 1, 15, 40, 00),
				},
				Selection.GetDateTimes(new ZDateTime(2010, 1, 1, 15, 40, 00)));
		}

		public void TestDates_Weekly_LanguageChange()
		{
			using (IMockResourceStringCache grmMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData())
			{
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
				{
					grmMockData.Put("AutoDayOfWeekCodeList|Sunday", new ResourceStringData("AutoDayOfWeekCodeList|Sunday", "Dimanche"));
					grmMockData.Put("AutoDayOfWeekCodeList|Monday", new ResourceStringData("AutoDayOfWeekCodeList|Monday", "Lundi"));
					grmMockData.Put("AutoDayOfWeekCodeList|Tuesday", new ResourceStringData("AutoDayOfWeekCodeList|Tuesday", "Mardi"));
					grmMockData.Put("AutoDayOfWeekCodeList|Wednesday", new ResourceStringData("AutoDayOfWeekCodeList|Wednesday", "Mercredi"));
					grmMockData.Put("AutoDayOfWeekCodeList|Thursday", new ResourceStringData("AutoDayOfWeekCodeList|Thursday", "Jeudi"));
					grmMockData.Put("AutoDayOfWeekCodeList|Friday", new ResourceStringData("AutoDayOfWeekCodeList|Friday", "Vendredi"));
					grmMockData.Put("AutoDayOfWeekCodeList|Saturday", new ResourceStringData("AutoDayOfWeekCodeList|Saturday", "Samedi"));

					Selection.UseWeeklyPattern = true;
					Selection.FromDate = new ZDateTime(2010, 2, 5);
					Selection.ToDate = new ZDateTime(2010, 3, 1);
					Selection.RecurrenceInterval = 2;

					Selection.DayOfTheWeek["Lundi"].Value = true;
					Selection.DayOfTheWeek["Mercredi"].Value = true;
					Selection.DayOfTheWeek["Vendredi"].Value = true;

					AssertEquals(" Monday is true!", true, Selection.DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Monday].Value);
					AssertEquals(" Wednesday is true!", true, Selection.DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Wednesday].Value);
					AssertEquals(" Friday is true!", true, Selection.DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Friday].Value);

					AssertContainsExactElementsInAnyOrder(
						new ZDateTime[]
						{
							new ZDateTime(2010, 2, 5, 15, 40, 00),
							new ZDateTime(2010, 2, 15, 15, 40, 00),
							new ZDateTime(2010, 2, 17, 15, 40, 00),
							new ZDateTime(2010, 2, 19, 15, 40, 00),
							new ZDateTime(2010, 3, 1, 15, 40, 00)
						},
						Selection.GetDateTimes(new ZDateTime(2010, 1, 1, 15, 40, 00)));
				}
			}
		}

		public void TestDates_Monthly()
		{
			Selection.UseMonthlyPattern = true;
			Selection.FromDate = new ZDateTime(2010, 1, 1);
			Selection.ToDate = new ZDateTime(2010, 5, 6);
			Selection.RecurrenceInterval = 2;
			Selection.DayOfTheMonth = 4;

			AssertContainsExactElementsInAnyOrder(
				new ZDateTime[]
				{
					new ZDateTime(2010, 1, 4, 10, 30, 00),
					//new ZDateTime(2010, 2, 4, 10, 30, 00),
					new ZDateTime(2010, 3, 4, 10, 30, 00),
					//new ZDateTime(2010, 4, 4, 10, 30, 00),
					new ZDateTime(2010, 5, 4, 10, 30, 00),
				},
				Selection.GetDateTimes(new ZDateTime(2010, 1, 1, 10, 30, 00)));

			AssertContainsExactElementsInAnyOrder(
				new ZDateTime[]
				{
					new ZDateTime(2010, 2, 4, 10, 30, 00),
					//new ZDateTime(2010, 3, 4, 10, 30, 00),
					new ZDateTime(2010, 4, 4, 10, 30, 00),
					//new ZDateTime(2010, 5, 4, 10, 30, 00),
				},
				Selection.GetDateTimes(new ZDateTime(2010, 2, 1, 10, 30, 00)));
		}

		public void TestDates_Monthly_PastEndOfMonth()
		{
			Selection.UseMonthlyPattern = true;
			Selection.FromDate = new ZDateTime(2010, 1, 1);
			Selection.ToDate = new ZDateTime(2010, 5, 6);
			Selection.RecurrenceInterval = 1;
			Selection.DayOfTheMonth = 31;

			AssertContainsExactElementsInAnyOrder(
				new ZDateTime[]
				{
					new ZDateTime(2010, 1, 31, 19, 01, 00),
					//new ZDateTime(2010, 2, 31, 19, 01, 00),
					new ZDateTime(2010, 3, 31, 19, 01, 00),
					//new ZDateTime(2010, 4, 31, 19, 01, 00),
				},
				Selection.GetDateTimes(new ZDateTime(2010, 1, 1, 19, 01, 00)));
		}

		#region Implementation

		RepititionSelection Selection
		{
			get { return selection ?? (selection = new RepititionSelection()); }
		}
		RepititionSelection selection;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RepititionSelection();
		}

		#endregion
	}
}
