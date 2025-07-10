using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public static class LCTParser
	{
		const string TableXPath = "//table[caption[text()='LCT thresholds']]";
		const string LastModifiedXPath = "//strong[contains(text(), 'Last updated')]/..";

		public static IEnumerable<RefCusTaxOrFee> Parse(string html, out string lastModified)
		{
			var htmlDoc = new HtmlDocumentWrapper(html);
			lastModified = htmlDoc.GetLastModifiedDate(LastModifiedXPath);

			var fees = new List<RefCusTaxOrFee>();

			var tableData = htmlDoc.GetTableData(TableXPath);
			foreach (DataRow row in tableData.Rows)
			{
				fees.AddRange(CreateRefCusTaxOrFeeEntitiesFromDataRow(row));
			}

			SetHighestEndDateOpen(fees);

			return fees;
		}

		static IEnumerable<RefCusTaxOrFee> CreateRefCusTaxOrFeeEntitiesFromDataRow(DataRow row)
		{
			var years = ((string)row["FINANCIAL YEAR"]).Split(["–", "&ndash;", "&#x2013;"], StringSplitOptions.RemoveEmptyEntries);
			var startDate = new DateTime(ParseYear(years[0]), 7, 1, 0, 0, 0);
			var endDate = new DateTime(ParseYear(years[1]), 6, 30, 23, 59, 59);
			var lftValue = (string)row["FUEL-EFFICIENT VEHICLES"];
			var lntValue = (string)row["OTHER VEHICLES"];

			var provider = new CultureInfo("en-AU");
			var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

			yield return new RefCusTaxOrFee
			{
				ZZF_Code = "LFT",
				ZZF_Description = "LCT FEV Threshold",
				ZZF_Value = decimal.Parse(lftValue, style, provider),
				ZZF_StartDate = startDate,
				ZZF_EndDate = endDate
			};

			yield return new RefCusTaxOrFee
			{
				ZZF_Code = "LNT",
				ZZF_Description = "LCT Normal Vehicle Threshold",
				ZZF_Value = decimal.Parse(lntValue, style, provider),
				ZZF_StartDate = startDate,
				ZZF_EndDate = endDate
			};
		}

		static int ParseYear(string yearText)
		{
			var year = yearText.Trim();
			return int.Parse(year.Length == 2 ? "20" + year : year, CultureInfo.InvariantCulture);
		}

		static void SetHighestEndDateOpen(IEnumerable<RefCusTaxOrFee> fees)
		{
			var latestFeesWithLongExpiredTime = fees
				.GroupBy(e => e.ZZF_EndDate)
				.OrderByDescending(e => e.Key)
				.FirstOrDefault();

			// FYI - This crash is deliberate so that we get a report when it stops working.
			foreach (var fee in latestFeesWithLongExpiredTime)
			{
				fee.ZZF_EndDate = Constants.RefData_Common.MaximumDateTime;
			}
		}
	}
}
