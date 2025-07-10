using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGAttributeZZCollection))]
	sealed class UNDGAttributeZZCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGAttributeZZCollection>
	{
		public void TestDefaultLanguage()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			substance.DetailsLanguage = Core.SharedConstants.Languages.Hebrew;

			var attrib = substance.Names.AddNew();
			AssertEquals(Core.SharedConstants.Languages.Hebrew, attrib.DAZ_Language);
		}

		public void TestDefaultsForNewChild()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			var attrib = substance.Names.AddNew();
			AssertEquals(ViewUNDGAttributeLookups.TypeConstants.ProperShippingName, attrib.DAZ_Type);
		}

		public void TestLanguageFiltering()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceADR>();

			var attrib1 = substance.Names.AddNew();
			attrib1.DAZ_Language = Core.Constants.Languages.EnglishAmerican;

			var attrib2 = substance.Names.AddNew();
			attrib2.DAZ_Language = Core.Constants.Languages.EnglishAmerican;

			var attrib3 = substance.Names.AddNew();
			attrib3.DAZ_Language = Core.Constants.Languages.ChineseSimplified;

			substance.Names.AddNew();

			substance.DetailsLanguage = "";
			AssertEquals(4, substance.Names.Count);

			substance.DetailsLanguage = Core.Constants.Languages.EnglishAmerican;
			AssertEquals(3, substance.Names.Count);

			substance.DetailsLanguage = Core.Constants.Languages.Gujarati;
			AssertEquals(1, substance.Names.Count);

			substance.DetailsLanguage = Core.Constants.Languages.ChineseSimplified;
			AssertEquals(2, substance.Names.Count);
		}

		protected override UNDGAttributeZZCollection GetCollectionToTest()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			return new UNDGAttributeZZCollection(Factory, substance, ViewUNDGAttributeLookups.TypeConstants.ProperShippingName);
		}
	}
}
