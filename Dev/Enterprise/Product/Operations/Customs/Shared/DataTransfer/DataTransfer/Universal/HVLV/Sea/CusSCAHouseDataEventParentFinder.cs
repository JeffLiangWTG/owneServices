using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public class CusSCAHouseDataEventParentFinder : EventParentFinder
	{
		public CusSCAHouseDataEventParentFinder(BusinessObjectFactory factory, CusSCAHouseDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected sealed override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			var query = GetLogParentsQuery(xmlEvent);
			return query != null ? factory.Load<BaseCusSCAHouse>(query) : null;
		}

		protected virtual ZString GetApplicationCode(IXmlEventValueObject valueObject) => ZString.Empty;

		ZQuery GetLogParentsQuery(IXmlEventValueObject valueObject)
		{
			ZQuery result = null;
			var applicationCode = GetApplicationCode(valueObject);
			if (!applicationCode.IsEmpty && valueObject.Context.HBOLNumber.HasValue && !valueObject.Context.HBOLNumber.Value.IsEmpty)
			{
				result = new ZQuery(CusSCAHouseSchema.CA_HouseBill, valueObject.Context.HBOLNumber.GetValueOrDefault());
				result.AddToFilter(CusSCAHouseSchema.CA_MasterHouseBill, valueObject.Context.MasterHouseBill.GetValueOrDefault());
			}
			return result;
		}
	}
}
