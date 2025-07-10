using System;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public abstract class UniversalXmlRequestBaseHandler<TRequest> : UniversalXmlMessageHandler<TRequest>
		where TRequest : TopLevelDataObject, new()
	{
		protected UniversalXmlRequestBaseHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
		{
		}

		protected sealed override IHttpXmlProcessingResult ProcessDataObject(BusinessObjectFactory factory,
			TRequest requestDataObject,
			IHttpXmlEDIMessage requestEdiMessage,
			ICodeMappingManager codeMapper = null,
			IHttpXmlMessageSaver messageSaver = null)
		{
			var result = new HttpXmlProcessingResult() { Status = HttpXmlResultStatusList.Codes.Error };
			var isSuccessful = false;
			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			using (var contextSwitch = new ContextSwitching(requestDataObject.DataContext?.GetEnterpriseServerAndCompanyIDs(), xmlSessionTracker, requestDataObject))
			{
				if (contextSwitch.Success)
				{
					var dataProvider = GetDataProvider(factory, xmlSessionTracker, requestDataObject);
					if (dataProvider != null)
					{
						try
						{
							ThrowExceptionForTest();
							var (dataObject, isRequestSuccessful) = GetRequestedDataObject(requestDataObject, dataProvider, xmlSessionTracker);
							isSuccessful = isRequestSuccessful;
							if (dataObject is ITopLevelDataObject topLevelDataObject && isRequestSuccessful)
							{
								using (topLevelDataObject)
								{
									result.ResponseMessageText = new CargoWise.IO.Shim.SubStreamableStream();
									new XmlWriter().WriteXML(dataObject, result.ResponseMessageText, false);
								}
								if (messageSaver != null)
								{
									LinkResponseToParent(factory, messageSaver.ResponseMessage, dataProvider);
								}
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							xmlSessionTracker.Log(LogType.Error, "EXCEPTION: " + ex.Message);
							ErrorReporter.ReportOnce(ex.Message, ex);
						}
					}
				}
			}

			UpdateStatuses(isSuccessful, result, requestEdiMessage);

			factory.Save();

			messageSaver?.Save(result);

			return result;
		}

		[Conditional("DEBUG")]
		void ThrowExceptionForTest()
		{
#if DEBUG
			if (ExceptionToThrow != null)
			{
				throw ExceptionToThrow;
			}
#endif
		}

#if DEBUG

		public Exception ExceptionToThrow { get; set; }

#endif
		BusinessObject GetDataProvider(BusinessObjectFactory factory, IXmlImportLogger logger, TopLevelDataObject requestDataObject)
		{
			var dataProviders = new DataContextKeyLookup(factory, logger).Match(requestDataObject).ToArray();

			BusinessObject dataProvider = null;

			if (dataProviders.Length == 0)
			{
				logger.Log(LogType.Warning, "There is no business object matching the criteria.");
			}
			else if (dataProviders.Length > 1)
			{
				logger.Log(LogType.Error, MultipleResultsErrorMessage);
			}
			else
			{
				dataProvider = dataProviders[0];
			}
			return dataProvider;
		}

		protected abstract string MultipleResultsErrorMessage { get; }

		protected abstract (IDataObject, bool) GetRequestedDataObject(TRequest requestDataObject, BusinessObject dataProvider, IXmlSessionTracker xmlSessionTracker);

		void LinkResponseToParent(BusinessObjectFactory businessLogicFactory, IHttpXmlRequestResponse responseMessage, BusinessObject businessObject)
		{
			var ediMessage = (IHttpXmlEDIMessage)responseMessage;
			var parentBOWithLogs = businessObject as IStmALogParent;
			new MessageDataExportImportLogLinker(Events.DataExport, businessLogicFactory).LinkMessageToParentBOLogs(ediMessage, parentBOWithLogs);
		}
	}
}
