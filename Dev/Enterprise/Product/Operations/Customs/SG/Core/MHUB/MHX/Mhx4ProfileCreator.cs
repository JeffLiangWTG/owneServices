using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.SG.MHUB.MHX
{
	/// <summary>
	/// ProfileFile - path whcih contains details about the environemtn, incuding username, password, proxy, log paths, etc
	/// OutputFile - file to which status result is written. This goes into the profile.
	/// DownloadFile - name of the local file(s) to create when retriving data. This path goes into the Input file
	/// </summary>
	public class Mhx4ProfileCreator
	{
		public Mhx4ProfileCreator(IGlbExternalPassword credentials)
		{
			this.credentials = credentials;
			UserId = this.credentials.GP_UserID;
			TempFilePath = Path.Combine(Temp.TempPath, UserId);
			RandomSuffix = new Random().Next().ToString(CultureInfo.InvariantCulture);
		}

		public void CreateProfileFile(IMHUBSettings settingsProvider)
		{
			var clearPassword = credentials.CurrentDecryptedPassword;
			var serverAddressWithProtocolAndPath = settingsProvider.WebAddress;
			var proxy = settingsProvider.WebProxyAddress;
			string proxyHost = "";
			string proxyPort = "";
			var proxyConnect = false;
			if (!String.IsNullOrEmpty(proxy))
			{
				// The proxy, and IS's attitude towards it, can bite me.
				proxyHost = Regex.Split(proxy + ":", ":")[0];
				proxyPort = Regex.Split(proxy + ":", ":")[1];
				proxyConnect = true;
			}

			var uri = new Uri(serverAddressWithProtocolAndPath);
			string template = GetEmbeddedTemplate();
			template = template.Replace("<<PASSWORD>>", MhubEncryption.EncryptData(clearPassword, settingsProvider.EncryptionKey))
								.Replace("<<TRUSTSTOREPATH>>", settingsProvider.TrustStore)
								.Replace("<<PROXYHOST>>", proxyHost)
								.Replace("<<PROXYPORT>>", proxyPort)
								.Replace("<<RANDOMSUFFIX>>", RandomSuffix)
								.Replace("<<PROXYON>>", proxyConnect.ToString().ToLowerInvariant())  // this is the line that triggers CA1308
								.Replace("<<USERID>>", UserId)
								.Replace("<<TEMPPATH>>", MakeWindowsPathAcceptableToMhx(TempFilePath))
								.Replace("<<SERVERIP>>", uri.Host);
			ProfileFile = new FileInfo(Path.Combine(TempFilePath, "Profile_" + UserId + "_" + RandomSuffix + ".txt"));
			ProfileFile.Directory.Create();
			File.WriteAllText(ProfileFile.FullName, template);
		}

		public void CreateInputScript_Upload(string fileNameToUpload, string mailbox, LoggingInformation logger)
		{
			var inputScriptLocation = Path.Combine(TempFilePath, "Input_" + UserId + "_" + RandomSuffix + ".txt");
			var template = string.Empty;
			if (fileNameToUpload.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
			{
				var fileSubject = ExtractRefNumberFromFileName(fileNameToUpload);
				template = string.Format(CultureInfo.InvariantCulture, @"Command=Submit|cont_type=B|filename={0}|recip_id={1}|subj={2}|notifn=N", MakeWindowsPathAcceptableToMhx(fileNameToUpload), mailbox, fileSubject);
				logger.DebugLog("xml template: " + template);
			}
			else
			{
				template = string.Format(CultureInfo.InvariantCulture, @"Command=Submit|cont_type=E|filename={0}|notifn=N", MakeWindowsPathAcceptableToMhx(fileNameToUpload));
			}

			var inputFile = new FileInfo(inputScriptLocation);
			inputFile.Directory.Create();
			template += AddAttachmentFileNames(fileNameToUpload);
			File.WriteAllText(inputFile.FullName, template);
			InputFile = new FileInfo(inputScriptLocation);
			DownloadFile = null;
			OutputFile = new FileInfo(Path.Combine(TempFilePath, "Output_" + UserId + "_" + RandomSuffix + ".txt"));
		}

		public void CreateInputScript_Retrieve()
		{
			var downloadsDir = Path.Combine(TempFilePath, "Cw1Downloads");
			new DirectoryInfo(downloadsDir).Create();
			var downloadDataFile = Path.Combine(downloadsDir, "Download_" + UserId + "_" + RandomSuffix + ".txt");
			var inputScriptLocation = Path.Combine(TempFilePath, "Input_" + UserId + "_" + RandomSuffix + ".txt");
			var ouputScriptLocation = Path.Combine(TempFilePath, "Output_" + UserId + "_" + RandomSuffix + ".txt");
			var template = string.Format(CultureInfo.InvariantCulture, @"Command=Retrieve|filename={0}|loc=I", downloadDataFile);  // Don't need MakeWindowsPathAcceptableToMhx around downloadDataFile, it means absolute paths can't be used
			InputFile = new FileInfo(inputScriptLocation);
			InputFile.Directory.Create();
			File.WriteAllText(InputFile.FullName, template);
			OutputFile = new FileInfo(ouputScriptLocation);
			DownloadFile = new FileInfo(Path.Combine(TempFilePath, downloadDataFile));
		}

		string AddAttachmentFileNames(string fileNameToUpload)
		{
			//e.g. |attachment=true|attachment_files=D:\LoadTest\dog.jpg,D:\LoadTest\cat.jpg,
			var dir = new DirectoryInfo(Path.Combine(TempFilePath, fileNameToUpload + AttachmentsFolderName));
			if (dir.Exists)
			{
				var sb = new ZStringBuilder();
				foreach (var file in dir.GetFiles())
				{
					sb.Append(file.FullName);
				}
				return "|attachment=true|attachment_files=" + sb.ToStringWithDelimiterBetweenAppends(DelimiterForMultipleFilenames);
			}
			return "";
		}

		string MakeWindowsPathAcceptableToMhx(string windows)
		{
			return windows.Replace(@"\", @"\\").Replace(":", @"\:");
		}

		string ExtractRefNumberFromFileName(string fileName)
		{
			var subject = fileName;
			int startPos = fileName.IndexOf("_", StringComparison.OrdinalIgnoreCase) + 1;
			int endPos = fileName.IndexOf(".", StringComparison.OrdinalIgnoreCase);
			int characterCount = endPos - startPos;
			if (startPos > 0)
			{
				subject = fileName.Substring(startPos, characterCount);
			}

			return subject;
		}

		string GetEmbeddedTemplate()
		{
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.MHX.template.pro"))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}

		readonly IGlbExternalPassword credentials;
		public readonly string UserId;
		public readonly string TempFilePath;
		public readonly string RandomSuffix;

		public FileInfo OutputFile { get; private set; }
		public FileInfo InputFile { get; private set; }
		public FileInfo DownloadFile { get; private set; }
		public FileInfo ProfileFile { get; private set; }

		public const string AttachmentsFolderName = ".Attachments";
		public const string DelimiterForMultipleFilenames = ",";
	}
}
