using System.Linq;
using CargoWise.Types;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer
{
	public static class DtbDataObjectReaderHelper
	{
		public static bool MessageIsFromTestCba(string sender, TopLevelDataObject topLevelDataObject)
		{
			return SenderIsTestCbaId(sender) && topLevelDataObject != null && topLevelDataObject.HasRecipientRole(RecipientRoleType.BKP);
		}

		static bool SenderIsTestCbaId(string sender)
		{
			var testCBAIdregistry = TransportRegistry.Instance.TestCbaId;
			return (testCBAIdregistry != null && !string.IsNullOrEmpty(testCBAIdregistry.Value) && sender == testCBAIdregistry.Value);
		}

		public static (string interchangeType, string sender) GetInterchangeTypeAndSenderFromLogger(IXmlImportLogger logger)
		{
			var xmlLogger = logger as IXmlSessionTracker;
			var interchange = xmlLogger?.SourceMessage?.Interchange;
			var interchangeType = interchange?.EI_TransportType ?? string.Empty;
			var sender = interchange?.EI_From ?? string.Empty;
			return (interchangeType: interchangeType, sender: sender);
		}

		public static ZBool HasSourceContext(IDataContextDataObject dataContext, DataContextType contextType)
		{
			var dataSources = dataContext != null ? dataContext.DataSourceCollection : null;
			return dataSources != null && dataSources.Any(ds => ds.Type.GetValueOrDefault() == contextType.ToString());
		}
	}
}
