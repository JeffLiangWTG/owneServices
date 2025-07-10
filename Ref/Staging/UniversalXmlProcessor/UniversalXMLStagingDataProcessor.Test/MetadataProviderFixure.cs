using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class MetadataProviderFixure
	{
		[Test]
		public void GetConstantPropertyNamesAndValues()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			var result = provider.GetConstantPropertyNamesAndValues("RefCusTariff");
			CollectionAssert.AreEqual(new[] { Tuple.Create("ZZ1_ZZI_NKTariffType", "IMP"),
				Tuple.Create("ZZ1_ZZI_ZZZ_NKDataGrouping", "EUN"),
				Tuple.Create("ZZ1_ZZZ_NKDataGrouping", "EUN") }, result);
		}

		[TestCase("RefCusTariff", "RefCusRate", true)]
		[TestCase("RefCusRate", "RefCusApplicability", true)]
		[TestCase("RefCusTariff", "RefCusCondition", false)]
		[TestCase("RefCusCondition", "RefCusApplicability", false)]
		public void IsMandatory(string entityType, string propertyName, bool result)
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			Assert.AreEqual(result, provider.IsMandatory(entityType, propertyName));
		}

		[Test]
		public void GetUpdatableProperties()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" {0}>
		  <Key>
			<PropertyRef Name=""ZZ1_TariffCode"" />
		</Key>
		  <Property Name=""ZZ1_TariffCode"" {1} />
		  <Property Name=""ZZ1_ZZI_NKTariffType"" {1} />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			var provider = new MetadataProvider(string.Format(CultureInfo.InvariantCulture, xml, @"Data=""true""", string.Empty), cacheProvider, cacheProviderForOriginal);
			Assert.AreEqual(new[] { "ZZ1_ZZI_NKTariffType" }, provider.GetUpdatableProperties("RefCusTariff"));
			provider = new MetadataProvider(string.Format(CultureInfo.InvariantCulture, xml, @"Data=""true""", @"Data=""true"""), cacheProvider, cacheProviderForOriginal);
			Assert.AreEqual(new[] { "ZZ1_ZZI_NKTariffType" }, provider.GetUpdatableProperties("RefCusTariff"));
			provider = new MetadataProvider(string.Format(CultureInfo.InvariantCulture, xml, @"Data=""true""", @"Data=""false"""), cacheProvider, cacheProviderForOriginal);
			Assert.AreEqual(new string[0], provider.GetUpdatableProperties("RefCusTariff"));
			provider = new MetadataProvider(string.Format(CultureInfo.InvariantCulture, xml, string.Empty, @"Data=""true"""), cacheProvider, cacheProviderForOriginal);
			Assert.AreEqual(new[] { "ZZ1_ZZI_NKTariffType" }, provider.GetUpdatableProperties("RefCusTariff"));
			provider = new MetadataProvider(string.Format(CultureInfo.InvariantCulture, xml, string.Empty, @"Data=""false"""), cacheProvider, cacheProviderForOriginal);
			Assert.AreEqual(new string[0], provider.GetUpdatableProperties("RefCusTariff"));
			provider = new MetadataProvider(string.Format(CultureInfo.InvariantCulture, xml, string.Empty, string.Empty), cacheProvider, cacheProviderForOriginal);
			Assert.AreEqual(new string[0], provider.GetUpdatableProperties("RefCusTariff"));
		}

		[Test]
		public void GetUpdatableProperties_IgnoreStartsWithOperation()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""True"">
		  <Key>
			<PropertyRef Name=""ZZ1_TariffCode"" Op=""StartsWith""/>
		</Key>
		  <Property Name=""ZZ1_TariffCode"" />
		  <Property Name=""ZZ1_ZZI_NKTariffType"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			var provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			Assert.AreEqual(new[] { "ZZ1_ZZI_NKTariffType" }, provider.GetUpdatableProperties("RefCusTariff"));
		}

		[Test]
		public void GetUpdatableProperties_WithOrder()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""True"">
			<Key Order=""0"">
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
			<Key Order=""1"">
				<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			</Key>
			<Property Name=""ZZ1_TariffCode"" />
			<Property Name=""ZZ1_ZZI_NKTariffType"" />
			<Property Name=""ZZ1_ZZZ_NKDataGrouping"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			var provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			var updatableProperties = provider.GetUpdatableProperties("RefCusTariff", 0);
			Assert.False(updatableProperties.Contains("ZZ1_TariffCode"));
			Assert.True(updatableProperties.Contains("ZZ1_ZZI_NKTariffType"));
			updatableProperties = provider.GetUpdatableProperties("RefCusTariff", 1);
			Assert.True(updatableProperties.Contains("ZZ1_TariffCode"));
			Assert.False(updatableProperties.Contains("ZZ1_ZZI_NKTariffType"));
		}

		[Test]
		public void GetKeysWithOperations()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
		<Key>
			<PropertyRef Name=""ZZ1_TariffCode"" Op=""StartsWith"" />
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			var provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			var result = provider.GetKeys("RefCusTariff");
			Assert.AreEqual(result.ElementAt(0).Operation, Operations.StartsWith);
			Assert.AreEqual(result.ElementAt(1).Operation, Operations.Equals);
			Assert.AreEqual(result.ElementAt(2).Operation, Operations.Equals);
		}

		[Test]
		public void GetKeysWithConstantValue()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
		<Key>
			<PropertyRef Name=""ZZ1_TariffCode"" ConstantValue=""CValue"" />
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			var provider = (IMetadataProvider)new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			var result = provider.GetKeys("RefCusTariff");
			Assert.AreEqual("CValue", result.ElementAt(0).ConstantValue);
			Assert.AreEqual(null, result.ElementAt(1).ConstantValue);
			Assert.AreEqual(null, result.ElementAt(2).ConstantValue);
		}

		[Test]
		public void GetKeysWithOrder()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
		<Key Order=""0"">
			<PropertyRef Name=""ZZ1_TariffCode"" />
		</Key>
		<Key Order=""1"">
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			var provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			var result = provider.GetKeys("RefCusTariff");
			Assert.AreEqual("ZZ1_TariffCode", result.First(x => x.Order == 0).Name);
			CollectionAssert.AreEquivalent(new[] { "ZZ1_ZZI_NKTariffType", "ZZ1_ZZZ_NKDataGrouping" }, result.Where(x => x.Order == 1).Select(x => x.Name));
		}

		[Test]
		public void GetKeys()
		{
			var provider = (IMetadataProvider)new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			var result = provider.GetKeys("RefCusTariff");
			var expected = new KeyProperty[] {
				new KeyProperty { Name = "ZZ1_ZZI_NKTariffType" },
				new KeyProperty { Name = "ZZ1_ZZI_ZZZ_NKDataGrouping" },
				new KeyProperty { Name = "ZZ1_TariffCode" },
				new KeyProperty { Name = "ZZ1_IAMUnique" },
				new KeyProperty { Name = "ZZ1_ZZZ_NKDataGrouping" } };

			Assert.AreEqual(result.ElementAt(0).Name, expected.ElementAt(0).Name);
			Assert.AreEqual(result.ElementAt(1).Name, expected.ElementAt(1).Name);
			Assert.AreEqual(result.ElementAt(2).Name, expected.ElementAt(2).Name);
			Assert.AreEqual(result.ElementAt(3).Name, expected.ElementAt(3).Name);
			Assert.AreEqual(result.ElementAt(4).Name, expected.ElementAt(4).Name);
		}

		[Test]
		public void IsData()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			Assert.True(provider.IsData("RefCusTariff"));
		}

		[Test]
		public void IsData_KeyHasStartsWithOperator()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" IsData=""True"">
		<Key>
			<PropertyRef Name=""ZZ1_TariffCode"" Op=""StartsWith"" />
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			var provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			Assert.False(provider.IsData("RefCusTariff"));
		}

		[Test]
		public void EnableNullOrEmptyKeyMatching()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			Assert.True(provider.EnableNullOrEmptyKeyMatching("RefCusTariff"));

			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" IsData=""True""  EnableNullOrEmptyKeyMatching=""false"">
		<Key>
			<PropertyRef Name=""ZZ1_TariffCode"" />
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			Assert.False(provider.EnableNullOrEmptyKeyMatching("RefCusTariff"));
		}

		[Test]
		public void EnableExpirableRefCusTariffUOM()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			Assert.False(provider.EnableExpirable);

			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" IsData=""True"">
		<Key>
			<PropertyRef Name=""ZZ1_TariffCode"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			var result = true;
			Assert.DoesNotThrow(() => result = provider.EnableExpirable);
			Assert.False(result);

			xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariffUOM"" IsData=""True"">
		<Key>
			<PropertyRef Name=""ZZ8_Type"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			Assert.False(provider.EnableExpirable);

			xml = xml.Replace(@"IsData=""True""", @"EnableExpirable=""True""");
			provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			Assert.True(provider.EnableExpirable);
		}

		[Test]
		public void EnableExpirableRefCodeListAttribute()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			Assert.False(provider.EnableExpirable);

			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusCodeList"" IsData=""True"">
		<Key>
			<PropertyRef Name=""ZZ1_TariffCode"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			var result = true;
			Assert.DoesNotThrow(() => result = provider.EnableExpirable);
			Assert.False(result);

			xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusCodeListAttribute"" IsData=""True"">
		<Key>
			<PropertyRef Name=""ZZD_Code"" />
		</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>";
			provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			Assert.False(provider.EnableExpirable);

			xml = xml.Replace(@"IsData=""True""", @"EnableExpirable=""True""");
			provider = new MetadataProvider(xml, cacheProvider, cacheProviderForOriginal);
			Assert.True(provider.EnableExpirable);
		}

		[Test]
		public void GetProperties()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			CollectionAssert.AreEqual(new[] { "ZZT_StartDate", "ZZT_EndDate", "ZZT_ZZA_NKTradeGroup",
				"ZZT_ZZA_ZZZ_NKDataGrouping", "ZZT_AdditionalCode", "ZZT_OrderNumber", "RefCusExcludedTradeGroup" }, provider.GetProperties("RefCusApplicability"));
		}

		[Test]
		public void ShouldCalculateIAmUnique()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			Assert.False(provider.ShouldCalculateIAmUnique("RefCusTariff", "ZZ1"));
			string xml_IAmUnique = @"
