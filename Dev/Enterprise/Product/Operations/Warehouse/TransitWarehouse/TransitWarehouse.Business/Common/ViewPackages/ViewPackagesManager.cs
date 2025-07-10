using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transit.Business
{
	public class ViewPackagesManager : NonPersistentBusinessObject
	{
		public ViewPackagesManager(BusinessObjectFactory factory, ITransitWarehouseParent parent) : base(factory)
		{
			Parent = parent;
		}

		public ITransitWarehouseParent Parent { get; }

		#region AllPackages

		public WhsItemPackageStateCollection AllPackages
		{
			get
			{
				if (allPackages == null)
				{
					allPackages = new WhsItemPackageStateCollection(Factory);
				}

				return allPackages;
			}
		}
		WhsItemPackageStateCollection allPackages;

		#endregion

		#region SetCurrentCurrentWarehousePK

		public void SetCurrentWarehousePK(ZGuid warehousePK)
		{
			transitWarehousePK = warehousePK;
		}

		ZGuid TransitWarehousePK
		{
			get
			{
				if (transitWarehousePK == ZGuid.Empty)
				{
					transitWarehousePK = TransitWarehouseHelper.GetTransitWarehouse(Factory, Parent.GetPickupCFSOrgAddressPK())?.PK ?? ZGuid.Empty;
				}

				return transitWarehousePK;
			}
		}
		ZGuid transitWarehousePK;

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(TransitWarehousePK);

		#endregion

		#region Package Totals

		public ZInt TotalPackages => AllPackages.Sum(ps => ps.Package.KP_PackageQty);

		public ZPropertyInfo TotalPackagesInfo => GetZPropertyInfo(nameof(TotalPackages));

		public ZString TotalPackagesLabel => Invariant($"{TotalPackages},");  // To show totals on attach packages form

		public ZPropertyInfo TotalPackagesLabelInfo => GetZPropertyInfo(nameof(TotalPackagesLabel));

		public ZDecimal TotalWeight
		{
			get
			{
				ZDecimal result = 0m;
				ZString targetUQ = TotalWeightUQ;

				if (Constants.Weight.ContainsCode(targetUQ))
				{
					foreach (var package in AllPackages.Where(p => p.Package.KP_Weight > 0 && !p.Package.KP_WeightUQ.IsEmpty))
					{
						if (Constants.Weight.ContainsCode(package.Package.KP_WeightUQ))
						{
							result += Constants.Weight.Convert(package.Package.KP_Weight, package.Package.KP_WeightUQ, targetUQ);
						}
					}
				}

				return result;
			}
		}

		public ZPropertyInfo TotalWeightInfo => GetZPropertyInfo(nameof(TotalWeight));

		public ZDecimal TotalVolume
		{
			get
			{
				ZDecimal result = 0m;
				ZString targetUQ = TotalVolumeUQ;

				if (Constants.Volume.ContainsCode(targetUQ))
				{
					foreach (var package in AllPackages.Where(p => p.Package.KP_Volume > 0 && !p.Package.KP_VolumeUQ.IsEmpty))
					{
						if (Constants.Volume.ContainsCode(package.Package.KP_VolumeUQ))
						{
							result += Constants.Volume.Convert(package.Package.KP_Volume, package.Package.KP_VolumeUQ, targetUQ);
						}
					}
				}

				return result;
			}
		}

		public ZPropertyInfo TotalVolumeInfo => GetZPropertyInfo(nameof(TotalVolume));

		public ZString TotalVolumeUQ
		{
			get { return PackingRegistry.Instance.VolumeUnit.Value; }
		}

		public ZPropertyInfo TotalVolumeUQInfo => GetZPropertyInfo(nameof(TotalVolumeUQ));

		public ZString TotalWeightUQ
		{
			get { return PackingRegistry.Instance.WeightUnit.Value; }
		}

		public ZPropertyInfo TotalWeightUQInfo => GetZPropertyInfo(nameof(TotalWeightUQ));

		public ZString TotalWeightUQLabel => Invariant($"{string.Format(CultureInfo.InvariantCulture, "{0:n0}", TotalWeight)} {TotalWeightUQ},"); // To show totals on attach packages form

		public ZPropertyInfo TotalWeightUQLabelInfo => GetZPropertyInfo(nameof(TotalWeightUQLabel));

		public ZString TotaVolumeUQLabel => Invariant($"{string.Format(CultureInfo.InvariantCulture, "{0:n0}", TotalVolume)} {TotalVolumeUQ}");  // To show totals on attach packages form

		public ZPropertyInfo TotalVolumeUQLabelInfo => GetZPropertyInfo(nameof(TotaVolumeUQLabel));

		#endregion
	}
}
