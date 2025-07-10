using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsUnitRateInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsUnitRateInfo>
	{
		#region Test Cases

		public void TestAdditionalConstructors()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgSupplierPart part = factory.New<OrgSupplierPart>();
			WhsUnitRateInfoCollection collection = new WhsUnitRateInfoCollection(new OrgPartUnitCollection(part, factory));
			AssertNotNull(collection);
			AssertEquals(0, collection.Count);

			OrgPartUnitCollection partUnits = new OrgPartUnitCollection(part, factory);
			OrgPartUnit partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "P1";
			partUnit1.OF_ParentPackType = "PP1";
			partUnit1.OF_QuantityInParent = 10m;

			AssertEquals(1, partUnits.Count);
			collection = new WhsUnitRateInfoCollection(partUnits);
			AssertNotNull(collection);
			AssertEquals(1, collection.Count);
			AssertEquals("P1", collection[0].Package);
			AssertEquals("PP1", collection[0].Parent);
			AssertEquals(10m, collection[0].Units);

			OrgPartUnit partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "P2";
			partUnit2.OF_ParentPackType = "PP2";
			partUnit2.OF_QuantityInParent = 20m;

			AssertEquals(2, partUnits.Count);
			collection = new WhsUnitRateInfoCollection(partUnits);
			AssertNotNull(collection);
			AssertEquals(2, collection.Count);
			AssertEquals("P1", collection[0].Package);
			AssertEquals("PP1", collection[0].Parent);
			AssertEquals(10m, collection[0].Units);

			AssertEquals("P2", collection[1].Package);
			AssertEquals("PP2", collection[1].Parent);
			AssertEquals(20m, collection[1].Units);
		}

		#endregion

		#region Implementation

		protected new WhsUnitRateInfoCollection Parent
		{
			get
			{
				return (WhsUnitRateInfoCollection)base.Parent;
			}
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsUnitRateInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsUnitRateInfoCollection);
		}

		protected override WhsUnitRateInfo GetNewObjectInfo()
		{
			return new WhsUnitRateInfo();
		}

		protected override DataObjectInfoCollection<WhsUnitRateInfo> GetNewObjectInfoCollection()
		{
			return new WhsUnitRateInfoCollection();
		}

		#endregion
	}
}
