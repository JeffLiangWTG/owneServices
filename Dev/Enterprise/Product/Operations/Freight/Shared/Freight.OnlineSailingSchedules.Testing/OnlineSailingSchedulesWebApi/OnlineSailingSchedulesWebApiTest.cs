using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;
using Enterprise.Freight.OnlineSailingSchedules.ServiceModel;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	class OnlineSailingSchedulesWebApiTest : TestCaseWithFactory
	{
		#region Import

		public void TestImport_SimpleCase()
		{
			var payload = new ImportGSSPayloadForTest
			{
				GSSNaturalKey = new GSSNaturalKeyForTest
				{
					CarrierSCAC = "CMAC",
					Legs = new List<ConnectionNaturalKeyForTest>
					{
						new ConnectionNaturalKeyForTest
						{
							Origin = "AUSYD",
							Destination = "NZAKL",
							VesselName = "Santa Maria",
							VoyageCode = "754N",
							CarrierSCAC = "CMAC",
							Departure = new DateTime(2022, 1, 1),
							Arrival = new DateTime(2022, 1, 15),
							LegType = GssConstants.SeaLegType
						},
					}
				}
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] {
					CreateRoute(new[] { CreateLeg("AUSYD", "NZAKL") })
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportGSSFromNaturalKey(payload, onlineSchedules);

			AssertEquals(ResponseToString(response), ImportGSSResponseCode.Success, response.Code);
		}

		public void TestImport_SearchResultsAreFilteredByConnections()
		{
			var payload = new ImportGSSPayloadForTest
			{
				GSSNaturalKey = new GSSNaturalKeyForTest
				{
					CarrierSCAC = "CMAC",
					Legs = new List<ConnectionNaturalKeyForTest>
					{
						new ConnectionNaturalKeyForTest
						{
							Origin = "AUSYD",
							Destination = "NZAKL",
							VesselName = "Santa Maria",
							VoyageCode = "754N",
							CarrierSCAC = "CMAC",
							Departure = new DateTime(2022, 1, 1),
							Arrival = new DateTime(2022, 1, 15),
							LegType = GssConstants.SeaLegType
						},
					}
				}
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] {
					CreateRoute(new[] { CreateLeg("AUSYD", "NZAKL") }),
					CreateRoute(new[] { CreateLeg("AUSYD", "AUBRE") })
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportGSSFromNaturalKey(payload, onlineSchedules);

			AssertEquals(ResponseToString(response), ImportGSSResponseCode.Success, response.Code);
		}

		public void TestImport_CarrierNotExist()
		{
			var payload = new ImportGSSPayloadForTest
			{
				GSSNaturalKey = new GSSNaturalKeyForTest
				{
					CarrierSCAC = "ZZZZ",
					Legs = new List<ConnectionNaturalKeyForTest>
					{
						new ConnectionNaturalKeyForTest
						{
							Origin = "AUSYD",
							Destination = "NZAKL",
							VesselName = "Santa Maria",
							VoyageCode = "754N",
							CarrierSCAC = "ZZZZ",
							Departure = new DateTime(2022, 1, 1),
							Arrival = new DateTime(2022, 1, 15),
							LegType = GssConstants.SeaLegType
						},
					}
				}
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] {
					CreateRoute(new[] { CreateLeg("AUSYD", "NZAKL", carrierSCAC: "ZZZZ") }),
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportGSSFromNaturalKey(payload, onlineSchedules);

			CombineAssertions(ResponseToString(response), () =>
			{
				AssertEquals(ImportGSSResponseCode.OnlineSchedulesValidation, response.Code);
				AssertCollectionContains("Error - CarrierSCAC: No organization found for SCAC ZZZZ.", response.ErrorNotifications);
			});
		}

		public void TestGSSOverlap()
		{
			var payload = new ImportGSSPayloadForTest
			{
				GSSNaturalKey = new GSSNaturalKeyForTest
				{
					CarrierSCAC = "CMAC",
					Legs = new List<ConnectionNaturalKeyForTest>
					{
						new ConnectionNaturalKeyForTest
						{
							Origin = "AUSYD",
							Destination = "NZAKL",
							VesselName = "Santa Maria",
							VoyageCode = "754N",
							CarrierSCAC = "CMAC",
							Departure = new DateTime(2022, 6, 19),
							Arrival = new DateTime(2022, 6, 27),
							LegType = GssConstants.SeaLegType
						},
					}
				}
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] {
					CreateRoute(new[] { CreateLeg("AUSYD", "NZAKL", carrierSCAC: "CMAC", etd: new DateTime(2022, 6, 19), eta: new DateTime(2022, 6, 27)) }),
					CreateRoute(new[] { CreateLeg("AUSYD", "NZAKL", carrierSCAC: "CMAC", etd: new DateTime(2022, 6, 20), eta: new DateTime(2022, 6, 27)) }),
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportGSSFromNaturalKey(payload, onlineSchedules);

			AssertEquals(ResponseToString(response), ImportGSSResponseCode.Success, response.Code);
		}

		public void TestNonSeaConnectionsAreNull()
		{
			var payload = new ImportGSSPayloadForTest
			{
				GSSNaturalKey = new GSSNaturalKeyForTest
				{
					CarrierSCAC = "CMAC",
					Legs = new List<ConnectionNaturalKeyForTest>
					{
						new ConnectionNaturalKeyForTest
						{
							Origin = "GBLON",
							Destination = "GBLGP",
							VesselName = "RAIL",
							VoyageCode = "",
							CarrierSCAC = "CMAC",
							Departure = new DateTime(2022, 6, 18),
							Arrival = new DateTime(2022, 6, 23),
							LegType = GssConstants.RailLegType
						},
						new ConnectionNaturalKeyForTest
						{
							Origin = "GBLGP",
							Destination = "GBLON",
							VesselName = "APL NEW YORK",
							VoyageCode = "0NNIVE",
							CarrierSCAC = "CMAC",
							Departure = new DateTime(2022, 6, 23),
							Arrival = new DateTime(2022, 8, 13),
							LegType = GssConstants.SeaLegType
						},
					}
				}
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] {
					CreateRoute(new[] {
						CreateLeg("GBLON", "GBLGP", carrierSCAC: "CMAC", vesselName: "RAIL", voyageCode: "", etd: new DateTime(2022, 6, 18), eta: new DateTime(2022, 6, 23), legType: GssConstants.RailLegType),
						CreateLeg("GBLGP", "GBLON", carrierSCAC: "CMAC", vesselName: "APL NEW YORK", voyageCode: "0NNIVE", etd: new DateTime(2022, 6, 23), eta: new DateTime(2022, 8, 13))
					}),
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportGSSFromNaturalKey(payload, onlineSchedules);

			CombineAssertions(() =>
			{
				AssertEquals(ResponseToString(response), ImportGSSResponseCode.Success, response.Code);
				AssertEquals(2, response.Sailings.Count());
				AssertNull("Rail leg should be null", response.Sailings.ElementAt(0));
				AssertNotNull("Sea leg should be valid", response.Sailings.ElementAt(1));
			});
		}

		public void TestImport_VesselNameMismatch()
		{
			var payload = new ImportGSSPayloadForTest
			{
				GSSNaturalKey = new GSSNaturalKeyForTest
				{
					CarrierSCAC = "CMAC",
					Legs = new List<ConnectionNaturalKeyForTest>
					{
						new ConnectionNaturalKeyForTest
						{
							Origin = "GBLGP",
							Destination = "GBLON",
							VesselName = "MSC REBECCA III",
							VoyageCode = "10S",
							CarrierSCAC = "CMAC",
							Departure = new DateTime(2022, 6, 23),
							Arrival = new DateTime(2022, 8, 13),
							LegType = GssConstants.SeaLegType
						},
					}
				}
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] {
					CreateRoute(new[] {
						CreateLeg("GBLGP", "GBLON", carrierSCAC: "CMAC", vesselName: "MSC REBECCA III", voyageCode: "10S", imoNumber: "9139505", etd: new DateTime(2022, 6, 23), eta: new DateTime(2022, 8, 13))
					}),
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportGSSFromNaturalKey(payload, onlineSchedules);

			CombineAssertions(() =>
			{
				AssertEquals(ResponseToString(response), ImportGSSResponseCode.Success, response.Code);
				AssertEquals(1, response.Sailings.Count());
				AssertNotNull("MSC REBECCA III leg should be found", response.Sailings.ElementAt(0));
			});
		}

		#endregion

		#region ImportMany

		public void TestImportMany_SimpleCase()
		{
			var connection1 = new ConnectionQueryForTest
			{
				StartDate = new DateTime(2025, 1, 1),
				ExpiryDate = new DateTime(2025, 1, 2),
				LoadPort = "AUSYD",
				DischargePort = "SGSIN",
				AllowRelatedUNLOCOs = false,
				TradeLane = new GSSTradeLaneForTest { Code = "ZAO", Name = "ZAO - ZIM ASIA TO OCEANIA" }
			};
			var connection2 = new ConnectionQueryForTest
			{
				StartDate = new DateTime(2025, 1, 1),
				ExpiryDate = new DateTime(2025, 1, 2),
				LoadPort = "AUSYD",
				DischargePort = "NZAKL",
				AllowRelatedUNLOCOs = true,
				TradeLane = new GSSTradeLaneForTest { Code = "", Name = "Southern Star Service" }
			};

			var payload = new ImportManyImportGSSPayloadForTest
			{
				ConnectionQueries = new List<IConnectionQuery> { connection1, connection2 }
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns((string searchParams, UserInitiatedServiceRequestManager requestManager) =>
				{
					if (searchParams == "LoadPort=AUSYD&EtdFrom=2025-01-01&EtdTo=2025-01-02&DischargePort=SGSIN&IncludeRelatedPorts=False&ServiceString=ZAO")
					{
						return new[] {
							CreateRoute(new[] {
								CreateLeg("AUSYD", "SGSIN", tradeLane: new TradeLane { Name = "ZAO - ZIM ASIA TO OCEANIA" })
							})
						};
					}
					else if (searchParams == "LoadPort=AUSYD&EtdFrom=2025-01-01&EtdTo=2025-01-02&DischargePort=NZAKL&IncludeRelatedPorts=True&ServiceString=Southern+Star+Service")
					{
						return new[] {
							CreateRoute(new[] {
								CreateLeg("AUSYD", "NZTRG", tradeLane: new TradeLane { Name = "Southern Star Service" }),
								CreateLeg("NZTRG", "NZMKL", tradeLane: new TradeLane { Name = "" })
							})
						};					} 
					throw new Exception("Unrecognised searchParams " + searchParams);
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportManyGSS(payload, onlineSchedules);
			AssertEquals(true, response.Success);

			var zaoSailings = response.Sailings[0][0];
			AssertEquals(true, zaoSailings[0].MatchesConnectionQuery);

			var southernStarServiceSailings = response.Sailings[1][0];
			AssertEquals(true, southernStarServiceSailings[0].MatchesConnectionQuery);
			AssertEquals(false, southernStarServiceSailings[1].MatchesConnectionQuery);
		}

		public void TestImportMany_OnlyExactMatchesOnTradeLaneAreImported()
		{
			var connection = new ConnectionQueryForTest
			{
				StartDate = new DateTime(2025, 1, 1),
				ExpiryDate = new DateTime(2025, 1, 2),
				LoadPort = "AUSYD",
				DischargePort = "SGSIN",
				AllowRelatedUNLOCOs = false,
				TradeLane = new GSSTradeLaneForTest { Code = "ZAO", Name = "ZAO" }
			};

			var payload = new ImportManyImportGSSPayloadForTest
			{
				ConnectionQueries = new List<IConnectionQuery> { connection }
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns((string searchParams, UserInitiatedServiceRequestManager requestManager) =>
				{
					if (searchParams == "LoadPort=AUSYD&EtdFrom=2025-01-01&EtdTo=2025-01-02&DischargePort=SGSIN&IncludeRelatedPorts=False&ServiceString=ZAO")
					{
						return new[] {
							CreateRoute(new[] {
								CreateLeg("AUSYD", "SGSIN", tradeLane: new TradeLane { Name = "ZAO - ZIM ASIA TO OCEANIA" })
							}),
							CreateRoute(new[] {
								CreateLeg("AUSYD", "SGSIN", tradeLane: new TradeLane { Name = "ZAO" })
							})
						};
					}
					throw new Exception("Unrecognised searchParams " + searchParams);
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportManyGSS(payload, onlineSchedules);
			AssertEquals(true, response.Success);
			AssertEquals(1, response.Sailings[0].Count);
			var sailing = Factory.Load<JobSailing>(response.Sailings[0][0][0].PK);
			AssertEquals("ZAO", sailing.JX_ServiceString);
		}

		public void TestImportMany_ServiceStringQueryIsAllBeforeHyphen()
		{
			var connection = new ConnectionQueryForTest
			{
				StartDate = new DateTime(2025, 1, 1),
				ExpiryDate = new DateTime(2025, 1, 2),
				LoadPort = "AUSYD",
				DischargePort = "SGSIN",
				AllowRelatedUNLOCOs = false,
				TradeLane = new GSSTradeLaneForTest { Code = "ZAO", Name = "ZAO - ZIM ASIA TO OCEANIA" }
			};

			var payload = new ImportManyImportGSSPayloadForTest
			{
				ConnectionQueries = new List<IConnectionQuery> { connection }
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns((string searchParams, UserInitiatedServiceRequestManager requestManager) =>
				{
					if (searchParams == "LoadPort=AUSYD&EtdFrom=2025-01-01&EtdTo=2025-01-02&DischargePort=SGSIN&IncludeRelatedPorts=False&ServiceString=ZAO")
					{
						return new[] {
							CreateRoute(new[] {
								CreateLeg("AUSYD", "SGSIN", tradeLane: new TradeLane { Name = "ZAO - ZIM ASIA TO OCEANIA" })
							}),
							CreateRoute(new[] {
								CreateLeg("AUSYD", "SGSIN", tradeLane: new TradeLane { Name = "ZAO" })
							})
						};
					}
					throw new Exception("Unrecognised searchParams " + searchParams);
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportManyGSS(payload, onlineSchedules);
			AssertEquals(true, response.Success);
			AssertEquals(1, response.Sailings[0].Count);
			var sailing = Factory.Load<JobSailing>(response.Sailings[0][0][0].PK);
			AssertEquals("ZAO - ZIM ASIA TO OCEANIA", sailing.JX_ServiceString);
		}

		public void TestImportMany_VesselNameMismatch()
		{
			var connection1 = new ConnectionQueryForTest
			{
				StartDate = new DateTime(2025, 1, 1),
				ExpiryDate = new DateTime(2025, 1, 2),
				LoadPort = "AUSYD",
				DischargePort = "NZAKL",
				AllowRelatedUNLOCOs = true,
				TradeLane = new GSSTradeLaneForTest { Code = "", Name = "Southern Star Service" }
			};

			var payload = new ImportManyImportGSSPayloadForTest
			{
				ConnectionQueries = new List<IConnectionQuery> { connection1 }
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns((string searchParams, UserInitiatedServiceRequestManager requestManager) =>
				{
					if (searchParams == "LoadPort=AUSYD&EtdFrom=2025-01-01&EtdTo=2025-01-02&DischargePort=NZAKL&IncludeRelatedPorts=True&ServiceString=Southern+Star+Service")
					{
						return new[] {
							CreateRoute(new[] {
								CreateLeg("AUSYD", "NZTRG", vesselName: "MSC REBECCA", imoNumber: "9139505", tradeLane: new TradeLane { Name = "Southern Star Service" }),
								CreateLeg("NZTRG", "NZMKL", vesselName: "MSC REBECCA III", imoNumber: "9139505", tradeLane: new TradeLane { Name = "" })
							})
						};
					}
					throw new Exception("Unrecognised searchParams " + searchParams);
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportManyGSS(payload, onlineSchedules);
			AssertEquals(true, response.Success);
			AssertNotNull(response.Sailings[0][0]);
			AssertEquals(2, response.Sailings[0][0].Count);
			AssertNotNull(response.Sailings[0][0][0]);
			AssertNotNull(response.Sailings[0][0][1]);
		}

		public void TestImportMany_DirectRoutesOnly()
		{
			var connection1 = new ConnectionQueryForTest
			{
				StartDate = new DateTime(2025, 1, 1),
				ExpiryDate = new DateTime(2025, 1, 2),
				LoadPort = "AUSYD",
				DischargePort = "SGSIN",
				AllowRelatedUNLOCOs = false,
				TradeLane = new GSSTradeLaneForTest { Code = "ZAO", Name = "ZAO - ZIM ASIA TO OCEANIA" },
				DirectRoutesOnly = true,
			};

			var payload = new ImportManyImportGSSPayloadForTest
			{
				ConnectionQueries = new List<IConnectionQuery> { connection1 }
			};

			var routesProviderMock = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProviderMock.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns((string searchParams, UserInitiatedServiceRequestManager requestManager) =>
				{
					if (searchParams == "LoadPort=AUSYD&EtdFrom=2025-01-01&EtdTo=2025-01-02&DischargePort=SGSIN&LegsCount=1&IncludeRelatedPorts=False&ServiceString=ZAO")
					{
						return new[] {
							CreateRoute(new[] {
								CreateLeg("AUSYD", "SGSIN", tradeLane: new TradeLane { Name = "ZAO - ZIM ASIA TO OCEANIA" })
							})
						};
					}
					throw new Exception("Unrecognised searchParams " + searchParams);
				});
			var onlineSchedules = new OnlineSchedules(Factory, routesProviderMock.Object);
			var response = new OnlineSailingSchedulesWebApi().ImportManyGSS(payload, onlineSchedules);
			AssertEquals(true, response.Success);

			var zaoSailings = response.Sailings[0][0];
			AssertEquals(true, zaoSailings[0].MatchesConnectionQuery);
		}

		#endregion

		Route CreateRoute(IEnumerable<ServiceModel.Leg> legs)
		{
			var legWithVoyage = legs.First(t => t.Voyage != null);
			var serviceRoute = new ServiceModel.Route { Carrier = legWithVoyage.Voyage.Operator, Legs = legs.ToArray() };

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			return route;
		}

		ServiceModel.Leg CreateLeg(string loadPort, string dischargePort,
			TradeLane tradeLane = null, string carrierSCAC = "CMAC", string voyageCode = "754N",
			string vesselName = "Santa Maria", DateTime? etd = null, DateTime? eta = null, string legType = null,
			string imoNumber = "1234567")
		{
			var carrier = new Carrier
			{
				Code = carrierSCAC,
				Name = "CMA CGM"
			};

			var voyage = new Voyage
			{
				Code = voyageCode ?? "754N",
				TradeLane = tradeLane ?? new TradeLane { Name = "AAA" },
				Operator = carrier,
				Vessel = new Vessel { VesselName = vesselName ?? "Santa Maria", ImoNumber = imoNumber }
			};

			return new ServiceModel.Leg
			{
				LoadPort = new Port { Unloco = loadPort },
				DischargePort = new Port { Unloco = dischargePort },
				Etd = etd ?? new DateTime(2022, 1, 1),
				Eta = eta ?? new DateTime(2022, 1, 15),
				Voyage = voyage,
				LegType = legType ?? GssConstants.SeaLegType
			};
		}

		string ResponseToString(IImportGSSResponse response)
		{
			if (response.Code == ImportGSSResponseCode.Success)
			{
				return $"Success: {response.Message}\nSailings: {string.Join(",", response.Sailings.Select(sailing => sailing?.SailingID ?? "null"))}";
			}

			var message = $"ErrorCode: {response.Code}.\nErrorMessage: {response.ErrorMessage}";
			if (response.ErrorNotifications?.Any() ?? false)
			{
				message += "\nErrorNotifications:";
				foreach (var notification in response.ErrorNotifications)
				{
					message += $"\n* {notification}";
				}
			}
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "CMAC";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
		}
	}
}
