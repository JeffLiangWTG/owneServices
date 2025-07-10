using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryNumber))]
	sealed class CusEntryNumberTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusEntryNum = (CusEntryNumber)base.GetNewBusinessObjectForDeleteTest(factory);
			cusEntryNum.CE_ParentTable = "JobDeclaration";

			return cusEntryNum;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var cusEntryNum = (CusEntryNumber)base.GetBusinessObjectForFetchForLoad();
			cusEntryNum.CE_ParentTable = "JobDeclaration";

			return cusEntryNum;
		}

		[ExpectNoExceptions]
		public void TestNew()
		{
			DummyBusinessObject bO = Factory.New<DummyBusinessObject>();
			var number = CusEntryNumber.New(bO, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			NUnit.Framework.Assert.That(number, NUnit.Framework.Is.TypeOf<CusEntryNumber>());
			NUnit.Framework.Assert.That(number, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryNumber)));
			NUnit.Framework.Assert.That(number.CE_EntryType, NUnit.Framework.Is.EqualTo("~~~").Using(CustomComparers.TypeComparison), "EntryType should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_ParentID, NUnit.Framework.Is.EqualTo(bO.PK), "PK should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_ParentTable, NUnit.Framework.Is.EqualTo(bO.TableName).Using(CustomComparers.TypeComparison), "TableName should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "Country PK should match current company's country PK");
			NUnit.Framework.Assert.That(number.Parent, NUnit.Framework.Is.EqualTo(bO).Using(CustomComparers.TypeComparison));
			number = CusEntryNumber.New(bO, "AAA", Core.Constants.CountryCodes.Brazil);
			NUnit.Framework.Assert.That(number, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryNumber)));
			NUnit.Framework.Assert.That(number.CE_EntryType, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "EntryType should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_ParentID, NUnit.Framework.Is.EqualTo(bO.PK), "PK should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_ParentTable, NUnit.Framework.Is.EqualTo(bO.TableName).Using(CustomComparers.TypeComparison), "TableName should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Brazil).Using(CustomComparers.TypeComparison), "Country PK should match current company's country PK");
			NUnit.Framework.Assert.That(number.Parent, NUnit.Framework.Is.EqualTo(bO).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLoadFromBusinessObject()
		{
			var bizObj1 = Factory.New<DummyBusinessObject>();
			var bizObj2 = Factory.New<DummyBusinessObject>();
			var number1 = CusEntryNumber.New(bizObj1, "111", "TW");
			var number2 = CusEntryNumber.New(bizObj1, "222", "US");
			var number3 = CusEntryNumber.New(bizObj2, "999", "TW");
			var loadedNumbers = CusEntryNumber.Load(bizObj1, "111", "TW");
			NUnit.Framework.Assert.That(loadedNumbers.PK, NUnit.Framework.Is.EqualTo(number1.PK));
			loadedNumbers = CusEntryNumber.Load(bizObj1, "111", "US");
			NUnit.Framework.Assert.That(loadedNumbers, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryNumber)));
			loadedNumbers = CusEntryNumber.Load(bizObj2, "111", "TW");
			NUnit.Framework.Assert.That(loadedNumbers, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryNumber)));
			loadedNumbers = CusEntryNumber.Load(bizObj1, "222", "US");
			NUnit.Framework.Assert.That(loadedNumbers.PK, NUnit.Framework.Is.EqualTo(number2.PK));
			loadedNumbers = CusEntryNumber.Load(bizObj2, "999", "TW");
			NUnit.Framework.Assert.That(loadedNumbers.PK, NUnit.Framework.Is.EqualTo(number3.PK));
		}

		[ExpectNoExceptions]
		public void TestCE_EntryNum()
		{
			var declaration = Factory.New<JobDeclaration>();
			var number = CusEntryNumber.New(declaration, "111", "TW");
			declaration.MarkLightValidationAsValidForTesting();
			NUnit.Framework.Assert.That(declaration.LightValidationIsValid, NUnit.Framework.Is.True);
			number.CE_EntryNum = "1234";
			NUnit.Framework.Assert.That(number.CE_EntryNum, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(!declaration.LightValidationIsValid, NUnit.Framework.Is.True);
		}
	}
}
