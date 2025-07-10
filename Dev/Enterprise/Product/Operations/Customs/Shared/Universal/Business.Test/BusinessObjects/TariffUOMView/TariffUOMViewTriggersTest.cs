using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using WTG.NUnit;
using UHelper = Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper;

namespace Enterprise.Customs.Universal.Testing
{
	[CountrySpecificTest(Core.Constants.CountryCodes.Australia)]
	class TariffUOMViewTriggersTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			var uHelper = new UHelper(Factory);
			var cusUom = uHelper.CreateTariffUOM(cusTariff, "CU1", "KG", Core.Constants.CountryCodes.China);
			var refUom = uHelper.CreateTariffUOM(refTariff, "CU2", "KG", Core.Constants.CountryCodes.China);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertTariffUomView("Cus DB", newFactory.Load<TariffUOMView>(cusUom.PK), false, "CU1", "KG", cusTariff.PK);
			AssertTariffUomView("Ref DB", newFactory.Load<TariffUOMView>(refUom.PK), true, "CU2", "KG", refTariff.PK);
		}

		public void TestInsert()
		{
			var newFactory = new BusinessObjectFactory();
			var newCusUom = Factory.New<TariffUOMView>();
			newCusUom.ZZ8_ZZ1_ParentTariffOrNationalCode = cusTariff.PK;
			newCusUom.ZZ8_Type = "CU1";
			newCusUom.ZZ8_UOM = "KG";
			Factory.Save();
			AssertTariffUomView("Inserted TariffUOM", newFactory.Load<TariffUOMView>(newCusUom.PK), false, "CU1", "KG", cusTariff.PK);

			var newSysUom = Factory.New<TariffUOMView>();
			newSysUom.ZZ8_ZZ1_ParentTariffOrNationalCode = cusTariff.PK;
			newSysUom.ZZ8_Type = "CU1";
			newSysUom.ZZ8_UOM = "KG";
			newSysUom.ZZ8_DataSet = "Z";
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException)));
		}

		public void TestUpdate()
		{
			var uHelper = new UHelper(Factory);
			var cusUom = uHelper.CreateTariffUOM(cusTariff, "CU1", "KG", Core.Constants.CountryCodes.China);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cusUomLoaded = newFactory.Load<TariffUOMView>(cusUom.PK);
			cusUomLoaded.ZZ8_Type = "CU2";
			cusUomLoaded.ZZ8_UOM = "LTR";
			newFactory.Save();
			AssertTariffUomView("Updated TariffUOM", newFactory.Load<TariffUOMView>(cusUom.PK), false, "CU2", "LTR", cusTariff.PK);

			var refUom = uHelper.CreateTariffUOM(refTariff, "CU1", "KG");
			var refUomLoaded = newFactory.Load<TariffUOMView>(refUom.PK);
			refUomLoaded.ZZ8_Type = "CU2";
			refUomLoaded.ZZ8_UOM = "LTR";
			NUnit.Framework.Assert.That(() => newFactory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException)));
		}

		public void TestDelete()
		{
			var uHelper = new UHelper(Factory);
			var newFactory = new BusinessObjectFactory();
			var cusUom = uHelper.CreateTariffUOM(cusTariff, "CU1", "KG", Core.Constants.CountryCodes.China);
			Factory.Save();
			newFactory.Load<TariffUOMView>(cusUom.PK).Delete();
			newFactory.Save();
			AssertNull("Should have deleted and unable to load.", newFactory.Load<TariffUOMView>(cusUom.PK));

			var refUom = uHelper.CreateTariffUOM(refTariff, "CU1", "KG", Core.Constants.CountryCodes.China);
			Factory.Load<TariffUOMView>(refUom.PK).Delete();
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var uHelper = new UHelper(Factory);
			var tariffType = uHelper.CreateTariffType(Core.Constants.CountryCodes.China, "TTX");
			Factory.Save();
			cusTariff = uHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, tariffType.PK, "10000010", new ZDateTime(2019, 1, 1), new ZDateTime(2079, 1, 1), "Test Tariff 1", isSystem: false);
			refTariff = uHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, tariffType.PK, "10000011", new ZDateTime(2019, 1, 1), new ZDateTime(2079, 1, 1), "Test Tariff 2");
			Factory.Save();
		}

		TariffView cusTariff;
		TariffView refTariff;
		void AssertTariffUomView(string message, TariffUOMView testItem, bool isSystem, string type, string uom, ZGuid tariff)
		{
			CombineAssertions(message, () =>
			{
				AssertNotNull("NotNull", testItem);
				AssertEquals("ZZ8_IsSystem", isSystem, testItem.ZZ8_IsSystem);
				AssertEquals("ZZ8_Type", type, testItem.ZZ8_Type);
				AssertEquals("ZZ8_UOM", uom, testItem.ZZ8_UOM);
				AssertEquals("ZZ8_ZZ1_ParentTariffOrNationalCode", tariff, testItem.ZZ8_ZZ1_ParentTariffOrNationalCode);
			}

			);
		}
	}
}
