using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class AirManifestLineDataEventParentFinder : EventParentFinder
	{
		public AirManifestLineDataEventParentFinder(BusinessObjectFactory factory, AirManifestLineDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			BusinessObject[] result = null;
			var eventValueObject = (IXmlEventValueObject)xmlEvent;
			if (!eventValueObject.Context.HAWBNumber.IsEmpty)
			{
				var mawbs = GetMAWBs(eventValueObject);
				if (!mawbs.IsNullOrEmpty())
				{
					var query = new ZQuery(CusHAWBSchema.CS_HAWB, eventValueObject.Context.HAWBNumber);
					query.AddToFilter(CusHAWBSchema.CS_CM, mawbs.Select(mawb => mawb.PK));
					result = factory.Load<CusHAWB>(query);
				}
			}

			return result;
		}

		protected virtual CusMAWB[] GetMAWBs(IXmlEventValueObject valueObject)
		{
			return AirManifestDataEventParentFinder.GetMAWBs(factory, valueObject);
		}
	}
}
