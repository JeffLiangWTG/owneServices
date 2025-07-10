using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business.Testing
{
	public class IntercompanyTariffSecurityTest : RatingTestCase
	{
		public void TestWhenNoSecurityRightDefined()
		{
			var securityRightInfos = Enumerable.Empty<(string, string, bool)>();
			var accessibleServiceProviderPKs = Enumerable.Empty<ZGuid>();
			var grantedServiceProviderPKs = Enumerable.Empty<ZGuid>();

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestWhenCurrentUserIsController()
		{
			var securityRightInfos = Enumerable.Empty<(string, string, bool)>();
			var accessibleServiceProviderPKs = Enumerable.Empty<ZGuid>(); // We don't care about this list when current user is a controller
			var grantedServiceProviderPKs = Organizations.Values.Select(x => x.PK).ToList();

			using (SetCurrentUserInfo(isController: true, isSupport: false))
			{
				AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs, true);
			}
		}

		public void TestWhenCurrentUserIsSupport()
		{
			var securityRightInfos = Enumerable.Empty<(string, string, bool)>();
			var accessibleServiceProviderPKs = Enumerable.Empty<ZGuid>(); // We don't care about this list when current user is support
			var grantedServiceProviderPKs = Organizations.Values.Select(x => x.PK).ToList();

			using (SetCurrentUserInfo(isController: false, isSupport: true))
			{
				AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs, true);
			}
		}

		public void TestWhenFullGranted()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					(null, null, true),
			};
			var accessibleServiceProviderPKs = new List<ZGuid> { ZGuid.Empty };
			var grantedServiceProviderPKs = Organizations.Values.Select(x => x.PK).ToList();

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestWhenCompanyDeniedButThereIsFullGranted()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					(null, null, true),
					("CM1", null, false) // We don't care company when there ia a full access
			};
			var accessibleServiceProviderPKs = new List<ZGuid> { ZGuid.Empty };
			var grantedServiceProviderPKs = Organizations.Values.Select(x => x.PK).ToList();

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestWhenBranchDeniedButThereIsFullGranted()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					(null, null, true),
					(null, "BR2", false) // We don't care branch when there ia a full access
			};
			var accessibleServiceProviderPKs = new List<ZGuid> { ZGuid.Empty };
			var grantedServiceProviderPKs = Organizations.Values.Select(x => x.PK).ToList();

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestWhenBranchGranted()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					(null, "BR2", true)
			};

			var accessibleServiceProviderPKs = new List<ZGuid> { Organizations["ORG2"].PK };
			var grantedServiceProviderPKs = new List<ZGuid> { Organizations["ORG2"].PK };

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestWhenCompanyGranted()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					("CM1", null, true)
			};
			var accessibleServiceProviderPKs = new List<ZGuid>
				{
					Organizations["ORG1"].PK,
					Organizations["ORG2"].PK,
					Organizations["ORG3"].PK
				};
			var grantedServiceProviderPKs = new List<ZGuid>
				{
					Organizations["ORG1"].PK,
					Organizations["ORG2"].PK,
					Organizations["ORG3"].PK
				};

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestWhenCompanyGrantedButInnerBranchDenied()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					("CM1", null, true),
					(null, "BR3", false) // We don't care about branch when parent company is granted
			};
			var accessibleServiceProviderPKs = new List<ZGuid>
				{
					Organizations["ORG1"].PK,
					Organizations["ORG2"].PK,
					Organizations["ORG3"].PK
				};
			var grantedServiceProviderPKs = new List<ZGuid>
				{
					Organizations["ORG1"].PK,
					Organizations["ORG2"].PK,
					Organizations["ORG3"].PK
				};

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestWhenCompanyDeniedButInnerBranchGranted()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					("CM1", null, false),
					(null, "BR3", true) // We don't care about company when branch is granted
			};
			var accessibleServiceProviderPKs = new List<ZGuid> { Organizations["ORG3"].PK };
			var grantedServiceProviderPKs = new List<ZGuid> { Organizations["ORG3"].PK };

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestGrantedCompanyVsBranchesInOtherCompanies()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					("CM1", null, true),
					(null, "BR5", false),
					(null, "BR9", true)
			};
			var accessibleServiceProviderPKs = new List<ZGuid>
				{
					Organizations["ORG1"].PK,
					Organizations["ORG2"].PK,
					Organizations["ORG3"].PK,
					Organizations["ORG9"].PK
				};
			var grantedServiceProviderPKs = new List<ZGuid>
				{
					Organizations["ORG1"].PK,
					Organizations["ORG2"].PK,
					Organizations["ORG3"].PK,
					Organizations["ORG9"].PK
				};

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		public void TestDeniedCompanyVsBranchesInOtherCompanies()
		{
			var securityRightInfos = new (string, string, bool)[]
			{
					("CM1", null, false),
					(null, "BR5", false),
					(null, "BR9", true)
			};
			var accessibleServiceProviderPKs = new List<ZGuid> { Organizations["ORG9"].PK };
			var grantedServiceProviderPKs = new List<ZGuid> { Organizations["ORG9"].PK };

			AssertSecurityRights(securityRightInfos, accessibleServiceProviderPKs, grantedServiceProviderPKs);
		}

		void AssertSecurityRights(IEnumerable<(string companyCode, string branchCode, bool isAllowed)> securityRightInfos, IEnumerable<ZGuid> accessibleServiceProviderPKs, IEnumerable<ZGuid> grantedServiceProviderPKs, bool checkGrantedForAllSecurityRights = false)
		{
			foreach (var securityRightCode in SecurityRightCodes)
			{
				using (SetupSecurityRights(securityRightInfos.Select(x => (securityRightCode, x.companyCode, x.branchCode, x.isAllowed))))
				{
					var otherSecurityRightCodes = SecurityRightCodes.Except(securityRightCode).ToList();

					AssertContainsExactElementsInAnyOrder(accessibleServiceProviderPKs, GetAccessibleServiceProviderPKs(securityRightCode));
					otherSecurityRightCodes.ForEach(code => AssertEquals(0, GetAccessibleServiceProviderPKs(code).Count()));

					foreach (var orgProxyPK in Organizations.Values.Select(x => x.PK))
					{
						var isGranted = grantedServiceProviderPKs.Contains(orgProxyPK);
						AssertEquals(isGranted, HasAccessToOrgProxy(orgProxyPK, securityRightCode));
						otherSecurityRightCodes.ForEach(code =>
							AssertEquals(checkGrantedForAllSecurityRights && isGranted,
								HasAccessToOrgProxy(orgProxyPK, code)));
					}
				}
			}

			Assert(true);
		}

		bool HasAccessToOrgProxy(ZGuid orgProxyPK, string securityRight) =>
			IntercompanyTariffSecurity.HasCurrentUserAccessToOrgProxy(Factory, orgProxyPK, securityRight);

		IEnumerable<ZGuid> GetAccessibleServiceProviderPKs(string securityRight) =>
			IntercompanyTariffSecurity.GetAccessibleOrgProxyPKsForCurrentUser(Factory, securityRight);

		IDisposable SetupSecurityRights(IEnumerable<(string securityRight, string companyCode, string branchCode, bool isAllowed)> securityRightInfos)
		{
			var securityRights = new List<GlbSecurity>();

			foreach (var securityRightInfo in securityRightInfos)
			{
				var securityRight = Factory.NewWithValidTestData<GlbSecurity>();
				securityRight.GU_GS = Env.CurrentUserPK;
				securityRight.GU_SecurityRight = securityRightInfo.securityRight;
				securityRight.GU_GC = string.IsNullOrWhiteSpace(securityRightInfo.companyCode)
					? ZGuid.Empty
					: Companies[securityRightInfo.companyCode].PK;
				securityRight.GU_GB = string.IsNullOrWhiteSpace(securityRightInfo.branchCode)
					? ZGuid.Empty
					: Branches[securityRightInfo.branchCode].PK;
				securityRight.GU_SecurityItemIsAllowed = securityRightInfo.isAllowed;

				securityRights.Add(securityRight);
			}

			return new DisposableAction(() =>
			{
				securityRights.ForEach(x => x.Delete());
			});
		}

		protected override void TearDown()
		{
			Branches.Values.ToList().ForEach(x => x.Delete());
			Companies.Values.ToList().ForEach(x => x.Delete());
			Organizations.Values.ToList().ForEach(x => x.Delete());

			temporaryUserInfo.Dispose();

			base.FinalTearDown();
		}

		protected override void SetUp()
		{
			base.MasterSetUp();

			temporaryUserInfo = SetCurrentUserInfo(false, false);

			var setupItems = new (string companyCode, string branchCode, string orgProxy)[]
			{
					("CM1", "   ", "ORG1"),
					("CM1", "BR1", "ORG1"),
					("CM1", "BR2", "ORG2"),
					("CM1", "BR3", "ORG3"),
					("CM2", "   ", "ORG4"),
					("CM2", "BR4", "ORG4"),
					("CM2", "BR5", "ORG5"),
					("CM2", "BR6", "ORG6"),
					("CM3", "   ", "ORG7"),
					("CM3", "BR7", "ORG7"),
					("CM3", "BR8", "ORG8"),
					("CM3", "BR9", "ORG9")
			};

			foreach (var orgCode in setupItems.Select(x => x.orgProxy).Distinct())
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();
				organization.OH_Code = orgCode;

				Organizations.Add(orgCode, organization);
			}

