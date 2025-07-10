using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ConsignmentTransportEquipmentsTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCharacteristicCode()
		{
			NUnit.Framework.Assert.That(transportEquipment.CharacteristicCode, NUnit.Framework.Is.EqualTo("40GP").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(transportEquipment.ID, NUnit.Framework.Is.EqualTo("UUUU1234567").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUsedCapacityCode()
		{
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer.CO_IsPart = false;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = true;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison));
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Groupage;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BuyersConsol;
			cusContainer.CO_IsPart = false;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = true;
			NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("6").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSeals()
		{
			cusContainer.CO_Seal = "S001";
			cusContainer.CO_SecondSeal = "S002";
			var seals = transportEquipment.Seals;
			NUnit.Framework.Assert.That(seals.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(seals.Any(x => x.Equals(cusContainer.CO_Seal)), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(seals.Any(x => x.Equals(cusContainer.CO_SecondSeal)), NUnit.Framework.Is.True);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.JE_HouseBill = "HH111111";
			var container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "UUUU1234567";
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			cusContainer = container;
			transportEquipment = new Consignment(entryHeader).TransportEquipments.Single(x => x.ID == cusContainer.CO_ContainerNumber);
		}

		CusEntryHeader entryHeader;
		JobDeclaration Declaration => entryHeader.Declaration;
		ITransportEquipment transportEquipment;
		CusContainer cusContainer;
	}
}
