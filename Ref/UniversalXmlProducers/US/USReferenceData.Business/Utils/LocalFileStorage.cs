using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class LocalFileStorage
	{
		public LocalFileStorage(string dataType)
		{
			_dataType = dataType;
		}
		readonly string _dataType;

		string FileLocation
		{
			get
			{
				if (fileLocation == null)
				{
					var location = Assembly.GetExecutingAssembly().Location;
					fileLocation = Path.Combine(Path.GetDirectoryName(location), $"{Path.GetFileNameWithoutExtension(location)}.{_dataType}.trace");
				}
				return fileLocation;
			}
		}

		string fileLocation;

		public string Load()
		{
			return File.Exists(FileLocation) ? File.ReadAllText(FileLocation) : null;
		}

		public DateTime? LoadAsDateTime(string format)
		{
			var str = Load();
			if (string.IsNullOrEmpty(str))
			{
				return null;
			}
			return DateTime.ParseExact(str, format, CultureInfo.InvariantCulture);
		}

		public void Save(string data)
		{
			File.WriteAllText(FileLocation, data);
		}

		public void Save(DateTime dateTime, string format)
		{
			Save(dateTime.ToString(format, CultureInfo.InvariantCulture));
		}

		public void ClearData()
		{
			if (File.Exists(FileLocation))
			{
				File.Delete(FileLocation);
			}
		}

		public (bool Expired, DateTime? LastPublishDate) IsLastPublishDateExpired(DateTime? currentPublishDateTime)
		{
			var result = false;
			DateTime? lastPublishDate = null;
			if (currentPublishDateTime != null)
			{
				var currentPublishDate = currentPublishDateTime.Value.Date;
				lastPublishDate = LoadAsDateTime("yyyy-MM-dd");
				result = lastPublishDate == null || lastPublishDate.Value.Date < currentPublishDate;
			}
			return (result, lastPublishDate);
		}
	}
}
