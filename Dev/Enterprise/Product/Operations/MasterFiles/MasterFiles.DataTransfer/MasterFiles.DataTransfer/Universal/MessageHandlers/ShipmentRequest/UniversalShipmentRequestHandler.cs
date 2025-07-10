using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalShipmentRequestHandler : UniversalXmlRequestBaseHandler<ShipmentRequest>
	{
		public UniversalShipmentRequestHandler(IXmlSessionTracker xmlSessionTracker) : this(xmlSessionTracker, new DefaultProcessingConfig())
		{
		}

		public UniversalShipmentRequestHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
		{
		}

		protected override string RequestMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalShipmentRequest;

		protected override (IDataObject, bool) GetRequestedDataObject(ShipmentRequest requestDataObject, BusinessObject dataProvider, IXmlSessionTracker xmlSessionTracker)
		{
			var result = UniversalRequestHandlerHelper.GetRequestedDataObject<ShipmentRequest, IShipmentDataContextManager>(
				requestDataObject,
				dataProvider,
				GetWriterStrategy(requestDataObject.MessageProfile),
				(contextManager, writeManager) =>
				{
					var writer = contextManager.GetShipmentDataObjectWriter(writeManager);
					writeManager.OverrideSendCostingData = true;
					return writer;
				});

			if (result.DataObject is ITopLevelDataObject topLevelDataObject)
			{
				ApplyContentFilter(requestDataObject, xmlSessionTracker, result, topLevelDataObject);
			}

			return (result.DataObject, result.Success);
		}

		void ApplyContentFilter(ShipmentRequest requestDataObject, IXmlSessionTracker xmlSessionTracker, UniversalShipmentRequestDataObjectResult result, ITopLevelDataObject topLevelDataObject)
		{
			var messageProfile = requestDataObject.MessageProfile as IMessageProfile;
			var filterApplicator = ObjectFactory.New<IUniversalXmlContentFilterApplicator>();

			if (result.WritingManager != null && ShouldUseContentFilter(result.WritingManager.ContentFilterManager, xmlSessionTracker, requestDataObject.DataContext.ActionPurpose, messageProfile != null, out var filter))
			{
				filterApplicator.RemoveNonExportedCollections(topLevelDataObject, filter);
			}
			else if (messageProfile != null)
			{
				filterApplicator.RemoveNonExportedCollections(topLevelDataObject, messageProfile);
			}
		}

		bool ShouldUseContentFilter(IEDIMessageContentFilterManager contentFilterManager, IXmlSessionTracker xmlSessionTracker, ICodeDescriptionDataObject actionPurpose, bool messageProfileSpecified, out IEDIMessageContentFilter contentFilter)
		{
			contentFilter = null;

			if (actionPurpose == null)
			{
				if (messageProfileSpecified)
				{
					xmlSessionTracker.Log(LogType.Information, Res.GetString("9acca906-8afb-47ac-a719-5f482878dd66", "Purpose Code is not set"));
					LogFallBackToMessageProfile(xmlSessionTracker);
				}

				return false;
			}

			if (contentFilterManager == null)
			{
				return false;
			}

			var purpose = contentFilterManager.EDIMessagePurpose;
			if (purpose == null)
			{
				xmlSessionTracker.Log(LogType.Information, Res.GetString("6f7d7484-46d7-4278-b673-86ec90f75d40", "Purpose Code '{0}' does not exist", actionPurpose.Code));
				if (messageProfileSpecified)
				{
					LogFallBackToMessageProfile(xmlSessionTracker);
				}
				return false;
			}

			contentFilter = contentFilterManager.EDIMessageContentFilter;
			if (contentFilter == null)
			{
				xmlSessionTracker.Log(LogType.Warning, Res.GetString("76bdffdf-9f58-45e3-a096-5432950a1258", "Purpose Code '{0}' is not linked to an EDI Message Profile", purpose.EMP_Code));
				if (messageProfileSpecified)
				{
					LogFallBackToMessageProfile(xmlSessionTracker);
				}
				return false;
			}

			xmlSessionTracker.Log(LogType.Information, Res.GetString("580ef629-7b4e-44bb-bdf5-6eada0310e89", "Purpose Code '{0}' is linked to '{1}' EDI Message Profile, processing using EDI Message Profile '{1}'", purpose.EMP_Code, contentFilter.ECF_Name));
			return true;
		}

		void LogFallBackToMessageProfile(IXmlSessionTracker xmlSessionTracker)
		{
			xmlSessionTracker.Log(LogType.Information, Res.GetString("0f7fc9e8-e3b1-4edf-93c3-ff8aa93506c9", "Falling back to Schema Filter"));
		}

		protected override string MultipleResultsErrorMessage => (NoResString)"The Universal Shipment Query component can only return one UniversalShipment for each query. Multiple results were found using the references provided.";

		internal IDataObjectWriterStrategy GetWriterStrategy(IMessageProfile messageProfile)
			=> ObjectFactory.New<IUniversalXmlContentFilterDataObjectWriterFactory>().Load(messageProfile);
	}
}
