using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Moq;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class TaskManagementLocationFactTest : TestCaseWithFactory
	{
		#region TestConstructor_Throws

		public void TestConstructor_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLocationFact(null));
		}

		#endregion

		#region TestProperties

		public void TestProperties() => TestProperties(locationClass: LocationClasses.Codes.NOR);
		public void TestProperties_FixedPickFace() => TestProperties(locationClass: LocationClasses.Codes.FIX);
		public void TestProperties_FixedPickFace_CaseInsensitive() => TestProperties(locationClass: "fIx");
		public void TestProperties_DynamicPickFace() => TestProperties(locationClass: LocationClasses.Codes.DPF);
		public void TestProperties_DynamicPickFace_CaseInsensitive() => TestProperties(locationClass: "dPf");

		void TestProperties(string locationClass)
		{
			// Put unique values for all values that support enough values, test combos for booleans which can only have two values
			var pk = Guid.NewGuid();

			var locationTypeCode = "TYP";
			var areaName = "PUT";
			var rowName = "A";

			var column = 1;
			var level = 2;
			var tray = 3;

			var pickPath = 42;
			var putawayPath = 73;
			var rowPathSequence = (short)37;

			var locationStatus = "AVL";

			var locationMock = new Mock<IWhsLocation>(MockBehavior.Strict);
			locationMock.Setup(l => l.PK).Returns(pk);
			locationMock.Setup(l => l.WLV_LocationTypeCode).Returns(locationTypeCode);
			locationMock.Setup(l => l.WLV_LocationClass).Returns(locationClass);
			locationMock.Setup(l => l.PickingAreaName).Returns(areaName);
			locationMock.Setup(l => l.WLV_RowName).Returns(rowName);
			locationMock.Setup(l => l.RowPathSequence).Returns(rowPathSequence);
			locationMock.Setup(l => l.WLV_LocationStatus).Returns(locationStatus);
			locationMock.Setup(l => l.WLV_Column).Returns((short)column);
			locationMock.Setup(l => l.WLV_Level).Returns((short)level);
			locationMock.Setup(l => l.WLV_Tray).Returns((short)tray);
			locationMock.Setup(l => l.WLV_PickPathSequence).Returns(pickPath);
			locationMock.Setup(l => l.WLV_PutawayPathSequence).Returns(putawayPath);
			locationMock.Setup(l => l.WLV_LocationStatus).Returns(locationStatus);

			var putawayLocationFact = new TaskManagementLocationFact(locationMock.Object);

			AssertEquals(nameof(TaskManagementLocationFact.PK), pk, putawayLocationFact.PK);
			AssertEquals(nameof(TaskManagementLocationFact.LocationTypeCode), locationTypeCode, putawayLocationFact.LocationTypeCode);
			AssertEquals(nameof(TaskManagementLocationFact.LocationClass), locationClass, putawayLocationFact.LocationClass);
			AssertEquals(nameof(TaskManagementLocationFact.AreaName), areaName, putawayLocationFact.AreaName);
			AssertEquals(nameof(TaskManagementLocationFact.RowName), rowName, putawayLocationFact.RowName);
			AssertEquals(nameof(TaskManagementLocationFact.RowPathSequence), rowPathSequence, putawayLocationFact.RowPathSequence);
			AssertEquals(nameof(TaskManagementLocationFact.Column), column, putawayLocationFact.Column);
			AssertEquals(nameof(TaskManagementLocationFact.Level), level, putawayLocationFact.Level);
			AssertEquals(nameof(TaskManagementLocationFact.Tray), tray, putawayLocationFact.Tray);
			AssertEquals(nameof(TaskManagementLocationFact.LocationStatus), locationStatus, putawayLocationFact.LocationStatus);
			AssertEquals(nameof(TaskManagementLocationFact.PickPathSequence), pickPath, putawayLocationFact.PickPathSequence);
			AssertEquals(nameof(TaskManagementLocationFact.PutawayPathSequence), putawayPath, putawayLocationFact.PutawayPathSequence);
			AssertEquals(nameof(TaskManagementLocationFact.IsFixedPickFace), locationClass.Equals(LocationClasses.Codes.FIX, StringComparison.InvariantCultureIgnoreCase), putawayLocationFact.IsFixedPickFace);
			AssertEquals(nameof(TaskManagementLocationFact.IsDynamicPickFace), locationClass.Equals(LocationClasses.Codes.DPF, StringComparison.InvariantCultureIgnoreCase), putawayLocationFact.IsDynamicPickFace);

			locationMock.VerifyAll();
			locationMock.VerifyNoOtherCalls();
		}

		#endregion
	}
}
