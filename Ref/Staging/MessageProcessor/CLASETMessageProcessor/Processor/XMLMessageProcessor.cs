using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor
{
	public class XMLMessageProcessor : BaseMessageProcessor
	{
		public XMLMessageProcessor(string outputPath)
			: base(outputPath)
		{
		}

		#region Process

		protected override int ProcessCore(SourceData sourceData, string messageText)
		{
			var result = 0;

			var customsCommonCode = Deserialize(messageText)?.OutboundMessage?.CustomsCommonCode;

			if (customsCommonCode != null)
			{
				var releaseDate = customsCommonCode.ReleaseDate.ToString(CultureInfo.InvariantCulture);

				ReleaseDate = string.IsNullOrWhiteSpace(releaseDate)
						? sourceData.SDA_CreatedTime.Date
						: GetDate(releaseDate);

				ReleaseNumber = customsCommonCode.ReleaseNumberSpecified ? customsCommonCode.ReleaseNumber.ToString("00000", CultureInfo.InvariantCulture) : string.Empty;

				result = ProcessCustomsCommonCode(customsCommonCode);
				sourceData.SDA_Status = StatusProvider.GetMERStatus();
			}
			else
			{
				sourceData.SDA_Status = StatusProvider.GetERRStatus();
			}

			return result;
		}

		static TradenetResponse Deserialize(string messageText)
		{
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				using (var reader = new StringReader(messageText))
				{
					var serializer = new XmlSerializer(typeof(TradenetResponse));
#pragma warning disable CA5369 // Use XmlReader for 'XmlSerializer.Deserialize()'
					return serializer.Deserialize(reader) as TradenetResponse;
#pragma warning restore CA5369 // Use XmlReader for 'XmlSerializer.Deserialize()'
				}
			}
			catch
			{
				return null;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		int ProcessCustomsCommonCode(CustomsCommonCode customsCommonCode)
		{
			ProcessCustomsCommonCodeCore(customsCommonCode);
			ExportToXMLFile();

			return TotalProcessed;
		}

		void ProcessCustomsCommonCodeCore(CustomsCommonCode customsCommonCode)
		{
			foreach (var info in customsCommonCode.CustomsCommonCodeInformation)
			{
				var tableName = info.CustomsCommonCodeTableName;
				Func<CustomsCommonCodeInformationRecord, int> processCodeFunc = null;

				switch (tableName)
				{
					case Constants.ReferenceTypes.PortCode:
						processCodeFunc = ProcessPortCode;
						break;

					case Constants.ReferenceTypes.FacilityCode:
						processCodeFunc = ProcessFacilityCode;
						break;

					case Constants.ReferenceTypes.CustomsProcedureCode:
						processCodeFunc = ProcessCustomsProcedureCode;
						break;

					case Constants.ReferenceTypes.CommodityCode:
						processCodeFunc = ProcessCommondityCode;
						break;
				}

				if (processCodeFunc != null)
				{
					foreach (var record in info.Record)
					{
						TotalProcessed += processCodeFunc(record);
					}
				}
			}
		}

		#endregion

		#region Commondity Code

		int ProcessCommondityCode(CustomsCommonCodeInformationRecord record)
		{
			var updateType = record.OperationCode;

			var startDate = GetDateTime(record.EffectiveDate);

			var endDate = GetDateTime(record.ExpiryDate);
			endDate = endDate < startDate ? startDate : endDate;

			var codesLength = record.Code.Length;

			var descriptions = GetFullDescriptions(record);
			var descriptionsLength = descriptions.Length;

			var commodityCode = string.Empty;
			var commodityDescription = string.Empty;
			var tariffCode = string.Empty;

			for (var i = 0; i < codesLength; i++)
			{
				var code = record.Code[i] ?? string.Empty;
				var description = i < descriptionsLength ? descriptions[i] : string.Empty;

				if (string.IsNullOrWhiteSpace(description))
				{
					description = code;
				}

				if (i == 0)
				{
					commodityCode = code;
					commodityDescription = description;
				}
				else if (i == 1)
				{
					tariffCode = code;
					break;
				}
			}

			var controlType = record.Field?.FirstOrDefault(c => c.ID == Constants.TariffSegmentIdentifiers.ControlType)?.Value ?? string.Empty;
			var unitOfQty = record.Field?.FirstOrDefault(c => c.ID == Constants.TariffSegmentIdentifiers.UnitOfMeasurement)?.Value ?? string.Empty;

			if (updateType == Constants.UpdateType.Deleted)
			{
				endDate = GetValidEndDate(startDate);
			}

			return AddCommondityCode(commodityCode, commodityDescription, tariffCode, startDate, endDate, unitOfQty, controlType);
		}

		#endregion

		#region Customs Procedure Code

		int ProcessCustomsProcedureCode(CustomsCommonCodeInformationRecord record)
		{
			var updateType = record.OperationCode;

			var startDate = GetDateTime(record.EffectiveDate);

			var endDate = GetDateTime(record.ExpiryDate);
			endDate = endDate < startDate ? startDate : endDate;

			var codesLength = record.Code.Length;

			var descriptions = GetFullDescriptions(record);
			var descriptionsLength = descriptions.Length;

			var aPCode = string.Empty;
			var aPCName = string.Empty;
			var cPCode = string.Empty;

			for (var i = 0; i < codesLength; i++)
			{
				var code = record.Code[i] ?? string.Empty;
				var description = i < descriptionsLength ? descriptions[i] : string.Empty;

				if (string.IsNullOrWhiteSpace(description))
				{
					description = code;
				}

				if (i == 0)
				{
					aPCode = code;
					aPCName = description;
				}
				else if (i == 1)
				{
					cPCode = code;
					break;
				}
			}

			if (updateType == Constants.UpdateType.Deleted)
			{
				endDate = GetValidEndDate(startDate);
			}

			return AddCustomsProcedureCode(aPCode, aPCName, cPCode, startDate, endDate);
		}

		#endregion

		#region Facility Code

		int ProcessFacilityCode(CustomsCommonCodeInformationRecord record)
		{
			return ProcessGeneralCode(record, AddFacilityCode);
		}

		#endregion

		#region Port Code

		int ProcessPortCode(CustomsCommonCodeInformationRecord record)
		{
			return ProcessGeneralCode(record, AddPortCode);
		}

		#endregion

		#region Implement

		int ProcessGeneralCode(CustomsCommonCodeInformationRecord record, Func<string, string, DateTime, DateTime, int> addCodeFunc)
		{
			var updateType = record.OperationCode;

			var startDate = GetDateTime(record.EffectiveDate);

			var endDate = GetDateTime(record.ExpiryDate);
			endDate = endDate < startDate ? startDate : endDate;

			var totalProcessed = 0;

			var codesLength = record.Code.Length;

			var descriptions = GetFullDescriptions(record);
			var descriptionsLength = descriptions.Length;

			for (var i = 0; i < codesLength; i++)
			{
				var code = record.Code[i] ?? string.Empty;
				var description = i < descriptionsLength ? descriptions[i] : string.Empty;

				if (string.IsNullOrWhiteSpace(description))
				{
					description = code;
				}

				if (updateType == Constants.UpdateType.Deleted)
				{
					endDate = GetValidEndDate(startDate);
				}

				if (!string.IsNullOrWhiteSpace(code))
				{
					totalProcessed += addCodeFunc(code, description, startDate, endDate);
				}
			}

			return totalProcessed;
		}

		static string[] GetFullDescriptions(CustomsCommonCodeInformationRecord record)
		{
			var descriptions = record.Description ?? Enumerable.Empty<CustomsCommonCodeInformationRecordDescription>();
			if (descriptions.Any())
			{
				return descriptions.Select(c => GetFullDescription(c)).ToArray();
			}
			else
			{
				var descriptionField = record.Field?.FirstOrDefault(c => c.ID?.Equals(DescriptionID, StringComparison.Ordinal) ?? false);
				if (descriptionField != null)
				{
					return new [] { descriptionField.Value };
				}
			}

			return Array.Empty<string>();
		}

		const string DescriptionID = "DES";

		static string GetFullDescription(CustomsCommonCodeInformationRecordDescription descriptions)
		{
			return string.Join(string.Empty, descriptions?.FreeText?.Where(c => !string.IsNullOrWhiteSpace(c)) ?? Enumerable.Empty<string>());
		}

		static DateTime GetDateTime(string dateTime)
		{
			return new DateTime(Convert.ToInt32(dateTime.Substring(0, 4), CultureInfo.InvariantCulture), Convert.ToInt32(dateTime.Substring(4, 2), CultureInfo.InvariantCulture), Convert.ToInt32(dateTime.Substring(6, 2), CultureInfo.InvariantCulture), Convert.ToInt32(dateTime.Substring(8, 2), CultureInfo.InvariantCulture), Convert.ToInt32(dateTime.Substring(10, 2), CultureInfo.InvariantCulture), Convert.ToInt32(dateTime.Substring(12, 2), CultureInfo.InvariantCulture));
		}

		static DateTime GetDate(string date)
		{
			return new DateTime(Convert.ToInt32(date.Substring(0, 4), CultureInfo.InvariantCulture), Convert.ToInt32(date.Substring(4, 2), CultureInfo.InvariantCulture), Convert.ToInt32(date.Substring(6, 2), CultureInfo.InvariantCulture));
		}

		#endregion
	}
}
