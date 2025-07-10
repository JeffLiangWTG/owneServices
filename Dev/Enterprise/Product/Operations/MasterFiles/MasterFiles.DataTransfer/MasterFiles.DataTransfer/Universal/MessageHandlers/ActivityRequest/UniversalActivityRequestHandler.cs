using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalActivityRequestHandler : UniversalXmlRequestBaseHandler<ActivityRequest>
	{
		public UniversalActivityRequestHandler(IXmlSessionTracker xmlSessionTracker) : this(xmlSessionTracker, new DefaultProcessingConfig())
		{
		}

		public UniversalActivityRequestHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
		{
		}

		protected override string RequestMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest;

		protected override (IDataObject, bool) GetRequestedDataObject(ActivityRequest requestDataObject, BusinessObject dataProvider, IXmlSessionTracker xmlSessionTracker)
		{
			var result = UniversalRequestHandlerHelper.GetRequestedDataObject<ActivityRequest, IActivityDataContextManager>(requestDataObject, dataProvider, DefaultDataObjectWriterStrategy.Instance,
				(contextManager, writeManager) => contextManager.GetActivityDataObjectWriter(writeManager, shouldIncludeRelatedItems: true));
			return (result.DataObject, result.Success);
		}

		protected override string MultipleResultsErrorMessage => (NoResString)"The Universal Activity Query component can only return one UniversalActivity for each query. Multiple results were found using the references provided.";
	}
}
