using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class DataContextKeyLookup : IShipmentRequestLookup
	{
		public DataContextKeyLookup(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			this.Factory = factory;
			this.logger = logger;
		}
		readonly BusinessObjectFactory Factory;
		readonly IXmlImportLogger logger;

		IEnumerable<BusinessObject> IShipmentRequestLookup.Match(ShipmentRequest request)
		{
			return Match(request);
		}

		public IEnumerable<BusinessObject> Match(TopLevelDataObject request)
		{
			var target = request.DataContext?.DataTargetCollection?.FirstOrDefault();
			if (target != null
				&& Enum.TryParse(target.Type.GetValueOrDefault(), out DataContextType dataContextType)
				&& dataContextType.GetUniversalDataContextManager() is IDataContextManager dataContext)
			{
				return dataContext.LoadBusinessObjectsFromDataTarget(request, target, Factory, logger);
			}
			return Enumerable.Empty<BusinessObject>();
		}
	}
}
