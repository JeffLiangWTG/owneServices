using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using WTG.NUnit;
using UHelper = Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper;

namespace Enterprise.Customs.Universal.Testing
{
	[CountrySpecificTest(Core.Constants.CountryCodes.Australia)]
	class TariffViewTriggersTest : TestCaseWithFactory
	{
		void AssertTariffViewBo(string message, TariffView testItem, bool isSystem, ZGuid tariffType, ZString tariffTypeNK, ZString dataSet, ZDateTime startTime, ZDateTime endTime, string tariffCode, string desc = "", string taxOrFeeCode = "", string country = Core.Constants.CountryCodes.China)
		{
			CombineAssertions(message, () =>
			{
				AssertNotNull("Should not be null.", testItem);
				AssertEquals("ZZ1_IsSystem", isSystem, testItem.ZZ1_IsSystem);
				AssertEquals("ZZ1_ZZI_TariffType", tariffType, testItem.ZZ1_ZZI_TariffType);
				AssertEquals("ZZ1_ZZI_NKTariffType", tariffTypeNK, testItem.ZZ1_ZZI_NKTariffType);
				AssertEquals("ZZ1_DataSet", dataSet, testItem.ZZ1_DataSet);
				AssertEquals("ZZ1_TariffCode", tariffCode, testItem.ZZ1_TariffCode);
				AssertEquals("ZZ1_Description", desc, testItem.ZZ1_Description);
				AssertEquals("ZZ1_StartDate", startTime, testItem.ZZ1_StartDate);
				AssertEquals("ZZ1_EndDate", endTime, testItem.ZZ1_EndDate);
				AssertEquals("ZZ1_ZZF_NKTaxOrFeeCode", taxOrFeeCode, testItem.ZZ1_ZZF_NKTaxOrFeeCode);
				AssertEquals("ZZ1_ZZZ_NKDataGrouping", country, testItem.ZZ1_ZZZ_NKDataGrouping);
			});
		}

		public void TestLoad()
		{
			var helper = new UHelper(Factory);
			var cnRefTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, tariffTypePK, "1000000010", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), description: "Test CN Tariff Ref", isSystem: true);
			var cnCusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, tariffTypePK, "1000000011", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), description: "Test CN Tariff Cus", isSystem: false);
			var newFactory = new BusinessObjectFactory();
			var insertedTariffs = newFactory.Load<TariffView>(new ZQuery());
			AssertEquals("Should be able to load 2 items from dbo.TariffView", 2, insertedTariffs.Length);
			AssertTariffViewBo("CN Ref", newFactory.Load<TariffView>(cnRefTariff.PK), true, tariffTypePK, "TTX", "Z", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), "1000000010", "Test CN Tariff Ref", country: Core.Constants.CountryCodes.China);
			AssertTariffViewBo("CN Cus", newFactory.Load<TariffView>(cnCusTariff.PK), false, ZGuid.Empty, "TTX", "O", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), "1000000011", "Test CN Tariff Cus", country: Core.Constants.CountryCodes.China);
		}

		public void TestInsert()
		{
			var newFactory = new BusinessObjectFactory();
			var newTariffViewBo = Factory.New<TariffView>();
			newTariffViewBo.ZZ1_ZZI_NKTariffType = tariffType;
			newTariffViewBo.ZZ1_TariffCode = "1000000010";
			newTariffViewBo.ZZ1_Description = "Test Tariff";
			newTariffViewBo.ZZ1_StartDate = new ZDateTime(1900, 1, 1);
			newTariffViewBo.ZZ1_EndDate = new ZDateTime(2079, 6, 6);
			newTariffViewBo.ZZ1_ZZF_NKTaxOrFeeCode = "";
			newTariffViewBo.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.China;
			Factory.Save();
			AssertTariffViewBo("Values for new TariffView BO.", newFactory.Load<TariffView>(newTariffViewBo.PK), false, ZGuid.Empty, "TTX", "O", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), "1000000010", "Test Tariff");

			var newSysBo = Factory.New<TariffView>();
			newSysBo.ZZ1_ZZI_NKTariffType = tariffType;
			newSysBo.ZZ1_TariffCode = "1000000010";
			newSysBo.ZZ1_Description = "Test Tariff";
			newSysBo.ZZ1_StartDate = new ZDateTime(1900, 1, 1);
			newSysBo.ZZ1_EndDate = new ZDateTime(2079, 6, 6);
			newSysBo.ZZ1_ZZF_NKTaxOrFeeCode = "";
			newSysBo.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.China;
			newSysBo.ZZ1_DataSet = "Z";
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException)));
		}

		public void TestUpdate()
		{
			var helper = new UHelper(Factory);
			var newTariffType = helper.CreateNewOrGetExistingTariffType("US", "TTY");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, tariffTypePK, "1000000011", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), description: "Test CN Tariff Cus", isSystem: false);
			Factory.Save();
			cusTariff.ZZ1_ZZI_NKTariffType = newTariffType.ZZI_TariffType;
			cusTariff.ZZ1_TariffCode = "1000000011";
			cusTariff.ZZ1_Description = "Test Tariff Mod";
			cusTariff.ZZ1_StartDate = new ZDateTime(1900, 1, 2);
			cusTariff.ZZ1_EndDate = new ZDateTime(2079, 6, 5);
			cusTariff.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var updatedTariff = newFactory.Load<TariffView>(cusTariff.PK);
			AssertTariffViewBo("Updated TariffView", updatedTariff, false, ZGuid.Empty, "TTY", "O", new ZDateTime(1900, 1, 2), new ZDateTime(2079, 6, 5), "1000000011", desc: "Test Tariff Mod", country: "US");

			var refTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, tariffTypePK, "1000000012", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), description: "Ref Tariff", isSystem: true);
			Factory.Save();
			refTariff.ZZ1_TariffCode = "10000013";
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException)));
		}

		public void TestDelete()
		{
			Factory.Save();
			var helper = new UHelper(Factory);
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, tariffTypePK, "1000000011", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), description: "Test CN Tariff Cus", isSystem: false);
			Factory.Save();
			Factory.Load<TariffView>(cusTariff.PK).Delete();
			AssertNoExceptionThrown(() => Factory.Save());

			var newFactory = new BusinessObjectFactory();
			AssertNull("Should have been deleted from dbo.CusRefTariff", newFactory.Load<TariffView>(cusTariff.PK));

			var refTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, tariffTypePK, "1000000012", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), description: "Ref Tariff", isSystem: true);
			Factory.Save();
			Factory.Load<TariffView>(refTariff.PK).Delete();
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException)));
		}

		ZGuid tariffTypePK;
		ZString tariffType;
		protected override void SetUp()
		{
			base.SetUp();
			tariffType = "TTX";
			var helper = new UHelper(Factory);
			tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "TTX").PK;
		}
	}
}
