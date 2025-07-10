using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingPackLineCollection))]
	internal class BillOfLadingPackLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			BillOfLading bill = Factory.New<BillOfLading>();
			return bill.OuterPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BillOfLadingPackLine packLine = Factory.New<BillOfLadingPackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			return packLine;
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(BillOfLadingPackLineCollection), GetCollectionToTest().GetType());
		}

		public void TestSetDefaultsForNewChild_UnitOfDimensionIsDefaultedFromRegistry()
		{
			var billOfLading = Factory.New<BillOfLading>();
			AgencyRegistry.Instance.DefaultBillDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Inches);
			AssertEquals(Constants.Length.Inches, billOfLading.OuterPackLines.AddNew().JL_UnitOfDimension);
			AgencyRegistry.Instance.DefaultBillDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Feet);
			AssertEquals(Constants.Length.Feet, billOfLading.OuterPackLines.AddNew().JL_UnitOfDimension);
		}
	}
}
