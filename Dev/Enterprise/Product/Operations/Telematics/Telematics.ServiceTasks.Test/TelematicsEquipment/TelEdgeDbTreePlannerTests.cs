using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Moq;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment.Test
{
	public class TelEdgeDbTreePlannerTests : TestCaseWithFactory
	{
		public void TestFlattenHardwareTree()
		{
			// Arrange
			var baseEquipment = GenerateSubEquipment(deviceIdentifier, "RQ");
			var axleGroup1 = GenerateSubEquipment($"{deviceIdentifier}01", "A");
			var axleGroup2 = GenerateSubEquipment($"{deviceIdentifier}02", "A");
			var obm1 = GenerateSubEquipment("01", "O");
			var obm2 = GenerateSubEquipment("02", "O");
			var edgeEquipmentToAxle1 = GenerateEdge(baseEquipment, axleGroup1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipmentToAxle2 = GenerateEdge(baseEquipment, axleGroup2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var axle1ToObm1 = GenerateEdge(axleGroup1, obm1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var axle2ToObm2 = GenerateEdge(axleGroup2, obm2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			Factory.Save();
			var expectedNodes = new List<(ZGuid, ZGuid, ZGuid, DateTimeOffset, DateTimeOffset?)>();
			expectedNodes.Add((baseEquipment.PK, axleGroup1.PK, edgeEquipmentToAxle1.PK, baseDateTime, null));
			expectedNodes.Add((baseEquipment.PK, axleGroup2.PK, edgeEquipmentToAxle2.PK, baseDateTime, null));
			expectedNodes.Add((axleGroup1.PK, obm1.PK, axle1ToObm1.PK, baseDateTime, null));
			expectedNodes.Add((axleGroup2.PK, obm2.PK, axle2ToObm2.PK, baseDateTime, null));

			// Act
			AssertEquals(true, treePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, baseDateTime.AddHours(5), out var nodes));

			// Assert
			AssertDbEntriesAreEqual(expectedNodes, nodes);
		}

		public void TestFlattenHardwareTreeWithMultipleRootsSelectsOnlyCurrentTree()
		{
			// Arrange
			var t2 = baseDateTime.AddDays(3);
			var t3 = t2.AddDays(3);

			var baseEquipment1 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var baseEquipment2 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var axleGroup1 = GenerateSubEquipment($"{deviceIdentifier}01", "A");
			var axleGroup2 = GenerateSubEquipment($"{deviceIdentifier}02", "A");
			var obm1 = GenerateSubEquipment("01", "O");
			var obm2 = GenerateSubEquipment("02", "O");
			var edgeEquipment1ToAxle1 = GenerateEdge(baseEquipment1, axleGroup1, baseDateTime, t2, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment1ToAxle2 = GenerateEdge(baseEquipment1, axleGroup2, baseDateTime, t2, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle1 = GenerateEdge(baseEquipment2, axleGroup1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle2 = GenerateEdge(baseEquipment2, axleGroup2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var axle1ToObm1 = GenerateEdge(axleGroup1, obm1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var axle2ToObm2 = GenerateEdge(axleGroup2, obm2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			Factory.Save();
			var expectedNodes = new List<(ZGuid, ZGuid, ZGuid, DateTimeOffset, DateTimeOffset?)>();
			expectedNodes.Add((baseEquipment2.PK, axleGroup1.PK, edgeEquipment2ToAxle1.PK, baseDateTime, null));
			expectedNodes.Add((baseEquipment2.PK, axleGroup2.PK, edgeEquipment2ToAxle2.PK, baseDateTime, null));
			expectedNodes.Add((axleGroup1.PK, obm1.PK, axle1ToObm1.PK, baseDateTime, null));
			expectedNodes.Add((axleGroup2.PK, obm2.PK, axle2ToObm2.PK, baseDateTime, null));

			// Act
			AssertEquals(true, treePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, t3, out var nodes));

			// Assert
			AssertDbEntriesAreEqual(expectedNodes, nodes);
		}

		public void TestFlattenHardwareTreeWithInvalidTreeInDbReturnsFalse()
		{
			// Arrange
			var t2 = baseDateTime.AddDays(3);
			var t3 = t2.AddDays(3);

			var baseEquipment1 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var baseEquipment2 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var axleGroup1 = GenerateSubEquipment($"{deviceIdentifier}01", "A");
			var axleGroup2 = GenerateSubEquipment($"{deviceIdentifier}02", "A");
			var edgeEquipment1ToAxle1 = GenerateEdge(baseEquipment1, axleGroup1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment1ToAxle2 = GenerateEdge(baseEquipment1, axleGroup2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle1 = GenerateEdge(baseEquipment2, axleGroup1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle2 = GenerateEdge(baseEquipment2, axleGroup2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			Factory.Save();

			// Act
			// Assert
			AssertEquals(false, treePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, t3, out var nodes));
			AssertNull(nodes);
		}

		public void TestFlattenHardwareTreeWithInvalidTreeInDbClosesExcessiveOpenEdges()
		{
			// Arrange
			var t2 = baseDateTime.AddDays(3);
			var t3 = t2.AddDays(3);

			var baseEquipment1 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var baseEquipment2 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var axleGroup1 = GenerateSubEquipment($"{deviceIdentifier}01", "A");
			var axleGroup2 = GenerateSubEquipment($"{deviceIdentifier}02", "A");
			var edgeEquipment1ToAxle1 = GenerateEdge(baseEquipment1, axleGroup1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment1ToAxle2 = GenerateEdge(baseEquipment1, axleGroup2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle1 = GenerateEdge(baseEquipment2, axleGroup1, baseDateTime.AddSeconds(1), TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle2 = GenerateEdge(baseEquipment2, axleGroup2, baseDateTime.AddSeconds(1), TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			Factory.Save();

			// Act
			AssertEquals(false, treePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, t3, out var nodes));

			// Assert
			var edgeEquipment1ToAxle1FromDb = Factory.Load<TelEdge>(edgeEquipment1ToAxle1.PK);
			var edgeEquipment1ToAxle2FromDb = Factory.Load<TelEdge>(edgeEquipment1ToAxle2.PK);
			var edgeEquipment2ToAxle1FromDb = Factory.Load<TelEdge>(edgeEquipment2ToAxle1.PK);
			var edgeEquipment2ToAxle2FromDb = Factory.Load<TelEdge>(edgeEquipment2ToAxle2.PK);
			AssertEquals(edgeEquipment1ToAxle1FromDb.TE_EndTime.ToDateTimeOffset(), baseDateTime.AddSeconds(1));
			AssertEquals(edgeEquipment1ToAxle2FromDb.TE_EndTime.ToDateTimeOffset(), baseDateTime.AddSeconds(1));
			AssertEquals(edgeEquipment2ToAxle1FromDb.TE_EndTime, ZDateTimeOffset.Empty);
			AssertEquals(edgeEquipment2ToAxle2FromDb.TE_EndTime, ZDateTimeOffset.Empty);
		}

		public void TestFlattenHardwareTreeWithInvalidTreeInDbDoesNotCloseWhenOnlyOneEdgeIsOpen()
		{
			// Arrange
			var t2 = baseDateTime.AddDays(3);
			var t3 = t2.AddDays(3);

			var baseEquipment1 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var baseEquipment2 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var axleGroup1 = GenerateSubEquipment($"{deviceIdentifier}01", "A");
			var axleGroup2 = GenerateSubEquipment($"{deviceIdentifier}02", "A");
			var edgeEquipment1ToAxle1 = GenerateEdge(baseEquipment1, axleGroup1, baseDateTime, t3, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment1ToAxle2 = GenerateEdge(baseEquipment1, axleGroup2, baseDateTime, t3, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle1 = GenerateEdge(baseEquipment2, axleGroup1, baseDateTime.AddSeconds(1), TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle2 = GenerateEdge(baseEquipment2, axleGroup2, baseDateTime.AddSeconds(1), TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			Factory.Save();

			// Act
			AssertEquals(false, treePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, t2, out var nodes));

			// Assert
			var edgeEquipment1ToAxle1FromDb = Factory.Load<TelEdge>(edgeEquipment1ToAxle1.PK);
			var edgeEquipment1ToAxle2FromDb = Factory.Load<TelEdge>(edgeEquipment1ToAxle2.PK);
			var edgeEquipment2ToAxle1FromDb = Factory.Load<TelEdge>(edgeEquipment2ToAxle1.PK);
			var edgeEquipment2ToAxle2FromDb = Factory.Load<TelEdge>(edgeEquipment2ToAxle2.PK);
			AssertEquals(edgeEquipment1ToAxle1FromDb.TE_EndTime.ToDateTimeOffset(), t3);
			AssertEquals(edgeEquipment1ToAxle2FromDb.TE_EndTime.ToDateTimeOffset(), t3);
			AssertEquals(edgeEquipment2ToAxle1FromDb.TE_EndTime, ZDateTimeOffset.Empty);
			AssertEquals(edgeEquipment2ToAxle2FromDb.TE_EndTime, ZDateTimeOffset.Empty);
		}

		public void TestFlattenHardwareTreeWithManualEntryEdgesAreIgnored()
		{
			// Arrange
			var t2 = baseDateTime.AddDays(3);
			var t3 = t2.AddDays(3);

			var baseEquipment1 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var baseEquipment2 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var axleGroup1 = GenerateSubEquipment($"{deviceIdentifier}01", "A");
			var axleGroup2 = GenerateSubEquipment($"{deviceIdentifier}02", "A");
			var edgeEquipment1ToAxle1 = GenerateEdge(baseEquipment1, axleGroup1, baseDateTime, "RQ", TelEdgeEntityTableCodes.Codes.TSE, relationshipType: "");
			var edgeEquipment1ToAxle2 = GenerateEdge(baseEquipment1, axleGroup2, baseDateTime, "RQ", TelEdgeEntityTableCodes.Codes.TSE, relationshipType: "");
			var edgeEquipment2ToAxle1 = GenerateEdge(baseEquipment2, axleGroup1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle2 = GenerateEdge(baseEquipment2, axleGroup2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var expectedNodes = new List<(ZGuid, ZGuid, ZGuid, DateTimeOffset, DateTimeOffset?)>();
			expectedNodes.Add((baseEquipment2.PK, axleGroup1.PK, edgeEquipment2ToAxle1.PK, baseDateTime, null));
			expectedNodes.Add((baseEquipment2.PK, axleGroup2.PK, edgeEquipment2ToAxle2.PK, baseDateTime, null));
			Factory.Save();

			// Act
			// Assert
			AssertEquals(true, treePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, t3, out var nodes));
			AssertDbEntriesAreEqual(expectedNodes, nodes);
		}

		public void TestFlattenHardwareTreeWithMultipleRoots()
		{
			// Arrange
			var t2 = baseDateTime.AddDays(3);
			var t3 = t2.AddDays(3);

			var baseEquipment1 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var baseEquipment2 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var axleGroup1 = GenerateSubEquipment($"{deviceIdentifier}01", "A");
			var axleGroup2 = GenerateSubEquipment($"{deviceIdentifier}02", "A");
			var obm1 = GenerateSubEquipment("01", "O");
			var obm2 = GenerateSubEquipment("02", "O");
			var edgeEquipment1ToAxle1 = GenerateEdge(baseEquipment1, axleGroup1, baseDateTime, t2, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment1ToAxle2 = GenerateEdge(baseEquipment1, axleGroup2, baseDateTime, t2, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle1 = GenerateEdge(baseEquipment2, axleGroup1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var edgeEquipment2ToAxle2 = GenerateEdge(baseEquipment2, axleGroup2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var axle1ToObm1 = GenerateEdge(axleGroup1, obm1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			var axle2ToObm2 = GenerateEdge(axleGroup2, obm2, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			Factory.Save();
			var expectedNodes = new List<(ZGuid, ZGuid, ZGuid, DateTimeOffset, DateTimeOffset?)>();
			expectedNodes.Add((baseEquipment2.PK, axleGroup1.PK, edgeEquipment2ToAxle1.PK, baseDateTime, null));
			expectedNodes.Add((baseEquipment2.PK, axleGroup2.PK, edgeEquipment2ToAxle2.PK, baseDateTime, null));
			expectedNodes.Add((axleGroup1.PK, obm1.PK, axle1ToObm1.PK, baseDateTime, null));
			expectedNodes.Add((axleGroup2.PK, obm2.PK, axle2ToObm2.PK, baseDateTime, null));

			// Act
			var result = treePlanner.TryFlattenTree(deviceIdentifier, TelEdgeEntityTableCodes.Codes.TSE, t3, out var nodes);

			// Assert
			AssertEquals(true, result);
			AssertDbEntriesAreEqual(expectedNodes, nodes);
		}

		public void TestCanAddEntryTrue()
		{
			AssertCanAddEntry(true, 1);
		}

		public void TestCanAddEntryFalse()
		{
			AssertCanAddEntry(false, -1);
		}

		public void TestGetSubEquipmentFromTree()
		{
			// Arrange
			var tpmEquipment1 = Factory.New<TelSubEquipment>();
			var obmEquipment1 = Factory.New<TelSubEquipment>();
			var tpmEquipment2 = Factory.New<TelSubEquipment>();
			var obmEquipment2 = Factory.New<TelSubEquipment>();
			var startTime = new DateTimeOffset(2015, 07, 17, 11, 0, 0, TimeSpan.FromHours(10));
			SetupGenericTree(new[] { tpmEquipment1, tpmEquipment2 }, new[] { obmEquipment1, obmEquipment2 }, startTime);

			Factory.Save();

			// Act
			// Assert
			CombineAssertions(() =>
			{
				Test(
					TelSubEquipmentTypeList.Codes.Wheel,
					new[]
					{
						new SimpleSubEquipment(tpmEquipment1.PK, tpmEquipment1.TSE_Id, tpmEquipment1.TSE_Type),
						new SimpleSubEquipment(tpmEquipment2.PK, tpmEquipment2.TSE_Id, tpmEquipment2.TSE_Type),
					});
				Test(
					TelSubEquipmentTypeList.Codes.OBM,
					new[]
					{
						new SimpleSubEquipment(obmEquipment1.PK, obmEquipment1.TSE_Id, obmEquipment1.TSE_Type),
						new SimpleSubEquipment(obmEquipment2.PK, obmEquipment2.TSE_Id, obmEquipment2.TSE_Type),
					});
			});

			void Test(string subEquipmentType, IEnumerable<SimpleSubEquipment> expectedEquipment)
			{
				var result = treePlanner
					.GetSubEquipmentFromTree(Factory, deviceIdentifier, startTime.AddHours(5), subEquipmentType)
					.Values;
				AssertContainsExactElementsInAnyOrder(new SimpleSubEquipmentEqualityComparer(), expectedEquipment, result);
			}
		}

		class SimpleSubEquipmentEqualityComparer : IEqualityComparer<SimpleSubEquipment>
		{
			public bool Equals(SimpleSubEquipment x, SimpleSubEquipment y)
			{
				return x.Id == y.Id &&
					x.Pk == y.Pk &&
					x.Type == y.Type;
			}

			public int GetHashCode(SimpleSubEquipment obj)
			{
				throw new NotImplementedException();
			}
		}

		void SetupGenericTree(TelSubEquipment[] tpmEquipment, TelSubEquipment[] obmEquipment, DateTimeOffset startTime)
		{
			// Arrange
			var loggerMock = new Mock<ILogger>();
			var mobileServicesDeviceIdentifier = new byte[] { 0x01, 0x04, 0x07, 0xFF }; // No significance
			deviceIdentifier = "02050801";
			var endTime = startTime.AddDays(20);

			var glbDevice = Factory.New<GlbDevice>();
			glbDevice.V3_HumanReadableIdentifier = "TT00000001";
			glbDevice.V3_IsActive = true;
			glbDevice.V3_MobileServicesIdentifier = mobileServicesDeviceIdentifier;
			glbDevice.V3_HardwareIdentifier = deviceIdentifier;
			glbDevice.V3_Model = "GLaDOS v3.1";
			var devicePK = glbDevice.PK;

			var truck = Factory.New<RefEquipment>();
			truck.RQ_Registration = "TEST000001";
			truck.RQ_ShortCode = "TEST1";
			var truck1PK = truck.PK;

			var assignment = Factory.New<GlbDeviceAssignmentDivot>();
			assignment.V7_StartTimeUtc = new ZDateTime(startTime.UtcDateTime);
			assignment.V7_EndTimeUtc = new ZDateTime(endTime.UtcDateTime);
			assignment.V7_V3_Device = devicePK;
			assignment.V7_ParentID = truck1PK;
			assignment.V7_ParentTableCode = "RQ";

			var truckSubEquipment = Factory.New<TelSubEquipment>();
			truckSubEquipment.TSE_Type = "RQ";
			truckSubEquipment.TSE_Id = deviceIdentifier;
			truckSubEquipment.TSE_Name = "";
			truckSubEquipment.TSE_Configuration = "<EmptyXml />";

			var axleSubEquipment = Factory.New<TelSubEquipment>();
			axleSubEquipment.TSE_Type = TelSubEquipmentTypeList.Codes.Axle;
			axleSubEquipment.TSE_Id = "02";
			axleSubEquipment.TSE_Name = "";
			axleSubEquipment.TSE_Configuration = "<EmptyXml />";

			var edgeTruckToAxle = Factory.New<TelEdge>();
			edgeTruckToAxle.TE_EntityIdFrom = truckSubEquipment.PK;
			edgeTruckToAxle.TE_EntityIdTo = axleSubEquipment.PK;
			edgeTruckToAxle.TE_EntityTableCodeFrom = TelEdgeEntityTableCodes.Codes.TSE;
			edgeTruckToAxle.TE_EntityTableCodeTo = TelEdgeEntityTableCodes.Codes.TSE;
			edgeTruckToAxle.TE_RelationshipType = "HW";
			edgeTruckToAxle.TE_StartTime = startTime;

			for (var i = 0; i < obmEquipment.Length; i++)
			{
				obmEquipment[i].TSE_Type = TelSubEquipmentTypeList.Codes.OBM;
				obmEquipment[i].TSE_Id = $"0{i}";
				obmEquipment[i].TSE_Name = "";
				obmEquipment[i].TSE_Configuration = "";

				var edgeAxleToObm = Factory.New<TelEdge>();
				edgeAxleToObm.TE_EntityIdFrom = axleSubEquipment.PK;
				edgeAxleToObm.TE_EntityIdTo = obmEquipment[i].PK;
				edgeAxleToObm.TE_EntityTableCodeFrom = TelEdgeEntityTableCodes.Codes.TSE;
				edgeAxleToObm.TE_EntityTableCodeTo = TelEdgeEntityTableCodes.Codes.TSE;
				edgeAxleToObm.TE_RelationshipType = "HW";
				edgeAxleToObm.TE_StartTime = startTime;
			}

			for (var i = 0; i < tpmEquipment.Length; i++)
			{
				tpmEquipment[i].TSE_Type = TelSubEquipmentTypeList.Codes.Wheel;
				tpmEquipment[i].TSE_Id = $"0{i}020304";
				tpmEquipment[i].TSE_Name = "";
				tpmEquipment[i].TSE_Configuration = "";

				var edgeAxleToWheel = Factory.New<TelEdge>();
				edgeAxleToWheel.TE_EntityIdFrom = axleSubEquipment.PK;
				edgeAxleToWheel.TE_EntityIdTo = tpmEquipment[i].PK;
				edgeAxleToWheel.TE_EntityTableCodeFrom = TelEdgeEntityTableCodes.Codes.TSE;
				edgeAxleToWheel.TE_EntityTableCodeTo = TelEdgeEntityTableCodes.Codes.TSE;
				edgeAxleToWheel.TE_RelationshipType = "HW";
				edgeAxleToWheel.TE_StartTime = startTime;
			}
		}

		public void TestCanAddEntryFirstEntry()
		{
			AssertEquals(true, treePlanner.CanAddEntry("01020303030201", baseDateTime));
		}

		void AssertCanAddEntry(bool expected, int timeOffset)
		{
			// Arrange
			var baseEquipment1 = GenerateSubEquipment(deviceIdentifier, "RQ");
			var axleGroup1 = GenerateSubEquipment($"{deviceIdentifier}01", "A");
			var edgeEquipment1ToAxle1 = GenerateEdge(baseEquipment1, axleGroup1, baseDateTime, TelEdgeEntityTableCodes.Codes.TSE, TelEdgeEntityTableCodes.Codes.TSE);
			Factory.Save();

			// Act
			// Assert
			AssertEquals(expected, treePlanner.CanAddEntry(deviceIdentifier, baseDateTime.AddDays(timeOffset)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			deviceIdentifier = "0102030405060708090A0B0C";
			baseDateTime = DateTimeOffset.UtcNow.AddDays(-100);
			treePlanner = new TelEdgeDbTreePlanner(Factory);
		}

		string deviceIdentifier;
		DateTimeOffset baseDateTime;
		TelEdgeDbTreePlanner treePlanner;

		#region Assertions

		public void AssertDbEntriesAreEqual(IList<(ZGuid parentPk, ZGuid childPk, ZGuid TE_PK, DateTimeOffset startTime, DateTimeOffset? endTime)> expectedEntries, IList<TelEdgeEquipmentTreeNode> databaseResponse)
		{
			var orderedExpectations = expectedEntries.OrderBy(entry => entry.TE_PK);
			var orderedResponse = databaseResponse.OrderBy(entry => entry.TET_TelEdgePK);
			for (var i = 0; i < expectedEntries.Count; i++)
			{
				AssertEquals(orderedExpectations.ElementAt(i).TE_PK, orderedResponse.ElementAt(i).TET_TelEdgePK);
				AssertEquals(orderedExpectations.ElementAt(i).parentPk, orderedResponse.ElementAt(i).TET_ParentPK);
				AssertEquals(orderedExpectations.ElementAt(i).childPk, orderedResponse.ElementAt(i).TET_ChildPK);
				AssertEquals(orderedExpectations.ElementAt(i).startTime.ToString(), orderedResponse.ElementAt(i).TET_StartTime.ToString());
				if (orderedExpectations.ElementAt(i).endTime == null)
				{
					AssertNull(orderedResponse.ElementAt(i).TET_EndTime);
				}
				else
				{
					AssertEquals(orderedExpectations.ElementAt(i).endTime.ToString(), orderedResponse.ElementAt(i).TET_EndTime.ToString());
				}
			}
		}
		#endregion

		#region Implementation

		TelSubEquipment GenerateSubEquipment(string id, string type, string configuration)
		{
			var subEquipment = Factory.New<TelSubEquipment>();
			subEquipment.TSE_Id = id;
			subEquipment.TSE_Type = type;
			subEquipment.TSE_Configuration = configuration;
			return subEquipment;
		}

		TelSubEquipment GenerateSubEquipment(string id, string type)
		{
			var config = type == "W" || type == "O" ? "" : "<EmptyXml />";
			return GenerateSubEquipment(id, type, config);
		}

		TelEdge GenerateEdge(TelSubEquipment parent, TelSubEquipment child, DateTimeOffset startTime, string codeFrom, string codeTo, string relationshipType = "HW")
		{
			var edge = Factory.New<TelEdge>();
			edge.TE_EntityIdFrom = parent.PK;
			edge.TE_EntityIdTo = child.PK;
			edge.TE_StartTime = startTime;
			edge.TE_EntityTableCodeFrom = codeFrom;
			edge.TE_EntityTableCodeTo = codeTo;
			edge.TE_RelationshipType = relationshipType;
			return edge;
		}

		TelEdge GenerateEdge(TelSubEquipment parent, TelSubEquipment child, DateTimeOffset startTime, DateTimeOffset endTime, string codeFrom, string codeTo)
		{
			var edge = GenerateEdge(parent, child, startTime, codeFrom, codeTo);
			edge.TE_EndTime = endTime;
			return edge;
		}
		#endregion
	}
}
