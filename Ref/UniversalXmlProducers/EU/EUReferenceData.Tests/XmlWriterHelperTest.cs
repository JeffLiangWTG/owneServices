using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	class XmlWriterHelperTest
	{
		[Test]
		public void RefNctsCodesWriterConfigurationTypeTest()
		{
			Assert.That("CargoWise.RefDbRepo.Common.UniversalXmlWriter.XmlWriterConfiguration", Is.EqualTo(XmlWriterHelper.GetRefNctsCodesWriterConfiguration(new NctsAdditionalInformation().CodeType).GetType().ToString()));
		}

		[Test]
		public void XmlWriterConfig()
		{
			var config = XmlWriterHelper.GetRefNctsCodesWriterConfiguration(new NctsAdditionalInformation().CodeType);

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusCodeList);
			var codeListConfig = config.GetConfiguration(refType);

			Assert.That(codeListConfig, Is.Not.Null);
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Code))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZK_NKCodeType))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Description))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_StartDate))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_EndDate))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.RefCusCodeListLanguages))));

			AssertKeySets(nameof(RefCusCodeList), codeListConfig.GetKeySets(),
				nameof(RefCusCodeList.ZZD_Code),
				nameof(RefCusCodeList.ZZD_ZZK_NKCodeType),
				nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping));

			refType = typeof(RefCusCodeListLanguage);
			var languageConfig = config.GetConfiguration(refType);

			Assert.That(languageConfig, Is.Not.Null);
			Assert.That(languageConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListLanguage.ZXA_ZX6_NKLanguage))));
			Assert.That(languageConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListLanguage.ZXA_Description))));

			AssertKeySets(nameof(RefCusCodeListLanguage), languageConfig.GetKeySets(),
				nameof(RefCusCodeListLanguage.ZXA_ZX6_NKLanguage));
		}

		[Test]
		public void XmlWriterConfig_WithAttributes()
		{
			var config = XmlWriterHelper.GetRefNctsCodesWriterConfiguration(new NctsAdditionalInformation().CodeType, true);

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusCodeList);
			var codeListConfig = config.GetConfiguration(refType);

			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.RefCusCodeListAttributes))));

			refType = typeof(RefCusCodeListAttribute);
			var attributeConfig = config.GetConfiguration(refType);

			Assert.That(attributeConfig, Is.Not.Null);
			Assert.That(attributeConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListAttribute.ZZE_ZXE_NKName))));
			Assert.That(attributeConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListAttribute.ZZE_Value))));

			AssertKeySets(nameof(RefCusCodeListAttribute), attributeConfig.GetKeySets(),
				nameof(RefCusCodeListAttribute.ZZE_ZXE_NKName),
				nameof(RefCusCodeListAttribute.ZZE_Value));
		}

		[Test]
		public void XmlWriterConfig_WithoutAttributes()
		{
			var config = XmlWriterHelper.GetRefNctsCodesWriterConfiguration(new NctsAdditionalInformation().CodeType, false);

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusCodeList);
			var codeListConfig = config.GetConfiguration(refType);

			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.RefCusCodeListAttributes))), Is.False);
		}

		void AssertKeySets(string entity, IEnumerable<KeySet> keySets, params string[] properties)
		{
			var keys = keySets.Select(x => x.PropertySchema.Name).OrderBy(x => x).ToList();
			var fields = properties.OrderBy(x => x).ToList();

			Assert.That(string.Join(", ", fields), Is.EqualTo(string.Join(", ", keys)), $"Keys for {entity}");
		}
	}
}
