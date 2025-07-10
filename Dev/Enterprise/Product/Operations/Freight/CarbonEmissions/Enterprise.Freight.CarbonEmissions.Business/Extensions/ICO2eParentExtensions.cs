using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class ICO2eParentExtensions
	{
		public static ZDecimal GetTotalCO2e(this ICO2eParent parent, ZString type = default) =>
			parent.FindJobCO2eByType(type)?.TotalCO2e ?? 0;

		public static void SetTotalCO2e(this ICO2eParent parent, ZDecimal value, ZString type = default) =>
			parent.GetOrCreateJobCO2e(type: type).TotalCO2e = value;

		public static ZDecimal GetCO2ePerTonneInKg(this ICO2eParent parent, ZString type = default) =>
			parent.FindJobCO2eByType(type)?.CO2ePerTonneInKg ?? 0;

		public static void SetCO2ePerTonneInKg(this ICO2eParent parent, ZDecimal value, ZString type = default) =>
			parent.GetOrCreateJobCO2e(type: type).CO2ePerTonneInKg = value;

		public static ZDecimal GetCO2ePerTEUInKg(this ICO2eParent parent, ZString type = default) =>
			parent.FindJobCO2eByType(type)?.CO2ePerTEUInKg ?? 0;

		public static void SetCO2ePerTEUInKg(this ICO2eParent parent, ZDecimal value, ZString type = default) =>
			parent.GetOrCreateJobCO2e(type: type).CO2ePerTEUInKg = value;

		public static ZString GetCO2eStatus(this ICO2eParent parent, ZString type = default) =>
			parent.FindJobCO2eByType(type)?.Status ?? CO2eStatusList.Codes.NotCalculated;

		public static void SetCO2eStatus(this ICO2eParent parent, ZString value, ZString type = default) =>
			parent.GetOrCreateJobCO2e(type: type).Status = value;

		public static ZDecimal GetCO2eDistanceInKM(this ICO2eParent parent, ZString type = default) =>
			parent.FindJobCO2eByType(type)?.DistanceInKM ?? 0;

		public static void SetCO2eDistanceInKM(this ICO2eParent parent, ZDecimal value, ZString type = default) =>
			parent.GetOrCreateJobCO2e(type: type).DistanceInKM = value;

		public static IJobCO2e FindJobCO2eByType(this ICO2eParent parent, ZString type) => parent.JobCO2eCollection.Get(type);

		public static bool JobCO2eExists(this ICO2eParent parent, ZString type = default)
		{
			var jobCO2e = parent.FindJobCO2eByType(type) as BusinessObject;
			return jobCO2e != null && !jobCO2e.IsDeleting && !jobCO2e.IsDeleted;
		}

		public static IJobCO2e GetOrCreateJobCO2e(this ICO2eParent parent, ZString type = default) => parent.JobCO2eCollection.GetOrCreate(type);

		public static void CopyJobCO2eTo(this ICO2eParent fromParent, ICO2eParent toParent)
		{
			if (!fromParent.JobCO2eCollection.IsNullOrEmpty())
			{
				foreach (var jobCO2e in fromParent.JobCO2eCollection.ToList())
				{
					CopyJobCO2e(jobCO2e);
				}
			}

			void CopyJobCO2e(IJobCO2e jobCO2e)
			{
				var cloneJobCO2e = (JobCO2e)(((BusinessObject)jobCO2e).Clone());
				cloneJobCO2e.JCO_ParentTableCode = toParent.JobCO2eParentTableCode;
				cloneJobCO2e.JCO_ParentID = toParent.JobCO2eParentID;
				toParent.RefreshCO2e();
			}
		}

		public static void ImportEmission(this ICO2eParent parent, GreenhouseGasEmission greenhouseGasEmissionData, ZString type = default, ZDecimal additionalEmissions = default)
		{
			if (greenhouseGasEmissionData.TryGetTotalCO2eValueInKG(out var total))
			{
				parent.SetTotalCO2e(total + additionalEmissions, type);
			}
			if (greenhouseGasEmissionData.TryGetCO2eValueInKG(out var co2e))
			{
				parent.SetCO2ePerTonneInKg(co2e, type);
			}

			if (greenhouseGasEmissionData.TryGetCO2eTEUValueInKG(out var teu))
			{
				parent.SetCO2ePerTEUInKg(teu, type);
			}

			parent.SetCO2eDistanceInKM(greenhouseGasEmissionData?.CO2eDistanceInKm ?? parent.GetCO2eDistanceInKM(type), type);
		}

		public static bool IsSingle(this ICO2eParent parent) => parent.GetJobCO2eTypesAttribute() == null;

		static JobCO2eTypesAttribute GetJobCO2eTypesAttribute(this ICO2eParent parent)
		{
			var jobCO2eCollectionProperty = parent.GetType().GetProperty(nameof(ICO2eParent.JobCO2eCollection));
			return jobCO2eCollectionProperty?.GetCustomAttributes(typeof(JobCO2eTypesAttribute), false).FirstOrDefault() as JobCO2eTypesAttribute;
		}

		public static void ValidateCO2eType(this ICO2eParent parent, string type)
		{
			var typeAttribute = parent.GetJobCO2eTypesAttribute();

			if (typeAttribute != null)
			{
				if (!typeAttribute.Types.Contains(type))
				{
					ErrorReporter.ReportOnce("ValidateCO2eType - type not allowed",
						new ArgumentException($"Type '{type}' is not a valid CO2e type for the JobCO2eCollection of {parent.GetType().Name}."));
				}
			}
			else if (!string.IsNullOrEmpty(type))
			{
				ErrorReporter.ReportOnce("ValidateCO2eType - type not allowed",
					new ArgumentException($"Type '{type}' is not allowed. Only the default type is acceptable for the JobCO2eCollection of {parent.GetType().Name} without a CO2eTypes attribute."));
			}
		}

		public static bool IsCountryEmptyWithValidCityOrPostcode(this ICO2eParent supporter, ISupportWebAddressValidation address)
		{
			if (address is null)
			{
				return false;
			}

			var isCountryMissing = address.Country == null;
			var hasValidLocation = !string.IsNullOrWhiteSpace(address.City) || !string.IsNullOrWhiteSpace(address.Postcode);

			return isCountryMissing && hasValidLocation;
		}
	}
}
