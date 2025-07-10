using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffUOM))]
	class RefCusTariffUOMTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var result = Factory.New<RefCusTariffUOM>();
			result.ZZ8_ZZ1_Tariff = cusTariff.PK;
			result.ZZ8_Type = "CU1";
			result.ZZ8_UOM = "VWG";
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.New<RefCusTariffUOM>();
			result.ZZ8_ZZ1_Tariff = cusTariff.PK;
			var conflictUOMs = factory.Load<RefCusTariffUOM>(new ZQuery(RefCusTariffUOMSchema.ZZ8_ZZ1_Tariff, result.ZZ8_ZZ1_Tariff));
			result.ZZ8_Type = "CU" + Convert.ToByte(conflictUOMs.Length + 1);
			result.ZZ8_UOM = "VWG";
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			cusTariff = UniversalReferenceTestDataHelper.CreateInternalRefCusTariff(Factory, Core.Constants.CountryCodes.Eritrea, helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1").PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			Factory.Save();
		}

		RefCusTariff cusTariff;
		UniversalReferenceTestDataHelper helper;
	}
}
