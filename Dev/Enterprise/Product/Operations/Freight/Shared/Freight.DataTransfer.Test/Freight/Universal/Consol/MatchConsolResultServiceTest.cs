using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class MatchConsolResultServiceTest : TestCaseWithFactory
	{
		[TestDate(2018, 6, 6)]
		public void TestRegister()
		{
			var consols = new List<CommonConsol>()
			{
				CreateConsol("234", ZString.Empty, 3),
				CreateConsol("234", ZString.Empty, 2),
				CreateConsol("234", ZString.Empty, 1)
			};

			MatchConsolResultService.Register(Factory, consols, "234", ZString.Empty);
			var service = MatchConsolResultService.GetInstance(Factory);
			AssertNotNull(service);

			AssertEquals(MatchType.BookingReference, service.MatchType);
			AssertEquals(true, service.HasConsolsInYearRange);
		}

		[TestDate(2018, 6, 6)]
		public void TestRegister_NoCBRandMBLGiven()
		{
			var consols = new List<CommonConsol>
			{
				CreateConsol("CBR", "MBL", 1),
				CreateConsol("CBR", "MBL", 2)
			};

			MatchConsolResultService.Register(Factory, consols, ZString.Empty, ZString.Empty);
			AssertNull(MatchConsolResultService.GetInstance(Factory));
		}

		[TestDate(2018, 6, 6)]
		public void TestRegister_OnlyMBL_NotInOneYearRange()
		{
			var consols = new List<CommonConsol>()
			{
				CreateConsol(ZString.Empty, "MBL", 1),
				CreateConsol(ZString.Empty, "MBL", -500)
			};

			MatchConsolResultService.Register(Factory, consols, ZString.Empty, "MBL");
			var service = MatchConsolResultService.GetInstance(Factory);
			AssertNotNull(service);

			AssertEquals(MatchType.MasterBill, service.MatchType);
			AssertEquals(false, service.HasConsolsInYearRange);
		}

		[TestDate(2018, 6, 6)]
		public void TestRegister_OnlyMBL_InOneYearRange()
		{
			var consols = new List<CommonConsol>()
			{
				CreateConsol(ZString.Empty, "MBL", 1),
				CreateConsol(ZString.Empty, "MBL", 2)
			};

			MatchConsolResultService.Register(Factory, consols, ZString.Empty, "MBL");
			var service = MatchConsolResultService.GetInstance(Factory);
			AssertNotNull(service);

			AssertEquals(MatchType.MasterBill, service.MatchType);
			AssertEquals(true, service.HasConsolsInYearRange);
		}

		[TestDate(2018, 6, 6)]
		public void TestRegister_OnlyCBR_NotInOneYearRange()
		{
			var consols = new List<CommonConsol>()
			{
				CreateConsol("CBR", ZString.Empty, 1),
				CreateConsol("CBR", ZString.Empty, -500)
			};

			MatchConsolResultService.Register(Factory, consols, "CBR", ZString.Empty);
			var service = MatchConsolResultService.GetInstance(Factory);
			AssertNotNull(service);

			AssertEquals(MatchType.BookingReference, service.MatchType);
			AssertEquals(false, service.HasConsolsInYearRange);
		}

		[TestDate(2018, 6, 6)]
		public void TestRegister_OnlyCBR_InOneYearRange()
		{
			var consols = new List<CommonConsol>()
			{
				CreateConsol("CBR", ZString.Empty, 1),
				CreateConsol("CBR", ZString.Empty, 2)
			};

			MatchConsolResultService.Register(Factory, consols, "CBR", ZString.Empty);
			var service = MatchConsolResultService.GetInstance(Factory);
			AssertNotNull(service);

			AssertEquals(MatchType.BookingReference, service.MatchType);
			AssertEquals(true, service.HasConsolsInYearRange);
		}

		[TestDate(2018, 6, 6)]
		public void TestRegister_CBRandMBL_NotInOneYearRange()
		{
			var consols = new List<CommonConsol>()
			{
				CreateConsol("CBR", "MBL", 1),
				CreateConsol("CBR", "MBL", -500)
			};

			MatchConsolResultService.Register(Factory, consols, "CBR", "MBL");
			var service = MatchConsolResultService.GetInstance(Factory);
			AssertNotNull(service);

			AssertEquals(MatchType.All, service.MatchType);
			AssertEquals(false, service.HasConsolsInYearRange);
		}

		[TestDate(2018, 6, 6)]
		public void TestRegister_CBRandMBL_InOneYearRange()
		{
			var consols = new List<CommonConsol>()
			{
				CreateConsol("CBR", "MBL", 1),
				CreateConsol("CBR", "MBL", 2)
			};

			MatchConsolResultService.Register(Factory, consols, "CBR", "MBL");
			var service = MatchConsolResultService.GetInstance(Factory);
			AssertNotNull(service);

			AssertEquals(MatchType.All, service.MatchType);
			AssertEquals(true, service.HasConsolsInYearRange);
		}

		CommonConsol CreateConsol(ZString cbr, ZString mbl, int dateOffset)
		{
			var now = ZDateTime.Today;

			var consol = Factory.New<CommonConsol>();
			consol.JK_BookingReference = cbr;
			consol.JK_MasterBillNum = mbl;
			consol.JK_SystemCreateTimeUtc = now.AddDays(dateOffset);

			return consol;
		}
	}
}
