using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public enum MatchType
	{
		BookingReference,
		MasterBill,
		All
	}

	public sealed class MatchConsolResultService : IService
	{
		MatchConsolResultService(bool hasConsolsInYearRange, IEnumerable<ZString> consolNames, MatchType type)
		{
			HasConsolsInYearRange = hasConsolsInYearRange;
			ConsolNames = consolNames;
			MatchType = type;
		}

		public MatchType MatchType { get; }

		public bool HasConsolsInYearRange { get; }

		public IEnumerable<ZString> ConsolNames { get; }

		public static void Register(BusinessObjectFactory factory, IReadOnlyCollection<CommonConsol> consols, ZString carrierBookingRef, ZString masterBill)
		{
			UnRegister(factory);

			bool isBookingRefMatch = !carrierBookingRef.IsEmpty &&
				consols.All(c => (c.JK_AgentType != Constants.AgentType.CoLoad && c.JK_BookingReference == carrierBookingRef) || (c.JK_AgentType == Constants.AgentType.CoLoad && c.JK_CoLoadBookingReference == carrierBookingRef));
			bool isMasterBillMatch = !masterBill.IsEmpty &&
				consols.All(c => (c.JK_AgentType != Constants.AgentType.CoLoad && c.JK_MasterBillNum == masterBill) || (c.JK_AgentType == Constants.AgentType.CoLoad && c.JK_CoLoadMasterBill == masterBill));

			if (!isBookingRefMatch && !isMasterBillMatch)
			{
				return;
			}

			MatchType type = isBookingRefMatch
				? isMasterBillMatch
					? MatchType.All
					: MatchType.BookingReference
				: MatchType.MasterBill;

			var createDates = consols
				.Select(c => c.JK_SystemCreateTimeUtc)
				.Where(c => c.IsValid)
				.OrderByDescending(time => time)
				.ToArray();

			var hasConsolsInOneYearRange = createDates.Length >= 2 && createDates[0] - createDates[1] < TimeSpan.FromDays(365);

			var consolNames = hasConsolsInOneYearRange
				? consols.Select(c => c.HumanReadableShortcutName).OrderBy(c => c)
				: Enumerable.Empty<ZString>();

			var service = new MatchConsolResultService(hasConsolsInOneYearRange, consolNames, type);
			factory.ServiceContainer.AddService(service);
		}

		public static void UnRegister(BusinessObjectFactory factory)
		{
			factory.ServiceContainer.RemoveService<MatchConsolResultService>();
		}

		public static MatchConsolResultService GetInstance(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<MatchConsolResultService>();
		}
	}
}
