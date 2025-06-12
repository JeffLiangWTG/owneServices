using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using Enterprise.Customs.FR.TransportSvc.Utilities;
using Newtonsoft.Json.Linq;

namespace Enterprise.Customs.FR.TransportSvc.Messages
{
	public class BaseMessagesProcessor : IDisposable
	{
		public BaseMessagesProcessor(Logger logger)
		{
			this.logger = logger;
			ftpHelper = new FtpHelper();
		}

		protected bool SendFileToCW1(string filePath, string senderID, string recipientId)
		{
			var fileInfo = new FileInfo(filePath);

			if (!fileInfo.Exists)
			{
				throw new IOException(fileInfo.FullName + " not found");
			}

			if (!ExecuteForDebugging)
			{
				var password = senderID == EHubProductionLogin ? EHubProductionPassword : EHubTestPassword;
				var ehubAddress = GetEhubAddressToUploadTo(recipientId);
				eAdaptorSampleWebClient.SendMessage(ehubAddress, filePath, recipientId, senderID, password);
			}

			if (!string.IsNullOrEmpty(eAdaptorSampleWebClient.SendErrorMessage))
			{
				Logger.AddNotification(eAdaptorSampleWebClient.SendErrorMessage, Notification.MessageType.Error, Notification.Events.SendingError, verboseModeOnly: false);
			}

			return true;
		}

		public StringWriterWithEncoding GetTransformedMessage(XmlDocument xmlDocument, bool isResponse)
		{
			var transformedMessage = new StringWriterWithEncoding(Encoding.UTF8);
			if (xmlDocument != null)
			{
				var transformer = GetTransformer(xmlDocument, isResponse);
				transformedMessage = TransformMessage(xmlDocument, transformer);
			}
			return transformedMessage;
		}

		static StringWriterWithEncoding TransformMessage(XmlDocument xmlDocument, string transformer)
		{
			var xslTransform = new XslCompiledTransform();
			xslTransform.Load(Path.Combine(AssemblyPath, transformer));
			var transformedMessage = new StringWriterWithEncoding(Encoding.UTF8);
			xslTransform.Transform(xmlDocument.CreateNavigator(), new XsltArgumentList(), transformedMessage);
			return transformedMessage;
		}

		public string GetTransformer(XmlDocument xmlDocument, bool isResponse)
		{
			string result;
			if (isResponse)
			{
				var application = ToolBox.GetElementTextByTagNameSafely(xmlDocument, "Application");
				if (application == Constants.ApplicationTypes.DeltaT && ToolBox.GetElementTextByTagNameSafely(xmlDocument, "InterchangeType") == "FR5")
				{
					application = Constants.ApplicationTypes.TP5;
				}
				var transformer = new TransformerProvider(application).ResponseTransformer;
				result = transformer;
				if (string.IsNullOrEmpty(transformer))
				{
					Logger.AddNotification($"No transformer was found for application {application}.", Notification.MessageType.Error, Notification.Events.FTPFileRecovery, verboseModeOnly: false);
					result = null;
				}
			}
			else
			{
				var schemaID = GetSchemaID(xmlDocument);
				var provider = new TransformerProvider(schemaID);
				var transformer = provider.MessageTransformer;
				result = transformer;
				if (string.IsNullOrEmpty(transformer))
				{
					Logger.AddNotification($"No transformer was found for SchemaID {schemaID}.", Notification.MessageType.Error, Notification.Events.FTPFileRecovery, verboseModeOnly: false);
					return null;
				}
			}

			return result;
		}

		public virtual string GetTime() => DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture);

		public virtual string GetLongTime() => DateTime.Now.ToString("HHmmssfff", CultureInfo.InvariantCulture);

		public virtual string GetDate() => DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

