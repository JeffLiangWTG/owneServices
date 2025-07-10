using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class AirManifestDataEventParentFinder : EventParentFinder
	{
		public AirManifestDataEventParentFinder(BusinessObjectFactory factory, AirManifestDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override sealed BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			IXmlEventValueObject valueObject = xmlEvent;
			return valueObject.Context.HAWBNumber.IsEmpty && !valueObject.Context.MAWBNumber.IsEmpty ? GetMAWBs(valueObject) : null;
		}

		public static CusMAWB[] GetMAWBs(BusinessObjectFactory factory, IXmlEventValueObject valueObject)
		{
			var loader = new CusMAWB.Loader(factory);
			var mawbNumber = valueObject.Context.MAWBNumber.Replace("-", "").Replace(" ", "");
			var companyCode = GlbCompany.CurrentCompany.GC_Code;
			var masterHouseFilter = valueObject.Context.MasterHouseBill.HasValue
								  ? new ZQuery(CusMAWBSchema.CM_MasterHouseBill, valueObject.Context.MasterHouseBill)
								  : null;
			return loader.FindMatchingMAWBs(mawbNumber, companyCode, masterHouseFilter);
		}

		protected virtual CusMAWB[] GetMAWBs(IXmlEventValueObject valueObject)
		{
			return GetMAWBs(factory, valueObject);
		}
	}
}
