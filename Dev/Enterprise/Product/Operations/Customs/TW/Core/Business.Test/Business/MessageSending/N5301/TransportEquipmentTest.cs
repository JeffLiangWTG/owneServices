using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TransportEquipmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCharacteristicCode()
		{
			NUnit.Framework.Assert.That(transportEquipment.CharacteristicCode, NUnit.Framework.Is.EqualTo("40GP").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(transportEquipment.ID, NUnit.Framework.Is.EqualTo("C987456123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUsedCapacityCode()
		{
			container.BC_Mode = CusInBondContainerModeList.Codes.EmptyContainer;
			container.BC_IsPart = true;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
			container.BC_Mode = CusInBondContainerModeList.Codes.FCL;
			container.BC_IsPart = true;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison));
			container.BC_IsPart = false;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			container.BC_Mode = CusInBondContainerModeList.Codes.BCN;
			container.BC_IsPart = false;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));
			container.BC_IsPart = true;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("6").Using(CustomComparers.TypeComparison));
			container.BC_Mode = CusInBondContainerModeList.Codes.LCL;
			container.BC_IsPart = true;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			container.BC_Mode = CusInBondContainerModeList.Codes.GRP;
			container.BC_IsPart = true;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSeals()
		{
			NUnit.Framework.Assert.That(transportEquipment.Seals, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZString>)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var arrivalBill = header.ArrivalBill;
			var movementBill = header.MovementBill;
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40GP";
			container = moveDetail.Containers.AddNew();
			container.BC_RC = refContainer.PK;
			container.BC_ContainerNum = "C987456123";
			transportEquipment = new TransportEquipment(container);
		}

		CusInBondContainer container;
		ITransportEquipment transportEquipment;
	}
}
