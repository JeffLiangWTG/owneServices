using System;
using System.Collections.Generic;
using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

#if DEBUG
using Enterprise.ZArchitecture.Core.Testing;
#endif

namespace Enterprise.MasterFiles.Business
{
	public static class WorkingDays
	{
		public static IWorkTimeArithmetic GetInstance(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default)
		{
			return GetCacheFromFactory(factory).Get(factory, departmentPK, branchPK, staffPK);
		}

		public static IWorkTimeArithmetic GetInstance<T>(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default, Func<T> workingDaysCreator = null)
			where T : WorkTimeArithmetic
		{
			return GetCacheFromFactory(factory).Get(factory, departmentPK, branchPK, staffPK, workingDaysCreator);
		}

		static CalendarArithmeticCache GetCacheFromFactory(BusinessObjectFactory factory) => factory.GetCachedValue<CalendarArithmeticCache>(CacheStalenessPolicy.StaleOnFactorySave);
	}

	class CalendarArithmeticCache : IService
	{
		internal IWorkTimeArithmetic Get(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK, Func<IWorkTimeArithmetic> workingDaysCreator = null)
		{
			var key = Tuple.Create(departmentPK, branchPK, staffPK);
			return cache.TryGetValue(key, out var value) ? value : cache[key] = CreateWorkingDays(factory, departmentPK, branchPK, staffPK, workingDaysCreator);
		}

		readonly Dictionary<Tuple<ZGuid, ZGuid, ZGuid>, IWorkTimeArithmetic> cache = new Dictionary<Tuple<ZGuid, ZGuid, ZGuid>, IWorkTimeArithmetic>();

		static IWorkTimeArithmetic CreateWorkingDays(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK, Func<IWorkTimeArithmetic> workingDaysCreator)
		{
#if DEBUG
			DateSetInRelevantTestListener.Instance.SetWorkingDaysCalled();
#endif
			if (workingDaysCreator != null)
			{
				return workingDaysCreator();
			}

			ICalendarDataSource dataSource = new CalendarArithmeticDataSource(factory, departmentPK, branchPK, staffPK);
			dataSource = new CachedCalendarDataSource(dataSource);
			return new WorkTimeArithmetic(dataSource);
		}
	}

	public class WorkingDaysProvider : IWorkingDaysProvider
	{
		public IWorkTimeArithmetic GetWorkingDays(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default)
		{
			return WorkingDays.GetInstance(factory, departmentPK, branchPK, staffPK);
		}
	}
}
