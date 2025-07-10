using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBookingPackLineCollection))]
	internal class AgencyBookingPackLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			return shipment.OuterPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AgencyBookingPackLine packLine = Factory.New<AgencyBookingPackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			return packLine;
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(AgencyBookingPackLineCollection), GetCollectionToTest().GetType());
		}

		public void TestSetDefaultsForNewChild_UnitOfDimensionIsDefaultedFromRegistry()
		{
			var booking = Factory.New<AgencyBooking>();
			AgencyRegistry.Instance.DefaultBookingDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Inches);
			AssertEquals(Constants.Length.Inches, booking.OuterPackLines.AddNew().JL_UnitOfDimension);
			AgencyRegistry.Instance.DefaultBookingDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Feet);
			AssertEquals(Constants.Length.Feet, booking.OuterPackLines.AddNew().JL_UnitOfDimension);
		}
	}
}
