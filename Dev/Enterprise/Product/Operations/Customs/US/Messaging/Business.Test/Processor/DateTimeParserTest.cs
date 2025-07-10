using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class DateTimeParserTest : NUnit.Framework.TestCase
	{
		public void TestGetDateTimeFromZDateAndStringTime()
		{
			AssertEquals(new ZDateTime(1998, 1, 2, 1, 23, 0), DateTimeParser.GetDateTimeFromZDateAndStringTime(new ZDate(1998, 1, 2), "123"));
			AssertEquals(new ZDateTime(2008, 12, 2, 13, 3, 0), DateTimeParser.GetDateTimeFromZDateAndStringTime(new ZDate(2008, 12, 2), "1303"));
			AssertEquals(new ZDateTime(2008, 09, 11, 13, 13, 13), DateTimeParser.GetDateTimeFromZDateAndStringTime(new ZDate(2008, 09, 11), "131313"));
			AssertEquals(new ZDateTime(2008, 09, 11, 3, 13, 13), DateTimeParser.GetDateTimeFromZDateAndStringTime(new ZDate(2008, 09, 11), "31313"));
			AssertEquals(ZDateTime.Empty, DateTimeParser.GetDateTimeFromZDateAndStringTime(ZDate.Empty, "31313"));
		}

		public void TestGetFromJobBranchCurrentTime()
		{
			var factoryObj = new BusinessObjectFactory();
			var company1 = factoryObj.New<GlbCompany>();
			company1.GC_Code = "AAA";

			var branch1 = factoryObj.New<GlbBranch>();
			var branch2 = factoryObj.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "USPHL";
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "USLAX";
			branch2.GB_GC = company1.PK;

			var department = factoryObj.New<GlbDepartment>();
			department.GE_Code = "TTT";

			var objOrg = factoryObj.New<OrgHeader>();
			var logs = objOrg.GetLogs();

			var utcTimeNow = DateTime.UtcNow;
			var centralimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			var centralTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow, centralimeInfo);
			var pacificTimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
			var pacificTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow, pacificTimeInfo);

			AssertEquals("Get Time under Branch1", centralTimeZone.TimeOfDay.Hours, DateTimeParser.GetFromJobBranchCurrentTime(branch1).TimeOfDay.Hours);
			AssertEquals("Branch2 Time Zone", pacificTimeZone.TimeOfDay.Hours, DateTimeParser.GetFromJobBranchCurrentTime(branch2).TimeOfDay.Hours);
		}
	}
}
