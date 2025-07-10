using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestDate(2000, 1, 1)]
	sealed class DateCalculatorTest : TestCaseWithFactory
	{
		public void TestStorageDateForGeneralCargo()
		{
			Assert("Pre-condition: Expected registry to use Client Free Days by default for air", CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.Value);
			Assert("Pre-condition: Expected registry to use Client Free Days by default for sea", CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.Value);
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 3);
			EnvProxy.Instance.Registry.RawRegistry.CFSAirFreightLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 4);

			DateTime testDate = new DateTime(2012, 01, 01);

			AssertEquals(
				"Expected to calculate date using override days as client free days for Sea Cargo",
				testDate.AddDays(2),
				dateCalculator.CalculateDate(testDate, DatesToCalculate.Storage, Constants.TransportModes.Sea, false, 2)
			);

			AssertEquals(
				"Expected to calculate date using override days as client free days for Air Cargo",
				testDate.AddDays(2),
				dateCalculator.CalculateDate(testDate, DatesToCalculate.Storage, Constants.TransportModes.Air, false, 2)
			);

			AssertEquals(
				"Expected to use registry as client free days are 0 for Sea Cargo",
				testDate.AddDays(3),
				dateCalculator.CalculateDate(testDate, DatesToCalculate.Storage, Constants.TransportModes.Sea, false, 0)
			);

			AssertEquals(
				"Expected to use registry as client free days are 0 for Air Cargo",
				testDate.AddDays(4),
				dateCalculator.CalculateDate(testDate, DatesToCalculate.Storage, Constants.TransportModes.Air, false, 0)
			);

			CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			AssertEquals(
				"Expected to calculate free days from the registry setting and ignore override for Sea Cargo",
				testDate.AddDays(3),
				dateCalculator.CalculateDate(testDate, DatesToCalculate.Storage, Constants.TransportModes.Sea, false, 2)
			);

			AssertEquals(
				"Expected to calculate free days from the registry setting and ignore override for Air Cargo",
				testDate.AddDays(4),
				dateCalculator.CalculateDate(testDate, DatesToCalculate.Storage, Constants.TransportModes.Air, false, 2)
			);
		}

		public void TestStorageDateForDanergousGoods()
		{
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 0);
			EnvProxy.Instance.Registry.RawRegistry.CFSAirFreightLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 0);
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightDGLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 3);
			EnvProxy.Instance.Registry.RawRegistry.CFSAirFreightDGLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 4);

			AssertEquals(new DateTime(2002, 12, 31), dateCalculator.CalculateDate(new DateTime(2002, 12, 26), DatesToCalculate.Storage, Constants.TransportModes.Sea, true));
			AssertEquals(new DateTime(2003, 1, 2), dateCalculator.CalculateDate(new DateTime(2002, 12, 26), DatesToCalculate.Storage, Constants.TransportModes.Air, true));
		}

		public void TestAvailableDateForGeneralCargo()
		{
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLAvailableCommences.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, nameof(AvailableCommence.IME));
			EnvProxy.Instance.Registry.RawRegistry.CFSAirFreightLCLAvailableCommences.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, nameof(AvailableCommence.NBD));
			AssertEquals(new DateTime(2006, 1, 6), dateCalculator.CalculateDate(new DateTime(2006, 1, 6), DatesToCalculate.Available, Constants.TransportModes.Sea, false));
			AssertEquals(new DateTime(2006, 1, 9), dateCalculator.CalculateDate(new DateTime(2006, 1, 6), DatesToCalculate.Available, Constants.TransportModes.Air, false));
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLAvailableCommences.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, nameof(AvailableCommence.NCD));
			AssertEquals(new DateTime(2006, 1, 7), dateCalculator.CalculateDate(new DateTime(2006, 1, 6), DatesToCalculate.Available, Constants.TransportModes.Sea, false));
		}

		public void TestAvailableDateForDangerousGoods()
		{
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightDGLCLAvailableCommences.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, nameof(AvailableCommence.IME));
			EnvProxy.Instance.Registry.RawRegistry.CFSAirFreightDGLCLAvailableCommences.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, nameof(AvailableCommence.NBD));
			AssertEquals(new DateTime(2006, 1, 6), dateCalculator.CalculateDate(new DateTime(2006, 1, 6), DatesToCalculate.Available, Constants.TransportModes.Sea, true));
			AssertEquals(new DateTime(2006, 1, 9), dateCalculator.CalculateDate(new DateTime(2006, 1, 6), DatesToCalculate.Available, Constants.TransportModes.Air, true));
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightDGLCLAvailableCommences.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, nameof(AvailableCommence.NCD));
			AssertEquals(new DateTime(2006, 1, 7), dateCalculator.CalculateDate(new DateTime(2006, 1, 6), DatesToCalculate.Available, Constants.TransportModes.Sea, true));
		}

		public void TestNextBusinessDayDate()
		{
			AssertEquals(new DateTime(2005, 10, 24), dateCalculator.CalculateDate(new DateTime(2005, 10, 21), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2005, 10, 24), dateCalculator.CalculateDate(new DateTime(2005, 10, 22), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2005, 10, 24), dateCalculator.CalculateDate(new DateTime(2005, 10, 23), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2005, 10, 25), dateCalculator.CalculateDate(new DateTime(2005, 10, 24), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2005, 10, 26), dateCalculator.CalculateDate(new DateTime(2005, 10, 25), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2005, 10, 27), dateCalculator.CalculateDate(new DateTime(2005, 10, 26), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2005, 10, 28), dateCalculator.CalculateDate(new DateTime(2005, 10, 27), DatesToCalculate.NextBusinessDay));
		}

		public void TestNextWorkingDayWithOverride()
		{
			DateTime holidayDate = new DateTime(2006, 1, 1);
			dateCalculator = new DateCalculator(Factory, ZGuid.Empty, branchPK);
			AssertEquals(new DateTime(2006, 1, 2), dateCalculator.CalculateDate(new DateTime(2006, 1, 1), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2006, 1, 27), dateCalculator.CalculateDate(new DateTime(2006, 1, 1), DatesToCalculate.Storage, Constants.TransportModes.Air, false, 20));
			AssertEquals(new DateTime(2006, 1, 2), dateCalculator.CalculateDate(new DateTime(2006, 1, 1), DatesToCalculate.NextBusinessDay, Constants.TransportModes.Air, false, 20));
		}

		public void TestNextWorkingDay_NoDepartmentSpecified()
		{
			DateTime holidayDate = new DateTime(2006, 1, 1);
			dateCalculator = new DateCalculator(Factory, ZGuid.Empty, branchPK);
			AssertEquals(new DateTime(2006, 1, 2), dateCalculator.CalculateDate(new DateTime(2006, 1, 1), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2006, 1, 2), dateCalculator.CalculateDate(new DateTime(2005, 12, 30), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(2004, 1, 2), dateCalculator.CalculateDate(new DateTime(2004, 1, 1), DatesToCalculate.NextBusinessDay));
			AssertEquals(new DateTime(1999, 1, 4), dateCalculator.CalculateDate(new DateTime(1999, 1, 1), DatesToCalculate.NextBusinessDay));
		}

		protected override void SetUp()
		{
			base.SetUp();
			deptPK = EnvProxy.Instance.CurrentDepartment.PK;
			branchPK = EnvProxy.Instance.CurrentBranch.PK;
			staffPK = EnvProxy.Instance.CurrentUser.PK;
			dateCalculator = new DateCalculator(Factory, deptPK, branchPK, staffPK);

			WorkingDaysTestHelper.SetDefaultDepartmentWorkTimeWeek(Factory, deptPK);
			WorkingDaysTestHelper.SetBranchHolidays(Factory, branchPK);
		}

		DateCalculator dateCalculator;
		ZGuid deptPK;
		ZGuid staffPK;
		ZGuid branchPK;
	}
}
