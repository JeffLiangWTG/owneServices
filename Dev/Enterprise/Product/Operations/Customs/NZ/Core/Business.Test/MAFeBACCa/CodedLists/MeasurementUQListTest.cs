using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	using CargoWise.Integration;

	class MeasurementUQListTest : CodeDescriptionEnumListTestCase
	{
		public void TestTranslateFromCustomsStatisticalUQCode()
		{
			StatisticalUQList customsList = new StatisticalUQList();
			int translated = 0;
			int notTranslatable = 0;
			int notTranslated = 0;
			foreach (ICodeDescription element in customsList)
			{
				string customsCode = element.Code;
				string mafCode = MeasurementUQList.TranslateFromCustomsStatisticalUQCode(customsCode);
				if (mafCode == null)
				{
					notTranslated++;
				}
				else if (string.IsNullOrEmpty(mafCode))
				{
					notTranslatable++;
				}
				else
				{
					translated++;
				}
			}

			AssertEquals("There were no exceptions BUT... There might be a few new codes that can be translated."
				, "Translated: 8   Not Translatable: 14   Not Translated: 0"
				, "Translated: " + translated.ToString() + "   Not Translatable: " + notTranslatable.ToString() + "   Not Translated: " + notTranslated.ToString());
		}

		public void TestTranslateFromCustomsPackageTypeCode()
		{
			PackageTypeList customsList = new PackageTypeList();
			int translated = 0;
			int notTranslated = 0;
			foreach (ICodeDescription element in customsList)
			{
				string customsCode = element.Code;
				string mafCode = MeasurementUQList.TranslateFromCustomsPackageTypeCode(customsCode);
				if (string.IsNullOrEmpty(mafCode))
				{
					notTranslated++;
				}
				else
				{
					translated++;
				}
			}

			AssertEquals("There were no exceptions BUT... There might be a few new codes that can be translated."
				, "Translated: 109   Not Translated: 238"
				, "Translated: " + translated.ToString() + "   Not Translated: " + notTranslated.ToString());
		}

		protected override CodeDescriptionPairList GetNewList()
		{
			return new MeasurementUQList();
		}
	}
}
