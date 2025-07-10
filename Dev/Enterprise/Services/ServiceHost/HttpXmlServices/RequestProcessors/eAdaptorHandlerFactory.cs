using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using WTG.Foundation.FrameworkExtensions;

namespace Enterprise.Services.ServiceHost
{
	static class eAdaptorHandlerFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It doesn't need to be translated")]
		const string Native = "Native";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It doesn't need to be translated")]
		const string Body = "Body";

		internal static readonly Dictionary<string, string> handlers = new Dictionary<string, string>()
		{
			{ "UniversalShipmentRequest", "UniversalShipmentRequestHandler" },
			{ "UniversalTransactionBatchRequest", "UniversalTransactionBatchRequestHandler" },
			{ "UniversalTransaction", "UniversalTransactionImportHandler" },
			{ "UniversalDocumentRequest", "UniversalDocumentRequestHandler" },
			{ "UniversalEvent", "UniversalEventImportHandler" },
			{ "UniversalShipment", "UniversalShipmentImportHandler" },
			{ "UniversalActivity", "UniversalActivityImportHandler" },
			{ "UniversalActivityRequest", "UniversalActivityRequestHandler" },
			{ "UniversalInterchangeRequeueRequest", "UniversalInterchangeRequeueRequestHandler" },
			{ "UniversalTransactionBatch", "UniversalTransactionBatchImportHandler" },
			{ Native , "NativeXmlRequestHandler" },
		};

#if DEBUG
		static readonly Overridable<GetHandlerHookDelegate> OnGetHandler = new Overridable<GetHandlerHookDelegate>(null);

		public delegate void GetHandlerHookDelegate(ref IHttpXmlMessageHandler handler);

		public static void SetGetHandlerHook(GetHandlerHookDelegate onGetHandler)
		{
			OnGetHandler.Value = onGetHandler;
		}

		static void GetHandlerHookForUnitTests(ref IHttpXmlMessageHandler handler)
		{
			OnGetHandler.Value?.Invoke(ref handler);
		}
#endif

		internal static IHttpXmlMessageHandler GetHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig, string handlerName, SubStreamableStream incomingStream)
		{
#if DEBUG
			IHttpXmlMessageHandler handler = null;
			GetHandlerHookForUnitTests(ref handler);
			if (handler != null)
			{
				return handler;
			}
#endif
			var	nativeEntityName = string.Empty;
			if (handlerName.IsNullOrEmpty() || handlerName == "NativeXmlRequestHandler")
			{
				(handlerName, nativeEntityName) = GetHandlerAndNativeEntityName(incomingStream);
			}
			
			if (handlerName.IsNullOrEmpty())
			{
				return null;
			}

			return handlerName == "NativeXmlRequestHandler"
				? ObjectFactory.Get<IHttpXmlMessageHandler>(handlerName, nativeEntityName, xmlSessionTracker, processingConfig)
				: ObjectFactory.Get<IHttpXmlMessageHandler>(handlerName, xmlSessionTracker, processingConfig);
		}

		static (string handlerName, string nativeEntityName) GetHandlerAndNativeEntityName(Stream incomingStream)
		{
			var (rootElement, nativeEntityName) = ParseInputXmlRequest(incomingStream);
			handlers.TryGetValue(rootElement, out var handlerName);

			return (handlerName, nativeEntityName);
		}

		static (string rootElement, string nativeEntityName) ParseInputXmlRequest(Stream incomingStream)
		{
			var rootElement = string.Empty;
			var entityName = string.Empty;
			incomingStream.Position = 0;
			using (var reader = XmlReader.Create(new StreamReader(incomingStream)))
			{
				try
				{
					if (reader.MoveToContent() == XmlNodeType.Element)
					{
						rootElement = reader.Name;

						if (rootElement == Native)
						{
							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.Element && reader.Name == Body)
								{
									while (reader.Read())
									{
										if (reader.NodeType == XmlNodeType.Element)
										{
											entityName = reader.Name;
											break;
										}
									}
									break;
								}
							}
						}
					}
				}
				catch (XmlException) { }
			}
			return (rootElement, entityName);
		}
	}
}
