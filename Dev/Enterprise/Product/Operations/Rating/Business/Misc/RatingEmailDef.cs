using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public abstract class RatingEmailDef : EmailDef
	{
		#region Rate Entries Going To Expire

		public static List<RatingEmailDef> GetEmailsForRateEntriesGoingToExpire(Dictionary<IAutoRating, IEnumerable<RateEntry>> dictionary)
		{
			var ratingEmailDefs = new List<RatingEmailDef>();

			var clientRates = GetRateEntriesByTariffOrClient(dictionary, false);
			ratingEmailDefs.AddRange(GetClientRateEntryExpiringEmails(clientRates));

			var tariffRates = GetRateEntriesByTariffOrClient(dictionary, true);
			ratingEmailDefs.AddRange(GetCombinedCompanyTariffExpiringEmails(tariffRates));

			return ratingEmailDefs;
		}

		#region Client Rate Entry Going To Expire

		static IEnumerable<RatingEmailDef> GetClientRateEntryExpiringEmails(Dictionary<IAutoRating, IEnumerable<RateEntry>> dictionary)
		{
			var results = new List<RatingEmailDef>();
			if (!dictionary.Any() || !RatingDataRegistry.Instance.ClientRateGoingToExpireNotification.Value)
			{
				return results;
			}

			var uniqueRelatedOrgs = new HashSet<OrgHeader>();
			foreach (var autoRatingAndEntries in dictionary)
			{
				var autoRatingJob = autoRatingAndEntries.Key;
				var jobNumber = GetJobNumber(autoRatingJob);

				foreach (var entry in dictionary[autoRatingJob])
				{
					if (entry.Parent != null && entry.Parent.Header != null)
					{
						uniqueRelatedOrgs.Add(entry.Parent.Header);
					}
				}

				var emailBody = GetEmailBodyWithFormattedRatesAndJobs(jobNumber, autoRatingJob, autoRatingAndEntries.Value, false);

				foreach (var relatedOrg in uniqueRelatedOrgs)
				{
					var emailSubject = Res.GetString("d2ee0d14-63ce-45b5-b5ed-5dd36e3a832e", "Client Rate {0} Expiring Soon", relatedOrg.OH_FullNameTruncated);

					var email = new RatingEmailDefWithOrganisation(autoRatingJob, relatedOrg);
					using (var languageToken = email.SwitchLanguage())
					{
						email.SetEmailContent(emailSubject, emailBody, RatingDataRegistry.Instance.ClientRateGoingToExpireNotification);
					}

					results.Add(email);
				}
			}

			return results;
		}

		#endregion

		#region Combined Company Tariffs Going To Expire

		static IEnumerable<RatingEmailDef> GetCombinedCompanyTariffExpiringEmails(Dictionary<IAutoRating, IEnumerable<RateEntry>> dictionary)
		{
			var results = new List<RatingEmailDef>();
			if (!dictionary.Any() || !RatingDataRegistry.Instance.CompanyTariffGoingToExpireNotification.Value)
			{
				return results;
			}

			var emailSubject = Res.GetString("6f4a8b44-36e1-4711-b8ab-8a64eb985aab", "Company Tariff Expiring Soon");

			foreach (var autoRatingAndEntries in dictionary)
			{
				var autoRatingJob = autoRatingAndEntries.Key;
				var jobNumber = GetJobNumber(autoRatingJob);
				var emailBody = GetEmailBodyWithFormattedRatesAndJobs(jobNumber, autoRatingJob, autoRatingAndEntries.Value, true);

				var email = new RatingEmailDefWithGroup(RatingDataRegistry.Instance.CompanyTariffNotificationEmailGroup);
				using (var languageToken = email.SwitchLanguage())
				{
					email.SetEmailContent(emailSubject, emailBody, RatingDataRegistry.Instance.CompanyTariffGoingToExpireNotification);
				}

				results.Add(email);
			}

			return results;
		}

		#endregion

		#endregion

		#region Client Rate Just Expired

		public static RatingEmailDefWithOrganisation ClientRateJustExpired(IAutoRating autoRatingJob, IRateEntry entry)
		{
			var rate = entry.ParentRatingHeader;
			if (rate == null || rate.Header == null || !RatingDataRegistry.Instance.ClientRateJustExpiredNotification.Value)
			{
				return null;
			}

			var emailSubject = Res.GetString("f5858dc3-e4b8-4ca4-9b4e-57d7df934468", "Client Rate {0} Expired", rate.Header.OH_FullNameTruncated);
			var emailBody = GetBillingJobMessage(autoRatingJob.HumanReadableNames())
				+ ". " + Res.GetString("5cadc09f-7f15-47fd-abd0-02400a295992", "Expired Client Rates were found for {0}.", rate.Header.OH_FullNameTruncated)
				+ DoubleLineBreak
				+ GetJobDetails(autoRatingJob);

			var email = new RatingEmailDefWithOrganisation(autoRatingJob, rate.Header);
			using (var languageToken = email.SwitchLanguage())
			{
				email.SetEmailContent(emailSubject, emailBody, RatingDataRegistry.Instance.ClientRateJustExpiredNotification);
			}

			return email;
		}

		#endregion

		#region Company Tariff Just Expired

		public static RatingEmailDefWithGroup CompanyTariffJustExpired(IAutoRating autoRatingInfo, IRateEntry entry)
		{
			var rate = entry.ParentRatingHeader;
			if (rate == null || !RatingDataRegistry.Instance.CompanyTariffJustExpiredNotification.Value)
			{
				return null;
			}

			var emailSubject = Res.GetString("2e4992ab-2acc-4083-b8e2-075b56f08c4b", "Company Tariff Expired");
			var emailBody = GetBillingJobMessage(autoRatingInfo.HumanReadableNames())
				+ ". " + Res.GetString("e3c2ea35-fd4c-497e-823a-a7ebfad37bb2", "Expired Company Tariff Rates were found.")
				+ DoubleLineBreak
				+ GetJobDetails(autoRatingInfo);

			var email = new RatingEmailDefWithGroup(RatingDataRegistry.Instance.CompanyTariffNotificationEmailGroup);
			using (var languageToken = email.SwitchLanguage())
			{
				email.SetEmailContent(emailSubject, emailBody, RatingDataRegistry.Instance.CompanyTariffJustExpiredNotification);
			}

			return email;
		}

		#endregion

		#region Client Rate Not Found

		public static RatingEmailDefWithOrganisation ClientRateNotFound(RatingCriteria autoRatingInfo, OrgHeader organisation)
		{
			if (organisation == null || !RatingDataRegistry.Instance.ClientRateNotFoundNotification.Value)
			{
				return null;
			}

			OrgRateTariffLevel.Directions direction;
			ZString rateMode;

			if (autoRatingInfo != null)
			{
				rateMode = RatingHelper.ConvertToRateModeWhenItIsFreight(autoRatingInfo.FreightMode);
				direction = OrgRateTariffLevel.GetOrgRateTariffLevelDirection(autoRatingInfo.JobDirection);
			}
			else
			{
				rateMode = Core.Constants.RateMode.ALL;
				direction = OrgRateTariffLevel.Directions.ALL;
			}

			var company = RatingHelper.GetCompany(autoRatingInfo);
			var effectiveDate = autoRatingInfo.GetEffectiveDateWithFallback(specificChargeGroup: "FRT", isCosting: _Rating.Cost).date;

			var frtTariffLevel = RatingCache.GetCompanyTariffLevel(organisation, company, RatingConstants.RateCategory.AIR, rateMode, direction, effectiveDate).level;
			var tariffLevelMessage = frtTariffLevel > 0
				? Res.GetString("be6d2798-c489-434c-a9dd-9fdd43e79a65", "This client is using Company Tariff Level {0} for Freight, but no matching rates could be located.", frtTariffLevel)
				: Res.GetString("c25d061b-8648-485a-85d7-904ea284f381", "This client is NOT using Company Tariff for Freight.");

			var emailSubject = Res.GetString("e285b75b-b818-42a7-88e0-85e455a166f1", "Client Rate for {0} Not Found", organisation.OH_FullNameTruncated);

			var emailBody = GetBillingJobMessage(autoRatingInfo.HumanReadableNames())
				+ "."
				+ DoubleLineBreak
				+ Res.GetString("48a1332b-7617-43ab-a810-851c278de5a8", "No client rate was found for {0}.", organisation.OH_FullNameTruncated)
				+ DoubleLineBreak
				+ tariffLevelMessage
				+ DoubleLineBreak
				+ GetJobDetails(autoRatingInfo);

			var email = new RatingEmailDefWithOrganisation(autoRatingInfo, organisation);
			using (var languageToken = email.SwitchLanguage())
			{
				email.SetEmailContent(emailSubject, emailBody, RatingDataRegistry.Instance.ClientRateNotFoundNotification);
			}

			return email;
		}

		#endregion

		#region Call For Pricing

		public static RatingEmailDefWithOrganisation CallForPricing(IRateLine rateLine, RatingCriteria criteria, ZString callForPricingReason)
		{
			var organisation = rateLine.ParentRateEntry.ParentRatingHeader.Header ?? criteria.LocalClient;
			if (organisation == null || !RatingDataRegistry.Instance.ClientRateNotFoundNotification.Value)
			{
				return null;
			}

			var emailSubject = Res.GetString("bc5ac538-2065-4534-9d4f-c5ac5661d25c", "Rate restrictions were encountered during autorating");
			var jobNumbers = new ZStringBuilder(criteria.HumanReadableNames()).ToStringWithDelimiterBetweenAppends(", ");

			var emailBody = Res.GetString("717f535d-9393-4568-be8b-ca42bfe6f2e1", "{0} encountered Rate Restrictions during autorating.", jobNumbers)
				+ DoubleLineBreak
				+ callForPricingReason
				+ DoubleLineBreak
				+ DescriptionHelpers.GetRateLineDescription(rateLine, criteria)
				+ DoubleLineBreak
				+ Res.GetString("7ba082d8-a7d6-47e6-89c4-a948173665a8", "Note: A Rate Restriction cancels autorating entirely for the autorated job.");

			var email = new RatingEmailDefWithOrganisation(criteria, organisation);
			using (var languageToken = email.SwitchLanguage())
			{
				email.SetEmailContent(emailSubject, emailBody, RatingDataRegistry.Instance.ClientRateNotFoundNotification);
			}

			return email;
		}

		#endregion

		#region One Off Quote Used

		public static RatingEmailDefWithOrganisation OneOffQuoteUsed(IAutoRating autoRatingInfo, IRatingHeader quote)
		{
			if (quote == null || quote.Header == null || !RatingDataRegistry.Instance.OneOffQuoteUsedNotification.Value)
			{
				return null;
			}

			var emailSubject = Res.GetString("40a5dd86-69f8-4beb-b9b7-721912bb183e", "Spot Quote {0} for client {1} Used", quote.TH_QuoteNumber, quote.Header.OH_FullNameTruncated);
			var emailBody = Res.GetString("0856b02d-33b3-46c3-bfeb-5939ea88500a", "{0} using matching Spot Quotation {1}.", GetBillingJobMessage(autoRatingInfo.HumanReadableNames()), quote.TH_QuoteNumber)
				+ DoubleLineBreak
				+ Res.GetString("ae624bd4-d50f-40cf-b9a7-57c334352f57", "This spot quote has now been marked as USED and cannot be used for any further billing jobs.")
				+ DoubleLineBreak
				+ GetJobDetails(autoRatingInfo);

			var email = new RatingEmailDefWithOrganisation(autoRatingInfo, quote.Header);
			using (var languageToken = email.SwitchLanguage())
			{
				email.SetEmailContent(emailSubject, emailBody, RatingDataRegistry.Instance.OneOffQuoteUsedNotification);
			}

			return email;
		}

		#endregion

		#region One Off Web Quote

		public static RatingEmailDefWithOrganisation WebOneOffQuote(IAutoRating autoRatingInfo, RatingHeader quote, OrgContact contact)
		{
			var email = new RatingEmailDefWithOrganisation(autoRatingInfo, quote.Header);
			using (var languageToken = email.SwitchLanguage())
			{
				email.SetEmailContent(
					Res.GetString("7b86f4c6-c400-4ad6-adcc-eee60e2ca6c0", "Web Spot Quote {0} for client {1}", quote.TH_QuoteNumber, quote.Header.OH_FullNameTruncated),
					Res.GetString("cf756bef-6372-439a-9dd7-11bdfb6e4d7f", @"Spot Quote {0} was requested on the web by {1} contact {2} [{3}].
The generated quotation has been locked and an email has been sent to the client with a copy of the quotation document attached. The quotation document has also been stored on the eDocs tab for quotation {4}.
This quotation has no followup date set.", quote.TH_QuoteNumber, quote.Header.OH_FullNameTruncated, contact.OC_ContactName, contact.OC_Email, quote.TH_QuoteNumber), null);
			}
			return email;
		}

		#endregion

		#region Implementation

		#region GetRateEntriesByTariffOrClient

		static Dictionary<IAutoRating, IEnumerable<RateEntry>> GetRateEntriesByTariffOrClient(Dictionary<IAutoRating, IEnumerable<RateEntry>> dictionary, bool isGlobalTariff)
		{
			var result = new Dictionary<IAutoRating, IEnumerable<RateEntry>>();

			foreach (var job in dictionary.Keys)
			{
				if (dictionary[job].Any(x => x.IsCompanyTariff() == isGlobalTariff))
				{
					result.Add(job, dictionary[job].Where(x => x.IsCompanyTariff() == isGlobalTariff));
				}
			}

			return result;
		}

		#endregion

		#region Email Body Components

		static ZString GetEmailBodyWithFormattedRatesAndJobs(ZString jobNumber, IAutoRating autoRatingJob, IEnumerable<RateEntry> rateEntries, bool isGlobalTariff)
		{
			var rateEntryType = isGlobalTariff
				? Res.GetString("1c3931c3-0ce4-4803-ba67-f5efd7ac6bb8", "Company Tariff")
				: Res.GetString("d4291e3b-86ab-45f2-9a4f-850a32d425c6", "Client Rate");

			var result = Res.GetString("35f0f1cb-920e-4fbb-884d-e207912d511e",
				"{0} was autorated by {1} using {2}(s) that will expire soon:",
				jobNumber,
				GlbStaff.CurrentUser.GS_FullName,
				rateEntryType);

			foreach (var entry in rateEntries)
			{
				var clientName = string.Empty;
				if (!entry.IsCompanyTariff() && entry.Parent != null && entry.Parent.Header != null)
				{
					clientName = " " + entry.Parent.Header.OH_FullNameTruncated;
				}

				result += System.Environment.NewLine;
				result += "\t";
				result += Res.GetString("4ae52ab3-f505-4e26-a380-c3477fbb2f1b", "• {0}{1} expires on {2}", rateEntryType, clientName, entry.TI_RateEndDate.ToString("d"));
			}

			result += DoubleLineBreak;
			result += GetJobDetails(autoRatingJob);

			return result;
		}

		static string GetJobNumber(IAutoRating autoRatingJob)
		{
			var jobNumber = "";

			foreach (var bizObj in autoRatingJob.AutoRatedFor)
			{
				if (!bizObj.HumanReadableName.IsEmpty)
				{
					jobNumber = bizObj.HumanReadableName;
					break;
				}
			}

			return jobNumber;
		}

		static string DoubleLineBreak
		{
			get { return System.Environment.NewLine + System.Environment.NewLine; }
		}

		protected void SetEmailContent(string subject, string body, RegistryItemWrapper registryItem)
		{
			Subject = subject;
			Body = body;
			Body += "\r\n\r\n" + Res.GetString("4dd97a40-c328-40d6-a33b-b7c9e77700ab", "This message was automatically generated by {0}.", Core.Constants.ProductName);

			if (registryItem != null)
			{
				Body += " " + Res.GetString("3d18e3a1-dbf1-4c9a-858d-669d7d3c0b10", "To configure this email, please go to the registry item at Autorating --> Notifications --> {0}.", registryItem.Caption);
			}
		}

		#endregion

		protected abstract bool SendCore(BusinessObjectFactory factory);

		/// <summary>
		/// Send.
		/// If a factory is given, the email will be created in that factory and caller is responsible for calling Save.
		/// If no factory, the email will be saved in a new factory.
		/// </summary>
		public bool Send(BusinessObjectFactory factory = null)
		{
			if (!IsActive)
			{
				return false;
			}

			var result = false;
			try
			{
				result = SendCore(factory);
			}
			catch (EmailSendFailedException)
			{
			}

			return result;
		}

		public bool IsActive
		{
			get { return fIsActive; }
			protected set { fIsActive = value; }
		}

		bool fIsActive;

		#region Get Billing Job Message

		static string GetBillingJobMessage(ZString[] jobs)
		{
			if (jobs.Length > 1)
			{
				return Res.GetString("8917a269-5452-4741-932c-069a91ce6edd", "Billing jobs {0} were autorated by user {1}", new ZStringBuilder(jobs).ToStringWithDelimiterBetweenAppends(", "), GlbStaff.CurrentUser.GS_FullName);
			}
			if (jobs.Length == 1)
			{
				return Res.GetString("5b339fce-a7f5-4033-b660-7cb1983eb799", "Billing job {0} was autorated by user {1}", jobs[0], GlbStaff.CurrentUser.GS_FullName);
			}

			return Res.GetString("775534b8-68fa-479b-bcca-d70cb85423ed", "Billing job was autorated by user {0}", GlbStaff.CurrentUser.GS_FullName);
		}

		#endregion

		#endregion

		#region GetJobDetails

		static string GetJobDetails(IAutoRating info)
		{
			var result = new ZStringBuilder(Res.GetString("1314b48d-12d6-42bb-8b77-ea8015d02257", "Job Details:"));
			result.AppendLine();

			var origin = info.Origin?.Code ?? ZString.Empty;
			result.Append((NoResString)"Origin:\t");
			result.AppendLine(origin);

			var destination = info.Destination?.Code ?? ZString.Empty;
			result.Append((NoResString)"Destination:\t");
			result.AppendLine(destination);

			if (info.DebtorOrgs[RatingDebtorOrgTypes.CNE] != null)
			{
				result.Append((NoResString)"Consignee:\t");
				result.AppendLine(info.DebtorOrgs[RatingDebtorOrgTypes.CNE].OH_FullNameTruncated);
			}

			if (info.DebtorOrgs[RatingDebtorOrgTypes.CNR] != null)
			{
				result.Append((NoResString)"Consignor:\t");
				result.AppendLine(info.DebtorOrgs[RatingDebtorOrgTypes.CNR].OH_FullNameTruncated);
			}

			var departureDate = info.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate);
			if (departureDate.IsValid && !departureDate.IsEmpty)
			{
				result.AppendLine(Res.GetString("9cebd8c9-7614-4c69-940c-97f6182030fe", "Departure Date:\t{0}", departureDate.ToDateTime().ToShortDateString()));
			}

			var arrivalDate = info.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate);
			if (arrivalDate.IsValid && !arrivalDate.IsEmpty)
			{
				result.AppendLine(Res.GetString("2f8b9984-434e-4641-94c4-d7d17929ac6c", "Arrival Date:\t{0}", arrivalDate.ToDateTime().ToShortDateString()));
			}

			if (info.FreightMode != FreightMode.UKN)
			{
				result.Append((NoResString)"Mode:\t");
				result.AppendLine(info.FreightMode.ToString());
			}

			if ((info.FreightMode & FreightMode.Containerised) != 0)
			{
				result.Append(Res.GetString("98d99ae9-5fdc-477c-92ff-38e444d56466", "Outer Packs:"));
				result.Append("\t");
				result.AppendLine(GetMeasureDescription(info, MeasureType.Package));
			}
			else
			{
				result.Append((NoResString)"Weight:\t");
				result.AppendLine(GetMeasureDescription(info, MeasureType.Weight));
				result.Append((NoResString)"Volume:\t");
				result.Append(GetMeasureDescription(info, MeasureType.Volume));
			}

			return result.ToString();
		}

		static ZString GetMeasureDescription(IAutoRating autoRatingInfo, MeasureType measureType)
		{
			var result = ZString.Empty;
			var measures = (RateableMeasureSet)autoRatingInfo.RateableMeasures;
			if (measures.HasMeasureType(measureType))
			{
				result = measureType == MeasureType.Package
					? measures.ValueString(measureType)
					: measures.GetActual(measureType) + " " + measures.GetUnit(measureType);
			}

			return result;
		}

		#endregion

		#region Language

		protected RatingEmailLanguageToken SwitchLanguage()
		{
			return new RatingEmailLanguageToken(language.IsEmpty ? Res.DefaultLanguage : (string)language);
		}

		protected void AddRecipientLanguage(ZString recipientLanguage)
		{
			if (language.IsEmpty)
			{
				language = recipientLanguage;
			}
			else if (recipientLanguage != language)
			{
				language = Res.DefaultLanguage;
			}
		}
		ZString language;

		protected class RatingEmailLanguageToken : Disposable
		{
			public RatingEmailLanguageToken(string language)
			{
				languageChange = Res.TemporarilySwitchLanguage(language);
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					languageChange.Dispose();
				}
			}

			readonly IDisposable languageChange;
		}

		#endregion
	}
}

