using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Moq;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class AllocationLocationFactTest : TestCaseWithFactory
	{
		#region TestConstructor_Throws

		public void TestConstructor_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AllocationLocationFact(null));
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
			locationMock.Setup(l => l.WLV_PickingAreaType).Returns(AreaTypes.Codes.FreeStore);
			locationMock.Setup(l => l.WLV_LocationStatus).Returns(locationStatus);

			var putawayLocationFact = new AllocationLocationFact(locationMock.Object);

			AssertEquals(nameof(AllocationLocationFact.PK), pk, putawayLocationFact.PK);
			AssertEquals(nameof(AllocationLocationFact.LocationTypeCode), locationTypeCode, putawayLocationFact.LocationTypeCode);
			AssertEquals(nameof(AllocationLocationFact.LocationClass), locationClass, putawayLocationFact.LocationClass);
			AssertEquals(nameof(AllocationLocationFact.AreaName), areaName, putawayLocationFact.AreaName);
			AssertEquals(nameof(AllocationLocationFact.RowName), rowName, putawayLocationFact.RowName);
			AssertEquals(nameof(AllocationLocationFact.RowPathSequence), rowPathSequence, putawayLocationFact.RowPathSequence);
			AssertEquals(nameof(AllocationLocationFact.Column), column, putawayLocationFact.Column);
			AssertEquals(nameof(AllocationLocationFact.Level), level, putawayLocationFact.Level);
			AssertEquals(nameof(AllocationLocationFact.Tray), tray, putawayLocationFact.Tray);
			AssertEquals(nameof(AllocationLocationFact.LocationStatus), locationStatus, putawayLocationFact.LocationStatus);
			AssertEquals(nameof(AllocationLocationFact.PickPathSequence), pickPath, putawayLocationFact.PickPathSequence);
			AssertEquals(nameof(AllocationLocationFact.IsAllocatedOnThisPick), false, putawayLocationFact.IsAllocatedOnThisPick);
			AssertEquals(nameof(AllocationLocationFact.IsFixedPickFace), locationClass.Equals(LocationClasses.Codes.FIX, StringComparison.InvariantCultureIgnoreCase), putawayLocationFact.IsFixedPickFace);
			AssertEquals(nameof(AllocationLocationFact.IsDynamicPickFace), locationClass.Equals(LocationClasses.Codes.DPF, StringComparison.InvariantCultureIgnoreCase), putawayLocationFact.IsDynamicPickFace);

			putawayLocationFact.IsAllocatedOnThisPick = true;
			AssertEquals(nameof(AllocationLocationFact.IsAllocatedOnThisPick), true, putawayLocationFact.IsAllocatedOnThisPick);

			locationMock.VerifyAll();
			locationMock.VerifyNoOtherCalls();
		}

		#endregion

		#region TestIsBonded

		public void TestIsBonded() => TestIsBonded(AreaTypes.Codes.Bonded, expectedResult: true);
		public void TestIsBonded_CaseInsensitive() => TestIsBonded("bOn", expectedResult: true);
		public void TestIsBonded_FreeStore() => TestIsBonded(AreaTypes.Codes.FreeStore, expectedResult: false);
		public void TestIsBonded_Excise() => TestIsBonded(AreaTypes.Codes.Excise, expectedResult: false);
		public void TestIsBonded_InwardsProcessing() => TestIsBonded(AreaTypes.Codes.InwardProcessing, expectedResult: false);

		void TestIsBonded(string areaType, bool expectedResult)
		{
			var locationMock1 = new Mock<IWhsLocation>();
			locationMock1.Setup(l => l.PK).Returns(ZGuid.NewZGuid());
			locationMock1.Setup(l => l.WLV_PickingAreaType).Returns(areaType);

			var putawayLocationFact1 = new AllocationLocationFact(locationMock1.Object);
			AssertEquals(nameof(AllocationLocationFact.IsBonded), expectedResult, putawayLocationFact1.IsBonded);
		}

		#endregion

		#region TestIsExcise

		public void TestIsExcise() => TestIsExcise(AreaTypes.Codes.Excise, expectedResult: true);
		public void TestIsExcise_CaseInsensitive() => TestIsExcise("eXc", expectedResult: true);
		public void TestIsExcise_FreeStore() => TestIsExcise(AreaTypes.Codes.FreeStore, expectedResult: false);
		public void TestIsExcise_Bonded() => TestIsExcise(AreaTypes.Codes.Bonded, expectedResult: false);
		public void TestIsExcise_InwardsProcessing() => TestIsExcise(AreaTypes.Codes.InwardProcessing, expectedResult: false);

		void TestIsExcise(string areaType, bool expectedResult)
		{
			var locationMock1 = new Mock<IWhsLocation>();
			locationMock1.Setup(l => l.PK).Returns(ZGuid.NewZGuid());
			locationMock1.Setup(l => l.WLV_PickingAreaType).Returns(areaType);

			var putawayLocationFact1 = new AllocationLocationFact(locationMock1.Object);
			AssertEquals(nameof(AllocationLocationFact.IsExcise), expectedResult, putawayLocationFact1.IsExcise);
		}

		#endregion

		#region TestIsInwardsProcessing

		public void TestIsInwardsProcessing() => TestIsInwardsProcessing(AreaTypes.Codes.InwardProcessing, expectedResult: true);
		public void TestIsInwardsProcessing_CaseInsensitive() => TestIsInwardsProcessing("iPr", expectedResult: true);
		public void TestIsInwardsProcessing_FreeStore() => TestIsInwardsProcessing(AreaTypes.Codes.FreeStore, expectedResult: false);
		public void TestIsInwardsProcessing_Bonded() => TestIsInwardsProcessing(AreaTypes.Codes.Bonded, expectedResult: false);
		public void TestIsInwardsProcessing_Excise() => TestIsInwardsProcessing(AreaTypes.Codes.Excise, expectedResult: false);

		void TestIsInwardsProcessing(string areaType, bool expectedResult)
		{
			var locationMock1 = new Mock<IWhsLocation>();
			locationMock1.Setup(l => l.PK).Returns(ZGuid.NewZGuid());
			locationMock1.Setup(l => l.WLV_PickingAreaType).Returns(areaType);

			var putawayLocationFact1 = new AllocationLocationFact(locationMock1.Object);
			AssertEquals(nameof(AllocationLocationFact.IsInwardsProcessing), expectedResult, putawayLocationFact1.IsInwardsProcessing);
		}

		#endregion
	}
}
