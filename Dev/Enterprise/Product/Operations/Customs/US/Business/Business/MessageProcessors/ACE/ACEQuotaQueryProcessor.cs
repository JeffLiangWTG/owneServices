using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse)]
	public class ACEQuotaQueryProcessor : ACEABIWithDatabaseLockProcessor
	{
		public override void Process()
		{
			var emailBody = new StringBuilder();
			emailBody.Append("<b>ACE Quota Query Result</b>");
			emailBody.Append("<br />");
			emailBody.Append("<br />");

			ProcessCore(emailBody);

			var jobNumber = "Unknown";
			var uri = "";
			var branch = GlbBranch.CurrentBranch;

			var originalMessage = Message.OriginalMessage;
			var sentDate = originalMessage != null ? originalMessage.EM_SystemCreateTimeUtc : ZDateTime.Empty;

			var jobDeclarationLinked = OriginalMessageLinker.Link<JobDeclaration>(Message);
			if (jobDeclarationLinked != null)
			{
				jobNumber = jobDeclarationLinked.JobNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(jobDeclarationLinked);
				branch = jobDeclarationLinked.Branch;
			}
			else if (originalMessage != null)
			{
				jobNumber = originalMessage.EM_MessageNum;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(originalMessage);
				branch = originalMessage.Branch;
			}

			emailBody.Append("<br />");
			emailBody.Append("<b>Query was sent on: </b>" + sentDate.ToString());
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "ACE Query for Quota", emailBody.ToString(), false, branch, jobDeclarationLinked);
		}

		protected void ProcessCore(StringBuilder emailBody)
		{
			AQTAQ2 q2 = null;
			AQTAQ3 q3 = null;
			foreach (MessageBlock block in messageBlocks)
			{
				if (block.MandatoryCharacters == "Q2")
				{
					q2 = (AQTAQ2)block;
				}
				else if (block.MandatoryCharacters == "Q3")
				{
					q3 = (AQTAQ3)block;
				}
				else if (block.MandatoryCharacters == "Q4")
				{
					ProcessBlocks(q2, q3, (AQTAQ4)block, emailBody);
				}
				else if (block.MandatoryCharacters == "Q5")
				{
					ProcessErrorBlock((AQTAQ5)block, emailBody);
				}
			}
		}

		void ProcessBlocks(AQTAQ2 q2, AQTAQ3 q3, AQTAQ4 q4, StringBuilder emailBody)
		{
			var appliedToMultipleCountries = q2.CountryOfOrigin == ApplyToMultipleCountriesCode;
			var countryOfOrigin = appliedToMultipleCountries ? ZString.Empty : q2.CountryOfOrigin;

			var quota = new USCQuota.Loader(Factory).Load(q2.QuotaQueryID, countryOfOrigin, q2.QuotaID, "", q2.BeginningPeriodDate, q2.EndingPeriodDate);
			if (quota == null)
			{
				quota = Factory.New<USCQuota>();

				quota.UT_Code = q2.QuotaQueryID;
				quota.UT_UC_NKOriginCountry = countryOfOrigin;
				quota.UT_BeginDate = q2.BeginningPeriodDate;
				quota.UT_EndDate = q2.EndingPeriodDate;
				quota.UT_FirstNamesake = q2.QuotaID;
			}

			quota.UT_QuotaType = q2.QuotaQueryIDTypeCode == QueryTypeList.Codes.TextileCategoryNumber ? QuotaTypeList.Codes.TextileCategoryNumber : QuotaTypeList.Codes.TariffNumber;
			quota.UT_QuotaLimit = q2.QuotaLimit;
			quota.UT_QuotaUQ = q2.UnitOfMeasureCode;
			quota.UT_TextileConversionFactor = q2.TextileConversionFactor;
			quota.UT_QuotaPeriod = q2.QuotaPeriod.Right(2);
			quota.UT_ThresholdQty = q2.ThresholdQuantity;

			ZString periodProcessDateIndicatorCode = q3.PeriodProcessingIndicator.SubstringSafe(0, 2) == "PD" ? PeriodProcessingDateIndicatorList.Codes.PresentationDate : PeriodProcessingDateIndicatorList.Codes.ExportDate;
			quota.UT_PeriodProcessDateIndicator = periodProcessDateIndicatorCode;

			var quotaStatus = GetQuotaStatus(q3.PeriodProcessingIndicator.SubstringSafe(2, 4));
			quota.UT_QuotaStatus = quotaStatus;
			quota.UT_QuotaLimitType = q3.QuotaType;
			quota.UT_QtyToDate = q3.QuantityToDate;
			quota.UT_LastTrasactionDate = q4.LastQuotaTransactionDate;

			var lastUpdateDate = new ZDateTime(q4.DateOfStatus.Year, q4.DateOfStatus.Month, q4.DateOfStatus.Day, ZInt.ParseEmptyAsZero(q4.TimeOfStatus.Left(2)), ZInt.ParseEmptyAsZero(q4.TimeOfStatus.Right(2)), 0);
			quota.UT_LastUpdateDate = lastUpdateDate;

			quota.UT_SecondTariffNo = q3.SecondTariffNumber;
			var percentageFull = ZDecimal.Zero;
			if (q2.QuotaLimit != 0)
			{
				percentageFull = new ZDecimal(q3.QuantityToDate / q2.QuotaLimit).Round(2);
			}

			if (q2.QuotaQueryIDTypeCode == QueryTypeList.Codes.TextileCategoryNumber)
			{
				emailBody.Append("<b>Textile Category Number: " + q2.QuotaQueryID + "</b>");
			}
			else
			{
				var formattedTariff = new TariffFormatter().DisplayFormat(q2.QuotaQueryID);
				emailBody.Append("<b>Tariff Number: " + formattedTariff + "</b>");
			}
			emailBody.Append("<br />");
			emailBody.Append("<br />");

			var countryOfOriginPresentation = appliedToMultipleCountries ? ApplyToMultipleCountriesDescription : q2.CountryOfOrigin.ToString();
			var htmlTableRow1 = new HtmlTableCreator(new string[] { "Beginning Period Date", "Ending Period Date", "Description", "Country of Origin", "Quota Period", "Quota Limit", "UQ", "Quantity to Date", "Percentage Full", "Threshold Quantity" });
			htmlTableRow1.WriteRow(q2.BeginningPeriodDate, q2.EndingPeriodDate, q3.Description, countryOfOriginPresentation, q2.QuotaPeriod.Right(2), q2.QuotaLimit.ToStringTrimZeros("N"), q2.UnitOfMeasureCode, q3.QuantityToDate.ToStringTrimZeros("N"), percentageFull, q2.ThresholdQuantity.ToStringTrimZeros("N"));

			var quotaType = !q3.QuotaType.IsEmpty ? q3.QuotaType + " - " + GetQuotaTypeDescription(q3.QuotaType) : "";
			var processIndicator = !periodProcessDateIndicatorCode.IsEmpty ? periodProcessDateIndicatorCode + " - " + Factory.GetCachedValue<PeriodProcessingDateIndicatorList>().GetDescriptionFromCode(periodProcessDateIndicatorCode) : "";

			var descr = GetQuotaDescription(q2.QuotaID.SubstringSafe(0, 2));
			var cat = q2.QuotaID.SubstringSafe(2, 3);
			var partCategory = q2.QuotaID.SubstringSafe(5, 2);
			var quotaIDInPresentationFormat = descr + (!cat.IsEmpty ? " Cat:" + cat : "") + (!partCategory.IsEmpty ? " Part Cat:" + partCategory : "");

			var quotaStatusDescription = GetQuotaStatusDescription(q3.PeriodProcessingIndicator.SubstringSafe(2, 4));
			var htmlTableRow2 = new HtmlTableCreator(new string[] { "Record Number", "Quota ID", "Second Tariff Number", "Textile Conversion Factor", "Period Processing Indicator", "Quota Status", "Quota Type", "Last Quota Transaction Date", "Date of Status" });
			htmlTableRow2.WriteRow(q3.RecordNumber, quotaIDInPresentationFormat, q3.SecondTariffNumber, q2.TextileConversionFactor, processIndicator, quotaStatusDescription, quotaType, q4.LastQuotaTransactionDate, lastUpdateDate);

			emailBody.Append(htmlTableRow1.ToHtml());
			emailBody.Append("<br />");

			emailBody.Append(htmlTableRow2.ToHtml());
			emailBody.Append("<br />");
		}

		void ProcessErrorBlock(AQTAQ5 q5, StringBuilder emailBody)
		{
			if (q5.QuotaQueryIDTypeCode == QueryTypeList.Codes.TextileCategoryNumber)
			{
				emailBody.Append("<b>Textile Category Number: " + q5.QuotaQueryID + "</b>");
			}
			else
			{
				var formattedTariff = new TariffFormatter().DisplayFormat(q5.QuotaQueryID);
				emailBody.Append("<b>Tariff Number: " + formattedTariff + "</b>");
			}
			emailBody.Append("<br />");
			emailBody.Append("<br />");

			var htmlTableRow = new HtmlTableCreator(new string[] { "Country of Origin", "Condition Code", "Error Text" });

			var quotaCondition = ZString.Format("{0} - {1}", q5.ConditionCode, QuotaLineStatusCodes.GetDescriptionFromCode(q5.ConditionCode));
			htmlTableRow.WriteRow(q5.CountryOfOrigin, quotaCondition, q5.NarrativeText);

			emailBody.Append(htmlTableRow.ToHtml());
			emailBody.Append("<br />");
		}

		QuotaLineStatusCodeList QuotaLineStatusCodes
		{
			get { return Factory.GetCachedValue<QuotaLineStatusCodeList>(); }
		}

		ZString GetQuotaStatus(ZString status)
		{
			switch (status)
			{
				case "OPEN":
					return "1";
				case "POTF":
					return "2";
				case "FILL":
					return "FIL";
				case "EXPD":
					return "EXP";
				case "BAND":
					return "8";
				case "HOLD":
					return "HLD";
				case "EXCL":
					return "EXC";
				default:
					return "";
			}
		}

		ZString GetQuotaStatusDescription(ZString status)
		{
			switch (status)
			{
				case "OPEN":
					return "Quota open";
				case "POTF":
					return "Quota potentially filled";
				case "FILL":
					return "Quota filled";
				case "EXPD":
					return "Quota expired";
				case "BAND":
					return "Banned";
				case "HOLD":
					return "Held";
				case "EXCL":
					return "Excluded";
				default:
					return "";
			}
		}

		ZString GetQuotaDescription(ZString code)
		{
			switch (code)
			{
				case "22":
					return "Cotton and/or man made fiber";
				case "33":
					return "Cotton";
				case "44":
					return "Wool";
				case "66":
					return "Man made fiber";
				case "88":
					return "Silk blend or non cotton vegetable fibers";
				default:
					return "";
			}
		}

		ZString GetQuotaTypeDescription(ZString code)
		{
			switch (code)
			{
				case "TRQ":
					return "Tariff-Rate Quota";
				case "TPL":
					return "Tariff Preference Quota";
				case "ABS":
					return "Absolute Quota";
				case "STA":
					return "Statistical Quota";
				default:
					return "";
			}
		}

		protected override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.Quota; }
		}

		const string ApplyToMultipleCountriesCode = "99";
		const string ApplyToMultipleCountriesDescription = "Apply to all countries";
	}
}
