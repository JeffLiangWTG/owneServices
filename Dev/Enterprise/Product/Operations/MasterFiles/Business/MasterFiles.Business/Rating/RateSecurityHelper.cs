using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Class is inherited and cannot be static")]
	public class RateSecurityHelper
	{
		public static SecurityCheckResult GetFirstDeniedSecurityCheckPoint(IEnumerable<ZGuid> orgHeaderPKs, BusinessObjectFactory factory)
		{
			if (TryGetFirstDeniedSecurityCheckPointFromCache(orgHeaderPKs, factory, out var result))
			{
				return result;
			}
			else
			{
				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, orgHeaderPKs.Where(x => x.IsValid));

				return GetFirstDeniedSecurityCheckPoint(orgHeaderSubQuery, factory);
			}
		}

		static bool TryGetFirstDeniedSecurityCheckPointFromCache(IEnumerable<ZGuid> orgHeaderPKs, BusinessObjectFactory factory, out SecurityCheckResult securityCheckResult)
		{
			var (cacheCheckResult, _) = GetSecurityCheckpointCache(factory);
			if (tryGetFirstDeniedSecurityInCache(out var firstDeniedSecurityInCache))
			{
				securityCheckResult = firstDeniedSecurityInCache;
				return true;
			}
			else if (checkAllOrgPkIsCacheAsNonDenied())
			{
				securityCheckResult = null;
				return true;
			}
			else
			{
				securityCheckResult = null;
				return false;
			}

			bool tryGetFirstDeniedSecurityInCache(out SecurityCheckResult firstDeniedSecurity)
			{
				var deniedOrgsInCache = cacheCheckResult
					.Where(kv => kv.Value != null)
					.Select(kv => kv.Key)
					.ToArray();
				var deniedOrgs = orgHeaderPKs.Intersect(deniedOrgsInCache).ToArray();
				if (deniedOrgs.Any())
				{
					firstDeniedSecurity = cacheCheckResult[deniedOrgs.First()];
					return true;
				}
				else
				{
					firstDeniedSecurity = null;
					return false;
				}
			}

			bool checkAllOrgPkIsCacheAsNonDenied()
			{
				var deniedOrgsInCache = cacheCheckResult
					.Where(kv => kv.Value == null)
					.Select(kv => kv.Key)
					.ToArray();
				return !orgHeaderPKs.Except(deniedOrgsInCache).Any();
			}
		}

		protected static SecurityCheckResult GetFirstDeniedSecurityCheckPoint(ZDBOnlySubQuery orgHeaderSubQuery, BusinessObjectFactory factory)
		{
			var (cacheCheckResult, cacheSecurityPerGroup) = GetSecurityCheckpointCache(factory);

			var ocdSubQuery = new ZDBOnlyQuery(typeof(OrgCompanyData));
			ocdSubQuery.AddSubQuery(OrgCompanyDataSchema.OB_OH, orgHeaderSubQuery, JoinCondition.And);
			var orgCompanies = (OrgCompanyData[])factory.Load(typeof(OrgCompanyData), ocdSubQuery);
			var orgCompaniesInScope = orgCompanies.Where(c =>
				c.OB_GC == GlbCompany.CurrentCompany.PK && !string.IsNullOrEmpty(c.OB_RateSecurityGroup)
				).ToArray();

			foreach (var orgCompanyData in orgCompanies.Except(orgCompaniesInScope, OrgCompanyDataComparer.CompareByOrganization))
			{
				cacheCheckResult[orgCompanyData.OB_OH] = null;
			}

			foreach (var orgCompanyData in orgCompaniesInScope.Where(c => cacheSecurityPerGroup.ContainsKey(c.OB_RateSecurityGroup)))
			{
				GetSecurityCheckResultWithCacheUpdate(orgCompanyData, cacheSecurityPerGroup[orgCompanyData.OB_RateSecurityGroup], false);
			}

			var orgPksInCompanies = orgCompaniesInScope.Select(c => c.OB_OH).ToArray();
			if (!TryGetFirstDeniedSecurityCheckPointFromCache(orgPksInCompanies, factory, out SecurityCheckResult result))
			{
				foreach (var orgCompanyData in orgCompaniesInScope.Where(c => !cacheSecurityPerGroup.ContainsKey(c.OB_RateSecurityGroup)).OrderBy(c => c.OB_RateSecurityGroup))
				{
					var securityGroupCode = orgCompanyData.OB_RateSecurityGroup;

					if (result == null)
					{
						if (cacheSecurityPerGroup.TryGetValue(securityGroupCode, out var deniedSecurity))
						{
							result = GetSecurityCheckResultWithCacheUpdate(orgCompanyData, deniedSecurity, false);
						}
						else
						{
							deniedSecurity = Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + securityGroupCode) ?? Env.Security.RatesSecurity;
							result = GetSecurityCheckResultWithCacheUpdate(orgCompanyData, deniedSecurity, true);
						}
					}
					else if (result.OrgCompanyData.OB_RateSecurityGroup == securityGroupCode)
					{
						GetSecurityCheckResultWithCacheUpdate(orgCompanyData, result.SecurityCheckPoint, false);
					}
					else
					{
						break;
					}
				}
			}
			return result;

			SecurityCheckResult GetSecurityCheckResultWithCacheUpdate(OrgCompanyData orgCompanyData, SecurityCheckpoint deniedSecurity, bool isNewSecurityGroup)
			{
				var checkResult = deniedSecurity == null || deniedSecurity.IsAllowed
					? null
					: new SecurityCheckResult
					{
						SecurityCheckPoint = deniedSecurity,
						OrgCompanyData = orgCompanyData
					};

				cacheCheckResult[orgCompanyData.OB_OH] = checkResult;

				if (isNewSecurityGroup)
				{
					cacheSecurityPerGroup[orgCompanyData.OB_RateSecurityGroup] = deniedSecurity;
				}

				return checkResult;
			}
		}

		public class SecurityCheckResult
		{
			public SecurityCheckpoint SecurityCheckPoint { get; set; }
			public OrgCompanyData OrgCompanyData { get; set; }
		}

		static (IDictionary<ZGuid, SecurityCheckResult> cacheCheckResult, Dictionary<ZString, SecurityCheckpoint> cacheSecurityPerGroup)
			GetSecurityCheckpointCache(BusinessObjectFactory factory)
				=> factory.GetCachedValue($"{GlbCompany.CurrentCompany.PK.ToStringKey()}_RateSecurityHelperCache",
					() => (new Dictionary<ZGuid, SecurityCheckResult>(), new Dictionary<ZString, SecurityCheckpoint>()));
	}
}
