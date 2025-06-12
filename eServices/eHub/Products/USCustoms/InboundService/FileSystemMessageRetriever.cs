using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eServices.USCustoms.Integration;
using Common.Logging;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.InboundService
{
	public class FileSystemMessageRetriever : IMQMessageRetriever
	{
		internal DirectoryInfoWrapper inputDirectory;
		internal string fileMask;
		readonly IInboundServiceConfiguration config;
		readonly ILog logger;
		bool transactionStarted;
		FileInfoWrapper currentFile;

		public FileSystemMessageRetriever(string inputPath, IInboundServiceConfiguration config, ILog logger)
		{
			if (inputPath == null) throw new ArgumentNullException("inputPath");
			if (config == null) throw new ArgumentNullException("config");
			if (logger == null) throw new ArgumentNullException("logger");

			this.config = config;
			this.logger = logger;

			var match = Regex.Match(inputPath, @"(?<folderPath>^[^*]*$|^(\\?[^*\\]*\\)*)(?<fileMask>.*$)");
			string folderPath = match.Groups["folderPath"].Value;
			this.inputDirectory = new DirectoryInfoWrapper(String.IsNullOrWhiteSpace(folderPath) ? "." : folderPath);
			this.fileMask = match.Groups["fileMask"].Value;

			this.logger.InfoFormat("Initialised file system message retriever. Directory: '{0}' FileMask: '{1}'", this.inputDirectory, this.fileMask);
		}

		public void BeginTransaction()
		{
			if (this.transactionStarted) throw new InvalidOperationException("Parallel transactions are not supported.");
			this.transactionStarted = true;
		}

		public Stream Retrieve()
		{
			if (!this.transactionStarted) throw new InvalidOperationException("No active transaction.");
			this.currentFile = this.inputDirectory.GetFiles(this.fileMask).FirstOrDefault();
			if (this.currentFile == null)
				return null;

			this.logger.Debug("Receiving file: " + this.currentFile.FullName);

			string body;
			using (var fs = this.currentFile.OpenRead())
			using (var ce = fs.CompressAndEncode())
			using (var sr = new StreamReader(ce))
				body = sr.ReadToEnd();

			var msg = new XElement(Constants.MessageAttributes.StartElement,
				new XAttribute(Constants.MessageAttributes.IsProduction, this.config.IsProduction),
				new XAttribute(Constants.MessageAttributes.MessageType, this.config.MessageType),
				new XCData(body)
			);

			var ms = new MemoryStream();
			var xw = XmlWriter.Create(ms, new XmlWriterSettings() { OmitXmlDeclaration = true, Encoding = new UTF8Encoding(false) });
			msg.WriteTo(xw);
			xw.Flush();
			ms.Position = 0;

			return ms;
		}

		public void CommitTransaction()
		{
			if (!this.transactionStarted) throw new InvalidOperationException("No active transaction.");
			if (this.currentFile != null)
			{
				this.logger.Debug("Deleting file: " + this.currentFile.FullName);
				this.currentFile.Delete();
				this.currentFile = null;
			}
			this.transactionStarted = false;
		}

		public void RetrieveSafe()
		{
			throw new NotImplementedException();
		}

		public void RollbackTransaction()
		{
			if (this.currentFile != null)
			{
				this.logger.Debug("File not deleted: " + this.currentFile.FullName);
				this.currentFile = null;
			}
			this.transactionStarted = false;
		}

		internal class DirectoryInfoWrapper
		{
			readonly DirectoryInfo directoryInfo;
			public DirectoryInfoWrapper() { }
			public DirectoryInfoWrapper(string path) { this.directoryInfo = new DirectoryInfo(path); }
			internal virtual FileInfoWrapper[] GetFiles(string searchPattern) { return this.directoryInfo.GetFiles(searchPattern).Select(f => new FileInfoWrapper(f)).ToArray(); }
			public override string ToString() { return directoryInfo.ToString(); }
		}

		internal class FileInfoWrapper
		{
			readonly FileInfo fileInfo;
			public FileInfoWrapper() { }
			public FileInfoWrapper(FileInfo fileInfo) { this.fileInfo = fileInfo; }
			internal virtual string FullName { get { return this.fileInfo.FullName; } }
			internal virtual void Delete() { this.fileInfo.Delete(); }
			internal virtual Stream OpenRead() { return this.fileInfo.OpenRead(); }
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
