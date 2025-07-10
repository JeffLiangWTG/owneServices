using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.AIS.Testing
{
	sealed class PortMatcherTestCase : TestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestMatchAsync_ThrowsArgumentNullException_WhenRequestIsNull()
		{
			// Arrange
			var aisWebApiClientMock = new Mock<IAisWebApiClient>();
			var sut = new PortMatcherForTest(aisWebApiClientMock.Object);

			// Act
			_ = sut.MatchAsync(null).GetAwaiter().GetResult();
		}

		public void TestMatchAsync_ProperlyQueriesAIS()
		{
			// Arrange
			var portCallPage = new PortCallPage { Items = PortCalls };
			var aisWebApiClientMock = new Mock<IAisWebApiClient>(MockBehavior.Strict);

			string actualVesselImo = null;
			string actualTimeFrom = null;
			string actualTimeTo = null;
			string actualCarrierCode = null;
			string actualVoyageNumber = null;
			string actualDeparturePort = null;
			string actualArrivalPort = null;
			var setVariablesToVerify = new InvocationAction(invocation =>
			{
				actualVesselImo = (string)invocation.Arguments[0];
				actualTimeFrom = (string)invocation.Arguments[3];
				actualTimeTo = (string)invocation.Arguments[4];
				actualCarrierCode = (string)invocation.Arguments[5];
				actualVoyageNumber = (string)invocation.Arguments[6];
				actualDeparturePort = (string)invocation.Arguments[7];
				actualArrivalPort = (string)invocation.Arguments[8];
			});
			aisWebApiClientMock.Setup(api => api.PortCallsAsync(It.IsAny<string>(), It.IsAny<int?>(),
					It.IsAny<int?>(),
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(portCallPage))
				.Callback(setVariablesToVerify);

			var sut = new PortMatcherForTest(aisWebApiClientMock.Object);

			var vesselMovements = new VesselMovementsUrlModel
			{
				LloydsNumber = new ZString("1234567"),
				DeparturePortUnloco = new ZString("SGSIN"),
				DepartureTime = new ZDateTime(2023, 1, 5, 2, 30, 0),
				ArrivalPortUnloco = new ZString("TWTXG"),
				ArrivalTime = new ZDateTime(2023, 1, 15, 22, 40, 0),
				CarrierCode = new ZString("REG")
			};

			// Act
			_ = sut.MatchAsync(vesselMovements).GetAwaiter().GetResult();

			aisWebApiClientMock.VerifyAll();

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("Requested Vessel IMO Number must be equal to SEA Import Transport Leg Vessel ID.", "1234567", actualVesselImo);
				AssertEquals("Requested Time From must be equal to 7 days before SEA Import Transport Leg Departure Time UTC.", "2022-12-29", actualTimeFrom);
				AssertEquals("Requested Time To must be equal to 7 days after SEA Import Transport Leg Arrival Time UTC.", "2023-01-22", actualTimeTo);
				AssertEquals("Requested Carrier Code must be equal to SEA Import Transport Leg Carrier Customs Code.", "REG", actualCarrierCode);
				AssertNull("Requested Voyage Number must not be specified, because it is not a static value and it can be changed over time.", actualVoyageNumber);
				AssertNull("Requested Departure Port must not be specified.", actualDeparturePort);
				AssertNull("Requested Arrival Port must not be specified.", actualArrivalPort);
			});
		}

		public void TestMatchAsync_FindsTheBestMatchAmongAvailablePortCalls()
		{
			// Arrange
			var portCallPage = new PortCallPage { Items = PortCalls };
			var aisWebApiClientMock = new Mock<IAisWebApiClient>(MockBehavior.Strict);
			aisWebApiClientMock.Setup(api => api.PortCallsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(portCallPage));

			var sut = new PortMatcherForTest(aisWebApiClientMock.Object);

			var vesselMovements = new VesselMovementsUrlModel
			{
				LloydsNumber = new ZString("1234567"),
				DeparturePortUnloco = new ZString("SGSIN"),
				DepartureTime = new ZDateTime(2023, 1, 5, 2, 30, 0),
				ArrivalPortUnloco = new ZString("TWTXG"),
				ArrivalTime = new ZDateTime(2023, 1, 15, 22, 40, 0),
				CarrierCode = new ZString("REG")
			};

			// Act
			var portMatches = sut.MatchAsync(vesselMovements).GetAwaiter().GetResult();

			// Assert
			aisWebApiClientMock.VerifyAll();
			var firstArrivalPort = portMatches.FirstArrivalPort;
			var lastForeignPort = portMatches.LastForeignPort;
			CombineAssertions(() =>
			{
				AssertEquals("TWTPE", firstArrivalPort.Unloco);
				AssertEquals(DateTimeOffset.Parse("2023-01-12T00:00:00+08:00"), firstArrivalPort.ArrivalTime);
				AssertEquals(DateTimeOffset.Parse("2023-01-13T00:00:00+08:00"), firstArrivalPort.DepartureTime);

				AssertEquals("CNXMN", lastForeignPort.Unloco);
				AssertEquals(DateTimeOffset.Parse("2023-01-11T00:00:00+08:00"), lastForeignPort.ArrivalTime);
				AssertEquals(DateTimeOffset.Parse("2023-01-12T00:00:00+08:00"), lastForeignPort.DepartureTime);
			});
		}

		public void TestMatchAsync_ActualTimeTakesPreferenceOverEstimatedTime()
		{
			// Arrange
			var portCallPage = new PortCallPage
			{
				Items = new List<PortCall>
				{
					new PortCall
					{
						Port = new Port { Unloco = "TWTPE" }, // Taiwan
						Eta = DateTimeOffset.Parse("2023-01-12T00:00:00+08:00"),
						Ata = DateTimeOffset.Parse("2023-01-13T01:30:00+08:00"),
						Etd = DateTimeOffset.Parse("2023-01-13T00:00:00+08:00"),
						Atd = DateTimeOffset.Parse("2023-01-14T01:30:00+08:00")
					},
					new PortCall
					{
						Port = new Port { Unloco = "TWTXG" }, // Taiwan
						Eta = DateTimeOffset.Parse("2023-01-13T00:00:00+08:00"),
						Ata = DateTimeOffset.Parse("2023-01-12T02:00:00+08:00"),
						Etd = DateTimeOffset.Parse("2023-01-14T00:00:00+08:00"),
						Atd = DateTimeOffset.Parse("2023-01-13T02:00:00+08:00"),
					},
				}
			};
			var aisWebApiClientMock = new Mock<IAisWebApiClient>(MockBehavior.Strict);
			aisWebApiClientMock.Setup(api => api.PortCallsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(portCallPage));

			var sut = new PortMatcherForTest(aisWebApiClientMock.Object);

			var vesselMovements = new VesselMovementsUrlModel
			{
				LloydsNumber = new ZString("1234567"),
				DeparturePortUnloco = new ZString("SGSIN"),
				DepartureTime = new ZDateTime(2023, 1, 5),
				ArrivalPortUnloco = new ZString("TWTXG"),
				ArrivalTime = new ZDateTime(2023, 1, 16),
				CarrierCode = new ZString("REG")
			};

			// Act
			var portMatches = sut.MatchAsync(vesselMovements).GetAwaiter().GetResult();

			// Assert
			aisWebApiClientMock.VerifyAll();
			var firstArrivalPort = portMatches.FirstArrivalPort;
			CombineAssertions(() =>
			{
				AssertEquals("The expected First Arrival Port Call must be the one with earlier ATA/ETA.", "TWTXG", firstArrivalPort.Unloco);
				AssertEquals("The expected Arrival Time must be equal to First Arrival Port Call ATA.", DateTimeOffset.Parse("2023-01-12T02:00:00+08:00"), firstArrivalPort.ArrivalTime);
				AssertEquals("The expected Arrival Time must be equal to First Arrival Port Call ATD.", DateTimeOffset.Parse("2023-01-13T02:00:00+08:00"), firstArrivalPort.DepartureTime);
			});
		}

		public void TestMatchAsync_FirstArrivalAndLastForeignPortsAreNullIfThereAreNoMatchingPortCalls()
		{
			// Arrange
			var portCallPage = new PortCallPage { Items = PortCalls };
			var aisWebApiClientMock = new Mock<IAisWebApiClient>(MockBehavior.Strict);
			aisWebApiClientMock.Setup(api => api.PortCallsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(portCallPage));

			var sut = new PortMatcherForTest(aisWebApiClientMock.Object);

			var vesselMovements = new VesselMovementsUrlModel
			{
				LloydsNumber = new ZString("1234567"),
				DeparturePortUnloco = new ZString("USLAX"),
				DepartureTime = new ZDateTime(2023, 1, 5),
				ArrivalPortUnloco = new ZString("UAODS"),
				ArrivalTime = new ZDateTime(2023, 1, 16),
				CarrierCode = new ZString("REG")
			};

			// Act
			var portMatches = sut.MatchAsync(vesselMovements).GetAwaiter().GetResult();

			// Assert
			aisWebApiClientMock.VerifyAll();
			CombineAssertions(() =>
			{
				AssertNull("First Arrival Port must be unknown.", portMatches.FirstArrivalPort);
				AssertNull("Last Foreign Port must be unknown.", portMatches.LastForeignPort);
			});
		}

		public void TestMatchAsync_LastForeignPortMatchIsNullIfNoAppropriateDeparturePortCallIsFound()
		{
			// Arrange
			var portCallPage = new PortCallPage
			{
				Items = new List<PortCall>
				{
					new PortCall
					{
						Port = new Port { Unloco = "TWTPE" }, // Taiwan
						Eta = DateTimeOffset.Parse("2023-01-12T00:00:00+08:00"),
						Etd = DateTimeOffset.Parse("2023-01-13T00:00:00+08:00")
					},
					new PortCall
					{
						Port = new Port { Unloco = "TWTXG" }, // Taiwan
						Eta = DateTimeOffset.Parse("2023-01-13T00:00:00+08:00"),
						Etd = DateTimeOffset.Parse("2023-01-14T00:00:00+08:00")
					},
				}
			};
			var aisWebApiClientMock = new Mock<IAisWebApiClient>(MockBehavior.Strict);
			aisWebApiClientMock.Setup(api => api.PortCallsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(portCallPage));

			var sut = new PortMatcherForTest(aisWebApiClientMock.Object);

			var vesselMovements = new VesselMovementsUrlModel
			{
				LloydsNumber = new ZString("1234567"),
				DeparturePortUnloco = new ZString("SGSIN"),
				DepartureTime = new ZDateTime(2023, 1, 5),
				ArrivalPortUnloco = new ZString("TWTXG"),
				ArrivalTime = new ZDateTime(2023, 1, 16),
				CarrierCode = new ZString("REG")
			};

			// Act
			var portMatches = sut.MatchAsync(vesselMovements).GetAwaiter().GetResult();

			// Assert
			aisWebApiClientMock.VerifyAll();
			var firstArrivalPort = portMatches.FirstArrivalPort;
			var lastForeignPort = portMatches.LastForeignPort;
			CombineAssertions(() =>
			{
				AssertEquals("TWTPE", firstArrivalPort.Unloco);
				AssertEquals(DateTimeOffset.Parse("2023-01-12T00:00:00+08:00"), firstArrivalPort.ArrivalTime);
				AssertEquals(DateTimeOffset.Parse("2023-01-13T00:00:00+08:00"), firstArrivalPort.DepartureTime);

				AssertNull("LastForeignPort must be null if there is no match.", lastForeignPort);
			});
		}

		[ExpectNoExceptions]
		public void TestDispose_DisposesHttpClient()
		{
			// Arrange
			var aisWebApiClientMock = new Mock<IAisWebApiClient>(MockBehavior.Strict);
			aisWebApiClientMock.Setup(api => api.Dispose());

			var sut = new PortMatcherForTest(aisWebApiClientMock.Object);

			// Act
			sut.Dispose();

			// Assert
			aisWebApiClientMock.Verify(api => api.Dispose(), Times.Once);
		}

		public void TestCreatingPortMatcher_CreatesTokenProvider()
		{
			var branchesUsedWhenGettingToken = new List<IBranch>();
			ObjectFactory.Substitute(() =>
			{
				branchesUsedWhenGettingToken.Add(Env.CurrentBranch);
				return new Mock<IAuthTokenProvider>().Object;
			});

			ObjectFactory.Get<IPortMatcher>();
			AssertSequencesEqual(new[] { Env.CurrentBranchPK }, branchesUsedWhenGettingToken.Select(t => t.PK));
		}

		#region Implementation

		class PortMatcherForTest : PortMatcher
		{
			public PortMatcherForTest(IAisWebApiClient aisWebApiClient) : base(aisWebApiClient)
			{
			}

			protected override bool IsTest => false;
		}

		static ICollection<PortCall> PortCalls { get; } = new List<PortCall>
		{
			new PortCall
			{
				Port = new Port { Unloco = "TWTPE" }, // Taiwan
				Eta = DateTimeOffset.Parse("2023-01-01T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-02T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "TWTXG" }, // Taiwan
				Eta = DateTimeOffset.Parse("2023-01-02T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-03T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "CNXMN" }, // China
				Eta = DateTimeOffset.Parse("2023-01-03T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-04T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "CNSHK" }, // China
				Eta = DateTimeOffset.Parse("2023-01-05T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-06T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "SGSIN" }, // Singapore
				Eta = DateTimeOffset.Parse("2023-01-07T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-08T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "CNSHK" }, // China
				Eta = DateTimeOffset.Parse("2023-01-09T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-10T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "CNXMN" }, // China
				Eta = DateTimeOffset.Parse("2023-01-11T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-12T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "TWTPE" }, // Taiwan
				Eta = DateTimeOffset.Parse("2023-01-12T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-13T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "TWTXG" }, // Taiwan
				Eta = DateTimeOffset.Parse("2023-01-13T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-14T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "CNXMN" }, // China
				Eta = DateTimeOffset.Parse("2023-01-14T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-15T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "CNSHK" }, // China
				Eta = DateTimeOffset.Parse("2023-01-16T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-17T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "SGSIN" }, // Singapore
				Eta = DateTimeOffset.Parse("2023-01-18T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-19T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "CNSHK" }, // China
				Eta = DateTimeOffset.Parse("2023-01-20T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-21T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "CNXMN" }, // China
				Eta = DateTimeOffset.Parse("2023-01-22T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-23T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "TWTPE" }, // Taiwan
				Eta = DateTimeOffset.Parse("2023-01-23T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-24T00:00:00+08:00")
			},
			new PortCall
			{
				Port = new Port { Unloco = "TWTXG" }, // Taiwan
				Eta = DateTimeOffset.Parse("2023-01-24T00:00:00+08:00"),
				Etd = DateTimeOffset.Parse("2023-01-25T00:00:00+08:00")
			},
		};

		#endregion
	}
}
