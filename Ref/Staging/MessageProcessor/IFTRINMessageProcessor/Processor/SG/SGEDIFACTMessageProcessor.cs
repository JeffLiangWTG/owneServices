using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.IFTRIN;
using Enterprise.Edifact.D09B.Segments;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor
{
	public class SGEDIFACTMessageProcessor : BaseMessageProcessor
	{
		public SGEDIFACTMessageProcessor(string outputPath)
			: base(DataSourceConstants.Country.Singapore, outputPath)
		{
		}

		#region Process

		protected override (bool success, DateTime issueDate) ProcessCore(string messageText)
		{
			var success = false;
			var issueDate = DateTime.MinValue;
			int uNHIndex = messageText.IndexOf("UNH", StringComparison.Ordinal);
			if (uNHIndex > 0)
			{
				messageText = messageText.Substring(uNHIndex).Replace("\n", "").Replace("\r", "").Replace("\t", "");
			}

			var iftrin = (IFTRINMessage)MessageFactory.GetMessage(CharacterSet, messageText);
			if (iftrin != null)
			{
				issueDate = GetIssueDate(iftrin);

				ProcessIFTRINData(iftrin);
				success = true;
			}

			return (success, issueDate);
		}

		void ProcessIFTRINData(IFTRINMessage iftrin)
		{
			Argument.NotNull(iftrin, nameof(iftrin));

			var dtm = iftrin.Group1.OfType<SegmentGroup1>().FirstOrDefault()?.DTM;
			var startDate = GetDate(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.StartDateTime, DateFormat);
			var endDate = GetDate(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.EndDateTime, DateFormat);

			foreach (SegmentGroup5 sG5 in iftrin.Group5)
			{
				var cux = sG5.CUX.OfType<CUXSegment>().FirstOrDefault();

				if (cux != null)
				{
					var currencyExchangeRate = Convert.ToDecimal(cux.CurrencyExchangeRate, CultureInfo.InvariantCulture) / Convert.ToDecimal(cux.CurrencyDetails1.CurrencyRate, CultureInfo.InvariantCulture);
					AddExchangeRate(cux.CurrencyDetails1.CurrencyIdentificationCode, currencyExchangeRate, startDate, endDate);
				}
			}
		}

		#endregion

		#region Get Date

		static DateTime GetIssueDate(IFTRINMessage iftrin)
		{
			Argument.NotNull(iftrin, nameof(iftrin));

			var referenceGroup = iftrin.Group1.OfType<SegmentGroup1>().FirstOrDefault();
			var issueDate = GetDate(referenceGroup?.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.IssueDate, DateFormat);

			if (issueDate == DateTime.MinValue)
			{
				issueDate = GetDate(iftrin.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.IssueDate, DateFormat);
			}

			return issueDate;
		}

		static DateTime GetDate(DTMSegmentMessageSection section, string qualifier, string dateFormat)
		{
			var result = DateTime.MinValue;
			return !(section?.OfType<DTMSegment>().FirstOrDefault(x => x.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == qualifier)?.DateTimePeriod.DateOrTimeOrPeriodText.TryParseExact(out result, dateFormat) ?? false) ? DateTime.MinValue : result;
		}

		#endregion

		#region Implement

		UNOACharacterSet CharacterSet => characterSet ?? (characterSet = new UNOASGCharacterSet());
		UNOACharacterSet characterSet;

		MessageFactory MessageFactory
		{
			get
			{
				if (messageFactory == null)
				{
					messageFactory = new MessageFactory(new Enterprise.Edifact.D09B.EdifactD09BMessageFactory());
				}
				return messageFactory;
			}
		}
		MessageFactory messageFactory;

		#endregion
	}
}
