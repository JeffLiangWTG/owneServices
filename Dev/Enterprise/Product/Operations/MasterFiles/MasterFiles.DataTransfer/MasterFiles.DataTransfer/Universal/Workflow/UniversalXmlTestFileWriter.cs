using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Workflow
{
	public class UniversalXmlTestFileWriter : UniversalXmlWriter, IUniversalXmlTestFileWriter
	{
		public UniversalXmlTestFileWriter(IUniversalActionInfo actionInfo, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO)
			: base(actionInfo, dataWriterGetter, exportedBO)
		{
			Argument.NotNull(exportedBO, "BusinessObject exportedBO");

			this.manager = actionInfo.ParentBO.GetUniversalDataContextManager();
			Argument.NotNull(manager, "ProcessTaskNotification action.Parent.Parent.GetUniversalDataSourceManager()");
		}
		readonly IDataContextManager manager;

		public IUniversalXmlExportResult Export(string defaultExportDirectory)
		{
			var exportDirectory = GetExportDirectory(defaultExportDirectory);
			var outboundSessionTracker = new DataWritingManager(actionInfo);
			var rootElementName = dataWriterGetter(outboundSessionTracker).RootElementName;
			string baseFileName = Path.Combine(exportDirectory, MakeFilenameSafe.MakeSafe(rootElementName + " " + manager.DataContextKey + " " + ZDateTime.Now.ToString("dd-MMM-yy HHmm"), '_'));
			string fileName = GetUniqueFileName(baseFileName);

			if (fileName != null)
			{
				using (var dataObject = base.GetDataObjectToExport(actionInfo.ParentBO, exportedBO, outboundSessionTracker))
				{
					var xmlWriter = new XmlWriter();
					long bytesWritten = 0;
					try
					{
						using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
						using (var fileStream = new CargoWise.IO.Shim.SubStreamableStream((size) => fs, int.MaxValue))
						{
							xmlWriter.WriteXML(dataObject, fileStream);
							bytesWritten = fileStream.Length;
						}
					}
					catch (IOException exception)
					{
						return new ExportResult(Res.GetString("0e45a05c-3cb2-4076-9530-d27c5badd0d1", "Error saving {0} from [{1}] to file:\r\n{2}\r\n\r\nError Message: {3}", rootElementName, manager.DataContextKey, fileName, exception.Message), false);
					}

					return new ExportResult(Res.GetString("64c7936d-d778-4a30-936b-2d4dbb37834f", "{0} from [{1}] saved as {2} bytes to file:\r\n{3}", rootElementName, manager.DataContextKey, bytesWritten.ToString(), fileName), true);
				}
			}
			else
			{
				return new ExportResult(Res.GetString("71f7feb4-57e3-4c08-aaf8-53a2533cf3ff", "Could not establish unique filename to use based on:\r\n{0}", baseFileName), false);
			}
		}

		class ExportResult : IUniversalXmlExportResult
		{
			internal ExportResult(string message, bool success)
			{
				this.Message = message;
				this.Success = success;
			}

			public string Message
			{
				get;
				private set;
			}

			public bool Success
			{
				get;
				private set;
			}
		}

		static string GetUniqueFileName(string baseFileName)
		{
			for (int index = 0; index < 100; index++)
			{
				var fileName = baseFileName + (index > 0 ? "_" + index.ToString() : "") + ".xml";
				if (!File.Exists(fileName))
				{
					return fileName;
				}
			}

			return null;
		}

		static string GetExportDirectory(string defaultExportDirectory)
		{
			var exportDirectory = defaultExportDirectory;
			if (string.IsNullOrEmpty(exportDirectory))
			{
				exportDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
				if (string.IsNullOrEmpty(exportDirectory))
				{
					exportDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
				}
			}
			return exportDirectory;
		}
	}
}
