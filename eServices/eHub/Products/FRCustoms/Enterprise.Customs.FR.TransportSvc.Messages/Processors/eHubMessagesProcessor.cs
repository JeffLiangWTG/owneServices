using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml;
using CargoWise.eHub.Adapter;
using Enterprise.Customs.FR.TransportSvc.Utilities;
using Newtonsoft.Json.Linq;

namespace Enterprise.Customs.FR.TransportSvc.Messages
{
	public class eHubMessagesProcessor : BaseMessagesProcessor
	{
		public eHubMessagesProcessor(Logger logger) : base(logger)
		{
			System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
		}

		public bool ProcessIncomingCW1Messages()
		{
			FilesToReplayNextLoop = new List<string>();

			MessagesQueueManager.DownloadIncomingMessages(Logger);
			var messageList = MessagesQueueManager.RebuildCW1MessagesQueue(Logger); //There could be messages pending processing in working directory

			if (messageList.Count > 0)
			{
				FtpHelper.FtpConnect(Logger);

				foreach (var ieHubMessage in messageList)
				{
					try
					{
						CurrentMessageContentBeforeAnyChange = ToolBox.StreamToString(ieHubMessage.MessageStream);
						Process(ieHubMessage);
					}
					catch (Exception ex)
					{
						var exceptionType = ex.GetType();
						switch (exceptionType.Name)
						{
							case "XmlSchemaValidationException":
								Logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.MessageValidationError, verboseModeOnly: false);
								break;
							default:
								Logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.UnkownError, verboseModeOnly: false);
								break;
						}

						if (IsNetworkException(ex))
						{
							var currentMessageFilePath = Path.Combine(WorkingDirectory, ieHubMessage.Filename);
							RevertAnyChangeAndAddToNextRoundQueue(currentMessageFilePath);
						}

						try
						{
							SendUniversalEvent(ieHubMessage, Constants.UniversalEvent.Statuses.MessageNotDelivered, ex.Message, string.Empty);
						}
						catch
						{
						}
						finally
						{
							ieHubMessage?.MessageStream?.Close();
							Logger.AddNotification($"eHub message {ieHubMessage.TrackingID} processed KO.", Notification.MessageType.Error, Notification.Events.ProcessOver, verboseModeOnly: false);
						}
					}
					finally
					{
						ieHubMessage?.MessageStream?.Close();
					}
				}

