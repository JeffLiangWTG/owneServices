using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderPackageDataObjectWriter : DataObjectWriter<PkgPackage, UniversalShipment>, IOrderLineDictionaryProvider
	{
		public WhsOrderPackageDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override UniversalShipment PopulateDataObject(PkgPackage package)
		{
			LinksDictionary.Clear();
			var orderBO = (WhsOrder)package.PackageJob.ParentJob;
			return WhsOrderPackageDataObjectHelper.GetWhsOrderPackageDataObject(writeManager, orderBO, ExcludeElement.Packages, LinksDictionary);
		}

		Dictionary<ZGuid, ZInt> IOrderLineDictionaryProvider.GetOrderLineDictionary() => LinksDictionary;
		readonly Dictionary<ZGuid, ZInt> LinksDictionary = new Dictionary<ZGuid, ZInt>();
	}
}
