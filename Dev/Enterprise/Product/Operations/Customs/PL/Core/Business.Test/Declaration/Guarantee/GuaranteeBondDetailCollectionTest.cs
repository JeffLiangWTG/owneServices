using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(GuaranteeBondDetailCollection))]
class GuaranteeBondDetailCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestSetDefaultValuesForNewChild()
	{
		var coll = (GuaranteeBondDetailCollection)GetCollectionToTest();
		var element = coll.AddNew();
		AssertEquals(coll.Master.PK, element.Parent.PK);
		AssertEquals(GuaranteeBondDetail.Schema.ApplicationCode, element.PW_ApplicationCode);
		AssertEquals(CusEntryInstructionSchema.Constants.Prefix, element.PW_ParentTableCode);
		AssertEquals(GuaranteeBondDetail.Schema.ActivityCode, element.PW_ActivityCode);
		AssertEquals(Core.Constants.CountryCodes.Poland, element.PW_RN_NKCountryOfIssue);
		AssertEquals(Core.Constants.CurrencyCodes.Poland, element.PW_RX_NKCurrency);
		AssertEquals(0m, element.PW_BondAmount);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var cusEntryInstruction = Factory.New<CusEntryInstruction>();
		return new GuaranteeBondDetailCollection(cusEntryInstruction);
	}
}