		public virtual string GetDateTime() => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);

		protected static string GetSchemaID(XmlDocument xmlDocument)
		{
			var result = string.Empty;

			if (IsDeltaIEInterchange(xmlDocument))
			{
				var jsonMessage = ToolBox.GetElementTextByTagNameSafely(xmlDocument, "JsonMessage");
				if (string.IsNullOrEmpty(jsonMessage))
				{
					jsonMessage = ToolBox.GetElementTextByTagNameSafely(xmlDocument, "Body");
				}
				dynamic jsonMessageObject = JObject.Parse(jsonMessage);
				result = jsonMessageObject?.SchemaId?.Value ?? string.Empty;
			}
			else if (IsPNTSInterchange(xmlDocument))
			{
				result = ToolBox.GetElementTextByTagNameSafely(xmlDocument, "TypeMessage");
				if (string.IsNullOrEmpty(result))
				{
					result = ToolBox.GetFirstChildNameByTagNameSafely(xmlDocument, "Body");
				}
			}
			else if (IsDeltaTPhase5Interchange(xmlDocument))
			{
				result = ToolBox.GetElementTextByTagNameSafely(xmlDocument, "messageType");
			}
			else
			{
				result = ToolBox.GetElementTextByTagNameSafely(xmlDocument, "schemaID");
				if (string.IsNullOrEmpty(result))
				{
					var alternateAttribute = ToolBox.GetAttributeValueSafely(xmlDocument, "Request", "type");
					if (GetApplusMessageTypeList().Contains(alternateAttribute))
					{
						result = Constants.MessageTypes.APPLUS;
					}
				}
			}

			return result;
		}

		protected static string GetMode(string recipientID, bool isAPPLUS)
		{
			var result = string.Empty;

			if (!string.IsNullOrEmpty(recipientID))
			{
				result = recipientID.ToUpper().Contains("TEST") ? TestMode : ProductionMode;

				if (isAPPLUS && !string.IsNullOrEmpty(ApplusMode))
				{
					result = ApplusMode;
				}
			}

			return result;
		}

		protected static string GetAPPLUSJobReference(XmlDocument xmlDocument)
		{
			var result = string.Empty;

			var messageType = ToolBox.GetAttributeValueSafely(xmlDocument, "Request", "type");
			switch (messageType)
			{
				case "DOA":
					result = ToolBox.GetAttributeValueSafely(xmlDocument, "reference-doc", "num-doss");
					break;
				case "CAED":
					result = ToolBox.GetAttributeValueSafely(xmlDocument, "references-ctrl", "dos");
					break;
			}

			return result;
		}

		protected static string GetNewMESInterchangeControlReference()
		{
			var result = string.Empty;
			var availableChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
			var rnd = new Random();
			for (var i = 0; i < 14; i++)
			{
				result += availableChars[rnd.Next(availableChars.Length)];
			}
			return result;
		}

		protected static string GetJobReference(string declarationReference)
		{
			//declaration reference format is : 999999-JobReference/1
			var jobReference = string.Empty;

			if (!string.IsNullOrEmpty(declarationReference))
			{
				var separator1Position = declarationReference.IndexOf("-", StringComparison.InvariantCulture);
				jobReference = declarationReference.Substring(separator1Position + 1);
				var separator2Position = jobReference.IndexOf("/", StringComparison.InvariantCulture);
				if (separator2Position > 0)
				{
					jobReference = jobReference.Substring(0, separator2Position);
				}
			}
			return jobReference;
		}

		protected void CleanDirectoryWisely(string directory)
		{
			Logger.AddNotification("Cleaning working directory...", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: true);

			var di = new DirectoryInfo(directory);
			foreach (var file in di.GetFiles())
			{
				if (!filesToReplayNextLoop.Contains(file.FullName))
				{
					file.Delete();
				}
			}

			Logger.AddNotification("...done.", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: true);
		}

		public static string GetMessageType(XmlDocument sourceDocument)
		{
			return ToolBox.GetElementTextByTagNameSafely(sourceDocument, "TypeMessage");
		}

		public static string GetMessageAdditionalInfo(XmlDocument sourceDocument)
		{
			return ToolBox.GetElementTextByTagNameSafely(sourceDocument, "CompteRendu");
		}

		protected static string CleanTransactionID(string transactionID)
		{
			//transactionID format as returned by the platform is : XXXXX+DSI+YYYYYYY. Only get the YYYY
			var lastSeparator1Position = transactionID.LastIndexOf("+", StringComparison.InvariantCulture);
			return transactionID.Substring(lastSeparator1Position + 1);
		}

		public static bool IsNetworkException(Exception ex) => ex.Message.Contains("Erreur r�seau inattendue.")
														|| ex.Message.Contains("Unexpected network error.")
														|| ex.Message.Contains("La connexion sous-jacente a �t� ferm�e.")
														|| ex.Message.Contains("The underlying connection")
														|| ex.Message.Contains("Impossible d'�tablir un canal s�curis�")
														|| ex.Message.Contains("secure channel")
														|| ex.Message.Contains("Il n'existait pas de point de terminaison")
														|| ex.Message.Contains("ending point")
														|| ex.Message.Contains("There was no endpoint")
														|| ex.Message.Contains("Client not connected")
														|| ex.Message.Contains("The HTTP service located at")
														|| ex.Message.Contains("The request channel timed out")
														|| ex.Message.Contains("A socket operation")
														|| ex.HResult == -2146233088;

		protected void RevertAnyChangeAndAddToNextRoundQueue(string filePath)
		{
			File.WriteAllText(filePath, CurrentMessageContentBeforeAnyChange);
			filesToReplayNextLoop.Add(filePath);
		}

		public static string GetEhubAddressToUploadTo(string recipientId) => ClientsUsingeHubForTest.Contains(recipientId) ? EHubForTestAddress : EHubAddress;

		public Utilities.StringWriterWithEncoding MessageStringTransformed { get; set; } = new Utilities.StringWriterWithEncoding(Encoding.UTF8);

		protected static string WorkingDirectory => ApplicationConfig.Instance.WorkingDirectory;
		protected static string EHubAddress => ApplicationConfig.Instance.EHubAddress;
		protected static string EHubForTestAddress => ApplicationConfig.Instance.EHubForTestAddress;
		protected static string EHubProductionLogin => ApplicationConfig.Instance.EHubProductionLogin;
		protected static string EHubProductionPassword => ApplicationConfig.Instance.EHubProductionPassword;
		protected static string EHubTestLogin => ApplicationConfig.Instance.EHubTestLogin;
		protected static string EHubTestPassword => ApplicationConfig.Instance.EHubTestPassword;
		protected static bool ExecuteForDebugging => ApplicationConfig.Instance.ExecuteForDebugging == "1";
		protected static string ClientsUsingeHubForTest => ApplicationConfig.Instance.ClientsUsingeHubForTest;
		protected static string ApplusMode => ApplicationConfig.Instance.APPLUSMode;
		protected static string AssemblyPath => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		protected static string[] GetApplusMessageTypeList() => new string[] { "CAED", "DOA" };
		protected static bool IsUCC6(XmlDocument doc) => IsDeltaIEInterchange(doc) || IsPNTSInterchange(doc);
		protected static bool IsTest(XmlDocument doc) => ToolBox.GetElementTextByTagNameSafely(doc, "Mode") == TestMode;
		protected static bool IsDeltaIEInterchange(XmlDocument doc) => ToolBox.GetElementTextByTagNameSafely(doc, "InterchangeType") == "FRI";
		protected static bool IsPNTSInterchange(XmlDocument doc) => ToolBox.GetElementTextByTagNameSafely(doc, "InterchangeType") == "FRS";
		protected static bool IsDeltaTPhase5Interchange(XmlDocument doc) => ToolBox.GetElementTextByTagNameSafely(doc, "InterchangeType") == "FR5";
		protected static bool IsDeltaTPhase4(string schemaID) => schemaID == Constants.MessageSchemas.IE007Schema || schemaID == Constants.MessageSchemas.IE013Schema || schemaID == Constants.MessageSchemas.IE014Schema || schemaID == Constants.MessageSchemas.IE015Schema || schemaID == Constants.MessageSchemas.IEF15Schema || schemaID == Constants.MessageSchemas.IE044Schema || schemaID == Constants.MessageSchemas.IE141Schema;

		protected static string Right(string original, int numberCharacters)
		{
			var result = string.Empty;

			var originalLength = original.Length;
			if (originalLength >= numberCharacters)
			{
				result = original.Substring(originalLength - numberCharacters);
			}
			else
			{
				result = original;
			}

			return result;
		}

		public void Dispose()
		{
			((IDisposable)ftpHelper).Dispose();
		}

		protected Logger Logger => logger ?? (logger = new Logger());
		Logger logger;

		protected FtpHelper FtpHelper => ftpHelper ?? (ftpHelper = new FtpHelper());
		FtpHelper ftpHelper;

		protected List<string> FilesToReplayNextLoop
		{
			get => filesToReplayNextLoop;
			set => filesToReplayNextLoop = value;
		}
		List<string> filesToReplayNextLoop;

		protected const string Production = "Production";
		protected const string Test = "Test";
		protected const string ProductionMode = "PROD";
		protected const string TestMode = "TEST";
		protected const string Info = "INFO";
		protected const string OK = "OK";
		protected const string KO = "KO";

		protected string CurrentMessageContentBeforeAnyChange { get; set; }
		public string UniversalEventContentForTest { get; set; }
	}
}
