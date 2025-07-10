using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ViewUNDGAttribute))]
	sealed class ViewUNDGAttributeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var subs = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			var attr = Factory.NewWithValidTestData<ViewUNDGAttribute>();
			attr.DA_DG = subs.PK;
			return attr;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		public void TestTypeDescription()
		{
			var attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.ProperShippingName;
			AssertEquals("Names", attribute.TypeDescription);

			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.Observations;
			AssertEquals("Observations", attribute.TypeDescription);

			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.Properties;
			AssertEquals("Properties", attribute.TypeDescription);

			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.QualifyingDescriptiveText;
			AssertEquals("Qualifying Descriptive Text", attribute.TypeDescription);
		}

		public void TestReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_IsSystem = true;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			var attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_DG = substance.PK;

			Factory.Save();
			AssertEquals(false, attribute.DA_DescriptorInfo.ReadOnly);

			attribute.DA_Language = Core.Constants.Languages.English;
			Factory.Save();
			AssertEquals(true, attribute.DA_DescriptorInfo.ReadOnly);

			substance.DG_IsSystem = false;
			AssertEquals(false, attribute.DA_DescriptorInfo.ReadOnly);

			substance.DG_IsSystem = true;
			AssertEquals(true, attribute.DA_DescriptorInfo.ReadOnly);

			attribute.DA_Language = Core.Constants.Languages.Gujarati;
			AssertEquals(false, attribute.DA_DescriptorInfo.ReadOnly);

			substance.DetailsLanguage = "";
			Assert(!attribute.DA_LanguageInfo.ReadOnly);

			substance.DetailsLanguage = Core.Constants.Languages.English;
			Assert(attribute.DA_LanguageInfo.ReadOnly);
		}

		public void TestDelete()
		{
			var substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery());

			var attrib = substance.Properties.AddNew();
			attrib.Delete();
			AssertEquals(true, attrib.IsDeleted);

			attrib = substance.Properties.AddNew();
			attrib.DA_Language = Core.Constants.Languages.Gujarati;
			attrib.Delete();
			AssertEquals(true, attrib.IsDeleted);

			attrib = substance.Properties.AddNew();
			attrib.DA_Language = Core.Constants.Languages.English;
			attrib.DA_Type = "XXX";
			attrib.DA_Index = "312";
			Factory.Save();

			bool cannotDeleteThrown = false;
			try
			{
				attrib.Delete();
			}
			catch (CannotDeleteException)
			{
				cannotDeleteThrown = true;
			}

			AssertEquals(true, cannotDeleteThrown);
			AssertEquals(false, attrib.IsDeleted);

			attrib = substance.Properties.AddNew();
			attrib.DA_Language = Core.Constants.Languages.English;
			attrib.DA_Type = "USN";
			attrib.DA_Index = "312";
			Factory.Save();
			attrib.Delete();

			AssertEquals(true, attrib.IsDeleted);
		}
	}
}
