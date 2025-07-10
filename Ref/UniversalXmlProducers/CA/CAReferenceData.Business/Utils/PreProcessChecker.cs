using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.CAReferenceData.Business
{
	public class PreProcessChecker
	{
		public PreProcessChecker(string dataType)
		{
			this._dataType = dataType;
		}
		readonly string _dataType;

		string TrackingFileLocation
		{
			get
			{
				if (trackingFileLocation == null)
				{
					var location = Assembly.GetExecutingAssembly().Location;
					trackingFileLocation = Path.Combine(Path.GetDirectoryName(location), $"{Path.GetFileNameWithoutExtension(location)}.{_dataType}.trace");
				}
				return trackingFileLocation;
			}
		}

		string trackingFileLocation;

		DateTime? ReadLastProcessDateFromLocalFile()
		{
			DateTime? date = null;

			if (File.Exists(TrackingFileLocation))
			{
				var dateString = File.ReadAllText(TrackingFileLocation);
				if (DateTime.TryParse(dateString, out DateTime lastPublishDate))
				{
					date = lastPublishDate;
				}
			}
			return date;
		}

		public bool UpdateLastPublishDate(DateTime? currentPublishDateTime)
		{
			var result = false;
			if (currentPublishDateTime != null)
			{
				var currentPublishDate = currentPublishDateTime.Value.Date;
				var lastProcessDate = ReadLastProcessDateFromLocalFile();
				if (lastProcessDate == null || lastProcessDate.Value.Date < currentPublishDate)
				{
					SavePublishDate(currentPublishDate);
					result = true;
				}
			}
			return result;
		}

		public (bool Expired, DateTime? LastPublishDate) IsLastPublishDateExpired(DateTime? currentPublishDateTime)
		{
			var result = false;
			DateTime? lastPublishDate = null;
			if (currentPublishDateTime != null)
			{
				var currentPublishDate = currentPublishDateTime.Value.Date;
				lastPublishDate = ReadLastProcessDateFromLocalFile();
				result = lastPublishDate == null || lastPublishDate.Value.Date < currentPublishDate;
			}
			return (result, lastPublishDate);
		}

		public void SavePublishDate(DateTime lastPublishDate)
		{
			File.WriteAllText(TrackingFileLocation, lastPublishDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
		}

		public void MarkAsProcessRequired()
		{
			if (File.Exists(TrackingFileLocation))
			{
				File.Delete(TrackingFileLocation);
			}
		}
	}
}
