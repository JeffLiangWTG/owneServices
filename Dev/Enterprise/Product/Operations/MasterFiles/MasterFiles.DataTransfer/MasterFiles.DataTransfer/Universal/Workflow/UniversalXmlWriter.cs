using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Workflow
{
	public abstract class UniversalXmlWriter
	{
		protected UniversalXmlWriter(IUniversalActionInfo actionInfo, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO, IEventInfo eventInfo = null)
		{
			this.actionInfo = Argument.NotNull(actionInfo, "IUniversalActionInfo actionInfo");
			this.dataWriterGetter = Argument.NotNull(dataWriterGetter, "Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter");
			this.exportedBO = exportedBO;
			this.eventInfo = eventInfo;
		}

		protected readonly IUniversalActionInfo actionInfo;
		protected readonly IEventInfo eventInfo;
		protected readonly Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter;
		protected readonly BusinessObject exportedBO;

		protected ITopLevelDataObject GetDataObjectToExport(BusinessObject dataContextBO, BusinessObject exportedObject, IDataWritingManager outboundSessionTracker, ITopLevelDataObjectWriter dataWriter = null)
		{
			IExternalFetchHintSupporter externalFetchHintSupporter = exportedObject.Factory;
			using (externalFetchHintSupporter.SetupCreator())
			{
				ITopLevelDataObject exportedData;

				if (dataWriter == null)
				{
					dataWriter = dataWriterGetter(outboundSessionTracker);
					exportedData = dataWriter.GetDataObject(exportedObject);
				}
				else
				{
					exportedData = dataWriter.GetDataObject(exportedBO);
				}

				new DataContextDataObjectWriter().PopulateDataObject(actionInfo, GetDataContextBO(dataWriter, dataContextBO), exportedData?.DataContext, eventInfo);

				return exportedData;
			}
		}

		public static ITopLevelDataObject GetDataObject(RecipientRoleType roleType, BusinessObject topLevelBO, DataContextType? filteredDataContextType = null)
		{
			ITopLevelDataObject result = null;
			if (topLevelBO != null)
			{
				if (topLevelBO.GetUniversalDataContextManager() is IShipmentDataContextManager manager)
				{
					var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(roleType, topLevelBO)) { FilteredDataContextType = filteredDataContextType });
					using (((IExternalFetchHintSupporter)topLevelBO.Factory).SetupCreator())
					{
						result = writer.GetDataObject(topLevelBO);
					}
				}
			}
			return result;
		}

		protected virtual BusinessObject GetDataContextBO(ITopLevelDataObjectWriter dataWriter, BusinessObject dataContextBO) => dataContextBO;
	}
}