				CleanDirectoryWisely(WorkingDirectory);
				FtpHelper.DisconnectAndWait(Logger);
			}
			return true;
		}

		public virtual bool Process(IeHubMessage ieHubMessage)
		{
			if (ieHubMessage != null)
			{
				Logger.AddNotification($"Processing eHub message {ieHubMessage.TrackingID}.", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: false);

				var xmlOutputSourceDoc = LoadMessage(ieHubMessage);
				xmlOutputSourceDoc = AddEnveloppeEasylogInformation(xmlOutputSourceDoc);
				xmlOutputSourceDoc = PerformAdditionalChanges(xmlOutputSourceDoc);
				xmlOutputSourceDoc = RemoveMessageNameSpaces(xmlOutputSourceDoc);
				var isUCC6 = IsUCC6(xmlOutputSourceDoc);
				var isTest = IsTest(xmlOutputSourceDoc);
				using (var transformedMessage = GetTransformedMessage(xmlOutputSourceDoc, isResponse: false))
				{
					UploadTransformedMessageToPlatform(ieHubMessage, transformedMessage, isUCC6, isTest);
				}
			}

			return true;
		}

		void UploadTransformedMessageToPlatform(IeHubMessage ieHubMessage, StringWriterWithEncoding transformedMessage, bool isUCC6, bool isTest)
		{
			if (transformedMessage != null)
			{
				var fileContent = transformedMessage.ToString();
				var fileName = ieHubMessage?.TrackingID.ToString() + ".xml";
				var localFilePath = Path.Combine(WorkingDirectory, fileName);

				File.WriteAllText(localFilePath, fileContent);

				if (!ExecuteForDebugging)
				{
					var ftpPath = FtpHelper.GetFtpPathForUpload(isUCC6, isTest);
					FtpHelper.UploadFileToFTP(transformedMessage.ToString(), localFilePath, fileName, ftpPath);
				}

				Logger.AddNotification($"Transformed file {fileName} uploaded to platform.", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: true);

				SendUniversalEvent(ieHubMessage, Constants.UniversalEvent.Statuses.MessageDelivered, string.Empty, fileContent);

				File.Delete(localFilePath);
				transformedMessage.Dispose();
				Logger.AddNotification($"eHub message {ieHubMessage.TrackingID} processed OK.", Notification.MessageType.Information, Notification.Events.ProcessOver, verboseModeOnly: false);
			}
			else
			{
				Logger.AddNotification($"Transformation didn't give any result. eHub message {ieHubMessage.TrackingID} processed KO.", Notification.MessageType.Information, Notification.Events.ProcessOver, verboseModeOnly: false);
			}
		}

		public static XmlDocument RemoveMessageNameSpaces(XmlDocument xmlOutputSourceDoc)
		{
			if (xmlOutputSourceDoc != null)
			{
				xmlOutputSourceDoc.LoadXml(xmlOutputSourceDoc.OuterXml.Replace("ns0:GenericMessageInterchange", "root"));
				xmlOutputSourceDoc.LoadXml(ToolBox.XmlNoNamespace(xmlOutputSourceDoc.OuterXml));
			}

			return xmlOutputSourceDoc;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public XmlDocument AddEnveloppeEasylogInformation(XmlDocument messageDocument)
		{
			if (messageDocument == null)
			{
				return messageDocument;
			}

			//Creation of a new "EnveloppeEasylog" node
			var xmlMessageRoot = ToolBox.GetElementSafely(messageDocument.DocumentElement);
			var xmlMessageBody = ToolBox.GetElementSafely(xmlMessageRoot["Body"]);
			var xmlMessageMessage = ToolBox.GetElementSafely(xmlMessageBody["Message"]) ?? xmlMessageBody;
			var xmlMessageEnveloppeMessage = ToolBox.GetElementSafely(xmlMessageMessage["EnveloppeMessage"]);
			XmlNode xmlEnveloppeEasylog = messageDocument.CreateElement("EnveloppeEasylog");
			xmlMessageRoot.AppendChild(xmlEnveloppeEasylog);

			var isDeltaIE = IsDeltaIEInterchange(messageDocument);
			var jsonMessageObject = isDeltaIE ? SearchForJson(messageDocument) : null;
			var isPNTS = IsPNTSInterchange(messageDocument);
			try
			{
				var schemaID = GetSchemaID(messageDocument);

				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "SenderId", GetSenderId(messageDocument));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "SequentialNumber", GetSequentialNumber(messageDocument, isDeltaIE));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "IdDossier", GetIdDossier(messageDocument, jsonMessageObject, isDeltaIE, isPNTS));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "Flux", GetFlux(schemaID));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "MessageType", GetMessageType(schemaID));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "Application", GetApplication(schemaID));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "Mode", GetMode(ToolBox.GetElementTextByTagNameSafely(messageDocument, "RecipientID"), GetApplication(schemaID) == Constants.ApplicationTypes.APPLUS));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "PartyId", GetPartyId(messageDocument, isDeltaIE, isPNTS));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "DeclarationReference", GetDeclarationReference(messageDocument, schemaID, jsonMessageObject));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "DeltaReference", GetDeltaReference(messageDocument, schemaID));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "CodeClient", Constants.CW1);
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "CodeSociete", Constants.CW1);
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "DateYYYYMMDD", GetDate());
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "HourLong", GetLongTime());
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "MessageId", GetMessageId(messageDocument, isPNTS));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "TransactionId", GetTransactionId(messageDocument, schemaID, jsonMessageObject));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "CodAct", GetActionCode(messageDocument));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "LRN", GetLRN(messageDocument, schemaID, jsonMessageObject, isDeltaIE, isPNTS));
				ToolBox.AddElement(xmlEnveloppeEasylog, messageDocument, "MRN", GetMRN(schemaID, jsonMessageObject, isDeltaIE));
				if (xmlMessageEnveloppeMessage != null)
				{
					xmlMessageEnveloppeMessage["transactionId"].InnerText = GetTransactionId(messageDocument, schemaID, jsonMessageObject); //Not sure this is of any use anymore
				}
				return messageDocument;
			}
			catch
			{
				throw;
			}
		}

		static string GetSenderId(XmlDocument messageDocument) => ToolBox.GetElementTextByTagNameSafely(messageDocument, "SenderID");

		static string GetSequentialNumber(XmlDocument messageDocument, bool isDeltaIE) => isDeltaIE ? string.Empty : ToolBox.GetElementTextByTagNameSafely(messageDocument, "numseq", "0");

		static string GetIdDossier(XmlDocument messageDocument, dynamic jsonMessageObject, bool isDeltaIE, bool isPNTS)
		{
			var idDossier = string.Empty;

			if (isDeltaIE)
			{
				idDossier = Right(jsonMessageObject.TransactionId?.Value ?? string.Empty, 10);
			}
			else if (isPNTS)
			{
				idDossier = ToolBox.GetElementTextByTagNameSafely(messageDocument, "refToMessageId");
			}
			else
			{
				idDossier = ToolBox.GetElementTextByTagNameSafely(messageDocument, "transactionId");
			}

			return idDossier;
		}

		static string GetApplication(string schemaId)
		{
			switch (schemaId)
			{
				case Constants.MessageSchemas.DeltaCImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaCExportDeclarationSchema:
					return Constants.ApplicationTypes.DeltaC;
				case Constants.MessageSchemas.DeltaDImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDExportDeclarationSchema:
				case Constants.MessageSchemas.DcgSchema:
					return Constants.ApplicationTypes.DeltaDV30;
				case Constants.MessageSchemas.CodSchema:
					return Constants.ApplicationTypes.DeltaDG;
				case Constants.MessageSchemas.IE007Schema:
				case Constants.MessageSchemas.IE013Schema:
				case Constants.MessageSchemas.IE014Schema:
				case Constants.MessageSchemas.IE015Schema:
				case Constants.MessageSchemas.IEF15Schema:
				case Constants.MessageSchemas.IE044Schema:
				case Constants.MessageSchemas.IE141Schema:
				case Constants.MessageSchemas.CC007CSchema:
				case Constants.MessageSchemas.CC013CSchema:
				case Constants.MessageSchemas.CC014CSchema:
				case Constants.MessageSchemas.CC015CSchema:
				case Constants.MessageSchemas.CC034CSchema:
				case Constants.MessageSchemas.CC044CSchema:
				case Constants.MessageSchemas.CC141CSchema:
				case Constants.MessageSchemas.CC170CSchema:
					return Constants.ApplicationTypes.DeltaT;
				case Constants.MessageSchemas.CIN745Schema:
				case Constants.MessageSchemas.CIN750Schema:
				case Constants.MessageSchemas.CIN755Schema:
					return Constants.ApplicationTypes.CIN;
				case Constants.MessageSchemas.IE507Schema:
				case Constants.MessageSchemas.IE618Schema:
					return Constants.ApplicationTypes.ECS;
				case Constants.MessageSchemas.APPLUSSchema:
					return Constants.ApplicationTypes.APPLUS;
				case Constants.MessageSchemas.IE413Schema:
				case Constants.MessageSchemas.IE414Schema:
				case Constants.MessageSchemas.IE415Schema:
				case Constants.MessageSchemas.IE432Schema:
				case Constants.MessageSchemas.IE433Schema:
					return Constants.ApplicationTypes.DeltaIE;
				case Constants.MessageSchemas.IETS007Schema:
				case Constants.MessageSchemas.IETS015Schema:
				case Constants.MessageSchemas.IETS115Schema:
				case Constants.MessageSchemas.IETS413Schema:
				case Constants.MessageSchemas.IETS414Schema:
					return Constants.ApplicationTypes.PNTS;
				default:
					throw new XmlException("Unknown SchemaID value");
			}
		}

		static string GetFlux(string schemaId)
		{
			switch (schemaId)
			{
				case Constants.MessageSchemas.DeltaCImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDImportDeclarationSchema:
				case Constants.MessageSchemas.IE413Schema:
				case Constants.MessageSchemas.IE414Schema:
				case Constants.MessageSchemas.IE415Schema:
				case Constants.MessageSchemas.IE432Schema:
				case Constants.MessageSchemas.IE433Schema:
					return Constants.MessageFlows.FluxImpType;
				case Constants.MessageSchemas.DeltaCExportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDExportDeclarationSchema:
					return Constants.MessageFlows.FluxExpType;
				case Constants.MessageSchemas.DcgSchema:
					return Constants.MessageFlows.FluxDCG;
				case Constants.MessageSchemas.CodSchema:
					return Constants.MessageFlows.FluxCOD;
				case Constants.MessageSchemas.IE007Schema:
				case Constants.MessageSchemas.CC007CSchema:
				case Constants.MessageSchemas.IE044Schema:
				case Constants.MessageSchemas.CC044CSchema:
					return Constants.MessageFlows.FluxDeltaTArrivee;
				case Constants.MessageSchemas.IE013Schema:
				case Constants.MessageSchemas.CC013CSchema:
				case Constants.MessageSchemas.IE014Schema:
				case Constants.MessageSchemas.CC014CSchema:
				case Constants.MessageSchemas.IE015Schema:
				case Constants.MessageSchemas.CC015CSchema:
				case Constants.MessageSchemas.CC034CSchema:
				case Constants.MessageSchemas.IEF15Schema:
				case Constants.MessageSchemas.IE141Schema:
				case Constants.MessageSchemas.CC141CSchema:
				case Constants.MessageSchemas.CC170CSchema:
					return Constants.MessageFlows.FluxDeltaTDepart;
				case Constants.MessageSchemas.CIN745Schema:
				case Constants.MessageSchemas.CIN750Schema:
				case Constants.MessageSchemas.CIN755Schema:
					return Constants.MessageFlows.FluxCIN;
				case Constants.MessageSchemas.IE507Schema:
				case Constants.MessageSchemas.IE618Schema:
					return Constants.MessageFlows.FluxECS;
				case Constants.MessageSchemas.APPLUSSchema:
					return Constants.MessageFlows.FluxNSTI;
				case Constants.MessageSchemas.IETS007Schema:
				case Constants.MessageSchemas.IETS015Schema:
				case Constants.MessageSchemas.IETS115Schema:
				case Constants.MessageSchemas.IETS413Schema:
				case Constants.MessageSchemas.IETS414Schema:
					return Constants.MessageFlows.FluxPNTS;
				default:
					throw new XmlException("Unknown SchemaID value");
			}
		}

		static string GetMessageType(string schemaId)
		{
			switch (schemaId)
			{
				case Constants.MessageSchemas.DeltaCImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaCExportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDExportDeclarationSchema:
				case Constants.MessageSchemas.DcgSchema:
					return Constants.MessageTypes.Delta;
				case Constants.MessageSchemas.CodSchema:
					return Constants.MessageTypes.COD;
				case Constants.MessageSchemas.IE007Schema:
					return Constants.MessageTypes.DeltaT.IE007;
				case Constants.MessageSchemas.IE013Schema:
					return Constants.MessageTypes.DeltaT.IE013;
				case Constants.MessageSchemas.IE014Schema:
					return Constants.MessageTypes.DeltaT.IE014;
				case Constants.MessageSchemas.IE015Schema:
					return Constants.MessageTypes.DeltaT.IE015;
				case Constants.MessageSchemas.IEF15Schema:
					return Constants.MessageTypes.DeltaT.IEF15;
				case Constants.MessageSchemas.IE044Schema:
					return Constants.MessageTypes.DeltaT.IE044;
				case Constants.MessageSchemas.IE141Schema:
					return Constants.MessageTypes.DeltaT.IE141;
				case Constants.MessageSchemas.CC007CSchema:
					return Constants.MessageTypes.DeltaT.IE007;
				case Constants.MessageSchemas.CC013CSchema:
					return Constants.MessageTypes.DeltaT.IE013;
				case Constants.MessageSchemas.CC014CSchema:
					return Constants.MessageTypes.DeltaT.IE014;
				case Constants.MessageSchemas.CC015CSchema:
					return Constants.MessageTypes.DeltaT.IE015;
				case Constants.MessageSchemas.CC034CSchema:
					return Constants.MessageTypes.DeltaT.IE034;
				case Constants.MessageSchemas.CC044CSchema:
					return Constants.MessageTypes.DeltaT.IE044;
				case Constants.MessageSchemas.CC141CSchema:
					return Constants.MessageTypes.DeltaT.IE141;
				case Constants.MessageSchemas.CC170CSchema:
					return Constants.MessageTypes.DeltaT.IE170;
				case Constants.MessageSchemas.CIN745Schema:
				case Constants.MessageSchemas.CIN750Schema:
				case Constants.MessageSchemas.CIN755Schema:
					return Constants.MessageTypes.CIN;
				case Constants.MessageSchemas.IE507Schema:
				case Constants.MessageSchemas.IE618Schema:
					return Constants.MessageFlows.FluxECS;
				case Constants.MessageSchemas.APPLUSSchema:
					return Constants.MessageTypes.APPLUS;
				case Constants.MessageSchemas.IE413Schema:
					return Constants.MessageTypes.DeltaIE.IE413;
				case Constants.MessageSchemas.IE414Schema:
					return Constants.MessageTypes.DeltaIE.IE414;
				case Constants.MessageSchemas.IE415Schema:
					return Constants.MessageTypes.DeltaIE.IE415;
				case Constants.MessageSchemas.IE432Schema:
					return Constants.MessageTypes.DeltaIE.IE432;
				case Constants.MessageSchemas.IE433Schema:
					return Constants.MessageTypes.DeltaIE.IE433;
				case Constants.MessageSchemas.IETS007Schema:
					return Constants.MessageTypes.PNTS.IETS007;
				case Constants.MessageSchemas.IETS015Schema:
					return Constants.MessageTypes.PNTS.IETS015;
				case Constants.MessageSchemas.IETS115Schema:
					return Constants.MessageTypes.PNTS.IETS115;
				case Constants.MessageSchemas.IETS413Schema:
					return Constants.MessageTypes.PNTS.IETS413;
				case Constants.MessageSchemas.IETS414Schema:
					return Constants.MessageTypes.PNTS.IETS414;
				default:
					throw new XmlException("Unknown SchemaID value");
			}
		}

		static string GetPartyId(XmlDocument messageDocument, bool isDeltaIE, bool isPNTS) => isDeltaIE || isPNTS ? string.Empty : ToolBox.GetElementTextByTagNameSafely(messageDocument, "partyId");

		static string GetDeclarationReference(XmlDocument messageDocument, string schemaId, dynamic jsonMessageObject)
		{
			switch (schemaId)
			{
				case Constants.MessageSchemas.CIN750Schema:
					return ToolBox.GetElementTextByTagNameSafely(messageDocument, "temporaryStorageDeclaration");
				case Constants.MessageSchemas.CIN745Schema:
					var declarationreference = string.Empty;
					var nodeList = messageDocument.GetElementsByTagName("transactionID");
					if (nodeList != null && nodeList.Count > 1)
					{
						declarationreference =  nodeList[1].InnerText;
					}
					return declarationreference;
				case Constants.MessageSchemas.IE507Schema:
				case Constants.MessageSchemas.IE618Schema:
					return CleanTransactionID(ToolBox.GetElementTextByTagNameSafely(messageDocument, "transactionID"));
				case Constants.MessageTypes.APPLUS:
					return GetAPPLUSJobReference(messageDocument);
				case Constants.MessageSchemas.IE413Schema:
				case Constants.MessageSchemas.IE414Schema:
				case Constants.MessageSchemas.IE415Schema:
				case Constants.MessageSchemas.IE432Schema:
				case Constants.MessageSchemas.IE433Schema:
					return Right(jsonMessageObject.TransactionId?.Value ?? string.Empty, 10);
				case Constants.MessageSchemas.IETS007Schema:
				case Constants.MessageSchemas.IETS015Schema:
				case Constants.MessageSchemas.IETS115Schema:
				case Constants.MessageSchemas.IETS413Schema:
				case Constants.MessageSchemas.IETS414Schema:
					return ToolBox.GetElementTextByTagNameSafely(messageDocument, "lrn");
				case Constants.MessageSchemas.CC007CSchema:
				case Constants.MessageSchemas.CC013CSchema:
				case Constants.MessageSchemas.CC014CSchema:
				case Constants.MessageSchemas.CC015CSchema:
				case Constants.MessageSchemas.CC034CSchema:
				case Constants.MessageSchemas.CC044CSchema:
				case Constants.MessageSchemas.CC141CSchema:
				case Constants.MessageSchemas.CC170CSchema:
					var lrn = ToolBox.GetElementTextByTagNameSafely(messageDocument, "LRN");
					return !string.IsNullOrEmpty(lrn) ? lrn : ToolBox.GetElementTextByTagNameSafely(messageDocument, "MRN");
				default:
					return ToolBox.GetElementTextByTagNameSafely(messageDocument, "refdos");
			}
		}

		static string GetDeltaReference(XmlDocument messageDocument, string schemaId)
		{
			switch (schemaId)
			{
				case Constants.MessageSchemas.IE413Schema:
				case Constants.MessageSchemas.IE414Schema:
				case Constants.MessageSchemas.IE415Schema:
				case Constants.MessageSchemas.IE432Schema:
				case Constants.MessageSchemas.IE433Schema:
					return string.Empty;
				case Constants.MessageSchemas.IETS007Schema:
				case Constants.MessageSchemas.IETS015Schema:
				case Constants.MessageSchemas.IETS115Schema:
				case Constants.MessageSchemas.IETS413Schema:
				case Constants.MessageSchemas.IETS414Schema:
					return ToolBox.GetElementTextByTagNameSafely(messageDocument, "crn");
				case Constants.MessageSchemas.CC007CSchema:
				case Constants.MessageSchemas.CC013CSchema:
				case Constants.MessageSchemas.CC014CSchema:
				case Constants.MessageSchemas.CC015CSchema:
				case Constants.MessageSchemas.CC034CSchema:
				case Constants.MessageSchemas.CC044CSchema:
				case Constants.MessageSchemas.CC141CSchema:
				case Constants.MessageSchemas.CC170CSchema:
					return ToolBox.GetElementTextByTagNameSafely(messageDocument, "CRN");
				default:
					return ToolBox.GetElementTextByTagNameSafely(messageDocument, "refdec");
			}
		}

		static string GetMessageId(XmlDocument messageDocument, bool isPNTS) => isPNTS ? ToolBox.GetElementTextByTagNameSafely(messageDocument, "messageId") : string.Empty;

		static string GetTransactionId(XmlDocument messageDocument, string schemaId, dynamic jsonMessageObject)
		{
			var senderId = GetSenderId(messageDocument);
			var typeFlux = GetFlux(schemaId);
			switch (schemaId)
			{
				case Constants.MessageSchemas.IE507Schema:
				case Constants.MessageSchemas.IE618Schema:
					return ToolBox.GetElementTextByTagNameSafely(messageDocument, "transactionId");
				case Constants.MessageSchemas.IETS007Schema:
				case Constants.MessageSchemas.IETS015Schema:
				case Constants.MessageSchemas.IETS115Schema:
				case Constants.MessageSchemas.IETS413Schema:
				case Constants.MessageSchemas.IETS414Schema:
					return senderId + "+" + typeFlux + "+" + ToolBox.GetElementTextByTagNameSafely(messageDocument, "refToMessageId");
				case Constants.MessageSchemas.APPLUSSchema:
					return senderId + "+" + ToolBox.GetAttributeValueSafely(messageDocument, "Request", "type") + "+" + ToolBox.GetElementTextByTagNameSafely(messageDocument, "InterchangeNumber");
				case Constants.MessageSchemas.IE413Schema:
				case Constants.MessageSchemas.IE414Schema:
				case Constants.MessageSchemas.IE415Schema:
				case Constants.MessageSchemas.IE432Schema:
				case Constants.MessageSchemas.IE433Schema:
					return senderId + "+" + typeFlux + "+" + Right(jsonMessageObject.TransactionId?.Value ?? string.Empty, 10);
				case Constants.MessageSchemas.CC007CSchema:
				case Constants.MessageSchemas.CC044CSchema:
					return senderId + "+T5A+" + ToolBox.GetElementTextByTagNameSafely(messageDocument, "transactionId");
				case Constants.MessageSchemas.CC013CSchema:
				case Constants.MessageSchemas.CC014CSchema:
				case Constants.MessageSchemas.CC015CSchema:
				case Constants.MessageSchemas.CC034CSchema:
				case Constants.MessageSchemas.CC141CSchema:
				case Constants.MessageSchemas.CC170CSchema:
					return senderId + "+T5D+" + ToolBox.GetElementTextByTagNameSafely(messageDocument, "transactionId");
				default:
					return senderId + "+" + typeFlux + "+" + ToolBox.GetElementTextByTagNameSafely(messageDocument, "transactionId");
			}
		}

		static string GetActionCode(XmlDocument messageDocument) => ToolBox.GetElementTextByTagNameSafely(messageDocument, "codact");

		static string GetLRN(XmlDocument messageDocument, string schemaId, dynamic jsonMessageObject, bool isDeltaIE, bool isPNTS)
		{
			var lrn = string.Empty;
			if (isDeltaIE)
			{
				if (schemaId == Constants.MessageSchemas.IE414Schema)
				{
					lrn = jsonMessageObject.MessageJson?.ImportOperation[0]?.LRN?.Value ?? string.Empty;
				}
				else
				{
					lrn = jsonMessageObject.MessageJson?.ImportOperation?.LRN?.Value ?? string.Empty;
				}

			}
			else if (isPNTS)
			{
				lrn = ToolBox.GetElementTextByTagNameSafely(messageDocument, "lrn");
			}

			return lrn;
		}

		static string GetMRN(string schemaId, dynamic jsonMessageObject, bool isDeltaIE)
		{
			var mrn = string.Empty;
			if (isDeltaIE && schemaId != Constants.MessageSchemas.IE414Schema)
			{
				if (schemaId != Constants.MessageSchemas.IE414Schema)
				{
					mrn = jsonMessageObject.MessageJson?.ImportOperation?.MRN?.Value ?? string.Empty;
				}
			}
			return mrn;
		}

		public static XmlDocument PerformAdditionalChanges(XmlDocument messageDocument)
		{
			try
			{
				if (IsDeltaTPhase4(GetSchemaID(messageDocument)))
				{
					messageDocument = UpdateDeltaTPhase4Tags(messageDocument);
				}
				else if (IsDeltaIEInterchange(messageDocument))
				{
					messageDocument = AddPrunedEmbeddedJsonMessage(messageDocument);
				}

				return messageDocument;
			}
			catch
			{
				throw;
			}
		}

		static XmlDocument UpdateDeltaTPhase4Tags(XmlDocument messageDocument)
		{
			var newIntConRefMES11 = GetNewMESInterchangeControlReference();
			messageDocument = ToolBox.UpdateElementTextByTagName(messageDocument, "IntConRefMES11", newIntConRefMES11);
			messageDocument = ToolBox.UpdateElementTextByTagName(messageDocument, "MesIdeMES19", newIntConRefMES11);
			return messageDocument;
		}

		static dynamic SearchForJson(XmlDocument messageDocument)
		{
			var jsonMessage = ToolBox.GetElementTextByTagNameSafely(messageDocument, "Body");
			dynamic jsonMessageObject = JObject.Parse(jsonMessage);

			return jsonMessageObject;
		}

		public static XmlDocument AddPrunedEmbeddedJsonMessage(XmlDocument messageDocument)
		{
			if (messageDocument != null)
			{
				var xmlMessageRoot = ToolBox.GetElementSafely(messageDocument.DocumentElement);
				var jsonMessage = ToolBox.GetElementTextByTagNameSafely(messageDocument, "Body");
				var boundary = "\"MessageJson\":";
				var startIndexOfMesageForCustoms = jsonMessage.IndexOf(boundary) + boundary.Length;
				ToolBox.AddElement(xmlMessageRoot, messageDocument, "MessageJson", jsonMessage.Substring(startIndexOfMesageForCustoms, jsonMessage.Length - startIndexOfMesageForCustoms - 1));
			}

			return messageDocument;
		}

		void SendUniversalEvent(IeHubMessage ieHubMessage, string eventType, string eventAdditionalInfo, string transformedMessage)
		{
			try
			{
				Logger.AddNotification("Sending a " + eventType + " notification back to CW1.", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: true);

				var outputFilePath = CreateUniversalEventMessageFile(ieHubMessage, eventType, eventAdditionalInfo, transformedMessage);
				if (!string.IsNullOrEmpty(outputFilePath))
				{
					if (!ExecuteForDebugging)
					{
						SendFileToCW1(outputFilePath, ieHubMessage.RecipientID, ieHubMessage.SenderID); //This is where we switch Recipent/Sender
					}
					Logger.AddNotification($"{eventType} notification sent to CW1.", Notification.MessageType.Information, Notification.Events.NotificationSent, verboseModeOnly: false);
					File.Delete(outputFilePath);
				}
				else
				{
					Logger.AddNotification($"{eventType} notification not sent to CW1. Unable to create {outputFilePath}.", Notification.MessageType.Information, Notification.Events.NotificationSent, verboseModeOnly: false);
				}
			}
			catch (Exception ex)
			{
				Logger.AddNotification($"{eventType} notification not sent to CW1: {ex.Message}", Notification.MessageType.Information, Notification.Events.NotificationSent, verboseModeOnly: false);
			}
		}

		string CreateUniversalEventMessageFile(IeHubMessage ieHubMessage, string eventStatus, string eventAdditionalInfo, string transformedMessage = "")
		{
			var messageDocument = new XmlDocument();
			if (string.IsNullOrEmpty(transformedMessage))
			{
				messageDocument = LoadMessage(ieHubMessage);
			}
			else
			{
				messageDocument.LoadXml(transformedMessage);
			}

			var schemaID = GetSchemaID(messageDocument);
			var targetType = UniversalEventHelper.GetTargetType(schemaID);
			var eventType = UniversalEventHelper.GetEventType(schemaID);
			var universalEventRecipientID = ieHubMessage.SenderID;
			var universalEventSenderID = ieHubMessage.RecipientID;
			var interchangeNumber = ToolBox.GetElementTextByTagNameSafely(messageDocument, "InterchangeNumber");
			var transactionID = CleanTransactionID(ToolBox.GetElementTextByTagNameSafely(messageDocument, "transactionId"));
			var jobReference = schemaID == Constants.MessageSchemas.APPLUSSchema ? GetAPPLUSJobReference(messageDocument) : ToolBox.GetElementTextByTagNameSafely(messageDocument, "RefDossier");
			var declarationReference = GetJobReference(jobReference);
			var eventDate = GetDateTime();
			var xmlUniversalEventMessage = UniversalEventHelper.GetUniversalMessageContent(AssemblyPath, targetType, eventType, eventStatus, eventAdditionalInfo, eventDate, universalEventRecipientID, universalEventSenderID, declarationReference, interchangeNumber, transactionID);
			var outputFilePath = UniversalEventHelper.GetUniversalEventFilePath(WorkingDirectory, eventStatus, ieHubMessage.TrackingID.ToString());
			File.WriteAllText(outputFilePath, xmlUniversalEventMessage);

#pragma warning disable WTG2002 // Avoid conditional compilation based on DEBUG.
#if DEBUG
			UniversalEventContentForTest = xmlUniversalEventMessage;
#pragma warning restore WTG2002 // Avoid conditional compilation based on DEBUG.
#endif

			return outputFilePath;
		}

		public static XmlDocument LoadMessage(IeHubMessage ieHubMessage)
		{
			if (ieHubMessage == null)
			{
				throw new MessageException("Message is null", null);
			}

			if (ieHubMessage.MessageStream.Length == 0)
			{
				throw new MessageException("Message is empty", null);
			}

			//Creation of the input XML handler
			var xmlInput = ToolBox.StreamToString(ieHubMessage.MessageStream);
			var messageDocument = new XmlDocument();
			messageDocument.LoadXml(xmlInput);

			return messageDocument;
		}
	}
}
