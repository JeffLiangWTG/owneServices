using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.TransitWarehouse;

namespace Enterprise.Packing.Business
{
	public static class PkgPackageScreeningMethodExtensions
	{
		public static (ZString? ScreeningMethod, ZString? AviationSecurityAdditionalInspectionType) GetScreeningMethod(this PkgPackage package, IOutturnProvider provider)
		{
			if (package == null)
			{
				return (null, null);
			}

			return GetScreeningMethod(package, provider?.OverriddenAviationSecurityInspectionType, provider?.IsHighRisk);
		}

		public static (ZString? ScreeningMethod, ZString? AviationSecurityAdditionalInspectionType) GetScreeningMethod(this ITransitPackage transitPackage)
		{
			var package = transitPackage?.TransitPackage as PkgPackage;
			if (package == null)
			{
				return (null, null);
			}

			return GetScreeningMethod(package, transitPackage.OverriddenAviationSecurityInspectionType, transitPackage.IsHighRisk);
		}

		static (ZString? ScreeningMethod, ZString? AviationSecurityAdditionalInspectionType) GetScreeningMethod(PkgPackage package, ZString? overriddenAviationSecurityInspectionType, ZBool? isHighRisk)
		{
			ZString? screeningMethod = null;
			ZString? aviationSecurityAdditionalInspectionType = null;
			var last2Screenings = package.Screenings.OrderByDescending(s => s.KPS_Time).Take(2);
			var latestScreening = last2Screenings.FirstOrDefault();
			var lastSecondScreening = last2Screenings.Skip(1).FirstOrDefault();
			var innerPkgs = package.HandlingUnitPackedPackages;

			var aviationSecurityInspectionType = overriddenAviationSecurityInspectionType ?? ZString.Empty;
			if (aviationSecurityInspectionType.IsEmpty)
			{
				aviationSecurityInspectionType = (latestScreening?.KPS_Passed ?? false) ? latestScreening.KPS_Method : ZString.Empty;
				if (isHighRisk ?? false)
				{
					if ((lastSecondScreening?.KPS_Passed ?? false) && (latestScreening?.KPS_Passed ?? false))
					{
						aviationSecurityInspectionType = lastSecondScreening.KPS_Method;

						if (lastSecondScreening?.KPS_Method != latestScreening?.KPS_Method)
						{
							aviationSecurityAdditionalInspectionType = latestScreening.KPS_Method;
						}
					}
				}
				else if (latestScreening == null && innerPkgs.Any() && !package.IsTransitOverpackPackage)
				{
					var innerScreeningMethod = innerPkgs.FirstOrDefault().LatestScreening?.KPS_Method ?? "";

					if (!innerPkgs.Any(pkg => !(pkg.LatestScreening?.KPS_Passed ?? false) || pkg.LatestScreening.KPS_Method != innerScreeningMethod))
					{
						aviationSecurityInspectionType = innerScreeningMethod;
					}
				}
			}

			if (!aviationSecurityInspectionType.IsEmpty)
			{
				screeningMethod = aviationSecurityInspectionType;
			}

			return (screeningMethod, aviationSecurityAdditionalInspectionType);
		}
	}
}
