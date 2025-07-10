using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefTimeZoneSetVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var refTimeZoneSet = new RefTimeZoneSet
			{
				R3_PK = Guid.NewGuid(),
				R3_IsActive = true,
				R3_TimeZoneSetName = "TM"
			};

			var refTimeZoneDaylight = new RefTimeZone
			{
				R2_PK = Guid.NewGuid(),
				R2_CivilianTimeZoneCode = "AZST",
				R2_OffsetMinutesFromUTC = 5,
				R2_CivilianTimeZoneFullName = "TST",
				R2_MilitaryTimeZoneCode = "AA",
				R2_Type = "DLS",
				R2_R3_TimeZoneSet = refTimeZoneSet.R3_PK
			};

			var refTimeZoneStandard = new RefTimeZone
			{
				R2_PK = Guid.NewGuid(),
				R2_CivilianTimeZoneCode = "AMST",
				R2_OffsetMinutesFromUTC = 5,
				R2_CivilianTimeZoneFullName = "AMT",
				R2_MilitaryTimeZoneCode = "BB",
				R2_Type = "STD",
				R2_R3_TimeZoneSet = refTimeZoneSet.R3_PK
			};

			result.Add(refTimeZoneSet);
			result.Add(refTimeZoneDaylight);
			result.Add(refTimeZoneStandard);

			result.Add(new RefTimeZoneRule
			{
				R4_PK = Guid.NewGuid(),
				R4_FromYear = 2000,
				R4_ToYear = 0,
				R4_StartOrEndRule = "END",
				R4_DaylightSavingDayWeekDate = "MON",
				R4_DaylightSavingDate = DateTime.Now.AddDays(-1),
				R4_DaylightSavingDayCount = 5,
				R4_DaylightSavingDayName = "SUN",
				R4_DaylightSavingMonth = "OCT",
				R4_TypeOfTime = "LOC",
				R4_R2 = refTimeZoneDaylight.R2_PK,
				R4_DataSetPK = refTimeZoneSet.R3_PK,
				R4_DataSetCode = "R3"
			});

			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefTimeZoneRule zoneRule)
			{
				zoneRule.R4_StartOrEndRule = "STA";
				zoneRule.R4_FromYear = 2001;
			}
			if (data is RefTimeZoneSet timeZoneSet)
			{
				timeZoneSet.R3_TimeZoneSetName = "MT";
			}
			if (data is RefTimeZone timeZone)
			{
				timeZone.R2_OffsetMinutesFromUTC = 6;
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefTimeZoneSet
			{
				R3_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				R3_IsActive = true,
				R3_TimeZoneSetName = "BB"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefTimeZone zone)
			{
				zone.R2_R3_TimeZoneSet = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		[Test]
		public override void UpdateVersionOnInsert()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var dataSet = PrepareData();
				foreach (var data in dataSet)
				{
					context.Add(data);
					Thread.Sleep(100);
					context.SaveChanges();
					var versionControl = context.RefDbVersionControls.FirstOrDefault();
					if (versionControl == null && data.GetType() == typeof(RefTimeZoneSet))
					{
						continue;
					}
					Assert.IsFalse(versionControl.RVC_IsPublished);
				}
			}
		}

		[Test]
		public override void UpdateVersionOnDelete()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			object[] dataSet = null;
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				dataSet = PrepareData();
				foreach (var data in dataSet)
				{
					context.Add(data);
					context.SaveChanges();
				}
			}
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				foreach (var data in dataSet.Skip(1).Reverse())
				{
					DeleteEntity(context, data);
					var versionControl = context.RefDbVersionControls.FirstOrDefault();
					Assert.IsFalse(versionControl.RVC_IsPublished);
				}
			}
		}

		void DeleteEntity(SafeDbContext context, object data)
		{
			context.Attach(data);
			context.Remove(data);
			Thread.Sleep(100);
			context.SaveChanges();
		}
	}
}
