using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderContainerCollection : DependentCusAddInfoCollection<DeliveryOrderContainer, DeliveryOrderHeader>
	{
		public DeliveryOrderContainerCollection(DeliveryOrderHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDeliveryOrderContainer)
		{
		}

		public DeliveryOrderContainer this[ZString containerNumber]
		{
			get
			{
				DeliveryOrderContainer result = null;
				foreach (DeliveryOrderContainer container in this)
				{
					if (container.US_ContainerNumber == containerNumber)
					{
						result = container;
						break;
					}
				}
				return result;
			}
		}

		public void CopyContainersIfMissing(IEnumerable<CusContainer> containers)
		{
			if (containers != null)
			{
				foreach (CusContainer container in containers)
				{
					DeliveryOrderContainer orderContainer = this[container.CO_ContainerNumber];
					if (orderContainer == null)
					{
						orderContainer = AddNew();
						orderContainer.US_ContainerNumber = container.CO_ContainerNumber;
					}
					var totalPackagesUnit = container.CO_Calc_TotalPackagesUnit;
					var isAllPackagesHavingSameUnit = totalPackagesUnit != Constants.PkgUnit.Piece;
					orderContainer.US_PackageType = isAllPackagesHavingSameUnit ? totalPackagesUnit : ZString.Empty;
					orderContainer.US_NoOfPackages = isAllPackagesHavingSameUnit ? container.CO_Calc_TotalPackages : ZInt.Zero;
				}
			}
		}
	}
}
