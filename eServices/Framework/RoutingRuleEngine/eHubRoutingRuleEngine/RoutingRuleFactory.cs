using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;

namespace eServices.eHubRoutingRuleEngine
{
	public class RoutingRuleFactory : IRoutingRuleFactory
	{
		private readonly eHubTransactionsContext context;
		private readonly ILog logger;
		internal TimeSpan DefaultRuleCheckInterval = TimeSpan.FromSeconds(60);
		private readonly TimeSpan DefaultNoRuleCheckInterval = TimeSpan.FromMinutes(5);
		internal static readonly ConcurrentDictionary<string, (DateTime Expiry, IRule Rule)> ruleCache = new ConcurrentDictionary<string, (DateTime Expiry, IRule Rule)>();
		internal static readonly ConcurrentDictionary<string, SemaphoreSlim> ruleLock = new ConcurrentDictionary<string, SemaphoreSlim>();

		public RoutingRuleFactory(eHubTransactionsContext context, ILog logger)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			this.logger = logger;
		}

		public IRule GetForReading(eHubClient client) => GetForReading(client?.CC_ID);

		public IRule GetForReading(eHubClient client, TimeSpan ruleCheckInterval, TimeSpan noRuleCheckInterval)
			=> GetForReading(client?.CC_ID, ruleCheckInterval, noRuleCheckInterval);

		public IRule GetForReading(string ruleId) => GetForReading(ruleId, DefaultRuleCheckInterval, DefaultNoRuleCheckInterval);

		public IRule GetForReading(string ruleId, TimeSpan ruleCheckInterval, TimeSpan noRuleCheckInterval)
		{
			if (string.IsNullOrWhiteSpace(ruleId)) throw new ArgumentNullException(nameof(ruleId));

			try
			{
				logger.TraceFormat("Checking cache for ID '{0}'", ruleId);
				if (ruleCache.TryGetValue(ruleId, out (DateTime Expiry, IRule Rule) result) && result.Expiry > DateTime.UtcNow)
				{
					logger.TraceFormat("Using cache entry for ID '{0}'", ruleId);
				}
				else
				{
					var semaphore = ruleLock.GetOrAdd(ruleId, new SemaphoreSlim(1));
					try
					{
						logger.TraceFormat("Updating cache for ID '{0}'", ruleId);
						semaphore.Wait();
						if (ruleCache.TryGetValue(ruleId, out var current) && current.Expiry > DateTime.UtcNow)
						{
							logger.TraceFormat("Cache entry for ID '{0}' updated on another thread", ruleId);
							result = current;
						}
						else
						{
							(DateTime Expiry, IRule Rule) newCache;
							logger.TraceFormat("Checking database for ID '{0}'", ruleId);
							var client = context.eHubClients.FirstOrDefault(c => c.CC_ID == ruleId);
							if (client?.CC_RR == null)
							{
								logger.TraceFormat("Caching ID '{0}' as not a rule", ruleId);
								newCache = (DateTime.UtcNow.Add(noRuleCheckInterval), null);
								result = ruleCache.AddOrUpdate(ruleId, newCache, (id, oldCache) => newCache);
							}
							else
							{
								var lastUpdateUTC = context.SqlQuery<DateTime>("SELECT RR_LastUpdateUTC FROM eHubRoutingRule WITH (READPAST) WHERE RR_PK = @pk;", new SqlParameter("@pk", client.CC_RR)).FirstOrDefault();
								if (lastUpdateUTC == default(DateTime))
								{
									logger.InfoFormat("Routing Rule for ID '{0}' is locked. Using cached rule.", ruleId);
									result = current;
								}
								else if (current.Rule?.Timestamp == lastUpdateUTC)
								{
									logger.TraceFormat("Database timestamp matches cached rule for ID '{0}'. Keeping cached rule.", ruleId);
									newCache = (DateTime.UtcNow.Add(ruleCheckInterval), current.Rule);
									result = ruleCache.AddOrUpdate(ruleId, newCache, (id, oldCache) => newCache);
								}
								else
								{
									logger.TraceFormat("Loading Routing Rule for ID '{0}'", ruleId);
									var rule = Rule.GetForReading(client);
									logger.InfoFormat("Loaded Routing Rule for ID '{0}' with timestamp {1}", ruleId, rule.Timestamp);
									newCache = (DateTime.UtcNow.Add(ruleCheckInterval), rule);
									result = ruleCache.AddOrUpdate(ruleId, newCache, (id, oldCache) => newCache);
								}
							}
						}
					}
					finally
					{
						semaphore.Release();
					}
				}

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0}.{1} result: {{ ID={2}, IsRule={3}, CacheExpiry={4} }}",
						nameof(RoutingRuleFactory), nameof(GetForReading),
						ruleId,
						result.Rule == null ? "False" : $"True, Timestamp={result.Rule?.Timestamp}",
						result.Expiry);
				}

				return result.Rule;
			}
			catch (Exception ex)
			{
				logger.ErrorFormat("Exception getting Rule for ID '{0}'.", ex, ruleId);
				throw;
			}
		}

		public IRule GetForEditing(string ruleId) =>
			GetForEditing(context.eHubClients.SingleOrDefault(eHubClient => eHubClient.CC_ID == ruleId));

		public IRule GetForEditing(eHubClient client) => Rule.GetForEditing(context, client);
	}
}