#if NETFRAMEWORK
			foreach (var companyInfo in setupItems.Where(x => string.IsNullOrWhiteSpace(x.branchCode)).DistinctBy(x => x.companyCode))
#elif NET
			foreach (var companyInfo in Enumerable.DistinctBy(setupItems.Where(x => string.IsNullOrWhiteSpace(x.branchCode)), x => x.companyCode))
#endif
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Code = companyInfo.companyCode;
				company.GC_OH_OrgProxy = Organizations[companyInfo.orgProxy].PK;

				Companies.Add(companyInfo.companyCode, company);
			}

#if NETFRAMEWORK
			foreach (var branchInfo in setupItems.DistinctBy(x => x.branchCode))
#elif NET
			foreach (var branchInfo in Enumerable.DistinctBy(setupItems, x => x.branchCode))
#endif
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_Code = branchInfo.branchCode;
				branch.GB_GC = Companies[branchInfo.companyCode].PK;
				branch.GB_OH_OrgProxy = Organizations[branchInfo.orgProxy].PK;

				Branches.Add(branchInfo.branchCode, branch);
			}
		}

		IDisposable SetCurrentUserInfo(bool isController, bool isSupport)
		{
			var controllerStatus = GlbStaff.CurrentUser.GS_IsController;
			var loginName = GlbStaff.CurrentUser.GS_LoginName;

			GlbStaff.CurrentUser.GS_IsController = isController;
			GlbStaff.CurrentUser.GS_LoginName = isSupport ? User.SupportUserName : "ALAKI";

			return new DisposableAction(() =>
			{
				GlbStaff.CurrentUser.GS_IsController = controllerStatus;
				GlbStaff.CurrentUser.GS_LoginName = loginName;
			});
		}

		IDictionary<string, OrgHeader> Organizations { get; } = new Dictionary<string, OrgHeader>();
		IDictionary<string, GlbCompany> Companies { get; } = new Dictionary<string, GlbCompany>();
		IDictionary<string, GlbBranch> Branches { get; } = new Dictionary<string, GlbBranch>();

		IDisposable temporaryUserInfo = DisposableAction.NoAction;

		readonly string[] SecurityRightCodes =
		{
			Env.Security.IntercompanyTariffsView.Code,
			Env.Security.IntercompanyTariffsEdit.Code,
			Env.Security.IntercompanyTariffsDelete.Code
		};
	}
}
