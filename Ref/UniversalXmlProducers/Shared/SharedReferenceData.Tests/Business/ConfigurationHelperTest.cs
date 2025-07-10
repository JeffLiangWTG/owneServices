using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;
using CargoWise.RefDbRepo.SharedReferenceData.Business;

namespace CargoWise.RefDbRepo.SharedReferenceData.Tests
{
	[TestFixture]
	class ConfigurationHelperTest
	{
		[Test]
		public void XmlWriterConfig_WithCodeType()
		{
			var config = ConfigurationHelper.GetRefCusCodeTypeWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var type = typeof(RefCusCodeType);
			var codeTypeConfig = config.GetConfiguration(typeof(RefCusCodeType));

			Assert.That(codeTypeConfig, Is.Not.Null);
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeType.ZZK_CodeType))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeType.ZZK_Description))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeType.ZZK_ZZZ_NKDataGrouping))));

			AssertKeySets(nameof(RefCusCodeType), codeTypeConfig.GetKeySets(),
				nameof(RefCusCodeType.ZZK_CodeType),
				nameof(RefCusCodeType.ZZK_ZZZ_NKDataGrouping));
		}

		[Test]
		public void XmlWriterConfig_withCodeList()
		{
			var config = ConfigurationHelper.GetRefCusCodeListWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var type = typeof(RefCusCodeList);
			var codeListConfig = config.GetConfiguration(typeof(RefCusCodeList));

			Assert.That(codeListConfig, Is.Not.Null);
			Assert.That(codeListConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeList.ZZD_Code))));
			Assert.That(codeListConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeList.ZZD_Description))));
			Assert.That(codeListConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeList.ZZD_StartDate))));
			Assert.That(codeListConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeList.ZZD_EndDate))));
			Assert.That(codeListConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeList.ZZD_ZZK_NKCodeType))));
			Assert.That(codeListConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping))));
			Assert.That(codeListConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeList.RefCusCodeListAttributes))));

			AssertKeySets(nameof(RefCusCodeList), codeListConfig.GetKeySets(),
				nameof(RefCusCodeList.ZZD_Code),
				nameof(RefCusCodeList.ZZD_ZZK_NKCodeType),
				nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping),
				nameof(RefCusCodeList.RefCusCodeListAttributes));

			type = typeof(RefCusCodeListAttribute);
			var attributeConfig = config.GetConfiguration(type);

			Assert.That(attributeConfig, Is.Not.Null);
			Assert.That(attributeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttribute.ZZE_ZXE_NKName))));
			Assert.That(attributeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttribute.ZZE_Value))));

			AssertKeySets(nameof(RefCusCodeListAttribute), attributeConfig.GetKeySets(),
				nameof(RefCusCodeListAttribute.ZZE_ZXE_NKName),
				nameof(RefCusCodeListAttribute.ZZE_Value));
		}

		[Test]
		public void XmlWriterConfig_WithCodeListAttributeName()
		{
			var config = ConfigurationHelper.GetRefCusCodeListAttributeNameWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var type = typeof(RefCusCodeListAttributeName);
			var codeTypeConfig = config.GetConfiguration(typeof(RefCusCodeListAttributeName));

			Assert.That(codeTypeConfig, Is.Not.Null);
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_Name))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_Description))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_ZZK_NKCodeType))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_ZZZ_NKDataGrouping))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_IsMandatory))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_AllowDuplicates))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_IsValueMandatory))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_ValueDataType))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_MinLengthOrValue))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_MaxLengthOrValue))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_DecimalPlaces))));
			Assert.That(codeTypeConfig.IsIncluded(type.GetProperty(nameof(RefCusCodeListAttributeName.ZXE_ColumnCaption))));

			AssertKeySets(nameof(RefCusCodeListAttributeName), codeTypeConfig.GetKeySets(),
				nameof(RefCusCodeListAttributeName.ZXE_Name),
				nameof(RefCusCodeListAttributeName.ZXE_ZZK_NKCodeType),
				nameof(RefCusCodeListAttributeName.ZXE_ZZZ_NKDataGrouping));
		}

		void AssertKeySets(string entity, IEnumerable<KeySet> keySets, params string[] properties)
		{
			var keys = keySets.Select(x => x.PropertySchema.Name).OrderBy(x => x).ToList();
			var fields = properties.OrderBy(x => x).ToList();

			Assert.That(string.Join(", ", fields), Is.EqualTo(string.Join(", ", keys)), $"Keys for {entity}");
		}
	}
}
