using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Warehouse.Transit.Shared
{
	public static class TransitWarehouseJobsLoader
	{
		#region GetRelatedTransitWarehouseJobsEDocs

		public static IEnumerable<BusinessObject> GetRelatedTransitWarehouseJobsEDocs(ITransitWarehouseParent parent)
		{
			var list = new List<BusinessObject>();

			var rcns = GetRelatedRCNs(parent);
			list.AddRange(rcns);

			var dcns = GetRelatedDCNs(parent);
			list.AddRange(dcns);

			var relatedPackages = GetRelatedPackages(parent, rcns, dcns);
			list.AddRange(relatedPackages);

			var relatedPackagePKs = relatedPackages.Select(p => p.PK).ToArray();
			var rtus = GetRelatedRTUs(parent.Factory, relatedPackagePKs);
			list.AddRange(rtus);

			var dtus = GetRelatedDTUs(parent.Factory, relatedPackagePKs);
			list.AddRange(dtus);

			list = list.Union(GetRelatedBlindPackagesAndItsParents(parent)).ToList();
			return list;
		}

		public static IEnumerable<BusinessObject> GetRelatedTransitWarehouseJobsEDocs(BusinessObjectFactory factory, IForwardingConsol consol)
		{
			var list = new List<BusinessObject>();
			list.AddRange(GetRelatedRTUs(factory, consol));
			list.AddRange(GetRelatedDTUs(factory, consol));

			return list;
		}

		static BusinessObject[] GetRelatedRCNs(ITransitWarehouseParent parent)
		{
			return (BusinessObject[])parent.Factory.Load<ITransitReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ParentID, parent.PK));
		}

		static BusinessObject[] GetRelatedDCNs(ITransitWarehouseParent parent)
		{
			return (BusinessObject[])parent.Factory.Load<ITransitDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, parent.PK));
		}

		static BusinessObject[] GetRelatedRTUs(BusinessObjectFactory factory, ZGuid[] relatedPackagePKs)
		{
			var packageStateQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader);
			packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_KP_Package, relatedPackagePKs);

			var rtuQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			rtuQuery.AddSubQuery(packageStateQuery, JoinCondition.And);

			var rtus = factory.Load<WhsItemReceiveTransportationUnit>(rtuQuery);
			return rtus;
		}

		static BusinessObject[] GetRelatedDTUs(BusinessObjectFactory factory, ZGuid[] relatedPackagePKs)
		{
			var packageStateQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader);
			packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_KP_Package, relatedPackagePKs);

			var dtuQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
			dtuQuery.AddSubQuery(packageStateQuery, JoinCondition.And);

			var dtus = factory.Load<WhsItemDispatchTransportationUnit>(dtuQuery);
			return dtus;
		}

		static BusinessObject[] GetRelatedBlindPackagesAndItsParents(ITransitWarehouseParent parent)
		{
			var result = new List<BusinessObject>();
			var factory = parent.Factory;

			var additionalReference = TransitWarehouseHelper.GetPackageStateAdditionalReferenceQuery(parent.JobNumber);

			var packageStateQuery_Package = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_KP_Package);
			packageStateQuery_Package.AddSubQuery(additionalReference, JoinCondition.And);

			var packageStateQuery_rcn = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment);
			packageStateQuery_rcn.AddSubQuery(additionalReference, JoinCondition.And);

			var packageStateQuery_rtu = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader);
			packageStateQuery_rtu.AddSubQuery(additionalReference, JoinCondition.And);

			var packageStateQuery_dcn = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment);
			packageStateQuery_dcn.AddSubQuery(additionalReference, JoinCondition.And);

			var packageStateQuery_dtu = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader);
			packageStateQuery_dtu.AddSubQuery(additionalReference, JoinCondition.And);

			var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			packageQuery.AddSubQuery(packageStateQuery_Package, JoinCondition.And);

			var rcnQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveConsignment));
			rcnQuery.AddSubQuery(packageStateQuery_rcn, JoinCondition.And);

			var rtuQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			rtuQuery.AddSubQuery(packageStateQuery_rtu, JoinCondition.And);

			var dcnQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchConsignment));
			dcnQuery.AddSubQuery(packageStateQuery_dcn, JoinCondition.And);

			var dtuQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
			dtuQuery.AddSubQuery(packageStateQuery_dtu, JoinCondition.And);

			var packages = factory.Load<PkgPackage>(packageQuery);
			var rcns = factory.Load<WhsItemReceiveConsignment>(rcnQuery);
			var rtus = factory.Load<WhsItemReceiveTransportationUnit>(rtuQuery);
			var dcns = factory.Load<WhsItemDispatchConsignment>(dcnQuery);
			var dtus = factory.Load<WhsItemDispatchTransportationUnit>(dtuQuery);

			result.AddRange(packages);
			result.AddRange(rcns);
			result.AddRange(rtus);
			result.AddRange(dcns);
			result.AddRange(dtus);

			return result.ToArray();
		}

		static BusinessObject[] GetRelatedPackages(ITransitWarehouseParent parent, BusinessObject[] rcns, BusinessObject[] dcns)
		{
			var factory = parent.Factory;

			var result = new List<BusinessObject>();

			var hasRCNs = rcns.Any();
			var hasDCNs = dcns.Any();
			if (hasRCNs || hasDCNs)
			{
				var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));
				if (hasRCNs)
				{
					var rcnPackageJobsSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageSchema.KP_KJ_ParentPackageJob);
					rcnPackageJobsSubQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentID, rcns.Select(r => r.PK));

					packageQuery.AddSubQuery(rcnPackageJobsSubQuery, JoinCondition.Or);
				}

				if (hasDCNs)
				{
					var packageStateSubQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_KP_Package);
					packageStateSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, dcns.Select(d => d.PK));

					packageQuery.AddSubQuery(packageStateSubQuery, JoinCondition.Or);
				}

				result.AddRange(factory.Load<PkgPackage>(packageQuery));
			}

			return result.ToArray();
		}

		static BusinessObject[] GetRelatedRTUs(BusinessObjectFactory factory, IForwardingConsol consol)
		{
			var result = new List<BusinessObject>();

			var containerSubQuery = new ZDBOnlySubQuery(typeof(IForwardingContainer), JobContainerSchema.PK);
			containerSubQuery.AddToFilter(JobContainerSchema.JC_JK, consol.PK);

			var asnQuery = new ZDBOnlySubQuery(typeof(WhsItemReceiveASN), WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN);
			asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_ParentID, consol.PK);
			asnQuery.AddSubQuery(WhsItemReceiveASNSchema.WRP_ParentID, JobContainerSchema.PK, containerSubQuery, JoinCondition.Or);

			var asnRTUPivotQuery = new ZDBOnlySubQuery(typeof(WhsItemReceiveASNRTUPivot), WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit);
			asnRTUPivotQuery.AddSubQuery(asnQuery, JoinCondition.And);

			var rtuQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			rtuQuery.AddSubQuery(asnRTUPivotQuery, JoinCondition.And);

			var rtus = factory.Load<WhsItemReceiveTransportationUnit>(rtuQuery);
			result.AddRange(rtus);

			return result.ToArray();
		}

		static BusinessObject[] GetRelatedDTUs(BusinessObjectFactory factory, IForwardingConsol consol)
		{
			var result = new List<BusinessObject>();

			var dllQuery = new ZDBOnlySubQuery(typeof(WhsItemDispatchLoadList), WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList);
			dllQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_ParentID, consol.PK);

			var dllDTUPivotQuery = new ZDBOnlySubQuery(typeof(WhsItemDispatchLoadListDTUPivot), WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit);
			dllDTUPivotQuery.AddSubQuery(dllQuery, JoinCondition.And);

			var dtuQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
			dtuQuery.AddSubQuery(dllDTUPivotQuery, JoinCondition.And);

			var dtus = factory.Load<WhsItemDispatchTransportationUnit>(dtuQuery);
			result.AddRange(dtus);

			return result.ToArray();
		}

		#endregion
	}
}
