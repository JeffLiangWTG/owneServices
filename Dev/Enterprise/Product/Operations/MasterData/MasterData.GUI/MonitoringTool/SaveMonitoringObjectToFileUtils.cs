using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Newtonsoft.Json;

namespace Enterprise.MasterData.GUI
{
	public static class SaveMonitoringObjectToFileUtils
	{
		public static void SaveMonitoringObjectToFile(ConcurrentDictionary<string, MonitoringObjectValue> monitoringObjects)
		{
			if (monitoringObjects != null && monitoringObjects.Any())
			{
				var safi = GetSaveAsFileName();

				if (safi != null)
				{
					try
					{
						using (var stream = Serializer(monitoringObjects))
						{
							var buffer = stream.ToArray();
							safi.FileStream.Write(buffer, 0, buffer.Length);
						}
					}
					finally
					{
						safi.FileStream.Close();
					}
				}
			}
		}

		internal static MemoryStream Serializer(object instance)
		{
			var serializer = new JsonSerializer();
			serializer.TypeNameHandling = TypeNameHandling.All;
			serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			var stream = new MemoryStream();
			StreamWriter streamWriter = null;

			try
			{
				streamWriter = new StreamWriter(stream, Encoding.UTF8, 4096, true);

				using (var jsonWriter = new JsonTextWriter(streamWriter))
				{
					jsonWriter.Formatting = Formatting.Indented;
					jsonWriter.Indentation = 2;

					serializer.Serialize(jsonWriter, instance);

					jsonWriter.Flush();
					stream.Seek(0, SeekOrigin.Begin);

					streamWriter = null;
				}
			}
			finally
			{
				streamWriter?.Dispose();
			}

			return stream;
		}

		static SaveAsFileInfo GetSaveAsFileName()
		{
			using (var dialog = new ZSaveFileDialog())
			{
				dialog.FileName = "MonitoringObject.json";
				SetDialogFiltersWithDisabled(dialog);
				var result = dialog.ShowDialog();

				if (result == DialogResult.OK)
				{
					return new SaveAsFileInfo(dialog.OpenFile());
				}

				return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		static void SetDialogFiltersWithDisabled(ZSaveFileDialog dialog)
		{
			dialog.Filter += "Extensible Markup Language (*.json)|*.json";
		}

		class SaveAsFileInfo
		{
			internal Stream FileStream { get; }

			public SaveAsFileInfo(Stream fileStream)
			{
				FileStream = fileStream;
			}
		}
	}
}
