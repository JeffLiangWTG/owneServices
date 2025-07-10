using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public sealed class TrolleySlotInfoCollection : DataObjectInfoCollection<TrolleySlotInfo>
	{
		public TrolleySlotInfoCollection()
		{
		}

		public TrolleySlotInfoCollection(IEnumerable<WhsPickTrolleySlot> trolleySlots)
		: this()
		{
			AddFetchHintSlots(trolleySlots);
			foreach (var slot in trolleySlots)
			{
				Add(new TrolleySlotInfo(slot));
			}
		}

		void AddFetchHintSlots(IEnumerable<WhsPickTrolleySlot> trolleySlots)
		{
			if (trolleySlots.Any())
			{
				var factory = trolleySlots.FirstOrDefault().Factory;
				var packagePKs = trolleySlots.Select(s => s.WTS_KP_Package).ToArray();

				factory.AddFetchHint(GenAddOnColumnSchema.Instance, new ZQuery(GenAddOnColumnSchema.XA_ParentID, packagePKs));

				var packageQuery = new ZQuery(PkgPackageSchema.PK, packagePKs);
				factory.AddFetchHint(PkgPackageSchema.Instance, packageQuery);

				var packageHeaderQuery = new ZDBOnlyQuery(typeof(PkgPackageHeader));
				var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KPH_PackageHeader);
				packageSubQuery.AddToFilter(PkgPackageSchema.PK, packagePKs);
				packageHeaderQuery.AddSubQuery(packageSubQuery, JoinCondition.And);
				factory.AddFetchHint(PkgPackageHeaderSchema.Instance, packageHeaderQuery);

				var query = new ZDBOnlyQuery(typeof(PkgPackageJob));
				var subQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
				subQuery.AddToFilter(PkgPackageSchema.PK, packagePKs);
				query.AddSubQuery(subQuery, JoinCondition.And);
				factory.AddFetchHint(PkgPackageJobSchema.Instance, query);
			}
		}
	}
}
