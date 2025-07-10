using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsEquipment
{
	public class TelEquipmentTreeGeneratorTests : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			deviceIdentifier = "0102030405060708090A0B0C";
			loggerMock = new Mock<ILogger>(MockBehavior.Strict);
			jsonPlannerMock = new Mock<IJsonPlanner>(MockBehavior.Strict);
			dbTreePlanner = new TelEdgeDbTreePlanner(Factory);
			dbTreeGenerator = new TelEquipmentTreeGenerator(Factory, jsonPlannerMock.Object, dbTreePlanner, loggerMock.Object);
			startTime = DateTimeOffset.Now.AddDays(-100);
			vehicleDefaultConfig = @"<Configuration>
  <vehicleType>Prime Mover</vehicleType>
  <axleConfig>12</axleConfig>
</Configuration>";
			axle1DefaultConfig = @"<Configuration>
  <tareMass>3000</tareMass>
  <tarePressure>8.5</tarePressure>
  <massLimit>20000</massLimit>
</Configuration>";
			axle2DefaultConfig = @"<Configuration>
  <tareMass>3000</tareMass>
  <tarePressure>8.5</tarePressure>
  <massLimit>20000</massLimit>
</Configuration>";
			obmDefaultConfig = "";
			vehicleNewConfig = @"<Configuration>
  <vehicleType>B Trailer</vehicleType>
  <axleConfig>3</axleConfig>
</Configuration>";
			vehicleThirdConfig = @"<Configuration>
  <vehicleType>A Trailer</vehicleType>
  <axleConfig>2</axleConfig>
</Configuration>";
			axle1NewConfig = @"<Configuration>
  <tareMass>3</tareMass>
  <tarePressure>150</tarePressure>
  <massLimit>2</massLimit>
</Configuration>";
			axle2NewConfig = @"<Configuration>
  <tareMass>3</tareMass>
  <tarePressure>150</tarePressure>
  <massLimit>2</massLimit>
</Configuration>";
			axle1ThirdConfig = @"<Configuration>
  <tareMass>1</tareMass>
  <tarePressure>15</tarePressure>
  <massLimit>2</massLimit>
</Configuration>";
			axle2ThirdConfig = @"<Configuration>
  <tareMass>1</tareMass>
  <tarePressure>15</tarePressure>
  <massLimit>2</massLimit>
</Configuration>";
		}

		Mock<ILogger> loggerMock;
		Mock<IJsonPlanner> jsonPlannerMock;
		IDbTreeGenerator dbTreeGenerator;
		IDbTreePlanner dbTreePlanner;
		string deviceIdentifier;
		DateTimeOffset startTime;
		string vehicleDefaultConfig;
		string vehicleNewConfig;
		string vehicleThirdConfig;
		string axle1DefaultConfig;
		string axle2DefaultConfig;
		string axle1ThirdConfig;
		string axle2ThirdConfig;
		string obmDefaultConfig;
		string axle1NewConfig;
		string axle2NewConfig;

		public void TestGenerateTreeCaseReplaceVehicle()
		{
			// Arrange
			GenerateExistingEquipment();

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleNewConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1DefaultConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2DefaultConfig);
			var obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);

			// Act
			var configTime = startTime.AddDays(1);
			dbTreeGenerator.GenerateTree(deviceIdentifier, configTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(2, 4);
			AssertNumberOfSubEquipments(6);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, configTime.AddHours(5), out var currentTree);
			AssertEquals(4, currentTree.Count);
		}

		public void TestGenerateTreeCaseReplaceVehicleAndAxles()
		{
			// Arrange
			GenerateExistingEquipment();

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleNewConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1NewConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2NewConfig);
			var obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);

			// Act
			var configTime = startTime.AddDays(1);
			dbTreeGenerator.GenerateTree(deviceIdentifier, configTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(4, 4);
			AssertNumberOfSubEquipments(8);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, configTime.AddHours(5), out var currentTree);
			AssertEquals(4, currentTree.Count);
		}

		public void TestGenerateTreeNewVehicleConfigSentOlderThanCurrent()
		{
			// Arrange
			GenerateExistingEquipment();

			var t2Nodes = new EquipmentFlatJson();
			var t2BaseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleNewConfig);
			var t2AxleGroup1 = GenerateSubEquipmentData("01", "A", axle1DefaultConfig);
			var t2AxleGroup2 = GenerateSubEquipmentData("02", "A", axle2DefaultConfig);
			var t2Obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var t2Obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2BaseEquipment.id, type = t2BaseEquipment.type }, (new TelSubEquipmentData(), t2BaseEquipment));
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2AxleGroup1.id, type = t2AxleGroup1.type }, (t2BaseEquipment, t2AxleGroup1));
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2AxleGroup2.id, type = t2AxleGroup2.type }, (t2BaseEquipment, t2AxleGroup2));
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2Obm1.id, type = t2Obm1.type }, (t2AxleGroup1, t2Obm1));
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2Obm2.id, type = t2Obm2.type }, (t2AxleGroup2, t2Obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(t2Nodes);
			var t2ConfigTime = startTime.AddDays(100);
			dbTreeGenerator.GenerateTree(deviceIdentifier, t2ConfigTime, "Config");
			Factory.Save();

			// Precondition
			AssertNumberOfClosedAndOpenEdges(2, 4);
			AssertNumberOfSubEquipments(6);

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleThirdConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1DefaultConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2DefaultConfig);
			var obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);
			var t1ConfigTime = t2ConfigTime.AddDays(-5);

			// Act
			dbTreeGenerator.GenerateTree(deviceIdentifier, t1ConfigTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(4, 4);
			AssertNumberOfSubEquipments(7);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, t1ConfigTime.AddHours(5), out var currentTree);
			AssertEquals(4, currentTree.Count);
		}

		public void TestGenerateTreeCaseReplaceAxles()
		{
			// Arrange
			GenerateExistingEquipment();

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1NewConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2NewConfig);
			var obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);

			// Act
			var configTime = startTime.AddDays(1);
			dbTreeGenerator.GenerateTree(deviceIdentifier, configTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(4, 4);
			AssertNumberOfSubEquipments(7);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, configTime.AddHours(5), out var currentTree);
			AssertEquals(4, currentTree.Count);
		}

		public void TestGenerateTreeOneToManyObms()
		{
			// Arrange
			GenerateExistingEquipment();

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1DefaultConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2DefaultConfig);
			var obm1 = GenerateSubEquipmentData("05", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("06", "O", obmDefaultConfig);
			var obm3 = GenerateSubEquipmentData("07", "O", obmDefaultConfig);
			var obm4 = GenerateSubEquipmentData("08", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm3.id, type = obm3.type }, (axleGroup1, obm3));
			newNodes.Add(new TelSubEquipmentKey() { id = obm4.id, type = obm4.type }, (axleGroup2, obm4));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);

			// Act
			var configTime = startTime.AddDays(1);
			dbTreeGenerator.GenerateTree(deviceIdentifier, configTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(2, 6);
			AssertNumberOfSubEquipments(9);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, configTime.AddHours(5), out var currentTree);
			AssertEquals(6, currentTree.Count);
		}

		public void TestGenerateTreeAddNewAxleAndObm()
		{
			// Arrange
			GenerateExistingEquipment();

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1DefaultConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2DefaultConfig);
			var axleGroup3 = GenerateSubEquipmentData("03", "A", axle2DefaultConfig);
			var obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			var obm3 = GenerateSubEquipmentData("05", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup3.id, type = axleGroup3.type }, (baseEquipment, axleGroup3));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm3.id, type = obm3.type }, (axleGroup3, obm3));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);

			// Act
			var configTime = startTime.AddDays(1);
			dbTreeGenerator.GenerateTree(deviceIdentifier, configTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(0, 6);
			AssertNumberOfSubEquipments(7);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, configTime.AddHours(5), out var currentTree);
			AssertEquals(6, currentTree.Count);
		}

		public void TestGenerateTreeNoObms()
		{
			// Arrange
			GenerateExistingEquipment();

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1DefaultConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2DefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);

			// Act
			var configTime = startTime.AddDays(1);
			dbTreeGenerator.GenerateTree(deviceIdentifier, configTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(2, 2);
			AssertNumberOfSubEquipments(5);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, configTime.AddHours(5), out var currentTree);
			AssertEquals(2, currentTree.Count);
		}

		public void TestGenerateTreeResendSameConfig()
		{
			// Arrange
			GenerateExistingEquipment();

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1DefaultConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2DefaultConfig);
			var obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);

			// Act
			var configTime = startTime.AddDays(1);
			dbTreeGenerator.GenerateTree(deviceIdentifier, configTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(0, 4);
			AssertNumberOfSubEquipments(5);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, configTime.AddHours(5), out var currentTree);
			AssertEquals(4, currentTree.Count);
		}

		public void TestGenerateTreeConfigSentOlderThanCurrent()
		{
			// Arrange
			GenerateExistingEquipment();

			var t2Nodes = new EquipmentFlatJson();
			var t2BaseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig);
			var t2AxleGroup1 = GenerateSubEquipmentData("01", "A", axle1NewConfig);
			var t2AxleGroup2 = GenerateSubEquipmentData("02", "A", axle2NewConfig);
			var t2Obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var t2Obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2BaseEquipment.id, type = t2BaseEquipment.type }, (new TelSubEquipmentData(), t2BaseEquipment));
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2AxleGroup1.id, type = t2AxleGroup1.type }, (t2BaseEquipment, t2AxleGroup1));
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2AxleGroup2.id, type = t2AxleGroup2.type }, (t2BaseEquipment, t2AxleGroup2));
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2Obm1.id, type = t2Obm1.type }, (t2AxleGroup1, t2Obm1));
			t2Nodes.Add(new TelSubEquipmentKey() { id = t2Obm2.id, type = t2Obm2.type }, (t2AxleGroup2, t2Obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(t2Nodes);
			var t2ConfigTime = startTime.AddDays(100);
			dbTreeGenerator.GenerateTree(deviceIdentifier, t2ConfigTime, "Config");
			Factory.Save();

			// Precondition
			AssertNumberOfClosedAndOpenEdges(4, 4);
			AssertNumberOfSubEquipments(7);

			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1ThirdConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2ThirdConfig);
			var obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);
			var t1ConfigTime = t2ConfigTime.AddDays(-5);

			// Act
			dbTreeGenerator.GenerateTree(deviceIdentifier, t1ConfigTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(8, 4);
			AssertNumberOfSubEquipments(9);
			dbTreePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, t1ConfigTime.AddHours(5), out var currentTree);
			AssertEquals(4, currentTree.Count);
		}

		void SendGenericConfiguration(int open, int closed, int equipment, DateTimeOffset time)
		{
			var newNodes = new EquipmentFlatJson();
			var baseEquipment = GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig);
			var axleGroup1 = GenerateSubEquipmentData("01", "A", axle1DefaultConfig);
			var axleGroup2 = GenerateSubEquipmentData("02", "A", axle2DefaultConfig);
			var obm1 = GenerateSubEquipmentData("03", "O", obmDefaultConfig);
			var obm2 = GenerateSubEquipmentData("04", "O", obmDefaultConfig);
			newNodes.Add(new TelSubEquipmentKey() { id = baseEquipment.id, type = baseEquipment.type }, (new TelSubEquipmentData(), baseEquipment));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup1.id, type = axleGroup1.type }, (baseEquipment, axleGroup1));
			newNodes.Add(new TelSubEquipmentKey() { id = axleGroup2.id, type = axleGroup2.type }, (baseEquipment, axleGroup2));
			newNodes.Add(new TelSubEquipmentKey() { id = obm1.id, type = obm1.type }, (axleGroup1, obm1));
			newNodes.Add(new TelSubEquipmentKey() { id = obm2.id, type = obm2.type }, (axleGroup2, obm2));
			Factory.Save();
			jsonPlannerMock.Setup(jsonPlanner => jsonPlanner.FlattenJson(It.IsAny<string>(), It.IsAny<string>())).Returns(newNodes);
			var t1ConfigTime = time;

			// Act
			dbTreeGenerator.GenerateTree(deviceIdentifier, t1ConfigTime, "Config");
			Factory.Save();

			// Assert
			AssertNumberOfClosedAndOpenEdges(closed, open);
			AssertNumberOfSubEquipments(equipment);
		}

		public void TestGenerateTreeRejectsEntriesWithAnInvalidTimestamp()
		{
			// Arrange
			Expression<Action<ILogger>> action = logger => logger.Log(LogType.Information, $"Device: {deviceIdentifier}, sent configuration with invalid timestamp");
			loggerMock.Setup(action);
			var iterations = 5;

			// Act
			for (var i = 0; i < iterations; i++)
			{
				SendGenericConfiguration(4, 0, 5, startTime.AddDays(-i));
			}

			// Assert
			loggerMock.Verify(action, Times.Exactly(iterations - 1));
		}

		public void TestGenerateTreeRaisesErrorIfDbTreePlannerCannotBuildTree()
		{
			// Arrange
			var dbTreePlannerMock = new Mock<IDbTreePlanner>(MockBehavior.Strict);
			dbTreeGenerator = new TelEquipmentTreeGenerator(Factory, jsonPlannerMock.Object, dbTreePlannerMock.Object, loggerMock.Object);
			IList<TelEdgeEquipmentTreeNode> dbTree;
			dbTreePlannerMock.Setup(planner => planner.TryFlattenTree(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), out dbTree)).Returns(false);
			dbTreePlannerMock.Setup(planner => planner.CanAddEntry(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Returns(true);
			loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

			// Act
			dbTreeGenerator.GenerateTree(deviceIdentifier, startTime.AddDays(1), "blah");
			Factory.Save();

			// Assert
			AssertNoExceptionThrown(
				() => loggerMock.Verify(logger => logger.Log(LogType.Error, $"Device with hardwareId: {deviceIdentifier} has invalid database entries"))
			);
			AssertNumberOfClosedAndOpenEdges(0, 0);
			AssertNumberOfSubEquipments(0);
		}

		public void TestGenerateTreePassesFactoryToDbTreePlanner()
		{
			// Arrange
			var dbTreePlannerMock = new Mock<IDbTreePlanner>();
			dbTreeGenerator = new TelEquipmentTreeGenerator(jsonPlannerMock.Object, dbTreePlannerMock.Object, loggerMock.Object);
			IList<TelEdgeEquipmentTreeNode> dbTree;
			dbTreePlannerMock.Setup(planner => planner.TryFlattenTree(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), out dbTree)).Returns(false);
			dbTreePlannerMock.Setup(planner => planner.CanAddEntry(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Returns(true);
			loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

			dbTreeGenerator.GenerateTree(Factory, deviceIdentifier, startTime.AddDays(1), "blah");
			Factory.Save();

			// Assert
			AssertNoExceptionThrown(
				() =>
				{
					dbTreePlannerMock.Verify(planner => planner.TryFlattenTree(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), out dbTree), Times.Once);
					dbTreePlannerMock.Verify(planner => planner.TryFlattenTree(Factory, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), out dbTree), Times.Once);
				}
			);
		}

		#region Implementation

		TelSubEquipmentData GenerateSubEquipmentData(string id, string type, string configuration)
		{
			return new TelSubEquipmentData()
			{
				id = id,
				type = type,
				configuration = configuration
			};
		}

		TelSubEquipment GenerateSubEquipment(TelSubEquipmentData data)
		{
			var subEquipment = Factory.New<TelSubEquipment>();
			subEquipment.TSE_Id = data.id;
			subEquipment.TSE_Type = data.type;
			subEquipment.TSE_Configuration = data.configuration;
			return subEquipment;
		}

		TelEdge GenerateEdge(TelSubEquipment parent, TelSubEquipment child, DateTimeOffset startTime, string codeFrom, string codeTo)
		{
			var edge = Factory.New<TelEdge>();
			edge.TE_EntityIdFrom = parent.PK;
			edge.TE_EntityIdTo = child.PK;
			edge.TE_StartTime = startTime;
			edge.TE_EntityTableCodeFrom = codeFrom;
			edge.TE_EntityTableCodeTo = codeTo;
			edge.TE_RelationshipType = "HW";
			return edge;
		}

		void GenerateExistingEquipment()
		{
			var nodes = new Dictionary<TelSubEquipment, TelSubEquipment>();
			var baseEquipment = GenerateSubEquipment(GenerateSubEquipmentData(deviceIdentifier, "RQ", vehicleDefaultConfig));
			var axleGroup1 = GenerateSubEquipment(GenerateSubEquipmentData("01", "A", axle1DefaultConfig));
			var axleGroup2 = GenerateSubEquipment(GenerateSubEquipmentData("02", "A", axle2DefaultConfig));
			var obm1 = GenerateSubEquipment(GenerateSubEquipmentData("03", "O", obmDefaultConfig));
			var obm2 = GenerateSubEquipment(GenerateSubEquipmentData("04", "O", obmDefaultConfig));
			var edgeEquipmentToAxle1 = GenerateEdge(baseEquipment, axleGroup1, startTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipmentToAxle2 = GenerateEdge(baseEquipment, axleGroup2, startTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var axle1ToObm1 = GenerateEdge(axleGroup1, obm1, startTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var axle2ToObm2 = GenerateEdge(axleGroup2, obm2, startTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			Factory.Save();
		}
		#endregion

		#region Assertions

		void AssertNumberOfClosedAndOpenEdges(int numClosedEdges, int numOpenEdges)
		{
			var closedEdges = Factory.Load<TelEdge>(new ZQuery(TelEdgeSchema.TE_EndTime, SQLComparisonOperator.NotEqual, ZDateTimeOffset.Empty));
			var openEdges = Factory.Load<TelEdge>(new ZQuery(TelEdgeSchema.TE_EndTime, SQLComparisonOperator.Equal, ZDateTimeOffset.Empty));
			AssertEquals(numClosedEdges, closedEdges.Length);
			AssertEquals(numOpenEdges, openEdges.Length);
		}

		void AssertNumberOfSubEquipments(int numSubEquipment)
		{
			var subEquipment = Factory.Load<TelSubEquipment>(new ZQuery(TelSubEquipmentSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			AssertEquals(numSubEquipment, subEquipment.Length);
		}

		#endregion
	}
}
