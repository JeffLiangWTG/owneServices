using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class TradeGroupLoader
	{
		public static IEnumerable<TradeGroup> Load(string fileName)
		{
			var xls = new XlsFile(fileName);
			xls.ActiveSheet = 1;

			var list = new List<(string GroupCode, string GroupDescription, string GroupStartDate, string GroupEndDate, string CountryCode, string CountryDescription, string CountryStartDate, string CountryEndDate)>();
			for (int row = 2; row <= xls.RowCount; row++)
			{
				list.Add((
					xls.GetTrimmedStringFromCell(row, 1),
					xls.GetTrimmedStringFromCell(row, 2),
					xls.GetTrimmedStringFromCell(row, 3),
					xls.GetTrimmedStringFromCell(row, 4),
					xls.GetTrimmedStringFromCell(row, 5),
					xls.GetTrimmedStringFromCell(row, 6),
					xls.GetTrimmedStringFromCell(row, 7),
					xls.GetTrimmedStringFromCell(row, 8)
					));
			}

			return list.GroupBy(x => x.GroupCode).Select(grouping =>
			{
				return new TradeGroup
				{
					Code = grouping.Key,
					Description = grouping.Max(element => element.GroupDescription),
					StartDate = DateTime.Parse(grouping.Max(element => element.GroupStartDate), CultureInfo.InvariantCulture),
					EndDate = DateTime.Parse(grouping.Max(element => element.GroupEndDate), CultureInfo.InvariantCulture),
					Countries = grouping.Select(element =>
					{
						return new TradeGroupCountry
						{
							Code = element.CountryCode,
							Description = element.CountryDescription,
							StartDate = DateTime.Parse(element.CountryStartDate, CultureInfo.InvariantCulture),
							EndDate = DateTime.Parse(element.CountryEndDate, CultureInfo.InvariantCulture),
						};
					})
				};
			});
		}
	}
}
