using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment.Test
{
	class TelEdgeJsonEquipmentPlannerTests : TestCaseWithFactory
	{
		public void TestFlattenJsonWith2AxleGroupFullVehicle()
		{
			// Arrange
			var expectedNodes = new Dictionary<TelSubEquipmentKey, (TelSubEquipmentData from, TelSubEquipmentData to)>();
			var baseEquipment = GenerateSubEquipment(deviceIdentifier, "RQ", @"<Configuration>
  <vehicleType>Prime Mover</vehicleType>
  <axleConfig>12</axleConfig>
</Configuration>");
			var axleGroup1 = GenerateSubEquipment("00", "A", @"<Configuration>
  <tareMass>3000</tareMass>
  <tarePressure>8.5</tarePressure>
  <calibrationMass>3000</calibrationMass>
  <calibrationPressure>8.5</calibrationPressure>
  <massLimit>20000</massLimit>
</Configuration>");
			var axleGroup2 = GenerateSubEquipment("01", "A", @"<Configuration>
  <tareMass>3000</tareMass>
  <tarePressure>8.5</tarePressure>
  <calibrationMass>3000</calibrationMass>
  <calibrationPressure>8.5</calibrationPressure>
  <massLimit>20000</massLimit>
</Configuration>");
			var obm1 = GenerateSubEquipment("01", "O", string.Empty);
			var obm2 = GenerateSubEquipment("02", "O", string.Empty);
			expectedNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			expectedNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			expectedNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			expectedNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			expectedNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));

			// Act
			var result = treePlanner.FlattenJson(deviceIdentifier, vehicleInfoBasic);

			// Assert
			AssertNodesAreEquivalent(expectedNodes, result);
		}

		public void TestFlattenJsonWith1AxleGroupFullVehicle()
		{
			// Arrange
			var expectedNodes = new Dictionary<TelSubEquipmentKey, (TelSubEquipmentData from, TelSubEquipmentData to)>();
			var baseEquipment = GenerateSubEquipment(deviceIdentifier, "RQ", @"<Configuration>
  <vehicleType>B Trailer</vehicleType>
  <axleConfig>3</axleConfig>
</Configuration>");
			var axleGroup1 = GenerateSubEquipment("00", "A", @"<Configuration>
  <tareMass>3000</tareMass>
  <tarePressure>8.5</tarePressure>
  <calibrationMass>3000</calibrationMass>
  <calibrationPressure>8.5</calibrationPressure>
  <massLimit>20000</massLimit>
</Configuration>");
			var obm1 = GenerateSubEquipment("01", "O", string.Empty);
			expectedNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			expectedNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			expectedNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));

			// Act
			var result = treePlanner.FlattenJson(deviceIdentifier, vehicleInfoBTrailer);

			// Assert
			AssertNodesAreEquivalent(expectedNodes, result);
		}

		public void TestFlattenJsonWithNullObms()
		{
			// Arrange
			var expectedNodes = new Dictionary<TelSubEquipmentKey, (TelSubEquipmentData from, TelSubEquipmentData to)>();
			var baseEquipment = GenerateSubEquipment(deviceIdentifier, "RQ", @"<Configuration>
  <vehicleType>Prime Mover</vehicleType>
  <axleConfig>12</axleConfig>
</Configuration>");
			var axleGroup1 = GenerateSubEquipment("00", "A", @"<Configuration>
  <tareMass />
  <tarePressure />
  <calibrationMass />
  <calibrationPressure />
  <massLimit />
</Configuration>");
			var axleGroup2 = GenerateSubEquipment("01", "A", @"<Configuration>
  <tareMass />
  <tarePressure />
  <calibrationMass />
  <calibrationPressure />
  <massLimit />
</Configuration>");
			expectedNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			expectedNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			expectedNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));

			// Act
			var result = treePlanner.FlattenJson(deviceIdentifier, vehicleInfoNullObms);

			// Assert
			AssertNodesAreEquivalent(expectedNodes, result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			deviceIdentifier = "0102030405060708090A0B0C";
			treePlanner = new TelEdgeJsonEquipmentPlanner();
		}

		string deviceIdentifier;
		TelEdgeJsonEquipmentPlanner treePlanner;

		#region Assertions
		public void AssertNodesAreEquivalent(
			IDictionary<TelSubEquipmentKey, (TelSubEquipmentData from, TelSubEquipmentData to)> expectedNodes,
			IDictionary<TelSubEquipmentKey, (TelSubEquipmentData from, TelSubEquipmentData to)> givenNodes)
		{
			AssertEquals(expectedNodes.Count, givenNodes.Count);
			foreach (var pair in expectedNodes)
			{
				AssertEquals(pair.Value.from.id, givenNodes[pair.Key].from.id);
				AssertEquals(pair.Value.from.type, givenNodes[pair.Key].from.type);
				AssertEquals(pair.Value.from.configuration, givenNodes[pair.Key].from.configuration);
				AssertEquals(pair.Value.to.id, givenNodes[pair.Key].to.id);
				AssertEquals(pair.Value.to.type, givenNodes[pair.Key].to.type);
				AssertEquals(pair.Value.to.configuration, givenNodes[pair.Key].to.configuration);
			}
		}
		#endregion

		#region Implementation

		TelSubEquipmentData GenerateSubEquipment(string id, string type, string configuration)
		{
			return new TelSubEquipmentData()
			{
				id = id,
				type = type,
				configuration = configuration,
			};
		}
		#endregion

		#region VehicleInfoStrings
		readonly string vehicleInfoBasic = @"{
	""vehicleType"": ""Prime Mover"",
	""axleConfig"": ""12"",
	""axleGroups"": [
		{
			""obms"": [
				{
					""obmId"": ""01"",
				}
			],
			""tareMass"": 3000,
			""tarePressure"": 8.5,
			""calibrationMass"": 3000,
			""calibrationPressure"": 8.5,
			""massLimit"": 20000,
			""axles"": [
				{
					""wheels"": [
						{
							""tpmId"": ""0x0001""
						},
						{
							""tpmId"": ""0x0002""
						}
					]
				}
			]
		},
		{
			""obms"": [
				{
					""obmId"": ""02"",
				}
			],
			""tareMass"": 3000,
			""tarePressure"": 8.5,
			""calibrationMass"": 3000,
			""calibrationPressure"": 8.5,
			""massLimit"": 20000,
			""axles"": [
				{
					""wheels"": [
						{
							""tpmId"": ""0x0003""
						},
						{
							""tpmId"": ""0x0004""
						},
						{
							""tpmId"": ""0x0005""
						},
						{
							""tpmId"": ""0x0006""
						}
					]
				},
				{
					""wheels"": [
						{
							""tpmId"": ""0x0007""
						},
						{
							""tpmId"": ""0x0008""
						},
						{
							""tpmId"": ""0x0009""
						},
						{
							""tpmId"": ""0x000A""
						}
					]
				}
			]
		}
	]
}";
		readonly string vehicleInfoNullObms = @"{
	""vehicleType"": ""Prime Mover"",
	""axleConfig"": ""12"",
	""axleGroups"": [
		{
			""obms"": null,
			""tareMass"": null,
			""tarePressure"":  null,
			""calibrationMass"": null,
			""calibrationPressure"": null,
			""massLimit"": null,
			""axles"": [
				{
					""wheels"": [
						{
							""tpmId"": null
						},
						{
							""tpmId"": null
						}
					]
				}
			]
		},
		{
			""obms"": null,
			""tareMass"": null,
			""tarePressure"": null,
			""calibrationMass"": null,
			""calibrationPressure"": null,
			""massLimit"": null,
			""axles"": [
				{
					""wheels"": [
						{
							""tpmId"": null
						},
						{
							""tpmId"": null
						},
						{
							""tpmId"": null
						},
						{
							""tpmId"": null
						}
					]
				},
				{
					""wheels"": [
						{
							""tpmId"": null
						},
						{
							""tpmId"": null
						},
						{
							""tpmId"": null
						},
						{
							""tpmId"": null
						}
					]
				}
			]
		}
	]
}";
		readonly string vehicleInfoBTrailer = @"{
	""vehicleType"": ""B Trailer"",
	""axleConfig"": ""3"",
	""axleGroups"": [
		{
			""obms"": [
				{
					""obmId"": ""01"",
				}
			],
			""tareMass"": 3000,
			""tarePressure"": 8.5,
			""calibrationMass"": 3000,
			""calibrationPressure"": 8.5,
			""massLimit"": 20000,
			""axles"": [
				{
					""wheels"": [
						{
							""tpmId"": ""0x0001""
						},
						{
							""tpmId"": ""0x0002""
						},
						{
							""tpmId"": ""0x0003""
						},
						{
							""tpmId"": ""0x0004""
						}
					]
				},
				{
					""wheels"": [
						{
							""tpmId"": ""0x0005""
						},
						{
							""tpmId"": ""0x0006""
						},
						{
							""tpmId"": ""0x0007""
						},
						{
							""tpmId"": ""0x0008""
						}
					]
				},
				{
					""wheels"": [
						{
							""tpmId"": ""0x0009""
						},
						{
							""tpmId"": ""0x000A""
						},
						{
							""tpmId"": ""0x000B""
						},
						{
							""tpmId"": ""0x000C""
						}
					]
				}
			]
		}
	]
}";
		#endregion
	}
}
