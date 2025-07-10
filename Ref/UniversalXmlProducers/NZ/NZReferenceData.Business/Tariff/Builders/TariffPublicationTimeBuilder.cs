using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	sealed class TariffPublicationTimeBuilder<TProcessingData> : IBuilder<TProcessingData>
	{
		public TariffPublicationTimeBuilder(Func<TProcessingData, DateTime> getProcessingDataValue, Action<TProcessingData, DateTime> setProcessingDataValue)
		{
			GetProcessingDataValue = getProcessingDataValue;
			SetProcessingDataValue = setProcessingDataValue;
		}

		Func<TProcessingData, DateTime> GetProcessingDataValue { get; }
		Action<TProcessingData, DateTime> SetProcessingDataValue { get; }

		public bool Build(IDataRepo dataRepo, BuildersFilePath[] filePaths, TProcessingData processingData)
		{
			if (filePaths.Length != 1)
			{
				throw new ArgumentException($"TariffPublicationTimeBuilder needs to have exactly 1 file path");
			}

			var isNewPublication = false;
			var filePath = filePaths.Single().FilePath;
			var dateTimeString = PreProcessPublicationTime(filePath);
			string format = "ddd MMM d hh:mm:ss tt yyyy";
			if (DateTime.TryParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result))
			{
				dataRepo.PublicationTime = result;
				if (GetProcessingDataValue(processingData) < result)
				{
					isNewPublication = true;
					SetProcessingDataValue(processingData, result);
				}
			}
			else
			{
				throw new InvalidOperationException($"Cannot find a valid publication time from {filePath}.");
			}

			return isNewPublication;
		}

		static string PreProcessPublicationTime(string filePath)
		{
			var lines = File.ReadAllLines(filePath);
			var parts = lines[0].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			return $"{parts[0]} {parts[1]} {parts[2]} {parts[3]} {parts[4]} {parts[6]}";
		}
	}
}
