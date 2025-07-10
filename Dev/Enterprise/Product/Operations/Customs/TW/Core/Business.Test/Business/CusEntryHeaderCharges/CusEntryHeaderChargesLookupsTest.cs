using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryHeaderChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestEntryHeaderCharges()
		{
			CusEntryHeaderCharges parent = Factory.New<CusEntryHeaderCharges>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Lookups.EntryHeaderCharges));
		}

		[ExpectNoExceptions]
		public void TestChargeTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeTW = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "B40", refCusRateTypeTW.PK, description: "Business Tax");
			var refCusRateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "B52", refCusRateTypeTW.PK, description: "Trade promotion fee for exported goods");
			var startDate = ZDate.Today.AddMonths(-3);
			var endDate = ZDate.Today.AddMonths(3);
			var refCusTaxOrFeeTw = helper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate, description: "Trade Promotion Fee");
			var refCusTaxOrFeeZa = helper.CreateTaxOrFee("VAT", 0.15000000m, Core.Constants.CountryCodes.SouthAfrica, startDate: startDate, endDate: endDate, description: "VAT Normal");
			Factory.Save();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var customsEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var cusEntryHeaderCharge = customsEntryHeader.Charges.AddNew();
			var chargeTypeList = cusEntryHeaderCharge.Lookups.ChargeTypeList;
			NUnit.Framework.Assert.That(chargeTypeList.Count, NUnit.Framework.Is.EqualTo(7));
			NUnit.Framework.Assert.That(chargeTypeList.ContainsCode("B40"), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(chargeTypeList.ContainsCode("B52"), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(chargeTypeList.ContainsCode("TPF"), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(chargeTypeList.ContainsCode("VAT"), NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(chargeTypeList.ContainsCode("ADD"), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(chargeTypeList.ContainsCode("CVD"), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(chargeTypeList.ContainsCode("ADT"), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(chargeTypeList.ContainsCode("RTD"), NUnit.Framework.Is.EqualTo(true));
		}
	}
}
