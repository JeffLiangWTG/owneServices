using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using AuthenticationService.Client.Models;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business;

/// <summary>
///		A specialized version of the <see cref="WTGAuthTokenProvider"/>, designed specifically for rating purposes.
///		The main difference is that it overrides the EnterpriseCode in the token based on the
///		<see cref="RatingDataRegistry.Instance.EnterpriseCodeOverride"/> registry value.
/// </summary>
public class WTGAuthTokenProviderForRating : WTGAuthTokenProvider
{
	public WTGAuthTokenProviderForRating()
		: this(new WTGAuthServiceClientFactory(), new CargoWiseEnvironment())
	{
	}

	internal WTGAuthTokenProviderForRating(IWTGAuthServciceClientFactory authServiceClientFactory, ICargoWiseEnvironment cargoWiseEnvironment)
		: base(authServiceClientFactory)
	{
		this.cargoWiseEnvironment = Argument.NotNull(cargoWiseEnvironment, nameof(cargoWiseEnvironment));
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Doesn't matter in our case, we specify in the name that the value is in seconds")]
	protected override (string Token, string ValidationMessage) GetTokenCore(
		string correlationID,
		int secondsBeforeTokenExpiry = 30,
		Action<LoginInfo> overrideLoginInfo = null,
		CancellationToken cancellationToken = default,
		bool useEnvironmentLoginInfoFromConstructor = false)
	{
		return base.GetTokenCore(
			correlationID,
			secondsBeforeTokenExpiry,
			loginInfo =>
			{
				overrideLoginInfo?.Invoke(loginInfo);
				OverrideLoginInfo(loginInfo);
			},
			cancellationToken,
			useEnvironmentLoginInfoFromConstructor
		);
	}

	void OverrideLoginInfo(LoginInfo loginInfo)
	{
		// Overriding the EnterpriseCode is quite a dangerous thing. If it is accidentally done by a support person on a client machine,
		// the client may get rates for another client. The registry should not be visible on a client machine due to the registry setup,
		// but we make another check here to be 100% sure that we override EnterpriseCode only for CW1 instances in WTG network.
		var isInWtgDomain = cargoWiseEnvironment.IsCargoWiseDomain(IPGlobalProperties.GetIPGlobalProperties().DomainName);
		var enterpriseCodeOverride = RatingDataRegistry.Instance.EnterpriseCodeOverride.Value;

		if (isInWtgDomain && enterpriseCodeOverride != loginInfo.EnterpriseCode)
		{
			loginInfo.EnterpriseCodeOverride = enterpriseCodeOverride;
		}

		if (RatingFeatureHelper.Urs.IsEnabled)
		{
			loginInfo.Audience = "URS";
			loginInfo.UrsSecurityAccessRights = GetSecurityAccessRights();
		}
	}

	static IEnumerable<UrsSecurityAccessRights> GetSecurityAccessRights()
	{
		return
		[
			GetAccessRights(TransportMode.Ocean, Env.Security.WiseRatesCargoSphereRateSearchCompanyAccess),
			GetAccessRights(TransportMode.Air, Env.Security.WiseRatesCargoguideRateSearchCompanyAccess)
		];
	}

	static UrsSecurityAccessRights GetAccessRights(TransportMode transport, SecurityCheckpoint checkpoint)
	{
		var accessRights = new UrsSecurityAccessRights { TransportMode = transport };
		var readFactory = new ReadOnlyBusinessObjectFactory();

		// All allowed
		var locator = new SecurityLocator(GlbStaff.CurrentUser, readFactory);
		if (locator.IsSecurityAllowedForAllBranches(checkpoint))
		{
			accessRights.Allowed = new UrsSecurityAccessList
			{
				Companies = new [] { "*" },
				Countries = new [] { "*" }
			};

			return accessRights;
		}

		// All denied
		var allowedBranches = locator.GetAllBranchesThatStaffHasPermissionsFor(checkpoint)
			.Where(b => b.GB_IsActive)
			.ToList();

		if (!allowedBranches.Any())
		{
			accessRights.Denied = new UrsSecurityAccessList
			{
				Companies = ["*"],
				Countries = ["*"]
			};

			return accessRights;
		}

		// Mixed
		var allBranches = new GlbBranchCollection(readFactory, new ZQuery(GlbBranchSchema.GB_IsActive, true));
		allBranches.Load();
		var allCompanies = allBranches
			.Select(b => b.Company)
			.Where(c => c.GC_IsActive);
#if NETFRAMEWORK
		allCompanies = allCompanies.DistinctBy(c => c.GC_Code);
#elif NET
		allCompanies = Enumerable.DistinctBy(allCompanies, c => c.GC_Code);
#endif
		allCompanies = allCompanies.ToList();

		var allowedCompanies = allowedBranches.Select(b => b.Company).Where(c => c.GC_IsActive).ToHashSet();
		var deniedCompanies = allCompanies.Except(allowedCompanies).ToHashSet();

		var (isAllowed, list) = CreateAccessList(readFactory, allowedCompanies, deniedCompanies);
		if (isAllowed)
		{
			accessRights.Allowed = list;
		}
		else
		{
			accessRights.Denied = list;
		}

		return accessRights;
	}

	static (bool isAllowed, UrsSecurityAccessList list) CreateAccessList(ReadOnlyBusinessObjectFactory readFactory, HashSet<GlbCompany> allowedCompanies, HashSet<GlbCompany> deniedCompanies)
	{
		var allowedCountries = GetOperatingCountries(readFactory, allowedCompanies);

		if (allowedCompanies.Count < deniedCompanies.Count)
		{
			var list = new UrsSecurityAccessList
			{
				Companies = allowedCompanies.Select(c => (string)c.GC_Code).ToArray(),
				Countries = allowedCountries.Count != 0 ? allowedCountries : null
			};

			return (true, list);
		}
		else
		{
			var deniedCountries = GetOperatingCountries(readFactory, deniedCompanies).Except(allowedCountries).ToList();

			var list = new UrsSecurityAccessList
			{
				Companies = deniedCompanies.Select(c => (string)c.GC_Code).ToArray(),
				Countries = deniedCountries.Count != 0 ? deniedCountries : null
			};

			return (false, list);
		}
	}

	static HashSet<string> GetOperatingCountries(ReadOnlyBusinessObjectFactory readFactory, IEnumerable<GlbCompany> companies)
	{
		var countries = new HashSet<string>();

		foreach (var company in companies)
		{
			countries.Add(company.GC_RN_NKCountryCode);

			var otherCountries = FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			foreach (var countryPk in otherCountries)
			{
				var country = readFactory.Load<RefCountry>(countryPk);
				countries.Add(country.RN_Code);
			}
		}

		return countries;
	}

	readonly ICargoWiseEnvironment cargoWiseEnvironment;
}
