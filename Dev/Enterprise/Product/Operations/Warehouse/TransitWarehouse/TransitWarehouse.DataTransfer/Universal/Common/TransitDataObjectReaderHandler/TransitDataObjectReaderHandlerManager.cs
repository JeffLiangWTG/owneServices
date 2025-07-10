using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitDataObjectReaderHandlerManager
	{
		public enum HandlerType
		{
			Consol,
			Container,
			Vehicle,
			StoredProcedure
		}

		Dictionary<ZInt, List<PkgPackage>> PackagesByContainerLink { get; set; }
		HashSet<ZGuid> AffectedASNPKs { get; set; }
		Dictionary<ZGuid, PackingLine> MatchedPackagePKAndPackingLine { get; set; }
		HashSet<ZGuid> PackagesAlreadyOnDCN { get; set; }
		List<PackingLine> MatchedPackingLinesByPackingLineID { get; set; }

		#region PackagesByContainerLink

		public virtual void AddPackagesByContainerLink(ZInt link, List<PkgPackage> package)
		{
			if (PackagesByContainerLink.ContainsKey(link))
			{
				PackagesByContainerLink[link].AddRange(package);
			}
			else
			{
				PackagesByContainerLink.Add(link, package);
			}
		}

		public Dictionary<ZInt, List<PkgPackage>> GetPackagesByContainerLink()
		{
			return PackagesByContainerLink ?? [];
		}

		#endregion

		#region AffectedASNPKs

		public virtual void AddAffectedASNPK(ZGuid asnPK)
		{
			AffectedASNPKs.Add(asnPK);
		}

		public HashSet<ZGuid> GetAffectedASNPKs()
		{
			return AffectedASNPKs;
		}

		#endregion

		#region MatchedPackagePKAndPackingLine

		public void AddMatchedPackagePKAndPackingLine(ZGuid packagePK, PackingLine packingLine)
		{
			if (MatchedPackagePKAndPackingLine == null)
			{
				MatchedPackagePKAndPackingLine = new Dictionary<ZGuid, PackingLine>();
			}

			if (!MatchedPackagePKAndPackingLine.ContainsKey(packagePK))
			{
				MatchedPackagePKAndPackingLine.Add(packagePK, packingLine);
			}
		}

		public virtual Dictionary<ZGuid, PackingLine> GetMatchedPackagePKAndPackingLine()
		{
			return MatchedPackagePKAndPackingLine ?? new Dictionary<ZGuid, PackingLine>();
		}

		#endregion

		#region PackagesAlreadyOnDCN

		public void AddPackagesAlreadyOnDCN(ZGuid packagePK)
		{
			if (PackagesAlreadyOnDCN == null)
			{
				PackagesAlreadyOnDCN = new HashSet<ZGuid>();
			}

			PackagesAlreadyOnDCN.Add(packagePK);
		}

		public virtual HashSet<ZGuid> GetPackagesAlreadyOnDCN()
		{
			return PackagesAlreadyOnDCN ?? new HashSet<ZGuid>();
		}

		#endregion

		#region MatchedPackingLinesByPackingLineID

		public void AddMatchedPackingLinesByPackingLineID(PackingLine packingLine)
		{
			if (MatchedPackingLinesByPackingLineID == null)
			{
				MatchedPackingLinesByPackingLineID = new List<PackingLine>();
			}

			MatchedPackingLinesByPackingLineID.Add(packingLine);
		}

		public List<PackingLine> GetMatchedPackingLinesByPackingLineID()
		{
			return MatchedPackingLinesByPackingLineID ?? new List<PackingLine>();
		}

		#endregion

		#region Warehouse

		WhsWarehouse Warehouse { get; set; }

		public WhsWarehouse GetWarehouse()
		{
			return Warehouse;
		}

		#endregion

		public void Init(IXmlImportLogger logger, UniversalObjectFactory factory, Object parent, WhsWarehouse warehouse)
		{
			var isConsol = logger.TopLevelDataContext.DataTargetCollection.IsConsolDataTarget();
			var isFromGate = logger.TopLevelDataObject.GetMatchingDataSource(DataContextType.GateBooking) != null;

			if (((isConsol || isFromGate) && parent is WhsTransitReceiveConsolDataObjectReader)
				|| (!isConsol && parent is WhsTransitReceiveConsignmentDataObjectReader))
			{
				Warehouse = warehouse;

				PackagesByContainerLink = new Dictionary<ZInt, List<PkgPackage>>();
				AffectedASNPKs = new HashSet<ZGuid>();
			}

			if ((!isConsol &&  parent is WhsTransitDispatchConsignmentDataObjectReader)
				|| (isConsol && parent is WhsTransitDispatchConsolDataObjectReader))
			{
				MatchedPackagePKAndPackingLine = new Dictionary<ZGuid, PackingLine>();
				PackagesAlreadyOnDCN = new HashSet<ZGuid>();
				MatchedPackingLinesByPackingLineID = new List<PackingLine>();
			}
		}

		Dictionary<Type, ITransitDataObjectReaderHandler> handlers { get; set; }

		public void RegisterHandler(ITransitDataObjectReaderHandler handler)
		{
			if (handlers == null)
			{
				handlers = new Dictionary<Type, ITransitDataObjectReaderHandler>();
			}

			handlers[handler.GetType()] = handler;
		}

		public virtual ITransitDataObjectReaderHandler GetHandler<T>() where T : class
		{
			if (handlers == null)
			{
				return null;
			}

			handlers.TryGetValue(typeof(T), out var handler);
			return handler;
		}

		public ITransitDataObjectReaderHandler BuildHandler(HandlerType handlerType, UniversalShipment dataObject)
		{
			return new TransitHandlerFactory().BuildHandler(handlerType, dataObject);
		}
	}
}
