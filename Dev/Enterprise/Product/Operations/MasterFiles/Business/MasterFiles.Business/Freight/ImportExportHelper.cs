using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class ImportExportHelper
	{
		public static Directions GetJobDirection(ZString origin, ZString destination)
		{
			return GetJobDirection(origin, destination, GlbBranch.CurrentBranch.HomePort);
		}

		public static Directions GetJobDirection(ZString origin, ZString destination, ILocationReference homePort)
		{
			Func<bool?> isImport = () => IsImport(origin, destination, homePort);
			Func<bool?> isExport = () => IsExport(origin, destination, homePort);
			Func<bool?> isCrossTrade = () => IsCrossTrade(origin, destination, homePort);
			Func<bool?> isDomestic = () => IsDomestic(origin, destination, homePort);

			return GetJobDirection(isImport, isExport, isCrossTrade, isDomestic);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Directions GetJobDirection(Func<bool?> import = null, Func<bool?> export = null, Func<bool?> crossTrade = null, Func<bool?> domestic = null)
		{
			return Is(import)
				? Directions.Import
				: Is(export)
					? Directions.Export
					: Is(crossTrade)
						? Directions.CrossTrade
						: Is(domestic)
							? Directions.Domestic
							: Directions.Unknown;
		}

		static bool Is(Func<bool?> boolFunc)
		{
			var value = boolFunc?.Invoke();
			return value.HasValue && value.Value;
		}

		public static ZString GetDirectionCode(Directions directions)
		{
			switch (directions)
			{
				case Directions.Import:
					return OrgDocumentLookups.FilterDirectionConstants.Codes.Import;
				case Directions.Export:
					return OrgDocumentLookups.FilterDirectionConstants.Codes.Export;
				case Directions.Domestic:
					return OrgDocumentLookups.FilterDirectionConstants.Codes.Domestic;
				case Directions.CrossTrade:
					return OrgDocumentLookups.FilterDirectionConstants.Codes.CrossTrade;
				case Directions.Unknown:
					return OrgDocumentLookups.FilterDirectionConstants.Codes.Other;
				default:
					return ZString.Empty;
			}
		}

		static Guid GetCountryPK(ZString port)
		{
			if (!port.IsEmpty)
			{
				var rnCode = port.SubstringSafe(0, 2);
				var cacheKey = "ImportExportHelper.GetCountryPK." + rnCode;

				var country = RegistryFactory.Instance.GetCachedValue(cacheKey, () =>
								RegistryFactory.Instance.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, rnCode)));

				if (country != null)
				{
					return country.PK.ToGuid();
				}
			}

			return Guid.Empty;
		}

		#region Local

		public static bool IsBranchCountry(ZString code)
		{
			return IsLocal(code, GlbBranch.CurrentBranch.HomePort);
		}

		public static bool IsLocal(ZString code, ILocationReference homePort)
		{
			return homePort != null && homePort.IsLocalInRelationTo(code);
		}

		#endregion

		public static bool IsAnyBranchCountry(BusinessObjectFactory factory, ZString location)
		{
			bool result = false;

			if (factory != null && !location.IsEmpty && location.Length >= 2)
			{
				ZDBOnlyQuery branchQuery = new ZDBOnlyQuery(typeof(GlbBranch));
				branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);
				branchQuery.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, location.SubstringSafe(0, 2));

				ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
				companySubQuery.AddToFilter(GlbCompanySchema.GC_IsActive, ZBool.True);
				companySubQuery.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");
				branchQuery.AddSubQuery(companySubQuery, JoinCondition.And);

				result = factory.LoadTop1<GlbBranch>(branchQuery) != null; // don't use databasecount as this caches/db count doesn't
			}

			return result;
		}

		#region IsImport

		public static bool IsImport(ZString origin, ZString destination)
		{
			return IsImport(origin, destination, GlbBranch.CurrentBranch.HomePort);
		}

		public static bool IsImport(ZString origin, ZString destination, ILocationReference homePort)
		{
			bool result;

			if (!destination.IsEmpty && !origin.IsEmpty)
			{
				result = (!IsLocal(origin, homePort) && IsLocal(destination, homePort)) || IsCommunityRegionWithAForeignOppositePort(destination, origin);
			}
			else if (!origin.IsEmpty)
			{
				result = !IsLocal(origin, homePort);
			}
			else if (!destination.IsEmpty)
			{
				result = !GlbDepartment.CurrentDepartment.GE_Domestic && IsLocal(destination, homePort);
			}
			else
			{
				result = GlbDepartment.CurrentDepartment.GE_Import;
			}
			return result;
		}

		#endregion

		#region IsExport

		public static bool IsExport(ZString origin, ZString destination)
		{
			return IsExport(origin, destination, GlbBranch.CurrentBranch.HomePort);
		}

		public static bool IsExport(ZString origin, ZString destination, ILocationReference homePort)
		{
			bool result;

			if (!destination.IsEmpty && !origin.IsEmpty)
			{
				result = (IsLocal(origin, homePort) && !IsLocal(destination, homePort)) || IsCommunityRegionWithAForeignOppositePort(origin, destination);
			}
			else if (!origin.IsEmpty)
			{
				result = !GlbDepartment.CurrentDepartment.GE_Domestic && IsLocal(origin, homePort);
			}
			else if (!destination.IsEmpty)
			{
				result = !IsLocal(destination, homePort);
			}
			else
			{
				result = GlbDepartment.CurrentDepartment.GE_Export;
			}

			return result;
		}

		#endregion

		#region IsCrossTrade

		public static bool IsCrossTrade(ZString origin, ZString destination)
		{
			return IsCrossTrade(origin, destination, GlbBranch.CurrentBranch.HomePort);
		}

		public static bool IsCrossTrade(ZString origin, ZString destination, ILocationReference homePort)
		{
			return !destination.IsEmpty
				&& !origin.IsEmpty
				&& origin.SubstringSafe(0, 2) != destination.SubstringSafe(0, 2)
				&& ((!IsInCommunityRegion(origin)
				&& !IsInCommunityRegion(destination)
				&& !IsLocal(destination, homePort)
				&& !IsLocal(origin, homePort))
				|| (IsInCommunityRegion(origin) && IsInCommunityRegion(destination) && !IsBranchCountry(origin)));
		}

		#endregion

		#region IsDomestic

		public static bool IsDomestic(ZString origin, ZString destination)
		{
			return IsDomestic(origin, destination, GlbBranch.CurrentBranch.HomePort);
		}

		public static bool IsDomestic(ZString origin, ZString destination, ILocationReference homePort)
		{
			if (!destination.IsEmpty && !origin.IsEmpty
				&& destination.SubstringSafe(0, 2) == origin.SubstringSafe(0, 2))
			{
				return true;
			}
			else if (!destination.IsEmpty && !origin.IsEmpty)
			{
				return IsLocal(origin, homePort) && IsLocal(destination, homePort);
			}
			else if (!origin.IsEmpty)
			{
				return GlbDepartment.CurrentDepartment.GE_Domestic && IsLocal(origin, homePort);
			}
			else if (!destination.IsEmpty)
			{
				return GlbDepartment.CurrentDepartment.GE_Domestic && IsLocal(destination, homePort);
			}
			else
			{
				return GlbDepartment.CurrentDepartment.GE_Domestic;
			}
		}

		#endregion

		#region IsLocalRegionWithAForeignOppositePort

		public static bool IsCommunityRegionWithAForeignOppositePort(ZString probePortCode, ZString oppositePortCode)
		{
			var communityRegions = FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.Value;

			var probeCountryPK = GetCountryPK(probePortCode);
			var oppositeCountryCodePK = GetCountryPK(oppositePortCode);

			return communityRegions.Contains(probeCountryPK) && !communityRegions.Contains(oppositeCountryCodePK) && !IsBranchCountry(oppositePortCode);
		}

		public static bool IsInCommunityRegion(ZString portCode)
		{
			var communityRegions = FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.Value;
			return communityRegions.Contains(GetCountryPK(portCode));
		}

		#endregion
	}
}
