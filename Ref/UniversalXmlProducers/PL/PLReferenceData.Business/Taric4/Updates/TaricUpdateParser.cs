using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

public static class Isztar4ErrorCodes
{
	public const long AKCEPTUJEZOBOWIAZANIA_MUST_BE_TRUE = -1;
	public const long INVALID_START_DATE = -2;
	public const long TIME_DIFFERENCE_GREATER_THAN_7_DAYS = -3;
	public const long INVALID_EMD_DATE = -7;
}

sealed class TaricUpdateParser(ITaricUpdateParserConfig config) : ITaricUpdateParser
{
	readonly ITaricUpdateParserConfig taricConfig = config ?? throw new ArgumentNullException(nameof(config));

	public bool ParseAndSave(byte[] data)
	{
		try
		{
			var encodedResponse = System.Text.Encoding.UTF8.GetString(data);
			var responseDocument = XDocument.Parse(encodedResponse);

			XDocument unpackedXDocument;
			using (var stream = new MemoryStream(Base64KopertaContent(responseDocument)))
			{
				unpackedXDocument = Helper.UnpackStreamAndLoadXml(stream, "IsztarHistoryResponse");
			}
			var response = XmlParser.Deserialize<IsztarHistoryResponse>(unpackedXDocument);
			ValidateResponse(response);

			var fileLocation = taricConfig.TariffUpdateFolder;
			var date = response.ResultsInfo.startDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			var savePath = Path.Combine(fileLocation, $"update{date}.xml");
			unpackedXDocument.SaveToFile(savePath);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex);

			return false;
		}

		return true;
	}

	static byte[] Base64KopertaContent(XDocument xDoc)
	{
		if (xDoc.Root.Name.LocalName != "KopertaContent")
		{
			throw new Exception("Invalid xml have been passed.");
		}

		if (string.IsNullOrEmpty(xDoc.Root.Value))
		{
			throw new Exception("Empty value have been passed in xml root.");
		}

		return Convert.FromBase64String(xDoc.Root.Value);
	}

	static void ValidateResponse(IsztarHistoryResponse response)
	{
		if (Convert.ToInt64(response.ResultsInfo.totalRecords, CultureInfo.InvariantCulture) != 0)
		{
			return;
		}

		switch (response.ResultsInfo.errorCode)
		{
			case (long)Isztar4ErrorCodes.AKCEPTUJEZOBOWIAZANIA_MUST_BE_TRUE:
			{
				throw new Exception("akceptujeZobowiazania must be set to 'TRUE'.");
			}
			case (long)Isztar4ErrorCodes.INVALID_EMD_DATE:
			{
				throw new Exception("endDate value must be lower or equal current date.");
			}
			case (long)Isztar4ErrorCodes.INVALID_START_DATE:
			{
				throw new Exception("startDate value must be lower than endDate.");
			}
			case (long)Isztar4ErrorCodes.TIME_DIFFERENCE_GREATER_THAN_7_DAYS:
			{
				throw new Exception("Time difference between startDate and endDate must be lower than 7 days.");
			}
			default: // not specified error, so either its not an error or just dummy message occurred - based on Polish PUESC specification
				return;
		}
	}
}
