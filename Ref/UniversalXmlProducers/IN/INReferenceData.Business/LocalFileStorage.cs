using System.IO;
using System.Reflection;
using System.Text.Json;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class LocalFileStorage : ILocalFileStorage
	{
		public LocalFileStorage(string dataType)
		{
			_dataType = dataType;
		}
		readonly string _dataType;

		public string FileLocation
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

		public T Load<T>()
		{
			var data = Load();
			return string.IsNullOrEmpty(data) ? default : JsonSerializer.Deserialize<T>(data);
		}

		public void Save(string data)
		{
			File.WriteAllText(FileLocation, data);
		}

		public void Save<T>(T data)
		{
			if (data == null)
			{
				Save("");
				return;
			}

			Save(JsonSerializer.Serialize(data));
		}

		public void ClearData()
		{
			if (File.Exists(FileLocation))
			{
				File.Delete(FileLocation);
			}
		}
	}
}
