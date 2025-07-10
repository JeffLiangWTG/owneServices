using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(InnerPackLineCollection))]
	sealed class InnerPackLineCollectionBOCollectionTestShipment : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			InnerPackLineCollection packLines = (InnerPackLineCollection)GetCollectionToTest();
			PackLine packLine = packLines.AddNew();
			AssertEquals("Inner PackLine should be of InnerPackType", FreightConstants.InnerPackType, packLine.JL_FreightMode);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonShipment parent = CommonShipment.New(Factory);
			return parent.InnerPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			PackLine line = Factory.New<PackLine>();
			line.JL_FreightMode = FreightConstants.InnerPackType;
			return line;
		}

		public void TestCreateAdditionalFilter()
		{
			PackLine line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.InnerPackType;

			PackLine line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.OuterPackType;

			CommonShipment shipment = Factory.New<CommonShipment>();
			InnerPackLineCollection collection = new InnerPackLineCollection(shipment, Factory);

			collection.Add(line1);
			collection.Add(line2);
			collection.Load();
			AssertCollectionContains(line1, collection);
			AssertCollectionNotContains(line2, collection);
		}
	}
}
