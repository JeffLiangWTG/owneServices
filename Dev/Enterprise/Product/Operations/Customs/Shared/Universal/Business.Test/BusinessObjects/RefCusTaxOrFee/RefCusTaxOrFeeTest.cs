using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTaxOrFee))]
	public class RefCusTaxOrFeeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZZF_Description()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			helper.CreateOrGetLanguage("DE", "German");
			Factory.Save();

			helper.CreateRefCusTaxOrFeeType("VAT", "Value Added Tax");
			helper.CreateRefCusTaxOrFeeType("OTH", "Other");
			var traditionalChineseTaxOrFee = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, 0, 0, "VAT");
			traditionalChineseTaxOrFee.ZZF_Description = "Business tax";

			helper.CreateTaxOrFeeLanguage(traditionalChineseTaxOrFee, "ZHT", "營業稅");

			var germanTaxOrFee = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, 0, 0, "OTH");
			germanTaxOrFee.ZZF_Description = "Promotion trade service fee";
			helper.CreateTaxOrFeeLanguage(germanTaxOrFee, "DE", "Servicegebühr für Werbeaktionen");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "EN";
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Code in English", "Business tax", traditionalChineseTaxOrFee.ZZF_Description);
				AssertEquals("Code in English", "Promotion trade service fee", germanTaxOrFee.ZZF_Description);
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.German;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Code in German", "Business tax", traditionalChineseTaxOrFee.ZZF_Description);
				AssertEquals("Code in English", "Servicegebühr für Werbeaktionen", germanTaxOrFee.ZZF_Description);
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseTraditional;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Code in English", "營業稅", traditionalChineseTaxOrFee.ZZF_Description);
				AssertEquals("Code in Italian", "Promotion trade service fee", germanTaxOrFee.ZZF_Description);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var bo = (RefCusTaxOrFee)base.GetNewBusinessObjectForDeleteTest(factory);
			bo.ZZF_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			return bo;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
	}
}
