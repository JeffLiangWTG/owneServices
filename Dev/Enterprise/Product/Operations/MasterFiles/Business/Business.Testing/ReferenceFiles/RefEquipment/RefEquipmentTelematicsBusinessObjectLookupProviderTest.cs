using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefEquipmentTelematicsBusinessObjectLookupProviderTest : TestCaseWithFactory
	{
		public void TestCode()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, primaryKeyOfEquipment);
			AssertEquals("BUS", telematicsBizo.Code);
		}

		public void TestDescriptionForInterfaceUsesCurrentLanguage()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, primaryKeyOfEquipment);
			AssertEquals("I'm a foreign bus.", telematicsBizo.DescriptionForInterface);
		}

		public void TestDescriptionInEnglish()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, primaryKeyOfEquipment);
			AssertEquals("I'm a bus.", telematicsBizo.DescriptionInEnglish);
		}

		public void TestTypeIdentifier()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, primaryKeyOfEquipment);
			AssertEquals("Equipment", telematicsBizo.TypeIdentifier);
		}

		public void TestAssignmentOfNonexistentEquipment()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, ZGuid.Missing);
			AssertEquals(null, telematicsBizo);
		}

		ITelematicsBusinessObjectLookupProvider telematicsProvider;
		ZGuid primaryKeyOfEquipment;
		IDisposable changedLanguageDisposable;
		IDisposable mockResStringDataDisposable;

		protected override void SetUp()
		{
			const string LanguageForTest = Core.SharedConstants.Languages.Hungarian; // Hungarian

			base.SetUp();

			telematicsProvider = new RefEquipmentTelematicsBusinessObjectLookupProvider();

			changedLanguageDisposable = Res.TemporarilySwitchLanguage(LanguageForTest);
			var mockResStringData = Res.UseMockData();
			AssertEquals("If SetUp() fails here, a language may have been removed from Enterprise. Res.TemporarilySwitchLanguage(...) and Res.UseMockData() combined only work with valid languages.", LanguageForTest, mockResStringData.Language);

			mockResStringDataDisposable = mockResStringData;

			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_ShortCode = "BUS";
			equipment.RQ_Description = "I'm a bus.";

			mockResStringData.SetResourceString(equipment, e => e.RQ_DescriptionInfo, "I'm a foreign bus.");

			primaryKeyOfEquipment = equipment.PK;
		}

		protected override void TearDown()
		{
			if (mockResStringDataDisposable != null)
			{
				mockResStringDataDisposable.Dispose();
				mockResStringDataDisposable = null;
			}

			if (changedLanguageDisposable != null)
			{
				changedLanguageDisposable.Dispose();
				changedLanguageDisposable = null;
			}

			base.TearDown();
		}
	}
}
