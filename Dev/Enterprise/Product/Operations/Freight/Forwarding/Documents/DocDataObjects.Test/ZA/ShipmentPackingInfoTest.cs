using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.ZA
{
	[TestedType(typeof(ShipmentPackingInfo))]
	sealed class ShipmentPackingInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var info = new ShipmentPackingInfo(ZGuid.Empty);
			info.GoodsInfoCollection = new List<GoodsInfo>();
			info.ShipmentPackingInfos = new List<ShipmentPackingInfo>();

			return info;
		}
	}
}
