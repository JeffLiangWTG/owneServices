using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public abstract class SoapMessageObject
	{
		public SoapMessageObject(string messageText, TRBaseMessage message, LoggingInformation logger)
		{
			Logger = logger ?? new LoggingInformation();
			Message = message;

			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				RootDocument = xmlDocument;

				if (RootDocument.GetElementByPath(SOAPLevelErrorObject.RootErrorPath) is XmlElement soapRootErrorElement)
				{
					InnerMessageObjects = new InnerMessageObjectBase[] { new SOAPLevelErrorObject(soapRootErrorElement) };
				}
				else if (RootDocument.GetElementByPath(SOAPLevelErrorObject.ElementErrorPath) is XmlElement soapLevelErrorMessage)
				{
					InnerMessageObjects = new InnerMessageObjectBase[] { new SOAPLevelErrorObject(soapLevelErrorMessage) };
				}
				else if (RootDocument.GetElementByPath(SOAPLevelExceptionObject.ExceptionPath) is XmlElement soapLevelException)
				{
					InnerMessageObjects = new InnerMessageObjectBase[] { new SOAPLevelExceptionObject(soapLevelException) };
				}
				else
				{
					InnerMessageObjects = RootDocument.GetElementsByPath(InnerMessagePath).Select(GetInnerXmlMessageObject).WhereNotNull().ToArray();
				}
			}
			else
			{
				InnerMessageObjects = Array.Empty<InnerMessageObjectBase>();
			}

			if (!InnerMessageObjects.Any())
			{
				LogForUnrecognizedMessageText();
			}
		}

		#region Public

		public IReadOnlyCollection<InnerMessageObjectBase> InnerMessageObjects { get; protected set; }

		public abstract bool IsValidMessage { get; }

		string formattedMessageText;
		public override string ToString()
		{
			if (formattedMessageText == null)
			{
				formattedMessageText = RootDocument.ToFormattedString();
			}
			return formattedMessageText;
		}

		#endregion

		#region Protected

		protected XmlDocument RootDocument { get; }

		protected virtual InnerMessageObjectBase GetInnerObjectFromDocument(XmlDocument innerXmlDocument)
		{
			InnerMessageObjectBase result = default;

			if (innerXmlDocument.GetElementsByPath(InnerXmlErrorMessagesObject.MatchPath).Any())
			{
				result = new InnerXmlErrorMessagesObject(innerXmlDocument);
			}
			else if (InnerXmlRegistrationNumberObject.IsMatch(innerXmlDocument))
			{
				result = new InnerXmlRegistrationNumberObject(innerXmlDocument);
			}

			return result;
		}

		protected virtual InnerMessageObjectBase GetInnerObjectFromElement(XmlElement innerElement) => new InnerMessageObjectBase(innerElement.InnerText);

		protected virtual (string ElementName, string NamespaceUrl)[] InnerMessagePath => new (string, string)[]
		{
			(TRMessageConstants.Xml.DiffGramElementName, TRMessageConstants.Xml.DiffGramNamespace),
			(TRMessageConstants.Xml.ResultElementName, string.Empty),
			(TRMessageConstants.Xml.CustomsResponseElementName, string.Empty),
		};

		#endregion

		#region Private

		LoggingInformation Logger { get; }

		void LogForUnrecognizedMessageText()
		{
			Logger.LogError($"Unrecognized message text, type: {Message.EM_MessageType}, number: {Message.EM_MessageNum}");
		}

		TRBaseMessage Message { get; }

		InnerMessageObjectBase GetInnerXmlMessageObject(XmlElement innerElement)
		{
			InnerMessageObjectBase result;
			if (XmlUtils.IsValidXml(innerElement.InnerText, out var innerDocumnent))
			{
				result = GetInnerObjectFromDocument(innerDocumnent);
				ReplaceInnerXml(innerElement, innerDocumnent);
			}
			else
			{
				result = GetInnerObjectFromElement(innerElement);
			}
			return result;
		}

		void ReplaceInnerXml(XmlElement element, XmlDocument withDocument)
		{
			element.InnerXml = withDocument.ToFormattedString();
		}

		#endregion
	}
}
