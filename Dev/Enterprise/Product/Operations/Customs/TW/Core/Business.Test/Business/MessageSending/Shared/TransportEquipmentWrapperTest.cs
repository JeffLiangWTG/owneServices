using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TransportEquipmentWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCharacteristicCode()
		{
			var refContainer = cusContainer.Container;
			var twMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			twMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			twMap.RCM_RC_Container = refContainer.PK;
			twMap.RCM_Code = "TW1";
			refContainer.CodeMapCollection.Load();
			NUnit.Framework.Assert.That(TransportEquipment.CharacteristicCode, NUnit.Framework.Is.EqualTo("TW1").Using(CustomComparers.TypeComparison));
			twMap.Delete();
			refContainer.CodeMapCollection.Load();
			NUnit.Framework.Assert.That(TransportEquipment.CharacteristicCode, NUnit.Framework.Is.EqualTo("40GP").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(TransportEquipment.ID, NUnit.Framework.Is.EqualTo("UUUU1234567").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUsedCapacityCode()
		{
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;
			NUnit.Framework.Assert.That(TransportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = true;
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			NUnit.Framework.Assert.That(TransportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = false;
			NUnit.Framework.Assert.That(TransportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Groupage;
			NUnit.Framework.Assert.That(TransportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			NUnit.Framework.Assert.That(TransportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = true;
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BuyersConsol;
			NUnit.Framework.Assert.That(TransportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("6").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = false;
			NUnit.Framework.Assert.That(TransportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));
		}

		public void Seals()
		{
			cusContainer.CO_Seal = "XXX1";
			var seals = TransportEquipment.Seals;
			NUnit.Framework.Assert.That(seals.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(seals.Single(), NUnit.Framework.Is.EqualTo("XXX1").Using(CustomComparers.TypeComparison));
			cusContainer.CO_SecondSeal = "XXX2";
			seals = TransportEquipment.Seals;
			NUnit.Framework.Assert.That(seals.Any(x => x.Equals("XXX1")), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(seals.Any(x => x.Equals("XXX2")), NUnit.Framework.Is.True);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusContainer = Factory.NewWithValidTestData<CusContainer>();
			cusContainer.CO_ContainerNumber = "UUUU1234567";
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			cusContainer.CO_RC = refContainer.PK;
		}

		CusContainer cusContainer;
		ITransportEquipment TransportEquipment => new TransportEquipmentWrapper(cusContainer);
	}
}
