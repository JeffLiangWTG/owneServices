using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgProfitShareDetailsDependentCollection))]
	internal class OrgProfitShareDetailsDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetProfitShareAgreementWhereAgreementPortsEmpty()
		{
			SetupProfitShareDetails();

			OrgProfitShareDetails profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "", "NZAKL", "GBLON", null, null);
			AssertNull("Not found", profitShare);

			profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "", "NZAKL", "GBLON", null, ControllingAgent);
			AssertEquals("Profit share agreement for empty ports found", PS5.PK, profitShare.PK);
		}

		public void TestGetProfitShareAgreement_MatchLocation()
		{
			var zone1 = CreateZone("ZON1", RefZoneHeaderLookups.ZoneTypeCodes.Rating);
			var unloco1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "VNHAN");
			zone1.Countries.Add(unloco1.Country);

			var zone2 = CreateZone("ZON2", RefZoneHeaderLookups.ZoneTypeCodes.Rating);
			var unloco2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "HKHKG");
			zone2.Countries.Add(unloco2.Country);

			var agreementLocationsToTest = new[] { "ZON1", "USLAX", "TR", "BR" };
			var matchedLocationsForAgreements = new[]
			{
				new[] { "ZON1", "VN", "HAN", "VNHAN" },
				new[] { "USLAX" },
				new[] { "TR", "TRANK" },
				new[] { "BR", "BRBBN" },
			};
			var unmatchedLocations = new[] { "ZON2", "HK", "HKG", "HKHKG" };

			AssertGetProfitShareAgreement_MatchLocation(
				testSendingLocation: true,
				agreementLocationsToTest: agreementLocationsToTest,
				matchedLocationsForAgreements: matchedLocationsForAgreements,
				unmatchedLocations: unmatchedLocations
			);
			AssertGetProfitShareAgreement_MatchLocation(
				testSendingLocation: false,
				agreementLocationsToTest: agreementLocationsToTest,
				matchedLocationsForAgreements: matchedLocationsForAgreements,
				unmatchedLocations: unmatchedLocations
			);
		}

		void AssertGetProfitShareAgreement_MatchLocation(
			bool testSendingLocation,
			IEnumerable<string> agreementLocationsToTest,
			IReadOnlyList<string[]> matchedLocationsForAgreements,
			string[] unmatchedLocations)
		{
			var testLocationDataIndex = testSendingLocation ? 0 : 1;
			var otherLocationDataIndex = testSendingLocation ? 1 : 0;

			string[] CreateLocationPair(string testLocation)
			{
				var pair = new string[2];
				pair[testLocationDataIndex] = testLocation;
				pair[otherLocationDataIndex] = "INBOM";
				return pair;
			}

			var pairAgreementLocationsToTest = agreementLocationsToTest.Select(CreateLocationPair);
			var agreements = pairAgreementLocationsToTest.Select(pair => AddOrgProfitShareDetails(
				collection: CollectionForTest,
				freightMode: OrgProfitShareDetailsLookups.FreightModesList.FCL.Code,
				sendingPortOrCountry: pair[0],
				receivingPortOrCountry: pair[1],
				controllingAgent: ControllingAgent)).ToArray();
			Factory.Save();

			for (var idx = 0; idx < matchedLocationsForAgreements.Count; idx++)
			{
				foreach (var matchedLocation in matchedLocationsForAgreements[idx])
				{
					var expectedAgreement = agreements[idx];
					var pair = CreateLocationPair(matchedLocation);
					AssertGetProfitShareAgreement(pair[0], pair[1], expectedAgreement,
						$"Agreement should be found by matching locations ({pair[0]}, {pair[1]}) to agreement ({expectedAgreement.O4_SendingPortOrCountry}, {expectedAgreement.O4_ReceivingPortOrCountry})");
				}
			}

			foreach (var unmatchedZoneLocation in unmatchedLocations)
			{
				var pair = CreateLocationPair(unmatchedZoneLocation);
				AssertGetProfitShareAgreement(pair[0], pair[1], null,
					$"Agreement should not be found by locations ({pair[0]}, {pair[1]})");
			}

			var blankLocationPair = CreateLocationPair("");
			var agreementWithBlankLocation = AddOrgProfitShareDetails(
				collection: CollectionForTest,
				freightMode: OrgProfitShareDetailsLookups.FreightModesList.FCL.Code,
				sendingPortOrCountry: blankLocationPair[0],
				receivingPortOrCountry: blankLocationPair[1],
				controllingAgent: ControllingAgent);
			Factory.Save();

			foreach (var unmatchedZoneLocation in unmatchedLocations)
			{
				var pair = CreateLocationPair(unmatchedZoneLocation);
				AssertGetProfitShareAgreement(pair[0], pair[1], agreementWithBlankLocation,
					$"Blank location should be a fallback for ({pair[0]}, {pair[1]}) to find agreements");
			}
		}

		void AssertGetProfitShareAgreement(
			string sendingLocation,
			string receivingLocation,
			OrgProfitShareDetails expectedAgreement,
			string message)
		{
			var result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "SEA", "FCL", sendingLocation, receivingLocation, null, ControllingAgent);
			if (expectedAgreement != null)
			{
				if (result == null)
				{
					Assert(message, false);
				}
				else
				{
					AssertEquals(message, expectedAgreement.PK, result.PK);
				}
			}
			else
			{
				AssertNull(message, result);
			}
		}

		public void TestGetProfitShareAgreement_SenderFallback()
		{
			var agentRelationship = CreateAgentRelationship(Client, Client2);
			var agentCollection = CreateCollectionForTest(agentRelationship);
			var agentCol = new OrgProfitShareDetailsDependentCollection(agentRelationship);
			var senderAllRelationship = CreateAgentRelationship(Client, null);
			var senderAllCollection = CreateCollectionForTest(senderAllRelationship);

			var expectedDetails = AddOrgProfitShareDetails(senderAllCollection, new ZDateTime(2003, 4, 13), new ZDateTime(2003, 5, 13), freightMode: OrgProfitShareDetailsLookups.FreightModesList.FCL.Code, sendingPortOrCountry: "AUSYD", receivingPortOrCountry: "USLAX");
			Factory.Save();

			var profitShare = agentCol.GetProfitShareAgreement(new ZDateTime(2003, 4, 20), "SEA", "FCL", "AUSYD", "USLAX", null, null);
			AssertEquals("Sender to all details found", expectedDetails.PK, profitShare.PK);

			expectedDetails = AddOrgProfitShareDetails(agentCollection, new ZDateTime(2003, 4, 13), new ZDateTime(2003, 5, 13), freightMode: OrgProfitShareDetailsLookups.FreightModesList.FCL.Code, sendingPortOrCountry: "AUSYD", receivingPortOrCountry: "USLAX");
			Factory.Save();
			profitShare = agentCol.GetProfitShareAgreement(new ZDateTime(2003, 4, 20), "SEA", "FCL", "AUSYD", "USLAX", null, null);
			AssertEquals("Sender to receiver details found", expectedDetails.PK, profitShare.PK);
		}

		public void TestGetProfitShareAgreement_ReceiverFallback()
		{
			var agentRelationship = CreateAgentRelationship(Client, Client2);
			var agentCollection = CreateCollectionForTest(agentRelationship);
			var agentCol = new OrgProfitShareDetailsDependentCollection(agentRelationship);
			var receiverAllRelationship = CreateAgentRelationship(null, Client2);
			var receiverAllCollection = CreateCollectionForTest(receiverAllRelationship);

			var expectedDetails = AddOrgProfitShareDetails(receiverAllCollection, new ZDateTime(2003, 4, 13), new ZDateTime(2003, 5, 13), freightMode: OrgProfitShareDetailsLookups.FreightModesList.FCL.Code, sendingPortOrCountry: "AUSYD", receivingPortOrCountry: "USLAX");
			Factory.Save();

			var profitShare = agentCol.GetProfitShareAgreement(new ZDateTime(2003, 4, 20), "SEA", "FCL", "AUSYD", "USLAX", null, null);
			AssertEquals("All to receiver details found", expectedDetails.PK, profitShare.PK);

			expectedDetails = AddOrgProfitShareDetails(agentCollection, new ZDateTime(2003, 4, 13), new ZDateTime(2003, 5, 13), freightMode: OrgProfitShareDetailsLookups.FreightModesList.FCL.Code, sendingPortOrCountry: "AUSYD", receivingPortOrCountry: "USLAX");
			Factory.Save();
			profitShare = agentCol.GetProfitShareAgreement(new ZDateTime(2003, 4, 20), "SEA", "FCL", "AUSYD", "USLAX", null, null);
			AssertEquals("Sender to receiver details found", expectedDetails.PK, profitShare.PK);
		}

		public void TestGetProfitShareAgreement()
		{
			SetupProfitShareDetails();

			OrgProfitShareDetails profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2003, 4, 20), "SEA", "FCL", "INBOM", "AUSYD", null, null);
			AssertEquals("Correct profit share agreement found", PS1.PK, profitShare.PK);

			profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "SEA", "FCL", "INBOM", "AUSYD", null, null);
			AssertEquals("Correct profit share agreement found", PS2.PK, profitShare.PK);

			profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2005, 4, 20), "SEA", "FCL", "INBOM", "AUSYD", null, null);
			AssertNull("No profit share agreement found", profitShare);

			profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "", "AUMEL", "AUSYD", null, null);
			AssertEquals("Correct profit share agreement found, even for country", PS3.PK, profitShare.PK);
		}

		public void TestGetProfitShareAgreement_ModeFallback()
		{
			var psALL = AddOrgProfitShareDetails(CollectionForTest, freightMode: OrgProfitShareDetailsLookups.FreightModesList.ALL.Code);
			Factory.Save();

			Func<ZString, ZString, OrgProfitShareDetails> getProfitShare = (freightMode, containerMode) => CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), freightMode, containerMode, "AUSYD", "INBOM", new OrganisationsWithTypes(Client), null);
			var messageTemplate = "{0}: freight mode {1}, container mode {2}.";
			foreach (var freightMode in FreightModes)
			{
				foreach (var containerMode in ContainerModes)
				{
					AssertEquals(string.Format(messageTemplate, "Only ALL created", freightMode, containerMode), psALL.PK, getProfitShare(freightMode, containerMode).PK);
				}
			}

			var psFreightMode = AddOrgProfitShareDetails(CollectionForTest);

			messageTemplate = "ALL and {0} created: freight mode {1}, container mode {2}.";
			foreach (var freightModeToSet in FreightModes)
			{
				psFreightMode.O4_FreightMode = freightModeToSet;
				Factory.Save();
				foreach (var freightMode in FreightModes)
				{
					foreach (var containerMode in ContainerModes)
					{
						var expectedPS = freightMode == freightModeToSet ? psFreightMode : psALL;
						AssertEquals(string.Format(messageTemplate, freightModeToSet, freightMode, containerMode), expectedPS.PK, getProfitShare(freightMode, containerMode).PK);
					}
				}
			}

			var psContainerMode = AddOrgProfitShareDetails(CollectionForTest);
			messageTemplate = "ALL, {0} and {1} created: freight mode {2}, container mode {3}.";
			foreach (var freightModeToSet in FreightModes)
			{
				psFreightMode.O4_FreightMode = freightModeToSet;
				foreach (var containerModeToSet in ContainerModes)
				{
					psContainerMode.O4_FreightMode = containerModeToSet;
					Factory.Save();
					foreach (var freightMode in FreightModes)
					{
						foreach (var containerMode in ContainerModes)
						{
							var expectedPS = containerMode == containerModeToSet ? psContainerMode : (freightMode == freightModeToSet ? psFreightMode : psALL);
							AssertEquals(string.Format(messageTemplate, freightModeToSet, containerModeToSet, freightMode, containerMode), expectedPS.PK, getProfitShare(freightMode, containerMode).PK);
						}
					}
				}
			}
		}

		#region TestGetProfitShareAgreement - JobType and GatewayAgentType

		OrgProfitShareDetails CreateProfitShareAgreement(string jobType = null, string gatewayAgentType = null) =>
			AddOrgProfitShareDetails(CollectionForTest, freightMode: OrgProfitShareDetailsLookups.FreightModesList.ALL.Code, jobType: jobType, gatewayAgentType: gatewayAgentType);

		public void TestGetProfitShareAgreement_JobTypeExactMatch()
		{
			const string sendingLocation = "AUSYD";
			const string receivingLocation = "INBOM";

			// Agreements with job type SHP have higher priority than ones with blank. Cannot test Blank if SHP exists. Let's test SHP at the end.
			var agreementGCN = CreateProfitShareAgreement(jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.BGW);
			var agreementBlank = CreateProfitShareAgreement(jobType: JobTypesList.Codes.Blank);
			Factory.Save();

			var result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", sendingLocation, receivingLocation, null, null, JobTypesList.Codes.GCN, GatewayAgentTypesList.Codes.BGW);
			result.Should().Be(agreementGCN);

			result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", sendingLocation, receivingLocation, null, null, JobTypesList.Codes.Blank);
			result.Should().Be(agreementBlank);

			var agreementSHP = CreateProfitShareAgreement(jobType: JobTypesList.Codes.SHP);
			Factory.Save();
			result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", sendingLocation, receivingLocation, null, null, JobTypesList.Codes.SHP);
			result.Should().Be(agreementSHP);

			Assert("FluentAssertions is used", true);
		}

		public void TestGetProfitShareAgreement_JobTypeFallback()
		{
			AssertGetProfitShareAgreement_JobTypeFallback(
				jobTypesForSettingUpAgreements: new[] { JobTypesList.Codes.SHP, JobTypesList.Codes.Blank },
				jobTypeForGettingProfitShareAgreement: JobTypesList.Codes.GCN,
				expectedAgreementJobType: null,
				reason: "SHP and blank should not match GCN");

			AssertGetProfitShareAgreement_JobTypeFallback(
				jobTypesForSettingUpAgreements: new[] { JobTypesList.Codes.GCN, JobTypesList.Codes.Blank },
				jobTypeForGettingProfitShareAgreement: JobTypesList.Codes.SHP,
				expectedAgreementJobType: JobTypesList.Codes.Blank,
				reason: "SHP should have Blank as fallback");

			AssertGetProfitShareAgreement_JobTypeFallback(
				jobTypesForSettingUpAgreements: new[] { JobTypesList.Codes.GCN },
				jobTypeForGettingProfitShareAgreement: JobTypesList.Codes.SHP,
				expectedAgreementJobType: null,
				reason: "GCN should not match SHP");

			AssertGetProfitShareAgreement_JobTypeFallback(
				jobTypesForSettingUpAgreements: new[] { JobTypesList.Codes.GCN },
				jobTypeForGettingProfitShareAgreement: JobTypesList.Codes.Blank,
				expectedAgreementJobType: null,
				reason: "GCN should not match blank");

			AssertGetProfitShareAgreement_JobTypeFallback(
				jobTypesForSettingUpAgreements: new[] { JobTypesList.Codes.SHP },
				jobTypeForGettingProfitShareAgreement: JobTypesList.Codes.GCN,
				expectedAgreementJobType: null,
				reason: "SHP should not match GCN");

			AssertGetProfitShareAgreement_JobTypeFallback(
				jobTypesForSettingUpAgreements: new[] { JobTypesList.Codes.SHP },
				jobTypeForGettingProfitShareAgreement: JobTypesList.Codes.Blank,
				expectedAgreementJobType: JobTypesList.Codes.SHP,
				reason: "SHP should match blank");

			AssertGetProfitShareAgreement_JobTypeFallback(
				jobTypesForSettingUpAgreements: new[] { JobTypesList.Codes.Blank },
				jobTypeForGettingProfitShareAgreement: JobTypesList.Codes.SHP,
				expectedAgreementJobType: JobTypesList.Codes.Blank,
				reason: "Blank should match SHP");

			void AssertGetProfitShareAgreement_JobTypeFallback(
				IEnumerable<string> jobTypesForSettingUpAgreements,
				string jobTypeForGettingProfitShareAgreement,
				string expectedAgreementJobType,
				string reason = "")
			{
				CollectionForTest.RemoveAndDeleteAll();

				OrgProfitShareDetails expectedAgreement = null;
				jobTypesForSettingUpAgreements
					.ForEach(jobType =>
					{
						var agreement = CreateProfitShareAgreement(jobType);
						if (expectedAgreementJobType == jobType)
						{
							expectedAgreement = agreement;
						}
					});
				Factory.Save();

				var result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", "AUSYD",
					"INBOM", null, null, jobTypeForGettingProfitShareAgreement,
					jobTypeForGettingProfitShareAgreement == JobTypesList.Codes.GCN ? GatewayAgentTypesList.Codes.BGW : null);
				result.Should().Be(expectedAgreement, reason);
			}

			Assert("FluentAssertions is used", true);
		}

		public void TestGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeExactMatch()
		{
			const string sendingLocation = "AUSYD";
			const string receivingLocation = "INBOM";

			var agreementBGW = CreateProfitShareAgreement(jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.BGW);
			var agreementSGW = CreateProfitShareAgreement(jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.SGW);
			var agreementRGW = CreateProfitShareAgreement(jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.RGW);
			// Agreement is setup but never referred to because of exact matches.
			var agreementBlank = CreateProfitShareAgreement(jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.Blank);
			Factory.Save();

			var result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", sendingLocation, receivingLocation, null, null, JobTypesList.Codes.GCN, GatewayAgentTypesList.Codes.BGW);
			result.Should().Be(agreementBGW);
			result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", sendingLocation, receivingLocation, null, null, JobTypesList.Codes.GCN, GatewayAgentTypesList.Codes.SGW);
			result.Should().Be(agreementSGW);
			result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", sendingLocation, receivingLocation, null, null, JobTypesList.Codes.GCN, GatewayAgentTypesList.Codes.RGW);
			result.Should().Be(agreementRGW);

			Func<OrgProfitShareDetails> funcWithException = () =>
				CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", sendingLocation, receivingLocation, null, null, JobTypesList.Codes.GCN, GatewayAgentTypesList.Codes.Blank);
			funcWithException.Should()
				.Throw<DeveloperNotificationException>()
				.WithMessage("Gateway consol should have at least one gateway agent.");

			Assert("FluentAssertions is used", true);
		}

		public void TestGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback()
		{
			AssertGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				gatewayAgentTypeForGettingProfitShareAgreement: GatewayAgentTypesList.Codes.BGW,
				expectedAgreementGatewayAgentType: GatewayAgentTypesList.Codes.SGW,
				reason: "Fallback should be with conditions in order SGW, RGW, blank");

			AssertGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				gatewayAgentTypeForGettingProfitShareAgreement: GatewayAgentTypesList.Codes.BGW,
				expectedAgreementGatewayAgentType: GatewayAgentTypesList.Codes.RGW,
				reason: "Fallback should be with conditions in order RGW, blank");

			AssertGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.Blank
				},
				gatewayAgentTypeForGettingProfitShareAgreement: GatewayAgentTypesList.Codes.BGW,
				expectedAgreementGatewayAgentType: GatewayAgentTypesList.Codes.Blank,
				reason: "Fallback should be with condition blank");

			AssertGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				gatewayAgentTypeForGettingProfitShareAgreement: GatewayAgentTypesList.Codes.SGW,
				expectedAgreementGatewayAgentType: GatewayAgentTypesList.Codes.Blank,
				reason: "Fallback should be with condition blank");

			AssertGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.RGW
				},
				gatewayAgentTypeForGettingProfitShareAgreement: GatewayAgentTypesList.Codes.SGW,
				expectedAgreementGatewayAgentType: null,
				reason: "There should be no matched condition");

			AssertGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.Blank
				},
				gatewayAgentTypeForGettingProfitShareAgreement: GatewayAgentTypesList.Codes.RGW,
				expectedAgreementGatewayAgentType: GatewayAgentTypesList.Codes.Blank,
				reason: "Fallback should be with condition blank");

			AssertGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW
				},
				gatewayAgentTypeForGettingProfitShareAgreement: GatewayAgentTypesList.Codes.RGW,
				expectedAgreementGatewayAgentType: null,
				reason: "There should be no matched condition");

			void AssertGetProfitShareAgreement_JobTypeGCN_GatewayAgentTypeFallback(
				IEnumerable<string> gatewayAgentTypesForSettingUpAgreements,
				string gatewayAgentTypeForGettingProfitShareAgreement,
				string expectedAgreementGatewayAgentType,
				string reason = "")
			{
				CollectionForTest.RemoveAndDeleteAll();

				OrgProfitShareDetails expectedAgreement = null;
				gatewayAgentTypesForSettingUpAgreements
					.ForEach(gatewayAgentType =>
					{
						var agreement = CreateProfitShareAgreement(jobType: JobTypesList.Codes.GCN, gatewayAgentType);
						if (expectedAgreementGatewayAgentType == gatewayAgentType)
						{
							expectedAgreement = agreement;
						}
					});
				Factory.Save();

				var result = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "LSE", "AUSYD", "INBOM", null, null, JobTypesList.Codes.GCN, gatewayAgentTypeForGettingProfitShareAgreement);
				result.Should().Be(expectedAgreement, reason);
			}

			Assert("FluentAssertions is used", true);
		}

		#endregion

		public void TestModesListsForTestAreUpToDate()
		{
			var combinedModeList = new List<string>(ContainerModes);
			combinedModeList.AddRange(FreightModes);
			combinedModeList.Add(OrgProfitShareDetailsLookups.FreightModesList.ALL.Code);

			var profitShare = Factory.New<OrgProfitShareDetails>();
			AssertContainsExactElementsInAnyOrder(combinedModeList, profitShare.Lookups.FreightModes.Cast<ICodeDescription>().Select(x => x.Code));
		}

		#region Implementation

		protected virtual OrgProfitShareDetailsDependentCollection CreateCollectionForTest(OrgAgentRelationship agent)
		{
			return new OrgProfitShareDetailsDependentCollection(agent);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var agent = Factory.New<OrgAgentRelationship>();
			return CreateCollectionForTest(agent);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var rcvAgent = CreateOrganisation("Receiving Agent");
			var sndAgent = CreateOrganisation("Send Agent");
			ControllingAgent = CreateOrganisation("Other Agent");
			Client = CreateOrganisation("Test Org");
			Client2 = CreateOrganisation("Test Org 2");
			Client3 = CreateOrganisation("Test Org 3");

			var agent = CreateAgentRelationship(sndAgent, rcvAgent);
			CollectionForTest = CreateCollectionForTest(agent);
			CombinedCollection = new OrgProfitShareDetailsDependentCollection(agent);

			Factory.Save();

			ContainerModes = new[]
			{
				OrgProfitShareDetailsLookups.FreightModesList.ULD.Code,
				OrgProfitShareDetailsLookups.FreightModesList.LSE.Code,
				OrgProfitShareDetailsLookups.FreightModesList.FCL.Code,
				OrgProfitShareDetailsLookups.FreightModesList.LCL.Code,
				OrgProfitShareDetailsLookups.FreightModesList.BLK.Code,
				OrgProfitShareDetailsLookups.FreightModesList.LQD.Code,
				OrgProfitShareDetailsLookups.FreightModesList.BBK.Code,
				OrgProfitShareDetailsLookups.FreightModesList.ROR.Code,
				OrgProfitShareDetailsLookups.FreightModesList.FTL.Code,
				OrgProfitShareDetailsLookups.FreightModesList.LTL.Code,
				OrgProfitShareDetailsLookups.FreightModesList.OTH.Code,
				OrgProfitShareDetailsLookups.FreightModesList.GRP.Code,
				OrgProfitShareDetailsLookups.FreightModesList.BCN.Code
			};
			FreightModes = new[]
			{
				OrgProfitShareDetailsLookups.FreightModesList.AIR.Code,
				OrgProfitShareDetailsLookups.FreightModesList.SEA.Code,
				OrgProfitShareDetailsLookups.FreightModesList.RAI.Code,
				OrgProfitShareDetailsLookups.FreightModesList.ROA.Code
			};
		}

		protected virtual void SetupProfitShareDetails()
		{
			PS1 = AddOrgProfitShareDetails(CollectionForTest, new ZDateTime(2003, 4, 13), new ZDateTime(2003, 5, 13), freightMode: OrgProfitShareDetailsLookups.FreightModesList.FCL.Code, sendingPortOrCountry: "INBOM", receivingPortOrCountry: "AUSYD");
			PS2 = AddOrgProfitShareDetails(CollectionForTest, freightMode: OrgProfitShareDetailsLookups.FreightModesList.FCL.Code, sendingPortOrCountry: "INBOM", receivingPortOrCountry: "AUSYD");
			PS3 = AddOrgProfitShareDetails(CollectionForTest, receivingPortOrCountry: "AUSYD");
			PS4 = AddOrgProfitShareDetails(CollectionForTest);
			PS5 = AddOrgProfitShareDetails(CollectionForTest, sendingPortOrCountry: "", receivingPortOrCountry: "", controllingAgent: ControllingAgent);

			Factory.Save();
		}

		protected OrgProfitShareDetails AddOrgProfitShareDetails(OrgProfitShareDetailsDependentCollection collection,
			string freightMode = "AIR",
			string sendingPortOrCountry = "AU",
			string receivingPortOrCountry = "INBOM",
			OrgHeader controllingAgent = null,
			OrgHeader orgOverride = null,
			string orgOverrideType = null,
			string jobType = null,
			string gatewayAgentType = null)
		{
			return AddOrgProfitShareDetails(collection, new ZDateTime(2004, 4, 13), new ZDateTime(2004, 5, 13),
				freightMode, sendingPortOrCountry, receivingPortOrCountry, controllingAgent, orgOverride,
				orgOverrideType, jobType, gatewayAgentType);
		}

		OrgProfitShareDetails AddOrgProfitShareDetails(OrgProfitShareDetailsDependentCollection collection,
			ZDateTime startDate,
			ZDateTime endDate,
			string freightMode = "AIR",
			string sendingPortOrCountry = "AU",
			string receivingPortOrCountry = "INBOM",
			OrgHeader controllingAgent = null,
			OrgHeader orgOverride = null,
			string orgOverrideType = null,
			string jobType = null,
			string gatewayAgentType = null)
		{
			var ps = collection.AddNew();
			ps.O4_StartDate = startDate;
			ps.O4_EndDate = endDate;
			ps.O4_AgreementType = "ABC";
			ps.O4_FreightMode = freightMode;
			ps.O4_OH_OrgOverride = ZGuid.Empty;
			ps.O4_SendingPortOrCountry = sendingPortOrCountry;
			ps.O4_ReceivingPortOrCountry = receivingPortOrCountry;
			ps.O4_OH_ControllingAgent = controllingAgent != null ? controllingAgent.PK : ZGuid.Empty;
			ps.O4_OH_OrgOverride = orgOverride != null ? orgOverride.PK : ZGuid.Empty;
			if (orgOverrideType != null)
			{
				ps.O4_OrgOverrideType = orgOverrideType;
			}

			ps.O4_JobType = jobType;
			ps.O4_GatewayAgentType = gatewayAgentType;

			return ps;
		}

		OrgHeader CreateOrganisation(ZString fullName)
		{
			return CreateOrganisation(Factory, fullName);
		}

		static OrgHeader CreateOrganisation(BusinessObjectFactory factory, ZString fullName)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = fullName;

			return org;
		}

		OrgAgentRelationship CreateAgentRelationship(OrgHeader sendingAgent, OrgHeader receivingAgent)
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_ReceivingAgent = receivingAgent?.PK ?? ZGuid.Empty;
			agentRelationship.O3_OH_SendingAgent = sendingAgent?.PK ?? ZGuid.Empty;

			return agentRelationship;
		}

		RefZoneHeader CreateZone(string zoneCode, string zoneType)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_Description = zoneCode;
			zone.FZ_ZoneType = zoneType;

			return zone;
		}

		protected OrgProfitShareDetailsDependentCollection CollectionForTest;
		protected OrgProfitShareDetailsDependentCollection CombinedCollection;
		protected OrgProfitShareDetails PS1, PS2, PS3, PS4, PS5;
		protected OrgProfitShareDetails ClientSpecificPS1, ClientSpecificPS2;
		protected OrgHeader Client, Client2, Client3;
		protected OrgHeader ControllingAgent;
		string[] ContainerModes;
		string[] FreightModes;

		#endregion

		public class OrganisationsWithTypesTest : TestCaseWithFactory
		{
			public void TestConstructor()
			{
				var org1 = CreateOrganisation(Factory, "Org1");
				var org2 = CreateOrganisation(Factory, "Org2");
				var org3 = CreateOrganisation(Factory, "Org3");
				var org4 = CreateOrganisation(Factory, "Org4");
				var org5 = CreateOrganisation(Factory, "Org5");
				var org6 = CreateOrganisation(Factory, "Org6");
				var org7 = CreateOrganisation(Factory, "Org7");
				var invoicingSupporter = new DummyJobHeaderParentJobInvoicingSupporter();
				invoicingSupporter.Consignee = org2;
				invoicingSupporter.Consignor = org3;
				invoicingSupporter.PickUpAgent = org4;
				invoicingSupporter.DeliveryAgent = org5;
				invoicingSupporter.ImportBroker = org6;
				invoicingSupporter.ExportBroker = org7;

				var orgWithTypes = new OrganisationsWithTypes(null);
				AssertNull("LocalClient", orgWithTypes.LocalClient);
				AssertNull("Consignee", orgWithTypes.Consignee);
				AssertNull("Consignor", orgWithTypes.Consignor);
				AssertNull("PickUpAgent", orgWithTypes.PickUpAgent);
				AssertNull("ImportBroker", orgWithTypes.ImportBroker);
				AssertNull("ExportBroker", orgWithTypes.ExportBroker);

				orgWithTypes = new OrganisationsWithTypes(org1);
				AssertEquals("LocalClient", org1, orgWithTypes.LocalClient);
				AssertNull("Consignee", orgWithTypes.Consignee);
				AssertNull("Consignor", orgWithTypes.Consignor);
				AssertNull("PickUpAgent", orgWithTypes.PickUpAgent);
				AssertNull("ImportBroker", orgWithTypes.ImportBroker);
				AssertNull("ExportBroker", orgWithTypes.ExportBroker);

				orgWithTypes = new OrganisationsWithTypes(org1, invoicingSupporter);
				AssertEquals("LocalClient", org1, orgWithTypes.LocalClient);
				AssertEquals("Consignee", org2, orgWithTypes.Consignee);
				AssertEquals("Consignor", org3, orgWithTypes.Consignor);
				AssertEquals("PickUpAgent", org4, orgWithTypes.PickUpAgent);
				AssertEquals("ImportBroker", org6, orgWithTypes.ImportBroker);
				AssertEquals("ExportBroker", org7, orgWithTypes.ExportBroker);

				orgWithTypes = new OrganisationsWithTypes(null, invoicingSupporter);
				AssertNull("LocalClient", orgWithTypes.LocalClient);
				AssertEquals("Consignee", org2, orgWithTypes.Consignee);
				AssertEquals("Consignor", org3, orgWithTypes.Consignor);
				AssertEquals("PickUpAgent", org4, orgWithTypes.PickUpAgent);
				AssertEquals("ImportBroker", org6, orgWithTypes.ImportBroker);
				AssertEquals("ExportBroker", org7, orgWithTypes.ExportBroker);
			}

			public void TestGetOrgTypePKPairsAndPKs()
			{
				var org1 = CreateOrganisation(Factory, "Org1");
				var org2 = CreateOrganisation(Factory, "Org2");
				var org3 = CreateOrganisation(Factory, "Org3");
				var org4 = CreateOrganisation(Factory, "Org4");
				var org5 = CreateOrganisation(Factory, "Org5");
				var org6 = CreateOrganisation(Factory, "Org6");
				var org7 = CreateOrganisation(Factory, "Org7");
				var invoicingSupporter = new DummyJobHeaderParentJobInvoicingSupporter();
				invoicingSupporter.Consignee = org2;
				invoicingSupporter.Consignor = org3;
				invoicingSupporter.DeliveryAgent = org4;
				invoicingSupporter.PickUpAgent = org5;
				invoicingSupporter.ImportBroker = org6;
				invoicingSupporter.ExportBroker = org7;

				var orgWithTypes = new OrganisationsWithTypes(org1);
				var pairs = orgWithTypes.GetOrgTypePKPairs();
				AssertEquals(1, pairs.Length);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.LOC.Code, org1.PK), pairs[0]);

				orgWithTypes = new OrganisationsWithTypes(org1, invoicingSupporter);
				pairs = orgWithTypes.GetOrgTypePKPairs();
				AssertEquals(6, pairs.Length);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.LOC.Code, org1.PK), pairs[0]);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNE.Code, org2.PK), pairs[1]);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNR.Code, org3.PK), pairs[2]);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.PUA.Code, org5.PK), pairs[3]);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code, org6.PK), pairs[4]);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.EBR.Code, org7.PK), pairs[5]);

				var combinedOrgTypeList = new List<ZString>(pairs.Select(x => x.Item1));
				combinedOrgTypeList.Add(OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code);

				var profitShare = Factory.New<OrgProfitShareDetails>();
				AssertContainsExactElementsInAnyOrder("All org types were tested.", combinedOrgTypeList, profitShare.Lookups.OrgOverrideTypes.Cast<ICodeDescription>().Select(x => x.Code));

				orgWithTypes.LocalClient = null;
				orgWithTypes.PickUpAgent = null;
				orgWithTypes.ImportBroker = null;
				pairs = orgWithTypes.GetOrgTypePKPairs();
				AssertEquals(3, pairs.Length);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNE.Code, org2.PK), pairs[0]);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNR.Code, org3.PK), pairs[1]);
				AssertEquals(Tuple.Create((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.EBR.Code, org7.PK), pairs[2]);
			}

			public void TestApplyProfitShareDetailsFilter()
			{
				var org0 = CreateOrganisation(Factory, "Org0");
				var org1 = CreateOrganisation(Factory, "Org1");
				var org2 = CreateOrganisation(Factory, "Org2");
				var org3 = CreateOrganisation(Factory, "Org3");
				var org4 = CreateOrganisation(Factory, "Org4");
				var org5 = CreateOrganisation(Factory, "Org5");
				var org6 = CreateOrganisation(Factory, "Org6");
				var org7 = CreateOrganisation(Factory, "Org7");
				var org8 = CreateOrganisation(Factory, "Org8");
				var org9 = CreateOrganisation(Factory, "Org8");
				var invoicingSupporter = new DummyJobHeaderParentJobInvoicingSupporter();

				var agentRelationship = Factory.New<OrgAgentRelationship>();
				agentRelationship.O3_ProfitShareType = "AGY";
				var clientPS0 = CreateProfitShare(org0, OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code);
				var clientPS1 = CreateProfitShare(org1, OrgProfitShareDetailsLookups.OrgOverrideTypesList.LOC.Code);
				var clientPS2 = CreateProfitShare(org2, OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code);
				var clientPS3 = CreateProfitShare(org3, OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNE.Code);
				var clientPS4 = CreateProfitShare(org4, OrgProfitShareDetailsLookups.OrgOverrideTypesList.PUA.Code);
				var clientPS5 = CreateProfitShare(org5, OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code);
				var clientPS6 = CreateProfitShare(org6, OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code);
				var clientPS7 = CreateProfitShare(org7, OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code);
				var clientPS8 = CreateProfitShare(org8, OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code);
				var clientPS9 = CreateProfitShare(null, null);

				var profitShareMatcher = new ProfitShareMatcher(Factory);
				var inputPSAs = new[] { clientPS0, clientPS1, clientPS2, clientPS3, clientPS4, clientPS5, clientPS6, clientPS7, clientPS8, clientPS9 };
				AssertFilter(inputPSAs, ZDate.Today, new[] { clientPS1, clientPS9 });

				invoicingSupporter.Consignee = org2;
				AssertFilter(inputPSAs, ZDate.Today, new[] { clientPS1, clientPS2, clientPS9 });

				invoicingSupporter.Consignor = org3;
				AssertFilter(inputPSAs, ZDate.Today, new[] { clientPS1, clientPS2, clientPS9 });

				invoicingSupporter.DeliveryAgent = org4;
				AssertFilter(inputPSAs, ZDate.Today, new[] { clientPS1, clientPS2, clientPS9 });

				invoicingSupporter.PickUpAgent = org5;
				AssertFilter(inputPSAs, ZDate.Today, new[] { clientPS1, clientPS2, clientPS9 });

				invoicingSupporter.ImportBroker = org6;
				AssertFilter(inputPSAs, ZDate.Today, new[] { clientPS1, clientPS2, clientPS6, clientPS9 });

				invoicingSupporter.ExportBroker = org7;
				AssertFilter(inputPSAs, ZDate.Today, new[] { clientPS1, clientPS2, clientPS6, clientPS9 });

				AssertFilter(inputPSAs, ZDate.Today.AddDays(-1), Array.Empty<OrgProfitShareDetails>());

				AssertFilter(inputPSAs, ZDate.Today.AddDays(1), new[] { clientPS1, clientPS2, clientPS6, clientPS9 });

				AssertFilter(inputPSAs, ZDate.Today.AddDays(2), Array.Empty<OrgProfitShareDetails>());

				void AssertFilter(IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList, ZDate jobDate, IEnumerable<OrgProfitShareDetails> expectedPSAs)
				{
					var orgWithTypes = new OrganisationsWithTypes(org1, invoicingSupporter);
					AssertContainsExactElementsInAnyOrder(expectedPSAs, profitShareMatcher.ApplyProfitShareDetailsFilter(orgProfitShareDetailsList, jobDate, orgWithTypes));
				}

				OrgProfitShareDetails CreateProfitShare(OrgHeader orgOverride, string orgType)
				{
					var profitShare = agentRelationship.ProfitShareDetails.AddNew();
					profitShare.O4_FreightMode = "AIR";
					profitShare.O4_StartDate = ZDate.Today;
					profitShare.O4_EndDate = ZDate.Today.AddDays(1);
					profitShare.O4_SendingPortOrCountry = "AUSYD";
					profitShare.O4_ReceivingPortOrCountry = "SGSIN";
					if (orgOverride != null)
					{
						profitShare.O4_OH_OrgOverride = orgOverride.PK;
						profitShare.O4_OrgOverrideType = orgType;
					}
					profitShare.O4_JobType = "GCN";
					profitShare.O4_GatewayAgentType = "";
					profitShare.O4_GatewayProfitApportionmentMethod = "SHP";

					return profitShare;
				}
			}
		}
	}
}
