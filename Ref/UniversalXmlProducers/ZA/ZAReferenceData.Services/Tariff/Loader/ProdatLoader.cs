using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.PRODAT;
using Enterprise.Edifact.D96B.Segments;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader
{
	public static class ProdatLoader
	{
		public static Header PopulateHeader(PRODATMessage edifactMessage)
		{
			Header header = null;

			if (edifactMessage?.Group8 != null)
			{
				header = CreateHeader(edifactMessage);
			}

			return header;
		}

		static Header CreateHeader(PRODATMessage edifactMessage)
		{
			var header = new Header();

			header.TransactionType = GetTransactionTypeFromMessageFunction(edifactMessage.BGM[0].MessageFunctionCoded);
			header.GovernmentGazettePublicationNumber = edifactMessage.BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
			header.PublicationDate = GetDateTime(edifactMessage.DTM[0]);

			header.Tariffs.AddRange(GetTariffs(edifactMessage));

			return header;
		}

		static IEnumerable<TariffData> GetTariffs(PRODATMessage edifactMessage)
		{
			foreach (SegmentGroup8 seg in edifactMessage.Group8)
			{
				var tariff = new TariffData();

				tariff.LineNumber = seg.LIN[0].LineItemNumber.Trim();
				tariff.ItemNumber = seg.LIN[0].ItemNumberIdentification.ItemNumber.Trim();

				tariff.Heading = seg.PIA[0].ItemNumberIdentification2.ItemNumber.Trim();
				tariff.Code = seg.PIA[0].ItemNumberIdentification3.ItemNumber.Trim();
				tariff.SubHeading = seg.PIA[0].ItemNumberIdentification4.ItemNumber.Trim();
				tariff.CheckDigit = seg.PIA[0].ItemNumberIdentification5.ItemNumber.Trim();

				foreach (DTMSegment dtm in seg.DTM)
				{
					if (dtm.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.EffectiveDateTime)
					{
						tariff.StartDate = GetDateTime(dtm);
					}
					else if (dtm.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.EndDateTime)
					{
						tariff.EndDate = GetDateTime(dtm);
					}
				}

				tariff.StatisticalUnitOriginal = seg.MEA[0].MeasurementDetails.MeasurementAttribute;
				tariff.ScheduleTypeCode = seg.PGI[0].ProductGroup.ProductGroupCoded;

				tariff.Description = GetTextFromFTXSegments(seg.FTX, "AAA", "ACB");
				tariff.ImportedFrom = GetTextFromFTXSegments(seg.FTX, "A04");
				tariff.GovernmentGazetteNoticeNumber = GetTextFromFTXSegments(seg.FTX, "A10");

				tariff.Rates.AddRange(GetRates(seg.FTX));

				yield return tariff;
			}
		}

		static List<Rate> GetRates(FTXSegmentMessageSection ftxSection)
		{
			var results = new List<Rate>();

			foreach (FTXSegment ftx in ftxSection)
			{
				var rate = GetRate(ftx);

				if (rate != null)
				{
					results.Add(rate);
				}
			}

			return results;
		}

		internal static Rate GetRate(FTXSegment ftx)
		{
			Rate rate = null;

			var rateQualifier = ftx.TextSubjectQualifier.ToString();

			switch (rateQualifier)
			{
				case "A01":
					rate = Rate3Text2Formula(ftx, RateTypes.Standard);
					break;
				case "A02":
					rate = Rate2Text1Formula(ftx, RateTypes.SADC);
					break;
				case "A03":
					rate = Rate2Text1Formula(ftx, RateTypes.EU);
					break;
				case "A05":
					rate = Rate2Text1Formula(ftx, RateTypes.Standard);
					break;
				case "A06":
					rate = Rate2Text1Formula(ftx, RateTypes.Standard);
					break;
				case "A07":
					rate = Rate3Text2Formula(ftx, RateTypes.Standard);
					break;
				case "A08":
					rate = Rate3Text1Formula(ftx, RateTypes.Standard);
					break;
				case "A09":
					rate = Rate3Text1Formula(ftx, RateTypes.Standard);
					break;
				case "A11":
					rate = Rate2Text1Formula(ftx, RateTypes.EFTA);
					break;
				case "A12":
					rate = Rate2Text1FormulaWithPossibleCountry(ftx, RateTypes.MERCOSUR);
					break;
				case "A13":
					rate = Rate2Text1FormulaWithPossibleCountry(ftx, RateTypes.AFCFTA);
					break;
			}

			if (rate != null)
			{
				rate.RateQualifier = rateQualifier;
			}

			return rate;
		}

		static Rate Rate3Text2Formula(FTXSegment ftx, string rateType)
		{
			return new Rate
			{
				RateType = rateType,
				Description = string.Join("", ftx.TextLiteral.FreeText1, ftx.TextLiteral.FreeText2, string.IsNullOrWhiteSpace(ftx.TextLiteral.FreeText4) ? "" : ftx.TextLiteral.FreeText3).Trim(),
				FormulaCode = (string.IsNullOrWhiteSpace(ftx.TextLiteral.FreeText4) && !string.IsNullOrWhiteSpace(ftx.TextLiteral.FreeText3) ? ftx.TextLiteral.FreeText3 : ftx.TextLiteral.FreeText4).Trim()
			};
		}

		static Rate Rate2Text1Formula(FTXSegment ftx, string rateType)
		{
			return new Rate
			{
				RateType = rateType,
				Description = string.Join("", ftx.TextLiteral.FreeText1, ftx.TextLiteral.FreeText2).Trim(),
				FormulaCode = ftx.TextLiteral.FreeText3.Trim()
			};
		}

		static Rate Rate3Text1Formula(FTXSegment ftx, string rateType)
		{
			return new Rate
			{
				RateType = rateType,
				Description = string.Join("", ftx.TextLiteral.FreeText1, ftx.TextLiteral.FreeText2, ftx.TextLiteral.FreeText3).Trim(),
				FormulaCode = ftx.TextLiteral.FreeText4.Trim()
			};
		}

		static Rate Rate2Text1FormulaWithPossibleCountry(FTXSegment ftx, string rateType)
		{
			var fullDescription = string.Join("", ftx.TextLiteral.FreeText1, ftx.TextLiteral.FreeText2).Trim();
			var countryList = string.Empty;

			if (fullDescription.Contains(" TO "))
			{
				var idx = fullDescription.IndexOf(" TO ", StringComparison.Ordinal);
				countryList = fullDescription.Substring(idx + 4, fullDescription.Length - idx - 4);
				fullDescription = fullDescription.Substring(0, idx);
			}

			return new Rate
			{
				RateType = rateType,
				Description = fullDescription,
				FormulaCode = ftx.TextLiteral.FreeText3.Trim(),
				Countries = countryList
			};
		}

		internal static TransactionType GetTransactionTypeFromMessageFunction(MessageFunctionCodedList function)
		{
			var result = TransactionType.Undefined;

			if (function == MessageFunctionCodedList.Change)
			{
				result = TransactionType.Change;
			}
			else if (function == MessageFunctionCodedList.Original)
			{
				result = TransactionType.Original;
			}
			else if (function == MessageFunctionCodedList.Deletion)
			{
				result = TransactionType.Deletion;
			}
			else if (function == MessageFunctionCodedList.Addition)
			{
				result = TransactionType.Addition;
			}

			return result;
		}

		static DateTime GetDateTime(DTMSegment dtmSegment)
		{
			var format = string.Empty;

			// I would expect there to be an existing extension method for this but I don't know where
			if (dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmdd)
			{
				format = "yyyyMMdd";
			}
			else if (dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmddhhmmss)
			{
				format = "yyyyMMddHHmmss";
			}

			if (!string.IsNullOrEmpty(format))
			{
				return DateTime.ParseExact(dtmSegment.DateTimePeriod.DateTimePeriod, format, CultureInfo.InvariantCulture);
			}

			throw new NotSupportedException($"Edifact Date format {dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier} not supported yet");
		}

		static string GetTextFromFTXSegments(FTXSegmentMessageSection ftx, params string[] values)
		{
			return string.Join("", ftx.Cast<FTXSegment>()
				.Where(x => values.Contains(x.TextSubjectQualifier.ToString()))
				.Select(x => new
				{
					Code = x.TextSubjectQualifier.ToString(),
					Text = string.Join("", x.TextLiteral.FreeText1,
											x.TextLiteral.FreeText2,
											x.TextLiteral.FreeText3,
											x.TextLiteral.FreeText4,
											x.TextLiteral.FreeText5).Trim()
				})
				.OrderBy(x => x.Code)
				.Select(x => x.Text)).Trim();
		}
	}
}
