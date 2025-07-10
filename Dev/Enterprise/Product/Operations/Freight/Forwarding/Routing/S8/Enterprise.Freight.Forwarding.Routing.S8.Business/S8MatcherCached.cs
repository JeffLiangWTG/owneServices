using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class S8MatcherCached : IS8Matcher
	{
		IS8Matcher Matcher { get; }
		ICacheProvider CacheProvider { get; }

		public IServiceRequestManager ServiceRequestManager
		{
			get => Matcher.ServiceRequestManager;
			set => Matcher.ServiceRequestManager = value;
		}

		public S8MatcherCached(ICacheProvider cacheProvider) : this(new S8Matcher(), cacheProvider) { }

		public S8MatcherCached(IS8Matcher matcher, ICacheProvider cacheProvider)
		{
			Matcher = Argument.NotNull(matcher, nameof(matcher));
			CacheProvider = Argument.NotNull(cacheProvider, nameof(cacheProvider));
		}

		public IS8MatchResult Match(ScheduleInfo scheduleToMatch)
		{
			var dict = CacheProvider.GetCachedValue("S8MatcherCached", () => new Dictionary<ScheduleInfo, IS8MatchResult>());
			if (dict.TryGetValue(scheduleToMatch, out IS8MatchResult res))
			{
				return res;
			}
			dict[scheduleToMatch] = Matcher.Match(scheduleToMatch);
			return dict[scheduleToMatch];
		}
	}
}
