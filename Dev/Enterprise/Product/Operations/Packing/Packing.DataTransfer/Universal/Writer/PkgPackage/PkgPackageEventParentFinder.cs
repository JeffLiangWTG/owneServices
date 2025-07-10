using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageEventParentFinder : EventParentFinder
	{
		public PkgPackageEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var result = new List<BusinessObject>();
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			var packageId = eventValueObject.Context.TransportBookingPackageID;
			if (!packageId.IsEmpty)
			{
				var packages = factory.Load<PkgPackage>(GetPkgPackageQueryByPackageID(packageId));

				foreach (var package in packages)
				{
					var packageJob = package.PackageJob;
					if (packageJob != null)
					{
						var packingParent = factory.Load<IPackingParent>(packageJob.KJ_ParentTableCode, packageJob.KJ_ParentID);
						if (packingParent != null && packingParent.IsUXMLEventParent(eventValueObject))
						{
							result.Add(package);
							break;
						}
					}
				}
			}

			return result.Any() ? result.ToArray() : null;
		}

		ZQuery GetPkgPackageQueryByPackageID(ZString packageID)
		{
			var query = new ZDBOnlyQuery(typeof(PkgPackage));
			var packageHeaderSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageHeaderSchema.PK);
			packageHeaderSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, packageID);

			query.AddSubQuery(PkgPackageSchema.KP_KPH_PackageHeader, packageHeaderSubQuery, JoinCondition.And);

			return query;
		}
	}
}
