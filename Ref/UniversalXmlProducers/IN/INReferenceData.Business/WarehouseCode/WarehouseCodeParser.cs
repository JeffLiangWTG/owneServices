using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.INReferenceData.Business;

public static class WarehouseCodeParser
{
	public static List<RefCusCodeList> ParseResponse(string responseContent)
	{
		var result = new List<RefCusCodeList>();
		var htmlDoc = new HtmlDocument();
		htmlDoc.LoadHtml(responseContent);

		var rows = htmlDoc.DocumentNode.SelectSingleNode(AppConfig.WarehouseCode.TableXpath)?.SelectNodes(".//tr");
		Helper.Assume(rows != null, $"Failed to find the table in the response, Response is {responseContent}");

		result.AddRange(rows.Select(ConvertToRefCusCodeList).Where(code => code != null));

		Helper.Assume(result.Count != 0, $"Failed to parse warehouse codes from the response, Response is {responseContent}");

		HandleDuplicateCodes(result);
		return result;
	}

	static void HandleDuplicateCodes(List<RefCusCodeList> codes)
	{
		codes.GroupBy(code => code.ZZD_Code.ToUpperInvariant())
			.Where(group => group.Count() > 1).ToList()
			.ForEach(duplicateGroup =>
			{
				duplicateGroup.Where(HasPropertyNA).ToList().ForEach(code => codes.Remove(code));
			});

		codes.GroupBy(code => code.ZZD_Code.ToUpperInvariant())
			.Where(group => group.Count() > 1)
			.ToList().ForEach(duplicateGroup =>
			{
				duplicateGroup.ToList().OrderByDescending(order => order.ZZD_Code).Skip(1).ToList().ForEach(code => codes.Remove(code));
				var message = $"Found duplicate warehouse codes: {duplicateGroup.Key}, Count: {duplicateGroup.Count()}, Names: {string.Join(", ", duplicateGroup.Select(x => x.ZZD_Description))}";
				Logger.Log(LogType.ReviewRequired, message);
			});
	}

	static bool HasPropertyNA(RefCusCodeList code)
	{
		return code.ZZD_Description.Equals("N.A.", StringComparison.OrdinalIgnoreCase)
			|| code.RefCusCodeListAttributes[0].ZZE_Value.Equals("N.A.", StringComparison.OrdinalIgnoreCase);
	}

	static RefCusCodeList ConvertToRefCusCodeList(HtmlNode row)
	{
		var cells = row.SelectNodes(".//td");
		if (cells.Count != AppConfig.WarehouseCode.LegalTableColumns)
		{
			return null;
		}

		return new RefCusCodeList
		{
			ZZD_Code = WebUtility.HtmlDecode(cells[AppConfig.WarehouseCode.CodeIndex].InnerText.Trim()),
			ZZD_Description = WebUtility.HtmlDecode(cells[AppConfig.WarehouseCode.NameIndex].InnerText.Trim()),
			RefCusCodeListAttributes = [new RefCusCodeListAttribute { ZZE_Value = WebUtility.HtmlDecode(cells[AppConfig.WarehouseCode.AddressIndex].InnerText.Trim()) }]
		};
	}

	static Logger Logger => logger ??= new Logger(new DateTimeProvider());
	static Logger logger;
}
