using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BondedFactory))]
	sealed class BondedFactoryTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<BondedFactory>();

		[ExpectNoExceptions]
		public void TestBoundedId()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var addresse = header.Addresses[2];

			var dec = Factory.New<JobDeclarationForTest>();
			var bondedFactory = dec.BondedFactories.AddNew();
			bondedFactory.E2_OA_Address = addresse.PK;

			var customsCode = addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.EPZ, "EPZ001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(bondedFactory.BondedID, NUnit.Framework.Is.EqualTo("EPZ001").Using(CustomComparers.TypeComparison), "CBPCode");
			NUnit.Framework.Assert.That(bondedFactory.BondedIDTypeCode, NUnit.Framework.Is.EqualTo("EPZ").Using(CustomComparers.TypeComparison), "CBPCodeType");
			NUnit.Framework.Assert.That(bondedFactory.BondedIDTypeDescription, NUnit.Framework.Is.EqualTo("Export Processing Zone").Using(CustomComparers.TypeComparison), "CBPCodeTypeDescription");

			customsCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			customsCode.OK_CustomsRegNo = "FTZ001";
			NUnit.Framework.Assert.That(bondedFactory.BondedID, NUnit.Framework.Is.EqualTo("FTZ001").Using(CustomComparers.TypeComparison), "CBPCode");
			NUnit.Framework.Assert.That(bondedFactory.BondedIDTypeCode, NUnit.Framework.Is.EqualTo("FTZ").Using(CustomComparers.TypeComparison), "CBPCodeType");
			NUnit.Framework.Assert.That(bondedFactory.BondedIDTypeDescription, NUnit.Framework.Is.EqualTo("Free Trade Zone").Using(CustomComparers.TypeComparison), "CBPCodeTypeDescription");

			customsCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			customsCode.OK_CustomsRegNo = "CBF001";
			NUnit.Framework.Assert.That(bondedFactory.BondedID, NUnit.Framework.Is.EqualTo("CBF001").Using(CustomComparers.TypeComparison), "CBPCode");
			NUnit.Framework.Assert.That(bondedFactory.BondedIDTypeCode, NUnit.Framework.Is.EqualTo("CBF").Using(CustomComparers.TypeComparison), "CBPCodeType");
			NUnit.Framework.Assert.That(bondedFactory.BondedIDTypeDescription, NUnit.Framework.Is.EqualTo("Bonded Factory").Using(CustomComparers.TypeComparison), "CBPCodeTypeDescription");
		}

		[ExpectNoExceptions]
		public void TestPropertyCaptions()
		{
			var bondedFactory = Factory.New<BondedFactory>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(bondedFactory.OrganisationPKInfo, "Organization", "The VAT number of the bonded warehouse.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(bondedFactory.BondedIDInfo, "Bonded ID", "The Bonded ID of the bonded warehouse issued by customs.");
		}
	}
}
