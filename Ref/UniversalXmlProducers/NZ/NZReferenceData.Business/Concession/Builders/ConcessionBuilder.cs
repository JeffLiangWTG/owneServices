using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using static CargoWise.RefDbRepo.NZReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	internal sealed class ConcessionBuilder : IBuilder<NZConcessionProcessingData>
	{
		static int DescriptionMaxLength = 4000;

		public ConcessionBuilder(IDateProvider dateProvider, ILogger logger, IConcessionFileReader fileReader = null, IEmailService emailService = null)
		{
			DateProvider = dateProvider;
			FileReader = fileReader ?? new ConcessionFileReader();
			EmailService = emailService ?? new EmailService();
			Logger = logger;
		}

		IConcessionFileReader FileReader { get; }
		IEmailService EmailService { get; }
		ILogger Logger { get; }
		IDateProvider DateProvider { get; }

		public bool Build(IDataRepo dataRepo, BuildersFilePath[] filePaths, NZConcessionProcessingData processingData)
		{
			var concessionDetailFilePath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.ConcessionDetail);
			var concessionRateFilePath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.ConcessionRate);
			var concessionToTariffFilePath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.ConcessionToTariff);
			var consolidatedListOfApprovalsJsonFilePath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.ConsolidatedListOfApprovalsJson);
			if (concessionToTariffFilePath == null || concessionDetailFilePath == null || concessionRateFilePath == null || consolidatedListOfApprovalsJsonFilePath == null)
			{
				throw new ArgumentException($"ConcessionBuilder needs to have exactly 1 file path for each concession symbol");
			}
			Logger.LogInfo("Start processing Concession file...");

			var concessionDetailLines = FileReader.ReadAllLines(concessionDetailFilePath.FilePath).Skip(1);
			var concessionRateLines = FileReader.ReadAllLines(concessionRateFilePath.FilePath).Skip(1);
			var concessionToTariffLines = FileReader.ReadAllLines(concessionToTariffFilePath.FilePath).Skip(1);
			var consolidatedListOfApprovalsJsonLines = FileReader.ReadAllLines(consolidatedListOfApprovalsJsonFilePath.FilePath);

			var concessionDetails = ConcessionDetails.GetConcessionDetails(concessionDetailLines, Logger, DateProvider);
			var concessionRates = ConcessionRates.GetConcessionRates(concessionRateLines, Logger, DateProvider);
			var concessionToTariffs = ConcessionToTariff.GetConcessionToTariffs(concessionToTariffLines, Logger).ToLookup(x => x.Code, x => x);
			var consolidatedListOfApprovals = ConsolidatedListOfApproval.GetConsolidatedListOfApprovalsFromJson(consolidatedListOfApprovalsJsonLines, Logger);

			var concessionOverrideFilePath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.ConcessionOverride)?.FilePath;
			var descriptionOverrides = CodeDescription.GetCodeDescriptionsFromJsonPath(concessionOverrideFilePath, Logger);

			try
			{
				var repo = (ITopLevelDataRepo<RefCusTariff>)dataRepo;
				var truncatedDescriptionConcessionCodes = new List<string>();

				foreach (var concessionDetailKV in concessionDetails)
				{
					var concessionDetail = concessionDetailKV.Value;
					var concessionCode = concessionDetail.Code;

					var descriptionSource =
						descriptionOverrides.TryGetValue(concessionCode, out var codeDescription) ? codeDescription.Description :
						consolidatedListOfApprovals.TryGetValue(concessionCode, out var approval) ? approval.description :
						null;
					var description = CleanDescription(descriptionSource, out var isDescriptionTruncated);

					if (string.IsNullOrEmpty(description))
					{
						Logger.LogError($"{concessionCode}: Cannot find description");
						continue;
					}

					if (isDescriptionTruncated)
					{
						truncatedDescriptionConcessionCodes.Add(concessionCode);
						Logger.LogError($"{concessionCode}: Description truncated (exceeds {DescriptionMaxLength} chars)");
					}

					var tariff = GetNewRefCusTariff(concessionDetail, description);
					dataRepo.Add(concessionDetail.Code, tariff);

					if (concessionRates.Contains(concessionCode))
					{
						var rates = concessionRates[concessionCode].ToArray();
						CreateRate(tariff, rates, concessionDetail);
					}
					else
					{
						Logger.LogError($"{concessionCode}: Cannot find an active rate");
					}

					if (concessionToTariffs.Contains(concessionCode))
					{
						var toTariffs = concessionToTariffs[concessionCode].ToArray();
						CreateOrUpdateRelationship(tariff, toTariffs);
					}
					else
					{
						Logger.LogError($"{concessionCode}: Cannot find relationship");
					}
				}

				NotifyTruncatedDescriptions(truncatedDescriptionConcessionCodes);
			}
			catch (RefDataParseException e)
			{
				Logger.LogError(e.Message);
				return false;
			}

			return true;
		}

		internal static RefCusTariff GetNewRefCusTariff(ConcessionDetails concessionDetails, string description)
		{
			var tariff = new RefCusTariff();
			tariff.ZZ1_TariffCode = concessionDetails.Code;
			tariff.ZZ1_StartDate = concessionDetails.StartDate;
			tariff.ZZ1_EndDate = concessionDetails.EndDate;
			tariff.ZZ1_Description = description;

			return tariff;
		}

		internal static void CreateRate(RefCusTariff tariff, IEnumerable<ConcessionRates> rates, ConcessionDetails details)
		{
			var currentRates = tariff.RefCusRates?.ToList() ?? new List<RefCusRate>();

			foreach (var rate in rates)
			{
				if (tariff.ZZ1_StartDate < rate.ExpiryDate)
				{
					var formula = string.Format(CultureInfo.InvariantCulture, rate.Formula, string.Empty);

					var applicability = new RefCusApplicability
					{
						ZZT_StartDate = details.StartDate,
						ZZT_EndDate = rate.ExpiryDate,
						ZZT_ZZA_NKTradeGroup = rate.RateGroup,
						ZZT_OrderNumber = rate.Code
					};

					if (rate.FormulaNumber == 2)
					{
						applicability.ZZT_AdditionalCode = ApplicabilityAdditionalCodes.IsManual;
					}

					var cusRate = new RefCusRate
					{
						ZZ2_StartDate = details.StartDate,
						ZZ2_EndDate = rate.ExpiryDate,
						ZZ2_RateFormula = formula,
						ZZ2_ZZS_NKPreference = rate.RateGroup,
						RefCusApplicabilities = [applicability]
					};

					currentRates.Add(cusRate);
				}
			}

			tariff.RefCusRates = currentRates.ToArray();
		}

		internal void CreateOrUpdateRelationship(RefCusTariff tariff, ConcessionToTariff[] concessionToTariffs)
		{
			bool shouldCreateRelationshipIndividually = concessionToTariffs.Length < Helper.SectionToChapters.Count;
			bool hasRowWithLevel = false;

			if (!shouldCreateRelationshipIndividually)
			{
				bool[] sectionNotHasLevelValue = new bool[21];
				foreach (var concessionToTariff in concessionToTariffs)
				{
					if (string.IsNullOrEmpty(concessionToTariff.Tariff))
					{
						sectionNotHasLevelValue[concessionToTariff.Section - 1] = true;
					}
					else
					{
						hasRowWithLevel = true;
					}
				}
				shouldCreateRelationshipIndividually = !sectionNotHasLevelValue.All(x => x);
			}

			List<RefCusTariffRelationship> relationships = new List<RefCusTariffRelationship>();

			if (!shouldCreateRelationshipIndividually && hasRowWithLevel)
			{
				Logger.LogError($"{tariff.ZZ1_TariffCode} has 21 ConcessionToTariffs which Tariff both are empty, but it also has another ConcessionToTariff which Tariff Level is not empty");
			}
			else if (shouldCreateRelationshipIndividually)
			{
				foreach (var concessionToTariff in concessionToTariffs)
				{
					if (string.IsNullOrEmpty(concessionToTariff.Tariff))
					{
						foreach (var chapter in concessionToTariff.Chapters)
						{
							relationships.Add(new RefCusTariffRelationship()
							{
								ZZH_TariffCode = chapter
							});
						}
					}
					else
					{
						relationships.Add(new RefCusTariffRelationship()
						{
							ZZH_TariffCode = concessionToTariff.Tariff
						});
					}
				}
			}
			else
			{
				relationships.Add(new RefCusTariffRelationship()
				{
					ZZH_TariffCode = string.Empty
				});
			}

			tariff.RefCusTariffRelationships = relationships.ToArray();
		}

		private static string CleanDescription(string description, out bool isTruncated)
		{
			isTruncated = false;
			return !string.IsNullOrEmpty(description)
				? WebUtility.HtmlDecode(description)
					.RemoveHtmlTags()
					.RemoveHtmlEntitiesExcludingIllegalBodyXmlChars()
					.EscapeBodyXmlCharacters()
					.Truncate(DescriptionMaxLength, out isTruncated)
				: description;
		}

		private void NotifyTruncatedDescriptions(List<string> truncatedConcessionCodes)
		{
			if (truncatedConcessionCodes.Count == 0)
				return;

			const string subject = "NZ RefCusTariffConcession Truncated Descriptions";
			var body = $"The following concession codes have truncated descriptions:\n{string.Join("\n", truncatedConcessionCodes)}";
			EmailService.SendEmail(subject, body, false);
		}
	}
}
