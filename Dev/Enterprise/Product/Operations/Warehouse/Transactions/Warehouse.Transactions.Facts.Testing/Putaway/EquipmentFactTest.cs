using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	public class EquipmentFactTest : TestCase
	{
		public void TestNullObject_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new EquipmentFact(null, new Mock<IRefContainer>().Object));
		}

		public void TestPK()
		{
			var equipmentMock = new Mock<IRefEquipment>();
			equipmentMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var equipmentFact = new EquipmentFact(equipmentMock.Object, null);
			AssertEquals(nameof(IEquipmentFact.PK), ZGuid.BrettsGuid, equipmentFact.PK);
			AssertEquals(nameof(IEquipmentFact.PK), ZGuid.BrettsGuid, ((IEquipmentFact)equipmentFact).PK);
			equipmentMock.VerifyGet(e => e.PK);
		}

		public void TestTypeCode()
		{
			var equipmentMock = new Mock<IRefEquipment>();
			var equipmentTypeMock = new Mock<IRefContainer>();
			equipmentMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			equipmentTypeMock.SetupGet(et => et.RC_Code).Returns("ABC");

			var equipmentFact = new EquipmentFact(equipmentMock.Object, equipmentTypeMock.Object);
			AssertEquals(nameof(IEquipmentFact.TypeCode), "ABC", equipmentFact.TypeCode);
			AssertEquals(nameof(IEquipmentFact.TypeCode), "ABC", ((IEquipmentFact)equipmentFact).TypeCode);
			equipmentTypeMock.VerifyGet(et => et.RC_Code);
		}

		public void TestTypeCode_NullEquipmentType()
		{
			var equipmentMock = new Mock<IRefEquipment>();
			equipmentMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var equipmentFact = new EquipmentFact(equipmentMock.Object, null);
			AssertEquals(nameof(IEquipmentFact.TypeCode), "", equipmentFact.TypeCode);
			AssertEquals(nameof(IEquipmentFact.TypeCode), "", ((IEquipmentFact)equipmentFact).TypeCode);
		}

		public void TestGroupCode()
		{
			var equipmentMock = new Mock<IRefEquipment>();
			equipmentMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			equipmentMock.SetupGet(e => e.RQ_EquipmentGroup).Returns("XYZ");

			var equipmentFact = new EquipmentFact(equipmentMock.Object, null);
			AssertEquals(nameof(IEquipmentFact.GroupCode), "XYZ", equipmentFact.GroupCode);
			AssertEquals(nameof(IEquipmentFact.GroupCode), "XYZ", ((IEquipmentFact)equipmentFact).GroupCode);
			equipmentMock.VerifyGet(e => e.RQ_EquipmentGroup);
		}
	}
}
