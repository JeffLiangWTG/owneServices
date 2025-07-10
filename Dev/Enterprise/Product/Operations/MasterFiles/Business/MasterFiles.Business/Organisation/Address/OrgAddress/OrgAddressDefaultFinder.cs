using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public sealed class OrgAddressDefaultFinder
	{
		public OrgAddressDefaultFinder(IEnumerable<OrgAddress> addressList, bool useCommonLanguage, string preferredLanguage)
		{
			if (addressList == null)
			{
				throw new ArgumentNullException(nameof(addressList), "IEnumerable<OrgAddress> addressList");
			}

			this.addressList = addressList;
			this.parentOrg = GetParentOrg();
			this.parentIsGlobalAccount = parentOrg != null && parentOrg.OH_IsGlobalAccount && parentOrg.OH_IsShippingProvider;
			if (useCommonLanguage)
			{
				this.commonLanguage = GetCommonLanguage();
			}
			this.preferredLanguage = preferredLanguage;
		}

		string GetCommonLanguage()
		{
			string result = Constants.Languages.English;
			if (parentOrg != null)
			{
				ZString companyLanguage = GlbCompany.CurrentCompany.OrgProxy != null ? GlbCompany.CurrentCompany.OrgProxy.OH_Language : ZString.Empty;

				if (!companyLanguage.IsEmpty && companyLanguage != Constants.Languages.English && parentOrg.OH_Language == companyLanguage)
				{
					result = companyLanguage;
				}
			}
			return result;
		}

		readonly IEnumerable<OrgAddress> addressList;
		readonly bool parentIsGlobalAccount;
		readonly string preferredLanguage;
		readonly string commonLanguage;
		readonly OrgHeader parentOrg;

		OrgHeader GetParentOrg()
		{
			foreach (OrgAddress address in addressList)
			{
				if (!address.IsDeleted)
				{
					OrgHeader result = address.Header;
					if (result != null && !result.IsDeleted)
					{
						return result;
					}
				}
			}
			return null;
		}

		public OrgAddress DefaultAddressOfType(OrgAddressType addressType, bool activeOnly = true)
		{
			OrgAddress result = null;
			int resultScore = 0;
			var maxScore = MaxScore;

			foreach (var address in addressList.OrderBy(address => address.OA_SystemCreateTimeUtc))
			{
				int score = 0;
				if (!address.IsDeleted && (!activeOnly || address.OA_IsActive) && address.IsMainAddressOfType(addressType))
				{
					if (parentIsGlobalAccount)
					{
						if (GlbBranch.CurrentBranch != null && !address.OA_RL_NKRelatedPortCode.IsEmpty && !GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty
							&& address.OA_RL_NKRelatedPortCode == GlbBranch.CurrentBranch.GB_RL_NKHomePort)
						{
							score += 7;
						}
						else if (GlbCompany.CurrentCompany != null && address.RelatedCountry != null && GlbCompany.CurrentCompany.Country != null
						&& address.RelatedCountry.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
						{
							score += 6;
						}
					}

					if (!string.IsNullOrEmpty(preferredLanguage) && address.OA_Language == preferredLanguage)
					{
						score += 5;
					}
					else if (!string.IsNullOrEmpty(commonLanguage) && address.OA_Language == commonLanguage)
					{
						score += 4;
					}
					else if (address.OA_Language == Constants.Languages.English)
					{
						score += 3;
					}
					else if (parentOrg != null && address.OA_Language == parentOrg.OH_Language)
					{
						score += 2;
					}
					else
					{
						score += 1;
					}
				}

				if (score > resultScore)
				{
					result = address;
					resultScore = score;

					if (score == maxScore)
					{
						break;
					}
				}
			}

			return result;
		}

		int MaxScore
		{
			get
			{
				var score = 0;

				if (parentIsGlobalAccount)
				{
					if (GlbBranch.CurrentBranch != null && !GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
					{
						score += 7;
					}
					else if (GlbCompany.CurrentCompany != null && GlbCompany.CurrentCompany.Country != null)
					{
						score += 6;
					}
				}

				if (!string.IsNullOrEmpty(preferredLanguage))
				{
					score += 5;
				}
				else if (!string.IsNullOrEmpty(commonLanguage))
				{
					score += 4;
				}
				else
				{
					score += 3;
				}

				return score;
			}
		}
	}
}
