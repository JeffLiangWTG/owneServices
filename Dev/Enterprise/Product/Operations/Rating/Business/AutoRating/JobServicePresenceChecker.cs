using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class JobServicePresenceChecker
	{
		public void CombineRatingResults(IAutoRating itemToRate, IEnumerable<AutoRateInfo> rateResults)
		{
			var servicesCollection = itemToRate.JobServices;
			if (servicesCollection != null)
			{
				services.AddRange(servicesCollection.Where(serviceInfo => serviceInfo.IsEnabled && serviceInfo.Rate.IsEmpty && serviceInfo.TotalCost.IsEmpty));
			}

			chargeCodes.AddRange(rateResults.Select(x => x.ChargeCode).Where(x => x != null).Distinct());
		}

		internal JobServicesCollection ServicesToCheck { get => services; }

		readonly JobServicesCollection services = new JobServicesCollection();
		readonly List<AccChargeCode> chargeCodes = new List<AccChargeCode>();

		public string GetMissingJobServicesNotification(ZString jobName)
		{
			var missingServices = new JobServicesCollection();
			var matchedServices = new List<KeyValuePair<JobServiceInfo, AccChargeCode>>();

			foreach (var serviceInfo in services)
			{
				var matchedChargeCode = chargeCodes.FirstOrDefault(code => serviceInfo.IsServiceFor(code.AC_ChargeGroup, code.AC_ChargeSubGroup));
				if (matchedChargeCode != null)
				{
					matchedServices.Add(new KeyValuePair<JobServiceInfo, AccChargeCode>(serviceInfo, matchedChargeCode));
				}
				else
				{
					missingServices.Add(serviceInfo);
				}
			}

			var missingServicesNotification = new ZStringBuilder();
			var matchedServicesNotification = new ZStringBuilder();

			foreach (var missingService in missingServices)
			{
				var matchFound = false;

				foreach (var matchedPair in matchedServices)
				{
					if (missingService.ServiceCode == matchedPair.Key.ServiceCode)
					{
						matchFound = true;
						matchedServicesNotification.AppendIfNotEmpty(BuildMatchedNotification(missingService, matchedPair.Value));
						break;
					}
				}

				if (!matchFound)
				{
					var serviceDescription = "  " + Res.GetString("d7aae4bd-3ec7-450f-a9a6-939b8730b0ea",
						"{0} {1}: {2} / {3} for {4}",
						RatingConstants.BulletPoint,
						missingService.ServiceDescription,
						missingService.ChargeCodeGroup,
						missingService.ServiceCode,
						jobName);

					missingServicesNotification.Append(serviceDescription);
				}
			}

			var notification = new ZStringBuilder();

			if (!missingServicesNotification.IsEmpty)
			{
				notification.Append(Res.GetString("7d5a84f8-cf35-4d72-ae21-78b2f8021223", "Rates for the below job services were not found. Please either create these charge codes, or ensure that your rate contains these charges and have the correct commodity code, service level and validity dates.\r\nUntil you do this, these charges will not be rated.") + System.Environment.NewLine);
				notification.Append(missingServicesNotification.ToStringWithNewLineBetweenAppends());
			}

			if (!matchedServicesNotification.IsEmpty)
			{
				notification.Append(Res.GetString("262b44fe-f44a-4ba8-99ce-ab5706eea2bc", "Rates for the below job services were not matched for exact charge group/service type pairs. These job services were still matched and rated during this Autorating process as shown below:") + System.Environment.NewLine);
				notification.Append(matchedServicesNotification.ToStringWithNewLineBetweenAppends());
			}

			return notification.ToStringWithNewLineBetweenAppends();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		ZString BuildMatchedNotification(JobServiceInfo missingService, AccChargeCode matchedChargeCode)
		{
			var result = new ZStringBuilder();

			result.Append(Res.GetString("4bf32ae1-71df-4510-bbaf-b3a9ada6e0db", "{0} {1} ({2}) service", RatingConstants.BulletPoint, missingService.ServiceCode, missingService.ServiceDescription));

			if (!missingService.ChargeCodeGroup.IsEmpty)
			{
				result.Append(" " + Res.GetString("07e83819-f0af-401f-9a0c-058198fc8e49", "for {0} charge group", missingService.ChargeCodeGroup));
			}

			var chargeDescription = matchedChargeCode.AC_Desc.IsEmpty ? ZString.Empty : ZString.Format(" ({0})", matchedChargeCode.AC_Desc);

			result.Append(" " + Res.GetString("78069f21-374a-45b0-bdda-7770b02aa41d", "has not been matched exactly but still has been rated for another charge group match and {0}{1} charge code was applied.", matchedChargeCode.AC_Code, chargeDescription));

			return result.ToString();
		}
	}
}

