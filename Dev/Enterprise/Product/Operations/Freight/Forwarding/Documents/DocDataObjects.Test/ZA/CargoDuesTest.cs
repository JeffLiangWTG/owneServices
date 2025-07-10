using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.ZA
{
	[TestedType(typeof(CargoDues))]
	sealed class CargoDuesTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dataObject = new CargoDues("JobDeclaration", "B0000001", DataContext.CargoDuesBrokerage);
			dataObject.ShipmentPackingInfos = new List<ShipmentPackingInfo>();
			dataObject.Containers = new List<Container>();
			return dataObject;
		}
	}

	[TestedType(typeof(GoodsInfo))]
	sealed class GoodsInfoDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new GoodsInfo(ZGuid.Empty);
		}
	}

	[TestedType(typeof(DuesCollectionRow))]
	sealed class CargoDuesRowDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DuesCollectionRow();
		}
	}
}
