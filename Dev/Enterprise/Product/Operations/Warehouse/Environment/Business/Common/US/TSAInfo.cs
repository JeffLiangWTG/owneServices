using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.US
{
	public static class TSAInfo
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static bool IsOrgAddressTSAKnown(OrgAddress address)
		{
			return IsOrgAddressTSAKnown(address, true);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static bool IsOrgAddressTSAKnown(OrgAddress address, bool checkAddressCountry)
		{
			bool result = false;
			bool runQuery = !checkAddressCountry;
			if (address != null)
			{
				if (checkAddressCountry)
				{
					if (address.RelatedPortCode != null)
					{
						if (address.RelatedPortCode.RL_RN_NKCountryCode == new ZString(Enterprise.Core.Constants.CountryCodes.UnitedStates))
						{
							runQuery = true;
						}
					}
				}
				if (runQuery)
				{
					ZQuery filter = new ZQuery(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, Enterprise.Core.Constants.CountryCodes.UnitedStates);
					filter.AddToFilter(new ZQuery(OrgCountryDataSchema.OV_OA_ApprovedLocation, SQLComparisonOperator.Equal, address.PK), JoinCondition.And);
					OrgCountryData countryData = address.Factory.LoadTop1<OrgCountryData>(filter);
					if (countryData != null)
					{
						result = (countryData.OV_EXApprovedOrMajorExporter == CodeLists.US.TSAStatus.Codes.Known);
					}
				}
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static bool IsOrgHeaderTSAKnown(OrgHeader org)
		{
			return org != null && IsOrgAddressTSAKnown(org.MainAddress, false);
		}
	}
}
