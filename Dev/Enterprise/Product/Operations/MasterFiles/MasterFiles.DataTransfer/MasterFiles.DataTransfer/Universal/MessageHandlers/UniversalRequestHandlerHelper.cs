using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	static class UniversalRequestHandlerHelper
	{
		internal static UniversalShipmentRequestDataObjectResult GetRequestedDataObject<TRequest, TDataContextManager>(TRequest request, BusinessObject dataProvider, IDataObjectWriterStrategy writerStrategy, Func<TDataContextManager, DataWritingManager, ITopLevelDataObjectWriter> getWriterFunc)
			where TRequest : TopLevelDataObject, new()
			where TDataContextManager : class, IDataContextManager
		{
			var recipientRoles = GetRecipientRoles(request);
			var manager = dataProvider.GetUniversalDataContextManager() as TDataContextManager;

			if (manager == null)
			{
				return new UniversalShipmentRequestDataObjectResult(null, false, null);
			}

			var actionInfo = new ActionInfo(recipientRoles.ToRecipientRoleDetails(), dataProvider);
			var writingManager = GetDataWritingManager(request, actionInfo, writerStrategy, dataProvider.Factory);

			using (((IExternalFetchHintSupporter)dataProvider.Factory).SetupCreator())
			{
				var writer = getWriterFunc(manager, writingManager);
				var result = writer.GetDataObject(dataProvider);

				if (request is ShipmentRequest && writer is IDeclarationDataObjectWriter && dataProvider is IForwardingShipmentDeclarationProvider declarationProvider)
				{
					dataProvider = declarationProvider.GetDeclaration();
				}

				new DataContextDataObjectWriter().PopulateDataObject(actionInfo, dataProvider, result.DataContext, includeWorkflowInfo: false);

				return new UniversalShipmentRequestDataObjectResult(result, result != null, writingManager);
			}
		}

		static List<RecipientRoleType> GetRecipientRoles<TRequest>(TRequest request) where TRequest : TopLevelDataObject
		{
			var recipientRoles = new List<RecipientRoleType>();

			if (request.DataContext.RecipientRoleCollection != null)
			{
				recipientRoles.AddRange(request.DataContext.RecipientRoleCollection.Where(x => x.Code.HasValue).Select(x => x.Code.GetValueOrDefault()));
			}

			return recipientRoles;
		}

		static DataWritingManager GetDataWritingManager<TRequest>(TRequest request, ActionInfo actionInfo, IDataObjectWriterStrategy writerStrategy, BusinessObjectFactory factory) where TRequest : TopLevelDataObject
		{
			DataWritingManager writingManager = null;

			if (request is ShipmentRequest shipmentRequest)
			{
				var purposeCode = shipmentRequest.DataContext.ActionPurposeCode;
				if (!purposeCode.IsEmpty)
				{
					actionInfo.PurposeCode = purposeCode;
					writingManager = new DataWritingManager(actionInfo);
				}
			}

			return writingManager ?? new DataWritingManager(actionInfo, writerStrategy: writerStrategy);
		}
	}
}
