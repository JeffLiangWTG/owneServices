using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ViewUNDGAttributeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLanguage()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_IsSystem = true;

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			var attrib = substance.Observations.AddNew();
			attrib.DA_Index = "1";
			attrib.RunPreSaveValidation();
			AssertNoErrors(attrib);

			attrib.DA_Index = ZString.Empty;
			attrib.RunPreSaveValidation();
			AssertHasErrors(attrib.DA_LanguageInfo);

			attrib.DA_Language = Core.Constants.Languages.Ukrainian;
			AssertNoErrors(attrib.DA_LanguageInfo);

			attrib.DA_Language = Core.Constants.Languages.English;
			AssertHasErrors(attrib.DA_LanguageInfo);

			substance.DG_IsSystem = false;
			attrib.DA_Language = Core.Constants.Languages.Galician;
			attrib.DA_Language = Core.Constants.Languages.English;
			AssertNoErrors(attrib.DA_LanguageInfo);

			Factory.Save();
			attrib.DA_Language = Core.Constants.Languages.Russian;
			attrib.DA_Language = Core.Constants.Languages.English;
			AssertNoErrors(attrib.DA_LanguageInfo);

			attrib.DA_Type = ViewUNDGAttributeLookups.TypeConstants.UsrUSDOTShippingNames;
			attrib.DA_Language = Core.Constants.Languages.English;
			AssertNoErrors(attrib.DA_LanguageInfo);
		}

		public void TestDescriptor()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			var attrib = substance.Observations.AddNew();
			attrib.DA_Index = "1";
			attrib.RunPreSaveValidation();
			AssertNoErrors(attrib);

			attrib.DA_Index = ZString.Empty;
			attrib.RunPreSaveValidation();
			AssertHasErrors(attrib.DA_DescriptorInfo);

			attrib.DA_Descriptor = "XXX";
			AssertNoErrors(attrib.DA_DescriptorInfo);
		}
	}
}
