using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(ViewUNDGAttributeCollection))]
	sealed class ViewUNDGAttributeCollectionTest : ActiveBusinessObjectCollectionTestCase<ViewUNDGAttributeCollection>
	{
		public void TestDefaultLanguage()
		{
			var substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			substance.DetailsLanguage = Core.SharedConstants.Languages.Hebrew;
			var attrib = substance.Properties.AddNew();
			AssertEquals(Core.SharedConstants.Languages.Hebrew, attrib.DA_Language);
		}

		public void TestTypeless()
		{
			var substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			var typeless = new ViewUNDGAttributeCollection(substance, "");
			substance.DetailsLanguage = Core.SharedConstants.Languages.EnglishAmerican;
			Assert(typeless.AdditionalFilter.IsEmpty);
			var el = typeless.AddNew();
			AssertEquals("Should not set defauls for typless", "", el.DA_Language);
			AssertEquals("Should not set defauls for typless", "", el.DA_Type);
		}

		public void TestDefaultsForNewChild()
		{
			var substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			var attrib = substance.Properties.AddNew();
			AssertEquals(ViewUNDGAttributeLookups.TypeConstants.Properties, attrib.DA_Type);
		}

		public void TestLanguageFiltering()
		{
			var substance = Factory.New<UNDGSubstance>();

			var attrib1 = substance.Properties.AddNew();
			attrib1.DA_Language = Core.Constants.Languages.EnglishAmerican;

			var attrib2 = substance.Properties.AddNew();
			attrib2.DA_Language = Core.Constants.Languages.EnglishAmerican;

			var attrib3 = substance.Properties.AddNew();
			attrib3.DA_Language = Core.Constants.Languages.ChineseSimplified;

			substance.Properties.AddNew();

			substance.DetailsLanguage = "";
			AssertEquals(4, substance.Properties.Count);

			substance.DetailsLanguage = Core.Constants.Languages.EnglishAmerican;
			AssertEquals(3, substance.Properties.Count);

			substance.DetailsLanguage = Core.Constants.Languages.Gujarati;
			AssertEquals(1, substance.Properties.Count);

			substance.DetailsLanguage = Core.Constants.Languages.ChineseSimplified;
			AssertEquals(2, substance.Properties.Count);
		}

		protected override ViewUNDGAttributeCollection GetCollectionToTest()
		{
			var substance = Factory.New<UNDGSubstance>();
			return new ViewUNDGAttributeCollection(substance, ViewUNDGAttributeLookups.TypeConstants.Observations);
		}
	}
}
