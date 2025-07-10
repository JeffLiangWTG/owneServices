using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing
{
	sealed class ICO2ePrePostCarriageExtentionsTest : TestCaseWithFactory
	{
		public void TestGetPreCarriageLegs_TransportMode()
		{
			// Arrange
			var mockProvider = new Mock<ICO2ePrePostCarriage>();
			var locations = new[]
			{
				new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(20, 20)), Core.Constants.TransportModes.Rail),
				new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(10, 10)), Core.Constants.TransportModes.InlandWaterwayTransport),
			};
			mockProvider.Setup(p => p.GetPreCarriageLocations(It.IsAny<ZString>())).Returns(locations);

			// Act
			var legs = mockProvider.Object.GetPreCarriageLegs().ToList();

			// Assert
			AssertEquals(1, legs.Count);
			AssertEquals(Core.Constants.TransportModes.Rail, legs[0].TransportMode);
		}

		public void TestGetPreCarriageLegs_TransportMode_DefalutRoad()
		{
			// Arrange
			var mockProvider = new Mock<ICO2ePrePostCarriage>();
			var locations = new[]
			{
				new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(10, 10))),
				new PrePostCarriageLocationWrapper("Port"),
			};
			mockProvider.Setup(p => p.GetPreCarriageLocations(It.IsAny<ZString>())).Returns(locations);

			// Act
			var legs = mockProvider.Object.GetPreCarriageLegs().ToList();

			// Assert
			AssertEquals(1, legs.Count);
			AssertEquals(Core.Constants.TransportModes.Road, legs[0].TransportMode);
		}

		public void TestGetPostCarriageLegs_TransportMode()
		{
			// Arrange
			var mockProvider = new Mock<ICO2ePrePostCarriage>();
			var locations = new[]
			{
				new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(20, 20)), Core.Constants.TransportModes.InlandWaterwayTransport),
				new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(10, 10)), Core.Constants.TransportModes.Rail),
			};
			mockProvider.Setup(p => p.GetPostCarriageLocations(It.IsAny<ZString>())).Returns(locations);

			// Act
			var legs = mockProvider.Object.GetPostCarriageLegs().ToList();

			// Assert
			AssertEquals(1, legs.Count);
			AssertEquals(Core.Constants.TransportModes.Rail, legs[0].TransportMode);
		}

		public void TestGetPostCarriageLegs_TransportMode_DefaultRoad()
		{
			// Arrange
			var mockProvider = new Mock<ICO2ePrePostCarriage>();
			var locations = new[]
			{
				new PrePostCarriageLocationWrapper("Port"),
				new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(10, 10))),
			};
			mockProvider.Setup(p => p.GetPostCarriageLocations(It.IsAny<ZString>())).Returns(locations);

			// Act
			var legs = mockProvider.Object.GetPostCarriageLegs().ToList();

			// Assert
			AssertEquals(1, legs.Count);
			AssertEquals(Core.Constants.TransportModes.Road, legs[0].TransportMode);
		}

		OrgAddress CreateAddress(ZGeography geoLocation)
		{
			var address = Factory.New<OrgAddress>();
			address.OA_GeoLocation = geoLocation;
			return address;
		}
	}
}
