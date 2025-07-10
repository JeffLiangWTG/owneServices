using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public class CusSCAOceanBillDataEventParentFinder : EventParentFinder
	{
		public CusSCAOceanBillDataEventParentFinder(BusinessObjectFactory factory, CusSCAOceanBillDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected sealed override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			var query = GetLogParentsQuery(xmlEvent);
			return query != null ? factory.Load<BaseCusSCAOceanBill>(query) : null;
		}

		protected virtual ZString GetApplicationCode(IXmlEventValueObject valueObject)
		{
			return ZString.Empty;
		}

		ZQuery GetLogParentsQuery(IXmlEventValueObject valueObject)
		{
			ZQuery result = null;
			var applicationCode = GetApplicationCode(valueObject);
			if (!applicationCode.IsEmpty && valueObject.Context.MBOLNumber.HasValue && !valueObject.Context.MBOLNumber.Value.IsEmpty)
			{
				result = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, valueObject.Context.MBOLNumber.GetValueOrDefault());
				result.AddToFilter(CusSCAOceanBillSchema.CB_LloydsIMO, valueObject.Context.LloydsNumber.GetValueOrDefault());
				result.AddToFilter(CusSCAOceanBillSchema.CB_Voyage, valueObject.Context.VoyageNumber.GetValueOrDefault());
				result.AddToFilter(CusSCAOceanBillSchema.CB_MasterHouseBill, valueObject.Context.MasterHouseBill.GetValueOrDefault());
				result.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, applicationCode);
			}
			return result;
		}
	}
}