<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ1_TariffCode"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""RefCusTariffRelationship.ZZH_TariffCode"" />
		  </Key>
		  <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""IMP"" />
		  <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		  <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZ1_Description"" Type=""nvarchar(max)"" />
		  <Property Name=""ZZ1_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ1_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		</EntityType>		
	</Schema>
</UniversalReferenceData>";
			provider = new MetadataProvider(xml_IAmUnique, cacheProvider, cacheProviderForOriginal);
			Assert.True(provider.ShouldCalculateIAmUnique("RefCusTariff", "ZZ1"));
		}

		[Test]
		public void ChechOriginalMetadataProvider()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			Assert.That(provider.OriginalMetadataProvider, Is.Null);

			provider = new MetadataProvider(xmlContentWithEmptyOriginalSchema, cacheProvider, cacheProviderForOriginal);
			Assert.That(provider.OriginalMetadataProvider, Is.Not.Null);
		}

		[Test]
		public void GetPropertiesWithOriginalSchema()
		{
			var provider = new MetadataProvider(xmlContentWithOriginalSchema, cacheProvider, cacheProviderForOriginal);
			CollectionAssert.AreEquivalent(new[] { "RefCusRateApplicability", "RefCusCondition", "ZZ1_TariffCode" }, provider.GetProperties("RefCusTariff"));
			CollectionAssert.AreEquivalent(new[] { "RefCusRateApplicabilityUOM", "S01_AdditionalCode", "RefCusExcludedTradeGroupNew" }, provider.GetProperties("RefCusRateApplicability"));
			CollectionAssert.AreEquivalent(new[] { "ZZT_AdditionalCode", "RefCusExcludedTradeGroup" }, provider.GetProperties("RefCusApplicability"));
			CollectionAssert.AreEquivalent(new[] { "RefCusApplicability" }, provider.GetProperties("RefCusCondition"));
			CollectionAssert.AreEquivalent(new[] { "ZXG_UOM" }, provider.GetProperties("RefCusRateUOM"));
			CollectionAssert.AreEquivalent(new[] { "S02_UOM" }, provider.GetProperties("RefCusRateApplicabilityUOM"));
			CollectionAssert.AreEquivalent(new[] { "ZZC_ZZA_NKTradeGroup" }, provider.GetProperties("RefCusExcludedTradeGroup"));
			CollectionAssert.AreEquivalent(new[] { "S03_ZZA_NKTradeGroup" }, provider.GetProperties("RefCusExcludedTradeGroupNew"));

			CollectionAssert.AreEquivalent(new[] { "RefCusRate", "RefCusCondition", "ZZ1_TariffCode" },
									provider.OriginalMetadataProvider.GetProperties("RefCusTariff"));
			CollectionAssert.AreEquivalent(new[] { "RefCusApplicability", "RefCusRateUOM" },
									provider.OriginalMetadataProvider.GetProperties("RefCusRate"));
			CollectionAssert.AreEquivalent(new[] { "ZZT_AdditionalCode", "RefCusExcludedTradeGroup" },
									provider.OriginalMetadataProvider.GetProperties("RefCusApplicability"));
			CollectionAssert.AreEquivalent(new[] { "RefCusApplicability" },
									provider.OriginalMetadataProvider.GetProperties("RefCusCondition"));
			CollectionAssert.AreEquivalent(new[] { "ZXG_UOM" },
									provider.OriginalMetadataProvider.GetProperties("RefCusRateUOM"));
			CollectionAssert.AreEquivalent(new[] { "ZZC_ZZA_NKTradeGroup" },
									provider.OriginalMetadataProvider.GetProperties("RefCusExcludedTradeGroup"));
		}

		[Test]
		public void GetPropertiesWithOnlyOriginalSchema()
		{
			var provider = new MetadataProvider(xmlContentWithOnlyOriginalSchema, cacheProvider, cacheProviderForOriginal);
			Assert.Throws<NullReferenceException>(() => { provider.GetProperties("RefCusTariff"); });

			CollectionAssert.AreEquivalent(new[] { "ZZT_StartDate", "ZZT_EndDate", "ZZT_ZZA_NKTradeGroup",
				"ZZT_ZZA_ZZZ_NKDataGrouping", "ZZT_AdditionalCode", "ZZT_OrderNumber", "RefCusExcludedTradeGroup" }, provider.OriginalMetadataProvider.GetProperties("RefCusApplicability"));
		}

		[Test]
		public void Validate_WithoutExpirableTypsInKeyProps()
		{
			var provider = new MetadataProvider(xmlContent, cacheProvider, cacheProviderForOriginal);
			DataProviderHelper.SetEnableExpirable(true);
			Assert.DoesNotThrow(provider.Validate);
		}

		[Test]
		public void Validate_WithExpirableTypsInKeyProps_InWhiteList()
		{
			var provider = new MetadataProvider(xmlContentWithExpirableTypsInKeyProps_InWhiteList, cacheProvider, cacheProviderForOriginal);
			Assert.DoesNotThrow(provider.Validate);
		}

		[Test]
		public void Validate_WithExpirableTypsInKeyProps_NotInWhiteList()
		{
			var provider = new MetadataProvider(xmlContentWithExpirableTypsInKeyProps_NotInWhiteList, cacheProvider, cacheProviderForOriginal);
			Assert.That(provider.Validate, Throws.TypeOf<RefDataProcessingException>().
				And.Message.EqualTo("xml can't contains expirable type as key property RefCusTariff-RefCusRate;\r\nexcept RefCusRate-RefCusApplicability;RefCusCondition-RefCusApplicability.\r\nErrorCode: MER-00008, please check more details on https://devops.wisetechglobal.com/wtg/RefDataRepo/_wiki/wikis/RefDataRepo.wiki/11393/How-to-resolve-issues?anchor=mer-00008%3A-incorrectxmlschema"));
		}

		[Test]
		public void Validate_KeyPropsContainChildClassProps()
		{
			var provider = new MetadataProvider(xmlContent_KeyPropsContainChildClassProps, cacheProvider, cacheProviderForOriginal);
			DataProviderHelper.SetEnableExpirable(true);
			Assert.DoesNotThrow(provider.Validate);
		}

		[Test]
		public void Validate_WithMoreThanOneExpirableType()
		{
			var provider = new MetadataProvider(xmlContent_WithMoreThanOneExpirableType, cacheProvider, cacheProviderForOriginal);
			DataProviderHelper.SetEnableExpirable(true);
			Assert.That(provider.Validate, Throws.TypeOf<RefDataProcessingException>().
				And.Message.StartsWith($@"RefCusRate contains more than one expirable type as key property.
ErrorCode: {ErrorCodes.MultipleExpirableKeyProperties}"));
		}

		[TestCaseSource(nameof(xmlContent_NotSupportKeyProps))]
		public void Validate_KeyProps_NotSupported(string xmlContent_NotSupport, string keyProp, string nonPersistentFlatten)
		{
			var provider = new MetadataProvider(xmlContent_NotSupport, cacheProvider, cacheProviderForOriginal);
			DataProviderHelper.SetEnableExpirable(true);
			Assert.That(provider.Validate, Throws.TypeOf<RefDataProcessingException>().
				And.Message.EqualTo($"Not allow key property {keyProp} under {nonPersistentFlatten}\r\nErrorCode: MER-00013, please check more details on https://devops.wisetechglobal.com/wtg/RefDataRepo/_wiki/wikis/RefDataRepo.wiki/11393/How-to-resolve-issues?anchor=mer-00013%3A-notsupportedkeyproperty"));
		}

		ICacheProvider cacheProvider;
		ICacheProvider cacheProviderForOriginal;

		[SetUp]
		public void SetUp()
		{
			var cacheProviderMock = new Mock<ICacheProvider>();
			cacheProviderMock.SetupGet(x => x.MetadataCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			cacheProvider = cacheProviderMock.Object;

			var cacheProviderForOriginalMock = new Mock<ICacheProvider>();
			cacheProviderForOriginalMock.SetupGet(x => x.MetadataCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			cacheProviderForOriginal = cacheProviderForOriginalMock.Object;
		}

		const string xmlContent = @"
<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ1_TariffCode"" />
			<PropertyRef Name=""ZZ1_IAMUnique"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""IMP"" />
		  <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		  <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZ1_IAMUnique"" Type=""smallint"" />
		  <Property Name=""ZZ1_Description"" Type=""nvarchar(max)"" />
		  <Property Name=""ZZ1_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ1_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		  <Property Name=""ZZ1_CompositeKeyOnZZ5"" Type=""varchar"" MaxLength=""100"" />
		  <Property Name=""RefCusRate"" Type=""RefCusRate"" Mandatory=""true""/>
		  <Property Name=""RefCusTariffAttribute"" Type=""RefCusTariffAttribute"" />
		  <Property Name=""RefCusTariffNationalCode"" Type=""RefCusTariffNationalCode"" />
		  <Property Name=""RefCusTariffRelationship"" Type=""RefCusTariffRelationship"" />
		  <Property Name=""RefCusCondition"" Type=""RefCusCondition"" />
		  <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
		  <Property Name=""RefCusVATApplicability"" Type=""RefCusVATApplicability"" />
		</EntityType>		
		<EntityType Name=""RefCusApplicability"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
			<PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZT_AdditionalCode"" />
		  </Key>
		  <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZT_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
		  <Property Name=""ZZT_OrderNumber"" Type=""varchar"" MaxLength=""15"" />
		  <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			<PropertyRef Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZX1_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX1_Source"" Type=""varchar(max)"" />
		  <Property Name=""ZX1_Comment"" Type=""varchar(max)"" />
		  <Property Name=""ZX1_IsImport"" Type=""bit"" />
		  <Property Name=""ZX1_IsExport"" Type=""bit"" />
		  <Property Name=""ZX1_ConditionValueTrueMeansStop"" Type=""bit"" />
		  <Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
		  <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZX3_Value"" />
			<PropertyRef Name=""ZX3_ZX4_NKValueType"" />
			<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZX3_ZX4_NKValueType"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZX3_Value"" Type=""varchar"" MaxLength=""500"" />
		  <Property Name=""ZX3_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX3_EndDate"" Type=""smalldatetime"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			<PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusRate"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ2_SelectorFormula"" />
			<PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
			<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			<PropertyRef Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ2_ZZS_NKPreference"" />
		  </Key>
		  <Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ2_RateFormula"" Type=""varchar"" MaxLength=""500"" />
		  <Property Name=""ZZ2_ZZS_NKPreference"" Type=""varchar"" MaxLength=""10"" />
		  <Property Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ2_SelectorFormula"" Type=""varchar"" MaxLength=""500"" />
		  <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""RefCusApplicability"" Type=""RefCusApplicability""  Mandatory=""true""/>
		</EntityType>
		<EntityType Name=""RefCusTariffAttribute"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ3_Name"" />
		  </Key>
		  <Property Name=""ZZ3_Name"" Type=""varchar"" MaxLength=""50"" />
		  <Property Name=""ZZ3_Value"" Type=""nvarchar(max)"" />
		</EntityType>
		<EntityType Name=""RefCusTariffNationalCode"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZW_NationalCode"" />
			<PropertyRef Name=""ZZW_ZZF_NKTaxOrFeeCode"" />
			<PropertyRef Name=""ZZW_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZZW_NationalCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZW_Description"" Type=""nvarchar(max)"" />
		  <Property Name=""ZZW_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZW_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZW_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZW_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""RefCusRate"" Type=""RefCusRate"" />
		  <Property Name=""RefCusVATApplicability"" Type=""RefCusVATApplicability"" />
		  <Property Name=""RefCusTariffAttribute"" Type=""RefCusTariffAttribute"" />
		  <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
		</EntityType>
		<EntityType Name=""RefCusTariffRelationship"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZH_ZZI_NKTariffType"" />
		  </Key>
		  <Property Name=""ZZH_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZH_TariffCode"" Type=""varchar"" MaxLength=""35"" />
		</EntityType>
		<EntityType Name=""RefCusTariffUOM"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ8_Type"" />
		  </Key>
		  <Property Name=""ZZ8_Type"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ8_UOM"" Type=""varchar"" MaxLength=""10"" />
		</EntityType>
		<EntityType Name=""RefCusVATApplicability"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZX5_ZZF_NKTaxOrFeeCode"" />
			<PropertyRef Name=""ZX5_AdditionalCode"" />
		  </Key>
		  <Property Name=""ZX5_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZX5_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX5_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX5_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
		  <Property Name=""ZX5_Description"" Type=""nvarchar(max)"" />
		  <Property Name=""ZX5_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>
";

		const string xmlContentWithEmptyOriginalSchema = @"
<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
	</Schema>
	<OriginalSchema>
	</OriginalSchema>
</UniversalReferenceData>
";

		const string xmlContentWithOriginalSchema = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
			<Property Name=""RefCusRateApplicability"" Type=""RefCusRateApplicability"" />
			<Property Name=""RefCusCondition"" Type=""RefCusCondition"" />
			<Property Name=""ZZ1_TariffCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_AdditionalCode"" />
			</Key>
			<Property Name=""RefCusRateApplicabilityUOM"" Type=""RefCusRateApplicabilityUOM"" />
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateApplicabilityUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""S02_UOM"" />
			</Key>
			<Property Name=""S02_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
			<Key>
				<PropertyRef Name=""S03_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""S03_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
			<Property Name=""RefCusRate"" Type=""RefCusRate"" />
			<Property Name=""RefCusCondition"" Type=""RefCusCondition"" />
			<Property Name=""ZZ1_TariffCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string xmlContentWithOnlyOriginalSchema = @"
<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
		  <Key>
		  </Key>
		  <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZT_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
		  <Property Name=""ZZT_OrderNumber"" Type=""varchar"" MaxLength=""15"" />
		  <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string xmlContentWithExpirableTypsInKeyProps_InWhiteList = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusRate"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusApplicability"">
			<Key>
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""RefCusExcludedTradeGroup"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup""></EntityType>
		<EntityType Name=""RefCusRateUOM"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZZZ_NKDataGrouping"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>
";

		const string xmlContentWithExpirableTypsInKeyProps_NotInWhiteList = @"
<UniversalReferenceData>
	<DataSource>Rate Cond AdditionalCode</DataSource>
	<PublicationTime>2022-01-18T13:31:25</PublicationTime>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""RefCusRate"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusRate"">
			<Key>
			</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>
";

		const string xmlContent_KeyPropsContainChildClassProps = @"
<UniversalReferenceData>
  <DataSource>SampleTariffs</DataSource>
  <PublicationTime>2024-02-16T00:06:01</PublicationTime>
  <UpdateType>Partial</UpdateType>
  <!--Internal Tags Start: Just for Schedulers in RefData Team, Not useful to Business Data-->
  <AppName>CargoWise.RefDbRepo.ZAReferenceData.CmdLine.dll</AppName>
  <AppProgramArgs>TARIFF</AppProgramArgs>
  <!--Internal Tags End-->
  <Schema>
	<EntityType Name=""RefCusTariff"" Data=""true"">
	  <Key>
		<PropertyRef Name=""RefCusTariffAttribute.ZZ3_Name"" ConstantValue=""CheckDigit"" />
		<PropertyRef Name=""RefCusTariffAttribute.ZZ3_Value"" ConstantValue=""3"" />
		<PropertyRef Name=""ZZ1_IAMUnique"" />
		<PropertyRef Name=""ZZ1_TariffCode"" />
		<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
		<PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
		<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
	  </Key>
	  <Property Name=""RefCusTariffAttribute"" Type=""RefCusTariffAttribute"" />
	  <Property Name=""ZZ1_Description"" Type=""nvarchar"" />
	  <Property Name=""ZZ1_EndDate"" Type=""datetime"" />
	  <Property Name=""ZZ1_IAMUnique"" Type=""smallint"" />
	  <Property Name=""ZZ1_StartDate"" Type=""datetime"" />
	  <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
	  <Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""4"" ConstantValue=""VAT"" />
	  <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
	  <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""ZA"" />
	  <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""ZA"" />
	</EntityType>
	<EntityType Name=""RefCusTariffAttribute"" Data=""true"">
	  <Key>
		<PropertyRef Name=""ZZ3_Name"" />
		<PropertyRef Name=""ZZ3_Value"" />
	  </Key>
	  <Property Name=""ZZ3_Name"" Type=""varchar"" MaxLength=""50"" />
	</EntityType>
  </Schema>
</UniversalReferenceData>
";

		const string xmlContent_WithMoreThanOneExpirableType = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusRate"">
			<Key>
				<PropertyRef Name=""RefCusCondition"" />
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
		</EntityType>
	</Schema>
</UniversalReferenceData>
";

		const string xmlContent_NotSupportGroups1 = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusRateApplicability"">
			<Key>
				<PropertyRef Name=""RefCusApplicability.ZZT_AdditionalCode"" />
			</Key>
		</EntityType>
	</Schema>
	<OriginalSchema />
</UniversalReferenceData>
";

		const string xmlContent_NotSupportGroups2 = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusRateApplicability"">
			<Key>
				<PropertyRef Name=""RefCusRateUOM.ZXG_UOM"" />
			</Key>
		</EntityType>
	</Schema>
	<OriginalSchema />
</UniversalReferenceData>
";

		const string xmlContent_NotSupportGroups3 = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusRateApplicability"">
			<Key>
				<PropertyRef Name=""RefCusExcludedTradeGroup.ZZC_ZZA_NKTradeGroup"" />
			</Key>
		</EntityType>
	</Schema>
	<OriginalSchema />
</UniversalReferenceData>
";

		const string xmlContent_NotSupportGroups4 = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"">
			<Key>
				<PropertyRef Name=""RefCusApplicability.ZZT_AdditionalCode"" />
			</Key>
		</EntityType>
	</Schema>
	<OriginalSchema />
</UniversalReferenceData>
";

		const string xmlContent_NotSupportGroups5 = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"">
			<Key>
				<PropertyRef Name=""RefCusExcludedTradeGroup.ZZC_ZZA_NKTradeGroup"" />
			</Key>
		</EntityType>
	</Schema>
	<OriginalSchema />
</UniversalReferenceData>
";

		const string xmlContent_NotSupportGroups6 = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"">
			<Key>
				<PropertyRef Name=""RefCusConditionValue.ZX3_Value"" />
			</Key>
		</EntityType>
	</Schema>
	<OriginalSchema />
</UniversalReferenceData>
";

		const string xmlContent_NotSupportGroups7 = @"
<UniversalReferenceData>
	<UpdateType>Full</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"">
			<Key>
				<PropertyRef Name=""RefCusConditionLanguage.ZXJ_Comment"" />
			</Key>
		</EntityType>
	</Schema>
	<OriginalSchema />
</UniversalReferenceData>
";

		static IEnumerable<TestCaseData> xmlContent_NotSupportKeyProps
		{
			get
			{
				yield return new TestCaseData(xmlContent_NotSupportGroups1, "RefCusApplicability.ZZT_AdditionalCode", "RefCusRateApplicability").SetName("Validate_KeyProps_NotSupported1");
				yield return new TestCaseData(xmlContent_NotSupportGroups2, "RefCusRateUOM.ZXG_UOM", "RefCusRateApplicability").SetName("Validate_KeyProps_NotSupported2");
				yield return new TestCaseData(xmlContent_NotSupportGroups3, "RefCusExcludedTradeGroup.ZZC_ZZA_NKTradeGroup", "RefCusRateApplicability").SetName("Validate_KeyProps_NotSupported3");
				yield return new TestCaseData(xmlContent_NotSupportGroups4, "RefCusApplicability.ZZT_AdditionalCode", "RefCusConditionApplicability").SetName("Validate_KeyProps_NotSupported4");
				yield return new TestCaseData(xmlContent_NotSupportGroups5, "RefCusExcludedTradeGroup.ZZC_ZZA_NKTradeGroup", "RefCusConditionApplicability").SetName("Validate_KeyProps_NotSupported5");
				yield return new TestCaseData(xmlContent_NotSupportGroups6, "RefCusConditionValue.ZX3_Value", "RefCusConditionApplicability").SetName("Validate_KeyProps_NotSupported6");
				yield return new TestCaseData(xmlContent_NotSupportGroups7, "RefCusConditionLanguage.ZXJ_Comment", "RefCusConditionApplicability").SetName("Validate_KeyProps_NotSupported7");
			}
		}
	}
}
